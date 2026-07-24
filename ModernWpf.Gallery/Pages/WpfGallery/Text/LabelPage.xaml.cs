using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    /// <summary>
    /// Interaction logic for LabelPage.xaml
    /// </summary>
    public partial class LabelPage : Page
    {
        public LabelPageViewModel ViewModel { get; }
        public LabelPage(LabelPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
