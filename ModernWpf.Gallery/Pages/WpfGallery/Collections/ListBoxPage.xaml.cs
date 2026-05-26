using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public partial class ListBoxPage : Page
    {
        public ListBoxPageViewModel ViewModel { get; }
        public ListBoxPage(ListBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
