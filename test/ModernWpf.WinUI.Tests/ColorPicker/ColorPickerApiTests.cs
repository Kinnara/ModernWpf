using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using ColorPickerControl = ModernWpf.Controls.ColorPicker;
using ColorSpectrumControl = ModernWpf.Controls.Primitives.ColorSpectrum;
using ColorPickerSliderControl = ModernWpf.Controls.Primitives.ColorPickerSlider;

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
            Assert.AreEqual(Colors.Green, colorPicker.Color);

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

            Assert.AreNotEqual(Colors.Green, colorPicker.Color);
            Assert.AreEqual(Colors.Red, colorPicker.PreviousColor);
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
    public void ColorPickerRaisesColorChanged()
    {
        WpfTestHost.Run(() =>
        {
            var colorPicker = new ColorPickerControl();
            ColorChangedEventArgs? eventArgs = null;

            colorPicker.ColorChanged += (_, args) => eventArgs = args;
            colorPicker.Color = Colors.Green;

            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(Colors.White, eventArgs!.OldColor);
            Assert.AreEqual(Colors.Green, eventArgs.NewColor);
        });
    }

    [TestMethod]
    public void ColorSpectrumDefaultsAndHsvSynchronization()
    {
        WpfTestHost.Run(() =>
        {
            var colorSpectrum = new ColorSpectrumControl();

            Assert.AreEqual(Colors.White, colorSpectrum.Color);
            AssertVectorClose(new Vector4(0, 0, 1, 1), colorSpectrum.HsvColor);
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
            Assert.IsTrue(Math.Abs(colorSpectrum.HsvColor.X - 120.0) < 0.1);
            Assert.IsTrue(Math.Abs(colorSpectrum.HsvColor.Y - 1.0) < 0.1);
            Assert.IsTrue(Math.Abs(colorSpectrum.HsvColor.Z - 0.5) < 0.1);
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
            AssertVectorClose(new Vector4(120, 1, 1, 1), colorSpectrum.HsvColor);
        });
    }

    [TestMethod]
    public void ColorSpectrumRaisesColorChanged()
    {
        WpfTestHost.Run(() =>
        {
            var colorSpectrum = new ColorSpectrumControl { Color = Colors.Red };
            ColorChangedEventArgs? eventArgs = null;

            colorSpectrum.ColorChanged += (_, args) => eventArgs = args;
            colorSpectrum.Color = Colors.Green;

            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(Colors.Red, eventArgs!.OldColor);
            Assert.AreEqual(Colors.Green, eventArgs.NewColor);
        });
    }

    [TestMethod]
    public void ColorPickerSliderDefaultsAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var slider = new ColorPickerSliderControl();

            Assert.AreEqual(ColorPickerHsvChannel.Value, slider.ColorChannel);

            slider.ColorChannel = ColorPickerHsvChannel.Alpha;

            Assert.AreEqual(ColorPickerHsvChannel.Alpha, slider.ColorChannel);
        });
    }

    [TestMethod]
    public void ColorPickerRejectsInvalidHueRange()
    {
        WpfTestHost.Run(() =>
        {
            var colorPicker = new ColorPickerControl();
            var exception = Assert.ThrowsException<ArgumentException>(() => colorPicker.MinHue = -1);

            StringAssert.Contains(exception.Message, "MinHue must be between 0 and 359.");
        });
    }

    [TestMethod]
    public void ColorSpectrumHandlesFractionalSize()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorSpectrum = new ColorSpectrumControl
            {
                Width = 332.75,
                Height = 332.75
            };

            using var host = new TestWindowHost(colorSpectrum, width: 420, height: 420);

            Assert.AreEqual(332.75, colorSpectrum.Width);
            Assert.AreEqual(332.75, colorSpectrum.Height);
            Assert.IsNotNull(FindNamedDescendant<Rectangle>(colorSpectrum, "SpectrumRectangle"));
        });
    }

    [TestMethod]
    public void ClearingTextInputsDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                IsAlphaEnabled = true,
                IsHexInputVisible = true
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 560);
            var hexTextBox = FindNamedDescendant<TextBox>(colorPicker, "HexTextBox");
            var alphaTextBox = FindNamedDescendant<TextBox>(colorPicker, "AlphaTextBox");

            Assert.IsTrue(hexTextBox.Text.Length > 0);
            Assert.IsTrue(alphaTextBox.Text.Length > 0);

            hexTextBox.Text = string.Empty;
            alphaTextBox.Text = string.Empty;
            host.UpdateLayout();

            Assert.AreEqual(string.Empty, hexTextBox.Text);
            Assert.AreEqual(string.Empty, alphaTextBox.Text);
        });
    }

    [TestMethod]
    public void ColorPickerTemplateVisibilityFollowsProperties()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                IsAlphaEnabled = true,
                IsColorSpectrumVisible = true,
                IsColorSliderVisible = true,
                IsAlphaSliderVisible = true,
                IsColorPreviewVisible = true
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 560);

            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<ColorSpectrumControl>(colorPicker, "ColorSpectrum").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<ColorPickerSliderControl>(colorPicker, "ThirdDimensionSlider").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<ColorPickerSliderControl>(colorPicker, "AlphaSlider").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<Rectangle>(colorPicker, "PreviewRectangle").Visibility);

            colorPicker.IsColorSpectrumVisible = false;
            colorPicker.IsColorSliderVisible = false;
            colorPicker.IsAlphaSliderVisible = false;
            colorPicker.IsColorPreviewVisible = false;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<ColorSpectrumControl>(colorPicker, "ColorSpectrum").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<ColorPickerSliderControl>(colorPicker, "ThirdDimensionSlider").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<ColorPickerSliderControl>(colorPicker, "AlphaSlider").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Rectangle>(colorPicker, "PreviewRectangle").Visibility);
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
            Assert.IsTrue(resources.Contains("ColorPickerBorderStyle"));

            AssertThemeResourceReference("Light", "ColorPickerSliderThumbBackground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("Light", "ColorPickerSliderThumbBackgroundPointerOver", "SystemControlHighlightChromeAltLowBrush");
            AssertThemeResourceReference("Light", "ColorPickerSliderThumbBackgroundPressed", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("Light", "ColorPickerSliderThumbBackgroundDisabled", "ControlStrongFillColorDisabledBrush");
            AssertThemeResourceReference("Light", "ColorPickerSliderTrackFillDisabled", "AccentFillColorDisabledBrush");
            AssertThemeResourceReference("Light", "ColorPickerHeaderContentDisabled", "TextFillColorDisabledBrush");
            AssertThemeResourceReference("Light", "ColorPickerBorderBrush", "ControlStrokeColorDefaultBrush");

            AssertThemeResourceReference("Dark", "ColorPickerSliderThumbBackground", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("Dark", "ColorPickerSliderThumbBackgroundPointerOver", "SystemControlHighlightChromeAltLowBrush");
            AssertThemeResourceReference("Dark", "ColorPickerSliderThumbBackgroundPressed", "TextFillColorPrimaryBrush");
            AssertThemeResourceReference("Dark", "ColorPickerSliderThumbBackgroundDisabled", "ControlStrongFillColorDisabledBrush");
            AssertThemeResourceReference("Dark", "ColorPickerSliderTrackFillDisabled", "AccentFillColorDisabledBrush");
            AssertThemeResourceReference("Dark", "ColorPickerHeaderContentDisabled", "TextFillColorDisabledBrush");
            AssertThemeResourceReference("Dark", "ColorPickerBorderBrush", "ControlStrokeColorDefaultBrush");

            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackgroundPointerOver", "SystemControlHighlightChromeAltLowBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackgroundPressed", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderThumbBackgroundDisabled", "SystemControlDisabledChromeDisabledHighBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerSliderTrackFillDisabled", "SystemControlDisabledBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerHeaderContentDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ColorPickerBorderBrush", "SystemControlForegroundListLowBrush");
        });
    }

    private static void AssertVectorClose(Vector4 expected, Vector4 actual)
    {
        Assert.IsTrue(Math.Abs(expected.X - actual.X) < 0.1, "X");
        Assert.IsTrue(Math.Abs(expected.Y - actual.Y) < 0.1, "Y");
        Assert.IsTrue(Math.Abs(expected.Z - actual.Z) < 0.1, "Z");
        Assert.IsTrue(Math.Abs(expected.W - actual.W) < 0.1, "W");
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
}
