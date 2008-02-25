using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using Rsdn.WMI.ROOT.CIMV2;

namespace Rsdn.Nntp.Editor
{
  public class IPAddressConverter : TypeConverter
  {
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
      return true;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      var ipAddresses = new StringCollection
				{ IPAddress.Any.ToString(), IPAddress.Loopback.ToString() };
    	foreach (NetworkAdapterConfiguration netConfig in NetworkAdapterConfiguration.GetInstances("IPEnabled=1"))
        ipAddresses.AddRange(netConfig.IPAddress);

      return new StandardValuesCollection(ipAddresses);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
    	if (sourceType == typeof(string))
        return true;
    	return base.CanConvertFrom(context, sourceType);
    }

  	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      return (value is string) ?
				IPAddress.Parse((string)value).ToString() :
				base.ConvertFrom(context, culture, value);
    }
  }
}
