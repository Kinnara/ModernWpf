using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class IconElementSourceAuditTests
    {
        [TestMethod]
        public void CurrentIconElementGalleryAndLiveGatesArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "iconelement-winui3-source-audit.md");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "StylesSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(root, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "9f9e42eb762032186daf4781ec3a67db514517e9");
            StringAssert.Contains(audit, "c1e7032d52401d1e433ec94d405af5f6a927fe91");
            StringAssert.Contains(audit, "4f149a7754aafc5cc7fb0e7c498199a15200619d");
            StringAssert.Contains(audit, "433e47644730e1d2ad5cfbd151376ee5c9448575");
            StringAssert.Contains(audit, "bdefb4e767635ddcc99e4f02836ce57c35b07121");
            StringAssert.Contains(audit, "4c096a248c4a42b32829a16fd4557608746c1696");

            StringAssert.Contains(sample, "CreateIconElementExamples");
            StringAssert.Contains(sample, "A BitmapIcon with a multicolor bitmap image");
            StringAssert.Contains(sample, "A FontIcon using a glyph from a specific font family in a button");
            StringAssert.Contains(sample, "A ImageIcon using a bitmap image in a button");
            StringAssert.Contains(sample, "A ImageIcon using a SVG image in a button");
            StringAssert.Contains(sample, "A PathIcon in a button");
            StringAssert.Contains(sample, "A SymbolIcon in a button");
            StringAssert.Contains(sampleTests, "IconElementSampleMatchesWinUIGalleryExamples");

            StringAssert.Contains(harness, "\"IconElement\" { return 0.1 }");
            StringAssert.Contains(harness, "GallerySample_IconElement_SlicesIcon.png");
            StringAssert.Contains(recorder, "\"IconElement\" { return $true }");
            StringAssert.Contains(recorder, "\"IconElement\" { return \"Monochrome\" }");

            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-032510-192-22356/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-032607-022-99256/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-032648-459/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-032713-648/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-032741-289-92820/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-032800-286-98924/report.md");
        }

        private static string Read(string root, params string[] parts)
        {
            var path = root;
            foreach (var part in parts)
            {
                path = Path.Combine(path, part);
            }

            return File.ReadAllText(path);
        }

        private static string FindRepoRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
            return string.Empty;
        }
    }
}
