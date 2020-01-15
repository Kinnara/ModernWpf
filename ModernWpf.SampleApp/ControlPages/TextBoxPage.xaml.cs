using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class TextBoxPage
    {
        public TextBoxPage()
        {
            InitializeComponent();
        }

        private void ClearClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
        }

        private void OptionsExpander_Expanded(object sender, RoutedEventArgs e)
        {
            OptionsExpander.Header = "Hide options";
        }

        private void OptionsExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            OptionsExpander.Header = "Show options";
        }
    }
}
