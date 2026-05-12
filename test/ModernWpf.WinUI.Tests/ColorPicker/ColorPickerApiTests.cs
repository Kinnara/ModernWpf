using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using ColorPickerControl = ModernWpf.Controls.ColorPicker;
using ColorPickerSlider = ModernWpf.Controls.Primitives.ColorPickerSlider;
using ColorSpectrum = ModernWpf.Controls.Primitives.ColorSpectrum;

namespace ModernWpf.WinUI.Tests.ColorPicker;

[TestClass]
public class ColorPickerApiTests
{
    [TestMethod]
    public void ColorPickerDefaultsAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var colorPicker = new ColorPickerControl();

            Assert.AreEqual(Colors.White, colorPicker.Color);
            Assert.IsNull(colorPicker.PreviousColor);
            Assert.IsFalse(colorPicker.IsAlphaEnabled);
            Assert.IsTrue(colorPicker.IsColorSpectrumVisible);
            Assert.IsTrue(colorPicker.IsColorPreviewVisible);
            Assert.IsTrue(colorPicker.IsColorSliderVisible);
            Assert.IsTrue(colorPicker.IsAlphaSliderVisible);
            Assert.IsFalse(colorPicker.IsMoreButtonVisible);
            Assert.IsTrue(colorPicker.IsColorChannelTextInputVisible);
            Assert.IsTrue(colorPicker.IsAlphaTextInputVisible);
            Assert.IsTrue(colorPicker.IsHexInputVisible);
            Assert.AreEqual(0, colorPicker.MinHue);
            Assert.AreEqual(359, colorPicker.MaxHue);
            Assert.AreEqual(0, colorPicker.MinSaturation);
            Assert.AreEqual(100, colorPicker.MaxSaturation);
            Assert.AreEqual(0, colorPicker.MinValue);
            Assert.AreEqual(100, colorPicker.MaxValue);
            Assert.AreEqual(ColorSpectrumShape.Box, colorPicker.ColorSpectrumShape);
            Assert.AreEqual(ColorSpectrumComponents.HueSaturation, colorPicker.ColorSpectrumComponents);
            Assert.AreEqual(Orientation.Vertical, colorPicker.Orientation);

            colorPicker.Color = Colors.Green;
            var unclampedColor = colorPicker.Color;

            colorPicker.PreviousColor = Colors.Red;
            colorPicker.IsAlphaEnabled = true;
            colorPicker.IsColorSpectrumVisible = false;
            colorPicker.IsColorPreviewVisible = false;
            colorPicker.IsColorSliderVisible = false;
            colorPicker.IsAlphaSliderVisible = false;
            colorPicker.IsMoreButtonVisible = true;
            colorPicker.IsColorChannelTextInputVisible = false;
            colorPicker.IsAlphaTextInputVisible = false;
            colorPicker.IsHexInputVisible = false;
            colorPicker.MinHue = 10;
            colorPicker.MaxHue = 300;
            colorPicker.MinSaturation = 10;
            colorPicker.MaxSaturation = 90;
            colorPicker.MinValue = 10;
            colorPicker.MaxValue = 90;
            colorPicker.ColorSpectrumShape = ColorSpectrumShape.Ring;
            colorPicker.ColorSpectrumComponents = ColorSpectrumComponents.HueValue;
            colorPicker.Orientation = Orientation.Horizontal;

