using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public sealed class CanvasPageViewModel : WpfGalleryPageViewModel
    {
        public CanvasPageViewModel()
            : base("Canvas", string.Empty)
        {
        }
    }

    public sealed class ImagePageViewModel : WpfGalleryPageViewModel
    {
        public ImagePageViewModel()
            : base("Image", string.Empty)
        {
        }
    }
}
