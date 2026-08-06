using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryBrandingAuditTests
    {
        private static readonly string[] ForbiddenOfficialIdentityMarkers =
        {
            "github.com/microsoft/WPF-Samples",
            "© 2025 Microsoft",
            "Microsoft Services Agreement",
            "Microsoft Privacy Statement",
            "CommunityToolkit.Mvvm",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Hosting",
            "Assets/Tiles/GalleryIcon",
            "Header-WinUI",
            "http://www.microsoft.com",
            "https://www.microsoft.com"
        };

        [TestMethod]
        public void GalleryOwnedSurfacesDoNotUseOfficialSampleIdentity()
        {
            var galleryRoot = Path.Combine(GetRepoRoot(), "ModernWpf.Gallery");
            var extensions = new HashSet<string>(
                new[] { ".cs", ".csproj", ".txt", ".xaml" },
                StringComparer.OrdinalIgnoreCase);
            var sourceFiles = Directory
                .GetFiles(galleryRoot, "*", SearchOption.AllDirectories)
                .Where(path => extensions.Contains(Path.GetExtension(path)))
                .Where(path => !HasDirectorySegment(path, "bin"))
                .Where(path => !HasDirectorySegment(path, "obj"));

            foreach (var sourceFile in sourceFiles)
            {
                var source = File.ReadAllText(sourceFile);
                foreach (var marker in ForbiddenOfficialIdentityMarkers)
                {
                    Assert.IsFalse(
                        source.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0,
                        $"{GetRelativePath(galleryRoot, sourceFile)} contains official-gallery marker '{marker}'.");
                }
            }
        }

        [TestMethod]
        public void DisplayedSourceDrawersUseWpfAndModernWpfTypes()
        {
            var galleryRoot = Path.Combine(GetRepoRoot(), "ModernWpf.Gallery");
            var sampleCodeRoot = Path.Combine(galleryRoot, "Samples", "SampleCode");
            var pagesRoot = Path.Combine(galleryRoot, "Pages");
            var sourceFiles = Directory
                .GetFiles(sampleCodeRoot, "*.txt", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(pagesRoot, "*SampleFactory.cs", SearchOption.TopDirectoryOnly));

            foreach (var sourceFile in sourceFiles)
            {
                var source = File.ReadAllText(sourceFile);
                Assert.IsFalse(
                    source.IndexOf("Microsoft.UI.Xaml", StringComparison.Ordinal) >= 0,
                    $"{GetRelativePath(galleryRoot, sourceFile)} exposes a WinUI namespace.");
                Assert.IsFalse(
                    source.IndexOf("WinUIGallery.", StringComparison.Ordinal) >= 0,
                    $"{GetRelativePath(galleryRoot, sourceFile)} exposes the official WinUI Gallery namespace.");
                Assert.IsFalse(
                    source.IndexOf("WinUI Gallery source", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    source.IndexOf("WinUI Gallery repo", StringComparison.OrdinalIgnoreCase) >= 0,
                    $"{GetRelativePath(galleryRoot, sourceFile)} directs users to the WinUI Gallery source.");
            }
        }

        [TestMethod]
        public void ModernWpfCatalogUsesProjectApiNamespaces()
        {
            var wpfPrimitiveItems = new HashSet<string>(
                new[] { "RepeatButton", "ToggleButton", "Popup" },
                StringComparer.OrdinalIgnoreCase);

            foreach (var item in GalleryCatalogData.Items)
            {
                var expectedNamespace = wpfPrimitiveItems.Contains(item.UniqueId)
                    ? "System.Windows.Controls.Primitives"
                    : string.Equals(item.UniqueId, "ThemeShadow", StringComparison.OrdinalIgnoreCase)
                        ? "ModernWpf.Controls.Primitives"
                        : "ModernWpf.Controls";

                Assert.AreEqual(
                    expectedNamespace,
                    item.ApiNamespace,
                    $"{item.UniqueId} must show its WPF/ModernWPF namespace instead of an upstream WinUI namespace.");
                Assert.IsFalse(
                    item.Title.IndexOf("(WinUI)", StringComparison.OrdinalIgnoreCase) >= 0,
                    $"{item.UniqueId} must use ModernWPF display terminology.");
            }
        }

        [TestMethod]
        public void CopiedOfficialGalleryIdentityAssetsAreRemoved()
        {
            var assetsRoot = Path.Combine(GetRepoRoot(), "ModernWpf.Gallery", "Assets");

            Assert.IsFalse(Directory.Exists(Path.Combine(assetsRoot, "AppIcons")));
            Assert.IsFalse(Directory.Exists(Path.Combine(assetsRoot, "HomeHeaderTiles")));
            Assert.IsFalse(Directory.Exists(Path.Combine(assetsRoot, "Tiles")));
            Assert.IsFalse(File.Exists(Path.Combine(assetsRoot, "win11-dashboard.png")));
            Assert.IsFalse(File.Exists(Path.Combine(assetsRoot, "win11-dashboard.light.png")));
            Assert.IsFalse(File.Exists(Path.Combine(assetsRoot, "win11-dashboard.dark.png")));
            Assert.IsFalse(File.Exists(Path.Combine(assetsRoot, "GalleryHeaderImage.png")));
            Assert.IsFalse(File.Exists(Path.Combine(assetsRoot, "CopyLinkTeachingTip.png")));
        }

        [TestMethod]
        public void GalleryLinksUseMaintainedRepositorySurfaces()
        {
            var galleryRoot = Path.Combine(GetRepoRoot(), "ModernWpf.Gallery");
            var brandingSource = File.ReadAllText(
                Path.Combine(galleryRoot, "GalleryBranding.cs"));
            StringAssert.Contains(
                brandingSource,
                "QuickStartUrl = RepositoryUrl + \"#getting-started\"");
            StringAssert.Contains(
                brandingSource,
                "DocumentationUrl = RepositoryUrl + \"#documentation\"");
            StringAssert.Contains(
                brandingSource,
                "NewIssueUrl = RepositoryUrl + \"/issues/new?template=preview-bug.yml\"");

            foreach (var sourceFile in Directory.GetFiles(
                galleryRoot,
                "*",
                SearchOption.AllDirectories).Where(path =>
                    !HasDirectorySegment(path, "bin") &&
                    !HasDirectorySegment(path, "obj") &&
                    new[] { ".cs", ".xaml" }.Contains(
                        Path.GetExtension(path),
                        StringComparer.OrdinalIgnoreCase)))
            {
                var source = File.ReadAllText(sourceFile);
                Assert.IsFalse(
                    source.Contains(
                        "github.com/Kinnara/ModernWpf/wiki",
                        StringComparison.OrdinalIgnoreCase),
                    $"{GetRelativePath(galleryRoot, sourceFile)} links to the retired GitHub wiki.");
            }
        }

        private static bool HasDirectorySegment(string path, string segment)
        {
            var marker = Path.DirectorySeparatorChar + segment + Path.DirectorySeparatorChar;
            return path.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetRelativePath(string root, string path)
        {
            return path.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar);
        }
    }
}
