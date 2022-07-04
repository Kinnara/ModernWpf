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

        public static string DefaultIfNullOrEmpty(this string s, string defaultValue)
        {
            return !string.IsNullOrEmpty(s) ? s : defaultValue;
        }
    }
}
