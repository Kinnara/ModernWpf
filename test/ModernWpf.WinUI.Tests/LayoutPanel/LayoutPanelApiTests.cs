using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;
using LayoutPanel = ModernWpf.Controls.LayoutPanel;
using NonVirtualizingLayout = ModernWpf.Controls.NonVirtualizingLayout;
using NonVirtualizingLayoutContext = ModernWpf.Controls.NonVirtualizingLayoutContext;
using StackLayout = ModernWpf.Controls.StackLayout;
using UniformGridLayout = ModernWpf.Controls.UniformGridLayout;

namespace ModernWpf.WinUI.Tests.LayoutPanels;

[TestClass]
public class LayoutPanelApiTests
{
    [TestMethod]
    public void VerifyPaddingAndBorderThicknessLayoutOffset()
    {
        WpfTestHost.Run(() =>
        {
            const double width = 400;
            const double height = 400;
            var borderThickness = new Thickness(5, 10, 15, 20);
            var padding = new Thickness(2, 4, 6, 8);

            var panel = new LayoutPanel
            {
                Width = width,
                Height = height,
                BorderBrush = Brushes.Red,
                BorderThickness = borderThickness,
                Padding = padding
            };

            var button = new Button
            {
                Content = "Button",
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            var expectedButtonLayoutSlot = new Rect
            {
                Width = width - borderThickness.Left - borderThickness.Right - padding.Left - padding.Right,
                Height = height - borderThickness.Top - borderThickness.Bottom - padding.Top - padding.Bottom,
                X = borderThickness.Left + padding.Left,
                Y = borderThickness.Top + padding.Top,
            };
            panel.Children.Add(button);

            using var host = new TestWindowHost(panel);

            Assert.AreEqual(expectedButtonLayoutSlot, LayoutInformation.GetLayoutSlot(button));
        });
    }

    [TestMethod]
    public void VerifyPaddingAndBorderThicknessLayoutOffsetStackLayout()
    {
        WpfTestHost.Run(() =>
        {
            const double width = 400;
            const double height = 400;
            var borderThickness = new Thickness(5, 10, 15, 20);
            var padding = new Thickness(2, 4, 6, 8);

            var panel = new LayoutPanel
            {
                Layout = new StackLayout(),
                Width = width,
                Height = height,
                BorderBrush = Brushes.Red,
                BorderThickness = borderThickness,
                Padding = padding
            };

            var unpaddedWidth = width - borderThickness.Left - borderThickness.Right - padding.Left - padding.Right;
            const double itemHeight = 50;
            var unpaddedX = borderThickness.Left + padding.Left;
            var unpaddedY = borderThickness.Top + padding.Top;

            var button1 = new Button
            {
                Content = "Button",
                Height = itemHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            var button2 = new Button
            {
                Content = "Button",
                Height = itemHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var expectedButton1LayoutSlot = new Rect
            {
                Width = unpaddedWidth,
                Height = itemHeight,
                X = unpaddedX,
                Y = unpaddedY,
            };
            var expectedButton2LayoutSlot = new Rect
            {
                Width = unpaddedWidth,
                Height = itemHeight,
                X = unpaddedX,
                Y = unpaddedY + itemHeight,
            };
            panel.Children.Add(button1);
            panel.Children.Add(button2);

            using var host = new TestWindowHost(panel);

            Assert.AreEqual(expectedButton1LayoutSlot, LayoutInformation.GetLayoutSlot(button1));
            Assert.AreEqual(expectedButton2LayoutSlot, LayoutInformation.GetLayoutSlot(button2));
        });
    }

    [TestMethod]
    public void VerifySwitchingLayoutDynamically()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new LayoutPanel { Width = 400, Height = 400 };
            panel.Layout = new StackLayout
            {
                Orientation = Orientation.Vertical
            };

            var button1 = new Button { Height = 100, Content = "1" };
            var button2 = new Button { Height = 100, Content = "2" };
            panel.Children.Add(button1);
            panel.Children.Add(button2);

            using var host = new TestWindowHost(panel);

            Assert.AreEqual(new Rect(0, 0, 400, 100), LayoutInformation.GetLayoutSlot(button1));
            Assert.AreEqual(new Rect(0, 100, 400, 100), LayoutInformation.GetLayoutSlot(button2));

            panel.Layout = new UniformGridLayout
            {
                MinItemWidth = 100,
                MinItemHeight = 100
            };
            host.UpdateLayout();

            Assert.AreEqual(new Rect(0, 0, 100, 100), LayoutInformation.GetLayoutSlot(button1));
            Assert.AreEqual(new Rect(100, 0, 100, 100), LayoutInformation.GetLayoutSlot(button2));
        });
    }

