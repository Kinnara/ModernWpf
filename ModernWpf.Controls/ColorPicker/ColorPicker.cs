using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;
using static ModernWpf.Controls.ColorConversion;
using WpfPrimitives = System.Windows.Controls.Primitives;

namespace ModernWpf.Controls
{
    public partial class ColorPicker : Control
    {
        private const string ColorSpectrumName = "ColorSpectrum";
        private const string ColorPreviewRectangleGridName = "ColorPreviewRectangleGrid";
        private const string ColorPreviewRectangleName = "ColorPreviewRectangle";
        private const string PreviousColorRectangleName = "PreviousColorRectangle";
        private const string ColorPreviewCheckeredBackgroundRectangleName = "ColorPreviewCheckeredBackgroundRectangle";
        private const string ThirdDimensionSliderName = "ThirdDimensionSlider";
        private const string ThirdDimensionSliderGradientBrushName = "ThirdDimensionSliderGradientBrush";
        private const string AlphaSliderName = "AlphaSlider";
        private const string AlphaSliderGradientBrushName = "AlphaSliderGradientBrush";
        private const string AlphaSliderBackgroundRectangleName = "AlphaSliderBackgroundRectangle";
        private const string AlphaSliderCheckeredBackgroundRectangleName = "AlphaSliderCheckeredBackgroundRectangle";
        private const string MoreButtonName = "MoreButton";
        private const string MoreButtonLabelName = "MoreButtonLabel";
        private const string ColorRepresentationComboBoxName = "ColorRepresentationComboBox";
        private const string RGBComboBoxItemName = "RGBComboBoxItem";
        private const string HSVComboBoxItemName = "HSVComboBoxItem";
        private const string RedTextBoxName = "RedTextBox";
        private const string GreenTextBoxName = "GreenTextBox";
        private const string BlueTextBoxName = "BlueTextBox";
        private const string HueTextBoxName = "HueTextBox";
        private const string SaturationTextBoxName = "SaturationTextBox";
        private const string ValueTextBoxName = "ValueTextBox";
        private const string AlphaTextBoxName = "AlphaTextBox";
        private const string HexTextBoxName = "HexTextBox";
        private const string RedLabelName = "RedLabel";
        private const string GreenLabelName = "GreenLabel";
        private const string BlueLabelName = "BlueLabel";
        private const string HueLabelName = "HueLabel";
        private const string SaturationLabelName = "SaturationLabel";
        private const string ValueLabelName = "ValueLabel";
        private const string AlphaLabelName = "AlphaLabel";

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public event TypedEventHandler<ColorPicker, ColorChangedEventArgs> ColorChanged;

        public override void OnApplyTemplate()
        {
            UnhookTemplateEvents();

            base.OnApplyTemplate();

            _colorSpectrum = GetTemplateChild(ColorSpectrumName) as ColorSpectrum;
            _colorPreviewRectangleGrid = GetTemplateChild(ColorPreviewRectangleGridName) as Grid;
            _colorPreviewRectangle = GetTemplateChild(ColorPreviewRectangleName) as Rectangle;
            _previousColorRectangle = GetTemplateChild(PreviousColorRectangleName) as Rectangle;
            _colorPreviewCheckeredBackgroundRectangle = GetTemplateChild(ColorPreviewCheckeredBackgroundRectangleName) as Rectangle;
            _thirdDimensionSlider = GetTemplateChild(ThirdDimensionSliderName) as ColorPickerSlider;
            _thirdDimensionSliderGradientBrush = GetTemplateChild(ThirdDimensionSliderGradientBrushName) as LinearGradientBrush;
            _alphaSlider = GetTemplateChild(AlphaSliderName) as ColorPickerSlider;
            _alphaSliderGradientBrush = GetTemplateChild(AlphaSliderGradientBrushName) as LinearGradientBrush;
            _alphaSliderBackgroundRectangle = GetTemplateChild(AlphaSliderBackgroundRectangleName) as Rectangle;
            _alphaSliderCheckeredBackgroundRectangle = GetTemplateChild(AlphaSliderCheckeredBackgroundRectangleName) as Rectangle;
            _moreButton = GetTemplateChild(MoreButtonName) as WpfPrimitives.ButtonBase;
            _moreButtonLabel = GetTemplateChild(MoreButtonLabelName) as TextBlock;
            _colorRepresentationComboBox = GetTemplateChild(ColorRepresentationComboBoxName) as ComboBox;
            _rgbComboBoxItem = GetTemplateChild(RGBComboBoxItemName) as ComboBoxItem;
            _hsvComboBoxItem = GetTemplateChild(HSVComboBoxItemName) as ComboBoxItem;
            _redTextBox = GetTemplateChild(RedTextBoxName) as TextBox;
            _greenTextBox = GetTemplateChild(GreenTextBoxName) as TextBox;
            _blueTextBox = GetTemplateChild(BlueTextBoxName) as TextBox;
            _hueTextBox = GetTemplateChild(HueTextBoxName) as TextBox;
            _saturationTextBox = GetTemplateChild(SaturationTextBoxName) as TextBox;
            _valueTextBox = GetTemplateChild(ValueTextBoxName) as TextBox;
            _alphaTextBox = GetTemplateChild(AlphaTextBoxName) as TextBox;
            _hexTextBox = GetTemplateChild(HexTextBoxName) as TextBox;
            _redLabel = GetTemplateChild(RedLabelName) as TextBlock;
            _greenLabel = GetTemplateChild(GreenLabelName) as TextBlock;
            _blueLabel = GetTemplateChild(BlueLabelName) as TextBlock;
            _hueLabel = GetTemplateChild(HueLabelName) as TextBlock;
            _saturationLabel = GetTemplateChild(SaturationLabelName) as TextBlock;
            _valueLabel = GetTemplateChild(ValueLabelName) as TextBlock;
            _alphaLabel = GetTemplateChild(AlphaLabelName) as TextBlock;

            HookTemplateEvents();
            InitializeTemplateStrings();
            CreateColorPreviewCheckeredBackground();
            CreateAlphaSliderCheckeredBackground();
            UpdateVisualState(false);
            InitializeColor();
            UpdatePreviousColorRectangle();
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            var oldColor = (Color)e.OldValue;
            var newColor = (Color)e.NewValue;

            if (!colorPicker._updatingColor)
            {
                colorPicker.SetCurrentColorState(newColor);
                colorPicker.UpdateColorControls(ColorUpdateReason.ColorPropertyChanged);
            }

            if (oldColor != newColor)
            {
                colorPicker.ColorChanged?.Invoke(colorPicker, new ColorChangedEventArgs(oldColor, newColor));
            }
        }

