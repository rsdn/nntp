using System;
using System.ComponentModel;
using System.Globalization;

namespace Rsdn.Nntp.Editor
{
	/// <summary>
	/// Summary description for TypeConverter.
	/// </summary>
	public class TypeClassConverter : TypeConverter
	{
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
        return (value == null) ? null : ((Type)value).AssemblyQualifiedName;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			return base.CanConvertTo(context, destinationType);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
				return Type.GetType((string)value, true);

			return base.ConvertFrom(context, culture, value);
		}
	}
}