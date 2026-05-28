using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class RadioButtonVisualStateTests
{
    [TestMethod]
    public void DefaultRadioButtonStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultRadioButtonStyle");
            var implicitRadioButtonStyle = (Style)Application.Current.FindResource(typeof(RadioButton));
            Assert.AreEqual(typeof(RadioButton), defaultStyle.TargetType);
            Assert.AreEqual(typeof(RadioButton), implicitRadioButtonStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitRadioButtonStyle.BasedOn);

            var radioButton = CreateRadioButton();
            using var host = new TestWindowHost(radioButton, width: 180, height: 80);

            Assert.AreEqual((Thickness)Application.Current.FindResource("RadioButtonPadding"), radioButton.Padding);
            Assert.IsTrue(radioButton.Focusable);
            Assert.IsTrue(KeyboardNavigation.GetIsTabStop(radioButton));
            AssertTemplateUsesOfficialWpfPresenter(radioButton);
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(radioButton));
            AssertOfficialVisualStateShape(radioButton);
            AssertOfficialTriggerShape(radioButton.Template);
            AssertUncheckedDisabledTriggerAppliesResources(radioButton);
        });
    }

    [TestMethod]
    public void CheckedAndDisabledStatesUseOfficialWpfFluentResources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = CreateRadioButton();
            radioButton.IsChecked = true;
            using var host = new TestWindowHost(radioButton, width: 180, height: 80);
            host.UpdateLayout();

            var checkGlyph = GetTemplateChild<Ellipse>(radioButton, "CheckGlyph");
            var outerEllipse = GetTemplateChild<Ellipse>(radioButton, "OuterEllipse");
            var checkOuterEllipse = GetTemplateChild<Ellipse>(radioButton, "CheckOuterEllipse");

            Assert.AreEqual(1.0, checkGlyph.Opacity);
            Assert.AreEqual(0.0, outerEllipse.Opacity);
            Assert.AreEqual(1.0, checkOuterEllipse.Opacity);

            radioButton.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreSame(radioButton.TryFindResource("RadioButtonForegroundDisabled"), radioButton.Foreground);
            Assert.AreSame(radioButton.TryFindResource("RadioButtonBackgroundDisabled"), radioButton.Background);
            Assert.AreSame(outerEllipse.TryFindResource("RadioButtonOuterEllipseFillDisabled"), outerEllipse.Fill);
            Assert.AreSame(outerEllipse.TryFindResource("RadioButtonOuterEllipseCheckedStrokeDisabled"), outerEllipse.Stroke);
            Assert.AreSame(checkOuterEllipse.TryFindResource("RadioButtonOuterEllipseCheckedStrokeDisabled"), checkOuterEllipse.Stroke);
            Assert.AreSame(checkOuterEllipse.TryFindResource("RadioButtonOuterEllipseCheckedFillDisabled"), checkOuterEllipse.Fill);
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialRadioButtonCheckedResources()
    {
        WpfTestHost.Run(() =>
        {
            AssertThemeResourceReference("Light", "RadioButtonCheckOuterEllipseCheckedStrokePressed", "AccentFillColorTertiaryBrush");
            AssertThemeResourceReference("Light", "RadioButtonCheckOuterEllipseCheckedFillPointerOver", "AccentFillColorSecondaryBrush");
            AssertThemeResourceReference("Light", "RadioButtonCheckOuterEllipseCheckedFillPressed", "AccentFillColorTertiaryBrush");

            AssertThemeResourceReference("Dark", "RadioButtonCheckOuterEllipseCheckedStrokePressed", "AccentFillColorTertiaryBrush");
            AssertThemeResourceReference("Dark", "RadioButtonCheckOuterEllipseCheckedFillPointerOver", "AccentFillColorSecondaryBrush");
            AssertThemeResourceReference("Dark", "RadioButtonCheckOuterEllipseCheckedFillPressed", "AccentFillColorTertiaryBrush");

            AssertThemeResourceReference("HighContrast", "RadioButtonCheckOuterEllipseCheckedStrokePressed", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "RadioButtonCheckOuterEllipseCheckedFillPointerOver", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "RadioButtonCheckOuterEllipseCheckedFillPressed", "SystemColorButtonTextColorBrush");
        });
    }

    [TestMethod]
    public void ThemeResourcesUseOfficialRadioButtonHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceValue(themeName, "RadioButtonBorderThemeThickness", 1.0);
                AssertThemeResourceReferences(themeName,
                    ("RadioButtonForeground", "TextFillColorPrimaryBrush"),
                    ("RadioButtonForegroundPointerOver", "TextFillColorPrimaryBrush"),
                    ("RadioButtonForegroundPressed", "TextFillColorPrimaryBrush"),
                    ("RadioButtonForegroundDisabled", "TextFillColorDisabledBrush"),
                    ("RadioButtonBackground", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBackgroundPointerOver", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBackgroundPressed", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBackgroundDisabled", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBorderBrush", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBorderBrushPointerOver", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBorderBrushPressed", "ControlFillColorTransparentBrush"),
                    ("RadioButtonBorderBrushDisabled", "ControlFillColorTransparentBrush"),
                    ("RadioButtonOuterEllipseStroke", "ControlStrongStrokeColorDefaultBrush"),
                    ("RadioButtonOuterEllipseStrokePointerOver", "ControlStrongStrokeColorDefaultBrush"),
                    ("RadioButtonOuterEllipseStrokePressed", "ControlStrongStrokeColorDisabledBrush"),
                    ("RadioButtonOuterEllipseStrokeDisabled", "ControlStrongStrokeColorDisabledBrush"),
                    ("RadioButtonOuterEllipseFill", "ControlAltFillColorSecondaryBrush"),
                    ("RadioButtonOuterEllipseFillPointerOver", "ControlAltFillColorTertiaryBrush"),
                    ("RadioButtonOuterEllipseFillPressed", "ControlAltFillColorQuarternaryBrush"),
                    ("RadioButtonOuterEllipseFillDisabled", "ControlAltFillColorDisabledBrush"),
                    ("RadioButtonOuterEllipseCheckedStroke", "AccentFillColorDefaultBrush"),
                    ("RadioButtonOuterEllipseCheckedStrokePointerOver", "AccentFillColorSecondaryBrush"),
                    ("RadioButtonOuterEllipseCheckedStrokePressed", "AccentFillColorTertiaryBrush"),
                    ("RadioButtonOuterEllipseCheckedStrokeDisabled", "AccentFillColorDisabledBrush"),
                    ("RadioButtonCheckOuterEllipseCheckedStrokePressed", "AccentFillColorTertiaryBrush"),
                    ("RadioButtonOuterEllipseCheckedFill", "AccentFillColorDefaultBrush"),
                    ("RadioButtonOuterEllipseCheckedFillPointerOver", "AccentFillColorSecondaryBrush"),
                    ("RadioButtonOuterEllipseCheckedFillPressed", "AccentFillColorTertiaryBrush"),
                    ("RadioButtonOuterEllipseCheckedFillDisabled", "AccentFillColorDisabledBrush"),
                    ("RadioButtonCheckOuterEllipseCheckedFillPointerOver", "AccentFillColorSecondaryBrush"),
                    ("RadioButtonCheckOuterEllipseCheckedFillPressed", "AccentFillColorTertiaryBrush"),
                    ("RadioButtonCheckGlyphFill", "TextOnAccentFillColorPrimaryBrush"),
                    ("RadioButtonCheckGlyphFillPointerOver", "TextOnAccentFillColorPrimaryBrush"),
                    ("RadioButtonCheckGlyphFillPressed", "TextOnAccentFillColorPrimaryBrush"),
                    ("RadioButtonCheckGlyphFillDisabled", "TextOnAccentFillColorPrimaryBrush"),
                    ("RadioButtonCheckGlyphStroke", "CircleElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokePointerOver", "CircleElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokePressed", "CircleElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokeDisabled", "CircleElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokeChecked", "AccentControlElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokeCheckedPointerOver", "AccentControlElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokeCheckedPressed", "AccentControlElevationBorderBrush"),
                    ("RadioButtonCheckGlyphStrokeCheckedDisabled", "ControlElevationBorderBrush"),
                    ("RadioButtonsHeaderForeground", "TextFillColorPrimaryBrush"),
                    ("RadioButtonsHeaderForegroundDisabled", "TextFillColorDisabledBrush"));
            }

            AssertThemeResourceValue("HighContrast", "RadioButtonBorderThemeThickness", 1.0);
            AssertThemeResourceReferences("HighContrast",
                ("RadioButtonForeground", "SystemControlForegroundBaseHighBrush"),
                ("RadioButtonForegroundPointerOver", "SystemControlForegroundBaseHighBrush"),
                ("RadioButtonForegroundPressed", "SystemControlForegroundBaseHighBrush"),
                ("RadioButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("RadioButtonBackground", "SystemControlTransparentBrush"),
                ("RadioButtonBackgroundPointerOver", "SystemControlTransparentBrush"),
                ("RadioButtonBackgroundPressed", "SystemControlTransparentBrush"),
                ("RadioButtonBackgroundDisabled", "SystemControlTransparentBrush"),
                ("RadioButtonBorderBrush", "SystemControlTransparentBrush"),
                ("RadioButtonBorderBrushPointerOver", "SystemControlTransparentBrush"),
                ("RadioButtonBorderBrushPressed", "SystemControlTransparentBrush"),
                ("RadioButtonBorderBrushDisabled", "SystemControlTransparentBrush"),
                ("RadioButtonOuterEllipseStroke", "SystemControlForegroundBaseMediumBrush"),
                ("RadioButtonOuterEllipseStrokePointerOver", "SystemColorHighlightColorBrush"),
                ("RadioButtonOuterEllipseStrokePressed", "SystemColorHighlightTextColorBrush"),
                ("RadioButtonOuterEllipseStrokeDisabled", "SystemColorGrayTextColorBrush"),
                ("RadioButtonOuterEllipseFill", "SystemColorButtonFaceColorBrush"),
                ("RadioButtonOuterEllipseFillPointerOver", "SystemColorHighlightTextColorBrush"),
                ("RadioButtonOuterEllipseFillPressed", "SystemColorHighlightTextColorBrush"),
                ("RadioButtonOuterEllipseFillDisabled", "SystemColorButtonFaceColorBrush"),
                ("RadioButtonOuterEllipseCheckedStroke", "SystemControlHighlightAccentBrush"),
                ("RadioButtonOuterEllipseCheckedStrokePointerOver", "SystemColorButtonTextColorBrush"),
                ("RadioButtonOuterEllipseCheckedStrokePressed", "SystemColorButtonFaceColorBrush"),
                ("RadioButtonOuterEllipseCheckedStrokeDisabled", "SystemColorGrayTextColorBrush"),
                ("RadioButtonCheckOuterEllipseCheckedStrokePressed", "SystemColorButtonTextColorBrush"),
                ("RadioButtonOuterEllipseCheckedFill", "SystemControlHighlightAltTransparentBrush"),
                ("RadioButtonOuterEllipseCheckedFillPointerOver", "SystemColorButtonFaceColorBrush"),
                ("RadioButtonOuterEllipseCheckedFillPressed", "SystemColorButtonFaceColorBrush"),
                ("RadioButtonOuterEllipseCheckedFillDisabled", "SystemColorButtonFaceColorBrush"),
                ("RadioButtonCheckOuterEllipseCheckedFillPointerOver", "SystemColorButtonTextColorBrush"),
                ("RadioButtonCheckOuterEllipseCheckedFillPressed", "SystemColorButtonTextColorBrush"),
                ("RadioButtonCheckGlyphFill", "SystemControlHighlightBaseMediumHighBrush"),
                ("RadioButtonCheckGlyphFillPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("RadioButtonCheckGlyphFillPressed", "SystemControlHighlightAltBaseMediumBrush"),
                ("RadioButtonCheckGlyphFillDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("RadioButtonCheckGlyphStroke", "SystemControlTransparentBrush"),
                ("RadioButtonCheckGlyphStrokePointerOver", "SystemControlTransparentBrush"),
                ("RadioButtonCheckGlyphStrokePressed", "SystemControlTransparentBrush"),
                ("RadioButtonCheckGlyphStrokeDisabled", "SystemControlTransparentBrush"),
                ("RadioButtonCheckGlyphStrokeChecked", "SystemControlTransparentBrush"),
                ("RadioButtonCheckGlyphStrokeCheckedPointerOver", "SystemColorButtonTextColorBrush"),
                ("RadioButtonCheckGlyphStrokeCheckedPressed", "SystemColorButtonTextColorBrush"),
                ("RadioButtonCheckGlyphStrokeCheckedDisabled", "SystemControlTransparentBrush"),
                ("RadioButtonsHeaderForeground", "SystemControlForegroundBaseHighBrush"),
                ("RadioButtonsHeaderForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"));
        });
    }

    private static RadioButton CreateRadioButton()
    {
        return new RadioButton
        {
            Width = 150,
            Height = 48,
            Content = "Option"
        };
    }

    private static void AssertTemplateUsesOfficialWpfPresenter(RadioButton radioButton)
    {
        radioButton.ApplyTemplate();

        var rootBorder = GetTemplateChild<Border>(radioButton, "RootBorder");
        var rootGrid = GetTemplateChild<Grid>(radioButton, "RootGrid");
        var contentPresenter = GetTemplateChild<ContentPresenter>(radioButton, "ContentPresenter");
        var outerEllipse = GetTemplateChild<Ellipse>(radioButton, "OuterEllipse");
        var checkOuterEllipse = GetTemplateChild<Ellipse>(radioButton, "CheckOuterEllipse");
        var checkGlyph = GetTemplateChild<Ellipse>(radioButton, "CheckGlyph");
        var pressedCheckGlyph = GetTemplateChild<Border>(radioButton, "PressedCheckGlyph");

        Assert.AreEqual(radioButton.Content, contentPresenter.Content);
        Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
        Assert.IsTrue(contentPresenter.RecognizesAccessKey);
        Assert.AreSame(radioButton.Foreground, TextElement.GetForeground(contentPresenter));
        Assert.AreEqual(((CornerRadius)radioButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), rootBorder.CornerRadius);
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(rootGrid).Count);

        var strokeThickness = (double)Application.Current.FindResource("RadioButtonStrokeThickness");
        Assert.AreEqual(strokeThickness, outerEllipse.StrokeThickness);
        Assert.AreEqual(strokeThickness, checkOuterEllipse.StrokeThickness);
        Assert.IsInstanceOfType(checkGlyph.RenderTransform, typeof(ScaleTransform));
        Assert.AreEqual(4.0, pressedCheckGlyph.Width);
        Assert.AreEqual(4.0, pressedCheckGlyph.Height);
    }

    private static void AssertOfficialVisualStateShape(RadioButton radioButton)
    {
        var rootBorder = GetTemplateChild<Border>(radioButton, "RootBorder");
        var group = VisualStateManager.GetVisualStateGroups(rootBorder)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        var states = group.States.Cast<VisualState>().ToArray();

        CollectionAssert.AreEqual(new[] { "Normal", "MouseOver", "Pressed" }, states.Select(item => item.Name).ToArray());
        Assert.IsFalse(states.Any(item => item.GetType().Name == "VisualStateEx"));
    }

    private static void AssertOfficialTriggerShape(ControlTemplate template)
    {
        var multiTriggers = template.Triggers.OfType<MultiTrigger>().ToArray();
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        Assert.AreEqual(6, multiTriggers.Length);
        Assert.AreEqual(2, triggers.Length);

        AssertTrigger(multiTriggers,
            new[] { ("IsPressed", (object)true), ("IsChecked", (object)false), ("IsEnabled", (object)true) },
            ("CheckGlyph", "Opacity", 1.0),
            ("OuterEllipse", "Fill", "RadioButtonOuterEllipseFillPressed"),
            ("OuterEllipse", "Stroke", "RadioButtonOuterEllipseStrokePressed"),
            ("RootBorder", "BorderBrush", "RadioButtonBorderBrushPressed"),
            ("RootBorder", "Background", "RadioButtonBackgroundPressed"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object)true), ("IsChecked", (object)false), ("IsEnabled", (object)true) },
            ("OuterEllipse", "Fill", "RadioButtonOuterEllipseFillPointerOver"),
            ("OuterEllipse", "Stroke", "RadioButtonOuterEllipseStrokePointerOver"),
            ("RootBorder", "BorderBrush", "RadioButtonBorderBrushPointerOver"),
            ("RootBorder", "Background", "RadioButtonBackgroundPointerOver"));

        AssertTrigger(multiTriggers,
            new[] { ("IsMouseOver", (object)true), ("IsChecked", (object)true), ("IsEnabled", (object)true) },
            ("OuterEllipse", "Fill", "RadioButtonOuterEllipseFillPointerOver"),
            ("OuterEllipse", "Stroke", "RadioButtonOuterEllipseStrokePointerOver"),
            ("CheckOuterEllipse", "Fill", "RadioButtonCheckOuterEllipseCheckedFillPointerOver"),
            ("PressedCheckGlyph", "Background", "RadioButtonOuterEllipseCheckedStrokePointerOver"),
            ("RootBorder", "BorderBrush", "RadioButtonBorderBrushPointerOver"),
            ("RootBorder", "Background", "RadioButtonBackgroundPointerOver"));

        AssertTrigger(multiTriggers,
            new[] { ("IsPressed", (object)true), ("IsChecked", (object)true), ("IsEnabled", (object)true) },
            ("OuterEllipse", "Fill", "RadioButtonCheckOuterEllipseCheckedFillPressed"),
            ("CheckOuterEllipse", "Fill", "RadioButtonCheckOuterEllipseCheckedFillPressed"),
            ("RootBorder", "BorderBrush", "RadioButtonBorderBrushPressed"),
            ("RootBorder", "Background", "RadioButtonBackgroundPressed"),
            ("PressedCheckGlyph", "Background", "RadioButtonCheckOuterEllipseCheckedStrokePressed"));

        AssertTrigger(multiTriggers,
            new[] { ("IsChecked", (object)false), ("IsEnabled", (object)false) },
            ("", "Foreground", "RadioButtonForegroundDisabled"),
            ("", "Background", "RadioButtonBackgroundDisabled"),
            ("OuterEllipse", "Fill", "RadioButtonOuterEllipseFillDisabled"),
            ("OuterEllipse", "Stroke", "RadioButtonOuterEllipseStrokeDisabled"));

        AssertTrigger(multiTriggers,
            new[] { ("IsChecked", (object)true), ("IsEnabled", (object)false) },
            ("", "Foreground", "RadioButtonForegroundDisabled"),
            ("", "Background", "RadioButtonBackgroundDisabled"),
            ("OuterEllipse", "Fill", "RadioButtonOuterEllipseFillDisabled"),
            ("OuterEllipse", "Stroke", "RadioButtonOuterEllipseCheckedStrokeDisabled"),
            ("CheckOuterEllipse", "Stroke", "RadioButtonOuterEllipseCheckedStrokeDisabled"),
            ("CheckOuterEllipse", "Fill", "RadioButtonOuterEllipseCheckedFillDisabled"));

        AssertCheckedTrigger(triggers);
        AssertRightToLeftTrigger(triggers);
    }

    private static void AssertTrigger(
        MultiTrigger[] triggers,
        (string PropertyName, object Value)[] expectedConditions,
        params (string TargetName, string PropertyName, object Value)[] expectedSetters)
    {
        var trigger = triggers.Single(item => expectedConditions.All(condition => HasCondition(item, condition.PropertyName, condition.Value)));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expectedSetter in expectedSetters)
        {
            AssertSetter(setters, expectedSetter.TargetName, expectedSetter.PropertyName, expectedSetter.Value);
        }
    }

    private static void AssertCheckedTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "IsChecked" && Equals(item.Value, true));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        Assert.AreEqual(3, setters.Length);
        AssertSetter(setters, "CheckGlyph", "Opacity", 1.0);
        AssertSetter(setters, "OuterEllipse", "Opacity", 0.0);
        AssertSetter(setters, "CheckOuterEllipse", "Opacity", 1.0);
    }

    private static void AssertRightToLeftTrigger(Trigger[] triggers)
    {
        var trigger = triggers.Single(item => item.Property.Name == "FlowDirection" && Equals(item.Value, FlowDirection.RightToLeft));
        var setter = trigger.Setters.OfType<Setter>().Single();

        Assert.AreEqual(string.Empty, setter.TargetName ?? string.Empty);
        Assert.AreEqual("HorizontalAlignment", setter.Property.Name);
        Assert.AreEqual(HorizontalAlignment.Right, setter.Value);
    }

    private static bool HasCondition(MultiTrigger trigger, string propertyName, object value)
    {
        return trigger.Conditions.Cast<Condition>().Any(item =>
            item.Property.Name == propertyName &&
            Equals(item.Value, value));
    }

    private static void AssertSetter(Setter[] setters, string targetName, string propertyName, object value)
    {
        var setter = setters.Single(item =>
            (item.TargetName ?? string.Empty) == targetName &&
            item.Property.Name == propertyName);

        if (value is string resourceKey)
        {
            Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
            var resource = (DynamicResourceExtension)setter.Value;
            Assert.AreEqual(resourceKey, resource.ResourceKey);
        }
        else
        {
            Assert.AreEqual(value, setter.Value);
        }
    }

    private static void AssertUncheckedDisabledTriggerAppliesResources(RadioButton radioButton)
    {
        var outerEllipse = GetTemplateChild<Ellipse>(radioButton, "OuterEllipse");

        radioButton.IsEnabled = false;
        radioButton.UpdateLayout();

        Assert.AreSame(radioButton.TryFindResource("RadioButtonForegroundDisabled"), radioButton.Foreground);
        Assert.AreSame(radioButton.TryFindResource("RadioButtonBackgroundDisabled"), radioButton.Background);
        Assert.AreSame(outerEllipse.TryFindResource("RadioButtonOuterEllipseFillDisabled"), outerEllipse.Fill);
        Assert.AreSame(outerEllipse.TryFindResource("RadioButtonOuterEllipseStrokeDisabled"), outerEllipse.Stroke);
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(key), $"Theme is missing {key}.");
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }

    private static void AssertThemeResourceReferences(
        string themeName,
        params (string ResourceKey, object ExpectedResourceKey)[] references)
    {
        foreach (var reference in references)
        {
            AssertThemeResourceReference(themeName, reference.ResourceKey, reference.ExpectedResourceKey);
        }
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(resourceKey), $"Theme is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, theme[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }
}
