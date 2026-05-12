using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class ColorPicker : Control
    {
        private const string ColorSpectrumName = "ColorSpectrum";
        private const string ThirdDimensionSliderName = "ThirdDimensionSlider";
        private const string AlphaSliderName = "AlphaSlider";
        private const string ColorPreviewRectangleName = "ColorPreviewRectangle";
        private const string PreviousColorRectangleName = "PreviousColorRectangle";
        private const string MoreButtonName = "MoreButton";
        private const string HexTextBoxName = "HexTextBox";
        private const string AlphaTextBoxName = "AlphaTextBox";
        private const string RedTextBoxName = "RedTextBox";
        private const string GreenTextBoxName = "GreenTextBox";
        private const string BlueTextBoxName = "BlueTextBox";
        private const string HueTextBoxName = "HueTextBox";
        private const string SaturationTextBoxName = "SaturationTextBox";
        private const string ValueTextBoxName = "ValueTextBox";

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(
                nameof(Color),
                typeof(Color),
                typeof(ColorPicker),
                new PropertyMetadata(Colors.White, OnColorPropertyChanged));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty PreviousColorProperty =
            DependencyProperty.Register(
                nameof(PreviousColor),
                typeof(Color?),
                typeof(ColorPicker),
                new PropertyMetadata(null, OnTemplatePropertyChanged));

        public Color? PreviousColor
        {
            get => (Color?)GetValue(PreviousColorProperty);
            set => SetValue(PreviousColorProperty, value);
        }

        public static readonly DependencyProperty IsAlphaEnabledProperty =
            DependencyProperty.Register(
                nameof(IsAlphaEnabled),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(false, OnTemplatePropertyChanged));

        public bool IsAlphaEnabled
        {
            get => (bool)GetValue(IsAlphaEnabledProperty);
            set => SetValue(IsAlphaEnabledProperty, value);
        }

        public static readonly DependencyProperty IsColorSpectrumVisibleProperty =
            DependencyProperty.Register(
                nameof(IsColorSpectrumVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsColorSpectrumVisible
        {
            get => (bool)GetValue(IsColorSpectrumVisibleProperty);
            set => SetValue(IsColorSpectrumVisibleProperty, value);
        }

        public static readonly DependencyProperty IsColorPreviewVisibleProperty =
            DependencyProperty.Register(
                nameof(IsColorPreviewVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsColorPreviewVisible
        {
            get => (bool)GetValue(IsColorPreviewVisibleProperty);
            set => SetValue(IsColorPreviewVisibleProperty, value);
        }

        public static readonly DependencyProperty IsColorSliderVisibleProperty =
            DependencyProperty.Register(
                nameof(IsColorSliderVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsColorSliderVisible
        {
            get => (bool)GetValue(IsColorSliderVisibleProperty);
            set => SetValue(IsColorSliderVisibleProperty, value);
        }

        public static readonly DependencyProperty IsAlphaSliderVisibleProperty =
            DependencyProperty.Register(
                nameof(IsAlphaSliderVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsAlphaSliderVisible
        {
            get => (bool)GetValue(IsAlphaSliderVisibleProperty);
            set => SetValue(IsAlphaSliderVisibleProperty, value);
        }

        public static readonly DependencyProperty IsMoreButtonVisibleProperty =
            DependencyProperty.Register(
                nameof(IsMoreButtonVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(false, OnTemplatePropertyChanged));

        public bool IsMoreButtonVisible
        {
            get => (bool)GetValue(IsMoreButtonVisibleProperty);
            set => SetValue(IsMoreButtonVisibleProperty, value);
        }

        public static readonly DependencyProperty IsColorChannelTextInputVisibleProperty =
            DependencyProperty.Register(
                nameof(IsColorChannelTextInputVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsColorChannelTextInputVisible
        {
            get => (bool)GetValue(IsColorChannelTextInputVisibleProperty);
            set => SetValue(IsColorChannelTextInputVisibleProperty, value);
        }

        public static readonly DependencyProperty IsAlphaTextInputVisibleProperty =
            DependencyProperty.Register(
                nameof(IsAlphaTextInputVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsAlphaTextInputVisible
        {
            get => (bool)GetValue(IsAlphaTextInputVisibleProperty);
            set => SetValue(IsAlphaTextInputVisibleProperty, value);
        }

        public static readonly DependencyProperty IsHexInputVisibleProperty =
            DependencyProperty.Register(
                nameof(IsHexInputVisible),
                typeof(bool),
                typeof(ColorPicker),
                new PropertyMetadata(true, OnTemplatePropertyChanged));

        public bool IsHexInputVisible
        {
            get => (bool)GetValue(IsHexInputVisibleProperty);
            set => SetValue(IsHexInputVisibleProperty, value);
        }

        public static readonly DependencyProperty MinHueProperty =
            DependencyProperty.Register(
                nameof(MinHue),
                typeof(int),
                typeof(ColorPicker),
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
                typeof(ColorPicker),
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
                typeof(ColorPicker),
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
                typeof(ColorPicker),
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
                typeof(ColorPicker),
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
                typeof(ColorPicker),
                new PropertyMetadata(100, OnPercentageRangePropertyChanged));

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty ColorSpectrumShapeProperty =
            DependencyProperty.Register(
                nameof(ColorSpectrumShape),
                typeof(ColorSpectrumShape),
                typeof(ColorPicker),
                new PropertyMetadata(ColorSpectrumShape.Box, OnTemplatePropertyChanged));

        public ColorSpectrumShape ColorSpectrumShape
        {
            get => (ColorSpectrumShape)GetValue(ColorSpectrumShapeProperty);
            set => SetValue(ColorSpectrumShapeProperty, value);
        }

        public static readonly DependencyProperty ColorSpectrumComponentsProperty =
            DependencyProperty.Register(
                nameof(ColorSpectrumComponents),
                typeof(ColorSpectrumComponents),
                typeof(ColorPicker),
                new PropertyMetadata(ColorSpectrumComponents.HueSaturation, OnTemplatePropertyChanged));

        public ColorSpectrumComponents ColorSpectrumComponents
        {
            get => (ColorSpectrumComponents)GetValue(ColorSpectrumComponentsProperty);
            set => SetValue(ColorSpectrumComponentsProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(ColorPicker),
                new PropertyMetadata(Orientation.Vertical, OnTemplatePropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public event TypedEventHandler<ColorPicker, ColorChangedEventArgs> ColorChanged;

        public override void OnApplyTemplate()
        {
            UnhookTemplateEvents();

            base.OnApplyTemplate();

            _colorSpectrum = GetTemplateChild(ColorSpectrumName) as ColorSpectrum;
            _thirdDimensionSlider = GetTemplateChild(ThirdDimensionSliderName) as ColorPickerSlider;
            _alphaSlider = GetTemplateChild(AlphaSliderName) as ColorPickerSlider;
            _previewRectangle = GetTemplateChild(ColorPreviewRectangleName) as Rectangle;
            _previousColorRectangle = GetTemplateChild(PreviousColorRectangleName) as Rectangle;
            _moreButton = GetTemplateChild(MoreButtonName) as ButtonBase;
            _hexTextBox = GetTemplateChild(HexTextBoxName) as TextBox;
            _alphaTextBox = GetTemplateChild(AlphaTextBoxName) as TextBox;
            _redTextBox = GetTemplateChild(RedTextBoxName) as TextBox;
            _greenTextBox = GetTemplateChild(GreenTextBoxName) as TextBox;
            _blueTextBox = GetTemplateChild(BlueTextBoxName) as TextBox;
            _hueTextBox = GetTemplateChild(HueTextBoxName) as TextBox;
            _saturationTextBox = GetTemplateChild(SaturationTextBoxName) as TextBox;
            _valueTextBox = GetTemplateChild(ValueTextBoxName) as TextBox;

            HookTemplateEvents();
            UpdateTemplate();
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            var oldColor = (Color)e.OldValue;
            var newColor = (Color)e.NewValue;

            colorPicker.UpdateTemplate();

            if (oldColor != newColor)
            {
                colorPicker.ColorChanged?.Invoke(colorPicker, new ColorChangedEventArgs(oldColor, newColor));
            }
        }

        private static void OnHueRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorConversion.ValidateHue((int)e.NewValue, e.Property.Name);
            ((ColorPicker)d).CoerceColorToRanges();
            ((ColorPicker)d).UpdateTemplate();
        }

        private static void OnPercentageRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorConversion.ValidatePercentage((int)e.NewValue, e.Property.Name);
            ((ColorPicker)d).CoerceColorToRanges();
            ((ColorPicker)d).UpdateTemplate();
        }

        private static void OnTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).UpdateTemplate();
        }

        private void HookTemplateEvents()
        {
            if (_colorSpectrum != null)
            {
                _colorSpectrum.ColorChanged += OnSpectrumColorChanged;
            }

            if (_thirdDimensionSlider != null)
            {
                _thirdDimensionSlider.ValueChanged += OnThirdDimensionSliderValueChanged;
            }

            if (_alphaSlider != null)
            {
                _alphaSlider.ValueChanged += OnAlphaSliderValueChanged;
            }
        }

        private void UnhookTemplateEvents()
        {
            if (_colorSpectrum != null)
            {
                _colorSpectrum.ColorChanged -= OnSpectrumColorChanged;
            }

            if (_thirdDimensionSlider != null)
            {
                _thirdDimensionSlider.ValueChanged -= OnThirdDimensionSliderValueChanged;
            }

            if (_alphaSlider != null)
            {
                _alphaSlider.ValueChanged -= OnAlphaSliderValueChanged;
            }
        }

        private void OnSpectrumColorChanged(ColorSpectrum sender, ColorChangedEventArgs args)
        {
            if (_updatingTemplate)
            {
                return;
            }

            SetCurrentValue(ColorProperty, args.NewColor);
        }

        private void OnThirdDimensionSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingTemplate)
            {
                return;
            }

            var hsv = ColorConversion.RgbToHsv(Color);
            hsv.Z = (float)(e.NewValue / 100.0);
            SetCurrentValue(ColorProperty, ColorConversion.HsvToRgb(hsv));
        }

        private void OnAlphaSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingTemplate)
            {
                return;
            }

            var color = Color;
            color.A = (byte)Math.Round(Math.Max(0, Math.Min(100, e.NewValue)) / 100.0 * 255, MidpointRounding.AwayFromZero);
            SetCurrentValue(ColorProperty, color);
        }

        private void CoerceColorToRanges()
        {
            var hsv = ColorConversion.ClampHsv(
                ColorConversion.RgbToHsv(Color),
                MinHue,
                MaxHue,
                MinSaturation,
                MaxSaturation,
                MinValue,
                MaxValue);
            var coercedColor = ColorConversion.HsvToRgb(hsv);

            if (coercedColor != Color)
            {
                SetCurrentValue(ColorProperty, coercedColor);
            }
        }

        private void UpdateTemplate()
        {
            _updatingTemplate = true;
            try
            {
                var hsv = ColorConversion.RgbToHsv(Color);

                if (_colorSpectrum != null)
                {
                    _colorSpectrum.Color = Color;
                    _colorSpectrum.MinHue = MinHue;
                    _colorSpectrum.MaxHue = MaxHue;
                    _colorSpectrum.MinSaturation = MinSaturation;
                    _colorSpectrum.MaxSaturation = MaxSaturation;
                    _colorSpectrum.MinValue = MinValue;
                    _colorSpectrum.MaxValue = MaxValue;
                    _colorSpectrum.Shape = ColorSpectrumShape;
                    _colorSpectrum.Components = ColorSpectrumComponents;
                    _colorSpectrum.Visibility = ToVisibility(IsColorSpectrumVisible);
                }

                if (_thirdDimensionSlider != null)
                {
                    _thirdDimensionSlider.Value = hsv.Z * 100;
                    _thirdDimensionSlider.Visibility = ToVisibility(IsColorSliderVisible);
                }

                if (_alphaSlider != null)
                {
                    _alphaSlider.Value = Color.A / 255.0 * 100;
                    _alphaSlider.Visibility = ToVisibility(IsAlphaEnabled && IsAlphaSliderVisible);
                }

                if (_previewRectangle != null)
                {
                    _previewRectangle.Fill = new SolidColorBrush(Color);
                    _previewRectangle.Visibility = ToVisibility(IsColorPreviewVisible);
                }

                if (_previousColorRectangle != null)
                {
                    _previousColorRectangle.Fill = PreviousColor.HasValue ? new SolidColorBrush(PreviousColor.Value) : Brushes.Transparent;
                    _previousColorRectangle.Visibility = ToVisibility(IsColorPreviewVisible && PreviousColor.HasValue);
                }

                if (_moreButton != null)
                {
                    _moreButton.Visibility = ToVisibility(IsMoreButtonVisible);
                }

                UpdateTextBoxes(hsv);
            }
            finally
            {
                _updatingTemplate = false;
            }
        }

        private void UpdateTextBoxes(Vector4 hsv)
        {
            SetText(_hexTextBox, ToHex(Color), IsHexInputVisible);
            SetText(_alphaTextBox, Math.Round(Color.A / 255.0 * 100).ToString("0"), IsAlphaEnabled && IsAlphaTextInputVisible);
            SetText(_redTextBox, Color.R.ToString(), IsColorChannelTextInputVisible);
            SetText(_greenTextBox, Color.G.ToString(), IsColorChannelTextInputVisible);
            SetText(_blueTextBox, Color.B.ToString(), IsColorChannelTextInputVisible);
            SetText(_hueTextBox, Math.Round(hsv.X).ToString("0"), IsColorChannelTextInputVisible);
            SetText(_saturationTextBox, Math.Round(hsv.Y * 100).ToString("0"), IsColorChannelTextInputVisible);
            SetText(_valueTextBox, Math.Round(hsv.Z * 100).ToString("0"), IsColorChannelTextInputVisible);
        }

        private static void SetText(TextBox textBox, string text, bool visible)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.Text = text;
            textBox.Visibility = ToVisibility(visible);
        }

        private static string ToHex(Color color)
        {
            return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        private static Visibility ToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool _updatingTemplate;
        private ColorSpectrum _colorSpectrum;
        private ColorPickerSlider _thirdDimensionSlider;
        private ColorPickerSlider _alphaSlider;
        private Rectangle _previewRectangle;
        private Rectangle _previousColorRectangle;
        private ButtonBase _moreButton;
        private TextBox _hexTextBox;
        private TextBox _alphaTextBox;
        private TextBox _redTextBox;
        private TextBox _greenTextBox;
        private TextBox _blueTextBox;
        private TextBox _hueTextBox;
        private TextBox _saturationTextBox;
        private TextBox _valueTextBox;
    }
}
