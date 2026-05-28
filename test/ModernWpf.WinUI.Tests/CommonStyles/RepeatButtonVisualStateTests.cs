using System;
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
public class RepeatButtonVisualStateTests
{
    [TestMethod]
    public void DefaultRepeatButtonStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultRepeatButtonStyle");
            var implicitRepeatButtonStyle = (Style)Application.Current.FindResource(typeof(RepeatButton));
            Assert.AreEqual(typeof(RepeatButton), defaultStyle.TargetType);
            Assert.AreEqual(typeof(RepeatButton), implicitRepeatButtonStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitRepeatButtonStyle.BasedOn);

            var repeatButton = CreateRepeatButton();
            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);

            Assert.AreEqual((Thickness)Application.Current.FindResource("RepeatButtonPadding"), repeatButton.Padding);
            Assert.AreEqual((Thickness)Application.Current.FindResource("RepeatButtonBorderThemeThickness"), repeatButton.BorderThickness);
            AssertTemplateUsesOfficialWpfPresenter(repeatButton);
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(repeatButton));
            AssertOfficialTriggerShape(repeatButton.Template);
            AssertDisabledTriggerAppliesResources(repeatButton);
        });
    }

    [TestMethod]
    public void RepeatButtonStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultRepeatButtonStyle");
            var implicitRepeatButtonStyle = (Style)Application.Current.FindResource(typeof(RepeatButton));
            var repeatButton = CreateRepeatButton();

            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);
            host.UpdateLayout();

            Assert.AreSame(defaultStyle, implicitRepeatButtonStyle.BasedOn);
            AssertDynamicResourceSetter(defaultStyle, Control.FocusVisualStyleProperty, SystemParameters.FocusVisualStyleKey);
            AssertDynamicResourceSetter(defaultStyle, Control.BackgroundProperty, "RepeatButtonBackground");
            AssertDynamicResourceSetter(defaultStyle, Control.ForegroundProperty, "RepeatButtonForeground");
            AssertDynamicResourceSetter(defaultStyle, Control.BorderBrushProperty, "RepeatButtonBorderBrush");
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
            AssertTemplateSetter(defaultStyle, typeof(RepeatButton));

            Assert.AreSame(repeatButton.TryFindResource("RepeatButtonBackground"), repeatButton.Background);
            Assert.AreSame(repeatButton.TryFindResource("RepeatButtonForeground"), repeatButton.Foreground);
            Assert.AreSame(repeatButton.TryFindResource("RepeatButtonBorderBrush"), repeatButton.BorderBrush);
            Assert.AreEqual(repeatButton.TryFindResource("RepeatButtonBorderThemeThickness"), repeatButton.BorderThickness);
            Assert.AreEqual(repeatButton.TryFindResource("RepeatButtonPadding"), repeatButton.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, repeatButton.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, repeatButton.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Center, repeatButton.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, repeatButton.VerticalContentAlignment);
            Assert.AreEqual(FontWeights.Normal, repeatButton.FontWeight);
            Assert.AreEqual(repeatButton.TryFindResource("UseSystemFocusVisuals"), FocusVisualHelper.GetUseSystemFocusVisuals(repeatButton));
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(repeatButton));
            Assert.AreEqual(repeatButton.TryFindResource("ControlCornerRadius"), repeatButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty));
            Assert.IsTrue(repeatButton.SnapsToDevicePixels);
            Assert.IsTrue(repeatButton.OverridesDefaultStyle);
            Assert.IsFalse(Stylus.GetIsPressAndHoldEnabled(repeatButton));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(repeatButton));

            var contentBorder = GetTemplateChild<Border>(repeatButton, "ContentBorder");
            var contentPresenter = GetTemplateChild<ContentPresenter>(repeatButton, "ContentPresenter");

            Assert.AreEqual(repeatButton.Width, contentBorder.Width);
            Assert.AreEqual(repeatButton.Height, contentBorder.Height);
            Assert.AreEqual(repeatButton.Padding, contentBorder.Padding);
            Assert.AreEqual(repeatButton.HorizontalAlignment, contentBorder.HorizontalAlignment);
            Assert.AreEqual(repeatButton.VerticalAlignment, contentBorder.VerticalAlignment);
            Assert.AreSame(repeatButton.Background, contentBorder.Background);
            Assert.AreSame(repeatButton.BorderBrush, contentBorder.BorderBrush);
            Assert.AreEqual(repeatButton.BorderThickness, contentBorder.BorderThickness);
            Assert.AreEqual(repeatButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty), contentBorder.CornerRadius);
            Assert.AreEqual(repeatButton.Content, contentPresenter.Content);
            Assert.IsTrue(contentPresenter.RecognizesAccessKey);
            Assert.AreEqual(repeatButton.HorizontalContentAlignment, contentPresenter.HorizontalAlignment);
            Assert.AreEqual(repeatButton.VerticalContentAlignment, contentPresenter.VerticalAlignment);
            Assert.AreSame(repeatButton.Foreground, TextElement.GetForeground(contentPresenter));
            Assert.AreEqual(repeatButton.FontSize, TextElement.GetFontSize(contentPresenter));
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2RepeatButtonHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "RepeatButtonBorderThemeThickness", new Thickness(1));
                AssertThemeResourceReference(themeName, "RepeatButtonBackground", "ControlFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBackgroundPointerOver", "ControlFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBackgroundPressed", "ControlFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBackgroundDisabled", "ControlFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonForegroundPointerOver", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonForegroundPressed", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBorderBrush", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBorderBrushPointerOver", "ControlElevationBorderBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "RepeatButtonBorderBrushDisabled", "ControlStrokeColorDefaultBrush");
            }

            AssertThemeResourceValue("HighContrast", "RepeatButtonBorderThemeThickness", new Thickness(1));
            AssertThemeResourceReference("HighContrast", "RepeatButtonBackground", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBackgroundPointerOver", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBackgroundPressed", "SystemControlBackgroundBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBackgroundDisabled", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonForegroundPointerOver", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonForegroundPressed", "SystemControlHighlightBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBorderBrush", "SystemControlForegroundTransparentBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBorderBrushPointerOver", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBorderBrushPressed", "SystemControlHighlightTransparentBrush");
            AssertThemeResourceReference("HighContrast", "RepeatButtonBorderBrushDisabled", "SystemControlDisabledTransparentBrush");
        });
    }

    private static RepeatButton CreateRepeatButton()
    {
        return new RepeatButton
        {
            Width = 100,
            Height = 40,
            Content = "Repeat"
        };
    }

    private static void AssertTemplateUsesOfficialWpfPresenter(RepeatButton repeatButton)
    {
        repeatButton.ApplyTemplate();

        var contentBorder = GetTemplateChild<Border>(repeatButton, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(repeatButton, "ContentPresenter");

        Assert.AreEqual(repeatButton.Content, contentPresenter.Content);
        Assert.IsTrue(contentPresenter.RecognizesAccessKey);
        Assert.AreSame(repeatButton.Foreground, TextElement.GetForeground(contentPresenter));
        Assert.AreEqual(repeatButton.FontSize, TextElement.GetFontSize(contentPresenter));
        Assert.AreEqual(((CornerRadius)repeatButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), contentBorder.CornerRadius);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(contentBorder).Count);
    }

    private static void AssertOfficialTriggerShape(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        Assert.AreEqual(3, triggers.Length);

        AssertTrigger(triggers, "IsEnabled", false,
            ("ContentBorder", "Background", "RepeatButtonBackgroundDisabled"),
            ("ContentBorder", "BorderBrush", "RepeatButtonBorderBrushDisabled"),
            ("ContentPresenter", "Foreground", "RepeatButtonForegroundDisabled"));

        AssertTrigger(triggers, "IsMouseOver", true,
            ("ContentBorder", "Background", "RepeatButtonBackgroundPointerOver"));

        AssertTrigger(triggers, "IsPressed", true,
            ("ContentBorder", "Background", "RepeatButtonBackgroundPressed"),
            ("ContentBorder", "BorderBrush", "RepeatButtonBorderBrushPressed"),
            ("ContentPresenter", "Foreground", "RepeatButtonForegroundPressed"));
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        string propertyName,
        object value,
        params (string TargetName, string PropertyName, string ResourceKey)[] expectedSetters)
    {
        var trigger = triggers.Single(item => item.Property.Name == propertyName && Equals(item.Value, value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expectedSetter in expectedSetters)
        {
            AssertSetter(setters, expectedSetter.TargetName, expectedSetter.PropertyName, expectedSetter.ResourceKey);
        }
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

    private static void AssertTemplateSetter(Style style, Type expectedTargetType)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == Control.TemplateProperty);
        Assert.IsNotNull(setter, "Expected a direct Template setter.");
        Assert.IsInstanceOfType(setter!.Value, typeof(ControlTemplate));

        var template = (ControlTemplate)setter.Value;
        Assert.AreEqual(expectedTargetType, template.TargetType);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(item => item.Property == property);
        Assert.IsNotNull(setter, $"Expected setter for {property.Name}.");
        Assert.IsInstanceOfType(setter!.Value, typeof(DynamicResourceExtension));

        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(expectedResourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertDisabledTriggerAppliesResources(RepeatButton repeatButton)
    {
        var contentBorder = GetTemplateChild<Border>(repeatButton, "ContentBorder");
        var contentPresenter = GetTemplateChild<ContentPresenter>(repeatButton, "ContentPresenter");

        repeatButton.IsEnabled = false;
        repeatButton.UpdateLayout();

        Assert.AreSame(contentBorder.TryFindResource("RepeatButtonBackgroundDisabled"), contentBorder.Background);
        Assert.AreSame(contentBorder.TryFindResource("RepeatButtonBorderBrushDisabled"), contentBorder.BorderBrush);
        Assert.AreSame(contentPresenter.TryFindResource("RepeatButtonForegroundDisabled"), TextElement.GetForeground(contentPresenter));
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
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
