using System;
using System.ComponentModel;
using System.Text;

namespace Rsdn.Nntp
{
	/// <summary>
	/// Converter for array of ServerEndPoint class instances (server bindings).
	/// </summary>
	public class BindingsConverter : ArrayConverter
	{
		/// <summary>
		/// Create converter.
		/// </summary>
		public BindingsConverter()
		{
		}

		/// <summary>
		/// Convert object to specific type.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="culture">Culture.</param>
		/// <param name="value">Source object.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>Converted object.</returns>
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

		/// <summary>
		/// Check if converter can convert object to destination type.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>True if can convert.</returns>
		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}
	}
}