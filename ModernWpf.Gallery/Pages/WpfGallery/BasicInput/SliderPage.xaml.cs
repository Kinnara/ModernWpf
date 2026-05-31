using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for SliderPage.xaml
    /// </summary>
    public partial class SliderPage : Page
    {
        public SliderPageViewModel ViewModel { get; }

        public SliderPage(SliderPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
