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
	public class DataProviderSettings : CacheDataProviderSettings, ICustomTypeDescriptor
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
		[Description("Type of the proxy to use for connections.\n" + 
			"None - don't use ptoxy, Default - use proxy's settings from IE,\n" +
			"Explicit - use explicit provided settings.")]
		[DefaultValue(ProxyType.Default)]
		public ProxyType ProxyType
		{
			get { return proxyType; }
			set
			{
				proxyType = value;
				// refresh type descriptors of this object
				TypeDescriptor.Refresh(this);
			}
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
		#region ICustomTypeDescriptor Members

		public TypeConverter GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		public EventDescriptorCollection GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		public string GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		public AttributeCollection GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		/// <summary>
		/// Get object's properties.
		/// If ProxyType is not explicit - don't show Proxy property.
		/// </summary>
		/// <param name="attributes">Filter attributes.</param>
		/// <returns>Property descriptor collection.</returns>
		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection properties =
				new PropertyDescriptorCollection(null);

			foreach (PropertyDescriptor desc in TypeDescriptor.GetProperties(this, attributes, true))
				if ((desc.Name != "Proxy") || (proxyType == ProxyType.Explicit))
					properties.Add(desc);

			return properties;
		}

		public PropertyDescriptorCollection GetProperties()
		{
			return TypeDescriptor.GetProperties(this, true);
		}

		public object GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(this, true);
		}

		public EventDescriptor GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		public string GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		#endregion
	}
}
