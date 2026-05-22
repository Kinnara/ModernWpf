using System.Windows.Controls;
using System.Windows.Input;

namespace ModernWpf.Gallery.Pages.WpfGallery.BasicInput
{
    public sealed partial class RadioButtonPage : UserControl
    {
        public RadioButtonPage()
        {
            ViewModel = new WpfGalleryBasicInputPageViewModel("RadioButton");
            DataContext = this;
            InitializeComponent();
        }

        public WpfGalleryBasicInputPageViewModel ViewModel { get; }

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
