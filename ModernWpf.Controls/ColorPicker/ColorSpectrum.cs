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

        public ColorSpectrum()
        {
            Focusable = true;
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
            UnhookSpectrumInput(_spectrumRectangle);
            UnhookSpectrumInput(_spectrumEllipse);

            base.OnApplyTemplate();

            _spectrumRectangle = GetTemplateChild(SpectrumRectangleName) as Rectangle;
            _spectrumOverlayRectangle = GetTemplateChild(SpectrumOverlayRectangleName) as Rectangle;
            _spectrumEllipse = GetTemplateChild(SpectrumEllipseName) as Ellipse;
            _spectrumOverlayEllipse = GetTemplateChild(SpectrumOverlayEllipseName) as Ellipse;
            _selectionEllipsePanel = GetTemplateChild(SelectionEllipsePanelName) as FrameworkElement;
            _selectionEllipse = GetTemplateChild(SelectionEllipseName) as Ellipse;

            HookSpectrumInput(_spectrumRectangle);
            HookSpectrumInput(_spectrumEllipse);

            UpdateShapeVisibility();
            UpdateSelection();
        }

        internal void SetColorFromPointForTesting(Point point)
        {
            SetColorFromPoint(point, Shape == ColorSpectrumShape.Ring ? (FrameworkElement)_spectrumEllipse : _spectrumRectangle);
        }

        internal void AdjustColorForTesting(int horizontalSteps, int verticalSteps)
        {
            AdjustColorFromKeyboard(horizontalSteps, verticalSteps);
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Left:
                    AdjustColorFromKeyboard(-1, 0);
                    e.Handled = true;
                    break;

                case Key.Right:
                    AdjustColorFromKeyboard(1, 0);
                    e.Handled = true;
                    break;

                case Key.Up:
                    AdjustColorFromKeyboard(0, 1);
                    e.Handled = true;
                    break;

                case Key.Down:
                    AdjustColorFromKeyboard(0, -1);
                    e.Handled = true;
                    break;
            }
        }

        private void HookSpectrumInput(FrameworkElement element)
        {
            if (element != null)
            {
                element.MouseLeftButtonDown += OnSpectrumMouseLeftButtonDown;
                element.MouseMove += OnSpectrumMouseMove;
            }
        }

        private void UnhookSpectrumInput(FrameworkElement element)
        {
            if (element != null)
            {
                element.MouseLeftButtonDown -= OnSpectrumMouseLeftButtonDown;
                element.MouseMove -= OnSpectrumMouseMove;
            }
        }

        private void OnSpectrumMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement source)
            {
                return;
            }

            Focus();
            source.CaptureMouse();
            SetColorFromPoint(e.GetPosition(source), source);
            e.Handled = true;
        }

        private void OnSpectrumMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not FrameworkElement source)
            {
                return;
            }

            if (source.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                SetColorFromPoint(e.GetPosition(source), source);
                e.Handled = true;
            }
            else if (source.IsMouseCaptured)
            {
                source.ReleaseMouseCapture();
            }
        }

        private void SetColorFromPoint(Point point, FrameworkElement source)
        {
            if (source == null)
            {
                return;
            }

            if (Shape == ColorSpectrumShape.Ring && ReferenceEquals(source, _spectrumEllipse))
            {
                SetRingColorFromPoint(point, source);
                return;
            }

            var width = Math.Max(1, source.ActualWidth);
            var height = Math.Max(1, source.ActualHeight);
            var x = Clamp01(point.X / width);
            var y = Clamp01(1 - point.Y / height);
            SetColorFromNormalizedPoint(x, y);
        }

        private void SetColorFromNormalizedPoint(double x, double y)
        {
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

        private void SetRingColorFromPoint(Point point, FrameworkElement source)
        {
            var width = Math.Max(1, source.ActualWidth);
            var height = Math.Max(1, source.ActualHeight);
            var radius = Math.Max(1, Math.Min(width, height) / 2);
            var center = new Point(width / 2, height / 2);
            var dx = point.X - center.X;
            var dy = center.Y - point.Y;
            var angle = (Math.Atan2(dy, dx) * 180 / Math.PI + 360) % 360;
            var radial = Clamp01(Math.Sqrt(dx * dx + dy * dy) / radius);

            SetColorFromNormalizedPoint(angle / 359.0, radial);
        }

        private void UpdateSelection()
        {
            if (_selectionEllipse == null || _selectionEllipsePanel == null || _spectrumRectangle == null)
            {
                return;
            }

            var hsv = HsvColor;
            if (Shape == ColorSpectrumShape.Ring)
            {
                UpdateRingSelection(hsv);
                return;
            }

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

        private void UpdateRingSelection(Vector4 hsv)
        {
            var hueRatio = MaxHue == MinHue ? 0 : Clamp01((hsv.X - MinHue) / (MaxHue - MinHue));
            var radialRatio = GetVerticalComponent() == HsvComponent.Value
                ? MaxValue == MinValue ? 0 : Clamp01((hsv.Z - MinValue / 100f) / ((MaxValue - MinValue) / 100f))
                : MaxSaturation == MinSaturation ? 0 : Clamp01((hsv.Y - MinSaturation / 100f) / ((MaxSaturation - MinSaturation) / 100f));

            var width = Math.Max(0, _spectrumRectangle.ActualWidth);
            var height = Math.Max(0, _spectrumRectangle.ActualHeight);
            var radius = Math.Min(width, height) / 2 * radialRatio;
            var angle = hueRatio * 359 * Math.PI / 180;
            var centerX = width / 2;
            var centerY = height / 2;

            Canvas.SetLeft(_selectionEllipsePanel, centerX + Math.Cos(angle) * radius - _selectionEllipsePanel.Width / 2);
            Canvas.SetTop(_selectionEllipsePanel, centerY - Math.Sin(angle) * radius - _selectionEllipsePanel.Height / 2);
        }

        private void AdjustColorFromKeyboard(int horizontalSteps, int verticalSteps)
        {
            if (horizontalSteps == 0 && verticalSteps == 0)
            {
                return;
            }

            var hsv = HsvColor;
            ApplyComponentStep(ref hsv, GetHorizontalComponent(), horizontalSteps);
            ApplyComponentStep(ref hsv, GetVerticalComponent(), verticalSteps);
            SetCurrentValue(HsvColorProperty, hsv);
        }

        private void ApplyComponentStep(ref Vector4 hsv, HsvComponent component, int steps)
        {
            if (steps == 0)
            {
                return;
            }

            switch (component)
            {
                case HsvComponent.Hue:
                    hsv.X = (float)Math.Max(MinHue, Math.Min(MaxHue, hsv.X + steps));
                    break;

                case HsvComponent.Saturation:
                    hsv.Y = (float)Math.Max(MinSaturation / 100.0, Math.Min(MaxSaturation / 100.0, hsv.Y + steps / 100.0));
                    break;

                case HsvComponent.Value:
                    hsv.Z = (float)Math.Max(MinValue / 100.0, Math.Min(MaxValue / 100.0, hsv.Z + steps / 100.0));
                    break;
            }
        }

        private HsvComponent GetHorizontalComponent()
        {
            switch (Components)
            {
                case ColorSpectrumComponents.ValueHue:
                case ColorSpectrumComponents.ValueSaturation:
                    return HsvComponent.Value;

                case ColorSpectrumComponents.SaturationHue:
                case ColorSpectrumComponents.SaturationValue:
                    return HsvComponent.Saturation;

                default:
                    return HsvComponent.Hue;
            }
        }

        private HsvComponent GetVerticalComponent()
        {
            switch (Components)
            {
                case ColorSpectrumComponents.HueValue:
                case ColorSpectrumComponents.SaturationValue:
                    return HsvComponent.Value;

                case ColorSpectrumComponents.ValueHue:
                case ColorSpectrumComponents.SaturationHue:
                    return HsvComponent.Hue;

                default:
                    return HsvComponent.Saturation;
            }
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

        private enum HsvComponent
        {
            Hue,
            Saturation,
            Value
        }
    }
}
