using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Configuration;
using System.Text;
using System.Reflection;
using System.IO;

namespace Rsdn.Nntp
{
	using Util = Rsdn.Mime.Util;

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
		
		/// Connections performance counter
		protected const string connectionsCounterName = "Current Connections";
		protected static PerformanceCounter globalConnectionsCounter;
		protected PerformanceCounter connectionsCounter;
		/// Max Connections performance counter
		protected const string maxConnectionsCounterName = "Maximum Connections";
		protected static PerformanceCounter globalMaxConnectionsCounter;
		protected PerformanceCounter maxConnectionsCounter;
		/// Bytes Received per sec
		protected const string bytesReceivedCounterName = "Bytes Received / sec";
		protected internal static PerformanceCounter globalBytesReceivedCounter;
		protected internal PerformanceCounter bytesReceivedCounter;
		/// Bytes Sent per sec
		protected const string bytesSentCounterName = "Bytes Sent / sec";
		protected internal static PerformanceCounter globalBytesSentCounter;
		protected internal PerformanceCounter bytesSentCounter;
		/// Bytes Total per sec
		protected const string bytesTotalCounterName = "Bytes Total / sec";
		protected internal static PerformanceCounter globalBytesTotalCounter;
		protected internal PerformanceCounter bytesTotalCounter;
#endif

		protected static readonly TraceSwitch tracing;

		static Manager()
		{
			// tracing
			tracing = new TraceSwitch("Show", "RSDN NNTP Server Tracing");

#if PERFORMANCE_COUNTERS
			// create performance counters' category if necessary
			if (!PerformanceCounterCategory.Exists(ServerCategoryName ))
			{
				CounterCreationDataCollection perfomanceCountersCollection = new CounterCreationDataCollection();
				// connections
				CounterCreationData connectionsCounterData = new CounterCreationData(connectionsCounterName,
					"Number of client's connections",	PerformanceCounterType.NumberOfItems32);
				perfomanceCountersCollection.Add(connectionsCounterData);
				CounterCreationData maxConnectionsCounterData = new CounterCreationData(maxConnectionsCounterName,
					"Maximum number of client's connections",	PerformanceCounterType.NumberOfItems32);
				perfomanceCountersCollection.Add(maxConnectionsCounterData);
				// bytes
				CounterCreationData bytesReceivedCounterData = new CounterCreationData(bytesReceivedCounterName,
					"Received bytes rate",	PerformanceCounterType.RateOfCountsPerSecond32);
				perfomanceCountersCollection.Add(bytesReceivedCounterData);
				CounterCreationData bytesSentCounterData = new CounterCreationData(bytesSentCounterName,
					"Sent bytes rate",	PerformanceCounterType.RateOfCountsPerSecond32);
				perfomanceCountersCollection.Add(bytesSentCounterData);
				CounterCreationData bytesTotalCounterData = new CounterCreationData(bytesTotalCounterName,
					"Total bytes rate",	PerformanceCounterType.RateOfCountsPerSecond32);
				perfomanceCountersCollection.Add(bytesTotalCounterData);
				PerformanceCounterCategory.Create(ServerCategoryName , "", perfomanceCountersCollection);
			}

			/// create global performance counters
			// connections
			globalConnectionsCounter = new PerformanceCounter(ServerCategoryName, connectionsCounterName,
				GlobalInstanceName, false);
			globalMaxConnectionsCounter = new PerformanceCounter(ServerCategoryName, maxConnectionsCounterName,
				GlobalInstanceName, false);
			// bytes
			globalBytesReceivedCounter = new PerformanceCounter(ServerCategoryName, bytesReceivedCounterName,
				GlobalInstanceName, false);
			globalBytesSentCounter = new PerformanceCounter(ServerCategoryName, bytesSentCounterName,
				GlobalInstanceName, false);
			globalBytesTotalCounter = new PerformanceCounter(ServerCategoryName, bytesTotalCounterName,
				GlobalInstanceName, false);
#endif
		}

