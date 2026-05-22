using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class TreeViewPage : UserControl
    {
        public TreeViewPage()
        {
            ViewModel = new TreeViewPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public TreeViewPageViewModel ViewModel { get; }
    }
}
