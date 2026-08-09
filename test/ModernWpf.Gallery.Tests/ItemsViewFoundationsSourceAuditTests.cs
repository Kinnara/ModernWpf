using System;
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
        public void GalleryUsesRealItemsViewFoundationsAndControl()
        {
            WpfTestHost.Run(() =>
            {
                var itemContainerItem = GalleryCatalog.FindItem("ItemContainer");
                var itemsViewItem = GalleryCatalog.FindItem("ItemsView");
                var linedFlowLayoutItem = GalleryCatalog.FindItem("LinedFlowLayout");

                Assert.IsNotNull(itemContainerItem);
                Assert.IsNotNull(itemsViewItem);
                Assert.IsNotNull(linedFlowLayoutItem);
                Assert.AreEqual("ModernWpf.Controls", itemContainerItem.ApiNamespace);
                Assert.AreEqual("ModernWpf.Controls", itemsViewItem.ApiNamespace);
                Assert.AreEqual("ModernWpf.Controls", linedFlowLayoutItem.ApiNamespace);
                Assert.IsTrue(itemContainerItem.IsNew);
                Assert.IsTrue(itemsViewItem.IsNew);
                Assert.IsTrue(linedFlowLayoutItem.IsNew);
                Assert.IsTrue(itemContainerItem.Docs.Any(link => link.Title == "ItemContainer - API"));
                Assert.IsTrue(itemsViewItem.Docs.Any(link => link.Title == "ItemsView - API"));
                Assert.IsTrue(linedFlowLayoutItem.Docs.Any(link => link.Title == "LinedFlowLayout - API"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(item => item.UniqueId == "ItemContainer"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(item => item.UniqueId == "ItemsView"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(item => item.UniqueId == "LinedFlowLayout"));

                var itemExamples = ItemsViewFoundationsSampleFactory.CreateExamples("ItemContainer");
                var itemsViewExamples = ItemsViewFoundationsSampleFactory.CreateExamples("ItemsView");
                var layoutExamples = ItemsViewFoundationsSampleFactory.CreateExamples("LinedFlowLayout");
                Assert.AreEqual(1, itemExamples.Count);
                Assert.AreEqual(3, itemsViewExamples.Count);
                Assert.AreEqual(1, layoutExamples.Count);
                Assert.IsInstanceOfType<FrameworkElement>(ItemsViewFoundationsSampleFactory.Create("ItemsView"));

                var itemContent = (FrameworkElement)itemExamples[0].ExampleContent;
                using (var host = new WindowHost(itemContent, 820, 420))
                {
                    var itemContainer = Descendants(itemContent).OfType<Mux.ItemContainer>().Single();
                    var selected = Descendants((DependencyObject)itemExamples[0].OptionsContent)
                        .OfType<CheckBox>()
                        .Single();
                    Assert.IsTrue(itemContainer.IsSelected);
                    selected.IsChecked = false;
                    Assert.IsFalse(itemContainer.IsSelected);
                }

                var basicItemsViewContent = (FrameworkElement)itemsViewExamples[0].ExampleContent;
                using (var host = new WindowHost(basicItemsViewContent, 980, 680))
                {
                    var itemsView = Descendants(basicItemsViewContent).OfType<Mux.ItemsView>().Single();
                    var itemContainer = Descendants(itemsView).OfType<Mux.ItemContainer>().First();
                    var output = Descendants(basicItemsViewContent)
                        .OfType<TextBlock>()
                        .Single(textBlock => AutomationProperties.GetName(textBlock) == "Invocation result");

                    itemContainer.RaiseItemInvoked(
                        Mux.ItemContainerInteractionTrigger.DoubleClick,
                        itemContainer);

                    StringAssert.StartsWith(output.Text, "Invoked: Item ");
                    Assert.IsNotNull(itemsView.ScrollView);
                    Assert.IsTrue(itemsView.IsItemInvokedEnabled);
                }

                var layoutsItemsViewContent = (FrameworkElement)itemsViewExamples[1].ExampleContent;
                using (var host = new WindowHost(layoutsItemsViewContent, 980, 680))
                {
                    var itemsView = Descendants(layoutsItemsViewContent).OfType<Mux.ItemsView>().Single();
                    var layoutSelector = Descendants((DependencyObject)itemsViewExamples[1].OptionsContent)
                        .OfType<ComboBox>()
                        .Single();

                    layoutSelector.SelectedItem = "StackLayout";
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType<Mux.StackLayout>(itemsView.Layout);

                    layoutSelector.SelectedItem = "LinedFlowLayout";
                    WpfTestHost.DoEvents();
                    host.Window.UpdateLayout();
                    Assert.IsInstanceOfType<Mux.LinedFlowLayout>(itemsView.Layout);
                    Assert.IsGreaterThan(0.0, ((Mux.LinedFlowLayout)itemsView.Layout).ActualLineHeight);
                }

                var selectionItemsViewContent = (FrameworkElement)itemsViewExamples[2].ExampleContent;
                using (var host = new WindowHost(selectionItemsViewContent, 980, 680))
                {
                    var itemsView = Descendants(selectionItemsViewContent).OfType<Mux.ItemsView>().Single();
                    itemsView.SelectAll();
                    Assert.AreEqual(18, itemsView.SelectedItems.Count);
                    itemsView.InvertSelection();
                    Assert.AreEqual(0, itemsView.SelectedItems.Count);
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
                }
            });
        }

        [TestMethod]
        public void ProductAndGalleryPinCurrentSourcesAndDocumentWpfAdaptations()
        {
            string root = FindRepoRoot();
            string itemAudit = Read(root, "docs", "itemcontainer-winui3-source-audit.md");
            string layoutAudit = Read(root, "docs", "linedflowlayout-winui3-source-audit.md");
            string itemsViewAudit = Read(root, "docs", "itemsview-winui3-source-audit.md");
            string scrollingAudit = Read(root, "docs", "itemsview-scrolling-wpf-adaptation.md");
            string itemProduct = Read(root, "ModernWpf.Controls", "ItemContainer", "ItemContainer.cs");
            string itemAutomationPeer = Read(root, "ModernWpf.Controls", "ItemContainer", "ItemContainerAutomationPeer.cs");
            string itemStrings = Read(root, "ModernWpf.Controls", "ItemContainer", "Strings", "Resources.resx");
            string itemsViewProduct = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsView.cs");
            string itemsViewAutomationPeer = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsViewAutomationPeer.cs");
            string itemsViewStyle = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsView.xaml");
            string layoutProduct = Read(root, "ModernWpf.Controls", "Repeater", "Layouts", "LinedFlowLayout", "LinedFlowLayout.cs");
            string scrollHost = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsViewScrollHost.cs");
            string galleryFactory = Read(root, "ModernWpf.Gallery", "Pages", "ItemsViewFoundationsSampleFactory.cs");

            foreach (string audit in new[] { itemAudit, layoutAudit, itemsViewAudit, scrollingAudit })
            {
                StringAssert.Contains(audit, "e1aa8f64df98d6229f6cd4074d59b654616254da");
                StringAssert.Contains(audit, "WPF");
            }

            StringAssert.Contains(itemAudit, "89a7d29e365eb73d31fed61b81d373f6ba3ed8b2");
            StringAssert.Contains(itemAudit, "b56c1a0b4abbe42b77c8cd74e47e42a2d7e8a2a6");
            StringAssert.Contains(itemAudit, "d8430b5ee5ce7aff9dd6e60f849c59c869db6411");
            StringAssert.Contains(layoutAudit, "862ec26d51942f7f47c4290ab21638af079c4129");
            StringAssert.Contains(layoutAudit, "cf8659f59e35c680aed5ed6f0e66061429304645");
            StringAssert.Contains(itemsViewAudit, "9237bfe51b411d4c5b498bf93f064270c76e3bf3");
            StringAssert.Contains(itemsViewAudit, "65acb3f3eda381808c855ceeafd3ecf0e86a34f9");
            StringAssert.Contains(itemsViewAudit, "bbb693eb23ed098023c1794cf44390f534e63ebb");
            StringAssert.Contains(itemsViewAudit, "36f27f326076677ef2da671410bad69a755004e7");
            StringAssert.Contains(itemsViewAudit, "System.Windows.Controls.ScrollViewer");
            StringAssert.Contains(itemsViewAudit, "ItemTransitionProvider");
            StringAssert.Contains(itemsViewAudit, "adds no ItemsView keys");
            StringAssert.Contains(scrollingAudit, "System.Windows.Controls.ScrollViewer");
            StringAssert.Contains(scrollingAudit, "not a new ModernWPF");

            StringAssert.Contains(itemProduct, "class ItemContainer : Control");
            StringAssert.Contains(itemProduct, "ItemContainerAutomationPeer");
            StringAssert.Contains(itemAutomationPeer, "SR_ItemContainerDefaultControlName");
            StringAssert.Contains(itemStrings, "ItemContainerDefaultControlName");
            StringAssert.Contains(itemsViewProduct, "class ItemsView : Control");
            StringAssert.Contains(itemsViewProduct, "SelectAllFlat");
            StringAssert.Contains(itemsViewProduct, "StartBringItemIntoView");
            StringAssert.Contains(itemsViewProduct, "ItemTemplate's root element must be an ItemContainer.");
            StringAssert.Contains(itemsViewProduct, "#if NET48_OR_NEWER");
            StringAssert.Contains(itemsViewAutomationPeer, "ISelectionProvider");
            StringAssert.Contains(itemsViewStyle, "PART_ScrollView");
            StringAssert.Contains(itemsViewStyle, "PART_ItemsRepeater");
            StringAssert.Contains(itemsViewStyle, "local:StackLayout");
            StringAssert.Contains(layoutProduct, "FlowAlgorithm.Measure");
            StringAssert.Contains(layoutProduct, "LockItemToLine");
            StringAssert.Contains(layoutProduct, "RequestedRangeStartIndex");
            StringAssert.Contains(scrollHost, "ScrollToRequested");
            StringAssert.Contains(scrollHost, "NotifyRequestedScrollCompleted");
            StringAssert.Contains(galleryFactory, "new Mux.ItemContainer");
            StringAssert.Contains(galleryFactory, "new Mux.LinedFlowLayout");
            StringAssert.Contains(galleryFactory, "new Mux.ItemsRepeaterScrollHost");
            StringAssert.Contains(galleryFactory, "new Mux.ItemsView");

            foreach (string theme in new[] { "Light.xaml", "Dark.xaml", "HighContrast.xaml" })
            {
                string resources = Read(root, "ModernWpf", "ThemeResources", theme);
                StringAssert.Contains(resources, "ItemContainerSelectionVisualBackground");
                StringAssert.Contains(resources, "ItemContainerCheckboxBackgroundUnchecked");
            }
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
