using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.Reflection;
using derIgel.NNTP.Commands;
using System.Text.RegularExpressions;
using derIgel.MIME;

namespace derIgel.NNTP
{
	using Util = derIgel.MIME.Util;

	/// <summary>
	/// NNTP Session
	/// </summary>
	public class Session : IDisposable
	{

		protected Statistics stat;
		protected TextWriter errorOutput = System.Console.Error;

		static Session()
		{
			commandsTypes = new Hashtable();
			// initialize types for NNTP commands classes
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				if (assembly.IsDefined(typeof(NNTPCommandAttribute), false))
					// if assembly contains NNTP commands
					foreach (Type type in	assembly.GetTypes())
						if (type.IsDefined(typeof(NNTPCommandAttribute), true)&&
								type.IsSubclassOf(typeof(Generic)) &&
								!type.IsAbstract)
						{
							foreach(object attr in type.GetCustomAttributes(typeof(NNTPCommandAttribute), true))
								commandsTypes[((NNTPCommandAttribute)attr).Name] = type;
						}

			// answers for commands during allowed states
			notAllowedStateAnswer = new Hashtable();
			notAllowedStateAnswer[States.AuthRequired] = new Response(480);
			notAllowedStateAnswer[States.MoreAuthRequired] = new Response(381);

		}

		public Session(Socket client, IDataProvider dataProvider, WaitHandle exitEvent, Statistics stat,
			TextWriter errorOutput)
		{
			sessionState = dataProvider.InitialSessionState;

			this.stat = stat;
			this.errorOutput = errorOutput;

			this.exitEvent = exitEvent;
			commandBuffer = new byte[bufferSize];
			this.client = client;
			netStream = new NetworkStream(client);
			this.dataProvider = dataProvider;

			// Init client's command array
			commands = new Hashtable();
			foreach (DictionaryEntry entry in commandsTypes)
				commands[entry.Key] = 
					Activator.CreateInstance((Type)entry.Value, new Object[]{this});

		}

		protected static Hashtable commandsTypes;
		public string Username;
		public string Password;

		// resonse answer from server
		protected void Answer(int code)
		{
			Answer(new Response(code));
		}

