using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class BorderPage : UserControl
    {
        public BorderPage()
        {
            ViewModel = new BorderPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public BorderPageViewModel ViewModel { get; }
    }
}
