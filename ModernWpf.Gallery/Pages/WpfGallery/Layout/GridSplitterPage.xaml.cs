using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GridSplitterPage : UserControl
    {
        public GridSplitterPage()
        {
            ViewModel = new GridSplitterPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public GridSplitterPageViewModel ViewModel { get; }
    }
}
