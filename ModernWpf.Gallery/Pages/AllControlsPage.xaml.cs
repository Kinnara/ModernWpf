using System;
using System.ComponentModel;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class AllControlsPage : INotifyPropertyChanged
    {
        private object _filteredItems;

        public AllControlsPage()
        {
            InitializeComponent();
            FilteredItems = GalleryCatalog.AllControlsItems;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Action<GalleryItem> ItemRequested { get; set; }

        public object FilteredItems
        {
            get { return _filteredItems; }
            private set
            {
                _filteredItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteredItems)));
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
    }
}
