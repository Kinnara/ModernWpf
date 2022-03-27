using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ModernWpf.Media
{
    internal class AcrylicElement
    {
        public static Color GetTintColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(TintColorProperty);
        }

        public static void SetTintColor(DependencyObject obj, Color value)
        {
            obj.SetValue(TintColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for TintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.RegisterAttached("TintColor", typeof(Color), typeof(AcrylicElement), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.Inherits));

        public static double GetTintOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(TintOpacityProperty);
        }

        public static void SetTintOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(TintOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for TintOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TintOpacityProperty =
            DependencyProperty.RegisterAttached("TintOpacity", typeof(double), typeof(AcrylicElement), new FrameworkPropertyMetadata(0.6, FrameworkPropertyMetadataOptions.Inherits));

        public static double GetNoiseOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(NoiseOpacityProperty);
        }

        public static void SetNoiseOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(NoiseOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for NoiseOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoiseOpacityProperty =
            DependencyProperty.RegisterAttached("NoiseOpacity", typeof(double), typeof(AcrylicElement), new FrameworkPropertyMetadata(0.03, FrameworkPropertyMetadataOptions.Inherits));

        public static Color GetFallbackColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(FallbackColorProperty);
        }

        public static void SetFallbackColor(DependencyObject obj, Color value)
        {
            obj.SetValue(FallbackColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for FallbackColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FallbackColorProperty =
            DependencyProperty.RegisterAttached("FallbackColor", typeof(Color), typeof(AcrylicElement), new FrameworkPropertyMetadata(Colors.LightGray, FrameworkPropertyMetadataOptions.Inherits));
    }

    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var col = (Color)value;
                return new SolidColorBrush(col);
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
