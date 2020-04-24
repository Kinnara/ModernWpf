using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal static class Extensions
    {
        public static FlyoutBase Flyout(this Button button)
        {
            return FlyoutService.GetFlyout(button);
        }

        public static void RemoveAtEnd(this IList list)
        {
            list.RemoveAt(list.Count - 1);
        }

        public static void RemoveAtEnd<T>(this IList<T> list)
        {
            list.RemoveAt(list.Count - 1);
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

        public static GeneralTransform SafeTransformToVisual(this Visual self, Visual visual)
        {
            if (self.FindCommonVisualAncestor(visual) != null)
            {
                return self.TransformToVisual(visual);
            }
            return Transform.Identity;
        }
    }
}
