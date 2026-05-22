using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public sealed partial class ImagePage : UserControl
    {
        public ImagePage()
        {
            ViewModel = new ImagePageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ImagePageViewModel ViewModel { get; }
    }
}
