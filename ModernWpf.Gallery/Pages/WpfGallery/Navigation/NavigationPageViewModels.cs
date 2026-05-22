using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed class MenuPageViewModel : WpfGalleryPageViewModel
    {
        public MenuPageViewModel()
            : base("Menu", string.Empty)
        {
        }
    }

    public sealed class TabControlPageViewModel : WpfGalleryPageViewModel
    {
        public TabControlPageViewModel()
            : base("TabControl", string.Empty)
        {
        }
    }

    public sealed class FramePageViewModel : WpfGalleryPageViewModel
    {
        public FramePageViewModel()
            : base("Frame", string.Empty)
        {
        }
    }

    public sealed class NavigationWindowPageViewModel : WpfGalleryPageViewModel
    {
        public NavigationWindowPageViewModel()
            : base("Navigation Window", string.Empty)
        {
        }
    }
}
