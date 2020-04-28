using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class WindowPage
    {
        public WindowPage()
        {
            InitializeComponent();
        }

        private void ResizeWindow(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem)
            {
                var size = Size.Parse(((string)menuItem.Header).Replace('×', ','));
                Application.Current.MainWindow.Width = size.Width;
                Application.Current.MainWindow.Height = size.Height;
            }
        }

        private void OpenWindow(object sender, RoutedEventArgs e)
        {
            var window = new WindowWithCustomTitleBar
            {
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }
    }
}
