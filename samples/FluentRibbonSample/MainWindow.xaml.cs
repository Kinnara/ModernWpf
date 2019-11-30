using ModernWpf;
using ModernWpf.Controls;
using System.Windows;

namespace FluentRibbonSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InvertTheme(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                Fluent.ThemeManager.ChangeThemeBaseColor(Application.Current, "Light");
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                Fluent.ThemeManager.ChangeThemeBaseColor(Application.Current, "Dark");
            }
        }

        private async void ShowContentDialog(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Title",
                Content = "Content",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };
            await dialog.ShowAsync();
        }
    }
}
