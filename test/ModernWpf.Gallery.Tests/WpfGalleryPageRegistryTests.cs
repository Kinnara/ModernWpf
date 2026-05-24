using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryPageRegistryTests
    {
        private static readonly string[] WpfGalleryDirectPageIds =
        {
            "Border",
            "Button",
            "Canvas",
            "Calendar",
            "CheckBox",
            "Clipboard",
            "Color",
            "ComboBox",
            "DataGrid",
            "DatePicker",
            "Expander",
            "FileAndFolderDialogs",
            "Frame",
            "Geometry",
            "Grid",
            "GridSplitter",
            "GroupBox",
            "Hyperlink",
            "Iconography",
            "Image",
            "Label",
            "ListBox",
            "ListView",
            "Menu",
            "MessageBox",
            "NavigationWindow",
            "PasswordBox",
            "ProgressBar",
            "RadioButton",
            "ResizeGrip",
            "RichTextEdit",
            "Slider",
            "Spacing",
            "StackPanel",
            "TabControl",
            "TextBlock",
            "TextBox",
            "ToolTip",
            "TreeView",
            "Typography",
            "UserDashboard"
        };

        [TestMethod]
        public void DirectPageRegistryCoversWpfGalleryEquivalentPages()
        {
            var expected = WpfGalleryDirectPageIds
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var actual = WpfGalleryPageRegistry.DirectPageIds.ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DirectPageRegistryCreatesWpfGalleryPageInstances()
        {
            WpfTestHost.Run(() =>
            {
                foreach (var uniqueId in WpfGalleryDirectPageIds)
                {
                    var page = WpfGalleryPageRegistry.CreatePageContent(uniqueId);

                    Assert.IsNotNull(page, uniqueId);
                    Assert.IsInstanceOfType(page, typeof(UIElement), uniqueId);
                    StringAssert.Contains(page.GetType().FullName, ".Pages.WpfGallery.");
                    Assert.AreEqual(
                        Application.Current.FindResource("BodyTextBlockFontSize"),
                        TextElement.GetFontSize(page),
                        uniqueId);
                }

                Assert.IsFalse(WpfGalleryPageRegistry.HasDirectPageContent("NavigationView"));
                Assert.IsNull(WpfGalleryPageRegistry.CreatePageContent("NavigationView"));
            });
        }

        [TestMethod]
        public void DirectPageRegistryItemsUseDirectPageHosting()
        {
            WpfTestHost.Run(() =>
            {
                foreach (var uniqueId in WpfGalleryDirectPageIds)
                {
                    var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));

                    Assert.IsTrue(page.UsesWpfGalleryPageMode, uniqueId);
                    Assert.IsTrue(page.HasDirectPageContent, uniqueId);
                    Assert.AreEqual(new Thickness(0), page.DirectPageContentMargin, uniqueId);
                    Assert.AreEqual(0, page.Examples.Count, uniqueId);
                    Assert.IsFalse(page.ShowScrolledPageContent, uniqueId);
                    Assert.IsFalse(page.ShowCatalogDetails, uniqueId);

                    var directPageHost = (Frame)page.FindName("DirectPageContentHost");
                    Assert.AreEqual(NavigationUIVisibility.Hidden, directPageHost.NavigationUIVisibility, uniqueId);
                }
            });
        }
    }
}
