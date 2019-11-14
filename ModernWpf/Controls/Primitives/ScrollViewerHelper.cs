using System.Windows;

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

        public static bool GetAutoHideScrollBars(DependencyObject element)
        {
            return (bool)element.GetValue(AutoHideScrollBarsProperty);
        }

        public static void SetAutoHideScrollBars(DependencyObject element, bool value)
        {
            element.SetValue(AutoHideScrollBarsProperty, value);
        }

        #endregion
    }
}
