using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public partial class SliderPage : Page
    {
        public SliderPage(SliderPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public SliderPageViewModel ViewModel { get; }
    }
}
