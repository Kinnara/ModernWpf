using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    public sealed partial class ProgressBarPage : UserControl
    {
        public ProgressBarPage()
        {
            ViewModel = new ProgressBarPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ProgressBarPageViewModel ViewModel { get; }
    }
}
