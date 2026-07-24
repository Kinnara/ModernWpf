using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public partial class MenuPageViewModel : WpfGalleryPageViewModel
    {
        public MenuPageViewModel()
            : base("Menu", "")
        {
        }
    }

    public partial class TabControlPageViewModel : WpfGalleryPageViewModel
    {
        public TabControlPageViewModel()
            : base("TabControl", "")
        {
        }
    }

    public partial class FramePageViewModel : WpfGalleryPageViewModel
    {
        public FramePageViewModel()
            : base("Frame", "")
        {
        }
    }

    public partial class NavigationWindowPageViewModel : WpfGalleryPageViewModel
    {
        public NavigationWindowPageViewModel()
            : base("Navigation Window", "")
        {
        }
    }
}
