using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class DataGridPage : UserControl
    {
        public DataGridPage()
        {
            ViewModel = new DataGridPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public DataGridPageViewModel ViewModel { get; }
    }
}
