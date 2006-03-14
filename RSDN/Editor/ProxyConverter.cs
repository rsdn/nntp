using System;
using System.Net;
using System.ComponentModel;
using System.Text;

namespace Rsdn.RsdnNntp.Public.Editor
{
	/// <summary>
	/// Summary description for ProxyConverter.
	/// </summary>
	public class ProxyConverter : TypeConverter
	{
		public ProxyConverter()
		{
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
		{
			if ((value is WebProxy) && (destinationType == typeof(string)))
			{
				return Convert.ToString(((WebProxy)value).Address);
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
				if ((string)value != "")
				{
					UriBuilder uriBuilder = new UriBuilder((string)value);
					NetworkCredential credential = new NetworkCredential();
					credential.UserName = uriBuilder.UserName;
					credential.Password = uriBuilder.Password;
					// don't show password
					uriBuilder.Password = "";
					return new WebProxy(uriBuilder.Uri.GetLeftPart(UriPartial.Authority), false, null, credential);
				}
				else
					return null;
	
			return base.ConvertFrom(context, culture, value);
		}
	}
}
