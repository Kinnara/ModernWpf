using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class SpacingPage : UserControl
    {
        public SpacingPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Spacing", "Guide showing how to use spacing in your app");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
