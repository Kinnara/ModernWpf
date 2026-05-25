using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    public partial class ProgressBarPage : Page
    {
        public ProgressBarPage(ProgressBarPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ProgressBarPageViewModel ViewModel { get; }
    }
}
