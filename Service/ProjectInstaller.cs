using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : Installer
	{
		private ServiceInstaller serviceInstaller;
		private ServiceProcessInstaller serviceProcessInstaller;
		/// <summary>
		/// 
		/// Required designer variable.
		/// </summary>
		private Container components;

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
			this.serviceInstaller.ServicesDependedOn = new string[] {
																																"Tcpip"};
			this.serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																																							this.serviceProcessInstaller,
																																							this.serviceInstaller});

		}
		#endregion

		private void serviceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
		{
		
		}
	}
}
