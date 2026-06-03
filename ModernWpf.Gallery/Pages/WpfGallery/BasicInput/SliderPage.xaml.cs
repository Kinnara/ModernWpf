using System.Windows.Controls;
using ModernWpf.Gallery.Pages;

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
            GalleryAutomation.WithAutomationId(SimpleSliderExample, GalleryAutomation.SampleRootId("Slider"));
            GalleryAutomation.WithAutomationId(SimpleSlider, GalleryAutomation.SampleElementId("Slider", "Slider"));
        }
    }
}
