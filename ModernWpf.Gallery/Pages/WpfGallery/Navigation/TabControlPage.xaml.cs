using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class TabControlPage : UserControl
    {
        public TabControlPage()
        {
            ViewModel = new WpfGalleryPageViewModel("TabControl", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
