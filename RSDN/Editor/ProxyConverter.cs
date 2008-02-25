using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Rsdn.RsdnNntp.Public.Editor
{
	/// <summary>
	/// Summary description for ProxyConverter.
	/// </summary>
	public class ProxyConverter : TypeConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if ((value is WebProxy) && (destinationType == typeof(string)))
			{
				return Convert.ToString(((WebProxy)value).Address);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;
			else
				return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
				if ((string)value != "")
				{
					var uriBuilder = new UriBuilder((string)value);
					var credential = new NetworkCredential();
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
