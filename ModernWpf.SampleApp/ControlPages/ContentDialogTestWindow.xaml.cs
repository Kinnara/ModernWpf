using ModernWpf.Controls;
using SamplesCommon;
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

        private void ShowDialogInMainWindow(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(async () =>
            {
                try
                {
                    await new TestContentDialog() { Owner = Application.Current.MainWindow }.ShowAsync();
                }
                catch (Exception ex)
                {
                    this.RunOnUIThread(() =>
                    {
                        _ = new ContentDialog
                        {
                            Owner = this,
                            Content = ex.Message,
                            CloseButtonText = "Close"
                        }.ShowAsync();
                    });
                }
            });
        }
    }
}
