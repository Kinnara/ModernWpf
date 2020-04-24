using System;
using System.ComponentModel;
using System.Globalization;

namespace ModernWpf.Controls
{
    public class IconElementConverter : TypeConverter
    {
        public IconElementConverter()
        {
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s && Enum.TryParse(s, true, out Symbol symbol))
            {
                return new SymbolIcon(symbol);
            }

            throw GetConvertFromException(value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            throw GetConvertToException(value, destinationType);
        }
    }
}