		protected void Answer(Response response)
		{
			byte[] bytes = response.GetResponse();
			netStream.Write(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Process client
		/// </summary>
		public void Process(Object manager)
		{
			string delimeter;
			Response result = null;
			try
			{
				// response OK
				Answer(dataProvider.PostingAllowed ? 200 : 201);

				StringBuilder bufferString = new StringBuilder();
				while (true)
				{
					do
					{
						IAsyncResult asyncResult = netStream.BeginRead(commandBuffer, 0, bufferSize, null, null);
						switch (WaitHandle.WaitAny(
							new WaitHandle[] {asyncResult.AsyncWaitHandle, exitEvent},
							connectionTimeout, false))
						{
							case WaitHandle.WaitTimeout	:
								// timeout
								Answer(402);
								return;
							case 1	:
								// sorry, bye!
								Answer(400);
								return;
						}
						int receivedBytes = netStream.EndRead(asyncResult);
						if (receivedBytes  > 0)
							bufferString.Append(Util.BytesToString(commandBuffer, receivedBytes));
						else
							return;
					}
					while (netStream.DataAvailable);

					if (sessionState == States.PostWaiting)
						delimeter = Util.CRLF + "." + Util.CRLF;
					else
						delimeter = Util.CRLF;

					if (bufferString.ToString().IndexOf(delimeter) != -1)
					{
						// get command string till delimeter
						commandString = bufferString.ToString().Substring(0, bufferString.ToString().IndexOf(delimeter));
						
						#if DEBUG || SHOW
							errorOutput.WriteLine(commandString);
						#endif

						bufferString.Remove(0, bufferString.ToString().IndexOf(delimeter) + delimeter.Length);
					
						try
						{
							switch (sessionState)
							{
								case States.PostWaiting	:
									dataProvider.PostMessage(Message.Parse(commandString));
									sessionState = States.Normal;
									result = new Response(240);
									break;
								default	:
									// get first word in upper case delimeted by space or tab characters 
									command = commandString.Split(new char[]{' ', '\t', '\r'}, 2)[0].ToUpper();

									stat.AddStatistic(command);

									Commands.Generic nntpCommand = commands[command] as Commands.Generic;
									// check suppoting command
									if (nntpCommand != null)
									{	
										if (nntpCommand.IsAllowed(sessionState))
											result = nntpCommand.Process();
										else
										{
											result = notAllowedStateAnswer[sessionState] as Response;
											if (result == null)
												result = new Response(403); // command not allowed
										}
									}
									else
										result = new Response(500); // no such command
									break;
							}
						}
						catch (Response.ParamsException)
						{
							result = new Response(503);
						}
						catch (DataProviderException exception)
						{
							switch (exception.Error)
							{
								case DataProviderErrors.NoSuchGroup:
									result = new Response(411);
									break;
								case DataProviderErrors.NoSelectedGroup:
									result = new Response(412);
									break;
								case DataProviderErrors.NoSelectedArticle:
									result = new Response(420);
									break;
								case DataProviderErrors.NoNextArticle:
									result = new Response(421);
									break;
								case DataProviderErrors.NoPrevArticle:
									result = new Response(422);
									break;
								case DataProviderErrors.NoSuchArticleNumber:
									result = new Response(423);
									break;
								case DataProviderErrors.NoSuchArticle:
									result = new Response(430);
									break;
								case DataProviderErrors.NoPermission:
									result = new Response(480);
									sessionState = States.AuthRequired;
									break;
								case DataProviderErrors.NotSupported:
									result = new Response(500);
									break;
								case DataProviderErrors.PostingFailed:
									result = new Response(441);
									break;
								case DataProviderErrors.ServiceUnaviable:
									result = new Response(400);
									break;
								default:
									result = new Response(503); //error
									break;
							}
						}
						catch(Exception e)
						{
							#if DEBUG || SHOW
								errorOutput.WriteLine("\texception: " + Util.ExpandException(e));
							#endif
							result = new Response(503);
						}

						Answer(result);

#if DEBUG || SHOW
						string firstLine = Encoding.ASCII.GetString(result.GetResponse());
						/*if ((firstLine.Length -
									(firstLine.IndexOf(derIgel.Utils.Util.CRLF) + derIgel.Utils.Util.CRLF.Length)) > 0)
						{
							firstLine = firstLine.Remove(firstLine.IndexOf(derIgel.Utils.Util.CRLF),
								firstLine.Length - firstLine.IndexOf(derIgel.Utils.Util.CRLF)) +
								derIgel.Utils.Util.CRLF + "..." + derIgel.Utils.Util.CRLF;
						}*/
						errorOutput.Write(firstLine);
#endif

						if (result.Code >= 400)
							stat.AddError(result.Code, commandString);

						stat.CheckSend();

						switch(result.Code)
						{
							case 205: // quit
							case 400: // service disctontined
							case 401: // service unaviable
							case 402: // timeout
								return;
						}
					}
				}
			}
			catch (IOException)
			{
				// network error
			}
			finally
			{
				Dispose();
			}
		}

		/// <summary>
		/// network client
		/// </summary>
		protected Socket client;
		/// <summary>
		/// network stream for client's session
		/// </summary>
		protected NetworkStream netStream;
		/// <summary>
		/// size of commandBuffer
		/// </summary>
		protected const int bufferSize = 1024;
		/// <summary>
		/// buffer for command string bytes
		/// </summary>
		protected byte[] commandBuffer;
		/// <summary>
		/// last received command string
		/// </summary>
		protected internal string commandString;
		/// <summary>
		/// last received command
		/// </summary>
		string command;

		/// <summary>
		/// NNTP Session states
		/// </summary>
		[Flags]
		public enum States {None = 0, Normal = 1, AuthRequired = 2, MoreAuthRequired = 4, PostWaiting = 8}
	
		/// <summary>
		/// State of current session
		/// </summary>
		protected internal States sessionState = States.Normal;
		/// <summary>
		/// Associative arrays of NNTP client commands
		/// </summary>
		protected internal Hashtable commands;

		protected IDataProvider dataProvider;
		public IDataProvider DataProvider
		{
			get {return dataProvider;}
		}
		/// <summary>
		/// connection timeout interval in milliseconds (3 min)
		/// </summary>
		protected const int connectionTimeout = 180000;

		/// <summary>
		/// free management resources
		/// </summary>
		public void Dispose()
		{
			netStream.Close();
			if (client.Connected)
			{
				client.Shutdown(SocketShutdown.Both);
				client.Close();
			}
			
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		public event EventHandler Disposed;

		protected WaitHandle exitEvent;
		protected static Hashtable notAllowedStateAnswer;
	}
}