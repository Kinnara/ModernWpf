using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public class ColorsPageViewModel : WpfGalleryPageViewModel
    {
        public ColorsPageViewModel()
            : base("Colors", "Guide showing how to use colors in your app")
        {
        }
    }

    public class TypographyPageViewModel : WpfGalleryPageViewModel
    {
        public TypographyPageViewModel()
            : base("Typography", "Guide showing how to use typography in your app")
        {
        }
    }

    public class SpacingPageViewModel : WpfGalleryPageViewModel
    {
        public SpacingPageViewModel()
            : base("Spacing", "Guide showing how to use spacing in your app")
        {
        }
    }

    public class GeometryPageViewModel : WpfGalleryPageViewModel
    {
        public GeometryPageViewModel()
            : base("Geometry", string.Empty)
        {
        }
    }
}
