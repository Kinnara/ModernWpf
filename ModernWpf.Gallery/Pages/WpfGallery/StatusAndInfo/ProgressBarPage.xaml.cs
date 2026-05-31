using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    /// <summary>
    /// Interaction logic for ProgressBarPage.xaml
    /// </summary>
    public partial class ProgressBarPage : Page
    {
        public ProgressBarPageViewModel ViewModel { get; }

        public ProgressBarPage(ProgressBarPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
