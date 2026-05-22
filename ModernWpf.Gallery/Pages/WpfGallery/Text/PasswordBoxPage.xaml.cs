using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class PasswordBoxPage : UserControl
    {
        public PasswordBoxPage()
        {
            ViewModel = new PasswordBoxPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public PasswordBoxPageViewModel ViewModel { get; }
    }
}
