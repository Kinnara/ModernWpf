using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public partial class CanvasPageViewModel : WpfGalleryPageViewModel
    {
        public CanvasPageViewModel()
            : base("Canvas", string.Empty)
        {
        }
    }

    public partial class ImagePageViewModel : WpfGalleryPageViewModel
    {
        public ImagePageViewModel()
            : base("Image", string.Empty)
        {
        }
    }
}
