using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using MuxGridView = ModernWpf.Controls.GridView;
using MuxGridViewItem = ModernWpf.Controls.GridViewItem;
using MuxListView = ModernWpf.Controls.ListView;
using MuxListViewItem = ModernWpf.Controls.ListViewItem;

namespace ModernWpf.WinUI.Tests.ListView;

[TestClass]
public class ListViewApiTests
{
    [TestMethod]
    public void ListViewItemTemplateUsesWinUICommonAndMultiSelectStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new MuxListViewItem { Content = "Item" };
            using var host = new TestWindowHost(item, width: 240, height: 80);

            var root = FindTemplateChild<FrameworkElement>(item, "ContentBorder");
            Assert.AreEqual(0, item.Template.Triggers.Count);
            AssertStateSetter(root, "CommonStates", "PointerOver", "ContentBorder.Background", "ContentPresenter.Foreground");
            AssertStateSetter(root, "CommonStates", "Selected", "ContentBorder.Background", "MultiSelectCheck.Opacity");
            AssertStateSetter(root, "CommonStates", "PointerOverSelected", "ContentBorder.Background", "MultiSelectCheck.Opacity");
            AssertStateSetter(root, "CommonStates", "PressedSelected", "ContentBorder.Background", "MultiSelectCheck.Opacity");
            AssertStateSetter(root, "CommonStates", "SelectedDisabled", "ContentBorder.Background", "ContentBorder.Opacity");
            AssertStateSetter(root, "MultiSelectStates", "NoMultiSelect", "MultiSelectSquare.Visibility", "ContentPresenterGrid.Margin");
            AssertStateSetter(root, "MultiSelectStates", "ListMultiSelect", "MultiSelectSquare.Visibility", "ContentPresenterGrid.Margin");
        });
    }

    [TestMethod]
    public void GridViewItemTemplateUsesWinUICommonAndMultiSelectStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new MuxGridViewItem { Content = "Item" };
            using var host = new TestWindowHost(item, width: 120, height: 120);

            var root = FindTemplateChild<FrameworkElement>(item, "ContentBorder");
            var presenter = FindTemplateChild<ModernWpf.Controls.ContentPresenterEx>(item, "ContentPresenter");
            Assert.AreEqual(item.TryFindResource("GridViewItemCornerRadius"), item.CornerRadius);
            Assert.IsTrue(presenter.ClipToBounds);
            Assert.AreEqual(item.CornerRadius, presenter.CornerRadius);
            Assert.AreEqual(0, item.Template.Triggers.Count);
            AssertStateSetter(root, "CommonStates", "PointerOver", "ContentBorder.Background", "ContentPresenter.Foreground");
            AssertStateSetter(root, "CommonStates", "Selected", "ContentBorder.Background", "SelectedBorder.Opacity", "MultiSelectCheck.Opacity");
            AssertStateSetter(root, "CommonStates", "PointerOverSelected", "ContentBorder.Background", "SelectedBorder.Opacity");
            AssertStateSetter(root, "CommonStates", "PressedSelected", "ContentBorder.Background", "SelectedBorder.Opacity");
            AssertStateSetter(root, "CommonStates", "SelectedDisabled", "ContentBorder.Background", "ContentBorder.Opacity");
            AssertStateSetter(root, "MultiSelectStates", "NoMultiSelect", "MultiSelectSquare.Visibility");
            AssertStateSetter(root, "MultiSelectStates", "GridMultiSelect", "MultiSelectSquare.Visibility");
        });
    }

    [TestMethod]
    public void ListViewFamilyEnablesWpfTouchPanning()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var list in new Control[] { new MuxListView(), new MuxGridView() })
            {
                using var host = new TestWindowHost(list, width: 260, height: 140);
                var scrollViewer = FindTemplateChild<ScrollViewer>(list, "ScrollViewer");

                Assert.AreEqual(PanningMode.Both, ScrollViewer.GetPanningMode(list));
                Assert.AreEqual(PanningMode.Both, scrollViewer.PanningMode);
                Assert.IsTrue(scrollViewer.IsManipulationEnabled);
            }
        });
    }

    [TestMethod]
    public void GridViewMouseWheelScrollsHorizontalOverflow()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var gridView = new MuxGridView
            {
                ItemsPanel = (ItemsPanelTemplate)XamlReader.Parse(
                    """
                    <ItemsPanelTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                        <StackPanel Orientation="Horizontal" />
                    </ItemsPanelTemplate>
                    """)
            };
            ScrollViewer.SetHorizontalScrollBarVisibility(gridView, ScrollBarVisibility.Visible);
            ScrollViewer.SetVerticalScrollBarVisibility(gridView, ScrollBarVisibility.Disabled);

            for (int index = 0; index < 4; index++)
            {
                gridView.Items.Add(new Border { Width = 120, Height = 60 });
            }

            using var host = new TestWindowHost(gridView, width: 180, height: 120);
            host.UpdateLayout();

            var scrollViewer = FindTemplateChild<ModernWpf.Controls.ScrollViewerEx>(gridView, "ScrollViewer");
            Assert.IsTrue(scrollViewer.ScrollableWidth > 0, "The GridView must have horizontal overflow.");
            Assert.AreEqual(0, scrollViewer.ScrollableHeight, 0.01, "The regression requires no vertical extent.");

            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = scrollViewer
            };

            scrollViewer.RaiseEvent(args);
            host.UpdateLayout();

            Assert.IsTrue(args.Handled, "The horizontal-only ScrollViewerEx should consume the wheel event.");
            Assert.IsTrue(scrollViewer.HorizontalOffset > 0, "The mouse wheel should move horizontal overflow.");
            Assert.AreEqual(0, scrollViewer.VerticalOffset, 0.01);
        });
    }

    [TestMethod]
    public void ListViewBaseItemDrivesWinUINamedSelectionAndMultiSelectStates()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new MuxListViewItem { Content = "Item" };
            var listView = new MuxListView
            {
                SelectionMode = SelectionMode.Multiple
            };
            listView.Items.Add(item);

            using var host = new TestWindowHost(listView, width: 260, height: 100);
            host.UpdateLayout();

            var root = FindTemplateChild<FrameworkElement>(item, "ContentBorder");
            AssertCurrentState(root, "MultiSelectStates", "ListMultiSelect");

            item.IsSelected = true;
            host.UpdateLayout();
            AssertCurrentState(root, "CommonStates", "Selected");

            item.IsEnabled = false;
            host.UpdateLayout();
            AssertCurrentState(root, "CommonStates", "SelectedDisabled");
        });
    }

    [TestMethod]
    public void GridViewBaseItemDrivesGridMultiSelectState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new MuxGridViewItem { Content = "Item" };
            var gridView = new MuxGridView
            {
                SelectionMode = SelectionMode.Multiple
            };
            gridView.Items.Add(item);

            using var host = new TestWindowHost(gridView, width: 260, height: 140);
            host.UpdateLayout();

            var root = FindTemplateChild<FrameworkElement>(item, "ContentBorder");
            AssertCurrentState(root, "MultiSelectStates", "GridMultiSelect");
        });
    }

    [TestMethod]
    public void ItemClickUsesOwnContainerContentAndSpaceKey()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            object? clickedItem = null;
            var item = new MuxListViewItem { Content = "Own container content" };
            var listView = new MuxListView
            {
                IsItemClickEnabled = true
            };
            listView.Items.Add(item);
            listView.ItemClick += (_, args) => clickedItem = args.ClickedItem;

            using var host = new TestWindowHost(listView, width: 260, height: 100);
            host.UpdateLayout();

            RaiseKey(item, Keyboard.KeyDownEvent, Key.Space);

            Assert.AreEqual("Own container content", clickedItem);
        });
    }

    [TestMethod]
    public void ItemClickUsesOwnGridContainerContent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            object? clickedItem = null;
            var item = new MuxGridViewItem { Content = "Own grid item content" };
            var gridView = new MuxGridView
            {
                IsItemClickEnabled = true
            };
            gridView.Items.Add(item);
            gridView.ItemClick += (_, args) => clickedItem = args.ClickedItem;

            using var host = new TestWindowHost(gridView, width: 260, height: 140);
            host.UpdateLayout();

            gridView.NotifyListItemClicked(item);

            Assert.AreEqual("Own grid item content", clickedItem);
        });
    }

    [TestMethod]
    public void GridViewItemAutomationInvokeRaisesItemClick()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            object? clickedItem = null;
            var item = new MuxGridViewItem { Content = "Automation grid item content" };
            var gridView = new MuxGridView
            {
                IsItemClickEnabled = true
            };
            gridView.Items.Add(item);
            gridView.ItemClick += (_, args) => clickedItem = args.ClickedItem;

            using var host = new TestWindowHost(gridView, width: 260, height: 140);
            host.UpdateLayout();

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(gridView);
            var itemPeer = peer.GetChildren().Single();
            var invokeProvider = itemPeer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

            Assert.IsNotNull(invokeProvider);

            invokeProvider!.Invoke();

            Assert.AreEqual("Automation grid item content", clickedItem);
        });
    }

    [TestMethod]
    public void AutomationPeersMatchWinUIClassTypesAndConditionalInvokePattern()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var listView = new MuxListView();
            var listPeer = new ModernWpf.Controls.ListViewBaseAutomationPeer(listView);
            var listItemPeer = new ModernWpf.Controls.ListViewBaseItemAutomationPeer(
                new MuxListViewItem { Content = "List item" },
                listPeer);
            Assert.AreEqual("ListView", listPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.List, listPeer.GetAutomationControlType());
            Assert.AreEqual("ListViewItem", listItemPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.ListItem, listItemPeer.GetAutomationControlType());
            Assert.IsNull(listItemPeer.GetPattern(PatternInterface.Invoke));

            var gridView = new MuxGridView();
            var gridPeer = new ModernWpf.Controls.ListViewBaseAutomationPeer(gridView);
            var gridItemPeer = new ModernWpf.Controls.ListViewBaseItemAutomationPeer(
                new MuxGridViewItem { Content = "Grid item" },
                gridPeer);
            Assert.AreEqual("GridView", gridPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.List, gridPeer.GetAutomationControlType());
            Assert.AreEqual("GridViewItem", gridItemPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.ListItem, gridItemPeer.GetAutomationControlType());
            Assert.IsNull(gridItemPeer.GetPattern(PatternInterface.Invoke));

            gridView.IsItemClickEnabled = true;
            Assert.IsNotNull(gridItemPeer.GetPattern(PatternInterface.Invoke));
        });
    }

    [TestMethod]
    public void FocusVisualBrushPropertiesUseBrushTypes()
    {
        WpfTestHost.Run(() =>
        {
            Assert.AreEqual(typeof(Brush), typeof(MuxListViewItem).GetProperty(nameof(MuxListViewItem.FocusVisualPrimaryBrush))!.PropertyType);
            Assert.AreEqual(typeof(Brush), typeof(MuxListViewItem).GetProperty(nameof(MuxListViewItem.FocusVisualSecondaryBrush))!.PropertyType);
            Assert.AreEqual(typeof(Brush), typeof(MuxGridViewItem).GetProperty(nameof(MuxGridViewItem.FocusVisualPrimaryBrush))!.PropertyType);
            Assert.AreEqual(typeof(Brush), typeof(MuxGridViewItem).GetProperty(nameof(MuxGridViewItem.FocusVisualSecondaryBrush))!.PropertyType);
        });
    }

    [TestMethod]
    public void ListViewSourceSelectedDisabledResourcesExistInEveryTheme()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
                Assert.IsTrue(themeDictionary.Contains("ListViewItemBackgroundSelectedDisabled"), themeName);
                Assert.IsTrue(themeDictionary.Contains("GridViewItemBackgroundSelectedDisabled"), themeName);
            }
        });
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}'.");
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States
            .OfType<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        foreach (var expectedTarget in expectedTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == expectedTarget),
                $"{groupName}.{stateName} should set {expectedTarget}.");
        }
    }

    private static void AssertCurrentState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        Assert.AreEqual(expectedStateName, FindVisualStateGroup(stateGroupsRoot, groupName).CurrentState?.Name);
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        return VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(group => group.Name == groupName);
    }

    private static void RaiseKey(UIElement element, RoutedEvent routedEvent, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            Environment.TickCount,
            key)
        {
            RoutedEvent = routedEvent,
            Source = element
        };

        element.RaiseEvent(args);
    }

}
