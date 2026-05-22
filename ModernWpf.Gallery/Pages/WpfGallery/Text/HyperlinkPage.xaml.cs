using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using ModernWpf.Gallery.Pages.WpfGallery;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class HyperlinkPage : UserControl
    {
        public HyperlinkPage()
        {
            ViewModel = new WpfGalleryPageViewModel("Hyperlink", string.Empty);
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryPageViewModel ViewModel { get; }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
