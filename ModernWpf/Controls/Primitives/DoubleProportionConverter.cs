using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public class DoubleProportionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double result = (double)value;
                if (parameter != null)
                {
                    var zoom = System.Convert.ToDouble(parameter);
                    result = (double)value * zoom;
                }
                if (targetType == typeof(string))
                {
                    return string.Format("{0:0.##}", result);
                }
                return result;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                if (parameter != null)
                {
                    var zoom = System.Convert.ToDouble(parameter);
                    return (double)value / zoom;
                }
            }
            return value;
        }
    }
}
