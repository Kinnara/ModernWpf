using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class LabelPage : Page
    {
        public LabelPage(LabelPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public LabelPageViewModel ViewModel { get; }
    }
}
