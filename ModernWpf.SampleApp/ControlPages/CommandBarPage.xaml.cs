using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class CommandBarPage : UserControl
    {
        public CommandBarPage()
        {
            InitializeComponent();
        }

        private void OnElementClicked(object sender, RoutedEventArgs e)
        {
            SelectedOptionText.Text = "You clicked: " + (sender as AppBarButton).Label;
        }
    }
}
