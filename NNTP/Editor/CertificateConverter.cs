using System;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace Rsdn.Nntp.Editor
{
	/// <summary>
	/// Summary description for TypeConverter.
	/// </summary>
	public class CertificateConverter : TypeConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
        return (value == null) ? null : ((X509Certificate2)value).Subject;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			return base.CanConvertTo(context, destinationType);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
        return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
      {
      	if (string.IsNullOrEmpty((string)value))
          return null;
      	throw new ArgumentException("Wrong certificate!", "value");
      }
			return base.ConvertFrom(context, culture, value);
		}
	}
}