using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// Interaction logic for AllSamplesPage.xaml
    /// </summary>
    public partial class AllSamplesPage : Page
    {
        public AllSamplesPageViewModel ViewModel { get; }

        public AllSamplesPage()
            : this(null)
        {
        }

        public AllSamplesPage(AllSamplesPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? new AllSamplesPageViewModel(OnNavigateCard);
            DataContext = this;
        }

        public Action<GalleryItem> ItemRequested { get; set; }

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
