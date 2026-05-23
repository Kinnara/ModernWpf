using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class RadioButtonPage : Page
    {
        public RadioButtonPage(RadioButtonPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public RadioButtonPageViewModel ViewModel { get; }

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
