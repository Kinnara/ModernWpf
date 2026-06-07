using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.PopupTests;

[TestClass]
public class WindowedPopupApiTests
{
    [TestMethod]
    public void PopupPlacementModeMatchesWinUIOrder()
    {
        Assert.AreEqual(0, (int)PopupPlacementMode.Auto);
        Assert.AreEqual(1, (int)PopupPlacementMode.Top);
        Assert.AreEqual(2, (int)PopupPlacementMode.Bottom);
        Assert.AreEqual(3, (int)PopupPlacementMode.Left);
        Assert.AreEqual(4, (int)PopupPlacementMode.Right);
        Assert.AreEqual(5, (int)PopupPlacementMode.TopEdgeAlignedLeft);
        Assert.AreEqual(6, (int)PopupPlacementMode.TopEdgeAlignedRight);
        Assert.AreEqual(7, (int)PopupPlacementMode.BottomEdgeAlignedLeft);
        Assert.AreEqual(8, (int)PopupPlacementMode.BottomEdgeAlignedRight);
        Assert.AreEqual(9, (int)PopupPlacementMode.LeftEdgeAlignedTop);
        Assert.AreEqual(10, (int)PopupPlacementMode.LeftEdgeAlignedBottom);
        Assert.AreEqual(11, (int)PopupPlacementMode.RightEdgeAlignedTop);
        Assert.AreEqual(12, (int)PopupPlacementMode.RightEdgeAlignedBottom);
    }

    [TestMethod]
    public void DesiredPlacementPlacesContentAgainstTarget()
    {
        WpfTestHost.Run(() =>
        {
            var target = CreateTarget();
            var child = new Border
            {
                Width = 90,
                Height = 32,
                Background = Brushes.Transparent
            };
            var popup = new WindowedPopup
            {
                Child = child,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = CreateRoot(target, popup);
            using var host = CreateVisibleHost(root, width: 240, height: 180);

            try
            {
                var actualPlacements = new List<PopupPlacementMode>();
                popup.ActualPlacementChanged += (_, _) => actualPlacements.Add(popup.ActualPlacement);

                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                var expectedContentTopLeft = target.PointToScreen(new Point(0, target.ActualHeight));
                var actualContentTopLeft = child.PointToScreen(new Point());

                Assert.AreEqual(PopupPlacementMode.BottomEdgeAlignedLeft, popup.ActualPlacement);
                Assert.IsTrue(actualPlacements.Contains(PopupPlacementMode.BottomEdgeAlignedLeft));
                Assert.AreEqual(expectedContentTopLeft.X, actualContentTopLeft.X, 1.0);
                Assert.AreEqual(expectedContentTopLeft.Y, actualContentTopLeft.Y, 1.0);
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void ReservedShadowContentBoundsDoNotShiftPlacement()
    {
        WpfTestHost.Run(() =>
        {
            var target = CreateTarget();
            var child = new Border
            {
                Width = 90,
                Height = 32,
                Background = Brushes.Transparent
            };
            var chrome = new ThemeShadowChrome
            {
                Depth = 32,
                WindowedPopupInsetMode = ThemeShadowChromeWindowedPopupInsetMode.Medium,
                ReservesShadowSpace = true,
                Child = child
            };
            var popup = new WindowedPopup
            {
                Child = chrome,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = CreateRoot(target, popup);
            using var host = CreateVisibleHost(root, width: 240, height: 180);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();
                chrome.UpdateLayout();

                var expectedContentTopLeft = target.PointToScreen(new Point(0, target.ActualHeight));
                var actualContentTopLeft = child.PointToScreen(new Point());

                Assert.AreEqual(new Point(10, 2), child.TranslatePoint(new Point(), chrome));
                Assert.AreEqual(PopupPlacementMode.BottomEdgeAlignedLeft, popup.ActualPlacement);
                Assert.AreEqual(expectedContentTopLeft.X, actualContentTopLeft.X, 1.0);
                Assert.AreEqual(expectedContentTopLeft.Y, actualContentTopLeft.Y, 1.0);
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void ActualPlacementFlipsBeforeShowWhenOutOfBounds()
    {
        WpfTestHost.Run(() =>
        {
            var target = new Border
            {
                Width = 80,
                Height = 24,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(24, 54, 0, 0)
            };
            var child = new Border
            {
                Width = 90,
                Height = 120,
                Background = Brushes.Transparent
            };
            var popup = new WindowedPopup
            {
                Child = child,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = CreateRoot(target, popup);
            var workArea = SystemParameters.WorkArea;
            using var host = CreateVisibleHost(
                root,
                width: 220,
                height: 90,
                left: workArea.Left + 24,
                top: Math.Max(workArea.Top, workArea.Bottom - 90));

            try
            {
                var actualPlacements = new List<PopupPlacementMode>();
                popup.ActualPlacementChanged += (_, _) => actualPlacements.Add(popup.ActualPlacement);

                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                Assert.AreEqual(PopupPlacementMode.TopEdgeAlignedLeft, popup.ActualPlacement);
                Assert.IsTrue(actualPlacements.Contains(PopupPlacementMode.TopEdgeAlignedLeft));
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    private static Border CreateTarget()
    {
        return new Border
        {
            Width = 80,
            Height = 24,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(32)
        };
    }

    private static Grid CreateRoot(params UIElement[] children)
    {
        var root = new Grid
        {
            Width = 240,
            Height = 180,
            Background = Brushes.White
        };

        foreach (var child in children)
        {
            root.Children.Add(child);
        }

        return root;
    }

    private static WindowHost CreateVisibleHost(FrameworkElement content, double width, double height, double? left = null, double? top = null)
    {
        var workArea = SystemParameters.WorkArea;
        var window = new Window
        {
            Width = width,
            Height = height,
            Left = left ?? workArea.Left + 40,
            Top = top ?? workArea.Top + 40,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            Content = content
        };

        window.Show();
        WpfTestHost.DoEvents();
        window.UpdateLayout();
        WpfTestHost.DoEvents();

        return new WindowHost(window);
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Window window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
            WpfTestHost.DoEvents();
        }

        private readonly Window _window;
    }
}
