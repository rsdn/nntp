using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using System.Reflection;
using derIgel.NNTP.Commands;
using derIgel.Utils;
//using derIgel.NNTP.Commands;

namespace derIgel.NNTP
{
	/// <summary>
	/// NNTP Session
	/// </summary>
	public class Session : IDisposable
	{
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

		public Session(Socket client, DataProvider dataProvider, WaitHandle exitEvent)
		{
			sessionState = States.Normal;

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

		// resonse answer from server
		protected void Answer(int code)
		{
			Answer(new Response(code));
		}

		protected void Answer(Response response)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(response.GetResponse());
			netStream.Write(bytes, 0, bytes.GetLength(0));
		}

		/// <summary>
		/// Process client
		/// </summary>
		public void Process(Object state)
		{
			string delimeter;
			Response result = null;
			try
			{
				// response OK
				Answer(dataProvider.PostingAllowed ? 200 : 201);

				string bufferString = string.Empty;
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
							bufferString += Encoding.ASCII.GetString(commandBuffer, 0, receivedBytes);
						else
							return;
					}
					while (netStream.DataAvailable);

					if (sessionState == States.PostWaiting)
						delimeter = Util.CRLF + "." + Util.CRLF;
					else
						delimeter = Util.CRLF;
					if (bufferString.IndexOf(delimeter) != -1)
					{
						// get command string till delimeter
						commandString = bufferString.Substring(0, bufferString.IndexOf(delimeter));
						
						#if DEBUG
							Console.Error.WriteLine(commandString);
						#endif

						bufferString = bufferString.Remove(0,
							bufferString.IndexOf(delimeter) + delimeter.Length);
					
						if ((bufferString != string.Empty) && (bufferString.Trim() == string.Empty))
							bufferString = string.Empty;

						try
						{
							switch (sessionState)
							{
								case States.PostWaiting	:
									dataProvider.PostMessage(commandString);
									sessionState = States.Normal;
									result = new Response(240);
									break;
								default	:
									// get first word in upper case delimeted by space or tab characters 
									command = commandString.Split(new char[]{' ', '\t', '\r'}, 2)[0].ToUpper();

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
						catch (DataProvider.Exception exception)
						{
							switch (exception.Error)
							{
								case DataProvider.Errors.NoSuchGroup:
									result = new Response(411);
									break;
								case DataProvider.Errors.NoSelectedGroup:
									result = new Response(412);
									break;
								case DataProvider.Errors.NoSelectedArticle:
									result = new Response(420);
									break;
								case DataProvider.Errors.NoNextArticle:
									result = new Response(421);
									break;
								case DataProvider.Errors.NoPrevArticle:
									result = new Response(422);
									break;
								case DataProvider.Errors.NoSuchArticleNumber:
									result = new Response(423);
									break;
								case DataProvider.Errors.NoSuchArticle:
									result = new Response(430);
									break;
								case DataProvider.Errors.NoPermission:
									result = new Response(480);
									sessionState = States.AuthRequired;
									break;
								case DataProvider.Errors.NotSupported:
									result = new Response(500);
									break;
								case DataProvider.Errors.PostingFailed:
									result = new Response(441);
									break;
								default:
									result = new Response(503); //error
									break;
							}
						}
						catch(Exception e)
						{
							#if DEBUG
								System.Console.Error.WriteLine("\texception: " + Util.ExpandException(e));
							#endif
							result = new Response(503);
						}

						Answer(result);

						#if DEBUG
							Console.Error.Write(result.GetResponse());
						#endif

						if (result.Code == 205) //quit response
							return;
					}
				}
			}
			catch (IOException)
			{
				// network error
			}
			finally
			{
				Close();
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
		protected const int bufferSize = 512;
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
		/// <summary>
		/// Current selected group
		/// </summary>
		protected string currentGroup = null;
		/// <summary>
		/// Current selected article
		/// </summary>
		protected int currentArticle = -1;

		/// <summary>
		/// close connection
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		protected internal DataProvider dataProvider;
		/// <summary>
		/// connection timeout interval in milliseconds (3 min)
		/// </summary>
		protected const int connectionTimeout = 180000;

		public void Dispose()
		{
			netStream.Close();
			client.Shutdown(SocketShutdown.Both);
			client.Close();		
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		public event EventHandler Disposed;

		protected WaitHandle exitEvent;
		protected static Hashtable notAllowedStateAnswer;
	}
}