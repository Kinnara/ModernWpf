using System;
using ModernWpf.SampleApp.Models;

namespace ModernWpf.SampleApp.Pages
{
    public sealed partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
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
            get { return GalleryCatalog.Groups; }
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
