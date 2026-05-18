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

            double? targetHorizontalOffset = null;
            double? targetVerticalOffset = null;

            if (horizontalOffset.HasValue)
            {
                targetHorizontalOffset = CoerceOffset(
                    horizontalOffset.Value,
                    scrollViewer.ScrollableWidth,
                    nameof(horizontalOffset));
            }

            if (verticalOffset.HasValue)
            {
                targetVerticalOffset = CoerceOffset(
                    verticalOffset.Value,
                    scrollViewer.ScrollableHeight,
                    nameof(verticalOffset));
            }

            var handled = false;

            if (targetHorizontalOffset.HasValue)
            {
                if (!AreClose(scrollViewer.HorizontalOffset, targetHorizontalOffset.Value))
                {
                    scrollViewer.ScrollToHorizontalOffset(targetHorizontalOffset.Value);
                    handled = true;
                }
            }

            if (targetVerticalOffset.HasValue)
            {
                if (!AreClose(scrollViewer.VerticalOffset, targetVerticalOffset.Value))
                {
                    scrollViewer.ScrollToVerticalOffset(targetVerticalOffset.Value);
                    handled = true;
                }
            }

            return handled;
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
