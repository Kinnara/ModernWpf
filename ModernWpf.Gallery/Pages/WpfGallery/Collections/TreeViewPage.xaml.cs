using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public partial class TreeViewPage : Page
    {
        public TreeViewPageViewModel ViewModel { get; }

        public TreeViewPage(TreeViewPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
