using System;
using System.ComponentModel;
using System.Text;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Summary description for BindingsConverter.
	/// </summary>
	public class BindingsConverter : ArrayConverter
	{
		public BindingsConverter()
		{
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				StringBuilder list = new StringBuilder();
				for (int i = 0; i < ((ServerEndPoint[])value).Length; i++)
				{
					if (i != 0)
						list.Append(", ");
					list.Append(((ServerEndPoint[])value)[i]);
				}
				return list.ToString();
			}
			else
				return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}
	}
}