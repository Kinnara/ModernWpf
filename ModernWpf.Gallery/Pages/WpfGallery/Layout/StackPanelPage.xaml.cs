using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class StackPanelPage : UserControl
    {
        public StackPanelPage()
        {
            ViewModel = new WpfGalleryPageViewModel("StackPanel", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
