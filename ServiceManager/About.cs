using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

namespace RSDN
{
	/// <summary>
	/// Summary description for About.
	/// </summary>
	public class About : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.RichTextBox richTextAbout;
		private System.Windows.Forms.TreeView treeView;
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
				Assembly.GetExecutingAssembly().GetManifestResourceStream("RSDN.About.rtf"),
				RichTextBoxStreamType.RichText);

			AddAssembly(Assembly.GetExecutingAssembly().GetName(), treeView.Nodes, true);
			AddAssembly(Assembly.Load("RsdnNntpServer").GetName(), treeView.Nodes, true);
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
			this.SuspendLayout();
			// 
			// richTextAbout
			// 
			this.richTextAbout.BackColor = System.Drawing.SystemColors.Control;
			this.richTextAbout.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextAbout.Cursor = System.Windows.Forms.Cursors.Default;
			this.richTextAbout.Location = new System.Drawing.Point(72, 8);
			this.richTextAbout.Name = "richTextAbout";
			this.richTextAbout.ReadOnly = true;
			this.richTextAbout.Size = new System.Drawing.Size(208, 80);
			this.richTextAbout.TabIndex = 0;
			this.richTextAbout.TabStop = false;
			this.richTextAbout.Text = "";
			this.richTextAbout.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextAbout_LinkClicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(8, 24);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(48, 48);
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// treeView
			// 
			this.treeView.ImageIndex = -1;
			this.treeView.Location = new System.Drawing.Point(8, 104);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = -1;
			this.treeView.Size = new System.Drawing.Size(272, 112);
			this.treeView.TabIndex = 2;
			// 
			// About
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 223);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																																	this.treeView,
																																	this.pictureBox1,
																																	this.richTextAbout});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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

		protected void AddAssembly(AssemblyName assembly, TreeNodeCollection tree, bool inner)
		{
			TreeNode node = tree.Add(assembly.Name + ", version " + assembly.Version.ToString());
			if (inner)
			{
				AssemblyName[] referencedAssemblies = Assembly.Load(assembly).GetReferencedAssemblies();
				foreach (AssemblyName referencedAssembly in referencedAssemblies)
					AddAssembly(referencedAssembly, node.Nodes, false);
			}
		}
	}
}
