using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Collections
{
    public sealed partial class ListBoxPage : UserControl
    {
        public ListBoxPage()
        {
            ViewModel = new ListBoxPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ListBoxPageViewModel ViewModel { get; }
    }
}
