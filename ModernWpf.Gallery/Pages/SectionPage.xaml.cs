using System;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class SectionPage
    {
        public SectionPage(GalleryGroup group)
        {
            InitializeComponent();
            DataContext = group;
        }

        public Action<GalleryItem> ItemRequested { get; set; }

        private void OnItemCardClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = ((System.Windows.FrameworkElement)sender).DataContext as GalleryItem;
            if (item != null)
            {
                ItemRequested?.Invoke(item);
            }
        }
    }
}
