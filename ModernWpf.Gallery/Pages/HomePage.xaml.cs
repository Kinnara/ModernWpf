using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            NavigateCommand = new GalleryCommand(OnNavigateCard);
            InitializeComponent();
            SetWpfGalleryAutomation();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public Action<GalleryGroup> GroupRequested { get; set; }
        public Action AllControlsRequested { get; set; }
        public ICommand NavigateCommand { get; }

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
