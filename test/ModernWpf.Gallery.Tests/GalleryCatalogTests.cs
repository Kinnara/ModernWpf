using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryCatalogTests
    {
        [TestMethod]
        public void GroupsMatchWpfFirstGalleryOrder()
        {
            var expected = new[]
            {
                "Design Guidance",
                "Samples",
                "Basic Input",
                "Collections",
                "Date & Calendar",
                "Layout",
                "Media Controls",
                "Navigation",
                "Status & Info",
                "Text",
                "System",
                "ModernWpf controls",
                "Platform & patterns"
            };

            var actual = GalleryCatalog.Groups.Select(group => group.Title).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HomeOverviewGroupsMatchWpfGalleryReferenceWithModernExtensions()
        {
            var expected = new[]
            {
                "BasicInput",
                "Collections",
                "DateAndCalendar",
                "Layout",
                "Media",
                "Navigation",
                "StatusAndInfo",
                "Text",
                "System",
                "ModernWpfControls",
                "PlatformAndPatterns"
            };

            var actual = GalleryCatalog.OverviewGroups.Select(group => group.UniqueId).ToArray();

            CollectionAssert.AreEqual(expected, actual);
            Assert.IsFalse(actual.Contains("DesignGuidance"));
            Assert.IsFalse(actual.Contains("Samples"));
        }

        [TestMethod]
        public void AllControlsItemsExcludeWpfGallerySamplesLikeReference()
        {
            var allControlIds = GalleryCatalog.AllControlsItems.Select(item => item.UniqueId).ToArray();

            Assert.IsFalse(allControlIds.Contains("UserDashboard"), "The official WPF Gallery excludes the Samples section from All Controls.");
            Assert.IsTrue(allControlIds.Contains("Color"), "Design guidance items remain part of All Controls.");
            Assert.IsTrue(allControlIds.Contains("NavigationView"), "ModernWpf control pages remain part of the combined gallery.");
        }

        [TestMethod]
        public void WpfGallerySectionPageDescriptionsMatchReferenceViewModels()
        {
            var expected = new[]
            {
                new { UniqueId = "DesignGuidance", PageDescription = "Design guidelines on how to use colors, typography, and icons in your app." },
                new { UniqueId = "Samples", PageDescription = "Sample pages for common scenarios" },
                new { UniqueId = "BasicInput", PageDescription = "Controls for getting user input" },
                new { UniqueId = "Collections", PageDescription = "Controls for collection presentation" },
                new { UniqueId = "DateAndCalendar", PageDescription = "Controls for date and calendar" },
                new { UniqueId = "Layout", PageDescription = "Controls for layouting" },
                new { UniqueId = "Media", PageDescription = "Controls for media presentation" },
                new { UniqueId = "Navigation", PageDescription = "Controls for navigation and actions" },
                new { UniqueId = "StatusAndInfo", PageDescription = "Controls to show progress and extra information" },
                new { UniqueId = "Text", PageDescription = "Controls for displaying and editing text" },
                new { UniqueId = "System", PageDescription = "System-level controls and dialogs" }
            };

            foreach (var item in expected)
            {
                var group = GalleryCatalog.FindGroup(item.UniqueId);

                Assert.IsNotNull(group, item.UniqueId);
                Assert.AreEqual(item.PageDescription, group.PageDescription, item.UniqueId);
                Assert.AreNotEqual(group.Subtitle, group.PageDescription, item.UniqueId);
            }
        }

        [TestMethod]
        public void NavigationSectionCardsMatchOfficialWpfGalleryCatalog()
        {
            AssertNavigationItem("Menu", "A classic menu, allowing the display of MenuItems containing MenuFlyoutItems.", "Pivot.png");
            AssertNavigationItem("TabControl", "A control that displays a collection of tabs.", "TabView.png");
            AssertNavigationItem("Frame", "A navigation control that allows displaying different Page content within an application.", "MenuBar.png");
            AssertNavigationItem("NavigationWindow", "A control that supports navigation between pages, similar to a web browser.", "CreateMultipleWindows.png");
        }

        [TestMethod]
        public void BasicInputSectionCardsMatchOfficialWpfGalleryCatalogArtwork()
        {
            AssertItemImage("Button", "Button.png");
            AssertItemImage("CheckBox", "CheckBox.png");
            AssertItemImage("ComboBox", "CheckBox.png");
            AssertItemImage("RadioButton", "RadioButton.png");
            AssertItemImage("Slider", "Slider.png");
        }

        [TestMethod]
        public void WpfGalleryItemCardsMatchOfficialCatalogArtwork()
        {
            var expected = new[]
            {
                new { UniqueId = "Color", ImageFileName = "ColorPaletteResources.png" },
                new { UniqueId = "Typography", ImageFileName = "TextBlock.png" },
                new { UniqueId = "Spacing", ImageFileName = "CompactSizing.png" },
                new { UniqueId = "Geometry", ImageFileName = "Border.png" },
                new { UniqueId = "Iconography", ImageFileName = "IconElement.png" },
                new { UniqueId = "UserDashboard", ImageFileName = "PersonPicture.png" },
                new { UniqueId = "Button", ImageFileName = "Button.png" },
                new { UniqueId = "CheckBox", ImageFileName = "Checkbox.png" },
                new { UniqueId = "ComboBox", ImageFileName = "Checkbox.png" },
                new { UniqueId = "RadioButton", ImageFileName = "RadioButton.png" },
                new { UniqueId = "Slider", ImageFileName = "Slider.png" },
                new { UniqueId = "DataGrid", ImageFileName = "DataGrid.png" },
                new { UniqueId = "ListBox", ImageFileName = "ListBox.png" },
                new { UniqueId = "ListView", ImageFileName = "ListView.png" },
                new { UniqueId = "TreeView", ImageFileName = "TreeView.png" },
                new { UniqueId = "Calendar", ImageFileName = "CalendarView.png" },
                new { UniqueId = "DatePicker", ImageFileName = "DatePicker.png" },
                new { UniqueId = "Expander", ImageFileName = "Expander.png" },
                new { UniqueId = "Grid", ImageFileName = "Grid.png" },
                new { UniqueId = "ResizeGrip", ImageFileName = "ResizeGrip.png" },
                new { UniqueId = "GridSplitter", ImageFileName = "GridSplitter.png" },
                new { UniqueId = "GroupBox", ImageFileName = "GroupBox.png" },
                new { UniqueId = "StackPanel", ImageFileName = "StackPanel.png" },
                new { UniqueId = "Border", ImageFileName = "Border.png" },
                new { UniqueId = "Canvas", ImageFileName = "Canvas.png" },
                new { UniqueId = "Image", ImageFileName = "Image.png" },
                new { UniqueId = "Menu", ImageFileName = "Pivot.png" },
                new { UniqueId = "TabControl", ImageFileName = "TabView.png" },
                new { UniqueId = "Frame", ImageFileName = "MenuBar.png" },
                new { UniqueId = "NavigationWindow", ImageFileName = "CreateMultipleWindows.png" },
                new { UniqueId = "ProgressBar", ImageFileName = "ProgressBar.png" },
                new { UniqueId = "ToolTip", ImageFileName = "ToolTip.png" },
                new { UniqueId = "Label", ImageFileName = "Button.png" },
                new { UniqueId = "TextBox", ImageFileName = "TextBox.png" },
                new { UniqueId = "TextBlock", ImageFileName = "TextBlock.png" },
                new { UniqueId = "RichTextEdit", ImageFileName = "RichEditBox.png" },
                new { UniqueId = "PasswordBox", ImageFileName = "PasswordBox.png" },
                new { UniqueId = "Hyperlink", ImageFileName = "HyperlinkButton.png" },
                new { UniqueId = "FileAndFolderDialogs", ImageFileName = "FilePicker.png" },
                new { UniqueId = "MessageBox", ImageFileName = "ContentDialog.png" },
                new { UniqueId = "Clipboard", ImageFileName = "Clipboard.png" }
            };

            foreach (var item in expected)
            {
                AssertItemImage(item.UniqueId, item.ImageFileName);
            }
        }

        [TestMethod]
        public void WpfGalleryOverviewGroupCardsMatchOfficialCatalogMetadata()
        {
            AssertGroupCard("BasicInput", "Basic Input", "Button, CheckBox, ComboBox, RadioButton, Slider", "Button.png");
            AssertGroupCard("Collections", "Collections", "DataGrid, ListBox, ListView, TreeView", "DataGrid.png");
            AssertGroupCard("DateAndCalendar", "Date & Calendar", "Calendar, DatePicker", "CalendarView.png");
            AssertGroupCard("Layout", "Layout", "Expander,Grid, ResizeGrip, GridSplitter, GroupBox, StackPanel, Border", "Expander.png");
            AssertGroupCard("Media", "Media Controls", "Canvas, Image", "Image.png");
            AssertGroupCard("Navigation", "Navigation", "Menu, TabControl, Frame, NavigationWindow", "Pivot.png");
            AssertGroupCard("StatusAndInfo", "Status & Info", "ProgressBar, ToolTip", "ProgressBar.png");
            AssertGroupCard("Text", "Text", "Label, TextBox, TextBlock, RichTextEdit, PasswordBox", "TextBlock.png");
            AssertGroupCard("System", "System", "File and Folder Dialogs, MessageBox, Clipboard", "FilePicker.png");
        }

        [TestMethod]
        public void WpfGalleryRecentlyAddedItemsMatchOfficialCatalogOrder()
        {
            var expected = new[]
            {
                "Spacing",
                "Geometry",
                "Iconography",
                "DataGrid",
                "Grid",
                "ResizeGrip",
                "GridSplitter",
                "GroupBox",
                "StackPanel",
                "Border",
                "Frame",
                "NavigationWindow",
                "TextBox",
                "FileAndFolderDialogs",
                "MessageBox",
                "Clipboard"
            };

            var actual = GalleryCatalog.NewOrUpdatedItems.Select(item => item.UniqueId).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SearchForAFindsEveryGroup()
        {
            var resultItemIds = GalleryCatalog.Search("a")
                .Select(item => item.UniqueId)
                .ToArray();

            foreach (var group in GalleryCatalog.Groups)
            {
                Assert.IsTrue(
                    group.Items.Any(item => resultItemIds.Contains(item.UniqueId, StringComparer.OrdinalIgnoreCase)),
                    "Expected query 'a' to return at least one result from group '{0}'.",
                    group.Title);
            }
        }

        [TestMethod]
        public void CatalogItemsHaveUniqueIds()
        {
            var duplicates = GalleryCatalog.Items
                .GroupBy(item => item.UniqueId, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            CollectionAssert.AreEqual(Array.Empty<string>(), duplicates);
        }

        [TestMethod]
        public void CatalogRelationshipsResolve()
        {
            foreach (var item in GalleryCatalog.Items)
            {
                Assert.IsNotNull(GalleryCatalog.FindGroup(item.GroupId), "Missing group '{0}' for '{1}'.", item.GroupId, item.UniqueId);

                foreach (var relatedControlId in item.RelatedControlIds)
                {
                    Assert.IsNotNull(
                        GalleryCatalog.FindItem(relatedControlId),
                        "Missing related item '{0}' referenced by '{1}'.",
                        relatedControlId,
                        item.UniqueId);
                }
            }
        }

        [TestMethod]
        public void CatalogContainsWpfFirstGallerySurface()
        {
            Assert.AreEqual(13, GalleryCatalog.Groups.Count);
            Assert.AreEqual(136, GalleryCatalog.Items.Count);
        }

        private static void AssertNavigationItem(string uniqueId, string subtitle, string imageFileName)
        {
            var item = GalleryCatalog.FindItem(uniqueId);

            Assert.IsNotNull(item, uniqueId);
            Assert.AreEqual(subtitle, item.Subtitle, uniqueId);
            AssertItemImage(item, imageFileName);
        }

        private static void AssertGroupCard(string uniqueId, string title, string subtitle, string imageFileName)
        {
            var group = GalleryCatalog.FindGroup(uniqueId);

            Assert.IsNotNull(group, uniqueId);
            Assert.AreEqual(title, group.Title, uniqueId);
            Assert.AreEqual(subtitle, group.Subtitle, uniqueId);
            AssertImagePath(group.ImagePath, imageFileName);
        }

        private static void AssertItemImage(string uniqueId, string imageFileName)
        {
            var item = GalleryCatalog.FindItem(uniqueId);

            Assert.IsNotNull(item, uniqueId);
            AssertItemImage(item, imageFileName);
        }

        private static void AssertItemImage(GalleryItem item, string imageFileName)
        {
            AssertImagePath(item.ImagePath, imageFileName);
        }

        private static void AssertImagePath(string imagePath, string imageFileName)
        {
            var expectedSuffix = "Assets/ControlImages/" + imageFileName;

            Assert.IsTrue(
                imagePath.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase),
                "Expected '{0}' to end with '{1}'.",
                imagePath,
                expectedSuffix);
        }
    }
}
