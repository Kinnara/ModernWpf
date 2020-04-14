using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DragablzSample
{
    public class InvertThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness t)
            {
                return new Thickness(-t.Left, -t.Top, -t.Right, -t.Bottom);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
