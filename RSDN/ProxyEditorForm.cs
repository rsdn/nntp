// $Id$
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// Summary description for ProxyEditor.
	/// </summary>
	public class ProxyEditorForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.ErrorProvider errorProvider;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox proxyUrl;
		private System.Windows.Forms.Button defaultButton;
		private System.Windows.Forms.TextBox username;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;

		protected WebProxy proxy;
		public WebProxy Proxy
		{
			get { return proxy; }
			set 
			{
				if (value == null)
					proxy = new WebProxy();
				else
					proxy = value;

				proxyUrl.Text = (proxy.Address != null) ? proxy.Address.GetLeftPart(UriPartial.Authority) : "";
				if ((proxy.Credentials as NetworkCredential) == null)
					proxy.Credentials = new NetworkCredential();

				username.Text = ((NetworkCredential)proxy.Credentials).UserName;
				password.Text = ((NetworkCredential)proxy.Credentials).Password;
			}
		}
		public ProxyEditorForm(WebProxy webProxy)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Proxy = webProxy;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.proxyUrl = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.password = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.username = new System.Windows.Forms.TextBox();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this.defaultButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// proxyUrl
			// 
			this.proxyUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.proxyUrl.Location = new System.Drawing.Point(40, 12);
			this.proxyUrl.Name = "proxyUrl";
			this.proxyUrl.Size = new System.Drawing.Size(200, 20);
			this.proxyUrl.TabIndex = 2;
			this.proxyUrl.Text = "";
			this.proxyUrl.Validating += new System.ComponentModel.CancelEventHandler(this.proxyUrl_Validating);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(19, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Url";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																																						this.label3,
																																						this.password,
																																						this.label2,
																																						this.username});
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 48);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(240, 96);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Autorization";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Location = new System.Drawing.Point(8, 56);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(54, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Password";
			// 
			// password
			// 
			this.password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.password.Location = new System.Drawing.Point(80, 52);
			this.password.Name = "password";
			this.password.PasswordChar = '*';
			this.password.Size = new System.Drawing.Size(128, 20);
			this.password.TabIndex = 1;
			this.password.Text = "";
			this.password.Validated += new System.EventHandler(this.password_Validated);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Location = new System.Drawing.Point(8, 28);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "Name";
			// 
			// username
			// 
			this.username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.username.Location = new System.Drawing.Point(80, 24);
			this.username.Name = "username";
			this.username.Size = new System.Drawing.Size(128, 20);
			this.username.TabIndex = 0;
			this.username.Text = "";
			this.username.Validated += new System.EventHandler(this.username_Validated);
			// 
			// defaultButton
			// 
			this.defaultButton.CausesValidation = false;
			this.defaultButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.defaultButton.Location = new System.Drawing.Point(90, 152);
			this.defaultButton.Name = "defaultButton";
			this.defaultButton.TabIndex = 4;
			this.defaultButton.Text = "Get default";
			this.defaultButton.Click += new System.EventHandler(this.defaultButton_Click);
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(8, 152);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 3;
			this.okButton.Text = "OK";
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(172, 152);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			// 
			// ProxyEditorForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(258, 186);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																																	this.cancelButton,
																																	this.okButton,
																																	this.defaultButton,
																																	this.groupBox1,
																																	this.label1,
																																	this.proxyUrl});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ProxyEditorForm";
			this.ShowInTaskbar = false;
			this.Text = "ProxyEditor";
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void defaultButton_Click(object sender, System.EventArgs e)
		{
			Proxy = WebProxy.GetDefaultProxy();
		}

		private void proxyUrl_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				proxy.Address = new UriBuilder((string)proxyUrl.Text).Uri;
				proxyUrl.Text = proxy.Address.GetLeftPart(UriPartial.Authority);
				errorProvider.SetError(proxyUrl, "");
			}
			catch (UriFormatException urlException)
			{
				e.Cancel = true;
				errorProvider.SetError(proxyUrl, urlException.Message);
			}
		}

		private void username_Validated(object sender, System.EventArgs e)
		{
			((NetworkCredential)proxy.Credentials).UserName = username.Text;
		}

		private void password_Validated(object sender, System.EventArgs e)
		{
			((NetworkCredential)proxy.Credentials).Password = username.Text;
		}
	}
}
