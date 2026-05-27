using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public class MenuPageViewModel : WpfGalleryPageViewModel
    {
        public MenuPageViewModel()
            : base("Menu", string.Empty)
        {
        }
    }

    public class TabControlPageViewModel : WpfGalleryPageViewModel
    {
        public TabControlPageViewModel()
            : base("TabControl", string.Empty)
        {
        }
    }

    public class FramePageViewModel : WpfGalleryPageViewModel
    {
        public FramePageViewModel()
            : base("Frame", string.Empty)
        {
        }
    }

    public class NavigationWindowPageViewModel : WpfGalleryPageViewModel
    {
        public NavigationWindowPageViewModel()
            : base("Navigation Window", string.Empty)
        {
        }
    }
}
