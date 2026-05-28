using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.FlyoutTests;

[TestClass]
public class FlyoutPresenterApiTests
{
    [TestMethod]
    public void FlyoutPresenterAcceptsWinUIContentPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new TransitionCollection();
            var presenter = new FlyoutPresenter
            {
                ContentTransitions = transitions,
                CornerRadius = new CornerRadius(4)
            };

            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(new CornerRadius(4), presenter.CornerRadius);
        });
    }

    [TestMethod]
    public void FlyoutPresenterTemplateUsesWinUIContentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border { Width = 80, Height = 24 };
            var transitions = new TransitionCollection();
            var background = new SolidColorBrush(Colors.Red);
            var borderBrush = new SolidColorBrush(Colors.Blue);
            var presenter = new FlyoutPresenter
            {
                Content = content,
                Background = background,
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1, 2, 3, 4),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(6, 7, 8, 9),
                ContentTransitions = transitions,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(presenter, width: 240, height: 160);

            var chrome = VisualTreeTestHelper.FindDescendant<BorderEx>(presenter)
                ?? throw new AssertFailedException("Expected FlyoutPresenter template to use BorderEx for WinUI chrome.");
            var contentPresenter = VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(presenter)
                ?? throw new AssertFailedException("Expected FlyoutPresenter template to use ContentPresenterEx for its content slot.");

            Assert.AreSame(background, chrome.Background);
            Assert.AreSame(borderBrush, chrome.BorderBrush);
            Assert.AreEqual(new Thickness(1, 2, 3, 4), chrome.BorderThickness);
            Assert.AreEqual(new CornerRadius(5), chrome.CornerRadius);
            Assert.AreEqual(BackgroundSizing.InnerBorderEdge, chrome.BackgroundSizing);

            Assert.AreSame(content, contentPresenter.Content);
            Assert.AreEqual(new Thickness(6, 7, 8, 9), contentPresenter.Margin);
            Assert.AreSame(transitions, contentPresenter.ContentTransitions);
            Assert.AreEqual(HorizontalAlignment.Right, contentPresenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, contentPresenter.VerticalAlignment);
        });
    }

    [TestMethod]
    public void FlyoutPresenterTemplateUsesSourceThemeShadow()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new FlyoutPresenter
            {
                Content = "Flyout",
                CornerRadius = new CornerRadius(6),
                IsDefaultShadowEnabled = false
            };

            using var host = new TestWindowHost(presenter, width: 240, height: 160);

            var shadowChrome = VisualTreeHelper.GetChild(presenter, 0) as ThemeShadowChrome
                ?? throw new AssertFailedException("Expected FlyoutPresenter template root to be ThemeShadowChrome.");
            var chrome = VisualTreeTestHelper.FindDescendant<BorderEx>(presenter)
                ?? throw new AssertFailedException("Expected FlyoutPresenter template to use BorderEx for WinUI chrome.");

            Assert.AreSame(chrome, shadowChrome.Child);
            Assert.AreEqual(32.0, shadowChrome.Depth);
            Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, shadowChrome.WindowedPopupInsetMode);
            Assert.AreEqual(new Thickness(10, 2, 10, 18), shadowChrome.PopupShadowPadding);
            Assert.AreEqual(new CornerRadius(6), shadowChrome.CornerRadius);
            Assert.IsFalse(shadowChrome.IsShadowEnabled);

            presenter.IsDefaultShadowEnabled = true;
            host.UpdateLayout();

            Assert.IsTrue(shadowChrome.IsShadowEnabled);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2FlyoutPresenterHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                AssertThemeResourceValue(themeName, "FlyoutThemeMaxHeight", 758.0);
                AssertThemeResourceValue(themeName, "FlyoutThemeMaxWidth", 456.0);
                AssertThemeResourceValue(themeName, "FlyoutThemeMinHeight", 40.0);
                AssertThemeResourceValue(themeName, "FlyoutThemeMinWidth", 96.0);
                AssertThemeResourceValue(themeName, "FlyoutThemeTouchMinWidth", 240.0);
                AssertThemeResourceValue(themeName, "FlyoutBorderThemeThickness", new Thickness(1));
                AssertThemeResourceValue(themeName, "FlyoutBorderThemePadding", new Thickness(0));
                AssertThemeResourceValue(themeName, "FlyoutContentThemePadding", new Thickness(12, 11, 12, 12));
            }

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "FlyoutPresenterBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlyoutBorderThemeBrush", "SurfaceStrokeColorFlyoutBrush");
            }

            AssertThemeResourceReference("HighContrast", "FlyoutPresenterBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "FlyoutBorderThemeBrush", "SystemColorWindowTextColorBrush");
        });
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
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }
}
