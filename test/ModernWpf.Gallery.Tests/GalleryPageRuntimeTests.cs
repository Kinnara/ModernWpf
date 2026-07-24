using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Pages.WpfGallery;
using ModernWpf.Gallery.Pages.WpfGallery.BasicInput;
using ModernWpf.Gallery.Pages.WpfGallery.Collections;
using ModernWpf.Gallery.Pages.WpfGallery.DateAndTime;
using ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance;
using ModernWpf.Gallery.Pages.WpfGallery.Layout;
using ModernWpf.Gallery.Pages.WpfGallery.Media;
using ModernWpf.Gallery.Pages.WpfGallery.Navigation;
using ModernWpf.Gallery.Pages.WpfGallery.Samples;
using ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo;
using ModernWpf.Gallery.Pages.WpfGallery.SystemPages;
using ModernWpf.Gallery.Pages.WpfGallery.Text;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryPageRuntimeTests
    {
        public static IEnumerable<object[]> CatalogItems()
        {
            return GalleryCatalog.Items.Select(item => new object[] { item.UniqueId });
        }

        public static string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            return methodInfo.Name + "_" + data[0];
        }

        [TestMethod]
        [DynamicData(nameof(CatalogItems), DynamicDataDisplayName = nameof(GetDisplayName))]
        public void CatalogItemPageCanLoadAndLayout(string uniqueId)
        {
            var item = GalleryCatalog.FindItem(uniqueId);
            Assert.IsNotNull(item, "Catalog item '{0}' was not found.", uniqueId);

            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(item);
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
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

                    Assert.IsTrue(page.HasWpfSampleContent, "No WPF sample content was available for '{0}'.", uniqueId);
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
        public void RetainedWpfAliasReplacementPagesUseWpfGalleryMode()
        {
            WpfTestHost.Run(() =>
            {
                foreach (var uniqueId in new[] { "Calendar", "RichTextEdit", "TabControl" })
                {
                    var item = GalleryCatalog.FindItem(uniqueId);
                    Assert.IsNotNull(item, uniqueId);

                    var page = new ItemPage(item);

                    Assert.IsTrue(page.UsesWpfGalleryPageMode, uniqueId);
                    Assert.IsFalse(page.ShowCatalogDetails, uniqueId);
                    Assert.IsFalse(page.ShowPageDescription, uniqueId);
                }
            });
        }

        [TestMethod]
        public void WpfGalleryItemPageDescriptionsMatchReferenceViewModels()
        {
            var expectedDescriptions = new Dictionary<string, string>
            {
                { "Color", "Guide showing how to use colors in your app" },
                { "Iconography", "Guide showing how to use icons in your application." },
                { "Label", string.Empty },
                { "PasswordBox", string.Empty },
                { "RichTextBox", string.Empty },
                { "Spacing", "Guide showing how to use spacing in your app" },
                { "TextBlock", string.Empty },
                { "TextBox", string.Empty },
                { "Typography", "Guide showing how to use typography in your app" },
            };

            WpfTestHost.Run(() =>
            {
                foreach (var expectedDescription in expectedDescriptions)
                {
                    var page = new ItemPage(GalleryCatalog.FindItem(expectedDescription.Key));
                    RenderPage(page);

                    if (page.HasDirectPageContent)
                    {
                        var pageHeader = FindDescendant<PageHeader>((DependencyObject)page.DirectPageContent);

                        Assert.IsFalse(page.ShowPageHeader, expectedDescription.Key);
                        Assert.IsNotNull(pageHeader, expectedDescription.Key);
                        var expectedTitle = expectedDescription.Key == "Color"
                            ? "Colors"
                            : expectedDescription.Key == "Iconography"
                                ? "Icons"
                                : expectedDescription.Key;
                        Assert.AreEqual(expectedTitle, pageHeader.Title, expectedDescription.Key);
                        Assert.AreEqual(expectedDescription.Value, pageHeader.Description, expectedDescription.Key);
                    }
                    else
                    {
                        Assert.IsTrue(page.ShowPageDescription, expectedDescription.Key);
                        Assert.AreEqual(expectedDescription.Value, page.Description, expectedDescription.Key);
                    }
                }
            });
        }

        [TestMethod]
        public void TopLevelWpfGalleryPagesAcceptInjectedViewModels()
        {
            WpfTestHost.Run(() =>
            {
                var dashboardViewModel = new DashboardPageViewModel(_ => { });
                var homePage = new DashboardPage(dashboardViewModel);
                RenderPage(homePage);
                Assert.AreSame(dashboardViewModel, homePage.ViewModel);
                Assert.AreSame(homePage, homePage.DataContext);
                var overviewItemsControl = FindDescendants<ItemsControl>(homePage)
                    .Single(itemsControl => string.Equals(AutomationProperties.GetName(itemsControl), "Items in group", StringComparison.Ordinal));
                CollectionAssert.AreEqual(
                    dashboardViewModel.NavigationCards.Select(group => group.UniqueId).ToArray(),
                    overviewItemsControl.ItemsSource.Cast<GalleryGroup>().Select(group => group.UniqueId).ToArray());

                var allSamplesViewModel = new AllSamplesPageViewModel(_ => { });
                var allControlsPage = new AllSamplesPage(allSamplesViewModel);
                RenderPage(allControlsPage);
                Assert.AreSame(allSamplesViewModel, allControlsPage.ViewModel);
                Assert.AreSame(allControlsPage, allControlsPage.DataContext);
                var allControlsItemsControl = FindDescendants<ItemsControl>(allControlsPage)
                    .Single(itemsControl => string.Equals(AutomationProperties.GetName(itemsControl), "Items in group", StringComparison.Ordinal));
                CollectionAssert.AreEqual(
                    allSamplesViewModel.NavigationCards.Select(item => item.UniqueId).ToArray(),
                    allControlsItemsControl.ItemsSource.Cast<GalleryItem>().Select(item => item.UniqueId).ToArray());

                var whatsNewViewModel = new WhatsNewPageViewModel(_ => { });
                var whatsNewPage = new WhatsNewPage(whatsNewViewModel);
                RenderPage(whatsNewPage);
                Assert.AreSame(whatsNewViewModel, whatsNewPage.ViewModel);
                Assert.AreSame(whatsNewPage, whatsNewPage.DataContext);
                var whatsNewHeader = FindDescendant<PageHeader>(whatsNewPage);
                Assert.AreEqual(whatsNewViewModel.PageTitle, whatsNewHeader.Title);
                Assert.AreEqual(whatsNewViewModel.PageDescription, whatsNewHeader.Description);

                var settingsViewModel = new SettingsPageViewModel();
                var settingsPage = new SettingsPage(settingsViewModel);
                Assert.AreSame(settingsViewModel, settingsPage.ViewModel);
                Assert.AreSame(settingsPage, settingsPage.DataContext);

                var designGuidanceGroup = GalleryCatalog.FindGroup("DesignGuidance");
                var sectionViewModel = new DesignGuidancePageViewModel(_ => { });
                var sectionPage = new SectionPage(designGuidanceGroup, sectionViewModel);
                RenderPage(sectionPage);
                Assert.AreSame(sectionViewModel, sectionPage.ViewModel);
                Assert.AreSame(sectionPage, sectionPage.DataContext);
                Assert.AreEqual("DesignGuidancePage", sectionPage.Title);
                var sectionHeader = FindDescendant<PageHeader>(sectionPage);
                Assert.AreEqual(sectionViewModel.PageTitle, sectionHeader.Title);
                Assert.AreEqual(sectionViewModel.PageDescription, sectionHeader.Description);
                var sectionRoot = (Grid)sectionPage.Content;
                Assert.AreEqual(string.Empty, sectionRoot.Name);
                var groupItemsControl = sectionRoot.Children.OfType<ItemsControl>().Single();
                Assert.AreEqual(string.Empty, groupItemsControl.Name);
                Assert.AreEqual("Items in group", AutomationProperties.GetName(groupItemsControl));
                CollectionAssert.AreEqual(
                    sectionViewModel.NavigationCards.Select(item => item.UniqueId).ToArray(),
                    groupItemsControl.ItemsSource.Cast<GalleryItem>().Select(item => item.UniqueId).ToArray());
            });
        }

        [TestMethod]
        public void ItemPageContentRootDoesNotExposeDiagnosticAutomationId()
        {
            WpfTestHost.Run(() =>
            {
                var generatedPage = new ItemPage(GalleryCatalog.FindItem("InfoBar"));
                var generatedRoot = (FrameworkElement)generatedPage.Content;
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(generatedRoot));

                var directPage = new ItemPage(GalleryCatalog.FindItem("UserDashboard"));
                var directRoot = (FrameworkElement)directPage.Content;
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(directRoot));
            });
        }

        [TestMethod]
        public void WhatsNewPageShowsModernWpfUpdateHub()
        {
            WpfTestHost.Run(() =>
            {
                var page = new WhatsNewPage();
                RenderPage(page);
                var pageHeader = FindDescendant<PageHeader>(page);

                Assert.AreEqual("What's New in ModernWpf", page.Title);
                Assert.AreEqual(new Thickness(0, 0, 0, 32), pageHeader.Margin);
                Assert.AreEqual("What's new in ModernWpf", pageHeader.Title);
                Assert.AreEqual(
                    "See the current ModernWpf direction, supported targets, and gallery improvements.",
                    pageHeader.Description);
                Assert.IsTrue(pageHeader.ShowDescription);
                AssertBindingPath(pageHeader, PageHeader.TitleProperty, "ViewModel.PageTitle");
                AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "ViewModel.PageDescription");

                pageHeader.ApplyTemplate();
                var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
                Assert.IsNotNull(titleLabel);
                Assert.AreEqual("What's new in ModernWpf Page", AutomationProperties.GetName(titleLabel));
                Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleLabel));
                Assert.AreEqual(0, KeyboardNavigation.GetTabIndex(titleLabel));

                var descriptionLabel = pageHeader.FindDescendants<Label>().Single(label => !ReferenceEquals(label, titleLabel));
                Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(descriptionLabel));
                Assert.AreEqual(1, KeyboardNavigation.GetTabIndex(descriptionLabel));
                Assert.AreEqual(Visibility.Visible, descriptionLabel.Visibility);

                var title = (TextBlock)titleLabel.Content;
                var description = (TextBlock)pageHeader.Template.FindName("DescriptionTextBlock", pageHeader);
                Assert.AreEqual("What's new in ModernWpf", title.Text);
                Assert.AreEqual(
                    "See the current ModernWpf direction, supported targets, and gallery improvements.",
                    description.Text);

                var root = (Grid)page.FindName("ContentPagePane");
                Assert.AreEqual("ContentPagePane", root.Name);
                Assert.IsTrue(double.IsNaN((double)root.ReadLocalValue(FrameworkElement.HeightProperty)));
                Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(root));
                Assert.AreEqual(
                    (double)Application.Current.FindResource("BodyTextBlockFontSize"),
                    TextElement.GetFontSize(root));

                var samples = (ItemsControl)page.FindName("NewOrUpdatedSamples");
                Assert.AreEqual(
                    "New and updated ModernWpf samples",
                    AutomationProperties.GetName(samples));
                CollectionAssert.AreEqual(
                    GalleryCatalog.NewOrUpdatedItems.Select(item => item.UniqueId).ToArray(),
                    samples.ItemsSource.Cast<GalleryItem>().Select(item => item.UniqueId).ToArray());

                var controlExample = FindDescendants<ControlExample>(page).Single();
                Assert.AreEqual("Application resources", controlExample.HeaderText);
                Assert.AreEqual(page.ViewModel.RecommendedResourcesXamlCode, controlExample.XamlCode);
                Assert.AreEqual(new Thickness(2, 0, 2, 24), controlExample.Margin);

                var visibleText = FindDescendants<TextBlock>(page)
                    .Select(textBlock => textBlock.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .ToArray();
                CollectionAssert.Contains(visibleText, ".NET Framework 4.6.2");
                CollectionAssert.Contains(visibleText, ".NET 8 for Windows");
                CollectionAssert.Contains(visibleText, ".NET 10 for Windows");
                CollectionAssert.Contains(visibleText, "WinUI-style shell");
            });
        }

        [TestMethod]
        public void WhatsNewCatalogCardsUseViewModelNavigationHandler()
        {
            WpfTestHost.Run(() =>
            {
                string requestedItemId = null;
                var page = new WhatsNewPage();
                page.ItemRequested = uniqueId => requestedItemId = uniqueId;
                var item = GalleryCatalog.NewOrUpdatedItems.First();

                page.ViewModel.NavigateCommand.Execute(item);
                Assert.AreEqual(item.UniqueId, requestedItemId);

                requestedItemId = null;
                page.ViewModel.Navigate(item.UniqueId);
                Assert.AreEqual(item.UniqueId, requestedItemId);
            });
        }

        [TestMethod]
        public void AdaptedWhatsNewPageUsesModernWpfCardResources()
        {
            WpfTestHost.Run(() =>
            {
                var whatsNewPage = new WhatsNewPage();
                var updateCardStyle = (Style)whatsNewPage.Resources["UpdateCardStyle"];
                Assert.AreEqual(typeof(Border), updateCardStyle.TargetType);
                AssertStyleSetter(updateCardStyle, Border.PaddingProperty, new Thickness(16));
                AssertStyleSetter(updateCardStyle, Border.BorderThicknessProperty, new Thickness(1));
                AssertStyleSetter(updateCardStyle, Border.CornerRadiusProperty, new CornerRadius(8));

                var imagePage = new ImagePage(new ImagePageViewModel());
                Assert.AreEqual("https://github.com/dotnet/wpf", imagePage.Resources["PageXamlUrl"]);
                Assert.AreEqual(
                    "https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/Image.cs",
                    imagePage.Resources["PageCsharpUrl"]);
            });
        }

        [TestMethod]
        public void SettingsPageMatchesWpfGalleryReferenceLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new SettingsPage();
                var window = new Window
                {
                    Width = 1024,
                    Height = 768,
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

                    Assert.IsInstanceOfType(page, typeof(Page));

                    Assert.IsNull(page.FindName("ContentRootGrid"));
                    var root = (Grid)page.Content;
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(root));
                    Assert.AreEqual(2, root.RowDefinitions.Count);
                    Assert.AreEqual(GridUnitType.Auto, root.RowDefinitions[0].Height.GridUnitType);
                    Assert.AreEqual(GridUnitType.Star, root.RowDefinitions[1].Height.GridUnitType);

                    Assert.IsInstanceOfType(page.ViewModel, typeof(SettingsPageViewModel));
                    Assert.AreEqual("Settings", page.ViewModel.PageTitle);
                    Assert.IsNull(page.ViewModel.PageDescription);

                    var pageHeader = FindDescendant<PageHeader>(root);
                    Assert.IsNotNull(pageHeader);
                    Assert.AreEqual(0, Grid.GetRow(pageHeader));
                    Assert.AreEqual(new Thickness(0, 0, 0, 40), pageHeader.Margin);
                    Assert.AreEqual("Settings", pageHeader.Title);
                    Assert.IsNull(pageHeader.Description);
                    AssertBindingPath(pageHeader, PageHeader.TitleProperty, "ViewModel.PageTitle");
                    AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "ViewModel.PageDescription");
                    var changedProperties = new List<string>();
                    page.ViewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
                    page.ViewModel.PageTitle = "Settings Preview";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Settings Preview", pageHeader.Title);
                    CollectionAssert.AreEqual(new[] { "PageTitle" }, changedProperties.ToArray());
                    page.ViewModel.PageTitle = "Settings";
                    WpfTestHost.DoEvents();
                    Assert.AreEqual("Settings", pageHeader.Title);

                    pageHeader.ApplyTemplate();
                    var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
                    Assert.IsNotNull(titleLabel);
                    Assert.AreEqual("Settings Page", AutomationProperties.GetName(titleLabel));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleLabel));
                    Assert.IsTrue(titleLabel.Focusable);
                    Assert.IsTrue(KeyboardNavigation.GetIsTabStop(titleLabel));
                    Assert.AreEqual(0, KeyboardNavigation.GetTabIndex(titleLabel));

                    var title = (TextBlock)titleLabel.Content;
                    Assert.AreEqual("Settings", title.Text);

                    var scrollViewer = root.Children.OfType<ScrollViewer>().Single();
                    Assert.AreEqual(1, Grid.GetRow(scrollViewer));
                    Assert.AreEqual(new Thickness(0, 0, 0, 24), scrollViewer.Margin);
                    Assert.AreEqual(new Thickness(0, 0, 24, 0), scrollViewer.Padding);

                    var textBlocks = FindDescendants<TextBlock>(root).ToArray();
                    var appearanceHeader = textBlocks.Single(textBlock => textBlock.Text == "Appearance & behavior");
                    var aboutHeader = textBlocks.Single(textBlock => textBlock.Text == "About");
                    AssertSettingsSectionHeader(appearanceHeader, "Appearance & behavior");
                    AssertSettingsSectionHeader(aboutHeader, "About");

                    var appIcon = (TextBlock)page.FindName("AppIcon");
                    Assert.AreEqual("App Icon", AutomationProperties.GetName(appIcon));
                    Assert.AreEqual(20.0, appIcon.Width);
                    Assert.AreEqual(20.0, appIcon.Height);
                    Assert.AreEqual(new Thickness(10, 5, 10, 5), appIcon.Margin);
                    Assert.AreEqual("\uE790", appIcon.Text);

                    var themeMode = (ComboBox)page.FindName("Change_ThemeMode");
                    Assert.AreEqual(200.0, themeMode.MinWidth);
                    Assert.AreEqual(HorizontalAlignment.Right, themeMode.HorizontalAlignment);
                    Assert.AreEqual(new Thickness(10), themeMode.Margin);
                    Assert.AreEqual("Change ThemeMode", AutomationProperties.GetName(themeMode));
                    CollectionAssert.AreEqual(
                        new[] { "Light", "Dark", "Use system setting" },
                        themeMode.Items.Cast<ComboBoxItem>().Select(item => item.Content.ToString()).ToArray());
                    Assert.AreEqual(2, themeMode.SelectedIndex);
                    Assert.AreEqual("Use system setting", ((ComboBoxItem)themeMode.SelectedItem).Content);

                    var aboutExpander = FindDescendants<Expander>(root)
                        .Single(expander => AutomationProperties.GetName(expander) == "WPF Gallery Preview");
                    Assert.AreEqual("WPF Gallery Preview", AutomationProperties.GetName(aboutExpander));
                    var expanderHeader = (Grid)aboutExpander.Header;
                    Assert.AreEqual(3, expanderHeader.ColumnDefinitions.Count);
                    var aboutHeaderText = (StackPanel)expanderHeader.Children[1];
                    Assert.AreEqual("WPF Gallery", ((TextBlock)aboutHeaderText.Children[0]).Text);
                    Assert.AreEqual("\u00A9 2025 Microsoft. All rights reserved.", ((TextBlock)aboutHeaderText.Children[1]).Text);

                    var cloneCommand = FindDescendants<TextBox>(root)
                        .Single(textBox => textBox.Text == "git clone https://github.com/microsoft/WPF-Samples.git");
                    Assert.IsFalse(cloneCommand.Focusable);
                    Assert.AreEqual("git clone https://github.com/microsoft/WPF-Samples.git", cloneCommand.Text);

                    var openIssues = FindDescendants<Button>(root)
                        .Single(button => AutomationProperties.GetName(button) == "Open Issues");
                    Assert.AreEqual("Open Issues", AutomationProperties.GetName(openIssues));
                    Assert.AreEqual(new Thickness(8), openIssues.Padding);
                    Assert.IsTrue(FocusManager.GetIsFocusScope(openIssues));

                    var groupBoxes = FindDescendants<GroupBox>(root).ToArray();
                    var dependencies = groupBoxes.Single(groupBox => AutomationProperties.GetName(groupBox) == "Dependencies and References");
                    var warranty = groupBoxes.Single(groupBox => AutomationProperties.GetName(groupBox).StartsWith("THIS CODE AND INFORMATION IS PROVIDED", StringComparison.Ordinal));
                    Assert.AreEqual("Dependencies and References", AutomationProperties.GetName(dependencies));
                    Assert.AreEqual("THIS CODE AND INFORMATION IS PROVIDED \u2018AS IS\u2019 WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.", AutomationProperties.GetName(warranty));
                    Assert.AreEqual(new Thickness(0), dependencies.BorderThickness);
                    Assert.AreEqual(new Thickness(0), warranty.BorderThickness);

                    var hyperlinks = FindDescendants<Hyperlink>(root).ToArray();
                    var toolkitInformationLink = hyperlinks.Single(hyperlink => GetHyperlinkText(hyperlink) == "CommunityToolkit.Mvvm");
                    var dependencyInjectionInformationLink = hyperlinks.Single(hyperlink => GetHyperlinkText(hyperlink) == "Microsoft.Extensions.DependencyInjection");
                    var hostingInformationLink = hyperlinks.Single(hyperlink => GetHyperlinkText(hyperlink) == "Microsoft.Extensions.Hosting");
                    Assert.AreEqual("CommunityToolkit.Mvvm", GetHyperlinkText(toolkitInformationLink));
                    Assert.AreEqual("Microsoft.Extensions.DependencyInjection", GetHyperlinkText(dependencyInjectionInformationLink));
                    Assert.AreEqual("Microsoft.Extensions.Hosting", GetHyperlinkText(hostingInformationLink));
                    Assert.AreEqual("Link to Dependency Injection NuGet Package", AutomationProperties.GetName(dependencyInjectionInformationLink));
                    Assert.AreEqual("Link to .NET Generic Host Package", AutomationProperties.GetName(hostingInformationLink));
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
        public void SettingsPageInitialThemeSelectionDoesNotOverrideForcedTheme()
        {
            WpfTestHost.Run(() =>
            {
                var previousTheme = ThemeManager.Current.ApplicationTheme;
                try
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

                    var page = new SettingsPage();
                    var themeMode = (ComboBox)page.FindName("Change_ThemeMode");

                    Assert.AreEqual(2, themeMode.SelectedIndex);
                    Assert.AreEqual("Use system setting", ((ComboBoxItem)themeMode.SelectedItem).Content);
                    Assert.AreEqual(ApplicationTheme.Dark, ThemeManager.Current.ApplicationTheme);
                }
                finally
                {
                    ThemeManager.Current.ApplicationTheme = previousTheme;
                }
            });
        }

        [TestMethod]
        public void SettingsPageVisualTestThemeSelectionMatchesRequestedThemeWithoutApplyingSelection()
        {
            WpfTestHost.Run(() =>
            {
                var previousTheme = ThemeManager.Current.ApplicationTheme;
                try
                {
                    GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--theme", "Light" }));
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

                    var lightPage = new SettingsPage();
                    var lightThemeMode = (ComboBox)lightPage.FindName("Change_ThemeMode");

                    Assert.AreEqual(0, lightThemeMode.SelectedIndex);
                    Assert.AreEqual("Light", ((ComboBoxItem)lightThemeMode.SelectedItem).Content);
                    Assert.AreEqual(ApplicationTheme.Light, ThemeManager.Current.ApplicationTheme);

                    GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test", "--theme", "Dark" }));
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

                    var darkPage = new SettingsPage();
                    var darkThemeMode = (ComboBox)darkPage.FindName("Change_ThemeMode");

                    Assert.AreEqual(1, darkThemeMode.SelectedIndex);
                    Assert.AreEqual("Dark", ((ComboBoxItem)darkThemeMode.SelectedItem).Content);
                    Assert.AreEqual(ApplicationTheme.Dark, ThemeManager.Current.ApplicationTheme);
                }
                finally
                {
                    ThemeManager.Current.ApplicationTheme = previousTheme;
                    GalleryDiagnostics.ResetForTests();
                }
            });
        }

        [TestMethod]
        public void SettingsPageThemeSelectionAppliesModernWpfTheme()
        {
            WpfTestHost.Run(() =>
            {
                var previousTheme = ThemeManager.Current.ApplicationTheme;
                try
                {
                    ThemeManager.Current.ApplicationTheme = null;

                    var page = new SettingsPage();
                    var themeMode = (ComboBox)page.FindName("Change_ThemeMode");

                    themeMode.SelectedIndex = 0;
                    Assert.AreEqual(ApplicationTheme.Light, ThemeManager.Current.ApplicationTheme);

                    themeMode.SelectedIndex = 1;
                    Assert.AreEqual(ApplicationTheme.Dark, ThemeManager.Current.ApplicationTheme);

                    themeMode.SelectedIndex = 2;
                    Assert.IsNull(ThemeManager.Current.ApplicationTheme);
                }
                finally
                {
                    ThemeManager.Current.ApplicationTheme = previousTheme;
                    GalleryDiagnostics.ResetForTests();
                }
            });
        }

        [TestMethod]
        public void WhatsNewPageUsesTargetAwareResourceEntry()
        {
            WpfTestHost.Run(() =>
            {
                var page = new WhatsNewPage();
                RenderPage(page);

                var resourceExample = FindDescendants<ControlExample>(page).Single();
                Assert.AreEqual("Application resources", resourceExample.HeaderText);
                StringAssert.Contains(resourceExample.XamlCode, "<ui:ThemeResources />");
                StringAssert.Contains(
                    resourceExample.XamlCode,
                    "<ui:FluentControlsResources UseCompactResources=\"False\" />");
                Assert.IsFalse(resourceExample.XamlCode.Contains("<ui:XamlControlsResources"));

                var resourceLabels = FindDescendants<TextBlock>((DependencyObject)resourceExample.ExampleContent)
                    .Select(textBlock => textBlock.Text)
                    .ToArray();
                CollectionAssert.Contains(resourceLabels, "ThemeResources");
                CollectionAssert.Contains(resourceLabels, "FluentControlsResources");
            });
        }

        [TestMethod]
        public void CollectionsPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertCollectionsViewModel<DataGridPage, DataGridPageViewModel>("DataGrid", "DataGrid");
                AssertCollectionsViewModel<ListBoxPage, ListBoxPageViewModel>("ListBox", "ListBox");
                AssertCollectionsViewModel<ListViewPage, ListViewPageViewModel>("ListView", "ListView");
                AssertCollectionsViewModel<TreeViewPage, TreeViewPageViewModel>("TreeView", "TreeView");

                var dataGridPage = (DataGridPage)new ItemPage(GalleryCatalog.FindItem("DataGrid")).DirectPageContent;
                Assert.AreEqual(50, dataGridPage.ViewModel.ProductsCollection.Count);
                var changedProperties = new List<string>();
                dataGridPage.ViewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
                dataGridPage.ViewModel.PageTitle = "Data Grids";
                Assert.AreEqual("Data Grids", dataGridPage.ViewModel.PageTitle);
                CollectionAssert.Contains(changedProperties, nameof(DataGridPageViewModel.PageTitle));
                var sampleDataGrid = (DataGrid)dataGridPage.FindName("SampleDataGrid");
                dataGridPage.ApplyPageVisuals(true);
                Assert.AreSame(SystemColors.ControlBrush, sampleDataGrid.Background);
                Assert.AreSame(SystemColors.ControlTextBrush, sampleDataGrid.Foreground);
                dataGridPage.ApplyPageVisuals(false);
                Assert.AreNotSame(SystemColors.ControlBrush, sampleDataGrid.Background);
                Assert.AreSame(Application.Current.FindResource("TextFillColorPrimaryBrush"), sampleDataGrid.Foreground);

                var listBoxPage = (ListBoxPage)new ItemPage(GalleryCatalog.FindItem("ListBox")).DirectPageContent;
                CollectionAssert.AreEqual(
                    new[] { "Arial", "Comic Sans MS", "Courier New", "Segoe UI", "Times New Roman" },
                    listBoxPage.ViewModel.ListBoxItems.ToArray());

                var listViewPage = (ListViewPage)new ItemPage(GalleryCatalog.FindItem("ListView")).DirectPageContent;
                Assert.AreEqual(50, listViewPage.ViewModel.BasicListViewItems.Count);
                Assert.AreEqual(50, listViewPage.ViewModel.GridViewItems.Count);
                changedProperties.Clear();
                listViewPage.ViewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
                listViewPage.ViewModel.ListViewSelectionModeComboBoxSelectedIndex = 1;
                Assert.AreEqual(SelectionMode.Multiple, listViewPage.ViewModel.ListViewSelectionMode);
                CollectionAssert.Contains(changedProperties, nameof(ListViewPageViewModel.ListViewSelectionModeComboBoxSelectedIndex));
                CollectionAssert.Contains(changedProperties, nameof(ListViewPageViewModel.ListViewSelectionMode));
                listViewPage.ViewModel.ListViewSelectionModeComboBoxSelectedIndex = 2;
                Assert.AreEqual(SelectionMode.Extended, listViewPage.ViewModel.ListViewSelectionMode);
            });
        }

        [TestMethod]
        public void CollectionsItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<DataGridPage>("DataGrid");
                AssertWpfGalleryPageRoot<ListBoxPage>("ListBox");
                AssertWpfGalleryPageRoot<ListViewPage>("ListView");
                AssertWpfGalleryPageRoot<TreeViewPage>("TreeView");
            });
        }

        [TestMethod]
        public void VisualTestModeUsesDeterministicWpfGallerySampleData()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));

                try
                {
                    var firstDataGrid = new DataGridPageViewModel();
                    var secondDataGrid = new DataGridPageViewModel();
                    CollectionAssert.AreEqual(
                        ProductSignatures(firstDataGrid.ProductsCollection.Take(8)),
                        ProductSignatures(secondDataGrid.ProductsCollection.Take(8)));

                    var firstListView = new ListViewPageViewModel();
                    var secondListView = new ListViewPageViewModel();
                    CollectionAssert.AreEqual(
                        PersonSignatures(firstListView.BasicListViewItems.Take(8)),
                        PersonSignatures(secondListView.BasicListViewItems.Take(8)));
                    CollectionAssert.AreEqual(
                        PersonSignatures(firstListView.GridViewItems.Take(8)),
                        PersonSignatures(secondListView.GridViewItems.Take(8)));
                    Assert.IsFalse(
                        PersonSignatures(firstListView.BasicListViewItems.Take(8))
                            .SequenceEqual(PersonSignatures(firstListView.GridViewItems.Take(8))),
                        "Basic ListView and GridView samples should stay independently generated in visual-test mode.");

                    var firstDashboard = new UserDashboardPageViewModel();
                    var secondDashboard = new UserDashboardPageViewModel();
                    CollectionAssert.AreEqual(
                        UserDashboardSignatures(firstDashboard.Users.Take(8)),
                        UserDashboardSignatures(secondDashboard.Users.Take(8)));
                }
                finally
                {
                    GalleryDiagnostics.ResetForTests();
                }
            });
        }

        [TestMethod]
        public void SimpleItemPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageViewModel<CalendarPage, CalendarPageViewModel>("Calendar", "Calendar", string.Empty);
                AssertWpfGalleryPageViewModel<DatePickerPage, DatePickerPageViewModel>("DatePicker", "DatePicker", string.Empty);
                AssertWpfGalleryPageViewModel<ProgressBarPage, ProgressBarPageViewModel>("ProgressBar", "ProgressBar", string.Empty);
                AssertWpfGalleryPageViewModel<ToolTipPage, ToolTipPageViewModel>("ToolTip", "ToolTip", string.Empty);
            });
        }

        [TestMethod]
        public void SimpleItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<CalendarPage>("Calendar");
                AssertWpfGalleryPageRoot<DatePickerPage>("DatePicker", "DatePicker");
                AssertWpfGalleryPageRoot<ProgressBarPage>("ProgressBar");
                AssertWpfGalleryPageRoot<ToolTipPage>("ToolTip");
            });
        }

        [TestMethod]
        public void LayoutPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageViewModel<BorderPage, BorderPageViewModel>("Border", "Border", string.Empty);
                AssertWpfGalleryPageViewModel<ExpanderPage, ExpanderPageViewModel>("Expander", "Expander", string.Empty);
                AssertWpfGalleryPageViewModel<GridPage, GridPageViewModel>("Grid", "Grid", string.Empty);
                AssertWpfGalleryPageViewModel<GridSplitterPage, GridSplitterPageViewModel>("GridSplitter", "GridSplitter", string.Empty);
                AssertWpfGalleryPageViewModel<GroupBoxPage, GroupBoxPageViewModel>("GroupBox", "GroupBox", string.Empty);
                AssertWpfGalleryPageViewModel<ResizeGripPage, ResizeGripPageViewModel>("ResizeGrip", "ResizeGrip", string.Empty);
                AssertWpfGalleryPageViewModel<StackPanelPage, StackPanelPageViewModel>("StackPanel", "StackPanel", string.Empty);
            });
        }

        [TestMethod]
        public void LayoutItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<BorderPage>("Border");
                AssertWpfGalleryPageRoot<ExpanderPage>("Expander");
                AssertWpfGalleryPageRoot<GridPage>("Grid");
                AssertWpfGalleryPageRoot<GridSplitterPage>("GridSplitter");
                AssertWpfGalleryPageRoot<GroupBoxPage>("GroupBox");
                AssertWpfGalleryPageRoot<ResizeGripPage>("ResizeGrip");
                AssertWpfGalleryPageRoot<StackPanelPage>("StackPanel");
            });
        }

        [TestMethod]
        public void TextPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageViewModel<LabelPage, LabelPageViewModel>("Label", "Label", string.Empty);
                AssertWpfGalleryPageViewModel<TextBoxPage, TextBoxPageViewModel>("TextBox", "TextBox", string.Empty);
                AssertWpfGalleryPageViewModel<TextBlockPage, TextBlockPageViewModel>("TextBlock", "TextBlock", string.Empty);
                AssertWpfGalleryPageViewModel<HyperlinkPage, HyperlinkPageViewModel>("Hyperlink", "Hyperlink", string.Empty);
                AssertWpfGalleryPageViewModel<RichTextEditPage, RichTextEditPageViewModel>("RichTextBox", "RichTextBox", string.Empty, "RichTextEditPageViewModel");
                AssertWpfGalleryPageViewModel<PasswordBoxPage, PasswordBoxPageViewModel>("PasswordBox", "PasswordBox", string.Empty);

                var textBoxPage = (TextBoxPage)new ItemPage(GalleryCatalog.FindItem("TextBox")).DirectPageContent;
                Assert.AreEqual(string.Empty, textBoxPage.ViewModel.ValidatedText);
                var changedProperties = new List<string>();
                textBoxPage.ViewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
                textBoxPage.ViewModel.ValidatedText = "abc";
                Assert.AreEqual("abc", textBoxPage.ViewModel.ValidatedText);
                CollectionAssert.Contains(changedProperties, "ValidatedText");
            });
        }

        [TestMethod]
        public void TextItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<LabelPage>("Label");
                AssertWpfGalleryPageRoot<TextBoxPage>("TextBox");
                AssertWpfGalleryPageRoot<TextBlockPage>("TextBlock");
                AssertWpfGalleryPageRoot<HyperlinkPage>("Hyperlink");
                AssertWpfGalleryPageRoot<RichTextEditPage>("RichTextBox", "RichTextBoxPage");
                AssertWpfGalleryPageRoot<PasswordBoxPage>("PasswordBox");
            });
        }

        [TestMethod]
        public void NavigationPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageViewModel<MenuPage, MenuPageViewModel>("Menu", "Menu", string.Empty);
                AssertWpfGalleryPageViewModel<TabControlPage, TabControlPageViewModel>("TabControl", "TabControl", string.Empty);
                AssertWpfGalleryPageViewModel<FramePage, FramePageViewModel>("Frame", "Frame", string.Empty);
                AssertWpfGalleryPageViewModel<NavigationWindowPage, NavigationWindowPageViewModel>("NavigationWindow", "Navigation Window", string.Empty);
            });
        }

        [TestMethod]
        public void NavigationItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<MenuPage>("Menu");
                AssertWpfGalleryPageRoot<TabControlPage>("TabControl");
                AssertWpfGalleryPageRoot<FramePage>("Frame");
                AssertWpfGalleryPageRoot<NavigationWindowPage>("NavigationWindow");
            });
        }

        [TestMethod]
        public void DesignGuidancePagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageViewModel<ColorsPage, ColorsPageViewModel>(
                    "Color",
                    "Colors",
                    "Guide showing how to use colors in your app",
                    "ColorsPageViewModel");
                AssertWpfGalleryPageViewModel<TypographyPage, TypographyPageViewModel>(
                    "Typography",
                    "Typography",
                    "Guide showing how to use typography in your app");
                AssertWpfGalleryPageViewModel<SpacingPage, SpacingPageViewModel>(
                    "Spacing",
                    "Spacing",
                    "Guide showing how to use spacing in your app");
                AssertWpfGalleryPageViewModel<GeometryPage, GeometryPageViewModel>("Geometry", "Geometry", string.Empty);

                var iconographyPage = (IconsPage)new ItemPage(GalleryCatalog.FindItem("Iconography")).DirectPageContent;
                Assert.IsInstanceOfType(iconographyPage.ViewModel, typeof(IconsPageViewModel));
                Assert.AreEqual("Icons", iconographyPage.ViewModel.PageTitle);
                Assert.AreEqual("Guide showing how to use icons in your application.", iconographyPage.ViewModel.PageDescription);
                var changedProperties = new List<string>();
                iconographyPage.ViewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
                iconographyPage.ViewModel.LoadDataCommand.Execute(null);
                Assert.IsTrue(iconographyPage.ViewModel.AllIcons.Count > 0);
                Assert.IsTrue(iconographyPage.ViewModel.SearchFilteredIcons.Count > 0);
                iconographyPage.ViewModel.ApplyTagFilterCommand.Execute("clipboard");
                Assert.AreEqual("clipboard", iconographyPage.ViewModel.SearchText);
                Assert.IsTrue(changedProperties.Contains("SearchText"));
            });
        }

        [TestMethod]
        public void IconographyWhitespaceSearchUsesWpfGalleryFilterValue()
        {
            var viewModel = new IconsPageViewModel();
            viewModel.AllIcons = new List<IconData>
            {
                new IconData { Name = "NoSpaceTag", Code = "E700", Tags = new List<string> { "plain" } },
                new IconData { Name = "SpaceTag", Code = "E701", Tags = new List<string> { "has space" } }
            };
            viewModel.SearchFilteredIcons = new System.Collections.ObjectModel.ObservableCollection<IconData>(viewModel.AllIcons);
            viewModel.DisplayedIcons = new System.Collections.ObjectModel.ObservableCollection<IconData>(viewModel.AllIcons);
            viewModel.SelectedIcon = viewModel.AllIcons.First();

            viewModel.SearchText = " ";

            Assert.AreEqual(1, viewModel.SearchFilteredIcons.Count);
            Assert.AreEqual("SpaceTag", viewModel.SearchFilteredIcons[0].Name);
            Assert.AreEqual("SpaceTag", viewModel.DisplayedIcons[0].Name);
            Assert.AreEqual("SpaceTag", viewModel.SelectedIcon.Name);
        }

        [TestMethod]
        public void IconographyReloadKeepsWpfGalleryCurrentPage()
        {
            var viewModel = new IconsPageViewModel();
            viewModel.LoadDataCommand.Execute(null);
            Assert.IsTrue(viewModel.NextPageCommand.CanExecute(null));

            viewModel.NextPageCommand.Execute(null);
            Assert.AreEqual(2, viewModel.CurrentPage);

            viewModel.LoadDataCommand.Execute(null);

            Assert.AreEqual(2, viewModel.CurrentPage);
        }

        [TestMethod]
        public void DesignGuidanceItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<ColorsPage>("Color", "ColorsPage");
                AssertWpfGalleryPageRoot<IconsPage>("Iconography", "IconsPage");
                AssertWpfGalleryPageRoot<TypographyPage>("Typography");
                AssertWpfGalleryPageRoot<SpacingPage>("Spacing");
                AssertWpfGalleryPageRoot<GeometryPage>("Geometry");
            });
        }

        [TestMethod]
        public void SourceInitFirstCopiedPagesRenderInjectedViewModelBindings()
        {
            WpfTestHost.Run(() =>
            {
                var colorsViewModel = new ColorsPageViewModel();
                AssertRenderedPageHeader(
                    new ColorsPage(colorsViewModel),
                    colorsViewModel,
                    colorsViewModel.PageTitle,
                    colorsViewModel.PageDescription);

                var iconographyViewModel = new IconsPageViewModel();
                AssertRenderedPageHeader(
                    new IconsPage(iconographyViewModel),
                    iconographyViewModel,
                    iconographyViewModel.PageTitle,
                    iconographyViewModel.PageDescription);

                var typographyViewModel = new TypographyPageViewModel();
                AssertRenderedPageHeader(
                    new TypographyPage(typographyViewModel),
                    typographyViewModel,
                    typographyViewModel.PageTitle,
                    typographyViewModel.PageDescription);

                var spacingViewModel = new SpacingPageViewModel();
                AssertRenderedPageHeader(
                    new SpacingPage(spacingViewModel),
                    spacingViewModel,
                    spacingViewModel.PageTitle,
                    spacingViewModel.PageDescription);

                var geometryViewModel = new GeometryPageViewModel();
                AssertRenderedPageHeader(
                    new GeometryPage(geometryViewModel),
                    geometryViewModel,
                    geometryViewModel.PageTitle,
                    geometryViewModel.PageDescription);

                var userDashboardViewModel = new UserDashboardPageViewModel();
                var userDashboardPage = new UserDashboardPage(userDashboardViewModel);
                RenderPage(userDashboardPage);
                Assert.AreSame(userDashboardViewModel, userDashboardPage.ViewModel);
                Assert.AreSame(userDashboardPage, userDashboardPage.DataContext);
                var userList = (ListView)userDashboardPage.FindName("UserList");
                Assert.AreSame(userDashboardViewModel.Users, userList.ItemsSource);
            });
        }

        [TestMethod]
        public void ListViewPageExamplesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ListView"));
                WithRenderedPage(page, () =>
                {
                    Assert.IsTrue(page.HasDirectPageContent);
                    var examples = GetRenderedExamples(page);

                    Assert.AreEqual(3, examples.Count);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "Basic ListView with Simple DataTemplate.",
                            "ListView with Selection Support.",
                            "ListView with GridView."
                        },
                        examples.Select(example => example.HeaderText).ToArray());

                    var basicListView = (ListView)examples[0].ExampleContent;
                    Assert.AreEqual(200.0, basicListView.Height);
                    Assert.AreEqual(2, basicListView.SelectedIndex);
                    Assert.AreEqual(SelectionMode.Single, basicListView.SelectionMode);
                    Assert.IsNotNull(basicListView.ItemTemplate);
                    StringAssert.Contains(examples[0].XamlCode, "ViewModel.BasicListViewItems, Mode=TwoWay");
                    StringAssert.Contains(examples[0].XamlCode, "Text=\"{Binding Name, Mode=OneWay}\"");

                    var selectionGrid = (Grid)examples[1].ExampleContent;
                    Assert.AreEqual(2, selectionGrid.ColumnDefinitions.Count);
                    Assert.AreEqual(GridUnitType.Star, selectionGrid.ColumnDefinitions[0].Width.GridUnitType);
                    Assert.AreEqual(GridUnitType.Auto, selectionGrid.ColumnDefinitions[1].Width.GridUnitType);

                    var selectionListView = selectionGrid.Children.OfType<ListView>().Single();
                    Assert.AreEqual(200.0, selectionListView.Height);
                    Assert.AreEqual(1, selectionListView.SelectedIndex);
                    Assert.AreEqual(SelectionMode.Single, selectionListView.SelectionMode);
                    Assert.IsNotNull(selectionListView.ItemTemplate);

                    var templateGrid = (Grid)selectionListView.ItemTemplate.LoadContent();
                    Assert.AreEqual(new Thickness(8, 0, 8, 0), templateGrid.Margin);
                    Assert.AreEqual(2, templateGrid.RowDefinitions.Count);
                    Assert.AreEqual(2, templateGrid.ColumnDefinitions.Count);

                    var ellipse = templateGrid.Children.OfType<Ellipse>().Single();
                    Assert.AreEqual(2, Grid.GetRowSpan(ellipse));
                    Assert.AreEqual(32.0, ellipse.Width);
                    Assert.AreEqual(32.0, ellipse.Height);
                    Assert.AreEqual(new Thickness(6), ellipse.Margin);

                    var textBlocks = templateGrid.Children.OfType<TextBlock>().ToArray();
                    Assert.AreEqual(2, textBlocks.Length);
                    Assert.AreEqual(0, Grid.GetRow(textBlocks[0]));
                    Assert.AreEqual(1, Grid.GetColumn(textBlocks[0]));
                    Assert.AreSame(Application.Current.FindResource("BodyStrongTextBlockStyle"), textBlocks[0].Style);
                    Assert.AreEqual(DependencyProperty.UnsetValue, textBlocks[0].ReadLocalValue(TextBlock.FontWeightProperty));
                    Assert.AreEqual(1, Grid.GetRow(textBlocks[1]));
                    Assert.AreEqual(1, Grid.GetColumn(textBlocks[1]));
                    Assert.AreEqual(0.7, textBlocks[1].Opacity, 0.001);

                    var controls = selectionGrid.Children.OfType<StackPanel>().Single();
                    Assert.AreEqual(120.0, controls.MinWidth);
                    Assert.AreEqual(new Thickness(12, 0, 0, 0), controls.Margin);
                    Assert.AreEqual(VerticalAlignment.Top, controls.VerticalAlignment);

                    var label = (Label)controls.Children[0];
                    var comboBox = (ComboBox)controls.Children[1];
                    Assert.AreEqual("Selection mode", label.Content);
                    Assert.AreSame(comboBox, label.Target);
                    Assert.AreEqual(0.7, label.Opacity, 0.001);
                    Assert.AreEqual("Selection Mode", AutomationProperties.GetName(comboBox));
                    CollectionAssert.AreEqual(
                        new[] { "Single", "Multiple", "Extended" },
                        comboBox.Items.Cast<ComboBoxItem>().Select(item => (string)item.Content).ToArray());

                    comboBox.SelectedIndex = 1;
                    Assert.AreEqual(SelectionMode.Multiple, selectionListView.SelectionMode);
                    comboBox.SelectedIndex = 2;
                    Assert.AreEqual(SelectionMode.Extended, selectionListView.SelectionMode);

                    StringAssert.Contains(examples[1].XamlCode, "<Grid.RowDefinitions>");
                    StringAssert.Contains(examples[1].XamlCode, "FontWeight=\"Bold\"");
                    StringAssert.Contains(examples[1].XamlCode, "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"");

                    var gridViewListView = (ListView)examples[2].ExampleContent;
                    Assert.AreEqual(280.0, gridViewListView.Height);
                    var gridView = (GridView)gridViewListView.View;
                    Assert.AreEqual(3, gridView.Columns.Count);
                    AssertGridViewColumn(gridView.Columns[0], "First Name", 150.0, "FirstName");
                    AssertGridViewColumn(gridView.Columns[1], "Last Name", 150.0, "LastName");
                    AssertGridViewColumn(gridView.Columns[2], "Company", 200.0, "Company");
                });
            });
        }

        [TestMethod]
        public void WpfGalleryExampleMarginsMatchReferencePages()
        {
            WpfTestHost.Run(() =>
            {
                AssertExampleMargins("Button", new Thickness(10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("CheckBox", new Thickness(10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("ComboBox", new Thickness(10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("Slider", new Thickness(10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("RadioButton", new Thickness(10), new Thickness(10, 36, 10, 10));
                AssertExampleMargins("ListView", new Thickness(10), new Thickness(10, 36, 10, 10), new Thickness(10, 36, 10, 10));
                AssertExampleMargins("TextBlock", new Thickness(10), new Thickness(10, 36, 10, 10), new Thickness(10, 36, 10, 10), new Thickness(10, 36, 10, 10));
                AssertExampleMargins("TextBox", new Thickness(10), new Thickness(10, 36, 10, 10), new Thickness(10, 36, 10, 10));
                AssertExampleMargins("Label", new Thickness(10), new Thickness(10, 36, 10, 10));
                AssertExampleMargins("Border", new Thickness(10), new Thickness(10), new Thickness(10));
                AssertExampleMargins("Grid", new Thickness(10), new Thickness(10), new Thickness(10));
                AssertExampleMargins("StackPanel", new Thickness(10), new Thickness(10));
                AssertExampleMargins("Expander", new Thickness(10));
                AssertExampleMargins("GridSplitter", new Thickness(10));
                AssertExampleMargins("GroupBox", new Thickness(10));
                AssertExampleMargins("ResizeGrip", new Thickness(10));
                AssertExampleMargins("Calendar", new Thickness(10));
                AssertExampleMargins("DatePicker", new Thickness(10));
                AssertExampleMargins("ProgressBar", new Thickness(10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("ToolTip", new Thickness(10));
                AssertExampleMargins("Menu", new Thickness(10));
                AssertExampleMargins("Frame", new Thickness(10));
                AssertExampleMargins("NavigationWindow", new Thickness(10));
                AssertExampleMargins("TabControl", new Thickness(10));
                AssertExampleMargins("ListBox", new Thickness(10), new Thickness(10, 36, 10, 10));
                AssertExampleMargins("TreeView", new Thickness(10));
                AssertExampleMargins("DataGrid", new Thickness(10));
                AssertExampleMargins("Hyperlink", new Thickness(10));
            });
        }

        [TestMethod]
        public void BasicInputPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertBasicInputViewModel<ButtonPage, ButtonPageViewModel>("Button", "Button");
                AssertBasicInputViewModel<CheckBoxPage, CheckBoxPageViewModel>("CheckBox", "CheckBox");
                AssertBasicInputViewModel<ComboBoxPage, ComboBoxPageViewModel>("ComboBox", "ComboBox");
                AssertBasicInputViewModel<RadioButtonPage, RadioButtonPageViewModel>("RadioButton", "RadioButton");
                AssertBasicInputViewModel<SliderPage, SliderPageViewModel>("Slider", "Slider");

                var buttonPage = (ButtonPage)new ItemPage(GalleryCatalog.FindItem("Button")).DirectPageContent;
                var changedProperties = new List<string>();
                buttonPage.ViewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
                buttonPage.ViewModel.PageTitle = "Buttons";
                buttonPage.ViewModel.Message = "Clicked";
                Assert.AreEqual("Buttons", buttonPage.ViewModel.PageTitle);
                Assert.AreEqual("Clicked", buttonPage.ViewModel.Message);
                CollectionAssert.Contains(changedProperties, nameof(ButtonPageViewModel.PageTitle));
                CollectionAssert.Contains(changedProperties, nameof(ButtonPageViewModel.Message));
            });
        }

        [TestMethod]
        public void BasicInputItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<ButtonPage>("Button", "Button");
                AssertWpfGalleryPageRoot<CheckBoxPage>("CheckBox", "CheckBox");
                AssertWpfGalleryPageRoot<ComboBoxPage>("ComboBox", "ComboBox");
                AssertWpfGalleryPageRoot<RadioButtonPage>("RadioButton", "RadioButton");
                AssertWpfGalleryPageRoot<SliderPage>("Slider");
            });
        }

        [TestMethod]
        public void BasicInputButtonAndCheckBoxPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var buttonPage = new ItemPage(GalleryCatalog.FindItem("Button"));
                WithRenderedPage(buttonPage, () =>
                {
                    Assert.IsTrue(buttonPage.HasDirectPageContent);
                    var buttonExamples = GetRenderedExamples(buttonPage);
                    Assert.AreEqual(2, buttonExamples.Count);
                    CollectionAssert.AreEqual(
                        new[] { "Simple Button", "WPF Accent Button" },
                        buttonExamples.Select(example => example.HeaderText).ToArray());

                    var simpleGrid = (Grid)buttonExamples[0].ExampleContent;
                    Assert.AreEqual(2, simpleGrid.ColumnDefinitions.Count);
                    Assert.AreEqual(GridUnitType.Star, simpleGrid.ColumnDefinitions[0].Width.GridUnitType);
                    Assert.AreEqual(GridUnitType.Auto, simpleGrid.ColumnDefinitions[1].Width.GridUnitType);

                    var simpleButton = (Button)simpleGrid.Children[0];
                    var disableButton = (CheckBox)simpleGrid.Children[1];
                    Assert.AreEqual("Standard WPF button", simpleButton.Content);
                    Assert.AreEqual("Standard WPF", AutomationProperties.GetName(simpleButton));
                    Assert.AreEqual("GallerySample_Button_Root", buttonExamples[0].AutomationId);
                    Assert.AreEqual("GallerySample_Button_PrimaryButton", AutomationProperties.GetAutomationId(simpleButton));
                    Assert.AreEqual("Disable button", disableButton.Content);
                    Assert.AreEqual(1, Grid.GetColumn(disableButton));
                    disableButton.IsChecked = true;
                    disableButton.Command.Execute(disableButton.CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(simpleButton.IsEnabled);
                    disableButton.IsChecked = false;
                    disableButton.Command.Execute(disableButton.CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(simpleButton.IsEnabled);

                    var accentButton = (Button)buttonExamples[1].ExampleContent;
                    Assert.AreEqual("WPF Accent", AutomationProperties.GetName(accentButton));
                    var accentContent = (StackPanel)accentButton.Content;
                    Assert.AreEqual(Orientation.Horizontal, accentContent.Orientation);
                    Assert.AreEqual("WPF Accent Button", ((TextBlock)accentContent.Children[0]).Text);
                });

                var checkBoxPage = new ItemPage(GalleryCatalog.FindItem("CheckBox"));
                WithRenderedPage(checkBoxPage, () =>
                {
                    Assert.IsTrue(checkBoxPage.HasDirectPageContent);
                    var checkBoxExamples = GetRenderedExamples(checkBoxPage);
                    Assert.AreEqual(3, checkBoxExamples.Count);
                    CollectionAssert.AreEqual(
                        new[] { "A 2-state CheckBox.", "A 3-state CheckBox.", "Using a 3-state CheckBox." },
                        checkBoxExamples.Select(example => example.HeaderText).ToArray());

                    var twoState = (CheckBox)checkBoxExamples[0].ExampleContent;
                    Assert.AreEqual("Two-state CheckBox", twoState.Content);
                    Assert.IsFalse(twoState.IsThreeState);
                    Assert.AreEqual(false, twoState.IsChecked);
                    Assert.AreEqual("Sample Two State", AutomationProperties.GetName(twoState));
                    Assert.AreEqual("GallerySample_CheckBox_Root", checkBoxExamples[0].AutomationId);
                    Assert.AreEqual("GallerySample_CheckBox_CheckBox", AutomationProperties.GetAutomationId(twoState));

                    var threeState = (CheckBox)checkBoxExamples[1].ExampleContent;
                    Assert.AreEqual("Three-state CheckBox", threeState.Content);
                    Assert.IsTrue(threeState.IsThreeState);
                    Assert.IsNull(threeState.IsChecked);
                    Assert.AreEqual("Sample Three State", AutomationProperties.GetName(threeState));

                    var group = (StackPanel)checkBoxExamples[2].ExampleContent;
                    Assert.AreEqual(4, group.Children.Count);
                    var selectAll = (CheckBox)group.Children[0];
                    var options = group.Children.OfType<CheckBox>().Skip(1).ToArray();
                    Assert.AreEqual("Select all", selectAll.Content);
                    Assert.IsTrue(selectAll.IsThreeState);
                    Assert.IsNull(selectAll.IsChecked);
                    CollectionAssert.AreEqual(new[] { "Option 1", "Option 2", "Option 3" }, options.Select(option => (string)option.Content).ToArray());
                    CollectionAssert.AreEqual(Enumerable.Repeat(new Thickness(24, 0, 0, 0), 3).ToArray(), options.Select(option => option.Margin).ToArray());
                    CollectionAssert.AreEqual(new bool?[] { false, true, false }, options.Select(option => option.IsChecked).ToArray());

                    options[0].IsChecked = true;
                    options[0].Command.Execute(options[0].CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.IsNull(selectAll.IsChecked);
                    options[2].IsChecked = true;
                    options[2].Command.Execute(options[2].CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(true, selectAll.IsChecked);
                    options[1].IsChecked = false;
                    options[1].Command.Execute(options[1].CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.IsNull(selectAll.IsChecked);
                    selectAll.IsChecked = false;
                    selectAll.Command.Execute(selectAll.CommandParameter);
                    WpfTestHost.DoEvents();
                    CollectionAssert.AreEqual(new bool?[] { false, false, false }, options.Select(option => option.IsChecked).ToArray());
                });
            });
        }

        [TestMethod]
        public void BasicInputComboBoxRadioButtonAndSliderPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var comboBoxPage = new ItemPage(GalleryCatalog.FindItem("ComboBox"));
                WithRenderedPage(comboBoxPage, () =>
                {
                    Assert.IsTrue(comboBoxPage.HasDirectPageContent);
                    var comboBoxExamples = GetRenderedExamples(comboBoxPage);
                    Assert.AreEqual(3, comboBoxExamples.Count);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "A ComboBox with items defined inline.",
                            "A ComboBox with ItemsSource set.",
                            "An editable ComboBox."
                        },
                        comboBoxExamples.Select(example => example.HeaderText).ToArray());

                    var inlineComboBox = (ComboBox)comboBoxExamples[0].ExampleContent;
                    AssertGalleryComboBox(inlineComboBox, "Sample defined inline");
                    Assert.AreEqual("GallerySample_ComboBox_Root", comboBoxExamples[0].AutomationId);
                    Assert.AreEqual("GallerySample_ComboBox_ComboBox", AutomationProperties.GetAutomationId(inlineComboBox));
                    CollectionAssert.AreEqual(
                        new[] { "Blue", "Green", "Red", "Yellow" },
                        inlineComboBox.Items.Cast<ComboBoxItem>().Select(item => (string)item.Content).ToArray());
                    Assert.AreEqual(0, inlineComboBox.SelectedIndex);

                    var fontFamilyComboBox = (ComboBox)comboBoxExamples[1].ExampleContent;
                    AssertGalleryComboBox(fontFamilyComboBox, "Sample item source set");
                    CollectionAssert.AreEqual(
                        new[] { "Arial", "Comic Sans MS", "Segoe UI", "Times New Roman" },
                        fontFamilyComboBox.ItemsSource.Cast<string>().ToArray());
                    Assert.IsNotNull(fontFamilyComboBox.ItemTemplate);
                    Assert.AreEqual(0, fontFamilyComboBox.SelectedIndex);

                    var editableComboBox = (ComboBox)comboBoxExamples[2].ExampleContent;
                    AssertGalleryComboBox(editableComboBox, "Editable");
                    Assert.IsTrue(editableComboBox.IsEditable);
                    CollectionAssert.AreEqual(
                        new[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 36, 48, 72 },
                        editableComboBox.ItemsSource.Cast<int>().ToArray());
                    Assert.AreEqual(0, editableComboBox.SelectedIndex);
                });

                var radioButtonPage = new ItemPage(GalleryCatalog.FindItem("RadioButton"));
                WithRenderedPage(radioButtonPage, () =>
                {
                    Assert.IsTrue(radioButtonPage.HasDirectPageContent);
                    var radioButtonExamples = GetRenderedExamples(radioButtonPage);
                    Assert.AreEqual(2, radioButtonExamples.Count);
                    CollectionAssert.AreEqual(
                        new[] { "Standard RadioButton.", "RadioButton with right to left flow direction." },
                        radioButtonExamples.Select(example => example.HeaderText).ToArray());

                    var radioGrid = (Grid)radioButtonExamples[0].ExampleContent;
                    Assert.AreEqual(2, radioGrid.ColumnDefinitions.Count);
                    var defaultRadioStack = (StackPanel)radioGrid.Children[0];
                    Assert.AreEqual(KeyboardNavigationMode.Once, KeyboardNavigation.GetTabNavigation(defaultRadioStack));
                    Assert.AreEqual(KeyboardNavigationMode.Cycle, KeyboardNavigation.GetDirectionalNavigation(defaultRadioStack));
                    var defaultRadios = defaultRadioStack.Children.OfType<RadioButton>().ToArray();
                    AssertRadioButtons(defaultRadios, "Default", "radio_group_one", FlowDirection.LeftToRight);
                    Assert.AreEqual("GallerySample_RadioButton_Root", radioButtonExamples[0].AutomationId);
                    Assert.AreEqual("GallerySample_RadioButton_RadioButton", AutomationProperties.GetAutomationId(defaultRadios[0]));

                    var disableRadioButtons = (CheckBox)radioGrid.Children[1];
                    Assert.AreEqual(1, Grid.GetColumn(disableRadioButtons));
                    Assert.AreEqual("Disable RadioButton's", disableRadioButtons.Content);
                    disableRadioButtons.IsChecked = true;
                    disableRadioButtons.Command.Execute(disableRadioButtons.CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(defaultRadios.All(radioButton => !radioButton.IsEnabled));
                    disableRadioButtons.IsChecked = false;
                    disableRadioButtons.Command.Execute(disableRadioButtons.CommandParameter);
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(defaultRadios.All(radioButton => radioButton.IsEnabled));

                    RaiseGotKeyboardFocus(defaultRadios[1]);
                    Assert.AreEqual(true, defaultRadios[1].IsChecked);
                    Assert.AreEqual(false, defaultRadios[0].IsChecked);

                    var leftFlowStack = (StackPanel)radioButtonExamples[1].ExampleContent;
                    Assert.AreEqual(KeyboardNavigationMode.Once, KeyboardNavigation.GetTabNavigation(leftFlowStack));
                    Assert.AreEqual(KeyboardNavigationMode.Cycle, KeyboardNavigation.GetDirectionalNavigation(leftFlowStack));
                    AssertRadioButtons(leftFlowStack.Children.OfType<RadioButton>().ToArray(), "Left Flow", "radio_group_two", FlowDirection.RightToLeft);
                });

                var sliderPage = new ItemPage(GalleryCatalog.FindItem("Slider"));
                WithRenderedPage(sliderPage, () =>
                {
                    Assert.IsTrue(sliderPage.HasDirectPageContent);
                    var sliderExamples = GetRenderedExamples(sliderPage);
                    Assert.AreEqual(4, sliderExamples.Count);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "A simple slider.",
                            "A slider with steps and range specified.",
                            "A slider with tick marks.",
                            "A vertical slider with range and tick marks specified."
                        },
                        sliderExamples.Select(example => example.HeaderText).ToArray());

                    Assert.AreEqual("GallerySample_Slider_Root", sliderExamples[0].AutomationId);
                    AssertSliderExample(sliderExamples[0], "Simple", 0, 100, 0, 1, TickPlacement.None, Orientation.Horizontal, "GallerySample_Slider_Slider");
                    AssertSliderExample(sliderExamples[1], "Range and steps specified", 500, 1000, 500, 50, TickPlacement.None, Orientation.Horizontal);
                    AssertSliderExample(sliderExamples[2], "Tick marks", 0, 100, 0, 20, TickPlacement.Both, Orientation.Horizontal);
                    AssertSliderExample(sliderExamples[3], "Vertical", 0, 100, 0, 20, TickPlacement.Both, Orientation.Vertical);
                });
            });
        }

        [TestMethod]
        public void TextPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var textBlockPage = new ItemPage(GalleryCatalog.FindItem("TextBlock"));
                Assert.IsTrue(textBlockPage.HasDirectPageContent);
                var textBlockExamples = GetRenderedExamples(textBlockPage);
                Assert.AreEqual(4, textBlockExamples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "A simple TextBlock.",
                        "A TextBlock with style applied.",
                        "A TextBlock with inline text elements.",
                        "A TextBlock with wrap property."
                    },
                    textBlockExamples.Select(example => example.HeaderText).ToArray());

                var simpleTextBlock = (TextBlock)textBlockExamples[0].ExampleContent;
                Assert.AreEqual("I am a text block.", simpleTextBlock.Text);

                var styledTextBlock = (TextBlock)textBlockExamples[1].ExampleContent;
                Assert.AreEqual("I am a styled TextBlock.", styledTextBlock.Text);
                Assert.AreEqual("Comic Sans MS", styledTextBlock.FontFamily.Source);
                Assert.AreEqual(FontStyles.Italic, styledTextBlock.FontStyle);

                var inlineTextBlock = (TextBlock)textBlockExamples[2].ExampleContent;
                Assert.AreEqual(14.0, inlineTextBlock.FontSize);
                var inlines = inlineTextBlock.Inlines.ToArray();
                var firstRun = inlines.OfType<Run>().First(run => run.Text.Contains("Text in a TextBlock"));
                Assert.AreEqual("Text in a TextBlock doesn't have to be a simple string.", firstRun.Text.Trim());
                Assert.AreEqual("Times New Roman", firstRun.FontFamily.Source);
                Assert.AreSame(Application.Current.FindResource("TextFillColorPrimaryBrush"), firstRun.Foreground);
                Assert.IsTrue(inlines.OfType<LineBreak>().Any());
                var nestedSpan = inlines.OfType<Span>().Single();
                var nestedText = new TextRange(nestedSpan.ContentStart, nestedSpan.ContentEnd).Text;
                StringAssert.Contains(nestedText, "Text can be ");
                StringAssert.Contains(nestedText, "bold, ");
                StringAssert.Contains(nestedText, "italic, ");
                StringAssert.Contains(nestedText, "or underlined");
                var nestedInlines = nestedSpan.Inlines.Cast<Inline>().ToArray();
                Assert.AreEqual("bold", nestedInlines.OfType<Bold>().Single().Inlines.OfType<Run>().Single().Text);
                Assert.AreEqual("italic", nestedInlines.OfType<Italic>().Single().Inlines.OfType<Run>().Single().Text);
                Assert.AreEqual("underlined", nestedInlines.OfType<Underline>().Single().Inlines.OfType<Run>().Single().Text);

                var wrappedTextBlock = (TextBlock)textBlockExamples[3].ExampleContent;
                Assert.AreEqual(14.0, wrappedTextBlock.FontSize);
                Assert.AreEqual(TextWrapping.Wrap, wrappedTextBlock.TextWrapping);
                StringAssert.Contains(wrappedTextBlock.Text, "The TextBlock control provides flexible text support");

                var textBoxPage = new ItemPage(GalleryCatalog.FindItem("TextBox"));
                Assert.IsTrue(textBoxPage.HasDirectPageContent);
                var textBoxExamples = GetRenderedExamples(textBoxPage);
                Assert.AreEqual(3, textBoxExamples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple TextBox.", "A TextBox with input validation.", "A multi-line TextBox." },
                    textBoxExamples.Select(example => example.HeaderText).ToArray());

                var simpleTextBox = (TextBox)textBoxExamples[0].ExampleContent;
                Assert.AreEqual("simple TextBox", AutomationProperties.GetName(simpleTextBox));

                var validatedTextBox = (TextBox)textBoxExamples[1].ExampleContent;
                Assert.AreEqual("validated TextBox", AutomationProperties.GetName(validatedTextBox));
                var textBinding = BindingOperations.GetBinding(validatedTextBox, TextBox.TextProperty);
                Assert.IsNotNull(textBinding);
                Assert.AreEqual(UpdateSourceTrigger.PropertyChanged, textBinding.UpdateSourceTrigger);
                Assert.AreEqual(1, textBinding.ValidationRules.Count);
                Assert.AreEqual("AlphabeticValidationRule", textBinding.ValidationRules[0].GetType().Name);
                Assert.AreEqual(ValidationResult.ValidResult, textBinding.ValidationRules[0].Validate(string.Empty, System.Globalization.CultureInfo.InvariantCulture));
                Assert.AreEqual(ValidationResult.ValidResult, textBinding.ValidationRules[0].Validate("Alphabetic", System.Globalization.CultureInfo.InvariantCulture));
                var invalidResult = textBinding.ValidationRules[0].Validate("abc123", System.Globalization.CultureInfo.InvariantCulture);
                Assert.IsFalse(invalidResult.IsValid);
                Assert.AreEqual("Only English alphabetic characters (a-z, A-Z) are allowed.", invalidResult.ErrorContent);

                var multilineTextBox = (TextBox)textBoxExamples[2].ExampleContent;
                Assert.IsTrue(multilineTextBox.AcceptsReturn);
                Assert.AreEqual(TextWrapping.Wrap, multilineTextBox.TextWrapping);
                Assert.AreEqual("multi-line TextBox", AutomationProperties.GetName(multilineTextBox));

                var labelPage = new ItemPage(GalleryCatalog.FindItem("Label"));
                Assert.IsTrue(labelPage.HasDirectPageContent);
                var labelExamples = GetRenderedExamples(labelPage);
                Assert.AreEqual(2, labelExamples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple Label.", "A Label for TextBox." },
                    labelExamples.Select(example => example.HeaderText).ToArray());

                var simpleLabel = (Label)labelExamples[0].ExampleContent;
                Assert.AreEqual("I am a Label.", simpleLabel.Content);
                Assert.AreEqual(0.7, simpleLabel.Opacity);

                var labelGrid = (Grid)labelExamples[1].ExampleContent;
                Assert.AreEqual(2, labelGrid.RowDefinitions.Count);
                var textBoxLabel = labelGrid.Children.OfType<Label>().Single();
                var labelledTextBox = labelGrid.Children.OfType<TextBox>().Single();
                Assert.AreEqual(0, Grid.GetRow(textBoxLabel));
                Assert.AreEqual("I am a Label of the TextBox below.", textBoxLabel.Content);
                Assert.AreEqual(0.7, textBoxLabel.Opacity);
                Assert.AreEqual(1, Grid.GetRow(labelledTextBox));
                Assert.AreEqual("Simple Text Box", AutomationProperties.GetName(labelledTextBox));

                var passwordBoxPage = new ItemPage(GalleryCatalog.FindItem("PasswordBox"));
                Assert.IsTrue(passwordBoxPage.HasDirectPageContent);
                var passwordBoxExamples = GetRenderedExamples(passwordBoxPage);
                Assert.AreEqual(1, passwordBoxExamples.Count);
                Assert.AreEqual("A simple PasswordBox.", passwordBoxExamples[0].HeaderText);
                Assert.AreEqual("Simple Password Box", AutomationProperties.GetName((PasswordBox)passwordBoxExamples[0].ExampleContent));

                var richTextPage = new ItemPage(GalleryCatalog.FindItem("RichTextEdit"));
                Assert.IsTrue(richTextPage.HasDirectPageContent);
                var richTextExamples = GetRenderedExamples(richTextPage);
                Assert.AreEqual(1, richTextExamples.Count);
                Assert.AreEqual("A simple RichTextBox", richTextExamples[0].HeaderText);
                var richTextBox = (RichTextBox)richTextExamples[0].ExampleContent;
                Assert.AreEqual("simple rich text editor", AutomationProperties.GetName(richTextBox));
                Assert.IsTrue(double.IsNaN(richTextBox.Width));
                Assert.IsTrue(double.IsNaN(richTextBox.Height));
                Assert.AreSame(DependencyProperty.UnsetValue, richTextBox.ReadLocalValue(FrameworkElement.MinHeightProperty));
            });
        }

        [TestMethod]
        public void LayoutPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var borderPage = new ItemPage(GalleryCatalog.FindItem("Border"));
                Assert.IsTrue(borderPage.HasDirectPageContent);
                var borderExamples = GetRenderedExamples(borderPage);
                Assert.AreEqual(3, borderExamples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A basic Border", "A Border with rounded corners", "A Border with different thickness on each side" },
                    borderExamples.Select(example => example.HeaderText).ToArray());

                var basicBorder = (Border)borderExamples[0].ExampleContent;
                Assert.AreEqual(Brushes.Gray, basicBorder.BorderBrush);
                Assert.AreEqual(new Thickness(2), basicBorder.BorderThickness);
                Assert.AreEqual(new Thickness(10), basicBorder.Padding);
                Assert.AreEqual("Content inside a Border", ((TextBlock)basicBorder.Child).Text);
                StringAssert.Contains(borderExamples[0].XamlCode, "<Border BorderBrush=\"Gray\" BorderThickness=\"2\" Padding=\"10\">");
                StringAssert.Contains(borderExamples[0].XamlCode, "<TextBlock Text=\"Content inside a Border\" />");

                var roundedBorder = (Border)borderExamples[1].ExampleContent;
                Assert.AreEqual(Brushes.LightBlue, roundedBorder.Background);
                Assert.AreEqual(Brushes.CornflowerBlue, roundedBorder.BorderBrush);
                Assert.AreEqual(new Thickness(2), roundedBorder.BorderThickness);
                Assert.AreEqual(new CornerRadius(10), roundedBorder.CornerRadius);
                Assert.AreEqual(new Thickness(15), roundedBorder.Padding);
                var roundedBorderText = (TextBlock)roundedBorder.Child;
                Assert.AreEqual("Rounded Border", roundedBorderText.Text);
                Assert.AreEqual(Brushes.Black, roundedBorderText.Foreground);
                StringAssert.Contains(borderExamples[1].XamlCode, "<Border BorderBrush=\"CornflowerBlue\" BorderThickness=\"2\" CornerRadius=\"10\" Padding=\"15\" Background=\"LightBlue\">");
                StringAssert.Contains(borderExamples[1].XamlCode, "<TextBlock Text=\"Rounded Border\" />");

                var variedBorder = (Border)borderExamples[2].ExampleContent;
                Assert.AreEqual(Brushes.DarkSlateGray, variedBorder.BorderBrush);
                Assert.AreEqual(new Thickness(1, 2, 4, 8), variedBorder.BorderThickness);
                Assert.AreEqual(new Thickness(10), variedBorder.Padding);
                Assert.AreEqual("Different border thickness (Left=1, Top=2, Right=4, Bottom=8)", ((TextBlock)variedBorder.Child).Text);
                StringAssert.Contains(borderExamples[2].XamlCode, "<Border BorderBrush=\"DarkSlateGray\" BorderThickness=\"1,2,4,8\" Padding=\"10\">");
                StringAssert.Contains(borderExamples[2].XamlCode, "<TextBlock Text=\"Different border thickness\" />");

                var gridPage = new ItemPage(GalleryCatalog.FindItem("Grid"));
                Assert.IsTrue(gridPage.HasDirectPageContent);
                var gridExamples = GetRenderedExamples(gridPage);
                Assert.AreEqual(3, gridExamples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple 3x3 Grid", "A Grid with custom sizing and spanning", "Grid using XAML shorthand syntax" },
                    gridExamples.Select(example => example.HeaderText).ToArray());

                var simpleGrid = (Grid)gridExamples[0].ExampleContent;
                Assert.AreEqual(250.0, simpleGrid.Height);
                Assert.IsTrue(simpleGrid.ShowGridLines);
                Assert.AreEqual(3, simpleGrid.RowDefinitions.Count);
                Assert.AreEqual(3, simpleGrid.ColumnDefinitions.Count);
                CollectionAssert.AreEqual(
                    Enumerable.Range(1, 9).Select(i => "Cell " + i).ToArray(),
                    simpleGrid.Children.OfType<TextBlock>().Select(textBlock => textBlock.Text).ToArray());

                var customGrid = (Grid)gridExamples[1].ExampleContent;
                AssertGridExample(customGrid, 300.0, new[] { "Row 0, Column 0", "Row 1, Spans all columns", "Row 2, Spans 2 columns" });

                var shorthandGrid = (Grid)gridExamples[2].ExampleContent;
                AssertGridExample(shorthandGrid, 300.0, new[] { "Header (100px)", "Main Content Area (fills available space)", "Footer (Auto height, spans all columns)" });
                Assert.AreEqual(100.0, shorthandGrid.ColumnDefinitions[0].Width.Value);
                Assert.AreEqual(2.0, shorthandGrid.ColumnDefinitions[1].Width.Value);

                var stackPanelPage = new ItemPage(GalleryCatalog.FindItem("StackPanel"));
                Assert.IsTrue(stackPanelPage.HasDirectPageContent);
                var stackPanelExamples = GetRenderedExamples(stackPanelPage);
                Assert.AreEqual(2, stackPanelExamples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A basic vertical StackPanel", "A horizontal StackPanel" },
                    stackPanelExamples.Select(example => example.HeaderText).ToArray());
                AssertStackPanelExample((StackPanel)stackPanelExamples[0].ExampleContent, Orientation.Vertical);
                AssertStackPanelExample((StackPanel)stackPanelExamples[1].ExampleContent, Orientation.Horizontal);

                var expanderPage = new ItemPage(GalleryCatalog.FindItem("Expander"));
                Assert.IsTrue(expanderPage.HasDirectPageContent);
                var expanderExamples = GetRenderedExamples(expanderPage);
                Assert.AreEqual(1, expanderExamples.Count);
                Assert.AreEqual("An Expander with text in the header and content areas", expanderExamples[0].HeaderText);
                var expanderGrid = (Grid)expanderExamples[0].ExampleContent;
                Assert.AreEqual(2, expanderGrid.ColumnDefinitions.Count);
                var expander = expanderGrid.Children.OfType<Expander>().Single();
                Assert.AreEqual(0, Grid.GetColumn(expander));
                Assert.AreEqual("This text is in the header", expander.Header);
                Assert.AreEqual("This is in the content", expander.Content);

                var gridSplitterPage = new ItemPage(GalleryCatalog.FindItem("GridSplitter"));
                Assert.IsTrue(gridSplitterPage.HasDirectPageContent);
                var gridSplitterExamples = GetRenderedExamples(gridSplitterPage);
                Assert.AreEqual(1, gridSplitterExamples.Count);
                Assert.AreEqual("A GridSplitter", gridSplitterExamples[0].HeaderText);
                var gridSplitterRoot = (Grid)gridSplitterExamples[0].ExampleContent;
                Assert.AreEqual(400.0, gridSplitterRoot.Height);
                Assert.AreEqual("Grid Splitter", ((TextBlock)gridSplitterRoot.Children[0]).Text);
                var splitterBorder = (Border)gridSplitterRoot.Children.OfType<Border>().Single();
                Assert.AreEqual(new Thickness(2), splitterBorder.BorderThickness);
                Assert.AreEqual(new Thickness(10), splitterBorder.Padding);
                Assert.AreEqual(new CornerRadius(4), splitterBorder.CornerRadius);
                Assert.AreEqual(1, Grid.GetRow(splitterBorder));
                var splitterGrid = (Grid)splitterBorder.Child;
                Assert.AreEqual(7, splitterGrid.RowDefinitions.Count);
                Assert.AreEqual(3, splitterGrid.ColumnDefinitions.Count);
                Assert.AreEqual(6, splitterGrid.Children.OfType<TextBlock>().Count());
                Assert.IsTrue(splitterGrid.Children.OfType<TextBlock>().All(textBlock => textBlock.Margin == new Thickness(0)));
                var splitters = splitterGrid.Children.OfType<GridSplitter>().ToArray();
                Assert.AreEqual(3, splitters.Length);
                Assert.AreEqual(GridResizeDirection.Columns, splitters[0].ResizeDirection);
                Assert.IsTrue(double.IsNaN(splitters[0].Width));
                Assert.AreEqual(5, Grid.GetRowSpan(splitters[0]));
                Assert.AreEqual(1, Grid.GetColumn(splitters[0]));
                Assert.IsTrue(splitters.Skip(1).All(splitter => splitter.ResizeDirection == GridResizeDirection.Rows));
                Assert.IsTrue(splitters.Skip(1).All(splitter => double.IsNaN(splitter.Height)));

                var groupBoxPage = new ItemPage(GalleryCatalog.FindItem("GroupBox"));
                Assert.IsTrue(groupBoxPage.HasDirectPageContent);
                var groupBoxExamples = GetRenderedExamples(groupBoxPage);
                Assert.AreEqual(1, groupBoxExamples.Count);
                Assert.AreEqual("A GroupBox", groupBoxExamples[0].HeaderText);
                var groupBox = (GroupBox)groupBoxExamples[0].ExampleContent;
                Assert.AreEqual("User Information", groupBox.Header);
                Assert.AreEqual(HorizontalAlignment.Left, groupBox.HorizontalAlignment);
                Assert.AreEqual(VerticalAlignment.Center, groupBox.VerticalAlignment);
                Assert.AreEqual(400.0, groupBox.Width);
                var groupStack = (StackPanel)groupBox.Content;
                Assert.AreEqual(3, groupStack.Children.Count);
                var nameRow = (StackPanel)groupStack.Children[0];
                var genderRow = (StackPanel)groupStack.Children[1];
                Assert.AreEqual("Name:", ((TextBlock)nameRow.Children[0]).Text);
                Assert.AreEqual("NameTextBox", ((TextBox)nameRow.Children[1]).Name);
                Assert.AreEqual("Gender:", ((TextBlock)genderRow.Children[0]).Text);
                Assert.AreEqual("GenderTextBox", ((TextBox)genderRow.Children[1]).Name);
                Assert.AreEqual("Submit", ((Button)groupStack.Children[2]).Content);
                Assert.IsTrue(double.IsNaN(((Button)groupStack.Children[2]).Width));

                var resizeGripPage = new ItemPage(GalleryCatalog.FindItem("ResizeGrip"));
                Assert.IsTrue(resizeGripPage.HasDirectPageContent);
                var resizeGripExamples = GetRenderedExamples(resizeGripPage);
                Assert.AreEqual(1, resizeGripExamples.Count);
                Assert.AreEqual("A ResizeGrip", resizeGripExamples[0].HeaderText);
                var resizeGripStack = (StackPanel)resizeGripExamples[0].ExampleContent;
                Assert.AreEqual(Orientation.Vertical, resizeGripStack.Orientation);
                Assert.AreEqual(1, Grid.GetRow(resizeGripStack));
                Assert.AreEqual(new Thickness(0, 10, 0, 40), ((TextBlock)resizeGripStack.Children[0]).Margin);
                var resizeGripButton = (Button)resizeGripStack.Children[1];
                Assert.AreEqual("OpenResizeGripWindow", resizeGripButton.Name);
                Assert.AreEqual("Open window with resize grip", resizeGripButton.Content);
                Assert.AreEqual(HorizontalAlignment.Center, resizeGripButton.HorizontalAlignment);
                Assert.AreEqual(VerticalAlignment.Center, resizeGripButton.VerticalAlignment);
            });
        }

        [TestMethod]
        public void DateAndStatusPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var calendarPage = new ItemPage(GalleryCatalog.FindItem("Calendar"));
                Assert.IsTrue(calendarPage.HasDirectPageContent);
                var calendarExamples = GetRenderedExamples(calendarPage);
                Assert.AreEqual(1, calendarExamples.Count);
                Assert.AreEqual("A basic Calendar control.", calendarExamples[0].HeaderText);
                var calendar = (Calendar)calendarExamples[0].ExampleContent;
                Assert.AreEqual(HorizontalAlignment.Left, calendar.HorizontalAlignment);
                Assert.AreEqual("Default", AutomationProperties.GetName(calendar));
                Assert.IsFalse(KeyboardNavigation.GetIsTabStop(calendar));

                var datePickerPage = new ItemPage(GalleryCatalog.FindItem("DatePicker"));
                Assert.IsTrue(datePickerPage.HasDirectPageContent);
                var datePickerExamples = GetRenderedExamples(datePickerPage);
                Assert.AreEqual(1, datePickerExamples.Count);
                Assert.AreEqual("A basic DatePicker control.", datePickerExamples[0].HeaderText);
                var datePicker = (DatePicker)datePickerExamples[0].ExampleContent;
                Assert.AreEqual(200.0, datePicker.MinWidth);
                Assert.AreEqual(HorizontalAlignment.Left, datePicker.HorizontalAlignment);
                Assert.AreEqual("Pick a date", AutomationProperties.GetName(datePicker));

                var progressBarPage = new ItemPage(GalleryCatalog.FindItem("ProgressBar"));
                Assert.IsTrue(progressBarPage.HasDirectPageContent);
                var progressBarExamples = GetRenderedExamples(progressBarPage);
                Assert.AreEqual(2, progressBarExamples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple progress bar.", "An indeterminate progress bar." },
                    progressBarExamples.Select(example => example.HeaderText).ToArray());
                var determinate = (ProgressBar)progressBarExamples[0].ExampleContent;
                Assert.AreEqual(new Thickness(24), determinate.Margin);
                Assert.AreEqual(40.0, determinate.Value);
                Assert.IsFalse(determinate.IsIndeterminate);
                Assert.AreEqual("A determinate", AutomationProperties.GetName(determinate));
                var indeterminate = (ProgressBar)progressBarExamples[1].ExampleContent;
                Assert.AreEqual(new Thickness(24), indeterminate.Margin);
                Assert.IsTrue(indeterminate.IsIndeterminate);
                Assert.AreEqual("An indeterminate", AutomationProperties.GetName(indeterminate));

                var toolTipPage = new ItemPage(GalleryCatalog.FindItem("ToolTip"));
                Assert.IsTrue(toolTipPage.HasDirectPageContent);
                var toolTipExamples = GetRenderedExamples(toolTipPage);
                Assert.AreEqual(1, toolTipExamples.Count);
                Assert.AreEqual("A button with a simple ToolTip.", toolTipExamples[0].HeaderText);
                var toolTipButton = (Button)toolTipExamples[0].ExampleContent;
                Assert.AreEqual("Button with a simple ToolTip.", toolTipButton.Content);
                Assert.AreEqual("TooltipButton", AutomationProperties.GetName(toolTipButton));
                Assert.AreEqual(100, ToolTipService.GetInitialShowDelay(toolTipButton));
                Assert.AreEqual(PlacementMode.MousePoint, ToolTipService.GetPlacement(toolTipButton));
                var simpleToolTip = ToolTipService.GetToolTip(toolTipButton) as ToolTip;
                Assert.IsNotNull(simpleToolTip);
                Assert.AreEqual("Simple ToolTip", simpleToolTip.Content);

                var retiredSystemItems = new[]
                {
                    GalleryCatalog.FindItem("Clipboard"),
                    GalleryCatalog.FindItem("FileAndFolderDialogs"),
                    GalleryCatalog.FindItem("MessageBox")
                };
                Assert.IsTrue(retiredSystemItems.All(item => item == null));

                // Keep the copied reference assertions available if these pages are
                // deliberately restored to the public catalog in the future.
                if (retiredSystemItems.All(item => item != null))
                {
                var clipboardPage = new ItemPage(retiredSystemItems[0]);
                WithRenderedPage(clipboardPage, () =>
                {
                    Assert.IsTrue(clipboardPage.HasDirectPageContent);
                    var clipboardExamples = GetRenderedExamples(clipboardPage);
                    Assert.AreEqual(6, clipboardExamples.Count);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "Copy text to Clipboard",
                            "Paste text from Clipboard",
                            "Clear Clipboard",
                            "Check Clipboard data formats",
                            "Copy image to Clipboard",
                            "Paste image from Clipboard"
                        },
                        clipboardExamples.Select(example => example.HeaderText).ToArray());

                    var copyClipboardStack = (StackPanel)clipboardExamples[0].ExampleContent;
                    var copyTextBox = (TextBox)copyClipboardStack.Children[0];
                    Assert.AreEqual("CopyTextBox", copyTextBox.Name);
                    Assert.AreEqual("Hello, Clipboard!", copyTextBox.Text);
                    Assert.AreEqual(300.0, copyTextBox.Width);
                    Assert.AreEqual(HorizontalAlignment.Left, copyTextBox.HorizontalAlignment);
                    Assert.AreEqual("Copy To Clipboard TextBox", AutomationProperties.GetName(copyTextBox));
                    Assert.AreEqual("Copy to Clipboard", ((Button)copyClipboardStack.Children[1]).Content);
                    var copyStatusText = (TextBlock)copyClipboardStack.Children[2];
                    Assert.AreEqual(string.Empty, copyStatusText.Text);
                    AssertDirectBindingPath(copyStatusText, TextBlock.TextProperty, "ViewModel.CopyStatus");

                    var pasteClipboardStack = (StackPanel)clipboardExamples[1].ExampleContent;
                    Assert.AreEqual("Paste from Clipboard", ((Button)pasteClipboardStack.Children[0]).Content);
                    Assert.AreEqual("Pasted Content:", ((TextBlock)pasteClipboardStack.Children[1]).Text);
                    var pasteTextBox = (TextBox)pasteClipboardStack.Children[2];
                    Assert.AreEqual("PasteTextBox", pasteTextBox.Name);
                    Assert.IsTrue(pasteTextBox.IsReadOnly);
                    Assert.AreEqual(TextWrapping.Wrap, pasteTextBox.TextWrapping);
                    Assert.AreEqual(60.0, pasteTextBox.MinHeight);
                    Assert.AreEqual(300.0, pasteTextBox.Width);
                    Assert.AreEqual("Paste content textbox", AutomationProperties.GetName(pasteTextBox));
                    AssertDirectBindingPath(pasteTextBox, TextBox.TextProperty, "ViewModel.PastedText", BindingMode.TwoWay);

                    var clearClipboardStack = (StackPanel)clipboardExamples[2].ExampleContent;
                    AssertButtonResultExample(clearClipboardStack, "Clear Clipboard", string.Empty);
                    AssertDirectBindingPath((TextBlock)clearClipboardStack.Children[1], TextBlock.TextProperty, "ViewModel.ClearStatus");

                    var formatsClipboardStack = (StackPanel)clipboardExamples[3].ExampleContent;
                    AssertButtonResultExample(formatsClipboardStack, "Check Clipboard Formats", string.Empty);
                    AssertDirectBindingPath((TextBlock)formatsClipboardStack.Children[1], TextBlock.TextProperty, "ViewModel.FormatsInfo");

                    var copyImageStack = (StackPanel)clipboardExamples[4].ExampleContent;
                    var sourceImage = (Image)copyImageStack.Children[0];
                    Assert.AreEqual("SourceImage", sourceImage.Name);
                    Assert.AreEqual(100.0, sourceImage.Width);
                    Assert.AreEqual(100.0, sourceImage.Height);
                    Assert.AreEqual(HorizontalAlignment.Left, sourceImage.HorizontalAlignment);
                    Assert.IsInstanceOfType(sourceImage.Source, typeof(BitmapSource));
                    StringAssert.Contains(sourceImage.Source.ToString(), "ControlImages/Clipboard.png");
                    Assert.AreEqual("Copy Image to Clipboard", ((Button)copyImageStack.Children[1]).Content);
                    AssertDirectBindingPath((TextBlock)copyImageStack.Children[2], TextBlock.TextProperty, "ViewModel.CopyImageStatus");

                    var pasteImageStack = (StackPanel)clipboardExamples[5].ExampleContent;
                    Assert.AreEqual("Paste Image from Clipboard", ((Button)pasteImageStack.Children[0]).Content);
                    Assert.AreEqual("Pasted Image:", ((TextBlock)pasteImageStack.Children[1]).Text);
                    var imageHost = (Border)pasteImageStack.Children[2];
                    Assert.AreEqual(Brushes.Gray, imageHost.BorderBrush);
                    Assert.AreEqual(new Thickness(1), imageHost.BorderThickness);
                    Assert.AreEqual(200.0, imageHost.Width);
                    Assert.AreEqual(200.0, imageHost.Height);
                    var pastedImage = (Image)imageHost.Child;
                    Assert.AreEqual("PastedImage", pastedImage.Name);
                    Assert.AreEqual(Stretch.Uniform, pastedImage.Stretch);
                    Assert.AreEqual(Visibility.Collapsed, pastedImage.Visibility);
                    AssertDirectBindingPath((TextBlock)pasteImageStack.Children[3], TextBlock.TextProperty, "ViewModel.PasteImageStatus");
                });

                var dialogsPage = new ItemPage(retiredSystemItems[1]);
                WithRenderedPage(dialogsPage, () =>
                {
                    Assert.IsTrue(dialogsPage.HasDirectPageContent);
                    var dialogsExamples = GetRenderedExamples(dialogsPage);
                    Assert.AreEqual(4, dialogsExamples.Count);
                    CollectionAssert.AreEqual(
                        new[] { "Pick Single File", "Pick Multiple Files", "Save File", "Pick Folder" },
                        dialogsExamples.Select(example => example.HeaderText).ToArray());
                    var singleFileStack = (StackPanel)dialogsExamples[0].ExampleContent;
                    AssertButtonResultExample(singleFileStack, "Pick a single file", "No file selected");
                    AssertDirectBindingPath((TextBlock)singleFileStack.Children[1], TextBlock.TextProperty, "ViewModel.SingleFilePath");

                    var multipleFilesStack = (StackPanel)dialogsExamples[1].ExampleContent;
                    AssertButtonResultExample(multipleFilesStack, "Pick multiple files", "No files selected");
                    AssertDirectBindingPath((TextBlock)multipleFilesStack.Children[1], TextBlock.TextProperty, "ViewModel.MultipleFilesPath");

                    var saveFileStack = (StackPanel)dialogsExamples[2].ExampleContent;
                    var saveTextBox = (TextBox)saveFileStack.Children[0];
                    Assert.AreEqual("Enter text here to save to a file...", saveTextBox.Text);
                    AssertDirectBindingPath(saveTextBox, TextBox.TextProperty, "ViewModel.FileContent");
                    Assert.AreEqual(UpdateSourceTrigger.PropertyChanged, BindingOperations.GetBinding(saveTextBox, TextBox.TextProperty).UpdateSourceTrigger);
                    Assert.IsTrue(saveTextBox.AcceptsReturn);
                    Assert.AreEqual(TextWrapping.Wrap, saveTextBox.TextWrapping);
                    Assert.AreEqual(80.0, saveTextBox.MinHeight);
                    Assert.AreEqual(ScrollBarVisibility.Auto, saveTextBox.VerticalScrollBarVisibility);
                    Assert.AreEqual("Save File Text Box", AutomationProperties.GetName(saveTextBox));
                    Assert.AreEqual("The text in the textbox will be saved to a file on button click", AutomationProperties.GetHelpText(saveTextBox));
                    Assert.AreEqual("Save a file", ((Button)saveFileStack.Children[1]).Content);
                    var savedFileOutput = (TextBlock)saveFileStack.Children[2];
                    Assert.AreEqual("No file saved", savedFileOutput.Text);
                    AssertDirectBindingPath(savedFileOutput, TextBlock.TextProperty, "ViewModel.SavedFilePath");

                    var folderStack = (StackPanel)dialogsExamples[3].ExampleContent;
                    AssertButtonResultExample(folderStack, "Pick a folder", "No folder selected");
                    AssertDirectBindingPath((TextBlock)folderStack.Children[1], TextBlock.TextProperty, "ViewModel.SelectedFolderPath");
                });

                var messageBoxPage = new ItemPage(retiredSystemItems[2]);
                WithRenderedPage(messageBoxPage, () =>
                {
                    Assert.IsTrue(messageBoxPage.HasDirectPageContent);
                    var messageBoxExamples = GetRenderedExamples(messageBoxPage);
                    var messageBoxControlExamples = FindDescendants<ControlExample>((DependencyObject)messageBoxPage.DirectPageContent).ToArray();
                    Assert.AreEqual(6, messageBoxExamples.Count);
                    Assert.AreEqual(messageBoxExamples.Count, messageBoxControlExamples.Length);
                    CollectionAssert.AreEqual(
                        new[]
                        {
                            "Simple MessageBox",
                            "MessageBox with Custom Title and Description",
                            "MessageBox with Different Buttons",
                            "MessageBox with Different Images",
                            "Information, Error, and Warning MessageBox",
                            "MessageBox with Custom Default Button"
                        },
                        messageBoxExamples.Select(example => example.HeaderText).ToArray());

                    var defaultMessageStack = (StackPanel)messageBoxExamples[0].ExampleContent;
                    AssertButtonResultExample(defaultMessageStack, "Simple MessageBox", "No message shown yet");
                    AssertDirectBindingPath((TextBlock)defaultMessageStack.Children[1], TextBlock.TextProperty, "ViewModel.DefaultMessageResult");

                    var customTitleStack = (StackPanel)messageBoxExamples[1].ExampleContent;
                    AssertButtonResultExample(customTitleStack, "Custom MessageBox", "No message shown yet");
                    AssertDirectBindingPath((TextBlock)customTitleStack.Children[1], TextBlock.TextProperty, "ViewModel.CustomTitleResult");

                    var buttonsGrid = (Grid)messageBoxExamples[2].ExampleContent;
                    AssertDirectBindingPath(messageBoxControlExamples[2], ControlExample.XamlCodeProperty, "ViewModel.DifferentButtonsXamlCode");
                    AssertDirectBindingPath(messageBoxControlExamples[2], ControlExample.CSharpCodeProperty, "ViewModel.DifferentButtonsCSharpCode");
                    AssertMessageBoxSelectorExample(buttonsGrid, "Button Type:", "MessageBox with Different Buttons", "MessageBox Button Selector", "No button clicked yet", new[] { "OK", "OK/Cancel", "Abort/Retry/Ignore", "Yes/No/Cancel", "Yes/No", "Retry/Cancel", "Cancel/Try/Continue" });
                    var buttonsOutput = (TextBlock)((StackPanel)buttonsGrid.Children.OfType<StackPanel>().Single(stackPanel => Grid.GetColumn(stackPanel) == 0)).Children[1];
                    var buttonsComboBox = (ComboBox)((StackPanel)buttonsGrid.Children.OfType<StackPanel>().Single(stackPanel => Grid.GetColumn(stackPanel) == 1)).Children[1];
                    AssertDirectBindingPath(buttonsOutput, TextBlock.TextProperty, "ViewModel.DifferentButtonsResult");
                    AssertDirectBindingPath(buttonsComboBox, Selector.SelectedIndexProperty, "ViewModel.SelectedButtonIndex");

                    var imagesGrid = (Grid)messageBoxExamples[3].ExampleContent;
                    AssertDirectBindingPath(messageBoxControlExamples[3], ControlExample.XamlCodeProperty, "ViewModel.DifferentImagesXamlCode");
                    AssertDirectBindingPath(messageBoxControlExamples[3], ControlExample.CSharpCodeProperty, "ViewModel.DifferentImagesCSharpCode");
                    AssertMessageBoxSelectorExample(imagesGrid, "Icon Type:", "MessageBox with different images", "MessageBox Image Selector", "No image example shown yet", new[] { "None", "Error", "Question", "Warning", "Information" });
                    var imagesOutput = (TextBlock)((StackPanel)imagesGrid.Children.OfType<StackPanel>().Single(stackPanel => Grid.GetColumn(stackPanel) == 0)).Children[1];
                    var imagesComboBox = (ComboBox)((StackPanel)imagesGrid.Children.OfType<StackPanel>().Single(stackPanel => Grid.GetColumn(stackPanel) == 1)).Children[1];
                    AssertDirectBindingPath(imagesOutput, TextBlock.TextProperty, "ViewModel.DifferentImagesResult");
                    AssertDirectBindingPath(imagesComboBox, Selector.SelectedIndexProperty, "ViewModel.SelectedImageIndex");

                    var commonMessagesStack = (StackPanel)messageBoxExamples[4].ExampleContent;
                    AssertDirectBindingPath(messageBoxControlExamples[4], ControlExample.XamlCodeProperty, "ViewModel.CommonMessagesXamlCode");
                    AssertDirectBindingPath(messageBoxControlExamples[4], ControlExample.CSharpCodeProperty, "ViewModel.CommonMessagesCSharpCode");
                    var commonButtons = ((WrapPanel)commonMessagesStack.Children[0]).Children.OfType<Button>().ToArray();
                    CollectionAssert.AreEqual(new[] { "Information", "Error", "Warning" }, commonButtons.Select(button => (string)button.Content).ToArray());
                    Assert.AreEqual(new Thickness(0, 0, 5, 0), commonButtons[0].Margin);
                    Assert.AreEqual(new Thickness(0, 0, 5, 0), commonButtons[1].Margin);
                    Assert.AreEqual(new Thickness(0), commonButtons[2].Margin);
                    var commonOutput = (TextBlock)commonMessagesStack.Children[1];
                    Assert.AreEqual("No common message shown yet", commonOutput.Text);
                    AssertDirectBindingPath(commonOutput, TextBlock.TextProperty, "ViewModel.CommonMessagesResult");

                    var customDefaultStack = (StackPanel)messageBoxExamples[5].ExampleContent;
                    AssertButtonResultExample(customDefaultStack, "Show with 'No' as default", "No selection made");
                    AssertDirectBindingPath((TextBlock)customDefaultStack.Children[1], TextBlock.TextProperty, "ViewModel.CustomDefaultResult");
                });
                }
            });
        }

        [TestMethod]
        public void NavigationCollectionsAndHyperlinkPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var menuPage = new ItemPage(GalleryCatalog.FindItem("Menu"));
                WithRenderedPage(menuPage, () =>
                {
                    Assert.IsTrue(menuPage.HasDirectPageContent);
                    var menuExamples = GetRenderedExamples(menuPage);
                    Assert.AreEqual(1, menuExamples.Count);
                    Assert.AreEqual("Standard Menu.", menuExamples[0].HeaderText);
                    var menuStack = (StackPanel)menuExamples[0].ExampleContent;
                    Assert.AreEqual(2, menuStack.Children.Count);
                    var statusMenuItem = (TextBlock)menuStack.Children[0];
                    Assert.AreEqual("StatusMenuItem", statusMenuItem.Name);
                    Assert.AreEqual(string.Empty, statusMenuItem.Text);

                    var menu = (Menu)menuStack.Children[1];
                    var menuItems = menu.Items.Cast<object>().ToArray();
                    Assert.AreEqual(6, menuItems.Length);
                    var fileMenu = (MenuItem)menuItems[0];
                    Assert.AreEqual("File", fileMenu.Header);
                    Assert.AreEqual(7, fileMenu.Items.Count);
                    CollectionAssert.AreEqual(new[] { "New", "New window", "Open", "Save", "Save As" }, fileMenu.Items.Cast<object>().Take(5).Cast<MenuItem>().Select(item => (string)item.Header).ToArray());
                    Assert.IsInstanceOfType(fileMenu.Items[5], typeof(Separator));
                    Assert.AreEqual("Exit", ((MenuItem)fileMenu.Items[6]).Header);

                    var editMenu = (MenuItem)menuItems[1];
                    Assert.AreEqual("Edit", editMenu.Header);
                    Assert.AreEqual(12, editMenu.Items.Count);
                    Assert.AreEqual("Undo", ((MenuItem)editMenu.Items[0]).Header);
                    Assert.IsInstanceOfType(editMenu.Items[1], typeof(Separator));
                    CollectionAssert.AreEqual(new[] { "Cut", "Copy", "Paste" }, editMenu.Items.Cast<object>().Skip(2).Take(3).Cast<MenuItem>().Select(item => (string)item.Header).ToArray());
                    Assert.IsFalse(((MenuItem)editMenu.Items[5]).IsEnabled);
                    Assert.IsInstanceOfType(editMenu.Items[6], typeof(Separator));
                    CollectionAssert.AreEqual(new[] { "Search with browser", "Find", "Find next" }, editMenu.Items.Cast<object>().Skip(7).Take(3).Cast<MenuItem>().Select(item => (string)item.Header).ToArray());
                    Assert.IsInstanceOfType(editMenu.Items[10], typeof(Separator));
                    Assert.AreEqual("Select All", ((MenuItem)editMenu.Items[11]).Header);
                    Assert.IsInstanceOfType(menuItems[2], typeof(Separator));
                    AssertGlyphMenuItem((MenuItem)menuItems[3], "Bold", "\uE8DD");
                    AssertGlyphMenuItem((MenuItem)menuItems[4], "Italic", "\uE8DB");
                    AssertGlyphMenuItem((MenuItem)menuItems[5], "Underlined", "\uE8DC");
                    var boldMenuItem = (MenuItem)menuItems[3];
                    boldMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, boldMenuItem));
                    Assert.AreEqual(Visibility.Visible, statusMenuItem.Visibility);
                    Assert.AreEqual("You pressed Bold", statusMenuItem.Text);
                    StringAssert.Contains(menuExamples[0].XamlCode, "<MenuItem Header=\"File\">");
                });

                var framePage = new ItemPage(GalleryCatalog.FindItem("Frame"));
                WithRenderedPage(framePage, () =>
                {
                    Assert.IsTrue(framePage.HasDirectPageContent);
                    var frameExamples = GetRenderedExamples(framePage);
                    Assert.AreEqual(1, frameExamples.Count);
                    Assert.AreEqual("A Frame", frameExamples[0].HeaderText);
                    var frameButton = (Button)frameExamples[0].ExampleContent;
                    Assert.AreEqual("OpenFrameWindow", frameButton.Name);
                    Assert.AreEqual("Open window to view Frame", frameButton.Content);
                    Assert.AreEqual(HorizontalAlignment.Center, frameButton.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, frameButton.VerticalAlignment);
                    StringAssert.Contains(frameExamples[0].XamlCode, "NavigationUIVisibility=\"Visible\"");
                });

                var navigationWindowPage = new ItemPage(GalleryCatalog.FindItem("NavigationWindow"));
                WithRenderedPage(navigationWindowPage, () =>
                {
                    Assert.IsTrue(navigationWindowPage.HasDirectPageContent);
                    var navigationWindowExamples = GetRenderedExamples(navigationWindowPage);
                    Assert.AreEqual(1, navigationWindowExamples.Count);
                    Assert.AreEqual("A Navigation Window", navigationWindowExamples[0].HeaderText);
                    var navigationWindowButton = (Button)navigationWindowExamples[0].ExampleContent;
                    Assert.AreEqual("OpenNavigationWindow", navigationWindowButton.Name);
                    Assert.AreEqual("Open window to view NavigationWindow", navigationWindowButton.Content);
                    Assert.AreEqual(HorizontalAlignment.Center, navigationWindowButton.HorizontalAlignment);
                    Assert.AreEqual(VerticalAlignment.Center, navigationWindowButton.VerticalAlignment);
                    StringAssert.Contains(navigationWindowExamples[0].XamlCode, "Source=\"/Views/Navigation/Page1.xaml\"");
                    StringAssert.Contains(navigationWindowExamples[0].XamlCode, "Width=\"800\"");
                });

                var navigationWindow = new NavigationWindow
                {
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Source = new Uri("pack://application:,,,/ModernWpf.Gallery;component/Pages/WpfGallery/Navigation/Page1.xaml", UriKind.Absolute)
                };
                try
                {
                    navigationWindow.Show();
                    WpfTestHost.DoEvents();
                    navigationWindow.UpdateLayout();
                    WpfTestHost.DoEvents();

                    StringAssert.Contains(
                        navigationWindow.Source.OriginalString,
                        "ModernWpf.Gallery;component/Pages/WpfGallery/Navigation/Page1.xaml");
                    Assert.IsInstanceOfType(navigationWindow.Content, typeof(Page1));
                }
                finally
                {
                    navigationWindow.Close();
                    WpfTestHost.DoEvents();
                }

                var frameWindow = new FrameWindow
                {
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };
                try
                {
                    frameWindow.Show();
                    WpfTestHost.DoEvents();
                    frameWindow.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var hostedFrame = FindDescendant<Frame>(frameWindow);
                    Assert.IsNotNull(hostedFrame);
                    StringAssert.Contains(
                        hostedFrame.Source.OriginalString,
                        "ModernWpf.Gallery;component/pages/wpfgallery/navigation/Page1.xaml");
                    Assert.IsInstanceOfType(hostedFrame.Content, typeof(Page1));
                }
                finally
                {
                    frameWindow.Close();
                    WpfTestHost.DoEvents();
                }

                var tabControlPage = new ItemPage(GalleryCatalog.FindItem("TabControl"));
                WithRenderedPage(tabControlPage, () =>
                {
                    Assert.IsTrue(tabControlPage.HasDirectPageContent);
                    var tabControlExamples = GetRenderedExamples(tabControlPage);
                    Assert.AreEqual(1, tabControlExamples.Count);
                    Assert.AreEqual("Standard TabControl.", tabControlExamples[0].HeaderText);
                    var tabControl = (TabControl)tabControlExamples[0].ExampleContent;
                    Assert.AreEqual(new Thickness(0, 8, 0, 0), tabControl.Margin);
                    Assert.AreEqual(2, tabControl.Items.Count);
                    AssertTabItem((TabItem)tabControl.Items[0], "Hello", "Hello Tab", "World", false);
                    AssertTabItem((TabItem)tabControl.Items[1], "The cake", "The cake Tab", "Is a lie.", true);
                    StringAssert.Contains(tabControlExamples[0].XamlCode, "<TabControl Margin=\"0,8,0,0\">");
                });

                var listBoxPage = new ItemPage(GalleryCatalog.FindItem("ListBox"));
                WithRenderedPage(listBoxPage, () =>
                {
                    Assert.IsTrue(listBoxPage.HasDirectPageContent);
                    var listBoxExamples = GetRenderedExamples(listBoxPage);
                    Assert.AreEqual(2, listBoxExamples.Count);
                    CollectionAssert.AreEqual(
                        new[] { "ListBox with items defined inline.", "A ListBox with its ItemsSource and Height set." },
                        listBoxExamples.Select(example => example.HeaderText).ToArray());
                    var colorListBox = (ListBox)listBoxExamples[0].ExampleContent;
                    Assert.AreEqual("Color ListBox", AutomationProperties.GetName(colorListBox));
                    Assert.AreEqual(0, colorListBox.SelectedIndex);
                    CollectionAssert.AreEqual(new[] { "Blue", "Green", "Red", "Yellow" }, colorListBox.Items.Cast<ListBoxItem>().Select(item => (string)item.Content).ToArray());
                    var fontListBox = (ListBox)listBoxExamples[1].ExampleContent;
                    Assert.AreEqual(164.0, fontListBox.Height);
                    Assert.AreEqual("Font ListBox", AutomationProperties.GetName(fontListBox));
                    Assert.AreEqual(2, fontListBox.SelectedIndex);
                    CollectionAssert.AreEqual(new[] { "Arial", "Comic Sans MS", "Courier New", "Segoe UI", "Times New Roman" }, fontListBox.ItemsSource.Cast<string>().ToArray());
                });

                var treeViewPage = new ItemPage(GalleryCatalog.FindItem("TreeView"));
                WithRenderedPage(treeViewPage, () =>
                {
                    Assert.IsTrue(treeViewPage.HasDirectPageContent);
                    var treeViewExamples = GetRenderedExamples(treeViewPage);
                    Assert.AreEqual(1, treeViewExamples.Count);
                    Assert.AreEqual("Simple TreeView.", treeViewExamples[0].HeaderText);
                    var treeView = (TreeView)treeViewExamples[0].ExampleContent;
                    Assert.IsTrue(treeView.AllowDrop);
                    Assert.AreEqual("Sample TreeView", AutomationProperties.GetName(treeView));
                    Assert.IsFalse(ScrollViewer.GetCanContentScroll(treeView));
                    var workDocuments = AssertTreeViewItem(treeView.Items, 0, "Work Documents");
                    Assert.IsTrue(workDocuments.IsExpanded);
                    Assert.IsTrue(workDocuments.IsSelected);
                    AssertTreeViewItem(workDocuments.Items, 0, "Feature Schedule");
                    AssertTreeViewItem(workDocuments.Items, 1, "Overall Project Plan");
                    var personalDocuments = AssertTreeViewItem(treeView.Items, 1, "Personal Documents");
                    AssertTreeViewItem(personalDocuments.Items, 0, "Contractor contact info");
                    var homeRemodel = AssertTreeViewItem(personalDocuments.Items, 1, "Home Remodel");
                    AssertTreeViewItem(homeRemodel.Items, 0, "Paint Color Scheme");
                    AssertTreeViewItem(homeRemodel.Items, 1, "Flooring Woodgrain Type");
                    AssertTreeViewItem(homeRemodel.Items, 2, "Kitchen Cabinet Style");
                });

                var dataGridPage = new ItemPage(GalleryCatalog.FindItem("DataGrid"));
                WithRenderedPage(dataGridPage, () =>
                {
                    Assert.IsTrue(dataGridPage.HasDirectPageContent);
                    var dataGridExamples = GetRenderedExamples(dataGridPage);
                    Assert.AreEqual(1, dataGridExamples.Count);
                    Assert.AreEqual("Default DataGrid with ItemsSource.", dataGridExamples[0].HeaderText);
                    var dataGrid = (DataGrid)dataGridExamples[0].ExampleContent;
                    Assert.AreEqual("SampleDataGrid", dataGrid.Name);
                    Assert.AreEqual(400.0, dataGrid.Height);
                    Assert.AreEqual("Sample Data Grid", AutomationProperties.GetName(dataGrid));
                    Assert.AreEqual(50, dataGrid.ItemsSource.Cast<object>().Count());
                });

                var hyperlinkPage = new ItemPage(GalleryCatalog.FindItem("Hyperlink"));
                Assert.IsTrue(hyperlinkPage.HasDirectPageContent);
                var hyperlinkExamples = GetRenderedExamples(hyperlinkPage);
                Assert.AreEqual(1, hyperlinkExamples.Count);
                Assert.AreEqual("A Hyperlink with in-app navigation handling", hyperlinkExamples[0].HeaderText);
                var hyperlinkExample = FindDescendant<ControlExample>((DependencyObject)hyperlinkPage.DirectPageContent);
                StringAssert.Contains(hyperlinkExample.CSharpCode, "e.Handled = true");
                var hyperlinkStack = (StackPanel)hyperlinkExamples[0].ExampleContent;
                var hyperlinkTextBlock = (TextBlock)hyperlinkStack.Children[0];
                Assert.AreEqual(new Thickness(20), hyperlinkTextBlock.Margin);
                var hyperlink = hyperlinkTextBlock.Inlines.OfType<Hyperlink>().Single();
                Assert.AreEqual(new System.Uri("https://www.microsoft.com"), hyperlink.NavigateUri);
                Assert.AreEqual("Hyperlink", hyperlink.Inlines.OfType<Run>().Single().Text);

                var navigationStatus = (TextBlock)hyperlinkStack.Children[1];
                Assert.AreEqual(Visibility.Collapsed, navigationStatus.Visibility);
                var requestNavigateArgs = new RequestNavigateEventArgs(hyperlink.NavigateUri, null)
                {
                    RoutedEvent = Hyperlink.RequestNavigateEvent
                };
                hyperlink.RaiseEvent(requestNavigateArgs);
                Assert.IsTrue(requestNavigateArgs.Handled);
                Assert.AreEqual(Visibility.Visible, navigationStatus.Visibility);
                Assert.AreEqual("Navigation request: https://www.microsoft.com/", navigationStatus.Text);
            });
        }

        [TestMethod]
        public void DesignGuidancePagesMatchWpfGalleryReferenceLayoutDetails()
        {
            WpfTestHost.Run(() =>
            {
                var spacingPage = new ItemPage(GalleryCatalog.FindItem("Spacing"));
                Assert.IsTrue(spacingPage.HasDirectPageContent);
                AssertNoContentPagePaneHook(spacingPage);
                var spacingBody = GetDirectPageBodyStack(spacingPage);
                Assert.AreEqual("Consistent spacing helps create visual harmony and improves the readability and usability of your application.", ((TextBlock)spacingBody.Children[0]).Text);
                var spacingUsage = (TextBlock)spacingBody.Children[1];
                Assert.AreEqual("Use the following spacing values to maintain a consistent layout throughout your app.", spacingUsage.Text);
                Assert.AreEqual(new Thickness(0), spacingUsage.Padding);
                var images = (StackPanel)spacingBody.Children[2];
                Assert.AreEqual(Orientation.Horizontal, images.Orientation);
                Assert.AreEqual(new Thickness(0, 0, 0, 16), images.Margin);
                Assert.AreEqual(2, images.Children.Count);

                var cardsFrame = (Grid)images.Children[0];
                var dialogFrame = (Grid)images.Children[1];
                Assert.AreEqual(VerticalAlignment.Top, cardsFrame.VerticalAlignment);
                Assert.AreEqual(VerticalAlignment.Top, dialogFrame.VerticalAlignment);
                Assert.AreEqual(2, cardsFrame.RowDefinitions.Count);
                Assert.AreEqual(GridUnitType.Auto, cardsFrame.RowDefinitions[0].Height.GridUnitType);
                Assert.AreEqual(GridUnitType.Star, cardsFrame.RowDefinitions[1].Height.GridUnitType);
                Assert.AreEqual(2, dialogFrame.RowDefinitions.Count);
                Assert.AreEqual(GridUnitType.Auto, dialogFrame.RowDefinitions[0].Height.GridUnitType);
                Assert.AreEqual(GridUnitType.Star, dialogFrame.RowDefinitions[1].Height.GridUnitType);
                Assert.AreEqual("Page with cards layout", ((TextBlock)cardsFrame.Children[0]).Text);
                Assert.AreEqual(HorizontalAlignment.Center, ((TextBlock)cardsFrame.Children[0]).HorizontalAlignment);
                Assert.AreEqual(new Thickness(0), ((TextBlock)cardsFrame.Children[0]).Margin);
                Assert.AreEqual(500.0, ((Border)cardsFrame.Children[1]).Height);
                var cardsImage = (Image)((Border)cardsFrame.Children[1]).Child;
                Assert.IsTrue(double.IsNaN(cardsImage.Width));
                Assert.IsTrue(double.IsNaN(cardsImage.Height));
                Assert.AreEqual("Example of spacing in a page with cards layout", AutomationProperties.GetName(cardsImage));
                StringAssert.Contains(((BitmapImage)cardsImage.Source).UriSource.ToString(), "Cards.light.png");

                Assert.AreEqual("Form layout", ((TextBlock)dialogFrame.Children[0]).Text);
                Assert.AreEqual(500.0, ((Border)dialogFrame.Children[1]).Height);
                var dialogImage = (Image)((Border)dialogFrame.Children[1]).Child;
                Assert.AreEqual("Example of spacing in a form layout", AutomationProperties.GetName(dialogImage));
                StringAssert.Contains(((BitmapImage)dialogImage.Source).UriSource.ToString(), "Dialog.light.png");

                var spacingContent = (SpacingPage)spacingPage.DirectPageContent;
                spacingContent.ApplyImageResources(ElementTheme.Dark);
                StringAssert.Contains(((BitmapImage)cardsImage.Source).UriSource.ToString(), "Cards.dark.png");
                StringAssert.Contains(((BitmapImage)dialogImage.Source).UriSource.ToString(), "Dialog.dark.png");

                var spacingTableFrame = (Border)spacingBody.Children[3];
                Assert.AreEqual(new Thickness(0, 10, 0, 10), spacingTableFrame.Margin);
                Assert.AreEqual(new Thickness(16), spacingTableFrame.Padding);
                var spacingTable = (Grid)spacingTableFrame.Child;
                Assert.AreEqual(new Thickness(0), spacingTable.Margin);
                Assert.AreEqual(HorizontalAlignment.Stretch, spacingTable.HorizontalAlignment);
                Assert.AreEqual(0, spacingTable.ColumnDefinitions.Count);
                Assert.AreEqual(8, spacingTable.RowDefinitions.Count);

                var spacingHeader = GetTableRowGrid(spacingTable, 0);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), spacingHeader.Margin);
                Assert.AreEqual(HorizontalAlignment.Stretch, spacingHeader.HorizontalAlignment);
                CollectionAssert.AreEqual(new[] { 100.0, 100.0, 400.0 }, spacingHeader.ColumnDefinitions.Select(column => column.Width.Value).ToArray());
                CollectionAssert.AreEqual(
                    new[] { "Value", "Usage" },
                    spacingHeader.Children.OfType<TextBlock>().OrderBy(Grid.GetColumn).Select(textBlock => textBlock.Text).ToArray());
                CollectionAssert.AreEqual(
                    new[] { new Thickness(16, 0, 0, 0), new Thickness(0) },
                    spacingHeader.Children.OfType<TextBlock>().OrderBy(Grid.GetColumn).Select(textBlock => textBlock.Margin).ToArray());

                var spacingRows = Enumerable.Range(1, 7).Select(row => GetTableRowGrid(spacingTable, row)).ToArray();
                CollectionAssert.AreEqual(Enumerable.Repeat(60.0, 7).ToArray(), spacingRows.Select(row => row.MinHeight).ToArray());
                CollectionAssert.AreEqual(Enumerable.Repeat(HorizontalAlignment.Stretch, 7).ToArray(), spacingRows.Select(row => row.HorizontalAlignment).ToArray());
                CollectionAssert.AreEqual(
                    Enumerable.Repeat(new[] { 90.0, 100.0, 400.0 }, 7).SelectMany(widths => widths).ToArray(),
                    spacingRows.SelectMany(row => row.ColumnDefinitions.Select(column => column.Width.Value)).ToArray());
                Assert.AreEqual("4px", GetTableText(spacingRows[0], 0).Text);
                var firstSpacingBarHost = spacingRows[0].Children.OfType<StackPanel>().Single(panel => Grid.GetColumn(panel) == 1);
                Assert.AreEqual(new Thickness(0, 8, 0, 8), firstSpacingBarHost.Margin);
                Assert.AreEqual(Orientation.Horizontal, firstSpacingBarHost.Orientation);
                Assert.AreEqual(4.0, ((Border)firstSpacingBarHost.Children[0]).Width);
                Assert.AreEqual("48px", GetTableText(spacingRows[6], 0).Text);
                Assert.AreEqual(48.0, ((Border)spacingRows[6].Children.OfType<StackPanel>().Single(panel => Grid.GetColumn(panel) == 1).Children[0]).Width);

                var typographyPage = new ItemPage(GalleryCatalog.FindItem("Typography"));
                Assert.IsTrue(typographyPage.HasDirectPageContent);
                AssertNoContentPagePaneHook(typographyPage);
                var typographyBody = GetDirectPageBodyStack(typographyPage);
                Assert.AreEqual("Type helps provide structure and hierarchy to UI. Use ModernWpf's text styles so the appropriate Segoe family is selected for the current Windows version.", ((TextBlock)typographyBody.Children[0]).Text);
                Assert.AreEqual("Best practice is to use Regular weight for most text, use Semibold for titles.", ((TextBlock)typographyBody.Children[1]).Text);
                var typographyMinimum = (TextBlock)typographyBody.Children[2];
                Assert.AreEqual("The minimum values should be 12px Regular, 14px Semibold.", typographyMinimum.Text);
                Assert.AreEqual(new Thickness(0), typographyMinimum.Padding);
                var typeRampExample = (ControlExample)typographyBody.Children[3];
                var typeRamp = (Grid)typeRampExample.ExampleContent;
                Assert.AreEqual(new Thickness(0), typeRamp.Margin);
                Assert.AreEqual(HorizontalAlignment.Stretch, typeRamp.HorizontalAlignment);
                Assert.AreEqual(0, typeRamp.ColumnDefinitions.Count);
                Assert.AreEqual(8, typeRamp.RowDefinitions.Count);
                var typeRampRows = Enumerable.Range(0, 8).Select(row => GetTableRowGrid(typeRamp, row)).ToArray();
                CollectionAssert.AreEqual(Enumerable.Repeat(HorizontalAlignment.Stretch, 8).ToArray(), typeRampRows.Select(row => row.HorizontalAlignment).ToArray());
                var headers = typeRampRows[0].Children.OfType<TextBlock>().OrderBy(Grid.GetColumn).ToArray();
                CollectionAssert.AreEqual(new[] { "Example", "Variable Font", "Size/Line height", "Style" }, headers.Select(header => header.Text).ToArray());
                Assert.AreEqual(new Thickness(16, 0, 0, 0), headers[0].Margin);
                CollectionAssert.AreEqual(Enumerable.Repeat(new Thickness(0), 3).ToArray(), headers.Skip(1).Select(header => header.Margin).ToArray());
                Assert.AreEqual(new Thickness(0, 0, 0, 24), typeRampRows[0].Margin);

                var bodyStrongStyleName = GetTableText(typeRampRows[3], 3);
                Assert.AreEqual("CaptionTextBlockStyle", bodyStrongStyleName.Text);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), typeRampRows[5].Margin);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), typeRampRows[6].Margin);
                var displayRow = typeRampRows[7];
                Assert.AreEqual(68.0, displayRow.MinHeight);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), displayRow.Margin);
                Assert.AreEqual(5, displayRow.ColumnDefinitions.Count);
                Assert.AreEqual("Consolas", GetTableText(displayRow, 3).FontFamily.Source);

                var geometryPage = new ItemPage(GalleryCatalog.FindItem("Geometry"));
                Assert.IsTrue(geometryPage.HasDirectPageContent);
                AssertNoContentPagePaneHook(geometryPage);
                var geometryBody = GetDirectPageBodyStack(geometryPage);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), geometryBody.Margin);
                Assert.AreEqual(5, geometryBody.Children.Count);
                Assert.AreEqual("Geometry describes the shape, size and position of UI elements on screen.", ((TextBlock)geometryBody.Children[0]).Text);
                Assert.AreEqual("These fundamental design elements help experiences feel coherent across the entire design system.", ((TextBlock)geometryBody.Children[1]).Text);
                var geometryUsage = (TextBlock)geometryBody.Children[2];
                Assert.AreEqual("You can reference built-in corner radii styles using: CornerRadius=\"{StaticResource ControlCornerRadius}\" .", geometryUsage.Text);
                Assert.AreEqual(new Thickness(0, 0, 0, 12), geometryUsage.Margin);
                Assert.AreEqual(new Thickness(0), geometryUsage.Padding);

                var geometryImageHost = (Border)geometryBody.Children[3];
                Assert.AreEqual(500.0, geometryImageHost.Width);
                Assert.AreEqual(300.0, geometryImageHost.Height);
                Assert.AreEqual(HorizontalAlignment.Left, geometryImageHost.HorizontalAlignment);
                var geometryImage = (Image)geometryImageHost.Child;
                Assert.IsTrue(double.IsNaN(geometryImage.Width));
                Assert.IsTrue(double.IsNaN(geometryImage.Height));
                Assert.AreEqual(Stretch.Uniform, geometryImage.Stretch);
                Assert.AreEqual("Example of corner radius.", AutomationProperties.GetName(geometryImage));
                var geometryLightBitmap = (BitmapImage)geometryImage.Source;
                StringAssert.Contains(geometryLightBitmap.UriSource.ToString(), "Geometry.light.png");
                Assert.AreEqual(586, geometryLightBitmap.PixelWidth);
                Assert.AreEqual(315, geometryLightBitmap.PixelHeight);

                var geometryContent = (GeometryPage)geometryPage.DirectPageContent;
                geometryContent.ApplyImageResources(ElementTheme.Dark);
                var geometryDarkBitmap = (BitmapImage)geometryImage.Source;
                StringAssert.Contains(geometryDarkBitmap.UriSource.ToString(), "Geometry.dark.png");
                Assert.AreEqual(585, geometryDarkBitmap.PixelWidth);
                Assert.AreEqual(313, geometryDarkBitmap.PixelHeight);

                var geometryExample = (ControlExample)geometryBody.Children[4];
                Assert.IsNull(geometryExample.HeaderText);
                StringAssert.Contains(geometryExample.XamlCode, "OverlayCornerRadius");
                var cornerRadiusTable = (Grid)geometryExample.ExampleContent;
                AssertCornerRadiusTable(cornerRadiusTable);
            });
        }

        [TestMethod]
        public void DesignGuidanceImagesUseForcedApplicationThemeBeforeLoaded()
        {
            WpfTestHost.Run(() =>
            {
                var previousTheme = ThemeManager.Current.ApplicationTheme;
                SpacingPage spacingPage = null;
                GeometryPage geometryPage = null;

                try
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

                    spacingPage = new SpacingPage(new SpacingPageViewModel());
                    AssertNamedImageSource(spacingPage, "CardImage", "Cards.dark.png");
                    AssertNamedImageSource(spacingPage, "DialogImage", "Dialog.dark.png");

                    geometryPage = new GeometryPage(new GeometryPageViewModel());
                    AssertNamedImageSource(geometryPage, "GeometryImage", "Geometry.dark.png");
                }
                finally
                {
                    UnloadPageForEventCleanup(spacingPage);
                    UnloadPageForEventCleanup(geometryPage);
                    ThemeManager.Current.ApplicationTheme = previousTheme;
                }
            });
        }

        [TestMethod]
        public void ColorPageUsesWpfGallerySelectorAndTextSectionLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Color"));
                Assert.IsTrue(page.HasDirectPageContent);
                AssertDirectPagePane(page);

                var body = GetDirectPageBodyStack(page);
                Assert.AreEqual(3, body.Children.Count);
                Assert.AreEqual(new Thickness(0), ((TextBlock)body.Children[0]).Padding);
                var selector = (ComboBox)body.Children[1];
                var sectionHost = (ContentControl)body.Children[2];

                CollectionAssert.AreEqual(
                    new[] { "Text", "Fill", "Stroke", "Background", "Signal", "HighContrast" },
                    selector.Items.Cast<string>().ToArray());
                Assert.AreEqual(200.0, selector.Width);
                Assert.AreEqual("Page Selector", AutomationProperties.GetName(selector));
                Assert.AreSame(DependencyProperty.UnsetValue, sectionHost.ReadLocalValue(TextElement.FontSizeProperty));

                selector.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, selector));
                WpfTestHost.DoEvents();

                Assert.AreEqual("TextSection", sectionHost.Content.GetType().Name);
                Assert.AreEqual(14.0, TextElement.GetFontSize((DependencyObject)sectionHost.Content));
                var textSection = GetColorSectionStack(sectionHost.Content);
                Assert.AreEqual("Text", GetColorPageExampleTitle(textSection, 0));
                Assert.AreEqual("Accent Text", GetColorPageExampleTitle(textSection, 2));
                Assert.AreEqual("Text On Accent", GetColorPageExampleTitle(textSection, 4));

                var firstTilesPanel = (Border)textSection.Children[1];
                var firstTilesGrid = (Grid)firstTilesPanel.Child;
                Assert.AreEqual(4, firstTilesGrid.ColumnDefinitions.Count);
                Assert.AreEqual("Text / Primary", GetColorTileName(firstTilesGrid.Children[0]));
                Assert.AreEqual("Text / Disabled", GetColorTileName(firstTilesGrid.Children[3]));

                var firstTextTile = (ColorTile)firstTilesGrid.Children[0];
                Assert.AreEqual(new CornerRadius(8, 0, 0, 8), firstTextTile.TileRadius);
                var lastTextTile = (ColorTile)firstTilesGrid.Children[3];
                Assert.AreEqual(new CornerRadius(0, 8, 8, 0), lastTextTile.TileRadius);

                var textOnAccentTiles = GetColorTilesGrid(textSection, 5);
                Assert.AreEqual(2, textOnAccentTiles.ColumnDefinitions.Count);
                Assert.AreEqual("Text on Accent / Secondary", GetColorTileName(textOnAccentTiles.Children[1]));
                Assert.AreEqual(2, Grid.GetColumn(textOnAccentTiles.Children[1]));

                var textOnAccentSelectedTiles = GetColorTilesGrid(textSection, 6);
                Assert.AreEqual(2, textOnAccentSelectedTiles.ColumnDefinitions.Count);
                Assert.AreEqual("Text on Accent / Selected Text", GetColorTileName(textOnAccentSelectedTiles.Children[1]));
                Assert.AreEqual(2, Grid.GetColumn(textOnAccentSelectedTiles.Children[1]));

                selector.SelectedIndex = 1;
                WpfTestHost.DoEvents();
                var fillSection = GetColorSectionStack(sectionHost.Content);
                Assert.AreEqual(17, fillSection.Children.Count);
                Assert.AreEqual("Control Fill", GetColorPageExampleTitle(fillSection, 0));
                Assert.AreEqual("Control Alt Fill", GetColorPageExampleTitle(fillSection, 3));
                Assert.AreEqual("Control Solid", GetColorPageExampleTitle(fillSection, 6));
                Assert.AreEqual("Control Strong Fill", GetColorPageExampleTitle(fillSection, 8));
                Assert.AreEqual("Subtle Fill", GetColorPageExampleTitle(fillSection, 10));
                Assert.AreEqual("Control On Image Fill", GetColorPageExampleTitle(fillSection, 12));
                Assert.AreEqual("Accent Fill", GetColorPageExampleTitle(fillSection, 14));

                var controlFillTiles = GetColorTilesGrid(fillSection, 1);
                Assert.AreEqual(4, controlFillTiles.ColumnDefinitions.Count);
                Assert.AreEqual("Control / Default", GetColorTileName(controlFillTiles.Children[0]));
                Assert.AreEqual("Control / Quartenary", GetColorTileName(controlFillTiles.Children[3]));

                var controlFillSecondRow = GetColorTilesGrid(fillSection, 2);
                Assert.AreEqual(3, controlFillSecondRow.ColumnDefinitions.Count);
                Assert.AreEqual("Control / Disabled", GetColorTileName(controlFillSecondRow.Children[0]));
                Assert.AreEqual("Control / Input Active", GetColorTileName(controlFillSecondRow.Children[2]));

                var accentFillSecondRow = GetColorTilesGrid(fillSection, 16);
                Assert.AreEqual("Accent / Selected Text Background", GetColorTileName(accentFillSecondRow.Children[1]));

                selector.SelectedIndex = 2;
                WpfTestHost.DoEvents();
                var strokeSection = GetColorSectionStack(sectionHost.Content);
                Assert.AreEqual(16, strokeSection.Children.Count);
                Assert.AreEqual("Control Elevation (gradient strokes)", GetColorPageExampleTitle(strokeSection, 0));
                Assert.AreEqual("Control Stroke", GetColorPageExampleTitle(strokeSection, 3));
                Assert.AreEqual("Card Stroke", GetColorPageExampleTitle(strokeSection, 6));
                Assert.AreEqual("Control Strong Stroke", GetColorPageExampleTitle(strokeSection, 8));
                Assert.AreEqual("Surface Stroke", GetColorPageExampleTitle(strokeSection, 10));
                Assert.AreEqual("Divider Stroke", GetColorPageExampleTitle(strokeSection, 12));
                Assert.AreEqual("Focus Stroke", GetColorPageExampleTitle(strokeSection, 14));

                var elevationTiles = GetColorTilesGrid(strokeSection, 1);
                Assert.AreEqual(3, elevationTiles.ColumnDefinitions.Count);
                Assert.AreEqual("Control / Border", GetColorTileName(elevationTiles.Children[0]));
                Assert.AreEqual("Text Control / Border", GetColorTileName(elevationTiles.Children[2]));

                var controlStrokeSecondRow = GetColorTilesGrid(strokeSection, 5);
                Assert.AreEqual(3, controlStrokeSecondRow.ColumnDefinitions.Count);
                Assert.AreEqual("Control Stroke / For Strong Fill When On Image", GetColorTileName(controlStrokeSecondRow.Children[2]));

                var focusTiles = GetColorTilesGrid(strokeSection, 15);
                Assert.AreEqual("Focus / Outer", GetColorTileName(focusTiles.Children[0]));
                Assert.AreEqual("Focus / Inner", GetColorTileName(focusTiles.Children[1]));

                selector.SelectedIndex = 3;
                WpfTestHost.DoEvents();
                var backgroundSection = GetColorSectionStack(sectionHost.Content);
                Assert.AreEqual(20, backgroundSection.Children.Count);
                Assert.AreEqual("Card Background", GetColorPageExampleTitle(backgroundSection, 0));
                Assert.AreEqual("Smoke Background", GetColorPageExampleTitle(backgroundSection, 2));
                Assert.AreEqual("Layer", GetColorPageExampleTitle(backgroundSection, 4));
                Assert.AreEqual("Layer on Acrylic", GetColorPageExampleTitle(backgroundSection, 6));
                Assert.AreEqual("Layer on Mica Base Alt", GetColorPageExampleTitle(backgroundSection, 8));
                Assert.AreEqual("Solid Background", GetColorPageExampleTitle(backgroundSection, 11));
                Assert.AreEqual("Mica Background", GetColorPageExampleTitle(backgroundSection, 14));
                Assert.AreEqual("Acrylic Background", GetColorPageExampleTitle(backgroundSection, 16));
                Assert.AreEqual("Accent Acrylic Background", GetColorPageExampleTitle(backgroundSection, 18));

                var cardTiles = GetColorTilesGrid(backgroundSection, 1);
                Assert.AreEqual(3, cardTiles.ColumnDefinitions.Count);
                Assert.AreEqual("Card Background / Tertiary", GetColorTileName(cardTiles.Children[2]));

                var micaTiles = GetColorTilesGrid(backgroundSection, 15);
                Assert.AreEqual("Mica Background / Base Alt", GetColorTileName(micaTiles.Children[1]));

                var accentAcrylicTiles = GetColorTilesGrid(backgroundSection, 19);
                Assert.AreEqual("Accent Acrylic Background / Default", GetColorTileName(accentAcrylicTiles.Children[1]));

                selector.SelectedIndex = 4;
                WpfTestHost.DoEvents();
                var signalSection = GetColorSectionStack(sectionHost.Content);
                Assert.AreEqual(6, signalSection.Children.Count);
                Assert.AreEqual("System", GetColorPageExampleTitle(signalSection, 0));
                var signalStatusTiles = GetColorTilesGrid(signalSection, 1);
                Assert.AreEqual("System / Success", GetColorTileName(signalStatusTiles.Children[0]));
                Assert.AreEqual("System / Critical", GetColorTileName(signalStatusTiles.Children[2]));
                var signalNeutralTiles = GetColorTilesGrid(signalSection, 3);
                Assert.AreEqual("System / Solid Neutral", GetColorTileName(signalNeutralTiles.Children[2]));
                var signalSolidAttentionTiles = GetColorTilesGrid(signalSection, 5);
                Assert.AreEqual(1, signalSolidAttentionTiles.ColumnDefinitions.Count);
                Assert.AreEqual("System / Solid Attention Background", GetColorTileName(signalSolidAttentionTiles.Children[0]));

                selector.SelectedIndex = 5;
                WpfTestHost.DoEvents();
                var highContrastSection = GetColorSectionStack(sectionHost.Content);
                Assert.AreEqual(9, highContrastSection.Children.Count);
                StringAssert.StartsWith(((TextBlock)highContrastSection.Children[0]).Text, "Below are the default highcontrast themes shown.");
                Assert.AreEqual("Aquatic", ((TextBlock)highContrastSection.Children[1]).Text);
                var aquaticTiles = (Grid)highContrastSection.Children[2];
                Assert.AreEqual(4, aquaticTiles.ColumnDefinitions.Count);
                Assert.AreEqual(2, aquaticTiles.RowDefinitions.Count);
                Assert.AreEqual(8, aquaticTiles.Children.Count);
                Assert.AreEqual("Window Text Color", GetColorTileName(aquaticTiles.Children[0]));
                Assert.AreEqual("Grey Text Color / Disabled", GetColorTileName(aquaticTiles.Children[7]));
                Assert.AreEqual("Night Sky", ((TextBlock)highContrastSection.Children[7]).Text);
                var nightSkyTiles = (Grid)highContrastSection.Children[8];
                Assert.AreEqual("Hotlight Color", GetColorTileName(nightSkyTiles.Children[6]));
            });
        }

        [TestMethod]
        [DataRow("Text", "TextSection", 7, "Text")]
        [DataRow("Fill", "FillSection", 17, "Control Fill")]
        [DataRow("Stroke", "StrokeSection", 16, "Control Elevation (gradient strokes)")]
        [DataRow("Background", "BackgroundSection", 20, "Card Background")]
        [DataRow("Signal", "SignalSection", 6, "System")]
        [DataRow("HighContrast", "HighContrastSection", 9, "Aquatic")]
        public void ColorPageVisualTestCanOpenWpfGallerySubsection(
            string colorSubpage,
            string expectedSectionTypeName,
            int expectedChildCount,
            string expectedFirstVisibleTitle)
        {
            WpfTestHost.Run(() =>
            {
                try
                {
                    GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[]
                    {
                        "--visual-test",
                        "--color-subpage",
                        colorSubpage
                    }));

                    var page = new ColorsPage(new ColorsPageViewModel());
                    var selector = (ComboBox)page.FindName("PageSelector");
                    var sectionHost = (ContentControl)page.FindName("ColorSubpageNavigationFrame");

                    selector.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, selector));
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(colorSubpage, selector.SelectedItem);
                    Assert.AreEqual(expectedSectionTypeName, sectionHost.Content.GetType().Name);
                    Assert.AreEqual(14.0, TextElement.GetFontSize((DependencyObject)sectionHost.Content));

                    var section = GetColorSectionStack(sectionHost.Content);
                    Assert.AreEqual(expectedChildCount, section.Children.Count);

                    if (string.Equals(colorSubpage, "HighContrast", StringComparison.Ordinal))
                    {
                        Assert.AreEqual(14.0, ((TextBlock)section.Children[0]).FontSize);
                        Assert.AreEqual(expectedFirstVisibleTitle, ((TextBlock)section.Children[1]).Text);
                        Assert.AreEqual("Night Sky", ((TextBlock)section.Children[7]).Text);
                    }
                    else
                    {
                        Assert.AreEqual(expectedFirstVisibleTitle, GetColorPageExampleTitle(section, 0));
                    }
                }
                finally
                {
                    GalleryDiagnostics.ResetForTests();
                }
            });
        }

        [TestMethod]
        public void IconographyPageUsesWpfGalleryIconLibraryLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Iconography"));
                Assert.IsTrue(page.HasDirectPageContent);
                AssertNoContentPagePaneHook(page);

                var directPage = (IconsPage)page.DirectPageContent;
                RenderPage(directPage);

                var body = (Grid)directPage.Content;
                Assert.AreEqual(new Thickness(0, 0, 0, 10), body.Margin);
                Assert.AreEqual(6, body.RowDefinitions.Count);

                var instructions = (Expander)body.Children.Cast<UIElement>().Single(child => Grid.GetRow(child) == 1);
                Assert.AreEqual("Instructions on how to use Fluent icons", instructions.Header);
                Assert.IsFalse(instructions.IsExpanded);
                Assert.AreEqual(new Thickness(2, -8, 0, 0), instructions.Margin);

                var libraryTitle = (TextBlock)body.Children.Cast<UIElement>().Single(child => Grid.GetRow(child) == 2);
                Assert.AreEqual("Fluent Icons Library", libraryTitle.Text);

                var searchHost = (Grid)body.Children.Cast<UIElement>().Single(child => Grid.GetRow(child) == 3);
                var searchBox = (TextBox)searchHost.Children[0];
                var searchPlaceholder = (TextBlock)searchHost.Children[1];
                Assert.AreEqual(500.0, searchBox.Width);
                Assert.AreEqual("Search Icons by Name, Tag", AutomationProperties.GetName(searchBox));
                Assert.AreEqual("Search Icons by Name, Tag", searchPlaceholder.Text);
                Assert.AreEqual(Visibility.Visible, searchPlaceholder.Visibility);
                var searchTextBinding = searchBox.GetBindingExpression(TextBox.TextProperty);
                Assert.IsNotNull(searchTextBinding);
                Assert.AreEqual(string.Empty, directPage.ViewModel.SearchText);

                var libraryGrid = (Grid)body.Children.Cast<UIElement>().Single(child => Grid.GetRow(child) == 4);
                Assert.AreEqual(2, libraryGrid.ColumnDefinitions.Count);
                Assert.AreEqual(300.0, libraryGrid.ColumnDefinitions[1].Width.Value);

                var iconsListView = libraryGrid.Children.OfType<ListView>().Single();
                Assert.AreEqual("Icons", AutomationProperties.GetName(iconsListView));
                Assert.IsTrue(double.IsNaN(iconsListView.Height));
                Assert.AreEqual(250, iconsListView.Items.Count);
                Assert.AreEqual(0, iconsListView.SelectedIndex);
                Assert.AreEqual("StopSlideShow", ((IconData)iconsListView.Items[0]).Name);

                var detailsPane = libraryGrid.Children.OfType<Grid>().Single(child => Grid.GetColumn(child) == 1);
                Assert.IsTrue(double.IsNaN(detailsPane.Width));
                Assert.IsTrue(double.IsNaN(detailsPane.Height));
                var detailsStack = (StackPanel)((ScrollViewer)detailsPane.Children[0]).Content;
                var selectedName = (TextBlock)detailsStack.Children[0];
                var selectedGlyph = (TextBlock)detailsStack.Children[1];
                Assert.AreEqual("StopSlideShow", selectedName.Text);
                Assert.AreNotEqual(string.Empty, selectedGlyph.Text);
                Assert.AreEqual("StopSlideShow", ((ContentControl)detailsStack.Children[3]).Content);
                Assert.AreEqual("E620", ((ContentControl)detailsStack.Children[5]).Content);
                Assert.AreEqual("&#xE620;", ((ContentControl)detailsStack.Children[7]).Content);
                Assert.AreEqual("\\xE620", ((ContentControl)detailsStack.Children[9]).Content);

                var firstIconDataPresenter = (ContentControl)detailsStack.Children[3];
                firstIconDataPresenter.ApplyTemplate();
                var copyButton = FindDescendant<Button>(firstIconDataPresenter);
                Assert.IsNotNull(copyButton);
                Assert.AreEqual(ApplicationCommands.Copy, copyButton.Command);
                AssertRelativeSourceAncestorBinding(copyButton, Button.CommandTargetProperty, typeof(Page));

                var tagsItemsControl = (ItemsControl)directPage.FindName("TagsItemsControl");
                Assert.AreEqual("Selected Icon Tags", AutomationProperties.GetName(tagsItemsControl));
                var tagButton = (Button)tagsItemsControl.ItemTemplate.LoadContent();
                AssertRelativeSourceAncestorBinding(tagButton, Button.CommandProperty, typeof(Page), "ViewModel.ApplyTagFilterCommand");

                var pagination = (Grid)body.Children.Cast<UIElement>().Single(child => Grid.GetRow(child) == 5);
                var navigation = (StackPanel)pagination.Children[0];
                var previousButton = (Button)navigation.Children[0];
                var currentPageText = (TextBlock)navigation.Children[1];
                var totalPagesText = (TextBlock)navigation.Children[2];
                var nextButton = (Button)navigation.Children[3];
                Assert.IsFalse(previousButton.IsEnabled);
                Assert.IsTrue(nextButton.IsEnabled);
                Assert.AreEqual("Page 1 of", currentPageText.Text);
                Assert.AreEqual("6", totalPagesText.Text);

                var pageSize = (StackPanel)pagination.Children[1];
                var pageSizeComboBox = (ComboBox)pageSize.Children[1];
                CollectionAssert.AreEqual(new[] { "100", "250", "500", "1000", "All" }, pageSizeComboBox.Items.Cast<string>().ToArray());
                Assert.AreEqual(1, pageSizeComboBox.SelectedIndex);

                searchBox.Text = "GlobalNavButton";
                Assert.AreEqual(Visibility.Hidden, searchPlaceholder.Visibility);
                Assert.AreEqual(string.Empty, directPage.ViewModel.SearchText);
                searchTextBinding.UpdateSource();
                WpfTestHost.DoEvents();
                Assert.AreEqual("GlobalNavButton", directPage.ViewModel.SearchText);
                Assert.AreEqual("GlobalNavButton", selectedName.Text);
                Assert.IsTrue(iconsListView.Items.Count > 0);
                Assert.IsTrue(iconsListView.Items.Count < 250);

                searchBox.Text = string.Empty;
                Assert.AreEqual(Visibility.Visible, searchPlaceholder.Visibility);
                Assert.AreEqual("GlobalNavButton", directPage.ViewModel.SearchText);
                searchTextBinding.UpdateSource();
                pageSizeComboBox.SelectedIndex = 0;
                WpfTestHost.DoEvents();
                Assert.AreEqual(string.Empty, directPage.ViewModel.SearchText);
                Assert.AreEqual(Visibility.Visible, searchPlaceholder.Visibility);
                Assert.AreEqual(100, iconsListView.Items.Count);
                Assert.AreEqual("Page 1 of", currentPageText.Text);
                Assert.AreEqual("15", totalPagesText.Text);
            });
        }

        [TestMethod]
        public void RetainedUserDashboardSourcePageMatchesWpfGalleryReferenceLayoutAndBehavior()
        {
            WpfTestHost.Run(() =>
            {
                Assert.IsNull(GalleryCatalog.FindItem("UserDashboard"));
                var directPage = new UserDashboardPage(new UserDashboardPageViewModel());
                Assert.AreEqual("UserDashboardPage", directPage.Title);
                Assert.IsInstanceOfType(directPage.ViewModel, typeof(UserDashboardPageViewModel));

                Assert.IsNull(directPage.FindName("ContentRootGrid"));
                var root = (Grid)directPage.Content;
                var window = new Window
                {
                    Width = 900,
                    Height = 720,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = directPage
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    Assert.AreEqual(2, root.ColumnDefinitions.Count);
                    Assert.AreEqual(GridLength.Auto, root.ColumnDefinitions[0].Width);
                    Assert.AreEqual(2, root.RowDefinitions.Count);
                    Assert.AreEqual(280.0, root.RowDefinitions[0].MaxHeight);
                    Assert.AreEqual(new GridLength(2, GridUnitType.Star), root.RowDefinitions[1].Height);
                    Assert.AreEqual(string.Empty, root.Name);
                    Assert.AreEqual(string.Empty, AutomationProperties.GetAutomationId(root));
                    Assert.IsNull(root.Style);
                    Assert.AreEqual(14.0, TextElement.GetFontSize(root));

                    var userListGrid = root.Children.OfType<Grid>().Single(child => Grid.GetColumn(child) == 0);
                    var userList = userListGrid.Children.OfType<ListView>().Single();
                    Assert.AreEqual(BindingMode.TwoWay, BindingOperations.GetBinding(userList, ItemsControl.ItemsSourceProperty).Mode);
                    Assert.AreEqual("Users", AutomationProperties.GetName(userList));
                    Assert.AreEqual(300.0, userList.Width);
                    Assert.AreEqual(SelectionMode.Single, userList.SelectionMode);
                    Assert.AreEqual(20, userList.Items.Count);
                    Assert.AreEqual(-1, userList.SelectedIndex);
                    var firstUser = (UserDashboardUser)userList.Items[0];
                    var firstUserItem = (ListViewItem)userList.ItemContainerGenerator.ContainerFromIndex(0);
                    Assert.IsNotNull(firstUserItem);
                    Assert.AreEqual(firstUser.Name, AutomationProperties.GetName(firstUserItem));
                    Assert.AreEqual("Ellipse", FindDescendant<Ellipse>(firstUserItem).Name);
                    var firstUserName = FindTextBlock(firstUserItem, firstUser.Name);
                    Assert.AreEqual(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(firstUserName));
                    Assert.AreEqual(14.0, firstUserName.FontSize);

                    var addUserButton = userListGrid.Children.OfType<Button>().Single();
                    Assert.AreEqual("Add New User", addUserButton.Content);
                    Assert.AreEqual(new Thickness(10), addUserButton.Margin);
                    Assert.AreEqual(HorizontalAlignment.Center, addUserButton.HorizontalAlignment);
                    Assert.AreEqual(14.0, addUserButton.FontSize);

                    var detailsGrid = root.Children.OfType<Grid>().Single(child => Grid.GetColumn(child) == 1);
                    var header = (StackPanel)detailsGrid.Children[0];
                    Assert.AreEqual(Orientation.Horizontal, header.Orientation);
                    Assert.AreEqual(new Thickness(20, 10, 20, 10), header.Margin);
                    Assert.AreEqual(96.0, ((Ellipse)header.Children[0]).Width);

                    var formGrid = (Grid)detailsGrid.Children[1];
                    Assert.AreEqual(Visibility.Collapsed, header.Visibility);
                    Assert.AreEqual(Visibility.Collapsed, formGrid.Visibility);

                    userList.SelectedIndex = 0;
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    var selectedUser = (UserDashboardUser)userList.SelectedItem;
                    Assert.AreSame(firstUser, selectedUser);
                    Assert.AreEqual(Visibility.Visible, header.Visibility);
                    Assert.AreEqual(Visibility.Visible, formGrid.Visibility);
                    Assert.AreEqual(new Thickness(20, 10, 20, 10), formGrid.Margin);
                    var form = (StackPanel)((ScrollViewer)formGrid.Children[0]).Content;
                    Assert.AreEqual(new Thickness(20, 0, 20, 0), form.Margin);

                    var nameGrid = (Grid)form.Children[0];
                    var firstNamePanel = (StackPanel)nameGrid.Children[0];
                    var lastNamePanel = (StackPanel)nameGrid.Children[1];
                    var firstNameBox = (TextBox)firstNamePanel.Children[1];
                    var lastNameBox = (TextBox)lastNamePanel.Children[1];
                    Assert.AreEqual("First Name", ((Label)firstNamePanel.Children[0]).Content);
                    Assert.AreEqual("First Name", AutomationProperties.GetName(firstNameBox));
                    Assert.AreEqual(selectedUser.FirstName, firstNameBox.Text);
                    Assert.AreEqual(UpdateSourceTrigger.Default, BindingOperations.GetBinding(firstNameBox, TextBox.TextProperty).UpdateSourceTrigger);
                    Assert.IsTrue(firstNameBox.IsReadOnly);
                    Assert.AreEqual("Last Name", AutomationProperties.GetName(lastNameBox));
                    Assert.AreEqual(selectedUser.LastName, lastNameBox.Text);

                    var companyBox = (TextBox)form.Children[2];
                    var addressBox = (TextBox)form.Children[4];
                    Assert.AreEqual("Company", AutomationProperties.GetName(companyBox));
                    Assert.AreEqual(selectedUser.Company, companyBox.Text);
                    Assert.AreEqual("Address", AutomationProperties.GetName(addressBox));
                    Assert.AreEqual(selectedUser.Address, addressBox.Text);

                    var ageSlider = (Slider)form.Children[6];
                    Assert.AreEqual("Age", AutomationProperties.GetName(ageSlider));
                    Assert.AreEqual(21.0, ageSlider.Minimum);
                    Assert.AreEqual(62.0, ageSlider.Maximum);
                    Assert.IsTrue(ageSlider.IsSnapToTickEnabled);
                    Assert.AreEqual(BindingMode.Default, BindingOperations.GetBinding(ageSlider, RangeBase.ValueProperty).Mode);
                    Assert.IsFalse(ageSlider.IsEnabled);
                    Assert.AreEqual((double)selectedUser.Age, ageSlider.Value);

                    var datePicker = (DatePicker)form.Children[8];
                    Assert.AreEqual("Date of Joining", AutomationProperties.GetName(datePicker));
                    Assert.IsFalse(datePicker.IsEnabled);
                    Assert.AreEqual(selectedUser.DateOfJoining, datePicker.SelectedDate.GetValueOrDefault());

                    var graduatePanel = (StackPanel)form.Children[9];
                    var graduateCheckBox = (CheckBox)graduatePanel.Children[1];
                    Assert.AreEqual("Is user a new graduate ?", AutomationProperties.GetName(graduateCheckBox));
                    Assert.IsFalse(graduateCheckBox.IsEnabled);
                    Assert.AreEqual(selectedUser.IsNewGraduate, graduateCheckBox.IsChecked.GetValueOrDefault());

                    var commands = (StackPanel)form.Children[10];
                    var savedStatus = (TextBlock)commands.Children[0];
                    var deletedStatus = (TextBlock)commands.Children[1];
                    var editButton = (Button)commands.Children[2];
                    var deleteButton = (Button)commands.Children[3];
                    var saveButton = (Button)commands.Children[4];
                    var cancelButton = (Button)commands.Children[5];
                    var savedStatusRuns = savedStatus.Inlines.OfType<Run>().ToArray();
                    Assert.AreEqual(1, savedStatusRuns.Length);
                    Assert.AreEqual("Saved!", savedStatusRuns[0].Text.Trim());
                    Assert.AreEqual(Visibility.Collapsed, savedStatus.Visibility);
                    var deletedStatusRuns = deletedStatus.Inlines
                        .OfType<Run>()
                        .Where(run => !string.IsNullOrWhiteSpace(run.Text) ||
                            BindingOperations.GetBinding(run, Run.TextProperty) != null)
                        .ToArray();
                    Assert.AreEqual(3, deletedStatusRuns.Length);
                    Assert.AreEqual("User", deletedStatusRuns[0].Text);
                    Assert.AreEqual(string.Empty, deletedStatusRuns[1].Text);
                    Assert.AreEqual("Deleted!", deletedStatusRuns[2].Text);
                    var deletedNameBinding = BindingOperations.GetBinding(deletedStatusRuns[1], Run.TextProperty);
                    Assert.IsNotNull(deletedNameBinding);
                    Assert.AreEqual("ViewModel.DeletedName", deletedNameBinding.Path.Path);
                    Assert.AreEqual(BindingMode.OneWay, deletedNameBinding.Mode);
                    Assert.IsNull(typeof(UserDashboardPageViewModel).GetProperty("DeletedStatusText"));
                    Assert.AreEqual(Visibility.Collapsed, deletedStatus.Visibility);
                    Assert.AreEqual("Edit", editButton.Content);
                    Assert.AreEqual("Delete", deleteButton.Content);
                    Assert.AreEqual(Visibility.Collapsed, saveButton.Visibility);
                    Assert.AreEqual(Visibility.Collapsed, cancelButton.Visibility);

                    editButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsFalse(firstNameBox.IsReadOnly);
                    Assert.IsTrue(ageSlider.IsEnabled);
                    Assert.IsTrue(datePicker.IsEnabled);
                    Assert.IsTrue(graduateCheckBox.IsEnabled);
                    Assert.AreEqual(Visibility.Collapsed, editButton.Visibility);
                    Assert.AreEqual(Visibility.Visible, saveButton.Visibility);

                    cancelButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    WpfTestHost.DoEvents();
                    Assert.IsTrue(firstNameBox.IsReadOnly);
                    Assert.IsFalse(ageSlider.IsEnabled);
                    Assert.AreEqual(Visibility.Visible, editButton.Visibility);
                    Assert.AreEqual(Visibility.Collapsed, saveButton.Visibility);

                    directPage.Width = 700;
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(240.0, userList.Width);
                    Assert.AreEqual(new Thickness(-10, 0, -20, 0), detailsGrid.Margin);
                    Assert.AreEqual(Orientation.Vertical, header.Orientation);
                    Assert.AreEqual(1, Grid.GetRow(lastNamePanel));
                    Assert.AreEqual(2, Grid.GetColumnSpan(firstNamePanel));

                    directPage.Width = 500;
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    Assert.AreSame(DependencyProperty.UnsetValue, userList.ReadLocalValue(FrameworkElement.WidthProperty));
                    Assert.AreEqual(1, Grid.GetRow(detailsGrid));
                    Assert.AreEqual(2, Grid.GetColumnSpan(detailsGrid));
                    Assert.AreEqual(HorizontalAlignment.Right, addUserButton.HorizontalAlignment);
                    Assert.AreEqual(Orientation.Horizontal, header.Orientation);
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                }
            });
        }

        private static string GetColorPageExampleTitle(StackPanel section, int childIndex)
        {
            var colorPageExample = section.Children[childIndex] as ColorPageExample;
            if (colorPageExample != null)
            {
                return colorPageExample.Title;
            }

            var example = (Border)section.Children[childIndex];
            var grid = (Grid)example.Child;
            return ((TextBlock)grid.Children[0]).Text;
        }

        private static StackPanel GetColorSectionStack(object content)
        {
            var stack = content as StackPanel;
            if (stack != null)
            {
                return stack;
            }

            var userControl = content as UserControl;
            if (userControl != null)
            {
                return (StackPanel)userControl.Content;
            }

            return (StackPanel)((Page)content).Content;
        }

        private static Grid GetColorTilesGrid(StackPanel section, int childIndex)
        {
            var tilesPanel = (Border)section.Children[childIndex];
            return (Grid)tilesPanel.Child;
        }

        private static string GetColorTileName(UIElement element)
        {
            var tile = element as ColorTile;
            if (tile != null)
            {
                return tile.ColorName;
            }

            return AutomationProperties.GetName(element);
        }

        private static TextBlock FindTextBlock(DependencyObject root, string text)
        {
            if (root == null)
            {
                return null;
            }

            var textBlock = root as TextBlock;
            if (textBlock != null && textBlock.Text == text)
            {
                return textBlock;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindTextBlock(VisualTreeHelper.GetChild(root, i), text);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static T FindDescendant<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root == null)
            {
                return null;
            }

            var element = root as T;
            if (element != null)
            {
                return element;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var result = FindDescendant<T>(VisualTreeHelper.GetChild(root, i));
                if (result != null)
                {
                    return result;
                }
            }

            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                var dependencyObject = child as DependencyObject;
                if (dependencyObject == null)
                {
                    continue;
                }

                var result = FindDescendant<T>(dependencyObject);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void RenderPage(FrameworkElement page)
        {
            var window = new Window
            {
                Width = 1024,
                Height = 768,
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
            }
            finally
            {
                window.Content = null;
                window.Close();
                WpfTestHost.DoEvents();
            }
        }

        private static void AssertRenderedPageHeader<TViewModel>(
            Page page,
            TViewModel viewModel,
            string expectedTitle,
            string expectedDescription)
        {
            RenderPage(page);
            Assert.AreSame(viewModel, page.GetType().GetProperty("ViewModel").GetValue(page, null));
            Assert.AreSame(page, page.DataContext);

            var pageHeader = FindDescendant<PageHeader>(page);
            Assert.IsNotNull(pageHeader);
            Assert.AreEqual(expectedTitle, pageHeader.Title);
            if (string.IsNullOrEmpty(expectedDescription))
            {
                Assert.IsTrue(string.IsNullOrEmpty(pageHeader.Description));
            }
            else
            {
                Assert.AreEqual(expectedDescription, pageHeader.Description);
            }
        }

        private static IEnumerable<T> FindDescendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root == null)
            {
                yield break;
            }

            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                var dependencyObject = child as DependencyObject;
                if (dependencyObject == null)
                {
                    continue;
                }

                var element = dependencyObject as T;
                if (element != null)
                {
                    yield return element;
                }

                foreach (var descendant in FindDescendants<T>(dependencyObject))
                {
                    yield return descendant;
                }
            }
        }

        private static TextBlock GetTableText(Grid row, int column)
        {
            return row.Children.OfType<TextBlock>().Single(textBlock => Grid.GetColumn(textBlock) == column);
        }

        private static Grid GetTableRowGrid(Grid table, int row)
        {
            var frame = table.Children.OfType<StackPanel>().Single(panel => Grid.GetRow(panel) == row);
            return frame.Children.OfType<Grid>().Single();
        }

        private static StackPanel GetDirectPageBodyStack(ItemPage page)
        {
            var scrollViewer = FindDescendants<ScrollViewer>((DependencyObject)page.DirectPageContent)
                .FirstOrDefault(candidate => candidate.Content is StackPanel);
            Assert.IsNotNull(scrollViewer, page.UniqueId);
            return (StackPanel)scrollViewer.Content;
        }

        private static void AssertDirectPagePane(ItemPage page)
        {
            var pane = ((FrameworkElement)page.DirectPageContent).FindName("ContentPagePane") as FrameworkElement;
            Assert.IsNotNull(pane, page.UniqueId);
            Assert.AreEqual("ContentPagePane", pane.Name);
            Assert.IsTrue(double.IsNaN(pane.Height), page.UniqueId);
        }

        private static void AssertNoContentPagePaneHook(ItemPage page)
        {
            var pane = ((FrameworkElement)page.DirectPageContent).FindName("ContentPagePane") as FrameworkElement;
            Assert.IsNull(pane, page.UniqueId);
        }

        private static void AssertNamedImageSource(FrameworkElement root, string name, string fileName)
        {
            var image = root.FindName(name) as Image;
            Assert.IsNotNull(image, name);
            var bitmap = image.Source as BitmapImage;
            Assert.IsNotNull(bitmap, name);
            StringAssert.Contains(bitmap.UriSource.ToString(), fileName);
        }

        private static void UnloadPageForEventCleanup(Page page)
        {
            if (page == null)
            {
                return;
            }

            var window = new Window
            {
                Width = 1,
                Height = 1,
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
                window.Content = null;
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
            }
        }

        private static string[] ProductSignatures(IEnumerable<Product> products)
        {
            return products
                .Select(product => string.Format(
                    "{0}|{1}|{2}|{3:R}|{4}",
                    product.ProductId,
                    product.ProductCode,
                    product.ProductName,
                    product.UnitPrice,
                    product.UnitsInStock))
                .ToArray();
        }

        private static string[] PersonSignatures(IEnumerable<Person> persons)
        {
            return persons
                .Select(person => person.FirstName + "|" + person.LastName + "|" + person.Company)
                .ToArray();
        }

        private static string[] UserDashboardSignatures(IEnumerable<UserDashboardUser> users)
        {
            return users
                .Select(user => string.Format(
                    "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                    user.ImageId,
                    user.FirstName,
                    user.LastName,
                    user.Company,
                    user.Address,
                    user.Age,
                    user.DateOfJoining.Ticks,
                    user.IsNewGraduate))
                .ToArray();
        }

        private static void AssertGlyphMenuItem(MenuItem item, string name, string glyph)
        {
            Assert.AreEqual(name, AutomationProperties.GetName(item));
            Assert.AreEqual(name, item.Tag);
            var header = (TextBlock)item.Header;
            Assert.AreEqual(glyph, header.Text);
            Assert.AreEqual(12.0, header.FontSize);
            Assert.IsFalse(header.Focusable);
        }

        private static void AssertTabItem(TabItem tabItem, string headerText, string automationName, string contentText, bool isSelected)
        {
            Assert.AreEqual(automationName, AutomationProperties.GetName(tabItem));
            Assert.AreEqual(isSelected, tabItem.IsSelected);
            var header = (StackPanel)tabItem.Header;
            Assert.AreEqual(Orientation.Horizontal, header.Orientation);
            Assert.AreEqual(headerText, ((TextBlock)header.Children[0]).Text);
            var contentGrid = (Grid)tabItem.Content;
            var content = contentGrid.Children.OfType<TextBlock>().Single();
            Assert.AreEqual(new Thickness(12), content.Margin);
            Assert.AreEqual(contentText, content.Text);
        }

        private static TreeViewItem AssertTreeViewItem(ItemCollection items, int index, string header)
        {
            var item = (TreeViewItem)items[index];
            Assert.AreEqual(header, item.Header);
            return item;
        }

        private static void AssertCornerRadiusTable(Grid table)
        {
            Assert.AreEqual(HorizontalAlignment.Stretch, table.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0), table.Margin);
            Assert.AreEqual(0, table.ColumnDefinitions.Count);
            Assert.AreEqual(4, table.RowDefinitions.Count);

            var headerFrame = table.Children.OfType<StackPanel>().Single(panel => Grid.GetRow(panel) == 0);
            var header = (Grid)headerFrame.Children[0];
            Assert.AreEqual(new Thickness(0, 0, 0, 24), header.Margin);
            AssertOfficialGeometryTableColumns(header);

            CollectionAssert.AreEqual(
                new[] { "Corner radius", "Usage", "Style" },
                header.Children.OfType<TextBlock>().OrderBy(Grid.GetColumn).Select(textBlock => textBlock.Text).ToArray());

            CollectionAssert.AreEqual(
                Enumerable.Repeat(new Thickness(16, 0, 0, 0), 3).ToArray(),
                header.Children.OfType<TextBlock>().OrderBy(Grid.GetColumn).Select(textBlock => textBlock.Margin).ToArray());

            AssertCornerRadiusRow(table, 1, "8px", new CornerRadius(8), "Top-level containers such as app windows, flyouts, cards and dialogs.", "OverlayCornerRadius");
            AssertCornerRadiusRow(table, 2, "4px", new CornerRadius(4), "In-page elements such as controls and list backplates.", "ControlCornerRadius");
            AssertCornerRadiusRow(table, 3, "0px", new CornerRadius(0), "Straight edges that intersect with other straight edges.", "N/A");
        }

        private static void AssertCornerRadiusRow(Grid table, int row, string radiusText, CornerRadius radius, string usage, string styleName)
        {
            var rowFrame = table.Children.OfType<StackPanel>().Single(panel => Grid.GetRow(panel) == row);
            var rowBorder = (Border)rowFrame.Children[0];
            var rowGrid = (Grid)rowBorder.Child;
            Assert.AreEqual(60.0, rowGrid.MinHeight);
            AssertOfficialGeometryTableColumns(rowGrid);

            var sample = rowGrid.Children.OfType<StackPanel>().Single();
            Assert.AreEqual(Orientation.Horizontal, sample.Orientation);
            var shape = (Border)sample.Children[0];
            Assert.AreEqual(20.0, shape.Width);
            Assert.AreEqual(20.0, shape.Height);
            Assert.AreEqual(new Thickness(16, 0, 0, 0), shape.Margin);
            Assert.AreEqual(radius, shape.CornerRadius);
            Assert.AreEqual(radiusText, ((TextBlock)sample.Children[1]).Text);

            var usageText = rowGrid.Children.OfType<TextBlock>().Single(textBlock => Grid.GetColumn(textBlock) == 1);
            var styleText = rowGrid.Children.OfType<TextBlock>().Single(textBlock => Grid.GetColumn(textBlock) == 2);
            Assert.AreEqual(new Thickness(16, 0, 0, 0), usageText.Margin);
            Assert.AreEqual(new Thickness(16, 0, 0, 0), styleText.Margin);
            Assert.AreEqual(usage, usageText.Text);
            Assert.AreEqual(styleName, styleText.Text);
        }

        private static void AssertOfficialGeometryTableColumns(Grid grid)
        {
            Assert.AreEqual(3, grid.ColumnDefinitions.Count);
            Assert.AreEqual(new GridLength(148), grid.ColumnDefinitions[0].Width);
            Assert.AreEqual(new GridLength(400), grid.ColumnDefinitions[1].Width);
            Assert.AreEqual(GridUnitType.Star, grid.ColumnDefinitions[2].Width.GridUnitType);
            Assert.AreEqual(1.0, grid.ColumnDefinitions[2].Width.Value);
        }

        private static void WithRenderedPage(ItemPage page, Action assertions)
        {
            var window = new Window
            {
                Width = 1024,
                Height = 768,
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
                assertions();
            }
            finally
            {
                window.Content = null;
                window.Close();
                WpfTestHost.DoEvents();
            }
        }

        private sealed class RenderedExample
        {
            public RenderedExample(string headerText, object exampleContent, string xamlCode, Thickness margin, string automationId)
            {
                HeaderText = headerText;
                ExampleContent = exampleContent;
                XamlCode = xamlCode;
                Margin = margin;
                AutomationId = automationId;
            }

            public string HeaderText { get; }

            public object ExampleContent { get; }

            public string XamlCode { get; }

            public Thickness Margin { get; }

            public string AutomationId { get; }
        }

        private static IReadOnlyList<RenderedExample> GetRenderedExamples(ItemPage page)
        {
            if (page.HasDirectPageContent)
            {
                return FindDescendants<ControlExample>((DependencyObject)page.DirectPageContent)
                    .Select(example => new RenderedExample(example.HeaderText, example.ExampleContent, example.XamlCode, example.Margin, AutomationProperties.GetAutomationId(example)))
                    .ToArray();
            }

            return page.Examples
                .Select(example => new RenderedExample(example.HeaderText, example.ExampleContent, example.XamlCode, example.Margin, string.Empty))
                .ToArray();
        }

        private static void AssertExampleMargins(string uniqueId, params Thickness[] expectedMargins)
        {
            var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var examples = GetRenderedExamples(page);
            Assert.AreEqual(expectedMargins.Length, examples.Count, uniqueId);
            for (var i = 0; i < expectedMargins.Length; i++)
            {
                Assert.AreEqual(expectedMargins[i], examples[i].Margin, uniqueId + " example " + i);
            }
        }

        private static void AssertBasicInputViewModel<TPage, TViewModel>(string uniqueId, string expectedTitle)
            where TPage : FrameworkElement
            where TViewModel : BasicInputPageViewModelBase
        {
            var itemPage = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var directPage = (TPage)itemPage.DirectPageContent;
            var viewModel = (TViewModel)directPage.GetType().GetProperty("ViewModel").GetValue(directPage, null);

            AssertObservablePageState(viewModel, expectedTitle, string.Empty, uniqueId);
            Assert.AreEqual(uniqueId + "PageViewModel", viewModel.GetType().Name, uniqueId);
        }

        private static void AssertCollectionsViewModel<TPage, TViewModel>(string uniqueId, string expectedTitle)
            where TPage : FrameworkElement
            where TViewModel : CollectionsPageViewModelBase
        {
            var itemPage = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var directPage = (TPage)itemPage.DirectPageContent;
            var viewModel = (TViewModel)directPage.GetType().GetProperty("ViewModel").GetValue(directPage, null);

            AssertObservablePageState(viewModel, expectedTitle, string.Empty, uniqueId);
            Assert.AreEqual(uniqueId + "PageViewModel", viewModel.GetType().Name, uniqueId);
        }

        private static void AssertSystemViewModel<TPage, TViewModel>(
            string uniqueId,
            string expectedTitle,
            string expectedDescription)
            where TPage : FrameworkElement
            where TViewModel : SystemPageViewModelBase
        {
            var itemPage = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var directPage = (TPage)itemPage.DirectPageContent;
            var viewModel = (TViewModel)directPage.GetType().GetProperty("ViewModel").GetValue(directPage, null);

            AssertObservablePageState(viewModel, expectedTitle, expectedDescription, uniqueId);
            Assert.AreEqual(uniqueId + "PageViewModel", viewModel.GetType().Name, uniqueId);
        }

        private static void AssertWpfGalleryPageViewModel<TPage, TViewModel>(
            string uniqueId,
            string expectedTitle,
            string expectedDescription,
            string expectedViewModelTypeName = null)
            where TPage : FrameworkElement
            where TViewModel : WpfGalleryPageViewModel
        {
            var itemPage = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var directPage = (TPage)itemPage.DirectPageContent;
            var viewModel = (TViewModel)directPage.GetType().GetProperty("ViewModel").GetValue(directPage, null);

            AssertObservablePageState(viewModel, expectedTitle, expectedDescription, uniqueId);
            Assert.AreEqual(expectedViewModelTypeName ?? uniqueId + "PageViewModel", viewModel.GetType().Name, uniqueId);
        }

        private static void AssertObservablePageState(
            WpfGalleryPageViewModel viewModel,
            string expectedTitle,
            string expectedDescription,
            string uniqueId)
        {
            Assert.AreEqual(expectedTitle, viewModel.PageTitle, uniqueId);
            Assert.AreEqual(expectedDescription, viewModel.PageDescription, uniqueId);

            var changedProperties = new List<string>();
            viewModel.PropertyChanged += (sender, args) => changedProperties.Add(args.PropertyName);
            viewModel.PageTitle = expectedTitle + " Updated";
            viewModel.PageDescription = (expectedDescription ?? string.Empty) + " Updated";

            Assert.AreEqual(expectedTitle + " Updated", viewModel.PageTitle, uniqueId);
            Assert.AreEqual((expectedDescription ?? string.Empty) + " Updated", viewModel.PageDescription, uniqueId);
            CollectionAssert.Contains(changedProperties, nameof(WpfGalleryPageViewModel.PageTitle), uniqueId);
            CollectionAssert.Contains(changedProperties, nameof(WpfGalleryPageViewModel.PageDescription), uniqueId);
        }

        private static void AssertWpfGalleryPageRoot<TPage>(string uniqueId, string expectedTitle = null)
            where TPage : Page
        {
            var itemPage = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var directPage = itemPage.DirectPageContent;

            Assert.IsInstanceOfType(directPage, typeof(TPage), uniqueId);
            Assert.AreEqual(expectedTitle ?? uniqueId + "Page", ((Page)directPage).Title, uniqueId);
        }

        private static void AssertSettingsSectionHeader(TextBlock header, string expectedText)
        {
            Assert.AreEqual(expectedText, header.Text);
            Assert.AreEqual(new Thickness(10), header.Margin);
            Assert.AreEqual(14.0, header.FontSize);
            Assert.AreEqual(FontWeights.SemiBold, header.FontWeight);
        }

        private static string GetHyperlinkText(Hyperlink hyperlink)
        {
            return new TextRange(hyperlink.ContentStart, hyperlink.ContentEnd).Text.Trim();
        }

        private static void AssertBindingPath(DependencyObject target, DependencyProperty property, string expectedPath)
        {
            var expression = BindingOperations.GetBindingExpression(target, property);
            Assert.IsNotNull(expression);
            Assert.AreEqual(expectedPath, expression.ParentBinding.Path.Path);
        }

        private static void AssertRelativeSourceAncestorBinding(
            DependencyObject target,
            DependencyProperty property,
            Type ancestorType,
            string expectedPath = null)
        {
            var binding = BindingOperations.GetBinding(target, property);
            Assert.IsNotNull(binding);
            Assert.IsNotNull(binding.RelativeSource);
            Assert.AreEqual(RelativeSourceMode.FindAncestor, binding.RelativeSource.Mode);
            Assert.AreEqual(ancestorType, binding.RelativeSource.AncestorType);
            if (expectedPath != null)
            {
                Assert.AreEqual(expectedPath, binding.Path.Path);
            }
        }

        private static void AssertDirectBindingPath(
            DependencyObject target,
            DependencyProperty property,
            string expectedPath,
            BindingMode? expectedMode = null)
        {
            var binding = BindingOperations.GetBinding(target, property);
            Assert.IsNotNull(binding);
            Assert.AreEqual(expectedPath, binding.Path.Path);
            Assert.IsNull(binding.RelativeSource);
            Assert.IsNull(binding.ElementName);
            Assert.IsNull(binding.Source);
            if (expectedMode.HasValue)
            {
                Assert.AreEqual(expectedMode.Value, binding.Mode);
            }
        }

        private static void AssertGridExample(Grid grid, double height, string[] expectedTexts)
        {
            Assert.AreEqual(height, grid.Height);
            Assert.AreEqual(3, grid.RowDefinitions.Count);
            Assert.AreEqual(3, grid.ColumnDefinitions.Count);

            var borders = grid.Children.OfType<Border>().ToArray();
            Assert.IsTrue(borders.Length >= expectedTexts.Length);
            var texts = borders.Select(border => ((TextBlock)border.Child).Text).ToArray();
            foreach (var expectedText in expectedTexts)
            {
                Assert.IsTrue(texts.Contains(expectedText), expectedText);
            }

            foreach (var border in borders)
            {
                Assert.AreEqual(new Thickness(5), border.Margin);
                Assert.AreEqual(new Thickness(10), border.Padding);
            }
        }

        private static void AssertStackPanelExample(StackPanel stackPanel, Orientation orientation)
        {
            Assert.AreEqual(orientation, stackPanel.Orientation);
            var rectangles = stackPanel.Children.OfType<Rectangle>().ToArray();
            Assert.AreEqual(3, rectangles.Length);
            CollectionAssert.AreEqual(new[] { Brushes.CornflowerBlue, Brushes.LightCoral, Brushes.MediumSeaGreen }, rectangles.Select(rectangle => rectangle.Fill).ToArray());
            foreach (var rectangle in rectangles)
            {
                Assert.AreEqual(100.0, rectangle.Width);
                Assert.AreEqual(30.0, rectangle.Height);
                Assert.AreEqual(new Thickness(5), rectangle.Margin);
            }
        }

        private static void AssertButtonResultExample(StackPanel stackPanel, string expectedButtonContent, string expectedOutputText)
        {
            Assert.AreEqual(2, stackPanel.Children.Count);
            Assert.AreEqual(expectedButtonContent, ((Button)stackPanel.Children[0]).Content);
            var output = (TextBlock)stackPanel.Children[1];
            Assert.AreEqual(expectedOutputText, output.Text);
            Assert.AreEqual(TextWrapping.Wrap, output.TextWrapping);
        }

        private static void AssertMessageBoxSelectorExample(Grid grid, string expectedLabel, string expectedButtonName, string expectedComboBoxName, string expectedOutputText, string[] expectedItems)
        {
            Assert.AreEqual(2, grid.ColumnDefinitions.Count);
            Assert.AreEqual(GridUnitType.Star, grid.ColumnDefinitions[0].Width.GridUnitType);
            Assert.AreEqual(GridUnitType.Auto, grid.ColumnDefinitions[1].Width.GridUnitType);

            var left = (StackPanel)grid.Children.OfType<StackPanel>().Single(stackPanel => Grid.GetColumn(stackPanel) == 0);
            var button = (Button)left.Children[0];
            Assert.AreEqual("Show MessageBox", button.Content);
            Assert.AreEqual(expectedButtonName, AutomationProperties.GetName(button));
            var output = (TextBlock)left.Children[1];
            Assert.AreEqual(expectedOutputText, output.Text);
            Assert.AreEqual(TextWrapping.Wrap, output.TextWrapping);

            var right = (StackPanel)grid.Children.OfType<StackPanel>().Single(stackPanel => Grid.GetColumn(stackPanel) == 1);
            Assert.AreEqual(new Thickness(10, 0, 0, 0), right.Margin);
            Assert.AreEqual(expectedLabel, ((TextBlock)right.Children[0]).Text);
            Assert.AreEqual(new Thickness(0, 0, 0, 5), ((TextBlock)right.Children[0]).Margin);

            var comboBox = (ComboBox)right.Children[1];
            Assert.AreEqual(expectedComboBoxName, AutomationProperties.GetName(comboBox));
            Assert.AreEqual(150.0, comboBox.MinWidth);
            Assert.AreEqual(0, comboBox.SelectedIndex);
            CollectionAssert.AreEqual(expectedItems, comboBox.Items.Cast<object>().Select(GetComboBoxItemText).ToArray());
        }

        private static string GetComboBoxItemText(object item)
        {
            var comboBoxItem = item as ComboBoxItem;
            if (comboBoxItem != null)
            {
                return comboBoxItem.Content == null ? string.Empty : comboBoxItem.Content.ToString();
            }

            return item == null ? string.Empty : item.ToString();
        }

        private static void AssertGalleryComboBox(ComboBox comboBox, string automationName)
        {
            Assert.AreEqual(200.0, comboBox.MinWidth);
            Assert.AreEqual(HorizontalAlignment.Left, comboBox.HorizontalAlignment);
            Assert.AreEqual(automationName, AutomationProperties.GetName(comboBox));
        }

        private static void AssertRadioButtons(RadioButton[] radioButtons, string automationNamePrefix, string groupName, FlowDirection flowDirection)
        {
            Assert.AreEqual(3, radioButtons.Length);
            for (var i = 0; i < radioButtons.Length; i++)
            {
                Assert.AreEqual("Option " + (i + 1), radioButtons[i].Content);
                Assert.AreEqual(groupName, radioButtons[i].GroupName);
                Assert.AreEqual(flowDirection, radioButtons[i].FlowDirection);
                Assert.AreEqual(automationNamePrefix + " Radio Option " + (i + 1), AutomationProperties.GetName(radioButtons[i]));
            }

            Assert.AreEqual(true, radioButtons[0].IsChecked);
        }

        private static void RaiseGotKeyboardFocus(RadioButton radioButton)
        {
            radioButton.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 0, null, radioButton)
            {
                RoutedEvent = Keyboard.GotKeyboardFocusEvent
            });
        }

        private static void AssertSliderExample(RenderedExample example, string automationName, double minimum, double maximum, double value, double tickFrequency, TickPlacement tickPlacement, Orientation orientation, string automationId = "")
        {
            var grid = (Grid)example.ExampleContent;
            Assert.AreEqual(2, grid.ColumnDefinitions.Count);
            Assert.AreEqual(GridUnitType.Star, grid.ColumnDefinitions[0].Width.GridUnitType);
            Assert.AreEqual(GridUnitType.Auto, grid.ColumnDefinitions[1].Width.GridUnitType);

            var slider = grid.Children.OfType<Slider>().Single();
            Assert.AreEqual(200.0, slider.Width);
            Assert.IsTrue(double.IsNaN(slider.Height));
            Assert.AreEqual(new Thickness(0), slider.Margin);
            Assert.AreEqual(HorizontalAlignment.Left, slider.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, slider.VerticalAlignment);
            Assert.AreEqual(automationName, AutomationProperties.GetName(slider));
            Assert.AreEqual(automationId, AutomationProperties.GetAutomationId(slider));
            Assert.IsTrue(slider.IsSnapToTickEnabled);
            Assert.AreEqual(minimum, slider.Minimum);
            Assert.AreEqual(maximum, slider.Maximum);
            Assert.AreEqual(value, slider.Value);
            Assert.AreEqual(tickFrequency, slider.TickFrequency);
            Assert.AreEqual(tickPlacement, slider.TickPlacement);
            Assert.AreEqual(orientation, slider.Orientation);

            var outputGrid = grid.Children.OfType<Grid>().Single(child => Grid.GetColumn(child) == 1);
            var outputStack = (StackPanel)outputGrid.Children[0];
            Assert.AreEqual(VerticalAlignment.Center, outputStack.VerticalAlignment);
            var outputLabel = (TextBlock)outputStack.Children[0];
            var outputValue = (TextBlock)outputStack.Children[1];
            Assert.AreEqual("Output:", outputLabel.Text);
            Assert.AreEqual(value.ToString("0"), outputValue.Text);

            slider.Value = minimum == maximum ? value : minimum + 1;
            Assert.AreEqual(slider.Value.ToString("0"), outputValue.Text);
        }

        private static void AssertStyleSetter(Style style, DependencyProperty property, object expectedValue)
        {
            var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
            Assert.AreEqual(expectedValue, setter.Value);
        }

        private static void AssertGridViewColumn(GridViewColumn column, string header, double width, string path)
        {
            Assert.AreEqual(header, column.Header);
            Assert.AreEqual(width, column.Width);
            var binding = (Binding)column.DisplayMemberBinding;
            Assert.AreEqual(path, binding.Path.Path);
        }
    }
}
