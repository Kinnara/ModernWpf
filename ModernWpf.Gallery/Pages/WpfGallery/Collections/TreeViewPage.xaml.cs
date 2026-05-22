using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class TreeViewPage : UserControl
    {
        public TreeViewPage()
        {
            ViewModel = new WpfGalleryCollectionsPageViewModel("TreeView");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryCollectionsPageViewModel ViewModel { get; }
    }
}
