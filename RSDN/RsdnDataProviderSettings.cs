using System;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using System.Net;
using derIgel.NNTP;

namespace derIgel.RsdnNntp
{
	/// <summary>
	/// settings for application
	/// </summary>
	[XmlRoot("Settings")]
	public class RsdnDataProviderSettings : NNTPSettings
	{
		public RsdnDataProviderSettings()
		{
			serviceAddress = new Uri(defaultServiceAddress);
			encoding = System.Text.Encoding.UTF8;
		}

		protected Uri serviceAddress;
		protected WebProxy proxyAddress;
		protected System.Text.Encoding encoding;
		
		[Category("Connections")]
		[DefaultValue("")]
		[Description("Web Proxy for connection")]
		public string Proxy
		{
			get
			{
				return (proxyAddress != null) ? proxyAddress.Address.ToString() : string.Empty;
			}
			set
			{
				if (value == string.Empty)
					proxyAddress = null;
				else
					proxyAddress = new WebProxy(value);
			}
		}

		protected NetworkCredential credential = new NetworkCredential();

		[Category("Connections")]
		[DefaultValue("")]
		[Description("Username for web proxy authorization")]
		public string proxyUsername
		{
			get
			{
				return credential.UserName;
			}
			set
			{
				credential.UserName = value;
			}
		}

		[Category("Connections")]
		[DefaultValue("")]
		[Description("Password for web proxy authorization")]
		public string proxyPassword
		{
			get
			{
				return credential.Password;
			}
			set
			{
				credential.Password = value;
			}
		}

		[Category("Connections")]
		[DefaultValue(defaultServiceAddress)]
		[Description("URL of RSDN Forum's Web Service")]
		public string Service
		{
			get
			{
				return serviceAddress.ToString();
			}
			set
			{
				serviceAddress = new Uri(value);
			}
		}

		protected int cacheSize;
		[Category("Others")]
		[DefaultValue(0)]
		[Description("Cache size (0 - disabled)")]
		public int CacheSize
		{
			get {return cacheSize;}
			set {cacheSize = value;}
		}

		[Category("Others")]
		[DefaultValue("utf-8")]
		[Description("Output encoding,for example, utf-8 or windows-1251")]
		public string Encoding
		{
			get
			{
				return encoding.HeaderName;
			}
			set
			{
				System.Text.Encoding enc = System.Text.Encoding.GetEncoding(value);
				if (!enc.IsMailNewsDisplay)
					throw new NotSupportedException(string.Format(
						"{0} encoding is not suitable for news client.", enc.HeaderName));
				encoding = enc;
			}
		}

		[BrowsableAttribute(false)]
		[XmlIgnore]
		public System.Text.Encoding GetEncoding
		{
			get
			{
				return encoding;
			}
		}
			
		[BrowsableAttribute(false)]
		[XmlIgnore]
		public WebProxy GetProxy
		{
			get
			{
				if (proxyAddress != null)
					proxyAddress.Credentials = credential;
				return proxyAddress;
			}
		}

		protected const string defaultServiceAddress = "http://rsdn.ru/ws/forum.asmx";
	}
}