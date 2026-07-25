using System.Windows;

namespace ModernWpf
{
    internal static class ThemeResourceHelper
    {
        private static readonly DependencyProperty ColorKeyProperty =
            DependencyProperty.RegisterAttached(
                "ColorKey",
                typeof(object),
                typeof(ThemeResourceHelper));

        internal static object GetColorKey(DependencyObject element)
        {
            return element.GetValue(ColorKeyProperty);
        }

        internal static void SetColorKey(DependencyObject element, object value)
        {
            element.SetValue(ColorKeyProperty, value);
        }
    }
}
