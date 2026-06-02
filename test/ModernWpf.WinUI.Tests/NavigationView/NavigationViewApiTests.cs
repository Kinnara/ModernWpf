using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
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
    public void TemplateSettingsOpenPaneLengthUsesWinUISourceClamp()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left,
                Width = 100.0,
                OpenPaneLength = 320.0,
                Content = "Content"
            };

            navView.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "Home" });

            using var host = new TestWindowHost(navView, width: 640, height: 360);
            host.UpdateLayout();

            var splitView = FindNamedDescendant<ModernWpf.Controls.SplitView>(navView, "RootSplitView");
            var shadowCaster = FindNamedDescendant<ThemeShadowChrome>(navView, "ShadowCaster");

            Assert.AreEqual(100.0, navView.TemplateSettings.OpenPaneLength);
            Assert.AreEqual(100.0, splitView.OpenPaneLength);
            Assert.AreEqual(100.0, shadowCaster.Width);
            AssertNavigationViewPaneOverlayShadow(shadowCaster);

            navView.Width = 500.0;
            host.UpdateLayout();

            Assert.AreEqual(320.0, navView.TemplateSettings.OpenPaneLength);
            Assert.AreEqual(320.0, splitView.OpenPaneLength);
            Assert.AreEqual(320.0, shadowCaster.Width);
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
            Assert.IsInstanceOfType(peer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top;
            host.UpdateLayout();

            peer = FrameworkElementAutomationPeer.CreatePeerForElement(menuItem1);
            Assert.IsNotNull(peer);
            Assert.AreEqual(AutomationControlType.TabItem, peer!.GetAutomationControlType());
            Assert.IsInstanceOfType(peer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
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
    public void NavigationViewAutomationPeerUsesSourceSelectionProviderShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            Assert.IsTrue(
                typeof(NavigationViewAutomationPeer).IsPublic,
                "NavigationViewAutomationPeer should be public like the WinUI automation peer surface.");

            var navView = new ModernWpf.Controls.NavigationView
            {
                Width = 1008,
            };
            var menuItem1 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1",
            };
            var menuItem2 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 2",
            };

            navView.MenuItems.Add(menuItem1);
            navView.MenuItems.Add(menuItem2);

            using var host = new TestWindowHost(navView);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(navView);
            Assert.IsInstanceOfType(peer, typeof(NavigationViewAutomationPeer));

            var selectionProvider = peer!.GetPattern(PatternInterface.Selection) as System.Windows.Automation.Provider.ISelectionProvider;
            Assert.IsNotNull(selectionProvider);
            Assert.IsFalse(selectionProvider!.CanSelectMultiple);
            Assert.IsFalse(selectionProvider.IsSelectionRequired);
            Assert.AreEqual(0, selectionProvider.GetSelection().Length);

            menuItem2.IsSelected = true;
            host.UpdateLayout();

            Assert.AreEqual(1, selectionProvider.GetSelection().Length);
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

    [TestMethod]
    public void VerifyMenuItemAndContainerMappingMenuItemsSource()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                MenuItemsSource = new ObservableCollection<string> { "Item 1", "Item 2" },
                Width = 1008
            };

            using var host = new TestWindowHost(navView);

            var menuItem = "Item 2";
            var itemContainer = navView.ContainerFromMenuItem(menuItem) as ModernWpf.Controls.NavigationViewItem;
            Assert.IsNotNull(itemContainer);
            Assert.AreEqual(menuItem, itemContainer!.Content as string);

            var returnedItem = navView.MenuItemFromContainer(itemContainer) as string;
            Assert.AreEqual(menuItem, returnedItem);
        });
    }

    [TestMethod]
    public void VerifyMenuItemAndContainerMappingMenuItems()
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

            var itemContainer = navView.ContainerFromMenuItem(menuItem2) as ModernWpf.Controls.NavigationViewItem;
            Assert.AreSame(menuItem2, itemContainer);

            var returnedItem = navView.MenuItemFromContainer(menuItem2) as ModernWpf.Controls.NavigationViewItem;
            Assert.AreSame(menuItem2, returnedItem);

            var menuItem3 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 3"
            };
            Assert.IsNull(navView.MenuItemFromContainer(menuItem3));
        });
    }

    [TestMethod]
    public void VerifyClearingItemsCollectionDoesNotCrashWhenItemSelectedOnTopNav()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navViewItem1 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "MenuItem 1"
            };
            var navViewItem2 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "MenuItem 2"
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top
            };
            navView.MenuItems.Add(navViewItem1);
            navView.MenuItems.Add(navViewItem2);

            using var host = new TestWindowHost(navView);

            navView.SelectedItem = navViewItem1;
            host.UpdateLayout();
            Assert.AreSame(navViewItem1, navView.SelectedItem);

            navView.MenuItems.Clear();
            host.UpdateLayout();

            var itemsSource = new ObservableCollection<ModernWpf.Controls.NavigationViewItem>
            {
                navViewItem1,
                navViewItem2
            };
            navView.MenuItemsSource = itemsSource;
            host.UpdateLayout();

            navView.SelectedItem = navViewItem1;
            host.UpdateLayout();
            Assert.AreSame(navViewItem1, navView.SelectedItem);

            itemsSource.Clear();
            host.UpdateLayout();
        });
    }

    [TestMethod]
    public void VerifyHierarchicalNavigationTopModeMenuItemsSourceDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var childItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1.1"
            };
            var parentItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1",
                MenuItemsSource = new ObservableCollection<ModernWpf.Controls.NavigationViewItem> { childItem }
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top,
                MenuItemsSource = new ObservableCollection<ModernWpf.Controls.NavigationViewItem> { parentItem }
            };

            using var host = new TestWindowHost(navView);

            Assert.AreSame(parentItem, navView.ContainerFromMenuItem(parentItem));
        });
    }

    [TestMethod]
    public void VerifyNavigationViewItemToolTipCreation()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuItem1 = new ModernWpf.Controls.NavigationViewItem();
            var menuItem2 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = string.Empty
            };
            var menuItem3 = new ModernWpf.Controls.NavigationViewItem();
            ToolTipService.SetToolTip(menuItem3, "Custom tooltip");
            var menuItem4 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 4"
            };

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left,
                IsPaneOpen = false
            };
            navView.MenuItems.Add(menuItem1);
            navView.MenuItems.Add(menuItem2);
            navView.MenuItems.Add(menuItem3);
            navView.MenuItems.Add(menuItem4);

            using var host = new TestWindowHost(navView);

            Assert.IsNull(GetToolTipContent(menuItem1));
            Assert.IsNull(GetToolTipContent(menuItem2));
            Assert.AreEqual("Custom tooltip", GetToolTipContent(menuItem3));
            Assert.AreEqual("Item 4", GetToolTipContent(menuItem4));
        });
    }

    [TestMethod]
    public void VerifyNavigationViewItemToolTipPaneDisplayMode()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuItem1 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 1"
            };
            var menuItem2 = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item 2"
            };
            ToolTipService.SetToolTip(menuItem2, "Custom tooltip");

            var navView = new ModernWpf.Controls.NavigationView();
            navView.MenuItems.Add(menuItem1);
            navView.MenuItems.Add(menuItem2);

            using var host = new TestWindowHost(navView);

            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.Left, false, "Item 1", "Custom tooltip");
            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.Left, true, null, "Custom tooltip");
            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftCompact, false, "Item 1", "Custom tooltip");
            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftCompact, true, null, "Custom tooltip");
            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftMinimal, true, null, "Custom tooltip");
            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.Top, false, null, "Custom tooltip");
            SetPaneConfigAndVerifyToolTips(ModernWpf.Controls.NavigationViewPaneDisplayMode.Top, true, null, "Custom tooltip");

            void SetPaneConfigAndVerifyToolTips(
                ModernWpf.Controls.NavigationViewPaneDisplayMode paneDisplayMode,
                bool isPaneOpen,
                string? expectedDefaultToolTip,
                string? expectedCustomToolTip)
            {
                navView.PaneDisplayMode = paneDisplayMode;
                navView.IsPaneOpen = isPaneOpen;
                host.UpdateLayout();

                Assert.AreEqual(expectedDefaultToolTip, GetToolTipContent(menuItem1));
                Assert.AreEqual(expectedCustomToolTip, GetToolTipContent(menuItem2));
            }
        });
    }

    [TestMethod]
    public void VerifyNavigationViewItemOutlivingNavigationViewDoesNotCrash()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView();
            var menuItem = new ModernWpf.Controls.NavigationViewItem();
            navView.MenuItems.Add(menuItem);

            using var host = new TestWindowHost(navView);

            navView.MenuItems.Clear();
            host.Window.Content = menuItem;
            host.UpdateLayout();

            GC.Collect();
            menuItem.IsSelected = !menuItem.IsSelected;
        });
    }

    [TestMethod]
    public void VerifyPaneDisplayModeAndDisplayModeMapping()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = CreateNavigationViewHost(out var navView);

            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Expanded, navView.DisplayMode);
            Assert.IsTrue(navView.IsPaneOpen);

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Minimal, navView.DisplayMode);

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Left;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Expanded, navView.DisplayMode);

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftCompact;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Compact, navView.DisplayMode);

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Minimal, navView.DisplayMode);

            navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Expanded, navView.DisplayMode);

            navView.Width = navView.ExpandedModeThresholdWidth - 10.0;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Compact, navView.DisplayMode);

            navView.Width = navView.CompactModeThresholdWidth - 10.0;
            host.UpdateLayout();
            Assert.AreEqual(ModernWpf.Controls.NavigationViewDisplayMode.Minimal, navView.DisplayMode);
        });
    }

    [TestMethod]
    public void VerifyPaneDisplayModeChangingPaneAccordingly()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = CreateNavigationViewHost(out var navView);

            foreach (var value in Enum.GetValues(typeof(ModernWpf.Controls.NavigationViewPaneDisplayMode)))
            {
                var paneDisplayMode = (ModernWpf.Controls.NavigationViewPaneDisplayMode)value;

                navView.PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
                navView.IsPaneOpen = false;
                navView.Width = navView.CompactModeThresholdWidth - 20;
                host.UpdateLayout();

                navView.PaneDisplayMode = paneDisplayMode;
                host.UpdateLayout();

                Assert.AreEqual(
                    paneDisplayMode == ModernWpf.Controls.NavigationViewPaneDisplayMode.Left,
                    navView.IsPaneOpen);
            }
        });
    }

    [TestMethod]
    public void VerifyPaneDisplayModeAndIsPaneOpenInterplayOnNavViewLaunch()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            VerifyLaunchPaneState(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftMinimal, true, false);
            VerifyLaunchPaneState(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftMinimal, false, false);
            VerifyLaunchPaneState(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftCompact, true, false);
            VerifyLaunchPaneState(ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftCompact, false, false);
            VerifyLaunchPaneState(ModernWpf.Controls.NavigationViewPaneDisplayMode.Left, true, true);
            VerifyLaunchPaneState(ModernWpf.Controls.NavigationViewPaneDisplayMode.Left, false, false);
            VerifyLaunchPaneState(
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
                true,
                false,
                ModernWpf.Controls.NavigationViewDisplayMode.Minimal);
            VerifyLaunchPaneState(
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
                true,
                false,
                ModernWpf.Controls.NavigationViewDisplayMode.Compact);
            VerifyLaunchPaneState(
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
                true,
                true,
                ModernWpf.Controls.NavigationViewDisplayMode.Expanded);
            VerifyLaunchPaneState(
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
                false,
                false,
                ModernWpf.Controls.NavigationViewDisplayMode.Minimal);
            VerifyLaunchPaneState(
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
                false,
                false,
                ModernWpf.Controls.NavigationViewDisplayMode.Compact);
            VerifyLaunchPaneState(
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
                false,
                false,
                ModernWpf.Controls.NavigationViewDisplayMode.Expanded);
        });

        static void VerifyLaunchPaneState(
            ModernWpf.Controls.NavigationViewPaneDisplayMode paneDisplayMode,
            bool isPaneOpen,
            bool expectedIsPaneOpen,
            ModernWpf.Controls.NavigationViewDisplayMode displayMode = ModernWpf.Controls.NavigationViewDisplayMode.Expanded)
        {
            using var host = CreateNavigationViewHost(out var navView, paneDisplayMode, isPaneOpen, displayMode);
            Assert.AreEqual(expectedIsPaneOpen, navView.IsPaneOpen);
        }
    }

    [TestMethod]
    public void VerifyClosedCompactVisualState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Item"
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.LeftCompact,
                IsPaneOpen = false,
                IsSettingsVisible = false
            };
            navView.MenuItems.Add(menuItem);

            using var host = new TestWindowHost(navView);

            var presenter = VisualTreeTestHelper.FindDescendant<ModernWpf.Controls.Primitives.NavigationViewItemPresenter>(menuItem);
            Assert.IsNotNull(presenter);
            var presenterLayoutRoot = VisualTreeHelper.GetChild(presenter!, 0) as FrameworkElement;
            Assert.IsNotNull(presenterLayoutRoot);

            var stateName = VisualStateManager.GetVisualStateGroups(presenterLayoutRoot!)
                .OfType<VisualStateGroup>()
                .Single(group => group.Name == "PaneAndTopLevelItemStates")
                .CurrentState
                .Name;

            Assert.AreEqual("ClosedCompactAndTopLevelItem", stateName);
        });
    }

    [TestMethod]
    public void VerifyExpandCollapseChevronVisibility()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var children = new ObservableCollection<string>();
            var parentItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "ParentItem",
                MenuItemsSource = children
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                Width = 1008
            };
            navView.MenuItems.Add(parentItem);

            using var host = new TestWindowHost(navView);

            var chevron = FindNamedDescendant<FrameworkElement>(parentItem, "ExpandCollapseChevron");
            Assert.AreEqual(Visibility.Collapsed, chevron.Visibility);

            children.Add("Child 1");
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);

            children.Clear();
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, chevron.Visibility);

            children.Add("Child 2");
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);

            parentItem.MenuItemsSource = null;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, chevron.Visibility);

            parentItem.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "Child 3" });
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);

            parentItem.MenuItems.Clear();
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, chevron.Visibility);
        });
    }

    [TestMethod]
    public void ExpandCollapseChevronMouseDownDoesNotLetPresenterStealCapture()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var parentItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "ParentItem",
                IsExpanded = true
            };
            parentItem.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "ChildItem" });

            var navView = new ModernWpf.Controls.NavigationView
            {
                Width = 1008,
                IsSettingsVisible = false
            };
            navView.MenuItems.Add(parentItem);

            using var host = new TestWindowHost(navView);

            var presenter = VisualTreeTestHelper.FindDescendant<ModernWpf.Controls.Primitives.NavigationViewItemPresenter>(parentItem);
            Assert.IsNotNull(presenter);

            var chevron = FindNamedDescendant<FrameworkElement>(presenter!, "ExpandCollapseChevron");
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);

            var mouseDown = new MouseButtonEventArgs(Mouse.PrimaryDevice, 1, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = chevron
            };
            chevron.RaiseEvent(mouseDown);

            Assert.IsTrue(mouseDown.Handled);
            Assert.IsFalse(presenter!.IsMouseCaptured);

        });
    }

    [TestMethod]
    public void NavigationViewItemTemplateUsesWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var icon = new ModernWpf.Controls.SymbolIcon { Symbol = ModernWpf.Controls.Symbol.Home };
            var infoBadge = new ModernWpf.Controls.InfoBadge { Value = 7 };
            var menuItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Home",
                Icon = icon,
                InfoBadge = infoBadge
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                IsSettingsVisible = false
            };
            navView.MenuItems.Add(menuItem);

            using var host = new TestWindowHost(navView);

            var iconPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(menuItem, "Icon");
            Assert.AreSame(icon, iconPresenter.Content);

            var contentPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(menuItem, "ContentPresenter");
            Assert.AreEqual("Home", contentPresenter.Content);

            var itemPresenter = VisualTreeTestHelper.FindDescendant<ModernWpf.Controls.Primitives.NavigationViewItemPresenter>(menuItem);
            Assert.IsNotNull(itemPresenter);
            Assert.AreSame(infoBadge, itemPresenter!.InfoBadge);

            var infoBadgePresenter = FindNamedDescendant<ContentPresenter>(itemPresenter, "InfoBadgePresenter");
            Assert.AreSame(infoBadge, infoBadgePresenter.Content);
        });
    }

    [TestMethod]
    public void NavigationViewLeftPaneItemPresenterStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Home",
                Icon = new ModernWpf.Controls.SymbolIcon { Symbol = ModernWpf.Controls.Symbol.Home },
                InfoBadge = new ModernWpf.Controls.InfoBadge { Value = 7 }
            };
            var navView = new ModernWpf.Controls.NavigationView
            {
                IsSettingsVisible = false
            };
            navView.MenuItems.Add(menuItem);

            using var host = new TestWindowHost(navView);

            var presenter = VisualTreeTestHelper.FindDescendant<ModernWpf.Controls.Primitives.NavigationViewItemPresenter>(menuItem);
            Assert.IsNotNull(presenter);
            var itemPresenter = presenter!;
            var layoutRoot = (Border)VisualTreeHelper.GetChild(itemPresenter, 0);
            var icon = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(itemPresenter, "Icon");
            var contentPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(itemPresenter, "ContentPresenter");
            var iconBox = FindNamedDescendant<FrameworkElement>(itemPresenter, "IconBox");
            var iconColumn = FindNamedDescendant<Border>(itemPresenter, "IconColumn");
            var chevron = FindNamedDescendant<FrameworkElement>(itemPresenter, "ExpandCollapseChevron");
            var chevronIcon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(itemPresenter, "ExpandCollapseChevronIcon");
            var contentGrid = FindNamedDescendant<Grid>(itemPresenter, "ContentGrid");
            var infoBadgePresenter = FindNamedDescendant<ContentPresenter>(itemPresenter, "InfoBadgePresenter");

            AssertStateSetter(layoutRoot, "PointerStates", "PointerOver",
                "LayoutRoot.Background",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "Pressed",
                "LayoutRoot.Background",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PointerOverSelected",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PressedSelected",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "DisabledStates", "Disabled",
                "LayoutRoot.Opacity");
            AssertStateSetter(layoutRoot, "IconStates", "IconCollapsed",
                "IconBox.Visibility",
                "IconColumn.Width");
            AssertStateSetter(layoutRoot, "InfoBadgeStates", "InfoBadgeCollapsed",
                "InfoBadgePresenter.Visibility");
            AssertStateSetter(layoutRoot, "ChevronStates", "ChevronVisibleOpen",
                "ExpandCollapseChevron.Visibility",
                "ExpandCollapseChevronIcon.Visibility",
                "ExpandCollapseChevronRotateTransform.Angle");
            AssertChevronAnimatedIconStateSetters(layoutRoot);
            AssertStateSetter(layoutRoot, "PaneAndTopLevelItemStates", "ClosedCompactAndTopLevelItem",
                "ContentPresenter.Margin",
                "ContentGrid.Margin",
                "InfoBadgePresenter.(Grid.Column)",
                "InfoBadgePresenter.(Grid.ColumnSpan)",
                "InfoBadgePresenter.VerticalAlignment",
                "InfoBadgePresenter.HorizontalAlignment",
                "InfoBadgePresenter.Margin");

            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "PointerOver", false));
            AssertCurrentState(layoutRoot, "PointerStates", "PointerOver");
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemBackgroundPointerOver"), layoutRoot.Background);
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPointerOver"), icon.Foreground);
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPointerOver"), contentPresenter.Foreground);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "Pressed", false));
            AssertCurrentState(layoutRoot, "PointerStates", "Pressed");
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPressed"), icon.Foreground);
            Assert.AreEqual("Pressed", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "Disabled", false));
            AssertCurrentState(layoutRoot, "DisabledStates", "Disabled");
            Assert.AreEqual(itemPresenter.TryFindResource("ListViewItemDisabledThemeOpacity"), layoutRoot.Opacity);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "IconCollapsed", false));
            AssertCurrentState(layoutRoot, "IconStates", "IconCollapsed");
            Assert.AreEqual(Visibility.Collapsed, iconBox.Visibility);
            Assert.AreEqual(8.0, iconColumn.Width);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "ChevronVisibleOpen", false));
            AssertCurrentState(layoutRoot, "ChevronStates", "ChevronVisibleOpen");
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);
            AssertChevronAnimatedIconStateTransitions(itemPresenter, layoutRoot, chevronIcon);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "ClosedCompactAndTopLevelItem", false));
            AssertCurrentState(layoutRoot, "PaneAndTopLevelItemStates", "ClosedCompactAndTopLevelItem");
            Assert.AreEqual(itemPresenter.TryFindResource("NavigationViewCompactItemContentPresenterMargin"), contentPresenter.Margin);
            Assert.AreEqual(new Thickness(0), contentGrid.Margin);
            Assert.AreEqual(0, Grid.GetColumn(infoBadgePresenter));
            Assert.AreEqual(4, Grid.GetColumnSpan(infoBadgePresenter));
            Assert.AreEqual(VerticalAlignment.Top, infoBadgePresenter.VerticalAlignment);
            Assert.AreEqual(HorizontalAlignment.Right, infoBadgePresenter.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0, 2, 2, 0), infoBadgePresenter.Margin);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "InfoBadgeCollapsed", false));
            AssertCurrentState(layoutRoot, "InfoBadgeStates", "InfoBadgeCollapsed");
            Assert.AreEqual(Visibility.Collapsed, infoBadgePresenter.Visibility);
        });
    }

    [TestMethod]
    public void NavigationViewTopPaneItemPresenterStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Home",
                Icon = new ModernWpf.Controls.SymbolIcon { Symbol = ModernWpf.Controls.Symbol.Home },
                InfoBadge = new ModernWpf.Controls.InfoBadge { Value = 7 },
                IsExpanded = true
            };
            menuItem.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "Child" });

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top,
                IsSettingsVisible = false,
                Width = 1008
            };
            navView.MenuItems.Add(menuItem);

            using var host = new TestWindowHost(navView);

            var presenter = VisualTreeTestHelper.FindDescendant<ModernWpf.Controls.Primitives.NavigationViewItemPresenter>(menuItem);
            Assert.IsNotNull(presenter);
            var itemPresenter = presenter!;
            var layoutRoot = (Border)VisualTreeHelper.GetChild(itemPresenter, 0);
            var pointerRectangle = FindNamedDescendant<Rectangle>(itemPresenter, "PointerRectangle");
            var icon = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(itemPresenter, "Icon");
            var contentPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(itemPresenter, "ContentPresenter");
            var iconBox = FindNamedDescendant<FrameworkElement>(itemPresenter, "IconBox");
            var selectionIndicatorGrid = FindNamedDescendant<FrameworkElement>(itemPresenter, "SelectionIndicatorGrid");
            var chevron = FindNamedDescendant<FrameworkElement>(itemPresenter, "ExpandCollapseChevron");
            var chevronIcon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(itemPresenter, "ExpandCollapseChevronIcon");
            var chevronRotateTransform = (RotateTransform)chevronIcon.RenderTransform;
            var infoBadgePresenter = FindNamedDescendant<ContentPresenter>(itemPresenter, "InfoBadgePresenter");

            AssertStateSetter(layoutRoot, "PointerStates", "PointerOver",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "Pressed",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PointerOverSelected",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PressedSelected",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "DisabledStates", "Disabled",
                "Icon.Foreground",
                "ContentPresenter.Foreground");
            AssertStateSetter(layoutRoot, "NavigationViewIconPositionStates", "IconOnly",
                "LayoutRoot.Width",
                "LayoutRoot.Height",
                "LayoutRoot.Margin",
                "IconBox.Margin",
                "ContentPresenter.Visibility",
                "SelectionIndicatorGrid.Margin",
                "ExpandCollapseChevron.Margin");
            AssertStateSetter(layoutRoot, "NavigationViewIconPositionStates", "ContentOnly",
                "IconBox.Visibility",
                "ContentPresenter.Margin",
                "SelectionIndicatorGrid.Margin",
                "ExpandCollapseChevron.Margin");
            AssertStateSetter(layoutRoot, "InfoBadgeStates", "InfoBadgeCollapsed",
                "InfoBadgePresenter.Visibility");
            AssertStateSetter(layoutRoot, "ChevronStates", "ChevronVisibleOpen",
                "ExpandCollapseChevron.Visibility",
                "ExpandCollapseChevronRotateTransform.Angle");
            AssertStateSetter(layoutRoot, "PointerChevronStates", "PointerOverChevronVisibleOpen",
                "ExpandCollapseChevronIcon.Foreground");
            AssertStateSetter(layoutRoot, "PointerChevronStates", "PressedChevronVisibleClosed",
                "ExpandCollapseChevronIcon.Foreground");
            AssertChevronAnimatedIconStateSetters(layoutRoot);

            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "PointerOver", false));
            AssertCurrentState(layoutRoot, "PointerStates", "PointerOver");
            Assert.AreSame(itemPresenter.TryFindResource("TopNavigationViewItemBackgroundPointerOver"), layoutRoot.Background);
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemBackgroundPointerOver"), pointerRectangle.Fill);
            Assert.AreSame(itemPresenter.TryFindResource("TopNavigationViewItemForegroundPointerOver"), icon.Foreground);
            Assert.AreSame(itemPresenter.TryFindResource("TopNavigationViewItemForegroundPointerOver"), contentPresenter.Foreground);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "Pressed", false));
            AssertCurrentState(layoutRoot, "PointerStates", "Pressed");
            Assert.AreSame(itemPresenter.TryFindResource("TopNavigationViewItemForegroundPressed"), icon.Foreground);
            Assert.AreEqual("Pressed", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "PointerOverSelected", false));
            AssertCurrentState(layoutRoot, "PointerStates", "PointerOverSelected");
            Assert.AreSame(itemPresenter.TryFindResource("TopNavigationViewItemBackgroundSelectedPointerOver"), layoutRoot.Background);
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemBackgroundSelectedPointerOver"), pointerRectangle.Fill);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "IconOnly", false));
            AssertCurrentState(layoutRoot, "NavigationViewIconPositionStates", "IconOnly");
            Assert.AreEqual(36.0, layoutRoot.Width);
            Assert.AreEqual(36.0, layoutRoot.Height);
            Assert.AreEqual(new Thickness(2), layoutRoot.Margin);
            Assert.AreEqual(new Thickness(10, 0, 10, 0), iconBox.Margin);
            Assert.AreEqual(Visibility.Collapsed, contentPresenter.Visibility);
            Assert.AreEqual(new Thickness(0), selectionIndicatorGrid.Margin);
            Assert.AreEqual(itemPresenter.TryFindResource("TopNavigationViewItemIconOnlyExpandChevronMargin"), chevron.Margin);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "ContentOnly", false));
            AssertCurrentState(layoutRoot, "NavigationViewIconPositionStates", "ContentOnly");
            Assert.AreEqual(Visibility.Collapsed, iconBox.Visibility);
            Assert.AreEqual(itemPresenter.TryFindResource("TopNavigationViewItemContentOnlyContentPresenterMargin"), contentPresenter.Margin);
            Assert.AreEqual(new Thickness(12, 0, 12, 4), selectionIndicatorGrid.Margin);
            Assert.AreEqual(itemPresenter.TryFindResource("TopNavigationViewItemContentOnlyExpandChevronMargin"), chevron.Margin);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "InfoBadgeCollapsed", false));
            AssertCurrentState(layoutRoot, "InfoBadgeStates", "InfoBadgeCollapsed");
            Assert.AreEqual(Visibility.Collapsed, infoBadgePresenter.Visibility);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "ChevronVisibleOpen", false));
            AssertCurrentState(layoutRoot, "ChevronStates", "ChevronVisibleOpen");
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);
            Assert.AreEqual(180.0, chevronRotateTransform.Angle);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "PointerOverChevronVisibleOpen", false));
            AssertCurrentState(layoutRoot, "PointerChevronStates", "PointerOverChevronVisibleOpen");
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPointerOver"), chevronIcon.Foreground);
            AssertChevronAnimatedIconStateTransitions(itemPresenter, layoutRoot, chevronIcon);
        });
    }

    [TestMethod]
    public void NavigationViewTopPaneOverflowItemPresenterStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Overflow",
                Icon = new ModernWpf.Controls.SymbolIcon { Symbol = ModernWpf.Controls.Symbol.Home }
            };

            using var host = new TestWindowHost(item, width: 180, height: 80);

            var itemRoot = FindNamedDescendant<Grid>(item, "NVIRootGrid");
            var overflowStyle = GetStateSetterValue<Style>(
                itemRoot,
                "ItemOnNavigationViewListPositionStates",
                "OnTopNavigationOverflow",
                "NavigationViewItemPresenter.Style");

            var itemPresenter = new ModernWpf.Controls.Primitives.NavigationViewItemPresenter
            {
                Style = overflowStyle,
                Content = "Overflow",
                Icon = new ModernWpf.Controls.SymbolIcon { Symbol = ModernWpf.Controls.Symbol.Home },
                InfoBadge = new ModernWpf.Controls.InfoBadge { Value = 7 }
            };
            host.Window.Content = itemPresenter;
            host.UpdateLayout();

            var layoutRoot = (Grid)VisualTreeHelper.GetChild(itemPresenter, 0);
            var icon = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(itemPresenter, "Icon");
            var contentPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(itemPresenter, "ContentPresenter");
            var iconBox = FindNamedDescendant<FrameworkElement>(itemPresenter, "IconBox");
            var chevron = FindNamedDescendant<FrameworkElement>(itemPresenter, "ExpandCollapseChevron");
            var chevronIcon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(itemPresenter, "ExpandCollapseChevronIcon");
            var chevronRotateTransform = (RotateTransform)chevronIcon.RenderTransform;
            var infoBadgePresenter = FindNamedDescendant<ContentPresenter>(itemPresenter, "InfoBadgePresenter");

            AssertStateSetter(layoutRoot, "PointerStates", "PointerOver",
                "LayoutRoot.Background",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "Pressed",
                "LayoutRoot.Background",
                "Icon.Foreground",
                "ContentPresenter.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PointerOverSelected",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PressedSelected",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "DisabledStates", "Disabled",
                "Icon.Foreground",
                "ContentPresenter.Foreground");
            AssertStateSetter(layoutRoot, "NavigationViewIconPositionStates", "ContentOnly",
                "IconBox.Visibility",
                "ContentPresenter.Margin");
            AssertStateSetter(layoutRoot, "InfoBadgeStates", "InfoBadgeCollapsed",
                "InfoBadgePresenter.Visibility");
            AssertStateSetter(layoutRoot, "ChevronStates", "ChevronVisibleOpen",
                "ExpandCollapseChevron.Visibility",
                "ExpandCollapseChevronRotateTransform.Angle");
            AssertChevronAnimatedIconStateSetters(layoutRoot);

            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "PointerOver", false));
            AssertCurrentState(layoutRoot, "PointerStates", "PointerOver");
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemBackgroundPointerOver"), layoutRoot.Background);
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPointerOver"), icon.Foreground);
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPointerOver"), contentPresenter.Foreground);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "Pressed", false));
            AssertCurrentState(layoutRoot, "PointerStates", "Pressed");
            Assert.AreSame(itemPresenter.TryFindResource("NavigationViewItemForegroundPressed"), icon.Foreground);
            Assert.AreEqual("Pressed", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "ContentOnly", false));
            AssertCurrentState(layoutRoot, "NavigationViewIconPositionStates", "ContentOnly");
            Assert.AreEqual(Visibility.Collapsed, iconBox.Visibility);
            Assert.AreEqual(itemPresenter.TryFindResource("TopNavigationViewItemOnOverflowNoIconContentPresenterMargin"), contentPresenter.Margin);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "InfoBadgeCollapsed", false));
            AssertCurrentState(layoutRoot, "InfoBadgeStates", "InfoBadgeCollapsed");
            Assert.AreEqual(Visibility.Collapsed, infoBadgePresenter.Visibility);

            Assert.IsTrue(VisualStateManager.GoToState(itemPresenter, "ChevronVisibleOpen", false));
            AssertCurrentState(layoutRoot, "ChevronStates", "ChevronVisibleOpen");
            Assert.AreEqual(Visibility.Visible, chevron.Visibility);
            Assert.AreEqual(180.0, chevronRotateTransform.Angle);
            AssertChevronAnimatedIconStateTransitions(itemPresenter, layoutRoot, chevronIcon);
        });
    }

    [TestMethod]
    public void NavigationViewTopPaneSettingsItemStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top,
                IsSettingsVisible = true,
                Width = 1008
            };

            using var host = new TestWindowHost(navView);

            var settingsItem = (ModernWpf.Controls.NavigationViewItem)navView.SettingsItem;
            var infoBadge = new ModernWpf.Controls.InfoBadge { Value = 7 };
            settingsItem.InfoBadge = infoBadge;
            host.UpdateLayout();

            var layoutRoot = FindNamedDescendant<Border>(settingsItem, "LayoutRoot");
            var pointerRectangle = FindNamedDescendant<Rectangle>(settingsItem, "PointerRectangle");
            var icon = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(settingsItem, "Icon");
            var infoBadgePresenter = FindNamedDescendant<ContentPresenter>(settingsItem, "InfoBadgePresenter");
            Assert.AreSame(infoBadge, infoBadgePresenter.Content);

            AssertStateSetter(layoutRoot, "PointerStates", "PointerOver",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "Pressed",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PointerOverSelected",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "PointerStates", "PressedSelected",
                "LayoutRoot.Background",
                "PointerRectangle.Fill",
                "Icon.Foreground",
                "Icon.(ui:AnimatedIcon.State)");
            AssertStateSetter(layoutRoot, "DisabledStates", "Disabled",
                "Icon.Foreground");

            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToElementState(layoutRoot, "PointerOver", false));
            AssertCurrentState(layoutRoot, "PointerStates", "PointerOver");
            Assert.AreSame(settingsItem.TryFindResource("TopNavigationViewItemBackgroundPointerOver"), layoutRoot.Background);
            Assert.AreSame(settingsItem.TryFindResource("NavigationViewItemBackgroundPointerOver"), pointerRectangle.Fill);
            Assert.AreSame(settingsItem.TryFindResource("TopNavigationViewItemForegroundPointerOver"), icon.Foreground);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToElementState(layoutRoot, "Pressed", false));
            AssertCurrentState(layoutRoot, "PointerStates", "Pressed");
            Assert.AreSame(settingsItem.TryFindResource("TopNavigationViewItemForegroundPressed"), icon.Foreground);
            Assert.AreEqual("Pressed", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            Assert.IsTrue(VisualStateManager.GoToElementState(layoutRoot, "PointerOverSelected", false));
            AssertCurrentState(layoutRoot, "PointerStates", "PointerOverSelected");
            Assert.AreSame(settingsItem.TryFindResource("TopNavigationViewItemBackgroundSelectedPointerOver"), layoutRoot.Background);
            Assert.AreSame(settingsItem.TryFindResource("NavigationViewItemBackgroundSelectedPointerOver"), pointerRectangle.Fill);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

            settingsItem.IsEnabled = false;
            host.UpdateLayout();
            AssertCurrentState(layoutRoot, "DisabledStates", "Disabled");
            Assert.AreSame(settingsItem.TryFindResource("TopNavigationViewItemForegroundDisabled"), icon.Foreground);
        });
    }

    [TestMethod]
    public void NavigationViewOverflowButtonStylesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = (ResourceDictionary)Application.LoadComponent(
                new Uri("/ModernWpf.Controls;component/NavigationView/NavigationView.xaml", UriKind.Relative));

            AssertOverflowButtonStyleUsesWinUIVisualStateSetters(
                (Style)resources["NavigationViewOverflowButtonStyleWhenPaneOnTop"]);
            AssertOverflowButtonStyleUsesWinUIVisualStateSetters(
                (Style)resources["NavigationViewOverflowButtonNoLabelStyleWhenPaneOnTop"]);
        });
    }

    [TestMethod]
    public void NavigationViewPaneToggleButtonStyleUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = (ResourceDictionary)Application.LoadComponent(
                new Uri("/ModernWpf;component/Styles/NavigationView.xaml", UriKind.Relative));

            AssertPaneToggleButtonStyleUsesWinUIVisualStateSetters((Style)resources["PaneToggleButtonStyle"]);
        });
    }

    [TestMethod]
    public void NavigationViewPaneToggleButtonTemplateUsesWinUIPresenterSlot()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = CreateNavigationViewHost(
                out var navView,
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Left,
                isPaneOpen: true);

            var toggleButton = FindNamedDescendant<Button>(navView, "TogglePaneButton");
            var contentPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(toggleButton, "ContentPresenter");

            Assert.IsInstanceOfType(contentPresenter.Content, typeof(TextBlock));
            Assert.AreSame(contentPresenter.TryFindResource("NavigationViewItemForegroundChecked"), contentPresenter.Foreground);
            Assert.AreEqual(toggleButton.FontSize, contentPresenter.FontSize);
        });
    }

    [TestMethod]
    public void NavigationViewFooterItemsHostUsesWinUIBottomAnchor()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = CreateNavigationViewHost(
                out var navView,
                ModernWpf.Controls.NavigationViewPaneDisplayMode.Left,
                isPaneOpen: true);

            var footerScrollViewer = FindNamedDescendant<ScrollViewer>(navView, "FooterItemsScrollViewer");
            var scrollHost = VisualTreeHelper.GetParent(footerScrollViewer) as ModernWpf.Controls.ItemsRepeaterScrollHost;

            Assert.IsNotNull(scrollHost);
            Assert.AreEqual(0.0, scrollHost!.HorizontalAnchorRatio);
            Assert.AreEqual(1.0, scrollHost.VerticalAnchorRatio);
        });
    }

    [TestMethod]
    public void NavigationViewItemSeparatorUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var separator = new ModernWpf.Controls.NavigationViewItemSeparator();

            using var host = new TestWindowHost(separator, width: 180, height: 80);

            var root = FindNamedDescendant<Grid>(separator, "NavigationViewItemSeparatorRootGrid");
            var line = FindNamedDescendant<Rectangle>(separator, "SeparatorLine");

            AssertStateSetter(root, "NavigationSeparatorLineStates", "HorizontalLineCompact",
                "SeparatorLine.Margin");
            AssertStateSetter(root, "NavigationSeparatorLineStates", "VerticalLine",
                "SeparatorLine.Height",
                "SeparatorLine.Width",
                "SeparatorLine.Margin",
                "SeparatorLine.VerticalAlignment",
                "SeparatorLine.Fill");

            Assert.IsTrue(VisualStateManager.GoToState(separator, "VerticalLine", false));
            AssertCurrentState(root, "NavigationSeparatorLineStates", "VerticalLine");
            Assert.AreEqual(24.0, line.Height);
            Assert.AreEqual(line.TryFindResource("TopNavigationViewItemSeparatorWidth"), line.Width);
            Assert.AreEqual(line.TryFindResource("TopNavigationViewItemSeparatorMargin"), line.Margin);
            Assert.AreEqual(VerticalAlignment.Center, line.VerticalAlignment);
            Assert.AreSame(line.TryFindResource("TopNavigationViewItemSeparatorForeground"), line.Fill);

            Assert.IsTrue(VisualStateManager.GoToState(separator, "HorizontalLineCompact", false));
            AssertCurrentState(root, "NavigationSeparatorLineStates", "HorizontalLineCompact");
            Assert.AreEqual(line.TryFindResource("NavigationViewCompactItemSeparatorMargin"), line.Margin);
        });
    }

    [TestMethod]
    public void NavigationViewItemPositionStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Home"
            };

            using var host = new TestWindowHost(item, width: 180, height: 80);

            var root = FindNamedDescendant<Grid>(item, "NVIRootGrid");
            var presenter = FindNamedDescendant<ModernWpf.Controls.Primitives.NavigationViewItemPresenter>(
                item, "NavigationViewItemPresenter");
            var flyout = (ModernWpf.Controls.Flyout)item.Template.FindName("ChildrenFlyout", item);
            Assert.IsNotNull(flyout);

            AssertStateSetter(root, "ItemOnNavigationViewListPositionStates", "OnLeftNavigation",
                "NavigationViewItemPresenter.Style");
            AssertStateSetter(root, "ItemOnNavigationViewListPositionStates", "OnTopNavigationPrimary",
                "NavigationViewItemPresenter.Margin",
                "NavigationViewItemPresenter.Foreground",
                "NavigationViewItemPresenter.Style",
                "ChildrenFlyout.Placement");
            AssertStateSetter(root, "ItemOnNavigationViewListPositionStates", "OnTopNavigationOverflow",
                "NavigationViewItemPresenter.Style");

            var onLeftStyle = GetStateSetterValue<Style>(
                root,
                "ItemOnNavigationViewListPositionStates",
                "OnLeftNavigation",
                "NavigationViewItemPresenter.Style");
            var onTopStyle = GetStateSetterValue<Style>(
                root,
                "ItemOnNavigationViewListPositionStates",
                "OnTopNavigationPrimary",
                "NavigationViewItemPresenter.Style");
            var onTopOverflowStyle = GetStateSetterValue<Style>(
                root,
                "ItemOnNavigationViewListPositionStates",
                "OnTopNavigationOverflow",
                "NavigationViewItemPresenter.Style");

            Assert.IsTrue(VisualStateManager.GoToState(item, "OnTopNavigationPrimary", false));
            AssertCurrentState(root, "ItemOnNavigationViewListPositionStates", "OnTopNavigationPrimary");
            Assert.AreEqual(item.TryFindResource("TopNavigationViewItemMargin"), presenter.Margin);
            Assert.AreSame(onTopStyle, presenter.Style);
            Assert.AreEqual(
                ModernWpf.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft,
                flyout.Placement);

            Assert.IsTrue(VisualStateManager.GoToState(item, "OnTopNavigationOverflow", false));
            AssertCurrentState(root, "ItemOnNavigationViewListPositionStates", "OnTopNavigationOverflow");
            Assert.AreSame(onTopOverflowStyle, presenter.Style);

            Assert.IsTrue(VisualStateManager.GoToState(item, "OnLeftNavigation", false));
            AssertCurrentState(root, "ItemOnNavigationViewListPositionStates", "OnLeftNavigation");
            Assert.AreSame(onLeftStyle, presenter.Style);
        });
    }

    [TestMethod]
    public void NavigationViewItemHeaderStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var header = new ModernWpf.Controls.NavigationViewItemHeader
            {
                Content = "Section"
            };

            using var host = new TestWindowHost(header, width: 180, height: 80);

            var root = FindNamedDescendant<Grid>(header, "NavigationViewItemHeaderRootGrid");
            var headerText = FindNamedDescendant<FrameworkElement>(header, "HeaderText");
            var innerHeaderGrid = FindNamedDescendant<Grid>(header, "InnerHeaderGrid");

            AssertStateSetter(root, "PaneStates", "HeaderTextCollapsed",
                "HeaderText.Visibility",
                "InnerHeaderGrid.Height");
            AssertStateSetter(root, "DisplayModeStates", "TopMode",
                "InnerHeaderGrid.Margin");

            Assert.IsTrue(VisualStateManager.GoToState(header, "HeaderTextCollapsed", false));
            AssertCurrentState(root, "PaneStates", "HeaderTextCollapsed");
            Assert.AreEqual(Visibility.Collapsed, headerText.Visibility);
            Assert.AreEqual(0.0, innerHeaderGrid.Height);

            Assert.IsTrue(VisualStateManager.GoToState(header, "TopMode", false));
            AssertCurrentState(root, "DisplayModeStates", "TopMode");
            Assert.AreEqual(header.TryFindResource("TopNavigationViewItemInnerHeaderMargin"), innerHeaderGrid.Margin);
        });
    }

    [TestMethod]
    public void NavigationViewHeaderAndAutoSuggestStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                Header = "Header",
                IsSettingsVisible = false,
                Width = 800.0,
                Height = 600.0,
                Content = "Content"
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");

            AssertStateSetter(root, "HeaderGroup", "HeaderCollapsed", "HeaderContent.Visibility");
            AssertStateSetter(root, "AutoSuggestGroup", "AutoSuggestBoxCollapsed",
                "AutoSuggestArea.Visibility",
                "TopPaneAutoSuggestArea.Visibility");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "HeaderCollapsed", false));
            AssertCurrentState(root, "HeaderGroup", "HeaderCollapsed");
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<FrameworkElement>(navView, "HeaderContent").Visibility);

            Assert.IsTrue(VisualStateManager.GoToState(navView, "AutoSuggestBoxCollapsed", false));
            AssertCurrentState(root, "AutoSuggestGroup", "AutoSuggestBoxCollapsed");
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<FrameworkElement>(navView, "AutoSuggestArea").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindNamedDescendant<FrameworkElement>(navView, "TopPaneAutoSuggestArea").Visibility);
        });
    }

    [TestMethod]
    public void NavigationViewPaneSeparatorUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var separator = FindNamedDescendant<FrameworkElement>(navView, "VisualItemsSeparator");

            AssertStateSetter(root, "PaneSeparatorStates", "SeparatorVisible",
                "VisualItemsSeparator.Visibility");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "SeparatorVisible", false));
            AssertCurrentState(root, "PaneSeparatorStates", "SeparatorVisible");
            Assert.AreEqual(Visibility.Visible, separator.Visibility);
        });
    }

    [TestMethod]
    public void NavigationViewClosedCompactStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var autoSuggestPresenter = FindNamedDescendant<FrameworkElement>(navView, "PaneAutoSuggestBoxPresenter");
            var autoSuggestButton = FindNamedDescendant<FrameworkElement>(navView, "PaneAutoSuggestButton");

            AssertStateSetter(root, "PaneStateGroup", "ClosedCompact",
                "PaneAutoSuggestBoxPresenter.Visibility",
                "PaneAutoSuggestButton.Visibility");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "ClosedCompact", false));
            AssertCurrentState(root, "PaneStateGroup", "ClosedCompact");
            Assert.AreEqual(Visibility.Collapsed, autoSuggestPresenter.Visibility);
            Assert.AreEqual(Visibility.Visible, autoSuggestButton.Visibility);
        });
    }

    [TestMethod]
    public void NavigationViewBackButtonCollapsedStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top,
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var placeholder = navView.Template.FindName("BackButtonPlaceholderOnTopNav", navView) as ColumnDefinition;

            Assert.IsNotNull(placeholder);
            AssertStateSetter(root, "BackButtonGroup", "BackButtonCollapsed",
                "BackButtonPlaceholderOnTopNav.Width");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "BackButtonCollapsed", false));
            AssertCurrentState(root, "BackButtonGroup", "BackButtonCollapsed");
            Assert.AreEqual(new GridLength(0), placeholder!.Width);
        });
    }

    [TestMethod]
    public void NavigationViewPaneCollapsedStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                IsPaneOpen = true,
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var splitView = FindNamedDescendant<ModernWpf.Controls.SplitView>(navView, "RootSplitView");
            var shadowCaster = FindNamedDescendant<ThemeShadowChrome>(navView, "ShadowCaster");
            var paneToggleButtonGrid = FindNamedDescendant<FrameworkElement>(navView, "PaneToggleButtonGrid");

            AssertStateSetter(root, "PaneVisibilityGroup", "PaneCollapsed",
                "RootSplitView.CompactPaneLength",
                "ShadowCaster.Width",
                "PaneToggleButtonGrid.Visibility");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "PaneCollapsed", false));
            AssertCurrentState(root, "PaneVisibilityGroup", "PaneCollapsed");
            Assert.AreEqual(0.0, splitView.CompactPaneLength);
            Assert.AreEqual(0.0, shadowCaster.Width);
            Assert.AreEqual(Visibility.Collapsed, paneToggleButtonGrid.Visibility);
        });
    }

    [TestMethod]
    public void NavigationViewListSizeCompactStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                CompactPaneLength = 72.0,
                IsPaneOpen = true,
                PaneTitle = "Menu",
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var paneContentGrid = FindNamedDescendant<FrameworkElement>(navView, "PaneContentGrid");
            var shadowCaster = FindNamedDescendant<ThemeShadowChrome>(navView, "ShadowCaster");
            var paneTitleTextBlock = FindNamedDescendant<FrameworkElement>(navView, "PaneTitleTextBlock");
            var paneHeaderContentBorder = FindNamedDescendant<FrameworkElement>(navView, "PaneHeaderContentBorder");
            var paneCustomContentBorder = FindNamedDescendant<FrameworkElement>(navView, "PaneCustomContentBorder");
            var footerContentBorder = FindNamedDescendant<FrameworkElement>(navView, "FooterContentBorder");

            AssertStateSetter(root, "PaneStateListSizeGroup", "ListSizeCompact",
                "PaneContentGrid.Width",
                "ShadowCaster.Width",
                "PaneTitleTextBlock.Visibility",
                "PaneHeaderContentBorder.Visibility",
                "PaneContentGrid.HorizontalAlignment",
                "PaneCustomContentBorder.HorizontalAlignment",
                "FooterContentBorder.HorizontalAlignment");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "ListSizeCompact", false));
            AssertCurrentState(root, "PaneStateListSizeGroup", "ListSizeCompact");
            Assert.AreEqual(72.0, paneContentGrid.Width);
            Assert.AreEqual(72.0, shadowCaster.Width);
            Assert.AreEqual(Visibility.Collapsed, paneTitleTextBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, paneHeaderContentBorder.Visibility);
            Assert.AreEqual(HorizontalAlignment.Left, paneContentGrid.HorizontalAlignment);
            Assert.AreEqual(HorizontalAlignment.Left, paneCustomContentBorder.HorizontalAlignment);
            Assert.AreEqual(HorizontalAlignment.Left, footerContentBorder.HorizontalAlignment);
        });
    }

    [TestMethod]
    public void NavigationViewTogglePaneButtonVisibleStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneTitle = "Menu",
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var toggleButtonRow = navView.Template.FindName("PaneContentGridToggleButtonRow", navView) as RowDefinition;
            var paneTitlePresenter = FindNamedDescendant<FrameworkElement>(navView, "PaneTitlePresenter");

            Assert.IsNotNull(toggleButtonRow);
            AssertStateSetter(root, "TogglePaneGroup", "TogglePaneButtonVisible",
                "PaneContentGridToggleButtonRow.MinHeight",
                "PaneTitlePresenter.Margin");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "TogglePaneButtonVisible", false));
            AssertCurrentState(root, "TogglePaneGroup", "TogglePaneButtonVisible");
            Assert.AreEqual(root.TryFindResource("NavigationViewPaneHeaderRowMinHeight"), toggleButtonRow!.MinHeight);
            Assert.AreEqual(root.TryFindResource("NavigationViewItemInnerHeaderMargin"), paneTitlePresenter.Margin);
        });
    }

    [TestMethod]
    public void NavigationViewOverflowButtonNoLabelStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top,
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var overflowButton = FindNamedDescendant<Button>(navView, "TopNavOverflowButton");

            AssertStateSetter(root, "OverflowLabelGroup", "OverflowButtonNoLabel",
                "TopNavOverflowButton.Style");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "OverflowButtonNoLabel", false));
            AssertCurrentState(root, "OverflowLabelGroup", "OverflowButtonNoLabel");
            Assert.AreSame(root.TryFindResource("NavigationViewOverflowButtonNoLabelStyleWhenPaneOnTop"), overflowButton.Style);
        });
    }

    [TestMethod]
    public void NavigationViewDisplayModeStatesUseWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = CreateNavigationViewHost(out var navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var headerContent = (FrameworkElement)navView.Template.FindName("HeaderContent", navView);
            var backButton = (Button)navView.Template.FindName("NavigationViewBackButton", navView);
            var contentGrid = (Border)navView.Template.FindName("ContentGrid", navView);
            Assert.IsNotNull(headerContent);
            Assert.IsNotNull(backButton);
            Assert.IsNotNull(contentGrid);

            AssertStateSetter(root, "DisplayModeGroup", "Minimal",
                "HeaderContent.Margin",
                "NavigationViewBackButton.Style",
                "ContentGrid.BorderThickness",
                "ContentGrid.CornerRadius",
                "ContentGrid.Margin");
            AssertStateSetter(root, "DisplayModeGroup", "TopNavigationMinimal",
                "ContentGrid.BorderThickness",
                "ContentGrid.CornerRadius",
                "ContentGrid.Margin");
            AssertStateSetter(root, "DisplayModeGroup", "MinimalWithBackButton",
                "HeaderContent.Margin",
                "NavigationViewBackButton.Style",
                "ContentGrid.BorderThickness",
                "ContentGrid.CornerRadius",
                "ContentGrid.Margin");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "MinimalWithBackButton", false));
            AssertCurrentState(root, "DisplayModeGroup", "MinimalWithBackButton");
            Assert.AreEqual(new Thickness(-24, 44, 0, 0), headerContent.Margin);
            Assert.AreSame(navView.FindResource("NavigationBackButtonSmallStyle"), backButton.Style);
            Assert.AreEqual(new Thickness(0, 1, 0, 0), contentGrid.BorderThickness);
            Assert.AreEqual(new CornerRadius(0), contentGrid.CornerRadius);
            Assert.AreEqual(new Thickness(0), contentGrid.Margin);

            Assert.IsTrue(VisualStateManager.GoToState(navView, "TopNavigationMinimal", false));
            AssertCurrentState(root, "DisplayModeGroup", "TopNavigationMinimal");
            Assert.AreEqual(new Thickness(0, 1, 0, 0), contentGrid.BorderThickness);
            Assert.AreEqual(new CornerRadius(0), contentGrid.CornerRadius);
            Assert.AreEqual(new Thickness(0), contentGrid.Margin);
        });
    }

    [TestMethod]
    public void NavigationViewPaneNotOverlayingStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            using var host = CreateNavigationViewHost(out var navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var splitView = (ModernWpf.Controls.SplitView)navView.Template.FindName("RootSplitView", navView);
            var paneContentGrid = (Border)navView.Template.FindName("PaneContentGrid", navView);
            var shadowCaster = (ThemeShadowChrome)navView.Template.FindName("ShadowCaster", navView);
            var shadowCasterTransform = navView.Template.FindName("ShadowCasterTransform", navView);
            Assert.IsNotNull(splitView);
            Assert.IsNotNull(paneContentGrid);
            Assert.IsNotNull(shadowCaster);
            AssertNavigationViewPaneOverlayShadow(shadowCaster);
            Assert.IsInstanceOfType(shadowCasterTransform, typeof(TranslateTransform));

            AssertStateSetter(root, "PaneOverlayGroup", "PaneNotOverlaying",
                "RootSplitView.BorderBrush",
                "ShadowCaster.Opacity",
                "RootSplitView.CornerRadius",
                "RootSplitView.BorderThickness",
                "PaneContentGrid.BorderThickness",
                "RootSplitView.PaneBackground");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "PaneNotOverlaying", false));
            AssertCurrentState(root, "PaneOverlayGroup", "PaneNotOverlaying");

            Assert.IsInstanceOfType(splitView.BorderBrush, typeof(SolidColorBrush));
            var borderBrush = (SolidColorBrush)splitView.BorderBrush;
            Assert.AreEqual(Colors.Transparent, borderBrush.Color);
            Assert.AreEqual(new CornerRadius(0), splitView.CornerRadius);
            Assert.AreEqual(new Thickness(0), splitView.BorderThickness);
            Assert.AreEqual(new Thickness(0, 0, 1, 0), paneContentGrid.BorderThickness);
            Assert.AreEqual(0.0, shadowCaster.Opacity);
            Assert.AreSame(navView.FindResource("NavigationViewExpandedPaneBackground"), splitView.PaneBackground);
        });
    }

    [TestMethod]
    public void NavigationViewTitleBarCollapsedStateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                MenuItems =
                {
                    new ModernWpf.Controls.NavigationViewItem
                    {
                        Content = "Home"
                    }
                }
            };

            using var host = new TestWindowHost(navView);

            var root = FindNamedDescendant<Grid>(navView, "RootGrid");
            var paneContentGrid = FindNamedDescendant<FrameworkElement>(navView, "PaneContentGrid");

            AssertStateSetter(root, "TitleBarVisibilityGroup", "TitleBarCollapsed",
                "PaneContentGrid.Margin");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "TitleBarCollapsed", false));
            AssertCurrentState(root, "TitleBarVisibilityGroup", "TitleBarCollapsed");
            Assert.AreEqual(new Thickness(0, 32, 0, 0), paneContentGrid.Margin);
        });
    }

    [TestMethod]
    public void VerifyOverflowButtonToolTip()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView
            {
                PaneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Top
            };

            using var host = new TestWindowHost(navView);

            var overflowButton = FindNamedDescendant<Button>(navView, "TopNavOverflowButton");
            Assert.AreEqual("More", GetToolTipContent(overflowButton));
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2NavigationViewBackButtonStyle()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var navView = new ModernWpf.Controls.NavigationView();
            using var host = new TestWindowHost(navView);

            Assert.AreEqual(40.0, navView.TryFindResource("NavigationBackButtonWidth"));
            Assert.AreEqual(36.0, navView.TryFindResource("NavigationBackButtonHeight"));

            var normalStyle = AssertStyleResource(navView, "NavigationBackButtonNormalStyle");
            AssertDynamicResourceSetter(normalStyle, Control.BackgroundProperty, "NavigationViewBackButtonBackground");
            AssertDynamicResourceSetter(normalStyle, Control.ForegroundProperty, "NavigationViewItemForeground");
            AssertDynamicResourceSetter(normalStyle, FrameworkElement.HeightProperty, "NavigationBackButtonHeight");
            AssertDynamicResourceSetter(normalStyle, FrameworkElement.WidthProperty, "NavigationBackButtonWidth");
            AssertSetterValue(normalStyle, Control.FontSizeProperty, 16.0);
            AssertSetterValue(normalStyle, FrameworkElement.MarginProperty, new Thickness(4, 2, 4, 2));
            AssertSetterValue(
                normalStyle,
                ModernWpf.Controls.Primitives.ButtonHelper.VisualStateSettersEnabledProperty,
                true);

            var button = new Button
            {
                Style = normalStyle
            };
            using var buttonHost = new TestWindowHost(button);

            var rootGrid = FindNamedDescendant<Border>(button, "RootGrid");
            var content = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(button, "Content");

            Assert.AreEqual(0, button.Template.Triggers.Count);
            AssertStateSetter(rootGrid, "CommonStates", "PointerOver",
                "RootGrid.Background",
                "Content.Foreground",
                "Content.(local:AnimatedIcon.State)");
            Assert.AreEqual(
                "PointerOver",
                GetStateSetterValue<string>(rootGrid, "CommonStates", "PointerOver", "Content.(local:AnimatedIcon.State)"));
            AssertStateSetter(rootGrid, "CommonStates", "Pressed",
                "RootGrid.Background",
                "Content.Foreground",
                "Content.(local:AnimatedIcon.State)");
            Assert.AreEqual(
                "Pressed",
                GetStateSetterValue<string>(rootGrid, "CommonStates", "Pressed", "Content.(local:AnimatedIcon.State)"));
            AssertStateSetter(rootGrid, "CommonStates", "Disabled",
                "Content.Foreground");

            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(content));
            Assert.IsTrue(VisualStateManager.GoToState(button, "PointerOver", false));
            Assert.AreSame(button.TryFindResource("NavigationViewButtonBackgroundPointerOver"), rootGrid.Background);
            Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundPointerOver"), content.Foreground);
            Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(content));
            Assert.IsTrue(VisualStateManager.GoToState(button, "Pressed", false));
            Assert.AreSame(button.TryFindResource("NavigationViewButtonBackgroundPressed"), rootGrid.Background);
            Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundPressed"), content.Foreground);
            Assert.AreEqual("Pressed", ModernWpf.Controls.AnimatedIcon.GetState(content));
            button.IsEnabled = false;
            buttonHost.UpdateLayout();
            AssertCurrentState(rootGrid, "CommonStates", "Disabled");
            Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundDisabled"), content.Foreground);
            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(content));

            button.IsEnabled = true;
            buttonHost.UpdateLayout();
            Assert.IsTrue(VisualStateManager.GoToState(button, "Normal", false));
            Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(content));

            var smallStyle = AssertStyleResource(navView, "NavigationBackButtonSmallStyle");
            Assert.AreSame(normalStyle, smallStyle.BasedOn);
            AssertSetterValue(smallStyle, FrameworkElement.MarginProperty, new Thickness(4, 2, 0, 2));
            AssertNoLocalSetter(smallStyle, Control.FontSizeProperty);
            AssertNoLocalSetter(smallStyle, FrameworkElement.HeightProperty);
            AssertNoLocalSetter(smallStyle, FrameworkElement.WidthProperty);
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2NavigationViewThemeResources()
    {
        WpfTestHost.Run(() =>
        {
            var lightDarkResourceReferences = new[]
            {
                ("NavigationViewDefaultPaneBackground", "SolidBackgroundFillColorBaseBrush"),
                ("NavigationViewExpandedPaneBackground", "ControlFillColorTransparentBrush"),
                ("NavigationViewTopPaneBackground", "ControlFillColorTransparentBrush"),
                ("NavigationViewContentBackground", "SolidBackgroundFillColorBaseBrush"),
                ("NavigationViewItemBackground", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                ("NavigationViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush"),
                ("NavigationViewItemBackgroundDisabled", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBackgroundChecked", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBackgroundCheckedPointerOver", "SubtleFillColorSecondaryBrush"),
                ("NavigationViewItemBackgroundCheckedPressed", "SubtleFillColorTertiaryBrush"),
                ("NavigationViewItemBackgroundCheckedDisabled", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush"),
                ("NavigationViewItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush"),
                ("NavigationViewItemBackgroundSelectedPressed", "SubtleFillColorSecondaryBrush"),
                ("NavigationViewItemBackgroundSelectedDisabled", "SubtleFillColorSecondaryBrush"),
                ("NavigationViewItemForeground", "TextFillColorSecondaryBrush"),
                ("NavigationViewItemForegroundPointerOver", "TextFillColorSecondaryBrush"),
                ("NavigationViewItemForegroundPressed", "TextFillColorPrimaryBrush"),
                ("NavigationViewItemForegroundDisabled", "TextFillColorDisabledBrush"),
                ("NavigationViewItemForegroundChecked", "TextFillColorPrimaryBrush"),
                ("NavigationViewItemForegroundCheckedPointerOver", "TextFillColorPrimaryBrush"),
                ("NavigationViewItemForegroundCheckedPressed", "TextFillColorSecondaryBrush"),
                ("NavigationViewItemForegroundCheckedDisabled", "TextFillColorDisabledBrush"),
                ("NavigationViewItemForegroundSelected", "TextFillColorPrimaryBrush"),
                ("NavigationViewItemForegroundSelectedPointerOver", "TextFillColorPrimaryBrush"),
                ("NavigationViewItemForegroundSelectedPressed", "TextFillColorSecondaryBrush"),
                ("NavigationViewItemForegroundSelectedDisabled", "TextFillColorDisabledBrush"),
                ("NavigationViewItemBorderBrush", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushPointerOver", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushPressed", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushDisabled", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushChecked", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushCheckedPointerOver", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushCheckedPressed", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushCheckedDisabled", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushSelected", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushSelectedPointerOver", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushSelectedPressed", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemBorderBrushSelectedDisabled", "SubtleFillColorTransparentBrush"),
                ("NavigationViewItemSeparatorForeground", "DividerStrokeColorDefaultBrush"),
                ("NavigationViewSelectionIndicatorForeground", "AccentFillColorDefaultBrush"),
                ("TopNavigationViewItemForeground", "TextFillColorSecondaryBrush"),
                ("TopNavigationViewItemForegroundPointerOver", "TextFillColorPrimaryBrush"),
                ("TopNavigationViewItemForegroundPressed", "TextFillColorPrimaryBrush"),
                ("TopNavigationViewItemForegroundDisabled", "TextFillColorDisabledBrush"),
                ("TopNavigationViewItemForegroundSelected", "TextFillColorPrimaryBrush"),
                ("TopNavigationViewItemForegroundSelectedPointerOver", "TextFillColorSecondaryBrush"),
                ("TopNavigationViewItemForegroundSelectedPressed", "TextFillColorTertiaryBrush"),
                ("TopNavigationViewItemBackgroundPointerOver", "SubtleFillColorTransparentBrush"),
                ("TopNavigationViewItemBackgroundPressed", "SubtleFillColorTransparentBrush"),
                ("TopNavigationViewItemBackgroundSelected", "SubtleFillColorTransparentBrush"),
                ("TopNavigationViewItemBackgroundSelectedPointerOver", "SubtleFillColorTransparentBrush"),
                ("TopNavigationViewItemBackgroundSelectedPressed", "SubtleFillColorTransparentBrush"),
                ("TopNavigationViewItemSeparatorForeground", "DividerStrokeColorDefaultBrush"),
                ("NavigationViewButtonBackgroundPointerOver", "SubtleFillColorSecondaryBrush"),
                ("NavigationViewButtonBackgroundPressed", "SubtleFillColorTertiaryBrush"),
                ("NavigationViewButtonBackgroundDisabled", "ControlFillColorDisabledBrush"),
                ("NavigationViewButtonForegroundPointerOver", "TextFillColorPrimaryBrush"),
                ("NavigationViewButtonForegroundPressed", "TextFillColorSecondaryBrush"),
                ("NavigationViewButtonForegroundDisabled", "TextFillColorDisabledBrush"),
                ("NavigationViewBackButtonBackground", "SubtleFillColorTransparentBrush")
            };

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                foreach (var (resourceKey, expectedResourceKey) in lightDarkResourceReferences)
                {
                    AssertThemeResourceReference(themeName, resourceKey, expectedResourceKey);
                }
            }

            var highContrastResourceReferences = new[]
            {
                ("NavigationViewDefaultPaneBackground", "AcrylicInAppFillColorDefaultBrush"),
                ("NavigationViewExpandedPaneBackground", "SystemColorWindowColorBrush"),
                ("NavigationViewTopPaneBackground", "AcrylicInAppFillColorDefaultBrush"),
                ("NavigationViewItemBackground", "SystemControlBackgroundBaseLowBrush"),
                ("NavigationViewItemBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundDisabled", "SystemControlBackgroundBaseLowBrush"),
                ("NavigationViewItemBackgroundChecked", "SystemControlTransparentRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundCheckedPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundCheckedPressed", "SystemControlHighlightListMediumRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundCheckedDisabled", "SystemControlTransparentRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundSelected", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundSelectedPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundSelectedPressed", "SystemControlHighlightListMediumRevealBackgroundBrush"),
                ("NavigationViewItemBackgroundSelectedDisabled", "SystemControlTransparentRevealBackgroundBrush"),
                ("NavigationViewItemForeground", "SystemControlForegroundBaseHighBrush"),
                ("NavigationViewItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("NavigationViewItemForegroundChecked", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundCheckedPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundCheckedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundCheckedDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("NavigationViewItemForegroundSelected", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundSelectedPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundSelectedPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewItemForegroundSelectedDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("NavigationViewItemBorderBrush", "SystemControlTransparentBrush"),
                ("NavigationViewItemBorderBrushPointerOver", "SystemControlHighlightAltTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushPressed", "SystemControlHighlightAltTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushDisabled", "SystemControlTransparentBrush"),
                ("NavigationViewItemBorderBrushChecked", "SystemControlBackgroundTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushCheckedPointerOver", "SystemControlHighlightAltTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushCheckedPressed", "SystemControlHighlightAltTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushCheckedDisabled", "SystemControlTransparentBrush"),
                ("NavigationViewItemBorderBrushSelected", "SystemControlBackgroundTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushSelectedPointerOver", "SystemControlHighlightAltTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushSelectedPressed", "SystemControlHighlightAltTransparentRevealBorderBrush"),
                ("NavigationViewItemBorderBrushSelectedDisabled", "SystemControlTransparentBrush"),
                ("NavigationViewItemSeparatorForeground", "SystemControlForegroundBaseLowBrush"),
                ("NavigationViewSelectionIndicatorForeground", "SystemColorHighlightTextColorBrush"),
                ("TopNavigationViewItemForeground", "NavigationViewItemForeground"),
                ("TopNavigationViewItemForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("TopNavigationViewItemForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("TopNavigationViewItemForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("TopNavigationViewItemForegroundSelected", "SystemControlHighlightAltBaseHighBrush"),
                ("TopNavigationViewItemForegroundSelectedPointerOver", "NavigationViewItemForeground"),
                ("TopNavigationViewItemForegroundSelectedPressed", "NavigationViewItemForeground"),
                ("TopNavigationViewItemBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("TopNavigationViewItemBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush"),
                ("TopNavigationViewItemBackgroundSelected", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("TopNavigationViewItemBackgroundSelectedPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush"),
                ("TopNavigationViewItemBackgroundSelectedPressed", "SystemControlHighlightListMediumRevealBackgroundBrush"),
                ("TopNavigationViewItemSeparatorForeground", "SystemControlForegroundBaseLowBrush"),
                ("NavigationViewButtonBackgroundPointerOver", "SystemControlHighlightListLowBrush"),
                ("NavigationViewButtonBackgroundPressed", "SystemControlHighlightListMediumBrush"),
                ("NavigationViewButtonBackgroundDisabled", "SystemControlBackgroundBaseLowBrush"),
                ("NavigationViewButtonForegroundPointerOver", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewButtonForegroundPressed", "SystemControlHighlightAltBaseHighBrush"),
                ("NavigationViewButtonForegroundDisabled", "SystemControlDisabledBaseMediumLowBrush"),
                ("NavigationViewBackButtonBackground", "SystemControlBackgroundBaseLowBrush")
            };

            foreach (var (resourceKey, expectedResourceKey) in highContrastResourceReferences)
            {
                AssertThemeResourceReference("HighContrast", resourceKey, expectedResourceKey);
            }

            AssertThemeSolidColorBrushColorReference("HighContrast", "NavigationViewContentBackground", "SystemChromeMediumColor");
        });
    }

    private static void AssertNavigationViewPaneOverlayShadow(ThemeShadowChrome shadowCaster)
    {
        Assert.AreEqual(16.0, shadowCaster.Depth);
        Assert.AreEqual(ThemeShadowChromeWindowedPopupInsetMode.Default, shadowCaster.WindowedPopupInsetMode);
        Assert.AreEqual(new Thickness(8, 4, 8, 12), shadowCaster.ShadowPadding);
    }

    private static object GetToolTipContent(FrameworkElement element)
    {
        var toolTip = ToolTipService.GetToolTip(element);
        return toolTip is ToolTip toolTipElement ? toolTipElement.Content : toolTip;
    }

    private static Style AssertStyleResource(FrameworkElement element, object resourceKey)
    {
        var style = element.TryFindResource(resourceKey) as Style;
        Assert.IsNotNull(style, $"Expected style resource '{resourceKey}'.");
        return style!;
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = GetLocalSetter(style, property);
        var dynamicResource = setter.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertSetterValue<T>(Style style, DependencyProperty property, T expectedValue)
    {
        var setter = GetLocalSetter(style, property);
        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static void AssertNoLocalSetter(Style style, DependencyProperty property)
    {
        Assert.IsFalse(
            style.Setters.OfType<Setter>().Any(setter => setter.Property == property),
            $"Expected no local setter for {property.Name}.");
    }

    private static Setter GetLocalSetter(Style style, DependencyProperty property)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        return setter!;
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeSolidColorBrushColorReference(string themeName, string resourceKey, object expectedColorResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedColorResourceKey), $"{themeName} is missing {expectedColorResourceKey}.");
        Assert.IsInstanceOfType(themeDictionary[resourceKey], typeof(SolidColorBrush), $"{themeName}:{resourceKey}");
        Assert.AreEqual(
            themeDictionary[expectedColorResourceKey],
            ((SolidColorBrush)themeDictionary[resourceKey]).Color,
            $"{themeName}:{resourceKey}");
    }

    private static void AssertOverflowButtonStyleUsesWinUIVisualStateSetters(Style style)
    {
        Assert.IsNotNull(style);
        AssertSetterValue(
            style,
            ModernWpf.Controls.Primitives.ButtonHelper.VisualStateSettersEnabledProperty,
            true);

        var button = new Button
        {
            Style = style
        };

        using var host = new TestWindowHost(button);

        var rootGrid = FindNamedDescendant<Grid>(button, "RootGrid");
        var pointerRectangle = FindNamedDescendant<Rectangle>(button, "PointerRectangle");
        var icon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(button, "Icon");

        AssertStateSetter(rootGrid, "CommonStates", "PointerOver",
            "RootGrid.Background",
            "PointerRectangle.Fill",
            "Icon.Foreground");
        AssertStateSetter(rootGrid, "CommonStates", "Pressed",
            "RootGrid.Background",
            "PointerRectangle.Fill",
            "Icon.Foreground");
        AssertStateSetter(rootGrid, "CommonStates", "Disabled",
            "Icon.Foreground");

        Assert.IsTrue(VisualStateManager.GoToElementState(rootGrid, "PointerOver", false));
        AssertCurrentState(rootGrid, "CommonStates", "PointerOver");
        Assert.AreSame(button.TryFindResource("TopNavigationViewItemBackgroundPointerOver"), rootGrid.Background);
        Assert.AreSame(button.TryFindResource("NavigationViewItemBackgroundPointerOver"), pointerRectangle.Fill);
        Assert.AreSame(button.TryFindResource("TopNavigationViewItemForegroundPointerOver"), icon.Foreground);

        Assert.IsTrue(VisualStateManager.GoToElementState(rootGrid, "Pressed", false));
        AssertCurrentState(rootGrid, "CommonStates", "Pressed");
        Assert.AreSame(button.TryFindResource("TopNavigationViewItemBackgroundPressed"), rootGrid.Background);
        Assert.AreSame(button.TryFindResource("NavigationViewItemBackgroundPressed"), pointerRectangle.Fill);
        Assert.AreSame(button.TryFindResource("TopNavigationViewItemForegroundPressed"), icon.Foreground);

        button.IsEnabled = false;
        host.UpdateLayout();
        AssertCurrentState(rootGrid, "CommonStates", "Disabled");
        Assert.AreSame(button.TryFindResource("TopNavigationViewItemForegroundDisabled"), icon.Foreground);
    }

    private static void AssertPaneToggleButtonStyleUsesWinUIVisualStateSetters(Style style)
    {
        Assert.IsNotNull(style);
        AssertSetterValue(
            style,
            ModernWpf.Controls.Primitives.ButtonHelper.VisualStateSettersEnabledProperty,
            true);

        var button = new Button
        {
            Style = style,
            Content = "Menu"
        };

        using var host = new TestWindowHost(button);

        var layoutRoot = FindNamedDescendant<Border>(button, "LayoutRoot");
        var contentPresenter = FindNamedDescendant<ModernWpf.Controls.ContentPresenterEx>(button, "ContentPresenter");
        var icon = FindNamedDescendant<ModernWpf.Controls.FontIconFallback>(button, "Icon");

        AssertStateSetter(layoutRoot, "CommonStates", "PointerOver",
            "LayoutRoot.Background",
            "ContentPresenter.Foreground",
            "Icon.Foreground",
            "Icon.(local:AnimatedIcon.State)");
        AssertStateSetter(layoutRoot, "CommonStates", "Pressed",
            "LayoutRoot.Background",
            "ContentPresenter.Foreground",
            "Icon.Foreground",
            "Icon.(local:AnimatedIcon.State)");
        AssertStateSetter(layoutRoot, "CommonStates", "Disabled",
            "LayoutRoot.Background",
            "ContentPresenter.Foreground");

        Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(icon));

        Assert.IsTrue(VisualStateManager.GoToElementState(layoutRoot, "PointerOver", false));
        AssertCurrentState(layoutRoot, "CommonStates", "PointerOver");
        Assert.AreSame(button.TryFindResource("NavigationViewButtonBackgroundPointerOver"), layoutRoot.Background);
        Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundPointerOver"), contentPresenter.Foreground);
        Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundPointerOver"), icon.Foreground);
        Assert.AreEqual("PointerOver", ModernWpf.Controls.AnimatedIcon.GetState(icon));

        Assert.IsTrue(VisualStateManager.GoToElementState(layoutRoot, "Pressed", false));
        AssertCurrentState(layoutRoot, "CommonStates", "Pressed");
        Assert.AreSame(button.TryFindResource("NavigationViewButtonBackgroundPressed"), layoutRoot.Background);
        Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundPressed"), contentPresenter.Foreground);
        Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundPressed"), icon.Foreground);
        Assert.AreEqual("Pressed", ModernWpf.Controls.AnimatedIcon.GetState(icon));

        button.IsEnabled = false;
        host.UpdateLayout();
        AssertCurrentState(layoutRoot, "CommonStates", "Disabled");
        Assert.AreSame(button.TryFindResource("NavigationViewButtonBackgroundDisabled"), layoutRoot.Background);
        Assert.AreSame(button.TryFindResource("NavigationViewButtonForegroundDisabled"), contentPresenter.Foreground);
        Assert.AreEqual("Normal", ModernWpf.Controls.AnimatedIcon.GetState(icon));
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T match && match.Name == name)
            {
                return match;
            }
        }

        Assert.Fail($"Could not find descendant named '{name}'.");
        throw new InvalidOperationException();
    }

    private static void AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var actualTargets = state.Setters
            .Select(setter => string.IsNullOrEmpty(setter.Target) ? setter.Property : setter.Target)
            .ToArray();

        CollectionAssert.IsSubsetOf(expectedTargets, actualTargets);
    }

    private static T GetStateSetterValue<T>(FrameworkElement stateGroupsRoot, string groupName, string stateName, string target)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var setter = state.Setters.Single(item => item.Target == target);
        return (T)setter.Value;
    }

    private static void AssertChevronAnimatedIconStateSetters(FrameworkElement stateGroupsRoot)
    {
        const string target = "ExpandCollapseChevronIcon.(ui:AnimatedIcon.State)";

        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "NormalChevronVisibleOpen", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "NormalChevronVisibleClosed", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronHidden", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronVisibleOpen", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronVisibleClosed", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "PressedChevronHidden", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "PressedChevronVisibleOpen", target);
        AssertStateSetter(stateGroupsRoot, "PointerChevronStates", "PressedChevronVisibleClosed", target);

        Assert.AreEqual("NormalOn", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "NormalChevronVisibleOpen", target));
        Assert.AreEqual("NormalOff", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "NormalChevronVisibleClosed", target));
        Assert.AreEqual("PointerOverOff", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronHidden", target));
        Assert.AreEqual("PointerOverOn", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronVisibleOpen", target));
        Assert.AreEqual("PointerOverOff", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronVisibleClosed", target));
        Assert.AreEqual("PressedOff", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "PressedChevronHidden", target));
        Assert.AreEqual("PressedOn", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "PressedChevronVisibleOpen", target));
        Assert.AreEqual("PressedOff", GetStateSetterValue<string>(stateGroupsRoot, "PointerChevronStates", "PressedChevronVisibleClosed", target));
    }

    private static void AssertChevronAnimatedIconStateTransitions(
        Control control,
        FrameworkElement stateGroupsRoot,
        DependencyObject chevronIcon)
    {
        Assert.IsTrue(VisualStateManager.GoToState(control, "NormalChevronVisibleClosed", false));
        AssertCurrentState(stateGroupsRoot, "PointerChevronStates", "NormalChevronVisibleClosed");
        Assert.AreEqual("NormalOff", ModernWpf.Controls.AnimatedIcon.GetState(chevronIcon));

        Assert.IsTrue(VisualStateManager.GoToState(control, "NormalChevronVisibleOpen", false));
        AssertCurrentState(stateGroupsRoot, "PointerChevronStates", "NormalChevronVisibleOpen");
        Assert.AreEqual("NormalOn", ModernWpf.Controls.AnimatedIcon.GetState(chevronIcon));

        Assert.IsTrue(VisualStateManager.GoToState(control, "PointerOverChevronVisibleOpen", false));
        AssertCurrentState(stateGroupsRoot, "PointerChevronStates", "PointerOverChevronVisibleOpen");
        Assert.AreEqual("PointerOverOn", ModernWpf.Controls.AnimatedIcon.GetState(chevronIcon));

        Assert.IsTrue(VisualStateManager.GoToState(control, "PressedChevronVisibleClosed", false));
        AssertCurrentState(stateGroupsRoot, "PointerChevronStates", "PressedChevronVisibleClosed");
        Assert.AreEqual("PressedOff", ModernWpf.Controls.AnimatedIcon.GetState(chevronIcon));
    }

    private static void AssertCurrentState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        Assert.AreEqual(expectedStateName, FindVisualStateGroup(stateGroupsRoot, groupName).CurrentState?.Name);
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        return VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
    }

    private static TestWindowHost CreateNavigationViewHost(
        out ModernWpf.Controls.NavigationView navView,
        ModernWpf.Controls.NavigationViewPaneDisplayMode paneDisplayMode = ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto,
        bool isPaneOpen = true,
        ModernWpf.Controls.NavigationViewDisplayMode displayMode = ModernWpf.Controls.NavigationViewDisplayMode.Expanded)
    {
        navView = new ModernWpf.Controls.NavigationView
        {
            PaneTitle = "Title",
            IsBackButtonVisible = ModernWpf.Controls.NavigationViewBackButtonVisible.Visible,
            IsSettingsVisible = true,
            PaneDisplayMode = paneDisplayMode,
            IsPaneOpen = isPaneOpen,
            OpenPaneLength = 120.0,
            ExpandedModeThresholdWidth = 600.0,
            CompactModeThresholdWidth = 400.0,
            Width = 800.0,
            Height = 600.0,
            Content = "This is a simple test"
        };
        navView.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "Undo" });
        navView.MenuItems.Add(new ModernWpf.Controls.NavigationViewItem { Content = "Cut" });

        if (paneDisplayMode == ModernWpf.Controls.NavigationViewPaneDisplayMode.Auto)
        {
            navView.Width = displayMode switch
            {
                ModernWpf.Controls.NavigationViewDisplayMode.Minimal => navView.CompactModeThresholdWidth - 10.0,
                ModernWpf.Controls.NavigationViewDisplayMode.Compact => navView.ExpandedModeThresholdWidth - 10.0,
                _ => navView.ExpandedModeThresholdWidth + 10.0
            };
        }

        return new TestWindowHost(navView);
    }
}
