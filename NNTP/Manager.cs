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

		/// <summary>
		/// sessions' perfomance counter
		/// </summary>
		PerformanceCounter sessionsCounter;
		/// <summary>
		/// global sessions' perfomance counter
		/// </summary>
		PerformanceCounter globalSessionsCounter;
    	
		/// <summary>
		/// NNTP Connection Manager constructor
		/// dataProvider is provider of data
		/// </summary>
		public Manager(Type dataProviderType, NNTPSettings settings)
		{
			if (Array.BinarySearch(dataProviderType.GetInterfaces(), typeof(IDataProvider)) < 0)
				throw new ArgumentException("dataProviderType is not implemented DataProvider interface.",
					"dataProviderType");

			this.dataProviderType = dataProviderType;
			this.settings = settings;
				
			if (settings.ErrorOutputFilename != null)
				errorOutput = new StreamWriter(settings.ErrorOutputFilename, false, System.Text.Encoding.ASCII);

			stopEvent = new ManualResetEvent(false);
			sessions = new ArrayList();

			// create perfomance counters' category if necessary
			string PerfomanceCategoryName = "RSDN NNTP Server Manager";
			CounterCreationDataCollection perfomanceCountersCollection = new CounterCreationDataCollection();
			CounterCreationData sessionsCounterData = new CounterCreationData("Sessions",
				"Count of client's sessions",	PerformanceCounterType.NumberOfItems32);
			perfomanceCountersCollection.Add(sessionsCounterData);
			if (!PerformanceCounterCategory.Exists(PerfomanceCategoryName))
				PerformanceCounterCategory.Create(PerfomanceCategoryName, "", perfomanceCountersCollection);

			listener = new Socket(settings.EndPoint.AddressFamily, SocketType.Stream,
				ProtocolType.Tcp);
			listener.Bind(settings.EndPoint);
			listener.Listen(listenConnections);

			// create perfomance counters
			sessionsCounter = new PerformanceCounter(PerfomanceCategoryName,
				sessionsCounterData.CounterName, "Port " + settings.Port, false);
			globalSessionsCounter = new PerformanceCounter(PerfomanceCategoryName,
				sessionsCounterData.CounterName, "All", false);

			Start();
		}

		/// <summary>
		/// TCP Port Listener
		/// </summary>
		protected Socket listener = null;

		/// <summary>
		/// Start work
		/// </summary>
		public void Start()
		{
			stopEvent.Reset();
			try
			{
				listener.BeginAccept(new AsyncCallback(AcceptClient), null);
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
					// get client's socket
					Socket socket = listener.EndAccept(ar);
					// start listen for next client
					listener.BeginAccept(new AsyncCallback(AcceptClient), null);
					if (paused)
					{
						Response.Answer(401, socket);
						socket.Shutdown(SocketShutdown.Both);
						socket.Close();
					}
					else
					{
						IDataProvider dataProvider = Activator.CreateInstance(dataProviderType) as IDataProvider;
						dataProvider.Config(settings);
						Session session = new Session(socket, dataProvider,	stopEvent, errorOutput);
						session.Disposed += new EventHandler(SessionDisposedHandler);
						sessions.Add(session);
						ThreadPool.QueueUserWorkItem(new WaitCallback(session.Process), this);
						sessionsCounter.Increment();
						globalSessionsCounter.Increment();
					}
				}
				// it's okay when we stopped manager (closed socket)
				// we can't cancel asynchronious callback
				catch(System.ObjectDisposedException)	{	}
				catch(Exception e)
				{
					#if DEBUG || SHOW
					errorOutput.WriteLine(Util.ExpandException(e));
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
		/// type of class for providing of data
		/// </summary>
		protected Type dataProviderType;
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
			paused = false;
		}

		public void Resume()
		{
			paused = true;
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
			sessionsCounter.Decrement();
			globalSessionsCounter.Decrement();
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