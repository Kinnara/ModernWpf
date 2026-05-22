using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class ButtonPage : UserControl
    {
        public ButtonPage()
        {
            ViewModel = new ButtonPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ButtonPageViewModel ViewModel { get; }
    }
}
