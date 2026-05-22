using System.Windows.Controls;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class PasswordBoxPage : UserControl
    {
        public PasswordBoxPage()
        {
            ViewModel = new WpfGalleryPageViewModel("PasswordBox", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }
    }
}
