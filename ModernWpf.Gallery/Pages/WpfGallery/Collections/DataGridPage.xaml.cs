using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class DataGridPage : UserControl
    {
        public DataGridPage()
        {
            ViewModel = new WpfGalleryCollectionsPageViewModel("DataGrid");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryCollectionsPageViewModel ViewModel { get; }
    }
}
