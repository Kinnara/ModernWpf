using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    /// <summary>
    /// Interaction logic for RadioButtonPage.xaml
    /// </summary>
    public partial class RadioButtonPage : Page
    {
        public RadioButtonPageViewModel ViewModel { get; }

        public RadioButtonPage(RadioButtonPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            GalleryAutomation.WithAutomationId(StandardRadioButtonExample, GalleryAutomation.SampleRootId("RadioButton"));
            GalleryAutomation.WithAutomationId(DefaultRadioButtonOption1, GalleryAutomation.SampleElementId("RadioButton", "RadioButton"));
        }

        private void RadioButton_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                radioButton.IsChecked = true;
            }
        }
    }
}
