using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySettingsSnippetTests
    {
        [TestMethod]
        public void SettingsLinkHandlersUseOfficialProcessStartShape()
        {
            var source = ReadRepoFile("ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs");

            Assert.IsFalse(source.Contains("OpenUri("), "Copied Settings link handlers should keep the official direct Process.Start source shape.");
            AssertContainsInOrder(
                source,
                "Process.Start(new ProcessStartInfo(\"https://go.microsoft.com/fwlink/?LinkId=822631\") { UseShellExecute = true });",
                "Process.Start(new ProcessStartInfo(\"https://go.microsoft.com/fwlink/?LinkId=521839\") { UseShellExecute = true });",
                "Process.Start(new ProcessStartInfo(\"https://github.com/microsoft/WPF-Samples/issues/new\") { UseShellExecute = true });",
                "Process.Start(new ProcessStartInfo(\"https://www.nuget.org/packages/CommunityToolkit.Mvvm/\") { UseShellExecute = true });",
                "Process.Start(new ProcessStartInfo(\"https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/\") { UseShellExecute = true });",
                "Process.Start(new ProcessStartInfo(\"https://www.nuget.org/packages/Microsoft.Extensions.Hosting\") { UseShellExecute = true });");
        }
    }
}
