using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Pages.WpfGallery;
using ModernWpf.Gallery.Pages.WpfGallery.SystemPages;
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.ViewModels;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryNavigationRuntimeTests
    {
        private static readonly Tuple<string, string>[] WpfGalleryNavigationResourceAliases =
        {
            Tuple.Create("NavigationViewItemBackground", "TreeViewItemBackground"),
            Tuple.Create("NavigationViewItemBackgroundPointerOver", "TreeViewItemBackgroundPointerOver"),
            Tuple.Create("NavigationViewItemBackgroundPressed", "TreeViewItemBackgroundPressed"),
            Tuple.Create("NavigationViewItemBackgroundDisabled", "TreeViewItemBackgroundDisabled"),
            Tuple.Create("NavigationViewItemBackgroundChecked", "TreeViewItemBackgroundSelected"),
            Tuple.Create("NavigationViewItemBackgroundCheckedPointerOver", "TreeViewItemBackgroundSelectedPointerOver"),
            Tuple.Create("NavigationViewItemBackgroundCheckedPressed", "TreeViewItemBackgroundSelectedPressed"),
            Tuple.Create("NavigationViewItemBackgroundCheckedDisabled", "TreeViewItemBackgroundSelectedDisabled"),
            Tuple.Create("NavigationViewItemBackgroundSelected", "TreeViewItemBackgroundSelected"),
            Tuple.Create("NavigationViewItemBackgroundSelectedPointerOver", "TreeViewItemBackgroundSelectedPointerOver"),
            Tuple.Create("NavigationViewItemBackgroundSelectedPressed", "TreeViewItemBackgroundSelectedPressed"),
            Tuple.Create("NavigationViewItemBackgroundSelectedDisabled", "TreeViewItemBackgroundSelectedDisabled"),
            Tuple.Create("NavigationViewItemForeground", "TreeViewItemForeground"),
            Tuple.Create("NavigationViewItemForegroundPointerOver", "TreeViewItemForegroundPointerOver"),
            Tuple.Create("NavigationViewItemForegroundPressed", "TreeViewItemForegroundPressed"),
            Tuple.Create("NavigationViewItemForegroundDisabled", "TreeViewItemForegroundDisabled"),
            Tuple.Create("NavigationViewItemForegroundChecked", "TreeViewItemForegroundSelected"),
            Tuple.Create("NavigationViewItemForegroundCheckedPointerOver", "TreeViewItemForegroundSelectedPointerOver"),
            Tuple.Create("NavigationViewItemForegroundCheckedPressed", "TreeViewItemForegroundSelectedPressed"),
            Tuple.Create("NavigationViewItemForegroundCheckedDisabled", "TreeViewItemForegroundSelectedDisabled"),
            Tuple.Create("NavigationViewItemForegroundSelected", "TreeViewItemForegroundSelected"),
            Tuple.Create("NavigationViewItemForegroundSelectedPointerOver", "TreeViewItemForegroundSelectedPointerOver"),
            Tuple.Create("NavigationViewItemForegroundSelectedPressed", "TreeViewItemForegroundSelectedPressed"),
            Tuple.Create("NavigationViewItemForegroundSelectedDisabled", "TreeViewItemForegroundSelectedDisabled"),
            Tuple.Create("NavigationViewItemBorderBrush", "TreeViewItemBorderBrush"),
            Tuple.Create("NavigationViewItemBorderBrushPointerOver", "TreeViewItemBorderBrushPointerOver"),
            Tuple.Create("NavigationViewItemBorderBrushPressed", "TreeViewItemBorderBrushPressed"),
            Tuple.Create("NavigationViewItemBorderBrushDisabled", "TreeViewItemBorderBrushDisabled"),
            Tuple.Create("NavigationViewItemBorderBrushChecked", "TreeViewItemBorderBrushSelected"),
            Tuple.Create("NavigationViewItemBorderBrushCheckedPointerOver", "TreeViewItemBorderBrushSelectedPointerOver"),
            Tuple.Create("NavigationViewItemBorderBrushCheckedPressed", "TreeViewItemBorderBrushSelectedPressed"),
            Tuple.Create("NavigationViewItemBorderBrushCheckedDisabled", "TreeViewItemBorderBrushSelectedDisabled"),
            Tuple.Create("NavigationViewItemBorderBrushSelected", "TreeViewItemBorderBrushSelected"),
            Tuple.Create("NavigationViewItemBorderBrushSelectedPointerOver", "TreeViewItemBorderBrushSelectedPointerOver"),
            Tuple.Create("NavigationViewItemBorderBrushSelectedPressed", "TreeViewItemBorderBrushSelectedPressed"),
            Tuple.Create("NavigationViewItemBorderBrushSelectedDisabled", "TreeViewItemBorderBrushSelectedDisabled"),
            Tuple.Create("NavigationViewSelectionIndicatorForeground", "TreeViewItemSelectionIndicatorForeground")
        };

        [TestMethod]
        public void ShellCanNavigateEveryCatalogRouteWithoutExceptions()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                var failures = new List<string>();
                var page = new NavigationRootPage();
                var window = new Window
                {
                    Width = 1180,
                    Height = 820,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var contentHost = GetContentHost(page);
                    foreach (var route in CatalogRoutes())
                    {
                        try
                        {
                            page.NavigateTo(route);
                            WpfTestHost.DoEvents();
                            window.UpdateLayout();
                            WpfTestHost.DoEvents();

                            if (!string.IsNullOrWhiteSpace(GalleryDiagnostics.LastException))
                            {
                                failures.Add(route + ": " + GalleryDiagnostics.LastException);
                            }

                            var expectedPageType = GetExpectedGroupPageType(route);
                            if (expectedPageType != null && !expectedPageType.IsInstanceOfType(contentHost.Content))
                            {
                                failures.Add(route + ": expected " + expectedPageType.Name + " but got " + (contentHost.Content?.GetType().Name ?? "<null>"));
                            }
                        }
                        catch (Exception ex)
                        {
                            failures.Add(route + ": " + ex.GetType().Name + ": " + ex.Message);
                        }
                    }
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                    GalleryDiagnostics.ResetForTests();
                }

                Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures.Take(20)));
            });
        }

        [TestMethod]
        public void ShellNavigationMenuUsesWinUIGalleryChromeAndWpfContentHost()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                page.DataContext = new { ViewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings, page.GoForward) };
                var navigation = GetNavigationView(page);
                var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();

                Assert.AreEqual(320d, navigation.OpenPaneLength);
                Assert.AreEqual(NavigationViewPaneDisplayMode.Auto, navigation.PaneDisplayMode);
                Assert.AreEqual("Navigation Pane", AutomationProperties.GetName(navigation));
                Assert.AreEqual(NavigationViewBackButtonVisible.Collapsed, navigation.IsBackButtonVisible);
                Assert.IsFalse(navigation.IsPaneToggleButtonVisible);
                Assert.IsTrue(navigation.IsSettingsVisible);
                Assert.IsFalse(navigation.IsTabStop);
                Assert.IsFalse(navigation.IsTitleBarAutoPaddingEnabled);
                Assert.AreEqual(string.Empty, navigation.PaneTitle);
                Assert.IsNull(navigation.PaneCustomContent);
                Assert.AreEqual(0, navigation.MenuItems.OfType<NavigationViewItemSeparator>().Count());
                Assert.IsNull(navigation.PaneFooter);
                Assert.AreEqual(0, navigation.FooterMenuItems.Count);
                Assert.IsFalse(page.Resources.Contains("NavigationViewItemExpandedPath"));
                Assert.IsFalse(navigation.Resources.Contains("NavigationViewItemExpandedPath"));
                Assert.IsFalse(navigation.Resources.Contains("NavigationViewItemBackground"));
                Assert.AreEqual(new Thickness(0), navigation.Resources["NavigationViewContentGridBorderThickness"]);
                Assert.AreEqual(new Thickness(0, 4, 0, 4), navigation.Resources["NavigationViewPaneContentGridMargin"]);
                Assert.AreEqual(new CornerRadius(0), navigation.Resources["NavigationViewContentGridCornerRadius"]);
                Assert.AreEqual(Colors.Transparent, ((SolidColorBrush)navigation.Resources["NavigationViewContentBackground"]).Color);

                Assert.IsNull(page.FindName("ContentFrameBorder"));
                var contentFrameBorder = GetContentFrameBorder(page);
                Assert.AreEqual(new Thickness(4, 0, 0, 0), contentFrameBorder.Margin);
                Assert.AreEqual(new Thickness(24, 16, 24, 0), contentFrameBorder.Padding);
                Assert.AreSame(page.FindResource("LayerFillColorDefaultBrush"), contentFrameBorder.Background);
                Assert.AreSame(page.FindResource("CardStrokeColorDefaultBrush"), contentFrameBorder.BorderBrush);
                Assert.AreEqual(new Thickness(1), contentFrameBorder.BorderThickness);
                Assert.AreEqual(new CornerRadius(8, 0, 0, 0), contentFrameBorder.CornerRadius);

                CollectionAssert.AreEqual(
                    new[] { "Home", "What's New", "Design Guidance", "All Controls", "Basic Input" },
                    topLevelItems.Take(5).Select(GetNavigationItemText).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Collections", "Date & Calendar", "Layout", "Navigation", "Status & Info", "Text", "ModernWpf controls" },
                    topLevelItems.Skip(5).Select(GetNavigationItemText).ToArray());
                Assert.AreEqual(12, topLevelItems.Count, "Retired navigation groups should not remain in the shell menu.");

                AssertFontIconGlyph(topLevelItems[0], "\uE80F");
                AssertFontIconGlyph(topLevelItems[1], "\uEB51");
                AssertFontIconGlyph(topLevelItems[2], "\uEB3C");
                AssertFontIconGlyph(topLevelItems[3], "\uE8A9");
                AssertFontIconGlyph(topLevelItems[4], "\uE73A");
                AssertNavigationItemsDoNotExposeLocalAutomationIds(topLevelItems);
                Assert.IsInstanceOfType(topLevelItems[0].Content, typeof(string));
                Assert.AreEqual("Home", topLevelItems[0].Content);
                Assert.IsFalse(topLevelItems[2].IsExpanded);
                Assert.IsFalse(topLevelItems[4].IsExpanded);

                var designGuidanceItems = topLevelItems[2].MenuItems.OfType<NavigationViewItem>().ToList();
                CollectionAssert.AreEqual(
                    new[] { "Colors", "Typography", "Spacing", "Geometry", "Icons" },
                    designGuidanceItems.Select(GetNavigationItemText).ToArray());
                AssertFontIconGlyph(designGuidanceItems[0], "\uE790");
                Assert.AreEqual("Colors", designGuidanceItems[0].Content);

                var basicInputItems = topLevelItems[4].MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.IsNull(basicInputItems[0].Icon);
                Assert.IsInstanceOfType(basicInputItems[0].Content, typeof(string));

                var modernWpfItem = topLevelItems[11];
                Assert.AreEqual("ModernWpf controls", GetNavigationItemText(modernWpfItem));
                AssertFontIconGlyph(modernWpfItem, "\uEA37");
                var modernWpfItems = modernWpfItem.MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.IsTrue(modernWpfItems.Count > 0);
                foreach (var modernWpfChild in modernWpfItems)
                {
                    AssertFontIconGlyph(modernWpfChild, "\uE729");
                    Assert.IsInstanceOfType(modernWpfChild.Content, typeof(string));
                }

                Assert.IsNull(page.FindName("SettingsButton"));
                navigation.ApplyTemplate();
                Assert.IsInstanceOfType(navigation.SettingsItem, typeof(NavigationViewItem));
                Assert.AreEqual("Settings", GetNavigationItemText((NavigationViewItem)navigation.SettingsItem));

                page.NavigateTo("item/Color");
                WpfTestHost.DoEvents();
                Assert.IsTrue(topLevelItems[2].IsExpanded);

                page.NavigateTo("category/BasicInput");
                Assert.IsTrue(topLevelItems[4].IsExpanded);

                page.OpenSettings();
                WpfTestHost.DoEvents();
                Assert.AreSame(navigation.SettingsItem, navigation.SelectedItem);
                Assert.IsInstanceOfType(GetContentHost(page).Content, typeof(SettingsPage));
            });
        }

        [TestMethod]
        public void ShellVisualTestStatusHooksStayOutOfNormalAutomationTree()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.ResetForTests();

                try
                {
                    var normalPage = new NavigationRootPage();
                    AssertVisualTestStatusNamesRemoved(normalPage);
                    var normalPanel = GetVisualTestStatusPanel(normalPage);
                    AssertVisualTestStatusPanelHidden(normalPanel);
                    AssertVisualTestStatusTextAutomationIds(normalPage);
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(normalPage));
                    Assert.IsNull(normalPage.FindName("Navigation"));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(GetNavigationView(normalPage)));
                    Assert.IsNull(normalPage.FindName("ContentHost"));
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(GetContentHost(normalPage)));

                    GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                    var visualTestPage = new NavigationRootPage();
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    AssertVisualTestStatusNamesRemoved(visualTestPage);
                    var visualTestPanel = GetVisualTestStatusPanel(visualTestPage);
                    Assert.AreEqual(Visibility.Visible, visualTestPanel.Visibility);
                    AssertVisualTestStatusPanelNonInteractive(visualTestPanel);
                    AssertVisualTestStatusTextAutomationIds(visualTestPage);
                    Assert.AreEqual("GalleryNavigationRoot", AutomationProperties.GetAutomationId(visualTestPage));
                    Assert.IsNull(visualTestPage.FindName("Navigation"));
                    Assert.AreEqual("GalleryNavigationView", AutomationProperties.GetAutomationId(GetNavigationView(visualTestPage)));
                    Assert.IsNull(visualTestPage.FindName("ContentHost"));
                    Assert.AreEqual("GalleryContentHost", AutomationProperties.GetAutomationId(GetContentHost(visualTestPage)));
                    Assert.AreEqual("home", GetVisualTestStatusText(visualTestPage, "GalleryVisualTestCurrentRoute").Text);
                    Assert.AreEqual("Ready:home", GetVisualTestStatusText(visualTestPage, "GalleryVisualTestReadyState").Text);
                }
                finally
                {
                    GalleryDiagnostics.ResetForTests();
                }
            });
        }

        [TestMethod]
        public void ShellNavigationGroupRowsToggleExpansionWhenInvoked()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                page.DataContext = new { ViewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings, page.GoForward) };

                RenderPage(page, () =>
                {
                    var navigation = GetNavigationView(page);
                    var designGuidanceItem = navigation.MenuItems
                        .OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "Design Guidance", StringComparison.Ordinal));
                    var basicInputItem = navigation.MenuItems
                        .OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "Basic Input", StringComparison.Ordinal));

                    Assert.IsFalse(designGuidanceItem.IsExpanded);

                    InvokeNavigationViewItem(navigation, designGuidanceItem);
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();

                    Assert.IsTrue(designGuidanceItem.IsExpanded, "User-invoked group rows should expand.");
                    Assert.IsTrue(designGuidanceItem.IsSelected, "User-invoked group rows should still navigate/select the group.");
                    Assert.IsInstanceOfType(GetContentHost(page).Content, typeof(DesignGuidancePage));

                    InvokeNavigationViewItem(navigation, designGuidanceItem);
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();

                    Assert.IsFalse(designGuidanceItem.IsExpanded, "User-invoked expanded group rows should collapse.");
                    Assert.IsTrue(designGuidanceItem.IsSelected, "Collapsing the selected group should not clear its page selection.");
                    Assert.IsInstanceOfType(GetContentHost(page).Content, typeof(DesignGuidancePage));

                    InvokeNavigationViewItem(navigation, basicInputItem);
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();

                    Assert.IsTrue(basicInputItem.IsExpanded, "User-invoked Basic Input row should expand.");
                    Assert.IsTrue(basicInputItem.IsSelected, "User-invoked Basic Input row should still navigate/select the group.");
                    Assert.IsInstanceOfType(GetContentHost(page).Content, typeof(BasicInputPage));
                });
            });
        }

        [TestMethod]
        public void ShellTitlePaneToggleChangesNavigationViewState()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                var navigation = GetNavigationView(page);
                navigation.IsPaneOpen = true;

                page.ToggleNavigationPane();
                Assert.IsFalse(navigation.IsPaneOpen);

                page.ToggleNavigationPane();
                Assert.IsTrue(navigation.IsPaneOpen);
            });
        }

        [TestMethod]
        public void ShellSearchSuggestsAndNavigatesToGalleryItems()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                var searchBox = new AutoSuggestBox
                {
                    Text = "NavigationView"
                };

                page.OnSearchTextChanged(
                    searchBox,
                    new AutoSuggestBoxTextChangedEventArgs(
                        searchBox,
                        searchBox.TextChangedEventCounter,
                        AutoSuggestionBoxTextChangeReason.UserInput));

                var suggestions = ((System.Collections.IEnumerable)searchBox.ItemsSource).Cast<object>().ToArray();
                var navigationViewSuggestion = suggestions
                    .OfType<GalleryItem>()
                    .Single(item => string.Equals(item.UniqueId, "NavigationView", StringComparison.Ordinal));

                page.OnSearchQuerySubmitted(
                    searchBox,
                    new AutoSuggestBoxQuerySubmittedEventArgs
                    {
                        ChosenSuggestion = navigationViewSuggestion,
                        QueryText = navigationViewSuggestion.Title
                    });
                WpfTestHost.DoEvents();

                var itemPage = (ItemPage)GetContentHost(page).Content;
                Assert.AreEqual("NavigationView", itemPage.UniqueId);
                Assert.IsTrue(page.CanGoBack);

                searchBox.Text = "no-gallery-result-expected";
                page.OnSearchTextChanged(
                    searchBox,
                    new AutoSuggestBoxTextChangedEventArgs(
                        searchBox,
                        searchBox.TextChangedEventCounter,
                        AutoSuggestionBoxTextChangeReason.UserInput));
                CollectionAssert.AreEqual(
                    new object[] { "No results found" },
                    ((System.Collections.IEnumerable)searchBox.ItemsSource).Cast<object>().ToArray());
            });
        }

        private static void AssertVisualTestStatusNamesRemoved(NavigationRootPage root)
        {
            Assert.IsNull(root.FindName("VisualTestStatusPanel"));
            Assert.IsNull(root.FindName("VisualTestCurrentRouteText"));
            Assert.IsNull(root.FindName("VisualTestReadyStateText"));
            Assert.IsNull(root.FindName("VisualTestLastExceptionText"));
        }

        private static StackPanel GetVisualTestStatusPanel(NavigationRootPage root)
        {
            var panel = ((Grid)root.Content).Children.OfType<StackPanel>().Single();
            Assert.AreEqual(string.Empty, panel.Name);
            return panel;
        }

        private static TextBlock GetVisualTestStatusText(NavigationRootPage root, string automationId)
        {
            return GetVisualTestStatusPanel(root)
                .Children
                .OfType<TextBlock>()
                .Single(text => string.Equals(
                    AutomationProperties.GetAutomationId(text),
                    automationId,
                    StringComparison.Ordinal));
        }

        private static Button GetVisualTestRefreshButton(NavigationRootPage root)
        {
            return GetVisualTestStatusPanel(root)
                .Children
                .OfType<Button>()
                .Single(button => string.Equals(
                    AutomationProperties.GetAutomationId(button),
                    "GalleryVisualTestRefreshArtifacts",
                    StringComparison.Ordinal));
        }

        private static void AssertVisualTestStatusPanelHidden(FrameworkElement panel)
        {
            Assert.AreEqual(Visibility.Collapsed, panel.Visibility);
            AssertVisualTestStatusPanelNonInteractive(panel);
        }

        private static void AssertVisualTestStatusPanelNonInteractive(FrameworkElement panel)
        {
            Assert.AreEqual(1d, panel.Width);
            Assert.AreEqual(1d, panel.Height);
            Assert.AreEqual(0d, panel.Opacity);
            Assert.IsFalse(panel.Focusable);
            Assert.IsFalse(panel.IsHitTestVisible);
        }

        private static void AssertVisualTestStatusTextAutomationIds(NavigationRootPage root)
        {
            Assert.AreEqual(
                "GalleryVisualTestCurrentRoute",
                AutomationProperties.GetAutomationId(GetVisualTestStatusText(root, "GalleryVisualTestCurrentRoute")));
            Assert.AreEqual(
                "GalleryVisualTestReadyState",
                AutomationProperties.GetAutomationId(GetVisualTestStatusText(root, "GalleryVisualTestReadyState")));
            Assert.AreEqual(
                "GalleryVisualTestLastException",
                AutomationProperties.GetAutomationId(GetVisualTestStatusText(root, "GalleryVisualTestLastException")));
            Assert.AreEqual(
                "GalleryVisualTestRefreshArtifacts",
                AutomationProperties.GetAutomationId(GetVisualTestRefreshButton(root)));
        }

        [TestMethod]
        public void ShellNavigationUsesDefaultWinUIHierarchyAndWpfCanvas()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                RenderPage(page, () =>
                {
                    page.NavigateTo("category/Navigation");
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();

                    var navigation = GetNavigationView(page);
                    var contentHost = GetContentHost(page);
                    var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();
                    var homeItem = topLevelItems[0];
                    var navigationItem = topLevelItems.Single(item => string.Equals(GetNavigationItemText(item), "Navigation", StringComparison.Ordinal));
                    var menuItem = navigationItem.MenuItems.OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "Menu", StringComparison.Ordinal));

                    Assert.AreEqual("Navigation", navigationItem.Content);
                    Assert.AreEqual("Menu", menuItem.Content);
                    Assert.IsTrue(navigationItem.IsExpanded);
                    Assert.IsFalse(homeItem.IsSelected, "Home should not retain the shell selection after category navigation.");
                    Assert.IsTrue(navigationItem.IsSelected, "Navigation category should own the shell selection.");
                    Assert.IsFalse(navigationItem.IsChildSelected, "Category selection should not mark a child selected.");
                    Assert.IsFalse(menuItem.IsSelected, "Menu should not be selected until item navigation.");
                    Assert.AreEqual(0, navigationItem.Depth);
                    Assert.AreEqual(1, menuItem.Depth);
                    Assert.IsInstanceOfType(contentHost.Content, typeof(NavigationPage));

                    page.NavigateTo("item/Menu");
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();

                    Assert.IsFalse(homeItem.IsSelected, "Home should not retain the shell selection after child navigation.");
                    Assert.IsFalse(navigationItem.IsSelected, "Parent row should not stay directly selected after child navigation.");
                    Assert.IsTrue(navigationItem.IsChildSelected, "Parent row should track selected child navigation.");
                    Assert.IsTrue(menuItem.IsSelected, "Menu child row should own item navigation selection.");
                    Assert.IsInstanceOfType(contentHost.Content, typeof(ItemPage));

                    var contentBorder = GetContentFrameBorder(page);
                    Assert.AreEqual(new Thickness(4, 0, 0, 0), contentBorder.Margin);
                    Assert.AreEqual(new Thickness(24, 16, 24, 0), contentBorder.Padding);
                    Assert.AreEqual(new CornerRadius(8, 0, 0, 0), contentBorder.CornerRadius);
                });
            });
        }

        [TestMethod]
        public void ShellModernWpfChildrenUseDefaultWinUIHierarchy()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                RenderPage(page, () =>
                {
                    var navigation = GetNavigationView(page);
                    var modernWpfItem = navigation.MenuItems.OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "ModernWpf controls", StringComparison.Ordinal));
                    var navigationViewItem = modernWpfItem.MenuItems.OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "NavigationView", StringComparison.Ordinal));

                    modernWpfItem.IsExpanded = true;
                    navigationViewItem.BringIntoView();
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual("ModernWpf controls", modernWpfItem.Content);
                    Assert.AreEqual("NavigationView", navigationViewItem.Content);
                    AssertFontIconGlyph(modernWpfItem, "\uEA37");
                    AssertFontIconGlyph(navigationViewItem, "\uE729");
                    Assert.AreEqual(0, modernWpfItem.Depth);
                    Assert.AreEqual(1, navigationViewItem.Depth);

                    page.NavigateTo("item/NavigationView");
                    WpfTestHost.DoEvents();
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(modernWpfItem.IsChildSelected);
                    Assert.IsTrue(navigationViewItem.IsSelected);
                    Assert.IsInstanceOfType(GetContentHost(page).Content, typeof(ItemPage));
                });
            });
        }

        [TestMethod]
        public void ShellNavigationViewUsesThemeNativeWinUIResources()
        {
            WpfTestHost.Run(() =>
            {
                var previousTheme = ThemeManager.Current.ApplicationTheme;
                var page = new NavigationRootPage();

                try
                {
                    RenderPage(page, () =>
                    {
                        var navigation = GetNavigationView(page);

                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        WpfTestHost.DoEvents();
                        Assert.IsFalse(navigation.Resources.Contains("NavigationViewItemBackground"));
                        Assert.IsFalse(navigation.Resources.Contains("NavigationViewItemForeground"));
                        Assert.IsFalse(navigation.Resources.Contains("NavigationViewSelectionIndicatorForeground"));
                        Assert.IsNotNull(navigation.TryFindResource("NavigationViewItemBackground"));
                        Assert.IsNotNull(navigation.TryFindResource("NavigationViewItemForeground"));
                        Assert.IsNotNull(navigation.TryFindResource("NavigationViewSelectionIndicatorForeground"));

                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        WpfTestHost.DoEvents();
                        Assert.IsFalse(navigation.Resources.Contains("NavigationViewItemBackground"));
                        Assert.IsNotNull(navigation.TryFindResource("NavigationViewItemBackground"));
                        Assert.IsNotNull(navigation.TryFindResource("NavigationViewItemForeground"));
                        Assert.IsNotNull(navigation.TryFindResource("NavigationViewSelectionIndicatorForeground"));
                    });
                }
                finally
                {
                    ThemeManager.Current.ApplicationTheme = previousTheme;
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void ThemeResourcesProvideNativeNavigationViewTokens()
        {
            WpfTestHost.Run(() =>
            {
                var themeResources = new ModernWpf.ThemeResources();
                foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
                {
                    var themeDictionary = GetModernWpfThemeDictionary(themeResources, themeName);
                    foreach (var resourceKey in new[]
                    {
                        "NavigationViewItemBackground",
                        "NavigationViewItemForeground",
                        "NavigationViewItemBorderBrush",
                        "NavigationViewSelectionIndicatorForeground"
                    })
                    {
                        Assert.IsTrue(
                            themeDictionary.Contains(resourceKey),
                            themeName + " is missing the native NavigationView token " + resourceKey + ".");
                    }
                }
            });
        }

        [TestMethod]
        public void MainWindowUsesWinUITitleBarWithWpfContentHost()
        {
            WpfTestHost.Run(() =>
            {
                var window = new MainWindow();
                try
                {
                    var chrome = WindowChrome.GetWindowChrome(window);
                    Assert.IsNotNull(chrome);
                    Assert.AreEqual(48d, chrome.CaptionHeight);
                    Assert.AreEqual(new CornerRadius(12), chrome.CornerRadius);
                    Assert.AreEqual(new Thickness(-1), chrome.GlassFrameThickness);
                    Assert.AreEqual(new Thickness(4), chrome.ResizeBorderThickness);
                    Assert.IsTrue(chrome.UseAeroCaptionButtons);
                    Assert.AreEqual(MainWindow.GetPrefferedNonClientFrameEdges(), chrome.NonClientFrameEdges);
                    Assert.AreSame(Application.Current.FindResource("WindowBackground"), window.Background);
                    Assert.AreSame(window, window.DataContext);
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(window));
                    Assert.AreEqual("WPF Gallery", window.ViewModel.ApplicationTitle);
                    Assert.AreEqual("WPF Gallery", window.Title);
                    Assert.AreEqual(780d, window.MinWidth);
                    Assert.AreEqual(470d, window.MinHeight);
                    Assert.AreEqual("ViewModel.ApplicationTitle",
                        BindingOperations.GetBindingExpression(window, Window.TitleProperty)?.ParentBinding.Path.Path);

                    var mainGrid = (Grid)window.FindName("MainGrid");
                    Assert.AreEqual(48d, mainGrid.RowDefinitions[0].Height.Value);
                    Assert.AreEqual(
                        MainWindow.GetMainGridMargin(WindowState.Normal, SystemParameters.HighContrast),
                        mainGrid.Margin);

                    var highContrastBorder = (Border)window.FindName("HighContrastBorder");
                    Assert.AreEqual(new Thickness(0), highContrastBorder.BorderThickness);

                    var backButton = (Button)window.FindName("BackButton");
                    Assert.AreEqual("Back", AutomationProperties.GetName(backButton));
                    Assert.AreEqual(40d, backButton.Width);
                    Assert.IsTrue(double.IsNaN(backButton.Height));
                    Assert.AreEqual(40d, backButton.MinWidth);
                    Assert.AreEqual(new Thickness(2), backButton.Margin);
                    Assert.AreEqual(VerticalAlignment.Stretch, backButton.VerticalAlignment);
                    Assert.AreEqual(HorizontalAlignment.Center, backButton.HorizontalContentAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, backButton.VerticalContentAlignment);
                    Assert.AreSame(window.ViewModel.BackCommand, backButton.Command);
                    Assert.AreEqual("ViewModel.BackCommand",
                        BindingOperations.GetBindingExpression(backButton, System.Windows.Controls.Primitives.ButtonBase.CommandProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual("ViewModel.CanNavigateback",
                        BindingOperations.GetBindingExpression(backButton, UIElement.IsEnabledProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual("ViewModel.CanNavigateback",
                        BindingOperations.GetBindingExpression(backButton, UIElement.VisibilityProperty)?.ParentBinding.Path.Path);
                    Assert.IsFalse(window.ViewModel.CanNavigateback);
                    Assert.IsFalse(backButton.IsEnabled);
                    Assert.AreEqual(Visibility.Collapsed, backButton.Visibility);

                    var titleBar = mainGrid.Children.OfType<Grid>()
                        .Single(grid => Grid.GetRow(grid) == 0);
                    Assert.AreEqual(48d, titleBar.Height);
                    Assert.AreEqual(10, titleBar.ColumnDefinitions.Count);
                    Assert.AreEqual(0d, titleBar.ColumnDefinitions[0].Width.Value);
                    Assert.AreEqual(2d, titleBar.ColumnDefinitions[3].Width.Value);
                    Assert.AreEqual(48d, titleBar.ColumnDefinitions[6].Width.Value);

                    var paneToggleButton = (Button)window.FindName("PaneToggleButton");
                    Assert.AreEqual("Navigation", AutomationProperties.GetName(paneToggleButton));
                    Assert.AreEqual(40d, paneToggleButton.Width);
                    Assert.IsTrue(double.IsNaN(paneToggleButton.Height));
                    Assert.AreEqual(new Thickness(2), paneToggleButton.Margin);
                    Assert.AreEqual(VerticalAlignment.Stretch, paneToggleButton.VerticalAlignment);
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(paneToggleButton));
                    Assert.AreEqual(
                        "\uE700",
                        ((TextBlock)paneToggleButton.Content).Text);

                    var titleHeader = titleBar.Children.OfType<StackPanel>().Single();
                    Assert.AreEqual(1d, ((TranslateTransform)titleHeader.RenderTransform).Y);
                    var appIcon = titleHeader.Children.OfType<Image>().Single();
                    Assert.AreEqual(16d, appIcon.Width);
                    Assert.AreEqual(16d, appIcon.Height);
                    Assert.AreEqual(new Thickness(0, 0, 16, 0), appIcon.Margin);

                    var titleText = titleHeader.Children.OfType<TextBlock>()
                        .Single(text => string.Equals(text.Text, "WPF Gallery", StringComparison.Ordinal));
                    Assert.AreEqual("WPF Gallery", titleText.Text);
                    Assert.AreEqual("ViewModel.ApplicationTitle",
                        BindingOperations.GetBindingExpression(titleText, TextBlock.TextProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleText));

                    var searchBox = (AutoSuggestBox)window.FindName("ControlsSearchBox");
                    Assert.AreEqual(320d, searchBox.Width);
                    Assert.AreEqual(32d, searchBox.Height);
                    Assert.AreEqual(160d, searchBox.MinWidth);
                    Assert.AreEqual(580d, searchBox.MaxWidth);
                    Assert.AreEqual(new Thickness(20, 0, 0, 0), searchBox.Margin);
                    Assert.AreEqual(1d, ((TranslateTransform)searchBox.RenderTransform).Y);
                    Assert.AreEqual("Search controls and samples...", searchBox.PlaceholderText);
                    Assert.AreEqual(string.Empty, searchBox.DisplayMemberPath);
                    Assert.AreEqual(string.Empty, searchBox.TextMemberPath);
                    Assert.AreEqual("ControlsSearchBox", AutomationProperties.GetAutomationId(searchBox));
                    Assert.AreEqual("Search controls and samples", AutomationProperties.GetName(searchBox));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(searchBox));
                    Assert.IsInstanceOfType(searchBox.QueryIcon, typeof(SymbolIcon));
                    Assert.AreEqual(Symbol.Find, ((SymbolIcon)searchBox.QueryIcon).Symbol);

                    Assert.IsNull(window.FindName("RootPage"));
                    var rootPage = GetNavigationRootPage(window);
                    rootPage.UpdateLayout();
                    WpfTestHost.DoEvents();
                    var navigation = GetNavigationView(rootPage);
                    navigation.ApplyTemplate();
                    var contentHost = GetContentHost(rootPage);
                    window.ViewModel.SettingsCommand.Execute(null);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(rootPage.CanGoBack);
                    Assert.IsTrue(window.ViewModel.CanNavigateback);
                    Assert.IsTrue(backButton.IsEnabled);
                    Assert.AreEqual(Visibility.Visible, backButton.Visibility);
                    Assert.IsInstanceOfType(contentHost.Content, typeof(SettingsPage));
                    Assert.AreSame(navigation.SettingsItem, navigation.SelectedItem);

                    var minimizeButton = (Button)window.FindName("MinimizeButton");
                    var maximizeButton = (Button)window.FindName("MaximizeButton");
                    var closeButton = (Button)window.FindName("CloseButton");
                    Assert.AreEqual(Visibility.Visible, minimizeButton.Visibility);
                    Assert.AreEqual(Visibility.Visible, maximizeButton.Visibility);
                    Assert.AreEqual(Visibility.Visible, closeButton.Visibility);
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(minimizeButton));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(maximizeButton));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(closeButton));
                    Assert.AreEqual(48d, minimizeButton.Height);
                    Assert.AreEqual(48d, maximizeButton.Height);
                    Assert.AreEqual(48d, closeButton.Height);
                    Assert.AreEqual("\uE922", ((TextBlock)window.FindName("MaximizeIcon")).Text);

                    Assert.IsInstanceOfType(rootPage, typeof(NavigationRootPage));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [TestMethod]
        public void MainWindowViewModelOfficialCommandHandlersDriveShellActions()
        {
            var backCount = 0;
            var settingsCount = 0;
            var forwardCount = 0;
            var canNavigateBack = false;
            var viewModel = new MainWindowViewModel(
                () => backCount++,
                () => settingsCount++,
                () => forwardCount++,
                () => canNavigateBack);

            viewModel.Back();
            viewModel.Settings();
            viewModel.Forward();
            viewModel.BackCommand.Execute(null);
            viewModel.SettingsCommand.Execute(null);
            viewModel.ForwardCommand.Execute(null);

            Assert.AreEqual(2, backCount);
            Assert.AreEqual(2, settingsCount);
            Assert.AreEqual(2, forwardCount);
            Assert.IsFalse(viewModel.CanNavigateback);
            canNavigateBack = true;
            viewModel.UpdateCanNavigateBack();
            Assert.IsTrue(viewModel.CanNavigateback);
        }

        [TestMethod]
        public void MainWindowViewModelForwardCommandRestoresShellForwardNavigation()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                var viewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings, page.GoForward);
                page.DataContext = new { ViewModel = viewModel };
                var window = new Window
                {
                    Width = 1180,
                    Height = 820,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var contentHost = GetContentHost(page);
                    var dashboard = (DashboardPage)contentHost.Content;
                    var targetItem = dashboard.RecentlyAddedOrUpdatedSamplesInfo.First();

                    dashboard.ViewModel.Navigate(targetItem);
                    WpfTestHost.DoEvents();
                    var itemPage = (ItemPage)contentHost.Content;
                    Assert.AreEqual(targetItem.UniqueId, itemPage.UniqueId);

                    viewModel.BackCommand.Execute(null);
                    WpfTestHost.DoEvents();
                    Assert.IsInstanceOfType(contentHost.Content, typeof(DashboardPage));

                    viewModel.ForwardCommand.Execute(null);
                    WpfTestHost.DoEvents();
                    itemPage = (ItemPage)contentHost.Content;
                    Assert.AreEqual(targetItem.UniqueId, itemPage.UniqueId);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        [TestMethod]
        public void MainWindowDiagnosticAutomationIdIsVisualTestOnly()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                var window = new MainWindow();
                try
                {
                    Assert.AreEqual("ModernWpfGalleryMainWindow", AutomationProperties.GetAutomationId(window));
                }
                finally
                {
                    window.Close();
                    GalleryDiagnostics.ResetForTests();
                }
            });
        }

        [TestMethod]
        public void MainWindowChromePolicyMatchesWpfGalleryHighContrastPath()
        {
            WpfTestHost.Run(() =>
            {
                var chrome = MainWindow.CreateWpfGalleryWindowChrome(ResizeMode.NoResize);
                Assert.AreEqual(48d, chrome.CaptionHeight);
                Assert.AreEqual(new CornerRadius(12), chrome.CornerRadius);
                Assert.AreEqual(new Thickness(-1), chrome.GlassFrameThickness);
                Assert.AreEqual(new Thickness(0), chrome.ResizeBorderThickness);
                Assert.IsTrue(chrome.UseAeroCaptionButtons);
                Assert.AreEqual(MainWindow.GetPrefferedNonClientFrameEdges(), chrome.NonClientFrameEdges);

                Assert.AreEqual(
                    new Thickness(8, 0, 8, 8),
                    MainWindow.GetMainGridMargin(
                        WindowState.Normal,
                        isHighContrast: false,
                        isWindows11OrGreater: true));
                Assert.AreEqual(
                    new Thickness(0),
                    MainWindow.GetMainGridMargin(
                        WindowState.Normal,
                        isHighContrast: false,
                        isWindows11OrGreater: false));
                Assert.AreEqual(
                    new Thickness(0),
                    MainWindow.GetMainGridMargin(
                        WindowState.Normal,
                        isHighContrast: true,
                        isWindows11OrGreater: true));
                Assert.AreEqual(new Thickness(8), MainWindow.GetMainGridMargin(WindowState.Maximized, false));
                Assert.AreEqual(new Thickness(0, 8, 0, 0), MainWindow.GetMainGridMargin(WindowState.Maximized, true));
                Assert.AreEqual(new Thickness(0), MainWindow.GetHighContrastBorderThickness(false));
                Assert.AreEqual(new Thickness(8, 1, 8, 8), MainWindow.GetHighContrastBorderThickness(true));

                Assert.AreEqual(
                    NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left,
                    MainWindow.GetPrefferedNonClientFrameEdges(isHighContrast: false, isWindows11OrGreater: true));
                Assert.AreEqual(
                    NonClientFrameEdges.None,
                    MainWindow.GetPrefferedNonClientFrameEdges(isHighContrast: true, isWindows11OrGreater: true));
                Assert.AreEqual(
                    NonClientFrameEdges.None,
                    MainWindow.GetPrefferedNonClientFrameEdges(isHighContrast: false, isWindows11OrGreater: false));
            });
        }

        [TestMethod]
        public void WinUITitleBarButtonsKeepAccessibleHighContrastHoverStyles()
        {
            WpfTestHost.Run(() =>
            {
                var window = new MainWindow();
                try
                {
                    var titleBarButtonStyle = (Style)window.Resources["BorderlessButtonStyle"];
                    AssertWpfGalleryHighContrastHoverTrigger(titleBarButtonStyle, "title bar button");

                    var defaultButtonStyle = (Style)window.Resources["TitleBarDefaultButtonStyle"];
                    Assert.AreSame(titleBarButtonStyle, defaultButtonStyle.BasedOn);
                    Assert.AreEqual(
                        48d,
                        defaultButtonStyle.Setters.OfType<Setter>().Single(setter => setter.Property == FrameworkElement.HeightProperty).Value);
                    Assert.AreEqual(
                        48d,
                        defaultButtonStyle.Setters.OfType<Setter>().Single(setter => setter.Property == FrameworkElement.MinHeightProperty).Value);

                    var closeButtonStyle = (Style)window.Resources["TitleBarDefaultCloseButtonStyle"];
                    AssertWpfGalleryHighContrastHoverTrigger(closeButtonStyle, "title bar close button");

                    var navigationButtonStyle = (Style)window.Resources["WinUITitleBarNavigationButtonStyle"];
                    Assert.AreSame(titleBarButtonStyle, navigationButtonStyle.BasedOn);
                    Assert.AreEqual(
                        40d,
                        navigationButtonStyle.Setters.OfType<Setter>().Single(setter => setter.Property == FrameworkElement.WidthProperty).Value);
                    Assert.AreEqual(
                        40d,
                        navigationButtonStyle.Setters.OfType<Setter>().Single(setter => setter.Property == FrameworkElement.MinWidthProperty).Value);
                    Assert.AreEqual(
                        new Thickness(2),
                        navigationButtonStyle.Setters.OfType<Setter>().Single(setter => setter.Property == FrameworkElement.MarginProperty).Value);
                    Assert.AreEqual(
                        VerticalAlignment.Stretch,
                        navigationButtonStyle.Setters.OfType<Setter>().Single(setter => setter.Property == FrameworkElement.VerticalAlignmentProperty).Value);
                    Assert.AreEqual(
                        1d,
                        ((TranslateTransform)navigationButtonStyle.Setters
                            .OfType<Setter>()
                            .Single(setter => setter.Property == UIElement.RenderTransformProperty)
                            .Value).Y);
                    AssertWpfGalleryHighContrastHoverTrigger(navigationButtonStyle, "WinUI title bar navigation button");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [TestMethod]
        public void DashboardPageOverviewUsesWpfReferenceGroupFilter()
        {
            WpfTestHost.Run(() =>
            {
                var page = new DashboardPage();
                var expected = GalleryCatalog.OverviewGroups.Select(group => group.UniqueId).ToArray();
                var actual = ((IEnumerable<GalleryGroup>)page.NavigationCards).Select(group => group.UniqueId).ToArray();

                CollectionAssert.AreEqual(expected, actual);
                Assert.IsFalse(actual.Contains("DesignGuidance"));
                Assert.IsFalse(actual.Contains("Media"));
                Assert.IsFalse(actual.Contains("Samples"));
                Assert.IsFalse(actual.Contains("ModernWpfControls"));
                CollectionAssert.AreEqual(
                    GalleryCatalog.NewOrUpdatedItems.Select(item => item.UniqueId).ToArray(),
                    ((IEnumerable<GalleryItem>)page.RecentlyAddedOrUpdatedSamplesInfo).Select(item => item.UniqueId).ToArray());
            });
        }

        [TestMethod]
        public void TopLevelPagesUseGalleryViewModels()
        {
            WpfTestHost.Run(() =>
            {
                object requestedPayload = null;
                var dashboardViewModel = new DashboardPageViewModel(payload => requestedPayload = payload);
                dashboardViewModel.Navigate(typeof(DashboardPage));
                Assert.AreSame(typeof(DashboardPage), requestedPayload);
                requestedPayload = null;
                dashboardViewModel.Navigate(GalleryCatalog.OverviewGroups.First());
                Assert.AreSame(GalleryCatalog.OverviewGroups.First(), requestedPayload);

                var homePage = new DashboardPage();
                Assert.IsInstanceOfType(homePage, typeof(System.Windows.Controls.Page));
                Assert.IsInstanceOfType(homePage.ViewModel, typeof(DashboardPageViewModel));
                AssertNavigationCardIds(GalleryCatalog.OverviewGroups, homePage.ViewModel.NavigationCards, "Home");
                AssertNavigationCardIds(GalleryCatalog.NewOrUpdatedItems, homePage.ViewModel.RecentlyAddedOrUpdatedSamplesInfo, "Home recently added");

                GalleryGroup requestedGroup = null;
                GalleryItem requestedItem = null;
                homePage.GroupRequested = group => requestedGroup = group;
                homePage.ItemRequested = item => requestedItem = item;
                homePage.ViewModel.NavigateCommand.Execute(GalleryCatalog.OverviewGroups.First());
                Assert.AreSame(GalleryCatalog.OverviewGroups.First(), requestedGroup);
                requestedGroup = null;
                homePage.ViewModel.Navigate(GalleryCatalog.OverviewGroups.First());
                Assert.AreSame(GalleryCatalog.OverviewGroups.First(), requestedGroup);
                homePage.ViewModel.NavigateCommand.Execute(GalleryCatalog.NewOrUpdatedItems.First());
                Assert.AreSame(GalleryCatalog.NewOrUpdatedItems.First(), requestedItem);
                requestedItem = null;
                homePage.ViewModel.Navigate(GalleryCatalog.NewOrUpdatedItems.First());
                Assert.AreSame(GalleryCatalog.NewOrUpdatedItems.First(), requestedItem);

                var whatsNewPage = new WhatsNewPage();
                Assert.IsInstanceOfType(whatsNewPage, typeof(System.Windows.Controls.Page));
                Assert.IsInstanceOfType(whatsNewPage.ViewModel, typeof(WhatsNewPageViewModel));
                Assert.AreEqual("What's new in ModernWpf", whatsNewPage.ViewModel.PageTitle);
                Assert.AreEqual(
                    "See the current ModernWpf direction, supported targets, and gallery improvements.",
                    whatsNewPage.ViewModel.PageDescription);
                AssertNavigationCardIds(
                    GalleryCatalog.NewOrUpdatedItems,
                    whatsNewPage.ViewModel.NewOrUpdatedItems,
                    "What's New");

                string requestedItemId = null;
                whatsNewPage.ItemRequested = uniqueId => requestedItemId = uniqueId;
                var whatsNewItem = GalleryCatalog.NewOrUpdatedItems.First();
                whatsNewPage.ViewModel.NavigateCommand.Execute(whatsNewItem);
                Assert.AreEqual(whatsNewItem.UniqueId, requestedItemId);
                requestedItemId = null;
                whatsNewPage.ViewModel.Navigate(whatsNewItem);
                Assert.AreEqual(whatsNewItem.UniqueId, requestedItemId);

                var allControlsPage = new AllSamplesPage();
                Assert.IsInstanceOfType(allControlsPage, typeof(System.Windows.Controls.Page));
                Assert.IsInstanceOfType(allControlsPage.ViewModel, typeof(AllSamplesPageViewModel));
                Assert.AreEqual("All Controls", allControlsPage.ViewModel.PageTitle);
                Assert.AreEqual(string.Empty, allControlsPage.ViewModel.PageDescription);
                AssertNavigationCardIds(GalleryCatalog.AllControlsItems, allControlsPage.ViewModel.NavigationCards, "All Controls");
                requestedItem = null;
                allControlsPage.ItemRequested = item => requestedItem = item;
                allControlsPage.ViewModel.Navigate(GalleryCatalog.AllControlsItems.First());
                Assert.AreSame(GalleryCatalog.AllControlsItems.First(), requestedItem);
            });
        }

        [TestMethod]
        public void SectionPagesUseOfficialWpfGalleryViewModels()
        {
            WpfTestHost.Run(() =>
            {
                var expectedViewModels = new[]
                {
                    new { UniqueId = "DesignGuidance", PageType = typeof(DesignGuidancePage), ViewModelType = typeof(DesignGuidancePageViewModel), PageTitle = "DesignGuidancePage" },
                    new { UniqueId = "BasicInput", PageType = typeof(BasicInputPage), ViewModelType = typeof(BasicInputPageViewModel), PageTitle = "BasicInputPage" },
                    new { UniqueId = "Collections", PageType = typeof(CollectionsPage), ViewModelType = typeof(CollectionsPageViewModel), PageTitle = "CollectionsPage" },
                    new { UniqueId = "DateAndCalendar", PageType = typeof(DateAndTimePage), ViewModelType = typeof(DateAndTimePageViewModel), PageTitle = "DateAndTimePage" },
                    new { UniqueId = "Layout", PageType = typeof(LayoutPage), ViewModelType = typeof(LayoutPageViewModel), PageTitle = "LayoutPage" },
                    new { UniqueId = "Navigation", PageType = typeof(NavigationPage), ViewModelType = typeof(NavigationPageViewModel), PageTitle = "NavigationPage" },
                    new { UniqueId = "StatusAndInfo", PageType = typeof(StatusAndInfoPage), ViewModelType = typeof(StatusAndInfoPageViewModel), PageTitle = "StatusAndInfoPage" },
                    new { UniqueId = "Text", PageType = typeof(TextPage), ViewModelType = typeof(TextPageViewModel), PageTitle = "TextPage" }
                };

                foreach (var expected in expectedViewModels)
                {
                    var group = GalleryCatalog.FindGroup(expected.UniqueId);
                    Assert.IsNotNull(group, expected.UniqueId);

                    var page = (SectionPage)Activator.CreateInstance(expected.PageType);
                    Assert.IsInstanceOfType(page, typeof(System.Windows.Controls.Page), expected.UniqueId);
                    Assert.IsInstanceOfType(page, expected.PageType, expected.UniqueId);
                    Assert.AreEqual(expected.PageTitle, page.Title, expected.UniqueId);
                    Assert.IsInstanceOfType(page.ViewModel, expected.ViewModelType, expected.UniqueId);
                    Assert.AreEqual(group.Title, page.ViewModel.PageTitle, expected.UniqueId);
                    Assert.AreEqual(group.PageDescription, page.ViewModel.PageDescription, expected.UniqueId);
                    AssertNavigationCardIds(group.Items, page.ViewModel.NavigationCards, expected.UniqueId);

                    object requestedPayload = null;
                    var navigationViewModel = new WpfGalleryNavigationPageViewModel(
                        group.Title,
                        group.PageDescription,
                        group.Items,
                        payload => requestedPayload = payload);
                    navigationViewModel.Navigate(typeof(SectionPage));
                    Assert.AreSame(typeof(SectionPage), requestedPayload, expected.UniqueId);
                    requestedPayload = null;
                    navigationViewModel.Navigate(group.Items.First());
                    Assert.AreSame(group.Items.First(), requestedPayload, expected.UniqueId);

                    GalleryItem requestedItem = null;
                    page.ItemRequested = item => requestedItem = item;
                    page.ViewModel.NavigateCommand.Execute(group.Items.First());
                    Assert.AreSame(group.Items.First(), requestedItem, expected.UniqueId);
                    requestedItem = null;
                    page.ViewModel.Navigate(group.Items.First());
                    Assert.AreSame(group.Items.First(), requestedItem, expected.UniqueId);
                }

                var modernWpfGroup = GalleryCatalog.FindGroup("ModernWpfControls");
                Assert.IsNotNull(modernWpfGroup);

                var modernWpfPage = new SectionPage(modernWpfGroup);

                Assert.AreEqual(typeof(WpfGalleryNavigationPageViewModel), modernWpfPage.ViewModel.GetType());
                AssertNavigationCardIds(modernWpfGroup.Items, modernWpfPage.ViewModel.NavigationCards, modernWpfGroup.UniqueId);
            });
        }

        [TestMethod]
        public void SectionPagesAcceptOfficialWpfGalleryDisplayGroupIds()
        {
            WpfTestHost.Run(() =>
            {
                var expectedGroups = new[]
                {
                    new { LookupId = "Design Guidance", CanonicalId = "DesignGuidance", PageType = typeof(DesignGuidancePage), ViewModelType = typeof(DesignGuidancePageViewModel), PageTitle = "DesignGuidancePage" },
                    new { LookupId = "Basic Input", CanonicalId = "BasicInput", PageType = typeof(BasicInputPage), ViewModelType = typeof(BasicInputPageViewModel), PageTitle = "BasicInputPage" },
                    new { LookupId = "Date & Calendar", CanonicalId = "DateAndCalendar", PageType = typeof(DateAndTimePage), ViewModelType = typeof(DateAndTimePageViewModel), PageTitle = "DateAndTimePage" },
                    new { LookupId = "Status & Info", CanonicalId = "StatusAndInfo", PageType = typeof(StatusAndInfoPage), ViewModelType = typeof(StatusAndInfoPageViewModel), PageTitle = "StatusAndInfoPage" }
                };

                foreach (var expected in expectedGroups)
                {
                    var canonicalGroup = GalleryCatalog.FindGroup(expected.LookupId);
                    Assert.IsNotNull(canonicalGroup, expected.LookupId);
                    Assert.AreEqual(expected.CanonicalId, canonicalGroup.UniqueId, expected.LookupId);

                    var displayGroup = new GalleryGroup(
                        expected.LookupId,
                        canonicalGroup.Title,
                        canonicalGroup.Subtitle,
                        canonicalGroup.ImagePath,
                        canonicalGroup.IsSpecialSection,
                        canonicalGroup.Items,
                        canonicalGroup.PageDescription);

                    var factoryPage = WpfGallerySectionPageFactory.Create(displayGroup);
                    Assert.IsInstanceOfType(factoryPage, expected.PageType, expected.LookupId);
                    Assert.AreEqual(expected.PageTitle, factoryPage.Title, expected.LookupId);
                    Assert.IsInstanceOfType(factoryPage.ViewModel, expected.ViewModelType, expected.LookupId);
                    Assert.AreEqual(canonicalGroup.Title, factoryPage.ViewModel.PageTitle, expected.LookupId);
                    AssertNavigationCardIds(canonicalGroup.Items, factoryPage.ViewModel.NavigationCards, expected.LookupId);

                    var genericPage = new SectionPage(displayGroup);
                    Assert.AreEqual(expected.PageTitle, genericPage.Title, expected.LookupId);
                    Assert.IsInstanceOfType(genericPage.ViewModel, expected.ViewModelType, expected.LookupId);
                    Assert.AreEqual(canonicalGroup.Title, genericPage.ViewModel.PageTitle, expected.LookupId);
                    AssertNavigationCardIds(canonicalGroup.Items, genericPage.ViewModel.NavigationCards, expected.LookupId);
                }
            });
        }

        [TestMethod]
        public void WpfGalleryPageShellCardsMatchReferenceAutomationAndLayout()
        {
            WpfTestHost.Run(() =>
            {
                var homePage = new DashboardPage();
                RenderPage(homePage, () =>
                {
                    Assert.IsNull(homePage.FindName("ContentRootGrid"));
                    var homeScrollViewer = (ScrollViewer)homePage.Content;
                    var homeContentGrid = (Grid)homeScrollViewer.Content;
                    var heroVersionText = FindVisualChildren<TextBlock>(homeContentGrid)
                        .Single(textBlock => string.Equals(textBlock.Text, ".NET 10", StringComparison.Ordinal));
                    var heroTitleText = FindVisualChildren<TextBlock>(homeContentGrid)
                        .Single(textBlock => string.Equals(textBlock.Text, "WPF Gallery", StringComparison.Ordinal));
                    var overviewHeaderText = FindVisualChildren<TextBlock>(homeContentGrid)
                        .Single(textBlock => string.Equals(textBlock.Text, "Overview", StringComparison.Ordinal));
                    var recentlyAddedHeaderText = FindVisualChildren<TextBlock>(homeContentGrid)
                        .Single(textBlock => string.Equals(textBlock.Text, "Recently added and updated", StringComparison.Ordinal));
                    var overviewItemsControl = FindVisualChildren<ItemsControl>(homeContentGrid)
                        .Single(itemsControl => string.Equals(AutomationProperties.GetName(itemsControl), "Items in group", StringComparison.Ordinal));
                    var recentlyAddedItemsControl = FindVisualChildren<ItemsControl>(homeContentGrid)
                        .Single(itemsControl => string.Equals(AutomationProperties.GetName(itemsControl), "Recently Added and Updated Samples Section", StringComparison.Ordinal));

                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(homeScrollViewer));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(heroVersionText));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(heroTitleText));
                    Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(overviewHeaderText));
                    Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(recentlyAddedHeaderText));
                    AssertNavigationItemsControl(overviewItemsControl, "Items in group");
                    AssertNavigationItemsControl(recentlyAddedItemsControl, "Recently Added and Updated Samples Section");
                    AssertBindingPath(overviewItemsControl, ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertBindingPath(recentlyAddedItemsControl, ItemsControl.ItemsSourceProperty, "ViewModel.RecentlyAddedOrUpdatedSamplesInfo");
                    Assert.AreEqual(new Thickness(0), homeContentGrid.Margin);
                    Assert.AreEqual(new Thickness(0), recentlyAddedHeaderText.Margin);

                    var firstGroup = GalleryCatalog.OverviewGroups.First();
                    AssertRenderedNavigationCard(overviewItemsControl, firstGroup.Title, firstGroup.Description, homePage.ViewModel.NavigateCommand);
                });

                var basicInputGroup = GalleryCatalog.FindGroup("BasicInput");
                var sectionPage = new BasicInputPage();
                RenderPage(sectionPage, () =>
                {
                    var sectionItemsControl = GetOfficialSectionItemsControl(sectionPage);
                    AssertReferencePageHeader(FindVisualChildren<PageHeader>(sectionPage).Single(), basicInputGroup.Title, basicInputGroup.PageDescription, true);
                    AssertNavigationItemsControl(sectionItemsControl, "Items in group");
                    AssertBindingPath(sectionItemsControl, ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertReferenceCategoryPageRoot(GetOfficialSectionRoot(sectionPage), false);
                    Assert.AreEqual(basicInputGroup.Title, sectionPage.PageTitle);
                    Assert.AreEqual(basicInputGroup.PageDescription, sectionPage.PageDescription);
                    AssertRenderedNavigationCard(sectionItemsControl, basicInputGroup.Items.First().Title, basicInputGroup.Items.First().Description, sectionPage.ViewModel.NavigateCommand);
                });

                var allControlsPage = new AllSamplesPage();
                RenderPage(allControlsPage, () =>
                {
                    var allControlsRoot = (Grid)allControlsPage.Content;
                    var allControlsItemsControl = FindVisualChildren<ItemsControl>(allControlsPage)
                        .Single(itemsControl => string.Equals(AutomationProperties.GetName(itemsControl), "Items in group", StringComparison.Ordinal));

                    Assert.AreEqual(string.Empty, allControlsRoot.Name);
                    AssertReferencePageHeader(FindVisualChildren<PageHeader>(allControlsPage).Single(), "All Controls", string.Empty, true);
                    AssertNavigationItemsControl(allControlsItemsControl, "Items in group");
                    Assert.AreEqual(1, Grid.GetRow(allControlsItemsControl));
                    AssertBindingPath(allControlsItemsControl, ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertReferenceCategoryPageRoot(allControlsRoot, true);
                    Assert.AreEqual("All Controls", allControlsPage.PageTitle);
                    Assert.AreEqual(string.Empty, allControlsPage.PageDescription);
                    AssertRenderedNavigationCard(allControlsItemsControl, GalleryCatalog.AllControlsItems.First().Title, GalleryCatalog.AllControlsItems.First().Description, allControlsPage.ViewModel.NavigateCommand);
                });

                var modernWpfGroup = GalleryCatalog.FindGroup("ModernWpfControls");
                var modernWpfSectionPage = new SectionPage(modernWpfGroup);
                RenderPage(modernWpfSectionPage, () =>
                {
                    AssertReferencePageHeader(FindVisualChildren<PageHeader>(modernWpfSectionPage).Single(), modernWpfGroup.Title, modernWpfGroup.PageDescription, true);
                    Assert.AreEqual(Visibility.Collapsed, GetOfficialSectionItemsControl(modernWpfSectionPage).Visibility);
                    Assert.IsNull(modernWpfSectionPage.FindName("ModernWpfGroupScrollViewer"));
                    Assert.IsNull(modernWpfSectionPage.FindName("ModernWpfGroupItemsControl"));
                    var scrollViewer = GetModernWpfExtensionScrollViewer(modernWpfSectionPage);
                    Assert.AreEqual(Visibility.Visible, scrollViewer.Visibility);
                    Assert.AreEqual(1, Grid.GetRow(scrollViewer));
                    Assert.AreEqual(new Thickness(0), scrollViewer.Margin);
                    Assert.AreEqual(ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);
                    var itemsControl = scrollViewer.Content as ItemsControl;
                    Assert.IsNotNull(itemsControl);
                    AssertNavigationItemsControl(itemsControl, "Items in group");
                    AssertBindingPath(itemsControl, ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertRenderedNavigationCard(itemsControl, modernWpfGroup.Items.First().Title, modernWpfGroup.Items.First().Description, modernWpfSectionPage.ViewModel.NavigateCommand);
                    Assert.IsTrue(scrollViewer.ExtentHeight > scrollViewer.ViewportHeight, "The retained ModernWpf controls section should scroll because it contains many cards.");
                    scrollViewer.ScrollToEnd();
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(scrollViewer.VerticalOffset > 0);
                });

                var navigationViewPage = new ItemPage(GalleryCatalog.FindItem("NavigationView"));
                RenderPage(navigationViewPage, () =>
                {
                    var pageHeader = FindVisualChildren<PageHeader>(navigationViewPage)
                        .Single(header => ReferenceEquals(header.DataContext, navigationViewPage));
                    Assert.AreEqual(Visibility.Visible, pageHeader.Visibility);
                    Assert.AreEqual(new Thickness(0, 0, 0, 32), pageHeader.Margin);
                    Assert.AreEqual(navigationViewPage.Title, pageHeader.Title);
                    Assert.AreEqual(navigationViewPage.Description, pageHeader.Description);
                    AssertBindingPath(pageHeader, PageHeader.TitleProperty, "Title");
                    AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "PageHeaderDescription");

                    pageHeader.ApplyTemplate();
                    var labels = FindVisualChildren<Label>(pageHeader).ToArray();
                    Assert.AreEqual(2, labels.Length);
                    AssertPageHeaderLabel(labels[0], navigationViewPage.Title + " Page", AutomationHeadingLevel.Level1, 0);
                    AssertPageHeaderLabel(labels[1], string.Empty, AutomationHeadingLevel.Level2, 1);
                });

                var itemPage = new ItemPage(GalleryCatalog.FindItem("Color"));
                RenderPage(itemPage, () =>
                {
                    var wrapperHeader = FindVisualChildren<PageHeader>(itemPage)
                        .Single(header => ReferenceEquals(header.DataContext, itemPage));
                    Assert.AreEqual(Visibility.Collapsed, wrapperHeader.Visibility);
                    Assert.AreEqual(itemPage.Title, wrapperHeader.Title);
                    Assert.IsNull(wrapperHeader.Description);
                });
            });
        }

        private static string GetNavigationItemText(NavigationViewItem item)
        {
            var automationName = AutomationProperties.GetName(item);
            if (!string.IsNullOrEmpty(automationName))
            {
                return automationName;
            }

            return item.Content as string;
        }

        private static void AssertBounds(FrameworkElement root, FrameworkElement element, double expectedLeft, double expectedWidth, string context)
        {
            var bounds = GetElementBounds(root, element);
            Assert.AreEqual(expectedLeft, bounds.Left, 1.0, context + " left");
            Assert.AreEqual(expectedWidth, bounds.Width, 1.0, context + " width");
        }

        private static void AssertTextLeft(FrameworkElement root, DependencyObject scope, string text, double expectedLeft, string context)
        {
            var textBlock = FindVisualChildren<TextBlock>(scope)
                .FirstOrDefault(block => string.Equals(block.Text, text, StringComparison.Ordinal));

            Assert.IsNotNull(textBlock, context);
            Assert.AreEqual(expectedLeft, GetElementBounds(root, textBlock).Left, 1.0, context + " left");
        }

        private static void AssertTextTop(FrameworkElement root, DependencyObject scope, string text, double expectedTop, string context)
        {
            var textBlock = FindVisualChildren<TextBlock>(scope)
                .FirstOrDefault(block => string.Equals(block.Text, text, StringComparison.Ordinal));

            Assert.IsNotNull(textBlock, context);
            Assert.AreEqual(expectedTop, GetElementBounds(root, textBlock).Top, 1.0, context + " top");
        }

        private static Rect GetElementBounds(FrameworkElement root, FrameworkElement element)
        {
            element.UpdateLayout();
            return element.TransformToAncestor(root).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        }

        private static void AssertFontIconGlyph(NavigationViewItem item, string expectedGlyph)
        {
            var icon = item.Icon as FontIcon;
            if (icon != null)
            {
                Assert.AreEqual(expectedGlyph, icon.Glyph);
                return;
            }

            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);
            Assert.AreEqual(expectedGlyph, contentGrid.Tag as string);
        }

        private static void AssertNavigationItemContentMargin(NavigationViewItem item, double expectedLeft)
        {
            AssertNavigationItemContentMargin(item, expectedLeft, 0, GetNavigationItemText(item) + " content margin");
        }

        private static void AssertNavigationItemContentMargin(NavigationViewItem item, double expectedLeft, double expectedTop, string context)
        {
            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);
            Assert.AreEqual(expectedLeft, contentGrid.Margin.Left, context + " left");
            Assert.AreEqual(expectedTop, contentGrid.Margin.Top, context + " top");
        }

        private static void AssertNavigationTitleTextLayout(NavigationViewItem item, string expectedTitle)
        {
            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);

            var titleText = contentGrid.Children.OfType<TextBlock>()
                .Single(text => string.Equals(text.Text, expectedTitle, StringComparison.Ordinal));
            Assert.AreEqual(HorizontalAlignment.Left, titleText.HorizontalAlignment);
        }

        private static void AssertSelectionIndicatorBounds(NavigationViewItem item, double expectedLeft, double expectedTop, string context)
        {
            var indicator = FindVisualChildren<FrameworkElement>(item)
                .SingleOrDefault(element => string.Equals(element.Name, "SelectionIndicator", StringComparison.Ordinal));
            Assert.IsNotNull(indicator, context);

            var bounds = indicator.TransformToAncestor(item)
                .TransformBounds(new Rect(0, 0, indicator.ActualWidth, indicator.ActualHeight));
            Assert.AreEqual(expectedLeft, bounds.Left, 1.0, context + " left");
            Assert.AreEqual(expectedTop, bounds.Top, 1.0, context + " top");
            Assert.AreEqual(3d, bounds.Width, 1.0, context + " width");
            Assert.AreEqual(16d, bounds.Height, 1.0, context + " height");
        }

        private static void AssertNavigationItemContentVerticallyCentered(
            FrameworkElement root,
            NavigationViewItem item,
            IEnumerable<string> texts,
            string context)
        {
            var layoutRoot = FindNavigationItemLayoutRoot(item, context);
            var layoutBounds = GetElementBounds(root, layoutRoot);
            var layoutCenter = layoutBounds.Top + (layoutBounds.Height / 2);

            foreach (var text in texts)
            {
                var textBlock = FindVisualChildren<TextBlock>(item)
                    .FirstOrDefault(block => string.Equals(block.Text, text, StringComparison.Ordinal));
                Assert.IsNotNull(textBlock, context + " " + text);

                var textBounds = GetElementBounds(root, textBlock);
                var textCenter = textBounds.Top + (textBounds.Height / 2);
                Assert.AreEqual(layoutCenter, textCenter, 1.0, context + " " + text + " vertical center");
            }
        }

        private static void AssertNavigationItemLayoutRootMargin(NavigationViewItem item, Thickness expectedMargin, string context)
        {
            var layoutRoot = FindNavigationItemLayoutRoot(item, context);

            Assert.AreEqual(expectedMargin, layoutRoot.Margin, context);
        }

        private static void AssertNavigationItemLayoutRootHeight(FrameworkElement root, NavigationViewItem item, double expectedHeight, string context)
        {
            var layoutRoot = FindNavigationItemLayoutRoot(item, context);
            var bounds = GetElementBounds(root, layoutRoot);

            Assert.AreEqual(expectedHeight, bounds.Height, 1.0, context + " height");
        }

        private static void AssertNavigationItemPresenterRowAutoHeight(NavigationViewItem item, string context)
        {
            var rootGrid = FindVisualChildren<Grid>(item)
                .SingleOrDefault(grid => string.Equals(grid.Name, "NVIRootGrid", StringComparison.Ordinal));

            Assert.IsNotNull(rootGrid, context);
            Assert.IsTrue(rootGrid.RowDefinitions.Count > 0, context);
            Assert.AreEqual(GridUnitType.Auto, rootGrid.RowDefinitions[0].Height.GridUnitType, context);
        }

        private static void AssertExpandedNavigationChildVisible(
            FrameworkElement root,
            NavigationViewItem parent,
            NavigationViewItem child,
            NavigationViewItem followingItem,
            string childText)
        {
            var parentLayoutRoot = FindNavigationItemLayoutRoot(parent, childText + " parent row");
            var parentRowBounds = GetElementBounds(root, parentLayoutRoot);
            var childBounds = GetElementBounds(root, child);
            var followingBounds = GetElementBounds(root, followingItem);

            Assert.IsTrue(child.IsVisible, childText + " child item should be visible.");
            Assert.IsTrue(childBounds.Height > 0, childText + " child item should have height.");
            Assert.IsTrue(childBounds.Top >= parentRowBounds.Bottom - 1, childText + " child item should be below its expanded parent row.");
            Assert.IsTrue(childBounds.Top - parentRowBounds.Bottom <= 48, childText + " child item should stay close to its expanded parent row.");
            Assert.IsTrue(childBounds.Bottom <= followingBounds.Top + 1, childText + " child item should be above the following top-level row.");

            var titleText = FindVisualChildren<TextBlock>(child)
                .SingleOrDefault(text => string.Equals(text.Text, childText, StringComparison.Ordinal));
            Assert.IsNotNull(titleText, childText + " child text");

            var textBounds = GetElementBounds(root, titleText);
            Assert.IsTrue(titleText.IsVisible, childText + " child text should be visible.");
            Assert.IsTrue(textBounds.Width > 0, childText + " child text should have width.");
            Assert.IsTrue(textBounds.Height > 0, childText + " child text should have height.");
            Assert.IsTrue(textBounds.Bottom <= followingBounds.Top + 1, childText + " child text should not be hidden behind the following row.");
        }

        private static void AssertCollapsedNavigationChildrenReleased(
            FrameworkElement root,
            NavigationViewItem parent,
            NavigationViewItem child,
            NavigationViewItem followingItem,
            string childText)
        {
            var parentLayoutRoot = FindNavigationItemLayoutRoot(parent, childText + " collapsed parent row");
            var parentRowBounds = GetElementBounds(root, parentLayoutRoot);
            var followingBounds = GetElementBounds(root, followingItem);

            Assert.IsFalse(child.IsVisible, childText + " child item should not be visible after collapse.");
            Assert.IsTrue(
                followingBounds.Top - parentRowBounds.Bottom <= 48,
                childText + " collapsed child area should not keep a large blank gap before the following row. " +
                "Gap=" + (followingBounds.Top - parentRowBounds.Bottom).ToString("F1", CultureInfo.InvariantCulture) +
                ", parentRow=" + parentRowBounds.ToString(CultureInfo.InvariantCulture) +
                ", parentActual=" + parent.ActualHeight.ToString("F1", CultureInfo.InvariantCulture) +
                ", childActual=" + child.ActualHeight.ToString("F1", CultureInfo.InvariantCulture) +
                ", following=" + followingBounds.ToString(CultureInfo.InvariantCulture));
        }

        private static Border FindNavigationItemLayoutRoot(NavigationViewItem item, string context)
        {
            var layoutRoot = FindVisualChildren<Border>(item)
                .FirstOrDefault(border => string.Equals(border.Name, "LayoutRoot", StringComparison.Ordinal));
            Assert.IsNotNull(layoutRoot, context);
            return layoutRoot;
        }

        private static void AssertWpfGalleryNavigationPaneBackground(NavigationView navigation, string resourceKey, Color expectedColor)
        {
            var background = navigation.Resources[resourceKey] as SolidColorBrush;
            Assert.IsNotNull(background);
            Assert.AreEqual(expectedColor, background.Color);
        }

        private static TextBlock GetNavigationDisclosureChevron(NavigationViewItem item)
        {
            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);

            return contentGrid.Children.OfType<TextBlock>()
                .SingleOrDefault(text => string.Equals(text.Text, "\uE76C", StringComparison.Ordinal));
        }

        private static void AssertNavigationItemsDoNotExposeLocalAutomationIds(IEnumerable<NavigationViewItem> items)
        {
            foreach (var item in items)
            {
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(item), GetNavigationItemText(item));
                AssertNavigationItemsDoNotExposeLocalAutomationIds(item.MenuItems.OfType<NavigationViewItem>());
            }
        }

        private static void AssertNavigationViewResourceAlias(
            FrameworkElement scope,
            string navigationResourceKey,
            string treeViewResourceKey)
        {
            Assert.AreSame(
                scope.TryFindResource(treeViewResourceKey),
                scope.TryFindResource(navigationResourceKey),
                navigationResourceKey);
        }

        private static void AssertWpfGalleryNavigationResourceAliases(FrameworkElement scope)
        {
            foreach (var resourcePair in WpfGalleryNavigationResourceAliases)
            {
                AssertNavigationViewResourceAlias(scope, resourcePair.Item1, resourcePair.Item2);
            }
        }

        private static ResourceDictionary GetModernWpfThemeDictionary(ModernWpf.ThemeResources themeResources, string themeName)
        {
            var method = typeof(ModernWpf.ThemeResources).GetMethod(
                "GetThemeDictionary",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, "ModernWpf.ThemeResources.GetThemeDictionary should remain available to verify shell resource tokens.");
            return (ResourceDictionary)method.Invoke(themeResources, new object[] { themeName });
        }

        private static object GetExpectedHighContrastTreeViewResourceKey(string treeViewResourceKey)
        {
            switch (treeViewResourceKey)
            {
                case "TreeViewItemBackground":
                case "TreeViewItemBackgroundPointerOver":
                case "TreeViewItemBackgroundPressed":
                case "TreeViewItemBackgroundDisabled":
                case "TreeViewItemBackgroundSelected":
                case "TreeViewItemBackgroundSelectedDisabled":
                case "TreeViewItemBorderBrush":
                case "TreeViewItemBorderBrushPressed":
                case "TreeViewItemBorderBrushDisabled":
                    return "SystemColorWindowColorBrush";

                case "TreeViewItemBackgroundSelectedPointerOver":
                case "TreeViewItemBackgroundSelectedPressed":
                case "TreeViewItemBorderBrushSelectedPressed":
                    return "SystemColorButtonFaceColorBrush";

                case "TreeViewItemForeground":
                    return "SystemColorWindowTextColorBrush";

                case "TreeViewItemForegroundPointerOver":
                case "TreeViewItemForegroundPressed":
                case "TreeViewItemForegroundSelected":
                case "TreeViewItemBorderBrushPointerOver":
                case "TreeViewItemBorderBrushSelected":
                case "TreeViewItemSelectionIndicatorForeground":
                    return "SystemColorHighlightColorBrush";

                case "TreeViewItemForegroundDisabled":
                case "TreeViewItemForegroundSelectedDisabled":
                case "TreeViewItemBorderBrushSelectedDisabled":
                    return "SystemColorGrayTextColorBrush";

                case "TreeViewItemForegroundSelectedPointerOver":
                case "TreeViewItemForegroundSelectedPressed":
                case "TreeViewItemBorderBrushSelectedPointerOver":
                    return "SystemColorButtonTextColorBrush";

                default:
                    Assert.Fail("No expected high-contrast TreeView resource mapping for " + treeViewResourceKey + ".");
                    return null;
            }
        }

        private static void AssertHighContrastTreeViewResourceReference(
            ResourceDictionary themeDictionary,
            string treeViewResourceKey,
            object expectedResourceKey,
            string navigationResourceKey)
        {
            Assert.IsTrue(
                themeDictionary.Contains(expectedResourceKey),
                "HighContrast is missing the expected WPF Fluent resource " + expectedResourceKey + " for " + treeViewResourceKey + ".");
            Assert.AreSame(
                themeDictionary[expectedResourceKey],
                themeDictionary[treeViewResourceKey],
                navigationResourceKey + " should inherit " + treeViewResourceKey + " from " + expectedResourceKey + " in HighContrast.");
        }

        private static void AssertPageHeaderLabel(Label label, string automationName, AutomationHeadingLevel headingLevel, int tabIndex)
        {
            BindingOperations.GetBindingExpression(label, AutomationProperties.NameProperty)?.UpdateTarget();
            Assert.IsTrue(label.Focusable);
            Assert.AreEqual(KeyboardNavigationMode.Continue, KeyboardNavigation.GetTabNavigation(label));
            Assert.IsTrue(KeyboardNavigation.GetIsTabStop(label));
            Assert.AreEqual(tabIndex, KeyboardNavigation.GetTabIndex(label));
            Assert.AreEqual(automationName, AutomationProperties.GetName(label));
            Assert.AreEqual(headingLevel, AutomationProperties.GetHeadingLevel(label));
        }

        private static void AssertReferencePageHeader(PageHeader pageHeader, string title, string description, bool assertOfficialBindings = false)
        {
            Assert.IsNotNull(pageHeader);
            Assert.AreEqual(new Thickness(0, 0, 0, 40), pageHeader.Margin);
            Assert.AreEqual(title, pageHeader.Title);
            Assert.AreEqual(description, pageHeader.Description);

            if (assertOfficialBindings)
            {
                AssertBindingPath(pageHeader, PageHeader.TitleProperty, "ViewModel.PageTitle");
                AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "ViewModel.PageDescription");
            }

            pageHeader.ApplyTemplate();

            var labels = FindVisualChildren<Label>(pageHeader).ToArray();
            Assert.AreEqual(2, labels.Length);

            var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
            Assert.AreSame(labels[0], titleLabel);
            Assert.AreEqual(string.Empty, labels[1].Name);

            AssertPageHeaderLabel(
                titleLabel,
                title + " Page",
                AutomationHeadingLevel.Level1,
                0);

            AssertPageHeaderLabel(
                labels[1],
                string.Empty,
                AutomationHeadingLevel.Level2,
                1);
        }

        private static void AssertBindingPath(DependencyObject target, DependencyProperty property, string expectedPath)
        {
            var expression = BindingOperations.GetBindingExpression(target, property);
            Assert.IsNotNull(expression);
            Assert.AreEqual(expectedPath, expression.ParentBinding.Path.Path);
        }

        private static void AssertWpfGalleryHighContrastHoverTrigger(Style style, string context)
        {
            Assert.IsNotNull(style, context);

            var trigger = style.Triggers
                .OfType<MultiDataTrigger>()
                .SingleOrDefault(item =>
                    HasDynamicResourceTriggerSetter(item, Control.BackgroundProperty, "SystemColorHighlightColorBrush") &&
                    HasDynamicResourceTriggerSetter(item, Control.ForegroundProperty, "SystemColorHighlightTextColorBrush"));

            Assert.IsNotNull(trigger, context + " high-contrast hover trigger");
            Assert.AreEqual(2, trigger.Conditions.Count, context + " condition count");
            AssertBindingCondition(
                trigger,
                path => string.Equals(path, "HighContrast", StringComparison.Ordinal) ||
                        string.Equals(path, "SystemParameters.HighContrast", StringComparison.Ordinal) ||
                        string.Equals(path, "(SystemParameters.HighContrast)", StringComparison.Ordinal) ||
                        string.Equals(path, "(0)", StringComparison.Ordinal),
                "True",
                expectedSelfRelativeSource: false,
                context,
                "SystemParameters.HighContrast");
            AssertBindingCondition(
                trigger,
                path => string.Equals(path, "IsMouseOver", StringComparison.Ordinal),
                "True",
                expectedSelfRelativeSource: true,
                context,
                "IsMouseOver");
            AssertDynamicResourceTriggerSetter(
                trigger,
                Control.BackgroundProperty,
                "SystemColorHighlightColorBrush",
                context);
            AssertDynamicResourceTriggerSetter(
                trigger,
                Control.ForegroundProperty,
                "SystemColorHighlightTextColorBrush",
                context);
        }

        private static void AssertBindingCondition(
            MultiDataTrigger trigger,
            Func<string, bool> pathMatches,
            string expectedValue,
            bool expectedSelfRelativeSource,
            string context,
            string conditionName)
        {
            Assert.IsTrue(
                trigger.Conditions
                .OfType<System.Windows.Condition>()
                .Any(condition =>
                {
                    var binding = condition.Binding as Binding;
                    if (binding == null)
                    {
                        return false;
                    }

                    var hasExpectedRelativeSource = expectedSelfRelativeSource
                        ? binding.RelativeSource?.Mode == RelativeSourceMode.Self
                        : binding.RelativeSource == null;

                    return pathMatches(binding.Path?.Path ?? string.Empty) &&
                           string.Equals(condition.Value?.ToString(), expectedValue, StringComparison.OrdinalIgnoreCase) &&
                           hasExpectedRelativeSource;
                }),
                context + " " + conditionName + " condition. Actual conditions: " + DescribeConditions(trigger));
        }

        private static void AssertDynamicResourceTriggerSetter(
            MultiDataTrigger trigger,
            DependencyProperty property,
            object resourceKey,
            string context)
        {
            var setter = trigger.Setters.OfType<Setter>().Single(item => item.Property == property);
            var dynamicResource = setter.Value as DynamicResourceExtension;
            Assert.IsNotNull(dynamicResource, context + " " + property.Name);
            Assert.AreEqual(resourceKey, dynamicResource.ResourceKey, context + " " + property.Name);
        }

        private static bool HasDynamicResourceTriggerSetter(
            MultiDataTrigger trigger,
            DependencyProperty property,
            object resourceKey)
        {
            return trigger.Setters
                .OfType<Setter>()
                .Any(setter =>
                {
                    var dynamicResource = setter.Value as DynamicResourceExtension;
                    return setter.Property == property &&
                           dynamicResource != null &&
                           Equals(resourceKey, dynamicResource.ResourceKey);
                });
        }

        private static string DescribeConditions(MultiDataTrigger trigger)
        {
            return string.Join(
                "; ",
                trigger.Conditions
                    .OfType<System.Windows.Condition>()
                    .Select(condition =>
                    {
                        var binding = condition.Binding as Binding;
                        var relativeSource = binding?.RelativeSource == null
                            ? "<null>"
                            : binding.RelativeSource.Mode.ToString();
                        return (binding?.Path?.Path ?? "<null>") + "=" + (condition.Value ?? "<null>") + "@" + relativeSource;
                    }));
        }

        private static void AssertNavigationItemsControl(ItemsControl itemsControl, string automationName)
        {
            Assert.AreEqual(automationName, AutomationProperties.GetName(itemsControl));
            Assert.IsFalse(itemsControl.Focusable);
            var panel = (System.Windows.Controls.WrapPanel)itemsControl.ItemsPanel.LoadContent();
            Assert.AreEqual(new Thickness(10), panel.Margin);
        }

        private static ItemsControl GetOfficialSectionItemsControl(SectionPage sectionPage)
        {
            var root = GetOfficialSectionRoot(sectionPage);
            var itemsControl = root.Children.OfType<ItemsControl>().Single();
            Assert.AreEqual(string.Empty, itemsControl.Name);
            return itemsControl;
        }

        private static ScrollViewer GetModernWpfExtensionScrollViewer(SectionPage sectionPage)
        {
            var root = GetOfficialSectionRoot(sectionPage);
            var scrollViewer = root.Children.OfType<ScrollViewer>().Single();
            Assert.AreEqual(string.Empty, scrollViewer.Name);
            return scrollViewer;
        }

        private static Grid GetOfficialSectionRoot(SectionPage sectionPage)
        {
            var root = (Grid)sectionPage.Content;
            Assert.AreEqual(string.Empty, root.Name);
            return root;
        }

        private static Border GetContentFrameBorder(NavigationRootPage page)
        {
            var contentHost = GetContentHost(page);
            var border = contentHost.Parent as Border;
            Assert.IsNotNull(border);
            Assert.AreEqual(string.Empty, border.Name);
            return border;
        }

        private static System.Windows.Controls.Frame GetContentHost(NavigationRootPage page)
        {
            var navigation = GetNavigationView(page);
            var contentBorder = (Border)navigation.Content;
            var contentHost = (System.Windows.Controls.Frame)contentBorder.Child;
            Assert.AreEqual(string.Empty, contentHost.Name);
            return contentHost;
        }

        private static NavigationView GetNavigationView(NavigationRootPage page)
        {
            var root = (Grid)page.Content;
            var navigation = root.Children.OfType<NavigationView>().Single();
            Assert.AreEqual(string.Empty, navigation.Name);
            return navigation;
        }

        private static void RaiseMouseClick(UIElement element, int timestamp)
        {
            var previewMouseDown = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                Source = element
            };
            element.RaiseEvent(previewMouseDown);

            var mouseDown = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp + 1, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = element
            };
            element.RaiseEvent(mouseDown);
            Assert.IsTrue(mouseDown.Handled);

            var mouseUp = new MouseButtonEventArgs(Mouse.PrimaryDevice, timestamp + 2, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                Source = element
            };
            element.RaiseEvent(mouseUp);
            Assert.IsTrue(mouseUp.Handled);
        }

        private static void InvokeNavigationViewItem(NavigationView navigation, NavigationViewItem item)
        {
            var method = typeof(NavigationView).GetMethod(
                "OnNavigationViewItemInvoked",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, "NavigationView.OnNavigationViewItemInvoked should remain available for shell interaction coverage.");
            method.Invoke(navigation, new object[] { item });
        }

        private static NavigationRootPage GetNavigationRootPage(MainWindow window)
        {
            var mainGrid = (Grid)window.FindName("MainGrid");
            var rootPage = mainGrid.Children.OfType<NavigationRootPage>().Single();
            Assert.AreEqual(string.Empty, rootPage.Name);
            return rootPage;
        }

        private static Border GetHighContrastNavigationPaneEdgeCover(NavigationRootPage page)
        {
            var root = (Grid)page.Content;
            var border = root.Children.OfType<Border>().Single();
            Assert.AreEqual(string.Empty, border.Name);
            return border;
        }

        private static void AssertReferenceCategoryPageRoot(Grid root, bool hasItemsScrollViewer)
        {
            Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(root));
            Assert.AreEqual(new Thickness(0), root.Margin);
            Assert.AreEqual(2, root.RowDefinitions.Count);
            Assert.AreEqual(GridUnitType.Auto, root.RowDefinitions[0].Height.GridUnitType);
            Assert.AreEqual(GridUnitType.Star, root.RowDefinitions[1].Height.GridUnitType);

            var scrollViewers = root.Children
                .OfType<ScrollViewer>()
                .Where(scrollViewer => scrollViewer.Visibility == Visibility.Visible)
                .ToArray();
            if (hasItemsScrollViewer)
            {
                Assert.AreEqual(1, scrollViewers.Length);
                Assert.AreEqual(1, Grid.GetRow(scrollViewers[0]));
                Assert.AreEqual(new Thickness(0), scrollViewers[0].Margin);
                Assert.AreEqual(ScrollBarVisibility.Auto, scrollViewers[0].VerticalScrollBarVisibility);
            }
            else
            {
                Assert.AreEqual(0, scrollViewers.Length);
            }
        }

        private static void AssertRenderedNavigationCard(ItemsControl itemsControl, string title, string description, ICommand expectedCommand)
        {
            itemsControl.UpdateLayout();
            WpfTestHost.DoEvents();

            var button = FindVisualChildren<Button>(itemsControl).FirstOrDefault();
            Assert.IsNotNull(button);
            Assert.AreEqual(360d, button.Width);
            Assert.AreEqual(90d, button.Height);
            Assert.AreEqual(new Thickness(7), button.Margin);
            Assert.AreEqual(new Thickness(20, 10, 20, 10), button.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalContentAlignment);
            Assert.AreSame(expectedCommand, button.Command);
            Assert.AreSame(itemsControl.Items[0], button.CommandParameter);
            Assert.AreEqual(title + "Page", AutomationProperties.GetName(button));

            var image = FindVisualChildren<Image>(button).FirstOrDefault();
            Assert.IsNotNull(image);
            Assert.AreEqual(50d, image.Width);
            Assert.AreEqual(50d, image.Height);
            Assert.AreEqual(new Thickness(0, 0, 8, 0), image.Margin);

            var titleText = FindVisualChildren<TextBlock>(button).FirstOrDefault(textBlock => string.Equals(textBlock.Text, title, StringComparison.Ordinal));
            Assert.IsNotNull(titleText);
            Assert.AreEqual(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(titleText));

            if (!string.IsNullOrEmpty(description))
            {
                var descriptionText = FindVisualChildren<TextBlock>(button).FirstOrDefault(textBlock => string.Equals(textBlock.Text, description, StringComparison.Ordinal));
                Assert.IsNotNull(descriptionText);
                Assert.AreEqual(240d, descriptionText.Width);
                Assert.AreEqual(0.7, descriptionText.Opacity, 0.001);
            }
        }

        private static void AssertNavigationCardIds(IReadOnlyList<GalleryGroup> expected, IReadOnlyList<GalleryGroup> actual, string context)
        {
            CollectionAssert.AreEqual(
                expected.Select(group => group.UniqueId).ToArray(),
                actual.Select(group => group.UniqueId).ToArray(),
                context);
        }

        private static void AssertNavigationCardIds(IReadOnlyList<GalleryItem> expected, IReadOnlyList<GalleryItem> actual, string context)
        {
            CollectionAssert.AreEqual(
                expected.Select(item => item.UniqueId).ToArray(),
                actual.Select(item => item.UniqueId).ToArray(),
                context);
        }

        private static void RenderPage(FrameworkElement page, Action assert)
        {
            var window = new Window
            {
                Width = 1180,
                Height = 820,
                Left = -32000,
                Top = -32000,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Content = page
            };

            try
            {
                window.Show();
                WpfTestHost.DoEvents();
                window.UpdateLayout();
                WpfTestHost.DoEvents();
                assert();
            }
            finally
            {
                window.Content = null;
                window.Close();
                WpfTestHost.DoEvents();
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject element)
            where T : DependencyObject
        {
            if (element == null)
            {
                yield break;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        private static IEnumerable<string> CatalogRoutes()
        {
            yield return "home";
            yield return "WhatsNew";
            yield return "AllControls";

            foreach (var group in GalleryCatalog.Groups)
            {
                yield return "category/" + group.UniqueId;
            }

            foreach (var item in GalleryCatalog.Items)
            {
                yield return "item/" + item.UniqueId;
            }
        }

        private static Type GetExpectedGroupPageType(string route)
        {
            const string categoryPrefix = "category/";
            if (!route.StartsWith(categoryPrefix, StringComparison.Ordinal))
            {
                return null;
            }

            switch (route.Substring(categoryPrefix.Length))
            {
                case "DesignGuidance":
                    return typeof(DesignGuidancePage);
                case "Samples":
                    return typeof(SamplesPage);
                case "BasicInput":
                    return typeof(BasicInputPage);
                case "Collections":
                    return typeof(CollectionsPage);
                case "DateAndCalendar":
                    return typeof(DateAndTimePage);
                case "Layout":
                    return typeof(LayoutPage);
                case "Media":
                    return typeof(MediaPage);
                case "Navigation":
                    return typeof(NavigationPage);
                case "StatusAndInfo":
                    return typeof(StatusAndInfoPage);
                case "Text":
                    return typeof(TextPage);
                case "System":
                    return typeof(SystemPage);
                default:
                    return null;
            }
        }
    }
}
