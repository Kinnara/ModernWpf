using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Samples
{
    public partial class UserDashboardPage : Page
    {
        public UserDashboardPage(UserDashboardPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = this;
        }

        public UserDashboardPageViewModel ViewModel { get; }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Command;
            var commandParameter = (sender as Button)?.CommandParameter;
            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }

            save_button.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Command;
            var commandParameter = (sender as Button)?.CommandParameter;

            var currentUserName = ViewModel.EditableUser?.Name ?? string.Empty;

            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }

            RaiseNotification(sender as Button, $"User {currentUserName} saved", "ButtonClickedActivity");
            edit_button.Focus();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Command;
            var commandParameter = (sender as Button)?.CommandParameter;

            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }

            RaiseNotification(sender as Button, $"User {ViewModel.DeletedName} deleted", "ButtonClickedActivity");
            edit_button.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var command = (sender as Button)?.Command;
            var commandParameter = (sender as Button)?.CommandParameter;
            if (command != null && command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }

            edit_button.Focus();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 600)
            {
                Grid_SetProperties(UserListGrid, 0, 0, 1, 2);
                Grid_SetProperties(UserDetailsGrid, 1, 0, 1, 2);
                UserDetailsGrid.Margin = new Thickness(0);
                UserDetailFormGrid.Margin = new Thickness(20, 10, 20, 10);
                UserList.ClearValue(WidthProperty);
                NewUserButton.HorizontalAlignment = HorizontalAlignment.Right;
                Grid_SetProperties(FirstNamePanel, 0, 0, 2, 1);
                Grid_SetProperties(LastNamePanel, 0, 1, 2, 1);
                FirstNamePanel.Margin = new Thickness(0, 0, 10, 0);
                UserDetailHeader.Orientation = Orientation.Horizontal;
                UserDetailHeaderPanel.HorizontalAlignment = HorizontalAlignment.Left;
                UserDetailHeaderNameBox.HorizontalAlignment = HorizontalAlignment.Left;
                UserDetailHeaderCompanyBox.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if (e.NewSize.Width >= 400 && e.NewSize.Width < 800)
            {
                Grid_SetProperties(UserListGrid, 0, 0, 2, 1);
                Grid_SetProperties(UserDetailsGrid, 0, 1, 2, 1);
                UserDetailsGrid.Margin = new Thickness(-10, 0, -20, 0);
                UserDetailFormGrid.Margin = new Thickness(0, 10, 0, 10);
                UserList.Width = 240;
                NewUserButton.HorizontalAlignment = HorizontalAlignment.Center;
                Grid_SetProperties(FirstNamePanel, 0, 0, 1, 2);
                Grid_SetProperties(LastNamePanel, 1, 0, 1, 2);
                FirstNamePanel.Margin = new Thickness(0);
                UserDetailHeader.Orientation = Orientation.Vertical;
                UserDetailHeaderPanel.HorizontalAlignment = HorizontalAlignment.Center;
                UserDetailHeaderNameBox.HorizontalAlignment = HorizontalAlignment.Center;
                UserDetailHeaderCompanyBox.HorizontalAlignment = HorizontalAlignment.Center;
            }
            else
            {
                Grid_SetProperties(UserListGrid, 0, 0, 2, 1);
                Grid_SetProperties(UserDetailsGrid, 0, 1, 2, 1);
                UserDetailsGrid.Margin = new Thickness(0);
                UserDetailFormGrid.Margin = new Thickness(20, 10, 20, 10);
                UserList.Width = 300;
                NewUserButton.HorizontalAlignment = HorizontalAlignment.Center;
                Grid_SetProperties(FirstNamePanel, 0, 0, 2, 1);
                Grid_SetProperties(LastNamePanel, 0, 1, 2, 1);
                FirstNamePanel.Margin = new Thickness(0, 0, 10, 0);
                UserDetailHeader.Orientation = Orientation.Horizontal;
                UserDetailHeaderPanel.HorizontalAlignment = HorizontalAlignment.Left;
                UserDetailHeaderNameBox.HorizontalAlignment = HorizontalAlignment.Left;
                UserDetailHeaderCompanyBox.HorizontalAlignment = HorizontalAlignment.Left;
            }
        }

        private void Grid_SetProperties(UIElement uiElement, int row, int column, int rowSpan, int columnSpan)
        {
            Grid.SetRow(uiElement, row);
            Grid.SetColumn(uiElement, column);
            Grid.SetRowSpan(uiElement, rowSpan);
            Grid.SetColumnSpan(uiElement, columnSpan);
        }

        private void AgeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(sender is Slider slider))
            {
                return;
            }

            var newAge = (int)e.NewValue;

            RaiseNotification(slider, $"New age {newAge}", "SliderValueChangedActivity");
        }

        private static void RaiseNotification(UIElement element, string notification, string activityId)
        {
#if NET8_0_OR_GREATER
            if (element == null)
            {
                return;
            }

            var peer = UIElementAutomationPeer.CreatePeerForElement(element);
            if (peer == null)
            {
                return;
            }

            peer.RaiseNotificationEvent(
                AutomationNotificationKind.Other,
                AutomationNotificationProcessing.ImportantMostRecent,
                notification,
                activityId);
#endif
        }
    }
}
