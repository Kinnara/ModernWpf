using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ModernWpf.Controls;
using static ModernWpf.Controls.ColorConversion;

namespace ModernWpf.Controls.Primitives
{
    public partial class ColorSpectrum : Control
    {
        private const string LayoutRootName = "LayoutRoot";
        private const string SizingGridName = "SizingGrid";
        private const string SpectrumRectangleName = "SpectrumRectangle";
        private const string SpectrumOverlayRectangleName = "SpectrumOverlayRectangle";
        private const string SpectrumEllipseName = "SpectrumEllipse";
        private const string SpectrumOverlayEllipseName = "SpectrumOverlayEllipse";
        private const string InputTargetName = "InputTarget";
        private const string SelectionEllipsePanelName = "SelectionEllipsePanel";
        private const string ColorNameToolTipName = "ColorNameToolTip";

        static ColorSpectrum()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSpectrum), new FrameworkPropertyMetadata(typeof(ColorSpectrum)));
        }

        public ColorSpectrum()
        {
            Focusable = true;
            Unloaded += OnUnloaded;
        }

        public event TypedEventHandler<ColorSpectrum, ColorChangedEventArgs> ColorChanged;

        public override void OnApplyTemplate()
        {
            UnhookTemplateEvents();

            base.OnApplyTemplate();

            _layoutRoot = GetTemplateChild(LayoutRootName) as FrameworkElement;
            _sizingGrid = GetTemplateChild(SizingGridName) as FrameworkElement;
            _spectrumRectangle = GetTemplateChild(SpectrumRectangleName) as Rectangle;
            _spectrumOverlayRectangle = GetTemplateChild(SpectrumOverlayRectangleName) as Rectangle;
            _spectrumEllipse = GetTemplateChild(SpectrumEllipseName) as Ellipse;
            _spectrumOverlayEllipse = GetTemplateChild(SpectrumOverlayEllipseName) as Ellipse;
            _inputTarget = GetTemplateChild(InputTargetName) as FrameworkElement;
            _selectionEllipsePanel = GetTemplateChild(SelectionEllipsePanelName) as FrameworkElement;
            _colorNameToolTip = GetTemplateChild(ColorNameToolTipName) as ToolTip;

            HookTemplateEvents();

            if (_hsvValues.Count == 0)
            {
                CreateBitmapsAndColorMap();
            }

            UpdateEllipse();
            UpdateVisualState(false);
        }

        internal void SetColorFromPointForTesting(Point point)
        {
            if (_hsvValues.Count == 0)
            {
                CreateBitmapsAndColorMap();
            }

            UpdateColorFromPoint(point);
        }

        internal void AdjustColorForTesting(int horizontalSteps, int verticalSteps)
        {
            if (horizontalSteps < 0)
            {
                AdjustColorFromKey(Key.Left, false);
            }
            else if (horizontalSteps > 0)
            {
                AdjustColorFromKey(Key.Right, false);
            }

            if (verticalSteps < 0)
            {
                AdjustColorFromKey(Key.Down, false);
            }
            else if (verticalSteps > 0)
            {
                AdjustColorFromKey(Key.Up, false);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ColorSpectrumAutomationPeer(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key != Key.Left && e.Key != Key.Right && e.Key != Key.Up && e.Key != Key.Down)
            {
                base.OnKeyDown(e);
                return;
            }

            AdjustColorFromKey(e.Key, (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
            e.Handled = true;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (_colorNameToolTip != null)
            {
                _colorNameToolTip.IsOpen = true;
            }
            UpdateVisualState(true);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            if (_colorNameToolTip != null)
            {
                _colorNameToolTip.IsOpen = false;
            }
            UpdateVisualState(true);
        }

        private static void OnColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSpectrum = (ColorSpectrum)d;

            if (!colorSpectrum._updatingColor)
            {
                Color color = (Color)e.NewValue;
                colorSpectrum._updatingHsvColor = true;
                colorSpectrum.SetCurrentValue(HsvColorProperty, RgbToHsv(color));
                colorSpectrum._updatingHsvColor = false;

                colorSpectrum.UpdateEllipse();
                colorSpectrum.UpdateBitmapSources();
                colorSpectrum.UpdateVisualState(true);
            }

            colorSpectrum._oldColor = (Color)e.OldValue;

            if (!colorSpectrum._updatingColor)
            {
                colorSpectrum.RaiseColorChanged();
            }
        }

        private static void OnHsvColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorSpectrum = (ColorSpectrum)d;

            if (!colorSpectrum._updatingHsvColor)
            {
                colorSpectrum.SetColor();
            }

            colorSpectrum._oldHsvColor = (Vector4)e.OldValue;
        }

        private static void OnHueRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidateHue((int)e.NewValue, e.Property.Name);
            var colorSpectrum = (ColorSpectrum)d;

            if (colorSpectrum.Components != ColorSpectrumComponents.SaturationValue &&
                colorSpectrum.Components != ColorSpectrumComponents.ValueSaturation)
            {
                colorSpectrum.CreateBitmapsAndColorMap();
            }
            else
            {
                colorSpectrum.UpdateEllipse();
            }
        }

        private static void OnSaturationRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidatePercentage((int)e.NewValue, e.Property.Name);
            var colorSpectrum = (ColorSpectrum)d;

            if (colorSpectrum.Components != ColorSpectrumComponents.HueValue &&
                colorSpectrum.Components != ColorSpectrumComponents.ValueHue)
            {
                colorSpectrum.CreateBitmapsAndColorMap();
            }
            else
            {
                colorSpectrum.UpdateEllipse();
            }
        }

        private static void OnValueRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValidatePercentage((int)e.NewValue, e.Property.Name);
            var colorSpectrum = (ColorSpectrum)d;

            if (colorSpectrum.Components != ColorSpectrumComponents.HueSaturation &&
                colorSpectrum.Components != ColorSpectrumComponents.SaturationHue)
            {
                colorSpectrum.CreateBitmapsAndColorMap();
            }
            else
            {
                colorSpectrum.UpdateEllipse();
            }
        }

        private static void OnShapePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorSpectrum)d).CreateBitmapsAndColorMap();
        }

        private static void OnComponentsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorSpectrum)d).CreateBitmapsAndColorMap();
        }

        private void SetColor()
        {
            Vector4 hsvColor = HsvColor;
            _updatingColor = true;
            SetCurrentValue(ColorProperty, ColorFromRgba(new Hsv(hsvColor.X, hsvColor.Y, hsvColor.Z), hsvColor.W));
            _updatingColor = false;

            UpdateEllipse();
            UpdateBitmapSources();
            RaiseColorChanged();
        }

        private void RaiseColorChanged()
        {
            Color newColor = Color;
            bool colorChanged = _oldColor.A != newColor.A ||
                                _oldColor.R != newColor.R ||
                                _oldColor.G != newColor.G ||
                                _oldColor.B != newColor.B;
            bool areBothColorsBlack =
                (_oldColor.R == newColor.R && newColor.R == 0) ||
                (_oldColor.G == newColor.G && newColor.G == 0) ||
                (_oldColor.B == newColor.B && newColor.B == 0);

            if (colorChanged || areBothColorsBlack)
            {
                ColorChanged?.Invoke(this, new ColorChangedEventArgs(_oldColor, newColor));

                if (_colorNameToolTip != null)
                {
                    _colorNameToolTip.Content = newColor.ToString();
                }

                if (FrameworkElementAutomationPeer.FromElement(this) is ColorSpectrumAutomationPeer peer)
                {
                    peer.RaiseValueChanged(_oldColor, newColor, _oldHsvColor, HsvColor);
                }
            }
        }

        private void AdjustColorFromKey(Key key, bool isControlDown)
        {
            ColorPickerHsvChannel incrementChannel = ColorPickerHsvChannel.Hue;
            bool isSaturationValue = false;

            if (key == Key.Left || key == Key.Right)
            {
                switch (Components)
                {
                    case ColorSpectrumComponents.HueSaturation:
                    case ColorSpectrumComponents.HueValue:
                        incrementChannel = ColorPickerHsvChannel.Hue;
                        break;

                    case ColorSpectrumComponents.SaturationValue:
                        isSaturationValue = true;
                        incrementChannel = ColorPickerHsvChannel.Saturation;
                        break;

                    case ColorSpectrumComponents.SaturationHue:
                        incrementChannel = ColorPickerHsvChannel.Saturation;
                        break;

                    case ColorSpectrumComponents.ValueHue:
                    case ColorSpectrumComponents.ValueSaturation:
                        incrementChannel = ColorPickerHsvChannel.Value;
                        break;
                }
            }
            else if (key == Key.Up || key == Key.Down)
            {
                switch (Components)
                {
                    case ColorSpectrumComponents.SaturationHue:
                    case ColorSpectrumComponents.ValueHue:
                        incrementChannel = ColorPickerHsvChannel.Hue;
                        break;

                    case ColorSpectrumComponents.HueSaturation:
                    case ColorSpectrumComponents.ValueSaturation:
                        incrementChannel = ColorPickerHsvChannel.Saturation;
                        break;

                    case ColorSpectrumComponents.SaturationValue:
                        isSaturationValue = true;
                        incrementChannel = ColorPickerHsvChannel.Value;
                        break;

                    case ColorSpectrumComponents.HueValue:
                        incrementChannel = ColorPickerHsvChannel.Value;
                        break;
                }
            }

            double minBound = 0;
            double maxBound = 0;

            switch (incrementChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    minBound = MinHue;
                    maxBound = MaxHue;
                    break;
                case ColorPickerHsvChannel.Saturation:
                    minBound = MinSaturation;
                    maxBound = MaxSaturation;
                    break;
                case ColorPickerHsvChannel.Value:
                    minBound = MinValue;
                    maxBound = MaxValue;
                    break;
            }

            IncrementDirection direction =
                (incrementChannel == ColorPickerHsvChannel.Hue && (key == Key.Left || key == Key.Up)) ||
                (incrementChannel != ColorPickerHsvChannel.Hue && (key == Key.Right || key == Key.Down))
                    ? IncrementDirection.Lower
                    : IncrementDirection.Higher;

            if ((FlowDirection == FlowDirection.RightToLeft) != isSaturationValue &&
                (key == Key.Left || key == Key.Right))
            {
                direction = direction == IncrementDirection.Higher ? IncrementDirection.Lower : IncrementDirection.Higher;
            }

            IncrementAmount amount = isControlDown ? IncrementAmount.Large : IncrementAmount.Small;
            Vector4 hsvColor = HsvColor;
            UpdateColor(IncrementColorChannel(
                new Hsv(hsvColor.X, hsvColor.Y, hsvColor.Z),
                incrementChannel,
                direction,
                amount,
                true,
                minBound,
                maxBound));
        }

        private void UpdateColor(Hsv newHsv)
        {
            _updatingColor = true;
            _updatingHsvColor = true;

            float alpha = HsvColor.W;
            SetCurrentValue(ColorProperty, ColorFromRgba(newHsv, alpha));
            SetCurrentValue(HsvColorProperty, new Vector4((float)newHsv.H, (float)newHsv.S, (float)newHsv.V, alpha));

            UpdateEllipse();
            UpdateVisualState(true);

            _updatingHsvColor = false;
            _updatingColor = false;

            RaiseColorChanged();
        }

        private void UpdateColorFromPoint(Point point)
        {
            if (_hsvValues.Count == 0)
            {
                return;
            }

            double xPosition = point.X;
            double yPosition = point.Y;
            double radius = Math.Min(_imageWidthFromLastBitmapCreation, _imageHeightFromLastBitmapCreation) / 2;
            double distanceFromRadius = Math.Sqrt(Math.Pow(xPosition - radius, 2) + Math.Pow(yPosition - radius, 2));

            if (distanceFromRadius > radius && Shape == ColorSpectrumShape.Ring)
            {
                xPosition = (radius / distanceFromRadius) * (xPosition - radius) + radius;
                yPosition = (radius / distanceFromRadius) * (yPosition - radius) + radius;
            }

            int x = (int)Math.Round(xPosition);
            int y = (int)Math.Round(yPosition);
            int width = (int)Math.Round(_imageWidthFromLastBitmapCreation);

            x = (int)Clamp(x, 0, _imageWidthFromLastBitmapCreation - 1);
            y = (int)Clamp(y, 0, _imageHeightFromLastBitmapCreation - 1);

            Hsv hsvAtPoint = _hsvValues[y * width + x];
            Vector4 hsvColor = HsvColor;

            switch (Components)
            {
                case ColorSpectrumComponents.HueValue:
                case ColorSpectrumComponents.ValueHue:
                    hsvAtPoint.S = hsvColor.Y;
                    break;

                case ColorSpectrumComponents.HueSaturation:
                case ColorSpectrumComponents.SaturationHue:
                    hsvAtPoint.V = hsvColor.Z;
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                case ColorSpectrumComponents.SaturationValue:
                    hsvAtPoint.H = hsvColor.X;
                    break;
            }

            UpdateColor(hsvAtPoint);
        }

        private void UpdateEllipse()
        {
            if (_selectionEllipsePanel == null)
            {
                return;
            }

            if (_imageWidthFromLastBitmapCreation == 0 || _imageHeightFromLastBitmapCreation == 0)
            {
                _selectionEllipsePanel.Visibility = Visibility.Collapsed;
                return;
            }

            _selectionEllipsePanel.Visibility = Visibility.Visible;

            double xPosition;
            double yPosition;
            Vector4 hsvColor = HsvColor;
            double hue = Clamp(hsvColor.X, _minHueFromLastBitmapCreation, _maxHueFromLastBitmapCreation);
            double saturation = Clamp(hsvColor.Y, _minSaturationFromLastBitmapCreation / 100.0, _maxSaturationFromLastBitmapCreation / 100.0);
            double value = Clamp(hsvColor.Z, _minValueFromLastBitmapCreation / 100.0, _maxValueFromLastBitmapCreation / 100.0);

            if (_shapeFromLastBitmapCreation == ColorSpectrumShape.Box)
            {
                double hPercent = Percent(hue, _minHueFromLastBitmapCreation, _maxHueFromLastBitmapCreation);
                double sPercent = Percent(saturation * 100.0, _minSaturationFromLastBitmapCreation, _maxSaturationFromLastBitmapCreation);
                double vPercent = Percent(value * 100.0, _minValueFromLastBitmapCreation, _maxValueFromLastBitmapCreation);

                if (_componentsFromLastBitmapCreation == ColorSpectrumComponents.HueSaturation ||
                    _componentsFromLastBitmapCreation == ColorSpectrumComponents.SaturationHue)
                {
                    sPercent = 1 - sPercent;
                }
                else
                {
                    vPercent = 1 - vPercent;
                }

                double xPercent = 0;
                double yPercent = 0;

                switch (_componentsFromLastBitmapCreation)
                {
                    case ColorSpectrumComponents.HueValue:
                        xPercent = hPercent;
                        yPercent = vPercent;
                        break;
                    case ColorSpectrumComponents.HueSaturation:
                        xPercent = hPercent;
                        yPercent = sPercent;
                        break;
                    case ColorSpectrumComponents.ValueHue:
                        xPercent = vPercent;
                        yPercent = hPercent;
                        break;
                    case ColorSpectrumComponents.ValueSaturation:
                        xPercent = vPercent;
                        yPercent = sPercent;
                        break;
                    case ColorSpectrumComponents.SaturationHue:
                        xPercent = sPercent;
                        yPercent = hPercent;
                        break;
                    case ColorSpectrumComponents.SaturationValue:
                        xPercent = sPercent;
                        yPercent = vPercent;
                        break;
                }

                xPosition = _imageWidthFromLastBitmapCreation * xPercent;
                yPosition = _imageHeightFromLastBitmapCreation * yPercent;
            }
            else
            {
                double hThetaValue = Percent(hue, _minHueFromLastBitmapCreation, _maxHueFromLastBitmapCreation) * 360;
                double sThetaValue = Percent(saturation * 100.0, _minSaturationFromLastBitmapCreation, _maxSaturationFromLastBitmapCreation) * 360;
                double vThetaValue = Percent(value * 100.0, _minValueFromLastBitmapCreation, _maxValueFromLastBitmapCreation) * 360;
                double hRValue = Percent(hue, _minHueFromLastBitmapCreation, _maxHueFromLastBitmapCreation) - 1;
                double sRValue = Percent(saturation * 100.0, _minSaturationFromLastBitmapCreation, _maxSaturationFromLastBitmapCreation) - 1;
                double vRValue = Percent(value * 100.0, _minValueFromLastBitmapCreation, _maxValueFromLastBitmapCreation) - 1;

                if (_componentsFromLastBitmapCreation == ColorSpectrumComponents.HueSaturation ||
                    _componentsFromLastBitmapCreation == ColorSpectrumComponents.SaturationHue)
                {
                    sThetaValue = 360 - sThetaValue;
                    sRValue = -sRValue - 1;
                }
                else
                {
                    vThetaValue = 360 - vThetaValue;
                    vRValue = -vRValue - 1;
                }

                double thetaValue = 0;
                double rValue = 0;

                switch (_componentsFromLastBitmapCreation)
                {
                    case ColorSpectrumComponents.HueValue:
                        thetaValue = hThetaValue;
                        rValue = vRValue;
                        break;
                    case ColorSpectrumComponents.HueSaturation:
                        thetaValue = hThetaValue;
                        rValue = sRValue;
                        break;
                    case ColorSpectrumComponents.ValueHue:
                        thetaValue = vThetaValue;
                        rValue = hRValue;
                        break;
                    case ColorSpectrumComponents.ValueSaturation:
                        thetaValue = vThetaValue;
                        rValue = sRValue;
                        break;
                    case ColorSpectrumComponents.SaturationHue:
                        thetaValue = sThetaValue;
                        rValue = hRValue;
                        break;
                    case ColorSpectrumComponents.SaturationValue:
                        thetaValue = sThetaValue;
                        rValue = vRValue;
                        break;
                }

                double radius = Math.Min(_imageWidthFromLastBitmapCreation, _imageHeightFromLastBitmapCreation) / 2;
                xPosition = Math.Cos((thetaValue * Math.PI / 180) + Math.PI) * radius * rValue + radius;
                yPosition = Math.Sin((thetaValue * Math.PI / 180) + Math.PI) * radius * rValue + radius;
            }

            Canvas.SetLeft(_selectionEllipsePanel, xPosition - _selectionEllipsePanel.Width / 2);
            Canvas.SetTop(_selectionEllipsePanel, yPosition - _selectionEllipsePanel.Height / 2);
            UpdateVisualState(true);
        }

        private void CreateBitmapsAndColorMap()
        {
            double minDimension = GetSpectrumDimension();
            if (minDimension <= 0)
            {
                return;
            }

            int size = Math.Max(1, (int)Math.Round(minDimension));
            if (_sizingGrid != null)
            {
                _sizingGrid.Width = size;
                _sizingGrid.Height = size;
            }

            var minPixels = new byte[size * size * 4];
            var maxPixels = new byte[size * size * 4];
            var middle1Pixels = new byte[size * size * 4];
            var middle2Pixels = new byte[size * size * 4];
            var middle3Pixels = new byte[size * size * 4];
            var middle4Pixels = new byte[size * size * 4];
            var hsvValues = new List<Hsv>(size * size);

            var baseHsv = new Hsv(HsvColor.X, HsvColor.Y, HsvColor.Z);
            if (Shape == ColorSpectrumShape.Box)
            {
                // WinUI writes box pixels with x as the outer loop and y as the
                // inner loop, both descending.  The resulting buffer is still
                // row-major: the source y coordinate becomes the on-screen x
                // axis and the source x coordinate becomes the on-screen y
                // axis.  Preserve that ordering so Components.HueSaturation,
                // for example, renders hue horizontally and saturation
                // vertically just like WinUI.
                int pixelIndex = 0;
                for (int x = size - 1; x >= 0; x--)
                {
                    for (int y = size - 1; y >= 0; y--)
                    {
                        int offset = pixelIndex++ * 4;
                        FillPixelForBox(x, y, baseHsv, size, Components, MinHue, MaxHue, MinSaturation, MaxSaturation, MinValue, MaxValue, minPixels, middle1Pixels, middle2Pixels, middle3Pixels, middle4Pixels, maxPixels, hsvValues, offset);
                    }
                }
            }
            else
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int offset = (y * size + x) * 4;
                        FillPixelForRing(x, y, size / 2.0, baseHsv, Components, MinHue, MaxHue, MinSaturation, MaxSaturation, MinValue, MaxValue, minPixels, middle1Pixels, middle2Pixels, middle3Pixels, middle4Pixels, maxPixels, hsvValues, offset);
                    }
                }
            }

            _saturationMinimumBitmap = CreateBitmap(size, minPixels);
            _saturationMaximumBitmap = CreateBitmap(size, maxPixels);
            _valueBitmap = CreateBitmap(size, maxPixels);
            _hueRedBitmap = CreateBitmap(size, minPixels);
            _hueYellowBitmap = CreateBitmap(size, middle1Pixels);
            _hueGreenBitmap = CreateBitmap(size, middle2Pixels);
            _hueCyanBitmap = CreateBitmap(size, middle3Pixels);
            _hueBlueBitmap = CreateBitmap(size, middle4Pixels);
            _huePurpleBitmap = CreateBitmap(size, maxPixels);

            _shapeFromLastBitmapCreation = Shape;
            _componentsFromLastBitmapCreation = Components;
            _imageWidthFromLastBitmapCreation = size;
            _imageHeightFromLastBitmapCreation = size;
            _minHueFromLastBitmapCreation = MinHue;
            _maxHueFromLastBitmapCreation = MaxHue;
            _minSaturationFromLastBitmapCreation = MinSaturation;
            _maxSaturationFromLastBitmapCreation = MaxSaturation;
            _minValueFromLastBitmapCreation = MinValue;
            _maxValueFromLastBitmapCreation = MaxValue;
            _hsvValues = hsvValues;

            UpdateBitmapSources();
            UpdateEllipse();
        }

        private void UpdateBitmapSources()
        {
            if (_spectrumRectangle == null || _spectrumEllipse == null || _spectrumOverlayRectangle == null || _spectrumOverlayEllipse == null)
            {
                return;
            }

            Vector4 hsvColor = HsvColor;
            ImageSource baseImage = null;
            ImageSource overlayImage = null;
            double overlayOpacity = 0;

            switch (Components)
            {
                case ColorSpectrumComponents.HueValue:
                case ColorSpectrumComponents.ValueHue:
                    baseImage = _saturationMinimumBitmap;
                    overlayImage = _saturationMaximumBitmap;
                    overlayOpacity = hsvColor.Y;
                    break;

                case ColorSpectrumComponents.HueSaturation:
                case ColorSpectrumComponents.SaturationHue:
                    baseImage = _valueBitmap;
                    overlayImage = _valueBitmap;
                    overlayOpacity = 0;
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                case ColorSpectrumComponents.SaturationValue:
                    double sextant = hsvColor.X / 60.0;
                    if (sextant < 1)
                    {
                        baseImage = _hueRedBitmap;
                        overlayImage = _hueYellowBitmap;
                    }
                    else if (sextant < 2)
                    {
                        baseImage = _hueYellowBitmap;
                        overlayImage = _hueGreenBitmap;
                    }
                    else if (sextant < 3)
                    {
                        baseImage = _hueGreenBitmap;
                        overlayImage = _hueCyanBitmap;
                    }
                    else if (sextant < 4)
                    {
                        baseImage = _hueCyanBitmap;
                        overlayImage = _hueBlueBitmap;
                    }
                    else if (sextant < 5)
                    {
                        baseImage = _hueBlueBitmap;
                        overlayImage = _huePurpleBitmap;
                    }
                    else
                    {
                        baseImage = _huePurpleBitmap;
                        overlayImage = _hueRedBitmap;
                    }

                    overlayOpacity = sextant - Math.Floor(sextant);
                    break;
            }

            _spectrumRectangle.Fill = CreateImageBrush(baseImage);
            _spectrumEllipse.Fill = CreateImageBrush(baseImage);
            _spectrumOverlayRectangle.Fill = CreateImageBrush(overlayImage);
            _spectrumOverlayEllipse.Fill = CreateImageBrush(overlayImage);
            _spectrumOverlayRectangle.Opacity = overlayOpacity;
            _spectrumOverlayEllipse.Opacity = overlayOpacity;
        }

        private static void FillPixelForBox(
            double x,
            double y,
            Hsv baseHsv,
            double minDimension,
            ColorSpectrumComponents components,
            double minHue,
            double maxHue,
            double minSaturation,
            double maxSaturation,
            double minValue,
            double maxValue,
            byte[] minPixels,
            byte[] middle1Pixels,
            byte[] middle2Pixels,
            byte[] middle3Pixels,
            byte[] middle4Pixels,
            byte[] maxPixels,
            List<Hsv> hsvValues,
            int offset)
        {
            double xPercent = minDimension == 1 ? 0 : (minDimension - 1 - x) / (minDimension - 1);
            double yPercent = minDimension == 1 ? 0 : (minDimension - 1 - y) / (minDimension - 1);
            FillPixel(xPercent, yPercent, baseHsv, components, minHue, maxHue, minSaturation, maxSaturation, minValue, maxValue, minPixels, middle1Pixels, middle2Pixels, middle3Pixels, middle4Pixels, maxPixels, hsvValues, offset);
        }

        private static void FillPixelForRing(
            double x,
            double y,
            double radius,
            Hsv baseHsv,
            ColorSpectrumComponents components,
            double minHue,
            double maxHue,
            double minSaturation,
            double maxSaturation,
            double minValue,
            double maxValue,
            byte[] minPixels,
            byte[] middle1Pixels,
            byte[] middle2Pixels,
            byte[] middle3Pixels,
            byte[] middle4Pixels,
            byte[] maxPixels,
            List<Hsv> hsvValues,
            int offset)
        {
            double distanceFromRadius = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2));
            double xToUse = x;
            double yToUse = y;

            if (distanceFromRadius > radius)
            {
                xToUse = (radius / distanceFromRadius) * (x - radius) + radius;
                yToUse = (radius / distanceFromRadius) * (y - radius) + radius;
                distanceFromRadius = radius;
            }

            double r = 1 - distanceFromRadius / radius;
            double theta = Math.Atan2(radius - yToUse, radius - xToUse) * 180.0 / Math.PI;
            theta += 180.0;
            theta = Math.Floor(theta);

            while (theta > 360)
            {
                theta -= 360;
            }

            FillPixel(r, theta / 360, baseHsv, components, minHue, maxHue, minSaturation, maxSaturation, minValue, maxValue, minPixels, middle1Pixels, middle2Pixels, middle3Pixels, middle4Pixels, maxPixels, hsvValues, offset);
        }

        private static void FillPixel(
            double firstPercent,
            double secondPercent,
            Hsv baseHsv,
            ColorSpectrumComponents components,
            double minHue,
            double maxHue,
            double minSaturation,
            double maxSaturation,
            double minValue,
            double maxValue,
            byte[] minPixels,
            byte[] middle1Pixels,
            byte[] middle2Pixels,
            byte[] middle3Pixels,
            byte[] middle4Pixels,
            byte[] maxPixels,
            List<Hsv> hsvValues,
            int offset)
        {
            double hMin = minHue;
            double hMax = maxHue;
            double sMin = minSaturation / 100.0;
            double sMax = maxSaturation / 100.0;
            double vMin = minValue / 100.0;
            double vMax = maxValue / 100.0;

            Hsv hsvMin = baseHsv;
            Hsv hsvMiddle1 = baseHsv;
            Hsv hsvMiddle2 = baseHsv;
            Hsv hsvMiddle3 = baseHsv;
            Hsv hsvMiddle4 = baseHsv;
            Hsv hsvMax = baseHsv;

            switch (components)
            {
                case ColorSpectrumComponents.HueValue:
                    SetHue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, hMin + secondPercent * (hMax - hMin));
                    SetValue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, vMin + firstPercent * (vMax - vMin));
                    hsvMin.S = 0;
                    hsvMax.S = 1;
                    break;

                case ColorSpectrumComponents.HueSaturation:
                    SetHue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, hMin + secondPercent * (hMax - hMin));
                    SetSaturation(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, sMin + firstPercent * (sMax - sMin));
                    hsvMin.V = 0;
                    hsvMax.V = 1;
                    break;

                case ColorSpectrumComponents.ValueHue:
                    SetValue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, vMin + secondPercent * (vMax - vMin));
                    SetHue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, hMin + firstPercent * (hMax - hMin));
                    hsvMin.S = 0;
                    hsvMax.S = 1;
                    break;

                case ColorSpectrumComponents.ValueSaturation:
                    SetValue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, vMin + secondPercent * (vMax - vMin));
                    SetSaturation(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, sMin + firstPercent * (sMax - sMin));
                    SetHueStops(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax);
                    break;

                case ColorSpectrumComponents.SaturationHue:
                    SetSaturation(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, sMin + secondPercent * (sMax - sMin));
                    SetHue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, hMin + firstPercent * (hMax - hMin));
                    hsvMin.V = 0;
                    hsvMax.V = 1;
                    break;

                case ColorSpectrumComponents.SaturationValue:
                    SetSaturation(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, sMin + secondPercent * (sMax - sMin));
                    SetValue(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax, vMin + firstPercent * (vMax - vMin));
                    SetHueStops(ref hsvMin, ref hsvMiddle1, ref hsvMiddle2, ref hsvMiddle3, ref hsvMiddle4, ref hsvMax);
                    break;
            }

            if (components == ColorSpectrumComponents.HueSaturation ||
                components == ColorSpectrumComponents.SaturationHue)
            {
                InvertSaturation(ref hsvMin, sMin, sMax);
                InvertSaturation(ref hsvMiddle1, sMin, sMax);
                InvertSaturation(ref hsvMiddle2, sMin, sMax);
                InvertSaturation(ref hsvMiddle3, sMin, sMax);
                InvertSaturation(ref hsvMiddle4, sMin, sMax);
                InvertSaturation(ref hsvMax, sMin, sMax);
            }
            else
            {
                InvertValue(ref hsvMin, vMin, vMax);
                InvertValue(ref hsvMiddle1, vMin, vMax);
                InvertValue(ref hsvMiddle2, vMin, vMax);
                InvertValue(ref hsvMiddle3, vMin, vMax);
                InvertValue(ref hsvMiddle4, vMin, vMax);
                InvertValue(ref hsvMax, vMin, vMax);
            }

            hsvValues.Add(hsvMin);
            WritePixel(minPixels, offset, HsvToRgb(hsvMin));
            WritePixel(maxPixels, offset, HsvToRgb(hsvMax));

            if (components == ColorSpectrumComponents.ValueSaturation ||
                components == ColorSpectrumComponents.SaturationValue)
            {
                WritePixel(middle1Pixels, offset, HsvToRgb(hsvMiddle1));
                WritePixel(middle2Pixels, offset, HsvToRgb(hsvMiddle2));
                WritePixel(middle3Pixels, offset, HsvToRgb(hsvMiddle3));
                WritePixel(middle4Pixels, offset, HsvToRgb(hsvMiddle4));
            }
        }

        private static void SetHue(ref Hsv min, ref Hsv middle1, ref Hsv middle2, ref Hsv middle3, ref Hsv middle4, ref Hsv max, double value)
        {
            min.H = middle1.H = middle2.H = middle3.H = middle4.H = max.H = value;
        }

        private static void SetSaturation(ref Hsv min, ref Hsv middle1, ref Hsv middle2, ref Hsv middle3, ref Hsv middle4, ref Hsv max, double value)
        {
            min.S = middle1.S = middle2.S = middle3.S = middle4.S = max.S = value;
        }

        private static void SetValue(ref Hsv min, ref Hsv middle1, ref Hsv middle2, ref Hsv middle3, ref Hsv middle4, ref Hsv max, double value)
        {
            min.V = middle1.V = middle2.V = middle3.V = middle4.V = max.V = value;
        }

        private static void SetHueStops(ref Hsv min, ref Hsv middle1, ref Hsv middle2, ref Hsv middle3, ref Hsv middle4, ref Hsv max)
        {
            min.H = 0;
            middle1.H = 60;
            middle2.H = 120;
            middle3.H = 180;
            middle4.H = 240;
            max.H = 300;
        }

        private static void InvertSaturation(ref Hsv hsv, double min, double max)
        {
            hsv.S = max - hsv.S + min;
        }

        private static void InvertValue(ref Hsv hsv, double min, double max)
        {
            hsv.V = max - hsv.V + min;
        }

        private static void WritePixel(byte[] pixels, int offset, Rgb rgb)
        {
            pixels[offset] = ToByte(rgb.B);
            pixels[offset + 1] = ToByte(rgb.G);
            pixels[offset + 2] = ToByte(rgb.R);
            pixels[offset + 3] = 255;
        }

        private static WriteableBitmap CreateBitmap(int size, byte[] pixels)
        {
            var bitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, size, size), pixels, size * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }

        private static Brush CreateImageBrush(ImageSource image)
        {
            if (image == null)
            {
                return null;
            }

            var brush = new ImageBrush(image)
            {
                Stretch = Stretch.Fill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };
            brush.Freeze();
            return brush;
        }

        private bool SelectionEllipseShouldBeLight()
        {
            Color displayedColor;

            if (Components == ColorSpectrumComponents.HueSaturation ||
                Components == ColorSpectrumComponents.SaturationHue)
            {
                Vector4 hsvColor = HsvColor;
                displayedColor = ColorFromRgba(new Hsv(hsvColor.X, hsvColor.Y, 1.0), hsvColor.W);
            }
            else
            {
                displayedColor = Color;
            }

            double rg = displayedColor.R <= 10 ? displayedColor.R / 3294.0 : Math.Pow(displayedColor.R / 269.0 + 0.0513, 2.4);
            double gg = displayedColor.G <= 10 ? displayedColor.G / 3294.0 : Math.Pow(displayedColor.G / 269.0 + 0.0513, 2.4);
            double bg = displayedColor.B <= 10 ? displayedColor.B / 3294.0 : Math.Pow(displayedColor.B / 269.0 + 0.0513, 2.4);

            return 0.2126 * rg + 0.7152 * gg + 0.0722 * bg <= 0.5;
        }

        private void UpdateVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, _isPointerPressed ? (_shouldShowLargeSelection ? "PressedLarge" : "Pressed") : _isPointerOver ? "PointerOver" : "Normal", useTransitions);
            VisualStateManager.GoToState(this, _shapeFromLastBitmapCreation == ColorSpectrumShape.Box ? "BoxSelected" : "RingSelected", useTransitions);
            VisualStateManager.GoToState(this, SelectionEllipseShouldBeLight() ? "SelectionEllipseLight" : "SelectionEllipseDark", useTransitions);
            VisualStateManager.GoToState(this, IsEnabled && IsKeyboardFocusWithin ? "Focused" : "Unfocused", useTransitions);
        }

        private void HookTemplateEvents()
        {
            if (_layoutRoot != null)
            {
                _layoutRoot.SizeChanged += OnLayoutRootSizeChanged;
            }

            if (_inputTarget != null)
            {
                _inputTarget.MouseEnter += OnInputTargetMouseEntered;
                _inputTarget.MouseLeave += OnInputTargetMouseExited;
                _inputTarget.MouseLeftButtonDown += OnInputTargetMouseLeftButtonDown;
                _inputTarget.MouseMove += OnInputTargetMouseMoved;
                _inputTarget.MouseLeftButtonUp += OnInputTargetMouseLeftButtonUp;
            }
        }

        private void UnhookTemplateEvents()
        {
            if (_layoutRoot != null)
            {
                _layoutRoot.SizeChanged -= OnLayoutRootSizeChanged;
            }

            if (_inputTarget != null)
            {
                _inputTarget.MouseEnter -= OnInputTargetMouseEntered;
                _inputTarget.MouseLeave -= OnInputTargetMouseExited;
                _inputTarget.MouseLeftButtonDown -= OnInputTargetMouseLeftButtonDown;
                _inputTarget.MouseMove -= OnInputTargetMouseMoved;
                _inputTarget.MouseLeftButtonUp -= OnInputTargetMouseLeftButtonUp;
            }
        }

        private void OnLayoutRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CreateBitmapsAndColorMap();
        }

        private void OnInputTargetMouseEntered(object sender, MouseEventArgs e)
        {
            _isPointerOver = true;
            UpdateVisualState(true);
            e.Handled = true;
        }

        private void OnInputTargetMouseExited(object sender, MouseEventArgs e)
        {
            _isPointerOver = false;
            UpdateVisualState(true);
            e.Handled = true;
        }

        private void OnInputTargetMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_inputTarget == null)
            {
                return;
            }

            Focus();
            _inputTarget.CaptureMouse();
            _isPointerPressed = true;
            _shouldShowLargeSelection = true;
            UpdateColorFromPoint(e.GetPosition(_inputTarget));
            UpdateVisualState(true);
            e.Handled = true;
        }

        private void OnInputTargetMouseMoved(object sender, MouseEventArgs e)
        {
            if (_inputTarget == null)
            {
                return;
            }

            if (_inputTarget.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateColorFromPoint(e.GetPosition(_inputTarget));
                e.Handled = true;
            }
        }

        private void OnInputTargetMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_inputTarget != null && _inputTarget.IsMouseCaptured)
            {
                _inputTarget.ReleaseMouseCapture();
            }

            _isPointerPressed = false;
            _shouldShowLargeSelection = false;
            UpdateVisualState(true);
            e.Handled = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                _hsvValues.Clear();
            }
        }

        private double GetSpectrumDimension()
        {
            double width = _layoutRoot?.ActualWidth ?? ActualWidth;
            double height = _layoutRoot?.ActualHeight ?? ActualHeight;

            if (width <= 0 || double.IsNaN(width))
            {
                width = Width > 0 && !double.IsNaN(Width) ? Width : MinWidth;
            }

            if (height <= 0 || double.IsNaN(height))
            {
                height = Height > 0 && !double.IsNaN(Height) ? Height : MinHeight;
            }

            return Math.Floor(Math.Min(width, height));
        }

        private static double Percent(double value, double min, double max)
        {
            return max != min ? (value - min) / (max - min) : 0;
        }

        private bool _updatingColor;
        private bool _updatingHsvColor;
        private bool _isPointerOver;
        private bool _isPointerPressed;
        private bool _shouldShowLargeSelection;
        private List<Hsv> _hsvValues = new List<Hsv>();

        private FrameworkElement _layoutRoot;
        private FrameworkElement _sizingGrid;
        private Rectangle _spectrumRectangle;
        private Ellipse _spectrumEllipse;
        private Rectangle _spectrumOverlayRectangle;
        private Ellipse _spectrumOverlayEllipse;
        private FrameworkElement _inputTarget;
        private FrameworkElement _selectionEllipsePanel;
        private ToolTip _colorNameToolTip;

        private WriteableBitmap _hueRedBitmap;
        private WriteableBitmap _hueYellowBitmap;
        private WriteableBitmap _hueGreenBitmap;
        private WriteableBitmap _hueCyanBitmap;
        private WriteableBitmap _hueBlueBitmap;
        private WriteableBitmap _huePurpleBitmap;
        private WriteableBitmap _saturationMinimumBitmap;
        private WriteableBitmap _saturationMaximumBitmap;
        private WriteableBitmap _valueBitmap;

        private ColorSpectrumShape _shapeFromLastBitmapCreation = ColorSpectrumShape.Box;
        private ColorSpectrumComponents _componentsFromLastBitmapCreation = ColorSpectrumComponents.HueSaturation;
        private double _imageWidthFromLastBitmapCreation;
        private double _imageHeightFromLastBitmapCreation;
        private int _minHueFromLastBitmapCreation;
        private int _maxHueFromLastBitmapCreation;
        private int _minSaturationFromLastBitmapCreation;
        private int _maxSaturationFromLastBitmapCreation;
        private int _minValueFromLastBitmapCreation;
        private int _maxValueFromLastBitmapCreation;
        private Color _oldColor = Colors.White;
        private Vector4 _oldHsvColor = new Vector4(0, 0, 1, 1);
    }
}
