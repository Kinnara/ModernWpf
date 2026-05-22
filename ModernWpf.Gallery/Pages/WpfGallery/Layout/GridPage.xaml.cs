using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class GridPage : UserControl
    {
        public GridPage()
        {
            ViewModel = new GridPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public GridPageViewModel ViewModel { get; }
    }
}
