using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using derIgel.NNTP;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;
using System.IO;

namespace derIgel
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
				RsdnNntpSettings serverSettings = (RsdnNntpSettings)
					RsdnNntpSettings.Deseriazlize(
						Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
							"config.xml"),
						typeof(RsdnNntpSettings));

				nntpManager = new Manager(typeof(RsdnDataProvider),	serverSettings);
				nntpManager.Start();
			}
			catch (Exception e)
			{
				string message = string.Empty;
				Exception e1 = e;
				do 
				{
					message += e1.Message + "\n";
					e1 = e1.InnerException;
				}
				while (e1 != null);
				EventLog.WriteEntry(message, EventLogEntryType.Error);
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
	}
}
