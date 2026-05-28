using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
    public void ColorPickerSliderTemplateUsesWinUISourceStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl();

            using var host = new TestWindowHost(colorPicker, width: 420, height: 560);

            var slider = FindNamedDescendant<ColorPickerSlider>(colorPicker, "ThirdDimensionSlider");
            var stateRoot = VisualTreeTestHelper
                .EnumerateDescendants(slider)
                .OfType<FrameworkElement>()
                .FirstOrDefault(element => VisualStateManager.GetVisualStateGroups(element)
                    .OfType<VisualStateGroup>()
                    .Any(group => group.Name == "CommonStates"))
                ?? throw new AssertFailedException("Expected ColorPickerSlider template to contain source visual-state groups.");
            var sliderContainer = FindNamedDescendant<Grid>(slider, "SliderContainer");
            var track = FindNamedDescendant<Track>(slider, "PART_Track");
            var thumb = FindNamedDescendant<Thumb>(slider, "HorizontalThumb");

            Assert.AreSame(thumb, track.Thumb);
            Assert.IsTrue(ModernWpf.Controls.Primitives.FocusVisualHelper.GetIsTemplateFocusTarget(sliderContainer));

            AssertStateSetter(stateRoot, "CommonStates", "PointerOver", "HorizontalThumb.Background");
            AssertStateSetter(stateRoot, "CommonStates", "Pressed", "HorizontalThumb.Background");
            AssertStateSetter(
                stateRoot,
                "CommonStates",
                "Disabled",
                "HeaderContentPresenter.Foreground",
                "HorizontalThumb.Background",
                "HorizontalTrackRect.Fill",
                "HorizontalDecreaseRect.Fill");
            AssertStateSetter(
                stateRoot,
                "FocusEngagementStates",
                "FocusEngagedHorizontal",
                "SliderContainer.(FocusVisualHelper.IsTemplateFocusTarget)",
                "HorizontalThumb.(FocusVisualHelper.IsTemplateFocusTarget)");
            AssertStateSetter(
                stateRoot,
                "FocusEngagementStates",
                "FocusEngagedVertical",
                "SliderContainer.(FocusVisualHelper.IsTemplateFocusTarget)",
                "HorizontalThumb.(FocusVisualHelper.IsTemplateFocusTarget)");

            Assert.IsTrue(VisualStateManager.GoToState(slider, "FocusEngagedHorizontal", false));

            Assert.IsFalse(ModernWpf.Controls.Primitives.FocusVisualHelper.GetIsTemplateFocusTarget(sliderContainer));
            Assert.IsTrue(ModernWpf.Controls.Primitives.FocusVisualHelper.GetIsTemplateFocusTarget(thumb));
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
            var rectangleBorder = FindNamedDescendant<Rectangle>(colorSpectrum, "RectangleBorder");
            var ellipseBorder = FindNamedDescendant<Ellipse>(colorSpectrum, "EllipseBorder");
            var layoutRoot = FindNamedDescendant<Grid>(colorSpectrum, "LayoutRoot");
            AssertStateSetter(
                layoutRoot,
                "ShapeSelected",
                "RingSelected",
                "SpectrumRectangle.Visibility",
                "SpectrumOverlayRectangle.Visibility",
                "RectangleBorder.Visibility",
                "SpectrumEllipse.Visibility",
                "SpectrumOverlayEllipse.Visibility",
                "EllipseBorder.Visibility");

            Assert.IsNotNull(spectrumRectangle.Fill);
            Assert.AreEqual(Visibility.Visible, rectangleBorder.Visibility);
            Assert.AreEqual(Visibility.Collapsed, ellipseBorder.Visibility);

            colorSpectrum.Shape = ColorSpectrumShape.Ring;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, spectrumRectangle.Visibility);
            Assert.AreEqual(Visibility.Visible, spectrumEllipse.Visibility);
            Assert.AreEqual(Visibility.Collapsed, rectangleBorder.Visibility);
            Assert.AreEqual(Visibility.Visible, ellipseBorder.Visibility);
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

            AssertClose(0, colorSpectrum.HsvColor.X, 3.0);
            AssertClose(0, colorSpectrum.HsvColor.Y, 0.02);

            colorSpectrum.SetColorFromPointForTesting(new Point(spectrumEllipse.ActualWidth / 2, spectrumEllipse.ActualHeight / 2));

            AssertClose(359, colorSpectrum.HsvColor.X, 3.0);
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

            AssertClose(19, colorSpectrum.HsvColor.X, 0.01);
            AssertClose(0.49, colorSpectrum.HsvColor.Z, 0.01);
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

            var focusEllipse = FindNamedDescendant<Ellipse>(colorSpectrum, "FocusEllipse");
            var selectionEllipse = FindNamedDescendant<Ellipse>(colorSpectrum, "SelectionEllipse");

            Assert.IsTrue(colorSpectrum.Focusable);
            Assert.AreEqual(Colors.White, ((SolidColorBrush)focusEllipse.Stroke).Color);
            Assert.AreEqual(Colors.Black, ((SolidColorBrush)selectionEllipse.Stroke).Color);
            Assert.AreEqual(2.0, selectionEllipse.StrokeThickness);
        });
    }

    [TestMethod]
    public void ColorSpectrumInputAndFocusStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorSpectrum = new ColorSpectrum
            {
                Width = 200,
                Height = 200,
                Color = Colors.White
            };

            using var host = new TestWindowHost(colorSpectrum, width: 260, height: 260);

            var layoutRoot = FindNamedDescendant<Grid>(colorSpectrum, "LayoutRoot");
            var focusEllipse = FindNamedDescendant<Ellipse>(colorSpectrum, "FocusEllipse");
            var selectionEllipse = FindNamedDescendant<Ellipse>(colorSpectrum, "SelectionEllipse");

            AssertStateSetter(layoutRoot, "CommonStates", "PointerOver", "SelectionEllipse.Opacity");
            AssertStateSetter(layoutRoot, "CommonStates", "PressedLarge", "SelectionEllipsePanel.Width", "SelectionEllipsePanel.Height");
            AssertStateSetter(layoutRoot, "SelectionEllipseColor", "SelectionEllipseDark", "FocusEllipse.Stroke", "SelectionEllipse.Stroke");
            AssertStateSetter(layoutRoot, "FocusStates", "Focused", "FocusEllipse.Visibility");

            Assert.AreEqual("SelectionEllipseDark", GetCurrentStateName(layoutRoot, "SelectionEllipseColor"));
            Assert.AreEqual(Colors.Black, ((SolidColorBrush)selectionEllipse.Stroke).Color);

            colorSpectrum.Color = Colors.Blue;
            host.UpdateLayout();

            Assert.AreEqual("SelectionEllipseLight", GetCurrentStateName(layoutRoot, "SelectionEllipseColor"));
            Assert.AreEqual(Colors.White, ((SolidColorBrush)selectionEllipse.Stroke).Color);

            Assert.AreEqual(Visibility.Collapsed, focusEllipse.Visibility);
            colorSpectrum.Focus();
            host.UpdateLayout();

            Assert.AreEqual("Focused", GetCurrentStateName(layoutRoot, "FocusStates"));
            Assert.AreEqual(Visibility.Visible, focusEllipse.Visibility);
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
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Grid>(colorPicker, "ColorPreviewRectangleGrid").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Grid>(colorPicker, "ThirdDimensionSliderGrid").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Grid>(colorPicker, "AlphaSliderGrid").Visibility);
            Assert.AreEqual(Visibility.Visible, FindNamedDescendant<ToggleButton>(colorPicker, "MoreButton").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Grid>(colorPicker, "ColorChannelTextInputPanel").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<Grid>(colorPicker, "AlphaPanel").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<TextBox>(colorPicker, "HexTextBox").Visibility);
        });
    }

    [TestMethod]
    public void HexTextBoxFollowsAlphaEnabledState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                Color = Color.FromArgb(0x80, 0x01, 0x02, 0x03)
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 520);
            var hexTextBox = FindNamedDescendant<TextBox>(colorPicker, "HexTextBox");

            Assert.AreEqual("#010203", hexTextBox.Text);
            Assert.AreEqual(7, hexTextBox.MaxLength);

            colorPicker.IsAlphaEnabled = true;
            host.UpdateLayout();

            Assert.AreEqual("#80010203", hexTextBox.Text);
            Assert.AreEqual(9, hexTextBox.MaxLength);

            colorPicker.IsAlphaEnabled = false;
            host.UpdateLayout();

            Assert.AreEqual("#010203", hexTextBox.Text);
            Assert.AreEqual(7, hexTextBox.MaxLength);
        });
    }

    [TestMethod]
    public void OrientationStateUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                Orientation = Orientation.Horizontal
            };

            using var host = new TestWindowHost(colorPicker, width: 620, height: 360);

            var rootGrid = FindNamedDescendant<Grid>(colorPicker, "RootGrid");
            var thirdDimensionSlider = FindNamedDescendant<ColorPickerSlider>(colorPicker, "ThirdDimensionSlider");
            var alphaSlider = FindNamedDescendant<ColorPickerSlider>(colorPicker, "AlphaSlider");
            var moreButton = FindNamedDescendant<ToggleButton>(colorPicker, "MoreButton");
            var textEntryGrid = FindNamedDescendant<Grid>(colorPicker, "TextEntryGrid");

            AssertStateSetter(
                rootGrid,
                "Orientation",
                "Horizontal",
                "RootGrid.MinHeight",
                "RootGrid.MaxHeight",
                "RootGrid.MinWidth",
                "RootGrid.MaxWidth",
                "ColorSpectrumGrid.Margin",
                "ColorPreviewRectangleGrid.Margin",
                "ThirdDimensionSliderGrid.(Grid.Column)",
                "ThirdDimensionSliderGrid.(Grid.Row)",
                "ThirdDimensionSliderGrid.Margin",
                "ThirdDimensionSlider.Orientation",
                "ThirdDimensionBackgroundRectangle.Height",
                "ThirdDimensionBackgroundRectangle.Width",
                "ThirdDimensionBackgroundRectangle.HorizontalAlignment",
                "ThirdDimensionBackgroundRectangle.VerticalAlignment",
                "AlphaSliderGrid.(Grid.Column)",
                "AlphaSliderGrid.(Grid.Row)",
                "AlphaSliderGrid.Margin",
                "AlphaSlider.Orientation",
                "AlphaSliderCheckeredBackgroundRectangle.Height",
                "AlphaSliderCheckeredBackgroundRectangle.Width",
                "AlphaSliderCheckeredBackgroundRectangle.HorizontalAlignment",
                "AlphaSliderCheckeredBackgroundRectangle.VerticalAlignment",
                "AlphaSliderBackgroundRectangle.Height",
                "AlphaSliderBackgroundRectangle.Width",
                "AlphaSliderBackgroundRectangle.HorizontalAlignment",
                "AlphaSliderBackgroundRectangle.VerticalAlignment",
                "MoreEntriesPanel.(Grid.Column)",
                "MoreEntriesPanel.(Grid.Row)",
                "MoreEntriesPanel.Margin",
                "MoreButton.Margin",
                "ColorRepresentationComboBox.(Grid.Row)",
                "ColorTextInputPanels.(Grid.Row)",
                "HexTextBox.TabIndex",
                "HexTextBox.(Grid.Row)",
                "HexTextBox.(Grid.Column)",
                "HexTextBox.(Grid.ColumnSpan)",
                "HexTextBox.Margin",
                "HexTextBox.HorizontalAlignment",
                "HexTextBox.Width",
                "RgbTextLabelColumn.Width",
                "HsvTextLabelColumn.Width",
                "AlphaTextLabelColumn.Width");

            Assert.AreEqual("Horizontal", GetCurrentStateName(rootGrid, "Orientation"));
            Assert.AreEqual(312.0, rootGrid.MinHeight);
            Assert.AreEqual(392.0, rootGrid.MaxHeight);
            Assert.AreEqual(0.0, rootGrid.MinWidth);
            Assert.AreEqual(10000.0, rootGrid.MaxWidth);
            Assert.AreEqual(Orientation.Vertical, thirdDimensionSlider.Orientation);
            Assert.AreEqual(Orientation.Vertical, alphaSlider.Orientation);
            Assert.AreEqual(new Thickness(0), moreButton.Margin);
            Assert.AreEqual(Visibility.Visible, textEntryGrid.Visibility);

            colorPicker.Orientation = Orientation.Vertical;
            host.UpdateLayout();

            Assert.AreEqual("Vertical", GetCurrentStateName(rootGrid, "Orientation"));
            Assert.AreEqual(Orientation.Horizontal, thirdDimensionSlider.Orientation);
            Assert.AreEqual(Orientation.Horizontal, alphaSlider.Orientation);
        });
    }

    [TestMethod]
    public void ColorPickerTemplateVisibilityUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl();
            using var host = new TestWindowHost(colorPicker, width: 420, height: 520);

            var rootGrid = FindNamedDescendant<Grid>(colorPicker, "RootGrid");

            AssertStateSetter(
                rootGrid,
                "ColorSpectrumVisibility",
                "ColorSpectrumCollapsed",
                "ColorSpectrum.Visibility",
                "ColorPreviewRectangleGrid.Width",
                "ColorPreviewRectangleGrid.Height",
                "ColorPreviewRectangleGrid.Margin",
                "ColorPreviewRectangleGrid.(Grid.Column)",
                "ColorPreviewRectangleGrid.(Grid.ColumnSpan)");
            AssertStateSetter(rootGrid, "ColorPreviewVisibility", "ColorPreviewCollapsed", "ColorPreviewRectangleGrid.Visibility");
            AssertStateSetter(rootGrid, "PreviousColorVisibility", "PreviousColorVisibleVertical", "ColorPreviewRectangle.(Grid.RowSpan)", "PreviousColorRectangle.Visibility");
            AssertStateSetter(
                rootGrid,
                "PreviousColorVisibility",
                "PreviousColorVisibleHorizontal",
                "ColorPreviewRectangle.(Grid.ColumnSpan)",
                "PreviousColorRectangle.Visibility",
                "PreviousColorRectangle.(Grid.Row)",
                "PreviousColorRectangle.(Grid.Column)",
                "PreviousColorRectangle.(Grid.RowSpan)",
                "PreviousColorRectangle.(Grid.ColumnSpan)");
            AssertStateSetter(rootGrid, "ThirdDimensionSliderVisibility", "ThirdDimensionSliderCollapsed", "ThirdDimensionSliderGrid.Visibility");
            AssertStateSetter(rootGrid, "AlphaSliderVisibility", "AlphaSliderCollapsed", "AlphaSliderGrid.Visibility");
            AssertStateSetter(rootGrid, "MoreButtonVisibility", "MoreButtonCollapsed", "MoreButton.Visibility");
            AssertStateSetter(rootGrid, "TextEntryGridVisibility", "TextEntryGridVisible", "TextEntryGrid.Visibility", "MoreGlyph.Text");
            AssertStateSetter(
                rootGrid,
                "ColorChannelTextInputVisibility",
                "ColorChannelTextInputCollapsed",
                "ColorRepresentationComboBox.Visibility",
                "ColorChannelTextInputPanel.Visibility",
                "HexTextBox.(Grid.Column)",
                "HexTextBox.HorizontalAlignment");
            AssertStateSetter(rootGrid, "AlphaTextInputVisibility", "AlphaTextInputCollapsed", "AlphaPanel.Visibility");
            AssertStateSetter(rootGrid, "ColorRepresentationSelected", "HsvSelected", "RgbPanel.Visibility", "HsvPanel.Visibility");
            AssertStateSetter(rootGrid, "HexInputVisibility", "HexInputCollapsed", "HexTextBox.Visibility");
            AssertStateSetter(rootGrid, "AlphaEnabledState", "AlphaEnabled", "HexTextBox.MaxLength");
            AssertStateSetter(
                rootGrid,
                "Orientation",
                "Horizontal",
                "RootGrid.MinHeight",
                "RootGrid.MaxHeight",
                "RootGrid.MinWidth",
                "RootGrid.MaxWidth",
                "ColorSpectrumGrid.Margin",
                "ColorPreviewRectangleGrid.Margin",
                "ThirdDimensionSliderGrid.(Grid.Column)",
                "ThirdDimensionSliderGrid.(Grid.Row)",
                "ThirdDimensionSliderGrid.Margin",
                "ThirdDimensionSlider.Orientation",
                "ThirdDimensionBackgroundRectangle.Height",
                "ThirdDimensionBackgroundRectangle.Width",
                "ThirdDimensionBackgroundRectangle.HorizontalAlignment",
                "ThirdDimensionBackgroundRectangle.VerticalAlignment",
                "AlphaSliderGrid.(Grid.Column)",
                "AlphaSliderGrid.(Grid.Row)",
                "AlphaSliderGrid.Margin",
                "AlphaSlider.Orientation",
                "AlphaSliderCheckeredBackgroundRectangle.Height",
                "AlphaSliderCheckeredBackgroundRectangle.Width",
                "AlphaSliderCheckeredBackgroundRectangle.HorizontalAlignment",
                "AlphaSliderCheckeredBackgroundRectangle.VerticalAlignment",
                "AlphaSliderBackgroundRectangle.Height",
                "AlphaSliderBackgroundRectangle.Width",
                "AlphaSliderBackgroundRectangle.HorizontalAlignment",
                "AlphaSliderBackgroundRectangle.VerticalAlignment",
                "MoreEntriesPanel.(Grid.Column)",
                "MoreEntriesPanel.(Grid.Row)",
                "MoreEntriesPanel.Margin",
                "MoreButton.Margin",
                "ColorRepresentationComboBox.(Grid.Row)",
                "ColorTextInputPanels.(Grid.Row)",
                "HexTextBox.TabIndex",
                "HexTextBox.(Grid.Row)",
                "HexTextBox.(Grid.Column)",
                "HexTextBox.(Grid.ColumnSpan)",
                "HexTextBox.Margin",
                "HexTextBox.HorizontalAlignment",
                "HexTextBox.Width",
                "RgbTextLabelColumn.Width",
                "HsvTextLabelColumn.Width",
                "AlphaTextLabelColumn.Width");
        });
    }

    [TestMethod]
    public void ColorPickerTextEntryFollowsWinUISourceBehavior()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                Color = Color.FromArgb(0x80, 10, 20, 30),
                IsAlphaEnabled = true,
                IsMoreButtonVisible = true
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 560);

            var textEntryGrid = FindNamedDescendant<Grid>(colorPicker, "TextEntryGrid");
            var moreButton = FindNamedDescendant<ToggleButton>(colorPicker, "MoreButton");
            var redTextBox = FindNamedDescendant<TextBox>(colorPicker, "RedTextBox");
            var alphaTextBox = FindNamedDescendant<TextBox>(colorPicker, "AlphaTextBox");
            var hexTextBox = FindNamedDescendant<TextBox>(colorPicker, "HexTextBox");

            Assert.AreEqual(Visibility.Collapsed, textEntryGrid.Visibility);

            moreButton.IsChecked = true;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, textEntryGrid.Visibility);

            redTextBox.Text = "128";
            host.UpdateLayout();

            Assert.AreEqual(128, colorPicker.Color.R);

            alphaTextBox.Text = "25";
            host.UpdateLayout();

            Assert.AreEqual("25%", alphaTextBox.Text);
            AssertClose(0.25, colorPicker.Color.A / 255.0, 0.01);

            hexTextBox.Text = "40010203";
            host.UpdateLayout();

            Assert.AreEqual("#40010203", hexTextBox.Text);
            Assert.AreEqual(Color.FromArgb(0x40, 0x01, 0x02, 0x03), colorPicker.Color);
        });
    }

    [TestMethod]
    public void ColorPickerThirdDimensionSliderFollowsWinUISourceComponents()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorPicker = new ColorPickerControl
            {
                Color = Colors.Red
            };

            using var host = new TestWindowHost(colorPicker, width: 420, height: 560);
            var slider = FindNamedDescendant<ColorPickerSlider>(colorPicker, "ThirdDimensionSlider");
            var gradient = (LinearGradientBrush)FindNamedDescendant<Rectangle>(colorPicker, "ThirdDimensionBackgroundRectangle").Fill;

            Assert.AreEqual(ColorPickerHsvChannel.Value, slider.ColorChannel);
            Assert.AreEqual(100.0, slider.Value);
            Assert.AreEqual(2, gradient.GradientStops.Count);

            colorPicker.ColorSpectrumComponents = ColorSpectrumComponents.HueValue;
            host.UpdateLayout();

            Assert.AreEqual(ColorPickerHsvChannel.Saturation, slider.ColorChannel);
            Assert.AreEqual(100.0, slider.Value);

            colorPicker.ColorSpectrumComponents = ColorSpectrumComponents.ValueSaturation;
            host.UpdateLayout();

            Assert.AreEqual(ColorPickerHsvChannel.Hue, slider.ColorChannel);
            Assert.AreEqual(0.0, slider.Value);
            Assert.IsTrue(gradient.GradientStops.Count >= 2);
        });
    }

    [TestMethod]
    public void ColorSpectrumAutomationPeerExposesValuePattern()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var colorSpectrum = new ColorSpectrum
            {
                HsvColor = new Vector4(120, 0.5f, 0.75f, 1)
            };

            using var host = new TestWindowHost(colorSpectrum, width: 260, height: 260);
            var peer = UIElementAutomationPeer.CreatePeerForElement(colorSpectrum);

            Assert.IsInstanceOfType(peer, typeof(ColorSpectrumAutomationPeer));
            var valueProvider = (IValueProvider)peer.GetPattern(PatternInterface.Value);

            Assert.IsFalse(valueProvider.IsReadOnly);
            StringAssert.Contains(valueProvider.Value, "Hue 120");
            StringAssert.Contains(valueProvider.Value, "saturation 50");
            StringAssert.Contains(valueProvider.Value, "value 75");
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
            AssertDynamicResourceSetter(borderStyle, Shape.StrokeProperty, "ColorPickerBorderBrush");
            var strokeThicknessSetter = borderStyle.Setters.OfType<Setter>().Single(setter => setter.Property == Shape.StrokeThicknessProperty);
            Assert.AreEqual(2.0, strokeThicknessSetter.Value);

            var sliderThumbStyle = (Style)resources["ColorPickerSliderThumbStyle"];
            AssertDynamicResourceSetter(sliderThumbStyle, Control.BackgroundProperty, "ColorPickerSliderThumbBackground");

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

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");

        var dynamicResource = setter!.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static VisualStateEx AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] expectedTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        var state = group.States
            .OfType<VisualState>()
            .Single(candidate => candidate.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        CollectionAssert.AreEquivalent(expectedTargets, stateEx.Setters.Select(setter => setter.Target).ToArray());
        return stateEx;
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(candidate => candidate.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
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
