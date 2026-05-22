using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class LabelPage : UserControl
    {
        public LabelPage()
        {
            ViewModel = new LabelPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public LabelPageViewModel ViewModel { get; }
    }
}
