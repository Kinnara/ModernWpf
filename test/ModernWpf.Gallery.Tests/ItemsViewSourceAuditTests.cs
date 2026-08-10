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
    public class ItemsViewSourceAuditTests
    {
        [TestMethod]
        public void GalleryExercisesItemsViewSelectionInvocationLayoutsAndTransitions()
        {
            WpfTestHost.Run(() =>
            {
                var catalogItem = GalleryCatalog.FindItem("ItemsView");

                Assert.IsNotNull(catalogItem);
                Assert.AreEqual("ModernWpf.Controls", catalogItem.ApiNamespace);
                Assert.IsTrue(catalogItem.IsNew);
                Assert.IsTrue(catalogItem.Docs.Any(link => link.Title == "ItemsView - API"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(item => item.UniqueId == "ItemsView"));

                var examples = ItemsViewSampleFactory.CreateExamples("ItemsView");
                Assert.AreEqual(3, examples.Count);
                Assert.IsInstanceOfType<FrameworkElement>(ItemsViewSampleFactory.Create("ItemsView"));
                Assert.IsNull(ItemsViewSampleFactory.Create("ItemsRepeater"));

                var basicContent = (FrameworkElement)examples[0].ExampleContent;
                using (var host = new WindowHost(basicContent, 980, 680))
                {
                    var itemsView = Descendants(basicContent).OfType<Mux.ItemsView>().Single();
                    var itemContainer = Descendants(itemsView).OfType<Mux.ItemContainer>().First();
                    var output = FindByAutomationId<TextBlock>(
                        basicContent,
                        GalleryAutomation.SampleElementId("ItemsView", "InvocationResult"));

                    itemsView.Select(0);
                    Assert.AreEqual(1, itemsView.SelectedItems.Count);

                    itemContainer.RaiseItemInvoked(
                        Mux.ItemContainerInteractionTrigger.DoubleTap,
                        itemContainer);

                    StringAssert.StartsWith(output.Text, "Invoked: Item ");
                    Assert.IsNotNull(itemsView.ScrollView);
                    Assert.IsTrue(itemsView.IsItemInvokedEnabled);
                }

                var layoutsContent = (FrameworkElement)examples[1].ExampleContent;
                using (var host = new WindowHost(layoutsContent, 980, 680))
                {
                    var itemsView = Descendants(layoutsContent).OfType<Mux.ItemsView>().Single();
                    var repeater = Descendants(itemsView).OfType<Mux.ItemsRepeater>().Single();
                    var options = (DependencyObject)examples[1].OptionsContent;
                    var layoutSelector = FindByAutomationId<ComboBox>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "LayoutSelector"));
                    var add = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "AddItem"));
                    var remove = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "RemoveItem"));
                    var items = (IList)itemsView.ItemsSource;

                    Assert.IsInstanceOfType<Mux.UniformGridLayout>(itemsView.Layout);
                    Assert.IsNull(itemsView.ItemTransitionProvider);

                    layoutSelector.SelectedItem = "StackLayout";
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType<Mux.StackLayout>(itemsView.Layout);
                    Assert.IsNull(itemsView.ItemTransitionProvider);

                    layoutSelector.SelectedItem = "LinedFlowLayout";
                    WpfTestHost.DoEvents();
                    host.Window.UpdateLayout();
                    Assert.IsInstanceOfType<Mux.LinedFlowLayout>(itemsView.Layout);
                    Assert.IsInstanceOfType<Mux.LinedFlowLayoutItemCollectionTransitionProvider>(
                        itemsView.ItemTransitionProvider);
                    Assert.AreSame(itemsView.ItemTransitionProvider, repeater.ItemTransitionProvider);
                    Assert.IsGreaterThan(0.0, ((Mux.LinedFlowLayout)itemsView.Layout).ActualLineHeight);

                    add.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(81, items.Count);
                    remove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(80, items.Count);

                    layoutSelector.SelectedItem = "UniformGridLayout";
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType<Mux.UniformGridLayout>(itemsView.Layout);
                    Assert.IsNull(itemsView.ItemTransitionProvider);
                }

                var selectionContent = (FrameworkElement)examples[2].ExampleContent;
                using (var host = new WindowHost(selectionContent, 980, 680))
                {
                    var itemsView = Descendants(selectionContent).OfType<Mux.ItemsView>().Single();
                    var options = (DependencyObject)examples[2].OptionsContent;
                    var mode = FindByAutomationId<ComboBox>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "SelectionMode"));
                    var invocation = FindByAutomationId<CheckBox>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "InvocationEnabled"));
                    var selectAll = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "SelectAll"));
                    var clear = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "ClearSelection"));
                    var invert = FindByAutomationId<Button>(
                        options,
                        GalleryAutomation.SampleElementId("ItemsView", "InvertSelection"));
                    var status = FindByAutomationId<TextBlock>(
                        selectionContent,
                        GalleryAutomation.SampleElementId("ItemsView", "SelectionStatus"));

                    Assert.AreEqual(Mux.ItemsViewSelectionMode.Multiple, itemsView.SelectionMode);
                    mode.SelectedItem = Mux.ItemsViewSelectionMode.Extended;
                    invocation.IsChecked = false;
                    Assert.AreEqual(Mux.ItemsViewSelectionMode.Extended, itemsView.SelectionMode);
                    Assert.IsFalse(itemsView.IsItemInvokedEnabled);

                    selectAll.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(18, itemsView.SelectedItems.Count);
                    StringAssert.StartsWith(status.Text, "Selected: Item 1");
                    invert.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(0, itemsView.SelectedItems.Count);
                    Assert.AreEqual("No items selected.", status.Text);
                    selectAll.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    clear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(0, itemsView.SelectedItems.Count);
                }
            });
        }

        [TestMethod]
        public void ProductAndGalleryPinCurrentItemsViewSourcesAndWpfAdaptations()
        {
            string root = FindRepoRoot();
            string audit = Read(root, "docs", "itemsview-winui3-source-audit.md");
            string product = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsView.cs");
            string automationPeer = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsViewAutomationPeer.cs");
            string style = Read(root, "ModernWpf.Controls", "ItemsView", "ItemsView.xaml");
            string scrollViewerAdapter = Read(root, "ModernWpf.Controls", "Repeater", "ScrollViewerExtensions.cs");
            string scrollHost = Read(
                root,
                "ModernWpf.Controls",
                "Repeater",
                "ItemsRepeater",
                "ItemsRepeaterScrollHost.cs");
            string galleryFactory = Read(root, "ModernWpf.Gallery", "Pages", "ItemsViewSampleFactory.cs");

            StringAssert.Contains(audit, "23a73be03d194ea0ece97da71de98b6b53021b70");
            StringAssert.Contains(audit, "a97562621a1d1ea397a38a3f512c9eef99db52d8");
            StringAssert.Contains(audit, "b78c440193aab788215888561e45adf72da848cb");
            StringAssert.Contains(audit, "78488c1735dbce861d3dcf57ff2666e522201492");
            StringAssert.Contains(audit, "System.Windows.Controls.ScrollViewer");
            StringAssert.Contains(audit, "ItemTransitionProvider");
            StringAssert.Contains(audit, "ScrollContentPresenter");
            StringAssert.Contains(audit, "adds no ItemsView-specific public resource keys");

            StringAssert.Contains(product, "class ItemsView : Control");
            StringAssert.Contains(product, "SelectAllFlat");
            StringAssert.Contains(product, "StartBringItemIntoView");
            StringAssert.Contains(product, "ItemTemplate's root element must be an ItemContainer.");
            StringAssert.Contains(product, "ItemTransitionProvider");
            StringAssert.Contains(automationPeer, "ISelectionProvider");
            StringAssert.Contains(style, "PART_ScrollView");
            StringAssert.Contains(style, "PART_ItemsRepeater");
            StringAssert.Contains(style, "local:StackLayout");
            StringAssert.Contains(scrollViewerAdapter, "GetEffectiveViewportSize");
            StringAssert.Contains(scrollHost, "InvalidateScrollInfo");
            StringAssert.Contains(galleryFactory, "new Mux.ItemsView");
            StringAssert.Contains(galleryFactory, "new Mux.LinedFlowLayout");
            StringAssert.Contains(galleryFactory, "LinedFlowLayoutItemCollectionTransitionProvider");
        }

        [TestMethod]
        public void ItemsViewGalleryEvidenceUsesWinAppPhysicalInputAndRealHighContrastGuard()
        {
            string root = FindRepoRoot();
            string script = Read(
                root,
                "tools",
                "visual-checks",
                "Invoke-ItemsViewGalleryEvidence.ps1");

            StringAssert.Contains(script, "modernwpf-itemsview-gallery-evidence-v1");
            StringAssert.Contains(script, "WinApp Windows.Graphics.Capture + physical pointer + SendInput keyboard");
            StringAssert.Contains(script, "GallerySample_ItemsView_PrimaryItemsView");
            StringAssert.Contains(script, "GallerySample_ItemsView_LayoutSelector");
            StringAssert.Contains(script, "GallerySample_ItemsView_SelectionItemsView");
            StringAssert.Contains(script, "GallerySample_ItemsView_SelectionStatus");
            StringAssert.Contains(script, "--double");
            StringAssert.Contains(script, "ctrl+a");
            StringAssert.Contains(script, "[System.Windows.SystemParameters]::HighContrast");
            StringAssert.Contains(script, "Requested themes do not match real OS High Contrast state");
            StringAssert.Contains(script, "git -C $RepositoryRoot status --porcelain");
            StringAssert.Contains(script, "[Environment]::OSVersion.Version.ToString()");
            StringAssert.Contains(script, "WinApp Windows.Graphics.Capture");
            StringAssert.Contains(script, "SHA256SUMS");
            StringAssert.Contains(script, "net462");
            StringAssert.Contains(script, "net8.0-windows7.0");
            StringAssert.Contains(script, "net10.0-windows7.0");
            Assert.IsFalse(script.Contains("UIAutomationClient", StringComparison.Ordinal));
            Assert.IsFalse(script.Contains("System.Windows.Automation", StringComparison.Ordinal));
            Assert.IsFalse(script.Contains("D:\\repos\\ModernWpf", StringComparison.Ordinal));
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
