using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls.Primitives
{
    public static class ScrollViewerHelper
    {
        #region AutoHideScrollBars

        public static readonly DependencyProperty AutoHideScrollBarsProperty =
            DependencyProperty.RegisterAttached(
                "AutoHideScrollBars",
                typeof(bool),
                typeof(ScrollViewerHelper));

        public static bool GetAutoHideScrollBars(ScrollViewer scrollViewer)
        {
            return (bool)scrollViewer.GetValue(AutoHideScrollBarsProperty);
        }

        public static void SetAutoHideScrollBars(ScrollViewer scrollViewer, bool value)
        {
            scrollViewer.SetValue(AutoHideScrollBarsProperty, value);
        }

        #endregion
    }
}
