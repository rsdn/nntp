using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Serialization;
using System.Text;

namespace derIgel.RsdnNntp
{
	public class ProxySettingsConverter : ExpandableObjectConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context,
			System.Type destinationType) 
		{
			if (destinationType == typeof(ProxySettings))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context,
			CultureInfo culture, object value, System.Type destinationType) 
		{
			if (destinationType == typeof(string) )
			{
				return ((ProxySettings)value).ToString();
			}
			else
				return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context,
			System.Type sourceType) 
		{
			if (sourceType == typeof(string))
				return true;
			else
				return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context,
			CultureInfo culture, object value) 
		{
			if (value is string) 
			{
				return new ProxySettings((string)value);
			}
			else
				return base.ConvertFrom(context, culture, value);
		}
	}

	/// <summary>
	/// Summary description for ProxySettings.
	/// </summary>
	[TypeConverterAttribute(typeof(ProxySettingsConverter))]
	public class ProxySettings
	{
		public static Uri CreateUri(string scheme, string host, int port, string username)
		{
			StringBuilder uriString = new StringBuilder();
			uriString.Append(scheme).Append(Uri.SchemeDelimiter);
			if (username != null)
				uriString.Append(username).Append('@');
			uriString.Append(host).Append(':').Append(port);
			return new Uri(uriString.ToString());
		}

		public ProxySettings()
		{
		}

		protected static readonly Regex userInfo =
			new Regex(@"(?n)(?<username>[^:]+)(:(?<password>.*))?", RegexOptions.Compiled);

		public ProxySettings(Uri proxyUri)
		{
			if (proxyUri != null)
			{
					protocol = proxyUri.Scheme;
				host = proxyUri.Host;
				port = proxyUri.Port;
				Match userInfoMatch = userInfo.Match(proxyUri.UserInfo);
				if (userInfoMatch.Success)
				{
					username = userInfoMatch.Groups["username"].Value;
					password = userInfoMatch.Groups["password"].Value;
				}
			}
		}

		public ProxySettings(string proxyAddress) : this(proxyAddress != "" ? new Uri(proxyAddress) : null) {	}

		[XmlIgnore]
		[Browsable(false)]
		public Uri ProxyUri
		{
			get {return (host != null) ? CreateUri(protocol, host, port, username) : null;}
		}

		protected string protocol = Uri.UriSchemeHttp;
		[Description("Protocol")]
		[DefaultValue("http")]
		public string Protocol
		{
			get { return protocol; }
			set
			{
				if (!Uri.CheckSchemeName(value))
					throw new ArgumentException("Wrong scheme name");
				protocol = value;
			}
		}

		protected string host;
		[Description("Proxy address")]
		public string Host
		{
			get	{	return host;	}
			set
			{
				if (value != "")
				{
					if (Uri.CheckHostName(value) == UriHostNameType.Unknown)
						throw new ArgumentException("Unknown host name");
				}
				host = (value == "") ? null : value;
			}
		}

		protected int port = 80;
		[DefaultValue(80)]
		[Description("Port")]
		public int Port
		{
			get	{	return port;	}
			set	{	port =  value; }
		}

		protected string username;
		[Description("username")]
		public string Username
		{
			get	{	return username;}
			set	{	username = value; }
		}

		protected string password;
		[Description("Password")]
		public string Password
		{
			get	{	return password; }
			set	{password = value; }
		}

		public override string ToString()
		{
			return (ProxyUri != null) ? ProxyUri.GetLeftPart(UriPartial.Authority) : null;
		}

		[XmlIgnore]
		[Browsable(false)]
		public ICredentials Credentials
		{
			get { return new NetworkCredential(username, password); }
		}
	}
}
