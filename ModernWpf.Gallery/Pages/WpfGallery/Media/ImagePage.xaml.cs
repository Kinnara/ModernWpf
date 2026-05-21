using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public sealed partial class ImagePage : UserControl
    {
        public ImagePage()
        {
            ViewModel = new WpfGalleryPageViewModel("Image", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
