using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    public sealed partial class ToolTipPage : UserControl
    {
        public ToolTipPage()
        {
            ViewModel = new WpfGalleryPageViewModel("ToolTip", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
