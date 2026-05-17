using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;
using ModernWrapPanel = ModernWpf.Controls.WrapPanel;
using WrapPanelItemsStretch = ModernWpf.Controls.WrapPanelItemsStretch;

namespace ModernWpf.WinUI.Tests.WrapPanel;

[TestClass]
public class WrapPanelApiTests
{
    [TestMethod]
    public void VerifyPaddingLayoutOffset()
    {
        WpfTestHost.Run(() =>
        {
            var padding = new Thickness(10, 20, 30, 40);
            var panel = new ModernWrapPanel
            {
                Width = 400,
                Height = 400,
                Padding = padding,
                Orientation = Orientation.Horizontal
            };
            var button = CreateButton("Button", 100, 50);
            panel.Children.Add(button);

            using var host = new TestWindowHost(panel, width: 420, height: 420);

            AssertLayoutSlot(button, new Rect(padding.Left, padding.Top, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyHorizontalWrapLayout()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 300,
                Height = 400,
                Orientation = Orientation.Horizontal,
                ItemSpacing = 10,
                LineSpacing = 5
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            var button4 = CreateButton("Button4", 100, 50);
            AddChildren(panel, button1, button2, button3, button4);

            using var host = new TestWindowHost(panel, width: 320, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(110, 0, 100, 50));
            AssertLayoutSlot(button3, new Rect(0, 55, 100, 50));
            AssertLayoutSlot(button4, new Rect(110, 55, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyVerticalWrapLayout()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 400,
                Height = 200,
                Orientation = Orientation.Vertical,
                ItemSpacing = 5,
                LineSpacing = 10
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            var button4 = CreateButton("Button4", 100, 50);
            AddChildren(panel, button1, button2, button3, button4);

            using var host = new TestWindowHost(panel, width: 420, height: 220);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(0, 55, 100, 50));
            AssertLayoutSlot(button3, new Rect(0, 110, 100, 50));
            AssertLayoutSlot(button4, new Rect(110, 0, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyItemsStretchLast()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 300,
                Height = 400,
                Orientation = Orientation.Horizontal,
                ItemsStretch = WrapPanelItemsStretch.Last
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            AddChildren(panel, button1, button2);

            using var host = new TestWindowHost(panel, width: 320, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(100, 0, 200, 50));
        });
    }

    [TestMethod]
    public void VerifyItemsStretchNone()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 300,
                Height = 400,
                Orientation = Orientation.Horizontal,
                ItemsStretch = WrapPanelItemsStretch.None
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            AddChildren(panel, button1, button2);

            using var host = new TestWindowHost(panel, width: 320, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(100, 0, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyPaddingWithSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var padding = new Thickness(5, 10, 15, 20);
            var panel = new ModernWrapPanel
            {
                Width = 400,
                Height = 400,
                Padding = padding,
                Orientation = Orientation.Horizontal,
                ItemSpacing = 8,
                LineSpacing = 12
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            AddChildren(panel, button1, button2);

            using var host = new TestWindowHost(panel, width: 420, height: 420);

            AssertLayoutSlot(button1, new Rect(5, 10, 100, 50));
            AssertLayoutSlot(button2, new Rect(113, 10, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyCollapsedChildrenAreIgnored()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 300,
                Height = 400,
                Orientation = Orientation.Horizontal,
                ItemSpacing = 10
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            button2.Visibility = Visibility.Collapsed;
            AddChildren(panel, button1, button2, button3);

            using var host = new TestWindowHost(panel, width: 320, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button3, new Rect(110, 0, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyDynamicOrientationChange()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 250,
                Height = 400,
                Orientation = Orientation.Horizontal,
                ItemSpacing = 10,
                LineSpacing = 5
            };
            var button1 = CreateButton("1", 100, 50);
            var button2 = CreateButton("2", 100, 50);
            var button3 = CreateButton("3", 100, 50);
            AddChildren(panel, button1, button2, button3);

            using var host = new TestWindowHost(panel, width: 420, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(110, 0, 100, 50));
            AssertLayoutSlot(button3, new Rect(0, 55, 100, 50));

            panel.Orientation = Orientation.Vertical;
            panel.Width = 400;
            panel.Height = 170;
            host.UpdateLayout();

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(0, 60, 100, 50));
            AssertLayoutSlot(button3, new Rect(0, 120, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyVariableSizedChildren()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 220,
                Height = 400,
                Orientation = Orientation.Horizontal,
                ItemSpacing = 5,
                LineSpacing = 10
            };
            var button1 = CreateButton("Small", 60, 30);
            var button2 = CreateButton("Large", 120, 80);
            var button3 = CreateButton("Medium", 90, 50);
            var button4 = CreateButton("Tiny", 40, 20);
            AddChildren(panel, button1, button2, button3, button4);

            using var host = new TestWindowHost(panel, width: 240, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 60, 80));
            AssertLayoutSlot(button2, new Rect(65, 0, 120, 80));
            AssertLayoutSlot(button3, new Rect(0, 90, 90, 50));
            AssertLayoutSlot(button4, new Rect(95, 90, 40, 50));
        });
    }

    [TestMethod]
    public void VerifyCollapsedChildFirst()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateHorizontalPanel(400, 200);
            var button1 = CreateButton("Button1", 150, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 125, 50);
            var button4 = CreateButton("Button4", 50, 50);
            button1.Visibility = Visibility.Collapsed;
            AddChildren(panel, button1, button2, button3, button4);

            using var host = new TestWindowHost(panel, width: 420, height: 220);

            AssertLayoutSlot(button2, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button3, new Rect(100, 0, 125, 50));
            AssertLayoutSlot(button4, new Rect(225, 0, 50, 50));
        });
    }

    [TestMethod]
    public void VerifyCollapsedChildMiddle()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateHorizontalPanel(400, 200);
            var button1 = CreateButton("Button1", 150, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 125, 50);
            var button4 = CreateButton("Button4", 50, 50);
            button2.Visibility = Visibility.Collapsed;
            AddChildren(panel, button1, button2, button3, button4);

            using var host = new TestWindowHost(panel, width: 420, height: 220);

            AssertLayoutSlot(button1, new Rect(0, 0, 150, 50));
            AssertLayoutSlot(button3, new Rect(150, 0, 125, 50));
            AssertLayoutSlot(button4, new Rect(275, 0, 50, 50));
        });
    }

    [TestMethod]
    public void VerifyCollapsedChildLast()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateHorizontalPanel(400, 200);
            var button1 = CreateButton("Button1", 150, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 125, 50);
            var button4 = CreateButton("Button4", 50, 50);
            button4.Visibility = Visibility.Collapsed;
            AddChildren(panel, button1, button2, button3, button4);

            using var host = new TestWindowHost(panel, width: 420, height: 220);

            AssertLayoutSlot(button1, new Rect(0, 0, 150, 50));
            AssertLayoutSlot(button2, new Rect(150, 0, 100, 50));
            AssertLayoutSlot(button3, new Rect(250, 0, 125, 50));
        });
    }

    [TestMethod]
    public void VerifyHorizontalLayoutWithUniformSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateHorizontalPanel(400, 200);
            panel.ItemSpacing = 10;
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            AddChildren(panel, button1, button2, button3);

            using var host = new TestWindowHost(panel, width: 420, height: 220);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(110, 0, 100, 50));
            AssertLayoutSlot(button3, new Rect(220, 0, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyVerticalLayoutWithUniformSpacing()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 200,
                Height = 400,
                Orientation = Orientation.Vertical,
                ItemSpacing = 10
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            AddChildren(panel, button1, button2, button3);

            using var host = new TestWindowHost(panel, width: 220, height: 420);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(0, 60, 100, 50));
            AssertLayoutSlot(button3, new Rect(0, 120, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyHorizontalLayoutWithSpacingAndPadding()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateHorizontalPanel(400, 200);
            panel.ItemSpacing = 10;
            panel.Padding = new Thickness(20);
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            AddChildren(panel, button1, button2, button3);

            using var host = new TestWindowHost(panel, width: 420, height: 220);

            AssertLayoutSlot(button1, new Rect(20, 20, 100, 50));
            AssertLayoutSlot(button2, new Rect(130, 20, 100, 50));
            AssertLayoutSlot(button3, new Rect(240, 20, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyVerticalLayoutWithSpacingAndPadding()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 200,
                Height = 400,
                Orientation = Orientation.Vertical,
                ItemSpacing = 10,
                Padding = new Thickness(20)
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            var button3 = CreateButton("Button3", 100, 50);
            AddChildren(panel, button1, button2, button3);

            using var host = new TestWindowHost(panel, width: 220, height: 420);

            AssertLayoutSlot(button1, new Rect(20, 20, 100, 50));
            AssertLayoutSlot(button2, new Rect(20, 80, 100, 50));
            AssertLayoutSlot(button3, new Rect(20, 140, 100, 50));
        });
    }

    [TestMethod]
    public void VerifyWrappingBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var panel = CreateHorizontalPanel(250, 200);
            var button1 = CreateButton("Button1", 150, 50);
            var button2 = CreateButton("Button2", 150, 50);
            AddChildren(panel, button1, button2);

            using var host = new TestWindowHost(panel, width: 270, height: 220);

            AssertLayoutSlot(button1, new Rect(0, 0, 150, 50));
            AssertLayoutSlot(button2, new Rect(0, 50, 150, 50));
        });
    }

    [TestMethod]
    public void VerifyVerticalWrappingBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Width = 200,
                Height = 120,
                Orientation = Orientation.Vertical
            };
            var button1 = CreateButton("Button1", 100, 80);
            var button2 = CreateButton("Button2", 100, 80);
            AddChildren(panel, button1, button2);

            using var host = new TestWindowHost(panel, width: 220, height: 140);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 80));
            AssertLayoutSlot(button2, new Rect(100, 0, 100, 80));
        });
    }

    [TestMethod]
    public void ArrangeKeepsMeasuredRowsWhenFinalSizeExpands()
    {
        WpfTestHost.Run(() =>
        {
            var panel = new ModernWrapPanel
            {
                Orientation = Orientation.Horizontal
            };
            var button1 = CreateButton("Button1", 100, 50);
            var button2 = CreateButton("Button2", 100, 50);
            AddChildren(panel, button1, button2);

            var hostPanel = new MeasureConstrainedArrangeExpandedPanel
            {
                MeasureSize = new Size(150, 200),
                ArrangeSize = new Size(300, 200)
            };
            hostPanel.Children.Add(panel);

            using var host = new TestWindowHost(hostPanel, width: 320, height: 220);

            AssertLayoutSlot(button1, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(button2, new Rect(0, 50, 100, 50));
        });
    }

    private static ModernWrapPanel CreateHorizontalPanel(double width, double height)
    {
        return new ModernWrapPanel
        {
            Width = width,
            Height = height,
            Orientation = Orientation.Horizontal
        };
    }

    private static Button CreateButton(string content, double width, double height)
    {
        return new Button
        {
            Content = content,
            Width = width,
            Height = height
        };
    }

    private static void AddChildren(ModernWrapPanel panel, params UIElement[] children)
    {
        foreach (var child in children)
        {
            panel.Children.Add(child);
        }
    }

    private static void AssertLayoutSlot(FrameworkElement element, Rect expected)
    {
        Assert.AreEqual(expected, LayoutInformation.GetLayoutSlot(element));
    }

    private sealed class MeasureConstrainedArrangeExpandedPanel : Panel
    {
        public Size MeasureSize { get; set; }

        public Size ArrangeSize { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(MeasureSize);
            }

            return ArrangeSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(new Rect(ArrangeSize));
            }

            return finalSize;
        }
    }
}
