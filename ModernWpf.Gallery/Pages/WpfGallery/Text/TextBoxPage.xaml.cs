using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    /// <summary>
    /// Interaction logic for TextBoxPage.xaml
    /// </summary>
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
