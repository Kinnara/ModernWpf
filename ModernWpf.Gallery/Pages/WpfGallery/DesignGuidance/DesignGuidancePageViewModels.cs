using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public partial class ColorsPageViewModel : WpfGalleryPageViewModel
    {
        public ColorsPageViewModel()
            : base("Colors", "Guide showing how to use colors in your app")
        {
        }
    }

    public partial class TypographyPageViewModel : WpfGalleryPageViewModel
    {
        public TypographyPageViewModel()
            : base("Typography", "Guide showing how to use typography in your app")
        {
        }
    }

    public partial class SpacingPageViewModel : WpfGalleryPageViewModel
    {
        public SpacingPageViewModel()
            : base("Spacing", "Guide showing how to use spacing in your app")
        {
        }
    }

    public partial class GeometryPageViewModel : WpfGalleryPageViewModel
    {
        public GeometryPageViewModel()
            : base("Geometry", string.Empty)
        {
        }
    }
}
