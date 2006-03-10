using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using log4net;

using Rsdn.Mime;
using Rsdn.Nntp.Commands;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Rsdn.Nntp
{
	/// <summary>
	/// NNTP Session
	/// </summary>
	public class Session : IDisposable
	{

		public static readonly string Hostname;
		public static readonly string FullHostname;

		/// <summary>
		/// Logger
		/// </summary>
		protected ILog logger;

		static Session()
		{
			// DNS
			try
			{
				Hostname = Dns.GetHostName();
				FullHostname = Dns.GetHostEntry(Hostname).HostName;
			}
			catch (SocketException)
			{
				Hostname = "";
				FullHostname = "";
			}

      commandsTypes = new Dictionary<string, Type>();
			// initialize types for NNTP commands classes
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				if (assembly.IsDefined(typeof(NntpCommandsAssemblyAttribute), false))
					// if assembly contains NNTP commands
					foreach (Type type in	assembly.GetTypes())
						if (type.IsDefined(typeof(NntpCommandAttribute), false) &&
								type.IsSubclassOf(typeof(Generic)) &&
								!type.IsAbstract)
						{
							foreach(NntpCommandAttribute attr in
									type.GetCustomAttributes(typeof(NntpCommandAttribute), false))
								commandsTypes[attr.Name] = type;
						}

			// answers for commands during allowed states
      notAllowedStateAnswer = new Dictionary<States, Response>();
			notAllowedStateAnswer[States.AuthRequired] = new Response(NntpResponse.AuthentificationRequired);
			notAllowedStateAnswer[States.MoreAuthRequired] = new Response(NntpResponse.MoreAuthentificationRequired);
		}

#if PERFORMANCE_COUNTERS_OLD
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

		/// <summary>
		/// Manager (parent) object
		/// </summary>
		protected Manager _manager;
		public Manager Manager
		{
			get { return _manager; }
		}

    /// <summary>
    /// Create user session
    /// </summary>
    /// <param name="client">Established client's socket</param>
    /// <param name="certificate">Certificate for SSL connection, NULL - if no SSL.</param>
    /// <param name="dataProvider">Data Provider</param>
    /// <param name="manager">Sessions' manager.</param>
		public Session(Socket client, X509Certificate2 certificate, IDataProvider dataProvider, Manager manager)
		{
      logger = LogManager.GetLogger(manager.Name);

      sessionState = dataProvider.InitialSessionState;

      commandBuffer = new byte[bufferSize];

      _certificate = certificate;
			_client = client;
      netStream = new NetworkStream(client);
      if (certificate != null)
			  netStream = new SslStream(netStream, false) ;
			_dataProvider = dataProvider;
			_manager = manager;

			// Init client's command array
      commands = new Dictionary<string, Generic>();
			foreach (KeyValuePair<string, Type> entry in commandsTypes)
				commands[entry.Key] = (Generic)
					Activator.CreateInstance(entry.Value, new Object[]{this});

#if PERFORMANCE_COUNTERS_OLD
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

		/// <summary>
		/// Currently selected group
		/// </summary>
		protected internal string currentGroup = null;
		/// <summary>
		/// Currently selected article
		/// </summary>
		protected internal int currentArticle = -1;

		protected static IDictionary<string, Type> commandsTypes;
		public string Username;
		public string Password;
		protected internal string sender;

		// resonse answer from server
		protected void Answer(int code)
		{
			Answer(new Response(code));
		}

    protected void AnswerWithSslCheck(NntpResponse code)
    {
      SslStream sslStream = netStream as SslStream;
      if (sslStream != null)
      {
        if (!sslStream.CanWrite)
          return;
      }
      Answer(new Response(code));
    }

		protected void Answer(NntpResponse code)
		{
			Answer(new Response(code));
		}

		protected void Answer(Response response)
		{
			byte[] bytes = Util.StringToBytes(response.GetResponse());
			netStream.Write(bytes, 0, bytes.Length);
#if PERFORMANCE_COUNTERS
			_manager.GetPerformanceCounter(Manager.bytesSentPerSecCounterName).IncrementBy(bytes.Length);
			Manager.GetGlobalPerformanceCounter(Manager.bytesSentPerSecCounterName).IncrementBy(bytes.Length);
			_manager.GetPerformanceCounter(Manager.bytesSentCounterName).IncrementBy(bytes.Length);
			Manager.GetGlobalPerformanceCounter(Manager.bytesSentCounterName).IncrementBy(bytes.Length);
			_manager.GetPerformanceCounter(Manager.bytesTotalPerSecCounterName).IncrementBy(bytes.Length);
			Manager.GetGlobalPerformanceCounter(Manager.bytesTotalPerSecCounterName).IncrementBy(bytes.Length);
			_manager.GetPerformanceCounter(Manager.bytesTotalCounterName).IncrementBy(bytes.Length);
			Manager.GetGlobalPerformanceCounter(Manager.bytesTotalCounterName).IncrementBy(bytes.Length);
#endif
		}

		/// <summary>
		/// Process client
		/// </summary>
		public void Process(Object obj)
		{
			// Set nested device context for current client
			NDC.Push(_client.RemoteEndPoint.ToString());

			if (logger.IsInfoEnabled)
				logger.Info(string.Format("Session started. Local end point {0}.", _client.LocalEndPoint));

			bool posting = false;
			string delimeter;
			Response result = null;
			try
			{
        WaitHandle startEvent;
        IAsyncResult sslAuthDone = null;
        // TODO: SSL
        if (netStream is SslStream)
        {
          SslStream sslStream = (SslStream)netStream;
          sslAuthDone = sslStream.BeginAuthenticateAsServer(_certificate, false,
            SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls, false, null, sslStream);
          startEvent = sslAuthDone.AsyncWaitHandle;
        }
        else
        {
          startEvent = new AutoResetEvent(true);
        }

				StringBuilder bufferString = new StringBuilder();

        IAsyncResult asyncResult = null;
        WaitHandle[] eventsWaitTable =
          new WaitHandle[] { new ManualResetEvent(false), _manager.ExitEvent, startEvent};
        bool firstPass = true;

				while (true)
				{
          if (!firstPass)
          {
            asyncResult = netStream.BeginRead(commandBuffer, 0, bufferSize, null, null);
            eventsWaitTable[0] = asyncResult.AsyncWaitHandle;
          }
					switch (WaitHandle.WaitAny(eventsWaitTable, connectionTimeout, false))
					{
            // timeout
            case WaitHandle.WaitTimeout:
              AnswerWithSslCheck(NntpResponse.TimeOut);
							return;
            // _manager.ExitEvent
            case 1 :
              // terminate session
              AnswerWithSslCheck(NntpResponse.ServiceDiscontinued);
              return;
            // startEvent
						case 2 :
							// complete ssl authentification, if necessary
              if (sslAuthDone != null)
                ((SslStream)sslAuthDone.AsyncState).EndAuthenticateAsServer(sslAuthDone);

              // response OK
              Answer(new Response(_dataProvider.PostingAllowed ?
                NntpResponse.Ok : NntpResponse.OkNoPosting, null,
                  string.Format("{0} ({1}; {2})",
                  _manager.Name, Manager.ServerID, _dataProvider.Identity)));

              // first pass finished
              firstPass = false;
              eventsWaitTable = new WaitHandle[] { null, _manager.ExitEvent };
              continue;

             // data is ready
            default :
              int receivedBytes = netStream.EndRead(asyncResult);

              if (receivedBytes  > 0)
					    {
						    bufferString.Append(Util.BytesToString(commandBuffer, receivedBytes));
    #if PERFORMANCE_COUNTERS
						    _manager.GetPerformanceCounter(Manager.bytesReceivedPerSecCounterName).IncrementBy(receivedBytes);
						    Manager.GetGlobalPerformanceCounter(Manager.bytesReceivedPerSecCounterName).IncrementBy(receivedBytes);
						    _manager.GetPerformanceCounter(Manager.bytesReceivedCounterName).IncrementBy(receivedBytes);
						    Manager.GetGlobalPerformanceCounter(Manager.bytesReceivedCounterName).IncrementBy(receivedBytes);
						    _manager.GetPerformanceCounter(Manager.bytesTotalPerSecCounterName).IncrementBy(receivedBytes);
						    Manager.GetGlobalPerformanceCounter(Manager.bytesTotalPerSecCounterName).IncrementBy(receivedBytes);
						    _manager.GetPerformanceCounter(Manager.bytesTotalCounterName).IncrementBy(receivedBytes);
						    Manager.GetGlobalPerformanceCounter(Manager.bytesTotalCounterName).IncrementBy(receivedBytes);
    #endif
                break;
					    }
					    else
						    // stream is closed
						    return;
          }

					switch (sessionState)
					{
						case States.PostWaiting :
						case States.TransferWaiting :
							delimeter = Util.CRLF + "." + Util.CRLF;
							break;
						default:
							delimeter = Util.CRLF;
							break;
					}

					while (bufferString.ToString().IndexOf(delimeter) != -1)
					{
						// get command string till delimeter
						commandString = bufferString.ToString().Substring(0, bufferString.ToString().IndexOf(delimeter));

						// remove retrivied command from buffer
						bufferString.Remove(0, bufferString.ToString().IndexOf(delimeter) + delimeter.Length);
						
						// debug tracing
						logger.Debug(commandString);

						// especialy for Outlook Express
						// it send sometimes blank lines
						if (commandString == string.Empty)
							continue;

						try
						{
							switch (sessionState)
							{
								case States.PostWaiting	:
								case States.TransferWaiting :
									try
									{
										posting = true;
										string message = Response.DemodifyTextResponse(commandString);
										Message postingMessage = Message.Parse(message, true, true,
											new Regex("(?i)Subject"));
								
										// add addtitional server headers
										if (sender != null)
											postingMessage["Sender"] = sender;
										if (postingMessage["Path"] == null)
											postingMessage["Path"] = "not-for-mail";
										postingMessage["Path"] = FullHostname + "!" + postingMessage["Path"];
								
										_dataProvider.PostMessage(postingMessage);
										result = new Response(sessionState == States.PostWaiting ? NntpResponse.PostedOk : NntpResponse.TransferOk);
									}
									catch (MimeFormattingException ex)
									{
										throw new DataProviderException(DataProviderErrors.PostingFailed,
											ex.Message);
									}
									break;
								default	:
									// get first word in upper case delimeted by space or tab characters 
									command = commandString.Split(new char[]{' ', '\t', '\r'}, 2)[0].ToUpper();

#if PERFORMANCE_COUNTERS_OLD
									requestsCounter.Increment();
									globalRequestsCounter.Increment();
#endif
									// check suppoting command
                  if (commands.ContainsKey(command))
									{
                    Commands.Generic nntpCommand = commands[command];
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
									{
										result = new Response(NntpResponse.NotRecognized); // no such command
										logger.Warn(string.Format("Command not recognized ({0}).", commandString));
									}
									break;
							}
						}
						catch (DataProviderException exception)
						{
							switch (exception.Error)
							{
								case DataProviderErrors.NoSuchGroup:
									// Assume that exception's message contains not founded group name
									result = new Response(NntpResponse.NoSuchGroup, null, exception.Message);
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
									logger.Warn(string.Format("{0} provider don't support {1} command.", _dataProvider.GetType(), command));
									break;
								case DataProviderErrors.PostingFailed:
									result = new Response(sessionState == States.TransferWaiting ?
										NntpResponse.TransferFailed :  NntpResponse.PostingFailed, null,
										exception.Message);
									break;
								case DataProviderErrors.ServiceUnaviable:
									result = new Response(NntpResponse.ServiceDiscontinued);
									break;
								case DataProviderErrors.Timeout:
									result = new Response(NntpResponse.TimeOut);
									break;
								default:
									// Unknown data provider error
									result = new Response(NntpResponse.ProgramFault, null, exception.Message);
									break;
							}
							logger.Warn(string.Format("Data Provider Error ({0})", exception.Error), exception);
						}
            catch (AuthenticationException authException)
            {
              logger.Warn("SSL authentification error", authException);
              return;
            }
						// not good....
						catch(Exception e)
						{
							logger.Error(
								string.Format("Exception during processing command" +
								" (selected group '{0}', last request '{1}').\n",
								currentGroup, commandString), e);
							result = new Response(NntpResponse.ProgramFault, null, e.Message);
						}
						finally
						{
							if (posting)
							{
								sessionState = States.Normal;
								posting = false;
							}
						}

						Answer(result);

						// debug tracing
						logger.Debug(result);

						if (result.Code >= 400)
							// result code indicates error
						{
#if PERFORMANCE_COUNTERS_OLD
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
#if PERFORMANCE_COUNTERS_OLD
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
			catch (IOException ioException)
			{
				// network error
        logger.Warn("Network error", ioException); 
			}
			catch (Exception e)
			{
				// something wrong......
				logger.Fatal("Fatal error", e);
			}
			finally
			{
				logger.Info("Session finished");
				NDC.Pop();
				Dispose();
			}
		}

		/// <summary>
		/// network client
		/// </summary>
		protected Socket _client;
    /// <summary>
    /// Client's endpoint
    /// </summary>
    public EndPoint RemoteEndPoint
    {
      get { return _client.RemoteEndPoint;  }
    }
    /// <summary>
    /// Server vertificate using for SSL authentification
    /// </summary>
    protected X509Certificate _certificate;
		/// <summary>
		/// network stream for client's session
		/// </summary>
		protected Stream netStream;
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
		public enum States
		{
			/// <summary>
			/// Not defined state.
			/// </summary>
			None = 0,
			/// <summary>
			/// Normal state.
			/// </summary>
			Normal = 1,
			/// <summary>
			/// Authorization is required.
			/// </summary>
			AuthRequired = 2,
			/// <summary>
			/// Additional information for authorization is required.
			/// </summary>
			MoreAuthRequired = 4,
			/// <summary>
			/// Waiting for post of message.
			/// </summary>
			PostWaiting = 8,
			/// <summary>
			/// Waiting for transfer of message.
			/// </summary>
			TransferWaiting = 16
		}
	
		/// <summary>
		/// State of current session
		/// </summary>
		protected internal States sessionState = States.Normal;
		/// <summary>
		/// Associative arrays of NNTP client commands
		/// </summary>
		protected internal IDictionary<string, Generic> commands;

		protected IDataProvider _dataProvider;
		public IDataProvider DataProvider
		{
			get {return _dataProvider;}
		}
		/// <summary>
		/// connection timeout interval in milliseconds (5 min)
		/// </summary>
		protected const int connectionTimeout = 300000;

		/// <summary>
		/// free management resources
		/// </summary>
		public void Dispose()
		{
			// break circular connection
			_manager = null;
			// free data provider
			_dataProvider.Dispose();

			netStream.Close();
			if (_client.Connected)
			{
				_client.Shutdown(SocketShutdown.Both);
				_client.Close();
			}
			
			if (Disposed != null)
				Disposed(this, EventArgs.Empty);
		}

		public event EventHandler Disposed;

		protected static IDictionary<States, Response> notAllowedStateAnswer;
	}
}