using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public partial class CanvasPage : Page
    {
        public CanvasPage(CanvasPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public CanvasPageViewModel ViewModel { get; }
    }
}
