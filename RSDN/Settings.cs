using System;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using System.Net;

using Rsdn.Nntp;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// MIME message formatting style
	/// </summary>
	public enum FormattingStyle
	{
		/// <summary>
		/// Only plain text.
		/// </summary>
		PlainText,
		/// <summary>
		/// HTML and plain text.
		/// </summary>
		Html,
		/// <summary>
		/// HTML with inline images and plain text.
		/// </summary>
		HtmlInlineImages
	}

	/// <summary>
	/// Type of proxy to use.
	/// </summary>
	public enum ProxyType
	{
		/// <summary>
		/// Don't use proxy.
		/// </summary>
		None,
		/// <summary>
		/// Use defaut proxy (from IE settings).
		/// </summary>
		Default,
		/// <summary>
		/// Use explicit specified proxy.
		/// </summary>
		Explicit
	}

	/// <summary>
	/// Settings for RSDN Data Provider
	/// </summary>
	[Serializable]
	public class DataProviderSettings : MarshalByRefObject
	{
		/// <summary>
		/// Initialize settings.
		/// </summary>
		public DataProviderSettings()
		{
			serviceAddress = new Uri(defaultServiceAddress);
			encoding = System.Text.Encoding.UTF8;
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

		/// <summary>
		/// Proxy's type.
		/// By default is none.
		/// </summary>
		protected ProxyType proxyType = ProxyType.Default;

		/// <summary>
		/// Proxy's type.
		/// </summary>
		[Category("Connections")]
		[Description("Type of the proxy to use for connections.")]
		[DefaultValue(ProxyType.Default)]
		public ProxyType ProxyType
		{
			get { return proxyType; }
			set { proxyType = value; }
		}

		/// <summary>
		/// Web proxy.
		/// </summary>
		protected WebProxy proxy = new WebProxy();

		/// <summary>
		/// Web proxy.
		/// </summary>
		[Category("Connections")]
		[Description("Web Proxy in format http://username:password@host.com:port\n" +
			 "Username, password, and port may be skipped.")]
		[XmlIgnore]
		[TypeConverter(typeof(ProxyConverter))]
		[EditorAttribute(typeof(ProxyEditor), typeof(System.Drawing.Design.UITypeEditor))]
		public WebProxy Proxy
		{
			get	{	return proxy;	}
			set { proxy = value; }
		}

		[BrowsableAttribute(false)]
		public ProxySettings proxySettings
		{
			get { return new ProxySettings(proxy); }
			set	{ proxy = value.Proxy; }
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

		protected FormattingStyle formatting = FormattingStyle.Html;

		[Category("Others")]
		[DefaultValue(FormattingStyle.Html)]
		public FormattingStyle Formatting
		{
			get { return formatting; }
			set { formatting = value; }
		}
	}
}
