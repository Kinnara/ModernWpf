using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public partial class ImagePage : Page
    {
        public ImagePageViewModel ViewModel { get; }

        public ImagePage(ImagePageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
