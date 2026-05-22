using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed class BorderPageViewModel : WpfGalleryPageViewModel
    {
        public BorderPageViewModel()
            : base("Border", string.Empty)
        {
        }
    }

    public sealed class ExpanderPageViewModel : WpfGalleryPageViewModel
    {
        public ExpanderPageViewModel()
            : base("Expander", string.Empty)
        {
        }
    }

    public sealed class GridPageViewModel : WpfGalleryPageViewModel
    {
        public GridPageViewModel()
            : base("Grid", string.Empty)
        {
        }
    }

    public sealed class GridSplitterPageViewModel : WpfGalleryPageViewModel
    {
        public GridSplitterPageViewModel()
            : base("GridSplitter", string.Empty)
        {
        }
    }

    public sealed class GroupBoxPageViewModel : WpfGalleryPageViewModel
    {
        public GroupBoxPageViewModel()
            : base("GroupBox", string.Empty)
        {
        }
    }

    public sealed class ResizeGripPageViewModel : WpfGalleryPageViewModel
    {
        public ResizeGripPageViewModel()
            : base("ResizeGrip", string.Empty)
        {
        }
    }

    public sealed class StackPanelPageViewModel : WpfGalleryPageViewModel
    {
        public StackPanelPageViewModel()
            : base("StackPanel", string.Empty)
        {
        }
    }
}
