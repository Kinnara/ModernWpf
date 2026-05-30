using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryCatalogTests
    {
        private static readonly string[] RetainedModernWpfExtensionItemIds =
        {
            "NavigationView",
            "InfoBar",
            "NumberBox",
            "AutoSuggestBox",
            "ContentDialog",
            "TeachingTip",
            "CommandBar",
            "CommandBarFlyout",
            "AppBarButton",
            "AppBarToggleButton",
            "AppBarSeparator",
            "DropDownButton",
            "SplitButton",
            "ToggleSplitButton",
            "RepeatButton",
            "ToggleButton",
            "MenuBar",
            "MenuFlyout",
            "ItemsRepeater",
            "PipsPager",
            "RatingControl",
            "ToggleSwitch",
            "ColorPicker",
            "HyperlinkButton",
            "ProgressRing",
            "InfoBadge",
            "Flyout",
            "Popup",
            "Pivot",
            "BreadcrumbBar",
            "SelectorBar",
            "SplitView",
            "AnnotatedScrollBar",
            "ParallaxView",
            "PullToRefresh",
            "GridView",
            "SwipeControl",
            "PersonPicture",
            "IconElement",
            "ThemeShadow",
            "TitleBar"
        };

        private static readonly string[] DeletedWinUIPageImplementationIds =
        {
            "CalendarDatePicker",
            "CalendarView",
            "TimePicker",
            "TabView",
            "RichEditBox",
            "RichTextBlock",
            "ScrollViewer",
            "ScrollView",
            "FlipView",
            "ItemsView",
            "EasingFunction",
            "PageTransition",
            "ThemeTransition",
            "ImplicitTransition",
            "ConnectedAnimation",
            "SemanticZoom",
            "StandardUICommand",
            "XamlUICommand",
            "RadialGradientBrush",
            "SystemBackdrop",
            "CompactSizing",
            "AppWindow",
            "AppWindowTitleBar",
            "CreateMultipleWindows",
            "StoragePickers",
            "AnimatedIcon",
            "MediaPlayerElement",
            "MapControl",
            "WebView2",
            "Sound",
            "Acrylic",
            "LinePage",
            "ShapePage"
        };

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
                "Navigation",
                "Status & Info",
                "Text",
                "System",
                "Media Controls",
                "ModernWpf controls"
            };

            var actual = GalleryCatalog.Groups.Select(group => group.Title).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void HomeOverviewGroupsMatchWpfGalleryReference()
        {
            var expected = new[]
            {
                "BasicInput",
                "Collections",
                "DateAndCalendar",
                "Layout",
                "Navigation",
                "StatusAndInfo",
                "Text",
                "System"
            };

            var actual = GalleryCatalog.OverviewGroups.Select(group => group.UniqueId).ToArray();

            CollectionAssert.AreEqual(expected, actual);
            Assert.IsFalse(actual.Contains("DesignGuidance"));
            Assert.IsFalse(actual.Contains("Media"));
            Assert.IsFalse(actual.Contains("Samples"));
            Assert.IsFalse(actual.Contains("ModernWpfControls"));
            Assert.IsNotNull(GalleryCatalog.FindGroup("ModernWpfControls"));
        }

        [TestMethod]
        public void AllControlsItemsFollowOfficialWpfGalleryCatalogFilter()
        {
            var allControlIds = GalleryCatalog.AllControlsItems.Select(item => item.UniqueId).ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    "Color",
                    "Typography",
                    "Spacing",
                    "Geometry",
                    "Iconography",
                    "Button",
                    "CheckBox",
                    "ComboBox",
                    "RadioButton",
                    "Slider",
                    "DataGrid",
                    "ListBox",
                    "ListView",
                    "TreeView",
                    "Calendar",
                    "DatePicker",
                    "Expander",
                    "Grid",
                    "ResizeGrip",
                    "GridSplitter",
                    "GroupBox",
                    "StackPanel",
                    "Border",
                    "Menu",
                    "TabControl",
                    "Frame",
                    "NavigationWindow",
                    "ProgressBar",
                    "ToolTip",
                    "Label",
                    "TextBox",
                    "TextBlock",
                    "RichTextEdit",
                    "PasswordBox",
                    "Hyperlink",
                    "FileAndFolderDialogs",
                    "MessageBox",
                    "Clipboard"
                },
                allControlIds,
                "All Controls should keep the official WPF Gallery item sequence and content extent.");

            Assert.IsFalse(allControlIds.Contains("UserDashboard"), "The official WPF Gallery excludes the Samples section from All Controls.");
            Assert.IsFalse(allControlIds.Contains("Canvas"), "The current official WPF Gallery catalog omits the orphaned Media group from All Controls.");
            Assert.IsFalse(allControlIds.Contains("Image"), "The current official WPF Gallery catalog omits the orphaned Media group from All Controls.");
            Assert.IsNotNull(GalleryCatalog.FindGroup("Media"), "ModernWpf still exposes the orphaned Media pages as a dedicated combined-gallery section.");
            Assert.IsTrue(allControlIds.Contains("Color"), "Design guidance items remain part of All Controls.");
            Assert.IsFalse(allControlIds.Contains("NavigationView"), "ModernWpf/WinUI extension pages stay in their own navigation sections so All Controls matches the official WPF Gallery.");
            Assert.IsNotNull(GalleryCatalog.FindItem("NavigationView"), "ModernWpf control pages remain reachable outside the WPF Gallery All Controls page.");

            CollectionAssert.AreEqual(
                new[] { "Menu", "TabControl", "Frame", "NavigationWindow" },
                allControlIds
                    .Where(id => id == "Menu" || id == "TabControl" || id == "Frame" || id == "NavigationWindow")
                    .ToArray(),
                "The Navigation controls should retain the official WPF Gallery All Controls order.");
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
            AssertNavigationItem("NavigationWindow", "A control that supports navigation between pages, similar to a web browser.", "NavigationWindow.png");
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
                new { UniqueId = "Spacing", ImageFileName = "Spacing.png" },
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
                new { UniqueId = "NavigationWindow", ImageFileName = "NavigationWindow.png" },
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
        public void WinUIExtensionCatalogMatchesImplementedModernWpfSurfaces()
        {
            var modernWpfGroup = GalleryCatalog.FindGroup("ModernWpfControls");
            Assert.IsNotNull(modernWpfGroup);
            CollectionAssert.AreEqual(
                RetainedModernWpfExtensionItemIds,
                modernWpfGroup.Items.Select(item => item.UniqueId).ToArray());
            CollectionAssert.IsSubsetOf(
                RetainedModernWpfExtensionItemIds,
                GalleryCatalog.Items.Select(item => item.UniqueId).ToArray());
        }

        [TestMethod]
        public void GeneratedWinUIMetadataOnlyContainsRetainedModernWpfSurfaces()
        {
            var generatedItemIds = GalleryCatalogData.Items
                .Select(item => item.UniqueId)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var expectedItemIds = RetainedModernWpfExtensionItemIds
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            CollectionAssert.AreEqual(expectedItemIds, generatedItemIds);
        }

        [TestMethod]
        public void SourceWinUIControlInfoDataOnlyContainsRetainedModernWpfSurfaces()
        {
            var controlInfoPath = FindRepoFile("ModernWpf.Gallery", "Samples", "Data", "ControlInfoData.json");
            var retainedItemIds = new HashSet<string>(RetainedModernWpfExtensionItemIds, StringComparer.OrdinalIgnoreCase);
            using (var document = JsonDocument.Parse(File.ReadAllText(controlInfoPath)))
            {
                var groups = document.RootElement.GetProperty("Groups").EnumerateArray().ToArray();
                Assert.IsTrue(groups.All(group => group.GetProperty("Items").GetArrayLength() > 0), "Source data should not keep empty groups after page pruning.");

                var sourceItemIds = groups
                    .SelectMany(group => group.GetProperty("Items").EnumerateArray())
                    .Select(item => item.GetProperty("UniqueId").GetString())
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var expectedItemIds = RetainedModernWpfExtensionItemIds
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                CollectionAssert.AreEqual(expectedItemIds, sourceItemIds);

                foreach (var item in groups.SelectMany(group => group.GetProperty("Items").EnumerateArray()))
                {
                    var itemId = item.GetProperty("UniqueId").GetString();
                    if (item.TryGetProperty("RelatedControls", out var relatedControls))
                    {
                        foreach (var relatedControl in relatedControls.EnumerateArray().Select(value => value.GetString()))
                        {
                            Assert.IsTrue(
                                retainedItemIds.Contains(relatedControl),
                                "Source item '{0}' still references deleted page '{1}'.",
                                itemId,
                                relatedControl);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TrackerCurrentModernWpfSurfaceMatchesRetainedCatalogGuard()
        {
            var trackerPath = FindRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");
            var tracker = File.ReadAllText(trackerPath);
            const string CurrentSurfaceStart = "Current ModernWpf/WinUI extension surface:";
            const string CurrentSurfaceEnd = "Do not keep WinUI alias pages";
            var startIndex = tracker.IndexOf(CurrentSurfaceStart, StringComparison.Ordinal);
            Assert.IsTrue(startIndex >= 0, CurrentSurfaceStart);
            var endIndex = tracker.IndexOf(CurrentSurfaceEnd, startIndex, StringComparison.Ordinal);
            Assert.IsTrue(endIndex > startIndex, CurrentSurfaceEnd);

            var currentSurfaceText = tracker.Substring(startIndex, endIndex - startIndex);
            var trackerSurfaceIds = currentSurfaceText
                .Split('`')
                .Where((_, index) => index % 2 == 1)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var expectedSurfaceIds = RetainedModernWpfExtensionItemIds
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            CollectionAssert.AreEqual(expectedSurfaceIds, trackerSurfaceIds);
        }

        [TestMethod]
        public void ActiveGallerySourceDoesNotKeepDeletedWinUIPageImplementationArtifacts()
        {
            var sourceRoots = new[]
            {
                FindRepoDirectory("ModernWpf.Gallery", "Generated"),
                FindRepoDirectory("ModernWpf.Gallery", "Models"),
                FindRepoDirectory("ModernWpf.Gallery", "Pages"),
                FindRepoDirectory("ModernWpf.Gallery", "Samples", "Data"),
                FindRepoDirectory("ModernWpf.Gallery", "Samples", "SampleCode"),
                FindRepoDirectory("tools", "visual-checks")
            };
            var sourceExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".cs",
                ".json",
                ".ps1",
                ".txt",
                ".xaml"
            };
            var repoRoot = new DirectoryInfo(FindRepoDirectory("ModernWpf.Gallery")).Parent.FullName;
            var violations = new List<string>();

            foreach (var sourceFile in sourceRoots
                .SelectMany(root => Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
                .Where(path => sourceExtensions.Contains(Path.GetExtension(path))))
            {
                var relativePath = Path.GetRelativePath(repoRoot, sourceFile);
                var text = File.ReadAllText(sourceFile);

                foreach (var deletedItemId in DeletedWinUIPageImplementationIds)
                {
                    foreach (var marker in CreateDeletedPageImplementationMarkers(deletedItemId))
                    {
                        if (relativePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            text.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            violations.Add(relativePath + " contains deleted page implementation marker '" + marker + "'.");
                        }
                    }
                }
            }

            Assert.AreEqual(
                0,
                violations.Count,
                "Deleted gallery pages should not leave route/factory/sample/source markers in active gallery source:" + Environment.NewLine +
                string.Join(Environment.NewLine, violations));
        }

        [TestMethod]
        public void WpfGalleryVisualAuditRejectsLightDarkWhileOsHighContrastIsEnabled()
        {
            var script = File.ReadAllText(FindRepoFile("tools", "visual-checks", "Run-WpfGalleryVisualAudit.ps1"));

            StringAssert.Contains(script, "$osHighContrastEnabled = Test-OsHighContrastEnabled");
            StringAssert.Contains(script, "($Theme -eq \"Light\" -or $Theme -eq \"Dark\") -and $osHighContrastEnabled");
            StringAssert.Contains(
                script,
                "Light/Dark audits under OS High Contrast produce mismatched ModernWpf and official direct-host content-crop sizes and invalid comparison evidence.");
        }

        [TestMethod]
        public void CatalogImageResourcesAreShipped()
        {
            var resourceNames = GetGalleryResourceNames();
            var imagePaths = GalleryCatalog.Groups
                .Select(group => group.ImagePath)
                .Concat(GalleryCatalog.Items.Select(item => item.ImagePath))
                .Distinct()
                .ToArray();

            foreach (var imagePath in imagePaths)
            {
                var resourceKey = GetGalleryResourceKey(imagePath);
                Assert.IsTrue(
                    resourceNames.Contains(resourceKey),
                    "Missing embedded resource for catalog image '{0}'.",
                    imagePath);
            }
        }

        [TestMethod]
        public void ControlImageResourcesMatchRetainedCatalogImages()
        {
            var retainedSampleImageResources = new[]
            {
                "assets/controlimages/combobox.png"
            };

            var expectedImageResources = GalleryCatalog.Groups
                .Select(group => group.ImagePath)
                .Concat(GalleryCatalog.Items.Select(item => item.ImagePath))
                .Concat(GalleryCatalogData.Groups.Select(group => group.ImagePath))
                .Concat(GalleryCatalogData.Groups.SelectMany(group => group.Items.Select(item => item.ImagePath)))
                .Where(path => path.IndexOf("/Assets/ControlImages/", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(GetGalleryResourceKey)
                .Concat(retainedSampleImageResources)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var actualImageResources = GetGalleryResourceNames()
                .Where(name => name.StartsWith("assets/controlimages/", StringComparison.OrdinalIgnoreCase))
                .Where(name => name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            CollectionAssert.AreEqual(expectedImageResources, actualImageResources);
        }

        [TestMethod]
        public void WpfGalleryReferenceNonControlAssetsMatchOfficialHashes()
        {
            var expectedAssets = new[]
            {
                Tuple.Create("assets/win11-dashboard.png", "DF35A419D9827E88E1719588843CDB574980A6D7BFDF5DFF3DDEC6117CDC6E99"),
                Tuple.Create("assets/win11-dashboard.light.png", "0DF877D2E7967D05B8CE05261B408C6E551CDB1FBD507ACDCCE02C3A5478679B"),
                Tuple.Create("assets/win11-dashboard.dark.png", "FBA3B611A11D0EDE81545B08CEE7576A517B5B94F29DAE4182B9952805676D5E"),
                Tuple.Create("assets/appicons/wpfgallery_48px.png", "95D5ABF87383212AC803F92650D5211881D5F5AEB0766DCB5314D54D1D926CA1"),
                Tuple.Create("assets/appicons/wpfgallery_512px.png", "169BEF1B31DBC5938ACD504AE461909494B7751A1BF10D5B176D7FDFDE80DA27"),
                Tuple.Create("assets/appicons/wpfgallery.ico", "A58A690D437F60C052331340718413BF751419EC49C904640DD73797FF33B780"),
                Tuple.Create("assets/appicons/wpfgallery.svg", "5F889DAEFF8B62BFF9E3E18DA20257A68A1619E322E3C050C588FECA601480A9"),
                Tuple.Create("assets/design/cards.dark.png", "ACB4D2761297334FF2876440C3BF240A960C7A4FD91EFC97AB804DA461B43E63"),
                Tuple.Create("assets/design/cards.light.png", "C0471913EEF2E019DEEB14EEC7B4B23E2AED9E851A85636026D10F1E65677545"),
                Tuple.Create("assets/design/dialog.dark.png", "DB68563DA1196BFE88F844C30E24F3A96EA6CCF9CED9080BEE90630B1FCFC75F"),
                Tuple.Create("assets/design/dialog.light.png", "00AAC573496C01498B9493AFEB2D303E20EFC255715F4376569EA464996B926F"),
                Tuple.Create("assets/design/geometry.dark.png", "1AB58C4478FB25A9C9E32C5218C0FFF1DB807BC9910166AA13BF97D940416D41"),
                Tuple.Create("assets/design/geometry.light.png", "1B14DAAD987EEF58D8F5D942D0038A7E26EF1D72F0B4FE3F5FFCCC563E5F3933"),
                Tuple.Create("assets/homeheadertiles/header-store.dark.png", "192228FB9504B8CEC5EE5762A6AD582992291FC96FE5600C5FEE4E7DBB487F32"),
                Tuple.Create("assets/homeheadertiles/header-windowsdesign.png", "EF286FC0AEA37C98FFEAD7A8CB0DB3229B6C834383F2D0A594D9DD3ADD48AEA7"),
                Tuple.Create("assets/userdashboard/64-100x100.jpg", "7343E974F503581DE2A89AB49B240F8FA5CB54C7908FC6908363E1341B45C43F"),
                Tuple.Create("assets/userdashboard/65-100x100.jpg", "C5257B2DCAF32304C3D95FCAB526BB736D947C7EE5EF479EFEDB76730639AB43"),
                Tuple.Create("assets/userdashboard/91-100x100.jpg", "6DA12F78B0A32C27A805E778B641C9E067B1185454E7973306D1428B40F5FA77"),
                Tuple.Create("assets/userdashboard/103-100x100.jpg", "1D9E007DBB304E7464A0F1E5F6455B6E814923A0441AC134F6816ECFC3575C54"),
                Tuple.Create("assets/userdashboard/177-100x100.jpg", "3268D18B8981CFC0A85D4DF299CDF4958632E530846F8C9371F759620892DDA2"),
                Tuple.Create("assets/userdashboard/334-100x100.jpg", "C18314EDB7758BE3A16F2E30FD352F4E793A5E1442E005CCDE149B1F1328295C"),
                Tuple.Create("assets/userdashboard/338-100x100.jpg", "D34D047BE8AC5B3EDFEE48113DAB488062ED889BB449520C61D3220BD91BB4A0"),
                Tuple.Create("assets/userdashboard/342-100x100.jpg", "60C009611AE7464FE02D168571C15F956B15DFE84E4B795E0E59506C45C5F92F"),
                Tuple.Create("assets/userdashboard/349-100x100.jpg", "544A38020F7391CAA5B691C6F6D589B45D9F52C5C4255C2C1866BE3130DAB0FE"),
                Tuple.Create("assets/userdashboard/366-100x100.jpg", "18982F0F8B661E61D85524A11562094F1E5A348EDAA3538B2705EE12C1F80E1C"),
                Tuple.Create("assets/userdashboard/367-100x100.jpg", "FEBC0C60756786ED90CCC3F59847087927A32B40CFFC15B690B703577B377BAD"),
                Tuple.Create("assets/userdashboard/373-100x100.jpg", "5EB732F0A51EFC117CC60EE64601F5D33C6F197DE2B661840E363B0812639516"),
                Tuple.Create("assets/userdashboard/375-100x100.jpg", "AD173B25423E43AD2F92EECA4222843C08835DFB1BD10591B433F9CC0D047EEC"),
                Tuple.Create("assets/userdashboard/378-100x100.jpg", "C21745B7EDCF45D4E5FD9F32CB4F3950359EAC338983D2E780FFEEF5E976E79E"),
                Tuple.Create("assets/userdashboard/399-100x100.jpg", "94ABCE8A0DC2EE8C0EB1A48B2D731E8371BF1DAE43E70422B845CF54A7B825BF"),
                Tuple.Create("assets/userdashboard/447-100x100.jpg", "C4034F61BB6F3CC883D7002EA65E8606F6B2E2C57A15B9B57C360021CC19286F"),
                Tuple.Create("assets/userdashboard/453-100x100.jpg", "B0C3E8808B8AB8AEB12155C518FA9344F10D7C0FF81BEC9378A1EA98C40EB23F"),
                Tuple.Create("assets/userdashboard/469-100x100.jpg", "32E04DCC3C6EA70EF3094CEA7BFF2A7EA5367BBD417C96FF1E42474BEC6B4A1B"),
                Tuple.Create("assets/userdashboard/473-100x100.jpg", "A3B75ABB48EC434F74D984263451D75729550C269227EDABDEBA4580B756069E"),
                Tuple.Create("assets/userdashboard/505-100x100.jpg", "75D0A8D847DA5113D8CF9CDAF408CD4CD81E3180F6E89D717003CE032FAD2ECB")
            };

            var resourceNames = new HashSet<string>(GetGalleryResourceNames(), StringComparer.OrdinalIgnoreCase);

            foreach (var expectedAsset in expectedAssets)
            {
                Assert.IsTrue(
                    resourceNames.Contains(expectedAsset.Item1),
                    "Missing embedded WPF Gallery reference asset '{0}'.",
                    expectedAsset.Item1);

                var filePath = FindRepoFile(new[] { "ModernWpf.Gallery" }.Concat(expectedAsset.Item1.Split('/')).ToArray());
                Assert.AreEqual(
                    expectedAsset.Item2,
                    ComputeSha256(filePath),
                    "WPF Gallery reference asset should remain byte-identical: " + expectedAsset.Item1);
            }
        }

        [TestMethod]
        public void CatalogContainsWpfFirstGallerySurface()
        {
            Assert.AreEqual(12, GalleryCatalog.Groups.Count);
            Assert.AreEqual(82, GalleryCatalog.Items.Count);
        }

        [TestMethod]
        public void CatalogItemsMatchVisibleNavigationItems()
        {
            var visibleItemIds = GalleryCatalog.Groups
                .SelectMany(group => group.Items)
                .Select(item => item.UniqueId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var catalogItemIds = GalleryCatalog.Items
                .Select(item => item.UniqueId)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            CollectionAssert.AreEqual(visibleItemIds, catalogItemIds);
        }

        private static void AssertNavigationItem(string uniqueId, string subtitle, string imageFileName)
        {
            var item = GalleryCatalog.FindItem(uniqueId);

            Assert.IsNotNull(item, uniqueId);
            Assert.AreEqual(subtitle, item.Subtitle, uniqueId);
            Assert.AreEqual(subtitle, item.Description, uniqueId);
            AssertItemImage(item, imageFileName);
        }

        private static void AssertGroupCard(string uniqueId, string title, string subtitle, string imageFileName)
        {
            var group = GalleryCatalog.FindGroup(uniqueId);

            Assert.IsNotNull(group, uniqueId);
            Assert.AreEqual(title, group.Title, uniqueId);
            Assert.AreEqual(subtitle, group.Subtitle, uniqueId);
            Assert.AreEqual(subtitle, group.Description, uniqueId);
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

        private static string[] GetGalleryResourceNames()
        {
            var assembly = typeof(GalleryCatalog).Assembly;
            var resourceName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                Assert.IsNotNull(stream, resourceName);

                using (var reader = new ResourceReader(stream))
                {
                    return reader
                        .Cast<object>()
                        .Select(entry => ((System.Collections.DictionaryEntry)entry).Key)
                        .Cast<string>()
                        .Select(key => key.ToLowerInvariant())
                        .ToArray();
                }
            }
        }

        private static string ComputeSha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha256 = SHA256.Create())
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static string FindRepoFile(params string[] relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativePath).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Could not find repository file '{0}'.", string.Join(Path.DirectorySeparatorChar.ToString(), relativePath));
            return null;
        }

        private static string FindRepoDirectory(params string[] relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(new[] { directory.FullName }.Concat(relativePath).ToArray());
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Could not find repository directory '{0}'.", string.Join(Path.DirectorySeparatorChar.ToString(), relativePath));
            return null;
        }

        private static IEnumerable<string> CreateDeletedPageImplementationMarkers(string itemId)
        {
            if (itemId.EndsWith("Page", StringComparison.OrdinalIgnoreCase))
            {
                yield return itemId;
            }

            yield return "GallerySample_" + itemId;
            yield return "Create" + itemId;
            yield return itemId + "Page";
            yield return "case \"" + itemId + "\"";
            yield return "\"UniqueId\": \"" + itemId + "\"";
        }

        private static string GetGalleryResourceKey(string imagePath)
        {
            const string PackPrefix = "pack://application:,,,/";
            const string GalleryPackPrefix = "pack://application:,,,/ModernWpf.Gallery;component/";

            Assert.IsTrue(
                imagePath.StartsWith(PackPrefix, StringComparison.OrdinalIgnoreCase),
                imagePath);

            var resourcePath = imagePath.StartsWith(GalleryPackPrefix, StringComparison.OrdinalIgnoreCase)
                ? imagePath.Substring(GalleryPackPrefix.Length)
                : imagePath.Substring(PackPrefix.Length);

            return resourcePath
                .Replace('\\', '/')
                .TrimStart('/')
                .ToLowerInvariant();
        }
    }
}
