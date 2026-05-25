using System.Windows.Controls;
namespace ModernWpf.Gallery.Pages.WpfGallery.Text
{
    public partial class PasswordBoxPage : Page
    {
        public PasswordBoxPageViewModel ViewModel { get; }

        public PasswordBoxPage(PasswordBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
