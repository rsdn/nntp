// $Id$
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Diagnostics;

using Rsdn.Nntp;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceInstaller serviceInstaller;
		private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
		/// <summary>
		/// 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller
			// 
			this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.serviceProcessInstaller.Password = null;
			this.serviceProcessInstaller.Username = null;
			this.serviceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller_AfterInstall);
			// 
			// serviceInstaller
			// 
			this.serviceInstaller.DisplayName = "RSDN NNTP Server";
			this.serviceInstaller.ServiceName = "rsdnnntp";
			this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																																							this.serviceProcessInstaller,
																																							this.serviceInstaller});

		}
		#endregion

		private void serviceProcessInstaller_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e)
		{
		
		}

		private void customInstaller_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e)
		{
		
		}

		public override void Uninstall(System.Collections.IDictionary savedState)
		{
			base.Uninstall(savedState);

			string installdir = Path.GetDirectoryName(Context.Parameters["assemblypath"]);
			// remove messages cache file
			File.Delete(Path.Combine(installdir, "RsdnDataProvider.cache"));
			// remove references cache file
			File.Delete(Path.Combine(installdir, "RsdnDataProvider.references.cache"));
			// remove log file
			File.Delete(Path.Combine(installdir, "rsdnnntp.log"));
			// remove performance counters
			if (PerformanceCounterCategory.Exists(Manager.ServerCategoryName))
				PerformanceCounterCategory.Delete(Manager.ServerCategoryName);
		}
	}
}
