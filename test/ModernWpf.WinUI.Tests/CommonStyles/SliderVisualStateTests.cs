using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class SliderVisualStateTests
{
    [TestMethod]
    public void OfficialWpfFluentSliderStylesUseOfficialResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf;component/Styles/Slider.xaml", UriKind.Relative)
            };

            var sliderButtonStyle = (Style)resources["SliderButtonStyle"];
            AssertSetterValue(sliderButtonStyle, Control.IsTabStopProperty, false);
            AssertSetterValue(sliderButtonStyle, UIElement.FocusableProperty, false);
            AssertSetterValue(sliderButtonStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(sliderButtonStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            Assert.IsInstanceOfType(FindSetter(sliderButtonStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var sliderThumbStyle = (Style)resources["SliderThumbStyle"];
            AssertSetterValue(sliderThumbStyle, FrameworkElement.HeightProperty, 20d);
            AssertSetterValue(sliderThumbStyle, FrameworkElement.WidthProperty, 20d);
            AssertSetterValue(sliderThumbStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            AssertDynamicResourceSetter(sliderThumbStyle, Control.BorderBrushProperty, "SliderThumbBorderBrush");
            AssertDynamicResourceSetter(sliderThumbStyle, Control.BackgroundProperty, "SliderThumbBackground");
            AssertSetterValue(sliderThumbStyle, Control.BorderThicknessProperty, new Thickness(1));
            Assert.IsInstanceOfType(FindSetter(sliderThumbStyle, Control.TemplateProperty)?.Value, typeof(ControlTemplate));

            var horizontalTemplate = (ControlTemplate)resources["SliderHorizontal"];
            var verticalTemplate = (ControlTemplate)resources["SliderVertical"];
            var sliderStyle = (Style)resources["DefaultSliderStyle"];
            AssertDynamicResourceSetter(sliderStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertSetterValue(sliderStyle, Stylus.IsPressAndHoldEnabledProperty, false);
            AssertSetterValue(sliderStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(sliderStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            AssertStyleTriggerSetter(sliderStyle, Slider.OrientationProperty, Orientation.Horizontal, FrameworkElement.MinWidthProperty, 104d);
            AssertStyleTriggerSetter(sliderStyle, Slider.OrientationProperty, Orientation.Horizontal, Control.TemplateProperty, horizontalTemplate);
            AssertStyleTriggerSetter(sliderStyle, Slider.OrientationProperty, Orientation.Vertical, FrameworkElement.MinHeightProperty, 104d);
            AssertStyleTriggerSetter(sliderStyle, Slider.OrientationProperty, Orientation.Vertical, Control.TemplateProperty, verticalTemplate);

            var implicitSliderStyle = (Style)resources[typeof(Slider)];
            Assert.AreSame(sliderStyle, implicitSliderStyle.BasedOn);
        });
    }

    [TestMethod]
    public void HorizontalSliderUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var slider = CreateSlider(Orientation.Horizontal);
            using var host = new TestWindowHost(slider, width: 240, height: 100);
            host.UpdateLayout();

            AssertOfficialMetrics(slider);
            AssertTemplateTriggerShape(slider.Template);

            var root = slider.GetTemplateRoot();
            var topTick = FindTemplatePart<TickBar>(slider, "TopTick");
            var bottomTick = FindTemplatePart<TickBar>(slider, "BottomTick");
            var trackBackground = FindTemplatePart<Border>(slider, "TrackBackground");
            var track = FindTemplatePart<Track>(slider, "PART_Track");
            var thumb = FindTemplatePart<Thumb>(slider, "Thumb");
            var selectedRange = FindTemplatePart<Border>(slider, "PART_SelectedRange");
            var selectionRange = FindTemplatePart<Border>(slider, "PART_SelectionRange");

            Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(root).Count);
            AssertHorizontalTemplateUsesOfficialResources(slider, root, topTick, bottomTick, trackBackground, track, thumb, selectedRange, selectionRange);
            Assert.AreEqual(Visibility.Collapsed, topTick.Visibility);
            Assert.AreEqual(Visibility.Collapsed, bottomTick.Visibility);
            Assert.AreEqual(Visibility.Visible, selectedRange.Visibility);
            Assert.AreEqual(Visibility.Hidden, selectionRange.Visibility);

            slider.TickPlacement = TickPlacement.TopLeft;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, topTick.Visibility);
            Assert.AreEqual(Visibility.Collapsed, bottomTick.Visibility);

            slider.TickPlacement = TickPlacement.Both;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, topTick.Visibility);
            Assert.AreEqual(Visibility.Visible, bottomTick.Visibility);

            slider.IsSelectionRangeEnabled = true;
            slider.SelectionStart = slider.Minimum;
            slider.SelectionEnd = slider.Value;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, selectionRange.Visibility);
            Assert.AreEqual(Visibility.Hidden, selectedRange.Visibility);
            Assert.IsInstanceOfType(selectionRange.Parent, typeof(SliderRangeCanvas));
            Assert.IsTrue(selectionRange.ActualWidth > 0);

            AssertThumbStatesUseWpfNames(thumb);
        });
    }

    [TestMethod]
    public void VerticalSliderUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var slider = CreateSlider(Orientation.Vertical);
            using var host = new TestWindowHost(slider, width: 100, height: 240);
            host.UpdateLayout();

            AssertTemplateTriggerShape(slider.Template);

            var root = slider.GetTemplateRoot();
            var leftTick = FindTemplatePart<TickBar>(slider, "TopTick");
            var rightTick = FindTemplatePart<TickBar>(slider, "BottomTick");
            var trackBackground = FindTemplatePart<Border>(slider, "TrackBackground");
            var track = FindTemplatePart<Track>(slider, "PART_Track");
            var thumb = FindTemplatePart<Thumb>(slider, "Thumb");
            var selectedRange = FindTemplatePart<Border>(slider, "PART_SelectedRange");
            var selectionRange = FindTemplatePart<Border>(slider, "PART_SelectionRange");

            Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(root).Count);
            AssertVerticalTemplateUsesOfficialResources(slider, root, leftTick, rightTick, trackBackground, track, thumb, selectedRange, selectionRange);
            Assert.AreEqual(Visibility.Collapsed, leftTick.Visibility);
            Assert.AreEqual(Visibility.Collapsed, rightTick.Visibility);

            slider.TickPlacement = TickPlacement.BottomRight;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, leftTick.Visibility);
            Assert.AreEqual(Visibility.Visible, rightTick.Visibility);

            slider.IsSelectionRangeEnabled = true;
            slider.SelectionStart = slider.Minimum;
            slider.SelectionEnd = slider.Value;
            host.UpdateLayout();
            Assert.IsInstanceOfType(selectionRange.Parent, typeof(SliderRangeCanvas));
            Assert.IsTrue(selectionRange.ActualHeight > 0);

            AssertThumbStatesUseWpfNames(thumb);
        });
    }

    [TestMethod]
    public void SelectionRangeEnabledBeforeFirstLayoutRendersItsCurrentValue()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var slider = CreateSlider(Orientation.Horizontal);
            slider.IsSelectionRangeEnabled = true;
            slider.SelectionStart = slider.Minimum;
            slider.SelectionEnd = slider.Value;

            using var host = new TestWindowHost(slider, width: 240, height: 100);
            host.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
            host.UpdateLayout();

            var selectionRange = FindTemplatePart<Border>(slider, "PART_SelectionRange");
            Assert.IsInstanceOfType(selectionRange.Parent, typeof(SliderRangeCanvas));
            Assert.AreEqual(Visibility.Visible, selectionRange.Visibility);
            Assert.IsTrue(selectionRange.Width > 0);
            Assert.AreEqual(selectionRange.Width, selectionRange.ActualWidth, 0.51);

            var initialWidth = selectionRange.ActualWidth;
            slider.Value = 75;
            slider.SelectionEnd = slider.Value;
            host.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
            host.UpdateLayout();
            Assert.IsTrue(selectionRange.ActualWidth > initialWidth);
            Assert.AreEqual(selectionRange.Width, selectionRange.ActualWidth, 0.51);
        });
    }

    [TestMethod]
    public void TemplateDynamicResourcesCanBeOverriddenAtElementScope()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var trackBrush = new SolidColorBrush(Colors.Red);
            var updatedTrackBrush = new SolidColorBrush(Colors.DarkRed);
            var tickBrush = new SolidColorBrush(Colors.Blue);
            var thumbBrush = new SolidColorBrush(Colors.Green);
            var updatedThumbBrush = new SolidColorBrush(Colors.DarkGreen);
            var thumbBorderBrush = new SolidColorBrush(Colors.Yellow);
            var outerThumbBrush = new SolidColorBrush(Colors.Purple);
            var grid = new Grid
            {
                Resources =
                {
                    ["SliderTrackFill"] = trackBrush,
                    ["SliderTickBarFill"] = tickBrush,
                    ["SliderThumbBackground"] = thumbBrush,
                    ["SliderThumbBorderBrush"] = thumbBorderBrush,
                    ["SliderOuterThumbBackground"] = outerThumbBrush,
                    ["SliderTrackCornerRadius"] = new CornerRadius(5),
                    ["SliderThumbCornerRadius"] = new CornerRadius(11),
                    ["SliderInnerThumbWidth"] = 16d,
                    ["SliderInnerThumbHeight"] = 17d,
                },
                Children =
                {
                    CreateSlider(Orientation.Horizontal)
                }
            };

            var slider = (Slider)grid.Children[0];
            using var host = new TestWindowHost(grid, width: 240, height: 100);
            host.UpdateLayout();

            var topTick = FindTemplatePart<TickBar>(slider, "TopTick");
            var trackBackground = FindTemplatePart<Border>(slider, "TrackBackground");
            var selectedRange = FindTemplatePart<Border>(slider, "PART_SelectedRange");
            var thumb = FindTemplatePart<Thumb>(slider, "Thumb");
            thumb.ApplyTemplate();

            var outerThumb = (Border)thumb.GetTemplateRoot();
            var innerThumb = FindNamedDescendant<Ellipse>(thumb, "SliderInnerThumb");

            Assert.AreSame(trackBrush, trackBackground.Background);
            Assert.AreSame(tickBrush, topTick.Fill);
            Assert.AreSame(thumbBrush, selectedRange.Background);
            Assert.AreSame(thumbBrush, thumb.Background);
            Assert.AreSame(thumbBorderBrush, thumb.BorderBrush);
            Assert.AreSame(outerThumbBrush, outerThumb.Background);
            Assert.AreEqual(new CornerRadius(5), trackBackground.CornerRadius);
            Assert.AreEqual(new CornerRadius(11), outerThumb.CornerRadius);
            Assert.AreEqual(16d, innerThumb.Width);
            Assert.AreEqual(17d, innerThumb.Height);
            Assert.AreSame(thumb.Background, innerThumb.Fill);

            grid.Resources["SliderTrackFill"] = updatedTrackBrush;
            grid.Resources["SliderThumbBackground"] = updatedThumbBrush;
            host.UpdateLayout();

            Assert.AreSame(updatedTrackBrush, trackBackground.Background);
            Assert.AreSame(updatedThumbBrush, selectedRange.Background);
            Assert.AreSame(updatedThumbBrush, thumb.Background);
            Assert.AreSame(thumb.Background, innerThumb.Fill);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2SliderHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "SliderOutsideTickBarThemeHeight", 4.0);
                AssertThemeResourceValue(themeName, "SliderTrackThemeHeight", 4.0);
                AssertThemeResourceValue(themeName, "SliderBorderThemeThickness", new Thickness(0));
                AssertThemeResourceValue(themeName, "SliderHeaderThemeMargin", new Thickness(0, 0, 0, 4));
                AssertThemeResourceValue(themeName, "SliderHeaderThemeFontWeight", FontWeights.Normal);
                AssertThemeResourceReference(themeName, "SliderContainerBackground", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "SliderContainerBackgroundPointerOver", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "SliderContainerBackgroundPressed", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "SliderContainerBackgroundDisabled", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "SliderThumbBackground", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderThumbBackgroundPointerOver", "AccentFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SliderThumbBackgroundPressed", "AccentFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "SliderThumbBackgroundDisabled", "AccentFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SliderThumbBorderBrush", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "SliderOuterThumbBackground", "ControlSolidFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderTrackFill", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderTrackFillPointerOver", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderTrackFillPressed", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderTrackFillDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SliderTrackValueFill", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderTrackValueFillPointerOver", "AccentFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "SliderTrackValueFillPressed", "AccentFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "SliderTrackValueFillDisabled", "AccentFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SliderHeaderForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "SliderHeaderForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SliderTickBarFill", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "SliderTickBarFillDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "SliderInlineTickBarFill", "ControlFillColorInputActiveBrush");
            }

            AssertThemeResourceValue("HighContrast", "SliderOutsideTickBarThemeHeight", 4.0);
            AssertThemeResourceValue("HighContrast", "SliderTrackThemeHeight", 2.0);
            AssertThemeResourceValue("HighContrast", "SliderBorderThemeThickness", new Thickness(1));
            AssertThemeResourceValue("HighContrast", "SliderHeaderThemeMargin", new Thickness(0, 0, 0, 4));
            AssertThemeResourceValue("HighContrast", "SliderHeaderThemeFontWeight", FontWeights.Normal);
            AssertThemeResourceReference("HighContrast", "SliderContainerBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "SliderContainerBackgroundPointerOver", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "SliderContainerBackgroundPressed", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "SliderContainerBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "SliderThumbBackground", "SystemControlForegroundAccentBrush");
            AssertThemeResourceReference("HighContrast", "SliderThumbBackgroundPointerOver", "SystemAccentColorLight1Brush");
            AssertThemeResourceReference("HighContrast", "SliderThumbBackgroundPressed", "SystemAccentColorDark1Brush");
            AssertThemeResourceReference("HighContrast", "SliderThumbBackgroundDisabled", "SystemControlDisabledChromeDisabledHighBrush");
            AssertThemeResourceReference("HighContrast", "SliderThumbBorderBrush", "SystemControlForegroundAccentBrush");
            AssertThemeResourceReference("HighContrast", "SliderOuterThumbBackground", "SystemControlForegroundAccentBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackFill", "SystemControlForegroundBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackFillPointerOver", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackFillPressed", "SystemControlForegroundBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackFillDisabled", "SystemControlDisabledChromeDisabledHighBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackValueFill", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackValueFillPointerOver", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackValueFillPressed", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "SliderTrackValueFillDisabled", "SystemControlDisabledChromeDisabledHighBrush");
            AssertThemeResourceReference("HighContrast", "SliderHeaderForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "SliderHeaderForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "SliderTickBarFill", "SystemControlForegroundBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "SliderTickBarFillDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "SliderInlineTickBarFill", "SystemControlBackgroundAltHighBrush");
        });
    }

    private static Slider CreateSlider(Orientation orientation)
    {
        return new Slider
        {
            Orientation = orientation,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
    }

    private static void AssertOfficialMetrics(Slider slider)
    {
        Assert.AreEqual(14.0, slider.TryFindResource("SliderPreContentMargin"));
        Assert.AreEqual(14.0, slider.TryFindResource("SliderPostContentMargin"));
        Assert.AreEqual(20.0, slider.TryFindResource("SliderHorizontalThumbWidth"));
        Assert.AreEqual(20.0, slider.TryFindResource("SliderHorizontalThumbHeight"));
        Assert.AreEqual(20.0, slider.TryFindResource("SliderVerticalThumbWidth"));
        Assert.AreEqual(20.0, slider.TryFindResource("SliderVerticalThumbHeight"));
    }

    private static void AssertHorizontalTemplateUsesOfficialResources(
        Slider slider,
        FrameworkElement root,
        TickBar topTick,
        TickBar bottomTick,
        Border trackBackground,
        Track track,
        Thumb thumb,
        Border selectedRange,
        Border selectionRange)
    {
        Assert.AreEqual(slider.Padding, root.Margin);
        Assert.AreEqual(slider.TryFindResource("SliderHorizontalHeight"), root.MinHeight);
        Assert.IsTrue(root.SnapsToDevicePixels);
        AssertTickBarUsesOfficialResources(slider, topTick, TickBarPlacement.Top, height: 6d);
        AssertTickBarUsesOfficialResources(slider, bottomTick, TickBarPlacement.Bottom, height: 6d);
        AssertTrackUsesOfficialResources(slider, trackBackground, height: 4d, width: double.NaN);
        AssertRangeUsesOfficialResources(slider, selectedRange, height: 4d, width: double.NaN);
        AssertRangeUsesOfficialResources(slider, selectionRange, height: 4d, width: double.NaN);
        AssertTrackButtonsUseOfficialResources(slider, track);
        AssertThumbUsesOfficialResources(thumb);
    }

    private static void AssertVerticalTemplateUsesOfficialResources(
        Slider slider,
        FrameworkElement root,
        TickBar leftTick,
        TickBar rightTick,
        Border trackBackground,
        Track track,
        Thumb thumb,
        Border selectedRange,
        Border selectionRange)
    {
        Assert.AreEqual(slider.Padding, root.Margin);
        Assert.AreEqual(slider.TryFindResource("SliderVerticalWidth"), root.MinWidth);
        Assert.IsTrue(root.SnapsToDevicePixels);
        AssertTickBarUsesOfficialResources(slider, leftTick, TickBarPlacement.Left, width: 6d);
        AssertTickBarUsesOfficialResources(slider, rightTick, TickBarPlacement.Right, width: 6d);
        AssertTrackUsesOfficialResources(slider, trackBackground, height: double.NaN, width: 4d);
        AssertRangeUsesOfficialResources(slider, selectedRange, height: double.NaN, width: 4d);
        AssertRangeUsesOfficialResources(slider, selectionRange, height: double.NaN, width: 4d);
        AssertTrackButtonsUseOfficialResources(slider, track);
        AssertThumbUsesOfficialResources(thumb);
    }

    private static void AssertTickBarUsesOfficialResources(Slider slider, TickBar tickBar, TickBarPlacement placement, double height = double.NaN, double width = double.NaN)
    {
        Assert.AreSame(slider.TryFindResource("SliderTickBarFill"), tickBar.Fill);
        Assert.AreEqual(placement, tickBar.Placement);
        Assert.IsTrue(tickBar.SnapsToDevicePixels);

        if (!double.IsNaN(height))
        {
            Assert.AreEqual(height, tickBar.Height);
        }

        if (!double.IsNaN(width))
        {
            Assert.AreEqual(width, tickBar.Width);
        }
    }

    private static void AssertTrackUsesOfficialResources(Slider slider, Border trackBackground, double height, double width)
    {
        Assert.AreSame(slider.TryFindResource("SliderTrackFill"), trackBackground.Background);
        Assert.AreEqual(new Thickness(0), trackBackground.BorderThickness);
        Assert.AreEqual(slider.TryFindResource("SliderTrackCornerRadius"), trackBackground.CornerRadius);

        if (!double.IsNaN(height))
        {
            Assert.AreEqual(height, trackBackground.Height);
        }

        if (!double.IsNaN(width))
        {
            Assert.AreEqual(width, trackBackground.Width);
        }
    }

    private static void AssertRangeUsesOfficialResources(Slider slider, Border range, double height, double width)
    {
        Assert.AreSame(slider.TryFindResource("SliderThumbBackground"), range.Background);
        Assert.AreEqual(new Thickness(0), range.BorderThickness);
        Assert.AreEqual(slider.TryFindResource("SliderTrackCornerRadius"), range.CornerRadius);

        if (!double.IsNaN(height))
        {
            Assert.AreEqual(height, range.Height);
        }

        if (!double.IsNaN(width))
        {
            Assert.AreEqual(width, range.Width);
        }
    }

    private static void AssertTrackButtonsUseOfficialResources(Slider slider, Track track)
    {
        Assert.AreSame(slider.TryFindResource("SliderButtonStyle"), track.DecreaseRepeatButton.Style);
        Assert.AreSame(slider.TryFindResource("SliderButtonStyle"), track.IncreaseRepeatButton.Style);
        Assert.AreEqual(Slider.DecreaseLarge, track.DecreaseRepeatButton.Command);
        Assert.AreEqual(Slider.IncreaseLarge, track.IncreaseRepeatButton.Command);
    }

    private static void AssertThumbUsesOfficialResources(Thumb thumb)
    {
        Assert.AreSame(thumb.TryFindResource("SliderThumbStyle"), thumb.Style);
        Assert.AreEqual(20d, thumb.Height);
        Assert.AreEqual(20d, thumb.Width);
        Assert.IsTrue(thumb.OverridesDefaultStyle);
        Assert.AreSame(thumb.TryFindResource("SliderThumbBorderBrush"), thumb.BorderBrush);
        Assert.AreSame(thumb.TryFindResource("SliderThumbBackground"), thumb.Background);
        Assert.AreEqual(new Thickness(1), thumb.BorderThickness);

        thumb.ApplyTemplate();

        var outerThumb = (Border)thumb.GetTemplateRoot();
        Assert.AreSame(thumb.TryFindResource("SliderOuterThumbBackground"), outerThumb.Background);
        Assert.AreSame(thumb.BorderBrush, outerThumb.BorderBrush);
        Assert.AreEqual(thumb.BorderThickness, outerThumb.BorderThickness);
        Assert.AreEqual(thumb.TryFindResource("SliderThumbCornerRadius"), outerThumb.CornerRadius);

        var innerThumb = FindNamedDescendant<Ellipse>(thumb, "SliderInnerThumb");
        Assert.AreEqual(thumb.TryFindResource("SliderInnerThumbWidth"), innerThumb.Width);
        Assert.AreEqual(thumb.TryFindResource("SliderInnerThumbHeight"), innerThumb.Height);
        Assert.AreSame(thumb.Background, innerThumb.Fill);
    }

    private static void AssertTemplateTriggerShape(ControlTemplate template)
    {
        Assert.AreEqual(6, template.Triggers.Count);
        AssertTriggerSetter(template, Slider.TickPlacementProperty, TickPlacement.TopLeft, "TopTick", "Visibility");
        AssertTriggerSetter(template, Slider.TickPlacementProperty, TickPlacement.BottomRight, "BottomTick", "Visibility");
        AssertTriggerSetter(template, Slider.TickPlacementProperty, TickPlacement.Both, "TopTick", "Visibility");
        AssertTriggerSetter(template, Slider.TickPlacementProperty, TickPlacement.Both, "BottomTick", "Visibility");
        AssertTriggerSetter(template, UIElement.IsMouseOverProperty, true, "TrackBackground", "Background");
        AssertTriggerSetter(template, UIElement.IsMouseOverProperty, true, "Thumb", "Foreground");
        AssertTriggerSetter(template, Slider.IsSelectionRangeEnabledProperty, true, "PART_SelectionRange", "Visibility");
        AssertTriggerSetter(template, Slider.IsSelectionRangeEnabledProperty, false, "PART_SelectedRange", "Visibility");
    }

    private static void AssertTriggerSetter(ControlTemplate template, DependencyProperty property, object value, string targetName, string propertyName)
    {
        var trigger = template.Triggers
            .OfType<Trigger>()
            .Single(item => item.Property == property && Equals(item.Value, value));

        Assert.IsTrue(
            trigger.Setters
                .OfType<Setter>()
                .Any(item => item.TargetName == targetName && item.Property.Name == propertyName),
            $"Expected trigger {property.Name}={value} to set {targetName}.{propertyName}.");
    }

    private static void AssertStyleTriggerSetter(Style style, DependencyProperty triggerProperty, object triggerValue, DependencyProperty setterProperty, object expectedValue)
    {
        var trigger = style.Triggers
            .OfType<Trigger>()
            .Single(item => item.Property == triggerProperty && Equals(item.Value, triggerValue));
        var setter = trigger.Setters
            .OfType<Setter>()
            .Single(item => item.Property == setterProperty);

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertThumbStatesUseWpfNames(Thumb thumb)
    {
        thumb.ApplyTemplate();
        var root = thumb.GetTemplateRoot();
        var group = VisualStateManager.GetVisualStateGroups(root)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");

        var stateNames = group.States
            .Cast<VisualState>()
            .Select(item => item.Name)
            .ToArray();

        CollectionAssert.AreEquivalent(new[] { "Normal", "MouseOver", "Pressed" }, stateNames);
        CollectionAssert.DoesNotContain(stateNames, "PointerOver");
        CollectionAssert.DoesNotContain(stateNames, "Disabled");
    }

    private static T FindTemplatePart<T>(Slider slider, string name)
        where T : DependencyObject
    {
        var part = slider.Template.FindName(name, slider) as T;
        if (part == null)
        {
            throw new AssertFailedException($"Expected Slider template part '{name}'.");
        }

        return part;
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

        throw new AssertFailedException($"Expected descendant '{name}'.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = FindSetter(style, property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static Setter? FindSetter(Style style, DependencyProperty property)
    {
        for (var current = style; current != null; current = current.BasedOn)
        {
            var setter = current.Setters
                .OfType<Setter>()
                .SingleOrDefault(item => item.Property == property);

            if (setter != null)
            {
                return setter;
            }
        }

        return null;
    }

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, object resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
