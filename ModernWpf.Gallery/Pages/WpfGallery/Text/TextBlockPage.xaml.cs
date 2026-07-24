using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    /// <summary>
    /// Interaction logic for TextBlockPage.xaml
    /// </summary>
    public partial class TextBlockPage : Page
    {
        public TextBlockPageViewModel ViewModel { get; }
        public TextBlockPage(TextBlockPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
