using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GridSplitterPage : UserControl
    {
        public GridSplitterPage()
        {
            ViewModel = new WpfGalleryPageViewModel("GridSplitter", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
