using MahApps.Metro.Controls;
using ModernWpf;
using ModernWpf.MahApps.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace MahAppsInteropSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateAppTitleBarMargin(NavView);

            NavView.SelectedItem = NavViewItems[0];
            Navigate(NavView.SelectedItem);
        }

        private void NavView_BackRequested(object sender, HamburgerMenuBackRequestedEventArgs e)
        {
            ContentFrame.GoBack();
        }

        private void NavView_ItemInvoked(object sender, HamburgerMenuItemInvokedEventArgs e)
        {
            if (e.IsItemOptions)
            {
                Navigate(NavView.SelectedOptionsItem);
            }
            else
            {
                Navigate(NavView.SelectedItem);
            }
        }

        private void NavView_PaneOpened(object sender, EventArgs e)
        {
            UpdateAppTitleBarMargin((HamburgerMenuEx)sender);
        }

        private void NavView_PaneClosed(object sender, EventArgs e)
        {
            UpdateAppTitleBarMargin((HamburgerMenuEx)sender);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.SelectedItem = NavViewItems.FirstOrDefault(x => (string)x.Tag == e.Uri.ToString());
            NavView.SelectedOptionsItem = NavViewOptions.FirstOrDefault(x => (string)x.Tag == e.Uri.ToString());

            var selectedItem = NavView.SelectedItem ?? NavView.SelectedOptionsItem;
            if (selectedItem is HamburgerMenuItem item)
            {
                NavView.Header = item.Label;
            }
        }

        private void UpdateAppTitleBarMargin(HamburgerMenuEx sender)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);

            UpdateAppTitleMargin(sender);
        }

        private void UpdateAppTitleMargin(HamburgerMenuEx sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            Thickness currMargin = AppTitle.Margin;

            if (sender.IsPaneOpen)
            {
                AppTitle.Margin = new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitle.Margin = new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        private void Navigate(object item)
        {
            if (item is HamburgerMenuItem menuItem)
            {
                string source = (string)menuItem.Tag;
                if (ContentFrame.CurrentSource?.ToString() != source)
                {
                    ContentFrame.Navigate(new Uri(source, UriKind.Relative));
                }
            }
        }

        private void InvertTheme(object sender, RoutedEventArgs e)
        {
            var tm = ThemeManager.Current;
            if (tm.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                tm.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                tm.ApplicationTheme = ApplicationTheme.Dark;
            }
        }
    }
}
