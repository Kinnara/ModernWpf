using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class BorderPage : Page
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
