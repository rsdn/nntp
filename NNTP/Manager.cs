using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Configuration;

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

		protected static Statistics stat;
		protected static string statFilename;

		static Manager()
		{
			statFilename = Assembly.GetExecutingAssembly().GetName().Name + ".stat";
			if (File.Exists(statFilename))
				stat = Statistics.Deserialize(statFilename);
			else
				stat = new Statistics();
		}

		/// <summary>
		/// Write statistics at the end
		/// </summary>
		~Manager()
		{
			stat.Serialize(statFilename);
		}

		/// <summary>
		/// sessions' perfomance counter
		/// </summary>
		PerformanceCounter sessionsCounter;
	
		/// <summary>
		/// NNTP Connection Manager constructor
		/// dataProvider is provider of data
		/// </summary>
		public Manager(Type dataProviderType, NNTPSettings settings)
		{
			if (!dataProviderType.IsSubclassOf(typeof(DataProvider)))
				throw new ArgumentException("dataProviderType is not inherited from DataProvider class",
					"dataProviderType");
			if (dataProviderType.IsAbstract)
				throw new ArgumentException("dataProviderType is abstract class",
					"dataProviderType");

			this.dataProviderType = dataProviderType;
			this.settings = settings;
				
			stat.fromMail = settings.FromMail;
			stat.toMail = settings.ToMail;
			stat.fromServer = settings.FromServer;
			stat.interval = new TimeSpan(0, settings.IntervalMinutes, 0);

			if (settings.ErrorOutputFilename != null)
				errorOutput = new StreamWriter(settings.ErrorOutputFilename, false, System.Text.Encoding.ASCII);

			stopEvent = new ManualResetEvent(false);
			sessions = new ArrayList();

			// create perfomance counters' category if necessary
			CounterCreationDataCollection perfomanceCountersCollection = new CounterCreationDataCollection();
			CounterCreationData sessionsCounterData = new CounterCreationData("Sessions",
				"Count of client's sessions",	PerformanceCounterType.NumberOfItems32);
			perfomanceCountersCollection.Add(sessionsCounterData);
			string categoryName = "RSDN NNTP Server";
			if (!PerformanceCounterCategory.Exists(categoryName))
				PerformanceCounterCategory.Create(categoryName, "", perfomanceCountersCollection);

			listener = new Socket(settings.EndPoint.AddressFamily, SocketType.Stream,
				ProtocolType.Tcp);
			listener.Bind(settings.EndPoint);
			listener.Listen(listenConnections);

			// create perfomance counters
			sessionsCounter = new PerformanceCounter(categoryName,
				sessionsCounterData.CounterName, "Port " + settings.Port, false);

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
						DataProvider dataProvider = Activator.CreateInstance(dataProviderType,
							new object[]{settings}) as DataProvider;
						Session session = new Session(socket, dataProvider,	stopEvent,
							stat, errorOutput);
						session.Disposed += new EventHandler(SessionDisposedHandler);
						sessions.Add(session);
						ThreadPool.QueueUserWorkItem(new WaitCallback(session.Process), this);
						sessionsCounter.Increment();
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
	}
}