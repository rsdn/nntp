using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Desktop.app
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class AboutForm : System.Windows.Forms.Form
	{
    private System.Windows.Forms.PictureBox pictureBox1;
    internal System.Timers.Timer timer1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AboutForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutForm));
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.timer1 = new System.Timers.Timer();
      ((System.ComponentModel.ISupportInitialize)(this.timer1)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = ((System.Drawing.Bitmap)(resources.GetObject("pictureBox1.Image")));
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(400, 300);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
      // 
      // timer1
      // 
      this.timer1.AutoReset = false;
      this.timer1.Enabled = true;
      this.timer1.Interval = 3000;
      this.timer1.SynchronizingObject = this;
      this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
      // 
      // AboutForm
      // 
      this.AutoScale = false;
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(400, 300);
      this.ControlBox = false;
      this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                  this.pictureBox1});
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "AboutForm";
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "AboutForm";
      this.TopMost = true;
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
      ((System.ComponentModel.ISupportInitialize)(this.timer1)).EndInit();
      this.ResumeLayout(false);

    }
		#endregion

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
        Visible = false;
    }

    private void AboutForm_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      Visible = false;
    }

    private void pictureBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      Visible = false;
    }

    private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      Visible = false;
    }
	}
}
