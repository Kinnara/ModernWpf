using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ParallaxView;

[TestClass]
public class ParallaxViewApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var parallaxView = new ModernWpf.Controls.ParallaxView();

            Assert.IsNull(parallaxView.Child);
            Assert.IsNull(parallaxView.Source);
            Assert.AreEqual(ParallaxSourceOffsetKind.Relative, parallaxView.HorizontalSourceOffsetKind);
            Assert.AreEqual(0.0, parallaxView.HorizontalSourceStartOffset);
            Assert.AreEqual(0.0, parallaxView.HorizontalSourceEndOffset);
            Assert.AreEqual(1.0, parallaxView.MaxHorizontalShiftRatio);
            Assert.AreEqual(0.0, parallaxView.HorizontalShift);
            Assert.IsTrue(parallaxView.IsHorizontalShiftClamped);
            Assert.AreEqual(ParallaxSourceOffsetKind.Relative, parallaxView.VerticalSourceOffsetKind);
            Assert.AreEqual(0.0, parallaxView.VerticalSourceStartOffset);
            Assert.AreEqual(0.0, parallaxView.VerticalSourceEndOffset);
            Assert.AreEqual(1.0, parallaxView.MaxVerticalShiftRatio);
            Assert.AreEqual(0.0, parallaxView.VerticalShift);
            Assert.IsTrue(parallaxView.IsVerticalShiftClamped);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var rectangle = new Border();
            var scrollViewer = new ScrollViewer();
            var parallaxView = new ModernWpf.Controls.ParallaxView
            {
                Child = rectangle,
                Source = scrollViewer,
                HorizontalSourceOffsetKind = ParallaxSourceOffsetKind.Absolute,
                HorizontalSourceStartOffset = 11.0,
                HorizontalSourceEndOffset = 22.0,
                MaxHorizontalShiftRatio = 0.123,
                HorizontalShift = 321.0,
                IsHorizontalShiftClamped = false,
                VerticalSourceOffsetKind = ParallaxSourceOffsetKind.Absolute,
                VerticalSourceStartOffset = 4.5,
                VerticalSourceEndOffset = 5.4,
                MaxVerticalShiftRatio = 0.321,
                VerticalShift = 45.6,
                IsVerticalShiftClamped = false
            };

            Assert.AreSame(rectangle, parallaxView.Child);
            Assert.AreSame(scrollViewer, parallaxView.Source);
            Assert.AreEqual(ParallaxSourceOffsetKind.Absolute, parallaxView.HorizontalSourceOffsetKind);
            Assert.AreEqual(11.0, parallaxView.HorizontalSourceStartOffset);
            Assert.AreEqual(22.0, parallaxView.HorizontalSourceEndOffset);
            Assert.AreEqual(0.123, parallaxView.MaxHorizontalShiftRatio);
            Assert.AreEqual(321.0, parallaxView.HorizontalShift);
            Assert.IsFalse(parallaxView.IsHorizontalShiftClamped);
            Assert.AreEqual(ParallaxSourceOffsetKind.Absolute, parallaxView.VerticalSourceOffsetKind);
            Assert.AreEqual(4.5, parallaxView.VerticalSourceStartOffset);
            Assert.AreEqual(5.4, parallaxView.VerticalSourceEndOffset);
            Assert.AreEqual(0.321, parallaxView.MaxVerticalShiftRatio);
            Assert.AreEqual(45.6, parallaxView.VerticalShift);
            Assert.IsFalse(parallaxView.IsVerticalShiftClamped);
        });
    }

    [TestMethod]
    public void MeasureArrangeExpandsChildAndClipsViewport()
    {
        WpfTestHost.Run(() =>
        {
            var child = new Border
            {
                Width = 40,
                Height = 40,
                Background = Brushes.Red
            };
            var parallaxView = new ModernWpf.Controls.ParallaxView
            {
                Width = 100,
                Height = 80,
                HorizontalShift = 50,
                VerticalShift = 20,
                Child = child
            };

            using var host = new TestWindowHost(parallaxView, width: 200, height: 160);

            var slot = LayoutInformation.GetLayoutSlot(child);
            Assert.AreEqual(new Rect(0, 0, 150, 100), slot);
            Assert.IsInstanceOfType(parallaxView.Clip, typeof(RectangleGeometry));
            Assert.AreEqual(new Rect(0, 0, 100, 80), ((RectangleGeometry)parallaxView.Clip).Rect);
        });
    }

    [TestMethod]
    public void ArrangeReusesSourceClipGeometry()
    {
        WpfTestHost.Run(() =>
        {
            var child = new Border
            {
                Width = 40,
                Height = 40
            };
            var parallaxView = new ModernWpf.Controls.ParallaxView
            {
                Width = 100,
                Height = 80,
                Child = child
            };

            using var host = new TestWindowHost(parallaxView, width: 200, height: 160);

            var clip = parallaxView.Clip;
            Assert.IsInstanceOfType(clip, typeof(RectangleGeometry));
            Assert.AreEqual(new Rect(0, 0, 100, 80), ((RectangleGeometry)clip).Rect);

            parallaxView.Width = 120;
            parallaxView.Height = 90;
            host.UpdateLayout();

            Assert.AreSame(clip, parallaxView.Clip);
            Assert.AreEqual(new Rect(0, 0, 120, 90), ((RectangleGeometry)parallaxView.Clip).Rect);
        });
    }

    [TestMethod]
    public void RefreshAutomaticOffsetsFollowSourceGuards()
    {
        WpfTestHost.Run(() =>
        {
            var parallaxView = new CountingParallaxView
            {
                Width = 100,
                Height = 80,
                Child = new Border { Width = 40, Height = 40 },
                HorizontalShift = 20,
                VerticalShift = 20,
                HorizontalSourceOffsetKind = ParallaxSourceOffsetKind.Absolute,
                VerticalSourceOffsetKind = ParallaxSourceOffsetKind.Absolute
            };

            using var host = new TestWindowHost(parallaxView, width: 200, height: 160);

            parallaxView.ArrangeCount = 0;
            parallaxView.RefreshAutomaticHorizontalOffsets();
            parallaxView.RefreshAutomaticVerticalOffsets();
            host.UpdateLayout();
            Assert.AreEqual(0, parallaxView.ArrangeCount);

            parallaxView.HorizontalSourceOffsetKind = ParallaxSourceOffsetKind.Relative;
            parallaxView.VerticalSourceOffsetKind = ParallaxSourceOffsetKind.Relative;
            host.UpdateLayout();

            parallaxView.ArrangeCount = 0;
            parallaxView.RefreshAutomaticHorizontalOffsets();
            host.UpdateLayout();
            Assert.IsTrue(parallaxView.ArrangeCount > 0);

            parallaxView.ArrangeCount = 0;
            parallaxView.RefreshAutomaticVerticalOffsets();
            host.UpdateLayout();
            Assert.IsTrue(parallaxView.ArrangeCount > 0);
        });
    }

    [TestMethod]
    public void ScrollViewerRelativeParallaxOffsetsChild()
    {
        WpfTestHost.Run(() =>
        {
            var source = CreateScrollViewerSource();
            var child = new Border
            {
                Width = 200,
                Height = 100,
                Background = Brushes.Blue
            };
            var parallaxView = new ModernWpf.Controls.ParallaxView
            {
                Width = 200,
                Height = 100,
                Source = source,
                Child = child,
                HorizontalShift = 100,
                VerticalShift = 50
            };
            var root = CreateRoot(source, parallaxView);

            using var host = new TestWindowHost(root, width: 260, height: 160);

            source.ScrollToHorizontalOffset(100);
            source.ScrollToVerticalOffset(50);
            host.UpdateLayout();

            var slot = LayoutInformation.GetLayoutSlot(child);
            Assert.AreEqual(CalculateExpectedShift(100, 0, source.ScrollableWidth, 100, 1.0, true), slot.X, 0.5);
            Assert.AreEqual(CalculateExpectedShift(50, 0, source.ScrollableHeight, 50, 1.0, true), slot.Y, 0.5);
            Assert.AreEqual(300, slot.Width, 0.5);
            Assert.AreEqual(150, slot.Height, 0.5);
        });
    }

    [TestMethod]
    public void AbsoluteUnclampedOffsetsCanExceedShift()
    {
        WpfTestHost.Run(() =>
        {
            var source = CreateScrollViewerSource();
            var child = new Border { Width = 200, Height = 100 };
            var parallaxView = new ModernWpf.Controls.ParallaxView
            {
                Width = 200,
                Height = 100,
                Source = source,
                Child = child,
                HorizontalShift = 80,
                HorizontalSourceOffsetKind = ParallaxSourceOffsetKind.Absolute,
                HorizontalSourceStartOffset = 40,
                HorizontalSourceEndOffset = 120
            };
            var root = CreateRoot(source, parallaxView);

            using var host = new TestWindowHost(root, width: 260, height: 160);

            source.ScrollToHorizontalOffset(140);
            host.UpdateLayout();

            Assert.AreEqual(-80, LayoutInformation.GetLayoutSlot(child).X, 0.5);

            parallaxView.IsHorizontalShiftClamped = false;
            host.UpdateLayout();

            Assert.AreEqual(-100, LayoutInformation.GetLayoutSlot(child).X, 0.5);
        });
    }

    [TestMethod]
    public void ParallaxViewXamlContentPropertyTest()
    {
        WpfTestHost.Run(() =>
        {
            var parallaxView = (ModernWpf.Controls.ParallaxView)XamlReader.Parse(
                @"<ui:ParallaxView xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                        xmlns:ui='http://schemas.modernwpf.com/2019'
                        HorizontalShift='12'
                        VerticalShift='8'>
                    <Border Width='20' Height='30'/>
                </ui:ParallaxView>");

            Assert.AreEqual(12.0, parallaxView.HorizontalShift);
            Assert.AreEqual(8.0, parallaxView.VerticalShift);
            Assert.IsInstanceOfType(parallaxView.Child, typeof(Border));
        });
    }

    private static ScrollViewer CreateScrollViewerSource()
    {
        return new ScrollViewer
        {
            Width = 200,
            Height = 100,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            Content = new Border
            {
                Width = 1200,
                Height = 600
            }
        };
    }

    private static Grid CreateRoot(UIElement source, UIElement parallaxView)
    {
        var root = new Grid
        {
            Width = 220,
            Height = 120
        };
        root.Children.Add(source);
        root.Children.Add(parallaxView);
        return root;
    }

    private static double CalculateExpectedShift(
        double sourceOffset,
        double startOffset,
        double endOffset,
        double shift,
        double maxShiftRatio,
        bool isShiftClamped)
    {
        var span = endOffset - startOffset;
        if (Math.Abs(shift) < 0.000001 || span <= 0.000001)
        {
            return 0;
        }

        var maxRatio = Math.Max(0.0, maxShiftRatio);
        if (shift > 0)
        {
            if (isShiftClamped)
            {
                if (sourceOffset <= startOffset)
                {
                    return 0;
                }

                if (sourceOffset < endOffset)
                {
                    return -Math.Min(maxRatio, shift / span) * (sourceOffset - startOffset);
                }

                return -Math.Min(maxRatio * span, shift);
            }

            return -Math.Min(maxRatio, shift / span) * (sourceOffset - startOffset);
        }

        if (isShiftClamped)
        {
            if (sourceOffset <= startOffset)
            {
                return -Math.Min(maxRatio * span, -shift);
            }

            if (sourceOffset < endOffset)
            {
                return Math.Min(maxRatio, shift / (startOffset - endOffset)) * (sourceOffset - endOffset);
            }

            return 0;
        }

        return Math.Min(maxRatio, shift / (startOffset - endOffset)) * (sourceOffset - endOffset);
    }

    private sealed class CountingParallaxView : ModernWpf.Controls.ParallaxView
    {
        public int ArrangeCount { get; set; }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            ArrangeCount++;
            return base.ArrangeOverride(arrangeSize);
        }
    }
}
