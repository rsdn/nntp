using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Win32Util;
using System.Threading;
using System.Configuration;
using derIgel.ROOT.CIMV2;
using System.Management;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Xml.Serialization;
using derIgel.NNTP;

namespace derIgel.RsdnNntp
{
	public enum StartupType {Auto, Manual, Disabled}

	/// <summary>
	/// Summary description for Notify.
	/// </summary>
	public class Notify : System.Windows.Forms.Form
	{
		protected ControlPanel controlPanel;
		protected Icon startedIcon;
		protected Icon pausedIcon;
		protected Icon stoppedIcon;

		internal protected Service service = new Service();
		protected ManagementPath serviceManagementPath;

		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.Timer timer;
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

		public Type serviceSettingsType; 

		static Notify()
		{
			Type settingsType = Assembly.LoadFrom(ConfigurationSettings.AppSettings["settings.Assembly"])
				.GetType(ConfigurationSettings.AppSettings["settings.Type"], true);

			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = "DynamicAssembly";

			// Create descendant class from defined class
			AssemblyBuilder dynamicAssembly = System.Threading.Thread.GetDomain().
				DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
					
			TypeBuilder typeBuilder =	dynamicAssembly.DefineDynamicModule("DynamicModule", "DynamicAssembly.dll").
				DefineType("ServiceDataProviderType", TypeAttributes.Class | TypeAttributes.Public,
				settingsType);

			// StartupMode
			FieldBuilder startupField = typeBuilder.DefineField("startupMode", typeof(StartupType),
				FieldAttributes.Private);

			MethodBuilder getStartup = typeBuilder.DefineMethod("get_StartupMode", MethodAttributes.Public,
				typeof(StartupType), null);
			ILGenerator methodIL = getStartup.GetILGenerator();
			methodIL.Emit(OpCodes.Ldarg_0);
			methodIL.Emit(OpCodes.Ldfld, startupField);
			methodIL.Emit(OpCodes.Ret);

			MethodBuilder setStartup = typeBuilder.DefineMethod("set_StartupMode", MethodAttributes.Public,
				null, new Type[]{typeof(StartupType)});
			methodIL = setStartup.GetILGenerator();
			methodIL.Emit(OpCodes.Ldarg_0);
			methodIL.Emit(OpCodes.Ldarg_1);
			methodIL.Emit(OpCodes.Stfld, startupField);
			methodIL.Emit(OpCodes.Ret);

			PropertyBuilder startupModeProperty = typeBuilder.DefineProperty("StartupMode",
				PropertyAttributes.HasDefault, typeof(StartupType), null);
			startupModeProperty.SetConstant(StartupType.Auto);
			startupModeProperty.SetGetMethod(getStartup);
			startupModeProperty.SetSetMethod(setStartup);
			
			// Attributes for StartupMode
			startupModeProperty.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(XmlIgnoreAttribute).GetConstructor(Type.EmptyTypes), new object[0]));
			startupModeProperty.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(CategoryAttribute).GetConstructor(new Type[]{typeof(string)}),
					new object[]{"Service settings"}));
			startupModeProperty.SetCustomAttribute(new CustomAttributeBuilder(
				typeof(DescriptionAttribute).GetConstructor(new Type[]{typeof(string)}),
				new object[]{"How server starts"}));

			// Machine name field
			FieldBuilder machine = typeBuilder.DefineField("Machine", typeof(System.String),
				FieldAttributes.Public | FieldAttributes.HasDefault);
			machine.SetConstant(".");

			// Service Name field
			FieldBuilder serviceName = typeBuilder.DefineField("ServiceName", typeof(System.String),
				FieldAttributes.Public | FieldAttributes.HasDefault);
			serviceName.SetConstant("rsdnnntp");
				
			typeBuilder.CreateType();
				
			dynamicAssembly.Save("DynamicAssembly.dll");
		}

		public Notify()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			try
			{
				// get dynamic created type
				serviceSettingsType = Activator.CreateInstanceFrom("DynamicAssembly.dll", "ServiceDataProviderType").
					Unwrap().GetType();

				serverSettings = NNTPSettings.Deseriazlize(
					ConfigurationSettings.AppSettings["settings.ConfigFile"], serviceSettingsType);
			}
			catch (Exception)
			{
				serverSettings = Activator.CreateInstance(serviceSettingsType);
			}

			serviceManagementPath = new ManagementPath(string.Format(@"\\{0}\root\CIMV2:Win32_Service.Name=""{1}""",
				serverSettings.GetType().GetField("Machine").GetValue(serverSettings),
				serverSettings.GetType().GetField("ServiceName").GetValue(serverSettings)));
			
			controlPanel = new ControlPanel();
			controlPanel.propertyGrid.SelectedObject = serverSettings;

			startedIcon	= new System.Drawing.Icon(this.GetType(), "Started.ico");
			pausedIcon	= new System.Drawing.Icon(this.GetType(), "Paused.ico");
			stoppedIcon	= new System.Drawing.Icon(this.GetType(), "Stopped.ico");

			Win32Window win32Window = new Win32Window(this.Handle);
			win32Window.MakeToolWindow();

			RefreshStatus();
			timer.Enabled = true;
		}

		/// <summary>
		/// Server setting
		/// </summary>
		protected object serverSettings;

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
			Notify mainForm = null;
			try
			{
				mainForm = new Notify();
				Application.Run(mainForm);
			}
			catch (ManagementException exception)
			{
				if (mainForm != null)
					mainForm.timer.Enabled = false;
				MessageBox.Show(exception.Message, "RSDN NNTP Manager",
					MessageBoxButtons.OK,	MessageBoxIcon.Error);
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
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuStart = new System.Windows.Forms.MenuItem();
			this.menuPause = new System.Windows.Forms.MenuItem();
			this.menuStop = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuAbout = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuExit = new System.Windows.Forms.MenuItem();
			this.timer = new System.Windows.Forms.Timer(this.components);
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
			// menuExit
			// 
			this.menuExit.Index = 8;
			this.menuExit.Text = "Exit";
			this.menuExit.Click += new System.EventHandler(this.Exit);
			// 
			// timer
			// 
			this.timer.Interval = 500;
			this.timer.Tick += new System.EventHandler(this.RefreshStatus);
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

		internal protected void RefreshStatus()
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
			if (!controlPanel.Visible)
			{
				menuOpen.Enabled = false;
				RefreshService();
				// Set StartupMode property in settins according current state of service
				serviceSettingsType.GetProperty("StartupMode").SetValue(controlPanel.propertyGrid.SelectedObject,
					Enum.Parse(typeof(StartupType), service.StartMode), null);
				controlPanel.ShowDialog(this);
				menuOpen.Enabled = true;
				RefreshStatus();
			}
		}

		private void Start(object sender, System.EventArgs e)
		{
			RefreshService();
			switch (service.State)
			{
				case "Pause Pending" :
				case "Paused" :
					service.ResumeService();
					break;
				default	:
					service.StartService();
					break;
			}
			RefreshStatus();
			controlPanel.ShowAlert(false);
		}

		private void Pause(object sender, System.EventArgs e)
		{
			service.PauseService();
			RefreshStatus();
		}

		private void Stop(object sender, System.EventArgs e)
		{
			service.StopService();
			RefreshStatus();
		}

		internal protected void RefreshService()
		{
			service.Path = serviceManagementPath;
		}

		private void RefreshStatus(object sender, System.EventArgs e)
		{
			RefreshService();	
			switch (service.State)
			{
				case "Running" :
				case "Continue Pending" :
				case "Start Pending" :
					notifyIcon.Icon = startedIcon;
					menuStart.Enabled = false;
					menuPause.Enabled = true;
					menuStop.Enabled = true;
					break;
				case "Pause Pending" :
				case "Paused" :
					notifyIcon.Icon = pausedIcon;
					menuStart.Enabled = true;
					menuStart.Text = "Continue";
					menuPause.Enabled = false;
					menuStop.Enabled = true;
					break;
				default:
				switch (service.StartMode)
				{
					case "Disabled" : 
						notifyIcon.Icon = stoppedIcon;
						menuStart.Text = "Start";
						menuStart.Enabled = false;
						menuPause.Enabled = false;
						menuStop.Enabled = false;
						break;
					default:
						notifyIcon.Icon = stoppedIcon;
						menuStart.Enabled = true;
						menuStart.Text = "Start";
						menuPause.Enabled = false;
						menuStop.Enabled = false;
						break;
				}
					break;
			}	
		}
	}
}
