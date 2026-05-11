using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using MuxMenuBar = ModernWpf.Controls.MenuBar;
using MuxMenuBarItem = ModernWpf.Controls.MenuBarItem;
using WpfMenuItem = System.Windows.Controls.MenuItem;

namespace ModernWpf.WinUI.Tests.MenuBar;

[TestClass]
public class MenuBarApiTests
{
    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            var menuBar = new MuxMenuBar();
            var menuBarItem = new MuxMenuBarItem();

            Assert.AreEqual(0, menuBar.Items.Count);
            Assert.AreEqual(new CornerRadius(), menuBar.CornerRadius);
            Assert.AreEqual(0, menuBarItem.Items.Count);
            Assert.AreEqual(string.Empty, menuBarItem.Title);
            Assert.IsNull(menuBarItem.Header);
            Assert.AreEqual(new CornerRadius(), menuBarItem.CornerRadius);

            menuBar.CornerRadius = new CornerRadius(4);
            menuBarItem.Title = "File";
            menuBarItem.CornerRadius = new CornerRadius(6);

            Assert.AreEqual(new CornerRadius(4), menuBar.CornerRadius);
            Assert.AreEqual("File", menuBarItem.Title);
            Assert.AreEqual("File", menuBarItem.Header);
            Assert.AreEqual(new CornerRadius(6), menuBarItem.CornerRadius);

            var flyout = new MenuBarItemFlyout();
            Assert.IsInstanceOfType(flyout, typeof(MenuFlyout));
            Assert.AreEqual(0, flyout.Items.Count);
        });
    }

    [TestMethod]
    public void AddRemoveMenuBarItemTest()
    {
        WpfTestHost.Run(() =>
        {
            var menuBar = new MuxMenuBar();
            var fileItem = new MuxMenuBarItem { Title = "File" };
            var formatItem = new MuxMenuBarItem { Title = "Format" };

            menuBar.Items.Add(fileItem);
            menuBar.Items.Add(formatItem);

            Assert.AreEqual(2, menuBar.Items.Count);
            Assert.AreSame(fileItem, menuBar.Items[0]);
            Assert.AreSame(formatItem, menuBar.Items[1]);

            var newItem = new MuxMenuBarItem { Title = "New Menu Bar Item" };
            menuBar.Items.Add(newItem);

            Assert.AreEqual(3, menuBar.Items.Count);
            Assert.AreSame(newItem, menuBar.Items[2]);

            menuBar.Items.Remove(newItem);
            Assert.AreEqual(2, menuBar.Items.Count);
            Assert.IsFalse(menuBar.Items.Contains(newItem));

            menuBar.Items.RemoveAt(1);
            Assert.AreEqual(1, menuBar.Items.Count);
            Assert.AreSame(fileItem, menuBar.Items[0]);
        });
    }

    [TestMethod]
    public void AddRemoveFlyoutItemTest()
    {
        WpfTestHost.Run(() =>
        {
            var fileItem = new MuxMenuBarItem { Title = "File" };
            var newItem = new WpfMenuItem { Header = "New" };
            var openItem = new WpfMenuItem { Header = "Open" };

            fileItem.Items.Add(newItem);
            fileItem.Items.Add(openItem);

            Assert.IsTrue(fileItem.HasItems);
            Assert.AreEqual(2, fileItem.Items.Count);
            Assert.AreSame(newItem, fileItem.Items[0]);

            var addedItem = new WpfMenuItem { Header = "New Flyout Item" };
            fileItem.Items.Add(addedItem);

            Assert.AreEqual(3, fileItem.Items.Count);
            Assert.AreSame(addedItem, fileItem.Items[2]);

            fileItem.Items.Remove(addedItem);
            Assert.AreEqual(2, fileItem.Items.Count);
            Assert.IsFalse(fileItem.Items.Contains(addedItem));

            fileItem.Items.Clear();
            Assert.IsFalse(fileItem.HasItems);
        });
    }

    [TestMethod]
    public void EmptyMenuBarItemNoPopupTest()
    {
        WpfTestHost.Run(() =>
        {
            var menuBarItem = new MuxMenuBarItem { Title = "One child" };

            Assert.IsFalse(menuBarItem.HasItems);

            menuBarItem.Items.Add(new WpfMenuItem { Header = "Popup" });
            Assert.IsTrue(menuBarItem.HasItems);

            menuBarItem.Items.Clear();
            Assert.IsFalse(menuBarItem.HasItems);
        });
    }

    [TestMethod]
    public void MenuBarSizeTest()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuBar = new MuxMenuBar
            {
                Height = 24
            };
            var menuBarItem = new MuxMenuBarItem { Title = "Size" };
            menuBar.Items.Add(menuBarItem);

            using var host = new TestWindowHost(menuBar, width: 320, height: 120);

            Assert.AreEqual(24, menuBar.ActualHeight, 0.5);
            Assert.IsNotNull(menuBar.Style);
            Assert.IsNotNull(menuBarItem.Style);
            Assert.AreEqual("Size", menuBarItem.Header);
            Assert.IsTrue(menuBarItem.ActualHeight <= menuBar.ActualHeight);
        });
    }

    [TestMethod]
    public void MenuBarXamlContentPropertyTest()
    {
        WpfTestHost.Run(() =>
        {
            var menuBar = (MuxMenuBar)XamlReader.Parse(
                @"<ui:MenuBar xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                        xmlns:ui='http://schemas.modernwpf.com/2019'>
                    <ui:MenuBarItem Title='File'>
                        <MenuItem Header='Open'/>
                    </ui:MenuBarItem>
                </ui:MenuBar>");

            Assert.AreEqual(1, menuBar.Items.Count);

            var fileItem = (MuxMenuBarItem)menuBar.Items[0];
            Assert.AreEqual("File", fileItem.Title);
            Assert.AreEqual("File", fileItem.Header);
            Assert.AreEqual(1, fileItem.Items.Count);
            Assert.AreEqual("Open", ((WpfMenuItem)fileItem.Items[0]).Header);
        });
    }
}
