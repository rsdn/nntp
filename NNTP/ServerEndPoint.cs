using System;
using System.Collections;
using System.Net;
using System.ComponentModel;
using System.Globalization;
using derIgel.ROOT.CIMV2;
using System.Xml.Serialization;

namespace derIgel.NNTP
{
	#region IPAddressConverter
	public class IPAddressConverter : TypeConverter
	{
		public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context)
		{
			return true;
		}

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
		{
			ArrayList ipAddresses = new ArrayList();
			ipAddresses.Add(IPAddress.Any);
			ipAddresses.Add(IPAddress.Loopback);
			foreach (NetworkAdapterConfiguration netConfig in NetworkAdapterConfiguration.GetInstances("IPEnabled=1"))
				ipAddresses.AddRange(netConfig.IPAddress);
				
			return new TypeConverter.StandardValuesCollection(ipAddresses);
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
			return (value is string) ? IPAddress.Parse((string)value).ToString() : base.ConvertFrom(context, culture, value);
		}
	}
	#endregion

	public class ServerEndPointConverter : ExpandableObjectConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context,
			System.Type destinationType) 
		{
			if (destinationType == typeof(ServerEndPoint))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context,
			CultureInfo culture, object value, System.Type destinationType) 
		{
			if (destinationType == typeof(string) )
			{
				return ((ServerEndPoint)value).ToString();
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
				string[] parts = ((string)value).Split(new char[]{':'}, 2);
        return new ServerEndPoint(IPAddress.Parse(parts[0]), (parts.Length > 1) ? int.Parse(parts[1]) : 0);
			}
			else
				return base.ConvertFrom(context, culture, value);
		}
	}

	/// <summary>
	/// Summary description for ServerEndPoint.
	/// </summary>
	[TypeConverterAttribute(typeof(ServerEndPointConverter))]
	public class ServerEndPoint
	{
		public ServerEndPoint() : this(IPAddress.Any, 0) {	}

		public ServerEndPoint(IPAddress address) : this(address, 0) {	}

		public ServerEndPoint(int port) : this(IPAddress.Any, port) {	}

		public ServerEndPoint(IPAddress address, int port)
		{
			endPoint = new IPEndPoint(address, port);
		}

		protected IPEndPoint endPoint;

		[XmlIgnore]
		[Browsable(false)]
		public IPEndPoint EndPoint
		{
			get { return endPoint; }
		}

		[TypeConverterAttribute(typeof(IPAddressConverter))]
		public string Address
		{
			get { return endPoint.Address.ToString(); }
			set { endPoint.Address = IPAddress.Parse(value); }
		}

		public int Port
		{
			get { return endPoint.Port; }
			set { endPoint.Port = value; }
		}

		public override string ToString()
		{
			return endPoint.ToString();
		}
	}
}
