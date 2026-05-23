using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GridSplitterPage : Page
    {
        public GridSplitterPage(GridSplitterPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public GridSplitterPageViewModel ViewModel { get; }
    }
}
