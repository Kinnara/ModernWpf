using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class TreeViewPage : Page
    {
        public TreeViewPage(TreeViewPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public TreeViewPageViewModel ViewModel { get; }
    }
}
