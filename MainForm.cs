using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Net.Sockets;
using System.Reflection;
using System.IO;

namespace Desktop.app
{
	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
    private System.ComponentModel.IContainer components;
    private System.Windows.Forms.Button ApplyBtn;
    private System.Windows.Forms.TabPage setupPage;
    private System.Windows.Forms.TabPage aboutPage;
    private System.Windows.Forms.ContextMenu iconMenu;
    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.MenuItem menuItem2;
    private System.Windows.Forms.MenuItem aboutItem;
    private System.Windows.Forms.MenuItem restoreItem;
    private System.Windows.Forms.MenuItem closeItem;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox webURL;
    private System.Windows.Forms.TextBox portNumber;
    private System.Windows.Forms.CheckBox autoLoad;
    private System.Windows.Forms.CheckBox off;
    private System.Windows.Forms.CheckBox httpFormat;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox user;
    private System.Windows.Forms.TextBox password;
    private System.Windows.Forms.RadioButton authNNTP;
    private System.Windows.Forms.RadioButton authExplicit;

    private NNTPManager socketManager = new NNTPManager();
    private System.Windows.Forms.RichTextBox aboutBox;

    private AboutForm aboutForm = new AboutForm();

		public MainForm()
		{
      aboutForm.Show();

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
      try
      {
        socketManager.StartWork();
      }
      catch(SocketException e)
      {
        // Данный порт уже занят - WSAEADDRINUSE
        if (e.ErrorCode == 10048)
        {
          throw; // Отправляем исключение выше (в Main)
        }
        else
          MessageBox.Show(this, e.Message);
      }
    }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
        if (socketManager != null)
        {
          socketManager.Close();
        }

				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
      this.components = new System.ComponentModel.Container();
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
      this.tabControl = new System.Windows.Forms.TabControl();
      this.setupPage = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.password = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.user = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.authExplicit = new System.Windows.Forms.RadioButton();
      this.authNNTP = new System.Windows.Forms.RadioButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.portNumber = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.webURL = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.httpFormat = new System.Windows.Forms.CheckBox();
      this.off = new System.Windows.Forms.CheckBox();
      this.autoLoad = new System.Windows.Forms.CheckBox();
      this.aboutPage = new System.Windows.Forms.TabPage();
      this.aboutBox = new System.Windows.Forms.RichTextBox();
      this.ApplyBtn = new System.Windows.Forms.Button();
      this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
      this.iconMenu = new System.Windows.Forms.ContextMenu();
      this.aboutItem = new System.Windows.Forms.MenuItem();
      this.menuItem2 = new System.Windows.Forms.MenuItem();
      this.restoreItem = new System.Windows.Forms.MenuItem();
      this.closeItem = new System.Windows.Forms.MenuItem();
      this.tabControl.SuspendLayout();
      this.setupPage.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.aboutPage.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      this.tabControl.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                             this.setupPage,
                                                                             this.aboutPage});
      this.tabControl.HotTrack = true;
      this.tabControl.Location = new System.Drawing.Point(4, 8);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(348, 292);
      this.tabControl.TabIndex = 0;
      // 
      // setupPage
      // 
      this.setupPage.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                            this.groupBox2,
                                                                            this.groupBox1,
                                                                            this.httpFormat,
                                                                            this.off,
                                                                            this.autoLoad});
      this.setupPage.Location = new System.Drawing.Point(4, 22);
      this.setupPage.Name = "setupPage";
      this.setupPage.Size = new System.Drawing.Size(340, 266);
      this.setupPage.TabIndex = 0;
      this.setupPage.Text = "Настройка";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                            this.password,
                                                                            this.label4,
                                                                            this.user,
                                                                            this.label3,
                                                                            this.authExplicit,
                                                                            this.authNNTP});
      this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox2.Location = new System.Drawing.Point(4, 148);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(332, 112);
      this.groupBox2.TabIndex = 4;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Авторизация";
      // 
      // password
      // 
      this.password.AutoSize = false;
      this.password.Enabled = false;
      this.password.Location = new System.Drawing.Point(220, 80);
      this.password.Name = "password";
      this.password.PasswordChar = '*';
      this.password.Size = new System.Drawing.Size(104, 24);
      this.password.TabIndex = 5;
      this.password.Text = "";
      // 
      // label4
      // 
      this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label4.Location = new System.Drawing.Point(220, 60);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(52, 16);
      this.label4.TabIndex = 4;
      this.label4.Text = "Пароль:";
      // 
      // user
      // 
      this.user.AutoSize = false;
      this.user.Enabled = false;
      this.user.Location = new System.Drawing.Point(8, 80);
      this.user.Name = "user";
      this.user.Size = new System.Drawing.Size(208, 24);
      this.user.TabIndex = 3;
      this.user.Text = "";
      // 
      // label3
      // 
      this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label3.Location = new System.Drawing.Point(8, 60);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(112, 16);
      this.label3.TabIndex = 2;
      this.label3.Text = "Имя пользователя:";
      // 
      // authExplicit
      // 
      this.authExplicit.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.authExplicit.Location = new System.Drawing.Point(8, 40);
      this.authExplicit.Name = "authExplicit";
      this.authExplicit.Size = new System.Drawing.Size(256, 16);
      this.authExplicit.TabIndex = 1;
      this.authExplicit.Text = "С использованием следующих реквизитов:";
      // 
      // authNNTP
      // 
      this.authNNTP.Checked = true;
      this.authNNTP.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.authNNTP.Location = new System.Drawing.Point(8, 20);
      this.authNNTP.Name = "authNNTP";
      this.authNNTP.Size = new System.Drawing.Size(148, 16);
      this.authNNTP.TabIndex = 0;
      this.authNNTP.TabStop = true;
      this.authNNTP.Text = "Через клиента NNTP";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                            this.portNumber,
                                                                            this.label2,
                                                                            this.webURL,
                                                                            this.label1});
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(4, 72);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(332, 72);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Настройка прокси";
      // 
      // portNumber
      // 
      this.portNumber.AutoSize = false;
      this.portNumber.Location = new System.Drawing.Point(232, 40);
      this.portNumber.MaxLength = 5;
      this.portNumber.Name = "portNumber";
      this.portNumber.Size = new System.Drawing.Size(92, 24);
      this.portNumber.TabIndex = 3;
      this.portNumber.Text = "";
      // 
      // label2
      // 
      this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2.Location = new System.Drawing.Point(232, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 16);
      this.label2.TabIndex = 2;
      this.label2.Text = "Номер порта:";
      // 
      // webURL
      // 
      this.webURL.AutoSize = false;
      this.webURL.Location = new System.Drawing.Point(8, 40);
      this.webURL.Name = "webURL";
      this.webURL.Size = new System.Drawing.Size(220, 24);
      this.webURL.TabIndex = 1;
      this.webURL.Text = "";
      // 
      // label1
      // 
      this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label1.Location = new System.Drawing.Point(8, 20);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "URL Web-сервиса:";
      // 
      // httpFormat
      // 
      this.httpFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.httpFormat.Location = new System.Drawing.Point(4, 48);
      this.httpFormat.Name = "httpFormat";
      this.httpFormat.Size = new System.Drawing.Size(200, 16);
      this.httpFormat.TabIndex = 2;
      this.httpFormat.Text = "HTTP формат сообщений";
      // 
      // off
      // 
      this.off.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.off.Location = new System.Drawing.Point(4, 28);
      this.off.Name = "off";
      this.off.Size = new System.Drawing.Size(104, 16);
      this.off.TabIndex = 1;
      this.off.Text = "Отключить";
      // 
      // autoLoad
      // 
      this.autoLoad.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autoLoad.Location = new System.Drawing.Point(4, 8);
      this.autoLoad.Name = "autoLoad";
      this.autoLoad.Size = new System.Drawing.Size(204, 16);
      this.autoLoad.TabIndex = 0;
      this.autoLoad.Text = "Загружать вместе с Windows";
      // 
      // aboutPage
      // 
      this.aboutPage.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                            this.aboutBox});
      this.aboutPage.Location = new System.Drawing.Point(4, 22);
      this.aboutPage.Name = "aboutPage";
      this.aboutPage.Size = new System.Drawing.Size(340, 266);
      this.aboutPage.TabIndex = 1;
      this.aboutPage.Text = "О программе";
      // 
      // aboutBox
      // 
      this.aboutBox.Location = new System.Drawing.Point(4, 4);
      this.aboutBox.Name = "aboutBox";
      this.aboutBox.ReadOnly = true;
      this.aboutBox.Size = new System.Drawing.Size(332, 260);
      this.aboutBox.TabIndex = 0;
      this.aboutBox.Text = "";
      // 
      // ApplyBtn
      // 
      this.ApplyBtn.Enabled = false;
      this.ApplyBtn.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.ApplyBtn.Location = new System.Drawing.Point(276, 308);
      this.ApplyBtn.Name = "ApplyBtn";
      this.ApplyBtn.TabIndex = 1;
      this.ApplyBtn.Text = "Применить";
      // 
      // notifyIcon
      // 
      this.notifyIcon.ContextMenu = this.iconMenu;
      this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
      this.notifyIcon.Text = "RSDN Desktop";
      this.notifyIcon.Visible = true;
      this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
      // 
      // iconMenu
      // 
      this.iconMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                             this.aboutItem,
                                                                             this.menuItem2,
                                                                             this.restoreItem,
                                                                             this.closeItem});
      // 
      // aboutItem
      // 
      this.aboutItem.Index = 0;
      this.aboutItem.Text = "О программе...";
      this.aboutItem.Click += new System.EventHandler(this.aboutItem_Click);
      // 
      // menuItem2
      // 
      this.menuItem2.Index = 1;
      this.menuItem2.Text = "-";
      // 
      // restoreItem
      // 
      this.restoreItem.DefaultItem = true;
      this.restoreItem.Index = 2;
      this.restoreItem.Text = "Восстановить";
      this.restoreItem.Click += new System.EventHandler(this.restoreItem_Click);
      // 
      // closeItem
      // 
      this.closeItem.Index = 3;
      this.closeItem.Text = "Закрыть";
      this.closeItem.Click += new System.EventHandler(this.closeItem_Click);
      // 
      // MainForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(356, 339);
      this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                  this.ApplyBtn,
                                                                  this.tabControl});
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.Name = "MainForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "RSDN Desktop";
      this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
      this.Resize += new System.EventHandler(this.MainForm_Resize);
      this.Load += new System.EventHandler(this.MainForm_Load);
      this.tabControl.ResumeLayout(false);
      this.setupPage.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.aboutPage.ResumeLayout(false);
      this.ResumeLayout(false);

    }
		#endregion

    private void closeItem_Click(object sender, System.EventArgs e)
    {
      Close();
    }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
      // Должен работать только один экземпляр
      //
      bool createdNew;
      Mutex mutex = new Mutex(false, "RSDN_Desktop_App", out createdNew);
      if (createdNew)
      {
        try
        {
          Application.Run(new MainForm());
        }
        catch(SocketException ex)
        {
          if (ex.ErrorCode == 10047)
            MessageBox.Show(null, "Данный порт уже используется !!!\nПрограмма будет закрыта", "Ошибка");
          else
            MessageBox.Show(ex.Message, "Ошибка");
        }
        catch(Exception e)
        {
          MessageBox.Show(null, e.Message, "Ошибка");
        }
      }
      else
      {
        MessageBox.Show(null, "RSDN Desktop уже работает !!!", "Внимание");
      }
		}

    private void aboutItem_Click(object sender, System.EventArgs e)
    {
      aboutForm.timer1.Enabled = false;
      aboutForm.Show();
    }

    private void ShowMainForm()
    {
      WindowState = FormWindowState.Normal;
      notifyIcon.Visible = false;
      ShowInTaskbar = true;
      Visible = true;
    }

    private void notifyIcon_DoubleClick(object sender, System.EventArgs e)
    {
      ShowMainForm();
    }

    private void restoreItem_Click(object sender, System.EventArgs e)
    {
      ShowMainForm();
    }

    private void MainForm_Resize(object sender, System.EventArgs e)
    {
      if (WindowState == FormWindowState.Minimized)
      {
        Visible = false;
        notifyIcon.Visible = true;
        ShowInTaskbar = false;
      }
    }

    private void MainForm_Load(object sender, System.EventArgs e)
    {
      using (Stream ios = Assembly.GetExecutingAssembly().GetManifestResourceStream("Desktop.app.about.rtf"))
      {
        try
        {
          aboutBox.LoadFile(ios, RichTextBoxStreamType.RichText);
        }
        catch(Exception ex)
        {
          MessageBox.Show(ex.Message, "Ошибка");
        }
      }
    }
	}
}
