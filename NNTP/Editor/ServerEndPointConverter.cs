using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Rsdn.Nntp.Editor
{
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
      if (destinationType == typeof(string))
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
        string[] parts = ((string)value).Split(new char[] { ':' }, 2);
        return new ServerEndPoint(IPAddress.Parse(parts[0]), (parts.Length > 1) ? int.Parse(parts[1]) : 0);
      }
      else
        return base.ConvertFrom(context, culture, value);
    }
  }
}
