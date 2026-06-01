using System;
using System.Collections.Generic;
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
        public void ShellNavigationMenuMatchesWpfGalleryReferenceChrome()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                page.DataContext = new { ViewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings, page.GoForward) };
                var navigation = GetNavigationView(page);
                var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();

                Assert.AreEqual(258d, navigation.OpenPaneLength);
                Assert.AreEqual("Navigation Pane", AutomationProperties.GetName(navigation));
                Assert.AreEqual(NavigationViewBackButtonVisible.Collapsed, navigation.IsBackButtonVisible);
                Assert.IsFalse(navigation.IsPaneToggleButtonVisible);
                Assert.IsFalse(navigation.IsSettingsVisible);
                Assert.AreEqual(string.Empty, navigation.PaneTitle);
                Assert.IsNull(navigation.PaneCustomContent);
                Assert.AreEqual(0, navigation.MenuItems.OfType<NavigationViewItemSeparator>().Count());
                Assert.IsNotNull(navigation.PaneFooter);
                Assert.AreEqual(0, navigation.FooterMenuItems.Count);
                var footerPanel = (StackPanel)navigation.PaneFooter;
                Assert.AreEqual(new Thickness(8, 10, 0, 10), footerPanel.Margin);
                Assert.AreEqual(Orientation.Vertical, footerPanel.Orientation);
                var footerSeparator = footerPanel.Children.OfType<Separator>().Single();
                Assert.IsTrue(double.IsNaN(footerSeparator.Width));
                Assert.AreEqual(HorizontalAlignment.Stretch, footerSeparator.HorizontalAlignment);
                Assert.AreSame(Geometry.Empty, page.Resources["NavigationViewItemExpandedPath"]);
                Assert.AreSame(Geometry.Empty, navigation.Resources["NavigationViewItemExpandedPath"]);
                Assert.AreEqual(new Thickness(0), navigation.Resources["NavigationViewContentGridBorderThickness"]);
                Assert.AreEqual(new CornerRadius(0), navigation.Resources["NavigationViewContentGridCornerRadius"]);
                Assert.AreEqual(Colors.Transparent, ((SolidColorBrush)navigation.Resources["NavigationViewContentBackground"]).Color);
                foreach (var resourcePair in WpfGalleryNavigationResourceAliases)
                {
                    AssertNavigationViewResourceAlias(navigation, resourcePair.Item1, resourcePair.Item2);
                }

                Assert.IsNull(page.FindName("ContentFrameBorder"));
                var contentFrameBorder = GetContentFrameBorder(page);
                Assert.AreEqual(new Thickness(4, 0, 0, 0), contentFrameBorder.Margin);
                Assert.AreEqual(new Thickness(24, 16, 24, 0), contentFrameBorder.Padding);

                CollectionAssert.AreEqual(
                    new[] { "Home", "What's New", "Design Guidance", "Samples", "All Controls", "Basic Input" },
                    topLevelItems.Take(6).Select(GetNavigationItemText).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Layout", "Navigation", "Status & Info", "Text", "System", "Media Controls", "ModernWpf controls" },
                    topLevelItems.Skip(8).Select(GetNavigationItemText).ToArray());
                Assert.AreEqual(15, topLevelItems.Count, "Deleted navigation groups should not remain in the shell menu.");

                AssertFontIconGlyph(topLevelItems[0], "\uE80F");
                AssertFontIconGlyph(topLevelItems[1], "\uEB51");
                AssertFontIconGlyph(topLevelItems[2], "\uEB3C");
                AssertFontIconGlyph(topLevelItems[3], "\uEF58");
                AssertFontIconGlyph(topLevelItems[4], "\uE71D");
                AssertFontIconGlyph(topLevelItems[5], "\uE73A");
                AssertNavigationItemsDoNotExposeLocalAutomationIds(topLevelItems);
                var expectedNavigationItemRightMargin = SystemParameters.HighContrast ? 2 : 0;
                var expectedTopLevelContentLeft = SystemParameters.HighContrast ? 20 : 32;
                var expectedChildGlyphContentLeft = SystemParameters.HighContrast ? -12 : 0;
                var expectedChildTextContentLeft = SystemParameters.HighContrast ? 4 : 16;
                var expectedTopLevelContentTop = SystemParameters.HighContrast ? -2 : 0;
                var expectedChildItemMargin = SystemParameters.HighContrast
                    ? new Thickness(20, 0, -1, 0)
                    : new Thickness(20, 1, 0, 1);
                Assert.AreEqual(new Thickness(8, 1, expectedNavigationItemRightMargin, 1), topLevelItems[0].Margin);
                AssertNavigationItemContentMargin(topLevelItems[0], expectedTopLevelContentLeft);
                AssertNavigationTitleTextLayout(topLevelItems[0], "Home");
                AssertNavigationTitleTextLayout(topLevelItems[4], "All Controls");
                Assert.IsNull(GetNavigationDisclosureChevron(topLevelItems[0]));
                Assert.IsFalse(topLevelItems[2].IsExpanded);
                Assert.IsFalse(topLevelItems[3].IsExpanded);
                Assert.IsFalse(topLevelItems[5].IsExpanded);

                var designGuidanceItems = topLevelItems[2].MenuItems.OfType<NavigationViewItem>().ToList();
                CollectionAssert.AreEqual(
                    new[] { "Colors", "Typography", "Spacing", "Geometry", "Icons" },
                    designGuidanceItems.Select(GetNavigationItemText).ToArray());
                Assert.AreEqual(expectedChildItemMargin, designGuidanceItems[0].Margin);
                AssertFontIconGlyph(designGuidanceItems[0], "\uE790");
                AssertNavigationItemContentMargin(designGuidanceItems[0], expectedChildGlyphContentLeft);
                var designGuidanceChevron = GetNavigationDisclosureChevron(topLevelItems[2]);
                Assert.IsNotNull(designGuidanceChevron);
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(designGuidanceChevron));
                Assert.AreEqual("\uE76C", designGuidanceChevron.Text);
                Assert.AreEqual(10d, designGuidanceChevron.FontSize);
                Assert.AreEqual(new Thickness(0, expectedTopLevelContentTop, 0, 0), designGuidanceChevron.Margin);
                Assert.AreEqual(0d, ((RotateTransform)designGuidanceChevron.RenderTransform).Angle);

                var basicInputItems = topLevelItems[5].MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.AreEqual(expectedChildItemMargin, basicInputItems[0].Margin);
                Assert.IsNull(basicInputItems[0].Icon);
                AssertNavigationItemContentMargin(basicInputItems[0], expectedChildTextContentLeft);

                var mediaItem = topLevelItems[13];
                Assert.AreEqual("Media Controls", GetNavigationItemText(mediaItem));
                AssertFontIconGlyph(mediaItem, "\uE8B9");
                CollectionAssert.AreEqual(
                    new[] { "Canvas", "Image" },
                    mediaItem.MenuItems.OfType<NavigationViewItem>().Select(GetNavigationItemText).ToArray());
                Assert.IsNull(mediaItem.MenuItems.OfType<NavigationViewItem>().First().Icon);

                var settingsButton = (Button)page.FindName("SettingsButton");
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(settingsButton));
                Assert.AreEqual("Settings", AutomationProperties.GetName(settingsButton));
                Assert.AreEqual(250d, settingsButton.Width);
                Assert.AreEqual(36d, settingsButton.Height);
                Assert.AreEqual(new Thickness(0, 4, 0, 0), settingsButton.Margin);
                Assert.AreEqual(HorizontalAlignment.Left, settingsButton.HorizontalContentAlignment);
                Assert.AreEqual(VerticalAlignment.Center, settingsButton.VerticalContentAlignment);
                Assert.AreEqual("ViewModel.SettingsCommand",
                    BindingOperations.GetBindingExpression(settingsButton, System.Windows.Controls.Primitives.ButtonBase.CommandProperty)?.ParentBinding.Path.Path);

                var settingsContent = (StackPanel)settingsButton.Content;
                Assert.AreEqual(Orientation.Horizontal, settingsContent.Orientation);
                Assert.AreEqual(new Thickness(11, 0, 0, 0), settingsContent.Margin);

                Assert.IsNull(page.FindName("SettingsIcon"));
                var settingsIcon = settingsContent.Children.OfType<TextBlock>()
                    .Single(text => string.Equals(text.Text, "\uE713", StringComparison.Ordinal));
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(settingsIcon));
                Assert.AreEqual("\uE713", settingsIcon.Text);
                Assert.AreEqual(14d, settingsIcon.FontSize);
                Assert.AreEqual(VerticalAlignment.Center, settingsIcon.VerticalAlignment);

                var settingsText = settingsContent.Children.OfType<TextBlock>()
                    .Single(text => string.Equals(text.Text, "Settings", StringComparison.Ordinal));
                Assert.AreEqual(14d, settingsText.FontSize);
                Assert.AreEqual(new Thickness(8, 0, 0, 0), settingsText.Margin);

                page.NavigateTo("item/Color");
                WpfTestHost.DoEvents();
                Assert.IsTrue(topLevelItems[2].IsExpanded);
                Assert.AreEqual(90d, ((RotateTransform)designGuidanceChevron.RenderTransform).Angle);

                page.NavigateTo("category/BasicInput");
                Assert.IsTrue(topLevelItems[5].IsExpanded);

                settingsButton.Command.Execute(settingsButton.CommandParameter);
                WpfTestHost.DoEvents();
                Assert.IsNull(navigation.SelectedItem);
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
        }

        [TestMethod]
        public void ShellNavigationPaneRowsUseWpfGalleryTreeViewInsets()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                RenderPage(page, () =>
                {
                    page.NavigateTo("category/Navigation");
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var navigation = GetNavigationView(page);
                    var contentHost = GetContentHost(page);
                    var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();
                    var homeItem = topLevelItems[0];
                    var navigationItem = topLevelItems.Single(item => string.Equals(GetNavigationItemText(item), "Navigation", StringComparison.Ordinal));
                    var menuItem = navigationItem.MenuItems.OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "Menu", StringComparison.Ordinal));
                    var settingsButton = (Button)page.FindName("SettingsButton");
                    var isHighContrast = SystemParameters.HighContrast;
                    var expectedTopLevelGlyphLeft = isHighContrast ? 44 : 56;
                    var expectedTopLevelTextLeft = isHighContrast ? 77 : 89;
                    var expectedGroupChevronLeft = isHighContrast ? 22.5 : 34.5;
                    var expectedTopLevelGlyphTop = isHighContrast ? 20 : 21;
                    var expectedTopLevelTextTop = isHighContrast ? 19 : 20;
                    var expectedGroupChevronTop = isHighContrast ? 399 : 400;
                    var expectedGroupGlyphTop = isHighContrast ? 398 : 399;
                    var expectedGroupTextTop = isHighContrast ? 397 : 398;
                    var expectedChildTextLeft = isHighContrast ? 80 : 92;
                    var expectedGroupContentLeft = isHighContrast ? -4 : 8;
                    var expectedChildSelectedContentLeft = isHighContrast ? -4 : 8;
                    var expectedChildDeselectedContentLeft = isHighContrast ? 4 : 16;

                    AssertBounds(page, homeItem, 8, 248, "Home row");
                    AssertBounds(page, navigationItem, 8, 248, "Navigation row");
                    AssertBounds(page, menuItem, 28, isHighContrast ? 229 : 228, "Navigation child row");
                    AssertBounds(page, settingsButton, 8, 250, "Settings row");

                    AssertTextLeft(page, homeItem, "\uE80F", expectedTopLevelGlyphLeft, "Home glyph");
                    AssertTextLeft(page, homeItem, "Home", expectedTopLevelTextLeft, "Home text");
                    AssertTextTop(page, homeItem, "\uE80F", expectedTopLevelGlyphTop, "Home glyph");
                    AssertTextTop(page, homeItem, "Home", expectedTopLevelTextTop, "Home text");
                    AssertNavigationItemContentVerticallyCentered(page, homeItem, new[] { "\uE80F", "Home" }, "Home selected row");
                    AssertTextLeft(page, navigationItem, "\uE76C", expectedGroupChevronLeft, "Navigation disclosure chevron");
                    AssertTextLeft(page, navigationItem, "\uE700", expectedTopLevelGlyphLeft, "Navigation glyph");
                    AssertTextLeft(page, navigationItem, "Navigation", expectedTopLevelTextLeft, "Navigation text");
                    AssertTextTop(page, navigationItem, "\uE76C", expectedGroupChevronTop, "Navigation disclosure chevron");
                    AssertTextTop(page, navigationItem, "\uE700", expectedGroupGlyphTop, "Navigation glyph");
                    AssertTextTop(page, navigationItem, "Navigation", expectedGroupTextTop, "Navigation text");
                    AssertTextLeft(page, menuItem, "Menu", expectedChildTextLeft, "Menu child text");
                    var paneBackground = (Brush)navigation.Resources["NavigationViewExpandedPaneBackground"];
                    var menuScrollViewer = FindVisualChildren<ScrollViewer>(navigation)
                        .Single(scrollViewer => string.Equals(scrollViewer.Name, "MenuItemsScrollViewer", StringComparison.Ordinal));
                    Assert.AreSame(paneBackground, menuScrollViewer.Background);
                    Assert.AreEqual(ScrollBarVisibility.Hidden, menuScrollViewer.VerticalScrollBarVisibility);
                    var itemsContainerGrid = FindVisualChildren<Grid>(navigation)
                        .Single(grid => string.Equals(grid.Name, "ItemsContainerGrid", StringComparison.Ordinal));
                    Assert.AreSame(paneBackground, itemsContainerGrid.Background);

                    Assert.AreSame(paneBackground, navigation.Resources["NavigationViewItemSeparatorForeground"]);
                    var rootSplitView = FindVisualChildren<SplitView>(navigation)
                        .Single(splitView => string.Equals(splitView.Name, "RootSplitView", StringComparison.Ordinal));
                    Assert.AreSame(paneBackground, rootSplitView.Background);
                    Assert.AreSame(paneBackground, rootSplitView.PaneBackground);
                    Assert.AreSame(paneBackground, rootSplitView.BorderBrush);
                    Assert.AreEqual(new Thickness(0), rootSplitView.BorderThickness);
                    var paneContentGrid = FindVisualChildren<Border>(navigation)
                        .Single(border => string.Equals(border.Name, "PaneContentGrid", StringComparison.Ordinal));
                    Assert.AreSame(paneBackground, paneContentGrid.Background);
                    Assert.AreSame(paneBackground, paneContentGrid.BorderBrush);
                    var expectedPaneBorderThickness = SystemParameters.HighContrast
                        ? new Thickness(0)
                        : new Thickness(0, 0, 1, 0);
                    Assert.AreEqual(expectedPaneBorderThickness, paneContentGrid.BorderThickness);
                    Assert.IsNull(page.FindName("HighContrastNavigationPaneEdgeCover"));
                    var edgeCover = GetHighContrastNavigationPaneEdgeCover(page);
                    Assert.AreSame(paneBackground, edgeCover.Background);
                    Assert.AreEqual(SystemParameters.HighContrast ? Visibility.Visible : Visibility.Collapsed, edgeCover.Visibility);
                    Assert.AreEqual(1d, edgeCover.Width);
                    Assert.AreEqual(698d, edgeCover.Height);
                    Assert.AreEqual(new Thickness(257, 8, 0, 0), edgeCover.Margin);
                    Assert.IsFalse(edgeCover.IsHitTestVisible);
                    var paneShadow = FindVisualChildren<ThemeShadowChrome>(navigation)
                        .Single(shadow => string.Equals(shadow.Name, "ShadowCaster", StringComparison.Ordinal));
                    Assert.AreEqual(Visibility.Collapsed, paneShadow.Visibility);
                    Assert.AreEqual(0d, paneShadow.Opacity);
                    Assert.AreEqual(0d, paneShadow.Depth);
                    Assert.IsFalse(paneShadow.IsShadowEnabled);
                    Assert.IsFalse(homeItem.IsSelected, "Home should not retain the shell selection after category navigation.");
                    Assert.IsTrue(navigationItem.IsSelected, "Navigation category should own the shell selection.");
                    Assert.IsFalse(navigationItem.IsChildSelected, "Category selection should not mark a child selected.");
                    Assert.IsFalse(menuItem.IsSelected, "Menu should not be selected until item navigation.");
                    AssertNavigationItemLayoutRootMargin(navigationItem, new Thickness(4, 2, 4, 2), "Navigation selected row background");
                    AssertNavigationItemLayoutRootMargin(menuItem, new Thickness(4, 2, 4, 2), "Menu unselected child row background");
                    AssertNavigationItemContentMargin(navigationItem, expectedGroupContentLeft, 0, "Navigation category content");
                    Assert.IsInstanceOfType(contentHost.Content, typeof(NavigationPage));

                    page.NavigateTo("item/Menu");
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.IsFalse(homeItem.IsSelected, "Home should not retain the shell selection after child navigation.");
                    Assert.IsFalse(navigationItem.IsSelected, "Parent row should not stay directly selected after child navigation.");
                    Assert.IsTrue(navigationItem.IsChildSelected, "Parent row should track selected child navigation.");
                    Assert.IsTrue(menuItem.IsSelected, "Menu child row should own item navigation selection.");
                    AssertSelectionIndicatorBounds(menuItem, 12, 19, "Menu child selection indicator");
                    AssertNavigationItemLayoutRootMargin(navigationItem, new Thickness(4, 2, 4, 2), "Navigation child-selected row background");
                    AssertNavigationItemLayoutRootMargin(menuItem, new Thickness(12, 7, -5, -5), "Menu selected child row background");
                    AssertNavigationItemContentMargin(menuItem, expectedChildSelectedContentLeft, -13, "Menu selected child content");
                    Assert.IsInstanceOfType(contentHost.Content, typeof(ItemPage));

                    page.NavigateTo("category/Navigation");
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    page.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.IsFalse(menuItem.IsSelected, "Menu should not keep item selection after category navigation.");
                    AssertNavigationItemLayoutRootMargin(menuItem, new Thickness(4, 2, 4, 2), "Menu deselected child row background");
                    AssertNavigationItemContentMargin(menuItem, expectedChildDeselectedContentLeft, 0, "Menu deselected child content");
                });
            });
        }

        [TestMethod]
        public void ShellNavigationViewTreeViewResourceAliasesTrackThemeChanges()
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
                        AssertWpfGalleryNavigationResourceAliases(navigation);
                        AssertWpfGalleryNavigationPaneBackground(navigation, "NavigationViewDefaultPaneBackground", Color.FromRgb(250, 250, 250));
                        AssertWpfGalleryNavigationPaneBackground(navigation, "NavigationViewExpandedPaneBackground", Color.FromRgb(250, 250, 250));

                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        WpfTestHost.DoEvents();
                        AssertWpfGalleryNavigationResourceAliases(navigation);
                        Assert.AreSame(
                            navigation.TryFindResource("SolidBackgroundFillColorBaseBrush"),
                            navigation.Resources["NavigationViewDefaultPaneBackground"]);
                        Assert.AreSame(
                            navigation.TryFindResource("SolidBackgroundFillColorBaseBrush"),
                            navigation.Resources["NavigationViewExpandedPaneBackground"]);
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
        public void ShellNavigationViewAliasesHaveWpfGalleryTreeViewHighContrastTokens()
        {
            WpfTestHost.Run(() =>
            {
                var themeResources = new ModernWpf.ThemeResources();
                foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
                {
                    var themeDictionary = GetModernWpfThemeDictionary(themeResources, themeName);
                    foreach (var resourcePair in WpfGalleryNavigationResourceAliases)
                    {
                        Assert.IsTrue(
                            themeDictionary.Contains(resourcePair.Item2),
                            themeName + " is missing the WPF Gallery TreeView token " + resourcePair.Item2 + " for " + resourcePair.Item1 + ".");
                    }
                }

                var highContrastDictionary = GetModernWpfThemeDictionary(themeResources, "HighContrast");
                foreach (var resourcePair in WpfGalleryNavigationResourceAliases)
                {
                    AssertHighContrastTreeViewResourceReference(
                        highContrastDictionary,
                        resourcePair.Item2,
                        GetExpectedHighContrastTreeViewResourceKey(resourcePair.Item2),
                        resourcePair.Item1);
                }
            });
        }

        [TestMethod]
        public void MainWindowUsesWpfGalleryTitleChrome()
        {
            WpfTestHost.Run(() =>
            {
                var window = new MainWindow();
                try
                {
                    var chrome = WindowChrome.GetWindowChrome(window);
                    Assert.IsNotNull(chrome);
                    Assert.AreEqual(50d, chrome.CaptionHeight);
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
                    Assert.AreEqual(44d, mainGrid.RowDefinitions[0].Height.Value);
                    Assert.AreEqual(new Thickness(0), mainGrid.Margin);

                    var highContrastBorder = (Border)window.FindName("HighContrastBorder");
                    Assert.AreEqual(new Thickness(0), highContrastBorder.BorderThickness);

                    var backButton = (Button)window.FindName("BackButton");
                    Assert.AreEqual("Back", AutomationProperties.GetName(backButton));
                    Assert.IsTrue(double.IsNaN(backButton.Width));
                    Assert.AreEqual(36d, backButton.Height);
                    Assert.AreEqual(36d, backButton.MinWidth);
                    Assert.AreEqual(new Thickness(8, 0, 8, 0), backButton.Margin);
                    Assert.AreSame(window.ViewModel.BackCommand, backButton.Command);
                    Assert.AreEqual("ViewModel.BackCommand",
                        BindingOperations.GetBindingExpression(backButton, System.Windows.Controls.Primitives.ButtonBase.CommandProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual("ViewModel.CanNavigateback",
                        BindingOperations.GetBindingExpression(backButton, UIElement.IsEnabledProperty)?.ParentBinding.Path.Path);
                    Assert.IsFalse(window.ViewModel.CanNavigateback);
                    Assert.IsFalse(backButton.IsEnabled);

                    Assert.IsNull(window.FindName("AppTitleBar"));
                    Assert.IsNull(window.FindName("TitleIcon"));
                    Assert.IsNull(window.FindName("TitleText"));
                    var titleBar = mainGrid.Children.OfType<Grid>()
                        .Single(grid => Grid.GetRow(grid) == 0);
                    var titleText = FindVisualChildren<TextBlock>(titleBar)
                        .Single(text => string.Equals(text.Text, "WPF Gallery", StringComparison.Ordinal));
                    Assert.AreEqual("WPF Gallery", titleText.Text);
                    Assert.AreEqual("ViewModel.ApplicationTitle",
                        BindingOperations.GetBindingExpression(titleText, TextBlock.TextProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleText));

                    Assert.IsNull(window.FindName("RootPage"));
                    var rootPage = GetNavigationRootPage(window);
                    rootPage.UpdateLayout();
                    WpfTestHost.DoEvents();
                    var settingsButton = (Button)rootPage.FindName("SettingsButton");
                    Assert.AreSame(
                        window.ViewModel.SettingsCommand,
                        settingsButton.Command,
                        "Settings button command should bind to the MainWindow ViewModel. Actual command: " +
                        (settingsButton.Command == null ? "<null>" : settingsButton.Command.GetType().FullName) +
                        "; root DataContext: " +
                        (rootPage.DataContext == null ? "<null>" : rootPage.DataContext.GetType().FullName));
                    Assert.AreEqual("ViewModel.SettingsCommand",
                        BindingOperations.GetBindingExpression(settingsButton, System.Windows.Controls.Primitives.ButtonBase.CommandProperty)?.ParentBinding.Path.Path);
                    var contentHost = GetContentHost(rootPage);
                    window.ViewModel.SettingsCommand.Execute(null);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(rootPage.CanGoBack);
                    Assert.IsTrue(window.ViewModel.CanNavigateback);
                    Assert.IsTrue(backButton.IsEnabled);
                    Assert.IsInstanceOfType(contentHost.Content, typeof(SettingsPage));

                    var minimizeButton = (Button)window.FindName("MinimizeButton");
                    var maximizeButton = (Button)window.FindName("MaximizeButton");
                    var closeButton = (Button)window.FindName("CloseButton");
                    Assert.AreEqual(Visibility.Visible, minimizeButton.Visibility);
                    Assert.AreEqual(Visibility.Visible, maximizeButton.Visibility);
                    Assert.AreEqual(Visibility.Visible, closeButton.Visibility);
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(minimizeButton));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(maximizeButton));
                    Assert.IsTrue(WindowChrome.GetIsHitTestVisibleInChrome(closeButton));
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
                Assert.AreEqual(50d, chrome.CaptionHeight);
                Assert.AreEqual(new CornerRadius(12), chrome.CornerRadius);
                Assert.AreEqual(new Thickness(-1), chrome.GlassFrameThickness);
                Assert.AreEqual(new Thickness(0), chrome.ResizeBorderThickness);
                Assert.IsTrue(chrome.UseAeroCaptionButtons);
                Assert.AreEqual(MainWindow.GetPrefferedNonClientFrameEdges(), chrome.NonClientFrameEdges);

                Assert.AreEqual(new Thickness(0), MainWindow.GetMainGridMargin(WindowState.Normal, false));
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
        public void ShellHighContrastHoverStylesMatchWpfGalleryReferenceChrome()
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

                    var closeButtonStyle = (Style)window.Resources["TitleBarDefaultCloseButtonStyle"];
                    AssertWpfGalleryHighContrastHoverTrigger(closeButtonStyle, "title bar close button");

                    var rootPage = GetNavigationRootPage(window);
                    var footerButtonStyle = (Style)rootPage.Resources["BorderlessButtonStyle"];
                    AssertWpfGalleryHighContrastHoverTrigger(footerButtonStyle, "navigation footer button");
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
        public void TopLevelPagesUseOfficialWpfGalleryViewModels()
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
                Assert.AreEqual("What's new in WPF", whatsNewPage.ViewModel.PageTitle);
                Assert.AreEqual("Discover all the new features, enhancements and APIs introduced in WPF", whatsNewPage.ViewModel.PageDescription);

                string requestedItemId = null;
                whatsNewPage.ItemRequested = uniqueId => requestedItemId = uniqueId;
                whatsNewPage.ViewModel.NavigateCommand.Execute("MessageBox");
                Assert.AreEqual("MessageBox", requestedItemId);
                requestedItemId = null;
                whatsNewPage.ViewModel.Navigate("MessageBox");
                Assert.AreEqual("MessageBox", requestedItemId);
                requestedItemId = null;
                whatsNewPage.ViewModel.Navigate(typeof(MessageBoxPage));
                Assert.AreEqual("MessageBox", requestedItemId);

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
                    new { UniqueId = "Samples", PageType = typeof(SamplesPage), ViewModelType = typeof(SamplesPageViewModel), PageTitle = "SamplesPage" },
                    new { UniqueId = "BasicInput", PageType = typeof(BasicInputPage), ViewModelType = typeof(BasicInputPageViewModel), PageTitle = "BasicInputPage" },
                    new { UniqueId = "Collections", PageType = typeof(CollectionsPage), ViewModelType = typeof(CollectionsPageViewModel), PageTitle = "CollectionsPage" },
                    new { UniqueId = "DateAndCalendar", PageType = typeof(DateAndTimePage), ViewModelType = typeof(DateAndTimePageViewModel), PageTitle = "DateAndTimePage" },
                    new { UniqueId = "Layout", PageType = typeof(LayoutPage), ViewModelType = typeof(LayoutPageViewModel), PageTitle = "LayoutPage" },
                    new { UniqueId = "Media", PageType = typeof(MediaPage), ViewModelType = typeof(MediaPageViewModel), PageTitle = "MediaPage" },
                    new { UniqueId = "Navigation", PageType = typeof(NavigationPage), ViewModelType = typeof(NavigationPageViewModel), PageTitle = "NavigationPage" },
                    new { UniqueId = "StatusAndInfo", PageType = typeof(StatusAndInfoPage), ViewModelType = typeof(StatusAndInfoPageViewModel), PageTitle = "StatusAndInfoPage" },
                    new { UniqueId = "Text", PageType = typeof(TextPage), ViewModelType = typeof(TextPageViewModel), PageTitle = "TextPage" },
                    new { UniqueId = "System", PageType = typeof(SystemPage), ViewModelType = typeof(SystemPageViewModel), PageTitle = "SystemPage" }
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
                    new { LookupId = "Status & Info", CanonicalId = "StatusAndInfo", PageType = typeof(StatusAndInfoPage), ViewModelType = typeof(StatusAndInfoPageViewModel), PageTitle = "StatusAndInfoPage" },
                    new { LookupId = "Media Controls", CanonicalId = "Media", PageType = typeof(MediaPage), ViewModelType = typeof(MediaPageViewModel), PageTitle = "MediaPage" }
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

                var mediaGroup = GalleryCatalog.FindGroup("Media");
                var mediaPage = new MediaPage();
                RenderPage(mediaPage, () =>
                {
                    var mediaItemsControl = GetOfficialSectionItemsControl(mediaPage);
                    AssertReferencePageHeader(FindVisualChildren<PageHeader>(mediaPage).Single(), mediaGroup.Title, mediaGroup.PageDescription, true);
                    AssertReferenceCategoryPageRoot(GetOfficialSectionRoot(mediaPage), false);
                    AssertRenderedNavigationCard(mediaItemsControl, "Canvas", GalleryCatalog.FindItem("Canvas").Description, mediaPage.ViewModel.NavigateCommand);
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
