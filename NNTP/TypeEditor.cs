// $Id$
using System;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Rsdn.Nntp
{
	/// <summary>
	/// 
	/// </summary>
	public class TypeEditor : UITypeEditor
	{
		public TypeEditor()
		{
			// 
			// TODO: Add constructor logic here
			//
		}

		public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService service =
				(IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			
			if (service == null)
				return null;
			
			TypeEditorForm editorForm = new TypeEditorForm(value as Type, typeof(IDataProvider));
			if (service.ShowDialog(editorForm) == DialogResult.OK)
			{
				return editorForm.SelectedType;
			}
			else
				return value;
		}
	}
}
