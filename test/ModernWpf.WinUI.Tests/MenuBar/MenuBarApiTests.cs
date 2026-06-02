using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Automation.Peers;
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

            Assert.IsNotInstanceOfType(menuBar, typeof(System.Windows.Controls.Menu));
            Assert.IsNotInstanceOfType(menuBarItem, typeof(WpfMenuItem));
            Assert.AreEqual(0, menuBar.Items.Count);
            Assert.AreEqual(0, menuBarItem.Items.Count);
            Assert.AreEqual(string.Empty, menuBarItem.Title);
            Assert.AreEqual(new CornerRadius(), menuBarItem.CornerRadius);

            menuBarItem.Title = "File";
            menuBarItem.CornerRadius = new CornerRadius(6);

            Assert.AreEqual("File", menuBarItem.Title);
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
            Assert.AreEqual(1, AutomationProperties.GetPositionInSet(fileItem));
            Assert.AreEqual(2, AutomationProperties.GetSizeOfSet(fileItem));
            Assert.AreEqual(2, AutomationProperties.GetPositionInSet(formatItem));
            Assert.AreEqual(2, AutomationProperties.GetSizeOfSet(formatItem));

            var newItem = new MuxMenuBarItem { Title = "New Menu Bar Item" };
            menuBar.Items.Add(newItem);

            Assert.AreEqual(3, menuBar.Items.Count);
            Assert.AreSame(newItem, menuBar.Items[2]);
            Assert.AreEqual(3, AutomationProperties.GetSizeOfSet(newItem));

            menuBar.Items.Remove(newItem);
            Assert.AreEqual(2, menuBar.Items.Count);
            Assert.IsFalse(menuBar.Items.Contains(newItem));

            menuBar.Items.RemoveAt(1);
            Assert.AreEqual(1, menuBar.Items.Count);
            Assert.AreSame(fileItem, menuBar.Items[0]);
        });
    }

    [TestMethod]
    public void MenuBarTemplateOwnsItemsControl()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuBar = new MuxMenuBar();
            var fileItem = new MuxMenuBarItem { Title = "File" };
            menuBar.Items.Add(fileItem);

            using var host = new TestWindowHost(menuBar, width: 320, height: 120);

            var layoutRoot = GetTemplateChild<Grid>(menuBar, "LayoutRoot");
            var contentRoot = GetTemplateChild<ItemsControl>(menuBar, "ContentRoot");

            Assert.IsNotNull(layoutRoot);
            Assert.IsNotNull(contentRoot);
            Assert.AreSame(menuBar.Items, contentRoot.ItemsSource);

            var button = GetTemplateChild<Button>(fileItem, "ContentButton");
            Assert.IsNotNull(button);
            Assert.AreEqual("File", button.Content);
            Assert.IsTrue(button.ActualWidth > 35, "MenuBarItem button should include the styled horizontal padding in its hit target.");
            Assert.IsTrue(button.ActualHeight > 20, "MenuBarItem button should include the styled vertical padding in its hit target.");
            Assert.AreSame(layoutRoot, fileItem.PassThroughElement);
        });
    }

    [TestMethod]
    public void RenderedContentButtonClickOpensMenuFlyout()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuBar = new MuxMenuBar();
            var fileItem = new InspectableMenuBarItem { Title = "File" };
            fileItem.Items.Add(new WpfMenuItem { Header = "Open" });
            menuBar.Items.Add(fileItem);

            using var host = new TestWindowHost(menuBar, width: 320, height: 120);

            var button = GetTemplateChild<Button>(fileItem, "ContentButton");
            Assert.IsNotNull(button);

            fileItem.InvokePreviewMouseLeftButtonDown(button);
            WpfTestHost.DoEvents();

            Assert.IsFalse(fileItem.IsFlyoutOpen, "MenuBarItem should wait for the rendered ContentButton click so physical clicks do not close the flyout on mouse-up.");

            fileItem.InvokePreviewMouseLeftButtonUp(button);
            WpfTestHost.DoEvents();

            Assert.IsTrue(fileItem.IsFlyoutOpen);
            Assert.IsTrue(fileItem.Flyout.IsOpen);
            Assert.IsTrue(menuBar.IsFlyoutOpen);

            fileItem.CloseMenuFlyout();
            WpfTestHost.DoEvents();

            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
            WpfTestHost.DoEvents();

            Assert.IsTrue(fileItem.IsFlyoutOpen);
            Assert.IsTrue(fileItem.Flyout.IsOpen);
        });
    }

    [TestMethod]
    public void AddRemoveFlyoutItemTest()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var fileItem = new MuxMenuBarItem { Title = "File" };
            var newItem = new WpfMenuItem { Header = "New" };
            var openItem = new WpfMenuItem { Header = "Open" };

            fileItem.Items.Add(newItem);
            fileItem.Items.Add(openItem);

            using var host = new TestWindowHost(fileItem, width: 320, height: 120);

            Assert.AreEqual(2, fileItem.Items.Count);
            Assert.AreEqual(2, fileItem.Flyout.Items.Count);
            Assert.AreSame(newItem, fileItem.Flyout.Items[0]);

            var addedItem = new WpfMenuItem { Header = "New Flyout Item" };
            fileItem.Items.Add(addedItem);

            Assert.AreEqual(3, fileItem.Items.Count);
            Assert.AreEqual(3, fileItem.Flyout.Items.Count);
            Assert.AreSame(addedItem, fileItem.Flyout.Items[2]);

            fileItem.Items.Remove(addedItem);
            Assert.AreEqual(2, fileItem.Items.Count);
            Assert.AreEqual(2, fileItem.Flyout.Items.Count);

            fileItem.Items.Clear();
            Assert.AreEqual(0, fileItem.Items.Count);
            Assert.AreEqual(0, fileItem.Flyout.Items.Count);
        });
    }

    [TestMethod]
    public void EmptyMenuBarItemNoPopupTest()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuBarItem = new MuxMenuBarItem { Title = "Empty" };

            using var host = new TestWindowHost(menuBarItem, width: 320, height: 120);

            menuBarItem.ShowMenuFlyout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(menuBarItem.IsFlyoutOpen);
            Assert.IsFalse(menuBarItem.Flyout.IsOpen);
        });
    }

    [TestMethod]
    public void ShowAndCloseMenuFlyoutUpdatesSourceState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var menuBar = new MuxMenuBar();
            var fileItem = new MuxMenuBarItem { Title = "File" };
            fileItem.Items.Add(new WpfMenuItem { Header = "Open" });
            menuBar.Items.Add(fileItem);

            using var host = new TestWindowHost(menuBar, width: 320, height: 120);

            fileItem.ShowMenuFlyout();
            WpfTestHost.DoEvents();

            Assert.IsTrue(fileItem.IsFlyoutOpen);
            Assert.IsTrue(menuBar.IsFlyoutOpen);
            Assert.AreEqual(ExpandCollapseState.Expanded, ((IExpandCollapseProvider)new MenuBarItemAutomationPeer(fileItem)).ExpandCollapseState);

            fileItem.CloseMenuFlyout();
            WpfTestHost.DoEvents();

            Assert.IsFalse(fileItem.IsFlyoutOpen);
            Assert.IsFalse(menuBar.IsFlyoutOpen);
            Assert.AreEqual(ExpandCollapseState.Collapsed, ((IExpandCollapseProvider)new MenuBarItemAutomationPeer(fileItem)).ExpandCollapseState);
        });
    }

    [TestMethod]
    public void AutomationPeersMatchWinUISourceShape()
    {
        WpfTestHost.Run(() =>
        {
            var menuBar = new MuxMenuBar();
            var menuBarPeer = new MenuBarAutomationPeer(menuBar);

            Assert.AreEqual(AutomationControlType.MenuBar, menuBarPeer.GetAutomationControlType());
            Assert.AreEqual("MenuBar", menuBarPeer.GetClassName());

            var item = new MuxMenuBarItem { Title = "File" };
            var itemPeer = new MenuBarItemAutomationPeer(item);

            Assert.AreEqual(AutomationControlType.MenuItem, itemPeer.GetAutomationControlType());
            Assert.AreEqual("MenuBarItem", itemPeer.GetClassName());
            Assert.AreEqual("File", itemPeer.GetName());
            Assert.IsInstanceOfType(itemPeer.GetPattern(PatternInterface.Invoke), typeof(IInvokeProvider));
            Assert.IsInstanceOfType(itemPeer.GetPattern(PatternInterface.ExpandCollapse), typeof(IExpandCollapseProvider));
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

            Assert.AreEqual(40, menuBar.ActualHeight, 0.5);
            Assert.IsNotNull(GetTemplateChild<ItemsControl>(menuBar, "ContentRoot"));
            Assert.IsNotNull(GetTemplateChild<Button>(menuBarItem, "ContentButton"));
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

            var fileItem = menuBar.Items[0];
            Assert.AreEqual("File", fileItem.Title);
            Assert.AreEqual(1, fileItem.Items.Count);
            Assert.AreEqual("Open", ((WpfMenuItem)fileItem.Items[0]).Header);
        });
    }

    [TestMethod]
    public void ThemeResourcesUseWinUI2MenuBarHighContrastTokens()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "MenuBarBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "MenuBarForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "ContextMenuBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ContextMenuBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "ContextMenuForeground", "TextFillColorPrimaryBrush");
                AssertThemeResourceReference(themeName, "FlyoutBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "FlyoutBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "MenuBorderColorDefaultBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "CheckBoxBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "CheckBoxBorderBrush", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBackgroundPressed", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBackgroundSelected", "SubtleFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBorderBrush", "ControlAltFillColorTertiaryBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBorderBrushPointerOver", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBorderBrushPressed", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "MenuBarItemBorderBrushSelected", "ControlStrokeColorDefaultBrush");
                AssertThemeResourceValue(themeName, "MenuBarItemBorderThickness", new Thickness(0));
            }

            AssertThemeResourceReference("HighContrast", "MenuBarBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ContextMenuBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "ContextMenuBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ContextMenuForeground", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "FlyoutBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "FlyoutBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuBorderColorDefaultBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "CheckBoxBackground", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "CheckBoxBorderBrush", "SystemColorWindowColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBackground", "SystemControlTransparentBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBackgroundPointerOver", "SystemControlBackgroundListLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBackgroundPressed", "SystemControlBackgroundListMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBackgroundSelected", "SystemControlBackgroundListMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBorderBrush", "SystemControlForegroundBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBorderBrushPointerOver", "SystemControlHighlightBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBorderBrushPressed", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "MenuBarItemBorderBrushSelected", "SystemControlHighlightBaseMediumLowBrush");
            AssertThemeResourceValue("HighContrast", "MenuBarItemBorderThickness", new Thickness(2));
        });
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();
        control.UpdateLayout();
        return (T)control.Template.FindName(name, control);
    }

    private sealed class InspectableMenuBarItem : MuxMenuBarItem
    {
        public void InvokePreviewMouseLeftButtonDown(UIElement source)
        {
            OnPreviewMouseLeftButtonDown(CreateMouseButtonArgs(UIElement.PreviewMouseLeftButtonDownEvent, source));
        }

        public void InvokePreviewMouseLeftButtonUp(UIElement source)
        {
            OnPreviewMouseLeftButtonUp(CreateMouseButtonArgs(UIElement.PreviewMouseLeftButtonUpEvent, source));
        }

        private static MouseButtonEventArgs CreateMouseButtonArgs(RoutedEvent routedEvent, UIElement source)
        {
            return new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = routedEvent,
                Source = source
            };
        }
    }

    private static void AssertThemeResourceReference(string themeName, object resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, object resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }
}
