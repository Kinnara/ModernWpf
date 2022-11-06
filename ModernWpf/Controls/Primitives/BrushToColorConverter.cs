using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public class BrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color)
            {
                return value;
            }

            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }

            return default(Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
