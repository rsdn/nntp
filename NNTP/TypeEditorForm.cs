using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;

namespace derIgel.NNTP
{
	/// <summary>
	/// Summary description for TypeEditorForm.
	/// </summary>
	public class TypeEditorForm : System.Windows.Forms.Form
	{
		//
		protected Hashtable types = new Hashtable();

		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.OpenFileDialog openAssemblyDialog;
		private System.Windows.Forms.Button selectFile;
		private System.Windows.Forms.TextBox assemblyPath;
		private System.Windows.Forms.ComboBox namespacesCombo;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TypeEditorForm() : this(null) {	}

		public TypeEditorForm(string initialValue)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
			this.assemblyPath = new System.Windows.Forms.TextBox();
			this.selectFile = new System.Windows.Forms.Button();
			this.namespacesCombo = new System.Windows.Forms.ComboBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.openAssemblyDialog = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// assemblyPath
			// 
			this.assemblyPath.Location = new System.Drawing.Point(16, 16);
			this.assemblyPath.Name = "assemblyPath";
			this.assemblyPath.Size = new System.Drawing.Size(232, 20);
			this.assemblyPath.TabIndex = 0;
			this.assemblyPath.Text = "";
			this.assemblyPath.Validating += new System.ComponentModel.CancelEventHandler(this.assemblyPath_Validating);
			this.assemblyPath.TextChanged += new System.EventHandler(this.assemblyPath_Validated);
			// 
			// selectFile
			// 
			this.selectFile.CausesValidation = false;
			this.selectFile.Location = new System.Drawing.Point(264, 16);
			this.selectFile.Name = "selectFile";
			this.selectFile.Size = new System.Drawing.Size(48, 23);
			this.selectFile.TabIndex = 1;
			this.selectFile.TabStop = false;
			this.selectFile.Text = "...";
			this.selectFile.Click += new System.EventHandler(this.selectFile_Click);
			// 
			// namespacesCombo
			// 
			this.namespacesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.namespacesCombo.Location = new System.Drawing.Point(16, 72);
			this.namespacesCombo.Name = "namespacesCombo";
			this.namespacesCombo.Size = new System.Drawing.Size(232, 21);
			this.namespacesCombo.TabIndex = 2;
			// 
			// comboBox2
			// 
			this.comboBox2.Location = new System.Drawing.Point(16, 126);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(232, 21);
			this.comboBox2.TabIndex = 3;
			this.comboBox2.Text = "comboBox2";
			// 
			// openAssemblyDialog
			// 
			this.openAssemblyDialog.DefaultExt = "dll";
			this.openAssemblyDialog.Filter = "Assemblies |*.dll";
			this.openAssemblyDialog.RestoreDirectory = true;
			this.openAssemblyDialog.Title = "Select assembly file";
			// 
			// TypeEditorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(328, 273);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																																	this.comboBox2,
																																	this.namespacesCombo,
																																	this.selectFile,
																																	this.assemblyPath});
			this.Name = "TypeEditorForm";
			this.Text = "Select type";
			this.ResumeLayout(false);

		}
		#endregion

		private void selectFile_Click(object sender, System.EventArgs e)
		{
			if (openAssemblyDialog.ShowDialog(this) == DialogResult.OK)
			{
				assemblyPath.Text = Assembly.LoadFrom(openAssemblyDialog.FileName).FullName;
				Validate();
			}
		}

		private void assemblyPath_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				Assembly.Load(assemblyPath.Text);
			}
			catch (Exception)
			{
				e.Cancel = true;
			}
		}

		private void assemblyPath_Validated(object sender, System.EventArgs e)
		{
			namespacesCombo.BeginUpdate();
			types.Clear();
			namespacesCombo.Items.Clear();
			foreach (Type type in Assembly.Load(assemblyPath.Text).GetTypes())
			{
				types[type.Namespace] = type;
				namespacesCombo.Items.Add(type.Namespace);
			}
			namespacesCombo.EndUpdate();
		}
	}
}
