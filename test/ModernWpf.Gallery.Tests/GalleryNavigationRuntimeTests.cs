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
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Pages.WpfGallery;
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;
using ModernWpf.Gallery.ViewModels;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryNavigationRuntimeTests
    {
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

                    var contentHost = (ContentControl)page.FindName("ContentHost");
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
                page.DataContext = new { ViewModel = new MainWindowViewModel(page.GoBack, page.OpenSettings) };
                var navigation = (NavigationView)page.FindName("Navigation");
                var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();

                Assert.AreEqual(257d, navigation.OpenPaneLength);
                Assert.AreEqual("Navigation Pane", AutomationProperties.GetName(navigation));
                Assert.AreEqual(NavigationViewBackButtonVisible.Collapsed, navigation.IsBackButtonVisible);
                Assert.IsFalse(navigation.IsPaneToggleButtonVisible);
                Assert.IsFalse(navigation.IsSettingsVisible);
                Assert.AreEqual(string.Empty, navigation.PaneTitle);
                Assert.IsNull(navigation.PaneCustomContent);
                Assert.AreEqual(0, navigation.MenuItems.OfType<NavigationViewItemSeparator>().Count());
                Assert.IsNotNull(navigation.PaneFooter);
                Assert.AreEqual(0, navigation.FooterMenuItems.Count);
                Assert.AreSame(Geometry.Empty, page.Resources["NavigationViewItemExpandedPath"]);
                Assert.AreSame(Geometry.Empty, navigation.Resources["NavigationViewItemExpandedPath"]);
                Assert.AreEqual(new Thickness(0), navigation.Resources["NavigationViewContentGridBorderThickness"]);
                Assert.AreEqual(new CornerRadius(0), navigation.Resources["NavigationViewContentGridCornerRadius"]);
                Assert.AreEqual(Colors.Transparent, ((SolidColorBrush)navigation.Resources["NavigationViewContentBackground"]).Color);
                foreach (var resourcePair in new[]
                {
                    Tuple.Create("NavigationViewItemBackground", "TreeViewItemBackground"),
                    Tuple.Create("NavigationViewItemBackgroundPointerOver", "TreeViewItemBackgroundPointerOver"),
                    Tuple.Create("NavigationViewItemBackgroundSelected", "TreeViewItemBackgroundSelected"),
                    Tuple.Create("NavigationViewItemForeground", "TreeViewItemForeground"),
                    Tuple.Create("NavigationViewItemForegroundPointerOver", "TreeViewItemForegroundPointerOver"),
                    Tuple.Create("NavigationViewItemForegroundSelected", "TreeViewItemForegroundSelected"),
                    Tuple.Create("NavigationViewItemBorderBrush", "TreeViewItemBorderBrush"),
                    Tuple.Create("NavigationViewItemBorderBrushPointerOver", "TreeViewItemBorderBrushPointerOver"),
                    Tuple.Create("NavigationViewItemBorderBrushSelected", "TreeViewItemBorderBrushSelected"),
                    Tuple.Create("NavigationViewSelectionIndicatorForeground", "TreeViewItemSelectionIndicatorForeground")
                })
                {
                    AssertNavigationViewResourceAlias(navigation, resourcePair.Item1, resourcePair.Item2);
                }

                var contentFrameBorder = (Border)page.FindName("ContentFrameBorder");
                Assert.AreEqual(new Thickness(5, 0, 0, 0), contentFrameBorder.Margin);
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
                Assert.AreEqual(new Thickness(8, 1, 0, 1), topLevelItems[0].Margin);
                AssertNavigationItemContentMargin(topLevelItems[0], 20);
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
                Assert.AreEqual(new Thickness(20, 1, 0, 1), designGuidanceItems[0].Margin);
                AssertFontIconGlyph(designGuidanceItems[0], "\uE790");
                AssertNavigationItemContentMargin(designGuidanceItems[0], -12);
                var designGuidanceChevron = GetNavigationDisclosureChevron(topLevelItems[2]);
                Assert.IsNotNull(designGuidanceChevron);
                Assert.AreEqual("GalleryNavigationDisclosureChevron", AutomationProperties.GetAutomationId(designGuidanceChevron));
                Assert.AreEqual("\uE76C", designGuidanceChevron.Text);
                Assert.AreEqual(10d, designGuidanceChevron.FontSize);
                Assert.AreEqual(new Thickness(0), designGuidanceChevron.Margin);
                Assert.AreEqual(0d, ((RotateTransform)designGuidanceChevron.RenderTransform).Angle);

                var basicInputItems = topLevelItems[5].MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.AreEqual(new Thickness(20, 1, 0, 1), basicInputItems[0].Margin);
                Assert.IsNull(basicInputItems[0].Icon);
                AssertNavigationItemContentMargin(basicInputItems[0], 4);

                var mediaItem = topLevelItems[13];
                Assert.AreEqual("Media Controls", GetNavigationItemText(mediaItem));
                AssertFontIconGlyph(mediaItem, "\uE8B9");
                CollectionAssert.AreEqual(
                    new[] { "Canvas", "Image" },
                    mediaItem.MenuItems.OfType<NavigationViewItem>().Select(GetNavigationItemText).ToArray());
                Assert.IsNull(mediaItem.MenuItems.OfType<NavigationViewItem>().First().Icon);

                var settingsButton = (Button)page.FindName("SettingsButton");
                Assert.AreEqual("SettingsButton", AutomationProperties.GetAutomationId(settingsButton));
                Assert.AreEqual("Settings", AutomationProperties.GetName(settingsButton));
                Assert.AreEqual(250d, settingsButton.Width);
                Assert.AreEqual(36d, settingsButton.Height);
                Assert.AreEqual(new Thickness(0, 4, 0, 0), settingsButton.Margin);
                Assert.AreEqual(HorizontalAlignment.Left, settingsButton.HorizontalContentAlignment);
                Assert.AreEqual(VerticalAlignment.Center, settingsButton.VerticalContentAlignment);

                var settingsContent = (StackPanel)settingsButton.Content;
                Assert.AreEqual(Orientation.Horizontal, settingsContent.Orientation);
                Assert.AreEqual(new Thickness(11, 0, 0, 0), settingsContent.Margin);

                var settingsIcon = (TextBlock)page.FindName("SettingsIcon");
                Assert.AreEqual("SettingsIcon", AutomationProperties.GetAutomationId(settingsIcon));
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
                Assert.IsInstanceOfType(((ContentControl)page.FindName("ContentHost")).Content, typeof(SettingsPage));
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
                    var normalPanel = (FrameworkElement)normalPage.FindName("VisualTestStatusPanel");
                    Assert.AreEqual(Visibility.Collapsed, normalPanel.Visibility);

                    GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                    var visualTestPage = new NavigationRootPage();
                    Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                    var visualTestPanel = (FrameworkElement)visualTestPage.FindName("VisualTestStatusPanel");
                    Assert.AreEqual(Visibility.Visible, visualTestPanel.Visibility);
                    Assert.AreEqual("home", ((TextBlock)visualTestPage.FindName("VisualTestCurrentRouteText")).Text);
                    Assert.AreEqual("Ready:home", ((TextBlock)visualTestPage.FindName("VisualTestReadyStateText")).Text);
                }
                finally
                {
                    GalleryDiagnostics.ResetForTests();
                }
            });
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

                    var navigation = (NavigationView)page.FindName("Navigation");
                    var contentHost = (ContentControl)page.FindName("ContentHost");
                    var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();
                    var homeItem = topLevelItems[0];
                    var navigationItem = topLevelItems.Single(item => string.Equals(GetNavigationItemText(item), "Navigation", StringComparison.Ordinal));
                    var menuItem = navigationItem.MenuItems.OfType<NavigationViewItem>()
                        .Single(item => string.Equals(GetNavigationItemText(item), "Menu", StringComparison.Ordinal));
                    var settingsButton = (Button)page.FindName("SettingsButton");

                    AssertBounds(page, homeItem, 8, 248, "Home row");
                    AssertBounds(page, navigationItem, 8, 248, "Navigation row");
                    AssertBounds(page, menuItem, 28, 228, "Navigation child row");
                    AssertBounds(page, settingsButton, 8, 250, "Settings row");

                    AssertTextLeft(page, homeItem, "\uE80F", 44, "Home glyph");
                    AssertTextLeft(page, homeItem, "Home", 76, "Home text");
                    AssertTextLeft(page, navigationItem, "\uE76C", 26.5, "Navigation disclosure chevron");
                    AssertTextLeft(page, navigationItem, "\uE700", 44, "Navigation glyph");
                    AssertTextLeft(page, navigationItem, "Navigation", 76, "Navigation text");
                    AssertTextLeft(page, menuItem, "Menu", 79, "Menu child text");
                    Assert.IsInstanceOfType(contentHost.Content, typeof(NavigationPage));
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
                        var navigation = (NavigationView)page.FindName("Navigation");

                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        WpfTestHost.DoEvents();
                        AssertNavigationViewResourceAlias(navigation, "NavigationViewItemForeground", "TreeViewItemForeground");
                        AssertNavigationViewResourceAlias(navigation, "NavigationViewSelectionIndicatorForeground", "TreeViewItemSelectionIndicatorForeground");

                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        WpfTestHost.DoEvents();
                        AssertNavigationViewResourceAlias(navigation, "NavigationViewItemForeground", "TreeViewItemForeground");
                        AssertNavigationViewResourceAlias(navigation, "NavigationViewSelectionIndicatorForeground", "TreeViewItemSelectionIndicatorForeground");
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
                    Assert.AreEqual(MainWindow.GetPreferredNonClientFrameEdges(), chrome.NonClientFrameEdges);
                    Assert.AreSame(Application.Current.FindResource("WindowBackground"), window.Background);
                    Assert.AreSame(window, window.DataContext);
                    Assert.AreEqual("WPF Gallery", window.ViewModel.ApplicationTitle);
                    Assert.AreEqual("WPF Gallery", window.Title);
                    Assert.AreEqual("ViewModel.ApplicationTitle",
                        BindingOperations.GetBindingExpression(window, Window.TitleProperty)?.ParentBinding.Path.Path);

                    var mainGrid = (Grid)window.FindName("MainGrid");
                    Assert.AreEqual(44d, mainGrid.RowDefinitions[0].Height.Value);
                    Assert.AreEqual(new Thickness(0), mainGrid.Margin);

                    var highContrastBorder = (Border)window.FindName("HighContrastBorder");
                    Assert.AreEqual(new Thickness(0), highContrastBorder.BorderThickness);

                    var backButton = (Button)window.FindName("BackButton");
                    Assert.AreEqual("Back", AutomationProperties.GetName(backButton));
                    Assert.AreSame(window.ViewModel.BackCommand, backButton.Command);
                    Assert.AreEqual("ViewModel.BackCommand",
                        BindingOperations.GetBindingExpression(backButton, System.Windows.Controls.Primitives.ButtonBase.CommandProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual("ViewModel.CanNavigateback",
                        BindingOperations.GetBindingExpression(backButton, UIElement.IsEnabledProperty)?.ParentBinding.Path.Path);
                    Assert.IsFalse(window.ViewModel.CanNavigateback);
                    Assert.IsFalse(backButton.IsEnabled);
                    window.SetBackButtonVisible(true);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(window.ViewModel.CanNavigateback);
                    Assert.IsTrue(backButton.IsEnabled);

                    var titleText = (TextBlock)window.FindName("TitleText");
                    Assert.AreEqual("WPF Gallery", titleText.Text);
                    Assert.AreEqual("ViewModel.ApplicationTitle",
                        BindingOperations.GetBindingExpression(titleText, TextBlock.TextProperty)?.ParentBinding.Path.Path);
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleText));

                    var rootPage = (NavigationRootPage)window.FindName("RootPage");
                    var settingsButton = (Button)rootPage.FindName("SettingsButton");
                    Assert.AreSame(window.ViewModel.SettingsCommand, settingsButton.Command);
                    Assert.AreEqual("Value.ViewModel.SettingsCommand",
                        BindingOperations.GetBindingExpression(settingsButton, System.Windows.Controls.Primitives.ButtonBase.CommandProperty)?.ParentBinding.Path.Path);
                    var contentHost = (ContentControl)rootPage.FindName("ContentHost");
                    window.ViewModel.SettingsCommand.Execute(null);
                    WpfTestHost.DoEvents();
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
                Assert.AreEqual(MainWindow.GetPreferredNonClientFrameEdges(), chrome.NonClientFrameEdges);

                Assert.AreEqual(new Thickness(0), MainWindow.GetMainGridMargin(WindowState.Normal, false));
                Assert.AreEqual(new Thickness(8), MainWindow.GetMainGridMargin(WindowState.Maximized, false));
                Assert.AreEqual(new Thickness(0, 8, 0, 0), MainWindow.GetMainGridMargin(WindowState.Maximized, true));
                Assert.AreEqual(new Thickness(0), MainWindow.GetHighContrastBorderThickness(false));
                Assert.AreEqual(new Thickness(8, 1, 8, 8), MainWindow.GetHighContrastBorderThickness(true));

                Assert.AreEqual(
                    NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left,
                    MainWindow.GetPreferredNonClientFrameEdges(isHighContrast: false, isWindows11OrGreater: true));
                Assert.AreEqual(
                    NonClientFrameEdges.None,
                    MainWindow.GetPreferredNonClientFrameEdges(isHighContrast: true, isWindows11OrGreater: true));
                Assert.AreEqual(
                    NonClientFrameEdges.None,
                    MainWindow.GetPreferredNonClientFrameEdges(isHighContrast: false, isWindows11OrGreater: false));
            });
        }

        [TestMethod]
        public void HomePageOverviewUsesWpfReferenceGroupFilter()
        {
            WpfTestHost.Run(() =>
            {
                var page = new HomePage();
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
                var homePage = new HomePage();
                Assert.IsInstanceOfType(homePage, typeof(System.Windows.Controls.Page));
                Assert.IsInstanceOfType(homePage.ViewModel, typeof(DashboardPageViewModel));
                AssertNavigationCardIds(GalleryCatalog.OverviewGroups, homePage.ViewModel.NavigationCards, "Home");
                AssertNavigationCardIds(GalleryCatalog.NewOrUpdatedItems, homePage.ViewModel.RecentlyAddedOrUpdatedSamplesInfo, "Home recently added");

                var whatsNewPage = new WhatsNewPage();
                Assert.IsInstanceOfType(whatsNewPage, typeof(System.Windows.Controls.Page));
                Assert.IsInstanceOfType(whatsNewPage.ViewModel, typeof(WhatsNewPageViewModel));
                Assert.AreEqual("What's new in WPF", whatsNewPage.ViewModel.PageTitle);
                Assert.AreEqual("Discover all the new features, enhancements and APIs introduced in WPF", whatsNewPage.ViewModel.PageDescription);

                string requestedItemId = null;
                whatsNewPage.ItemRequested = uniqueId => requestedItemId = uniqueId;
                whatsNewPage.ViewModel.NavigateCommand.Execute("MessageBox");
                Assert.AreEqual("MessageBox", requestedItemId);

                var allControlsPage = new AllControlsPage();
                Assert.IsInstanceOfType(allControlsPage, typeof(System.Windows.Controls.Page));
                Assert.IsInstanceOfType(allControlsPage.ViewModel, typeof(AllSamplesPageViewModel));
                Assert.AreEqual("All Controls", allControlsPage.ViewModel.PageTitle);
                Assert.AreEqual(string.Empty, allControlsPage.ViewModel.PageDescription);
                AssertNavigationCardIds(GalleryCatalog.AllControlsItems, allControlsPage.ViewModel.NavigationCards, "All Controls");
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

                    GalleryItem requestedItem = null;
                    page.ItemRequested = item => requestedItem = item;
                    page.ViewModel.NavigateCommand.Execute(group.Items.First());
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
        public void WpfGalleryPageShellCardsMatchReferenceAutomationAndLayout()
        {
            WpfTestHost.Run(() =>
            {
                var homePage = new HomePage();
                RenderPage(homePage, () =>
                {
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId((Grid)homePage.FindName("ContentRootGrid")));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("HeroVersionText")));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("HeroTitleText")));
                    Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("OverviewHeaderText")));
                    Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("RecentlyAddedHeaderText")));
                    AssertNavigationItemsControl((ItemsControl)homePage.FindName("OverviewItemsControl"), "Items in group");
                    AssertNavigationItemsControl((ItemsControl)homePage.FindName("RecentlyAddedItemsControl"), "Recently Added and Updated Samples Section");
                    AssertBindingPath((ItemsControl)homePage.FindName("OverviewItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertBindingPath((ItemsControl)homePage.FindName("RecentlyAddedItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.RecentlyAddedOrUpdatedSamplesInfo");
                    Assert.AreEqual(new Thickness(0), ((Grid)homePage.FindName("HomeContentGrid")).Margin);
                    Assert.AreEqual(new Thickness(0), ((TextBlock)homePage.FindName("RecentlyAddedHeaderText")).Margin);

                    var firstGroup = GalleryCatalog.OverviewGroups.First();
                    AssertRenderedNavigationCard((ItemsControl)homePage.FindName("OverviewItemsControl"), firstGroup.Title, firstGroup.Description, homePage.ViewModel.NavigateCommand);
                });

                var basicInputGroup = GalleryCatalog.FindGroup("BasicInput");
                var sectionPage = new BasicInputPage();
                RenderPage(sectionPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)sectionPage.FindName("PageHeader"), basicInputGroup.Title, basicInputGroup.PageDescription, true);
                    AssertNavigationItemsControl((ItemsControl)sectionPage.FindName("GroupItemsControl"), "Items in group");
                    AssertBindingPath((ItemsControl)sectionPage.FindName("GroupItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertReferenceCategoryPageRoot((Grid)sectionPage.FindName("ContentRootGrid"), false);
                    Assert.AreEqual(basicInputGroup.Title, sectionPage.PageTitle);
                    Assert.AreEqual(basicInputGroup.PageDescription, sectionPage.PageDescription);
                    AssertRenderedNavigationCard((ItemsControl)sectionPage.FindName("GroupItemsControl"), basicInputGroup.Items.First().Title, basicInputGroup.Items.First().Description, sectionPage.ViewModel.NavigateCommand);
                });

                var mediaGroup = GalleryCatalog.FindGroup("Media");
                var mediaPage = new MediaPage();
                RenderPage(mediaPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)mediaPage.FindName("PageHeader"), mediaGroup.Title, mediaGroup.PageDescription, true);
                    AssertReferenceCategoryPageRoot((Grid)mediaPage.FindName("ContentRootGrid"), false);
                    AssertRenderedNavigationCard((ItemsControl)mediaPage.FindName("GroupItemsControl"), "Canvas", GalleryCatalog.FindItem("Canvas").Description, mediaPage.ViewModel.NavigateCommand);
                });

                var allControlsPage = new AllControlsPage();
                RenderPage(allControlsPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)allControlsPage.FindName("PageHeader"), "All Controls", string.Empty, true);
                    AssertNavigationItemsControl((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), "Items in group");
                    AssertBindingPath((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertReferenceCategoryPageRoot((Grid)allControlsPage.FindName("ContentRootGrid"), true);
                    Assert.AreEqual("All Controls", allControlsPage.PageTitle);
                    Assert.AreEqual(string.Empty, allControlsPage.PageDescription);
                    AssertRenderedNavigationCard((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), GalleryCatalog.AllControlsItems.First().Title, GalleryCatalog.AllControlsItems.First().Description, allControlsPage.ViewModel.NavigateCommand);
                });

                var modernWpfGroup = GalleryCatalog.FindGroup("ModernWpfControls");
                var modernWpfSectionPage = new SectionPage(modernWpfGroup);
                RenderPage(modernWpfSectionPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)modernWpfSectionPage.FindName("PageHeader"), modernWpfGroup.Title, modernWpfGroup.PageDescription, true);
                    Assert.AreEqual(Visibility.Collapsed, ((ItemsControl)modernWpfSectionPage.FindName("GroupItemsControl")).Visibility);
                    var scrollViewer = (ScrollViewer)modernWpfSectionPage.FindName("ModernWpfGroupScrollViewer");
                    Assert.AreEqual(Visibility.Visible, scrollViewer.Visibility);
                    Assert.AreEqual(1, Grid.GetRow(scrollViewer));
                    Assert.AreEqual(new Thickness(0), scrollViewer.Margin);
                    Assert.AreEqual(ScrollBarVisibility.Auto, scrollViewer.VerticalScrollBarVisibility);
                    var itemsControl = (ItemsControl)modernWpfSectionPage.FindName("ModernWpfGroupItemsControl");
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
                    var pageHeader = (PageHeader)navigationViewPage.FindName("PageHeader");
                    Assert.IsNotNull(pageHeader);
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
                    var wrapperHeader = (PageHeader)itemPage.FindName("PageHeader");
                    Assert.IsNotNull(wrapperHeader);
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
            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);
            Assert.AreEqual(expectedLeft, contentGrid.Margin.Left);
        }

        private static void AssertNavigationTitleTextLayout(NavigationViewItem item, string expectedTitle)
        {
            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);

            var titleText = contentGrid.Children.OfType<TextBlock>()
                .Single(text => string.Equals(text.Text, expectedTitle, StringComparison.Ordinal));
            Assert.AreEqual(HorizontalAlignment.Left, titleText.HorizontalAlignment);
        }

        private static TextBlock GetNavigationDisclosureChevron(NavigationViewItem item)
        {
            var contentGrid = item.Content as Grid;
            Assert.IsNotNull(contentGrid);

            return contentGrid.Children.OfType<TextBlock>()
                .SingleOrDefault(text => string.Equals(
                    AutomationProperties.GetAutomationId(text),
                    "GalleryNavigationDisclosureChevron",
                    StringComparison.Ordinal));
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

        private static void AssertNavigationItemsControl(ItemsControl itemsControl, string automationName)
        {
            Assert.AreEqual(automationName, AutomationProperties.GetName(itemsControl));
            Assert.IsFalse(itemsControl.Focusable);
            var panel = (System.Windows.Controls.WrapPanel)itemsControl.ItemsPanel.LoadContent();
            Assert.AreEqual(new Thickness(10), panel.Margin);
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
