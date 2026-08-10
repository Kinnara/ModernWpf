using System;
using System.Collections.Generic;
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
public class LinedFlowLayoutApiTests
{
    [TestMethod]
    public void DefaultsAndXamlSurfaceMatchCurrentWinUI()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout();

            Assert.AreEqual(LinedFlowLayoutItemsJustification.Start, layout.ItemsJustification);
            Assert.AreEqual(LinedFlowLayoutItemsStretch.None, layout.ItemsStretch);
            Assert.AreEqual(0.0, layout.MinItemSpacing);
            Assert.AreEqual(0.0, layout.LineSpacing);
            Assert.IsTrue(double.IsNaN(layout.LineHeight));
            Assert.AreEqual(0.0, layout.ActualLineHeight);
            Assert.AreEqual(-1, layout.RequestedRangeStartIndex);
            Assert.AreEqual(0, layout.RequestedRangeLength);
            Assert.AreEqual(IndexBasedLayoutOrientation.TopToBottom, layout.IndexBasedLayoutOrientation);

            var parsed = (LinedFlowLayout)XamlReader.Parse(
                "<controls:LinedFlowLayout " +
                "xmlns:controls='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls' " +
                "ItemsJustification='SpaceEvenly' ItemsStretch='Fill' " +
                "MinItemSpacing='7' LineSpacing='9' LineHeight='41' />");

