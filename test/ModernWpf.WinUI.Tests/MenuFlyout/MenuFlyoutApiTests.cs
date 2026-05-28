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
                AssertLightDarkMenuFlyoutTheme(themeName);
            }

            AssertThemeResourceValue("HighContrast", "MenuFlyoutSeparatorThemeHeight", 1.0);
            AssertThemeResourceValue("HighContrast", "MenuFlyoutPresenterBorderThemeThickness", new Thickness(2));
            AssertThemeResourceValue("HighContrast", "MenuFlyoutItemBorderThickness", new Thickness(0));
            AssertThemeResourceValue("HighContrast", "MenuFlyoutSubItemBorderThickness", new Thickness(0));
            AssertThemeResourceValue("HighContrast", "MenuFlyoutItemThemePadding", new Thickness(11, 9, 11, 10));
            AssertThemeResourceValue("HighContrast", "MenuFlyoutItemThemePaddingNarrow", new Thickness(11, 4, 11, 7));
            AssertThemeResourceValue("HighContrast", "MenuFlyoutItemDoublePlaceholderThemeThickness", new Thickness(56, 0, 0, 0));
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSeparatorBackground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutPresenterBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutPresenterBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackgroundPointerOver", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackgroundPressed", "SystemControlHighlightListMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemBackgroundBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemKeyboardAcceleratorTextForeground", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver", "SystemControlHighlightAltBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed", "SystemControlHighlightAltBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBorderBrushPressed", "SystemControlTransparentRevealBorderBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBorderBrushPointerOver", "SystemControlTransparentRevealBorderBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutItemRevealBorderBrushDisabled", "SystemControlTransparentBrush");
            AssertHighContrastToggleMenuFlyoutItemTheme();
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackgroundPointerOver", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackgroundPressed", "SystemControlHighlightListAccentHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackgroundSubMenuOpened", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemBackgroundBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemForegroundSubMenuOpened", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevron", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronPointerOver", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronPressed", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronSubMenuOpened", "SystemColorHighlightColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemChevronDisabled", "SystemColorGrayTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBackgroundPressed", "SystemControlHighlightAccentRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBackgroundSubMenuOpened", "SystemControlHighlightListLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBackgroundDisabled", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBorderBrush", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBorderBrushPressed", "SystemControlTransparentRevealBorderBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBorderBrushPointerOver", "SystemControlTransparentRevealBorderBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBorderBrushSubMenuOpened", "SystemControlTransparentRevealBorderBrush");
            AssertThemeResourceReference("HighContrast", "MenuFlyoutSubItemRevealBorderBrushDisabled", "SystemControlTransparentBrush");
        });
    }

    private static void AssertLightDarkMenuFlyoutTheme(string themeName)
    {
        AssertThemeResourceValue(themeName, "MenuFlyoutSeparatorThemeHeight", 1.0);
        AssertThemeResourceValue(themeName, "MenuFlyoutPresenterBorderThemeThickness", new Thickness(1));
        AssertThemeResourceValue(themeName, "MenuFlyoutItemBorderThickness", new Thickness(0));
        AssertThemeResourceValue(themeName, "MenuFlyoutSubItemBorderThickness", new Thickness(0));
        AssertThemeResourceValue(themeName, "MenuFlyoutItemThemePadding", new Thickness(11, 9, 11, 10));
        AssertThemeResourceValue(themeName, "MenuFlyoutItemThemePaddingNarrow", new Thickness(11, 4, 11, 7));
        AssertThemeResourceValue(themeName, "MenuFlyoutItemDoublePlaceholderThemeThickness", new Thickness(56, 0, 0, 0));
        AssertThemeResourceReference(themeName, "MenuFlyoutSeparatorBackground", "DividerStrokeColorDefaultBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutPresenterBackground", "AcrylicBackgroundFillColorDefaultBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutPresenterBorderBrush", "SurfaceStrokeColorFlyoutBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemBackgroundPressed", "SubtleFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemBackgroundBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemForegroundPointerOver", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemForegroundPressed", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemKeyboardAcceleratorTextForeground", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBorderBrushPressed", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBorderBrushPointerOver", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutItemRevealBorderBrushDisabled", "SubtleFillColorTransparentBrush");
        AssertLightDarkToggleMenuFlyoutItemTheme(themeName);
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemBackground", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemBackgroundPressed", "SubtleFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemBackgroundSubMenuOpened", "SubtleFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemBackgroundBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemForeground", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemForegroundPointerOver", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemForegroundPressed", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemForegroundSubMenuOpened", "TextFillColorPrimaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemForegroundDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevron", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronPointerOver", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronPressed", "TextFillColorTertiaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronSubMenuOpened", "TextFillColorSecondaryBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemChevronDisabled", "TextFillColorDisabledBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBackgroundPressed", "SystemControlHighlightAccentRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBackgroundSubMenuOpened", "SystemControlHighlightListLowBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBorderBrushPressed", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBorderBrushPointerOver", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBorderBrushSubMenuOpened", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "MenuFlyoutSubItemRevealBorderBrushDisabled", "SubtleFillColorTransparentBrush");
    }

    private static void AssertLightDarkToggleMenuFlyoutItemTheme(string themeName)
    {
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemBackground", "SystemControlTransparentBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemBackgroundPointerOver", "SystemControlHighlightListLowBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemBackgroundPressed", "SystemControlHighlightListMediumBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemBackgroundDisabled", "SystemControlTransparentBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemForeground", "SystemControlForegroundBaseHighBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemKeyboardAcceleratorTextForeground", "SystemControlForegroundBaseMediumBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver", "SystemControlHighlightAltBaseMediumBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundPressed", "SystemControlHighlightAltBaseMediumBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemCheckGlyphForeground", "SystemControlForegroundBaseMediumHighBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemCheckGlyphForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemCheckGlyphForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemCheckGlyphForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBackgroundDisabled", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBorderBrush", "SubtleFillColorTransparentBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBorderBrushPressed", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBorderBrushPointerOver", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference(themeName, "ToggleMenuFlyoutItemRevealBorderBrushDisabled", "SubtleFillColorTransparentBrush");
    }

    private static void AssertHighContrastToggleMenuFlyoutItemTheme()
    {
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemBackground", "SystemControlTransparentBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemBackgroundPointerOver", "SystemControlHighlightListLowBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemBackgroundPressed", "SystemControlHighlightListMediumBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemBackgroundDisabled", "SystemControlTransparentBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemForeground", "SystemControlForegroundBaseHighBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemKeyboardAcceleratorTextForeground", "SystemControlForegroundBaseMediumBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver", "SystemControlHighlightAltBaseMediumBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundPressed", "SystemControlHighlightAltBaseMediumBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemCheckGlyphForeground", "SystemControlForegroundBaseMediumHighBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemCheckGlyphForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemCheckGlyphForegroundPressed", "SystemControlHighlightAltBaseHighBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemCheckGlyphForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBackground", "SystemControlTransparentRevealBackgroundBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBackgroundDisabled", "SystemControlTransparentBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBorderBrush", "SystemControlTransparentBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBorderBrushPressed", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBorderBrushPointerOver", "SystemControlTransparentRevealBorderBrush");
        AssertThemeResourceReference("HighContrast", "ToggleMenuFlyoutItemRevealBorderBrushDisabled", "SystemControlTransparentBrush");
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
