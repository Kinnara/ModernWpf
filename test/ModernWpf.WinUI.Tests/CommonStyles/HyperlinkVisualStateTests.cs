using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class HyperlinkVisualStateTests
{
    [TestMethod]
    public void DefaultHyperlinkStyleUsesOfficialWpfFluentStyleSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultHyperlinkStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(Hyperlink));
            Assert.AreEqual(typeof(Hyperlink), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Hyperlink), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(2, setters.Length);
            AssertDynamicResourceSetter(setters, TextElement.ForegroundProperty, "HyperlinkForeground");
            AssertTextDecorationsSetter(setters, TextDecorations.Underline);
            Assert.IsFalse(setters.Any(item => item.Property == HyperlinkHelper.IsPressedProperty));
            Assert.IsFalse(setters.Any(item => item.Property == FrameworkContentElement.CursorProperty));

            var triggers = defaultStyle.Triggers.OfType<Trigger>().ToArray();
            Assert.AreEqual(2, triggers.Length);
            AssertTrigger(
                triggers,
                ContentElement.IsMouseOverProperty,
                true,
                (TextElement.ForegroundProperty, "HyperlinkForegroundPointerOver"),
                (Inline.TextDecorationsProperty, null));
            AssertTrigger(
                triggers,
                ContentElement.IsEnabledProperty,
                false,
                (TextElement.ForegroundProperty, "HyperlinkForegroundDisabled"));

            Assert.IsNull(Application.Current.TryFindResource("HyperlinkUnderlineVisible"));
        });
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialHyperlinkAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "HyperlinkForeground", "AccentTextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkForegroundPointerOver", "AccentTextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "HyperlinkForegroundDisabled", "TextFillColorDisabledBrush");
            }

            AssertThemeResourceReference("HighContrast", "HyperlinkForeground", "SystemControlHyperlinkTextBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkForegroundPointerOver", "SystemControlHyperlinkTextBrush");
            AssertThemeResourceReference("HighContrast", "HyperlinkForegroundDisabled", "SystemColorGrayTextColorBrush");
        });
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertTextDecorationsSetter(Setter[] setters, TextDecorationCollection expected)
    {
        var setter = setters.Single(item => item.Property == Inline.TextDecorationsProperty);
        AssertTextDecorations(expected, setter.Value);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        DependencyProperty property,
        object value,
        params (DependencyProperty Property, object? Value)[] expectedSetters)
    {
        var trigger = triggers.Single(item => item.Property == property && Equals(item.Value, value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();
        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expected in expectedSetters)
        {
            var setter = setters.Single(item => item.Property == expected.Property);
            if (expected.Value is string resourceKey)
            {
                Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
                var dynamicResource = (DynamicResourceExtension)setter.Value;
                Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
            }
            else if (expected.Property == Inline.TextDecorationsProperty)
            {
                AssertTextDecorations(expected.Value as TextDecorationCollection, setter.Value);
            }
            else
            {
                Assert.AreEqual(expected.Value, setter.Value);
            }
        }
    }

    private static void AssertTextDecorations(TextDecorationCollection? expected, object? actualValue)
    {
        if (expected == null)
        {
            if (actualValue is TextDecorationCollection emptyCollection)
            {
                Assert.AreEqual(0, emptyCollection.Count);
            }
            else
            {
                Assert.IsNull(actualValue);
            }

            return;
        }

        var actual = actualValue as TextDecorationCollection;
        if (actual == null)
        {
            Assert.Fail("Expected TextDecorationCollection.");
            return;
        }

        Assert.AreEqual(expected.Count, actual.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(expected[i].Location, actual[i].Location);
        }
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }
}
