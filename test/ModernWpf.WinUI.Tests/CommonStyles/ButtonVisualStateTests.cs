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
public class ButtonVisualStateTests
{
    [TestMethod]
    public void DefaultButtonStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultButtonStyle");
            var implicitButtonStyle = (Style)Application.Current.FindResource(typeof(Button));
            Assert.AreEqual(typeof(ButtonBase), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Button), implicitButtonStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitButtonStyle.BasedOn);

            var button = CreateButton("Default");
            using var host = new TestWindowHost(button, width: 140, height: 80);

            AssertTemplateUsesOfficialWpfPresenter(button, recognizesAccessKey: true);
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            AssertOfficialTriggerShape(
                button.Template,
                "ButtonBackgroundPointerOver",
                "ButtonBorderBrushPointerOver",
                "ButtonForegroundPointerOver",
                "ButtonBackgroundPressed",
                "ButtonBorderBrushPressed",
                "ButtonForegroundPressed",
                "ButtonBackgroundDisabled",
                "ButtonBorderBrushDisabled",
                "ButtonForegroundDisabled");
            AssertDisabledTriggerAppliesResources(button, "ButtonBackgroundDisabled", "ButtonBorderBrushDisabled", "ButtonForegroundDisabled");
        });
    }

    [TestMethod]
    public void AccentButtonStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = (Style)Application.Current.FindResource("AccentButtonStyle");
            Assert.AreEqual(typeof(Button), style.TargetType);
            Assert.IsNull(style.BasedOn);

            var button = CreateButton("Accent");
            button.Style = style;
            using var host = new TestWindowHost(button, width: 140, height: 80);

            AssertTemplateUsesOfficialWpfPresenter(button, recognizesAccessKey: false);
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            AssertOfficialTriggerShape(
                button.Template,
                "AccentButtonBackgroundPointerOver",
                "AccentButtonBorderBrushPointerOver",
                "AccentButtonForegroundPointerOver",
                "AccentButtonBackgroundPressed",
                "AccentButtonBorderBrushPressed",
                "AccentButtonForegroundPressed",
                "AccentButtonBackgroundDisabled",
                "AccentButtonBorderBrushDisabled",
                "AccentButtonForegroundDisabled");
            AssertDisabledTriggerAppliesResources(button, "AccentButtonBackgroundDisabled", "AccentButtonBorderBrushDisabled", "AccentButtonForegroundDisabled");
        });
    }

    [TestMethod]
    public void SubtleButtonStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = (Style)Application.Current.FindResource("SubtleButtonStyle");
            Assert.AreEqual(typeof(Button), style.TargetType);
            Assert.IsNull(style.BasedOn);

            var button = CreateButton("Subtle");
            button.Style = style;
            using var host = new TestWindowHost(button, width: 140, height: 80);

            AssertTemplateUsesOfficialWpfPresenter(button, recognizesAccessKey: true);
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            AssertOfficialTriggerShape(
                button.Template,
                "SubtleButtonBackgroundPointerOver",
                "SubtleButtonBorderBrushPointerOver",
                "SubtleButtonForegroundPointerOver",
                "SubtleButtonBackgroundPressed",
                "SubtleButtonBorderBrushPressed",
                "SubtleButtonForegroundPressed",
                "SubtleButtonBackgroundDisabled",
                "SubtleButtonBorderBrushDisabled",
                "SubtleButtonForegroundDisabled");
            AssertDisabledTriggerAppliesResources(button, "SubtleButtonBackgroundDisabled", "SubtleButtonBorderBrushDisabled", "SubtleButtonForegroundDisabled");
        });
    }

    [TestMethod]
    public void ButtonStylesUseWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultButtonStyle");
            var implicitButtonStyle = (Style)Application.Current.FindResource(typeof(Button));
            var accentStyle = (Style)Application.Current.FindResource("AccentButtonStyle");
            var subtleStyle = (Style)Application.Current.FindResource("SubtleButtonStyle");

            AssertButtonStyleSetters(defaultStyle, "Button");
            AssertButtonStyleSetters(accentStyle, "AccentButton");
            AssertButtonStyleSetters(subtleStyle, "SubtleButton");

            var defaultButton = CreateButton("Default");
            var accentButton = CreateButton("Accent");
            var subtleButton = CreateButton("Subtle");
            accentButton.Style = accentStyle;
            subtleButton.Style = subtleStyle;

            var panel = new StackPanel();
            panel.Children.Add(defaultButton);
            panel.Children.Add(accentButton);
            panel.Children.Add(subtleButton);

            using var host = new TestWindowHost(panel, width: 360, height: 180);
            host.UpdateLayout();

            Assert.AreSame(defaultStyle, implicitButtonStyle.BasedOn);
            AssertButtonLiveResources(defaultButton, "Button", recognizesAccessKey: true);
            AssertButtonLiveResources(accentButton, "AccentButton", recognizesAccessKey: false);
            AssertButtonLiveResources(subtleButton, "SubtleButton", recognizesAccessKey: true);
        });
    }

    [TestMethod]
    public void ButtonThemeResourcesRemainWinUISourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertResourceValue(themeName, "ButtonBorderThemeThickness", new Thickness(1));

                AssertButtonTheme(themeName,
                    "Button",
                    "ControlFillColorDefaultBrush",
                    "ControlFillColorSecondaryBrush",
                    "ControlFillColorTertiaryBrush",
                    "ControlFillColorDisabledBrush",
                    "TextFillColorPrimaryBrush",
                    "TextFillColorPrimaryBrush",
                    "TextFillColorSecondaryBrush",
                    "TextFillColorDisabledBrush",
                    "ControlElevationBorderBrush",
                    "ControlElevationBorderBrush",
                    "ControlStrokeColorDefaultBrush",
                    "ControlStrokeColorDefaultBrush");

                AssertButtonTheme(themeName,
                    "AccentButton",
                    "AccentFillColorDefaultBrush",
                    "AccentFillColorSecondaryBrush",
                    "AccentFillColorTertiaryBrush",
                    "AccentFillColorDisabledBrush",
                    "TextOnAccentFillColorPrimaryBrush",
                    "TextOnAccentFillColorPrimaryBrush",
                    "TextOnAccentFillColorSecondaryBrush",
                    "TextOnAccentFillColorDisabledBrush",
                    "AccentControlElevationBorderBrush",
                    "AccentControlElevationBorderBrush",
                    "ControlFillColorTransparentBrush",
                    "ControlFillColorTransparentBrush");

                AssertButtonTheme(themeName,
                    "SubtleButton",
                    "SubtleFillColorTransparentBrush",
                    "SubtleFillColorSecondaryBrush",
                    "SubtleFillColorTertiaryBrush",
                    "SubtleFillColorTransparentBrush",
                    "TextFillColorPrimaryBrush",
                    "TextFillColorPrimaryBrush",
                    "TextFillColorSecondaryBrush",
                    "TextFillColorDisabledBrush",
                    "SubtleFillColorTransparentBrush",
                    "SubtleFillColorSecondaryBrush",
                    "SubtleFillColorTertiaryBrush",
                    "SubtleFillColorTransparentBrush");
            }

            AssertResourceValue("HighContrast", "ButtonBorderThemeThickness", new Thickness(1));

            AssertButtonTheme("HighContrast",
                "Button",
                "SystemControlBackgroundBaseLowBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemControlBackgroundBaseLowBrush",
                "SystemColorButtonTextColorBrush",
                "SystemControlHighlightBaseHighBrush",
                "SystemControlHighlightBaseHighBrush",
                "SystemControlDisabledBaseMediumLowBrush",
                "SystemControlForegroundTransparentBrush",
                "SystemColorHighlightColorBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemControlDisabledTransparentBrush");

            AssertButtonTheme("HighContrast",
                "AccentButton",
                "SystemControlBackgroundAccentBrush",
                "SystemAccentColorLight1Brush",
                "SystemAccentColorDark1Brush",
                "SystemControlBackgroundBaseLowBrush",
                "SystemControlForegroundChromeWhiteBrush",
                "SystemControlForegroundChromeWhiteBrush",
                "SystemControlForegroundChromeWhiteBrush",
                "SystemControlDisabledBaseMediumLowBrush",
                "SystemControlForegroundTransparentBrush",
                "SystemControlForegroundTransparentBrush",
                "SystemControlHighlightTransparentBrush",
                "SystemControlDisabledTransparentBrush");

            AssertButtonTheme("HighContrast",
                "SubtleButton",
                "SystemControlBackgroundBaseLowBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemControlBackgroundBaseLowBrush",
                "SystemColorButtonTextColorBrush",
                "SystemControlHighlightBaseHighBrush",
                "SystemControlHighlightBaseHighBrush",
                "SystemControlDisabledBaseMediumLowBrush",
                "SystemControlForegroundTransparentBrush",
                "SystemColorHighlightColorBrush",
                "SystemColorHighlightTextColorBrush",
                "SystemControlDisabledTransparentBrush");
        });
    }

    private static Button CreateButton(string content)
    {
        return new Button
        {
            Width = 100,
            Height = 40,
            Content = content
        };
    }

    private static void AssertTemplateUsesOfficialWpfPresenter(Button button, bool recognizesAccessKey)
    {
        button.ApplyTemplate();

        var contentBorder = GetTemplateChild<Border>(button, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(button, "ContentPresenter");

        Assert.AreEqual(button.Content, contentPresenter.Content);
        Assert.AreEqual(recognizesAccessKey, contentPresenter.RecognizesAccessKey);
        Assert.AreSame(button.Foreground, TextElement.GetForeground(contentPresenter));
        Assert.AreEqual(((CornerRadius)button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), contentBorder.CornerRadius);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(contentBorder).Count);
    }

    private static void AssertButtonStyleSetters(Style style, string prefix)
    {
        AssertDynamicResourceSetter(style, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
        AssertDynamicResourceSetter(style, Control.BackgroundProperty, $"{prefix}Background");
        AssertDynamicResourceSetter(style, Control.ForegroundProperty, $"{prefix}Foreground");
        AssertDynamicResourceSetter(style, Control.BorderBrushProperty, $"{prefix}BorderBrush");
        AssertSetterValue(style, Control.BorderThicknessProperty, new Thickness(1));
        AssertSetterValue(style, Control.PaddingProperty, new Thickness(11, 5, 11, 6));
        AssertSetterValue(style, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
        AssertSetterValue(style, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        AssertSetterValue(style, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center);
        AssertSetterValue(style, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
        AssertSetterValue(style, Control.FontWeightProperty, FontWeights.Normal);
        AssertDynamicResourceSetter(style, FocusVisualHelper.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
        AssertSetterValue(style, FocusVisualHelper.FocusVisualMarginProperty, new Thickness(-3));
        AssertDynamicResourceSetter(style, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
        AssertSetterValue(style, UIElement.SnapsToDevicePixelsProperty, true);
        AssertSetterValue(style, FrameworkElement.OverridesDefaultStyleProperty, true);
        AssertSetterValue(style, Stylus.IsPressAndHoldEnabledProperty, false);
    }

    private static void AssertButtonLiveResources(Button button, string prefix, bool recognizesAccessKey)
    {
        Assert.AreSame(button.TryFindResource($"{prefix}Background"), button.Background);
        Assert.AreSame(button.TryFindResource($"{prefix}Foreground"), button.Foreground);
        Assert.AreSame(button.TryFindResource($"{prefix}BorderBrush"), button.BorderBrush);
        Assert.AreEqual(button.TryFindResource("ButtonBorderThemeThickness"), button.BorderThickness);
        Assert.AreEqual(button.TryFindResource("ButtonPadding"), button.Padding);
        Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalAlignment);
        Assert.AreEqual(VerticalAlignment.Center, button.VerticalAlignment);
        Assert.AreEqual(HorizontalAlignment.Center, button.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Center, button.VerticalContentAlignment);
        Assert.AreEqual(FontWeights.Normal, button.FontWeight);
        Assert.AreEqual(button.TryFindResource("UseSystemFocusVisuals"), FocusVisualHelper.GetUseSystemFocusVisuals(button));
        Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(button));
        Assert.AreEqual(button.TryFindResource("ControlCornerRadius"), button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));
        Assert.IsTrue(button.SnapsToDevicePixels);
        Assert.IsTrue(button.OverridesDefaultStyle);
        Assert.IsFalse(Stylus.GetIsPressAndHoldEnabled(button));

        button.ApplyTemplate();
        var contentBorder = GetTemplateChild<Border>(button, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(button, "ContentPresenter");

        Assert.AreEqual(button.Width, contentBorder.Width);
        Assert.AreEqual(button.Height, contentBorder.Height);
        Assert.AreEqual(button.Padding, contentBorder.Padding);
        Assert.AreEqual(button.HorizontalAlignment, contentBorder.HorizontalAlignment);
        Assert.AreEqual(button.VerticalAlignment, contentBorder.VerticalAlignment);
        Assert.AreSame(button.Background, contentBorder.Background);
        Assert.AreSame(button.BorderBrush, contentBorder.BorderBrush);
        Assert.AreEqual(button.BorderThickness, contentBorder.BorderThickness);
        Assert.AreEqual(button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), contentBorder.CornerRadius);
        Assert.AreEqual(button.Content, contentPresenter.Content);
        Assert.AreEqual(recognizesAccessKey, contentPresenter.RecognizesAccessKey);
        Assert.AreEqual(button.HorizontalContentAlignment, contentPresenter.HorizontalAlignment);
        Assert.AreEqual(button.VerticalContentAlignment, contentPresenter.VerticalAlignment);
        Assert.AreSame(button.Foreground, TextElement.GetForeground(contentPresenter));
    }

    private static void AssertOfficialTriggerShape(
        ControlTemplate template,
        string pointerOverBackground,
        string pointerOverBorderBrush,
        string pointerOverForeground,
        string pressedBackground,
        string pressedBorderBrush,
        string pressedForeground,
        string disabledBackground,
        string disabledBorderBrush,
        string disabledForeground)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        Assert.AreEqual(3, triggers.Length);

        AssertTrigger(triggers, "IsMouseOver", true, pointerOverBackground, pointerOverBorderBrush, pointerOverForeground);
        AssertTrigger(triggers, "IsPressed", true, pressedBackground, pressedBorderBrush, pressedForeground);
        AssertTrigger(triggers, "IsEnabled", false, disabledBackground, disabledBorderBrush, disabledForeground);
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        string propertyName,
        object value,
        string backgroundKey,
        string borderBrushKey,
        string foregroundKey)
    {
        var trigger = triggers.Single(item => item.Property.Name == propertyName && Equals(item.Value, value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        Assert.AreEqual(3, setters.Length);
        AssertSetter(setters, "ContentBorder", "Background", backgroundKey);
        AssertSetter(setters, "ContentBorder", "BorderBrush", borderBrushKey);
        AssertSetter(setters, "ContentPresenter", "Foreground", foregroundKey);
    }

    private static void AssertSetter(Setter[] setters, string targetName, string propertyName, string resourceKey)
    {
        var setter = setters.Single(item =>
            item.TargetName == targetName &&
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

    private static void AssertDisabledTriggerAppliesResources(Button button, string backgroundKey, string borderBrushKey, string foregroundKey)
    {
        var contentBorder = GetTemplateChild<Border>(button, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(button, "ContentPresenter");

        button.IsEnabled = false;
        button.UpdateLayout();

        Assert.AreSame(contentBorder.TryFindResource(backgroundKey), contentBorder.Background);
        Assert.AreSame(contentBorder.TryFindResource(borderBrushKey), contentBorder.BorderBrush);
        Assert.AreSame(contentPresenter.TryFindResource(foregroundKey), TextElement.GetForeground(contentPresenter));
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }

    private static void AssertButtonTheme(
        string themeName,
        string prefix,
        string background,
        string backgroundPointerOver,
        string backgroundPressed,
        string backgroundDisabled,
        string foreground,
        string foregroundPointerOver,
        string foregroundPressed,
        string foregroundDisabled,
        string borderBrush,
        string borderBrushPointerOver,
        string borderBrushPressed,
        string borderBrushDisabled)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        AssertResourceReference(theme, $"{prefix}Background", background);
        AssertResourceReference(theme, $"{prefix}BackgroundPointerOver", backgroundPointerOver);
        AssertResourceReference(theme, $"{prefix}BackgroundPressed", backgroundPressed);
        AssertResourceReference(theme, $"{prefix}BackgroundDisabled", backgroundDisabled);
        AssertResourceReference(theme, $"{prefix}Foreground", foreground);
        AssertResourceReference(theme, $"{prefix}ForegroundPointerOver", foregroundPointerOver);
        AssertResourceReference(theme, $"{prefix}ForegroundPressed", foregroundPressed);
        AssertResourceReference(theme, $"{prefix}ForegroundDisabled", foregroundDisabled);
        AssertResourceReference(theme, $"{prefix}BorderBrush", borderBrush);
        AssertResourceReference(theme, $"{prefix}BorderBrushPointerOver", borderBrushPointerOver);
        AssertResourceReference(theme, $"{prefix}BorderBrushPressed", borderBrushPressed);
        AssertResourceReference(theme, $"{prefix}BorderBrushDisabled", borderBrushDisabled);
    }

    private static void AssertResourceValue(string themeName, string key, object expectedValue)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(key), $"Theme is missing {key}.");
        Assert.AreEqual(expectedValue, theme[key], key);
    }

    private static void AssertResourceReference(ResourceDictionary theme, string key, object expectedResourceKey)
    {
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }
}
