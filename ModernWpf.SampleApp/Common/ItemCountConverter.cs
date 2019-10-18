using System;
using System.Globalization;
using System.Windows.Data;

namespace ModernWpf.SampleApp.Common
{
    public class ItemCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                if (count > 1)
                {
                    return $"({count} items)";
                }
                else if (count == 1)
                {
                    return $"({count} item)";
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
