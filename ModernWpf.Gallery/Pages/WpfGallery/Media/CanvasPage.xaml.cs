using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public sealed partial class CanvasPage : UserControl
    {
        public CanvasPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Canvas", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
