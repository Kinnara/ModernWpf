using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ItemsView;

[TestClass]
public class ItemsViewScrollHostTests
{
    [TestMethod]
    public void BridgesExternalVerticalControllerToWpfScrollViewer()
    {
        WpfTestHost.Run(() =>
        {
            var controller = new TestScrollController();
            var scrollHost = new ItemsViewScrollHost
            {
                Width = 240,
                Height = 120,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = new Border { Width = 200, Height = 720 }
            };

            scrollHost.VerticalScrollController = controller;
            using var host = new TestWindowHost(scrollHost, width: 300, height: 180);
            host.UpdateLayout();

            Assert.AreEqual(ScrollBarVisibility.Hidden, scrollHost.VerticalScrollBarVisibility);
            Assert.AreEqual(0.0, controller.Minimum);
            Assert.IsTrue(controller.Maximum >= 590.0);
            Assert.IsTrue(controller.Viewport >= 110.0);
            Assert.IsTrue(controller.IsScrollable);

            scrollHost.IsEnabled = false;
            Assert.IsFalse(controller.IsScrollable);
            scrollHost.IsEnabled = true;
            Assert.IsTrue(controller.IsScrollable);

            var scrollTo = controller.RaiseScrollTo(140);
            Assert.AreNotEqual(-1, scrollTo.CorrelationId);
            WpfTestHost.DoEvents();
            host.UpdateLayout();
            Assert.AreEqual(140.0, scrollHost.VerticalOffset, 0.5);
            Assert.AreEqual(scrollTo.CorrelationId, controller.LastCompletedCorrelationId);

            var scrollBy = controller.RaiseScrollBy(35);
            WpfTestHost.DoEvents();
            host.UpdateLayout();
            Assert.AreEqual(175.0, scrollHost.VerticalOffset, 0.5);
            Assert.AreEqual(scrollBy.CorrelationId, controller.LastCompletedCorrelationId);

            var invalid = controller.RaiseScrollTo(double.NaN);
            WpfTestHost.DoEvents();
            Assert.AreEqual(175.0, scrollHost.VerticalOffset, 0.5);
            Assert.AreEqual(invalid.CorrelationId, controller.LastCompletedCorrelationId);

            var velocity = controller.RaiseAddVelocity(600);
            WpfTestHost.DoEvents();
            host.UpdateLayout();
            Assert.AreEqual(185.0, scrollHost.VerticalOffset, 0.5);
            Assert.AreEqual(velocity.CorrelationId, controller.LastCompletedCorrelationId);

            var replacementController = new TestScrollController();
            scrollHost.VerticalScrollController = replacementController;
            Assert.AreEqual(ScrollBarVisibility.Hidden, scrollHost.VerticalScrollBarVisibility);
            Assert.IsTrue(replacementController.IsScrollable);

            controller.RaiseScrollTo(20);
            WpfTestHost.DoEvents();
            Assert.AreEqual(185.0, scrollHost.VerticalOffset, 0.5);

            scrollHost.VerticalScrollController = null;
            Assert.AreEqual(ScrollBarVisibility.Auto, scrollHost.VerticalScrollBarVisibility);

            replacementController.RaiseScrollTo(20);
            WpfTestHost.DoEvents();
            Assert.AreEqual(185.0, scrollHost.VerticalOffset, 0.5);
        });
    }

    private sealed class TestScrollController : IScrollController
    {
        public IScrollControllerPanningInfo? PanningInfo => null;

        public bool CanScroll => IsScrollable;

        public bool IsScrollingWithMouse => false;

        public double Minimum { get; private set; }

        public double Maximum { get; private set; }

        public double Offset { get; private set; }

        public double Viewport { get; private set; }

        public bool IsScrollable { get; private set; }

        public int LastCompletedCorrelationId { get; private set; } = -1;

        public event TypedEventHandler<IScrollController, object>? CanScrollChanged
        {
            add { }
            remove { }
        }

        public event TypedEventHandler<IScrollController, object>? IsScrollingWithMouseChanged
        {
            add { }
            remove { }
        }

        public event TypedEventHandler<IScrollController, ScrollControllerScrollToRequestedEventArgs>? ScrollToRequested;

        public event TypedEventHandler<IScrollController, ScrollControllerScrollByRequestedEventArgs>? ScrollByRequested;

        public event TypedEventHandler<IScrollController, ScrollControllerAddScrollVelocityRequestedEventArgs>? AddScrollVelocityRequested;

        public void SetIsScrollable(bool isScrollable)
        {
            IsScrollable = isScrollable;
        }

        public void SetValues(double minOffset, double maxOffset, double offset, double viewportLength)
        {
            Minimum = minOffset;
            Maximum = maxOffset;
            Offset = offset;
            Viewport = viewportLength;
        }

        public object GetScrollAnimation(int correlationId, Point startPosition, Point endPosition, object defaultAnimation)
        {
            return defaultAnimation;
        }

        public void NotifyRequestedScrollCompleted(int correlationId)
        {
            LastCompletedCorrelationId = correlationId;
        }

        internal ScrollControllerScrollToRequestedEventArgs RaiseScrollTo(double offset)
        {
            var args = new ScrollControllerScrollToRequestedEventArgs(
                offset,
                new ScrollingScrollOptions(ScrollingAnimationMode.Disabled));
            ScrollToRequested?.Invoke(this, args);
            return args;
        }

        internal ScrollControllerScrollByRequestedEventArgs RaiseScrollBy(double delta)
        {
            var args = new ScrollControllerScrollByRequestedEventArgs(
                delta,
                new ScrollingScrollOptions(ScrollingAnimationMode.Disabled));
            ScrollByRequested?.Invoke(this, args);
            return args;
        }

        internal ScrollControllerAddScrollVelocityRequestedEventArgs RaiseAddVelocity(float velocity)
        {
            var args = new ScrollControllerAddScrollVelocityRequestedEventArgs(velocity, null);
            AddScrollVelocityRequested?.Invoke(this, args);
            return args;
        }
    }
}
