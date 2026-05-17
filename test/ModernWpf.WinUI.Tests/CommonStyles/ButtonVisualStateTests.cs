using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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
    public void SubtleButtonThemeResourcesRemainWinUISourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            AssertSubtleTheme("Light",
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

            AssertSubtleTheme("Dark",
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

            AssertSubtleTheme("HighContrast",
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
        Assert.AreEqual(ControlHelper.GetCornerRadius(button), contentBorder.CornerRadius);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(contentBorder).Count);
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

    private static void AssertSubtleTheme(
        string themeName,
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
        AssertResourceReference(theme, "SubtleButtonBackground", background);
        AssertResourceReference(theme, "SubtleButtonBackgroundPointerOver", backgroundPointerOver);
        AssertResourceReference(theme, "SubtleButtonBackgroundPressed", backgroundPressed);
        AssertResourceReference(theme, "SubtleButtonBackgroundDisabled", backgroundDisabled);
        AssertResourceReference(theme, "SubtleButtonForeground", foreground);
        AssertResourceReference(theme, "SubtleButtonForegroundPointerOver", foregroundPointerOver);
        AssertResourceReference(theme, "SubtleButtonForegroundPressed", foregroundPressed);
        AssertResourceReference(theme, "SubtleButtonForegroundDisabled", foregroundDisabled);
        AssertResourceReference(theme, "SubtleButtonBorderBrush", borderBrush);
        AssertResourceReference(theme, "SubtleButtonBorderBrushPointerOver", borderBrushPointerOver);
        AssertResourceReference(theme, "SubtleButtonBorderBrushPressed", borderBrushPressed);
        AssertResourceReference(theme, "SubtleButtonBorderBrushDisabled", borderBrushDisabled);
    }

    private static void AssertResourceReference(ResourceDictionary theme, string key, object expectedResourceKey)
    {
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }
}
