using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class ComboBoxPage : UserControl
    {
        public ComboBoxPage()
        {
            ViewModel = new WpfGalleryBasicInputPageViewModel("ComboBox");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryBasicInputPageViewModel ViewModel { get; }
    }
}
