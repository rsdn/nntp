using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using System.Net;

using Rsdn.Nntp;
using Rsdn.Nntp.Cache;
using Rsdn.RsdnNntp.Public;
using Rsdn.RsdnNntp.Public.Editor;

namespace Rsdn.RsdnNntp.Public
{
	/// <summary>
	/// Settings for RSDN Data Provider
	/// </summary>
	[Serializable]
	[XmlType("RsdnDataPublicProviderSettings")]
	public class DataProviderSettings : Rsdn.RsdnNntp.DataProviderSettings, ICustomTypeDescriptor
	{
		/// <summary>
		/// Initialize settings.
		/// </summary>
		public DataProviderSettings() : base()
		{
			serviceAddress = new Uri(defaultServiceAddress);
		}

		protected const string defaultServiceAddress = "http://rsdn.ru/ws/service2.asmx";

		protected Uri serviceAddress;

		[BrowsableAttribute(false)]
		[XmlIgnore]
		public Uri ServiceUri
		{
			get { return serviceAddress; }
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
}