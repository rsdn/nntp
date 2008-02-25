using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Rsdn.Nntp.Editor
{
	/// <summary>
	/// 
	/// </summary>
	public class TypeEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			var service =
				(IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			
			if (service == null)
				return null;
			
			var editorForm = new TypeEditorForm(value as Type, typeof(IDataProvider));
			if (service.ShowDialog(editorForm) == DialogResult.OK)
			{
				return editorForm.SelectedType;
			}
			return value;
		}
	}
}
