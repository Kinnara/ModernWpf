using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GroupBoxPage : UserControl
    {
        public GroupBoxPage()
        {
            ViewModel = new WpfGalleryPageViewModel("GroupBox", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
