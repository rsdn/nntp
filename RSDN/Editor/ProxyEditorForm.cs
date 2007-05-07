using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;

namespace Rsdn.RsdnNntp.Public.Editor
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
    private IContainer components;
    private System.Windows.Forms.TextBox proxyUrl;
		private System.Windows.Forms.TextBox username;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox password2;

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
				password.Text = password2.Text = ((NetworkCredential)proxy.Credentials).Password;
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
      this.components = new System.ComponentModel.Container();
      this.proxyUrl = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label4 = new System.Windows.Forms.Label();
      this.password2 = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.password = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.username = new System.Windows.Forms.TextBox();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // proxyUrl
      // 
      this.proxyUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.proxyUrl.Location = new System.Drawing.Point(40, 12);
      this.proxyUrl.Name = "proxyUrl";
      this.proxyUrl.Size = new System.Drawing.Size(200, 20);
      this.proxyUrl.TabIndex = 2;
      this.proxyUrl.Validating += new System.ComponentModel.CancelEventHandler(this.proxyUrl_Validating);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label1.Location = new System.Drawing.Point(14, 14);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(20, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Url";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.password2);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.password);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.username);
      this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.groupBox1.Location = new System.Drawing.Point(8, 48);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(240, 112);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Autorization";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label4.Location = new System.Drawing.Point(8, 80);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(90, 13);
      this.label4.TabIndex = 6;
      this.label4.Text = "Confirm password";
      // 
      // password2
      // 
      this.password2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.password2.Location = new System.Drawing.Point(104, 80);
      this.password2.Name = "password2";
      this.password2.PasswordChar = '*';
      this.password2.Size = new System.Drawing.Size(128, 20);
      this.password2.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label3.Location = new System.Drawing.Point(8, 56);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(53, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Password";
      // 
      // password
      // 
      this.password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.password.Location = new System.Drawing.Point(104, 52);
      this.password.Name = "password";
      this.password.PasswordChar = '*';
      this.password.Size = new System.Drawing.Size(128, 20);
      this.password.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.label2.Location = new System.Drawing.Point(8, 28);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(35, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Name";
      // 
      // username
      // 
      this.username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.username.Location = new System.Drawing.Point(104, 24);
      this.username.Name = "username";
      this.username.Size = new System.Drawing.Size(128, 20);
      this.username.TabIndex = 0;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // okButton
      // 
      this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(92, 166);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(173, 166);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 5;
      this.cancelButton.Text = "Cancel";
      // 
      // ProxyEditorForm
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(258, 200);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.proxyUrl);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "ProxyEditorForm";
      this.ShowInTaskbar = false;
      this.Text = "ProxyEditor";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

		}
		#endregion

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

		private void okButton_Click(object sender, System.EventArgs e)
		{
			((NetworkCredential)proxy.Credentials).UserName = username.Text;
			if (password2.Text.Equals(password.Text, StringComparison.Ordinal))
			{
				((NetworkCredential)proxy.Credentials).Password = password.Text;
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				errorProvider.SetError(password2, "Passwords does not match.");
			}
		}
	}
}
