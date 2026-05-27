using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public partial class MenuPageViewModel : WpfGalleryPageViewModel
    {
        public MenuPageViewModel()
            : base("Menu", string.Empty)
        {
        }
    }

    public partial class TabControlPageViewModel : WpfGalleryPageViewModel
    {
        public TabControlPageViewModel()
            : base("TabControl", string.Empty)
        {
        }
    }

    public partial class FramePageViewModel : WpfGalleryPageViewModel
    {
        public FramePageViewModel()
            : base("Frame", string.Empty)
        {
        }
    }

    public partial class NavigationWindowPageViewModel : WpfGalleryPageViewModel
    {
        public NavigationWindowPageViewModel()
            : base("Navigation Window", string.Empty)
        {
        }
    }
}
