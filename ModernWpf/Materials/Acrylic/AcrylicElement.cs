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
        #region Amount

        /// <summary>
        /// Gets the amount of gaussian blur to apply to the background.
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <returns>The amount of gaussian blur to apply to the background.</returns>
        public static double GetAmount(DependencyObject obj)
        {
            return (double)obj.GetValue(AmountProperty);
        }

        /// <summary>
        /// Sets the amount of gaussian blur to apply to the background.
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetAmount(DependencyObject obj, Color value)
        {
            obj.SetValue(AmountProperty, value);
        }

        /// <summary>
        /// Identifies the Amount dependency property.
        /// </summary>
        public static readonly DependencyProperty AmountProperty =
            DependencyProperty.RegisterAttached(
                "Amount",
                typeof(double),
                typeof(AcrylicElement),
                new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        #region TintColor

        /// <summary>
        /// Gets the tint for the panel
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <returns>The tint for the panel</returns>
        public static Color GetTintColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(TintColorProperty);
        }

        /// <summary>
        /// Sets the tint for the panel
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetTintColor(DependencyObject obj, Color value)
        {
            obj.SetValue(TintColorProperty, value);
        }

        /// <summary>
        /// Identifies the TintColor dependency property.
        /// </summary>
        public static readonly DependencyProperty TintColorProperty =
            DependencyProperty.RegisterAttached(
                "TintColor",
                typeof(Color),
                typeof(AcrylicElement),
                new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        #region TintOpacity

        /// <summary>
        /// Gets the tint opacity factor for the effect (default is 0.8, must be in the [0, 1] range)
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <returns>The tint opacity factor for the effect</returns>
        public static double GetTintOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(TintOpacityProperty);
        }

        /// <summary>
        /// Sets the tint opacity factor for the effect (default is 0.8, must be in the [0, 1] range)
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetTintOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(TintOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the TintOpacity dependency property.
        /// </summary>
        public static readonly DependencyProperty TintOpacityProperty =
            DependencyProperty.RegisterAttached(
                "TintOpacity",
                typeof(double),
                typeof(AcrylicElement),
                new FrameworkPropertyMetadata(0.8, FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        #region NoiseOpacity

        public static double GetNoiseOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(NoiseOpacityProperty);
        }

        public static void SetNoiseOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(NoiseOpacityProperty, value);
        }

        /// <summary>
        /// Identifies the NoiseOpacity dependency property.
        /// </summary>
        public static readonly DependencyProperty NoiseOpacityProperty =
            DependencyProperty.RegisterAttached(
                "NoiseOpacity",
                typeof(double),
                typeof(AcrylicElement),
                new FrameworkPropertyMetadata(0.03, FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        #region FallbackColor

        public static Color GetFallbackColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(FallbackColorProperty);
        }

        public static void SetFallbackColor(DependencyObject obj, Color value)
        {
            obj.SetValue(FallbackColorProperty, value);
        }

        /// <summary>
        /// Identifies the FallbackColor dependency property.
        /// </summary>
        public static readonly DependencyProperty FallbackColorProperty =
            DependencyProperty.RegisterAttached(
                "FallbackColor",
                typeof(Color),
                typeof(AcrylicElement),
                new FrameworkPropertyMetadata(Colors.LightGray, FrameworkPropertyMetadataOptions.Inherits));

        #endregion
    }

    public class ColorToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.Transparent;
        }
    }
}
