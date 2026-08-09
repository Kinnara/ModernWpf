using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class LinedFlowLayoutTests
{
    [TestMethod]
    public void MatchesCurrentWinUiApiDefaultsAndXamlShape()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout();

            Assert.IsFalse(typeof(LinedFlowLayout).IsSealed);
            Assert.AreEqual(LinedFlowLayoutItemsJustification.Start, layout.ItemsJustification);
            Assert.AreEqual(LinedFlowLayoutItemsStretch.None, layout.ItemsStretch);
            Assert.AreEqual(0.0, layout.MinItemSpacing);
            Assert.AreEqual(0.0, layout.LineSpacing);
            Assert.IsTrue(double.IsNaN(layout.LineHeight));
            Assert.AreEqual(0.0, layout.ActualLineHeight);
            Assert.AreEqual(-1, layout.RequestedRangeStartIndex);
            Assert.AreEqual(0, layout.RequestedRangeLength);

            var parsed = (LinedFlowLayout)XamlReader.Parse(
                "<ui:LinedFlowLayout " +
                "xmlns:ui='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls' " +
                "ItemsJustification='SpaceEvenly' ItemsStretch='Fill' " +
                "MinItemSpacing='8' LineSpacing='6' LineHeight='72' />");

            Assert.AreEqual(LinedFlowLayoutItemsJustification.SpaceEvenly, parsed.ItemsJustification);
            Assert.AreEqual(LinedFlowLayoutItemsStretch.Fill, parsed.ItemsStretch);
            Assert.AreEqual(8.0, parsed.MinItemSpacing);
            Assert.AreEqual(6.0, parsed.LineSpacing);
            Assert.AreEqual(72.0, parsed.LineHeight);
        });
    }

    [TestMethod]
    public void ItemsInfoArgumentsValidateCoverageAndCloneArrays()
    {
        var args = new LinedFlowLayoutItemsInfoRequestedEventArgs(4, 3);
        var ratios = new[] { 1.0, 1.5, 2.0 };

        args.SetDesiredAspectRatios(ratios);
        ratios[0] = 99.0;

        CollectionAssert.AreEqual(new[] { 1.0, 1.5, 2.0 }, args.DesiredAspectRatios);
        Assert.ThrowsExactly<ArgumentException>(() => args.SetMinWidths(new[] { 20.0, 30.0 }));
        Assert.ThrowsExactly<ArgumentException>(() => args.ItemsRangeStartIndex = 5);

        var expanded = new LinedFlowLayoutItemsInfoRequestedEventArgs(4, 3)
        {
            ItemsRangeStartIndex = 2
        };
        expanded.SetDesiredAspectRatios(new[] { 1.0, 1.0, 1.0, 1.0, 1.0 });
        Assert.AreEqual(5, expanded.EstablishedLength);
    }

    [TestMethod]
    public void UsesAspectRatiosSpacingAndLineHeightWithItemsRepeater()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout
            {
                LineHeight = 80,
                MinItemSpacing = 10,
                LineSpacing = 5
            };
            layout.ItemsInfoRequested += (_, args) =>
            {
                var ratios = new[] { 1.0, 2.0, 1.0, 1.0 };
                args.SetDesiredAspectRatios(Slice(ratios, args.ItemsRangeStartIndex, args.ItemsRangeRequestedLength));
            };
            var repeater = CreateRepeater(layout, 4);
            var scrollViewer = new ScrollViewer
            {
                Width = 330,
                Height = 180,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = repeater
            };

            using var host = new TestWindowHost(scrollViewer, width: 390, height: 240);
            host.UpdateLayout();

            Assert.AreEqual(80.0, layout.ActualLineHeight);
            AssertLayoutSlot(repeater, 0, new Rect(0, 0, 80, 80));
            AssertLayoutSlot(repeater, 1, new Rect(90, 0, 160, 80));
            AssertLayoutSlot(repeater, 2, new Rect(0, 85, 80, 80));
            AssertLayoutSlot(repeater, 3, new Rect(90, 85, 80, 80));
            Assert.IsTrue(layout.RequestedRangeStartIndex >= 0);
            Assert.IsTrue(layout.RequestedRangeLength > 0);
        });
    }

    [TestMethod]
    public void SupportsFillJustificationVirtualizationAndItemUnlocking()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout
            {
                LineHeight = 50,
                MinItemSpacing = 10,
                LineSpacing = 5,
                ItemsStretch = LinedFlowLayoutItemsStretch.Fill
            };
            layout.ItemsInfoRequested += (_, args) =>
                args.SetDesiredAspectRatios(Enumerable.Repeat(1.0, args.ItemsRangeRequestedLength).ToArray());
            var repeater = CreateRepeater(layout, 1000);
            var scrollViewer = new ScrollViewer
            {
                Width = 240,
                Height = 160,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = repeater
            };
            var scrollHost = new ItemsRepeaterScrollHost
            {
                ScrollViewer = scrollViewer
            };

            using var host = new TestWindowHost(scrollHost, width: 290, height: 220);
            host.UpdateLayout();

            var first = (FrameworkElement)repeater.TryGetElement(0)!;
            var firstSlot = LayoutInformation.GetLayoutSlot(first);
            Assert.IsGreaterThan(50.0, firstSlot.Width);
            Assert.IsLessThan(
                1000,
                Enumerable.Range(0, 1000).Count(index => repeater.TryGetElement(index) != null));

            int lockedLine = layout.LockItemToLine(1);
            Assert.AreEqual(0, lockedLine);

            bool itemsUnlocked = false;
            layout.ItemsUnlocked += (_, _) => itemsUnlocked = true;
            layout.MinItemSpacing = 12;
            Assert.IsTrue(itemsUnlocked);

            scrollViewer.ScrollToVerticalOffset(5000);
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.IsNull(repeater.TryGetElement(0));
            Assert.IsTrue(Enumerable.Range(100, 900).Any(index => repeater.TryGetElement(index) != null));
        });
    }

    [TestMethod]
    public void ArrangesEveryWinUiJustificationMode()
    {
        WpfTestHost.Run(() =>
        {
            var expectations = new[]
            {
                (LinedFlowLayoutItemsJustification.Start, 0.0, 60.0),
                (LinedFlowLayoutItemsJustification.Center, 75.0, 135.0),
                (LinedFlowLayoutItemsJustification.End, 150.0, 210.0),
                (LinedFlowLayoutItemsJustification.SpaceAround, 37.5, 172.5),
                (LinedFlowLayoutItemsJustification.SpaceBetween, 0.0, 210.0),
                (LinedFlowLayoutItemsJustification.SpaceEvenly, 50.0, 160.0)
            };

            foreach (var expectation in expectations)
            {
                var layout = new LinedFlowLayout
                {
                    LineHeight = 50,
                    MinItemSpacing = 10,
                    ItemsJustification = expectation.Item1
                };
                layout.ItemsInfoRequested += (_, args) =>
                    args.SetDesiredAspectRatios(
                        Enumerable.Repeat(1.0, args.ItemsRangeRequestedLength).ToArray());
                var repeater = CreateRepeater(layout, 2);
                var scrollViewer = new ScrollViewer
                {
                    Width = 260,
                    Height = 80,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = repeater
                };

                using var host = new TestWindowHost(scrollViewer, width: 320, height: 140);
                host.UpdateLayout();

                Assert.AreEqual(
                    expectation.Item2,
                    LayoutInformation.GetLayoutSlot((FrameworkElement)repeater.TryGetElement(0)!).X,
                    0.5,
                    $"{expectation.Item1} first item");
                Assert.AreEqual(
                    expectation.Item3,
                    LayoutInformation.GetLayoutSlot((FrameworkElement)repeater.TryGetElement(1)!).X,
                    0.5,
                    $"{expectation.Item1} second item");
            }
        });
    }

    private static ItemsRepeater CreateRepeater(LinedFlowLayout layout, int count)
    {
        return new ItemsRepeater
        {
            Layout = layout,
            ItemsSource = Enumerable.Range(0, count),
            ItemTemplate = (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border Background='SteelBlue'><TextBlock Text='{Binding}' /></Border>" +
                "</DataTemplate>")
        };
    }

    private static double[] Slice(double[] values, int start, int length)
    {
        return values.Skip(start).Take(length).ToArray();
    }

    private static void AssertLayoutSlot(ItemsRepeater repeater, int index, Rect expected)
    {
        var element = (FrameworkElement)repeater.TryGetElement(index)!;
        Assert.IsNotNull(element);
        Rect actual = LayoutInformation.GetLayoutSlot(element);
        Assert.AreEqual(expected.X, actual.X, 0.5, $"Item {index} X");
        Assert.AreEqual(expected.Y, actual.Y, 0.5, $"Item {index} Y");
        Assert.AreEqual(expected.Width, actual.Width, 0.5, $"Item {index} width");
        Assert.AreEqual(expected.Height, actual.Height, 0.5, $"Item {index} height");
    }
}
