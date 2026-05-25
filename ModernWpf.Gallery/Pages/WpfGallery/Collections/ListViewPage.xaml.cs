using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public partial class ListViewPage : Page
    {
        public ListViewPageViewModel ViewModel { get; }

        public ListViewPage(ListViewPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
