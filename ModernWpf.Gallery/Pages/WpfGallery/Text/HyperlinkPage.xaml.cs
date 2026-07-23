using System.Windows;
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
            NavigationStatusText.Text = "Navigation request: " + e.Uri.AbsoluteUri;
            NavigationStatusText.Visibility = Visibility.Visible;
            e.Handled = true;
        }
    }
}
