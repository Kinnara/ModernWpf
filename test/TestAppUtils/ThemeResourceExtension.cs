using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace System.Windows
{
    [TypeConverter(typeof(ThemeResouceExtensionConverter))]
    public class ThemeResourceExtension : ModernWpf.Markup.ThemeResourceExtension
    {
        public ThemeResourceExtension()
        {
        }

        public ThemeResourceExtension(object resourceKey) : base(resourceKey)
        {
        }
    }

    public class ThemeResouceExtensionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                ThemeResourceExtension dynamicResource = value as ThemeResourceExtension;

                if (dynamicResource == null)

                    throw new ArgumentException($"{value} must be of type {nameof(ThemeResourceExtension)}", nameof(value));

                return new InstanceDescriptor(typeof(ThemeResourceExtension).GetConstructor(new Type[] { typeof(object) }),
                    new object[] { dynamicResource.ResourceKey });
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
