using System;
using System.Windows.Input;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class AllControlsPage
    {
        public AllControlsPage()
        {
            NavigateCommand = new GalleryCommand(OnNavigateCard);
            InitializeComponent();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public ICommand NavigateCommand { get; }

        public string PageTitle
        {
            get { return "All Controls"; }
        }

        public string PageDescription
        {
            get { return string.Empty; }
        }

        public object NavigationCards
        {
            get { return GalleryCatalog.AllControlsItems; }
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
