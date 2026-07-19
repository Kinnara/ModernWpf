using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Controls
{
    public sealed class AppBarIconOrContentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] != null && values[0] != DependencyProperty.UnsetValue)
            {
                return values[0];
            }

            if (values.Length > 1 && values[1] != DependencyProperty.UnsetValue)
            {
                return values[1];
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