		/// <summary>
		/// NNTP Connection Manager constructor
		/// </summary>
		public Manager(NntpSettings settings)
		{
			if (!typeof(IDataProvider).IsAssignableFrom(settings.DataProviderType))
				throw new ArgumentException("DataProviderType in settings object is not implemented DataProvider interface.",
					"settings");

			this.settings = settings;
				
			stopEvent = new ManualResetEvent(false);
			sessions = new ArrayList();

			listeners = new Socket[settings.Bindings.Length];
			for (int i = 0; i < settings.Bindings.Length; i++)
			{
				listeners[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				listeners[i].Bind(settings.Bindings[i].EndPoint);
				listeners[i].Listen(listenConnections);
			}

#if PERFORMANCE_COUNTERS
			/// create performance counters
			// connections
			connectionsCounter = new PerformanceCounter(ServerCategoryName, connectionsCounterName,
				settings.Name, false);
			maxConnectionsCounter = new PerformanceCounter(ServerCategoryName, maxConnectionsCounterName,
				settings.Name, false);
			// bytes
			bytesReceivedCounter = new PerformanceCounter(ServerCategoryName, bytesReceivedCounterName,
				settings.Name, false);
			bytesSentCounter = new PerformanceCounter(ServerCategoryName, bytesSentCounterName,
				settings.Name, false);
			bytesTotalCounter = new PerformanceCounter(ServerCategoryName, bytesTotalCounterName,
				settings.Name, false);
#endif

			Start();
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
			stopEvent.Reset();
			try
			{
				foreach (Socket listener in listeners)
					listener.BeginAccept(new AsyncCallback(AcceptClient), listener);
			}
			// it's okay when we stopped manager (closed socket)
			// we can't cancel asynchronious callback
			catch(System.ObjectDisposedException)	{	}

			Trace.WriteIf(tracing.TraceInfo, "Server started", settings.Name);
		}

		
		/// <summary>
		/// Accept incoming connections
		/// </summary>
		protected void AcceptClient(IAsyncResult ar)
		{
			lock (this)
			{
				try
				{
					// get listener socket
					Socket listener = (Socket)ar.AsyncState;
					// get client's socket
					Socket socket = listener.EndAccept(ar);
					// start listen for next client
					listener.BeginAccept(new AsyncCallback(AcceptClient), listener);
					if (paused)
					{
						Response.Answer(NntpResponse.ServiceUnaviable, socket);
						socket.Shutdown(SocketShutdown.Both);
						socket.Close();
					}
					else
					{
						IDataProvider dataProvider = Activator.CreateInstance(settings.DataProviderType) as IDataProvider;
						dataProvider.Config(settings.DataProviderSettings);
						Session session = new Session(socket, dataProvider,	stopEvent);
						session.Disposed += new EventHandler(SessionDisposedHandler);
						sessions.Add(session);
						ThreadPool.QueueUserWorkItem(new WaitCallback(session.Process), this);
#if PERFORMANCE_COUNTERS
						connectionsCounter.Increment();
						globalConnectionsCounter.Increment();
						if (connectionsCounter.RawValue > maxConnectionsCounter.RawValue)
						{
							globalMaxConnectionsCounter.
								IncrementBy(connectionsCounter.RawValue - maxConnectionsCounter.RawValue);
							maxConnectionsCounter.RawValue = connectionsCounter.RawValue;
						}
#endif
					}
				}
				// it's okay when we stopped manager (closed socket)
				// we can't cancel asynchronious callback
				catch(System.ObjectDisposedException)	{	}
				catch(Exception e)
				{
					Trace.Fail(e.ToString());
					throw;
				}
			}
		}

		/// <summary>
		/// timeout for check of endng of sessions
		/// </summary>
		protected const int sessionsCheckInterval = 500;

		/// <summary>
		/// signalled when need to pause
		/// </summary>
		protected bool paused = false;
		/// <summary>
		/// signalled when need to stop
		/// </summary>
		protected ManualResetEvent stopEvent;

		public void Pause()
		{
			paused = true;
			Trace.WriteIf(tracing.TraceInfo, "Server paused", settings.Name);
		}

		public void Resume()
		{
			paused = false;
			Trace.WriteIf(tracing.TraceInfo, "Server resumed", settings.Name);
		}

		public void Stop()
		{
			Close();
			stopEvent.Set();
			while (sessions.Count > 0)
				Thread.Sleep(sessionsCheckInterval);
			Trace.WriteIf(tracing.TraceInfo, "Server stopped", settings.Name);
		}

		protected ArrayList sessions;

		public void SessionDisposedHandler(object obj, EventArgs args)
		{
			sessions.Remove(obj);
#if PERFORMANCE_COUNTERS
			connectionsCounter.Decrement();
			globalConnectionsCounter.Decrement();
#endif
		}

		protected const int listenConnections = 100;

		/// <summary>
		/// NNTP server settings
		/// </summary>
		protected NntpSettings settings;

		public void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			// listener socket do not need shutdown
			foreach (Socket listener in listeners)
				listener.Close();			
#if PERFORMANCE_COUNTERS
			globalMaxConnectionsCounter.IncrementBy(-maxConnectionsCounter.RawValue);
			// enough remove only one counter with specified instance?
			connectionsCounter.RemoveInstance();
			maxConnectionsCounter.RemoveInstance();
#endif
		}

		/// <summary>
		/// Quantity of sesions
		/// </summary>
		public int SessionsQuantity
		{
			get {return sessions.Count;	}
		}

		/// server identification string
		public static readonly string ServerID = Manager.GetProductTitle(Assembly.GetExecutingAssembly());

		public static string GetProductTitle(Assembly assembly)
		{
			StringBuilder builder = new StringBuilder();
			
			AssemblyProductAttribute productName = (AssemblyProductAttribute)
				Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

			AssemblyInformationalVersionAttribute productVersion = (AssemblyInformationalVersionAttribute)
				Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute));

			if (productName != null)
				builder.Append(productName.Product).Append(" ");

			if (productVersion != null)
				builder.Append(productVersion.InformationalVersion);
			
			return builder.ToString();
		}
	}
}