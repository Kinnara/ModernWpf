using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.Navigation
{
    public sealed partial class TabControlPage : UserControl
    {
        public TabControlPage()
        {
            ViewModel = new TabControlPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public TabControlPageViewModel ViewModel { get; }
    }
}
