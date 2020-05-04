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
    }
}
