using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    /// <summary>
    /// Interaction logic for StackPanelPage.xaml
    /// </summary>
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
