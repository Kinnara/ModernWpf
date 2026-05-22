using System;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class AllControlsPage : INotifyPropertyChanged
    {
        private object _filteredItems;

        public AllControlsPage()
        {
            NavigateCommand = new GalleryCommand(OnNavigateCard);
            InitializeComponent();
            FilteredItems = GalleryCatalog.AllControlsItems;
            DataContext = this;
            AutomationProperties.SetName(TitleLabel, "All Controls Page");
            GalleryAutomation.SetHeadingLevel(TitleLabel, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(DescriptionLabel, GalleryAutomationHeadingLevel.Level2);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Action<GalleryItem> ItemRequested { get; set; }
        public ICommand NavigateCommand { get; }

        public object FilteredItems
        {
            get { return _filteredItems; }
            private set
            {
                _filteredItems = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteredItems)));
            }
        }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is GalleryItem item)
            {
                ItemRequested?.Invoke(item);
            }
        }
    }
}
