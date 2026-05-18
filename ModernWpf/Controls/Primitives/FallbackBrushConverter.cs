using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public class FallbackBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush;
            }

            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
