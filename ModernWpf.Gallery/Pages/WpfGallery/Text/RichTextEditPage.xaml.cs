using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class RichTextEditPage : UserControl
    {
        public RichTextEditPage()
        {
            ViewModel = new WpfGalleryPageViewModel("RichTextEdit", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
