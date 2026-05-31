using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    /// <summary>
    /// Interaction logic for ListViewPage.xaml
    /// </summary>
    public partial class ListViewPage : Page
    {
        public ListViewPageViewModel ViewModel { get; }

        public ListViewPage(ListViewPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
