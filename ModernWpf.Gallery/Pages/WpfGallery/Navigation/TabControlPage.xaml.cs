using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public partial class TabControlPage : Page
    {
        public TabControlPageViewModel ViewModel { get; }

        public TabControlPage(TabControlPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
