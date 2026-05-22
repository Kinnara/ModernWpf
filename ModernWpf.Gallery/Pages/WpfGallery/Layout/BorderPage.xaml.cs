using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class BorderPage : UserControl
    {
        public BorderPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Border", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
