using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    /// <summary>
    /// Interaction logic for GridSplitterPage.xaml
    /// </summary>
    public partial class GridSplitterPage : Page
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
