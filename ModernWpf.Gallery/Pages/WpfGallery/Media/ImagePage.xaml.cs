using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public sealed partial class ImagePage : Page
    {
        public ImagePage(ImagePageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ImagePageViewModel ViewModel { get; }
    }
}
