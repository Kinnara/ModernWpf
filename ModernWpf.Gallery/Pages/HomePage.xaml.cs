using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
            SetWpfGalleryAutomation();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public Action<GalleryGroup> GroupRequested { get; set; }
        public Action AllControlsRequested { get; set; }

        public object FeaturedItems
        {
            get { return GalleryCatalog.NewOrUpdatedItems; }
        }

        public object Groups
        {
            get { return GalleryCatalog.OverviewGroups; }
        }

        private void SetWpfGalleryAutomation()
        {
            GalleryAutomation.SetHeadingLevel(HeroVersionText, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(HeroTitleText, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(OverviewHeaderText, GalleryAutomationHeadingLevel.Level2);
            GalleryAutomation.SetHeadingLevel(RecentlyAddedHeaderText, GalleryAutomationHeadingLevel.Level2);
        }

        private void OnScrollBackButtonClick(object sender, RoutedEventArgs e)
        {
            var newOffset = RootScrollViewer.HorizontalOffset - 210;
            RootScrollViewer.ScrollToHorizontalOffset(newOffset);
            UpdateScrollButtonsVisibility(newOffset);
        }

        private void OnScrollForwardButtonClick(object sender, RoutedEventArgs e)
        {
            var newOffset = RootScrollViewer.HorizontalOffset + 210;
            RootScrollViewer.ScrollToHorizontalOffset(newOffset);
            UpdateScrollButtonsVisibility(newOffset);
        }

        private void OnRootScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollButtonsVisibility();
        }

        private void UpdateScrollButtonsVisibility()
        {
            UpdateScrollButtonsVisibility(RootScrollViewer.HorizontalOffset);
        }

        private void UpdateScrollButtonsVisibility(double newOffset)
        {
            ScrollBackButton.Visibility = Visibility.Visible;
            ScrollForwardButton.Visibility = Visibility.Visible;

            if (RootScrollViewer.ActualWidth < TilesPanel.ActualWidth)
            {
                if (newOffset <= 0)
                {
                    ScrollBackButton.Visibility = Visibility.Collapsed;
                }
                else if (newOffset >= RootScrollViewer.ScrollableWidth)
                {
                    ScrollForwardButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ScrollBackButton.Visibility = Visibility.Collapsed;
                ScrollForwardButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnHeaderTileClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uri = button == null ? null : button.Tag as string;
            if (!string.IsNullOrEmpty(uri))
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            }
        }

        private void OnItemCardClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = ((System.Windows.FrameworkElement)sender).DataContext as GalleryItem;
            if (item != null)
            {
                ItemRequested?.Invoke(item);
            }
        }

        private void OnGroupCardClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var group = ((System.Windows.FrameworkElement)sender).DataContext as GalleryGroup;
            if (group != null)
            {
                GroupRequested?.Invoke(group);
            }
        }

        private void OnAllControlsClick(object sender, System.Windows.RoutedEventArgs e)
        {
            AllControlsRequested?.Invoke();
        }
    }
}
