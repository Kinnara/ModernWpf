using System.Windows;
using ModernWpf.Controls;

namespace ModernWpf.SampleApp
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnBackRequested(object sender, RoutedEventArgs e)
        {
            RootPage.GoBack();
        }

        internal void SetBackButtonVisible(bool isVisible)
        {
            TitleBar.SetIsBackButtonVisible(this, isVisible);
        }
    }
}
