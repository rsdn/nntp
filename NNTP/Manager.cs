using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Configuration;

using System.Reflection;
using System.IO;

namespace derIgel
{
	namespace NNTP
	{
		/// <summary>
		/// NNTP Connection Manager 
		/// </summary>
		public class Manager
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

				stopEvent = new ManualResetEvent(true);
				pauseEvent = new ManualResetEvent(false);
				sessions = new ArrayList();

				listener = new Socket(settings.EndPoint.AddressFamily, SocketType.Stream,
					ProtocolType.Tcp);
				listener.Bind(settings.EndPoint);
				listener.Listen(listenConnections);
				listener.BeginAccept(new AsyncCallback(AcceptClient), null);

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
						listener.BeginAccept(new AsyncCallback(AcceptClient), null);
						Socket socket = listener.EndAccept(ar);
						switch (WaitHandle.WaitAny(new WaitHandle[] {stopEvent, pauseEvent}, 0, false))
						{
							case	WaitHandle.WaitTimeout	:
								DataProvider dataProvider = Activator.CreateInstance(dataProviderType,
									new object[]{settings}) as DataProvider;
								Session session = new Session(socket, dataProvider,	stopEvent,
									stat, errorOutput);
								session.Disposed += new EventHandler(SessionDisposedHandler);
								sessions.Add(session);
								ThreadPool.QueueUserWorkItem(new WaitCallback(session.Process));
								break;
							case 1	:
								Response.Answer(401, socket);
								goto default;
							default	:
								socket.Shutdown(SocketShutdown.Both);
								socket.Close();
								break;
						}
					}
					catch(Exception e)
					{
						#if DEBUG || SHOW
							errorOutput.WriteLine(derIgel.Utils.Util.ExpandException(e));
						#endif
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
			/// signalled when need to stop
			/// </summary>
			protected ManualResetEvent stopEvent;
			/// <summary>
			/// signalled when need to pause
			/// </summary>
			protected ManualResetEvent pauseEvent;

			public void Pause()
			{
				errorOutput.Flush();
				pauseEvent.Set();
			}

			public void Resume()
			{
				pauseEvent.Reset();
			}

			public void Stop()
			{
				errorOutput.Flush();
				stopEvent.Set();
				while (sessions.Count > 0)
					Thread.Sleep(sessionsCheckInterval);
			}

			protected ArrayList sessions;

			public void SessionDisposedHandler(object obj, EventArgs args)
			{
				sessions.Remove(obj);
			}

			protected const int listenConnections = 100;

			/// <summary>
			/// NNTP server settings
			/// </summary>
			protected NNTPSettings settings;
		}
	}
}