using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Threading;
using log4net;

using Rsdn.Nntp;

[assembly: log4net.Config.DOMConfigurator(Watch=true)]

namespace Rsdn.RsdnNntp
{
	public class RsdnNntpServer : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Logger 
		/// </summary>
		private static ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

				nntpManager = new Manager(
					NntpSettings.Deseriazlize(ConfigurationSettings.AppSettings["settings.ConfigFile"]));
				nntpManager.Start();
			}
			catch (Exception e)
			{
				logger.Fatal("RSDN NNTP Server can't start.", e);
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
				if (nntpManager != null)
				{
					nntpManager.Stop();
					nntpManager = null;
				}
			}
			catch (Exception e)
			{
				logger.Fatal("RSDN NNTP Server can't stop.", e);
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
			if (nntpManager != null)
				nntpManager.Pause();
		}

		protected override void OnContinue()
		{
			if (nntpManager != null)
				nntpManager.Resume();
		}

		protected override void OnShutdown()
		{
			OnStop();
		}
	}
}
