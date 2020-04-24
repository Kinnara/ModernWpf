using ModernWpf.Controls;
using ModernWpf.MahApps.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace MahAppsSample
{
    public partial class MainWindow : Window
    {
        private static readonly Uri SettingsUri = new Uri("SettingsPage.xaml", UriKind.Relative);

        public MainWindow()
        {
            InitializeComponent();

            if (App.RepositoryImageMode)
            {
                Width = 1280;
                Height = 640;
                NavView.ClearValue(PaddingProperty);
                NavView.ClearValue(HamburgerMenuEx.HeaderTemplateProperty);
                NavView.MenuItems.RemoveAt(NavView.MenuItems.Count - 1);
            }

            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().First();
            Navigate(NavView.SelectedItem);

            Loaded += delegate
            {
                UpdateAppTitle();
            };
        }

        void UpdateAppTitle()
        {
            //ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, TitleBar.GetSystemOverlayRightInset(this), currMargin.Bottom);
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            ContentFrame.GoBack();
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                Navigate(SettingsUri);
            }
            else
            {
                Navigate(args.InvokedItemContainer);
            }
        }

        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness((sender.CompactPaneLength * 2), currMargin.Top, currMargin.Right, currMargin.Bottom);

            }
            else
            {
                AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }

            UpdateAppTitleMargin(sender);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri == SettingsUri)
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else
            {
                NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => GetNavigateUri(x) == e.Uri);
            }
        }

        private void UpdateAppTitleMargin(NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            Thickness currMargin = AppTitle.Margin;

            if ((sender.DisplayMode == NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                     sender.DisplayMode == NavigationViewDisplayMode.Minimal)
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
            if (item is NavigationViewItem menuItem)
            {
                Uri navigateUri = GetNavigateUri(menuItem);
                if (ContentFrame.CurrentSource != navigateUri)
                {
                    ContentFrame.Navigate(navigateUri);
                }
            }
        }

        private void Navigate(Uri source)
        {
            if (ContentFrame.CurrentSource != source)
            {
                ContentFrame.Navigate(source);
            }
        }

        private Uri GetNavigateUri(NavigationViewItem item)
        {
            if (item.Tag is Uri uri)
            {
                return uri;
            }
            return new Uri((string)item.Tag, UriKind.Relative);
        }
    }
}
