using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    public partial class ToolTipPage : Page
    {
        public ToolTipPageViewModel ViewModel { get; }

        public ToolTipPage(ToolTipPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
