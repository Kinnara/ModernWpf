using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySettingsSnippetTests
    {
        [TestMethod]
        public void SettingsLinkHandlersUseCentralModernWpfDestinations()
        {
            var source = ReadRepoFile("ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs");

            AssertContainsInOrder(
                source,
                "OpenUrl(GalleryBranding.RepositoryUrl);",
                "OpenUrl(GalleryBranding.LicenseUrl);",
                "OpenUrl(GalleryBranding.NewIssueUrl);",
                "OpenUrl(GalleryBranding.BehaviorsPackageUrl);",
                "private static void OpenUrl(string url)",
                "Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });");
            Assert.IsFalse(source.Contains("microsoft/WPF-Samples", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(source.Contains("go.microsoft.com/fwlink", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void SettingsThemeModeHandlerUsesOfficialSelectedItemShape()
        {
            var source = ReadRepoFile("ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs");

            Assert.IsFalse(
                source.Contains("switch (Change_ThemeMode.SelectedIndex)", System.StringComparison.Ordinal),
                "Copied Settings theme handler should keep the official selected ComboBoxItem source shape instead of a local index switch.");
            AssertContainsInOrder(
                source,
                "private void ThemeMode_SelectionChanged(object sender, SelectionChangedEventArgs e)",
                "if (Change_ThemeMode.SelectedItem is ComboBoxItem selectedItem)",
                "string selectedValue = selectedItem.Content.ToString();",
                "switch (selectedValue)",
                "case \"Light\":",
                "ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;",
                "case \"Dark\":",
                "ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;",
                "case \"Use system setting\":",
                "ThemeManager.Current.ApplicationTheme = null;",
                "default:",
                "break;");
        }
    }
}
