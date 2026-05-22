using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class ComboBoxPage : UserControl
    {
        public ComboBoxPage()
        {
            ViewModel = new ComboBoxPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public ComboBoxPageViewModel ViewModel { get; }
    }
}
