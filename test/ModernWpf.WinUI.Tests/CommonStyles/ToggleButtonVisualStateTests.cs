using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ToggleButtonVisualStateTests
{
    [TestMethod]
    public void DefaultToggleButtonStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultToggleButtonStyle");
            var implicitToggleButtonStyle = (Style)Application.Current.FindResource(typeof(ToggleButton));
            Assert.AreEqual(typeof(ToggleButton), defaultStyle.TargetType);
            Assert.AreEqual(typeof(ToggleButton), implicitToggleButtonStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitToggleButtonStyle.BasedOn);

            var toggleButton = CreateToggleButton();
            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);

            Assert.AreEqual((Thickness)Application.Current.FindResource("ToggleButtonPadding"), toggleButton.Padding);
            Assert.AreEqual((Thickness)Application.Current.FindResource("ToggleButtonBorderThemeThickness"), toggleButton.BorderThickness);
            AssertTemplateUsesOfficialWpfPresenter(toggleButton);
            Assert.IsFalse(ToggleButtonHelper.GetVisualStateSettersEnabled(toggleButton));
            AssertOfficialTriggerShape(toggleButton.Template);
            AssertUncheckedDisabledTriggerAppliesResources(toggleButton);
        });
    }

    [TestMethod]
    public void ToggleButtonStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultToggleButtonStyle");
            var implicitToggleButtonStyle = (Style)Application.Current.FindResource(typeof(ToggleButton));
            var toggleButton = CreateToggleButton();

            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);
            host.UpdateLayout();

            Assert.AreSame(defaultStyle, implicitToggleButtonStyle.BasedOn);
            AssertDynamicResourceSetter(defaultStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(defaultStyle, Control.BackgroundProperty, "ToggleButtonBackground");
            AssertDynamicResourceSetter(defaultStyle, Control.ForegroundProperty, "ToggleButtonForeground");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "ToggleButtonBorderBrush");
            AssertSetterValue(defaultStyle, Control.BorderThicknessProperty, new Thickness(1));
            AssertSetterValue(defaultStyle, Control.PaddingProperty, new Thickness(11, 5, 11, 6));
            AssertSetterValue(defaultStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            AssertSetterValue(defaultStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
            AssertSetterValue(defaultStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(defaultStyle, Control.FontWeightProperty, FontWeights.Normal);
            AssertDynamicResourceSetter(defaultStyle, FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertSetterValue(defaultStyle, FocusVisualHelper.FocusVisualMarginProperty, new Thickness(-3));
            AssertDynamicResourceSetter(defaultStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetterValue(defaultStyle, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetterValue(defaultStyle, FrameworkElement.OverridesDefaultStyleProperty, true);
            AssertSetterValue(defaultStyle, Stylus.IsPressAndHoldEnabledProperty, false);

            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBackground"), toggleButton.Background);
            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonForeground"), toggleButton.Foreground);
            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBorderBrush"), toggleButton.BorderBrush);
            Assert.AreEqual(toggleButton.TryFindResource("ToggleButtonBorderThemeThickness"), toggleButton.BorderThickness);
            Assert.AreEqual(toggleButton.TryFindResource("ToggleButtonPadding"), toggleButton.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, toggleButton.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, toggleButton.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Center, toggleButton.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, toggleButton.VerticalContentAlignment);
            Assert.AreEqual(FontWeights.Normal, toggleButton.FontWeight);
            Assert.AreEqual(toggleButton.TryFindResource("UseSystemFocusVisuals"), FocusVisualHelper.GetUseSystemFocusVisuals(toggleButton));
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(toggleButton));
            Assert.AreEqual(toggleButton.TryFindResource("ControlCornerRadius"), toggleButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));
            Assert.IsTrue(toggleButton.SnapsToDevicePixels);
            Assert.IsTrue(toggleButton.OverridesDefaultStyle);
            Assert.IsFalse(Stylus.GetIsPressAndHoldEnabled(toggleButton));
            Assert.IsFalse(ToggleButtonHelper.GetVisualStateSettersEnabled(toggleButton));

            var contentBorder = GetTemplateChild<Border>(toggleButton, "ContentBorder");
            var contentPresenter = GetTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

            Assert.AreEqual(toggleButton.Width, contentBorder.Width);
            Assert.AreEqual(toggleButton.Height, contentBorder.Height);
            Assert.AreEqual(toggleButton.Padding, contentBorder.Padding);
            Assert.AreEqual(toggleButton.HorizontalAlignment, contentBorder.HorizontalAlignment);
            Assert.AreEqual(toggleButton.VerticalAlignment, contentBorder.VerticalAlignment);
            Assert.AreSame(toggleButton.Background, contentBorder.Background);
            Assert.AreSame(toggleButton.BorderBrush, contentBorder.BorderBrush);
            Assert.AreEqual(toggleButton.BorderThickness, contentBorder.BorderThickness);
            Assert.AreEqual(toggleButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), contentBorder.CornerRadius);
            Assert.AreEqual(toggleButton.Content, contentPresenter.Content);
            Assert.IsTrue(contentPresenter.RecognizesAccessKey);
            Assert.AreEqual(toggleButton.HorizontalContentAlignment, contentPresenter.HorizontalAlignment);
            Assert.AreEqual(toggleButton.VerticalContentAlignment, contentPresenter.VerticalAlignment);
            Assert.AreSame(toggleButton.Foreground, TextElement.GetForeground(contentPresenter));
            Assert.AreEqual(toggleButton.FontSize, TextElement.GetFontSize(contentPresenter));
        });
    }

    [TestMethod]
    public void CheckedStateUsesOfficialWpfFluentResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleButton = CreateToggleButton();
            toggleButton.IsChecked = true;
            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);
            host.UpdateLayout();

            var contentBorder = GetTemplateChild<Border>(toggleButton, "ContentBorder");
            var contentPresenter = GetTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBackgroundChecked"), toggleButton.Background);
            Assert.AreSame(toggleButton.TryFindResource("AccentControlElevationBorderBrush"), toggleButton.BorderBrush);
            Assert.AreSame(contentPresenter.TryFindResource("ToggleButtonForegroundChecked"), TextElement.GetForeground(contentPresenter));
            Assert.AreSame(toggleButton.Background, contentBorder.Background);
            Assert.AreSame(toggleButton.BorderBrush, contentBorder.BorderBrush);

            toggleButton.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBackgroundCheckedDisabled"), toggleButton.Background);
            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBorderBrushCheckedDisabled"), toggleButton.BorderBrush);
            Assert.AreSame(contentPresenter.TryFindResource("ToggleButtonForegroundCheckedDisabled"), TextElement.GetForeground(contentPresenter));
        });
    }

    [TestMethod]
    public void IndeterminateStateUsesOfficialWpfFluentFallbackChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleButton = CreateToggleButton();
            toggleButton.IsThreeState = true;
            toggleButton.IsChecked = null;
            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);
            host.UpdateLayout();

            var contentBorder = GetTemplateChild<Border>(toggleButton, "ContentBorder");
            var contentPresenter = GetTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBackground"), toggleButton.Background);
            Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBorderBrush"), toggleButton.BorderBrush);
            Assert.AreSame(toggleButton.Foreground, TextElement.GetForeground(contentPresenter));
            Assert.AreSame(toggleButton.Background, contentBorder.Background);
            Assert.AreSame(toggleButton.BorderBrush, contentBorder.BorderBrush);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2ToggleButtonHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "ToggleButtonBorderThemeThickness", new Thickness(1));
                AssertLightDarkToggleButtonTheme(themeName);
            }

            AssertThemeResourceValue("HighContrast", "ToggleButtonBorderThemeThickness", new Thickness(1));

            AssertThemeResourceReference("HighContrast", "ToggleButtonBackground", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundDisabled", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundChecked", "SystemControlHighlightAccentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundCheckedPointerOver", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundCheckedPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundCheckedDisabled", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundIndeterminate", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundIndeterminatePointerOver", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundIndeterminatePressed", "SystemControlBackgroundBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBackgroundIndeterminateDisabled", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundPointerOver", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundPressed", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundChecked", "SystemControlHighlightAltChromeWhiteBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundCheckedPointerOver", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundCheckedPressed", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundCheckedDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundIndeterminate", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundIndeterminatePointerOver", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundIndeterminatePressed", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonForegroundIndeterminateDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrush", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushPointerOver", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushDisabled", "SystemControlDisabledTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushChecked", "SystemControlHighlightAltTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushCheckedPointerOver", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushCheckedPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushCheckedDisabled", "SystemControlDisabledTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushIndeterminate", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushIndeterminatePointerOver", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushIndeterminatePressed", "SystemControlHighlightTransparentBrush");
            AssertThemeResourceReference("HighContrast", "ToggleButtonBorderBrushIndeterminateDisabled", "SystemControlDisabledTransparentBrush");
        });
    }

    private static ToggleButton CreateToggleButton()
    {
        return new ToggleButton
        {
            Width = 100,
            Height = 40,
            Content = "Toggle"
        };
    }

    private static void AssertTemplateUsesOfficialWpfPresenter(ToggleButton toggleButton)
    {
        toggleButton.ApplyTemplate();

        var contentBorder = GetTemplateChild<Border>(toggleButton, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

        Assert.AreEqual(toggleButton.Content, contentPresenter.Content);
        Assert.IsTrue(contentPresenter.RecognizesAccessKey);
        Assert.AreSame(toggleButton.Foreground, TextElement.GetForeground(contentPresenter));
        Assert.AreEqual(toggleButton.FontSize, TextElement.GetFontSize(contentPresenter));
        Assert.AreEqual(((CornerRadius)toggleButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), contentBorder.CornerRadius);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(contentBorder).Count);
    }

    private static void AssertOfficialTriggerShape(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<MultiTrigger>().ToArray();
        Assert.AreEqual(7, triggers.Length);

        AssertTrigger(triggers,
            ("IsEnabled", false),
            ("IsChecked", false),
            ("", "Background", "ToggleButtonBackgroundDisabled"),
            ("", "BorderBrush", "ToggleButtonBorderBrushDisabled"),
            ("ContentPresenter", "Foreground", "ToggleButtonForegroundDisabled"));

        AssertTrigger(triggers,
            ("IsEnabled", false),
            ("IsChecked", true),
            ("", "Background", "ToggleButtonBackgroundCheckedDisabled"),
            ("", "BorderBrush", "ToggleButtonBorderBrushCheckedDisabled"),
            ("ContentPresenter", "Foreground", "ToggleButtonForegroundCheckedDisabled"));

        AssertTrigger(triggers,
            ("IsEnabled", true),
            ("IsChecked", true),
            ("", "Background", "ToggleButtonBackgroundChecked"),
            ("", "BorderBrush", "AccentControlElevationBorderBrush"),
            ("ContentPresenter", "Foreground", "ToggleButtonForegroundChecked"));

        AssertTrigger(triggers,
            ("IsMouseOver", true),
            ("IsChecked", false),
            ("", "Background", "ToggleButtonBackgroundPointerOver"));

        AssertTrigger(triggers,
            ("IsMouseOver", true),
            ("IsChecked", true),
            ("", "Background", "ToggleButtonBackgroundCheckedPointerOver"));

        AssertTrigger(triggers,
            ("IsPressed", true),
            ("IsChecked", false),
            ("", "Background", "ToggleButtonBackgroundPressed"),
            ("", "BorderBrush", "ToggleButtonBorderBrushPressed"),
            ("ContentPresenter", "Foreground", "ToggleButtonForegroundPressed"));

        AssertTrigger(triggers,
            ("IsPressed", true),
            ("IsChecked", true),
            ("", "Background", "ToggleButtonBackgroundCheckedPressed"),
            ("", "BorderBrush", "ToggleButtonBorderBrushCheckedPressed"),
            ("ContentPresenter", "Foreground", "ToggleButtonForegroundCheckedPressed"));
    }

    private static void AssertTrigger(
        MultiTrigger[] triggers,
        (string PropertyName, object Value) firstCondition,
        (string PropertyName, object Value) secondCondition,
        params (string TargetName, string PropertyName, string ResourceKey)[] expectedSetters)
    {
        var trigger = triggers.Single(item =>
            HasCondition(item, firstCondition.PropertyName, firstCondition.Value) &&
            HasCondition(item, secondCondition.PropertyName, secondCondition.Value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expectedSetter in expectedSetters)
        {
            AssertSetter(setters, expectedSetter.TargetName, expectedSetter.PropertyName, expectedSetter.ResourceKey);
        }
    }

    private static bool HasCondition(MultiTrigger trigger, string propertyName, object value)
    {
        return trigger.Conditions.Cast<Condition>().Any(item =>
            item.Property.Name == propertyName &&
            Equals(item.Value, value));
    }

    private static void AssertSetter(Setter[] setters, string targetName, string propertyName, string resourceKey)
    {
        var setter = setters.Single(item =>
            (item.TargetName ?? string.Empty) == targetName &&
            item.Property.Name == propertyName);

        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var resource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, resource.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.AreEqual(expectedValue, setter!.Value);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertUncheckedDisabledTriggerAppliesResources(ToggleButton toggleButton)
    {
        var contentBorder = GetTemplateChild<Border>(toggleButton, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

        toggleButton.IsEnabled = false;
        toggleButton.UpdateLayout();

        Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBackgroundDisabled"), toggleButton.Background);
        Assert.AreSame(toggleButton.TryFindResource("ToggleButtonBorderBrushDisabled"), toggleButton.BorderBrush);
        Assert.AreSame(contentPresenter.TryFindResource("ToggleButtonForegroundDisabled"), TextElement.GetForeground(contentPresenter));
        Assert.AreSame(toggleButton.Background, contentBorder.Background);
        Assert.AreSame(toggleButton.BorderBrush, contentBorder.BorderBrush);
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }

    private static void AssertLightDarkToggleButtonTheme(string themeName)
    {
        AssertThemeResourceReference(themeName, "ToggleButtonBackground", "ControlFillColorDefaultBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundPointerOver", "ControlFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundPressed", "ControlFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundDisabled", "ControlFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundChecked", "AccentFillColorDefaultBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundCheckedPointerOver", "AccentFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundCheckedPressed", "AccentFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundCheckedDisabled", "AccentFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundIndeterminate", "ControlFillColorDefaultBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundIndeterminatePointerOver", "ControlFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundIndeterminatePressed", "ControlFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBackgroundIndeterminateDisabled", "ControlFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundPressed", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundChecked", "TextOnAccentFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundCheckedPointerOver", "TextOnAccentFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundCheckedPressed", "TextOnAccentFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundCheckedDisabled", "TextOnAccentFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundIndeterminate", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundIndeterminatePointerOver", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundIndeterminatePressed", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonForegroundIndeterminateDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrush", "ControlElevationBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushPointerOver", "ControlElevationBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushDisabled", "ControlStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushChecked", "AccentControlElevationBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushCheckedPointerOver", "AccentControlElevationBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushCheckedPressed", "ControlFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushCheckedDisabled", "ControlFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushIndeterminate", "ControlElevationBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushIndeterminatePointerOver", "ControlElevationBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushIndeterminatePressed", "ControlStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "ToggleButtonBorderBrushIndeterminateDisabled", "ControlStrokeColorDefaultBrush");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
