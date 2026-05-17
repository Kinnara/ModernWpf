using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SelectorBar;

[TestClass]
public class SelectorBarApiTests
{
    [TestMethod]
    public void VerifyDefaultSelectorBarItemPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBarItem = new SelectorBarItem();

            Assert.AreEqual(string.Empty, selectorBarItem.Text);
            Assert.IsNull(selectorBarItem.Icon);
            Assert.IsNull(selectorBarItem.Child);
            Assert.IsFalse(selectorBarItem.IsSelected);
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, selectorBarItem.BackgroundSizing);
            Assert.AreEqual(new CornerRadius(), selectorBarItem.CornerRadius);
        });
    }

    [TestMethod]
    public void VerifyDefaultSelectorBarPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();

            Assert.IsNotNull(selectorBar.Items);
            Assert.AreEqual(0, selectorBar.Items.Count);
            Assert.IsNull(selectorBar.SelectedItem);
            Assert.IsFalse(selectorBar.Focusable);
            Assert.AreEqual(KeyboardNavigationMode.Once, KeyboardNavigation.GetTabNavigation(selectorBar));
        });
    }

    [TestMethod]
    public void SelectorBarItemTemplateUsesWinUIPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var icon = new SymbolIcon(Symbol.Delete);
            var child = new Border { Width = 20, Height = 12 };
            var foreground = new SolidColorBrush(Colors.Blue);
            var item = new SelectorBarItem
            {
                Text = "Deleted",
                Icon = icon,
                Child = child,
                BackgroundSizing = BackgroundSizing.OuterBorderEdge,
                CornerRadius = new CornerRadius(5),
                Foreground = foreground
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);

            var root = GetNamedDescendant<GridEx>(item, "PART_ContainerRoot");
            var iconPresenter = GetNamedDescendant<ContentPresenterEx>(item, "PART_IconVisual");
            var textVisual = GetNamedDescendant<TextBlock>(item, "PART_TextVisual");
            var selectionVisual = GetNamedDescendant<Rectangle>(item, "PART_SelectionVisual");
            var commonVisual = GetNamedDescendant<Rectangle>(item, "PART_CommonVisual");
            var contentStack = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<StackPanelEx>()
                .FirstOrDefault()
                ?? throw new AssertFailedException("Expected SelectorBarItem template to use StackPanelEx for source spacing.");

            Assert.IsFalse(
                VisualTreeTestHelper.EnumerateDescendants(item).OfType<Button>().Any(),
                "SelectorBarItem should not keep the old WPF button wrapper.");
            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, root.BackgroundSizing);
            Assert.AreEqual(new CornerRadius(5), root.CornerRadius);
            Assert.AreSame(icon, iconPresenter.Content);
            Assert.AreEqual("Deleted", textVisual.Text);
            Assert.AreSame(foreground, iconPresenter.Foreground);
            Assert.AreSame(foreground, textVisual.Foreground);
            Assert.AreEqual(8.0, contentStack.Spacing);
            Assert.AreEqual(0.0, selectionVisual.Opacity);
            Assert.AreEqual(1.0, commonVisual.StrokeThickness);
        });
    }

    [TestMethod]
    public void VerifySelectorBarItems()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var deleted = new SelectorBarItem
            {
                Text = "Deleted",
                Icon = new SymbolIcon(Symbol.Delete),
                IsEnabled = false
            };
            var remote = new SelectorBarItem
            {
                Text = "Remote",
                Icon = new SymbolIcon(Symbol.Remote),
                IsSelected = true
            };
            var shared = new SelectorBarItem
            {
                Text = "Shared",
                Icon = new SymbolIcon(Symbol.Share)
            };
            var favorites = new SelectorBarItem
            {
                Text = "Favorites",
                Icon = new SymbolIcon(Symbol.Favorite)
            };

            selectorBar.Items.Add(deleted);
            selectorBar.Items.Add(remote);
            selectorBar.Items.Add(shared);
            selectorBar.Items.Add(favorites);

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            Assert.AreEqual(4, selectorBar.Items.Count);
            Assert.AreSame(remote, selectorBar.SelectedItem);
            Assert.IsTrue(remote.IsSelected);

            selectorBar.Items.RemoveAt(1);

            Assert.AreEqual(3, selectorBar.Items.Count);
            Assert.IsNull(selectorBar.SelectedItem);

            selectorBar.Items.Clear();

            Assert.AreEqual(0, selectorBar.Items.Count);
        });
    }

    [TestMethod]
    public void ClickingItemUpdatesSelectedItem()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var first = new SelectorBarItem { Text = "First" };
            var second = new SelectorBarItem { Text = "Second" };
            selectorBar.Items.Add(first);
            selectorBar.Items.Add(second);

            var selectionChangedCount = 0;
            selectorBar.SelectionChanged += (sender, args) => selectionChangedCount++;

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            RaiseItemClick(second);

            Assert.AreSame(second, selectorBar.SelectedItem);
            Assert.IsFalse(first.IsSelected);
            Assert.IsTrue(second.IsSelected);
            Assert.AreEqual(1, selectionChangedCount);
        });
    }

    [TestMethod]
    public void SelectedItemMustBelongToItems()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();

            Assert.ThrowsException<System.ArgumentException>(() => selectorBar.SelectedItem = new SelectorBarItem());
        });
    }

    [TestMethod]
    public void VerifySelectionAutomation()
    {
        WpfTestHost.Run(() =>
        {
            var selectorBar = new ModernWpf.Controls.SelectorBar();
            var first = new SelectorBarItem { Text = "First" };
            var second = new SelectorBarItem { Text = "Second" };
            selectorBar.Items.Add(first);
            selectorBar.Items.Add(second);

            using var host = new TestWindowHost(selectorBar, width: 400, height: 120);

            var itemPeer = FrameworkElementAutomationPeer.CreatePeerForElement(second);
            var selectionItemProvider = (ISelectionItemProvider)itemPeer.GetPattern(PatternInterface.SelectionItem);
            selectionItemProvider.Select();

            Assert.AreSame(second, selectorBar.SelectedItem);
            Assert.IsTrue(selectionItemProvider.IsSelected);
            Assert.AreEqual("Second", itemPeer.GetName());
            Assert.AreEqual("SelectorBarItem", itemPeer.GetLocalizedControlType());

            var selectorPeer = FrameworkElementAutomationPeer.CreatePeerForElement(selectorBar);
            var selectionProvider = (ISelectionProvider)selectorPeer.GetPattern(PatternInterface.Selection);

            Assert.IsFalse(selectionProvider.CanSelectMultiple);
            Assert.IsFalse(selectionProvider.IsSelectionRequired);
            Assert.AreEqual(1, selectionProvider.GetSelection().Length);
        });
    }

    private static void RaiseItemClick(SelectorBarItem item)
    {
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonDownEvent
        });
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.MouseLeftButtonUpEvent
        });
    }

    private static T GetNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<T>()
            .FirstOrDefault(candidate => candidate.Name == name)
            ?? throw new AssertFailedException($"Expected to find template part {name}.");
    }
}
