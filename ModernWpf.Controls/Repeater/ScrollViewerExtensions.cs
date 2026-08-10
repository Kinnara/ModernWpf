using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal static class ScrollViewerExtensions
    {
        public static UIElement GetContentTemplateRoot(this ScrollViewer scrollViewer)
        {
            return scrollViewer.Content as UIElement;
        }

        public static Size GetEffectiveViewportSize(this ScrollViewer scrollViewer)
        {
            ScrollContentPresenter presenter = GetScrollContentPresenter(scrollViewer);
            return new Size(
                GetEffectiveMetric(scrollViewer.ViewportWidth, presenter?.ViewportWidth, scrollViewer.ActualWidth),
                GetEffectiveMetric(scrollViewer.ViewportHeight, presenter?.ViewportHeight, scrollViewer.ActualHeight));
        }

        public static Size GetEffectiveExtentSize(this ScrollViewer scrollViewer)
        {
            ScrollContentPresenter presenter = GetScrollContentPresenter(scrollViewer);
            Size viewport = scrollViewer.GetEffectiveViewportSize();
            return new Size(
                GetEffectiveMetric(scrollViewer.ExtentWidth, presenter?.ExtentWidth, viewport.Width),
                GetEffectiveMetric(scrollViewer.ExtentHeight, presenter?.ExtentHeight, viewport.Height));
        }

        public static Vector GetEffectiveOffset(this ScrollViewer scrollViewer)
        {
            ScrollContentPresenter presenter = GetScrollContentPresenter(scrollViewer);
            return new Vector(
                GetEffectiveOffset(scrollViewer.HorizontalOffset, presenter?.HorizontalOffset),
                GetEffectiveOffset(scrollViewer.VerticalOffset, presenter?.VerticalOffset));
        }

        public static bool ChangeView(this ScrollViewer scrollViewer,
            double? horizontalOffset,
            double? verticalOffset,
            float? zoomFactor)
        {
            return scrollViewer.ChangeView(horizontalOffset, verticalOffset, zoomFactor, false);
        }

        public static bool ChangeView(this ScrollViewer scrollViewer,
            double? horizontalOffset,
            double? verticalOffset,
            float? zoomFactor,
            bool disableAnimation)
        {
            if (zoomFactor.HasValue && (float.IsNaN(zoomFactor.Value) || float.IsInfinity(zoomFactor.Value)))
            {
                throw new ArgumentException("The value cannot be infinite or NaN.", nameof(zoomFactor));
            }

            Size viewport = scrollViewer.GetEffectiveViewportSize();
            Size extent = scrollViewer.GetEffectiveExtentSize();
            Vector currentOffset = scrollViewer.GetEffectiveOffset();
            ScrollContentPresenter presenter = GetScrollContentPresenter(scrollViewer);
            bool usePresenterFallback = presenter != null &&
                (scrollViewer.ViewportWidth <= 0 || scrollViewer.ViewportHeight <= 0) &&
                (presenter.ViewportWidth > 0 || presenter.ViewportHeight > 0);

            double? targetHorizontalOffset = null;
            double? targetVerticalOffset = null;

            if (horizontalOffset.HasValue)
            {
                targetHorizontalOffset = CoerceOffset(
                    horizontalOffset.Value,
                    Math.Max(0, extent.Width - viewport.Width),
                    nameof(horizontalOffset));
            }

            if (verticalOffset.HasValue)
            {
                targetVerticalOffset = CoerceOffset(
                    verticalOffset.Value,
                    Math.Max(0, extent.Height - viewport.Height),
                    nameof(verticalOffset));
            }

            var handled = false;

            if (targetHorizontalOffset.HasValue)
            {
                if (!AreClose(currentOffset.X, targetHorizontalOffset.Value))
                {
                    if (usePresenterFallback)
                    {
                        presenter.SetHorizontalOffset(targetHorizontalOffset.Value);
                    }
                    else
                    {
                        scrollViewer.ScrollToHorizontalOffset(targetHorizontalOffset.Value);
                    }
                    handled = true;
                }
            }

            if (targetVerticalOffset.HasValue)
            {
                if (!AreClose(currentOffset.Y, targetVerticalOffset.Value))
                {
                    if (usePresenterFallback)
                    {
                        presenter.SetVerticalOffset(targetVerticalOffset.Value);
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(targetVerticalOffset.Value);
                    }
                    handled = true;
                }
            }

            if (handled && usePresenterFallback)
            {
                scrollViewer.InvalidateScrollInfo();
            }

            return handled;
        }

        private static ScrollContentPresenter GetScrollContentPresenter(ScrollViewer scrollViewer)
        {
            scrollViewer.ApplyTemplate();
            return scrollViewer.Template?.FindName("PART_ScrollContentPresenter", scrollViewer) as ScrollContentPresenter;
        }

        private static double GetEffectiveMetric(double cachedValue, double? presenterValue, double fallbackValue)
        {
            if (cachedValue > 0)
            {
                return cachedValue;
            }

            if (presenterValue.HasValue && presenterValue.Value > 0)
            {
                return presenterValue.Value;
            }

            return Math.Max(0, fallbackValue);
        }

        private static double GetEffectiveOffset(double cachedValue, double? presenterValue)
        {
            if (presenterValue.HasValue && !AreClose(cachedValue, presenterValue.Value))
            {
                return presenterValue.Value;
            }

            return cachedValue;
        }

        private static double CoerceOffset(double offset, double scrollableExtent, string parameterName)
        {
            if (double.IsNaN(offset) || double.IsInfinity(offset))
            {
                throw new ArgumentException("The value cannot be infinite or NaN.", parameterName);
            }

            var maxOffset = double.IsNaN(scrollableExtent) || double.IsInfinity(scrollableExtent)
                ? 0
                : Math.Max(0, scrollableExtent);

            return Math.Max(0, Math.Min(offset, maxOffset));
        }

        private static bool AreClose(double value1, double value2)
        {
            return Math.Abs(value1 - value2) < 0.0001;
        }
    }
}
