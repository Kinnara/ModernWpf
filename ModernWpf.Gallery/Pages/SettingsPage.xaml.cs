using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ModernWpf;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class SettingsPage
    {
        private bool _canApplyThemeSelection;

        public SettingsPage()
        {
            ViewModel = new SettingsPageViewModel();
            DataContext = this;
            InitializeComponent();
            SelectCurrentTheme();
            _canApplyThemeSelection = true;
        }

        public SettingsPageViewModel ViewModel { get; }

        private void SelectCurrentTheme()
        {
            switch (ThemeManager.Current.ApplicationTheme)
            {
                case ApplicationTheme.Light:
                    Change_ThemeMode.SelectedIndex = 0;
                    break;
                case ApplicationTheme.Dark:
                    Change_ThemeMode.SelectedIndex = 1;
                    break;
                default:
                    Change_ThemeMode.SelectedIndex = 2;
                    break;
            }
        }

        private void OnThemeModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_canApplyThemeSelection)
            {
                return;
            }

            switch (Change_ThemeMode.SelectedIndex)
            {
                case 0:
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    break;
                case 1:
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    break;
                default:
                    ThemeManager.Current.ApplicationTheme = null;
                    break;
            }
        }

        private void OnOpenIssuesClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/Kinnara/ModernWpf/issues");
        }

        private void OnModernWpfClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/Kinnara/ModernWpf");
        }

        private void OnWpfSamplesClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/microsoft/WPF-Samples");
        }

        private void OnWinUIGalleryClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/microsoft/WinUI-Gallery");
        }

        private void OnServicesClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://www.microsoft.com/servicesagreement");
        }

        private void OnPrivacyClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://privacy.microsoft.com/privacystatement");
        }

        private static void OpenUri(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
    }

    public sealed class SettingsPageViewModel
    {
        public string PageTitle
        {
            get { return "Settings"; }
        }

        public string PageDescription
        {
            get { return null; }
        }
    }
}
