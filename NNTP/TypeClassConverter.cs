using System;
using System.ComponentModel;

namespace derIgel.NNTP
{
	/// <summary>
	/// Summary description for TypeConverter.
	/// </summary>
	public class TypeClassConverter : TypeConverter
	{
		public TypeClassConverter()
		{
		}

		public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return ((Type)value).AssemblyQualifiedName;
			}
			else
				return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
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
			if (value is string)
				return Type.GetType((string)value, true);

			return base.ConvertFrom(context, culture, value);
		}
	}
}