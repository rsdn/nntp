using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Configuration;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// Summary description for About.
	/// </summary>
	public class About : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.RichTextBox richTextAbout;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.Label titleLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public About()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			richTextAbout.LoadFile(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("derIgel.RsdnNntp.About.rtf"),
				RichTextBoxStreamType.RichText);

			titleLabel.Text = derIgel.NNTP.Manager.GetProductTitle(Assembly.GetExecutingAssembly());

			Assembly.Load("RsdnNntpServer");
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
				AddAssembly(assembly);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(About));
			this.richTextAbout = new System.Windows.Forms.RichTextBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.treeView = new System.Windows.Forms.TreeView();
			this.titleLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// richTextAbout
			// 
			this.richTextAbout.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.richTextAbout.BackColor = System.Drawing.SystemColors.Control;
			this.richTextAbout.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextAbout.Cursor = System.Windows.Forms.Cursors.Default;
			this.richTextAbout.Location = new System.Drawing.Point(72, 48);
			this.richTextAbout.Name = "richTextAbout";
			this.richTextAbout.ReadOnly = true;
			this.richTextAbout.Size = new System.Drawing.Size(388, 80);
			this.richTextAbout.TabIndex = 0;
			this.richTextAbout.TabStop = false;
			this.richTextAbout.Text = "";
			this.richTextAbout.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextAbout_LinkClicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(8, 32);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 48);
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// treeView
			// 
			this.treeView.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeView.ImageIndex = -1;
			this.treeView.Location = new System.Drawing.Point(8, 136);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = -1;
			this.treeView.Size = new System.Drawing.Size(452, 192);
			this.treeView.TabIndex = 2;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.titleLabel.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.titleLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.titleLabel.Location = new System.Drawing.Point(72, 16);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(129, 14);
			this.titleLabel.TabIndex = 3;
			this.titleLabel.Text = "RSDN NNTP Server ";
			// 
			// About
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(472, 341);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																																	this.titleLabel,
																																	this.treeView,
																																	this.pictureBox1,
																																	this.richTextAbout});
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "About";
			this.Text = "RSDN NNTP Server";
			this.ResumeLayout(false);

		}
		#endregion

		private void richTextAbout_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(e.LinkText);
		}

		protected void AddAssembly(Assembly assembly)
		{
			TreeNode node = treeView.Nodes.Add(assembly.FullName);
			foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
				node.Nodes.Add(referencedAssembly.FullName);
		}
	}
}
