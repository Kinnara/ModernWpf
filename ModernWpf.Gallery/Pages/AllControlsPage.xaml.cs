using System;
using System.Collections.Generic;
using System.Windows.Input;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class AllControlsPage
    {
        public AllControlsPage()
        {
            ViewModel = new AllSamplesPageViewModel(OnNavigateCard);
            InitializeComponent();
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }
        public AllSamplesPageViewModel ViewModel { get; }

        public ICommand NavigateCommand
        {
            get { return ViewModel.NavigateCommand; }
        }

        public string PageTitle
        {
            get { return ViewModel.PageTitle; }
        }

        public string PageDescription
        {
            get { return ViewModel.PageDescription; }
        }

        public IReadOnlyList<GalleryItem> NavigationCards
        {
            get { return ViewModel.NavigationCards; }
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
