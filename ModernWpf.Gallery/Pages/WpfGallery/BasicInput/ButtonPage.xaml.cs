using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class ButtonPage : UserControl
    {
        public ButtonPage()
        {
            ViewModel = new WpfGalleryBasicInputPageViewModel("Button");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryBasicInputPageViewModel ViewModel { get; }
    }
}
