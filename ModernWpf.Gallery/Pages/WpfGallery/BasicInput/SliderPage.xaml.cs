using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class SliderPage : UserControl
    {
        public SliderPage()
        {
            ViewModel = new WpfGalleryBasicInputPageViewModel("Slider");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryBasicInputPageViewModel ViewModel { get; }
    }
}
