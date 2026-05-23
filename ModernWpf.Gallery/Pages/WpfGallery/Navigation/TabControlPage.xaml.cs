using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class TabControlPage : Page
    {
        public TabControlPage(TabControlPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public TabControlPageViewModel ViewModel { get; }
    }
}
