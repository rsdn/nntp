// $Id$
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
	public enum FormattingStyle { PlainText, Html, HtmlInlineImages }

	/// <summary>
	/// Settings for RSDN Data Provider
	/// </summary>
	[Serializable]
	public class DataProviderSettings : MarshalByRefObject
	{

		public DataProviderSettings()
		{
			serviceAddress = new Uri(defaultServiceAddress);
			encoding = System.Text.Encoding.UTF8;
		}

		protected WebProxy proxy = new WebProxy();

		[Category("Connections")]
		[Description("Web Proxy in format http://username:password@host.com:port\n" +
			 "Username, password, and port may be skipped.")]
		[EditorAttribute(typeof(ProxyEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter(typeof(ProxyConverter))]
		[XmlIgnore]
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
