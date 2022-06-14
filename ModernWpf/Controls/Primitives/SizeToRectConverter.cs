using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    internal class SizeToRectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double a = values[0] is double ? (double)values[0] : 0;
            double b = values[1] is double ? (double)values[1] : 0;
            double c = values[2] is double ? (double)values[2] : 0;
            double d = values[3] is double ? (double)values[3] : 0;
            return new Rect(a, b, c, d);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Rect rect = value is Rect ? (Rect)value : new Rect(0, 0, 0, 0);
            return new object[] { rect.TopLeft.X, rect.TopLeft.Y, rect.BottomRight.X, rect.BottomRight.Y };
        }
    }
}
