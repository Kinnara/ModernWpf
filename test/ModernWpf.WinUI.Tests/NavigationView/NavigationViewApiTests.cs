using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
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
    public void NavigationViewItemTemplateUsesWinUIPresenterSlots()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var icon = new ModernWpf.Controls.SymbolIcon { Symbol = ModernWpf.Controls.Symbol.Home };
            var menuItem = new ModernWpf.Controls.NavigationViewItem
            {
                Content = "Home",
                Icon = icon
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
            var paneToggleButtonGrid = FindNamedDescendant<FrameworkElement>(navView, "PaneToggleButtonGrid");

            AssertStateSetter(root, "PaneVisibilityGroup", "PaneCollapsed",
                "RootSplitView.CompactPaneLength",
                "PaneToggleButtonGrid.Visibility");

            Assert.IsTrue(VisualStateManager.GoToState(navView, "PaneCollapsed", false));
            AssertCurrentState(root, "PaneVisibilityGroup", "PaneCollapsed");
            Assert.AreEqual(0.0, splitView.CompactPaneLength);
            Assert.AreEqual(Visibility.Collapsed, paneToggleButtonGrid.Visibility);
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
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "NavigationViewDefaultPaneBackground", "SolidBackgroundFillColorBaseBrush");
                AssertThemeResourceReference(themeName, "NavigationViewTopPaneBackground", "ControlFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemBackgroundPressed", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemBackgroundSelected", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemBackgroundSelectedPointerOver", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewItemForegroundSelected", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewSelectionIndicatorForeground", "AccentFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "TopNavigationViewItemForegroundSelectedPressed", "TextFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "NavigationViewBackButtonBackground", "SubtleFillColorTransparentBrush");
            }

            AssertThemeResourceReference("HighContrast", "NavigationViewDefaultPaneBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewTopPaneBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewItemBackground", "SystemControlBackgroundBaseLowBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewItemBackgroundPointerOver", "SystemControlHighlightListLowRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewItemBackgroundPressed", "SystemControlHighlightListMediumRevealBackgroundBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewItemForeground", "SystemControlForegroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewItemForegroundSelected", "SystemControlHighlightAltBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "NavigationViewSelectionIndicatorForeground", "SystemColorHighlightTextColorBrush");
            AssertThemeResourceReference("HighContrast", "TopNavigationViewItemForeground", "NavigationViewItemForeground");
            AssertThemeResourceReference("HighContrast", "NavigationViewBackButtonBackground", "SystemControlBackgroundBaseLowBrush");
        });
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
