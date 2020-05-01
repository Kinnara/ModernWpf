using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal static class Extensions
    {
        public static FlyoutBase Flyout(this Button button)
        {
            return FlyoutService.GetFlyout(button);
        }

        public static void UseSystemFocusVisuals(this Control control, bool value)
        {
            if (value)
            {
                control.ClearValue(FrameworkElement.FocusVisualStyleProperty);
            }
            else
            {
                control.FocusVisualStyle = null;
            }
        }
    }
}
