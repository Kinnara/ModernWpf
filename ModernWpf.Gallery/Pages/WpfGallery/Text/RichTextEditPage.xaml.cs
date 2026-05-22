using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class RichTextEditPage : UserControl
    {
        public RichTextEditPage()
        {
            ViewModel = new RichTextEditPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public RichTextEditPageViewModel ViewModel { get; }
    }
}
