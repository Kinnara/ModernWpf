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

        private void ResizeWindow(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem)
            {
                var size = Size.Parse((string)menuItem.Tag);
                Application.Current.MainWindow.Width = size.Width;
                Application.Current.MainWindow.Height = size.Height;
            }
        }
    }
}
