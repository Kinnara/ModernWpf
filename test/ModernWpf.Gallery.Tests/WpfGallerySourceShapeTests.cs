using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySourceShapeTests
    {
        [TestMethod]
        public void CopiedWpfGalleryCodeBehindClassesStayUnsealedLikeOfficialSource()
        {
            var repoRoot = GetRepoRoot();
            var wpfGalleryPageCodeBehind = Directory.EnumerateFiles(
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WpfGallery"),
                "*.xaml.cs",
                SearchOption.AllDirectories);
            var copiedTopLevelCodeBehind = new[]
            {
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "HomePage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "AllControlsPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "WhatsNewPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "SettingsPage.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls", "HeaderTile.xaml.cs"),
                Path.Combine(repoRoot, "ModernWpf.Gallery", "Controls", "TileGallery.xaml.cs")
            };

            foreach (var path in wpfGalleryPageCodeBehind.Concat(copiedTopLevelCodeBehind))
            {
                var source = File.ReadAllText(path);
                Assert.IsFalse(
                    source.Contains("public sealed partial class", StringComparison.Ordinal),
                    Path.GetRelativePath(repoRoot, path) + " should match the official WPF Gallery unsealed partial class shape.");
            }

            var sectionSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "WpfGallerySectionPages.cs");
            foreach (var className in new[]
            {
                "DesignGuidancePage",
                "SamplesPage",
                "BasicInputPage",
                "CollectionsPage",
                "DateAndTimePage",
                "LayoutPage",
                "MediaPage",
                "NavigationPage",
                "StatusAndInfoPage",
                "TextPage",
                "SystemPage"
            })
            {
                Assert.IsFalse(
                    sectionSource.Contains("public sealed class " + className + " : SectionPage", StringComparison.Ordinal),
                    className + " should remain unsealed like the official WPF Gallery section page type.");
            }
        }
    }
}
