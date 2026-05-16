using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class RepeaterLayoutTests
{
    [TestMethod]
    public void ValidateNonVirtualLayoutWithItemsRepeater()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                Layout = new NonVirtualStackLayout(),
                ItemsSource = Enumerable.Range(0, 5),
                ItemTemplate = CreateButtonTemplate(height: 100)
            };

            using var host = new TestWindowHost(repeater, width: 240, height: 620);

            var expectedYOffset = 0.0;
            for (var i = 0; i < repeater.ItemsSourceView.Count; i++)
            {
                var child = repeater.TryGetElement(i) as Button;
                Assert.IsNotNull(child);
                var realizedChild = child!;
                Assert.AreEqual(i, realizedChild.Content);
                AssertLayoutSlot(realizedChild, new Rect(0, expectedYOffset, realizedChild.DesiredSize.Width, 100));
                expectedYOffset += 100;
            }
        });
    }

    [TestMethod]
    public void ValidateStackLayoutDoesNotRetainIncorrectMinorWidth()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                ItemsSource = Enumerable.Range(0, 1)
            };
            var scrollViewer = new ScrollViewer
            {
                Width = 400,
                Height = 100,
                Content = repeater
            };

            using var host = new TestWindowHost(scrollViewer, width: 460, height: 180);

            repeater.Measure(new Size(600, 100));
            Assert.AreEqual(600, repeater.DesiredSize.Width);

            repeater.Measure(new Size(300, 100));
            Assert.AreEqual(300, repeater.DesiredSize.Width);

            host.UpdateLayout();
            Assert.AreEqual(400, repeater.ActualWidth);
        });
    }

    [TestMethod]
    public void ValidateStackLayoutDisabledVirtualizationWithItemsRepeater()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                Layout = new StackLayout { IsVirtualizationEnabled = false },
                ItemsSource = Enumerable.Range(0, 10),
                ItemTemplate = CreateButtonTemplate(height: 100)
            };
            var scrollViewer = new ScrollViewer
            {
                Width = 220,
                Height = 100,
                Content = repeater
            };

            using var host = new TestWindowHost(scrollViewer, width: 280, height: 180);

            for (var i = 0; i < repeater.ItemsSourceView.Count; i++)
            {
                Assert.IsNotNull(repeater.TryGetElement(i), $"Expected item {i} to remain realized.");
            }
        });
    }

    [TestMethod]
    public void StackLayoutUsesWinUIVirtualizationPropertySurface()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new StackLayout();

            Assert.IsTrue(layout.IsVirtualizationEnabled);
            Assert.IsFalse(layout.DisableVirtualization);

            layout.IsVirtualizationEnabled = false;

            Assert.IsFalse(layout.IsVirtualizationEnabled);
            Assert.IsTrue(layout.DisableVirtualization);

            layout.DisableVirtualization = false;

            Assert.IsTrue(layout.IsVirtualizationEnabled);
            Assert.IsFalse(layout.DisableVirtualization);

            layout.SetValue(StackLayout.DisableVirtualizationProperty, true);

            Assert.IsFalse(layout.IsVirtualizationEnabled);
            Assert.IsTrue(layout.DisableVirtualization);
        });
    }

    [TestMethod]
    public void LayoutsUseWinUIIndexBasedOrientationSurface()
    {
        WpfTestHost.Run(() =>
        {
            Assert.AreEqual(IndexBasedLayoutOrientation.None, new Layout().IndexBasedLayoutOrientation);

            var stackLayout = new StackLayout();
            Assert.AreEqual(IndexBasedLayoutOrientation.TopToBottom, stackLayout.IndexBasedLayoutOrientation);

            stackLayout.Orientation = Orientation.Horizontal;
            Assert.AreEqual(IndexBasedLayoutOrientation.LeftToRight, stackLayout.IndexBasedLayoutOrientation);

            stackLayout.Orientation = Orientation.Vertical;
            Assert.AreEqual(IndexBasedLayoutOrientation.TopToBottom, stackLayout.IndexBasedLayoutOrientation);

            var flowLayout = new FlowLayout();
            Assert.AreEqual(IndexBasedLayoutOrientation.LeftToRight, flowLayout.IndexBasedLayoutOrientation);

            flowLayout.Orientation = Orientation.Vertical;
            Assert.AreEqual(IndexBasedLayoutOrientation.TopToBottom, flowLayout.IndexBasedLayoutOrientation);

            var uniformGridLayout = new UniformGridLayout();
            Assert.AreEqual(IndexBasedLayoutOrientation.LeftToRight, uniformGridLayout.IndexBasedLayoutOrientation);

            uniformGridLayout.Orientation = Orientation.Vertical;
            Assert.AreEqual(IndexBasedLayoutOrientation.TopToBottom, uniformGridLayout.IndexBasedLayoutOrientation);
        });
    }

    [TestMethod]
    public void StackLayoutKeepsFractionalAverageForRegularElementsLikeWinUI()
    {
        WpfTestHost.Run(() =>
        {
            var stackLayout = new StackLayout();
            var stackState = new StackLayoutState();
            var context = new TestVirtualizingLayoutContext(itemCount: 3);
            stackState.OnElementSizesReset();

            stackState.OnElementMeasured(0, 10.5, 20);
            stackState.OnElementMeasured(1, 10.5, 20);

            Assert.IsTrue(stackState.AreElementsMeasuredRegular);
            Assert.AreEqual(10.5, GetAverageElementSize(stackLayout, context, stackState));

            stackState.OnElementMeasured(2, 12.5, 20);

            Assert.IsFalse(stackState.AreElementsMeasuredRegular);
            Assert.AreEqual(11.0, GetAverageElementSize(stackLayout, context, stackState));

            stackState.OnElementSizesReset();
            Assert.IsTrue(stackState.AreElementsMeasuredRegular);
            Assert.AreEqual(0, stackState.TotalElementsMeasured);
            Assert.AreEqual(0.0, stackState.TotalElementSize);
        });
    }

    [TestMethod]
    public void ValidateUniformGridLayoutWithItemsRepeater()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                Layout = new UniformGridLayout
                {
                    MinItemWidth = 100,
                    MinItemHeight = 50,
                    MinColumnSpacing = 10,
                    MinRowSpacing = 5
                },
                ItemsSource = Enumerable.Range(0, 4),
                ItemTemplate = CreateButtonTemplate()
            };
            var scrollViewer = new ScrollViewer
            {
                Width = 250,
                Height = 140,
                Content = repeater
            };

            using var host = new TestWindowHost(scrollViewer, width: 310, height: 220);

            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(0)!, new Rect(0, 0, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(1)!, new Rect(110, 0, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(2)!, new Rect(0, 55, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(3)!, new Rect(110, 55, 100, 50));
        });
    }

    [TestMethod]
    public void ValidateFlowLayoutWrapsItemsRepeaterChildren()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                Layout = new FlowLayout
                {
                    MinColumnSpacing = 10,
                    MinRowSpacing = 5
                },
                ItemsSource = Enumerable.Range(0, 4),
                ItemTemplate = CreateButtonTemplate(width: 100, height: 50)
            };
            var scrollViewer = new ScrollViewer
            {
                Width = 250,
                Height = 140,
                Content = repeater
            };

            using var host = new TestWindowHost(scrollViewer, width: 310, height: 220);

            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(0)!, new Rect(0, 0, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(1)!, new Rect(110, 0, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(2)!, new Rect(0, 55, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(3)!, new Rect(110, 55, 100, 50));
        });
    }

    [TestMethod]
    public void FlowLayoutUsesWinUISpacingPropertySurface()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new FlowLayout();

            Assert.AreEqual(Orientation.Horizontal, layout.Orientation);
            Assert.AreEqual(0.0, layout.LineSpacing);
            Assert.AreEqual(0.0, layout.MinItemSpacing);
            Assert.AreEqual(FlowLayoutLineAlignment.Start, layout.LineAlignment);
            Assert.IsNotNull(FlowLayout.LineSpacingProperty);
            Assert.IsNotNull(FlowLayout.MinItemSpacingProperty);
            Assert.IsNotNull(FlowLayout.LineAlignmentProperty);

            var parsed = (FlowLayout)XamlReader.Parse(
                "<controls:FlowLayout " +
                "xmlns:controls=\"clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls\" " +
                "MinItemSpacing=\"10\" LineSpacing=\"5\" LineAlignment=\"End\" />");

            Assert.AreEqual(10.0, parsed.MinItemSpacing);
            Assert.AreEqual(5.0, parsed.LineSpacing);
            Assert.AreEqual(FlowLayoutLineAlignment.End, parsed.LineAlignment);

            var repeater = new ItemsRepeater
            {
                Layout = new FlowLayout
                {
                    MinColumnSpacing = 40,
                    MinRowSpacing = 40,
                    MinItemSpacing = 10,
                    LineSpacing = 5
                },
                ItemsSource = Enumerable.Range(0, 4),
                ItemTemplate = CreateButtonTemplate(width: 100, height: 50)
            };
            var scrollViewer = new ScrollViewer
            {
                Width = 250,
                Height = 140,
                Content = repeater
            };

            using var host = new TestWindowHost(scrollViewer, width: 310, height: 220);

            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(0)!, new Rect(0, 0, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(1)!, new Rect(110, 0, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(2)!, new Rect(0, 55, 100, 50));
            AssertLayoutSlot((FrameworkElement)repeater.TryGetElement(3)!, new Rect(110, 55, 100, 50));
        });
    }

    [TestMethod]
    public void ItemsRepeaterScrollHostUsesVerticalAnchorRatioForAnchorCandidates()
    {
        WpfTestHost.Run(() =>
        {
            var first = new Border
            {
                Width = 100,
                Height = 60
            };
            var second = new Border
            {
                Width = 100,
                Height = 20
            };
            var content = new StackPanel();
            content.Children.Add(first);
            content.Children.Add(second);

            var scrollViewer = new ScrollViewer
            {
                Width = 100,
                Height = 50,
                Content = content
            };
            var scrollHost = new ItemsRepeaterScrollHost
            {
                HorizontalAnchorRatio = 0,
                VerticalAnchorRatio = 1,
                ScrollViewer = scrollViewer
            };

            using var host = new TestWindowHost(scrollHost, width: 160, height: 120);

            scrollViewer.ScrollToVerticalOffset(25);
            host.UpdateLayout();

            var scrollingSurface = (IRepeaterScrollingSurface)scrollHost;
            scrollingSurface.RegisterAnchorCandidate(first);
            scrollingSurface.RegisterAnchorCandidate(second);

            Assert.AreSame(second, scrollHost.CurrentAnchor);
        });
    }

    [TestMethod]
    public void ItemsRepeaterScrollHostStartsWithoutPendingBringIntoView()
    {
        WpfTestHost.Run(() =>
        {
            var scrollHost = new ItemsRepeaterScrollHost
            {
                ScrollViewer = new ScrollViewer
                {
                    Content = new Border
                    {
                        Width = 100,
                        Height = 100
                    }
                }
            };

            Assert.IsNull(GetPendingBringIntoViewTarget(scrollHost));
        });
    }

    private static double GetAverageElementSize(
        StackLayout stackLayout,
        VirtualizingLayoutContext context,
        StackLayoutState stackState)
    {
        var method = typeof(StackLayout).GetMethod(
            "GetAverageElementSize",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        return (double)method!.Invoke(
            stackLayout,
            new object[] { new Size(double.PositiveInfinity, double.PositiveInfinity), context, stackState })!;
    }

    private static DataTemplate CreateButtonTemplate(double? width = null, double? height = null)
    {
        var widthAttribute = width.HasValue ? $" Width='{width.Value}'" : string.Empty;
        var heightAttribute = height.HasValue ? $" Height='{height.Value}'" : string.Empty;

        return (DataTemplate)XamlReader.Parse(
            $@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                  <Button Content='{{Binding}}'{widthAttribute}{heightAttribute} />
              </DataTemplate>");
    }

    private static void AssertLayoutSlot(FrameworkElement element, Rect expected)
    {
        Assert.IsNotNull(element);
        Assert.AreEqual(expected, LayoutInformation.GetLayoutSlot(element));
    }

    private static UIElement? GetPendingBringIntoViewTarget(ItemsRepeaterScrollHost scrollHost)
    {
        var stateField = typeof(ItemsRepeaterScrollHost).GetField(
            "m_pendingBringIntoView",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(stateField);

        var state = stateField!.GetValue(scrollHost);
        Assert.IsNotNull(state);

        var targetProperty = state!.GetType().GetProperty(
            "TargetElement",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(targetProperty);

        return (UIElement?)targetProperty!.GetValue(state);
    }

    private sealed class TestVirtualizingLayoutContext : VirtualizingLayoutContext
    {
        public TestVirtualizingLayoutContext(int itemCount)
        {
            m_itemCount = itemCount;
        }

        protected override int ItemCountCore()
        {
            return m_itemCount;
        }

        private readonly int m_itemCount;
    }

    private sealed class NonVirtualStackLayout : NonVirtualizingLayout
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
