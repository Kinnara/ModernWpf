using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.MahApps.Controls
{
    [Obsolete]
    public class HamburgerMenuTitleBarPaddingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 &&
                values[0] is bool extendViewIntoTitleBar && extendViewIntoTitleBar &&
                values[1] is double height)
            {
                return new Thickness(0, height, 0, 0);
            }

            return new Thickness();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
