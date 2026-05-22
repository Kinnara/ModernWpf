using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class TextBlockPage : UserControl
    {
        public TextBlockPage()
        {
            ViewModel = new TextBlockPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public TextBlockPageViewModel ViewModel { get; }
    }
}
