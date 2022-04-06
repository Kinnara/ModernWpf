using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    [ValueConversion(typeof(double), typeof(double[]))]
    public class RoundMathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                throw new NotImplementedException();
            }
            double.TryParse(values[0].ToString(), out double d1);
            double.TryParse(values[1].ToString(), out double d2);
            if (d1 * d2 == 0)
            {
                return double.NaN;
            }
            return Math.Min(d1, d2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(System.Windows.CornerRadius), typeof(double[]))]
    public class RoundRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
            {
                throw new NotImplementedException();
            }
            double.TryParse(values[0].ToString(), out double d1);
            double.TryParse(values[1].ToString(), out double d2);
            double ratio = 1;
            if (values.Length >= 3)
            {
                double.TryParse(values[2].ToString(), out ratio);
            }
            return new System.Windows.CornerRadius(Math.Min(d1, d2) * ratio / 2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
