using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class ListBoxPage : Page
    {
        public ListBoxPage(ListBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ListBoxPageViewModel ViewModel { get; }
    }
}
