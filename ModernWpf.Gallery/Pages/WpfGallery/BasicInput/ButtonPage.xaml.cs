using System.Windows;
using System.Windows.Controls;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class ButtonPage : Page
    {
        public ButtonPageViewModel ViewModel { get; }

        public ButtonPage(ButtonPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            GalleryAutomation.WithAutomationId(SimpleButtonExample, GalleryAutomation.SampleRootId("Button"));
            GalleryAutomation.WithAutomationId(SimpleButton, GalleryAutomation.SampleElementId("Button", "PrimaryButton"));
        }

        private void DisableSimpleButtonCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                ViewModel.IsSimpleButtonEnabled = !(checkBox.IsChecked ?? false);
            }
        }
    }
}
