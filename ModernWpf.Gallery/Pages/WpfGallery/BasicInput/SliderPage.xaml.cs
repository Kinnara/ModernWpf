using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class SliderPage : UserControl
    {
        public SliderPage()
        {
            ViewModel = new SliderPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public SliderPageViewModel ViewModel { get; }
    }
}
