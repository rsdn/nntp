using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using log4net;

namespace Rsdn.Nntp
{
	/// <summary>
	/// NNTP Connection Manager 
	/// </summary>
	public class Manager : IDisposable
	{
#if PERFORMANCE_COUNTERS
		/// <summary>
		/// RSDN NNTP Server performance counters' category
		/// </summary>
		public const string ServerCategoryName = "RSDN NNTP Server";
		/// <summary>
		/// Summarized instance name for counters 
		/// </summary>
		public const string GlobalInstanceName = "_All";
		
		/// global performance counters collection
		protected static IDictionary<string, PerformanceCounter> globalPerformanceCounters =
			new Dictionary<string, PerformanceCounter>();
		/// <summary>
		/// Get specified global performance counter
		/// </summary>
		/// <param name="name">Counter's name</param>
		/// <returns>Specified counter</returns>
		public static PerformanceCounter GetGlobalPerformanceCounter(string name)
		{
			return globalPerformanceCounters[name];
		}

		/// instance performance counters collection
		protected IDictionary<string, PerformanceCounter> performanceCounters =
			new Dictionary<string, PerformanceCounter>();
		/// <summary>
		/// Get specified instance performance counter
		/// </summary>
		/// <param name="name">Counter's name</param>
		/// <returns>Specified counter</returns>
		public PerformanceCounter GetPerformanceCounter(string name)
		{
			return performanceCounters[name];
		}

		/// Connections performance counter
		public const string connectionsCounterName = "Current Connections";
		/// Max Connections performance counter
		public const string maxConnectionsCounterName = "Maximum Connections";
		/// Bytes Received per sec
		public const string bytesReceivedPerSecCounterName = "Bytes Received / sec";
		/// Bytes Received
		public const string bytesReceivedCounterName = "Bytes Received";
		/// Bytes Sent per sec
		public const string bytesSentPerSecCounterName = "Bytes Sent / sec";
		/// Bytes Sent
		public const string bytesSentCounterName = "Bytes Sent";
		/// Bytes Total per sec
		public const string bytesTotalPerSecCounterName = "Bytes Total / sec";
		/// Bytes Total
		public const string bytesTotalCounterName = "Bytes Total";

		protected static string[] performanceCountersNames =
			{
				connectionsCounterName,
				maxConnectionsCounterName,
				bytesReceivedPerSecCounterName,
				bytesReceivedCounterName,
				bytesSentPerSecCounterName,
				bytesSentCounterName,
				bytesTotalPerSecCounterName,
				bytesTotalCounterName,
		};
#endif

		/// <summary>
		/// Logger
		/// </summary>
		protected ILog logger;

		static Manager()
		{
#if PERFORMANCE_COUNTERS
			// create performance counters' category if necessary
			if (!PerformanceCounterCategory.Exists(ServerCategoryName ))
			{
				var perfomanceCountersCollection = new CounterCreationDataCollection();

				// connections
				perfomanceCountersCollection.Add(
					new CounterCreationData(connectionsCounterName,
						"Number of client's connections",	PerformanceCounterType.NumberOfItems32));
				perfomanceCountersCollection.Add(
					new CounterCreationData(maxConnectionsCounterName,
						"Maximum number of client's connections",	PerformanceCounterType.NumberOfItems32));
				// bytes received
				perfomanceCountersCollection.Add(
					new CounterCreationData(bytesReceivedPerSecCounterName,
						"Received bytes rate",	PerformanceCounterType.RateOfCountsPerSecond32));
				perfomanceCountersCollection.Add(
					new CounterCreationData(bytesReceivedCounterName,
						"Received bytes",	PerformanceCounterType.NumberOfItems32));
				// bytes sent
				perfomanceCountersCollection.Add(
					new CounterCreationData(bytesSentPerSecCounterName,
						"Sent bytes rate",	PerformanceCounterType.RateOfCountsPerSecond32));
				perfomanceCountersCollection.Add(
					new CounterCreationData(bytesSentCounterName,
						"Sent bytes",	PerformanceCounterType.NumberOfItems32));
				// bytes total
				perfomanceCountersCollection.Add(
					new CounterCreationData(bytesTotalPerSecCounterName,
						"Total bytes rate",	PerformanceCounterType.RateOfCountsPerSecond32));
				perfomanceCountersCollection.Add(
					new CounterCreationData(bytesTotalCounterName,
						"Total bytes",	PerformanceCounterType.NumberOfItems32));

				PerformanceCounterCategory.Create(ServerCategoryName, "",
					PerformanceCounterCategoryType.MultiInstance, perfomanceCountersCollection);
			}

			// create global performance counters
			CreatePerformanceCounters(globalPerformanceCounters, GlobalInstanceName);
#endif
		}

#if PERFORMANCE_COUNTERS
		private static void CreatePerformanceCounters(
			IDictionary<string, PerformanceCounter> store, string instanceName)
		{
			foreach (var counterName in performanceCountersNames)
				store.Add(counterName,
					new PerformanceCounter(ServerCategoryName, counterName, instanceName, false));
		}
#endif

