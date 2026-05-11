using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NavigationView;

[TestClass]
public class NavigationViewApiTests
{
    [TestMethod]
    public void VerifyDefaultsAndBasicSetting()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var footer = new Rectangle
            {
                Height = 40
            };

            var header = new TextBlock
            {
                Text = "Header"
            };

            var paneToggleButtonStyle = new Style();
            var navView = new ModernWpf.Controls.NavigationView();

            Assert.IsTrue(navView.IsPaneOpen);
            Assert.AreEqual(641, navView.CompactModeThresholdWidth);
            Assert.AreEqual(1008, navView.ExpandedModeThresholdWidth);
            Assert.IsNull(navView.PaneFooter);
            Assert.IsNull(navView.Header);
            Assert.IsTrue(navView.IsSettingsVisible);
            Assert.IsTrue(navView.IsPaneToggleButtonVisible);
            Assert.IsTrue(navView.IsTitleBarAutoPaddingEnabled);
            Assert.IsTrue(navView.AlwaysShowHeader);
            Assert.AreEqual(48, navView.CompactPaneLength);
            Assert.AreEqual(320, navView.OpenPaneLength);
            Assert.IsNull(navView.PaneToggleButtonStyle);
            Assert.AreEqual(0, navView.MenuItems.Count);
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Minimal, navView.DisplayMode);
            Assert.AreEqual(string.Empty, navView.PaneTitle);
            Assert.IsFalse(navView.IsBackEnabled);
            Assert.AreEqual(ModernWpf.Controls.NavigationViewBackButtonVisible.Auto, navView.IsBackButtonVisible);

            navView.IsPaneOpen = true;
            navView.CompactModeThresholdWidth = 500;
            navView.ExpandedModeThresholdWidth = 1000;
            navView.PaneFooter = footer;
            navView.Header = header;
            navView.IsSettingsVisible = false;
            navView.IsPaneToggleButtonVisible = false;
            navView.IsTitleBarAutoPaddingEnabled = false;
            navView.AlwaysShowHeader = false;
            navView.CompactPaneLength = 40;
            navView.OpenPaneLength = 300;
            navView.PaneToggleButtonStyle = paneToggleButtonStyle;
            navView.PaneTitle = "ChangedTitle";
            navView.IsBackEnabled = true;
            navView.IsBackButtonVisible = ModernWpf.Controls.NavigationViewBackButtonVisible.Visible;

            Assert.IsTrue(navView.IsPaneOpen);
            Assert.AreEqual(500, navView.CompactModeThresholdWidth);
            Assert.AreEqual(1000, navView.ExpandedModeThresholdWidth);
            Assert.AreSame(footer, navView.PaneFooter);
            Assert.AreSame(header, navView.Header);
            Assert.IsFalse(navView.IsSettingsVisible);
            Assert.IsFalse(navView.IsPaneToggleButtonVisible);
            Assert.IsFalse(navView.IsTitleBarAutoPaddingEnabled);
            Assert.IsFalse(navView.AlwaysShowHeader);
            Assert.AreEqual(40, navView.CompactPaneLength);
            Assert.AreEqual(300, navView.OpenPaneLength);
            Assert.AreSame(paneToggleButtonStyle, navView.PaneToggleButtonStyle);
            Assert.AreEqual("ChangedTitle", navView.PaneTitle);
            Assert.IsTrue(navView.IsBackEnabled);
            Assert.AreEqual(ModernWpf.Controls.NavigationViewBackButtonVisible.Visible, navView.IsBackButtonVisible);

            navView.PaneFooter = null;
            navView.Header = null;
            navView.PaneToggleButtonStyle = null;

            Assert.IsNull(navView.PaneFooter);
            Assert.IsNull(navView.Header);
            Assert.IsNull(navView.PaneToggleButtonStyle);
        });
    }

    [TestMethod]
    public void VerifyValuesCoercion()
    {
        WpfTestHost.Run(() =>
        {
            var navView = new ModernWpf.Controls.NavigationView
            {
                CompactModeThresholdWidth = -1,
                ExpandedModeThresholdWidth = -1,
                CompactPaneLength = -1,
                OpenPaneLength = -1
            };

            Assert.AreEqual(0, navView.CompactModeThresholdWidth);
            Assert.AreEqual(0, navView.ExpandedModeThresholdWidth);
            Assert.AreEqual(0, navView.CompactPaneLength);
            Assert.AreEqual(0, navView.OpenPaneLength);
        });
    }

    [TestMethod]
    public void VerifyPaneProperties()
    {
        WpfTestHost.Run(() =>
        {
            var navView = new ModernWpf.Controls.NavigationView
            {
                IsPaneOpen = false,
                CompactPaneLength = 100.0,
                OpenPaneLength = 200.0
            };

            Assert.IsFalse(navView.IsPaneOpen);
            Assert.AreEqual(100.0, navView.CompactPaneLength);
            Assert.AreEqual(200.0, navView.OpenPaneLength);

            navView.IsPaneOpen = true;
            Assert.IsTrue(navView.IsPaneOpen);
        });
    }

    [TestMethod]
    public void VerifySelectedItemIsNullWhenNoItemIsSelected()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                Width = 1008
            };
            var menuItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1"
            };
            navView.MenuItems.Add(menuItem);

            using var host = new TestWindowHost(navView);

            Assert.IsFalse(menuItem.IsSelected);
            Assert.IsNull(navView.SelectedItem);

            menuItem.IsSelected = true;
            host.UpdateLayout();

            Assert.IsTrue(menuItem.IsSelected);
            Assert.AreSame(menuItem, navView.SelectedItem);

            menuItem.IsSelected = false;
            host.UpdateLayout();

            Assert.IsFalse(menuItem.IsSelected);
            Assert.IsNull(navView.SelectedItem);
        });
    }

    [TestMethod]
    public void VerifyNavigationItemUIAType()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                Width = 1008
            };
            var menuItem1 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1"
            };
            var menuItem2 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 2"
            };

            navView.MenuItems.Add(menuItem1);
            navView.MenuItems.Add(menuItem2);
            using var host = new TestWindowHost(navView);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(menuItem1);
            Assert.IsNotNull(peer);
            Assert.AreEqual(AutomationControlType.ListItem, peer!.GetAutomationControlType());
            Assert.IsNull(peer.GetPattern(PatternInterface.Invoke));

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top;
            host.UpdateLayout();

            peer = FrameworkElementAutomationPeer.CreatePeerForElement(menuItem1);
            Assert.IsNotNull(peer);
            Assert.AreEqual(AutomationControlType.TabItem, peer!.GetAutomationControlType());
            Assert.IsNull(peer.GetPattern(PatternInterface.Invoke));
        });
    }

    [TestMethod]
    public void VerifyAutomationPeerExpandCollapsePatternBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var menuItem1 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1"
            };
            var menuItem2 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 2"
            };
            var menuItem3 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 3"
            };
            var menuItem4 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 4",
                HasUnrealizedChildren = true
            };

            menuItem2.MenuItems.Add(menuItem3);

            Assert.IsNull(FrameworkElementAutomationPeer
                .CreatePeerForElement(menuItem1)!
                .GetPattern(PatternInterface.ExpandCollapse));
            Assert.IsNotNull(FrameworkElementAutomationPeer
                .CreatePeerForElement(menuItem2)!
                .GetPattern(PatternInterface.ExpandCollapse));
            Assert.IsNotNull(FrameworkElementAutomationPeer
                .CreatePeerForElement(menuItem4)!
                .GetPattern(PatternInterface.ExpandCollapse));
        });
    }

    [TestMethod]
    public void VerifySettingsItemTagAndToolTip()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                IsSettingsVisible = true,
                IsPaneOpen = true,
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left
            };

            using var host = new TestWindowHost(navView);
            var settingsItem = navView.SettingsItem as ModernWpf.Controls.NavigationViewItem;
            Assert.IsNotNull(settingsItem);
            Assert.AreEqual("Settings", settingsItem!.Tag);
            Assert.IsNull(ToolTipService.GetToolTip(settingsItem));

            navView.IsPaneOpen = false;
            host.UpdateLayout();

            Assert.IsNotNull(ToolTipService.GetToolTip(settingsItem));
        });
    }

    [TestMethod]
    public void VerifyNavigationViewItemInFooterDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var footerItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Footer item"
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneFooter = footerItem,
                Width = 1008
            };

            using var host = new TestWindowHost(navView);

            Assert.AreSame(footerItem, navView.PaneFooter);
        });
    }
}
