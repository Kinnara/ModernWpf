using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public partial class RichTextEditPage : Page
    {
        public RichTextEditPageViewModel ViewModel { get; }

        public RichTextEditPage(RichTextEditPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
