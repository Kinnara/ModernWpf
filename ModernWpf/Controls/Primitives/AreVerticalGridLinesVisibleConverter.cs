using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public class AreVerticalGridLinesVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DataGridGridLinesVisibility)value)
            {
                case DataGridGridLinesVisibility.All:
                    return true;
                case DataGridGridLinesVisibility.Horizontal:
                case DataGridGridLinesVisibility.None:
                    return false;
                case DataGridGridLinesVisibility.Vertical:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
