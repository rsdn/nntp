using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Rsdn.Nntp.Editor
{
	/// <summary>
	/// Converter for array of ServerEndPoint class instances (server bindings).
	/// </summary>
	public class BindingsConverter : ArrayConverter
	{
		/// <summary>
		/// Convert object to specific type.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="culture">Culture.</param>
		/// <param name="value">Source object.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				var list = new StringBuilder();
				for (var i = 0; i < ((ServerEndPoint[])value).Length; i++)
				{
					if (i != 0)
						list.Append(", ");
					list.Append(((ServerEndPoint[])value)[i]);
				}
				return list.ToString();
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Check if converter can convert object to destination type.
		/// </summary>
		/// <param name="context">Context.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>True if can convert.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			return base.CanConvertTo(context, destinationType);
		}
	}
}