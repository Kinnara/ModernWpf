using System.Windows.Controls;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for CheckBox.xaml
    /// </summary>
    public partial class CheckBoxPage : Page
    {
        public CheckBoxPageViewModel ViewModel { get; }
        public CheckBoxPage(CheckBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            GalleryAutomation.WithAutomationId(TwoStateCheckBoxExample, GalleryAutomation.SampleRootId("CheckBox"));
            GalleryAutomation.WithAutomationId(TwoStateCheckBox, GalleryAutomation.SampleElementId("CheckBox", "CheckBox"));
        }
    }
}
