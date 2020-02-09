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
            if (horizontalOffset.HasValue)
            {
                scrollViewer.ScrollToHorizontalOffset(horizontalOffset.Value);
            }

            if (verticalOffset.HasValue)
            {
                scrollViewer.ScrollToVerticalOffset(verticalOffset.Value);
            }

            return true; // TODO
        }
    }
}
