using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Win32Util;
using System.ServiceProcess;

namespace RSDN
{
	/// <summary>
	/// Summary description for Notify.
	/// </summary>
	public class Notify : System.Windows.Forms.Form
	{
		protected ControlPanel controlPanel;
		protected Icon startedIcon;
		protected Icon pausedIcon;
		protected Icon stoppedIcon;

		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Timer timer;
		private System.ServiceProcess.ServiceController serviceController;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuStart;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuPause;
		private System.Windows.Forms.MenuItem menuStop;
		private System.Windows.Forms.MenuItem menuExit;
		private System.Windows.Forms.MenuItem menuAbout;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.ComponentModel.IContainer components;

		public Notify()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			controlPanel = new ControlPanel();

			startedIcon	= new System.Drawing.Icon(this.GetType(), "Started.ico");
			pausedIcon	= new System.Drawing.Icon(this.GetType(), "Paused.ico");
			stoppedIcon	= new System.Drawing.Icon(this.GetType(), "Stopped.ico");

			Win32Window win32Window = new Win32Window(this.Handle);
			win32Window.MakeToolWindow();

			RefreshStatus();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				Application.Run(new Notify());
			}
			catch (InvalidOperationException)
			{
				Application.Exit();
			}
		}
		
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuStart = new System.Windows.Forms.MenuItem();
			this.menuPause = new System.Windows.Forms.MenuItem();
			this.menuStop = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.serviceController = new System.ServiceProcess.ServiceController();
			this.menuAbout = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.contextMenu;
			this.notifyIcon.Text = "RSDN NNTP Manager";
			this.notifyIcon.Visible = true;
			this.notifyIcon.DoubleClick += new System.EventHandler(this.Open);
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																																								this.menuOpen,
																																								this.menuItem1,
																																								this.menuStart,
																																								this.menuPause,
																																								this.menuStop,
																																								this.menuItem5,
																																								this.menuAbout,
																																								this.menuItem3,
																																								this.menuExit});
			// 
			// menuOpen
			// 
			this.menuOpen.DefaultItem = true;
			this.menuOpen.Index = 0;
			this.menuOpen.Text = "Open Manager";
			this.menuOpen.Click += new System.EventHandler(this.Open);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 1;
			this.menuItem1.Text = "-";
			// 
			// menuStart
			// 
			this.menuStart.Index = 2;
			this.menuStart.Text = "Start";
			this.menuStart.Click += new System.EventHandler(this.Start);
			// 
			// menuPause
			// 
			this.menuPause.Index = 3;
			this.menuPause.Text = "Pause";
			this.menuPause.Click += new System.EventHandler(this.Pause);
			// 
			// menuStop
			// 
			this.menuStop.Index = 4;
			this.menuStop.Text = "Stop";
			this.menuStop.Click += new System.EventHandler(this.Stop);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 5;
			this.menuItem5.Text = "-";
			// 
			// menuExit
			// 
			this.menuExit.Index = 8;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.Exit);
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Interval = ((int)(configurationAppSettings.GetValue("timer.Interval", typeof(int))));
			this.timer.Tick += new System.EventHandler(this.RefreshStatus);
			// 
			// serviceController
			// 
			this.serviceController.MachineName = ((string)(configurationAppSettings.GetValue("serviceController.MachineName", typeof(string))));
			this.serviceController.ServiceName = ((string)(configurationAppSettings.GetValue("serviceController.ServiceName", typeof(string))));
			// 
			// menuAbout
			// 
			this.menuAbout.Index = 6;
			this.menuAbout.Text = "About";
			this.menuAbout.Click += new System.EventHandler(this.About);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 7;
			this.menuItem3.Text = "-";
			// 
			// Notify
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Name = "Notify";
			this.ShowInTaskbar = false;
			this.Text = "Dummy";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;

		}
		#endregion

		private void RefreshStatus()
		{
				RefreshStatus(this, EventArgs.Empty);
		}

		/// <summary>
		/// About dialog
		/// </summary>
		protected About about;

		private void About(object sender, System.EventArgs e)
		{
			if (about == null)
				about = new About();
			about.ShowDialog();
		}

		private void Exit(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void Open(object sender, System.EventArgs e)
		{
			menuOpen.Enabled = false;
			controlPanel.ShowDialog();
			menuOpen.Enabled = true;		
		}

		private void Start(object sender, System.EventArgs e)
		{
			serviceController.Refresh();
			switch (serviceController.Status)
			{
				case ServiceControllerStatus.Paused	:
				case	ServiceControllerStatus.PausePending	:
					serviceController.Continue();
					break;
				default	:
					serviceController.Start();
					break;
			}
			RefreshStatus();		
		}

		private void Pause(object sender, System.EventArgs e)
		{
			serviceController.Pause();
			RefreshStatus();
		}

		private void Stop(object sender, System.EventArgs e)
		{
			serviceController.Stop();
			RefreshStatus();
		}

		private void RefreshStatus(object sender, System.EventArgs e)
		{
			try
			{
				serviceController.Refresh();
				switch (serviceController.Status)
				{
					case	ServiceControllerStatus.ContinuePending	:
					case	ServiceControllerStatus.Running	:
					case	ServiceControllerStatus.StartPending	:
						notifyIcon.Icon = startedIcon;
						menuStart.Enabled = false;
						menuPause.Enabled = true;
						menuStop.Enabled = true;
						break;
					case	ServiceControllerStatus.Paused	:
					case	ServiceControllerStatus.PausePending	:
						notifyIcon.Icon = pausedIcon;
						menuStart.Enabled = true;
						menuStart.Text = "Continue";
						menuPause.Enabled = false;
						menuStop.Enabled = true;
						break;
					default:
						notifyIcon.Icon = stoppedIcon;
						menuStart.Enabled = true;
						menuStart.Text = "Start";
						menuPause.Enabled = false;
						menuStop.Enabled = false;
						break;
				}	
			}
			catch (InvalidOperationException io)
			{
				timer.Enabled = false;
				// no such service
				MessageBox.Show(io.Message, "RSDN NNTP Manager",
					MessageBoxButtons.OK,	MessageBoxIcon.Exclamation);
				throw;
			}		
		}
	}
}
