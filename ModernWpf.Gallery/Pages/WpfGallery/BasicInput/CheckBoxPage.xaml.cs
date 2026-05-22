using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class CheckBoxPage : UserControl
    {
        public CheckBoxPage()
        {
            ViewModel = new WpfGalleryBasicInputPageViewModel("CheckBox");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryBasicInputPageViewModel ViewModel { get; }
    }
}
