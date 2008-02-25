using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Rsdn.RsdnNntp.Public.Editor
{
	/// <summary>
	/// 
	/// </summary>
	public class ProxyEditor : UITypeEditor
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
			
			var proxyEditor = new ProxyEditorForm(value as WebProxy);
			if (service.ShowDialog(proxyEditor) == DialogResult.OK)
			{
				return proxyEditor.Proxy;
			}
			return value;
		}
	}
}
