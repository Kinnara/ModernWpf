using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class WindowPage : UserControl
    {
        public WindowPage()
        {
            InitializeComponent();
        }

        private void ResetTitleBar(object sender, RoutedEventArgs e)
        {
            var w = (Window)DataContext;
            w.ClearValue(TitleBar.BackgroundProperty);
            w.ClearValue(TitleBar.ForegroundProperty);
        }
    }
}
