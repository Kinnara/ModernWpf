using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using RadioMenuItem = ModernWpf.Controls.RadioMenuItem;

namespace ModernWpf.WinUI.Tests.RadioMenuFlyoutItem;

[TestClass]
public class RadioMenuFlyoutItemInteractionTests
{
    [TestMethod]
    public void BasicTest()
    {
        WpfTestHost.Run(() =>
        {
            var items = CreateBasicItems();
            using var host = new TestWindowHost(CreateMenu(items.Values));

            VerifySelectedItems(items, "Orange", "Compact");

            Check(items["Yellow"]);
            VerifySelectedItems(items, "Yellow", "Compact");

            Check(items["Expanded"]);
            VerifySelectedItems(items, "Yellow", "Expanded");

            Uncheck(items["Yellow"]);
            VerifySelectedItems(items, "Yellow", "Expanded");
        });
    }

    [TestMethod]
    public void SubMenuTest()
    {
        WpfTestHost.Run(() =>
        {
            var items = CreateSubMenuItems(out var radioSubMenu);
            var menu = CreateMenu(new[] { items["Name"], items["Date"], items["Size"] });
            menu.Items.Add(radioSubMenu);

            using var host = new TestWindowHost(menu);

            Check(items["ArtistName"]);
            VerifySelectedItems(items, "ArtistName");

            Check(items["Date"]);
            VerifySelectedItems(items, "Date");
        });
    }

