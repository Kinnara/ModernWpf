using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance
{
    public sealed partial class TypographyPage : UserControl
    {
        public TypographyPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Typography", "Guide showing how to use typography in your app");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
