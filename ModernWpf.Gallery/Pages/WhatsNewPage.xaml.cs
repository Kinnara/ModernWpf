using System;
using System.Windows.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// Interaction logic for WhatsNewPage.xaml
    /// </summary>
    public partial class WhatsNewPage : Page
    {
        public WhatsNewPageViewModel ViewModel { get; }

        public WhatsNewPage()
            : this(null)
        {
        }

        public WhatsNewPage(WhatsNewPageViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? new WhatsNewPageViewModel(OnNavigateCard);
            DataContext = this;
        }

        public Action<string> ItemRequested { get; set; }

        private void OnNavigateCard(object parameter)
        {
            if (parameter is GalleryItem item)
            {
                ItemRequested?.Invoke(item.UniqueId);
            }
            else if (parameter is string uniqueId)
            {
                ItemRequested?.Invoke(uniqueId);
            }
        }
    }
}
