using System;
using System.Globalization;
using System.Windows.Data;

namespace ModernWpf.SampleApp.Common
{
    public sealed class BooleanToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? parameter : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
