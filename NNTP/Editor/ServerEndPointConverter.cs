using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace Rsdn.Nntp.Editor
{
  public class ServerEndPointConverter : ExpandableObjectConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context,
      Type destinationType)
    {
    	if (destinationType == typeof(ServerEndPoint))
        return true;
    	return base.CanConvertTo(context, destinationType);
    }

  	public override object ConvertTo(ITypeDescriptorContext context,
      CultureInfo culture, object value, Type destinationType)
  	{
  		if (destinationType == typeof(string))
      {
        return (value).ToString();
      }
  		return base.ConvertTo(context, culture, value, destinationType);
  	}

  	public override bool CanConvertFrom(ITypeDescriptorContext context,
      Type sourceType)
  	{
  		if (sourceType == typeof(string))
        return true;
  		return base.CanConvertFrom(context, sourceType);
  	}

  	public override object ConvertFrom(ITypeDescriptorContext context,
      CultureInfo culture, object value)
  	{
  		if (value is string)
      {
        var parts = ((string)value).Split(new[] { ':' }, 2);
        return new ServerEndPoint(IPAddress.Parse(parts[0]), (parts.Length > 1) ? int.Parse(parts[1]) : 0);
      }
  		return base.ConvertFrom(context, culture, value);
  	}
  }
}
