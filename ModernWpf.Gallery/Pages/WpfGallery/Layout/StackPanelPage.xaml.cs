using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Layout
{
    public sealed partial class StackPanelPage : UserControl
    {
        public StackPanelPage()
        {
            ViewModel = new StackPanelPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public StackPanelPageViewModel ViewModel { get; }
    }
}
