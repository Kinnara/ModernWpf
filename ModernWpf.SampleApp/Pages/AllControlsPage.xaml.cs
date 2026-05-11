using System;
using System.ComponentModel;
using ModernWpf.SampleApp.Models;

namespace ModernWpf.SampleApp.Pages
{
    public sealed partial class AllControlsPage : INotifyPropertyChanged
    {
        private object _filteredItems;

        public AllControlsPage()
        {
            InitializeComponent();
            FilteredItems = GalleryCatalog.Items;
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

        private void OnFilterTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FilteredItems = GalleryCatalog.Search(FilterBox.Text);
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
