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
            w.ClearValue(TitleBar.InactiveBackgroundProperty);
            w.ClearValue(TitleBar.InactiveForegroundProperty);
            w.ClearValue(TitleBar.ButtonBackgroundProperty);
            w.ClearValue(TitleBar.ButtonForegroundProperty);
            w.ClearValue(TitleBar.ButtonHoverBackgroundProperty);
            w.ClearValue(TitleBar.ButtonHoverForegroundProperty);
            w.ClearValue(TitleBar.ButtonPressedBackgroundProperty);
            w.ClearValue(TitleBar.ButtonPressedForegroundProperty);
            w.ClearValue(TitleBar.ButtonInactiveBackgroundProperty);
            w.ClearValue(TitleBar.ButtonInactiveForegroundProperty);
        }
    }
}
