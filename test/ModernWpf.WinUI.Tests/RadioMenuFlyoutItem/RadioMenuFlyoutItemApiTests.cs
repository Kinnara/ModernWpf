using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.RadioMenuFlyoutItem;

[TestClass]
public class RadioMenuFlyoutItemApiTests
{
    [TestMethod]
    public void VerifyDefaultStyleAndWinUI2Resources()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new XamlControlsResources();
            AssertResourceValue(resources, "MenuFlyoutItemChevronMargin", new Thickness(24, 0, 0, -1));
            AssertResourceValue(resources, "MenuFlyoutItemPlaceholderThemeThickness", new Thickness(28, 0, 0, 0));
            AssertResourceValue(resources, "MenuFlyoutItemThemePadding", new Thickness(11, 8, 11, 9));
            AssertResourceValue(resources, "MenuFlyoutItemThemePaddingNarrow", new Thickness(11, 4, 11, 5));

            AssertLightDarkChevronResources("Light");
            AssertLightDarkChevronResources("Dark");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevron", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronSubMenuOpened", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronDisabled", "SystemColorGrayTextColorBrush");

            var item = new RadioMenuItem
            {
                Header = "Yellow"
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            Assert.AreEqual(resources["MenuFlyoutItemThemePadding"], item.Padding);
            Assert.AreEqual(HorizontalAlignment.Stretch, item.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, item.VerticalContentAlignment);

            item.ApplyTemplate();
            var checkGlyph = GetTemplateChild<FontIconFallback>(item, "CheckGlyph");
            Assert.AreEqual(12.0, checkGlyph.FontSize);
            Assert.AreEqual(new Thickness(0, 0, 16, 0), checkGlyph.Margin);
            Assert.AreEqual(0.0, checkGlyph.Opacity);

            item.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            host.UpdateLayout();

            Assert.AreEqual(1.0, checkGlyph.Opacity);
        });
    }

    private static void AssertLightDarkChevronResources(string themeName)
    {
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevron", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronPressed", "TextFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronSubMenuOpened", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronDisabled", "TextFillColorDisabledBrush");
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        var child = control.Template.FindName(name, control) as T;
        Assert.IsNotNull(child, $"Missing template part '{name}'.");
        return child!;
    }

    private static void AssertResourceValue<T>(ResourceDictionary resources, string resourceKey, T expectedValue)
    {
        Assert.IsTrue(resources.Contains(resourceKey), $"Missing root resource '{resourceKey}'.");
        Assert.AreEqual(expectedValue, resources[resourceKey], resourceKey);
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }
}
