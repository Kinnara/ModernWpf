using System.Windows;

namespace ModernWpf.Controls
{
    internal static class MenuBarStyleHelper
    {
        public static void InitializeStyle(FrameworkElement element, object key)
        {
            if (element.Style == null &&
                element.ReadLocalValue(FrameworkElement.StyleProperty) == DependencyProperty.UnsetValue)
            {
                element.SetResourceReference(FrameworkElement.StyleProperty, key);
            }
        }
    }
}
