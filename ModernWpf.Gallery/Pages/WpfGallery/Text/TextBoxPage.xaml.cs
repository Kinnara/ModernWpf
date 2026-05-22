using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class TextBoxPage : UserControl
    {
        public TextBoxPage()
        {
            ViewModel = new TextBoxPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public TextBoxPageViewModel ViewModel { get; }
    }
}
