using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Net;
using Rsdn.WMI.ROOT.CIMV2;

namespace Rsdn.Nntp.Editor
{
  public class IPAddressConverter : TypeConverter
  {
    public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context)
    {
      return true;
    }

    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
    {
      StringCollection ipAddresses = new StringCollection();
      ipAddresses.Add(IPAddress.Any.ToString());
      ipAddresses.Add(IPAddress.Loopback.ToString());
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
}