    [TestMethod]
    public void UnloadedCheckedItemLeavesSourceSelectionMap()
    {
        WpfTestHost.Run(() =>
        {
            var first = new RadioMenuItem
            {
                Header = "First",
                GroupName = "SourceGroup",
                IsChecked = true
            };
            var second = new RadioMenuItem
            {
                Header = "Second",
                GroupName = "SourceGroup"
            };
            var panel = new StackPanel();
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel);
            host.UpdateLayout();

            Assert.IsTrue(first.IsChecked);

            panel.Children.Remove(first);
            host.UpdateLayout();

            Check(second);
            host.UpdateLayout();

            Assert.IsTrue(first.IsChecked, "WinUI removes an unloaded checked item from the active group map without unchecking it.");
            Assert.IsTrue(second.IsChecked);
        });
    }

    [TestMethod]
    public void LoadedCheckedItemReclaimsSourceSelectionMap()
    {
        WpfTestHost.Run(() =>
        {
            var first = new RadioMenuItem
            {
                Header = "First",
                GroupName = "SourceGroup",
                IsChecked = true
            };
            var second = new RadioMenuItem
            {
                Header = "Second",
                GroupName = "SourceGroup"
            };
            var panel = new StackPanel();
            panel.Children.Add(first);
            panel.Children.Add(second);

            using var host = new TestWindowHost(panel);
            host.UpdateLayout();

            panel.Children.Remove(first);
            host.UpdateLayout();

            Check(second);
            host.UpdateLayout();

            panel.Children.Insert(0, first);
            host.UpdateLayout();

            Assert.IsTrue(first.IsChecked);
            Assert.IsFalse(second.IsChecked, "WinUI updates the active group map when a checked radio menu item loads.");
        });
    }

    [TestMethod]
    public void SubMenuCheckStateVisualTracksCheckedChildren()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var items = CreateSubMenuItems(out var radioSubMenu);
            RadioMenuItem.SetAreCheckStatesEnabled(radioSubMenu, true);
            radioSubMenu.Template = (ControlTemplate)Application.Current.FindResource(MenuItem.SubmenuHeaderTemplateKey);

            var menu = CreateNestedMenu(new object[] { items["Name"], items["Date"], items["Size"], radioSubMenu }, out var rootMenuItem);

            using var host = new TestWindowHost(menu);
            rootMenuItem.SetCurrentValue(MenuItem.IsSubmenuOpenProperty, true);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(RadioMenuItem.GetAreCheckStatesEnabled(radioSubMenu));
            Assert.IsFalse(radioSubMenu.IsChecked);
            AssertSubMenuCheckGlyph(radioSubMenu, Visibility.Visible, 0.0);

            Check(items["ArtistName"]);
            host.UpdateLayout();

            Assert.IsTrue(radioSubMenu.IsChecked);
            AssertSubMenuCheckGlyph(radioSubMenu, Visibility.Visible, 1.0);

            Check(items["Date"]);
            host.UpdateLayout();

            Assert.IsFalse(radioSubMenu.IsChecked);
            AssertSubMenuCheckGlyph(radioSubMenu, Visibility.Visible, 0.0);
        });
    }

    private static Dictionary<string, RadioMenuItem> CreateBasicItems()
    {
        var items = new Dictionary<string, RadioMenuItem>();

        AddItem(items, "Red", string.Empty);
        AddItem(items, "Orange", string.Empty, isChecked: true);
        AddItem(items, "Yellow", string.Empty);
        AddItem(items, "Green", string.Empty);
        AddItem(items, "Blue", string.Empty);
        AddItem(items, "Indigo", string.Empty);
        AddItem(items, "Violet", string.Empty);
        AddItem(items, "Compact", "Size", isChecked: true);
        AddItem(items, "Normal", "Size");
        AddItem(items, "Expanded", "Size");

        return items;
    }

    private static Dictionary<string, RadioMenuItem> CreateSubMenuItems(out MenuItem radioSubMenu)
    {
        var items = new Dictionary<string, RadioMenuItem>();

        AddItem(items, "Name", "SortGroup", isChecked: true);
        AddItem(items, "Date", "SortGroup");
        AddItem(items, "Size", "SortGroup");

        radioSubMenu = new MenuItem { Header = "Other" };
        radioSubMenu.Items.Add(AddItem(items, "AlbumName", "SortGroup"));
        radioSubMenu.Items.Add(AddItem(items, "ArtistName", "SortGroup"));
        radioSubMenu.Items.Add(AddItem(items, "Genre", "SortGroup"));

        return items;
    }

    private static RadioMenuItem AddItem(Dictionary<string, RadioMenuItem> items, string name, string groupName, bool isChecked = false)
    {
        var item = new RadioMenuItem
        {
            Header = name,
            GroupName = groupName,
            IsChecked = isChecked
        };

        items.Add(name, item);
        return item;
    }

    private static Menu CreateMenu(IEnumerable<RadioMenuItem> items)
    {
        var menu = new Menu();

        foreach (var item in items)
        {
            menu.Items.Add(item);
        }

        return menu;
    }

    private static Menu CreateNestedMenu(IEnumerable<object> items, out MenuItem rootMenuItem)
    {
        var menu = new Menu();
        rootMenuItem = new MenuItem { Header = "Root" };

        foreach (var item in items)
        {
            rootMenuItem.Items.Add(item);
        }

        menu.Items.Add(rootMenuItem);
        return menu;
    }

    private static void Check(RadioMenuItem item)
    {
        item.SetCurrentValue(MenuItem.IsCheckedProperty, true);
    }

    private static void Uncheck(RadioMenuItem item)
    {
        item.SetCurrentValue(MenuItem.IsCheckedProperty, false);
    }

    private static void VerifySelectedItems(IReadOnlyDictionary<string, RadioMenuItem> items, params string[] selectedNames)
    {
        var selected = selectedNames.ToHashSet();

        foreach (var item in items)
        {
            Assert.AreEqual(selected.Contains(item.Key), item.Value.IsChecked, item.Key);
        }
    }

    private static void AssertSubMenuCheckGlyph(MenuItem subMenu, Visibility expectedVisibility, double expectedOpacity)
    {
        subMenu.ApplyTemplate();

        Assert.AreSame(Application.Current.FindResource(MenuItem.SubmenuHeaderTemplateKey), subMenu.Template);

        var checkGlyph = subMenu.Template.FindName("CheckGlyph", subMenu) as UIElement;
        Assert.IsNotNull(checkGlyph, "Submenu headers should expose a check glyph visual.");
        Assert.AreEqual(expectedVisibility, checkGlyph!.Visibility);
        Assert.AreEqual(expectedOpacity, checkGlyph.Opacity);
    }
}
