using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using System.Net;

using Rsdn.Nntp;
using Rsdn.Nntp.Cache;

namespace Rsdn.RsdnNntp
{
	/// <summary>
	/// MIME message formatting style
	/// </summary>
	public enum FormattingStyle
	{
		/// <summary>
		/// Get preffered style from user settings at RSDN.RU
		/// </summary>
		UserSettings,
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
	/// Settings for RSDN Data Provider
	/// </summary>
	[Serializable]
	public class DataProviderSettings : CacheDataProviderSettings
	{
		/// <summary>
		/// Initialize settings.
		/// </summary>
		public DataProviderSettings()
		{
			encoding = System.Text.Encoding.UTF8;
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

		protected FormattingStyle formatting = FormattingStyle.UserSettings;

		[Category("Others")]
		[DefaultValue(FormattingStyle.UserSettings)]
		[Description("Format of messages. UserSettings is taken from user settings at RSDN.RU")]
		public FormattingStyle Formatting
		{
			get { return formatting; }
			set { formatting = value; }
		}
	}
}
