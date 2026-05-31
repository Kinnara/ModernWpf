using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
            : this(null)
        {
        }

        public DashboardPage(DashboardPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? new DashboardPageViewModel(OnNavigateCard);
            DataContext = this;
        }

        public DashboardPageViewModel ViewModel { get; }

        public Action<GalleryItem> ItemRequested { get; set; }
        public Action<GalleryGroup> GroupRequested { get; set; }
        public Action AllControlsRequested { get; set; }

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
