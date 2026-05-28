using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.MenuFlyoutTests;

[TestClass]
public class MenuFlyoutApiTests
{
    [TestMethod]
    public void TargetTracksOpenMenuFlyoutLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Content = "Target",
                Width = 120,
                Height = 36
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });
            bool cancelClosing = true;

            menuFlyout.Closing += (_, args) => args.Cancel = cancelClosing;

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            Assert.IsNull(menuFlyout.Target);

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);

            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);

            cancelClosing = false;
            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuFlyout.IsOpen);
            Assert.IsNull(menuFlyout.Target);
        });
    }

    [TestMethod]
    public void ClosingCanCancelHideLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Content = "Target",
                Width = 120,
                Height = 36
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            var events = new List<string>();
            bool cancelClosing = true;

            menuFlyout.Opened += (_, _) => events.Add("Opened");
            menuFlyout.Closing += (_, args) =>
            {
                events.Add($"Closing:{args.Cancel}");
                args.Cancel = cancelClosing;
                events.Add($"Cancel:{args.Cancel}");
            };
            menuFlyout.Closed += (_, _) => events.Add("Closed");

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);

            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            AssertEvents(
                events,
                "Opened",
                "Closing:False",
                "Cancel:True");

            cancelClosing = false;
            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuFlyout.IsOpen);
            AssertEvents(
                events,
                "Opened",
                "Closing:False",
                "Cancel:True",
                "Closing:False",
                "Cancel:False",
                "Closed");
        });
    }

    [TestMethod]
    public void OpeningSecondMenuFlyoutClosesFirstLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var firstTarget = new Button { Content = "First", Width = 120, Height = 36 };
            var secondTarget = new Button { Content = "Second", Width = 120, Height = 36 };
            var root = new StackPanel
            {
                Children =
                {
                    firstTarget,
                    secondTarget
                }
            };
            var firstFlyout = new MenuFlyout();
            var secondFlyout = new MenuFlyout();
            var events = new List<string>();

            firstFlyout.Items.Add(new MenuItem { Header = "Copy" });
            secondFlyout.Items.Add(new MenuItem { Header = "Paste" });
            firstFlyout.Opened += (_, _) => events.Add("FirstOpened");
            firstFlyout.Closed += (_, _) => events.Add("FirstClosed");
            secondFlyout.Opened += (_, _) => events.Add("SecondOpened");

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            firstFlyout.ShowAt(firstTarget);
            WpfTestHost.DoEvents();

            Assert.IsTrue(firstFlyout.IsOpen);
            Assert.AreSame(firstTarget, firstFlyout.Target);

            secondFlyout.ShowAt(secondTarget);
            WpfTestHost.DoEvents();

            Assert.IsFalse(firstFlyout.IsOpen);
            Assert.IsNull(firstFlyout.Target);
            Assert.IsTrue(secondFlyout.IsOpen);
            Assert.AreSame(secondTarget, secondFlyout.Target);
            AssertEvents(events, "FirstOpened", "FirstClosed", "SecondOpened");

            secondFlyout.Hide();
            WpfTestHost.DoEvents();
        });
    }

    [TestMethod]
    public void PlacementTargetUnloadedHidesMenuFlyoutLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button { Content = "Target", Width = 120, Height = 36 };
            var root = new StackPanel
            {
                Children =
                {
                    target
                }
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);

            root.Children.Remove(target);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuFlyout.IsOpen);
            Assert.IsNull(menuFlyout.Target);
        });
    }

    [TestMethod]
    public void ShowAtWithOptionsAppliesTargetPointPlacementAndShowModeLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Content = "Target",
                Width = 120,
                Height = 36
            };
            var menuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(
                target,
                new FlyoutShowOptions
                {
                    Position = new Point(18, 9),
                    Placement = FlyoutPlacementMode.Right,
                    ShowMode = FlyoutShowMode.Transient
                });
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);
            Assert.AreEqual(FlyoutPlacementMode.Right, menuFlyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Transient, menuFlyout.ShowMode);
            Assert.AreEqual(new Rect(18, 9, 0, 0), menuFlyout.GetPlacementRectangle(target));

            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.AreEqual(FlyoutPlacementMode.Bottom, menuFlyout.GetEffectivePlacement());
        });
    }

    [TestMethod]
    public void ShowAtWithOptionsSameTargetAppliesStateBeforeNoOpLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Content = "Target",
                Width = 120,
                Height = 36
            };
            var menuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });
            int openedCount = 0;
            menuFlyout.Opened += (_, _) => openedCount++;

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(FlyoutPlacementMode.Bottom, menuFlyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Standard, menuFlyout.ShowMode);

            menuFlyout.ShowAt(
                target,
                new FlyoutShowOptions
                {
                    Placement = FlyoutPlacementMode.Right,
                    ShowMode = FlyoutShowMode.Transient
                });
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(target, menuFlyout.Target);
            Assert.AreEqual(1, openedCount);
            Assert.AreEqual(FlyoutPlacementMode.Right, menuFlyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Transient, menuFlyout.ShowMode);

            menuFlyout.Hide();
            WpfTestHost.DoEvents();

            Assert.AreEqual(FlyoutPlacementMode.Bottom, menuFlyout.GetEffectivePlacement());
            Assert.AreEqual(FlyoutShowMode.Transient, menuFlyout.ShowMode);
        });
    }

    [TestMethod]
    public void ShowAtWithOptionsAllowsNullTargetWhenPositionProvidedLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var root = new Grid { Width = 240, Height = 180 };
            var menuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Top
            };
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            using var host = new TestWindowHost(root, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(
                null,
                new FlyoutShowOptions
                {
                    Position = new Point(18, 9),
                    Placement = FlyoutPlacementMode.Right,
                    ShowMode = FlyoutShowMode.Transient
                });
            WpfTestHost.DoEvents();

            Assert.IsTrue(menuFlyout.IsOpen);
            Assert.AreSame(root, menuFlyout.Target);
            Assert.AreEqual(FlyoutPlacementMode.Right, menuFlyout.GetEffectivePlacement());
            Assert.AreEqual(new Rect(18, 9, 0, 0), menuFlyout.GetPlacementRectangle(root));

            menuFlyout.Hide();
            WpfTestHost.DoEvents();
        });
    }

    [TestMethod]
    public void PresenterShadowFollowsWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var target = new Button
            {
                Content = "Target",
                Width = 120,
                Height = 36
            };
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });

            using var host = new TestWindowHost(target, width: 320, height: 220);
            host.UpdateLayout();

            menuFlyout.ShowAt(target);
            WpfTestHost.DoEvents();

            try
            {
                var presenter = menuFlyout.Presenter;
                presenter.ApplyTemplate();
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                Assert.IsFalse(presenter.HasDropShadow);

                var chrome = VisualTreeTestHelper.FindDescendant<ThemeShadowChrome>(presenter)
                    ?? throw new AssertFailedException("Expected MenuFlyoutPresenter to use ThemeShadowChrome.");
                Assert.AreEqual(presenter.IsDefaultShadowEnabled, chrome.IsShadowEnabled);

                presenter.IsDefaultShadowEnabled = true;
                WpfTestHost.DoEvents();
                Assert.IsTrue(chrome.IsShadowEnabled);

                presenter.IsDefaultShadowEnabled = false;
                WpfTestHost.DoEvents();
                Assert.IsFalse(chrome.IsShadowEnabled);

                Assert.AreEqual(32.0, chrome.Depth);
                Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, chrome.WindowedPopupInsetMode);
                Assert.AreEqual(new Thickness(10, 2, 10, 18), chrome.PopupShadowPadding);
                Assert.AreEqual(presenter.CornerRadius, chrome.CornerRadius);

                var border = VisualTreeTestHelper.FindDescendant<BorderEx>(presenter)
                    ?? throw new AssertFailedException("Expected MenuFlyoutPresenter template to use BorderEx for WinUI BackgroundSizing.");
                Assert.AreEqual(BackgroundSizing.InnerBorderEdge, border.BackgroundSizing);
                Assert.AreEqual(presenter.Background, border.Background);
                Assert.AreEqual(presenter.BorderBrush, border.BorderBrush);
                Assert.AreEqual(presenter.BorderThickness, border.BorderThickness);
            }
            finally
            {
                menuFlyout.Hide();
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2MenuFlyoutHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "MenuFlyoutSeparatorBackground", "DividerStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "MenuFlyoutPresenterBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "MenuFlyoutPresenterBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "MenuFlyoutItemBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "MenuFlyoutItemForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "MenuFlyoutItemForegroundDisabled", "TextFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "MenuFlyoutSubItemForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceValue(themeName, "MenuFlyoutPresenterBorderThemeThickness", new Thickness(1));
            }

            AssertThemeResourceReference("HighContrast", "MenuFlyoutSeparatorBackground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutPresenterBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutPresenterBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackgroundPointerOver", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackgroundPressed", "SystemControlHighlightListMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackgroundSubMenuOpened", "SystemControlHighlightListLowBrush");
            AssertThemeResourceValue("HighContrast", "MenuFlyoutPresenterBorderThemeThickness", new Thickness(2));
        });
    }

    private static void AssertEvents(List<string> actual, params string[] expected)
    {
        Assert.AreEqual(string.Join("|", expected), string.Join("|", actual));
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
