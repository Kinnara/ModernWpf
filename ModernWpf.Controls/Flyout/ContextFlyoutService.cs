using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public static class ContextFlyoutService
    {
        #region ContextFlyout

        public static readonly DependencyProperty ContextFlyoutProperty =
            DependencyProperty.RegisterAttached(
                "ContextFlyout",
                typeof(FlyoutBase),
                typeof(ContextFlyoutService),
                new PropertyMetadata(OnContextFlyoutChanged));

        public static FlyoutBase GetContextFlyout(FrameworkElement element)
        {
            return (FlyoutBase)element.GetValue(ContextFlyoutProperty);
        }

        public static void SetContextFlyout(FrameworkElement element, FlyoutBase value)
        {
            element.SetValue(ContextFlyoutProperty, value);
        }

        private static void OnContextFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            var oldValue = (FlyoutBase)e.OldValue;
            var newValue = (FlyoutBase)e.NewValue;

            if (oldValue != null)
            {
                element.ContextMenuOpening -= OnContextMenuOpening;
            }

            if (newValue != null)
            {
                element.ContextMenuOpening += OnContextMenuOpening;
            }
        }

        #endregion

        private static void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var flyout = GetContextFlyout(element);
            if (flyout != null)
            {
                e.Handled = true;
                flyout.ShowAsContextFlyout(element);
            }
        }
    }
}
