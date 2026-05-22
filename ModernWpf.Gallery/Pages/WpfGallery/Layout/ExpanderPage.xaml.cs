using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class ExpanderPage : UserControl
    {
        public ExpanderPage()
        {
            ViewModel = new ExpanderPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ExpanderPageViewModel ViewModel { get; }
    }
}
