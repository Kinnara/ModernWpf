using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for ButtonPage.xaml
    /// </summary>
    public partial class ButtonPage : Page
    {
        public ButtonPageViewModel ViewModel { get; }

        public ButtonPage(ButtonPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