		/// <summary>
		/// NNTP Connection Manager constructor
		/// </summary>
		public Manager(NntpSettings settings)
		{
			if (!typeof(IDataProvider).IsAssignableFrom(settings.DataProviderType))
				throw new ArgumentException("DataProviderType in settings object is not implemented DataProvider interface.",
					"settings");

			this.settings = settings;
			logger = LogManager.GetLogger(settings.Name);

			stopEvent = new ManualResetEvent(false);
			sessions = new List<Session>();

#if PERFORMANCE_COUNTERS
			// create performance counters
			CreatePerformanceCounters(performanceCounters, settings.Name);
#endif
		}

		/// <summary>
		/// TCP Port Listener
		/// </summary>
		protected Socket[] listeners;

		/// <summary>
		/// Start work
		/// </summary>
		public void Start()
		{
			listeners = new Socket[settings.Bindings.Length];
			for (var i = 0; i < settings.Bindings.Length; i++)
			{
				listeners[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				listeners[i].Bind(settings.Bindings[i].EndPoint);
				listeners[i].Listen(listenConnections);
				listeners[i].BeginAccept(new AsyncCallback(AcceptClient), i);
			}
			stopEvent.Reset();

			// trace start message
			if (logger.IsInfoEnabled)
			{
				var startInfo = new StringBuilder("Server started. Listen on ");
				for (var i = 0; i < listeners.Length; i++)
				{
					if (i > 0) startInfo.Append(',');
					startInfo.Append(listeners[i].LocalEndPoint);
				}
				logger.Info(startInfo.Append('.'));
			}
		}


		/// <summary>
		/// Accept incoming connections
		/// </summary>
		protected void AcceptClient(IAsyncResult ar)
		{
			try
			{
				var bindingIndex = (int)ar.AsyncState;

				// listen sockets already shutdowned
				if (listeners.Length <= bindingIndex)
					return;

				// get listener socket
				var listener = listeners[bindingIndex];
				// get client's socket
				var socket = listener.EndAccept(ar);
				// start listen for next client
				listener.BeginAccept(new AsyncCallback(AcceptClient), bindingIndex);
				if (paused)
				{
					Response.Answer(NntpResponse.ServiceUnaviable, socket);
					socket.Shutdown(SocketShutdown.Both);
					socket.Close();
				}
				else
				{
					var dataProvider = (IDataProvider)Activator.CreateInstance(settings.DataProviderType);
					dataProvider.Config(settings.DataProviderSettings);
					var session =
						new Session(socket, settings.Bindings[bindingIndex].Certificate, dataProvider, this);
					session.Disposed += SessionDisposedHandler;
					sessions.Add(session);
					// reset event (now we have child sessions)
					noSessions.Reset();

#if PERFORMANCE_COUNTERS
					// set connections counter
					var connectionsCounter =
						GetPerformanceCounter(connectionsCounterName);
					connectionsCounter.Increment();
					GetGlobalPerformanceCounter(connectionsCounterName).Increment();

					// set max connections counter
					var maxConnectionsCounter =
						GetPerformanceCounter(maxConnectionsCounterName);
					if (connectionsCounter.RawValue > maxConnectionsCounter.RawValue)
						maxConnectionsCounter.RawValue = connectionsCounter.RawValue;

					// set global max connections counter
					var globalMaxConnectionsCounter =
						GetGlobalPerformanceCounter(maxConnectionsCounterName);
					if (maxConnectionsCounter.RawValue > globalMaxConnectionsCounter.RawValue)
						globalMaxConnectionsCounter.RawValue = maxConnectionsCounter.RawValue;
#endif

					session.Process(this);

				}
			}
			// The socket is closed.
			catch (ObjectDisposedException) { }

			// The socket was closed before its connection was accepted.
			catch (SocketException) { }
		}

		/// <summary>
		/// enforce close child sessions after this timeout
		/// </summary>
		protected const int waitSessionsTimeout = 30000;

		/// <summary>
		/// signalled when need to pause
		/// </summary>
		protected bool paused;
		/// <summary>
		/// signalled when need to stop
		/// </summary>
		protected ManualResetEvent stopEvent;
		/// <summary>
		/// Signalled when need to stop. Used by child sessions.
		/// </summary>
		internal WaitHandle ExitEvent
		{
			get { return stopEvent; }
		}

		/// <summary>
		/// Stop accept new clients.
		/// Continue work with current clients.
		/// </summary>
		public void Pause()
		{
			paused = true;
			logger.Info("Server paused.");
		}

		/// <summary>
		/// Resume accept clients after pause.
		/// </summary>
		public void Resume()
		{
			paused = false;
			logger.Info("Server resumed.");
		}

		/// <summary>
		/// Stop listen & accept clients
		/// </summary>
		public void Stop()
		{
			Dispose();
			stopEvent.Set();
			if (!noSessions.WaitOne(waitSessionsTimeout, false))
			{
				sessions.Clear();
				logger.Info("Server forced closing of child sessions.");
			}
			logger.Info("Server stopped.");
		}

		/// <summary>
		/// array contains references to child nntp sessions
		/// </summary>
		protected IList<Session> sessions;

		/// <summary>
		/// event, signalled when no child sessions
		/// </summary>
		protected ManualResetEvent noSessions = new ManualResetEvent(true);

		public void SessionDisposedHandler(object obj, EventArgs args)
		{
			// check to ensure taht we have that object
			if (sessions.Contains((Session)obj))
			{
				// remove session from array
				sessions.Remove((Session)obj);
				// signal when there are no sessions
				if (sessions.Count == 0)
					noSessions.Set();

#if PERFORMANCE_COUNTERS
				GetPerformanceCounter(connectionsCounterName).Decrement();
				GetGlobalPerformanceCounter(connectionsCounterName).Decrement();
#endif
			}
		}

		/// <summary>
		/// length of queue of pending connections
		/// </summary>
		protected const int listenConnections = 100;

		/// <summary>
		/// NNTP server settings
		/// </summary>
		protected NntpSettings settings;

		/// <summary>
		/// free resources (end all child sessions) 
		/// </summary>
		public void Dispose()
		{
			// listener socket do not need shutdown
			foreach (var listener in listeners)
				listener.Close();
			listeners = new Socket[0];
		}

		/// <summary>
		/// Quantity of sesions
		/// </summary>
		public int SessionsQuantity
		{
			get { return sessions.Count; }
		}

		/// <summary>
		/// Common server identification string
		/// </summary>
		public static readonly string ServerID = GetProductTitle(Assembly.GetExecutingAssembly());

		/// <summary>
		/// Get product's title from assembly (AssemblyProduct + AssemblyInformationalVersion)
		/// </summary>
		/// <param name="assembly">Assembly</param>
		/// <returns>Title if assembly</returns>
		public static string GetProductTitle(Assembly assembly)
		{
			var builder = new StringBuilder();

			var productName = (AssemblyProductAttribute)
				Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

			var productVersion = (AssemblyInformationalVersionAttribute)
				Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute));

			if (productName != null)
				builder.Append(productName.Product).Append(" ");

			if (productVersion != null)
				builder.Append(productVersion.InformationalVersion);

			return builder.ToString();
		}

		/// <summary>
		/// Server name
		/// </summary>
		public string Name
		{
			get { return settings.Name; }
			set { settings.Name = value; }
		}
	}
}