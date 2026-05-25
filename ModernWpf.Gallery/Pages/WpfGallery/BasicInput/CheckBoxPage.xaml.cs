using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public partial class CheckBoxPage : Page
    {
        public CheckBoxPage(CheckBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public CheckBoxPageViewModel ViewModel { get; }
    }
}