            Assert.AreNotEqual(unclampedColor, colorPicker.Color);
            Assert.AreEqual(Colors.Red, colorPicker.PreviousColor!.Value);
            Assert.IsTrue(colorPicker.IsAlphaEnabled);
            Assert.IsFalse(colorPicker.IsColorSpectrumVisible);
            Assert.IsFalse(colorPicker.IsColorPreviewVisible);
            Assert.IsFalse(colorPicker.IsColorSliderVisible);
            Assert.IsFalse(colorPicker.IsAlphaSliderVisible);
            Assert.IsTrue(colorPicker.IsMoreButtonVisible);
            Assert.IsFalse(colorPicker.IsColorChannelTextInputVisible);
            Assert.IsFalse(colorPicker.IsAlphaTextInputVisible);
            Assert.IsFalse(colorPicker.IsHexInputVisible);
            Assert.AreEqual(10, colorPicker.MinHue);
            Assert.AreEqual(300, colorPicker.MaxHue);
            Assert.AreEqual(10, colorPicker.MinSaturation);
            Assert.AreEqual(90, colorPicker.MaxSaturation);
            Assert.AreEqual(10, colorPicker.MinValue);
            Assert.AreEqual(90, colorPicker.MaxValue);
            Assert.AreEqual(ColorSpectrumShape.Ring, colorPicker.ColorSpectrumShape);
            Assert.AreEqual(ColorSpectrumComponents.HueValue, colorPicker.ColorSpectrumComponents);
            Assert.AreEqual(Orientation.Horizontal, colorPicker.Orientation);
        });
    }

    [TestMethod]
    public void ColorPickerEventsFollowWinUIContract()
    {
        WpfTestHost.Run(() =>
        {
            var colorPicker = new ColorPickerControl();
            ColorPickerControl? sender = null;
            ColorChangedEventArgs? eventArgs = null;

            colorPicker.ColorChanged += (s, args) =>
            {
                sender = s;
                eventArgs = args;
            };

            colorPicker.Color = Colors.Green;

            Assert.AreSame(colorPicker, sender);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(Colors.White, eventArgs!.OldColor);
            Assert.AreEqual(Colors.Green, eventArgs.NewColor);
        });
    }

    [TestMethod]
    public void ColorSpectrumDefaultsSettersAndHsvSync()
    {
        WpfTestHost.Run(() =>
        {
            var colorSpectrum = new ColorSpectrum();

            Assert.AreEqual(Colors.White, colorSpectrum.Color);
            Assert.AreEqual(new Vector4(0, 0, 1, 1), colorSpectrum.HsvColor);
            Assert.AreEqual(0, colorSpectrum.MinHue);
            Assert.AreEqual(359, colorSpectrum.MaxHue);
            Assert.AreEqual(0, colorSpectrum.MinSaturation);
            Assert.AreEqual(100, colorSpectrum.MaxSaturation);
            Assert.AreEqual(0, colorSpectrum.MinValue);
            Assert.AreEqual(100, colorSpectrum.MaxValue);
            Assert.AreEqual(ColorSpectrumShape.Box, colorSpectrum.Shape);
            Assert.AreEqual(ColorSpectrumComponents.HueSaturation, colorSpectrum.Components);

            colorSpectrum.Color = Colors.Green;
            colorSpectrum.MinHue = 10;
            colorSpectrum.MaxHue = 300;
            colorSpectrum.MinSaturation = 10;
            colorSpectrum.MaxSaturation = 90;
            colorSpectrum.MinValue = 10;
            colorSpectrum.MaxValue = 90;
            colorSpectrum.Shape = ColorSpectrumShape.Ring;
            colorSpectrum.Components = ColorSpectrumComponents.HueValue;

            Assert.AreEqual(Colors.Green, colorSpectrum.Color);
            AssertClose(120.0, colorSpectrum.HsvColor.X, 0.1);
            AssertClose(1.0, colorSpectrum.HsvColor.Y, 0.1);
            AssertClose(0.5, colorSpectrum.HsvColor.Z, 0.1);
            Assert.AreEqual(10, colorSpectrum.MinHue);
            Assert.AreEqual(300, colorSpectrum.MaxHue);
            Assert.AreEqual(10, colorSpectrum.MinSaturation);
            Assert.AreEqual(90, colorSpectrum.MaxSaturation);
            Assert.AreEqual(10, colorSpectrum.MinValue);
            Assert.AreEqual(90, colorSpectrum.MaxValue);
            Assert.AreEqual(ColorSpectrumShape.Ring, colorSpectrum.Shape);
            Assert.AreEqual(ColorSpectrumComponents.HueValue, colorSpectrum.Components);

            colorSpectrum.HsvColor = new Vector4(120, 1, 1, 1);

            Assert.AreEqual(Color.FromArgb(255, 0, 255, 0), colorSpectrum.Color);
            Assert.AreEqual(new Vector4(120, 1, 1, 1), colorSpectrum.HsvColor);
        });
    }

    [TestMethod]
    public void ColorSpectrumEventsFollowWinUIContract()
    {
        WpfTestHost.Run(() =>
        {
            var colorSpectrum = new ColorSpectrum
            {
                Color = Colors.Red
            };
            ColorSpectrum? sender = null;
            ColorChangedEventArgs? eventArgs = null;

            colorSpectrum.ColorChanged += (s, args) =>
            {
                sender = s;
                eventArgs = args;
            };

            colorSpectrum.Color = Colors.Green;

            Assert.AreSame(colorSpectrum, sender);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(Colors.Red, eventArgs!.OldColor);
            Assert.AreEqual(Colors.Green, eventArgs.NewColor);
        });
    }

    [TestMethod]
    public void ColorPickerSliderDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var slider = new ColorPickerSlider();

            Assert.AreEqual(ColorPickerHsvChannel.Value, slider.ColorChannel);

            slider.ColorChannel = ColorPickerHsvChannel.Alpha;

            Assert.AreEqual(ColorPickerHsvChannel.Alpha, slider.ColorChannel);
        });
    }

    [TestMethod]
    public void ColorSpectrumSupportsDerivation()
    {
        WpfTestHost.Run(() =>
        {
            var colorSpectrum = new DerivedColorSpectrum();

            Assert.IsNotNull(colorSpectrum);
            Assert.AreEqual(Colors.White, colorSpectrum.Color);
        });
    }

    [TestMethod]
    public void ValidateHueAndPercentageRanges()
    {
        WpfTestHost.Run(() =>
        {
            var colorPicker = new ColorPickerControl();
            var hueException = Assert.ThrowsException<ArgumentException>(() => colorPicker.MinHue = -1);
            Assert.IsTrue(hueException.Message.Contains("MinHue must be between 0 and 359."));

            var colorSpectrum = new ColorSpectrum();
            var saturationException = Assert.ThrowsException<ArgumentException>(() => colorSpectrum.MaxSaturation = 101);
            Assert.IsTrue(saturationException.Message.Contains("MaxSaturation must be between 0 and 100."));
        });
    }

    [TestMethod]
    public void FractionalSpectrumSizeAndRingShapeDoNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorSpectrum = new ColorSpectrum
            {
                Width = 332.75,
                Height = 332.75
            };

            using var host = new TestWindowHost(colorSpectrum, width: 420, height: 420);

            var spectrumRectangle = FindNamedDescendant<Rectangle>(colorSpectrum, "SpectrumRectangle");
            var spectrumEllipse = FindNamedDescendant<Ellipse>(colorSpectrum, "SpectrumEllipse");

            Assert.IsNotNull(spectrumRectangle.Fill);

            colorSpectrum.Shape = ColorSpectrumShape.Ring;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, spectrumRectangle.Visibility);
            Assert.AreEqual(Visibility.Visible, spectrumEllipse.Visibility);
        });
    }

    [TestMethod]
    public void RingSpectrumHitTestingUsesAngleAndRadius()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorSpectrum = new ColorSpectrum
            {
                Width = 200,
                Height = 200,
                Shape = ColorSpectrumShape.Ring,
                Components = ColorSpectrumComponents.HueSaturation
            };

            using var host = new TestWindowHost(colorSpectrum, width: 260, height: 260);
            var spectrumEllipse = FindNamedDescendant<Ellipse>(colorSpectrum, "SpectrumEllipse");

            colorSpectrum.SetColorFromPointForTesting(new Point(spectrumEllipse.ActualWidth, spectrumEllipse.ActualHeight / 2));

            AssertClose(0, colorSpectrum.HsvColor.X, 0.5);
            AssertClose(1, colorSpectrum.HsvColor.Y, 0.01);

            colorSpectrum.SetColorFromPointForTesting(new Point(spectrumEllipse.ActualWidth / 2, spectrumEllipse.ActualHeight / 2));

            AssertClose(0, colorSpectrum.HsvColor.Y, 0.01);
        });
    }

    [TestMethod]
    public void KeyboardAdjustmentFollowsConfiguredComponents()
    {
        WpfTestHost.Run(() =>
        {
            var colorSpectrum = new ColorSpectrum
            {
                HsvColor = new Vector4(10, 0.50f, 0.50f, 1)
            };

            colorSpectrum.AdjustColorForTesting(1, 1);

            AssertClose(11, colorSpectrum.HsvColor.X, 0.01);
            AssertClose(0.51, colorSpectrum.HsvColor.Y, 0.01);

            colorSpectrum.Components = ColorSpectrumComponents.ValueHue;
            colorSpectrum.HsvColor = new Vector4(20, 0.50f, 0.50f, 1);
            colorSpectrum.AdjustColorForTesting(1, 1);

            AssertClose(21, colorSpectrum.HsvColor.X, 0.01);
            AssertClose(0.51, colorSpectrum.HsvColor.Z, 0.01);
        });
    }

    [TestMethod]
    public void SelectionRingHasContrastStrokeAndSpectrumIsFocusable()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorSpectrum = new ColorSpectrum
            {
                Width = 200,
                Height = 200
            };

            using var host = new TestWindowHost(colorSpectrum, width: 260, height: 260);

            var outerRing = FindNamedDescendant<Ellipse>(colorSpectrum, "SelectionEllipseOuter");

            Assert.IsTrue(colorSpectrum.Focusable);
            Assert.AreEqual(Colors.Black, ((SolidColorBrush)outerRing.Stroke).Color);
            Assert.AreEqual(4.0, outerRing.StrokeThickness);
        });
    }

    [TestMethod]
    public void ClearingTextInputFieldsDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                IsAlphaEnabled = true
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 520);

            var hexTextBox = FindNamedDescendant<TextBox>(colorPicker, "HexTextBox");
            var alphaTextBox = FindNamedDescendant<TextBox>(colorPicker, "AlphaTextBox");

            Assert.IsTrue(hexTextBox.Text.Length > 0);
            Assert.IsTrue(alphaTextBox.Text.Length > 0);

            hexTextBox.Text = string.Empty;
            alphaTextBox.Text = string.Empty;
            host.UpdateLayout();
        });
    }

    [TestMethod]
    public void TemplateVisibilityFollowsColorPickerProperties()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                IsAlphaEnabled = true,
                PreviousColor = Colors.Red,
                IsColorSpectrumVisible = false,
                IsColorPreviewVisible = false,
                IsColorSliderVisible = false,
                IsAlphaSliderVisible = false,
                IsMoreButtonVisible = true,
                IsColorChannelTextInputVisible = false,
                IsAlphaTextInputVisible = false,
                IsHexInputVisible = false
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 520);

            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<ColorSpectrum>(colorPicker, "ColorSpectrum").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Rectangle>(colorPicker, "ColorPreviewRectangle").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Rectangle>(colorPicker, "PreviousColorRectangle").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<ColorPickerSlider>(colorPicker, "ThirdDimensionSlider").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<ColorPickerSlider>(colorPicker, "AlphaSlider").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<ToggleButton>(colorPicker, "MoreButton").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<TextBox>(colorPicker, "RedTextBox").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<TextBox>(colorPicker, "AlphaTextBox").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<TextBox>(colorPicker, "HexTextBox").Visibility);
        });
    }

    [TestMethod]
    public void FinalWinUI2ColorPickerResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/ColorPicker/ColorPicker.xaml", UriKind.Relative)
            };

            AssertResource(resources, "ColorPickerSliderCornerRadius", new CornerRadius(6));
            AssertResource(resources, "ColorPickerSliderInnerThumbWidth", 10.0);
            AssertResource(resources, "ColorPickerSliderInnerThumbHeight", 10.0);
            AssertResource(resources, "ColorPickerVerticalOrientationMinWidth", 312.0);
            AssertResource(resources, "ColorPickerVerticalOrientationMaxWidth", 392.0);
            AssertResource(resources, "ColorPickerVerticalOrientationMinHeight", 312.0);
            AssertResource(resources, "ColorPickerVerticalOrientationMaxHeight", 392.0);
            AssertResource(resources, "ColorPickerTextInputHorizontalOrientationMargin", 122.0);

            var borderStyle = (Style)resources["ColorPickerBorderStyle"];
            var strokeThicknessSetter = borderStyle.Setters.OfType<Setter>().Single(setter => setter.Property == Shape.StrokeThicknessProperty);
            Assert.AreEqual(2.0, strokeThicknessSetter.Value);

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "ColorPickerSliderThumbBackground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ColorPickerSliderThumbBackgroundPointerOver", "SystemControlHighlightChromeAltLowBrush");
                AssertThemeResourceReference(themeName, "ColorPickerSliderThumbBackgroundPressed", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ColorPickerSliderThumbBackgroundDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ColorPickerSliderTrackFillDisabled", "AccentFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ColorPickerHeaderContentDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "ColorPickerBorderBrush", "ControlStrokeColorDefaultBrush");
            }

            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackgroundPointerOver", "SystemControlHighlightChromeAltLowBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackgroundPressed", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackgroundDisabled", "SystemControlDisabledChromeDisabledHighBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderTrackFillDisabled", "SystemControlDisabledBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerHeaderContentDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerBorderBrush", "SystemControlForegroundListLowBrush");
        });
    }

    private static void AssertClose(double expected, double actual, double tolerance)
    {
        Assert.IsTrue(Math.Abs(expected - actual) < tolerance, $"Expected {actual} to be within {tolerance} of {expected}.");
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expected)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expected, resources[key], $"Unexpected value for '{key}'.");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }

    private sealed class DerivedColorSpectrum : ColorSpectrum
    {
    }
}
