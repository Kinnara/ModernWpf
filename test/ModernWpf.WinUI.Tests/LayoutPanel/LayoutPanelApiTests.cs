using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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
}
