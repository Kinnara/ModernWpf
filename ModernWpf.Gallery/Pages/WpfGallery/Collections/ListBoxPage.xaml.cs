using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class ListBoxPage : UserControl
    {
        public ListBoxPage()
        {
            ViewModel = new WpfGalleryCollectionsPageViewModel("ListBox");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryCollectionsPageViewModel ViewModel { get; }
    }
}
