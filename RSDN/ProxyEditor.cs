using System;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// 
	/// </summary>
	public class ProxyEditor : UITypeEditor
	{
		public ProxyEditor()
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
			
			if (service.ShowDialog(new ProxyEditorForm()) == DialogResult.OK)
			{
				return value;
			}
			else
				return value;
		}
	}
}
