using System.Windows.Controls;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class CheckBoxPage : UserControl
    {
        public CheckBoxPage()
        {
            ViewModel = new CheckBoxPageViewModel();
            DataContext = this;
            InitializeComponent();
        }

        public CheckBoxPageViewModel ViewModel { get; }
    }
}
