using System;

namespace RSDN
{
	/// <summary>
	/// Summary description for UriConverter.
	/// </summary>
	public class UriConverter : System.ComponentModel.TypeConverter
	{
		public UriConverter()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
		{
			if (sourceType == typeof(string)) 
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string) 
			{
				return new Uri((string)value);
			}
			else
				return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
		{
			if (destinationType == typeof(string)) 
			{
				return ((Uri)value).AbsoluteUri;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
