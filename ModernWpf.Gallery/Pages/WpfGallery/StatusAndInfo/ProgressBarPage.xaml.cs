using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    public sealed partial class ProgressBarPage : UserControl
    {
        public ProgressBarPage()
        {
            ViewModel = new WpfGalleryPageViewModel("ProgressBar", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
