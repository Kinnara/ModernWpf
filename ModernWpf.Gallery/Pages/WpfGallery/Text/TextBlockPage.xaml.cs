using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class TextBlockPage : Page
    {
        public TextBlockPage(TextBlockPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public TextBlockPageViewModel ViewModel { get; }
    }
}
