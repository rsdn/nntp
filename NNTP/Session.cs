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
using System.Diagnostics;
using System.Net;

namespace derIgel.NNTP
{
	using Util = derIgel.MIME.Util;

	/// <summary>
	/// NNTP Session
	/// </summary>
	public class Session : IDisposable
	{

		protected TextWriter errorOutput = System.Console.Error;

		public static readonly string hostName;

		static Session()
		{
			try
			{
				hostName = Dns.GetHostName();
			}
			catch (SocketException)
			{
				hostName = "";
			}

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
			notAllowedStateAnswer[States.AuthRequired] = new Response(NntpResponse.AuthentificationRequired);
			notAllowedStateAnswer[States.MoreAuthRequired] = new Response(NntpResponse.MoreAuthentificationRequired);

		}

#if PERFORMANCE_COUNTERS
		/// <summary>
		/// perfomance counter for requests
		/// </summary>
		PerformanceCounter requestsCounter;
		/// <summary>
		/// perfomance counter for not performed requests
		/// </summary>
		PerformanceCounter badRequestsCounter;
		/// <summary>
		/// perfomance counter transfered articles
		/// </summary>
		PerformanceCounter articlesCounter;
		/// <summary>
		/// global perfomance counter for requests
		/// </summary>
		PerformanceCounter globalRequestsCounter;
		/// <summary>
		/// global perfomance counter for not performed requests
		/// </summary>
		PerformanceCounter globalBadRequestsCounter;
		/// <summary>
		/// global perfomance counter transfered articles
		/// </summary>
		PerformanceCounter globalArticlesCounter;
#endif

		public Session(Socket client, IDataProvider dataProvider, WaitHandle exitEvent, TextWriter errorOutput)
		{
			sessionState = dataProvider.InitialSessionState;

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

#if PERFORMANCE_COUNTERS
			// create perfomance counters' category if necessary
			string PerfomanceCategoryName = "RSDN NNTP Server Sessions";
			CounterCreationDataCollection perfomanceCountersCollection = new CounterCreationDataCollection();
			CounterCreationData requestsCounterData = new CounterCreationData("Requests",
				"Count of client's requests",	PerformanceCounterType.NumberOfItems32);
			perfomanceCountersCollection.Add(requestsCounterData );
			CounterCreationData badRequestsCounterData = new CounterCreationData("Not performed requests",
				"Count of not performed client's requests",	PerformanceCounterType.NumberOfItems32);
			perfomanceCountersCollection.Add(badRequestsCounterData );
			CounterCreationData articlesCounterData = new CounterCreationData("Articles",
				"Count of transfered articles",	PerformanceCounterType.NumberOfItems32);
			perfomanceCountersCollection.Add(articlesCounterData );
			if (!PerformanceCounterCategory.Exists(PerfomanceCategoryName))
				PerformanceCounterCategory.Create(PerfomanceCategoryName, "", perfomanceCountersCollection);

			// create perfomance counters
			requestsCounter = new PerformanceCounter(PerfomanceCategoryName,
				requestsCounterData .CounterName, "Client " + client.RemoteEndPoint.ToString(), false);
			badRequestsCounter = new PerformanceCounter(PerfomanceCategoryName,
				badRequestsCounterData .CounterName, "Client " + client.RemoteEndPoint.ToString(), false);
			articlesCounter = new PerformanceCounter(PerfomanceCategoryName,
				articlesCounterData .CounterName, "Client " + client.RemoteEndPoint.ToString(), false);
			globalRequestsCounter = new PerformanceCounter(PerfomanceCategoryName,
				requestsCounterData .CounterName, "All", false);
			globalBadRequestsCounter = new PerformanceCounter(PerfomanceCategoryName,
				badRequestsCounterData .CounterName, "All", false);
			globalArticlesCounter = new PerformanceCounter(PerfomanceCategoryName,
				articlesCounterData .CounterName, "All", false);
#endif
		}

		protected static Hashtable commandsTypes;
		public string Username;
		public string Password;
		protected internal string sender;

		// resonse answer from server
		protected void Answer(int code)
		{
			Answer(new Response(code));
		}

		protected void Answer(NntpResponse code)
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
				Answer(dataProvider.PostingAllowed ? NntpResponse.Ok : NntpResponse.OkNoPosting);

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
								Answer(NntpResponse.TimeOut);
								return;
							case 1	:
								// sorry, bye!
								Answer(NntpResponse.ServiceDiscontinued);
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

