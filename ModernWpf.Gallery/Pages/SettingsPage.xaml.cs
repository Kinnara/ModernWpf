using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ModernWpf;
using ModernWpf.Gallery.Pages.WpfGallery;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private bool _canApplyThemeSelection;

        public SettingsPageViewModel ViewModel { get; }

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

        private void Services_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://go.microsoft.com/fwlink/?LinkId=822631") { UseShellExecute = true });
        }

        private void Privacy_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://go.microsoft.com/fwlink/?LinkId=521839") { UseShellExecute = true });
        }

        private void Open_Issues(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/microsoft/WPF-Samples/issues/new") { UseShellExecute = true });
        }

        private void Open_ToolkitInformation(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.nuget.org/packages/CommunityToolkit.Mvvm/") { UseShellExecute = true });
        }

        private void Open_DIInformation(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/") { UseShellExecute = true });
        }

        private void Open_HostingInformation(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.nuget.org/packages/Microsoft.Extensions.Hosting") { UseShellExecute = true });
        }

        private void ThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_canApplyThemeSelection)
            {
                return;
            }

            if (Change_ThemeMode.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                switch (selectedValue)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        break;
                    case "Use system setting":
                        ThemeManager.Current.ApplicationTheme = null;
                        break;
                    default:
                        break;
                }
            }
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
    }

    public partial class SettingsPageViewModel : WpfGalleryPageViewModel
    {
        public SettingsPageViewModel()
            : base("Settings", null)
        {
        }
    }
}
