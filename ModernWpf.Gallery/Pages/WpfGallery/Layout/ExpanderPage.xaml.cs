using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class ExpanderPage : Page
    {
        public ExpanderPage(ExpanderPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ExpanderPageViewModel ViewModel { get; }
    }
}
