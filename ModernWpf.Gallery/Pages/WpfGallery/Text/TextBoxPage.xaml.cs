using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public partial class TextBoxPage : Page
    {
        public TextBoxPageViewModel ViewModel { get; }
        public TextBoxPage(TextBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