            Assert.AreEqual(LinedFlowLayoutItemsJustification.SpaceEvenly, parsed.ItemsJustification);
            Assert.AreEqual(LinedFlowLayoutItemsStretch.Fill, parsed.ItemsStretch);
            Assert.AreEqual(7.0, parsed.MinItemSpacing);
            Assert.AreEqual(9.0, parsed.LineSpacing);
            Assert.AreEqual(41.0, parsed.LineHeight);
            Assert.IsNotNull(LinedFlowLayout.ActualLineHeightProperty);
        });
    }

    [TestMethod]
    public void ItemInfoDrivesLineGeometryAndLockLifecycle()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout
            {
                LineHeight = 50,
                MinItemSpacing = 10,
                LineSpacing = 5
            };
            var requestCount = 0;
            layout.ItemsInfoRequested += (sender, args) =>
            {
                requestCount++;
                Assert.IsTrue(args.ItemsRangeRequestedLength > 0);
                args.SetDesiredAspectRatios(
                    Enumerable.Repeat(2.0, args.ItemsRangeRequestedLength).ToArray());
            };

            var repeater = CreateRepeater(layout, 4, width: 250);
            using var host = new TestWindowHost(repeater, width: 310, height: 180);

            Assert.IsTrue(requestCount > 0);
            Assert.AreEqual(50.0, layout.ActualLineHeight);
            Assert.AreEqual(0, layout.RequestedRangeStartIndex);
            Assert.AreEqual(4, layout.RequestedRangeLength);
            AssertLayoutSlot(repeater, 0, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(repeater, 1, new Rect(110, 0, 100, 50));
            AssertLayoutSlot(repeater, 2, new Rect(0, 55, 100, 50));
            AssertLayoutSlot(repeater, 3, new Rect(110, 55, 100, 50));

            Assert.AreEqual(1, layout.LockItemToLine(2));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => layout.LockItemToLine(-1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => layout.LockItemToLine(4));

            var unlockedCount = 0;
            layout.ItemsUnlocked += (sender, args) => unlockedCount++;
            layout.InvalidateItemsInfo();
            host.UpdateLayout();

            Assert.AreEqual(0, unlockedCount);
            Assert.IsTrue(requestCount > 1);

            repeater.Width = 150;
            host.UpdateLayout();

            Assert.AreEqual(1, unlockedCount);
        });
    }

    [TestMethod]
    public void StretchAndJustificationAffectArrangedBounds()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout
            {
                LineHeight = 50,
                MinItemSpacing = 10,
                ItemsStretch = LinedFlowLayoutItemsStretch.Fill
            };
            layout.ItemsInfoRequested += (sender, args) =>
                args.SetDesiredAspectRatios(Enumerable.Repeat(1.0, args.ItemsRangeRequestedLength).ToArray());

            var repeater = CreateRepeater(layout, 2, width: 250);
            using var host = new TestWindowHost(repeater, width: 310, height: 140);

            AssertLayoutSlot(repeater, 0, new Rect(0, 0, 120, 50));
            AssertLayoutSlot(repeater, 1, new Rect(130, 0, 120, 50));

            layout.ItemsStretch = LinedFlowLayoutItemsStretch.None;
            layout.ItemsJustification = LinedFlowLayoutItemsJustification.End;
            host.UpdateLayout();

            AssertLayoutSlot(repeater, 0, new Rect(140, 0, 50, 50));
            AssertLayoutSlot(repeater, 1, new Rect(200, 0, 50, 50));
        });
    }

    [TestMethod]
    public void ArrangesEveryCurrentWinUiJustificationMode()
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
                layout.ItemsInfoRequested += (sender, args) =>
                    args.SetDesiredAspectRatios(
                        Enumerable.Repeat(1.0, args.ItemsRangeRequestedLength).ToArray());
                var repeater = CreateRepeater(layout, 2, width: 260);

                using var host = new TestWindowHost(repeater, width: 320, height: 140);

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

    [TestMethod]
    public void FillHonorsPerItemMinimumAndMaximumWidths()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout
            {
                LineHeight = 50,
                MinItemSpacing = 10,
                ItemsStretch = LinedFlowLayoutItemsStretch.Fill
            };
            layout.ItemsInfoRequested += (sender, args) =>
            {
                args.SetDesiredAspectRatios(
                    Enumerable.Repeat(1.0, args.ItemsRangeRequestedLength).ToArray());
                args.SetMinWidths(new[] { 100.0, 20.0 });
                args.SetMaxWidths(new[] { 100.0, 200.0 });
            };
            var repeater = CreateRepeater(layout, 2, width: 250);

            using var host = new TestWindowHost(repeater, width: 310, height: 140);

            AssertLayoutSlot(repeater, 0, new Rect(0, 0, 100, 50));
            AssertLayoutSlot(repeater, 1, new Rect(110, 0, 140, 50));
        });
    }

    [TestMethod]
    public void LockKeepsItsLineUntilTheUnlockedAverageChanges()
    {
        WpfTestHost.Run(() =>
        {
            var ratios = Enumerable.Repeat(2.0, 6).ToArray();
            var layout = new LinedFlowLayout
            {
                LineHeight = 50,
                MinItemSpacing = 10,
                LineSpacing = 5
            };
            layout.ItemsInfoRequested += (sender, args) =>
                args.SetDesiredAspectRatios(
                    ratios.Skip(args.ItemsRangeStartIndex)
                        .Take(args.ItemsRangeRequestedLength)
                        .ToArray());
            var repeater = CreateRepeater(layout, 6, width: 250);

            using var host = new TestWindowHost(repeater, width: 310, height: 240);

            Assert.AreEqual(1, layout.LockItemToLine(2));
            Assert.AreEqual(1, layout.LockItemToLine(2));
            var unlockedCount = 0;
            layout.ItemsUnlocked += (sender, args) => unlockedCount++;

            ratios = new[] { 1.0, 1.0, 1.0, 4.8, 1.0, 1.0 };
            layout.InvalidateItemsInfo();
            host.UpdateLayout();

            Assert.AreEqual(0, unlockedCount);
            Assert.AreEqual(55.0, LayoutInformation.GetLayoutSlot(
                (FrameworkElement)repeater.TryGetElement(2)!).Y, 0.5);

            repeater.Width = 150;
            host.UpdateLayout();

            Assert.AreEqual(1, unlockedCount);
        });
    }

    [TestMethod]
    public void ScrollHostVirtualizesAndRealizesTheNewViewport()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout
            {
                LineHeight = 20,
                MinItemSpacing = 4,
                LineSpacing = 2
            };
            layout.ItemsInfoRequested += (sender, args) =>
                args.SetDesiredAspectRatios(Enumerable.Repeat(1.0, args.ItemsRangeRequestedLength).ToArray());

            var repeater = CreateRepeater(layout, 200, width: 220);
            repeater.HorizontalCacheLength = 0;
            repeater.VerticalCacheLength = 0;
            var scrollViewer = new ScrollViewer
            {
                Width = 220,
                Height = 70,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = repeater
            };
            var scrollHost = new ItemsRepeaterScrollHost { ScrollViewer = scrollViewer };

            using var host = new TestWindowHost(scrollHost, width: 280, height: 140);

            int firstRealizedCount = CountRealized(repeater);
            Assert.IsTrue(firstRealizedCount > 0);
            Assert.IsTrue(firstRealizedCount < 200);
            Assert.IsNotNull(repeater.TryGetElement(0));

            scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
            host.UpdateLayout();
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.IsNull(repeater.TryGetElement(0));
            Assert.IsTrue(Enumerable.Range(150, 50).Any(index => repeater.TryGetElement(index) != null));
            Assert.IsTrue(CountRealized(repeater) < 200);
        });
    }

    [TestMethod]
    public void ItemInfoArraysMustCoverTheRequestedRangeAndMatchLengths()
    {
        WpfTestHost.Run(() =>
        {
            var layout = new LinedFlowLayout { LineHeight = 20 };
            var validated = false;
            layout.ItemsInfoRequested += (sender, args) =>
            {
                Assert.ThrowsExactly<ArgumentException>(
                    () => args.SetDesiredAspectRatios(new double[args.ItemsRangeRequestedLength - 1]));
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => args.ItemsRangeStartIndex = -1);

                args.ItemsRangeStartIndex = 0;
                var expandedLength = args.ItemsRangeRequestedLength + args.ItemsRangeStartIndex;
                args.SetDesiredAspectRatios(Enumerable.Repeat(1.0, expandedLength).ToArray());
                Assert.ThrowsExactly<ArgumentException>(
                    () => args.SetMinWidths(new double[expandedLength + 1]));
                args.SetMinWidths(Enumerable.Repeat(10.0, expandedLength).ToArray());
                args.SetMaxWidths(Enumerable.Repeat(20.0, expandedLength).ToArray());
                validated = true;
            };

            var repeater = CreateRepeater(layout, 3, width: 100);
            using var host = new TestWindowHost(repeater, width: 160, height: 100);

            Assert.IsTrue(validated);
        });
    }

    [TestMethod]
    public void ItemInfoArraysAreCloned()
    {
        var args = new LinedFlowLayoutItemsInfoRequestedEventArgs(4, 3);
        var ratios = new[] { 1.0, 1.5, 2.0 };
        var minimums = new[] { 20.0, 30.0, 40.0 };
        var maximums = new[] { 120.0, 130.0, 140.0 };

        args.SetDesiredAspectRatios(ratios);
        args.SetMinWidths(minimums);
        args.SetMaxWidths(maximums);
        ratios[0] = 99.0;
        minimums[0] = 99.0;
        maximums[0] = 99.0;

        CollectionAssert.AreEqual(new[] { 1.0, 1.5, 2.0 }, args.DesiredAspectRatios);
        CollectionAssert.AreEqual(new[] { 20.0, 30.0, 40.0 }, args.MinWidths);
        CollectionAssert.AreEqual(new[] { 120.0, 130.0, 140.0 }, args.MaxWidths);
    }

    private static ItemsRepeater CreateRepeater(LinedFlowLayout layout, int count, double width)
    {
        return new ItemsRepeater
        {
            Width = width,
            Layout = layout,
            ItemsSource = Enumerable.Range(0, count),
            ItemTemplate = (DataTemplate)XamlReader.Parse(
                "<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Border><TextBlock Text='{Binding}' /></Border>" +
                "</DataTemplate>")
        };
    }

    private static int CountRealized(ItemsRepeater repeater)
    {
        return Enumerable.Range(0, repeater.ItemsSourceView.Count)
            .Count(index => repeater.TryGetElement(index) != null);
    }

    private static void AssertLayoutSlot(ItemsRepeater repeater, int index, Rect expected)
    {
        var element = repeater.TryGetElement(index) as FrameworkElement;
        var childIndexes = string.Join(
            ", ",
            repeater.Children.Cast<UIElement>().Select(repeater.GetElementIndex));
        Assert.IsNotNull(
            element,
            $"Expected item {index} to be realized. Child indexes: [{childIndexes}].");
        Assert.AreEqual(expected, LayoutInformation.GetLayoutSlot(element!));
    }
}
