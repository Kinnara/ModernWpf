using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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

            var button = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<Button>()
                .FirstOrDefault()
                ?? throw new AssertFailedException("Expected SelectorBarItem template to contain an item button.");
            var presenters = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<ContentPresenterEx>()
                .ToList();
            var iconPresenter = presenters.FirstOrDefault(candidate => ReferenceEquals(candidate.Content, icon))
                ?? throw new AssertFailedException("Expected SelectorBarItem template to use ContentPresenterEx for the icon slot.");
            var childPresenter = presenters.FirstOrDefault(candidate => ReferenceEquals(candidate.Content, child))
                ?? throw new AssertFailedException("Expected SelectorBarItem template to use ContentPresenterEx for the child slot.");

            Assert.AreEqual(BackgroundSizing.OuterBorderEdge, ControlHelper.GetBackgroundSizing(button));
            Assert.AreEqual(new CornerRadius(5), ControlHelper.GetCornerRadius(button));
            Assert.AreSame(foreground, iconPresenter.Foreground);
            Assert.AreSame(foreground, childPresenter.Foreground);
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

            RaiseItemButtonClick(second);

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

            var selectorPeer = FrameworkElementAutomationPeer.CreatePeerForElement(selectorBar);
            var selectionProvider = (ISelectionProvider)selectorPeer.GetPattern(PatternInterface.Selection);

            Assert.IsFalse(selectionProvider.CanSelectMultiple);
            Assert.IsFalse(selectionProvider.IsSelectionRequired);
            Assert.AreEqual(1, selectionProvider.GetSelection().Length);
        });
    }

    private static void RaiseItemButtonClick(SelectorBarItem item)
    {
        var button = VisualTreeTestHelper
            .EnumerateDescendants(item)
            .OfType<Button>()
            .FirstOrDefault();

        if (button == null)
        {
            Assert.Fail("Could not find selector item button.");
            throw new AssertFailedException();
        }

        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
}
