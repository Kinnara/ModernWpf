using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class ItemsWrapGridSourceAuditTests
    {
        [TestMethod]
        public void CurrentGridViewConsumerAndNoStandaloneGalleryBoundaryArePinned()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "variablesizedwrapgrid-winui3-source-audit.md");
            var factory = Read(root, "ModernWpf.Gallery", "Pages", "CollectionsSampleFactory.cs");
            var galleryTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");

            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "248a2341c9c876a398715723ac6b7924d1271e3d");
            StringAssert.Contains(audit, "6114d9dfab83f1359254083b9dd277ae55707eea");
            StringAssert.Contains(audit, "no standalone VariableSizedWrapGrid, WrapGrid, or ItemsWrapGrid page");

            StringAssert.Contains(factory, "CreateGridViewItemsWrapPanel(3");
            StringAssert.Contains(factory, "typeof(Mux.ItemsWrapGrid)");
            StringAssert.Contains(factory, "FrameworkElement.NameProperty, \"MaxItemsWrapGrid\"");
            StringAssert.Contains(factory, "Mux.ItemsWrapGrid.MaximumRowsOrColumnsProperty");
            StringAssert.Contains(factory, "Mux.ItemsWrapGrid.OrientationProperty, Orientation.Horizontal");
            StringAssert.Contains(factory, "styledGridWrapPanel.MaximumRowsOrColumns = (int)wrapItemCount.Value");

            StringAssert.Contains(galleryTests, "var maxItemsWrapGrid = FindNamedDescendant<Mux.ItemsWrapGrid>(page, \"MaxItemsWrapGrid\")");
            StringAssert.Contains(galleryTests, "Assert.AreEqual(3, maxItemsWrapGrid.MaximumRowsOrColumns)");
            StringAssert.Contains(galleryTests, "Assert.AreEqual(4, maxItemsWrapGrid.MaximumRowsOrColumns)");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260718-200820-485-79776/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260718-200857-428-3920/report.md");
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
