using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public partial class ComboBoxPage : Page
    {
        public ComboBoxPageViewModel ViewModel { get; }

        public ComboBoxPage(ComboBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
