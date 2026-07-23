using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Pages;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryWhatsNewSnippetTests
    {
        [TestMethod]
        public void WhatsNewControlExampleShowsRecommendedModernWpfResources()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new WhatsNewPage(),
                    new ExpectedExample(
                        "Application resources",
                        Lines(
                            "<Application",
                            "    ...",
                            "    xmlns:ui=\"http://schemas.modernwpf.com/2019\">",
                            "    <Application.Resources>",
                            "        <ResourceDictionary>",
                            "            <ResourceDictionary.MergedDictionaries>",
                            "                <ui:ThemeResources />",
                            "                <ui:FluentControlsResources UseCompactResources=\"False\" />",
                            "            </ResourceDictionary.MergedDictionaries>",
                            "        </ResourceDictionary>",
                            "    </Application.Resources>",
                            "</Application>")));
            });
        }

        [TestMethod]
        public void WhatsNewPageRoutesCatalogItemsWithoutExternalReleaseHandlers()
        {
            var source = ReadRepoFile("ModernWpf.Gallery", "Pages", "WhatsNewPage.xaml.cs");

            Assert.IsFalse(source.Contains("System.Diagnostics", System.StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("Process.Start", System.StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("Open_WhatsNew", System.StringComparison.Ordinal));
            Assert.IsFalse(source.Contains("MessageBox", System.StringComparison.Ordinal));
            StringAssert.Contains(source, "using ModernWpf.Gallery.Models;");
            StringAssert.Contains(source, "if (parameter is GalleryItem item)");
            StringAssert.Contains(source, "ItemRequested?.Invoke(item.UniqueId);");
            StringAssert.Contains(source, "else if (parameter is string uniqueId)");
            StringAssert.Contains(source, "ItemRequested?.Invoke(uniqueId);");
        }
    }
}
