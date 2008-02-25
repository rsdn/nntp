using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using log4net;
using log4net.Config;
using Rsdn.Nntp;

[assembly: XmlConfigurator(Watch=true)]

namespace Rsdn.RsdnNntp
{
	public class RsdnNntpServer : ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private Container components;

		/// <summary>
		/// Logger 
		/// </summary>
		private static readonly ILog logger = LogManager.GetLogger("RSDN NNTP Server");

		public RsdnNntpServer()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			EventLog.Source = "RSDN NNTP Server";
		}

		// The main entry point for the process
		static void Main()
		{
			// started as service
			if (Console.In == StreamReader.Null)
			{
				Run(new RsdnNntpServer());
			}
			else
			{
				var server = new RsdnNntpServer();
				server.OnStart(null);
				Console.ReadLine();
				server.OnStop();
			}
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
			AutoLog = false;
			CanPauseAndContinue = true;
			CanShutdown = true;
			ServiceName = "rsdnnntp";

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
			ThreadPool.QueueUserWorkItem(StartServer);
		}
 
		protected void StartServer(object obj)
		{
			try
			{
				Directory.SetCurrentDirectory(
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

				nntpManager = new Manager(
					NntpSettings.Deseriazlize(ConfigurationManager.AppSettings["settings.ConfigFile"]));
				nntpManager.Start();
			}
			catch (Exception e)
			{
				logger.Fatal("RSDN NNTP Server can't start.", e);
				nntpManager = null;
				// start timer, which will stop service in 1 sec
				new Timer(Stop, null, 1000, Timeout.Infinite);
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
		protected Manager nntpManager;

		protected void Stop(Object state)
		{
			var service = new ServiceController(ServiceName);
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
