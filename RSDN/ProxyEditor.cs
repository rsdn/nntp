using System;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Net;

namespace Rsdn.RsdnNntp.Public
{
	/// <summary>
	/// 
	/// </summary>
	public class ProxyEditor : UITypeEditor
	{
		public ProxyEditor()
		{
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
			
			ProxyEditorForm proxyEditor = new ProxyEditorForm(value as WebProxy);
			if (service.ShowDialog(proxyEditor) == DialogResult.OK)
			{
				return proxyEditor.Proxy;
			}
			else
				return value;
		}
	}
}
