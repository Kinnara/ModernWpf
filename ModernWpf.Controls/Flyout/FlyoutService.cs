using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public static class FlyoutService
    {
        public static readonly DependencyProperty FlyoutProperty =
            DependencyProperty.RegisterAttached(
                "Flyout",
                typeof(FlyoutBase),
                typeof(FlyoutService),
                new PropertyMetadata(OnFlyoutChanged));

        public static FlyoutBase GetFlyout(Button button)
        {
            return (FlyoutBase)button.GetValue(FlyoutProperty);
        }

        public static void SetFlyout(Button button, FlyoutBase value)
        {
            button.SetValue(FlyoutProperty, value);
        }

        private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;

            if (e.OldValue is FlyoutBase oldFlyout)
            {
                button.Click -= OnButtonClick;
            }

            if (e.NewValue is FlyoutBase newFlyout)
            {
                button.Click += OnButtonClick;
            }
        }

        private static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var flyout = GetFlyout(button);
            if (flyout != null)
            {
                flyout.ShowAt(button);
            }
        }
    }
}
