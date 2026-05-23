using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GridPage : Page
    {
        public GridPage(GridPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public GridPageViewModel ViewModel { get; }
    }
}
