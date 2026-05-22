using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class ExpanderPage : UserControl
    {
        public ExpanderPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Expander", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
