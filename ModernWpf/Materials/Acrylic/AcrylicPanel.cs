using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.Media
{
    public class AcrylicPanel : ContentControl
    {
        private bool _isChanged = false;

        #region Target

        /// <summary>
        /// Identifies the <see cref="Target"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(FrameworkElement),
                typeof(AcrylicPanel),
                null);

        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        #endregion

        #region Source

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(FrameworkElement),
                typeof(AcrylicPanel),
                null);

        public FrameworkElement Source
        {
            get { return (FrameworkElement)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region Amount

        /// <summary>
        /// Identifies the <see cref="Amount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AmountProperty =
            AcrylicElement.AmountProperty.AddOwner(typeof(AcrylicPanel));

        /// <summary>
        /// Gets or sets the amount of gaussian blur to apply to the background.
        /// </summary>
        public double Amount
        {
            get { return (double)GetValue(AmountProperty); }
            set { SetValue(AmountProperty, value); }
        }

        /// <summary>
        /// Gets the amount of gaussian blur to apply to the background.
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <returns>The amount of gaussian blur to apply to the background.</returns>
        public static double GetAmount(DependencyObject obj)
        {
            return (double)obj.GetValue(AcrylicElement.AmountProperty);
        }

        /// <summary>
        /// Sets the amount of gaussian blur to apply to the background.
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetAmount(DependencyObject obj, double value)
        {
            obj.SetValue(AcrylicElement.AmountProperty, value);
        }

        #endregion

        #region TintColor

        /// <summary>
        /// Identifies the <see cref="TintColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintColorProperty =
            AcrylicElement.TintColorProperty.AddOwner(typeof(AcrylicPanel));

        /// <summary>
        /// Gets or sets the tint for the panel
        /// </summary>
        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }

        /// <summary>
        /// Gets the tint for the panel
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <returns>The tint for the panel</returns>
        public static Color GetTintColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(AcrylicElement.TintColorProperty);
        }

        /// <summary>
        /// Sets the tint for the panel
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetTintColor(DependencyObject obj, Color value)
        {
            obj.SetValue(AcrylicElement.TintColorProperty, value);
        }

        #endregion

        #region TintOpacity

        /// <summary>
        /// Identifies the <see cref="TintOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TintOpacityProperty =
            AcrylicElement.TintOpacityProperty.AddOwner(typeof(AcrylicPanel));

        /// <summary>
        /// Gets or sets the tint opacity factor for the effect (default is 0.8, must be in the [0, 1] range)
        /// </summary>
        public double TintOpacity
        {
            get { return (double)GetValue(TintOpacityProperty); }
            set { SetValue(TintOpacityProperty, value); }
        }

        /// <summary>
        /// Gets the tint opacity factor for the effect (default is 0.8, must be in the [0, 1] range)
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <returns>The tint opacity factor for the effect</returns>
        public static double GetTintOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(AcrylicElement.TintOpacityProperty);
        }

        /// <summary>
        /// Sets the tint opacity factor for the effect (default is 0.8, must be in the [0, 1] range)
        /// </summary>
        /// <param name="obj">The element on which to set the attached property.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetTintOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(AcrylicElement.TintOpacityProperty, value);
        }

        #endregion

        #region NoiseOpacity

        /// <summary>
        /// Identifies the <see cref="NoiseOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NoiseOpacityProperty =
            AcrylicElement.NoiseOpacityProperty.AddOwner(typeof(AcrylicPanel));

        public double NoiseOpacity
        {
            get { return (double)GetValue(NoiseOpacityProperty); }
            set { SetValue(NoiseOpacityProperty, value); }
        }

        public static double GetNoiseOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(AcrylicElement.NoiseOpacityProperty);
        }

        public static void SetNoiseOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(AcrylicElement.NoiseOpacityProperty, value);
        }

        #endregion

        static AcrylicPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicPanel), new FrameworkPropertyMetadata(typeof(AcrylicPanel)));
        }

        public AcrylicPanel()
        {
            Target ??= this;
            Source ??= this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("Rect") is Rectangle rect)
            {
                rect.LayoutUpdated += (_, __) =>
                {
                    if (!_isChanged)
                    {
                        _isChanged = true;
                        BindingOperations.GetBindingExpressionBase(rect, RenderTransformProperty)?.UpdateTarget();

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _isChanged = false;
                        }), System.Windows.Threading.DispatcherPriority.DataBind);
                    }
                };
            }
        }
    }

    public class BrushTranslationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(o => o == DependencyProperty.UnsetValue || o == null)) return new TranslateTransform(0, 0);

            var parent = values[0] as UIElement;
            var ctrl = values[1] as UIElement;
            //var pointerPos = (Point)values[2];
            var relativePos = parent.TranslatePoint(new Point(0, 0), ctrl);

            return new TranslateTransform(relativePos.X, relativePos.Y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
