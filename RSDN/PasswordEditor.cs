using System;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// 
	/// </summary>
	public class PasswordEditor : UITypeEditor
	{
		public PasswordEditor()
		{
			// 
			// TODO: Add constructor logic here
			//
		}

		public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
		{
			return true;
		}

		public override void PaintValue(System.Drawing.Design.PaintValueEventArgs e)
		{

		}
	}
}
