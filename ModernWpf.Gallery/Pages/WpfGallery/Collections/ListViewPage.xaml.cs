using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class ListViewPage : Page
    {
        public ListViewPage(ListViewPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ListViewPageViewModel ViewModel { get; }
    }
}
