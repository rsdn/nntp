using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace Rsdn.Nntp.Editor
{
	/// <summary>
	/// Summary description for TypeConverter.
	/// </summary>
	public class CertificateConverter : TypeConverter
	{
    public CertificateConverter()
		{
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
		{
			if (destinationType == typeof(string))
			{
        return (value == null) ? null : ((X509Certificate2)value).Subject;
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

    public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;
      else
        return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      if (value is string)
      {
        if ((string)value == "")
          return null;
        else
          throw new ArgumentException("Wrong certificate!", "value");
      }
      else
        return base.ConvertFrom(context, culture, value);
    }

	}
}