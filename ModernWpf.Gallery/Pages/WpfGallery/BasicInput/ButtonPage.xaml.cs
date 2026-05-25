using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public partial class ButtonPage : Page
    {
        public ButtonPage(ButtonPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public ButtonPageViewModel ViewModel { get; }
    }
}
