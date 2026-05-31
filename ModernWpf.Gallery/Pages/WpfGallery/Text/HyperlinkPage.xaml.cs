using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    /// <summary>
    /// Interaction logic for HyperlinkPage.xaml
    /// </summary>
    public partial class HyperlinkPage : Page
    {
        public HyperlinkPage(HyperlinkPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public HyperlinkPageViewModel ViewModel { get; }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
