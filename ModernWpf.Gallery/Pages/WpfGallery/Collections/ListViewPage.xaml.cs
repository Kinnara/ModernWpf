using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class ListViewPage : UserControl
    {
        public ListViewPage()
        {
            ViewModel = new WpfGalleryCollectionsPageViewModel("ListView");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryCollectionsPageViewModel ViewModel { get; }
    }
}
