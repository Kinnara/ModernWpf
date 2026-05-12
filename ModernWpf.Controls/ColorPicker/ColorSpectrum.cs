using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls;

namespace ModernWpf.Controls.Primitives
{
    public class ColorSpectrum : Control
    {
        private const string SpectrumRectangleName = "SpectrumRectangle";
        private const string SpectrumOverlayRectangleName = "SpectrumOverlayRectangle";
        private const string SpectrumEllipseName = "SpectrumEllipse";
        private const string SpectrumOverlayEllipseName = "SpectrumOverlayEllipse";
        private const string SelectionEllipsePanelName = "SelectionEllipsePanel";
        private const string SelectionEllipseName = "SelectionEllipse";

        static ColorSpectrum()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSpectrum), new FrameworkPropertyMetadata(typeof(ColorSpectrum)));
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(
                nameof(Color),
                typeof(Color),
                typeof(ColorSpectrum),
                new PropertyMetadata(Colors.White, OnColorPropertyChanged));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty HsvColorProperty =
            DependencyProperty.Register(
                nameof(HsvColor),
                typeof(Vector4),
                typeof(ColorSpectrum),
                new PropertyMetadata(new Vector4(0, 0, 1, 1), OnHsvColorPropertyChanged));

        public Vector4 HsvColor
        {
            get => (Vector4)GetValue(HsvColorProperty);
            set => SetValue(HsvColorProperty, value);
        }

        public static readonly DependencyProperty MinHueProperty =
            DependencyProperty.Register(
                nameof(MinHue),
                typeof(int),
                typeof(ColorSpectrum),
                new PropertyMetadata(0, OnHueRangePropertyChanged));

        public int MinHue
        {
            get => (int)GetValue(MinHueProperty);
            set => SetValue(MinHueProperty, value);
        }

        public static readonly DependencyProperty MaxHueProperty =
            DependencyProperty.Register(
                nameof(MaxHue),
                typeof(int),
                typeof(ColorSpectrum),
                new PropertyMetadata(359, OnHueRangePropertyChanged));

        public int MaxHue
        {
            get => (int)GetValue(MaxHueProperty);
            set => SetValue(MaxHueProperty, value);
        }

        public static readonly DependencyProperty MinSaturationProperty =
            DependencyProperty.Register(
                nameof(MinSaturation),
                typeof(int),
                typeof(ColorSpectrum),
                new PropertyMetadata(0, OnPercentageRangePropertyChanged));

        public int MinSaturation
        {
            get => (int)GetValue(MinSaturationProperty);
            set => SetValue(MinSaturationProperty, value);
        }

        public static readonly DependencyProperty MaxSaturationProperty =
            DependencyProperty.Register(
                nameof(MaxSaturation),
                typeof(int),
                typeof(ColorSpectrum),
                new PropertyMetadata(100, OnPercentageRangePropertyChanged));

        public int MaxSaturation
        {
            get => (int)GetValue(MaxSaturationProperty);
            set => SetValue(MaxSaturationProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(
                nameof(MinValue),
                typeof(int),
                typeof(ColorSpectrum),
                new PropertyMetadata(0, OnPercentageRangePropertyChanged));

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                nameof(MaxValue),
                typeof(int),
                typeof(ColorSpectrum),
                new PropertyMetadata(100, OnPercentageRangePropertyChanged));

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register(
                nameof(Shape),
                typeof(ColorSpectrumShape),
                typeof(ColorSpectrum),
                new PropertyMetadata(ColorSpectrumShape.Box, OnTemplatePropertyChanged));

        public ColorSpectrumShape Shape
        {
            get => (ColorSpectrumShape)GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        public static readonly DependencyProperty ComponentsProperty =
            DependencyProperty.Register(
                nameof(Components),
                typeof(ColorSpectrumComponents),
                typeof(ColorSpectrum),
                new PropertyMetadata(ColorSpectrumComponents.HueSaturation, OnTemplatePropertyChanged));

        public ColorSpectrumComponents Components
        {
            get => (ColorSpectrumComponents)GetValue(ComponentsProperty);
            set => SetValue(ComponentsProperty, value);
        }

        public event TypedEventHandler<ColorSpectrum, ColorChangedEventArgs> ColorChanged;

        public override void OnApplyTemplate()
        {
            if (_spectrumRectangle != null)
            {
                _spectrumRectangle.MouseLeftButtonDown -= OnSpectrumMouseLeftButtonDown;
                _spectrumRectangle.MouseMove -= OnSpectrumMouseMove;
            }

            base.OnApplyTemplate();

            _spectrumRectangle = GetTemplateChild(SpectrumRectangleName) as Rectangle;
            _spectrumOverlayRectangle = GetTemplateChild(SpectrumOverlayRectangleName) as Rectangle;
            _spectrumEllipse = GetTemplateChild(SpectrumEllipseName) as Ellipse;
            _spectrumOverlayEllipse = GetTemplateChild(SpectrumOverlayEllipseName) as Ellipse;
            _selectionEllipsePanel = GetTemplateChild(SelectionEllipsePanelName) as FrameworkElement;
            _selectionEllipse = GetTemplateChild(SelectionEllipseName) as Ellipse;

            if (_spectrumRectangle != null)
            {
                _spectrumRectangle.MouseLeftButtonDown += OnSpectrumMouseLeftButtonDown;
                _spectrumRectangle.MouseMove += OnSpectrumMouseMove;
            }

            UpdateShapeVisibility();
            UpdateSelection();
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSpectrum = (ColorSpectrum)d;
            var oldColor = (Color)e.OldValue;
            var newColor = (Color)e.NewValue;

            if (!colorSpectrum._updatingColorFromHsv)
            {
                colorSpectrum._updatingHsvFromColor = true;
                try
                {
                    colorSpectrum.SetCurrentValue(HsvColorProperty, ColorConversion.RgbToHsv(newColor));
                }
                finally
                {
                    colorSpectrum._updatingHsvFromColor = false;
                }
            }

            colorSpectrum.UpdateSelection();

            if (oldColor != newColor)
            {
                colorSpectrum.ColorChanged?.Invoke(colorSpectrum, new ColorChangedEventArgs(oldColor, newColor));
            }
        }

        private static void OnHsvColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSpectrum = (ColorSpectrum)d;
            if (colorSpectrum._updatingHsvFromColor)
            {
                colorSpectrum.UpdateSelection();
                return;
            }

            colorSpectrum._updatingColorFromHsv = true;
            try
            {
                colorSpectrum.SetCurrentValue(ColorProperty, ColorConversion.HsvToRgb((Vector4)e.NewValue));
            }
            finally
            {
                colorSpectrum._updatingColorFromHsv = false;
            }

            colorSpectrum.UpdateSelection();
        }

        private static void OnHueRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorConversion.ValidateHue((int)e.NewValue, e.Property.Name);
            ((ColorSpectrum)d).UpdateSelection();
        }

        private static void OnPercentageRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorConversion.ValidatePercentage((int)e.NewValue, e.Property.Name);
            ((ColorSpectrum)d).UpdateSelection();
        }

        private static void OnTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSpectrum = (ColorSpectrum)d;
            colorSpectrum.UpdateShapeVisibility();
            colorSpectrum.UpdateSelection();
        }

        private void OnSpectrumMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_spectrumRectangle == null)
            {
                return;
            }

            _spectrumRectangle.CaptureMouse();
            SetColorFromPoint(e.GetPosition(_spectrumRectangle));
            e.Handled = true;
        }

        private void OnSpectrumMouseMove(object sender, MouseEventArgs e)
        {
            if (_spectrumRectangle?.IsMouseCaptured == true && e.LeftButton == MouseButtonState.Pressed)
            {
                SetColorFromPoint(e.GetPosition(_spectrumRectangle));
                e.Handled = true;
            }
            else if (_spectrumRectangle?.IsMouseCaptured == true)
            {
                _spectrumRectangle.ReleaseMouseCapture();
            }
        }

        private void SetColorFromPoint(Point point)
        {
            if (_spectrumRectangle == null)
            {
                return;
            }

            var width = Math.Max(1, _spectrumRectangle.ActualWidth);
            var height = Math.Max(1, _spectrumRectangle.ActualHeight);
            var x = Clamp01(point.X / width);
            var y = Clamp01(1 - point.Y / height);
            var hsv = HsvColor;

            switch (Components)
            {
                case ColorSpectrumComponents.HueValue:
                    hsv.X = (float)(MinHue + x * (MaxHue - MinHue));
                    hsv.Z = (float)(MinValue / 100.0 + y * ((MaxValue - MinValue) / 100.0));
                    break;

                case ColorSpectrumComponents.ValueHue:
                    hsv.Z = (float)(MinValue / 100.0 + x * ((MaxValue - MinValue) / 100.0));
                    hsv.X = (float)(MinHue + y * (MaxHue - MinHue));
                    break;

                case ColorSpectrumComponents.SaturationHue:
                    hsv.Y = (float)(MinSaturation / 100.0 + x * ((MaxSaturation - MinSaturation) / 100.0));
                    hsv.X = (float)(MinHue + y * (MaxHue - MinHue));
                    break;

                case ColorSpectrumComponents.SaturationValue:
                    hsv.Y = (float)(MinSaturation / 100.0 + x * ((MaxSaturation - MinSaturation) / 100.0));
                    hsv.Z = (float)(MinValue / 100.0 + y * ((MaxValue - MinValue) / 100.0));
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                    hsv.Z = (float)(MinValue / 100.0 + x * ((MaxValue - MinValue) / 100.0));
                    hsv.Y = (float)(MinSaturation / 100.0 + y * ((MaxSaturation - MinSaturation) / 100.0));
                    break;

                default:
                    hsv.X = (float)(MinHue + x * (MaxHue - MinHue));
                    hsv.Y = (float)(MinSaturation / 100.0 + y * ((MaxSaturation - MinSaturation) / 100.0));
                    break;
            }

            SetCurrentValue(HsvColorProperty, hsv);
        }

        private void UpdateSelection()
        {
            if (_selectionEllipse == null || _selectionEllipsePanel == null || _spectrumRectangle == null)
            {
                return;
            }

            var hsv = HsvColor;
            var x = MaxHue == MinHue ? 0 : (hsv.X - MinHue) / (MaxHue - MinHue);
            var y = MaxSaturation == MinSaturation ? 0 : (hsv.Y - MinSaturation / 100f) / ((MaxSaturation - MinSaturation) / 100f);

            switch (Components)
            {
                case ColorSpectrumComponents.HueValue:
                    y = MaxValue == MinValue ? 0 : (hsv.Z - MinValue / 100f) / ((MaxValue - MinValue) / 100f);
                    break;

                case ColorSpectrumComponents.ValueHue:
                    x = MaxValue == MinValue ? 0 : (hsv.Z - MinValue / 100f) / ((MaxValue - MinValue) / 100f);
                    y = MaxHue == MinHue ? 0 : (hsv.X - MinHue) / (MaxHue - MinHue);
                    break;

                case ColorSpectrumComponents.SaturationHue:
                    x = MaxSaturation == MinSaturation ? 0 : (hsv.Y - MinSaturation / 100f) / ((MaxSaturation - MinSaturation) / 100f);
                    y = MaxHue == MinHue ? 0 : (hsv.X - MinHue) / (MaxHue - MinHue);
                    break;

                case ColorSpectrumComponents.SaturationValue:
                    x = MaxSaturation == MinSaturation ? 0 : (hsv.Y - MinSaturation / 100f) / ((MaxSaturation - MinSaturation) / 100f);
                    y = MaxValue == MinValue ? 0 : (hsv.Z - MinValue / 100f) / ((MaxValue - MinValue) / 100f);
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                    x = MaxValue == MinValue ? 0 : (hsv.Z - MinValue / 100f) / ((MaxValue - MinValue) / 100f);
                    y = MaxSaturation == MinSaturation ? 0 : (hsv.Y - MinSaturation / 100f) / ((MaxSaturation - MinSaturation) / 100f);
                    break;
            }

            Canvas.SetLeft(_selectionEllipsePanel, Clamp01(x) * Math.Max(0, _spectrumRectangle.ActualWidth) - _selectionEllipsePanel.Width / 2);
            Canvas.SetTop(_selectionEllipsePanel, (1 - Clamp01(y)) * Math.Max(0, _spectrumRectangle.ActualHeight) - _selectionEllipsePanel.Height / 2);
        }

        private void UpdateShapeVisibility()
        {
            var boxVisible = Shape == ColorSpectrumShape.Box;
            SetVisibility(_spectrumRectangle, boxVisible);
            SetVisibility(_spectrumOverlayRectangle, boxVisible);
            SetVisibility(_spectrumEllipse, !boxVisible);
            SetVisibility(_spectrumOverlayEllipse, !boxVisible);
        }

        private static void SetVisibility(UIElement element, bool visible)
        {
            if (element != null)
            {
                element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0, Math.Min(1, value));
        }

        private bool _updatingColorFromHsv;
        private bool _updatingHsvFromColor;
        private Rectangle _spectrumRectangle;
        private Rectangle _spectrumOverlayRectangle;
        private Ellipse _spectrumEllipse;
        private Ellipse _spectrumOverlayEllipse;
        private FrameworkElement _selectionEllipsePanel;
        private Ellipse _selectionEllipse;
    }
}
