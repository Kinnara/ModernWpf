using ModernWpf.Controls;
using System;
using System.Windows;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ContentDialogTestWindow : Window
    {
        public ContentDialogTestWindow()
        {
            InitializeComponent();
        }

        private void ShowDialogInThisWindow(object sender, RoutedEventArgs e)
        {
            _ = new TestContentDialog().ShowAsync();
        }

        private async void ShowDialogInMainWindow(object sender, RoutedEventArgs e)
        {
            try
            {
                await new TestContentDialog() { Owner = Application.Current.MainWindow }.ShowAsync();
            }
            catch (Exception ex)
            {
                _ = new ContentDialog
                {
                    Owner = this,
                    Content = ex.Message,
                    CloseButtonText = "Close"
                }.ShowAsync();
            }
        }
    }
}
