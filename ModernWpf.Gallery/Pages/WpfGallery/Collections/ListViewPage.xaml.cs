using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class ListViewPage : UserControl
    {
        public ListViewPage()
        {
            ViewModel = new ListViewPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ListViewPageViewModel ViewModel { get; }
    }
}
