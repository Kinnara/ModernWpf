using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class TabViewSourceAuditTests
    {
        [TestMethod]
        public void PinnedGalleryExamplesUseTheRealControlAndExerciseInteractions()
        {
            WpfTestHost.Run(() =>
            {
                var item = GalleryCatalog.FindItem("TabView");
                Assert.IsNotNull(item);
                Assert.AreEqual("Navigation", item.GroupId);
                Assert.AreEqual("ModernWpf.Controls", item.ApiNamespace);
                Assert.IsTrue(item.IsNew);
                Assert.IsTrue(item.Docs.Any(link => link.Title == "TabView - API"));
                Assert.IsTrue(
                    GalleryCatalog.FindGroup("ModernWpfControls").Items.Any(candidate => candidate.UniqueId == "TabView"));

                var examples = TabViewSampleFactory.CreateExamples("TabView");
                Assert.AreEqual(10, examples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "A TabView with support for adding, closing, and rearranging tabs",
                        "A TabView with TabViewItems defined in markup",
                        "A TabView bound to a collection of MyData objects",
                        "A TabView with keyboarding support",
                        "You can put custom content in TabStripHeader and TabStripFooter",
                        "Tab widths can either be equally sized, sized to the content of the tab, or sized to only show the icon when unselected",
                        "The close button can be persistent or only visible on hover",
                        "TabView with color tab icons",
                        "A TabView with accent colored TabStrip background",
                        "Complete TabView windowing sample"
                    },
                    examples.Select(example => example.HeaderText).ToArray());

                foreach (var example in examples)
                {
                    Assert.IsTrue(
                        Descendants(example.ExampleContent as DependencyObject).OfType<Mux.TabView>().Any(),
                        "Every Preview 5 example must contain the real ModernWpf.Controls.TabView: " + example.HeaderText);
                    StringAssert.Contains(example.XamlCode, "TabView");
                }

                AssertAddCloseAndContextMove(examples[0].ExampleContent as FrameworkElement);
                AssertOverflowAndSelection(examples[5].ExampleContent as FrameworkElement);
                AssertWindowTearOutAndRejoin(examples[9].ExampleContent as FrameworkElement);

                Assert.IsNotNull(TabViewSampleFactory.Create("TabView"));
                Assert.IsNull(TabViewSampleFactory.Create("TabControl"));
            });
        }

        [TestMethod]
        public void GalleryPortPinsCurrentWinUIAndDocumentsTheWpfWindowAdaptation()
        {
            var root = FindRepoRoot();
            var audit = File.ReadAllText(Path.Combine(root, "docs", "tabview-winui3-source-audit.md"));
            var factory = File.ReadAllText(Path.Combine(root, "ModernWpf.Gallery", "Pages", "TabViewSampleFactory.cs"));
            var product = File.ReadAllText(Path.Combine(root, "ModernWpf.Controls", "TabView", "TabView.cs"));

            StringAssert.Contains(audit, "e1aa8f64df98d6229f6cd4074d59b654616254da");
            StringAssert.Contains(audit, "3669519356c67f1376152c33ed8ea45003a91f3a");
            StringAssert.Contains(audit, "WPF Window");
            StringAssert.Contains(audit, "subsequent ordinary WPF tab drag");
            StringAssert.Contains(factory, "A TabView with support for adding, closing, and rearranging tabs");
            StringAssert.Contains(factory, "Tab widths can either be equally sized");
            StringAssert.Contains(factory, "Complete TabView windowing sample");
            StringAssert.Contains(factory, "TabTearOutWindowRequested");
            StringAssert.Contains(factory, "ExternalTornOutTabsDropped");
            Assert.IsFalse(factory.Contains("new TabControl", StringComparison.Ordinal));
            StringAssert.Contains(product, "DragDrop.DoDragDrop");
            StringAssert.Contains(product, "RaiseTabTearOutWindowRequested");
        }

        private static void AssertAddCloseAndContextMove(FrameworkElement content)
        {
            using var host = new WindowHost(content, 920, 420);
            var tabView = (Mux.TabView)FindByAutomationId(content, "GallerySample_TabView_TabView");
            Assert.AreEqual(4, tabView.TabItems.Count);

            var addButton = (ButtonBase)tabView.Template.FindName("PART_AddButton", tabView);
            Assert.IsNotNull(addButton);
            addButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            WpfTestHost.DoEvents();
            Assert.AreEqual(5, tabView.TabItems.Count);

            var first = (Mux.TabViewItem)tabView.TabItems[0];
            var originalSecond = tabView.TabItems[1];
            var moveRight = (MenuItem)first.ContextMenu.Items[1];
            moveRight.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Assert.AreSame(originalSecond, tabView.TabItems[0]);
            Assert.AreSame(first, tabView.TabItems[1]);

            var close = (ButtonBase)first.Template.FindName("PART_CloseButton", first);
            Assert.IsNotNull(close);
            close.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(4, tabView.TabItems.Count);
        }

        private static void AssertOverflowAndSelection(FrameworkElement content)
        {
            using var host = new WindowHost(content, 760, 420);
            var tabView = (Mux.TabView)FindByAutomationId(content, "GallerySample_TabView_TabView3");
            var scrollViewer = (ScrollViewer)tabView.Template.FindName("PART_ScrollViewer", tabView);
            var increase = (RepeatButton)tabView.Template.FindName("PART_ScrollIncreaseButton", tabView);
            WpfTestHost.DoEvents();
            Assert.IsNotNull(scrollViewer);
            Assert.IsNotNull(increase);
            Assert.IsTrue(scrollViewer.ScrollableWidth > 0.0);
            Assert.AreEqual(Visibility.Visible, increase.Visibility);

            tabView.SelectedIndex = tabView.TabItems.Count - 1;
            WpfTestHost.DoEvents();
            Assert.IsTrue(scrollViewer.HorizontalOffset > 0.0);
            Assert.IsFalse(increase.IsEnabled);
        }

        private static void AssertWindowTearOutAndRejoin(FrameworkElement content)
        {
            using var host = new WindowHost(content, 920, 440);
            var source = (Mux.TabView)FindByAutomationId(content, "GallerySample_TabView_TabViewWindowingSource");
            var open = (Button)FindByAutomationId(content, "GallerySample_TabView_OpenTearOutWindowButton");
            Assert.AreEqual(2, source.TabItems.Count);
            open.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            WpfTestHost.DoEvents();

            var tearOutWindow = Application.Current.Windows
                .Cast<Window>()
                .Single(window => AutomationProperties.GetAutomationId(window) == "GallerySample_TabView_TearOutWindow");
            var destination = (Mux.TabView)FindByAutomationId(
                tearOutWindow,
                "GallerySample_TabView_TabViewWindowingDestination");
            Assert.AreEqual(1, source.TabItems.Count);
            Assert.AreEqual(1, destination.TabItems.Count);

            var rejoin = (Button)FindByAutomationId(tearOutWindow, "GallerySample_TabView_RejoinButton");
            rejoin.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            WpfTestHost.DoEvents();
            Assert.AreEqual(2, source.TabItems.Count);
            Assert.IsFalse(tearOutWindow.IsVisible);
        }

        private static DependencyObject FindByAutomationId(DependencyObject root, string automationId)
        {
            return Descendants(root)
                .FirstOrDefault(element => AutomationProperties.GetAutomationId(element) == automationId);
        }

        private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root == null)
            {
                yield break;
            }

            yield return root;
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var index = 0; index < count; index++)
            {
                foreach (var descendant in Descendants(VisualTreeHelper.GetChild(root, index)))
                {
                    yield return descendant;
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

        private sealed class WindowHost : IDisposable
        {
            public WindowHost(FrameworkElement content, double width, double height)
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

            public Window Window { get; }

            public void Dispose()
            {
                Window.Content = null;
                Window.Close();
                WpfTestHost.DoEvents();
            }
        }
    }
}
