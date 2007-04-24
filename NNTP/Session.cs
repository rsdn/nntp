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
		/// Текущая пауза при ошибке
		/// </summary>
		private int _errorPauseDuration;
		private Timer _errorPauseTimer;

    /// <summary>
    /// Create user session
    /// </summary>
    /// <param name="client">Established client's socket</param>
    /// <param name="certificate">Certificate for SSL connection, NULL - if no SSL.</param>
    /// <param name="dataProvider">Data Provider</param>
    /// <param name="manager">Sessions' manager.</param>
    /// <remarks>By default - starting error pause 100msec</remarks>
		public Session(Socket client, X509Certificate2 certificate, IDataProvider dataProvider, Manager manager)
			: this(client, certificate, dataProvider, manager, 100)
		{
		}

		/// <summary>
    /// Create user session
    /// </summary>
    /// <param name="client">Established client's socket</param>
    /// <param name="certificate">Certificate for SSL connection, NULL - if no SSL.</param>
    /// <param name="dataProvider">Data Provider</param>
    /// <param name="manager">Sessions' manager.</param>
		/// <param name="pauseOnError">Начальная пауза после ошибки, если ноль - нет паузы
		/// <remarks>Пауза нарастающая, после каждой ошибки увелеичение на 20%</remarks></param>
		public Session(Socket client, X509Certificate2 certificate, IDataProvider dataProvider, Manager manager, int pauseOnError)
		{
      logger = LogManager.GetLogger(manager.Name);

      sessionState = dataProvider.InitialSessionState;

      commandBuffer = new byte[bufferSize];

      _certificate = certificate;
			_client = client;
    	_clientID = client.RemoteEndPoint.ToString();
			_errorPauseDuration = pauseOnError;
			_errorPauseTimer = new Timer(AfterErrorPause);

      netStream = new NetworkStream(client);
      if (certificate != null)
        netStream = new SslStream(netStream, false);
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

    protected void AnswerWithCheck(NntpResponse code)
    {
      if (!netStream.CanWrite)
        return;

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

		public void Process(Object origObj)
		{
			ExceptionHandler(
				(WaitCallback)delegate(Object obj)
				{
					logger.InfoFormat("Session started. Local end point {0}.",
						_client.LocalEndPoint);

					// if SSL - start authentification
					if (netStream is SslStream)
					{
						SslStream sslStream = (SslStream)netStream;
						sslStream.BeginAuthenticateAsServer(_certificate, false,
							SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls, false, SslAuthDone, sslStream);
					}
					else
					{
						Start();
					}
				}, origObj);
		}

		private void ExceptionHandler(Delegate method, params object[] parameters)
		{
			NDC.Push(_clientID);
			try
			{
				if (method is WaitOrTimerCallback)
				{
					((WaitOrTimerCallback)method)(parameters[0], (bool)parameters[1]);
				}
				else if (method is WaitCallback)
				{
					((WaitCallback)method)(parameters[0]);
				}
				else if (method is AsyncCallback)
				{
					((AsyncCallback)method)((IAsyncResult)parameters[0]);
				}
				else
					method.DynamicInvoke(parameters);
			}
			catch (TargetInvocationException targetEx)
			{
				ExceptionHandlerInfo(
					(targetEx.InnerException != null) ? targetEx.InnerException : targetEx);
				SessionEnd();
			}
			catch (Exception ex)
			{
				ExceptionHandlerInfo(ex);
				SessionEnd();
			}
			finally
			{
				NDC.Pop();
			}
		}

		private void ExceptionHandlerInfo(Exception exception)
		{
			if (exception is AuthenticationException)
			{
				logger.WarnFormat("SSL auth failed: {0}", exception.Message);
			}
			else if ((exception is IOException) || (exception is SocketException))
			{
				// network error
				logger.WarnFormat("Network error: {0}", exception.Message);
			}
			else
			{
				// something wrong......
				logger.Fatal("Fatal error", exception);
			}
		}

		protected void AfterErrorPause(object origState)
		{
			ExceptionHandler((WaitCallback)
				delegate(object state)
     		{
					// if we still have unprocessed comannds after error - 
					// do not read new data - process those first
     			int dataToRead = bufferString.Length > 0 ? 0 : bufferSize;
					// read more data
					IAsyncResult readAsync =
						netStream.BeginRead(commandBuffer, 0, dataToRead, null, null);
					ThreadPool.RegisterWaitForSingleObject(readAsync.AsyncWaitHandle,
						ProcessData, readAsync, connectionTimeout, true);
     		}, origState);
		}

		protected void SslAuthDone(IAsyncResult origAsync)
		{
			ExceptionHandler((AsyncCallback)
				delegate(IAsyncResult async)
				{
					((SslStream)async.AsyncState).EndAuthenticateAsServer(async);
					Start();
				}, origAsync);
		}

		protected void Start()
		{
			// response OK
			Answer(new Response(_dataProvider.PostingAllowed ?
				NntpResponse.Ok : NntpResponse.OkNoPosting, null,
					string.Format("{0} ({1}; {2})",
					_manager.Name, Manager.ServerID, _dataProvider.Identity)));

			sessionState = States.Normal;

			IAsyncResult readAsync =
				netStream.BeginRead(commandBuffer, 0, bufferSize, null, null);
			ThreadPool.RegisterWaitForSingleObject(readAsync.AsyncWaitHandle,
				ProcessData, readAsync, connectionTimeout, true);
			ThreadPool.RegisterWaitForSingleObject(_manager.ExitEvent,
				ProcessData, null, -1, true);
		}

		private void SessionEnd()
		{
			logger.Info("Session finished");
			Dispose();
		}

		private StringBuilder bufferString = new StringBuilder();

		/// <summary>
		/// Process client
		/// </summary>
		protected void ProcessData(Object origObj, bool origTimeout)
		{
			ExceptionHandler((WaitOrTimerCallback)
				delegate(Object obj, bool timeout)
				{
					lock (bufferString)
					{
						if (timeout)
						{
							AnswerWithCheck(NntpResponse.TimeOut);
							SessionEnd();
							return;
						}

						if (obj == null)
						{
							// terminate session
							AnswerWithCheck(NntpResponse.ServiceDiscontinued);
							SessionEnd();
							return;
						}

						int receivedBytes;
						try
						{
							receivedBytes = netStream.EndRead((IAsyncResult) obj);
						}
						// okay only in this place (can't cancel async read)
						catch (ObjectDisposedException)
						{
							SessionEnd();
							return;
						}

						if (receivedBytes == 0)
						{
							// stream is closed
							SessionEnd();
							return;
						}

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
						string delimeter;
						Response result = null;

						switch (sessionState)
						{
							case States.PostWaiting:
							case States.TransferWaiting:
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
									case States.PostWaiting:
									case States.TransferWaiting:
										try
										{
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
										sessionState = States.Normal;
										break;
									default:
										// get first word in upper case delimeted by space or tab characters 
										command = commandString.Split(new char[] { ' ', '\t', '\r' }, 2)[0].ToUpper();

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
											logger.ErrorFormat("Command not recognized ({0}).", commandString);
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
										logger.WarnFormat("{0} provider don't support {1} command.",
											_dataProvider.GetType(), command);
										break;
									case DataProviderErrors.PostingFailed:
										result = new Response(sessionState == States.TransferWaiting ?
											NntpResponse.TransferFailed : NntpResponse.PostingFailed, null,
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
								logger.WarnFormat("Data Provider Error ({0}): {1}",
									exception.Error, exception.Message);
							}
							catch (AuthenticationException authException)
							{
								logger.WarnFormat("SSL auth error: ", authException.Message);
								SessionEnd();
								return;
							}
							// not good....
							catch (Exception e)
							{
								logger.Error(
									string.Format("Exception during processing command" +
									" (selected group '{0}', last request '{1}').\n",
									currentGroup, commandString), e);
								result = new Response(NntpResponse.ProgramFault, null, e.Message);
							}

							Answer(result);

							// debug tracing
							logger.Debug(result);

							// result code indicates error
							if (result.Code >= 400)
							{
								switch (sessionState)
								{
									case States.PostWaiting:
									case States.TransferWaiting:
										sessionState = States.Normal;
										break;
								}
#if PERFORMANCE_COUNTERS_OLD
							badRequestsCounter.Increment();
							globalBadRequestsCounter.Increment();
#endif
							}

							switch ((NntpResponse)result.Code)
							{
								case NntpResponse.ArticleHeadBodyRetrivied:
								case NntpResponse.ArticleHeadRetrivied:
								case NntpResponse.ArticleBodyRetrivied:
								case NntpResponse.ArticleNothingRetrivied:
#if PERFORMANCE_COUNTERS_OLD
								articlesCounter.Increment();
								globalArticlesCounter.Increment();
#endif
									break;
								case NntpResponse.Bye: // quit
								case NntpResponse.ServiceDiscontinued: // service disctontined
								case NntpResponse.ServiceUnaviable: // service unaviable
								case NntpResponse.TimeOut: // timeout
									SessionEnd();
									return;
							}

							// pause on error, if necesary and enabled
							if ((result.Code >= 400) && (_errorPauseDuration > 0))
							{
								_errorPauseTimer.Change(_errorPauseDuration, Timeout.Infinite);
								// врядли кто-нибудь дождётся паузы,
								// когда int близок к максимальному значению -
								// так что переполнение не проверяем.
								_errorPauseDuration = (int)(_errorPauseDuration * 1.2);
								return;
							}
						}

						// read more data
						IAsyncResult readAsync =
							netStream.BeginRead(commandBuffer, 0, bufferSize, null, null);
						ThreadPool.RegisterWaitForSingleObject(readAsync.AsyncWaitHandle,
							ProcessData, readAsync, connectionTimeout, true);
					}
				}, origObj, origTimeout);
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

		private string _clientID;

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
		protected internal States sessionState = States.None;
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