        private static void OnPreviousColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            colorPicker.UpdatePreviousColorRectangle();
            colorPicker.UpdateVisualState(true);
        }

        private static void OnIsAlphaEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            colorPicker._currentHex = colorPicker.GetCurrentHexValue();
            colorPicker.UpdateColorControls(ColorUpdateReason.ColorPropertyChanged);
            colorPicker.UpdateVisualState(true);
        }

        private static void OnPartVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).UpdateVisualState(true);
        }

        private static void OnMinMaxHuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidateHue((int)e.NewValue, e.Property.Name);
            var colorPicker = (ColorPicker)d;
            colorPicker._currentHsv.H = Clamp(colorPicker._currentHsv.H, colorPicker.MinHue, colorPicker.MaxHue);
            colorPicker.UpdateColor(colorPicker._currentHsv, ColorUpdateReason.ColorPropertyChanged);
            colorPicker.UpdateThirdDimensionSlider();
        }

        private static void OnMinMaxSaturationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidatePercentage((int)e.NewValue, e.Property.Name);
            var colorPicker = (ColorPicker)d;
            colorPicker._currentHsv.S = Clamp(colorPicker._currentHsv.S, colorPicker.MinSaturation / 100.0, colorPicker.MaxSaturation / 100.0);
            colorPicker.UpdateColor(colorPicker._currentHsv, ColorUpdateReason.ColorPropertyChanged);
            colorPicker.UpdateThirdDimensionSlider();
        }

        private static void OnMinMaxValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidatePercentage((int)e.NewValue, e.Property.Name);
            var colorPicker = (ColorPicker)d;
            colorPicker._currentHsv.V = Clamp(colorPicker._currentHsv.V, colorPicker.MinValue / 100.0, colorPicker.MaxValue / 100.0);
            colorPicker.UpdateColor(colorPicker._currentHsv, ColorUpdateReason.ColorPropertyChanged);
            colorPicker.UpdateThirdDimensionSlider();
        }

        private static void OnColorSpectrumShapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            if (colorPicker._colorSpectrum != null)
            {
                colorPicker._colorSpectrum.Shape = colorPicker.ColorSpectrumShape;
            }

            colorPicker.UpdateVisualState(true);
        }

        private static void OnColorSpectrumComponentsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            if (colorPicker._colorSpectrum != null)
            {
                colorPicker._colorSpectrum.Components = colorPicker.ColorSpectrumComponents;
            }

            colorPicker.UpdateThirdDimensionSlider();
            colorPicker.SetThirdDimensionSliderChannel();
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).UpdateVisualState(true);
        }

        private void HookTemplateEvents()
        {
            if (_colorSpectrum != null)
            {
                _colorSpectrum.ColorChanged += OnColorSpectrumColorChanged;
                _colorSpectrum.SizeChanged += OnColorSpectrumSizeChanged;
                AutomationProperties.SetName(_colorSpectrum, "Color spectrum");
            }

            if (_colorPreviewRectangleGrid != null)
            {
                _colorPreviewRectangleGrid.SizeChanged += OnColorPreviewRectangleGridSizeChanged;
            }

            if (_thirdDimensionSlider != null)
            {
                _thirdDimensionSlider.ValueChanged += OnThirdDimensionSliderValueChanged;
                SetThirdDimensionSliderChannel();
            }

            if (_alphaSlider != null)
            {
                _alphaSlider.ValueChanged += OnAlphaSliderValueChanged;
                _alphaSlider.ColorChannel = ColorPickerHsvChannel.Alpha;
                AutomationProperties.SetName(_alphaSlider, "Alpha");
            }

            if (_alphaSliderBackgroundRectangle != null)
            {
                _alphaSliderBackgroundRectangle.SizeChanged += OnAlphaSliderBackgroundRectangleSizeChanged;
            }

            if (_moreButton != null)
            {
                if (_moreButton is WpfPrimitives.ToggleButton toggleButton)
                {
                    toggleButton.Checked += OnMoreButtonChecked;
                    toggleButton.Unchecked += OnMoreButtonUnchecked;
                }
                else
                {
                    _moreButton.Click += OnMoreButtonClicked;
                }

                AutomationProperties.SetHelpText(_moreButton, "Show or hide color text inputs.");
            }

            if (_colorRepresentationComboBox != null)
            {
                _colorRepresentationComboBox.SelectionChanged += OnColorRepresentationComboBoxSelectionChanged;
                AutomationProperties.SetName(_colorRepresentationComboBox, "Color model");
            }

            HookRgbTextBox(_redTextBox, "Red");
            HookRgbTextBox(_greenTextBox, "Green");
            HookRgbTextBox(_blueTextBox, "Blue");
            HookHsvTextBox(_hueTextBox, OnHueTextChanged, "Hue");
            HookHsvTextBox(_saturationTextBox, OnSaturationTextChanged, "Saturation");
            HookHsvTextBox(_valueTextBox, OnValueTextChanged, "Value");

            if (_alphaTextBox != null)
            {
                _alphaTextBox.TextChanged += OnAlphaTextChanged;
                HookTextBoxFocus(_alphaTextBox);
                AutomationProperties.SetName(_alphaTextBox, "Alpha");
            }

            if (_hexTextBox != null)
            {
                _hexTextBox.TextChanged += OnHexTextChanged;
                HookTextBoxFocus(_hexTextBox);
                UpdateHexTextBoxAutomationName();
            }
        }

        private void UnhookTemplateEvents()
        {
            if (_colorSpectrum != null)
            {
                _colorSpectrum.ColorChanged -= OnColorSpectrumColorChanged;
                _colorSpectrum.SizeChanged -= OnColorSpectrumSizeChanged;
            }

            if (_colorPreviewRectangleGrid != null)
            {
                _colorPreviewRectangleGrid.SizeChanged -= OnColorPreviewRectangleGridSizeChanged;
            }

            if (_thirdDimensionSlider != null)
            {
                _thirdDimensionSlider.ValueChanged -= OnThirdDimensionSliderValueChanged;
            }

            if (_alphaSlider != null)
            {
                _alphaSlider.ValueChanged -= OnAlphaSliderValueChanged;
            }

            if (_alphaSliderBackgroundRectangle != null)
            {
                _alphaSliderBackgroundRectangle.SizeChanged -= OnAlphaSliderBackgroundRectangleSizeChanged;
            }

            if (_moreButton != null)
            {
                if (_moreButton is WpfPrimitives.ToggleButton toggleButton)
                {
                    toggleButton.Checked -= OnMoreButtonChecked;
                    toggleButton.Unchecked -= OnMoreButtonUnchecked;
                }
                else
                {
                    _moreButton.Click -= OnMoreButtonClicked;
                }
            }

            if (_colorRepresentationComboBox != null)
            {
                _colorRepresentationComboBox.SelectionChanged -= OnColorRepresentationComboBoxSelectionChanged;
            }

            UnhookRgbTextBox(_redTextBox);
            UnhookRgbTextBox(_greenTextBox);
            UnhookRgbTextBox(_blueTextBox);
            UnhookHsvTextBox(_hueTextBox, OnHueTextChanged);
            UnhookHsvTextBox(_saturationTextBox, OnSaturationTextChanged);
            UnhookHsvTextBox(_valueTextBox, OnValueTextChanged);
            UnhookTextBox(_alphaTextBox, OnAlphaTextChanged);
            UnhookTextBox(_hexTextBox, OnHexTextChanged);
        }

        private void HookRgbTextBox(TextBox textBox, string automationName)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.TextChanged += OnRgbTextChanged;
            HookTextBoxFocus(textBox);
            AutomationProperties.SetName(textBox, automationName);
        }

        private void UnhookRgbTextBox(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.TextChanged -= OnRgbTextChanged;
            UnhookTextBoxFocus(textBox);
        }

        private void HookHsvTextBox(TextBox textBox, TextChangedEventHandler handler, string automationName)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.TextChanged += handler;
            HookTextBoxFocus(textBox);
            AutomationProperties.SetName(textBox, automationName);
        }

        private void UnhookHsvTextBox(TextBox textBox, TextChangedEventHandler handler)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.TextChanged -= handler;
            UnhookTextBoxFocus(textBox);
        }

        private void UnhookTextBox(TextBox textBox, TextChangedEventHandler handler)
        {
            if (textBox == null)
            {
                return;
            }

            textBox.TextChanged -= handler;
            UnhookTextBoxFocus(textBox);
        }

        private static void HookTextBoxFocus(TextBox textBox)
        {
            textBox.GotFocus += OnTextBoxGotFocus;
            textBox.LostFocus += OnTextBoxLostFocus;
        }

        private static void UnhookTextBoxFocus(TextBox textBox)
        {
            textBox.GotFocus -= OnTextBoxGotFocus;
            textBox.LostFocus -= OnTextBoxLostFocus;
        }

        private void InitializeTemplateStrings()
        {
            if (_rgbComboBoxItem != null)
            {
                _rgbComboBoxItem.Content = "RGB";
            }

            if (_hsvComboBoxItem != null)
            {
                _hsvComboBoxItem.Content = "HSV";
            }

            SetLabel(_redLabel, "Red");
            SetLabel(_greenLabel, "Green");
            SetLabel(_blueLabel, "Blue");
            SetLabel(_hueLabel, "Hue");
            SetLabel(_saturationLabel, "Saturation");
            SetLabel(_valueLabel, "Value");
            SetLabel(_alphaLabel, "Alpha");

            if (_colorRepresentationComboBox != null && _colorRepresentationComboBox.SelectedIndex < 0)
            {
                _colorRepresentationComboBox.SelectedIndex = 0;
            }

            UpdateMoreButton(false);
            UpdateHexTextBoxAutomationName();
        }

        private static void SetLabel(TextBlock textBlock, string text)
        {
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        private void InitializeColor()
        {
            SetCurrentColorState(Color);
            SetColorAndUpdateControls(ColorUpdateReason.InitializingColor);
        }

        private void SetCurrentColorState(Color color)
        {
            _currentRgb = RgbFromColor(color);
            _currentHsv = RgbToHsv(_currentRgb);
            _currentAlpha = color.A / 255.0;
            _currentHex = GetCurrentHexValue();
        }

        private void UpdateColor(Rgb rgb, ColorUpdateReason reason)
        {
            _currentRgb = rgb;
            _currentHsv = RgbToHsv(_currentRgb);
            _currentHex = GetCurrentHexValue();

            SetColorAndUpdateControls(reason);
        }

        private void UpdateColor(Hsv hsv, ColorUpdateReason reason)
        {
            _currentHsv = new Hsv(
                Clamp(hsv.H, MinHue, MaxHue),
                Clamp(hsv.S, MinSaturation / 100.0, MaxSaturation / 100.0),
                Clamp(hsv.V, MinValue / 100.0, MaxValue / 100.0));
            _currentRgb = HsvToRgb(_currentHsv);
            _currentHex = GetCurrentHexValue();

            SetColorAndUpdateControls(reason);
        }

        private void UpdateColor(double alpha, ColorUpdateReason reason)
        {
            _currentAlpha = Clamp01(alpha);
            _currentHex = GetCurrentHexValue();

            SetColorAndUpdateControls(reason);
        }

        private void SetColorAndUpdateControls(ColorUpdateReason reason)
        {
            Color newColor = ColorFromRgba(_currentRgb, _currentAlpha);

            _updatingColor = true;
            SetCurrentValue(ColorProperty, newColor);
            UpdateColorControls(reason);
            _updatingColor = false;
        }

        private void UpdatePreviousColorRectangle()
        {
            if (_previousColorRectangle != null)
            {
                _previousColorRectangle.Fill = PreviousColor.HasValue ? new SolidColorBrush(PreviousColor.Value) : null;
            }
        }

        private void UpdateColorControls(ColorUpdateReason reason)
        {
            _updatingControls = true;
            try
            {
                if (reason != ColorUpdateReason.ColorSpectrumColorChanged && _colorSpectrum != null)
                {
                    _colorSpectrum.MinHue = MinHue;
                    _colorSpectrum.MaxHue = MaxHue;
                    _colorSpectrum.MinSaturation = MinSaturation;
                    _colorSpectrum.MaxSaturation = MaxSaturation;
                    _colorSpectrum.MinValue = MinValue;
                    _colorSpectrum.MaxValue = MaxValue;
                    _colorSpectrum.Shape = ColorSpectrumShape;
                    _colorSpectrum.Components = ColorSpectrumComponents;
                    _colorSpectrum.HsvColor = new System.Numerics.Vector4((float)_currentHsv.H, (float)_currentHsv.S, (float)_currentHsv.V, (float)_currentAlpha);
                }

                if (_colorPreviewRectangle != null)
                {
                    _colorPreviewRectangle.Fill = new SolidColorBrush(Color);
                }

                if (reason != ColorUpdateReason.ThirdDimensionSliderChanged && _thirdDimensionSlider != null)
                {
                    UpdateThirdDimensionSlider();
                }

                if (reason != ColorUpdateReason.AlphaSliderChanged && _alphaSlider != null)
                {
                    UpdateAlphaSlider();
                }

                if (reason != ColorUpdateReason.RgbTextBoxChanged)
                {
                    SetText(_redTextBox, ToByteText(_currentRgb.R));
                    SetText(_greenTextBox, ToByteText(_currentRgb.G));
                    SetText(_blueTextBox, ToByteText(_currentRgb.B));
                }

                if (reason != ColorUpdateReason.HsvTextBoxChanged)
                {
                    SetText(_hueTextBox, Math.Round(_currentHsv.H).ToString("0", CultureInfo.InvariantCulture));
                    SetText(_saturationTextBox, Math.Round(_currentHsv.S * 100).ToString("0", CultureInfo.InvariantCulture));
                    SetText(_valueTextBox, Math.Round(_currentHsv.V * 100).ToString("0", CultureInfo.InvariantCulture));
                }

                if (reason != ColorUpdateReason.AlphaTextBoxChanged)
                {
                    SetText(_alphaTextBox, Math.Round(_currentAlpha * 100).ToString("0", CultureInfo.InvariantCulture) + "%");
                }

                if (reason != ColorUpdateReason.HexTextBoxChanged)
                {
                    SetText(_hexTextBox, _currentHex);
                }
            }
            finally
            {
                _updatingControls = false;
            }
        }

        private void OnColorSpectrumColorChanged(ColorSpectrum sender, ColorChangedEventArgs args)
        {
            if (_updatingControls)
            {
                return;
            }

            var hsvColor = sender.HsvColor;
            UpdateColor(new Hsv(hsvColor.X, hsvColor.Y, hsvColor.Z), ColorUpdateReason.ColorSpectrumColorChanged);
        }

        private void OnColorSpectrumSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_colorSpectrum != null && Math.Abs(e.NewSize.Width - e.PreviousSize.Width) > double.Epsilon)
            {
                _colorSpectrum.Height = e.NewSize.Width;
            }
        }

        private void OnColorPreviewRectangleGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateColorPreviewCheckeredBackground();
        }

        private void OnAlphaSliderBackgroundRectangleSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateAlphaSliderCheckeredBackground();
        }

        private void OnThirdDimensionSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingControls)
            {
                return;
            }

            double h = _currentHsv.H;
            double s = _currentHsv.S;
            double v = _currentHsv.V;

            switch (ColorSpectrumComponents)
            {
                case ColorSpectrumComponents.HueValue:
                case ColorSpectrumComponents.ValueHue:
                    s = _thirdDimensionSlider.Value / 100.0;
                    break;

                case ColorSpectrumComponents.HueSaturation:
                case ColorSpectrumComponents.SaturationHue:
                    v = _thirdDimensionSlider.Value / 100.0;
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                case ColorSpectrumComponents.SaturationValue:
                    h = _thirdDimensionSlider.Value;
                    break;
            }

            UpdateColor(new Hsv(h, s, v), ColorUpdateReason.ThirdDimensionSliderChanged);
        }

        private void OnAlphaSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_updatingControls)
            {
                return;
            }

            UpdateColor(_alphaSlider.Value / 100.0, ColorUpdateReason.AlphaSliderChanged);
        }

        private void OnMoreButtonClicked(object sender, RoutedEventArgs e)
        {
            _textEntryGridOpened = !_textEntryGridOpened;
            UpdateMoreButton(true);
        }

        private void OnMoreButtonChecked(object sender, RoutedEventArgs e)
        {
            _textEntryGridOpened = true;
            UpdateMoreButton(true);
        }

        private void OnMoreButtonUnchecked(object sender, RoutedEventArgs e)
        {
            _textEntryGridOpened = false;
            UpdateMoreButton(true);
        }

        private void UpdateMoreButton(bool useTransitions)
        {
            if (_moreButton != null)
            {
                AutomationProperties.SetName(_moreButton, _textEntryGridOpened ? "Hide color values" : "Show color values");

                if (_moreButton is WpfPrimitives.ToggleButton toggleButton && toggleButton.IsChecked != _textEntryGridOpened)
                {
                    toggleButton.IsChecked = _textEntryGridOpened;
                }
            }

            if (_moreButtonLabel != null)
            {
                _moreButtonLabel.Text = _textEntryGridOpened ? "Less" : "More";
            }

            UpdateVisualState(useTransitions);
        }

        private void OnColorRepresentationComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHexTextBoxAutomationName();
            UpdateVisualState(true);
        }

        private static void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.TemplatedParent is ColorPicker colorPicker)
            {
                colorPicker._isFocusedTextBoxValid = true;
                colorPicker._previousString = textBox.Text;
            }
        }

        private static void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.TemplatedParent is ColorPicker colorPicker)
            {
                if (!colorPicker._isFocusedTextBoxValid)
                {
                    textBox.Text = colorPicker._previousString;
                }

                colorPicker.UpdateColorControls(ColorUpdateReason.ColorPropertyChanged);
            }
        }

        private void OnRgbTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            if (!(sender is TextBox textBox) || !IsValidIntText(textBox.Text, 0, 255))
            {
                _isFocusedTextBoxValid = false;
                return;
            }

            _isFocusedTextBoxValid = true;
            UpdateColor(ApplyConstraintsToRgbColor(GetRgbColorFromTextBoxes()), ColorUpdateReason.RgbTextBoxChanged);
        }

        private void OnHueTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            if (!IsValidIntText(_hueTextBox?.Text, MinHue, MaxHue))
            {
                _isFocusedTextBoxValid = false;
                return;
            }

            _isFocusedTextBoxValid = true;
            UpdateColor(GetHsvColorFromTextBoxes(), ColorUpdateReason.HsvTextBoxChanged);
        }

        private void OnSaturationTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            if (!IsValidIntText(_saturationTextBox?.Text, MinSaturation, MaxSaturation))
            {
                _isFocusedTextBoxValid = false;
                return;
            }

            _isFocusedTextBoxValid = true;
            UpdateColor(GetHsvColorFromTextBoxes(), ColorUpdateReason.HsvTextBoxChanged);
        }

        private void OnValueTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls)
            {
                return;
            }

            if (!IsValidIntText(_valueTextBox?.Text, MinValue, MaxValue))
            {
                _isFocusedTextBoxValid = false;
                return;
            }

            _isFocusedTextBoxValid = true;
            UpdateColor(GetHsvColorFromTextBoxes(), ColorUpdateReason.HsvTextBoxChanged);
        }

        private void OnAlphaTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls || _alphaTextBox == null)
            {
                return;
            }

            string text = _alphaTextBox.Text ?? string.Empty;
            if (text.Length == 0 || text[text.Length - 1] != '%')
            {
                int cursorPosition = _alphaTextBox.SelectionStart + _alphaTextBox.SelectionLength;
                _alphaTextBox.Text = text + "%";
                _alphaTextBox.SelectionStart = Math.Min(cursorPosition, _alphaTextBox.Text.Length);
                return;
            }

            string alphaText = text.Substring(0, text.Length - 1);
            int? alphaValue = TryParseInt(alphaText);
            if (!alphaValue.HasValue || alphaValue.Value < 0 || alphaValue.Value > 100)
            {
                _isFocusedTextBoxValid = false;
                return;
            }

            _isFocusedTextBoxValid = true;
            UpdateColor(alphaValue.Value / 100.0, ColorUpdateReason.AlphaTextBoxChanged);
        }

        private void OnHexTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingControls || _hexTextBox == null)
            {
                return;
            }

            string text = _hexTextBox.Text ?? string.Empty;
            if (text.Length == 0 || text[0] != '#')
            {
                _hexTextBox.Text = "#" + text;
                _hexTextBox.SelectionStart = _hexTextBox.Text.Length;
                return;
            }

            Rgb rgbValue;
            double alphaValue;
            if (IsAlphaEnabled)
            {
                (rgbValue, alphaValue) = HexToRgba(_hexTextBox.Text);
            }
            else
            {
                rgbValue = HexToRgb(_hexTextBox.Text);
                alphaValue = 1.0;
            }

            if ((rgbValue.R == -1 && rgbValue.G == -1 && rgbValue.B == -1 && alphaValue == -1) ||
                alphaValue < 0 ||
                alphaValue > 1)
            {
                _isFocusedTextBoxValid = false;
                return;
            }

            _isFocusedTextBoxValid = true;
            _currentRgb = ApplyConstraintsToRgbColor(rgbValue);
            _currentHsv = RgbToHsv(_currentRgb);
            _currentAlpha = alphaValue;
            _currentHex = GetCurrentHexValue();
            SetColorAndUpdateControls(ColorUpdateReason.HexTextBoxChanged);
        }

        private Rgb GetRgbColorFromTextBoxes()
        {
            return new Rgb(
                ParseInt(_redTextBox?.Text) / 255.0,
                ParseInt(_greenTextBox?.Text) / 255.0,
                ParseInt(_blueTextBox?.Text) / 255.0);
        }

        private Hsv GetHsvColorFromTextBoxes()
        {
            return new Hsv(
                ParseInt(_hueTextBox?.Text),
                ParseInt(_saturationTextBox?.Text) / 100.0,
                ParseInt(_valueTextBox?.Text) / 100.0);
        }

        private string GetCurrentHexValue()
        {
            return IsAlphaEnabled ? RgbaToHex(_currentRgb, _currentAlpha) : RgbToHex(_currentRgb);
        }

        private Rgb ApplyConstraintsToRgbColor(Rgb rgb)
        {
            Hsv hsv = RgbToHsv(rgb);
            hsv.H = Clamp(hsv.H, MinHue, MaxHue);
            hsv.S = Clamp(hsv.S, MinSaturation / 100.0, MaxSaturation / 100.0);
            hsv.V = Clamp(hsv.V, MinValue / 100.0, MaxValue / 100.0);

            return HsvToRgb(hsv);
        }

        private void UpdateThirdDimensionSlider()
        {
            if (_thirdDimensionSlider == null || _thirdDimensionSliderGradientBrush == null)
            {
                return;
            }

            _thirdDimensionSliderGradientBrush.GradientStops.Clear();

            switch (ColorSpectrumComponents)
            {
                case ColorSpectrumComponents.HueValue:
                case ColorSpectrumComponents.ValueHue:
                    UpdateThirdDimensionSliderForSaturation();
                    break;

                case ColorSpectrumComponents.HueSaturation:
                case ColorSpectrumComponents.SaturationHue:
                    UpdateThirdDimensionSliderForValue();
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                case ColorSpectrumComponents.SaturationValue:
                    UpdateThirdDimensionSliderForHue();
                    break;
            }
        }

        private void UpdateThirdDimensionSliderForSaturation()
        {
            int min = MinSaturation;
            int max = MaxSaturation;
            _thirdDimensionSlider.Minimum = min;
            _thirdDimensionSlider.Maximum = max;
            _thirdDimensionSlider.Value = _currentHsv.S * 100;

            if (min >= max)
            {
                max = min;
            }

            AddGradientStop(_thirdDimensionSliderGradientBrush, 0.0, new Hsv(_currentHsv.H, min / 100.0, 1.0), 1.0);
            AddGradientStop(_thirdDimensionSliderGradientBrush, 1.0, new Hsv(_currentHsv.H, max / 100.0, 1.0), 1.0);
        }

        private void UpdateThirdDimensionSliderForValue()
        {
            int min = MinValue;
            int max = MaxValue;
            _thirdDimensionSlider.Minimum = min;
            _thirdDimensionSlider.Maximum = max;
            _thirdDimensionSlider.Value = _currentHsv.V * 100;

            if (min >= max)
            {
                max = min;
            }

            AddGradientStop(_thirdDimensionSliderGradientBrush, 0.0, new Hsv(_currentHsv.H, _currentHsv.S, min / 100.0), 1.0);
            AddGradientStop(_thirdDimensionSliderGradientBrush, 1.0, new Hsv(_currentHsv.H, _currentHsv.S, max / 100.0), 1.0);
        }

        private void UpdateThirdDimensionSliderForHue()
        {
            int min = MinHue;
            int max = MaxHue;
            _thirdDimensionSlider.Minimum = min;
            _thirdDimensionSlider.Maximum = max;
            _thirdDimensionSlider.Value = _currentHsv.H;

            if (min >= max)
            {
                max = min;
            }

            double minOffset = min / 359.0;
            double maxOffset = max / 359.0;

            AddGradientStop(_thirdDimensionSliderGradientBrush, 0.0, new Hsv(min, 1.0, 1.0), 1.0);

            for (int sextant = 1; sextant <= 5; sextant++)
            {
                double offset = sextant / 6.0;
                if (minOffset < offset && maxOffset > offset)
                {
                    AddGradientStop(_thirdDimensionSliderGradientBrush, (offset - minOffset) / (maxOffset - minOffset), new Hsv(60.0 * sextant, 1.0, 1.0), 1.0);
                }
            }

            AddGradientStop(_thirdDimensionSliderGradientBrush, 1.0, new Hsv(max, 1.0, 1.0), 1.0);
        }

        private void SetThirdDimensionSliderChannel()
        {
            if (_thirdDimensionSlider == null)
            {
                return;
            }

            switch (ColorSpectrumComponents)
            {
                case ColorSpectrumComponents.ValueSaturation:
                case ColorSpectrumComponents.SaturationValue:
                    _thirdDimensionSlider.ColorChannel = ColorPickerHsvChannel.Hue;
                    AutomationProperties.SetName(_thirdDimensionSlider, "Hue");
                    break;

                case ColorSpectrumComponents.HueValue:
                case ColorSpectrumComponents.ValueHue:
                    _thirdDimensionSlider.ColorChannel = ColorPickerHsvChannel.Saturation;
                    AutomationProperties.SetName(_thirdDimensionSlider, "Saturation");
                    break;

                case ColorSpectrumComponents.HueSaturation:
                case ColorSpectrumComponents.SaturationHue:
                    _thirdDimensionSlider.ColorChannel = ColorPickerHsvChannel.Value;
                    AutomationProperties.SetName(_thirdDimensionSlider, "Value");
                    break;
            }
        }

        private void UpdateAlphaSlider()
        {
            if (_alphaSlider == null || _alphaSliderGradientBrush == null)
            {
                return;
            }

            _alphaSliderGradientBrush.GradientStops.Clear();
            _alphaSlider.Minimum = 0;
            _alphaSlider.Maximum = 100;
            _alphaSlider.Value = _currentAlpha * 100;

            AddGradientStop(_alphaSliderGradientBrush, 0.0, _currentHsv, 0.0);
            AddGradientStop(_alphaSliderGradientBrush, 1.0, _currentHsv, 1.0);
        }

        private void CreateColorPreviewCheckeredBackground()
        {
            if (_colorPreviewCheckeredBackgroundRectangle != null)
            {
                _colorPreviewCheckeredBackgroundRectangle.Fill = CreateCheckeredBackground();
            }
        }

        private void CreateAlphaSliderCheckeredBackground()
        {
            if (_alphaSliderCheckeredBackgroundRectangle != null)
            {
                _alphaSliderCheckeredBackgroundRectangle.Fill = CreateCheckeredBackground();
            }
        }

        private static Brush CreateCheckeredBackground()
        {
            var brush = new DrawingBrush
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 8, 8),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 8, 8),
                ViewboxUnits = BrushMappingMode.Absolute
            };

            var group = new DrawingGroup();
            group.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(230, 230, 230)), null, new RectangleGeometry(new Rect(0, 0, 8, 8))));
            group.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(180, 180, 180)), null, new RectangleGeometry(new Rect(0, 0, 4, 4))));
            group.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(180, 180, 180)), null, new RectangleGeometry(new Rect(4, 4, 4, 4))));
            brush.Drawing = group;
            brush.Freeze();
            return brush;
        }

        private static void AddGradientStop(LinearGradientBrush brush, double offset, Hsv hsvColor, double alpha)
        {
            if (brush == null)
            {
                return;
            }

            Rgb rgbColor = HsvToRgb(hsvColor);
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(ToByte(alpha), ToByte(rgbColor.R), ToByte(rgbColor.G), ToByte(rgbColor.B)), offset));
        }

        private void UpdateVisualState(bool useTransitions)
        {
            bool isAlphaEnabled = IsAlphaEnabled;
            bool isColorSpectrumVisible = IsColorSpectrumVisible;
            bool isVerticalOrientation = Orientation == Orientation.Vertical;

            string previousColorStateName = isColorSpectrumVisible
                ? PreviousColor.HasValue ? "PreviousColorVisibleVertical" : "PreviousColorCollapsedVertical"
                : PreviousColor.HasValue ? "PreviousColorVisibleHorizontal" : "PreviousColorCollapsedHorizontal";

            VisualStateManager.GoToState(this, isColorSpectrumVisible ? "ColorSpectrumVisible" : "ColorSpectrumCollapsed", useTransitions);
            VisualStateManager.GoToState(this, previousColorStateName, useTransitions);
            VisualStateManager.GoToState(this, IsColorPreviewVisible ? "ColorPreviewVisible" : "ColorPreviewCollapsed", useTransitions);
            VisualStateManager.GoToState(this, IsColorSliderVisible ? "ThirdDimensionSliderVisible" : "ThirdDimensionSliderCollapsed", useTransitions);
            VisualStateManager.GoToState(this, isAlphaEnabled && IsAlphaSliderVisible ? "AlphaSliderVisible" : "AlphaSliderCollapsed", useTransitions);
            VisualStateManager.GoToState(this, IsMoreButtonVisible && isVerticalOrientation ? "MoreButtonVisible" : "MoreButtonCollapsed", useTransitions);
            VisualStateManager.GoToState(this, !IsMoreButtonVisible || _textEntryGridOpened || !isVerticalOrientation ? "TextEntryGridVisible" : "TextEntryGridCollapsed", useTransitions);
            VisualStateManager.GoToState(this, _colorRepresentationComboBox?.SelectedIndex == 1 ? "HsvSelected" : "RgbSelected", useTransitions);
            VisualStateManager.GoToState(this, IsColorChannelTextInputVisible ? "ColorChannelTextInputVisible" : "ColorChannelTextInputCollapsed", useTransitions);
            VisualStateManager.GoToState(this, isAlphaEnabled && IsAlphaTextInputVisible ? "AlphaTextInputVisible" : "AlphaTextInputCollapsed", useTransitions);
            VisualStateManager.GoToState(this, IsHexInputVisible ? "HexInputVisible" : "HexInputCollapsed", useTransitions);
            VisualStateManager.GoToState(this, isAlphaEnabled ? "AlphaEnabled" : "AlphaDisabled", useTransitions);
            VisualStateManager.GoToState(this, isVerticalOrientation ? "Vertical" : "Horizontal", useTransitions);
        }

        private void UpdateHexTextBoxAutomationName()
        {
            if (_hexTextBox != null)
            {
                AutomationProperties.SetName(_hexTextBox, _colorRepresentationComboBox?.SelectedIndex == 1 ? "HSV hex value" : "RGB hex value");
            }
        }

        private static void SetText(TextBox textBox, string text)
        {
            if (textBox != null && textBox.Text != text)
            {
                textBox.Text = text;
            }
        }

        private static string ToByteText(double value)
        {
            return Math.Round(Clamp01(value) * 255).ToString("0", CultureInfo.InvariantCulture);
        }

        private static bool IsValidIntText(string text, int min, int max)
        {
            int? value = TryParseInt(text);
            return value.HasValue && value.Value >= min && value.Value <= max;
        }

        private static int ParseInt(string text)
        {
            return TryParseInt(text) ?? 0;
        }

        private enum ColorUpdateReason
        {
            InitializingColor,
            ColorPropertyChanged,
            ColorSpectrumColorChanged,
            ThirdDimensionSliderChanged,
            AlphaSliderChanged,
            RgbTextBoxChanged,
            HsvTextBoxChanged,
            AlphaTextBoxChanged,
            HexTextBoxChanged
        }

        private bool _updatingColor;
        private bool _updatingControls;
        private Rgb _currentRgb = new Rgb(1.0, 1.0, 1.0);
        private Hsv _currentHsv = new Hsv(0.0, 1.0, 1.0);
        private string _currentHex = "#FFFFFFFF";
        private double _currentAlpha = 1.0;
        private string _previousString = string.Empty;
        private bool _isFocusedTextBoxValid;
        private bool _textEntryGridOpened;

        private ColorSpectrum _colorSpectrum;
        private Grid _colorPreviewRectangleGrid;
        private Rectangle _colorPreviewRectangle;
        private Rectangle _previousColorRectangle;
        private Rectangle _colorPreviewCheckeredBackgroundRectangle;
        private ColorPickerSlider _thirdDimensionSlider;
        private LinearGradientBrush _thirdDimensionSliderGradientBrush;
        private ColorPickerSlider _alphaSlider;
        private LinearGradientBrush _alphaSliderGradientBrush;
        private Rectangle _alphaSliderBackgroundRectangle;
        private Rectangle _alphaSliderCheckeredBackgroundRectangle;
        private WpfPrimitives.ButtonBase _moreButton;
        private TextBlock _moreButtonLabel;
        private ComboBox _colorRepresentationComboBox;
        private ComboBoxItem _rgbComboBoxItem;
        private ComboBoxItem _hsvComboBoxItem;
        private TextBox _redTextBox;
        private TextBox _greenTextBox;
        private TextBox _blueTextBox;
        private TextBox _hueTextBox;
        private TextBox _saturationTextBox;
        private TextBox _valueTextBox;
        private TextBox _alphaTextBox;
        private TextBox _hexTextBox;
        private TextBlock _redLabel;
        private TextBlock _greenLabel;
        private TextBlock _blueLabel;
        private TextBlock _hueLabel;
        private TextBlock _saturationLabel;
        private TextBlock _valueLabel;
        private TextBlock _alphaLabel;
    }
}
