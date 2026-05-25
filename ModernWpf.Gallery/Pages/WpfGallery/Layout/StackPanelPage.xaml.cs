using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public partial class StackPanelPage : Page
    {
        public StackPanelPage(StackPanelPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public StackPanelPageViewModel ViewModel { get; }
    }
}
