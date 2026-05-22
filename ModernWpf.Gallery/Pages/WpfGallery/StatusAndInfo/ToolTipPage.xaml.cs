using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo
{
    public sealed partial class ToolTipPage : UserControl
    {
        public ToolTipPage()
        {
            ViewModel = new ToolTipPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ToolTipPageViewModel ViewModel { get; }
    }
}
