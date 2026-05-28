using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class SliderVisualStateTests
{
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
            var thumb = FindTemplatePart<Thumb>(slider, "Thumb");
            var selectedRange = FindTemplatePart<Border>(slider, "PART_SelectedRange");
            var selectionRange = FindTemplatePart<Border>(slider, "PART_SelectionRange");

            Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(root).Count);
            Assert.AreSame(slider.TryFindResource("SliderTrackFill"), trackBackground.Background);
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
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, selectionRange.Visibility);
            Assert.AreEqual(Visibility.Hidden, selectedRange.Visibility);

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
            var thumb = FindTemplatePart<Thumb>(slider, "Thumb");

            Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(root).Count);
            Assert.AreSame(slider.TryFindResource("SliderTrackFill"), trackBackground.Background);
            Assert.AreEqual(Visibility.Collapsed, leftTick.Visibility);
            Assert.AreEqual(Visibility.Collapsed, rightTick.Visibility);

            slider.TickPlacement = TickPlacement.BottomRight;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, leftTick.Visibility);
            Assert.AreEqual(Visibility.Visible, rightTick.Visibility);

            AssertThumbStatesUseWpfNames(thumb);
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
