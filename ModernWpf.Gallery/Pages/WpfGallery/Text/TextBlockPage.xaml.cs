using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class TextBlockPage : UserControl
    {
        public TextBlockPage()
        {
            ViewModel = new WpfGalleryPageViewModel("TextBlock", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
