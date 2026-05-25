using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public partial class TextBoxPage : Page
    {
        public TextBoxPage(TextBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public TextBoxPageViewModel ViewModel { get; }
    }
}
