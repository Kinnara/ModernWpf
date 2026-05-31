using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for CheckBoxPage.xaml
    /// </summary>
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
