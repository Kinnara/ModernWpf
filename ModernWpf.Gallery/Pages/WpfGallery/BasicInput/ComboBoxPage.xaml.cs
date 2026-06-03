using System.Windows.Controls;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for ComboBox.xaml
    /// </summary>
    public partial class ComboBoxPage : Page
    {
        public ComboBoxPageViewModel ViewModel { get; }

        public ComboBoxPage(ComboBoxPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            GalleryAutomation.WithAutomationId(InlineComboBoxExample, GalleryAutomation.SampleRootId("ComboBox"));
            GalleryAutomation.WithAutomationId(InlineComboBox, GalleryAutomation.SampleElementId("ComboBox", "ComboBox"));
        }
    }
}
