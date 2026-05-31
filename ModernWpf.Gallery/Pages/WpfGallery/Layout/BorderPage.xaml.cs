using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    /// <summary>
    /// Interaction logic for BorderPage.xaml
    /// </summary>
    public partial class BorderPage : Page
    {
        public BorderPage(BorderPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public BorderPageViewModel ViewModel { get; }
    }
}
