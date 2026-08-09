using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.TabView;

[TestClass]
public class TabViewApiTests
{
    [TestMethod]
    public void DefaultsMatchCurrentWinUISurface()
    {
        WpfTestHost.Run(() =>
        {
            var tabView = new ModernWpf.Controls.TabView();
            var item = new TabViewItem();

            Assert.AreEqual(TabViewWidthMode.Equal, tabView.TabWidthMode);
            Assert.AreEqual(TabViewCloseButtonOverlayMode.Auto, tabView.CloseButtonOverlayMode);
            Assert.IsTrue(tabView.IsAddTabButtonVisible);
            Assert.IsFalse(tabView.CanDragTabs);
            Assert.IsTrue(tabView.CanReorderTabs);
            Assert.IsTrue(tabView.AllowDropTabs);
            Assert.IsFalse(tabView.CanTearOutTabs);
            Assert.AreEqual(0, tabView.SelectedIndex);
            Assert.IsNull(tabView.SelectedItem);
            Assert.IsNotNull(tabView.TabItems);
            Assert.AreEqual(0, tabView.TabItems.Count);

            Assert.IsNull(item.Header);
            Assert.IsNull(item.HeaderTemplate);
            Assert.IsNull(item.IconSource);
            Assert.IsTrue(item.IsClosable);
            Assert.IsNotNull(item.TabViewTemplateSettings);
            Assert.IsNull(item.TabViewTemplateSettings.IconElement);
            Assert.IsNull(item.TabViewTemplateSettings.TabGeometry);
        });
    }

