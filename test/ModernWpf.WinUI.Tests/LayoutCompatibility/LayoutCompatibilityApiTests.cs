using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using ModernCanvasEx = ModernWpf.Controls.CanvasEx;
using ModernContentControlEx = ModernWpf.Controls.ContentControlEx;
using ModernGridEx = ModernWpf.Controls.GridEx;
using ModernItemsStackPanel = ModernWpf.Controls.ItemsStackPanel;
using ModernItemsWrapGrid = ModernWpf.Controls.ItemsWrapGrid;
using ModernRelativePanel = ModernWpf.Controls.RelativePanel;
using ModernStackPanelEx = ModernWpf.Controls.StackPanelEx;
using ModernVariableSizedWrapGrid = ModernWpf.Controls.VariableSizedWrapGrid;
using ModernWrapGrid = ModernWpf.Controls.WrapGrid;
using CultureInfo = System.Globalization.CultureInfo;
using NumberStyles = System.Globalization.NumberStyles;

namespace ModernWpf.WinUI.Tests.LayoutCompatibility;

[TestClass]
public class LayoutCompatibilityApiTests
{
    [TestMethod]
    public void BorderExAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) },
                ChildTransitions = new ModernWpf.Media.Animation.TransitionCollection()
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, border.BackgroundSizing);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), border.BackgroundTransition.Duration);
            Assert.IsNotNull(border.ChildTransitions);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeUsesDepthDrivenSoftwareRenderer()
    {
        WpfTestHost.Run(() =>
        {
            var chrome = new ThemeShadowChrome
            {
                Depth = 32,
                CornerRadius = new CornerRadius(8),
                Child = new Border
                {
                    Width = 80,
                    Height = 32,
                    Background = Brushes.White
                }
            };

            using var host = new TestWindowHost(chrome, width: 180, height: 120);
            host.UpdateLayout();

            Assert.IsTrue(chrome.UsesSoftwareRenderer);
            Assert.AreEqual(new Thickness(16, 8, 16, 24), chrome.ShadowPadding);
            Assert.IsFalse(FindVisualChildren<Border>(chrome).Any(border => border.Effect is System.Windows.Media.Effects.BlurEffect));

            chrome.Depth = 64;
            host.UpdateLayout();

            Assert.AreEqual(new Thickness(32, 16, 32, 48), chrome.ShadowPadding);

            var lightLowElevation = ThemeShadowChrome.ThemeShadowRenderer.GetLayerOpacities(16, ElementTheme.Light);
            Assert.AreEqual(0, lightLowElevation.Ambient, 0.001);
            Assert.AreEqual(0.14, lightLowElevation.Directional, 0.001);

            var darkLowElevation = ThemeShadowChrome.ThemeShadowRenderer.GetLayerOpacities(16, ElementTheme.Dark);
            Assert.AreEqual(0, darkLowElevation.Ambient, 0.001);
            Assert.AreEqual(0.26, darkLowElevation.Directional, 0.001);

            var lightHighElevation = ThemeShadowChrome.ThemeShadowRenderer.GetLayerOpacities(64, ElementTheme.Light);
            Assert.AreEqual(0.15, lightHighElevation.Ambient, 0.001);
            Assert.AreEqual(0.19, lightHighElevation.Directional, 0.001);

            var darkHighElevation = ThemeShadowChrome.ThemeShadowRenderer.GetLayerOpacities(64, ElementTheme.Dark);
            Assert.AreEqual(0.37, darkHighElevation.Ambient, 0.001);
            Assert.AreEqual(0.37, darkHighElevation.Directional, 0.001);

            chrome.Depth = 0;
            host.UpdateLayout();

            Assert.AreEqual(new Thickness(), chrome.ShadowPadding);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeExposesWinUIShapedShadowAliases()
    {
        WpfTestHost.Run(() =>
        {
            var chrome = new ThemeShadowChrome();

            Assert.IsNotNull(chrome.Shadow);
            Assert.IsTrue(chrome.IsShadowEnabled);
            Assert.IsTrue(chrome.ReservesShadowSpace);
            Assert.AreEqual(32, chrome.TranslationZ);
            Assert.AreEqual(chrome.Depth, chrome.TranslationZ);

            chrome.TranslationZ = 64;
            Assert.AreEqual(64, chrome.Depth);
            Assert.AreEqual(new Thickness(32, 16, 32, 48), chrome.ShadowPadding);

            chrome.Depth = 16;
            Assert.AreEqual(16, chrome.TranslationZ);
            Assert.AreEqual(new Thickness(8, 4, 8, 12), chrome.ShadowPadding);

            chrome.Shadow = null;
            Assert.IsFalse(chrome.IsShadowEnabled);

            chrome.IsShadowEnabled = true;
            Assert.IsNotNull(chrome.Shadow);

            var shadow = new ThemeShadow();
            chrome.Shadow = shadow;
            Assert.AreSame(shadow, chrome.Shadow);
            Assert.IsTrue(chrome.IsShadowEnabled);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeAcceptsWinUIShapedShadowPropertyElement()
    {
        WpfTestHost.Run(() =>
        {
            var chrome = (ThemeShadowChrome)XamlReader.Parse(
                @"<ui:ThemeShadowChrome xmlns:ui=""http://schemas.modernwpf.com/2019"" TranslationZ=""64"">
                    <ui:ThemeShadowChrome.Shadow>
                        <ui:ThemeShadow />
                    </ui:ThemeShadowChrome.Shadow>
                  </ui:ThemeShadowChrome>");

            Assert.IsNotNull(chrome.Shadow);
            Assert.IsTrue(chrome.IsShadowEnabled);
            Assert.AreEqual(64, chrome.TranslationZ);
            Assert.AreEqual(64, chrome.Depth);
            Assert.AreEqual(new Thickness(32, 16, 32, 48), chrome.ShadowPadding);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeUsesWinUIWindowedPopupInsets()
    {
        WpfTestHost.Run(() =>
        {
            var chrome = new ThemeShadowChrome { Depth = 32 };

            Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Default, chrome.WindowedPopupInsetMode);
            Assert.AreEqual(new Thickness(16, 8, 16, 24), chrome.PopupShadowPadding);

            chrome.WindowedPopupInsetMode = ThemeShadowChromeWindowedPopupInsetMode.Medium;
            Assert.AreEqual(new Thickness(10, 2, 10, 18), chrome.PopupShadowPadding);

            chrome.WindowedPopupInsetMode = ThemeShadowChromeWindowedPopupInsetMode.Small;
            Assert.AreEqual(new Thickness(4, 1, 4, 8), chrome.PopupShadowPadding);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeCanRenderSourceTranslationWithoutReservingLayoutSpace()
    {
        WpfTestHost.Run(() =>
        {
            var root = new Grid
            {
                Width = 272,
                Height = 272,
                Background = Brushes.White
            };
            var chrome = new ThemeShadowChrome
            {
                Depth = 32,
                TranslationZ = 32,
                ReservesShadowSpace = false,
                Margin = new Thickness(36),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Child = new Border
                {
                    Width = 200,
                    Height = 200,
                    Background = Brushes.Transparent
                }
            };
            root.Children.Add(chrome);

            ArrangeElement(root, 272, 272);

            Assert.AreEqual(200, chrome.ActualWidth, 0.1);
            Assert.AreEqual(200, chrome.ActualHeight, 0.1);
            Assert.AreEqual(new Point(36, 36), chrome.TranslatePoint(new Point(), root));
            Assert.AreEqual(new Point(36, 36), ((FrameworkElement)chrome.Child).TranslatePoint(new Point(), root));

            chrome.TranslationZ = 48;
            ArrangeElement(root, 272, 272);

            Assert.AreEqual(48, chrome.Depth);
            Assert.AreEqual(200, chrome.ActualWidth, 0.1);
            Assert.AreEqual(200, chrome.ActualHeight, 0.1);
            Assert.AreEqual(new Point(36, 36), chrome.TranslatePoint(new Point(), root));
            Assert.AreEqual(new Point(36, 36), ((FrameworkElement)chrome.Child).TranslatePoint(new Point(), root));
        });
    }

    [TestMethod]
    public void ThemeShadowChromePopupInsetsAreNotDoubleAppliedAsChildMargin()
    {
        WpfTestHost.Run(() =>
        {
            var root = new Grid
            {
                Width = 160,
                Height = 120,
                Background = Brushes.White
            };
            using var host = new TestWindowHost(root, width: 160, height: 120);

            var chrome = new ThemeShadowChrome
            {
                Depth = 32,
                WindowedPopupInsetMode = ThemeShadowChromeWindowedPopupInsetMode.Medium,
                Child = new Border
                {
                    Width = 50,
                    Height = 20,
                    Background = Brushes.Transparent
                }
            };
            var popup = new Popup
            {
                AllowsTransparency = true,
                Child = chrome,
                Placement = PlacementMode.Bottom,
                PlacementTarget = root
            };
            root.Children.Add(popup);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();
                chrome.UpdateLayout();

                Assert.AreEqual(new Thickness(10, 2, 10, 18), chrome.PopupShadowPadding);
                Assert.AreEqual(new Thickness(), chrome.Margin);
                Assert.AreEqual(70, chrome.ActualWidth, 0.1);
                Assert.AreEqual(40, chrome.ActualHeight, 0.1);
                Assert.AreEqual(new Point(10, 2), ((FrameworkElement)chrome.Child).TranslatePoint(new Point(), chrome));
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void ThemeShadowRendererReportsCalibrationMetrics()
    {
        WpfTestHost.Run(() =>
        {
            var dpi = new DpiScale(1, 1);
            var contentSize = new Size(80, 40);
            var cornerRadius = new CornerRadius(8);

            var light16 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(contentSize, cornerRadius, 16, ElementTheme.Light, dpi);
            AssertShadowMetrics(
                light16,
                bitmapWidth: 96,
                bitmapHeight: 56,
                contentLeft: 8,
                contentTop: 4,
                peakAlpha: 32,
                nonZeroPixelCount: 1432,
                nonZeroBounds: new Int32Rect(2, 2, 92, 52),
                alphaCentroidX: 47.5,
                alphaCentroidY: 42.03,
                alphaCentroidTolerance: 0.001);

            var dark16 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(contentSize, cornerRadius, 16, ElementTheme.Dark, dpi);
            AssertShadowMetrics(
                dark16,
                bitmapWidth: 96,
                bitmapHeight: 56,
                contentLeft: 8,
                contentTop: 4,
                peakAlpha: 60,
                nonZeroPixelCount: 1672,
                nonZeroBounds: new Int32Rect(1, 1, 94, 54),
                alphaCentroidX: 47.5,
                alphaCentroidY: 42.069,
                alphaCentroidTolerance: 0.001);
            Assert.IsTrue(dark16.PeakAlpha > light16.PeakAlpha);

            var light64 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(contentSize, cornerRadius, 64, ElementTheme.Light, dpi);
            AssertShadowMetrics(
                light64,
                bitmapWidth: 144,
                bitmapHeight: 104,
                contentLeft: 32,
                contentTop: 16,
                peakAlpha: 65,
                nonZeroPixelCount: 6832,
                nonZeroBounds: new Int32Rect(8, 8, 128, 88),
                alphaCentroidX: 71.5,
                alphaCentroidY: 60.108,
                alphaCentroidTolerance: 0.001);
            Assert.IsTrue(light64.NonZeroPixelCount > light16.NonZeroPixelCount);
            Assert.IsTrue(light64.PeakAlpha > light16.PeakAlpha);
            Assert.IsTrue(light64.AlphaCentroidY - light64.ContentCenterY > light16.AlphaCentroidY - light16.ContentCenterY);

            var empty = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(contentSize, cornerRadius, 0, ElementTheme.Light, dpi);
            Assert.IsFalse(empty.HasShadow);
            Assert.AreEqual(0, empty.BitmapWidth);
            Assert.AreEqual(0, empty.BitmapHeight);
        });
    }

    [TestMethod]
    public void ThemeShadowRendererMatchesWinUIMockDCompMasterGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var dpi = new DpiScale(1, 1);

            // WinUI source:
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowBasicDropShadow.master.xml
            // has a 100x100 caster at Translation.Z=32 and a DropShadowVisual sprite
            // sized 132x132 with OffsetX=-16 and OffsetY=-8.
            var z32 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(100, 100),
                new CornerRadius(),
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                z32,
                bitmapWidth: 132,
                bitmapHeight: 132,
                contentLeft: 16,
                contentTop: 8,
                contentWidth: 100,
                contentHeight: 100);

            // WinUI source:
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowDynamicCornerRadius.4_CR.master.xml
            // keeps the same outer DropShadowVisual geometry for RadiusX/RadiusY=4 and
            // only adjusts the NineGridBrush insets. ModernWpf renders a direct rounded mask,
            // so the corresponding parity requirement is stable outer geometry.
            var roundedZ32 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(100, 100),
                new CornerRadius(4),
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                roundedZ32,
                bitmapWidth: 132,
                bitmapHeight: 132,
                contentLeft: 16,
                contentTop: 8,
                contentWidth: 100,
                contentHeight: 100);

            // WinUI source:
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowWindowedPopup.Shadow.master.xml
            // has a 50x50 popup caster at Translation.Z=32 and an 82x82 DropShadowVisual
            // sprite with the same -16,-8 offset. The popup HWND is 140px at 200%, i.e.
            // 70 DIPs: child 50 + source medium insets 10+10 and 2+18.
            var popupZ32 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(50, 50),
                new CornerRadius(),
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                popupZ32,
                bitmapWidth: 82,
                bitmapHeight: 82,
                contentLeft: 16,
                contentTop: 8,
                contentWidth: 50,
                contentHeight: 50);

            var popupChrome = new ThemeShadowChrome
            {
                Depth = 32,
                WindowedPopupInsetMode = ThemeShadowChromeWindowedPopupInsetMode.Medium
            };
            var popupInsets = popupChrome.PopupShadowPadding;
            Assert.AreEqual(new Thickness(10, 2, 10, 18), popupInsets);
            Assert.AreEqual(70, 50 + popupInsets.Left + popupInsets.Right);
            Assert.AreEqual(70, 50 + popupInsets.Top + popupInsets.Bottom);

            // WinUI source:
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowWindowedPopup125.Shadow.master.xml
            // has a 50.4x50.4 DIP popup caster at 125% scale and a 82.4x82.4 DIP
            // DropShadowVisual sprite. In pixels, that maps to a 103x103 bitmap with
            // 20px/10px content offsets and 63x63px content bounds.
            var popup125Z32 = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(50.4, 50.4),
                new CornerRadius(),
                32,
                ElementTheme.Light,
                new DpiScale(1.25, 1.25));
            AssertWinUIMockDCompShadowGeometry(
                popup125Z32,
                bitmapWidth: 103,
                bitmapHeight: 103,
                contentLeft: 20,
                contentTop: 10,
                contentWidth: 63,
                contentHeight: 63);
            Assert.AreEqual(82.4, popup125Z32.BitmapWidth / 1.25, 0.001);
            Assert.AreEqual(82.4, popup125Z32.BitmapHeight / 1.25, 0.001);

            // WinUI source pixel masters:
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowSystemThemeRedrawRTB.Light.1.master.png
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowSystemThemeRedrawRTB.Dark.1.master.png
            // The source sample renders a 50x50 rounded caster at Canvas.Left/Top=25 in a 100x100
            // white RenderTargetBitmap. ModernWpf's shadow bitmap is placed at 25-contentOffset.
            var pixelLight = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(50, 50),
                new CornerRadius(4),
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIPixelMasterComparableShadow(
                pixelLight,
                canvasOffsetX: 9,
                canvasOffsetY: 17,
                expectedCanvasBounds: new Int32Rect(14, 22, 72, 72),
                expectedPeakDarkening: 31,
                expectedShadowPixels: 2330,
                expectedCanvasCentroidX: 49.402,
                expectedCanvasCentroidY: 71.869);

            var pixelDark = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(50, 50),
                new CornerRadius(4),
                32,
                ElementTheme.Dark,
                dpi);
            AssertWinUIPixelMasterComparableShadow(
                pixelDark,
                canvasOffsetX: 9,
                canvasOffsetY: 17,
                expectedCanvasBounds: new Int32Rect(14, 21, 72, 74),
                expectedPeakDarkening: 58,
                expectedShadowPixels: 2542,
                expectedCanvasCentroidX: 49.356,
                expectedCanvasCentroidY: 71.786);
        });
    }

    [TestMethod]
    public void ThemeShadowRendererMatchesWinUIControlDropShadowMasters()
    {
        WpfTestHost.Run(() =>
        {
            var dpi = new DpiScale(1, 1);
            var overlayRadius = new CornerRadius(8);

            // WinUI source:
            // Controls_Flyout_FlyoutIntegrationTests_CanFlyoutOpenCloseDropShadow.master.xml
            // has a 316x134 presenter caster at Translation.Z=32 and a 348x166
            // DropShadowVisual sprite with OffsetX=-16 and OffsetY=-8.
            var flyout = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(316, 134),
                overlayRadius,
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                flyout,
                bitmapWidth: 348,
                bitmapHeight: 166,
                contentLeft: 16,
                contentTop: 8,
                contentWidth: 316,
                contentHeight: 134);

            // WinUI source:
            // Controls_MenuFlyout_MenuFlyoutIntegrationTests_CanMenuFlyoutOpenCloseDropShadow.master.xml
            // has a 302x131 presenter caster at Translation.Z=32 and a 334x163
            // DropShadowVisual sprite with OffsetX=-16 and OffsetY=-8.
            var menuFlyout = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(302, 131),
                overlayRadius,
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                menuFlyout,
                bitmapWidth: 334,
                bitmapHeight: 163,
                contentLeft: 16,
                contentTop: 8,
                contentWidth: 302,
                contentHeight: 131);

            // WinUI source:
            // Controls_CommandBar_CommandBarIntegrationTests_CanOpenAndCloseUsingMoreButtonDropShadow.master.xml
            // has a 400x49 overflow caster at Translation.Z=32 and a 432x81
            // DropShadowVisual sprite with OffsetX=-16 and OffsetY=-8.
            var commandBarOverflow = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(400, 49),
                overlayRadius,
                32,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                commandBarOverflow,
                bitmapWidth: 432,
                bitmapHeight: 81,
                contentLeft: 16,
                contentTop: 8,
                contentWidth: 400,
                contentHeight: 49);

            // WinUI source:
            // Controls_ContentDialog_ContentDialogIntegrationTests_CanOpenAndCloseDropShadow.master.xml
            // has a 320x189 dialog caster at Translation.Z=128 and a 448x317
            // DropShadowVisual sprite with OffsetX=-64 and OffsetY=-32.
            var contentDialog = ThemeShadowChrome.ThemeShadowRenderer.GetRenderMetrics(
                new Size(320, 189),
                overlayRadius,
                128,
                ElementTheme.Light,
                dpi);
            AssertWinUIMockDCompShadowGeometry(
                contentDialog,
                bitmapWidth: 448,
                bitmapHeight: 317,
                contentLeft: 64,
                contentTop: 32,
                contentWidth: 320,
                contentHeight: 189);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeRendersHollowCenteredVisualShadow()
    {
        WpfTestHost.Run(() =>
        {
            var chrome = new ThemeShadowChrome
            {
                Depth = 32,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Child = new Border
                {
                    Width = 50,
                    Height = 50,
                    Background = Brushes.Transparent
                }
            };
            var root = new Grid
            {
                Width = 90,
                Height = 90,
                Background = Brushes.White
            };
            root.Children.Add(chrome);

            var center = RenderElementPixel(root, 41, 33, 90, 90);
            var lowerShadow = RenderElementPixel(root, 41, 63, 90, 90);
            var rightShadow = RenderElementPixel(root, 75, 33, 90, 90);

            Assert.IsTrue(center.R >= 250 && center.G >= 250 && center.B >= 250 && center.A == 255, $"Expected hollow shadow center to leave the transparent child area white. Pixel={center}");
            Assert.IsTrue(lowerShadow.R < center.R - 4 && lowerShadow.A == 255, $"Expected rendered shadow below the caster. Pixel={lowerShadow}");
            Assert.IsTrue(rightShadow.R < center.R && rightShadow.A == 255, $"Expected rendered shadow beside the caster. Pixel={rightShadow}");
        });
    }

    [TestMethod]
    public void ThemeShadowChromeShadowVisualIsTransparentForInputLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            var chrome = new ThemeShadowChrome
            {
                Depth = 32,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Child = new Border
                {
                    Width = 50,
                    Height = 50,
                    Background = Brushes.Transparent
                }
            };

            var root = new Grid
            {
                Width = 100,
                Height = 100,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            root.Children.Add(chrome);

            using var host = new TestWindowHost(root, width: 120, height: 120);
            host.UpdateLayout();

            var child = (FrameworkElement)chrome.Child;
            var childOrigin = child.TranslatePoint(new Point(), chrome);
            Assert.AreEqual(new Point(16, 8), childOrigin);

            Assert.IsNull(
                chrome.InputHitTest(new Point(4, 4)),
                "The top-left shadow extent should not be hit-testable.");
            Assert.IsNull(
                chrome.InputHitTest(new Point(41, 70)),
                "The lower shadow extent should not be hit-testable.");
            Assert.IsNotNull(
                chrome.InputHitTest(new Point(childOrigin.X + 10, childOrigin.Y + 10)),
                "The caster child should remain hit-testable.");
        });
    }

    [TestMethod]
    public void ThemeShadowChromeTracksCasterOpacityLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            // WinUI source:
            // Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowOpacity.master.xml
            // sets the caster Opacity to 0.5 and the generated DropShadowVisual
            // also has Opacity=0.5.
            var (root, chrome) = CreateThemeShadowSourceCanvas(ElementTheme.Light);
            var caster = (UIElement)chrome.Child;

            var fullOpacityStats = MeasureRenderedShadowPixels(root, 100, 100);
            AssertWinUIRenderedPixelMasterComparableShadow(
                fullOpacityStats,
                expectedCanvasBounds: new Int32Rect(14, 22, 72, 72),
                expectedPeakDarkening: 31,
                expectedShadowPixels: 2330,
                expectedCanvasCentroidX: 49.402,
                expectedCanvasCentroidY: 71.869);

            caster.Opacity = 0.5;
            root.UpdateLayout();
            WpfTestHost.DoEvents();

            var halfOpacityStats = MeasureRenderedShadowPixels(root, 100, 100);
            AssertNear(fullOpacityStats.PeakDarkening * 0.5, halfOpacityStats.PeakDarkening, 3, $"Caster opacity should scale the shadow peak darkening. Stats={halfOpacityStats}");
            AssertNear(fullOpacityStats.Bounds.X, halfOpacityStats.Bounds.X, 3, $"Caster opacity should preserve shadow geometry. Stats={halfOpacityStats}");
            AssertNear(fullOpacityStats.Bounds.Y, halfOpacityStats.Bounds.Y, 3, $"Caster opacity should preserve shadow geometry. Stats={halfOpacityStats}");
            AssertNear(fullOpacityStats.Bounds.Width, halfOpacityStats.Bounds.Width, 6, $"Caster opacity should preserve shadow geometry. Stats={halfOpacityStats}");
            AssertNear(fullOpacityStats.Bounds.Height, halfOpacityStats.Bounds.Height, 6, $"Caster opacity should preserve shadow geometry. Stats={halfOpacityStats}");
            Assert.IsTrue(
                halfOpacityStats.ShadowPixelCount > fullOpacityStats.ShadowPixelCount * 0.75,
                $"Caster opacity should dim the shadow without removing most of its mask. Full={fullOpacityStats}; Half={halfOpacityStats}");
        });
    }

    [TestMethod]
    public void ThemeShadowChromeTracksAnimatedCasterOpacityLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var (root, chrome) = CreateThemeShadowSourceCanvas(ElementTheme.Light);
            using var host = new TestWindowHost(root, width: 120, height: 120);
            host.UpdateLayout();
            var caster = (UIElement)chrome.Child;

            var fullOpacityStats = MeasureRenderedShadowPixels(root, 100, 100);

            caster.BeginAnimation(
                UIElement.OpacityProperty,
                new DoubleAnimation
                {
                    From = 0.5,
                    To = 0.5,
                    Duration = TimeSpan.FromMilliseconds(100)
                },
                HandoffBehavior.SnapshotAndReplace);
            root.UpdateLayout();
            WaitForRendering();

            var animatedOpacityStats = MeasureRenderedShadowPixels(root, 100, 100);
            AssertNear(fullOpacityStats.PeakDarkening * 0.5, animatedOpacityStats.PeakDarkening, 3, $"Animated caster opacity should scale shadow peak darkening. Stats={animatedOpacityStats}");

            caster.BeginAnimation(UIElement.OpacityProperty, null);
            root.UpdateLayout();
            WaitForRendering();

            var restoredOpacityStats = MeasureRenderedShadowPixels(root, 100, 100);
            AssertNear(fullOpacityStats.PeakDarkening, restoredOpacityStats.PeakDarkening, 1, $"Clearing caster opacity animation should restore shadow peak darkening. Stats={restoredOpacityStats}");
        });
    }

    [TestMethod]
    public void ThemeShadowChromeRenderedPixelsTrackWinUIPixelMasters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var light = RenderThemeShadowSourceCanvas(ElementTheme.Light);
            AssertWinUIRenderedPixelMasterComparableShadow(
                light,
                expectedCanvasBounds: new Int32Rect(14, 22, 72, 72),
                expectedPeakDarkening: 31,
                expectedShadowPixels: 2330,
                expectedCanvasCentroidX: 49.402,
                expectedCanvasCentroidY: 71.869);

            var dark = RenderThemeShadowSourceCanvas(ElementTheme.Dark);
            AssertWinUIRenderedPixelMasterComparableShadow(
                dark,
                expectedCanvasBounds: new Int32Rect(14, 21, 72, 74),
                expectedPeakDarkening: 58,
                expectedShadowPixels: 2542,
                expectedCanvasCentroidX: 49.356,
                expectedCanvasCentroidY: 71.786);

            Assert.IsTrue(dark.PeakDarkening > light.PeakDarkening);
            Assert.IsTrue(dark.ShadowPixelCount > light.ShadowPixelCount);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeRenderedPixelsTrackWinUIPixelMasterPngsWhenSourceRootProvided()
    {
        var sourceRoot = Environment.GetEnvironmentVariable("MODERNWPF_WINUI_SOURCE_ROOT");
        if (string.IsNullOrWhiteSpace(sourceRoot))
        {
            return;
        }

        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var mastersRoot = Path.Combine(
                sourceRoot,
                "src",
                "dxaml",
                "test",
                "resources",
                "masters");

            var light = RenderThemeShadowSourceCanvas(ElementTheme.Light);
            // The .1 masters are the RenderTargetBitmap surfaces for the 100x100 rtbCanvas in ThemeShadowDropShadowSystemThemeRedrawRTB.
            AssertWinUIPixelMasterPngComparableShadow(
                light,
                mastersRoot,
                "Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowSystemThemeRedrawRTB.Light.1.master.png");

            var dark = RenderThemeShadowSourceCanvas(ElementTheme.Dark);
            AssertWinUIPixelMasterPngComparableShadow(
                dark,
                mastersRoot,
                "Foundation_Graphics_ThemeShadowTests_ThemeShadowDropShadowSystemThemeRedrawRTB.Dark.1.master.png");
        });
    }

    [TestMethod]
    public void ShadowSnapshotMetricsRoundTripForReferenceComparison()
    {
        var stats = new RenderedShadowPixelStats(new Int32Rect(12, 20, 76, 76), 61, 2936, 49.356, 71.786);
        var text = CreateShadowSnapshotMetricsText("SampleControl", "shadow-only", 100, 100, stats);

        var metrics = ParseShadowSnapshotMetrics(
            "SampleControl-shadow-only.txt",
            text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

        Assert.AreEqual("SampleControl", metrics.Name);
        Assert.AreEqual("shadow-only", metrics.Kind);
        Assert.AreEqual(100, metrics.Width);
        Assert.AreEqual(100, metrics.Height);
        Assert.AreEqual(stats.Bounds, metrics.Stats.Bounds);
        Assert.AreEqual(stats.PeakDarkening, metrics.Stats.PeakDarkening);
        Assert.AreEqual(stats.ShadowPixelCount, metrics.Stats.ShadowPixelCount);
        Assert.AreEqual(stats.CentroidX, metrics.Stats.CentroidX, 0.001);
        Assert.AreEqual(stats.CentroidY, metrics.Stats.CentroidY, 0.001);
        AssertShadowSnapshotStatsMatchReference("SampleControl", "SampleControl-shadow-only", metrics.Stats, stats);
    }

    [TestMethod]
    public void ShadowSnapshotReferencePngRoundTripForReferenceComparison()
    {
        WpfTestHost.Run(() =>
        {
            var root = CreateWhiteCanvas(20, 20);
            root.Children.Add(new Border
            {
                Width = 10,
                Height = 10,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            });

            ArrangeElement(root, 20, 20);

            var bitmap = RenderShadowSnapshotBitmap(root, 20, 20);
            var metrics = CreateShadowSnapshotMetricsFromReferenceBitmap(
                "SampleControl",
                "shadow-only",
                "SampleControl-shadow-only.png",
                bitmap);

            Assert.AreEqual("SampleControl", metrics.Name);
            Assert.AreEqual("shadow-only", metrics.Kind);
            Assert.AreEqual(20, metrics.Width);
            Assert.AreEqual(20, metrics.Height);
            Assert.IsTrue(metrics.Stats.PeakDarkening > 0);
            Assert.IsTrue(metrics.Stats.ShadowPixelCount > 0);
            AssertShadowSnapshotImageMatchesReference("SampleControl", "SampleControl-shadow-only", bitmap, bitmap);
        });
    }

    [TestMethod]
    public void ShadowSnapshotReferenceMaskRoundTripForReferenceComparison()
    {
        var ignoredBounds = new Int32Rect(5, 6, 7, 8);
        var text = CreateShadowSnapshotReferenceMaskText("SampleControl", "shadow-only", 20, 20, ignoredBounds);

        var mask = ParseShadowSnapshotReferenceMask(
            "SampleControl-shadow-only.mask.txt",
            text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

        Assert.AreEqual("SampleControl", mask.Name);
        Assert.AreEqual("shadow-only", mask.Kind);
        Assert.AreEqual(20, mask.Width);
        Assert.AreEqual(20, mask.Height);
        Assert.AreEqual(ignoredBounds, mask.IgnoredBounds);
    }

    [TestMethod]
    public void ThemeShadowChromeRerendersWhenRequestedThemeChangesLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var (root, _) = CreateThemeShadowSourceCanvas(ElementTheme.Light);

            var light = MeasureRenderedShadowPixels(root, 100, 100);
            AssertWinUIRenderedPixelMasterComparableShadow(
                light,
                expectedCanvasBounds: new Int32Rect(14, 22, 72, 72),
                expectedPeakDarkening: 31,
                expectedShadowPixels: 2330,
                expectedCanvasCentroidX: 49.402,
                expectedCanvasCentroidY: 71.869);

            ThemeManager.SetRequestedTheme(root, ElementTheme.Dark);
            root.UpdateLayout();
            WpfTestHost.DoEvents();

            var dark = MeasureRenderedShadowPixels(root, 100, 100);
            AssertWinUIRenderedPixelMasterComparableShadow(
                dark,
                expectedCanvasBounds: new Int32Rect(14, 21, 72, 74),
                expectedPeakDarkening: 58,
                expectedShadowPixels: 2542,
                expectedCanvasCentroidX: 49.356,
                expectedCanvasCentroidY: 71.786);

            Assert.IsTrue(dark.PeakDarkening > light.PeakDarkening);
            Assert.IsTrue(dark.ShadowPixelCount > light.ShadowPixelCount);
        });
    }

    [TestMethod]
    public void ThemeShadowChromeRerendersWhenCornerRadiusChangesLikeWinUISource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var (root, chrome) = CreateThemeShadowSourceCanvas(ElementTheme.Light);

            var roundedStats = MeasureRenderedShadowPixels(root, 100, 100);
            AssertWinUIRenderedPixelMasterComparableShadow(
                roundedStats,
                expectedCanvasBounds: new Int32Rect(14, 22, 72, 72),
                expectedPeakDarkening: 31,
                expectedShadowPixels: 2330,
                expectedCanvasCentroidX: 49.402,
                expectedCanvasCentroidY: 71.869);

            var roundedCorner = RenderCurrentElementPixel(root, 25, 25, 100, 100);

            chrome.CornerRadius = new CornerRadius();
            root.UpdateLayout();
            WpfTestHost.DoEvents();

            var squareStats = MeasureRenderedShadowPixels(root, 100, 100);
            var squareCorner = RenderCurrentElementPixel(root, 25, 25, 100, 100);
            Assert.IsTrue(
                squareCorner.R > roundedCorner.R && squareCorner.A == 255,
                $"Expected square caster corner to be cleared from the hollow center after CornerRadius changes. Pixel={squareCorner}");
            Assert.AreNotEqual(
                roundedStats.ShadowPixelCount,
                squareStats.ShadowPixelCount,
                "Expected dynamic CornerRadius change to alter the rendered shadow mask.");

            chrome.CornerRadius = new CornerRadius(8);
            root.UpdateLayout();
            WpfTestHost.DoEvents();

            var largerRoundedStats = MeasureRenderedShadowPixels(root, 100, 100);
            var largerRoundedCorner = RenderCurrentElementPixel(root, 25, 25, 100, 100);
            Assert.IsTrue(
                largerRoundedCorner.R < squareCorner.R && largerRoundedCorner.A == 255,
                $"Expected larger rounded caster corner to restore visible shadow after dynamic CornerRadius update. Pixel={largerRoundedCorner}");
            Assert.AreNotEqual(
                squareStats.ShadowPixelCount,
                largerRoundedStats.ShadowPixelCount,
                "Expected the second dynamic CornerRadius change to update the rendered shadow mask.");
        });
    }

    [TestMethod]
    public void ThemeShadowChromeChildlessCasterUsesExplicitSizeAsSourceCaster()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var light = RenderThemeShadowChildlessSourceCanvas(ElementTheme.Light);
            AssertWinUIRenderedPixelMasterComparableShadow(
                light,
                expectedCanvasBounds: new Int32Rect(14, 22, 72, 72),
                expectedPeakDarkening: 31,
                expectedShadowPixels: 2330,
                expectedCanvasCentroidX: 49.402,
                expectedCanvasCentroidY: 71.869);

            var dark = RenderThemeShadowChildlessSourceCanvas(ElementTheme.Dark);
            AssertWinUIRenderedPixelMasterComparableShadow(
                dark,
                expectedCanvasBounds: new Int32Rect(14, 21, 72, 74),
                expectedPeakDarkening: 58,
                expectedShadowPixels: 2542,
                expectedCanvasCentroidX: 49.356,
                expectedCanvasCentroidY: 71.786);

            Assert.IsTrue(dark.PeakDarkening > light.PeakDarkening);
            Assert.IsTrue(dark.ShadowPixelCount > light.ShadowPixelCount);
        });
    }

    [TestMethod]
    public void ExistingPopupTemplatesUseWinUIWindowedPopupInsets()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyoutPresenter = new FlyoutPresenter { Content = "Flyout" };
            using (var host = new TestWindowHost(flyoutPresenter, width: 180, height: 120))
            {
                host.UpdateLayout();
                var chrome = FindVisualChild<ThemeShadowChrome>(flyoutPresenter)
                    ?? throw new AssertFailedException("Expected FlyoutPresenter to use ThemeShadowChrome.");
                AssertMediumWindowedPopupInsets(chrome);
            }

            var autoSuggestBox = new ModernWpf.Controls.AutoSuggestBox { Width = 180 };
            using (var host = new TestWindowHost(autoSuggestBox, width: 220, height: 120))
            {
                host.UpdateLayout();
                var popup = FindTemplateChild<Popup>(autoSuggestBox, "SuggestionsPopup");
                var chrome = popup.Child as ThemeShadowChrome
                    ?? throw new AssertFailedException("Expected AutoSuggestBox suggestions popup to use ThemeShadowChrome.");
                AssertMediumWindowedPopupInsets(chrome);
            }

            var commandBar = new CommandBar { Width = 220 };
            using (var host = new TestWindowHost(commandBar, width: 260, height: 120))
            {
                host.UpdateLayout();
                var popup = FindTemplateChild<Popup>(commandBar, "OverflowPopup");
                var chrome = FindVisualChild<ThemeShadowChrome>(popup.Child)
                    ?? throw new AssertFailedException("Expected CommandBar overflow popup to use ThemeShadowChrome.");
                AssertMediumWindowedPopupInsets(chrome);
            }
        });
    }

    [TestMethod]
    public void SourceBackedShadowTemplatesRenderVisibleShadowPixels()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var flyoutPresenter = new FlyoutPresenter
            {
                Content = new Border
                {
                    Width = 50,
                    Height = 50,
                    Background = Brushes.Transparent
                },
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(),
                Padding = new Thickness(),
                CornerRadius = new CornerRadius(4),
                IsDefaultShadowEnabled = true,
                MinWidth = 0,
                MinHeight = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24)
            };
            var flyoutRoot = CreateWhiteCanvas(140, 140);
            flyoutRoot.Children.Add(flyoutPresenter);
            ArrangeElement(flyoutRoot, 140, 140);

            var flyoutChrome = FindVisualChild<ThemeShadowChrome>(flyoutPresenter)
                ?? throw new AssertFailedException("Expected FlyoutPresenter to render through ThemeShadowChrome.");
            AssertMediumWindowedPopupInsets(flyoutChrome);
            AssertRenderedTemplateShadow(flyoutRoot, flyoutChrome, 140, 140, minPeakDarkening: 25, minShadowPixels: 1200, "FlyoutPresenter");

            var numberBox = new ModernWpf.Controls.NumberBox
            {
                SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Compact,
                Width = 160
            };
            using (var host = new TestWindowHost(numberBox, width: 220, height: 140))
            {
                host.UpdateLayout();
                var popup = FindTemplateChild<Popup>(numberBox, "UpDownPopup");
                var numberBoxChrome = popup.Child as ThemeShadowChrome
                    ?? throw new AssertFailedException("Expected NumberBox popup child to be ThemeShadowChrome.");
                Assert.AreEqual(16.0, numberBoxChrome.Depth);
                Assert.AreEqual(new Thickness(8, 4, 8, 12), numberBoxChrome.ShadowPadding);

                popup.Child = null;
                try
                {
                    AssertDetachedTemplateShadow(numberBoxChrome, 140, 140, new Thickness(24), minPeakDarkening: 10, minShadowPixels: 300, "NumberBox compact popup");
                }
                finally
                {
                    popup.Child = numberBoxChrome;
                }
            }

            var autoSuggestBox = new ModernWpf.Controls.AutoSuggestBox
            {
                ItemsSource = new[] { "One", "Two", "Three" },
                Width = 180,
                IsSuggestionListOpen = true
            };
            using (var host = new TestWindowHost(autoSuggestBox, width: 260, height: 180))
            {
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                var popup = FindTemplateChild<Popup>(autoSuggestBox, "SuggestionsPopup");
                var autoSuggestChrome = popup.Child as ThemeShadowChrome
                    ?? throw new AssertFailedException("Expected AutoSuggestBox suggestions popup child to be ThemeShadowChrome.");
                AssertMediumWindowedPopupInsets(autoSuggestChrome);

                popup.Child = null;
                try
                {
                    AssertDetachedTemplateShadow(autoSuggestChrome, 280, 220, new Thickness(24), minPeakDarkening: 20, minShadowPixels: 1800, "AutoSuggestBox suggestions popup");
                }
                finally
                {
                    popup.Child = autoSuggestChrome;
                }
            }

            var commandBar = new CommandBar
            {
                IsDynamicOverflowEnabled = false,
                Width = 220
            };
            commandBar.SecondaryCommands.Add(new AppBarButton { Label = "Share" });
            using (var host = new TestWindowHost(commandBar, width: 320, height: 160))
            {
                host.UpdateLayout();

                var commandBarChrome = FindTemplateChild<ThemeShadowChrome>(commandBar, "SecondaryItemsControlShadowWrapper");
                AssertMediumWindowedPopupInsets(commandBarChrome);
                AssertDetachedTemplateShadow(commandBarChrome, 320, 180, new Thickness(24), minPeakDarkening: 20, minShadowPixels: 1000, "CommandBar overflow popup");
            }

            var commandBarFlyoutCommandBar = new CommandBarFlyoutCommandBar
            {
                IsOpen = true,
                Width = 220,
                Height = 48,
                CornerRadius = new CornerRadius(4)
            };
            commandBarFlyoutCommandBar.SecondaryCommands.Add(new AppBarButton { Label = "Select all" });
            using (var host = new TestWindowHost(commandBarFlyoutCommandBar, width: 320, height: 180))
            {
                host.UpdateLayout();

                var commandBarFlyoutChrome = FindTemplateChild<ThemeShadowChrome>(commandBarFlyoutCommandBar, "OuterOverflowContentRootShadowChrome");
                AssertMediumWindowedPopupInsets(commandBarFlyoutChrome);

                var overflowPopup = FindTemplateChild<Popup>(commandBarFlyoutCommandBar, "OverflowPopup");
                overflowPopup.Child = null;
                try
                {
                    AssertDetachedTemplateShadow(commandBarFlyoutChrome, 320, 180, new Thickness(24), minPeakDarkening: 20, minShadowPixels: 1000, "CommandBarFlyout overflow root");
                }
                finally
                {
                    overflowPopup.Child = commandBarFlyoutChrome;
                }
            }

            var menuFlyoutOwner = new Button
            {
                Content = "Anchor",
                Width = 80,
                Height = 32
            };
            ThemeManager.SetRequestedTheme(menuFlyoutOwner, ElementTheme.Light);
            var menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuItem { Header = "Copy" });
            var menuFlyoutPresenter = menuFlyout.Presenter;
            ThemeManager.SetRequestedTheme(menuFlyoutPresenter, ElementTheme.Light);
            menuFlyoutPresenter.Width = 160;
            menuFlyoutPresenter.Height = 96;
            menuFlyoutPresenter.MinWidth = 0;
            menuFlyoutPresenter.MinHeight = 0;
            menuFlyoutPresenter.Padding = new Thickness();
            menuFlyoutPresenter.IsDefaultShadowEnabled = true;
            menuFlyoutPresenter.CornerRadius = new CornerRadius(4);

            using (var host = new TestWindowHost(menuFlyoutOwner, width: 200, height: 140))
            {
                host.UpdateLayout();
                menuFlyout.ShowAt(menuFlyoutOwner);
                WpfTestHost.DoEvents();
                WaitForDispatcherDelay(250);

                try
                {
                    var menuFlyoutChrome = FindVisualChild<ThemeShadowChrome>(menuFlyoutPresenter)
                        ?? throw new AssertFailedException("Expected MenuFlyoutPresenter to render through ThemeShadowChrome.");
                    AssertMediumWindowedPopupInsets(menuFlyoutChrome);
                    AssertRenderedTemplateShadowVisible(
                        menuFlyoutPresenter,
                        menuFlyoutChrome,
                        200,
                        140,
                        minPeakDarkening: 20,
                        minShadowPixels: 1000,
                        "MenuFlyoutPresenter");
                }
                finally
                {
                    menuFlyout.Hide();
                }
            }

            var teachingTip = new ModernWpf.Controls.TeachingTip
            {
                Content = "Tip content",
                Title = "Tip title",
                CornerRadius = new CornerRadius(4)
            };
            using (var host = new TestWindowHost(teachingTip, width: 360, height: 240))
            {
                host.UpdateLayout();

                var teachingTipChrome = FindTemplateChild<ThemeShadowChrome>(teachingTip, "ContentRootGridShadowChrome");
                AssertMediumWindowedPopupInsets(teachingTipChrome);
                AssertDetachedTemplateShadow(teachingTipChrome, 320, 220, new Thickness(32), minPeakDarkening: 20, minShadowPixels: 2500, "TeachingTip content root");
            }
        });
    }

    [TestMethod]
    public void SourceBackedChildlessShadowTemplatesRenderVisibleShadowPixels()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var contentDialog = new ContentDialog
            {
                Content = "Dialog content",
                IsShadowEnabled = true
            };
            using (var host = new TestWindowHost(contentDialog, width: 640, height: 480))
            {
                host.UpdateLayout();

                var contentDialogShadow = FindTemplateChild<ThemeShadowChrome>(contentDialog, "Shdw");
                Assert.AreEqual(128.0, contentDialogShadow.Depth);
                Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Default, contentDialogShadow.WindowedPopupInsetMode);
                Assert.AreEqual(new Thickness(64, 32, 64, 96), contentDialogShadow.ShadowPadding);

                AssertDetachedChildlessTemplateShadow(
                    contentDialogShadow,
                    casterWidth: 80,
                    casterHeight: 60,
                    canvasWidth: 260,
                    canvasHeight: 260,
                    margin: new Thickness(80),
                    minPeakDarkening: 40,
                    minShadowPixels: 7000,
                    "ContentDialog background shadow");
            }

            var navigationView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left,
                IsPaneOpen = true,
                OpenPaneLength = 100,
                Width = 320,
                Height = 180
            };
            navigationView.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "Home" });
            using (var host = new TestWindowHost(navigationView, width: 360, height: 220))
            {
                host.UpdateLayout();

                var shadowCaster = FindTemplateChild<ThemeShadowChrome>(navigationView, "ShadowCaster");
                Assert.AreEqual(16.0, shadowCaster.Depth);
                Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Default, shadowCaster.WindowedPopupInsetMode);
                Assert.AreEqual(new Thickness(8, 4, 8, 12), shadowCaster.ShadowPadding);

                AssertDetachedChildlessTemplateShadow(
                    shadowCaster,
                    casterWidth: 80,
                    casterHeight: 80,
                    canvasWidth: 130,
                    canvasHeight: 130,
                    margin: new Thickness(20),
                    minPeakDarkening: 10,
                    minShadowPixels: 600,
                    "NavigationView pane overlay shadow");
            }
        });
    }

    [TestMethod]
    public void LayoutChromeControlsUseBackgroundTransitionBrush()
    {
        WpfTestHost.Run(() =>
        {
            if (!Helper.IsAnimationsEnabled)
            {
                return;
            }

            AssertTransitionBrush(
                new BorderEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(BorderEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernCanvasEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernCanvasEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ContentPresenterEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ContentPresenterEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernGridEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernGridEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernRelativePanel(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernRelativePanel.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernStackPanelEx(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernStackPanelEx.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernItemsStackPanel(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernItemsStackPanel.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernVariableSizedWrapGrid(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernVariableSizedWrapGrid.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernWrapGrid(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernWrapGrid.BackgroundTransitionProperty),
                control => control.EffectiveBackground);

            AssertTransitionBrush(
                new ModernItemsWrapGrid(),
                (control, brush) => control.Background = brush,
                (control, transition) => control.BackgroundTransition = transition,
                control => control.ClearValue(ModernItemsWrapGrid.BackgroundTransitionProperty),
                control => control.EffectiveBackground);
        });
    }

    [TestMethod]
    public void CanvasExAcceptsWinUIPanelSurfaceAndAttachedProperties()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var child = new Border
            {
                Width = 12,
                Height = 8,
                Background = Brushes.Red
            };
            var canvas = new ModernCanvasEx
            {
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            canvas.Children.Add(child);
            ModernCanvasEx.SetLeft(child, 13);
            ModernCanvasEx.SetTop(child, 17);
            ModernCanvasEx.SetZIndex(child, 5);

            Assert.AreSame(backgroundTransition, canvas.BackgroundTransition);
            Assert.AreSame(childrenTransitions, canvas.ChildrenTransitions);
            Assert.AreEqual(13, Canvas.GetLeft(child));
            Assert.AreEqual(17, Canvas.GetTop(child));
            Assert.AreEqual(5, Panel.GetZIndex(child));

            canvas.Measure(new Size(100, 100));

            Assert.AreEqual(0, canvas.DesiredSize.Width, 0.1);
            Assert.AreEqual(0, canvas.DesiredSize.Height, 0.1);

            canvas.Arrange(new Rect(0, 0, 100, 100));
            canvas.UpdateLayout();

            var origin = child.TranslatePoint(new Point(), canvas);
            Assert.AreEqual(13, origin.X, 0.1);
            Assert.AreEqual(17, origin.Y, 0.1);
        });
    }

    [TestMethod]
    public void CanvasExParsesWinUIPanelSurfaceXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:CanvasEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    Background="Transparent">
                    <controls:CanvasEx.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:CanvasEx.BackgroundTransition>
                    <controls:CanvasEx.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:CanvasEx.ChildrenTransitions>
                    <Border
                        Width="12"
                        Height="8"
                        Background="Red"
                        controls:CanvasEx.Left="7"
                        controls:CanvasEx.Top="9"
                        controls:CanvasEx.ZIndex="3" />
                </controls:CanvasEx>
                """;

            var canvas = (ModernCanvasEx)XamlReader.Parse(xaml);
            var child = (UIElement)canvas.Children[0];

            Assert.IsNotNull(canvas.BackgroundTransition);
            Assert.IsNotNull(canvas.ChildrenTransitions);
            Assert.AreEqual(7, ModernCanvasEx.GetLeft(child));
            Assert.AreEqual(9, ModernCanvasEx.GetTop(child));
            Assert.AreEqual(3, ModernCanvasEx.GetZIndex(child));
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernVariableSizedWrapGrid
            {
                ItemHeight = 40,
                ItemWidth = 50,
                Orientation = Orientation.Horizontal,
                HorizontalChildrenAlignment = HorizontalAlignment.Center,
                VerticalChildrenAlignment = VerticalAlignment.Bottom,
                MaximumRowsOrColumns = 3,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(40, panel.ItemHeight);
            Assert.AreEqual(50, panel.ItemWidth);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.AreEqual(3, panel.MaximumRowsOrColumns);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridWrapsHorizontallyAndVertically()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateVariableSizedWrapGrid(Orientation.Horizontal, 7);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200)
                });

            var verticalPanel = CreateVariableSizedWrapGrid(Orientation.Vertical, 7);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0)
                });
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridSupportsRowAndColumnSpans()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateVariableSizedWrapGrid(Orientation.Horizontal, 7);

            ModernVariableSizedWrapGrid.SetColumnSpan(panel.Children[0], 2);
            ModernVariableSizedWrapGrid.SetRowSpan(panel.Children[2], 2);

            AssertVariableSizedWrapGridPositions(
                panel,
                new[]
                {
                    new Point(50, 0),
                    new Point(200, 0),
                    new Point(0, 150),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(100, 200),
                    new Point(200, 200)
                });
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridStopsPlacementWhenSourceOccupancyMapIsFull()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateVariableSizedWrapGrid(Orientation.Horizontal, 10);
            horizontalPanel.Measure(new Size(horizontalPanel.Width, horizontalPanel.Height));
            horizontalPanel.Arrange(new Rect(0, 0, horizontalPanel.Width, horizontalPanel.Height));
            horizontalPanel.UpdateLayout();

            Assert.AreEqual(300, horizontalPanel.DesiredSize.Width, 0.1);
            Assert.AreEqual(300, horizontalPanel.DesiredSize.Height, 0.1);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200),
                    new Point(100, 200),
                    new Point(200, 200)
                },
                expectedArrangedCount: 9);
            Assert.AreEqual(new Size(), ((UIElement)horizontalPanel.Children[9]).RenderSize);

            var verticalPanel = CreateVariableSizedWrapGrid(Orientation.Vertical, 10);
            verticalPanel.Measure(new Size(verticalPanel.Width, verticalPanel.Height));
            verticalPanel.Arrange(new Rect(0, 0, verticalPanel.Width, verticalPanel.Height));
            verticalPanel.UpdateLayout();

            Assert.AreEqual(300, verticalPanel.DesiredSize.Width, 0.1);
            Assert.AreEqual(300, verticalPanel.DesiredSize.Height, 0.1);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0),
                    new Point(200, 100),
                    new Point(200, 200)
                },
                expectedArrangedCount: 9);
            Assert.AreEqual(new Size(), ((UIElement)verticalPanel.Children[9]).RenderSize);
        });
    }

    [TestMethod]
    public void VariableSizedWrapGridParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:VariableSizedWrapGrid
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    ItemWidth="40"
                    ItemHeight="30"
                    Orientation="Horizontal"
                    MaximumRowsOrColumns="2"
                    HorizontalChildrenAlignment="Center"
                    VerticalChildrenAlignment="Bottom">
                    <controls:VariableSizedWrapGrid.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:VariableSizedWrapGrid.BackgroundTransition>
                    <controls:VariableSizedWrapGrid.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:VariableSizedWrapGrid.ChildrenTransitions>
                    <Border
                        Width="10"
                        Height="10"
                        Background="Red"
                        controls:VariableSizedWrapGrid.ColumnSpan="2"
                        controls:VariableSizedWrapGrid.RowSpan="3" />
                </controls:VariableSizedWrapGrid>
                """;

            var panel = (ModernVariableSizedWrapGrid)XamlReader.Parse(xaml);
            var child = (UIElement)panel.Children[0];

            Assert.AreEqual(40, panel.ItemWidth);
            Assert.AreEqual(30, panel.ItemHeight);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.MaximumRowsOrColumns);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(2, ModernVariableSizedWrapGrid.GetColumnSpan(child));
            Assert.AreEqual(3, ModernVariableSizedWrapGrid.GetRowSpan(child));
        });
    }

    [TestMethod]
    public void WrapGridAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernWrapGrid
            {
                ItemHeight = 40,
                ItemWidth = 50,
                Orientation = Orientation.Horizontal,
                HorizontalChildrenAlignment = HorizontalAlignment.Center,
                VerticalChildrenAlignment = VerticalAlignment.Bottom,
                MaximumRowsOrColumns = 3,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(40, panel.ItemHeight);
            Assert.AreEqual(50, panel.ItemWidth);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.AreEqual(3, panel.MaximumRowsOrColumns);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void WrapGridWrapsHorizontallyAndVertically()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateWrapGrid(Orientation.Horizontal, 7);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200)
                });

            var verticalPanel = CreateWrapGrid(Orientation.Vertical, 7);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0)
                });
        });
    }

    [TestMethod]
    public void WrapGridParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:WrapGrid
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    ItemWidth="40"
                    ItemHeight="30"
                    Orientation="Horizontal"
                    MaximumRowsOrColumns="2"
                    HorizontalChildrenAlignment="Center"
                    VerticalChildrenAlignment="Bottom">
                    <controls:WrapGrid.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:WrapGrid.BackgroundTransition>
                    <controls:WrapGrid.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:WrapGrid.ChildrenTransitions>
                    <Border Width="10" Height="10" Background="Red" />
                </controls:WrapGrid>
                """;

            var panel = (ModernWrapGrid)XamlReader.Parse(xaml);

            Assert.AreEqual(40, panel.ItemWidth);
            Assert.AreEqual(30, panel.ItemHeight);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.MaximumRowsOrColumns);
            Assert.AreEqual(HorizontalAlignment.Center, panel.HorizontalChildrenAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, panel.VerticalChildrenAlignment);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void ItemsStackPanelAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var defaultPanel = new ModernItemsStackPanel();

            Assert.AreEqual(Orientation.Vertical, defaultPanel.Orientation);
            Assert.AreEqual(new Thickness(), defaultPanel.GroupPadding);
            Assert.AreEqual(GroupHeaderPlacement.Top, defaultPanel.GroupHeaderPlacement);
            Assert.AreEqual(ItemsUpdatingScrollMode.KeepItemsInView, defaultPanel.ItemsUpdatingScrollMode);
            Assert.AreEqual(0.0, defaultPanel.CacheLength);
            Assert.IsTrue(defaultPanel.AreStickyGroupHeadersEnabled);
            Assert.AreEqual(-1, defaultPanel.FirstCacheIndex);
            Assert.AreEqual(-1, defaultPanel.FirstVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, defaultPanel.ScrollingDirection);

            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernItemsStackPanel
            {
                GroupPadding = new Thickness(1, 2, 3, 4),
                Orientation = Orientation.Horizontal,
                GroupHeaderPlacement = GroupHeaderPlacement.Left,
                ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
                CacheLength = 2.5,
                AreStickyGroupHeadersEnabled = false,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(ItemsUpdatingScrollMode.KeepLastItemInView, panel.ItemsUpdatingScrollMode);
            Assert.AreEqual(2.5, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void ItemsStackPanelStacksChildrenAndReportsRealizedRange()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateItemsStackPanel(Orientation.Horizontal, 3);
            AssertItemsStackPanelPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100)
                });

            Assert.AreEqual(0, horizontalPanel.FirstCacheIndex);
            Assert.AreEqual(0, horizontalPanel.FirstVisibleIndex);
            Assert.AreEqual(2, horizontalPanel.LastVisibleIndex);
            Assert.AreEqual(2, horizontalPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, horizontalPanel.ScrollingDirection);

            var verticalPanel = CreateItemsStackPanel(Orientation.Vertical, 3);
            AssertItemsStackPanelPositions(
                verticalPanel,
                new[]
                {
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200)
                });
        });
    }

    [TestMethod]
    public void ItemsStackPanelParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ItemsStackPanel
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    GroupPadding="1,2,3,4"
                    Orientation="Horizontal"
                    GroupHeaderPlacement="Left"
                    ItemsUpdatingScrollMode="KeepLastItemInView"
                    CacheLength="2"
                    AreStickyGroupHeadersEnabled="False">
                    <controls:ItemsStackPanel.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ItemsStackPanel.BackgroundTransition>
                    <controls:ItemsStackPanel.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:ItemsStackPanel.ChildrenTransitions>
                    <Border Width="10" Height="10" Background="Red" />
                </controls:ItemsStackPanel>
                """;

            var panel = (ModernItemsStackPanel)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(ItemsUpdatingScrollMode.KeepLastItemInView, panel.ItemsUpdatingScrollMode);
            Assert.AreEqual(2.0, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void ItemsWrapGridAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var defaultPanel = new ModernItemsWrapGrid();

            Assert.AreEqual(Orientation.Vertical, defaultPanel.Orientation);
            Assert.AreEqual(-1, defaultPanel.MaximumRowsOrColumns);
            Assert.IsTrue(double.IsNaN(defaultPanel.ItemWidth));
            Assert.IsTrue(double.IsNaN(defaultPanel.ItemHeight));
            Assert.AreEqual(new Thickness(), defaultPanel.GroupPadding);
            Assert.AreEqual(GroupHeaderPlacement.Top, defaultPanel.GroupHeaderPlacement);
            Assert.AreEqual(0.0, defaultPanel.CacheLength);
            Assert.IsTrue(defaultPanel.AreStickyGroupHeadersEnabled);
            Assert.AreEqual(-1, defaultPanel.FirstCacheIndex);
            Assert.AreEqual(-1, defaultPanel.FirstVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastVisibleIndex);
            Assert.AreEqual(-1, defaultPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, defaultPanel.ScrollingDirection);

            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernItemsWrapGrid
            {
                GroupPadding = new Thickness(1, 2, 3, 4),
                Orientation = Orientation.Horizontal,
                MaximumRowsOrColumns = 3,
                ItemWidth = 50,
                ItemHeight = 40,
                GroupHeaderPlacement = GroupHeaderPlacement.Left,
                CacheLength = 2.5,
                AreStickyGroupHeadersEnabled = false,
                BackgroundTransition = backgroundTransition,
                ChildrenTransitions = childrenTransitions
            };

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(3, panel.MaximumRowsOrColumns);
            Assert.AreEqual(50, panel.ItemWidth);
            Assert.AreEqual(40, panel.ItemHeight);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(2.5, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
        });
    }

    [TestMethod]
    public void ItemsWrapGridWrapsChildrenAndReportsRealizedRange()
    {
        WpfTestHost.Run(() =>
        {
            var horizontalPanel = CreateItemsWrapGrid(Orientation.Horizontal, 7);
            AssertVariableSizedWrapGridPositions(
                horizontalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(100, 0),
                    new Point(200, 0),
                    new Point(0, 100),
                    new Point(100, 100),
                    new Point(200, 100),
                    new Point(0, 200)
                });

            Assert.AreEqual(0, horizontalPanel.FirstCacheIndex);
            Assert.AreEqual(0, horizontalPanel.FirstVisibleIndex);
            Assert.AreEqual(6, horizontalPanel.LastVisibleIndex);
            Assert.AreEqual(6, horizontalPanel.LastCacheIndex);
            Assert.AreEqual(PanelScrollingDirection.None, horizontalPanel.ScrollingDirection);

            var verticalPanel = CreateItemsWrapGrid(Orientation.Vertical, 7);
            AssertVariableSizedWrapGridPositions(
                verticalPanel,
                new[]
                {
                    new Point(0, 0),
                    new Point(0, 100),
                    new Point(0, 200),
                    new Point(100, 0),
                    new Point(100, 100),
                    new Point(100, 200),
                    new Point(200, 0)
                });
        });
    }

    [TestMethod]
    public void ItemsWrapGridParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ItemsWrapGrid
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    GroupPadding="1,2,3,4"
                    ItemWidth="40"
                    ItemHeight="30"
                    Orientation="Horizontal"
                    MaximumRowsOrColumns="2"
                    GroupHeaderPlacement="Left"
                    CacheLength="2"
                    AreStickyGroupHeadersEnabled="False">
                    <controls:ItemsWrapGrid.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ItemsWrapGrid.BackgroundTransition>
                    <controls:ItemsWrapGrid.ChildrenTransitions>
                        <animation:TransitionCollection />
                    </controls:ItemsWrapGrid.ChildrenTransitions>
                    <Border Width="10" Height="10" Background="Red" />
                </controls:ItemsWrapGrid>
                """;

            var panel = (ModernItemsWrapGrid)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(1, 2, 3, 4), panel.GroupPadding);
            Assert.AreEqual(40, panel.ItemWidth);
            Assert.AreEqual(30, panel.ItemHeight);
            Assert.AreEqual(Orientation.Horizontal, panel.Orientation);
            Assert.AreEqual(2, panel.MaximumRowsOrColumns);
            Assert.AreEqual(GroupHeaderPlacement.Left, panel.GroupHeaderPlacement);
            Assert.AreEqual(2.0, panel.CacheLength);
            Assert.IsFalse(panel.AreStickyGroupHeadersEnabled);
            Assert.IsNotNull(panel.BackgroundTransition);
            Assert.IsNotNull(panel.ChildrenTransitions);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void BrushTransitionHelperUsesWinUISolidColorRules()
    {
        WpfTestHost.Run(() =>
        {
            if (!ModernWpf.Helper.IsAnimationsEnabled)
            {
                Assert.Inconclusive("BrushTransition follows the shared animation-enabled switch.");
            }

            var invalidations = 0;
            var helper = new BrushTransitionHelper(() => invalidations++);
            var transition = new BrushTransition { Duration = TimeSpan.FromHours(1) };
            var red = new SolidColorBrush(Colors.Red);

            helper.OnBrushChanged(null, red, transition);

            var fadeInBrush = AssertSolidColorBrush(helper.GetEffectiveBrush(red), Color.FromArgb(0, 255, 0, 0));
            Assert.AreNotSame(red, fadeInBrush);
            Assert.IsTrue(helper.IsTransitioning);

            var blue = new SolidColorBrush(Colors.Blue);
            helper.OnBrushChanged(red, blue, transition);

            Assert.AreSame(fadeInBrush, helper.GetEffectiveBrush(blue));
            Assert.IsTrue(helper.IsTransitioning);

            var gradient = new LinearGradientBrush(Colors.Red, Colors.Blue, 0);
            helper.OnBrushChanged(blue, gradient, transition);

            Assert.AreSame(gradient, helper.GetEffectiveBrush(gradient));
            Assert.IsFalse(helper.IsTransitioning);
            Assert.IsTrue(invalidations >= 3);
        });
    }

    [TestMethod]
    public void CoreTextInputStockTemplatesDoNotUseDescriptionPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var controls = new Control[]
            {
                new TextBox(),
                new PasswordBox(),
                new DatePicker()
            };

            foreach (var control in controls)
            {
                ControlHelper.SetDescription(control, control.GetType().Name + " description");
            }

            using var host = new TestWindowHost(new StackPanel { Children = { controls[0], controls[1], controls[2] } });
            host.UpdateLayout();

            foreach (var control in controls)
            {
                Assert.AreEqual(control.GetType().Name + " description", ControlHelper.GetDescription(control));
                Assert.IsNull(FindVisualChild<ContentPresenterEx>(control));
            }
        });
    }

    [TestMethod]
    public void CoreItemTemplatesUseOfficialWpfFluentPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var listBoxItem = new ListBoxItem
            {
                Content = "ListBox content",
                IsEnabled = false
            };
            var listViewItem = new System.Windows.Controls.ListViewItem
            {
                Content = "ListView content",
                IsSelected = true
            };
            var header = new GridViewColumnHeader
            {
                Style = (Style)Application.Current.FindResource("DefaultGridViewColumnHeaderStyle"),
                Content = "Header content"
            };

            using var host = new TestWindowHost(new StackPanel { Children = { listBoxItem, listViewItem, header } });
            host.UpdateLayout();

            var listBoxPresenter = FindVisualChild<ContentPresenter>(listBoxItem)
                ?? throw new AssertFailedException("Expected ListBoxItem template to use WPF ContentPresenter.");
            Assert.AreEqual(listBoxItem.Content, listBoxPresenter.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(listBoxItem));

            var listViewPresenter = FindVisualChild<ContentPresenter>(listViewItem)
                ?? throw new AssertFailedException("Expected ListViewItem template to use WPF ContentPresenter.");
            Assert.AreEqual(listViewItem.Content, listViewPresenter.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(listViewItem));

            var headerPresenter = FindVisualChild<ContentPresenter>(header)
                ?? throw new AssertFailedException("Expected GridViewColumnHeader template to use WPF ContentPresenter.");
            Assert.AreEqual(header.Content, headerPresenter.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(header));
        });
    }

    [TestMethod]
    public void CoreMenuItemTemplatesUseOfficialWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var topLevelItem = CreateMenuItemWithTemplate("TopLevelItemTemplateKey", "File", null, isEnabled: true);
            var topLevelHeader = CreateMenuItemWithTemplate("TopLevelHeaderTemplateKey", "Edit", null, isEnabled: true);
            var submenuItem = CreateMenuItemWithTemplate("SubmenuItemTemplateKey", "Open", new TextBlock { Text = "Icon" }, isEnabled: false);
            var submenuHeader = CreateMenuItemWithTemplate("SubmenuHeaderTemplateKey", "More", new TextBlock { Text = "Icon" }, isEnabled: false);

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { topLevelItem, topLevelHeader, submenuItem, submenuHeader }
            });
            host.UpdateLayout();

            AssertMenuTemplatePresenterSlot(topLevelItem);
            AssertMenuTemplatePresenterSlot(topLevelHeader);
            AssertMenuTemplatePresenterSlot(submenuItem);
            AssertMenuTemplatePresenterSlot(submenuHeader);
        });
    }

    [TestMethod]
    public void CoreTabControlTemplatesUseOfficialWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var tabItem = new TabItem
            {
                Header = "Tab Header",
                Content = "Tab Content"
            };
            var tabControl = new TabControl
            {
                Width = 320,
                Height = 160
            };
            tabControl.Items.Add(tabItem);

            using var host = new TestWindowHost(tabControl, width: 380, height: 220);
            host.UpdateLayout();

            var itemPresenter = FindTemplateChild<ContentPresenter>(tabItem, "ContentSite");
            Assert.AreEqual(tabItem.Header, itemPresenter.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(tabItem));

            var selectedContentHost = FindTemplateChild<ContentPresenter>(tabControl, "PART_SelectedContentHost");
            Assert.AreEqual(tabItem.Content, selectedContentHost.Content);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(tabControl));
        });
    }

    [TestMethod]
    public void CoreResidualTemplatesUseExpectedPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = new RadioButton
            {
                Content = "Radio content",
                Foreground = Brushes.Red
            };
            var listViewHeaderItem = new ListViewHeaderItem
            {
                Content = "List header",
                Foreground = Brushes.Blue
            };
            var titleBarButton = new TitleBarButton
            {
                Content = "X",
                Foreground = Brushes.Green,
                IsActive = true
            };

            var hostPanel = new StackPanel();
            hostPanel.Children.Add(radioButton);
            hostPanel.Children.Add(listViewHeaderItem);
            hostPanel.Children.Add(titleBarButton);

            using var host = new TestWindowHost(hostPanel, width: 320, height: 180);
            host.UpdateLayout();

            var radioPresenter = FindTemplateChild<ContentPresenter>(radioButton, "ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), radioPresenter.GetType());
            Assert.AreEqual(radioButton.Content, radioPresenter.Content);
            Assert.AreSame(radioButton.Foreground, TextElement.GetForeground(radioPresenter));

            var headerPresenter = FindTemplateChild<ContentPresenterEx>(listViewHeaderItem, "ContentPresenter");
            Assert.AreEqual(listViewHeaderItem.Content, headerPresenter.Content);
            Assert.AreSame(listViewHeaderItem.Foreground, headerPresenter.Foreground);

            var titlePresenter = FindTemplateChild<ContentPresenterEx>(titleBarButton, "Content");
            Assert.AreEqual(titleBarButton.Content, titlePresenter.Content);
            Assert.AreSame(titleBarButton.Foreground, titlePresenter.Foreground);
            Assert.AreEqual(titleBarButton.FontSize, titlePresenter.FontSize);
        });
    }

    [TestMethod]
    public void SimpleShellTemplatesUseWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var page = new ModernWpf.Controls.Page
            {
                Content = "Page content",
                Foreground = Brushes.Red
            };
            var frame = new ModernWpf.Controls.Frame();

            var hostPanel = new StackPanel();
            hostPanel.Children.Add(frame);

            using var pageHost = new TestWindowHost(page, width: 240, height: 120);
            using var host = new TestWindowHost(hostPanel, width: 360, height: 320);
            pageHost.UpdateLayout();
            host.UpdateLayout();

            var pagePresenter = FindVisualChild<ContentPresenterEx>(page)
                ?? throw new AssertFailedException("Expected Page template to use ContentPresenterEx.");
            Assert.AreEqual(page.Content, pagePresenter.Content);
            Assert.AreSame(page.Foreground, pagePresenter.Foreground);

            Assert.IsInstanceOfType(FindTemplateChild<ContentPresenterEx>(frame, "FirstContentPresenter"), typeof(ContentPresenterEx));
            Assert.IsInstanceOfType(FindTemplateChild<ContentPresenterEx>(frame, "SecondContentPresenter"), typeof(ContentPresenterEx));
        });
    }

    [TestMethod]
    public void ExpanderTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var expander = new System.Windows.Controls.Expander
            {
                Header = "Expander header",
                Content = "Expander content",
                Foreground = Brushes.Purple,
                IsExpanded = true
            };

            using var host = new TestWindowHost(expander, width: 360, height: 240);
            host.UpdateLayout();

            var contentPresenter = FindTemplateChild<ContentPresenter>(expander, "ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
            Assert.AreEqual(expander.Content, contentPresenter.Content);

            var headerSite = FindTemplateChild<ToggleButton>(expander, "HeaderSite");
            headerSite.ApplyTemplate();

            var headerPresenter = FindTemplateChild<ContentPresenter>(headerSite, "ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), headerPresenter.GetType());
            Assert.AreEqual(expander.Header, headerPresenter.Content);

            Assert.IsNull(FindVisualChild<ContentPresenterEx>(expander));
        });
    }

    [TestMethod]
    public void StatusBarTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var statusBar = new StatusBar();
            var statusBarItem = new StatusBarItem
            {
                Content = "Status content"
            };
            statusBar.Items.Add(statusBarItem);

            using var host = new TestWindowHost(statusBar, width: 260, height: 80);
            host.UpdateLayout();

            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemBackground"), statusBarItem.Background);
            Assert.AreEqual((Thickness)statusBarItem.TryFindResource("StatusBarItemPadding"), statusBarItem.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, statusBarItem.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Center, statusBarItem.VerticalContentAlignment);

            var border = FindVisualChild<Border>(statusBarItem)
                ?? throw new AssertFailedException("Expected StatusBarItem template to use official WPF Border chrome.");
            var presenter = VisualTreeTestHelper.EnumerateDescendants(statusBarItem)
                .OfType<ContentPresenter>()
                .Single(item => Equals(item.Content, statusBarItem.Content));

            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
            Assert.AreEqual(HorizontalAlignment.Left, presenter.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, presenter.VerticalAlignment);
            Assert.AreSame(statusBarItem.Background, border.Background);
            Assert.AreEqual(statusBarItem.Padding, border.Padding);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(statusBarItem));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(statusBarItem));

            statusBarItem.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemBackgroundDisabled"), statusBarItem.Background);
            Assert.AreSame(statusBarItem.TryFindResource("StatusBarItemForegroundDisabled"), statusBarItem.Foreground);
        });
    }

    [TestMethod]
    public void GroupBoxTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var groupBox = new GroupBox
            {
                Header = "Group header",
                Content = "Group content"
            };

            using var host = new TestWindowHost(groupBox, width: 240, height: 140);
            host.UpdateLayout();

            Assert.IsTrue(groupBox.OverridesDefaultStyle);
            Assert.AreSame(groupBox.TryFindResource("GroupBoxBackground"), groupBox.Background);
            Assert.AreSame(groupBox.TryFindResource("GroupBoxBorderBrush"), groupBox.BorderBrush);
            Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxBorderThickness"), groupBox.BorderThickness);
            Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxPadding"), groupBox.Padding);

            var border = FindVisualChild<Border>(groupBox)
                ?? throw new AssertFailedException("Expected GroupBox template to use official WPF Border chrome.");
            var presenters = VisualTreeTestHelper.EnumerateDescendants(groupBox)
                .OfType<ContentPresenter>()
                .ToArray();
            var headerPresenter = presenters.Single(item => Equals(item.Content, groupBox.Header));
            var contentPresenter = presenters.Single(item => Equals(item.Content, groupBox.Content));

            Assert.AreSame(groupBox.Background, border.Background);
            Assert.AreSame(groupBox.BorderBrush, border.BorderBrush);
            Assert.AreEqual(groupBox.BorderThickness, border.BorderThickness);
            Assert.AreEqual(typeof(ContentPresenter), headerPresenter.GetType());
            Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
            Assert.AreEqual(0, Grid.GetRow(headerPresenter));
            Assert.AreEqual(1, Grid.GetRow(contentPresenter));
            Assert.AreEqual((double)groupBox.TryFindResource("GroupBoxHeaderFontSize"), TextElement.GetFontSize(headerPresenter));
            Assert.AreSame(groupBox.TryFindResource("GroupBoxHeaderForeground"), TextElement.GetForeground(headerPresenter));
            Assert.AreEqual((Thickness)groupBox.TryFindResource("GroupBoxHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(groupBox.Padding, contentPresenter.Margin);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(groupBox));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(groupBox));
        });
    }

    [TestMethod]
    public void LabelStyleUsesOfficialWpfFluentStyleSurface()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultLabelStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(Label));
            Assert.AreEqual(typeof(Label), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Label), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            Assert.IsFalse(defaultStyle.Setters.OfType<Setter>().Any(item => item.Property == Control.TemplateProperty));
            Assert.IsFalse(defaultStyle.Setters.OfType<Setter>().Any(item => item.Property == Control.OverridesDefaultStyleProperty));

            var label = new Label
            {
                Width = 160,
                Height = 40,
                Content = "_Label content"
            };

            using var host = new TestWindowHost(label, width: 200, height: 80);
            host.UpdateLayout();

            Assert.AreEqual(new Thickness(0, 0, 0, 4), label.Padding);
            Assert.IsFalse(label.Focusable);
            Assert.IsTrue(label.SnapsToDevicePixels);
            Assert.IsFalse(label.OverridesDefaultStyle);
            Assert.AreSame(label.TryFindResource("LabelForeground"), label.Foreground);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(label));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(label));
        });
    }

    [TestMethod]
    public void CalendarNavigationButtonsUseOfficialWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var calendar = new Calendar();
            using var host = new TestWindowHost(calendar, width: 360, height: 320);
            host.UpdateLayout();

            var calendarItem = FindTemplateChild<CalendarItem>(calendar, "PART_CalendarItem");
            AssertCalendarNavigationButtonPresenter(FindTemplateChild<Button>(calendarItem, "PART_HeaderButton"));
            AssertCalendarNavigationButtonPresenter(FindTemplateChild<Button>(calendarItem, "PART_PreviousButton"));
            AssertCalendarNavigationButtonPresenter(FindTemplateChild<Button>(calendarItem, "PART_NextButton"));
        });
    }

    [TestMethod]
    public void DataGridTemplatesUseOfficialWpfPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var cell = new DataGridCell
            {
                Style = FindStyleResource("DefaultDataGridCellStyle"),
                Content = "Cell content",
                Foreground = Brushes.Red
            };
            var columnHeader = new DataGridColumnHeader
            {
                Style = FindStyleResource("DefaultDataGridColumnHeaderStyle"),
                Content = "Column header",
                Foreground = Brushes.Blue
            };
            var rowHeader = new DataGridRowHeader
            {
                Style = FindStyleResource("DefaultDataGridRowHeaderStyle"),
                Content = "Row header",
                Foreground = Brushes.Green
            };

            using var host = new TestWindowHost(new StackPanel
            {
                Children = { cell, columnHeader, rowHeader }
            }, width: 360, height: 180);
            host.UpdateLayout();

            AssertDataGridWpfPresenter(cell, cell.Content);
            AssertDataGridWpfPresenter(columnHeader, columnHeader.Content);
            AssertDataGridWpfPresenter(rowHeader, rowHeader.Content);
        });
    }

    [TestMethod]
    public void BorderExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:BorderEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Button Content="Parsed" />
                </controls:BorderEx>
                """;

            var border = (BorderEx)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(2), border.Padding);
            Assert.AreEqual(new CornerRadius(3), border.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, border.BackgroundSizing);
            Assert.IsInstanceOfType(border.Child, typeof(Button));
        });
    }

    [TestMethod]
    public void BorderExOuterBackgroundSizingPaintsBehindBorder()
    {
        WpfTestHost.Run(() =>
        {
            var inner = RenderBorderEdgePixel(BackgroundSizing.InnerBorderEdge);
            var outer = RenderBorderEdgePixel(BackgroundSizing.OuterBorderEdge);

            Assert.IsTrue(outer.R > inner.R + 40, $"Expected outer edge red channel above inner edge. Inner={inner}, Outer={outer}");
            Assert.IsTrue(outer.A > inner.A + 40, $"Expected outer edge alpha above inner edge. Inner={inner}, Outer={outer}");
        });
    }

    [TestMethod]
    public void BorderExOuterBackgroundSizingInflatesOuterCornerByHalfBorder()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            var roundedCorner = RenderBorderPixel(border, 27, 1, 30, 30);
            var straightEdge = RenderBorderPixel(border, 1, 15, 30, 30);

            Assert.IsTrue(roundedCorner.A < 30, $"Expected inflated WinUI outer corner to clip the pixel. Pixel={roundedCorner}");
            Assert.IsTrue(straightEdge.R > 200 && straightEdge.A > 200, $"Expected outer edge background under the transparent border. Pixel={straightEdge}");
        });
    }

    [TestMethod]
    public void BorderExLayoutClipUsesNonUniformCornerRadius()
    {
        WpfTestHost.Run(() =>
        {
            var border = new TestBorderEx
            {
                Width = 24,
                Height = 24,
                ClipToBounds = true,
                CornerRadius = new CornerRadius(0, 12, 0, 0)
            };
            border.Measure(new Size(24, 24));
            border.Arrange(new Rect(0, 0, 24, 24));
            border.UpdateLayout();

            var clip = border.GetLayoutClipForTest(new Size(24, 24));

            Assert.IsNotNull(clip);
            Assert.IsTrue(clip.FillContains(new Point(1, 1)), "Top-left corner should remain square.");
            Assert.IsFalse(clip.FillContains(new Point(23, 1)), "Top-right corner should be clipped by the non-uniform radius.");
            Assert.IsTrue(clip.FillContains(new Point(12, 12)), "Center should remain inside the clip.");
        });
    }

    [TestMethod]
    public void RoundedLayoutClipPreservesBaseLayoutClip()
    {
        WpfTestHost.Run(() =>
        {
            var baseClip = new RectangleGeometry(new Rect(0, 0, 12, 24));

            var clip = LayoutChromeHelper.CreateRoundedLayoutClip(
                new Size(24, 24),
                new CornerRadius(12),
                baseClip);

            Assert.IsNotNull(clip);
            Assert.IsTrue(clip.FillContains(new Point(6, 12)), "Point inside both clips should remain visible.");
            Assert.IsFalse(clip.FillContains(new Point(18, 12)), "Point outside the base layout clip should be clipped.");
            Assert.IsFalse(clip.FillContains(new Point(1, 1)), "Point outside the rounded corner should be clipped.");
        });
    }

    [TestMethod]
    public void LayoutChromeCornerRadiusChangeRefreshesChildClip()
    {
        WpfTestHost.Run(() =>
        {
            var border = new BorderEx
            {
                Width = 30,
                Height = 30,
                Child = CreateRedChildBox()
            };
            AssertDynamicRoundedChildClip(border, value => border.CornerRadius = value);

            var presenter = new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Content = CreateRedChildBox()
            };
            AssertDynamicRoundedChildClip(presenter, value => presenter.CornerRadius = value);

            var stackPanel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30
            };
            stackPanel.Children.Add(CreateRedChildBox());
            AssertDynamicRoundedChildClip(stackPanel, value => stackPanel.CornerRadius = value);

            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30
            };
            grid.Children.Add(CreateRedChildBox());
            AssertDynamicRoundedChildClip(grid, value => grid.CornerRadius = value);

            var relativePanel = new ModernRelativePanel
            {
                Width = 30,
                Height = 30
            };
            relativePanel.Children.Add(CreateRedChildBox());
            AssertDynamicRoundedChildClip(relativePanel, value => relativePanel.CornerRadius = value);
        });
    }

    [TestMethod]
    public void BorderExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new BorderEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void BorderExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChildRenderClip(new BorderEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0),
                Child = CreateRedChildBox()
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExOffsetsContentByChrome()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var presenter = new ContentPresenterEx
            {
                Width = 120,
                Height = 80,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10),
                Content = button
            };

            using var host = new TestWindowHost(presenter, width: 140, height: 100);

            AssertBoundsRelativeTo(button, presenter, new Rect(15, 15, 90, 50));
        });
    }

    [TestMethod]
    public void ContentPresenterExAlignsContentInsideChrome()
    {
        WpfTestHost.Run(() =>
        {
            var button = CreateButton(40, 20);
            var presenter = new ContentPresenterEx
            {
                Width = 140,
                Height = 100,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10),
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Content = button
            };

            using var host = new TestWindowHost(presenter, width: 160, height: 120);

            Assert.AreEqual(HorizontalAlignment.Stretch, new ContentPresenterEx().HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Stretch, new ContentPresenterEx().VerticalContentAlignment);
            AssertBoundsRelativeTo(button, presenter, new Rect(85, 65, 40, 20));
        });
    }

    [TestMethod]
    public void ContentPresenterExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ContentPresenterEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge"
                    CharacterSpacing="15"
                    HorizontalContentAlignment="Right"
                    IsTextScaleFactorEnabled="False"
                    LineHeight="37"
                    LineStackingStrategy="MaxHeight"
                    MaxLines="2"
                    OpticalMarginAlignment="TrimSideBearings"
                    TextLineBounds="TrimToBaseline"
                    TextWrapping="Wrap"
                    VerticalContentAlignment="Bottom">
                    <Button Content="Parsed" />
                </controls:ContentPresenterEx>
                """;

            var presenter = (ContentPresenterEx)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(2), presenter.Padding);
            Assert.AreEqual(new CornerRadius(3), presenter.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(15, presenter.CharacterSpacing);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(37, presenter.LineHeight);
            Assert.AreEqual(LineStackingStrategy.MaxHeight, presenter.LineStackingStrategy);
            Assert.AreEqual(2, presenter.MaxLines);
            Assert.AreEqual(ModernWpf.OpticalMarginAlignment.TrimSideBearings, presenter.OpticalMarginAlignment);
            Assert.AreEqual(ModernWpf.TextLineBounds.TrimToBaseline, presenter.TextLineBounds);
            Assert.AreEqual(TextWrapping.Wrap, presenter.TextWrapping);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
            Assert.IsInstanceOfType(presenter.Content, typeof(Button));
        });
    }

    [TestMethod]
    public void ContentPresenterExUsesWinUIInheritedTextMetadata()
    {
        WpfTestHost.Run(() =>
        {
            Assert.AreSame(ControlHelper.CharacterSpacingProperty, ContentPresenterEx.CharacterSpacingProperty);
            Assert.AreSame(ControlHelper.IsTextScaleFactorEnabledProperty, ContentPresenterEx.IsTextScaleFactorEnabledProperty);
            Assert.AreSame(ControlHelper.CharacterSpacingProperty, ModernContentControlEx.CharacterSpacingProperty);
            Assert.AreSame(ControlHelper.IsTextScaleFactorEnabledProperty, ModernContentControlEx.IsTextScaleFactorEnabledProperty);

            AssertInheritedTextMetadata(ContentPresenterEx.CharacterSpacingProperty, typeof(ContentPresenterEx));
            AssertInheritedTextMetadata(ContentPresenterEx.IsTextScaleFactorEnabledProperty, typeof(ContentPresenterEx));
            AssertInheritedTextMetadata(ModernContentControlEx.CharacterSpacingProperty, typeof(ModernContentControlEx));
            AssertInheritedTextMetadata(ModernContentControlEx.IsTextScaleFactorEnabledProperty, typeof(ModernContentControlEx));

            var parent = new StackPanel();
            var presenter = new ContentPresenterEx();

            parent.SetValue(ControlHelper.CharacterSpacingProperty, 24);
            parent.SetValue(ControlHelper.IsTextScaleFactorEnabledProperty, false);
            parent.Children.Add(presenter);

            using var host = new TestWindowHost(parent, width: 120, height: 40);

            Assert.AreEqual(24, presenter.CharacterSpacing);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);

            presenter.CharacterSpacing = 7;
            presenter.IsTextScaleFactorEnabled = true;

            Assert.AreEqual(7, presenter.CharacterSpacing);
            Assert.IsTrue(presenter.IsTextScaleFactorEnabled);
        });
    }

    [TestMethod]
    public void ContentPresenterExPushesSupportedTextPropertiesToDefaultTextBlock()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new ContentPresenterEx
            {
                Width = 120,
                Height = 80,
                Content = "Hello",
                FontFamily = new FontFamily("Courier New"),
                FontSize = 23,
                FontStretch = FontStretches.Condensed,
                FontStyle = FontStyles.Italic,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Blue,
                LineHeight = 37,
                LineStackingStrategy = LineStackingStrategy.MaxHeight,
                MaxLines = 2,
                TextWrapping = TextWrapping.Wrap
            };

            using var host = new TestWindowHost(presenter, width: 140, height: 100);

            var textBlock = FindVisualChild<TextBlock>(presenter)
                ?? throw new AssertFailedException("Expected ContentPresenterEx to generate a default TextBlock.");
            Assert.AreEqual("Courier New", textBlock.FontFamily.Source);
            Assert.AreEqual(23, textBlock.FontSize);
            Assert.AreEqual(FontStretches.Condensed, textBlock.FontStretch);
            Assert.AreEqual(FontStyles.Italic, textBlock.FontStyle);
            Assert.AreEqual(FontWeights.Bold, textBlock.FontWeight);
            Assert.AreSame(Brushes.Blue, textBlock.Foreground);
            Assert.AreEqual(TextWrapping.Wrap, textBlock.TextWrapping);
            Assert.AreEqual(37, textBlock.LineHeight);
            Assert.AreEqual(LineStackingStrategy.MaxHeight, textBlock.LineStackingStrategy);
            Assert.AreEqual(74, textBlock.MaxHeight);
            Assert.IsTrue(textBlock.ClipToBounds);

            presenter.Foreground = Brushes.Green;
            presenter.FontSize = 19;
            presenter.MaxLines = 0;

            Assert.AreSame(Brushes.Green, textBlock.Foreground);
            Assert.AreEqual(19, textBlock.FontSize);
            Assert.AreEqual(double.PositiveInfinity, textBlock.MaxHeight);
            Assert.IsFalse(textBlock.ClipToBounds);
        });
    }

    [TestMethod]
    public void ContentPresenterExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChildRenderClip(new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0),
                Content = CreateRedChildBox()
            });
        });
    }

    [TestMethod]
    public void ContentPresenterExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var presenter = new ContentPresenterEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void ContentControlExUsesWinUIDefaultAlignmentAndTransitions()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var control = new ModernContentControlEx
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                ContentTransitions = transitions
            };

            Assert.AreEqual(HorizontalAlignment.Left, control.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Top, control.VerticalContentAlignment);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, control.BackgroundSizing);
            Assert.AreSame(backgroundTransition, control.BackgroundTransition);
            Assert.AreEqual(0, control.CharacterSpacing);
            Assert.AreSame(transitions, control.ContentTransitions);
            Assert.IsTrue(control.IsTextScaleFactorEnabled);
        });
    }

    [TestMethod]
    public void ContentControlExTemplateForwardsContentTransitionsAndAlignment()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(83) };
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = CreateButton(40, 20);
            var control = new ModernContentControlEx
            {
                Width = 120,
                Height = 80,
                Background = Brushes.Red,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(5),
                CharacterSpacing = 21,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(10),
                Content = button,
                ContentTransitions = transitions,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                IsTextScaleFactorEnabled = false,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(control, width: 140, height: 100);

            var presenter = FindVisualChild<ContentPresenterEx>(control)
                ?? throw new AssertFailedException("Expected ContentControlEx template to use ContentPresenterEx.");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreSame(backgroundTransition, presenter.BackgroundTransition);
            Assert.AreEqual(21, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreSame(button, presenter.Content);
            Assert.AreEqual(new CornerRadius(3), presenter.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
            Assert.AreSame(button, control.ContentTemplateRoot);
            AssertBoundsRelativeTo(button, control, new Rect(65, 45, 40, 20));
        });
    }

    [TestMethod]
    public void ContentControlExExposesWinUIContentTemplateRoot()
    {
        WpfTestHost.Run(() =>
        {
            var template = (DataTemplate)XamlReader.Parse(
                """
                <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                    <TextBlock Text="{Binding}" />
                </DataTemplate>
                """);
            var control = new ModernContentControlEx
            {
                Width = 120,
                Height = 60,
                Content = "Templated",
                ContentTemplate = template
            };

            Assert.IsNull(control.ContentTemplateRoot);

            using var host = new TestWindowHost(control, width: 140, height: 80);

            var textBlock = control.ContentTemplateRoot as TextBlock
                ?? throw new AssertFailedException("Expected ContentTemplateRoot to expose the generated data-template root.");
            Assert.AreEqual("Templated", textBlock.Text);

            var button = CreateButton(40, 20);
            control.ContentTemplate = null;
            control.Content = button;
            host.UpdateLayout();

            Assert.AreSame(button, control.ContentTemplateRoot);
        });
    }

    [TestMethod]
    public void ContentControlExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:ContentControlEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    xmlns:animation="clr-namespace:ModernWpf.Media.Animation;assembly=ModernWpf"
                    BackgroundSizing="OuterBorderEdge"
                    CharacterSpacing="21"
                    Padding="2"
                    CornerRadius="3"
                    HorizontalContentAlignment="Right"
                    IsTextScaleFactorEnabled="False"
                    RecognizesAccessKey="True"
                    VerticalContentAlignment="Bottom">
                    <controls:ContentControlEx.BackgroundTransition>
                        <controls:BrushTransition Duration="0:0:0.083" />
                    </controls:ContentControlEx.BackgroundTransition>
                    <controls:ContentControlEx.ContentTransitions>
                        <animation:TransitionCollection />
                    </controls:ContentControlEx.ContentTransitions>
                    <Button Content="Parsed" />
                </controls:ContentControlEx>
                """;

            var control = (ModernContentControlEx)XamlReader.Parse(xaml);

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, control.BackgroundSizing);
            Assert.IsNotNull(control.BackgroundTransition);
            Assert.AreEqual(21, control.CharacterSpacing);
            Assert.AreEqual(new Thickness(2), control.Padding);
            Assert.AreEqual(new CornerRadius(3), control.CornerRadius);
            Assert.AreEqual(HorizontalAlignment.Right, control.HorizontalContentAlignment);
            Assert.IsFalse(control.IsTextScaleFactorEnabled);
            Assert.IsTrue(control.RecognizesAccessKey);
            Assert.IsNotNull(control.ContentTransitions);
            Assert.AreEqual(VerticalAlignment.Bottom, control.VerticalContentAlignment);
            Assert.IsInstanceOfType(control.Content, typeof(Button));
        });
    }

    [TestMethod]
    public void ControlHelperAcceptsWinUIControlTemplateSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var button = new Button();

            ControlHelper.SetBackgroundSizing(button, BackgroundSizing.OuterBorderEdge);
            ControlHelper.SetCharacterSpacing(button, 18);
            ControlHelper.SetContentTransitions(button, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(button, false);

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, ControlHelper.GetBackgroundSizing(button));
            Assert.AreEqual(18, ControlHelper.GetCharacterSpacing(button));
            Assert.AreSame(transitions, ControlHelper.GetContentTransitions(button));
            Assert.IsFalse(ControlHelper.GetIsTextScaleFactorEnabled(button));
        });
    }

    [TestMethod]
    public void ButtonTemplateUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                Width = 100,
                Height = 40,
                Content = "Button",
                Foreground = Brushes.Red
            };
            button.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));

            using var host = new TestWindowHost(button, width: 140, height: 80);

            var border = FindTemplateChild<Border>(button, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(button, "ContentPresenter");

            Assert.AreEqual(button.Content, presenter.Content);
            Assert.AreSame(button.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(((CornerRadius)button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(button));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(button));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            Assert.AreEqual(3, button.Template.Triggers.OfType<Trigger>().Count());
        });
    }

    [TestMethod]
    public void AccentButtonStyleUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var button = new Button
            {
                Width = 100,
                Height = 40,
                Content = "Accent",
                Foreground = Brushes.Blue,
                Style = (Style)Application.Current.FindResource("AccentButtonStyle")
            };
            button.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(8));

            using var host = new TestWindowHost(button, width: 140, height: 80);

            var border = FindTemplateChild<Border>(button, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(button, "ContentPresenter");

            Assert.AreEqual(button.Content, presenter.Content);
            Assert.AreSame(button.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(((CornerRadius)button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(button));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(button));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(button));
            Assert.AreEqual(3, button.Template.Triggers.OfType<Trigger>().Count());
        });
    }

    [TestMethod]
    public void RepeatButtonTemplateUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var repeatButton = new RepeatButton
            {
                Width = 100,
                Height = 40,
                Content = "Repeat",
                Foreground = Brushes.Blue
            };
            repeatButton.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(8));

            using var host = new TestWindowHost(repeatButton, width: 140, height: 80);

            var border = FindTemplateChild<Border>(repeatButton, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(repeatButton, "ContentPresenter");

            Assert.AreEqual(repeatButton.Content, presenter.Content);
            Assert.AreSame(repeatButton.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(((CornerRadius)repeatButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(repeatButton));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(repeatButton));
            Assert.IsFalse(ButtonHelper.GetVisualStateSettersEnabled(repeatButton));
            Assert.AreEqual(3, repeatButton.Template.Triggers.OfType<Trigger>().Count());
        });
    }

    [TestMethod]
    public void ToggleButtonTemplateUsesOfficialWpfFluentPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            var toggleButton = new ToggleButton
            {
                Width = 100,
                Height = 40,
                Content = "Toggle",
                Foreground = Brushes.Blue
            };
            toggleButton.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(8));

            using var host = new TestWindowHost(toggleButton, width: 140, height: 80);

            var border = FindTemplateChild<Border>(toggleButton, "ContentBorder");
            var presenter = FindTemplateChild<ContentPresenter>(toggleButton, "ContentPresenter");

            Assert.AreEqual(toggleButton.Content, presenter.Content);
            Assert.AreSame(toggleButton.Foreground, TextElement.GetForeground(presenter));
            Assert.AreEqual(((CornerRadius)toggleButton.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), border.CornerRadius);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(toggleButton));
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(toggleButton));
            Assert.IsFalse(ToggleButtonHelper.GetVisualStateSettersEnabled(toggleButton));
            Assert.AreEqual(7, toggleButton.Template.Triggers.OfType<MultiTrigger>().Count());
        });
    }

    [TestMethod]
    public void HyperlinkButtonTemplateUsesWinUIContentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new ModernWpf.Media.Animation.TransitionCollection();
            var hyperlinkButton = new HyperlinkButton
            {
                Width = 120,
                Height = 40,
                Content = "Link"
            };
            ControlHelper.SetCharacterSpacing(hyperlinkButton, 21);
            ControlHelper.SetContentTransitions(hyperlinkButton, transitions);
            ControlHelper.SetIsTextScaleFactorEnabled(hyperlinkButton, false);

            using var host = new TestWindowHost(hyperlinkButton, width: 160, height: 80);

            var presenter = FindVisualChild<ContentPresenterEx>(hyperlinkButton)
                ?? throw new AssertFailedException("Expected HyperlinkButton template to use ContentPresenterEx directly.");
            Assert.IsNull(FindVisualChild<ModernContentControlEx>(hyperlinkButton));
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, presenter.BackgroundSizing);
            Assert.AreEqual(21, presenter.CharacterSpacing);
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.IsFalse(presenter.IsTextScaleFactorEnabled);
            Assert.IsNotNull(presenter.BackgroundTransition);
            Assert.AreEqual(TimeSpan.FromMilliseconds(83), presenter.BackgroundTransition.Duration);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(hyperlinkButton));
            AssertAnimatedIconStateSetters(presenter, "ContentPresenter.(ui:AnimatedIcon.State)");
            AssertAnimatedIconStateTransitions(hyperlinkButton, presenter);
        });
    }

    [TestMethod]
    public void ToolTipTemplateUsesOfficialWpfFluentPresenterShape()
    {
        WpfTestHost.Run(() =>
        {
            var toolTip = new ToolTip
            {
                Width = 30,
                Height = 30,
                Content = "Tip",
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                Padding = new Thickness(0)
            };

            toolTip.ApplyTemplate();
            toolTip.Measure(new Size(30, 30));
            toolTip.Arrange(new Rect(0, 0, 30, 30));
            toolTip.UpdateLayout();

            var border = FindVisualChild<Border>(toolTip)
                ?? throw new AssertFailedException("Expected ToolTip template to use official WPF Border chrome.");
            var presenter = FindVisualChild<ContentPresenter>(toolTip)
                ?? throw new AssertFailedException("Expected ToolTip template to use official WPF ContentPresenter.");

            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(toolTip));
            Assert.IsNull(FindVisualChild<ThemeShadowChrome>(toolTip));
            Assert.AreEqual(new CornerRadius(4), border.CornerRadius);
            Assert.AreSame(toolTip.Background, border.Background);
            Assert.AreSame(toolTip.BorderBrush, border.BorderBrush);
            Assert.AreEqual(toolTip.BorderThickness, border.BorderThickness);
            Assert.IsInstanceOfType(border.Effect, typeof(System.Windows.Media.Effects.DropShadowEffect));
            Assert.AreEqual(new Thickness(0), presenter.Margin);
        });
    }

    [TestMethod]
    public void ContentControlExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var control = new ModernContentControlEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            using var host = new TestWindowHost(control, width: 50, height: 50);
            var presenter = FindVisualChild<ContentPresenterEx>(control)
                ?? throw new AssertFailedException("Expected ContentControlEx template to use ContentPresenterEx.");

            AssertOuterChromePixels(presenter);
        });
    }

    [TestMethod]
    public void StackPanelExSupportsSpacingAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 200,
                Height = 120,
                Orientation = Orientation.Vertical,
                Spacing = 10,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(height: 20);
            var second = CreateStretchButton(height: 20);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 220, height: 140);

            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 170, 20));
            AssertBoundsRelativeTo(second, panel, new Rect(15, 45, 170, 20));
        });
    }

    [TestMethod]
    public void StackPanelExSupportsNegativeSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 200,
                Height = 80,
                Orientation = Orientation.Vertical,
                Spacing = -10,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(height: 20);
            var second = CreateStretchButton(height: 20);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 220, height: 100);

            Assert.AreEqual(-10, panel.Spacing);
            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 170, 20));
            AssertBoundsRelativeTo(second, panel, new Rect(15, 25, 170, 20));
        });
    }

    [TestMethod]
    public void StackPanelExAcceptsWinUISnapPointSurface()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                AreScrollSnapPointsRegular = true
            };

            Assert.IsTrue(panel.AreScrollSnapPointsRegular);
            Assert.IsTrue(panel.AreVerticalSnapPointsRegular);
            Assert.IsFalse(panel.AreHorizontalSnapPointsRegular);

            panel.Orientation = Orientation.Horizontal;

            Assert.IsTrue(panel.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(panel.AreVerticalSnapPointsRegular);
        });
    }

    [TestMethod]
    public void StackPanelExComputesWinUISnapPoints()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 140,
                Height = 50,
                Orientation = Orientation.Horizontal,
                AreScrollSnapPointsRegular = true,
                Margin = new Thickness(3, 5, 7, 11)
            };
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));

            using var host = new TestWindowHost(panel, width: 180, height: 90);

            var snapInfo = (IScrollSnapPointsInfo)panel;
            Assert.IsTrue(snapInfo.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(snapInfo.AreVerticalSnapPointsRegular);

            var interval = snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near, out var offset);
            Assert.AreEqual(3.0f, offset, 0.001f);
            Assert.AreEqual(50.0f, interval, 0.001f);

            interval = snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Center, out offset);
            Assert.AreEqual(28.0f, offset, 0.001f);
            Assert.AreEqual(50.0f, interval, 0.001f);

            interval = snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Far, out offset);
            Assert.AreEqual(7.0f, offset, 0.001f);
            Assert.AreEqual(50.0f, interval, 0.001f);

            interval = snapInfo.GetRegularSnapPoints(Orientation.Vertical, SnapPointsAlignment.Near, out offset);
            Assert.AreEqual(0.0f, offset, 0.001f);
            Assert.AreEqual(0.0f, interval, 0.001f);
            Assert.ThrowsException<InvalidOperationException>(() => snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near));

            panel.AreScrollSnapPointsRegular = false;

            Assert.ThrowsException<InvalidOperationException>(() => snapInfo.GetRegularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near, out _));
            AssertSnapPoints(new[] { 0.0f, 53.0f }, snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near));
            AssertSnapPoints(new[] { 28.0f, 78.0f }, snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Center));
            AssertSnapPoints(new[] { 53.0f, 103.0f }, snapInfo.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Far));
            Assert.AreEqual(0, snapInfo.GetIrregularSnapPoints(Orientation.Vertical, SnapPointsAlignment.Near).Count);
        });
    }

    [TestMethod]
    public void StackPanelExRaisesWinUISnapPointChangeEvents()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 160,
                Height = 60,
                Orientation = Orientation.Horizontal,
                AreScrollSnapPointsRegular = true
            };
            var snapInfo = (IScrollSnapPointsInfo)panel;
            int horizontalChanges = 0;
            int verticalChanges = 0;
            snapInfo.HorizontalSnapPointsChanged += (_, __) => horizontalChanges++;
            snapInfo.VerticalSnapPointsChanged += (_, __) => verticalChanges++;

            panel.Children.Add(CreateStretchButton(width: 40, height: 20));

            using var host = new TestWindowHost(panel, width: 180, height: 80);

            Assert.AreEqual(1, horizontalChanges);
            Assert.AreEqual(0, verticalChanges);

            panel.AreScrollSnapPointsRegular = false;
            int beforeChildChange = horizontalChanges;
            panel.Children.Add(CreateStretchButton(width: 30, height: 20));
            host.UpdateLayout();

            Assert.IsTrue(horizontalChanges > beforeChildChange);
            Assert.AreEqual(0, verticalChanges);

            panel.Orientation = Orientation.Vertical;
            host.UpdateLayout();

            Assert.IsTrue(verticalChanges > 0);
            Assert.IsFalse(snapInfo.AreHorizontalSnapPointsRegular);
            Assert.IsFalse(snapInfo.AreVerticalSnapPointsRegular);
        });
    }

    [TestMethod]
    public void StackPanelExHorizontalSpacingSkipsCollapsedChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 160,
                Height = 70,
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            var first = CreateStretchButton(width: 40);
            var collapsed = CreateStretchButton(width: 50);
            collapsed.Visibility = Visibility.Collapsed;
            var second = CreateStretchButton(width: 30);
            panel.Children.Add(first);
            panel.Children.Add(collapsed);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 180, height: 90);

            AssertBoundsRelativeTo(first, panel, new Rect(15, 15, 40, 40));
            AssertBoundsRelativeTo(second, panel, new Rect(65, 15, 30, 40));
        });
    }

    [TestMethod]
    public void StackPanelExOrientationChangeReflowsChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 140,
                Height = 100,
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            var first = CreateStretchButton(width: 40, height: 20);
            var second = CreateStretchButton(width: 30, height: 15);
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel, width: 160, height: 120);

            AssertStackAxisOffsetRelativeTo(first, panel, Orientation.Vertical, 0);
            AssertStackAxisOffsetRelativeTo(second, panel, Orientation.Vertical, 25);

            panel.Orientation = Orientation.Horizontal;
            host.UpdateLayout();

            AssertStackAxisOffsetRelativeTo(first, panel, Orientation.Horizontal, 0);
            AssertStackAxisOffsetRelativeTo(second, panel, Orientation.Horizontal, 45);
        });
    }

    [TestMethod]
    public void StackPanelExDesiredSizeCountsVisibleSpacingAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Orientation = Orientation.Vertical,
                Spacing = 7,
                BorderThickness = new Thickness(5),
                Padding = new Thickness(10)
            };
            panel.Children.Add(CreateStretchButton(width: 50, height: 20));
            panel.Children.Add(new Button
            {
                Width = 100,
                Height = 80,
                Visibility = Visibility.Collapsed
            });
            panel.Children.Add(CreateStretchButton(width: 30, height: 15));

            panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.AreEqual(80, panel.DesiredSize.Width, 1.0, "Desired width should include max visible child width plus border and padding.");
            Assert.AreEqual(72, panel.DesiredSize.Height, 1.0, "Desired height should include visible children, one spacing gap, border, and padding.");
        });
    }

    [TestMethod]
    public void StackPanelExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:StackPanelEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    AreScrollSnapPointsRegular="True"
                    Spacing="4"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Button Content="Parsed" />
                </controls:StackPanelEx>
                """;

            var panel = (ModernStackPanelEx)XamlReader.Parse(xaml);

            Assert.IsTrue(panel.AreScrollSnapPointsRegular);
            Assert.AreEqual(4, panel.Spacing);
            Assert.AreEqual(new Thickness(2), panel.Padding);
            Assert.AreEqual(new CornerRadius(3), panel.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void StackPanelExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(panel);
        });
    }

    [TestMethod]
    public void StackPanelExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void StackPanelExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernStackPanelEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            panel.Children.Add(CreateRedChildBox());

            AssertRoundedChildRenderClip(panel);
        });
    }

    [TestMethod]
    public void GridExSupportsSpacingSpansAndChrome()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 230,
                Height = 130,
                UseLayoutRounding = false,
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(5),
                RowSpacing = 10,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            var spanned = CreateStretchButton();

            Grid.SetColumn(second, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(first);
            grid.Children.Add(second);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 250, height: 150);

            AssertBoundsRelativeTo(first, grid, new Rect(7, 7, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(117, 7, 100, 50));
            AssertBoundsRelativeTo(spanned, grid, new Rect(7, 67, 210, 50));
        });
    }

    [TestMethod]
    public void GridExUsesWinUINegativeSpacingLayout()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 190,
                Height = 90,
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            var spanned = CreateStretchButton();

            Grid.SetColumn(second, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(first);
            grid.Children.Add(second);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 210, height: 110);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(90, 0, 100, 50));
            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 40, 190, 50));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingHandlesAutoAndStarTracks()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 190,
                Height = 90,
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var autoCell = CreateLayoutBox(width: 80, height: 40);
            var starCell = CreateLayoutBox(height: 40);
            var spanned = CreateLayoutBox();

            Grid.SetColumn(starCell, 1);
            Grid.SetRow(spanned, 1);
            Grid.SetColumnSpan(spanned, 2);

            grid.Children.Add(autoCell);
            grid.Children.Add(starCell);
            grid.Children.Add(spanned);

            using var host = new TestWindowHost(grid, width: 210, height: 110);

            AssertBoundsRelativeTo(autoCell, grid, new Rect(0, 0, 80, 40));
            AssertBoundsRelativeTo(starCell, grid, new Rect(70, 0, 120, 40));
            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 30, 190, 60));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingDesiredSizeUsesAutoTracks()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                UseLayoutRounding = false,
                RowSpacing = -10,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var first = CreateLayoutBox(width: 80, height: 30);
            var second = CreateLayoutBox(width: 70, height: 20);
            Grid.SetColumn(second, 1);
            Grid.SetRow(second, 1);

            grid.Children.Add(first);
            grid.Children.Add(second);

            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.AreEqual(140, grid.DesiredSize.Width, 1.0, "Desired width should subtract the negative column spacing from auto tracks.");
            Assert.AreEqual(40, grid.DesiredSize.Height, 1.0, "Desired height should subtract the negative row spacing from auto tracks.");
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 110,
                Height = 30,
                UseLayoutRounding = false,
                ColumnSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var spanned = CreateLayoutBox(width: 110);
            var secondColumnProbe = CreateLayoutBox();
            Grid.SetColumnSpan(spanned, 2);
            Grid.SetColumn(secondColumnProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondColumnProbe);

            using var host = new TestWindowHost(grid, width: 130, height: 50);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 110, 30));
            AssertBoundsRelativeTo(secondColumnProbe, grid, new Rect(50, 0, 60, 30));
        });
    }

    [TestMethod]
    public void GridExPositiveSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 130,
                Height = 30,
                UseLayoutRounding = false,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });

            var spanned = CreateLayoutBox(width: 130);
            var secondColumnProbe = CreateLayoutBox();
            Grid.SetColumnSpan(spanned, 2);
            Grid.SetColumn(secondColumnProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondColumnProbe);

            using var host = new TestWindowHost(grid, width: 150, height: 50);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 130, 30));
            AssertBoundsRelativeTo(secondColumnProbe, grid, new Rect(70, 0, 60, 30));
        });
    }

    [TestMethod]
    public void GridExNegativeRowSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 110,
                UseLayoutRounding = false,
                RowSpacing = -10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var spanned = CreateLayoutBox(height: 110);
            var secondRowProbe = CreateLayoutBox();
            Grid.SetRowSpan(spanned, 2);
            Grid.SetRow(secondRowProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondRowProbe);

            using var host = new TestWindowHost(grid, width: 50, height: 130);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 30, 110));
            AssertBoundsRelativeTo(secondRowProbe, grid, new Rect(0, 50, 30, 60));
        });
    }

    [TestMethod]
    public void GridExPositiveRowSpacingDistributesSpannedAutoDesiredSize()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 130,
                UseLayoutRounding = false,
                RowSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var spanned = CreateLayoutBox(height: 130);
            var secondRowProbe = CreateLayoutBox();
            Grid.SetRowSpan(spanned, 2);
            Grid.SetRow(secondRowProbe, 1);
            grid.Children.Add(spanned);
            grid.Children.Add(secondRowProbe);

            using var host = new TestWindowHost(grid, width: 50, height: 150);

            AssertBoundsRelativeTo(spanned, grid, new Rect(0, 0, 30, 130));
            AssertBoundsRelativeTo(secondRowProbe, grid, new Rect(0, 70, 30, 60));
        });
    }

    [TestMethod]
    public void GridExPositiveSpacingHandlesStarSpans()
    {
        WpfTestHost.Run(() =>
        {
            var grid = CreateStarSpanGrid(width: 430, height: 320, spacing: 10);
            var first = CreateLayoutBox();
            var middle = CreateLayoutBox();
            var trailing = CreateLayoutBox();

            Grid.SetColumnSpan(first, 2);
            Grid.SetRow(middle, 1);
            Grid.SetColumn(middle, 1);
            Grid.SetColumnSpan(middle, 2);
            Grid.SetRow(trailing, 2);
            Grid.SetColumn(trailing, 2);
            Grid.SetColumnSpan(trailing, 2);

            grid.Children.Add(first);
            grid.Children.Add(middle);
            grid.Children.Add(trailing);

            using var host = new TestWindowHost(grid, width: 450, height: 340);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 210, 100));
            AssertBoundsRelativeTo(middle, grid, new Rect(110, 110, 210, 100));
            AssertBoundsRelativeTo(trailing, grid, new Rect(220, 220, 210, 100));
        });
    }

    [TestMethod]
    public void GridExNegativeSpacingHandlesStarSpans()
    {
        WpfTestHost.Run(() =>
        {
            var grid = CreateStarSpanGrid(width: 370, height: 280, spacing: -10);
            var first = CreateLayoutBox();
            var middle = CreateLayoutBox();
            var trailing = CreateLayoutBox();

            Grid.SetColumnSpan(first, 2);
            Grid.SetRow(middle, 1);
            Grid.SetColumn(middle, 1);
            Grid.SetColumnSpan(middle, 2);
            Grid.SetRow(trailing, 2);
            Grid.SetColumn(trailing, 2);
            Grid.SetColumnSpan(trailing, 2);

            grid.Children.Add(first);
            grid.Children.Add(middle);
            grid.Children.Add(trailing);

            using var host = new TestWindowHost(grid, width: 390, height: 300);

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 190, 100));
            AssertBoundsRelativeTo(middle, grid, new Rect(90, 90, 190, 100));
            AssertBoundsRelativeTo(trailing, grid, new Rect(180, 180, 190, 100));
        });
    }

    [TestMethod]
    public void GridExDefinitionChangesInvalidateLayout()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 220,
                Height = 50,
                UseLayoutRounding = false,
                ColumnSpacing = 10
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            var first = CreateStretchButton();
            var second = CreateStretchButton();
            Grid.SetColumn(second, 1);
            grid.Children.Add(first);
            grid.Children.Add(second);

            using var host = new TestWindowHost(grid, width: 240, height: 80);
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(0, 0, 100, 50));

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 100, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(110, 0, 100, 50));

            grid.ColumnDefinitions[0].Width = new GridLength(80);
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, grid, new Rect(0, 0, 80, 50));
            AssertBoundsRelativeTo(second, grid, new Rect(90, 0, 100, 50));
        });
    }

    [TestMethod]
    public void GridExOuterBackgroundSizingUsesWinUIChromeGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(12),
                CornerRadius = new CornerRadius(0, 4, 0, 0),
                BackgroundSizing = BackgroundSizing.OuterBorderEdge
            };

            AssertOuterChromePixels(grid);
        });
    }

    [TestMethod]
    public void GridExHitTestUsesRoundedChromeClip()
    {
        WpfTestHost.Run(() =>
        {
            AssertRoundedChromeHitTest(new ModernGridEx
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            });
        });
    }

    [TestMethod]
    public void GridExRoundedCornerClipAppliesToChildContent()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            grid.Children.Add(CreateRedChildBox());

            AssertRoundedChildRenderClip(grid);
        });
    }

    [TestMethod]
    public void GridExAllowsNegativeSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var grid = new ModernGridEx
            {
                RowSpacing = -10,
                ColumnSpacing = -11
            };

            Assert.AreEqual(-10, grid.RowSpacing);
            Assert.AreEqual(-11, grid.ColumnSpacing);
            Assert.ThrowsException<ArgumentException>(() => grid.RowSpacing = double.NaN);
            Assert.ThrowsException<ArgumentException>(() => grid.ColumnSpacing = double.NaN);
        });
    }

    [TestMethod]
    public void GridExParsesTemplateCompatibilityXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:GridEx
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    RowSpacing="4"
                    ColumnSpacing="6"
                    Padding="2"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <controls:GridEx.RowDefinitions>
                        <RowDefinition Height="Auto" />
                    </controls:GridEx.RowDefinitions>
                    <controls:GridEx.ColumnDefinitions>
                        <ColumnDefinition Width="Auto" />
                    </controls:GridEx.ColumnDefinitions>
                    <Button Content="Parsed" />
                </controls:GridEx>
                """;

            var grid = (ModernGridEx)XamlReader.Parse(xaml);

            Assert.AreEqual(4, grid.RowSpacing);
            Assert.AreEqual(6, grid.ColumnSpacing);
            Assert.AreEqual(new Thickness(2), grid.Padding);
            Assert.AreEqual(new CornerRadius(3), grid.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, grid.BackgroundSizing);
            Assert.AreEqual(1, grid.Children.Count);
        });
    }

    [TestMethod]
    public void RelativePanelAcceptsWinUILayoutSurface()
    {
        WpfTestHost.Run(() =>
        {
            var backgroundTransition = new BrushTransition { Duration = TimeSpan.FromMilliseconds(70) };
            var childrenTransitions = new ModernWpf.Media.Animation.TransitionCollection();
            var panel = new ModernRelativePanel
            {
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                BackgroundTransition = backgroundTransition,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(2),
                ChildrenTransitions = childrenTransitions,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(3)
            };

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreSame(backgroundTransition, panel.BackgroundTransition);
            Assert.AreSame(Brushes.Blue, panel.BorderBrush);
            Assert.AreEqual(new Thickness(2), panel.BorderThickness);
            Assert.AreSame(childrenTransitions, panel.ChildrenTransitions);
            Assert.AreEqual(new CornerRadius(4), panel.CornerRadius);
            Assert.AreEqual(new Thickness(3), panel.Padding);
        });
    }

    [TestMethod]
    public void RelativePanelArrangesWinUIConstraintsAndInvalidatesOnChange()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernRelativePanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = false
            };
            var first = CreateLayoutBox(width: 100, height: 100);
            var second = CreateLayoutBox(width: 100, height: 100);
            var third = CreateLayoutBox(width: 100, height: 100);

            ModernRelativePanel.SetRightOf(second, first);
            ModernRelativePanel.SetRightOf(third, second);
            panel.Children.Add(first);
            panel.Children.Add(second);
            panel.Children.Add(third);

            using var host = new TestWindowHost(panel, width: 400, height: 400);

            AssertBoundsRelativeTo(first, panel, new Rect(0, 0, 100, 100));
            AssertBoundsRelativeTo(second, panel, new Rect(100, 0, 100, 100));
            AssertBoundsRelativeTo(third, panel, new Rect(200, 0, 100, 100));

            ModernRelativePanel.SetRightOf(second, null);
            ModernRelativePanel.SetRightOf(third, null);
            ModernRelativePanel.SetBelow(second, first);
            ModernRelativePanel.SetBelow(third, second);
            host.UpdateLayout();

            AssertBoundsRelativeTo(first, panel, new Rect(0, 0, 100, 100));
            AssertBoundsRelativeTo(second, panel, new Rect(0, 100, 100, 100));
            AssertBoundsRelativeTo(third, panel, new Rect(0, 200, 100, 100));
        });
    }

    [TestMethod]
    public void RelativePanelUsesWinUIBorderChromeForLayout()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernRelativePanel
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(10),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = false
            };
            var first = CreateLayoutBox(width: 100, height: 100);
            var second = CreateLayoutBox(width: 100, height: 100);
            var third = CreateLayoutBox(width: 100, height: 100);

            ModernRelativePanel.SetRightOf(second, first);
            ModernRelativePanel.SetRightOf(third, second);
            panel.Children.Add(first);
            panel.Children.Add(second);
            panel.Children.Add(third);

            using var host = new TestWindowHost(panel, width: 400, height: 400);

            Assert.AreEqual(340, panel.RenderSize.Width, 1.0);
            Assert.AreEqual(140, panel.RenderSize.Height, 1.0);
            AssertBoundsRelativeTo(first, panel, new Rect(20, 20, 100, 100));
            AssertBoundsRelativeTo(second, panel, new Rect(120, 20, 100, 100));
            AssertBoundsRelativeTo(third, panel, new Rect(220, 20, 100, 100));
        });
    }

    [TestMethod]
    public void RelativePanelParsesWinUIConstraintXaml()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:RelativePanel
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf"
                    HorizontalAlignment="Left"
                    VerticalAlignment="Top"
                    Padding="5"
                    BorderThickness="1"
                    CornerRadius="3"
                    BackgroundSizing="OuterBorderEdge">
                    <Border x:Name="b0" Width="50" Height="20" Background="Transparent" />
                    <Border x:Name="b1" Width="30" Height="20" Background="Transparent" controls:RelativePanel.RightOf="b0" />
                </controls:RelativePanel>
                """;

            var panel = (ModernRelativePanel)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(5), panel.Padding);
            Assert.AreEqual(new Thickness(1), panel.BorderThickness);
            Assert.AreEqual(new CornerRadius(3), panel.CornerRadius);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, panel.BackgroundSizing);
            Assert.AreEqual(2, panel.Children.Count);

            using var host = new TestWindowHost(panel, width: 120, height: 80);

            AssertBoundsRelativeTo((FrameworkElement)panel.Children[0], panel, new Rect(6, 6, 50, 20));
            AssertBoundsRelativeTo((FrameworkElement)panel.Children[1], panel, new Rect(56, 6, 30, 20));
        });
    }

    [TestMethod]
    public void RelativePanelRejectsInvalidWinUIConstraints()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernRelativePanel();
            var first = CreateLayoutBox(width: 100, height: 100);
            var second = CreateLayoutBox(width: 100, height: 100);
            panel.Children.Add(first);
            panel.Children.Add(second);

            Assert.ThrowsException<ArgumentException>(() => ModernRelativePanel.SetRightOf(second, true));

            ModernRelativePanel.SetRightOf(second, "missing");
            Assert.ThrowsException<InvalidOperationException>(() => panel.Measure(new Size(300, 300)));

            ModernRelativePanel.SetRightOf(second, first);
            ModernRelativePanel.SetLeftOf(first, second);
            Assert.ThrowsException<InvalidOperationException>(() => panel.Measure(new Size(300, 300)));
        });
    }

    private static Button CreateButton(double width, double height)
    {
        return new Button
        {
            Width = width,
            Height = height
        };
    }

    private static Button CreateStretchButton(double? width = null, double? height = null)
    {
        return new Button
        {
            Width = width ?? double.NaN,
            Height = height ?? double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static System.Windows.Controls.Border CreateLayoutBox(double? width = null, double? height = null)
    {
        return new System.Windows.Controls.Border
        {
            Width = width ?? double.NaN,
            Height = height ?? double.NaN,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static System.Windows.Controls.Border CreateRedChildBox()
    {
        return new System.Windows.Controls.Border
        {
            Width = 30,
            Height = 30,
            Background = Brushes.Red,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    private static ModernGridEx CreateStarSpanGrid(double width, double height, double spacing)
    {
        var grid = new ModernGridEx
        {
            Width = width,
            Height = height,
            UseLayoutRounding = false,
            RowSpacing = spacing,
            ColumnSpacing = spacing
        };

        for (int i = 0; i < 4; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int i = 0; i < 3; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        return grid;
    }

    private static void AssertBoundsRelativeTo(FrameworkElement element, Visual ancestor, Rect expected)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point());
        var actual = new Rect(origin, element.RenderSize);
        Assert.AreEqual(expected.X, actual.X, 1.0, "X");
        Assert.AreEqual(expected.Y, actual.Y, 1.0, "Y");
        Assert.AreEqual(expected.Width, actual.Width, 2.0, "Width");
        Assert.AreEqual(expected.Height, actual.Height, 2.0, "Height");
    }

    private static void AssertTransitionBrush<T>(
        T control,
        Action<T, Brush> setBackground,
        Action<T, BrushTransition> setTransition,
        Action<T> clearTransition,
        Func<T, Brush> getEffectiveBackground)
    {
        var targetBrush = new SolidColorBrush(Colors.Blue);

        setBackground(control, Brushes.Red);
        setTransition(control, new BrushTransition { Duration = TimeSpan.FromSeconds(1) });
        setBackground(control, targetBrush);

        Assert.AreNotSame(targetBrush, getEffectiveBackground(control));

        clearTransition(control);

        Assert.AreSame(targetBrush, getEffectiveBackground(control));
    }

    private static void AssertMediumWindowedPopupInsets(ThemeShadowChrome chrome)
    {
        Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Medium, chrome.WindowedPopupInsetMode);
        Assert.AreEqual(new Thickness(10, 2, 10, 18), chrome.PopupShadowPadding);
    }

    private static void AssertShadowMetricsCoverContent(ThemeShadowChrome.ThemeShadowRenderer.ThemeShadowRenderMetrics metrics)
    {
        Assert.IsTrue(metrics.HasShadow);
        Assert.IsTrue(metrics.NonZeroBounds.X <= metrics.ContentLeft);
        Assert.IsTrue(metrics.NonZeroBounds.Y <= metrics.ContentTop);
        Assert.IsTrue(metrics.NonZeroBounds.X + metrics.NonZeroBounds.Width >= metrics.ContentLeft + metrics.ContentWidth);
        Assert.IsTrue(metrics.NonZeroBounds.Y + metrics.NonZeroBounds.Height >= metrics.ContentTop + metrics.ContentHeight);
    }

    private static void AssertShadowMetrics(
        ThemeShadowChrome.ThemeShadowRenderer.ThemeShadowRenderMetrics metrics,
        int bitmapWidth,
        int bitmapHeight,
        int contentLeft,
        int contentTop,
        int peakAlpha,
        int nonZeroPixelCount,
        Int32Rect nonZeroBounds,
        double alphaCentroidX,
        double alphaCentroidY,
        double alphaCentroidTolerance = 0.0001)
    {
        Assert.AreEqual(bitmapWidth, metrics.BitmapWidth);
        Assert.AreEqual(bitmapHeight, metrics.BitmapHeight);
        Assert.AreEqual(contentLeft, metrics.ContentLeft);
        Assert.AreEqual(contentTop, metrics.ContentTop);
        Assert.AreEqual(80, metrics.ContentWidth);
        Assert.AreEqual(40, metrics.ContentHeight);
        AssertShadowMetricsCoverContent(metrics);
        Assert.AreEqual(peakAlpha, metrics.PeakAlpha);
        Assert.AreEqual(nonZeroPixelCount, metrics.NonZeroPixelCount);
        Assert.AreEqual(nonZeroBounds, metrics.NonZeroBounds);
        Assert.AreEqual(alphaCentroidX, metrics.AlphaCentroidX, alphaCentroidTolerance);
        Assert.AreEqual(alphaCentroidY, metrics.AlphaCentroidY, alphaCentroidTolerance);
        Assert.IsTrue(metrics.AlphaCentroidY > metrics.ContentCenterY);
    }

    private static void AssertWinUIMockDCompShadowGeometry(
        ThemeShadowChrome.ThemeShadowRenderer.ThemeShadowRenderMetrics metrics,
        int bitmapWidth,
        int bitmapHeight,
        int contentLeft,
        int contentTop,
        int contentWidth,
        int contentHeight)
    {
        Assert.AreEqual(bitmapWidth, metrics.BitmapWidth);
        Assert.AreEqual(bitmapHeight, metrics.BitmapHeight);
        Assert.AreEqual(contentLeft, metrics.ContentLeft);
        Assert.AreEqual(contentTop, metrics.ContentTop);
        Assert.AreEqual(contentWidth, metrics.ContentWidth);
        Assert.AreEqual(contentHeight, metrics.ContentHeight);
        AssertShadowMetricsCoverContent(metrics);
        Assert.IsTrue(metrics.PeakAlpha > 0);
        Assert.IsTrue(metrics.AlphaCentroidY > metrics.ContentCenterY);
    }

    private static void AssertWinUIPixelMasterComparableShadow(
        ThemeShadowChrome.ThemeShadowRenderer.ThemeShadowRenderMetrics metrics,
        int canvasOffsetX,
        int canvasOffsetY,
        Int32Rect expectedCanvasBounds,
        int expectedPeakDarkening,
        int expectedShadowPixels,
        double expectedCanvasCentroidX,
        double expectedCanvasCentroidY)
    {
        var canvasBounds = new Int32Rect(
            metrics.NonZeroBounds.X + canvasOffsetX,
            metrics.NonZeroBounds.Y + canvasOffsetY,
            metrics.NonZeroBounds.Width,
            metrics.NonZeroBounds.Height);

        AssertNear(expectedCanvasBounds.X, canvasBounds.X, 2, "WinUI pixel-master shadow bounds X");
        AssertNear(expectedCanvasBounds.Y, canvasBounds.Y, 2, "WinUI pixel-master shadow bounds Y");
        AssertNear(expectedCanvasBounds.Width, canvasBounds.Width, 4, "WinUI pixel-master shadow bounds width");
        AssertNear(expectedCanvasBounds.Height, canvasBounds.Height, 4, "WinUI pixel-master shadow bounds height");
        AssertNear(expectedPeakDarkening, metrics.PeakAlpha, 4, "WinUI pixel-master peak darkening");
        AssertNear(expectedShadowPixels, metrics.NonZeroPixelCount, expectedShadowPixels * 0.2, "WinUI pixel-master shadow pixel count");
        Assert.AreEqual(expectedCanvasCentroidX, metrics.AlphaCentroidX + canvasOffsetX, 1.0);
        Assert.AreEqual(expectedCanvasCentroidY, metrics.AlphaCentroidY + canvasOffsetY, 1.0);
    }

    private static void AssertWinUIRenderedPixelMasterComparableShadow(
        RenderedShadowPixelStats stats,
        Int32Rect expectedCanvasBounds,
        int expectedPeakDarkening,
        int expectedShadowPixels,
        double expectedCanvasCentroidX,
        double expectedCanvasCentroidY)
    {
        AssertNear(expectedCanvasBounds.X, stats.Bounds.X, 2, $"WinUI rendered pixel-master shadow bounds X. Stats={stats}");
        AssertNear(expectedCanvasBounds.Y, stats.Bounds.Y, 2, $"WinUI rendered pixel-master shadow bounds Y. Stats={stats}");
        AssertNear(expectedCanvasBounds.Width, stats.Bounds.Width, 4, $"WinUI rendered pixel-master shadow bounds width. Stats={stats}");
        AssertNear(expectedCanvasBounds.Height, stats.Bounds.Height, 4, $"WinUI rendered pixel-master shadow bounds height. Stats={stats}");
        AssertNear(expectedPeakDarkening, stats.PeakDarkening, 4, $"WinUI rendered pixel-master peak darkening. Stats={stats}");
        AssertNear(expectedShadowPixels, stats.ShadowPixelCount, expectedShadowPixels * 0.2, $"WinUI rendered pixel-master shadow pixel count. Stats={stats}");
        Assert.AreEqual(expectedCanvasCentroidX, stats.CentroidX, 1.0);
        Assert.AreEqual(expectedCanvasCentroidY, stats.CentroidY, 1.0);
    }

    private static void AssertWinUIPixelMasterPngComparableShadow(
        RenderedShadowPixelStats actual,
        string mastersRoot,
        string masterName)
    {
        var masterPath = Path.Combine(mastersRoot, masterName);
        Assert.IsTrue(File.Exists(masterPath), $"Missing WinUI ThemeShadow pixel master '{masterPath}'.");

        var reference = MeasureWinUIThemeShadowSystemThemeRedrawPixelMaster(ReadShadowSnapshotPng(masterPath));
        AssertWinUIRenderedPixelMasterComparableShadow(actual, reference, masterName);
    }

    private static void AssertWinUIRenderedPixelMasterComparableShadow(
        RenderedShadowPixelStats actual,
        RenderedShadowPixelStats reference,
        string referenceName)
    {
        AssertNear(reference.Bounds.X, actual.Bounds.X, 2, $"{referenceName} shadow bounds X. Reference={reference}; Actual={actual}");
        AssertNear(reference.Bounds.Y, actual.Bounds.Y, 2, $"{referenceName} shadow bounds Y. Reference={reference}; Actual={actual}");
        AssertNear(reference.Bounds.Width, actual.Bounds.Width, 4, $"{referenceName} shadow bounds width. Reference={reference}; Actual={actual}");
        AssertNear(reference.Bounds.Height, actual.Bounds.Height, 4, $"{referenceName} shadow bounds height. Reference={reference}; Actual={actual}");
        AssertNear(reference.PeakDarkening, actual.PeakDarkening, 4, $"{referenceName} peak darkening. Reference={reference}; Actual={actual}");
        AssertNear(reference.ShadowPixelCount, actual.ShadowPixelCount, reference.ShadowPixelCount * 0.2, $"{referenceName} shadow pixel count. Reference={reference}; Actual={actual}");
        Assert.AreEqual(reference.CentroidX, actual.CentroidX, 1.0, $"{referenceName} shadow centroid X. Reference={reference}; Actual={actual}");
        Assert.AreEqual(reference.CentroidY, actual.CentroidY, 1.0, $"{referenceName} shadow centroid Y. Reference={reference}; Actual={actual}");
    }

    private static void AssertNear(double expected, double actual, double tolerance, string message)
    {
        Assert.IsTrue(Math.Abs(expected - actual) <= tolerance, $"{message}: expected {expected}, actual {actual}, tolerance {tolerance}.");
    }

    private static void AssertInheritedTextMetadata(DependencyProperty property, Type ownerType)
    {
        var metadata = (FrameworkPropertyMetadata)property.GetMetadata(ownerType);
        Assert.IsTrue(metadata.AffectsMeasure, $"{ownerType.Name}.{property.Name} should affect measure.");
        Assert.IsTrue(metadata.AffectsRender, $"{ownerType.Name}.{property.Name} should affect render.");
        Assert.IsTrue(metadata.Inherits, $"{ownerType.Name}.{property.Name} should inherit like WinUI text formatting properties.");
    }

    private static void AssertStackAxisOffsetRelativeTo(FrameworkElement element, Visual ancestor, Orientation orientation, double expected)
    {
        var actual = element.TransformToAncestor(ancestor).Transform(new Point());
        Assert.AreEqual(expected, orientation == Orientation.Horizontal ? actual.X : actual.Y, 1.0, orientation.ToString());
    }

    private static void AssertSnapPoints(float[] expected, IReadOnlyList<float> actual)
    {
        Assert.AreEqual(expected.Length, actual.Count);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], actual[i], 0.001f, $"Snap point {i}");
        }
    }

    private static SolidColorBrush AssertSolidColorBrush(Brush brush, Color expectedColor)
    {
        var solidColorBrush = brush as SolidColorBrush
            ?? throw new AssertFailedException("Expected a SolidColorBrush.");
        Assert.AreEqual(expectedColor, solidColorBrush.Color);
        return solidColorBrush;
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static T[] FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        var result = new List<T>();
        AddVisualChildren(root, result);
        return result.ToArray();
    }

    private static void AddVisualChildren<T>(DependencyObject root, List<T> result)
        where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typedChild)
            {
                result.Add(typedChild);
            }

            AddVisualChildren(child, result);
        }
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "SelectionStates");
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"SelectionStates.{stateName} should set {setterTarget}.");
    }

    private static void AssertAnimatedIconStateSetters(FrameworkElement stateGroupsRoot, string setterTarget)
    {
        AssertAnimatedIconStateSetter(stateGroupsRoot, "PointerOver", setterTarget, "PointerOver");
        AssertAnimatedIconStateSetter(stateGroupsRoot, "Pressed", setterTarget, "Pressed");
        AssertAnimatedIconStateSetter(stateGroupsRoot, "Disabled", setterTarget, "Normal");
    }

    private static void AssertAnimatedIconStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string setterTarget,
        string expectedValue)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        var setter = stateEx.Setters.SingleOrDefault(item => item.Target == setterTarget)
            ?? throw new AssertFailedException($"CommonStates.{stateName} should set {setterTarget}.");

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertAnimatedIconStateTransitions(Control control, DependencyObject stateTarget)
    {
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "PointerOver", false));
        Assert.AreEqual("PointerOver", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "Pressed", false));
        Assert.AreEqual("Pressed", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "Disabled", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));

        Assert.IsTrue(VisualStateManager.GoToState(control, "Normal", false));
        Assert.AreEqual("Normal", AnimatedIcon.GetState(stateTarget));
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static MenuItem CreateMenuItemWithTemplate(string templateResourceId, object header, object? icon, bool isEnabled)
    {
        return new MenuItem
        {
            Header = header,
            Icon = icon,
            IsEnabled = isEnabled,
            Template = FindMenuItemTemplate(templateResourceId)
        };
    }

    private static ControlTemplate FindMenuItemTemplate(string resourceId)
    {
        var key = new ComponentResourceKey(typeof(MenuItem), resourceId);
        return Application.Current.TryFindResource(key) as ControlTemplate
            ?? throw new AssertFailedException($"Expected MenuItem template resource '{resourceId}'.");
    }

    private static Style FindStyleResource(string resourceId)
    {
        return Application.Current.TryFindResource(resourceId) as Style
            ?? throw new AssertFailedException($"Expected style resource '{resourceId}'.");
    }

    private static void AssertMenuTemplatePresenterSlot(MenuItem menuItem)
    {
        var presenters = FindVisualChildren<ContentPresenter>(menuItem);

        Assert.IsTrue(
            presenters.Any(presenter => Equals(menuItem.Header, presenter.Content)),
            "Expected official WPF Fluent MenuItem template to present Header with a WPF ContentPresenter.");

        if (menuItem.Icon != null)
        {
            Assert.IsTrue(
                presenters.Any(presenter => Equals(menuItem.Icon, presenter.Content)),
                "Expected official WPF Fluent MenuItem template to present Icon with a WPF ContentPresenter.");
        }

        Assert.IsNull(FindVisualChild<ContentPresenterEx>(menuItem));
    }

    private static void AssertCalendarNavigationButtonPresenter(Button button)
    {
        var presenter = FindVisualChild<ContentPresenter>(button)
            ?? throw new AssertFailedException("Expected official WPF Fluent Calendar navigation button to use WPF ContentPresenter.");

        Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        Assert.AreEqual(button.Content, presenter.Content);
        Assert.IsNull(FindVisualChild<ContentPresenterEx>(button));
    }

    private static void AssertDataGridWpfPresenter(DependencyObject root, object expectedContent)
    {
        var presenter = FindVisualChildren<ContentPresenter>(root)
            .FirstOrDefault(item => Equals(item.Content, expectedContent))
            ?? throw new AssertFailedException($"Expected {root.GetType().Name} template to use WPF ContentPresenter.");
        Assert.AreEqual(expectedContent, presenter.Content);
        Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
        Assert.IsNull(FindVisualChild<ContentPresenterEx>(root));
    }

    private static ModernVariableSizedWrapGrid CreateVariableSizedWrapGrid(Orientation orientation, int itemCount)
    {
        var panel = new ModernVariableSizedWrapGrid
        {
            Width = 300,
            Height = 300,
            ItemWidth = 100,
            ItemHeight = 100,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static ModernWrapGrid CreateWrapGrid(Orientation orientation, int itemCount)
    {
        var panel = new ModernWrapGrid
        {
            Width = 300,
            Height = 300,
            ItemWidth = 100,
            ItemHeight = 100,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static ModernItemsStackPanel CreateItemsStackPanel(Orientation orientation, int itemCount)
    {
        var panel = new ModernItemsStackPanel
        {
            Width = 300,
            Height = 300,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static ModernItemsWrapGrid CreateItemsWrapGrid(Orientation orientation, int itemCount)
    {
        var panel = new ModernItemsWrapGrid
        {
            Width = 300,
            Height = 300,
            ItemWidth = 100,
            ItemHeight = 100,
            Orientation = orientation
        };

        for (int i = 0; i < itemCount; i++)
        {
            panel.Children.Add(new Border
            {
                Width = 100,
                Height = 100,
                Background = i % 2 == 0 ? Brushes.Red : Brushes.Blue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        return panel;
    }

    private static void AssertItemsStackPanelPositions(ModernItemsStackPanel panel, IReadOnlyList<Point> expectedPositions)
    {
        panel.Measure(new Size(panel.Width, panel.Height));
        panel.Arrange(new Rect(0, 0, panel.Width, panel.Height));
        panel.UpdateLayout();

        Assert.AreEqual(expectedPositions.Count, panel.Children.Count);

        for (int i = 0; i < expectedPositions.Count; i++)
        {
            var actual = ((UIElement)panel.Children[i]).TranslatePoint(new Point(), panel);
            Assert.AreEqual(expectedPositions[i].X, actual.X, 0.1, $"Unexpected X position for item {i}.");
            Assert.AreEqual(expectedPositions[i].Y, actual.Y, 0.1, $"Unexpected Y position for item {i}.");
        }
    }

    private static void AssertVariableSizedWrapGridPositions(
        ModernVariableSizedWrapGrid panel,
        IReadOnlyList<Point> expectedPositions,
        int? expectedArrangedCount = null)
    {
        if (!expectedArrangedCount.HasValue)
        {
            panel.Measure(new Size(panel.Width, panel.Height));
            panel.Arrange(new Rect(0, 0, panel.Width, panel.Height));
            panel.UpdateLayout();
        }

        Assert.AreEqual(300, panel.DesiredSize.Width, 0.1);
        Assert.AreEqual(300, panel.DesiredSize.Height, 0.1);
        if (expectedArrangedCount.HasValue)
        {
            Assert.AreEqual(expectedArrangedCount.Value, expectedPositions.Count);
            Assert.IsTrue(panel.Children.Count >= expectedArrangedCount.Value);
        }
        else
        {
            Assert.AreEqual(expectedPositions.Count, panel.Children.Count);
        }

        for (int i = 0; i < expectedPositions.Count; i++)
        {
            var actual = ((UIElement)panel.Children[i]).TranslatePoint(new Point(), panel);
            Assert.AreEqual(expectedPositions[i].X, actual.X, 0.1, $"Unexpected X position for item {i}.");
            Assert.AreEqual(expectedPositions[i].Y, actual.Y, 0.1, $"Unexpected Y position for item {i}.");
        }
    }

    private static Color RenderBorderEdgePixel(BackgroundSizing backgroundSizing)
    {
        var border = new BorderEx
        {
            Width = 24,
            Height = 24,
            Background = Brushes.Red,
            BorderBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)),
            BorderThickness = new Thickness(6),
            BackgroundSizing = backgroundSizing
        };

        return RenderBorderPixel(border, 3, 12, 24, 24);
    }

    private static Color RenderBorderPixel(BorderEx border, int x, int y, int width, int height)
    {
        return RenderElementPixel(border, x, y, width, height);
    }

    private static void AssertOuterChromePixels(FrameworkElement element)
    {
        var roundedCorner = RenderElementPixel(element, 27, 1, 30, 30);
        var straightEdge = RenderElementPixel(element, 1, 15, 30, 30);

        Assert.IsTrue(roundedCorner.A < 30, $"Expected inflated WinUI outer corner to clip the pixel. Pixel={roundedCorner}");
        Assert.IsTrue(straightEdge.R > 200 && straightEdge.A > 200, $"Expected outer edge background under the transparent border. Pixel={straightEdge}");
    }

    private static void AssertRoundedChromeHitTest(FrameworkElement element)
    {
        element.Measure(new Size(30, 30));
        element.Arrange(new Rect(0, 0, 30, 30));
        element.UpdateLayout();

        Assert.IsNull(VisualTreeHelper.HitTest(element, new Point(1, 1)), "Top-left point should be clipped by the rounded chrome.");
        Assert.IsNotNull(VisualTreeHelper.HitTest(element, new Point(15, 15)), "Center point should hit inside the rounded chrome.");
    }

    private static void AssertRoundedChildRenderClip(FrameworkElement element)
    {
        var clippedCorner = RenderElementPixel(element, 1, 1, 30, 30);
        var center = RenderElementPixel(element, 15, 15, 30, 30);

        Assert.IsTrue(clippedCorner.A < 30, $"Expected child content to be clipped out of the rounded corner. Pixel={clippedCorner}");
        Assert.IsTrue(center.R > 200 && center.A > 200, $"Expected child content to render inside the rounded clip. Pixel={center}");
    }

    private static void AssertDynamicRoundedChildClip(FrameworkElement element, Action<CornerRadius> setCornerRadius)
    {
        element.HorizontalAlignment = HorizontalAlignment.Left;
        element.VerticalAlignment = VerticalAlignment.Top;

        setCornerRadius(new CornerRadius());
        using var host = new TestWindowHost(element, width: 120, height: 90);

        var squareCorner = RenderCurrentElementPixel(element, 1, 1, 30, 30);
        Assert.IsTrue(squareCorner.R > 200 && squareCorner.A > 200, $"Expected square corner content before radius change. Pixel={squareCorner}");

        setCornerRadius(new CornerRadius(12, 0, 0, 0));
        host.UpdateLayout();

        var clippedCorner = RenderCurrentElementPixel(element, 1, 1, 30, 30);
        Assert.IsTrue(clippedCorner.A < 30, $"Expected rounded corner clip to refresh after CornerRadius change. Pixel={clippedCorner}");
    }

    private static RenderedShadowPixelStats RenderThemeShadowSourceCanvas(ElementTheme theme)
    {
        var (root, chrome) = CreateThemeShadowSourceCanvas(theme);
        AssertThemeShadowSourceCanvasLayout(root, chrome);

        return MeasureRenderedShadowPixels(root, 100, 100);
    }

    private static (Grid Root, ThemeShadowChrome Chrome) CreateThemeShadowSourceCanvas(ElementTheme theme)
    {
        var chrome = new ThemeShadowChrome
        {
            Depth = 32,
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(9, 17, 0, 0),
            Child = new Border
            {
                Width = 50,
                Height = 50,
                Background = Brushes.Transparent
            }
        };
        var root = new Grid
        {
            Width = 100,
            Height = 100,
            Background = Brushes.White
        };
        ThemeManager.SetRequestedTheme(root, theme);
        root.Children.Add(chrome);

        ArrangeSourceCanvas(root);
        return (root, chrome);
    }

    private static void AssertThemeShadowSourceCanvasLayout(FrameworkElement root, ThemeShadowChrome chrome)
    {
        Assert.AreEqual(82, chrome.ActualWidth, 0.1);
        Assert.AreEqual(82, chrome.ActualHeight, 0.1);
        Assert.AreEqual(new Point(25, 25), ((FrameworkElement)chrome.Child).TranslatePoint(new Point(), root));
    }

    private static void ArrangeSourceCanvas(FrameworkElement root)
    {
        root.Measure(new Size(100, 100));
        root.Arrange(new Rect(0, 0, 100, 100));
        root.UpdateLayout();
    }

    private static RenderedShadowPixelStats RenderThemeShadowChildlessSourceCanvas(ElementTheme theme)
    {
        var chrome = new ThemeShadowChrome
        {
            Width = 50,
            Height = 50,
            Depth = 32,
            CornerRadius = new CornerRadius(4),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(25)
        };
        var root = new Grid
        {
            Width = 100,
            Height = 100,
            Background = Brushes.White
        };
        ThemeManager.SetRequestedTheme(root, theme);
        root.Children.Add(chrome);

        root.Measure(new Size(100, 100));
        root.Arrange(new Rect(0, 0, 100, 100));
        root.UpdateLayout();

        Assert.AreEqual(50, chrome.ActualWidth, 0.1);
        Assert.AreEqual(50, chrome.ActualHeight, 0.1);

        return MeasureRenderedShadowPixels(root, 100, 100);
    }

    private static Color RenderElementPixel(FrameworkElement element, int x, int y, int width, int height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        return RenderCurrentElementPixel(element, x, y, width, height);
    }

    private static Color RenderCurrentElementPixel(FrameworkElement element, int x, int y, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        var pixels = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
        return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
    }

    private static void WaitForRendering()
    {
        var frame = new DispatcherFrame();
        var rendered = false;
        EventHandler renderingHandler = (_, _) =>
        {
            rendered = true;
            frame.Continue = false;
        };
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        timer.Tick += (_, _) => frame.Continue = false;

        try
        {
            CompositionTarget.Rendering += renderingHandler;
            timer.Start();
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            timer.Stop();
            CompositionTarget.Rendering -= renderingHandler;
        }

        Assert.IsTrue(rendered, "Timed out waiting for a WPF render tick.");
    }

    private static void WaitForDispatcherDelay(int milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(milliseconds)
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };

        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static Grid CreateWhiteCanvas(double width, double height)
    {
        return new Grid
        {
            Width = width,
            Height = height,
            Background = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
    }

    private static void ArrangeElement(FrameworkElement element, double width, double height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();
    }

    private static void AssertRenderedTemplateShadow(
        FrameworkElement root,
        ThemeShadowChrome chrome,
        int width,
        int height,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        if (chrome.Child is not FrameworkElement caster)
        {
            throw new AssertFailedException($"{templateName} shadow chrome should have a framework-element caster child.");
        }

        var casterBounds = GetElementBounds(caster, root);
        var previousVisibility = caster.Visibility;

        try
        {
            ExportShadowSnapshotIfRequested(templateName, "surface", root, width, height, null);

            caster.Visibility = Visibility.Hidden;
            root.UpdateLayout();
            WpfTestHost.DoEvents();

            var stats = MeasureRenderedShadowPixels(root, width, height);
            ExportShadowSnapshotIfRequested(templateName, "shadow-only", root, width, height, stats, ToShadowSnapshotBounds(casterBounds));
            AssertRenderedShadowExtendsBeyondCaster(stats, casterBounds, minPeakDarkening, minShadowPixels, templateName);
        }
        finally
        {
            caster.Visibility = previousVisibility;
            root.UpdateLayout();
        }
    }

    private static void AssertRenderedTemplateShadowVisible(
        FrameworkElement root,
        ThemeShadowChrome chrome,
        int width,
        int height,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        if (chrome.Child is not FrameworkElement caster)
        {
            throw new AssertFailedException($"{templateName} shadow chrome should have a framework-element caster child.");
        }

        var casterBounds = GetElementBounds(caster, root);
        var previousVisibility = caster.Visibility;

        try
        {
            ExportShadowSnapshotIfRequested(templateName, "surface", root, width, height, null);

            caster.Visibility = Visibility.Hidden;
            root.UpdateLayout();
            WpfTestHost.DoEvents();

            var stats = MeasureRenderedShadowPixels(root, width, height);
            ExportShadowSnapshotIfRequested(templateName, "shadow-only", root, width, height, stats, ToShadowSnapshotBounds(casterBounds));
            AssertRenderedShadowVisible(stats, casterBounds, minPeakDarkening, minShadowPixels, templateName);
        }
        finally
        {
            caster.Visibility = previousVisibility;
            root.UpdateLayout();
        }
    }

    private static void AssertDetachedTemplateShadow(
        ThemeShadowChrome chrome,
        int canvasWidth,
        int canvasHeight,
        Thickness margin,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        var parent = VisualTreeHelper.GetParent(chrome) as Panel;
        var parentIndex = parent?.Children.IndexOf(chrome) ?? -1;
        var previousIsShadowEnabled = chrome.IsShadowEnabled;
        var previousHorizontalAlignment = chrome.HorizontalAlignment;
        var previousVerticalAlignment = chrome.VerticalAlignment;
        var previousMargin = chrome.Margin;
        var previousOpacity = chrome.Opacity;
        var previousRenderTransform = chrome.RenderTransform;

        if (parent != null)
        {
            parent.Children.RemoveAt(parentIndex);
        }

        try
        {
            chrome.IsShadowEnabled = true;
            chrome.HorizontalAlignment = HorizontalAlignment.Left;
            chrome.VerticalAlignment = VerticalAlignment.Top;
            chrome.Margin = margin;
            chrome.Opacity = 1.0;
            chrome.RenderTransform = null;

            var root = CreateWhiteCanvas(canvasWidth, canvasHeight);
            ThemeManager.SetRequestedTheme(root, ElementTheme.Light);
            root.Children.Add(chrome);
            try
            {
                ArrangeElement(root, canvasWidth, canvasHeight);
                AssertRenderedTemplateShadow(root, chrome, canvasWidth, canvasHeight, minPeakDarkening, minShadowPixels, templateName);
            }
            finally
            {
                root.Children.Remove(chrome);
            }
        }
        finally
        {
            chrome.IsShadowEnabled = previousIsShadowEnabled;
            chrome.HorizontalAlignment = previousHorizontalAlignment;
            chrome.VerticalAlignment = previousVerticalAlignment;
            chrome.Margin = previousMargin;
            chrome.Opacity = previousOpacity;
            chrome.RenderTransform = previousRenderTransform;

            if (parent != null)
            {
                parent.Children.Insert(parentIndex, chrome);
            }
        }
    }

    private static void AssertDetachedChildlessTemplateShadow(
        ThemeShadowChrome chrome,
        double casterWidth,
        double casterHeight,
        int canvasWidth,
        int canvasHeight,
        Thickness margin,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        if (VisualTreeHelper.GetParent(chrome) is not Panel parent)
        {
            throw new AssertFailedException($"{templateName} shadow chrome should be parented by a panel in the source-backed template.");
        }

        var index = parent.Children.IndexOf(chrome);
        Assert.IsTrue(index >= 0, $"{templateName} shadow chrome should be present in its template parent.");

        parent.Children.RemoveAt(index);
        try
        {
            chrome.IsShadowEnabled = true;
            chrome.Width = casterWidth;
            chrome.Height = casterHeight;
            chrome.Margin = margin;
            chrome.HorizontalAlignment = HorizontalAlignment.Left;
            chrome.VerticalAlignment = VerticalAlignment.Top;
            chrome.Opacity = 1.0;
            chrome.RenderTransform = null;

            var root = CreateWhiteCanvas(canvasWidth, canvasHeight);
            ThemeManager.SetRequestedTheme(root, ElementTheme.Light);
            root.Children.Add(chrome);
            try
            {
                ArrangeElement(root, canvasWidth, canvasHeight);
                Assert.AreEqual(casterWidth, chrome.ActualWidth, 0.1);
                Assert.AreEqual(casterHeight, chrome.ActualHeight, 0.1);

                AssertRenderedChildlessTemplateShadow(root, chrome, canvasWidth, canvasHeight, minPeakDarkening, minShadowPixels, templateName);
            }
            finally
            {
                root.Children.Remove(chrome);
            }
        }
        finally
        {
            parent.Children.Insert(index, chrome);
        }
    }

    private static void AssertRenderedChildlessTemplateShadow(
        FrameworkElement root,
        ThemeShadowChrome chrome,
        int width,
        int height,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        var casterBounds = GetElementBounds(chrome, root);
        var stats = MeasureRenderedShadowPixels(root, width, height);
        ExportShadowSnapshotIfRequested(templateName, "shadow-only", root, width, height, stats, ToShadowSnapshotBounds(casterBounds));
        AssertRenderedShadowExtendsBeyondCaster(stats, casterBounds, minPeakDarkening, minShadowPixels, templateName);
    }

    private static Rect GetElementBounds(FrameworkElement element, UIElement ancestor)
    {
        var topLeft = element.TranslatePoint(new Point(), ancestor);
        return new Rect(topLeft, new Size(element.ActualWidth, element.ActualHeight));
    }

    private static Int32Rect ToShadowSnapshotBounds(Rect bounds)
    {
        return new Int32Rect(
            (int)Math.Round(bounds.X),
            (int)Math.Round(bounds.Y),
            (int)Math.Round(bounds.Width),
            (int)Math.Round(bounds.Height));
    }

    private static void AssertRenderedShadowExtendsBeyondCaster(
        RenderedShadowPixelStats stats,
        Rect casterBounds,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        var boundsRight = stats.Bounds.X + stats.Bounds.Width;
        var boundsBottom = stats.Bounds.Y + stats.Bounds.Height;
        var casterCenterY = casterBounds.Top + (casterBounds.Height / 2);

        Assert.IsTrue(
            stats.PeakDarkening >= minPeakDarkening,
            $"{templateName} should render a visible ThemeShadow. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            stats.ShadowPixelCount >= minShadowPixels,
            $"{templateName} should render enough shadow pixels to cover the source-backed template caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            stats.Bounds.X < casterBounds.Left,
            $"{templateName} shadow should extend left of the caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            stats.Bounds.Y <= casterBounds.Top + 4,
            $"{templateName} shadow should reach the upper caster edge within the clipped windowed-popup inset. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            boundsRight > casterBounds.Right,
            $"{templateName} shadow should extend right of the caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            boundsBottom > casterBounds.Bottom,
            $"{templateName} shadow should extend below the caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            stats.CentroidY > casterCenterY,
            $"{templateName} shadow centroid should sit below the caster center like WinUI ThemeShadow. Stats={stats}; Caster={casterBounds}");
    }

    private static void AssertRenderedShadowVisible(
        RenderedShadowPixelStats stats,
        Rect casterBounds,
        int minPeakDarkening,
        int minShadowPixels,
        string templateName)
    {
        var boundsRight = stats.Bounds.X + stats.Bounds.Width;
        var boundsBottom = stats.Bounds.Y + stats.Bounds.Height;
        var casterCenterY = casterBounds.Top + (casterBounds.Height / 2);

        Assert.IsTrue(
            stats.PeakDarkening >= minPeakDarkening,
            $"{templateName} should render a visible ThemeShadow. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            stats.ShadowPixelCount >= minShadowPixels,
            $"{templateName} should render enough shadow pixels to cover the source-backed template caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            boundsRight > casterBounds.Right,
            $"{templateName} shadow should extend right of the caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            boundsBottom > casterBounds.Bottom,
            $"{templateName} shadow should extend below the caster. Stats={stats}; Caster={casterBounds}");
        Assert.IsTrue(
            stats.CentroidY > casterCenterY,
            $"{templateName} shadow centroid should sit below the caster center like WinUI ThemeShadow. Stats={stats}; Caster={casterBounds}");
    }

    private static RenderedShadowPixelStats MeasureRenderedShadowPixels(FrameworkElement element, int width, int height)
    {
        return MeasureShadowPixels(RenderShadowSnapshotBitmap(element, width, height));
    }

    private static RenderTargetBitmap RenderShadowSnapshotBitmap(FrameworkElement element, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);
        return bitmap;
    }

    private static RenderedShadowPixelStats MeasureShadowPixels(BitmapSource bitmap)
    {
        return MeasureShadowPixels(bitmap, ignoredBounds: null);
    }

    private static RenderedShadowPixelStats MeasureWinUIThemeShadowSystemThemeRedrawPixelMaster(BitmapSource bitmap)
    {
        return MeasureShadowPixels(bitmap, new Int32Rect(25, 25, 50, 50));
    }

    private static RenderedShadowPixelStats MeasureShadowPixels(BitmapSource bitmap, Int32Rect? ignoredBounds)
    {
        var darkeningPixels = GetShadowSnapshotDarkeningPixels(bitmap);
        int width = bitmap.PixelWidth;
        int height = bitmap.PixelHeight;

        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;
        int peakDarkening = 0;
        int shadowPixelCount = 0;
        long darkeningSum = 0;
        long weightedX = 0;
        long weightedY = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (ignoredBounds.HasValue && Contains(ignoredBounds.Value, x, y))
                {
                    continue;
                }

                int darkening = darkeningPixels[(y * width) + x];

                if (darkening <= 0)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
                peakDarkening = Math.Max(peakDarkening, darkening);
                shadowPixelCount++;
                darkeningSum += darkening;
                weightedX += (long)x * darkening;
                weightedY += (long)y * darkening;
            }
        }

        var bounds = shadowPixelCount == 0
            ? new Int32Rect()
            : new Int32Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        double centroidX = darkeningSum == 0 ? double.NaN : (double)weightedX / darkeningSum;
        double centroidY = darkeningSum == 0 ? double.NaN : (double)weightedY / darkeningSum;

        return new RenderedShadowPixelStats(bounds, peakDarkening, shadowPixelCount, centroidX, centroidY);
    }

    private static bool Contains(Int32Rect bounds, int x, int y)
    {
        return x >= bounds.X
            && x < bounds.X + bounds.Width
            && y >= bounds.Y
            && y < bounds.Y + bounds.Height;
    }

    private static void ExportShadowSnapshotIfRequested(
        string templateName,
        string snapshotKind,
        FrameworkElement element,
        int width,
        int height,
        RenderedShadowPixelStats? stats,
        Int32Rect? ignoredReferenceBounds = null)
    {
        var fileBase = SanitizeSnapshotName($"{templateName}-{snapshotKind}");
        var snapshotRoot = Environment.GetEnvironmentVariable("MODERNWPF_SHADOW_SNAPSHOT_DIR");

        if (!string.IsNullOrWhiteSpace(snapshotRoot))
        {
            Directory.CreateDirectory(snapshotRoot);

            var bitmap = RenderShadowSnapshotBitmap(element, width, height);

            using (var stream = File.Create(Path.Combine(snapshotRoot, fileBase + ".png")))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
            }

            if (stats.HasValue)
            {
                File.WriteAllText(
                    Path.Combine(snapshotRoot, fileBase + ".txt"),
                    CreateShadowSnapshotMetricsText(templateName, snapshotKind, width, height, stats.Value));

                if (ignoredReferenceBounds.HasValue)
                {
                    File.WriteAllText(
                        Path.Combine(snapshotRoot, fileBase + ".mask.txt"),
                        CreateShadowSnapshotReferenceMaskText(templateName, snapshotKind, width, height, ignoredReferenceBounds.Value));
                }
            }
        }

        CompareShadowSnapshotReferenceIfRequested(templateName, snapshotKind, fileBase, element, width, height, stats);
    }

    private static string SanitizeSnapshotName(string name)
    {
        return Regex.Replace(name, @"[^A-Za-z0-9_.-]+", "-").Trim('-');
    }

    private static string CreateShadowSnapshotMetricsText(
        string templateName,
        string snapshotKind,
        int width,
        int height,
        RenderedShadowPixelStats stats)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Name={0}{5}" +
            "Kind={1}{5}" +
            "Size={2}x{3}{5}" +
            "Bounds={4}{5}" +
            "PeakDarkening={6}{5}" +
            "ShadowPixelCount={7}{5}" +
            "Centroid={8:0.###},{9:0.###}{5}" +
            "Stats={10}{5}",
            templateName,
            snapshotKind,
            width,
            height,
            FormatShadowSnapshotBounds(stats.Bounds),
            Environment.NewLine,
            stats.PeakDarkening,
            stats.ShadowPixelCount,
            stats.CentroidX,
            stats.CentroidY,
            stats);
    }

    private static string CreateShadowSnapshotReferenceMaskText(
        string templateName,
        string snapshotKind,
        int width,
        int height,
        Int32Rect ignoredBounds)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Name={0}{4}" +
            "Kind={1}{4}" +
            "Size={2}x{3}{4}" +
            "IgnoredBounds={5}{4}",
            templateName,
            snapshotKind,
            width,
            height,
            Environment.NewLine,
            FormatShadowSnapshotBounds(ignoredBounds));
    }

    private static void CompareShadowSnapshotReferenceIfRequested(
        string templateName,
        string snapshotKind,
        string fileBase,
        FrameworkElement element,
        int width,
        int height,
        RenderedShadowPixelStats? stats)
    {
        var referenceRoot = Environment.GetEnvironmentVariable("MODERNWPF_SHADOW_REFERENCE_DIR");
        if (string.IsNullOrWhiteSpace(referenceRoot) || !stats.HasValue)
        {
            return;
        }

        var metricsPath = Path.Combine(referenceRoot, fileBase + ".txt");
        var imagePath = Path.Combine(referenceRoot, fileBase + ".png");
        var referenceIgnoredBounds = ReadShadowSnapshotReferenceMaskIfExists(
            Path.Combine(referenceRoot, fileBase + ".mask.txt"),
            templateName,
            snapshotKind,
            width,
            height);
        var comparedReference = false;

        if (File.Exists(metricsPath))
        {
            var reference = ReadShadowSnapshotMetrics(metricsPath);

            Assert.AreEqual(templateName, reference.Name, $"Unexpected shadow reference name in {metricsPath}.");
            Assert.AreEqual(snapshotKind, reference.Kind, $"Unexpected shadow reference kind in {metricsPath}.");
            Assert.AreEqual(width, reference.Width, $"Unexpected shadow reference width in {metricsPath}.");
            Assert.AreEqual(height, reference.Height, $"Unexpected shadow reference height in {metricsPath}.");
            AssertShadowSnapshotStatsMatchReference(templateName, fileBase, reference.Stats, stats.Value);
            comparedReference = true;
        }

        if (File.Exists(imagePath))
        {
            var referenceImage = ReadShadowSnapshotPng(imagePath);
            var actualImage = RenderShadowSnapshotBitmap(element, width, height);
            var reference = CreateShadowSnapshotMetricsFromReferenceBitmap(templateName, snapshotKind, imagePath, referenceImage, referenceIgnoredBounds);
            var actualStats = referenceIgnoredBounds.HasValue
                ? MeasureShadowPixels(actualImage, referenceIgnoredBounds)
                : stats.Value;

            Assert.AreEqual(width, reference.Width, $"Unexpected shadow reference image width in {imagePath}.");
            Assert.AreEqual(height, reference.Height, $"Unexpected shadow reference image height in {imagePath}.");
            AssertShadowSnapshotStatsMatchReference(templateName, fileBase, reference.Stats, actualStats);
            AssertShadowSnapshotImageMatchesReference(templateName, fileBase, referenceImage, actualImage, referenceIgnoredBounds);
            comparedReference = true;
        }

        if (!comparedReference)
        {
            throw new AssertFailedException(
                $"Missing WinUI shadow reference '{metricsPath}' or '{imagePath}' for {templateName} {snapshotKind}. " +
                "Unset MODERNWPF_SHADOW_REFERENCE_DIR or add the matching WinUI metrics or PNG file.");
        }
    }

    private static RenderedShadowSnapshotMetrics ReadShadowSnapshotMetrics(string metricsPath)
    {
        return ParseShadowSnapshotMetrics(metricsPath, File.ReadLines(metricsPath));
    }

    private static BitmapSource ReadShadowSnapshotPng(string imagePath)
    {
        using (var stream = File.OpenRead(imagePath))
        {
            var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return decoder.Frames[0];
        }
    }

    private static Int32Rect? ReadShadowSnapshotReferenceMaskIfExists(
        string maskPath,
        string templateName,
        string snapshotKind,
        int width,
        int height)
    {
        if (!File.Exists(maskPath))
        {
            return null;
        }

        var mask = ParseShadowSnapshotReferenceMask(maskPath, File.ReadLines(maskPath));
        Assert.AreEqual(templateName, mask.Name, $"Unexpected shadow reference mask name in {maskPath}.");
        Assert.AreEqual(snapshotKind, mask.Kind, $"Unexpected shadow reference mask kind in {maskPath}.");
        Assert.AreEqual(width, mask.Width, $"Unexpected shadow reference mask width in {maskPath}.");
        Assert.AreEqual(height, mask.Height, $"Unexpected shadow reference mask height in {maskPath}.");
        AssertShadowSnapshotReferenceMaskBounds(mask.IgnoredBounds, width, height, maskPath);
        return mask.IgnoredBounds;
    }

    private static RenderedShadowSnapshotMetrics CreateShadowSnapshotMetricsFromReferenceBitmap(
        string templateName,
        string snapshotKind,
        string imageName,
        BitmapSource bitmap,
        Int32Rect? ignoredBounds = null)
    {
        if (bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0)
        {
            throw new AssertFailedException($"Shadow snapshot reference image '{imageName}' has invalid dimensions {bitmap.PixelWidth}x{bitmap.PixelHeight}.");
        }

        return new RenderedShadowSnapshotMetrics(
            templateName,
            snapshotKind,
            bitmap.PixelWidth,
            bitmap.PixelHeight,
            MeasureShadowPixels(bitmap, ignoredBounds));
    }

    private static RenderedShadowSnapshotMetrics ParseShadowSnapshotMetrics(string metricsName, IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            var separator = rawLine.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[rawLine.Substring(0, separator)] = rawLine.Substring(separator + 1);
        }

        var (width, height) = ParseShadowSnapshotSize(RequireShadowSnapshotMetric(values, "Size", metricsName), metricsName);
        var (centroidX, centroidY) = ParseShadowSnapshotPoint(RequireShadowSnapshotMetric(values, "Centroid", metricsName), "Centroid", metricsName);

        return new RenderedShadowSnapshotMetrics(
            RequireShadowSnapshotMetric(values, "Name", metricsName),
            RequireShadowSnapshotMetric(values, "Kind", metricsName),
            width,
            height,
            new RenderedShadowPixelStats(
                ParseShadowSnapshotBounds(RequireShadowSnapshotMetric(values, "Bounds", metricsName), metricsName),
                ParseShadowSnapshotInt(RequireShadowSnapshotMetric(values, "PeakDarkening", metricsName), "PeakDarkening", metricsName),
                ParseShadowSnapshotInt(RequireShadowSnapshotMetric(values, "ShadowPixelCount", metricsName), "ShadowPixelCount", metricsName),
                centroidX,
                centroidY));
    }

    private static RenderedShadowSnapshotReferenceMask ParseShadowSnapshotReferenceMask(string maskName, IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                continue;
            }

            var separator = rawLine.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[rawLine.Substring(0, separator)] = rawLine.Substring(separator + 1);
        }

        var (width, height) = ParseShadowSnapshotSize(RequireShadowSnapshotMetric(values, "Size", maskName), maskName);

        return new RenderedShadowSnapshotReferenceMask(
            RequireShadowSnapshotMetric(values, "Name", maskName),
            RequireShadowSnapshotMetric(values, "Kind", maskName),
            width,
            height,
            ParseShadowSnapshotBounds(RequireShadowSnapshotMetric(values, "IgnoredBounds", maskName), maskName));
    }

    private static string RequireShadowSnapshotMetric(IReadOnlyDictionary<string, string> values, string name, string metricsName)
    {
        if (values.TryGetValue(name, out var value))
        {
            return value;
        }

        throw new AssertFailedException($"Shadow snapshot metrics '{metricsName}' is missing '{name}'.");
    }

    private static (int Width, int Height) ParseShadowSnapshotSize(string value, string metricsName)
    {
        var parts = value.Split('x');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) &&
            int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
        {
            return (width, height);
        }

        throw new AssertFailedException($"Shadow snapshot metrics '{metricsName}' has invalid Size '{value}'.");
    }

    private static Int32Rect ParseShadowSnapshotBounds(string value, string metricsName)
    {
        var parts = value.Split(',');
        if (parts.Length == 4 &&
            int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) &&
            int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) &&
            int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) &&
            int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
        {
            return new Int32Rect(x, y, width, height);
        }

        throw new AssertFailedException($"Shadow snapshot metrics '{metricsName}' has invalid Bounds '{value}'.");
    }

    private static (double X, double Y) ParseShadowSnapshotPoint(string value, string name, string metricsName)
    {
        var parts = value.Split(',');
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return (x, y);
        }

        throw new AssertFailedException($"Shadow snapshot metrics '{metricsName}' has invalid {name} '{value}'.");
    }

    private static int ParseShadowSnapshotInt(string value, string name, string metricsName)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new AssertFailedException($"Shadow snapshot metrics '{metricsName}' has invalid {name} '{value}'.");
    }

    private static string FormatShadowSnapshotBounds(Int32Rect bounds)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0},{1},{2},{3}",
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height);
    }

    private static void AssertShadowSnapshotStatsMatchReference(
        string templateName,
        string fileBase,
        RenderedShadowPixelStats reference,
        RenderedShadowPixelStats actual)
    {
        const double boundsPositionTolerance = 8;
        const double boundsSizeTolerance = 8;
        const double peakDarkeningTolerance = 10;
        const double shadowPixelRelativeTolerance = 0.35;
        const double shadowPixelMinimumTolerance = 400;
        const double centroidTolerance = 8;

        AssertNear(reference.Bounds.X, actual.Bounds.X, boundsPositionTolerance, $"{templateName} reference shadow bounds X ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(reference.Bounds.Y, actual.Bounds.Y, boundsPositionTolerance, $"{templateName} reference shadow bounds Y ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(reference.Bounds.Width, actual.Bounds.Width, boundsSizeTolerance, $"{templateName} reference shadow bounds width ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(reference.Bounds.Height, actual.Bounds.Height, boundsSizeTolerance, $"{templateName} reference shadow bounds height ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(reference.PeakDarkening, actual.PeakDarkening, peakDarkeningTolerance, $"{templateName} reference shadow peak darkening ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(
            reference.ShadowPixelCount,
            actual.ShadowPixelCount,
            Math.Max(shadowPixelMinimumTolerance, reference.ShadowPixelCount * shadowPixelRelativeTolerance),
            $"{templateName} reference shadow pixel count ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(reference.CentroidX, actual.CentroidX, centroidTolerance, $"{templateName} reference shadow centroid X ({fileBase}). Reference={reference}; Actual={actual}");
        AssertNear(reference.CentroidY, actual.CentroidY, centroidTolerance, $"{templateName} reference shadow centroid Y ({fileBase}). Reference={reference}; Actual={actual}");
    }

    private static void AssertShadowSnapshotImageMatchesReference(
        string templateName,
        string fileBase,
        BitmapSource reference,
        BitmapSource actual,
        Int32Rect? ignoredBounds = null)
    {
        Assert.AreEqual(reference.PixelWidth, actual.PixelWidth, $"{templateName} reference shadow image width ({fileBase}).");
        Assert.AreEqual(reference.PixelHeight, actual.PixelHeight, $"{templateName} reference shadow image height ({fileBase}).");

        var referenceDarkening = GetShadowSnapshotDarkeningPixels(reference);
        var actualDarkening = GetShadowSnapshotDarkeningPixels(actual);
        ClearIgnoredShadowSnapshotBounds(referenceDarkening, reference.PixelWidth, ignoredBounds);
        ClearIgnoredShadowSnapshotBounds(actualDarkening, actual.PixelWidth, ignoredBounds);

        long canvasDelta = 0;
        long shadowDelta = 0;
        int shadowUnionPixels = 0;
        int changedShadowPixels = 0;
        int maxShadowDelta = 0;

        for (int i = 0; i < referenceDarkening.Length; i++)
        {
            int delta = Math.Abs(referenceDarkening[i] - actualDarkening[i]);
            canvasDelta += delta;

            if (referenceDarkening[i] > 0 || actualDarkening[i] > 0)
            {
                shadowUnionPixels++;
                shadowDelta += delta;
                maxShadowDelta = Math.Max(maxShadowDelta, delta);

                if (delta > 8)
                {
                    changedShadowPixels++;
                }
            }
        }

        var totalPixels = reference.PixelWidth * reference.PixelHeight;
        var meanCanvasDelta = totalPixels == 0 ? 0 : (double)canvasDelta / totalPixels;
        var meanShadowDelta = shadowUnionPixels == 0 ? 0 : (double)shadowDelta / shadowUnionPixels;
        var changedShadowPixelTolerance = Math.Max(400, shadowUnionPixels * 0.45);

        AssertNear(0, meanCanvasDelta, 6, $"{templateName} reference image mean canvas darkening delta ({fileBase}). ShadowUnion={shadowUnionPixels}; MaxDelta={maxShadowDelta}");
        AssertNear(0, meanShadowDelta, 18, $"{templateName} reference image mean shadow darkening delta ({fileBase}). ShadowUnion={shadowUnionPixels}; MaxDelta={maxShadowDelta}");
        AssertNear(0, changedShadowPixels, changedShadowPixelTolerance, $"{templateName} reference image changed shadow-pixel count ({fileBase}). ShadowUnion={shadowUnionPixels}; MaxDelta={maxShadowDelta}");
    }

    private static void AssertShadowSnapshotReferenceMaskBounds(Int32Rect bounds, int width, int height, string maskName)
    {
        Assert.IsTrue(bounds.X >= 0, $"Shadow snapshot reference mask '{maskName}' has negative X.");
        Assert.IsTrue(bounds.Y >= 0, $"Shadow snapshot reference mask '{maskName}' has negative Y.");
        Assert.IsTrue(bounds.Width >= 0, $"Shadow snapshot reference mask '{maskName}' has negative width.");
        Assert.IsTrue(bounds.Height >= 0, $"Shadow snapshot reference mask '{maskName}' has negative height.");
        Assert.IsTrue(bounds.X + bounds.Width <= width, $"Shadow snapshot reference mask '{maskName}' exceeds image width.");
        Assert.IsTrue(bounds.Y + bounds.Height <= height, $"Shadow snapshot reference mask '{maskName}' exceeds image height.");
    }

    private static void ClearIgnoredShadowSnapshotBounds(int[] darkeningPixels, int width, Int32Rect? ignoredBounds)
    {
        if (!ignoredBounds.HasValue || ignoredBounds.Value.IsEmpty)
        {
            return;
        }

        var bounds = ignoredBounds.Value;
        for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
        {
            for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
            {
                darkeningPixels[(y * width) + x] = 0;
            }
        }
    }

    private static int[] GetShadowSnapshotDarkeningPixels(BitmapSource bitmap)
    {
        BitmapSource source = bitmap.Format == PixelFormats.Pbgra32
            ? bitmap
            : new FormatConvertedBitmap(bitmap, PixelFormats.Pbgra32, null, 0);
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;
        var pixels = new byte[stride * height];
        source.CopyPixels(pixels, stride, 0);

        var darkeningPixels = new int[width * height];
        for (int i = 0; i < darkeningPixels.Length; i++)
        {
            int offset = i * 4;
            int alpha = pixels[offset + 3];
            int blue = Math.Min(255, pixels[offset] + 255 - alpha);
            int green = Math.Min(255, pixels[offset + 1] + 255 - alpha);
            int red = Math.Min(255, pixels[offset + 2] + 255 - alpha);
            int darkestChannel = Math.Min(red, Math.Min(green, blue));
            darkeningPixels[i] = 255 - darkestChannel;
        }

        return darkeningPixels;
    }

    private readonly struct RenderedShadowPixelStats
    {
        public RenderedShadowPixelStats(Int32Rect bounds, int peakDarkening, int shadowPixelCount, double centroidX, double centroidY)
        {
            Bounds = bounds;
            PeakDarkening = peakDarkening;
            ShadowPixelCount = shadowPixelCount;
            CentroidX = centroidX;
            CentroidY = centroidY;
        }

        public Int32Rect Bounds { get; }

        public int PeakDarkening { get; }

        public int ShadowPixelCount { get; }

        public double CentroidX { get; }

        public double CentroidY { get; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Bounds={0}, PeakDarkening={1}, ShadowPixelCount={2}, Centroid=({3:0.###},{4:0.###})",
                FormatShadowSnapshotBounds(Bounds),
                PeakDarkening,
                ShadowPixelCount,
                CentroidX,
                CentroidY);
        }
    }

    private readonly struct RenderedShadowSnapshotMetrics
    {
        public RenderedShadowSnapshotMetrics(
            string name,
            string kind,
            int width,
            int height,
            RenderedShadowPixelStats stats)
        {
            Name = name;
            Kind = kind;
            Width = width;
            Height = height;
            Stats = stats;
        }

        public string Name { get; }

        public string Kind { get; }

        public int Width { get; }

        public int Height { get; }

        public RenderedShadowPixelStats Stats { get; }
    }

    private readonly struct RenderedShadowSnapshotReferenceMask
    {
        public RenderedShadowSnapshotReferenceMask(
            string name,
            string kind,
            int width,
            int height,
            Int32Rect ignoredBounds)
        {
            Name = name;
            Kind = kind;
            Width = width;
            Height = height;
            IgnoredBounds = ignoredBounds;
        }

        public string Name { get; }

        public string Kind { get; }

        public int Width { get; }

        public int Height { get; }

        public Int32Rect IgnoredBounds { get; }
    }

    private sealed class TestBorderEx : BorderEx
    {
        public Geometry GetLayoutClipForTest(Size layoutSlotSize)
        {
            return base.GetLayoutClip(layoutSlotSize);
        }
    }
}