    [TestMethod]
    public void VerifyCustomNonVirtualizingLayout()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new LayoutPanel
            {
                Width = 400,
                Height = 400,
                Layout = new CustomNonVirtualizingStackLayout()
            };

            var button1 = new Button { Height = 100, Width = 400, Content = "1" };
            var button2 = new Button { Height = 100, Width = 400, Content = "2" };
            panel.Children.Add(button1);
            panel.Children.Add(button2);

            using var host = new TestWindowHost(panel);

            Assert.AreEqual(new Rect(0, 0, 400, 100), LayoutInformation.GetLayoutSlot(button1));
            Assert.AreEqual(new Rect(0, 100, 400, 100), LayoutInformation.GetLayoutSlot(button2));
        });
    }

    [TestMethod]
    public void VerifyBorderChromeRenders()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new LayoutPanel
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                BorderBrush = Brushes.Blue,
                BorderThickness = new Thickness(6)
            };

            var borderPixel = RenderElementPixel(panel, 3, 15, 30, 30);
            var backgroundPixel = RenderElementPixel(panel, 15, 15, 30, 30);

            Assert.IsTrue(borderPixel.B > 200 && borderPixel.A > 200, $"Expected LayoutPanel border chrome to render. Pixel={borderPixel}");
            Assert.IsTrue(backgroundPixel.R > 200 && backgroundPixel.A > 200, $"Expected LayoutPanel background chrome to render inside the border. Pixel={backgroundPixel}");
        });
    }

    [TestMethod]
    public void VerifyRoundedCornerClipAndHitTest()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new TestLayoutPanel
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red,
                CornerRadius = new CornerRadius(12, 0, 0, 0)
            };
            panel.Children.Add(new Border
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Red
            });

            panel.Measure(new Size(30, 30));
            panel.Arrange(new Rect(0, 0, 30, 30));
            panel.UpdateLayout();

            var clip = panel.GetLayoutClipForTest(new Size(30, 30));
            Assert.IsNotNull(clip);
            Assert.IsFalse(clip.FillContains(new Point(1, 1)), "Top-left corner should be clipped by LayoutPanel.CornerRadius.");
            Assert.IsTrue(clip.FillContains(new Point(15, 15)), "Center should remain inside the LayoutPanel clip.");

            Assert.IsNull(VisualTreeHelper.HitTest(panel, new Point(1, 1)), "Top-left point should be clipped by the rounded chrome.");
            Assert.IsNotNull(VisualTreeHelper.HitTest(panel, new Point(15, 15)), "Center point should hit inside the rounded chrome.");

            var clippedCorner = RenderElementPixel(panel, 1, 1, 30, 30);
            var center = RenderElementPixel(panel, 15, 15, 30, 30);
            Assert.IsTrue(clippedCorner.A < 30, $"Expected child content to be clipped out of the rounded corner. Pixel={clippedCorner}");
            Assert.IsTrue(center.R > 200 && center.A > 200, $"Expected child content to render inside the rounded clip. Pixel={center}");
        });
    }

    [TestMethod]
    public void VerifyTemplateCompatibilityXamlParsesChromeProperties()
    {
        WpfTestHost.Run(() =>
        {
            const string xaml =
                """
                <controls:LayoutPanel
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:controls="http://schemas.modernwpf.com/2019"
                    BorderThickness="1"
                    Padding="2"
                    CornerRadius="3">
                    <Button Content="Parsed" />
                </controls:LayoutPanel>
                """;

            var panel = (LayoutPanel)XamlReader.Parse(xaml);

            Assert.AreEqual(new Thickness(1), panel.BorderThickness);
            Assert.AreEqual(new Thickness(2), panel.Padding);
            Assert.AreEqual(new CornerRadius(3), panel.CornerRadius);
            Assert.AreEqual(1, panel.Children.Count);
        });
    }

    [TestMethod]
    public void LayoutChangeRevokesSourceInvalidationHandlers()
    {
        WpfTestHost.Run(() =>
        {
            var oldLayout = new InvalidatingLayout();
            var newLayout = new InvalidatingLayout();
            var panel = new CountingLayoutPanel
            {
                Width = 100,
                Height = 100,
                Layout = oldLayout
            };

            using var host = new TestWindowHost(panel, width: 100, height: 100);

            panel.Layout = newLayout;
            host.UpdateLayout();

            panel.MeasureOverrideCount = 0;
            panel.ArrangeOverrideCount = 0;

            oldLayout.RaiseMeasureInvalidated();
            oldLayout.RaiseArrangeInvalidated();
            host.UpdateLayout();

            Assert.AreEqual(0, panel.MeasureOverrideCount, "The old layout should be unhooked from LayoutPanel measure invalidation.");
            Assert.AreEqual(0, panel.ArrangeOverrideCount, "The old layout should be unhooked from LayoutPanel arrange invalidation.");

            newLayout.RaiseMeasureInvalidated();
            host.UpdateLayout();

            Assert.IsTrue(panel.MeasureOverrideCount > 0, "The new layout should invalidate LayoutPanel measure.");

            panel.MeasureOverrideCount = 0;
            panel.ArrangeOverrideCount = 0;

            newLayout.RaiseArrangeInvalidated();
            host.UpdateLayout();

            Assert.AreEqual(0, panel.MeasureOverrideCount);
            Assert.IsTrue(panel.ArrangeOverrideCount > 0, "The new layout should invalidate LayoutPanel arrange.");
        });
    }

    private sealed class CustomNonVirtualizingStackLayout : NonVirtualizingLayout
    {
        protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            var extentHeight = 0.0;
            var extentWidth = 0.0;
            foreach (var element in context.Children)
            {
                element.Measure(availableSize);
                extentHeight += element.DesiredSize.Height;
                extentWidth = Math.Max(extentWidth, element.DesiredSize.Width);
            }

            return new Size(extentWidth, extentHeight);
        }

        protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            var offset = 0.0;
            foreach (var element in context.Children)
            {
                element.Arrange(new Rect(0, offset, element.DesiredSize.Width, element.DesiredSize.Height));
                offset += element.DesiredSize.Height;
            }

            return finalSize;
        }
    }

    private sealed class InvalidatingLayout : NonVirtualizingLayout
    {
        public void RaiseMeasureInvalidated()
        {
            InvalidateMeasure();
        }

        public void RaiseArrangeInvalidated()
        {
            InvalidateArrange();
        }

        protected override Size MeasureOverride(NonVirtualizingLayoutContext context, Size availableSize)
        {
            return availableSize;
        }

        protected override Size ArrangeOverride(NonVirtualizingLayoutContext context, Size finalSize)
        {
            return finalSize;
        }
    }

    private static Color RenderElementPixel(FrameworkElement element, int x, int y, int width, int height)
    {
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(0, 0, width, height));
        element.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        var pixels = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
        return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
    }

    private sealed class TestLayoutPanel : LayoutPanel
    {
        public Geometry GetLayoutClipForTest(Size layoutSlotSize)
        {
            return base.GetLayoutClip(layoutSlotSize);
        }
    }

    private sealed class CountingLayoutPanel : LayoutPanel
    {
        public int MeasureOverrideCount { get; set; }

        public int ArrangeOverrideCount { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureOverrideCount++;
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangeOverrideCount++;
            return base.ArrangeOverride(finalSize);
        }
    }
}
