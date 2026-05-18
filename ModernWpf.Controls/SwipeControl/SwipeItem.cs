using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public partial class SwipeItem : DependencyObject
    {
        public event TypedEventHandler<SwipeItem, SwipeItemInvokedEventArgs> Invoked;

        internal void Invoke(SwipeControl swipeControl)
        {
            Invoked?.Invoke(this, new SwipeItemInvokedEventArgs(swipeControl));

            var command = Command;
            var parameter = CommandParameter;
            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
            }

            if (BehaviorOnInvoked == SwipeBehaviorOnInvoked.Auto ||
                BehaviorOnInvoked == SwipeBehaviorOnInvoked.Close)
            {
                swipeControl.Close();
            }
        }

        internal void GenerateControl(AppBarButton appBarButton, Style swipeItemStyle)
        {
            if (swipeItemStyle != null)
            {
                appBarButton.Style = swipeItemStyle;
            }
            else
            {
                appBarButton.SetResourceReference(FrameworkElement.StyleProperty, "SwipeItemStyle");
            }

            if (Background != null)
            {
                appBarButton.Background = Background;
            }

            if (Foreground != null)
            {
                appBarButton.Foreground = Foreground;
            }

            if (IconSource != null)
            {
                appBarButton.Icon = IconSource.CreateIconElement();
            }

            appBarButton.Label = Text;
            AutomationProperties.SetName(appBarButton, Text ?? string.Empty);
            AttachEventHandlers(appBarButton);
        }

        private void AttachEventHandlers(AppBarButton appBarButton)
        {
            appBarButton.Click += OnButtonClick;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var swipeControl = FindAncestorSwipeControl(sender as DependencyObject);
            if (swipeControl != null)
            {
                Invoke(swipeControl);
                e.Handled = true;
            }
        }

        private static SwipeControl FindAncestorSwipeControl(DependencyObject source)
        {
            while (source != null)
            {
                if (source is SwipeControl swipeControl)
                {
                    return swipeControl;
                }

                var parent = VisualTreeHelper.GetParent(source);
                source = parent ?? LogicalTreeHelper.GetParent(source);
            }

            return null;
        }
    }
}
