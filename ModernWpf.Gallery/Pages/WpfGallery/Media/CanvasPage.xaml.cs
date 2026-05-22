using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Media
{
    public sealed partial class CanvasPage : UserControl
    {
        public CanvasPage()
        {
            ViewModel = new CanvasPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public CanvasPageViewModel ViewModel { get; }
    }
}
