using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using derIgel.NNTP;
using derIgel.RsdnNntp;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using System.Configuration;
using derIgel.MIME;

namespace derIgel.RsdnNntp
{
	public class RsdnNntpServer : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RsdnNntpServer()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			this.EventLog.Source = "RSDN NNTP Server";
		}

		// The main entry point for the process
		static void Main()
		{
			System.ServiceProcess.ServiceBase[] ServicesToRun;
				
			// More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = New System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
			//
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new RsdnNntpServer() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// RsdnNntpServer
			// 
			this.AutoLog = false;
			this.CanPauseAndContinue = true;
			this.CanShutdown = true;
			this.ServiceName = "rsdnnntp";

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			try
			{
				Directory.SetCurrentDirectory(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
				DataProviderSettings serverSettings = (DataProviderSettings)
					DataProviderSettings.Deseriazlize(
						ConfigurationSettings.AppSettings["service.Config"],
						typeof(DataProviderSettings));

				nntpManager = new Manager(typeof(RsdnDataProvider),	serverSettings);
				nntpManager.Start();
			}
			catch (Exception e)
			{
				#if DEBUG || SHOW
					System.Console.Error.WriteLine(Util.ExpandException(e));
				#endif
				EventLog.WriteEntry(Util.ExpandException(e), EventLogEntryType.Error);
				nntpManager = null;
				// start timer, which will stop service in 1 sec
				Timer timer = new Timer(new TimerCallback(Stop), null, 1000, Timeout.Infinite);
			}
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			try
			{
				nntpManager.Stop();
				nntpManager = null;
			}
			catch (Exception e)
			{
				#if DEBUG || SHOW
					System.Console.Error.WriteLine(Util.ExpandException(e));
				#endif
				EventLog.WriteEntry(e.Message, EventLogEntryType.Error);
			}
		}

		/// <summary>
		/// NNTP Manager
		/// </summary>
		protected Manager nntpManager = null;

		protected void Stop(Object state)
		{
			ServiceController service = new ServiceController(ServiceName);
			service.Stop();
		}

		protected override void OnPause()
		{
			nntpManager.Pause();
		}

		protected override void OnContinue()
		{
			nntpManager.Resume();
		}

		protected override void OnShutdown()
		{
			OnStop();
		}
	}
}