					while (bufferString.ToString().IndexOf(delimeter) != -1)
					{
						// get command string till delimeter
						commandString = bufferString.ToString().Substring(0, bufferString.ToString().IndexOf(delimeter));

						// remove retrivied command from buffer
						bufferString.Remove(0, bufferString.ToString().IndexOf(delimeter) + delimeter.Length);
						
						#if DEBUG || SHOW
							errorOutput.WriteLine(commandString);
						#endif

						// especialy for Outlook Express
						// it send sometimes blank lines
						if (commandString == string.Empty)
							continue;

						try
						{
							switch (sessionState)
							{
								case States.PostWaiting	:
									sessionState = States.Normal;
									Message postingMessage = Message.Parse(commandString);
									
									// add addtitional server headers
									postingMessage["Sender"] = sender;
										postingMessage["Path"] = hostName +
											((postingMessage["Path"] != null) ? "!" + postingMessage["Path"] : null);
									
									dataProvider.PostMessage(postingMessage);
									result = new Response(NntpResponse.PostedOk);
									break;
								default	:
									// get first word in upper case delimeted by space or tab characters 
									command = commandString.Split(new char[]{' ', '\t', '\r'}, 2)[0].ToUpper();

#if PERFORMANCE_COUNTERS
									requestsCounter.Increment();
									globalRequestsCounter.Increment();
#endif
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
												result = new Response(NntpResponse.NotAllowed); // command not allowed
										}
									}
									else
										result = new Response(NntpResponse.NotRecognized); // no such command
									break;
							}
						}
						catch (Response.ParamsException)
						{
							result = new Response(NntpResponse.ProgramFault);
						}
						catch (DataProviderException exception)
						{
							switch (exception.Error)
							{
								case DataProviderErrors.NoSuchGroup:
									result = new Response(NntpResponse.NoSuchGroup);
									break;
								case DataProviderErrors.NoSelectedGroup:
									result = new Response(NntpResponse.NoSelectedGroup);
									break;
								case DataProviderErrors.NoSelectedArticle:
									result = new Response(NntpResponse.NoSelectedArticle);
									break;
								case DataProviderErrors.NoNextArticle:
									result = new Response(NntpResponse.NoNextArticle);
									break;
								case DataProviderErrors.NoPrevArticle:
									result = new Response(NntpResponse.NoPrevArticle);
									break;
								case DataProviderErrors.NoSuchArticleNumber:
									result = new Response(NntpResponse.NoSuchArticleNumber);
									break;
								case DataProviderErrors.NoSuchArticle:
									result = new Response(NntpResponse.NoSuchArticle);
									break;
								case DataProviderErrors.NoPermission:
									result = new Response(NntpResponse.NoPermission);
									sessionState = States.AuthRequired;
									break;
								case DataProviderErrors.NotSupported:
									result = new Response(NntpResponse.NotRecognized);
									break;
								case DataProviderErrors.PostingFailed:
									result = new Response(NntpResponse.PostingFailed);
									break;
								case DataProviderErrors.ServiceUnaviable:
									result = new Response(NntpResponse.ServiceDiscontinued);
									break;
								default:
									result = new Response(NntpResponse.ProgramFault); //error
									break;
							}
						}
						catch (derIgel.MIME.MimeFormattingException)
						{
							result = new Response(NntpResponse.PostingFailed);
						}
						catch(Exception e)
						{
							#if DEBUG || SHOW
								errorOutput.WriteLine("\texception: " + e.ToString());
							#endif
							result = new Response(NntpResponse.ProgramFault);
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
						// result code indicates error
						{
#if PERFORMANCE_COUNTERS
							badRequestsCounter.Increment();
							globalBadRequestsCounter.Increment();
#endif
						}

						switch((NntpResponse)result.Code)
						{
							case NntpResponse.ArticleHeadBodyRetrivied :
							case NntpResponse.ArticleHeadRetrivied :
							case NntpResponse.ArticleBodyRetrivied :
							case NntpResponse.ArticleNothingRetrivied :
#if PERFORMANCE_COUNTERS
								articlesCounter.Increment();
								globalArticlesCounter.Increment();
#endif
								break;
							case NntpResponse.Bye : // quit
							case NntpResponse.ServiceDiscontinued : // service disctontined
							case NntpResponse.ServiceUnaviable : // service unaviable
							case NntpResponse.TimeOut : // timeout
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
		protected internal Socket client;
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