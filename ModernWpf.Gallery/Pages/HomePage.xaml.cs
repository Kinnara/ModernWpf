using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            ViewModel = new WpfGalleryDashboardPageViewModel(OnNavigateCard);
            InitializeComponent();
            SetWpfGalleryAutomation();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public Action<GalleryGroup> GroupRequested { get; set; }
        public Action AllControlsRequested { get; set; }
        public WpfGalleryDashboardPageViewModel ViewModel { get; }

        public ICommand NavigateCommand
        {
            get { return ViewModel.NavigateCommand; }
        }

        public IReadOnlyList<GalleryGroup> NavigationCards
        {
            get { return ViewModel.NavigationCards; }
        }

        public IReadOnlyList<GalleryItem> RecentlyAddedOrUpdatedSamplesInfo
        {
            get { return ViewModel.RecentlyAddedOrUpdatedSamplesInfo; }
        }

        public IReadOnlyList<GalleryItem> FeaturedItems
        {
            get { return RecentlyAddedOrUpdatedSamplesInfo; }
        }

        public IReadOnlyList<GalleryGroup> Groups
        {
            get { return NavigationCards; }
        }

        private void SetWpfGalleryAutomation()
        {
            GalleryAutomation.SetHeadingLevel(HeroVersionText, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(HeroTitleText, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(OverviewHeaderText, GalleryAutomationHeadingLevel.Level2);
            GalleryAutomation.SetHeadingLevel(RecentlyAddedHeaderText, GalleryAutomationHeadingLevel.Level2);
        }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is GalleryItem item)
            {
                ItemRequested?.Invoke(item);
            }
            else if (parameter is GalleryGroup group)
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
