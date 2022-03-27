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
        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(FrameworkElement), typeof(AcrylicPanel), new PropertyMetadata(null));

        public FrameworkElement Source
        {
            get { return (FrameworkElement)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FrameworkElement), typeof(AcrylicPanel), new PropertyMetadata(null));

        public Color TintColor
        {
            get { return (Color)GetValue(TintColorProperty); }
            set { SetValue(TintColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TintColorProperty;
        public static Color GetTintColor(DependencyObject obj)
        {
            return (Color)obj.GetValue(AcrylicElement.TintColorProperty);
        }

        public static void SetTintColor(DependencyObject obj, Color value)
        {
            obj.SetValue(AcrylicElement.TintColorProperty, value);
        }


        public double TintOpacity
        {
            get { return (double)GetValue(TintOpacityProperty); }
            set { SetValue(TintOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TintOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TintOpacityProperty;
        public static double GetTintOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(AcrylicElement.TintOpacityProperty);
        }

        public static void SetTintOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(AcrylicElement.TintOpacityProperty, value);
        }



        public double NoiseOpacity
        {
            get { return (double)GetValue(NoiseOpacityProperty); }
            set { SetValue(NoiseOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoiseOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoiseOpacityProperty;
        public static double GetNoiseOpacity(DependencyObject obj)
        {
            return (double)obj.GetValue(AcrylicElement.NoiseOpacityProperty);
        }

        public static void SetNoiseOpacity(DependencyObject obj, double value)
        {
            obj.SetValue(AcrylicElement.NoiseOpacityProperty, value);
        }


        static AcrylicPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AcrylicPanel), new FrameworkPropertyMetadata(typeof(AcrylicPanel)));

            TintColorProperty = AcrylicElement.TintColorProperty.AddOwner(typeof(AcrylicPanel), new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.Inherits));
            TintOpacityProperty = AcrylicElement.TintOpacityProperty.AddOwner(typeof(AcrylicPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits));
            NoiseOpacityProperty = AcrylicElement.NoiseOpacityProperty.AddOwner(typeof(AcrylicPanel), new FrameworkPropertyMetadata(0.03, FrameworkPropertyMetadataOptions.Inherits));
        }


        public AcrylicPanel()
        {
            this.Source = this;
        }

        bool _isChanged = false;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var rect = this.GetTemplateChild("rect") as Rectangle;
            if (rect != null)
            {
                rect.LayoutUpdated += (_, __) =>
                {
                    if (!this._isChanged)
                    {
                        this._isChanged = true;
                        BindingOperations.GetBindingExpressionBase(rect, Rectangle.RenderTransformProperty)?.UpdateTarget();

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this._isChanged = false;
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