    [TestMethod]
    public void ExplicitItemsSelectDisplayAndResolveContainers()
    {
        WpfTestHost.Run(() =>
        {
            var firstContent = new TextBlock { Text = "First content" };
            var secondContent = new TextBlock { Text = "Second content" };
            var first = new TabViewItem { Header = "First", Content = firstContent };
            var second = new TabViewItem { Header = "Second", Content = secondContent };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);

            var changed = 0;
            tabView.SelectionChanged += (_, _) => changed++;

            using var host = new TestWindowHost(tabView, width: 640, height: 320);

            Assert.AreEqual(0, tabView.SelectedIndex);
            Assert.AreSame(first, tabView.SelectedItem);
            Assert.IsTrue(first.IsSelected);
            Assert.IsFalse(second.IsSelected);
            Assert.AreSame(first, tabView.ContainerFromItem(first));
            Assert.AreSame(second, tabView.ContainerFromIndex(1));
            Assert.AreSame(firstContent, Part<ContentPresenter>(tabView, "PART_TabContentPresenter").Content);

            tabView.SelectedIndex = 1;
            host.UpdateLayout();

            Assert.AreSame(second, tabView.SelectedItem);
            Assert.IsFalse(first.IsSelected);
            Assert.IsTrue(second.IsSelected);
            Assert.AreSame(secondContent, Part<ContentPresenter>(tabView, "PART_TabContentPresenter").Content);
            Assert.AreEqual(1, changed);
        });
    }

    [TestMethod]
    public void ObservableSourceUsesTabViewItemTemplateAndForwardsChanges()
    {
        WpfTestHost.Run(() =>
        {
            var source = new ObservableCollection<TabData>
            {
                new("One", "Content one"),
                new("Two", "Content two")
            };
            var tabView = new ModernWpf.Controls.TabView
            {
                TabItemsSource = source,
                TabItemTemplate = CreateTabTemplate()
            };
            var changes = 0;
            tabView.TabItemsChanged += (_, _) => changes++;

            using var host = new TestWindowHost(tabView, width: 640, height: 320);
            host.UpdateLayout();

            var first = tabView.ContainerFromItem(source[0]) as TabViewItem;
            var second = tabView.ContainerFromIndex(1) as TabViewItem;
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual("One", first.Header);
            Assert.AreEqual("Content one", first.Content);
            Assert.AreSame(source[0], first.DataContext);
            Assert.AreSame(source[0], tabView.SelectedItem);

            tabView.SelectedItem = source[1];
            host.UpdateLayout();
            Assert.AreEqual(1, tabView.SelectedIndex);
            Assert.IsTrue(second.IsSelected);
            Assert.AreEqual("Content two", Part<ContentPresenter>(tabView, "PART_TabContentPresenter").Content);

            source.RemoveAt(1);
            host.UpdateLayout();
            Assert.AreEqual(0, tabView.SelectedIndex);
            Assert.AreSame(source[0], tabView.SelectedItem);
            Assert.IsTrue(changes >= 1);
        });
    }

    [TestMethod]
    public void RemovalSelectsNearestEnabledVisibleTab()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var disabled = new TabViewItem { Header = "Disabled", IsEnabled = false };
            var selected = new TabViewItem { Header = "Selected" };
            var last = new TabViewItem { Header = "Last" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(disabled);
            tabView.TabItems.Add(selected);
            tabView.TabItems.Add(last);

            using var host = new TestWindowHost(tabView, width: 640, height: 320);
            tabView.SelectedItem = selected;

            tabView.TabItems.Remove(selected);
            host.UpdateLayout();

            Assert.AreEqual(2, tabView.SelectedIndex);
            Assert.AreSame(last, tabView.SelectedItem);
            Assert.IsFalse(disabled.IsSelected);

            tabView.TabItems.Clear();
            Assert.AreEqual(-1, tabView.SelectedIndex);
            Assert.IsNull(tabView.SelectedItem);
        });
    }

    [TestMethod]
    public void CloseRequestPreservesApplicationCollectionOwnership()
    {
        WpfTestHost.Run(() =>
        {
            var item = new TabViewItem { Header = "Closable", Content = "Document" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(item);
            TabViewTabCloseRequestedEventArgs? viewArgs = null;
            TabViewTabCloseRequestedEventArgs? itemArgs = null;
            tabView.TabCloseRequested += (_, args) => viewArgs = args;
            item.CloseRequested += (_, args) => itemArgs = args;

            using var host = new TestWindowHost(tabView, width: 400, height: 200);
            var closeButton = Descendants(item).OfType<ButtonBase>().Single(button => button.Name == "PART_CloseButton");
            closeButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.IsNotNull(viewArgs);
            Assert.AreSame(viewArgs, itemArgs);
            Assert.AreSame(item, viewArgs.Item);
            Assert.AreSame(item, viewArgs.Tab);
            Assert.AreEqual(1, tabView.TabItems.Count, "Close requests must not remove application-owned data.");
        });
    }

    [TestMethod]
    public void MiddleClickRequestsCloseAndRightClickDoesNotChangeSelection()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var second = new TabViewItem { Header = "Second" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);
            TabViewTabCloseRequestedEventArgs? closeArgs = null;
            tabView.TabCloseRequested += (_, args) => closeArgs = args;

            using var host = new TestWindowHost(tabView, width: 500, height: 240);
            Assert.AreSame(first, tabView.SelectedItem);

            second.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseDownEvent,
                Source = second
            });
            Assert.AreSame(first, tabView.SelectedItem, "Right-click must not select a background tab.");

            second.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = second
            });
            Assert.IsNotNull(closeArgs);
            Assert.AreSame(second, closeArgs.Tab);
            Assert.AreSame(second, closeArgs.Item);
            Assert.AreSame(first, tabView.SelectedItem, "A middle-click close request must preserve application-owned selection.");
        });
    }

    [TestMethod]
    public void CtrlShortcutsWrapSkipUnavailableTabsAndRequestClose()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var disabled = new TabViewItem { Header = "Disabled", IsEnabled = false };
            var hidden = new TabViewItem { Header = "Hidden", Visibility = Visibility.Collapsed };
            var last = new TabViewItem { Header = "Last" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(disabled);
            tabView.TabItems.Add(hidden);
            tabView.TabItems.Add(last);
            TabViewTabCloseRequestedEventArgs? closeArgs = null;
            tabView.TabCloseRequested += (_, args) => closeArgs = args;

            using var host = new TestWindowHost(tabView, width: 600, height: 240);
            Assert.AreSame(first, tabView.SelectedItem);

            Assert.IsTrue(tabView.ProcessKeyboardShortcut(Key.Tab, ModifierKeys.Control));
            Assert.AreSame(last, tabView.SelectedItem);
            Assert.IsTrue(tabView.ProcessKeyboardShortcut(Key.Tab, ModifierKeys.Control));
            Assert.AreSame(first, tabView.SelectedItem, "Ctrl+Tab must wrap.");
            Assert.IsTrue(tabView.ProcessKeyboardShortcut(Key.Tab, ModifierKeys.Control | ModifierKeys.Shift));
            Assert.AreSame(last, tabView.SelectedItem, "Ctrl+Shift+Tab must wrap backward.");

            Assert.IsTrue(tabView.ProcessKeyboardShortcut(Key.F4, ModifierKeys.Control));
            Assert.IsNotNull(closeArgs);
            Assert.AreSame(last, closeArgs.Tab);
            Assert.IsFalse(tabView.ProcessKeyboardShortcut(Key.F4, ModifierKeys.None));

            closeArgs = null;
            last.IsClosable = false;
            Assert.IsFalse(tabView.ProcessKeyboardShortcut(Key.F4, ModifierKeys.Control));
            Assert.IsNull(closeArgs);
        });
    }

    [TestMethod]
    public void AutomationReportsRequiredSingleTabSelection()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var second = new TabViewItem { Header = "Second" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);

            using var host = new TestWindowHost(tabView, width: 500, height: 240);

            var viewPeer = FrameworkElementAutomationPeer.CreatePeerForElement(tabView);
            var selection = (ISelectionProvider)viewPeer.GetPattern(PatternInterface.Selection);
            var itemPeer = FrameworkElementAutomationPeer.CreatePeerForElement(second);
            var selectionItem = (ISelectionItemProvider)itemPeer.GetPattern(PatternInterface.SelectionItem);
            selectionItem.Select();

            Assert.AreEqual("TabView", viewPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.Tab, viewPeer.GetAutomationControlType());
            Assert.IsFalse(selection.CanSelectMultiple);
            Assert.IsTrue(selection.IsSelectionRequired);
            Assert.AreEqual(1, selection.GetSelection().Length);
            Assert.AreSame(second, tabView.SelectedItem);
            Assert.IsTrue(selectionItem.IsSelected);
            Assert.AreEqual("Second", itemPeer.GetName());
            Assert.AreEqual("TabViewItem", itemPeer.GetClassName());
            Assert.AreEqual(AutomationControlType.TabItem, itemPeer.GetAutomationControlType());
            Assert.IsNotNull(selectionItem.SelectionContainer);
            Assert.IsNotNull(itemPeer.GetPattern(PatternInterface.ScrollItem));
        });
    }

    [TestMethod]
    public void WidthModesAndIconTemplateSettingsAreLive()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem
            {
                Header = "First",
                IconSource = new SymbolIconSource { Symbol = Symbol.Home }
            };
            var second = new TabViewItem
            {
                Header = "Second",
                IconSource = new SymbolIconSource { Symbol = Symbol.Document }
            };
            var tabView = new ModernWpf.Controls.TabView { Width = 400 };
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);

            using var host = new TestWindowHost(tabView, width: 440, height: 240);
            Assert.IsNotNull(first.TabViewTemplateSettings.IconElement);
            Assert.AreEqual(first.Width, second.Width, 0.01);

            tabView.TabWidthMode = TabViewWidthMode.Compact;
            host.UpdateLayout();
            Assert.AreEqual(48d, second.Width, 0.01);
            Assert.IsTrue(double.IsNaN(first.Width));

            tabView.TabWidthMode = TabViewWidthMode.SizeToContent;
            host.UpdateLayout();
            Assert.IsTrue(double.IsNaN(first.Width));
            Assert.IsTrue(double.IsNaN(second.Width));
        });
    }

    [TestMethod]
    public void PublicResourceOverridesAndDragVisualRemainLive()
    {
        WpfTestHost.Run(() =>
        {
            var item = new TabViewItem { Header = "Document" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.Resources["TabViewItemMinHeight"] = 52.0;
            tabView.Resources["TabViewButtonBorderThickness"] = new Thickness(2.0);
            tabView.TabItems.Add(item);

            using var host = new TestWindowHost(tabView, width: 500, height: 240);
            Assert.AreEqual(52.0, item.MinHeight);
            var addButton = Part<ButtonBase>(tabView, "PART_AddButton");
            Assert.AreEqual(new Thickness(2.0), addButton.BorderThickness);
            Assert.IsNotNull(item.DragVisual);
            Assert.AreEqual(Visibility.Collapsed, item.DragVisual.Visibility);

            item.SetDragging(true);
            Assert.AreEqual(Visibility.Visible, item.DragVisual.Visibility);
            item.SetDragging(false);
            Assert.AreEqual(Visibility.Collapsed, item.DragVisual.Visibility);

            tabView.Resources["TabViewItemMinHeight"] = 44.0;
            tabView.Resources["TabViewButtonBorderThickness"] = new Thickness(1.0);
            WpfTestHost.DoEvents();
            Assert.AreEqual(44.0, item.MinHeight);
            Assert.AreEqual(new Thickness(1.0), addButton.BorderThickness);
        });
    }

    [TestMethod]
    public void ScrollButtonsCloseButtonsAndSeparatorsUseSourceStateResources()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var second = new TabViewItem { Header = "Second" };
            var third = new TabViewItem { Header = "Third" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);
            tabView.TabItems.Add(third);

            using var host = new TestWindowHost(tabView, width: 500, height: 240);
            var decreaseButton = Part<RepeatButton>(tabView, "PART_ScrollDecreaseButton");
            var increaseButton = Part<RepeatButton>(tabView, "PART_ScrollIncreaseButton");
            Assert.IsFalse(decreaseButton.IsEnabled);
            Assert.IsFalse(increaseButton.IsEnabled);
            AssertBrushMatches(tabView, "TabViewScrollButtonBackgroundDisabled", decreaseButton.Background);
            AssertBrushMatches(tabView, "TabViewScrollButtonForegroundDisabled", decreaseButton.Foreground);
            AssertBrushMatches(tabView, "TabViewScrollButtonBorderBrushDisabled", decreaseButton.BorderBrush);
            AssertBrushMatches(tabView, "TabViewScrollButtonBackgroundDisabled", increaseButton.Background);
            AssertBrushMatches(second, "TabViewItemHeaderCloseButtonBackground", second.CloseButton.Background);
            AssertBrushMatches(second, "TabViewItemHeaderCloseButtonForeground", second.CloseButton.Foreground);

            Assert.IsNotNull(first.Separator);
            Assert.IsNotNull(second.Separator);
            Assert.IsNotNull(third.Separator);
            Assert.AreEqual(0.0, first.Separator.Opacity, "The selected tab's separator is hidden.");
            Assert.AreEqual(1.0, second.Separator.Opacity);
            Assert.AreEqual(1.0, third.Separator.Opacity);

            tabView.SelectedIndex = 1;
            WpfTestHost.DoEvents();
            Assert.AreEqual(0.0, first.Separator.Opacity, "The separator left of the selected tab is hidden.");
            Assert.AreEqual(0.0, second.Separator.Opacity, "The selected tab's separator is hidden.");
            Assert.AreEqual(1.0, third.Separator.Opacity);

            tabView.SelectedIndex = 2;
            WpfTestHost.DoEvents();
            Assert.AreEqual(1.0, first.Separator.Opacity);
            Assert.AreEqual(0.0, second.Separator.Opacity, "The separator left of the selected tab is hidden.");
            Assert.AreEqual(0.0, third.Separator.Opacity, "The selected tab's separator is hidden.");
        });
    }

    [TestMethod]
    public void CloseButtonModesMatchCurrentWinUIAndKeepKeyboardTabbingLocal()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var second = new TabViewItem { Header = "Second" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);

            using var host = new TestWindowHost(tabView, width: 500, height: 240);
            var firstClose = first.CloseButton;
            var secondClose = second.CloseButton;
            Assert.IsNotNull(firstClose);
            Assert.IsNotNull(secondClose);

            Assert.AreEqual(Visibility.Visible, firstClose.Visibility);
            Assert.AreEqual(Visibility.Visible, secondClose.Visibility, "Auto keeps every closable tab button visible.");
            Assert.IsFalse(firstClose.IsTabStop);
            Assert.IsFalse(secondClose.IsTabStop);

            tabView.CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.OnPointerOver;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, firstClose.Visibility, "The selected tab remains closable without hover.");
            Assert.AreEqual(Visibility.Collapsed, secondClose.Visibility);

            tabView.SelectedIndex = 1;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, firstClose.Visibility);
            Assert.AreEqual(Visibility.Visible, secondClose.Visibility);

            tabView.CloseButtonOverlayMode = TabViewCloseButtonOverlayMode.Always;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, firstClose.Visibility);
            Assert.AreEqual(Visibility.Visible, secondClose.Visibility);

            second.IsClosable = false;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, secondClose.Visibility);
        });
    }

    [TestMethod]
    public void SelectedTabGeometryIsLiveAndLocaleIndependent()
    {
        WpfTestHost.Run(() =>
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

                var item = new TabViewItem { Header = "Geometry" };
                var tabView = new ModernWpf.Controls.TabView();
                tabView.Resources["OverlayCornerRadius"] = new CornerRadius(13.5, 7.25, 0.0, 0.0);
                tabView.TabItems.Add(item);

                using var host = new TestWindowHost(tabView, width: 420, height: 220);
                host.UpdateLayout();

                var geometry = item.TabViewTemplateSettings.TabGeometry as StreamGeometry;
                Assert.IsNotNull(geometry);
                Assert.IsTrue(geometry.IsFrozen);
                Assert.IsTrue(geometry.Bounds.Width > item.ActualWidth);
                Assert.IsTrue(geometry.Bounds.Height > 0.0);
                Assert.IsFalse(double.IsNaN(geometry.Bounds.Width));
                Assert.IsFalse(double.IsInfinity(geometry.Bounds.Width));

                var firstPath = geometry.ToString(CultureInfo.InvariantCulture);
                tabView.Resources["OverlayCornerRadius"] = new CornerRadius(4.0);
                host.UpdateLayout();
                var updatedGeometry = item.TabViewTemplateSettings.TabGeometry as StreamGeometry;
                Assert.IsNotNull(updatedGeometry);
                Assert.AreNotEqual(firstPath, updatedGeometry.ToString(CultureInfo.InvariantCulture));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        });
    }

    [TestMethod]
    public void OverflowButtonsAreLocalizedScrollAndBringSelectionIntoView()
    {
        WpfTestHost.Run(() =>
        {
            var tabView = new ModernWpf.Controls.TabView();
            for (var index = 0; index < 8; index++)
            {
                tabView.TabItems.Add(new TabViewItem { Header = $"Document {index + 1}" });
            }

            using var host = new TestWindowHost(tabView, width: 420, height: 240);
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            var scrollViewer = Part<ScrollViewer>(tabView, "PART_ScrollViewer");
            var decrease = Part<RepeatButton>(tabView, "PART_ScrollDecreaseButton");
            var increase = Part<RepeatButton>(tabView, "PART_ScrollIncreaseButton");
            var add = Part<ButtonBase>(tabView, "PART_AddButton");
            Assert.IsTrue(scrollViewer.ScrollableWidth > 0.0);
            Assert.AreEqual(Visibility.Visible, decrease.Visibility);
            Assert.AreEqual(Visibility.Visible, increase.Visibility);
            Assert.IsFalse(decrease.IsEnabled);
            Assert.IsTrue(increase.IsEnabled);
            Assert.AreEqual("Add New Tab", AutomationProperties.GetName(add));
            Assert.AreEqual("Add new tab", ToolTipService.GetToolTip(add));
            Assert.AreEqual("Scroll tab list backward", ToolTipService.GetToolTip(decrease));
            Assert.AreEqual("Scroll tab list forward", ToolTipService.GetToolTip(increase));

            increase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            WpfTestHost.DoEvents();
            Assert.IsTrue(scrollViewer.HorizontalOffset > 0.0);
            Assert.IsTrue(decrease.IsEnabled);

            var priorOffset = scrollViewer.HorizontalOffset;
            tabView.SelectedIndex = 7;
            WpfTestHost.DoEvents();
            Assert.IsTrue(scrollViewer.HorizontalOffset >= priorOffset);
            Assert.IsFalse(increase.IsEnabled);

            host.Window.Width = 1200;
            host.UpdateLayout();
            WpfTestHost.DoEvents();
            Assert.AreEqual(Visibility.Collapsed, decrease.Visibility);
            Assert.AreEqual(Visibility.Collapsed, increase.Visibility);
        });
    }

    [TestMethod]
    public void LeftRightFocusTraversesTabsCloseButtonsAndAddButtonWithWrap()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var second = new TabViewItem { Header = "Second" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);

            using var host = new TestWindowHost(tabView, width: 520, height: 240);
            host.Window.Activate();
            var add = Part<ButtonBase>(tabView, "PART_AddButton");
            Assert.IsTrue(first.Focus());

            Assert.IsTrue(tabView.MoveFocus(true));
            Assert.AreSame(first.CloseButton, Keyboard.FocusedElement);
            Assert.IsFalse(first.CloseButton.IsTabStop, "Arrow focus must not add the close button to ordinary Tab traversal.");

            Assert.IsTrue(tabView.MoveFocus(true));
            Assert.AreSame(second, Keyboard.FocusedElement);
            Assert.IsTrue(tabView.MoveFocus(true));
            Assert.AreSame(second.CloseButton, Keyboard.FocusedElement);
            Assert.IsTrue(tabView.MoveFocus(true));
            Assert.AreSame(add, Keyboard.FocusedElement);
            Assert.IsTrue(tabView.MoveFocus(true));
            Assert.AreSame(first, Keyboard.FocusedElement);

            Assert.IsTrue(tabView.MoveFocus(false));
            Assert.AreSame(add, Keyboard.FocusedElement);
        });
    }

    [TestMethod]
    public void ReorderMovesMutableCollectionsAndPreservesSelection()
    {
        WpfTestHost.Run(() =>
        {
            var first = new TabViewItem { Header = "First" };
            var second = new TabViewItem { Header = "Second" };
            var third = new TabViewItem { Header = "Third" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(first);
            tabView.TabItems.Add(second);
            tabView.TabItems.Add(third);
            tabView.SelectedItem = second;

            Assert.IsTrue(tabView.ReorderTab(1, 2));
            CollectionAssert.AreEqual(new object[] { first, third, second }, tabView.TabItems);
            Assert.AreSame(second, tabView.SelectedItem);
            Assert.AreEqual(2, tabView.SelectedIndex);

            var source = new ObservableCollection<string> { "A", "B", "C" };
            tabView.TabItemsSource = source;
            tabView.SelectedItem = "A";
            Assert.IsTrue(tabView.ReorderTab(0, 2));
            CollectionAssert.AreEqual(new[] { "B", "C", "A" }, source);
            Assert.AreEqual("A", tabView.SelectedItem);
            Assert.AreEqual(2, tabView.SelectedIndex);

            tabView.CanReorderTabs = false;
            Assert.IsFalse(tabView.ReorderTab(2, 0));
        });
    }

    [TestMethod]
    public void DragStartingCarriesWritableWpfDataAndCanCancel()
    {
        WpfTestHost.Run(() =>
        {
            var item = new TabViewItem { Header = "Drag" };
            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(item);
            var data = new DataObject();
            TabViewTabDragStartingEventArgs? observed = null;
            tabView.TabDragStarting += (_, args) =>
            {
                observed = args;
                args.Data.SetData("application/x-modernwpf-test", "payload");
                args.Cancel = true;
            };

            var args = tabView.RaiseTabDragStarting(data, item, item);

            Assert.AreSame(args, observed);
            Assert.IsTrue(args.Cancel);
            Assert.AreSame(data, args.Data);
            Assert.AreSame(item, args.Item);
            Assert.AreSame(item, args.Tab);
            Assert.AreEqual("payload", data.GetData("application/x-modernwpf-test"));
        });
    }

    [TestMethod]
    public void DroppedOutsideUsesApplicationOwnedWpfWindowTearOutSequence()
    {
        WpfTestHost.Run(() =>
        {
            var item = new TabViewItem { Header = "Document", Content = "Body" };
            var source = new ModernWpf.Controls.TabView { CanTearOutTabs = true };
            var destination = new ModernWpf.Controls.TabView { CanTearOutTabs = true };
            source.TabItems.Add(item);
            var order = new System.Collections.Generic.List<string>();
            Window? tearOutWindow = null;

            source.TabDroppedOutside += (_, args) =>
            {
                order.Add("outside");
                Assert.AreSame(item, args.Item);
                Assert.AreSame(item, args.Tab);
            };
            source.TabTearOutWindowRequested += (_, args) =>
            {
                order.Add("window");
                tearOutWindow = new Window
                {
                    Width = 320,
                    Height = 180,
                    ShowInTaskbar = false,
                    Content = destination
                };
                args.NewWindow = tearOutWindow;
            };
            source.TabTearOutRequested += (_, args) =>
            {
                order.Add("move");
                Assert.AreSame(tearOutWindow, args.NewWindow);
                source.TabItems.Remove(item);
                destination.TabItems.Add(item);
            };

            try
            {
                source.CompleteDroppedOutside(item, item, new Point(-32000, -32000));

                CollectionAssert.AreEqual(new[] { "outside", "window", "move" }, order);
                Assert.AreEqual(0, source.TabItems.Count);
                Assert.AreEqual(1, destination.TabItems.Count);
                Assert.AreSame(item, destination.TabItems[0]);
                Assert.IsNotNull(tearOutWindow);
                Assert.IsTrue(tearOutWindow.IsVisible);
            }
            finally
            {
                tearOutWindow?.Close();
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void ExternalTornOutDropRequiresApplicationOptInBeforeMove()
    {
        WpfTestHost.Run(() =>
        {
            var item = new TabViewItem { Header = "Document" };
            var source = new ModernWpf.Controls.TabView();
            var destination = new ModernWpf.Controls.TabView { CanTearOutTabs = true };
            source.TabItems.Add(item);
            var dropped = 0;

            Assert.IsFalse(destination.CompleteExternalDrop(new object[] { item }, new UIElement[] { item }, 0));

            destination.ExternalTornOutTabsDropping += (_, args) =>
            {
                Assert.AreEqual(0, args.DropIndex);
                args.AllowDrop = true;
            };
            destination.ExternalTornOutTabsDropped += (_, args) =>
            {
                dropped++;
                source.TabItems.Remove(args.Items[0]);
                destination.TabItems.Insert(args.DropIndex, args.Items[0]);
            };

            Assert.IsTrue(destination.CompleteExternalDrop(new object[] { item }, new UIElement[] { item }, 0));
            Assert.AreEqual(1, dropped);
            Assert.AreEqual(0, source.TabItems.Count);
            Assert.AreSame(item, destination.TabItems[0]);
        });
    }

    private static DataTemplate CreateTabTemplate()
    {
        const string xaml = """
            <DataTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:controls="clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls">
                <controls:TabViewItem Header="{Binding Header}" Content="{Binding Content}" />
            </DataTemplate>
            """;
        return (DataTemplate)XamlReader.Parse(xaml);
    }

    private static T Part<T>(ModernWpf.Controls.TabView tabView, string name)
        where T : DependencyObject
    {
        var part = tabView.Template.FindName(name, tabView) as T;
        Assert.IsNotNull(part, $"Expected TabView template part '{name}'.");
        return part;
    }

    private static void AssertBrushMatches(FrameworkElement scope, string resourceKey, Brush actual)
    {
        Assert.IsInstanceOfType<SolidColorBrush>(scope.TryFindResource(resourceKey));
        Assert.IsInstanceOfType<SolidColorBrush>(actual);
        Assert.AreEqual(
            ((SolidColorBrush)scope.TryFindResource(resourceKey)).Color,
            ((SolidColorBrush)actual).Color,
            $"Expected the live '{resourceKey}' color.");
    }

    private static System.Collections.Generic.IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed record TabData(string Header, string Content);
}
