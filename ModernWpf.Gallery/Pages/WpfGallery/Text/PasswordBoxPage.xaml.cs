using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public sealed partial class PasswordBoxPage : Page
    {
        public PasswordBoxPage(PasswordBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public PasswordBoxPageViewModel ViewModel { get; }
    }
}
