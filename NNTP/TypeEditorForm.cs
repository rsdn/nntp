using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Summary description for TypeEditorForm.
	/// </summary>
	public class TypeEditorForm : System.Windows.Forms.Form
	{
		//
		protected Hashtable types = new Hashtable();
		private System.Windows.Forms.OpenFileDialog openAssemblyDialog;
		private System.Windows.Forms.Button selectFile;
		private System.Windows.Forms.TextBox assemblyPath;
		private System.Windows.Forms.ComboBox namespacesCombo;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ErrorProvider errorProvider;
		private System.Windows.Forms.ComboBox typesCombo;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TypeEditorForm() : this(null, null) {	}

		public TypeEditorForm(Type initialType) : this(initialType, null) {	}

		/// <summary>
		/// filter type.
		/// Null if none
		/// </summary>
		protected Type filter;

		public TypeEditorForm(Type initialType, Type filterType)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			typesCombo.DisplayMember = "Name";

			filter = filterType;
			if (initialType != null)
			{
				assemblyPath.Text = initialType.Assembly.FullName;
				InitCombos(initialType.Assembly);
				namespacesCombo.SelectedItem = initialType.Namespace;
				typesCombo.SelectedItem = initialType;
			}
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
			this.typesCombo = new System.Windows.Forms.ComboBox();
			this.openAssemblyDialog = new System.Windows.Forms.OpenFileDialog();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this.SuspendLayout();
			// 
			// assemblyPath
			// 
			this.assemblyPath.Location = new System.Drawing.Point(16, 16);
			this.assemblyPath.Name = "assemblyPath";
			this.assemblyPath.Size = new System.Drawing.Size(248, 20);
			this.assemblyPath.TabIndex = 0;
			this.assemblyPath.Text = "";
			this.assemblyPath.Validating += new System.ComponentModel.CancelEventHandler(this.assemblyPath_Validating);
			this.assemblyPath.Validated += new System.EventHandler(this.assemblyPath_Validated);
			// 
			// selectFile
			// 
			this.selectFile.CausesValidation = false;
			this.selectFile.Location = new System.Drawing.Point(288, 16);
			this.selectFile.Name = "selectFile";
			this.selectFile.Size = new System.Drawing.Size(32, 23);
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
			this.namespacesCombo.Size = new System.Drawing.Size(296, 21);
			this.namespacesCombo.Sorted = true;
			this.namespacesCombo.TabIndex = 2;
			this.namespacesCombo.SelectedIndexChanged += new System.EventHandler(this.namespacesCombo_SelectedIndexChanged);
			// 
			// typesCombo
			// 
			this.typesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typesCombo.Location = new System.Drawing.Point(16, 126);
			this.typesCombo.Name = "typesCombo";
			this.typesCombo.Size = new System.Drawing.Size(296, 21);
			this.typesCombo.Sorted = true;
			this.typesCombo.TabIndex = 3;
			this.typesCombo.SelectedIndexChanged += new System.EventHandler(this.typesCombo_SelectedIndexChanged);
			// 
			// openAssemblyDialog
			// 
			this.openAssemblyDialog.DefaultExt = "dll";
			this.openAssemblyDialog.Filter = "Assemblies |*.dll";
			this.openAssemblyDialog.RestoreDirectory = true;
			this.openAssemblyDialog.Title = "Select assembly file";
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(88, 168);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(192, 168);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			// 
			// TypeEditorForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(330, 218);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																																	this.cancelButton,
																																	this.okButton,
																																	this.typesCombo,
																																	this.namespacesCombo,
																																	this.selectFile,
																																	this.assemblyPath});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "TypeEditorForm";
			this.ShowInTaskbar = false;
			this.Text = "Select type";
			this.ResumeLayout(false);

		}
		#endregion

		private void selectFile_Click(object sender, System.EventArgs e)
		{
			if (openAssemblyDialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					assemblyPath.Text = Assembly.LoadFrom(openAssemblyDialog.FileName).FullName;
					assemblyPath_Validated(this, EventArgs.Empty);
				}
				catch (Exception exception)
				{
					assemblyPath.Text = openAssemblyDialog.FileName;
					errorProvider.SetError(assemblyPath, exception.Message);
				}
			}
		}

		private void assemblyPath_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				Assembly.Load(assemblyPath.Text);
			}
			catch (Exception exception)
			{
				e.Cancel = true;
				errorProvider.SetError(assemblyPath, exception.Message);
			}
		}

		private void assemblyPath_Validated(object sender, System.EventArgs e)
		{
			errorProvider.SetError(assemblyPath, "");
			Assembly selectedAssembly = Assembly.Load(assemblyPath.Text);
			if ((selectedType == null) || (selectedType.Assembly != selectedAssembly))
			{
				selectedType = null;
				InitCombos(selectedAssembly);
			}
		}

		protected void InitCombos(Assembly assembly)
		{
			okButton.Enabled = false;
			namespacesCombo.BeginUpdate();
			typesCombo.BeginUpdate();
			namespacesCombo.Items.Clear();
			typesCombo.Items.Clear();

			types.Clear();
			foreach (Type type in assembly.GetTypes())
				if (type.IsPublic && ((filter == null) || (filter.IsAssignableFrom(type))))
				{
					if (types[type.Namespace] == null)
					{
						namespacesCombo.Items.Add(type.Namespace);
						types[type.Namespace] = new ArrayList();
					}
					((ArrayList)types[type.Namespace]).Add(type);
				}
			if (namespacesCombo.Items.Count > 0)
				namespacesCombo.SelectedIndex = 0;

			typesCombo.EndUpdate();
			namespacesCombo.EndUpdate();
		}

		protected Type selectedType;

		private void namespacesCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			typesCombo.BeginUpdate();
			typesCombo.Items.Clear();
			foreach (Type type in (IEnumerable)types[namespacesCombo.Text])
			{
				typesCombo.Items.Add(type);
			}

			typesCombo.SelectedIndex = 0;

			typesCombo.EndUpdate();
		}

		private void typesCombo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			selectedType = (Type)typesCombo.SelectedItem;
			okButton.Enabled = true;
		}
	
		public Type SelectedType
		{
			get { return selectedType; }
			set
			{
				selectedType = value;
				assemblyPath.Text = value.Assembly.ToString();
				InitCombos(value.Assembly);
			}
		}
	}
}