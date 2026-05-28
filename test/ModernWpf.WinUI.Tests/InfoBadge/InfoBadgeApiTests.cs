using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.InfoBadge;

[TestClass]
public class InfoBadgeApiTests
{
    [TestMethod]
    public void InfoBadgeDisplayKindTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge();
            var symbolIconSource = new SymbolIconSource
            {
                Symbol = Symbol.Setting
            };

            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            var textBlock = FindNamedDescendant<TextBlock>(infoBadge, "ValueTextBlock");
            var iconPresenter = FindNamedDescendant<FrameworkElement>(infoBadge, "IconPresenter");

            Assert.AreEqual(Visibility.Collapsed, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);

            infoBadge.IconSource = symbolIconSource;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, textBlock.Visibility);
            Assert.AreEqual(Visibility.Visible, iconPresenter.Visibility);

            infoBadge.Value = 10;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);

            infoBadge.IconSource = null;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);

            infoBadge.Value = -1;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);
        });
    }

    [TestMethod]
    public void InfoBadgeSupportsWpfIconTypes()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge();
            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            infoBadge.IconSource = new SymbolIconSource { Symbol = Symbol.Setting };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(SymbolIcon));

            infoBadge.IconSource = new PathIconSource
            {
                Data = new RectangleGeometry(new Rect(0, 0, 5, 2))
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(PathIcon));

            infoBadge.IconSource = new FontIconSource
            {
                Glyph = "99+",
                FontFamily = new FontFamily("Segoe UI Symbol")
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(FontIcon));

            infoBadge.IconSource = new BitmapIconSource
            {
                UriSource = new Uri("pack://application:,,,/ModernWpf.WinUI.Tests;component/Assets/rating_set.png", UriKind.Absolute)
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(BitmapIcon));

            infoBadge.IconSource = new ImageIconSource
            {
                ImageSource = CreateTestImageSource()
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(ImageIcon));
        });
    }

    [TestMethod]
    public void InfoBadgeTemplateUsesWinUIIconPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge
            {
                IconSource = new SymbolIconSource { Symbol = Symbol.Setting }
            };

            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            var rootGrid = FindNamedDescendant<GridEx>(infoBadge, "RootGrid");
            var iconPresenter = FindNamedDescendant<Viewbox>(infoBadge, "IconPresenter");
            var valueTextBlock = FindNamedDescendant<TextBlock>(infoBadge, "ValueTextBlock");
            var presenter = FindContentPresenter(infoBadge, infoBadge.TemplateSettings.IconElement);

            Assert.AreSame(infoBadge.TryFindResource("InfoBadgeBackground"), infoBadge.Background);
            Assert.AreSame(infoBadge.TryFindResource("InfoBadgeForeground"), infoBadge.Foreground);
            Assert.AreEqual(infoBadge.TryFindResource("InfoBadgePadding"), infoBadge.Padding);
            Assert.AreEqual(infoBadge.TryFindResource("InfoBadgeMinHeight"), infoBadge.MinHeight);
            Assert.AreEqual(infoBadge.TryFindResource("InfoBadgeMinWidth"), infoBadge.MinWidth);
            Assert.AreEqual(infoBadge.TryFindResource("InfoBadgeMaxHeight"), infoBadge.MaxHeight);
            Assert.IsFalse(infoBadge.IsTabStop);
            Assert.AreEqual(infoBadge.Background, rootGrid.Background);
            Assert.AreEqual(infoBadge.Padding, rootGrid.Padding);
            Assert.AreEqual(infoBadge.TryFindResource("IconInfoBadgeIconMargin"), iconPresenter.Margin);
            Assert.AreEqual(infoBadge.TryFindResource("InfoBadgeValueFontSize"), valueTextBlock.FontSize);
            Assert.IsInstanceOfType(presenter.Content, typeof(SymbolIcon));
            Assert.IsFalse(ContainsPlainContentPresenter(infoBadge));
        });
    }

    [TestMethod]
    public void InfoBadgeUsesWinUI3ThemeResources()
    {
        foreach (var themeName in new[] { "Light", "Dark" })
        {
            AssertThemeResourceReference(themeName, "InfoBadgeForeground", "TextOnAccentFillColorPrimaryBrush");
            AssertThemeResourceReference(themeName, "InfoBadgeBackground", "AccentFillColorDefaultBrush");
            AssertThemeResourceValue(themeName, "InfoBadgeMinHeight", 4.0);
            AssertThemeResourceValue(themeName, "InfoBadgeMinWidth", 4.0);
            AssertThemeResourceValue(themeName, "InfoBadgeMaxHeight", 16.0);
            AssertThemeResourceValue(themeName, "InfoBadgeValueFontSize", 11.0);
            AssertThemeResourceValue(themeName, "InfoBadgeIconWidth", 12.0);
            AssertThemeResourceValue(themeName, "InfoBadgePadding", new Thickness(0));
            AssertThemeResourceValue(themeName, "IconInfoBadgeFontIconMargin", new Thickness(4, 0, 4, 2));
            AssertThemeResourceValue(themeName, "ValueInfoBadgeTextMargin", new Thickness(4, 0, 4, 2));
            AssertThemeResourceValue(themeName, "IconInfoBadgeIconMargin", new Thickness(4));
        }

        AssertThemeResourceValue("Light", "InfoBadgeIconHeight", 9.0);
        AssertThemeResourceValue("Dark", "InfoBadgeIconHeight", 8.0);
        AssertThemeResourceValue("Light", "SystemFillColorSolidNeutral", Color.FromRgb(0x8A, 0x8A, 0x8A));
        AssertThemeResourceValue("Dark", "SystemFillColorSolidNeutral", Color.FromRgb(0x9D, 0x9D, 0x9D));
        AssertThemeBrushColor("Light", "SystemFillColorSolidNeutralBrush", Color.FromRgb(0x8A, 0x8A, 0x8A));
        AssertThemeBrushColor("Dark", "SystemFillColorSolidNeutralBrush", Color.FromRgb(0x9D, 0x9D, 0x9D));

        AssertThemeResourceReference("HighContrast", "InfoBadgeForeground", "SystemControlHighlightAltChromeWhiteBrush");
        AssertThemeResourceReference("HighContrast", "InfoBadgeBackground", "SystemControlHighlightAccentBrush");
        AssertThemeResourceValue("HighContrast", "InfoBadgeIconHeight", 9.0);
        AssertThemeResourceExists("HighContrast", "SystemFillColorSolidNeutralBrush");
    }

    [TestMethod]
    public void InformationalInfoBadgeUsesWinUI3SolidNeutralBackground()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = (ResourceDictionary)Application.LoadComponent(
                new Uri("/ModernWpf.Controls;component/InfoBadge/InfoBadge.xaml", UriKind.Relative));
            var style = (Style)resources["InformationalDotInfoBadgeStyle"];

            var infoBadge = new ModernWpf.Controls.InfoBadge
            {
                Style = style
            };
            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);
            host.UpdateLayout();

            Assert.AreSame(infoBadge.TryFindResource("SystemFillColorSolidNeutralBrush"), infoBadge.Background);
        });
    }

    [TestMethod]
    public void InfoBadgeValueLessThanNegativeOneThrows()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge();
            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            Assert.ThrowsException<ArgumentException>(() => infoBadge.Value = -10);
        });
    }

    private static DrawingImage CreateTestImageSource()
    {
        return new DrawingImage(
            new GeometryDrawing(
                Brushes.Blue,
                null,
                new RectangleGeometry(new Rect(0, 0, 16, 16))));
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

    private static ContentPresenterEx FindContentPresenter(DependencyObject root, object content)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is ContentPresenterEx presenter && ReferenceEquals(presenter.Content, content))
            {
                return presenter;
            }
        }

        throw new InvalidOperationException("Could not find ContentPresenterEx for the expected content.");
    }

    private static bool ContainsPlainContentPresenter(DependencyObject root)
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant?.GetType() == typeof(ContentPresenter))
            {
                return true;
            }
        }

        return false;
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
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void AssertThemeResourceExists(string themeName, string resourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
    }

    private static void AssertThemeBrushColor(string themeName, string resourceKey, Color expectedColor)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        var brush = themeDictionary[resourceKey] as SolidColorBrush;
        Assert.IsNotNull(brush, $"{themeName}:{resourceKey} should be a SolidColorBrush.");
        Assert.AreEqual(expectedColor, brush!.Color);
    }
}
