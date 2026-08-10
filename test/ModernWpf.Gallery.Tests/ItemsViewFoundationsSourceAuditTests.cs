using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class ItemsViewFoundationsSourceAuditTests
    {
        [TestMethod]
        public void GalleryUsesRealItemContainerAndLinedFlowLayoutControls()
        {
            WpfTestHost.Run(() =>
            {
                var itemContainerItem = GalleryCatalog.FindItem("ItemContainer");
                var linedFlowLayoutItem = GalleryCatalog.FindItem("LinedFlowLayout");

                Assert.IsNotNull(itemContainerItem);
                Assert.IsNotNull(linedFlowLayoutItem);
                Assert.AreEqual("ModernWpf.Controls", itemContainerItem.ApiNamespace);
                Assert.AreEqual("ModernWpf.Controls", linedFlowLayoutItem.ApiNamespace);
                Assert.IsTrue(itemContainerItem.IsNew);
                Assert.IsTrue(linedFlowLayoutItem.IsNew);
                Assert.IsTrue(itemContainerItem.Docs.Any(link => link.Title == "ItemContainer - API"));
                Assert.IsTrue(linedFlowLayoutItem.Docs.Any(link => link.Title == "LinedFlowLayout - API"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(item => item.UniqueId == "ItemContainer"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(item => item.UniqueId == "LinedFlowLayout"));

                var itemExamples = ItemsViewFoundationsSampleFactory.CreateExamples("ItemContainer");
                var layoutExamples = ItemsViewFoundationsSampleFactory.CreateExamples("LinedFlowLayout");
                Assert.AreEqual(1, itemExamples.Count);
                Assert.AreEqual(1, layoutExamples.Count);
                Assert.IsNull(ItemsViewFoundationsSampleFactory.Create("ItemsView"));

                var itemContent = (FrameworkElement)itemExamples[0].ExampleContent;
                using (var host = new WindowHost(itemContent, 820, 420))
                {
                    var itemContainer = Descendants(itemContent).OfType<Mux.ItemContainer>().Single();
                    var options = (DependencyObject)itemExamples[0].OptionsContent;
                    var selected = FindByAutomationId<CheckBox>(
                        options,
                        GalleryAutomation.SampleElementId("ItemContainer", "SelectedOption"));
                    var enabled = FindByAutomationId<CheckBox>(
                        options,
                        GalleryAutomation.SampleElementId("ItemContainer", "EnabledOption"));
                    var status = FindByAutomationId<TextBlock>(
                        options,
                        GalleryAutomation.SampleElementId("ItemContainer", "Status"));
                    Assert.IsTrue(itemContainer.IsSelected);
                    selected.IsChecked = false;
                    Assert.IsFalse(itemContainer.IsSelected);
                    Assert.AreEqual("Not selected", status.Text);
                    enabled.IsChecked = false;
                    Assert.IsFalse(itemContainer.IsEnabled);
                    Assert.AreEqual("Disabled", status.Text);
                }

                var layoutContent = (FrameworkElement)layoutExamples[0].ExampleContent;
                using (var host = new WindowHost(layoutContent, 980, 640))
                {
                    var repeater = Descendants(layoutContent).OfType<Mux.ItemsRepeater>().Single();
                    var layout = (Mux.LinedFlowLayout)repeater.Layout;
                    var options = (DependencyObject)layoutExamples[0].OptionsContent;
                    var justification = Descendants(options).OfType<ComboBox>().Single();
                    var fill = Descendants(options).OfType<CheckBox>().Single();
                    var lineHeight = Descendants(options).OfType<Slider>().Single();
                    var add = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("LinedFlowLayout", "AddItem"));
                    var remove = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("LinedFlowLayout", "RemoveItem"));
                    var scroll = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("LinedFlowLayout", "ScrollToItem60"));
                    var scrollViewer = FindByAutomationId<ScrollViewer>(
                        layoutContent,
                        GalleryAutomation.SampleElementId("LinedFlowLayout", "ScrollViewer"));
                    var status = FindByAutomationId<TextBlock>(
                        layoutContent,
                        GalleryAutomation.SampleElementId("LinedFlowLayout", "Status"));
                    var items = (IList)repeater.ItemsSource;

                    Assert.IsGreaterThan(0.0, layout.ActualLineHeight);
                    Assert.IsLessThan(
                        80,
                        Enumerable.Range(0, 80).Count(index => repeater.TryGetElement(index) != null));

                    justification.SelectedItem = Mux.LinedFlowLayoutItemsJustification.SpaceEvenly;
                    fill.IsChecked = false;
                    lineHeight.Value = 120;
                    WpfTestHost.DoEvents();
                    host.Window.UpdateLayout();

                    Assert.AreEqual(Mux.LinedFlowLayoutItemsJustification.SpaceEvenly, layout.ItemsJustification);
                    Assert.AreEqual(Mux.LinedFlowLayoutItemsStretch.None, layout.ItemsStretch);
                    Assert.AreEqual(120.0, layout.LineHeight);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(status.Text));

                    object originalFirstItem = items[0];
                    add.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(81, items.Count);
                    Assert.AreNotSame(originalFirstItem, items[0]);
                    remove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(80, items.Count);
                    Assert.AreSame(originalFirstItem, items[0]);

                    scroll.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    WpfTestHost.DoEvents();
                    host.Window.UpdateLayout();
                    Assert.IsGreaterThan(0.0, scrollViewer.VerticalOffset);
                    Assert.IsLessThan(
                        80,
                        Enumerable.Range(0, 80).Count(index => repeater.TryGetElement(index) != null));
                }
            });
        }

        [TestMethod]
        public void ProductAndGalleryPinCurrentSourcesAndDocumentWpfAdaptations()
        {
            string root = FindRepoRoot();
            string itemAudit = Read(root, "docs", "itemcontainer-winui3-source-audit.md");
            string layoutAudit = Read(root, "docs", "linedflowlayout-winui3-source-audit.md");
            string scrollingAudit = Read(root, "docs", "itemsview-scrolling-wpf-adaptation.md");
            string itemProduct = Read(root, "ModernWpf.Controls", "ItemContainer", "ItemContainer.cs");
            string itemAutomationPeer = Read(root, "ModernWpf.Controls", "ItemContainer", "ItemContainerAutomationPeer.cs");
            string itemStrings = Read(root, "ModernWpf.Controls", "ItemContainer", "Strings", "Resources.resx");
            string layoutProduct = Read(root, "ModernWpf.Controls", "Repeater", "Layouts", "LinedFlowLayout", "LinedFlowLayout.cs");
            string scrollHost = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsViewScrollHost.cs");
            string galleryFactory = Read(root, "ModernWpf.Gallery", "Pages", "ItemsViewFoundationsSampleFactory.cs");
            string lightResources = Read(root, "ModernWpf", "ThemeResources", "Light.xaml");
            string darkResources = Read(root, "ModernWpf", "ThemeResources", "Dark.xaml");
            string highContrastResources = Read(root, "ModernWpf", "ThemeResources", "HighContrast.xaml");
            string sharedResources = Read(root, "ModernWpf", "ModernWpfControlsResources.xaml");

            foreach (string audit in new[] { itemAudit, layoutAudit, scrollingAudit })
            {
                StringAssert.Contains(audit, "23a73be03d194ea0ece97da71de98b6b53021b70");
                StringAssert.Contains(audit, "WPF");
            }

            StringAssert.Contains(itemAudit, "89a7d29e365eb73d31fed61b81d373f6ba3ed8b2");
            StringAssert.Contains(itemAudit, "b56c1a0b4abbe42b77c8cd74e47e42a2d7e8a2a6");
            StringAssert.Contains(itemAudit, "d8430b5ee5ce7aff9dd6e60f849c59c869db6411");
            StringAssert.Contains(layoutAudit, "862ec26d51942f7f47c4290ab21638af079c4129");
            StringAssert.Contains(layoutAudit, "cf8659f59e35c680aed5ed6f0e66061429304645");
            StringAssert.Contains(layoutAudit, "efc5a62df9fcb25805215bdd56cee76a2b060cff");
            StringAssert.Contains(layoutAudit, "ItemCollectionTransitionProvider");
            StringAssert.Contains(scrollingAudit, "System.Windows.Controls.ScrollViewer");
            StringAssert.Contains(scrollingAudit, "not a new ModernWPF");

            StringAssert.Contains(itemProduct, "class ItemContainer : Control");
            StringAssert.Contains(itemProduct, "ItemContainerAutomationPeer");
            StringAssert.Contains(itemAutomationPeer, "SR_ItemContainerDefaultControlName");
            StringAssert.Contains(itemStrings, "ItemContainerDefaultControlName");
            StringAssert.Contains(layoutProduct, "BuildLines");
            StringAssert.Contains(layoutProduct, "LockItemToLine");
            StringAssert.Contains(layoutProduct, "RequestedRangeStartIndex");
            StringAssert.Contains(layoutProduct, "CreateDefaultItemTransitionProvider");
            StringAssert.Contains(scrollHost, "ScrollToRequested");
            StringAssert.Contains(scrollHost, "NotifyRequestedScrollCompleted");
            StringAssert.Contains(galleryFactory, "new Mux.ItemContainer");
            StringAssert.Contains(galleryFactory, "new Mux.LinedFlowLayout");
            StringAssert.Contains(galleryFactory, "new Mux.ItemsRepeaterScrollHost");

            string itemTemplate = Read(root, "ModernWpf.Controls", "ItemContainer", "ItemContainer.xaml");
            StringAssert.Contains(itemTemplate, "ItemContainerSelectionVisualBackground");
            StringAssert.Contains(itemTemplate, "ItemContainerCheckboxMargin");
            foreach (string normalThemeResources in new[] { lightResources, darkResources })
            {
                StringAssert.Contains(normalThemeResources, "ItemContainerSelectionVisualBackground");
                StringAssert.Contains(normalThemeResources, "AccentFillColorDefaultBrush");
                StringAssert.Contains(normalThemeResources, "ItemContainerCheckboxBackgroundUnchecked");
                StringAssert.Contains(normalThemeResources, "ControlOnImageFillColorDefaultBrush");
            }

            StringAssert.Contains(highContrastResources, "ItemContainerSelectionVisualBackground");
            StringAssert.Contains(highContrastResources, "SystemControlHighlightListAccentLowBrush");
            StringAssert.Contains(highContrastResources, "ItemContainerCheckboxBackgroundUnchecked");
            StringAssert.Contains(highContrastResources, "SystemColorButtonFaceColorBrush");
            StringAssert.Contains(sharedResources, "ItemContainerCheckboxMargin");
            StringAssert.Contains(sharedResources, "ItemContainerSelectedInnerThickness");
        }

        private static T FindByAutomationId<T>(DependencyObject root, string automationId)
            where T : DependencyObject
        {
            return Descendants(root)
                .OfType<T>()
                .Single(element => AutomationProperties.GetAutomationId(element) == automationId);
        }

        private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root == null)
            {
                yield break;
            }

            yield return root;
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int index = 0; index < count; index++)
            {
                foreach (DependencyObject child in Descendants(VisualTreeHelper.GetChild(root, index)))
                {
                    yield return child;
                }
            }
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

        private static string Read(string root, params string[] segments)
        {
            return File.ReadAllText(Path.Combine(new[] { root }.Concat(segments).ToArray()));
        }

        private sealed class WindowHost : IDisposable
        {
            internal WindowHost(FrameworkElement content, double width, double height)
            {
                Window = new Window
                {
                    Width = width,
                    Height = height,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    Content = content
                };
                Window.Show();
                Window.UpdateLayout();
                WpfTestHost.DoEvents();
            }

            internal Window Window { get; }

            public void Dispose()
            {
                Window.Content = null;
                Window.Close();
                WpfTestHost.DoEvents();
            }
        }
    }
}
