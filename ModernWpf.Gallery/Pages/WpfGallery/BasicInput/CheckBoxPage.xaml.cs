using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public partial class CheckBoxPage : Page
    {
        public CheckBoxPageViewModel ViewModel { get; }
        public CheckBoxPage(CheckBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
