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
	public class DataProviderSettings
	{
		public DataProviderSettings()
		{
			serviceAddress = new Uri(defaultServiceAddress);
			encoding = System.Text.Encoding.UTF8;
		}

		protected WebProxy proxy;

		//[BrowsableAttribute(false)]
		[XmlIgnore]
		[EditorAttribute(typeof(ProxyEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public WebProxy Proxy
		{
			get	{	return proxy;	}
			set { proxy = value; }
		}

		protected ProxySettings proxySettings = new ProxySettings();

		[Category("Connections")]
		[Description("Web Proxy in format http://username:password@host.com:port.\n" +
			 "Username, password, and port may be skipped.")]
		public ProxySettings ProxyServer
		{
			get {return proxySettings;}
			set
			{
				proxySettings = value;
				proxy =
					new WebProxy(proxySettings.ProxyUri, false, null,	proxySettings.Credentials);
			}
		}

		protected const string defaultServiceAddress = "http://rsdn.ru/ws/forum.asmx";

		protected Uri serviceAddress;

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

		protected System.Text.Encoding encoding;
		
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

		protected bool plainText;
		[DefaultValue(false)]
		public bool PlainText
		{
			get { return plainText; }
			set { plainText = value; }
		}
	}
}