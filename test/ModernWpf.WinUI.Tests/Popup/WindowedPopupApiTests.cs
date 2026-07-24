using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.PopupTests;

[TestClass]
public class WindowedPopupApiTests
{
    private static readonly PopupPlacementMode[] AnchoredPlacements =
    {
        PopupPlacementMode.Top,
        PopupPlacementMode.Bottom,
        PopupPlacementMode.Left,
        PopupPlacementMode.Right,
        PopupPlacementMode.TopEdgeAlignedLeft,
        PopupPlacementMode.TopEdgeAlignedRight,
        PopupPlacementMode.BottomEdgeAlignedLeft,
        PopupPlacementMode.BottomEdgeAlignedRight,
        PopupPlacementMode.LeftEdgeAlignedTop,
        PopupPlacementMode.LeftEdgeAlignedBottom,
        PopupPlacementMode.RightEdgeAlignedTop,
        PopupPlacementMode.RightEdgeAlignedBottom
    };

    [TestMethod]
    public void PopupPlacementModeMatchesWinUIOrder()
    {
#pragma warning disable MSTEST0032 // Numeric enum values are a shipped WinUI-compatibility contract.
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
#pragma warning restore MSTEST0032
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

    [TestMethod]
    public void OpenCloseAndReopenWithAbsoluteOffsetsMatchesWinUISourceBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var child = CreatePopupChild(width: 120, height: 60);
            var popup = new WindowedPopup
            {
                Child = child,
                HorizontalOffset = 26,
                VerticalOffset = 15
            };
            var root = CreateRoot(popup);
            using var host = CreateVisibleHost(root, width: 320, height: 220);

            var openedCount = 0;
            var closedCount = 0;
            popup.Opened += (_, _) => openedCount++;
            popup.Closed += (_, _) => closedCount++;

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                AssertAbsolutePopupPosition(host, child, 26, 15);
                Assert.AreEqual(PopupPlacementMode.Auto, popup.ActualPlacement);
                Assert.AreEqual(1, openedCount);
                Assert.AreEqual(0, closedCount);
                Assert.AreNotEqual(new WindowInteropHelper(host.Window).Handle, GetPopupSource(child).Handle);

                popup.IsOpen = false;
                WpfTestHost.DoEvents();

                Assert.IsFalse(popup.IsOpen);
                Assert.AreEqual(1, openedCount);
                Assert.AreEqual(1, closedCount);
                Assert.IsNull(PresentationSource.FromVisual(child));

                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                AssertAbsolutePopupPosition(host, child, 26, 15);
                Assert.AreEqual(2, openedCount);
                Assert.AreEqual(1, closedCount);
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void OffsetsRepositionOpenAbsolutePopup()
    {
        WpfTestHost.Run(() =>
        {
            var child = CreatePopupChild(width: 90, height: 42);
            var popup = new WindowedPopup
            {
                Child = child,
                HorizontalOffset = 10,
                VerticalOffset = 12
            };
            var root = CreateRoot(popup);
            using var host = CreateVisibleHost(root, width: 320, height: 220);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();
                AssertAbsolutePopupPosition(host, child, 10, 12);

                popup.HorizontalOffset = 118;
                popup.VerticalOffset = 44;
                WpfTestHost.DoEvents();

                AssertAbsolutePopupPosition(host, child, 118, 44);
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void DesiredPlacementMatrixMatchesWinUIAnchoredGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var target = new Border
            {
                Width = 80,
                Height = 48,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(128, 96, 0, 0)
            };
            var root = CreateRoot(target);
            using var host = CreateVisibleHost(root, width: 360, height: 260);

            foreach (var desiredPlacement in AnchoredPlacements)
            {
                var child = CreatePopupChild(width: 62, height: 34);
                var popup = new WindowedPopup
                {
                    Child = child,
                    PlacementTarget = target,
                    DesiredPlacement = desiredPlacement
                };
                root.Children.Add(popup);
                WpfTestHost.DoEvents();

                try
                {
                    popup.IsOpen = true;
                    WpfTestHost.DoEvents();

                    var targetBounds = GetScreenRect(target);
                    var childBounds = GetScreenRect(child);
                    var expectedTopLeft = GetExpectedAnchoredTopLeft(desiredPlacement, targetBounds, childBounds.Size);

                    Assert.AreEqual(desiredPlacement, popup.ActualPlacement, desiredPlacement.ToString());
                    AssertPoint(expectedTopLeft, childBounds.TopLeft, desiredPlacement.ToString());
                }
                finally
                {
                    popup.IsOpen = false;
                    root.Children.Remove(popup);
                    WpfTestHost.DoEvents();
                }
            }
        });
    }

    [TestMethod]
    public void AnchoredOffsetsApplyAfterWinUIPlacement()
    {
        WpfTestHost.Run(() =>
        {
            var target = CreateTarget();
            var child = CreatePopupChild(width: 90, height: 32);
            var popup = new WindowedPopup
            {
                Child = child,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft,
                HorizontalOffset = 12,
                VerticalOffset = 7
            };
            var root = CreateRoot(target, popup);
            using var host = CreateVisibleHost(root, width: 260, height: 180);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                var expectedTopLeft = target.PointToScreen(new Point(12, target.ActualHeight + 7));
                AssertPoint(expectedTopLeft, GetScreenRect(child).TopLeft, "Anchored offset");
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void PlacementTargetLayoutUpdatesMoveOpenPopup()
    {
        WpfTestHost.Run(() =>
        {
            var target = new Border
            {
                Width = 80,
                Height = 30,
                Background = Brushes.Transparent
            };
            Canvas.SetLeft(target, 72);
            Canvas.SetTop(target, 64);

            var child = CreatePopupChild(width: 84, height: 36);
            var popup = new WindowedPopup
            {
                Child = child,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = new Canvas
            {
                Width = 320,
                Height = 220,
                Background = Brushes.White
            };
            root.Children.Add(target);
            root.Children.Add(popup);
            using var host = CreateVisibleHost(root, width: 320, height: 220);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();
                AssertPoint(target.PointToScreen(new Point(0, target.ActualHeight)), GetScreenRect(child).TopLeft, "Initial target placement");

                Canvas.SetLeft(target, 144);
                Canvas.SetTop(target, 96);
                host.UpdateLayout();

                AssertPoint(target.PointToScreen(new Point(0, target.ActualHeight)), GetScreenRect(child).TopLeft, "Moved target placement");
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void PlacementTargetUnloadClosesOpenPopup()
    {
        WpfTestHost.Run(() =>
        {
            var target = CreateTarget();
            var child = CreatePopupChild(width: 90, height: 32);
            var popup = new WindowedPopup
            {
                Child = child,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = CreateRoot(target, popup);
            using var host = CreateVisibleHost(root, width: 260, height: 180);

            var closedCount = 0;
            popup.Closed += (_, _) => closedCount++;

            popup.IsOpen = true;
            WpfTestHost.DoEvents();
            Assert.IsTrue(popup.IsOpen);

            root.Children.Remove(target);
            host.UpdateLayout();

            Assert.IsFalse(popup.IsOpen);
            Assert.AreEqual(1, closedCount);
            Assert.IsNull(PresentationSource.FromVisual(child));
        });
    }

    [TestMethod]
    public void ChildSizeChangeResizesWindowWithoutMovingAnchoredContent()
    {
        WpfTestHost.Run(() =>
        {
            var target = CreateTarget();
            var child = CreatePopupChild(width: 70, height: 24);
            var popup = new WindowedPopup
            {
                Child = child,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = CreateRoot(target, popup);
            using var host = CreateVisibleHost(root, width: 260, height: 200);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                var expectedTopLeft = target.PointToScreen(new Point(0, target.ActualHeight));
                AssertPoint(expectedTopLeft, GetScreenRect(child).TopLeft, "Initial child size");

                child.Width = 132;
                child.Height = 58;
                WpfTestHost.DoEvents();
                child.UpdateLayout();
                WpfTestHost.DoEvents();

                var childBounds = GetScreenRect(child);
                var hwndBounds = GetWindowRect(GetPopupSource(child).Handle);
                AssertPoint(expectedTopLeft, childBounds.TopLeft, "Resized child top-left");
                Assert.AreEqual(childBounds.Width, hwndBounds.Width, 1.0);
                Assert.AreEqual(childBounds.Height, hwndBounds.Height, 1.0);
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void ChildReplacementWhileOpenUpdatesDisplayedContent()
    {
        WpfTestHost.Run(() =>
        {
            var target = CreateTarget();
            var firstChild = CreatePopupChild(width: 72, height: 28);
            var secondChild = CreatePopupChild(width: 96, height: 40);
            var popup = new WindowedPopup
            {
                Child = firstChild,
                PlacementTarget = target,
                DesiredPlacement = PopupPlacementMode.BottomEdgeAlignedLeft
            };
            var root = CreateRoot(target, popup);
            using var host = CreateVisibleHost(root, width: 260, height: 200);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();
                Assert.IsNotNull(PresentationSource.FromVisual(firstChild));

                popup.Child = secondChild;
                WpfTestHost.DoEvents();

                var expectedTopLeft = target.PointToScreen(new Point(0, target.ActualHeight));
                Assert.IsNull(PresentationSource.FromVisual(firstChild));
                Assert.IsNotNull(PresentationSource.FromVisual(secondChild));
                AssertPoint(expectedTopLeft, GetScreenRect(secondChild).TopLeft, "Replacement child");
            }
            finally
            {
                popup.IsOpen = false;
            }
        });
    }

    [TestMethod]
    public void PopupHwndReturnsNoActivateForMouseActivate()
    {
        WpfTestHost.Run(() =>
        {
            var child = CreatePopupChild(width: 90, height: 42);
            var popup = new WindowedPopup
            {
                Child = child,
                HorizontalOffset = 20,
                VerticalOffset = 20
            };
            var root = CreateRoot(popup);
            using var host = CreateVisibleHost(root, width: 260, height: 180);

            try
            {
                popup.IsOpen = true;
                WpfTestHost.DoEvents();

                var result = SendMessage(GetPopupSource(child).Handle, WM_MOUSEACTIVATE, IntPtr.Zero, IntPtr.Zero);
                Assert.AreEqual(new IntPtr(MA_NOACTIVATE), result);
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

    private static Border CreatePopupChild(double width, double height)
    {
        return new Border
        {
            Width = width,
            Height = height,
            Background = Brushes.Transparent
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

    private static void AssertAbsolutePopupPosition(WindowHost host, FrameworkElement child, double horizontalOffset, double verticalOffset)
    {
        var origin = GetClientAreaScreenOrigin(host.Window);
        var offset = host.Window.PointToScreen(new Point(horizontalOffset, verticalOffset));
        var windowOrigin = host.Window.PointToScreen(new Point());
        var expectedTopLeft = new Point(
            origin.X + offset.X - windowOrigin.X,
            origin.Y + offset.Y - windowOrigin.Y);

        AssertPoint(expectedTopLeft, GetScreenRect(child).TopLeft, "Absolute popup offset");
    }

    private static HwndSource GetPopupSource(Visual child)
    {
        return PresentationSource.FromVisual(child) as HwndSource
            ?? throw new AssertFailedException("Expected popup child to be hosted in an HwndSource.");
    }

    private static Rect GetScreenRect(FrameworkElement element)
    {
        element.UpdateLayout();
        var topLeft = element.PointToScreen(new Point());
        var bottomRight = element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight));
        return new Rect(topLeft, bottomRight);
    }

    private static Rect GetWindowRect(IntPtr hwnd)
    {
        var rect = new RECT();
        if (!NativeGetWindowRect(hwnd, ref rect))
        {
            throw new AssertFailedException("GetWindowRect failed for popup HWND.");
        }

        return new Rect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    private static Point GetClientAreaScreenOrigin(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var point = new POINT();
        if (!ClientToScreen(hwnd, ref point))
        {
            throw new AssertFailedException("ClientToScreen failed for test window.");
        }

        return new Point(point.X, point.Y);
    }

    private static Point GetExpectedAnchoredTopLeft(PopupPlacementMode placement, Rect targetBounds, Size childSize)
    {
        return placement switch
        {
            PopupPlacementMode.Top => new Point(
                targetBounds.Left + (targetBounds.Width - childSize.Width) / 2,
                targetBounds.Top - childSize.Height),
            PopupPlacementMode.Bottom => new Point(
                targetBounds.Left + (targetBounds.Width - childSize.Width) / 2,
                targetBounds.Bottom),
            PopupPlacementMode.Left => new Point(
                targetBounds.Left - childSize.Width,
                targetBounds.Top + (targetBounds.Height - childSize.Height) / 2),
            PopupPlacementMode.Right => new Point(
                targetBounds.Right,
                targetBounds.Top + (targetBounds.Height - childSize.Height) / 2),
            PopupPlacementMode.TopEdgeAlignedLeft => new Point(targetBounds.Left, targetBounds.Top - childSize.Height),
            PopupPlacementMode.TopEdgeAlignedRight => new Point(targetBounds.Right - childSize.Width, targetBounds.Top - childSize.Height),
            PopupPlacementMode.BottomEdgeAlignedLeft => new Point(targetBounds.Left, targetBounds.Bottom),
            PopupPlacementMode.BottomEdgeAlignedRight => new Point(targetBounds.Right - childSize.Width, targetBounds.Bottom),
            PopupPlacementMode.LeftEdgeAlignedTop => new Point(targetBounds.Left - childSize.Width, targetBounds.Top),
            PopupPlacementMode.LeftEdgeAlignedBottom => new Point(targetBounds.Left - childSize.Width, targetBounds.Bottom - childSize.Height),
            PopupPlacementMode.RightEdgeAlignedTop => new Point(targetBounds.Right, targetBounds.Top),
            PopupPlacementMode.RightEdgeAlignedBottom => new Point(targetBounds.Right, targetBounds.Bottom - childSize.Height),
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }

    private static void AssertPoint(Point expected, Point actual, string message)
    {
        Assert.AreEqual(expected.X, actual.X, 1.5, $"{message} X");
        Assert.AreEqual(expected.Y, actual.Y, 1.5, $"{message} Y");
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Window window)
        {
            Window = window;
        }

        public Window Window { get; }

        public void UpdateLayout()
        {
            Window.UpdateLayout();
            WpfTestHost.DoEvents();
        }

        public void Dispose()
        {
            Window.Content = null;
            Window.Close();
            WpfTestHost.DoEvents();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

    [DllImport("user32.dll", EntryPoint = "GetWindowRect", SetLastError = true)]
    private static extern bool NativeGetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
