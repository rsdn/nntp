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

namespace derIgel.NNTP
{
	using Util = derIgel.MIME.Util;

	/// <summary>
	/// NNTP Connection Manager 
	/// </summary>
	public class Manager : IDisposable
	{
		protected TextWriter errorOutput = System.Console.Error;

#if PERFORMANCE_COUNTERS
		/// <summary>
		/// sessions' perfomance counter
		/// </summary>
		PerformanceCounter sessionsCounter;
		/// <summary>
		/// global sessions' perfomance counter
		/// </summary>
		PerformanceCounter globalSessionsCounter;
#endif
    	
		/// <summary>
		/// NNTP Connection Manager constructor
		/// </summary>
		public Manager(NNTPSettings settings)
		{
			if (Array.BinarySearch(settings.DataProviderType.GetInterfaces(), typeof(IDataProvider)) < 0)
				throw new ArgumentException("dataProviderType is not implemented DataProvider interface.",
					"dataProviderType");

			this.settings = settings;
				
			if (settings.ErrorOutputFilename != null)
				errorOutput = new StreamWriter(settings.ErrorOutputFilename, false, System.Text.Encoding.ASCII);

			stopEvent = new ManualResetEvent(false);
			sessions = new ArrayList();

#if PERFORMANCE_COUNTERS
			// create perfomance counters' category if necessary
			string PerfomanceCategoryName = "RSDN NNTP Server Manager";
			CounterCreationDataCollection perfomanceCountersCollection = new CounterCreationDataCollection();
			CounterCreationData sessionsCounterData = new CounterCreationData("Sessions",
				"Count of client's sessions",	PerformanceCounterType.NumberOfItems32);
			perfomanceCountersCollection.Add(sessionsCounterData);
			if (!PerformanceCounterCategory.Exists(PerfomanceCategoryName))
				PerformanceCounterCategory.Create(PerfomanceCategoryName, "", perfomanceCountersCollection);
#endif

			listeners = new Socket[settings.Bindings.Length];
			for (int i = 0; i < settings.Bindings.Length; i++)
			{
				listeners[i] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				listeners[i].Bind(settings.Bindings[i].EndPoint);
				listeners[i].Listen(listenConnections);
			}

#if PERFORMANCE_COUNTERS
			// create perfomance counters
			sessionsCounter = new PerformanceCounter(PerfomanceCategoryName,
				sessionsCounterData.CounterName, settings.Name, false);
			globalSessionsCounter = new PerformanceCounter(PerfomanceCategoryName,
				sessionsCounterData.CounterName, "All", false);
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
						Session session = new Session(socket, dataProvider,	stopEvent, errorOutput);
						session.Disposed += new EventHandler(SessionDisposedHandler);
						sessions.Add(session);
						ThreadPool.QueueUserWorkItem(new WaitCallback(session.Process), this);
#if PERFORMANCE_COUNTERS
						sessionsCounter.Increment();
						globalSessionsCounter.Increment();
#endif
					}
				}
				// it's okay when we stopped manager (closed socket)
				// we can't cancel asynchronious callback
				catch(System.ObjectDisposedException)	{	}
				catch(Exception e)
				{
					#if DEBUG || SHOW
					errorOutput.WriteLine(e.ToString());
					#endif
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
			errorOutput.Flush();
			paused = true;
		}

		public void Resume()
		{
			paused = false;
		}

		public void Stop()
		{
			errorOutput.Flush();
			Close();
			stopEvent.Set();
			while (sessions.Count > 0)
				Thread.Sleep(sessionsCheckInterval);
		}

		protected ArrayList sessions;

		public void SessionDisposedHandler(object obj, EventArgs args)
		{
			sessions.Remove(obj);
#if PERFORMANCE_COUNTERS
			sessionsCounter.Decrement();
			globalSessionsCounter.Decrement();
#endif
		}

		protected const int listenConnections = 100;

		/// <summary>
		/// NNTP server settings
		/// </summary>
		protected NNTPSettings settings;

		public void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			// listener socket do not need shutdown
			foreach (Socket listener in listeners)
				listener.Close();			
		}

		/// <summary>
		/// Quantity of sesions
		/// </summary>
		public int SessionsQuantity
		{
			get {return sessions.Count;	}
		}

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