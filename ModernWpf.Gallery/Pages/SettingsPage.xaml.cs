using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ModernWpf;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Pages
{
    public partial class SettingsPage
    {
        private bool _canApplyThemeSelection;

        public SettingsPage()
            : this(new SettingsPageViewModel())
        {
        }

        public SettingsPage(SettingsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            ApplyVisualTestThemeSelection();
            _canApplyThemeSelection = true;
        }

        public SettingsPageViewModel ViewModel { get; }

        private void ThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void Open_Issues(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/microsoft/WPF-Samples/issues/new");
        }

        private void ApplyVisualTestThemeSelection()
        {
            if (!GalleryDiagnostics.IsEnabled)
            {
                return;
            }

            if (string.Equals(GalleryDiagnostics.Theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                Change_ThemeMode.SelectedIndex = 0;
            }
            else if (string.Equals(GalleryDiagnostics.Theme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                Change_ThemeMode.SelectedIndex = 1;
            }
        }

        private void Open_ToolkitInformation(object sender, RoutedEventArgs e)
        {
            OpenUri("https://www.nuget.org/packages/CommunityToolkit.Mvvm/");
        }

        private void Open_DIInformation(object sender, RoutedEventArgs e)
        {
            OpenUri("https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/");
        }

        private void Open_HostingInformation(object sender, RoutedEventArgs e)
        {
            OpenUri("https://www.nuget.org/packages/Microsoft.Extensions.Hosting");
        }

        private void Services_Click(object sender, RoutedEventArgs e)
        {
            OpenUri("https://go.microsoft.com/fwlink/?LinkId=822631");
        }

        private void Privacy_Click(object sender, RoutedEventArgs e)
        {
            OpenUri("https://go.microsoft.com/fwlink/?LinkId=521839");
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
