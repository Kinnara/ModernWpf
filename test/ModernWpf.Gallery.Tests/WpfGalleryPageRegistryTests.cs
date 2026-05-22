using System;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                }

                Assert.IsFalse(WpfGalleryPageRegistry.HasDirectPageContent("NavigationView"));
                Assert.IsNull(WpfGalleryPageRegistry.CreatePageContent("NavigationView"));
            });
        }
    }
}
