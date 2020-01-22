using System.Windows;
using System.Windows.Controls;

namespace ModernWpfTestApp
{
    [TopLevelTestPage(Name = "ScrollViewer", Icon = "ScrollViewer.png")]
    public sealed partial class ScrollViewerPage : TestPage
    {
        public ScrollViewerPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sv.Visibility = Visibility.Visible;
            ((Button)sender).Visibility = Visibility.Collapsed;
        }
    }
}
