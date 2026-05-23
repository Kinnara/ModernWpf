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
        [DynamicData(nameof(CatalogItems), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetDisplayName))]
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
        public void DistinctModernWpfPagesDoNotUseWpfGalleryAliasContent()
        {
            WpfTestHost.Run(() =>
            {
                var calendarPage = new ItemPage(GalleryCatalog.FindItem("Calendar"));
                var calendarViewPage = new ItemPage(GalleryCatalog.FindItem("CalendarView"));
                var richTextEditPage = new ItemPage(GalleryCatalog.FindItem("RichTextEdit"));
                var richEditBoxPage = new ItemPage(GalleryCatalog.FindItem("RichEditBox"));

                Assert.IsTrue(calendarPage.UsesWpfGalleryPageMode);
                Assert.IsFalse(calendarPage.ShowCatalogDetails);
                Assert.IsFalse(calendarPage.ShowPageDescription);

                Assert.IsFalse(calendarViewPage.UsesWpfGalleryPageMode);
                Assert.IsTrue(calendarViewPage.ShowCatalogDetails);
                Assert.IsTrue(calendarViewPage.ShowDocs);
                Assert.AreEqual("Working WPF sample", calendarViewPage.Examples.Single().HeaderText);

                Assert.IsTrue(richTextEditPage.UsesWpfGalleryPageMode);
                Assert.IsFalse(richTextEditPage.ShowCatalogDetails);
                Assert.IsFalse(richTextEditPage.ShowPageDescription);

                Assert.IsFalse(richEditBoxPage.UsesWpfGalleryPageMode);
                Assert.IsTrue(richEditBoxPage.ShowCatalogDetails);
                Assert.IsTrue(richEditBoxPage.ShowDocs);
                Assert.AreEqual("Working WPF sample", richEditBoxPage.Examples.Single().HeaderText);
            });
        }

        [TestMethod]
        public void WpfGalleryItemPageDescriptionsMatchReferenceViewModels()
        {
            var expectedDescriptions = new Dictionary<string, string>
            {
                { "Canvas", string.Empty },
                { "Color", "Guide showing how to use colors in your app" },
                { "Iconography", "Guide showing how to use icons in your application." },
                { "Image", string.Empty },
                { "Label", string.Empty },
                { "PasswordBox", string.Empty },
                { "RichTextEdit", string.Empty },
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
        public void WhatsNewPageHeaderMatchesWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var page = new WhatsNewPage();
                var pageHeader = (PageHeader)page.FindName("PageHeader");

                Assert.AreEqual(new Thickness(0, 0, 0, 32), pageHeader.Margin);
                Assert.AreEqual("What's new in WPF", pageHeader.Title);
                Assert.AreEqual("Discover all the new features, enhancements and APIs introduced in WPF", pageHeader.Description);
                Assert.IsTrue(pageHeader.ShowDescription);
                AssertBindingPath(pageHeader, PageHeader.TitleProperty, "ViewModel.PageTitle");
                AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "ViewModel.PageDescription");

                pageHeader.ApplyTemplate();
                var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
                Assert.IsNotNull(titleLabel);
                Assert.AreEqual("What's new in WPF Page", AutomationProperties.GetName(titleLabel));
                Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleLabel));
                Assert.AreEqual(0, KeyboardNavigation.GetTabIndex(titleLabel));

                var descriptionLabel = pageHeader.FindDescendants<Label>().Single(label => !ReferenceEquals(label, titleLabel));
                Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(descriptionLabel));
                Assert.AreEqual(1, KeyboardNavigation.GetTabIndex(descriptionLabel));
                Assert.AreEqual(Visibility.Visible, descriptionLabel.Visibility);

                var title = (TextBlock)titleLabel.Content;
                var description = (TextBlock)pageHeader.Template.FindName("DescriptionTextBlock", pageHeader);
                Assert.AreEqual("What's new in WPF", title.Text);
                Assert.AreEqual("Discover all the new features, enhancements and APIs introduced in WPF", description.Text);

                var root = (Grid)page.FindName("ContentRootGrid");
                Assert.AreEqual(
                    (double)Application.Current.FindResource("BodyTextBlockFontSize"),
                    TextElement.GetFontSize(root));

                var gridShorthandParagraph = (TextBlock)page.FindName("GridShorthandSyntaxParagraphText");
                Assert.AreEqual(new Thickness(0, 0, 0, 12), gridShorthandParagraph.Margin);
                Assert.AreEqual(TextWrapping.Wrap, gridShorthandParagraph.TextWrapping);
                Assert.AreSame(DependencyProperty.UnsetValue, gridShorthandParagraph.ReadLocalValue(TextBlock.StyleProperty));
                StringAssert.Contains(
                    new TextRange(gridShorthandParagraph.ContentStart, gridShorthandParagraph.ContentEnd).Text,
                    "comma\u2011separated");

                var gridExample = (ControlExample)page.FindName("GridShorthandSyntaxExample");
                var accentExample = (ControlExample)page.FindName("AccentColorExample");
                var ligatureExample = (ControlExample)page.FindName("HyphenLigatureExample");
                Assert.AreEqual(new Thickness(2, 10, 2, 24), gridExample.Margin);
                Assert.AreEqual(new Thickness(2, 10, 2, 10), accentExample.Margin);
                Assert.AreEqual(new Thickness(2, 10, 2, 10), ligatureExample.Margin);
                Assert.IsInstanceOfType(accentExample.ExampleContent, typeof(Grid));
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

                    var root = (Grid)page.FindName("ContentRootGrid");
                    Assert.AreEqual(2, root.RowDefinitions.Count);
                    Assert.AreEqual(GridUnitType.Auto, root.RowDefinitions[0].Height.GridUnitType);
                    Assert.AreEqual(GridUnitType.Star, root.RowDefinitions[1].Height.GridUnitType);

                    Assert.IsInstanceOfType(page.ViewModel, typeof(SettingsPageViewModel));
                    Assert.AreEqual("Settings", page.ViewModel.PageTitle);
                    Assert.IsNull(page.ViewModel.PageDescription);

                    var pageHeader = (PageHeader)page.FindName("PageHeader");
                    Assert.AreEqual(0, Grid.GetRow(pageHeader));
                    Assert.AreEqual(new Thickness(0, 0, 0, 40), pageHeader.Margin);
                    Assert.AreEqual("Settings", pageHeader.Title);
                    Assert.IsNull(pageHeader.Description);
                    AssertBindingPath(pageHeader, PageHeader.TitleProperty, "ViewModel.PageTitle");
                    AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "ViewModel.PageDescription");

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

                    var appearanceHeader = (TextBlock)page.FindName("AppearanceHeaderText");
                    var aboutHeader = (TextBlock)page.FindName("AboutHeaderText");
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

                    var aboutExpander = (Expander)page.FindName("AboutExpander");
                    Assert.AreEqual("WPF Gallery Preview", AutomationProperties.GetName(aboutExpander));
                    var expanderHeader = (Grid)aboutExpander.Header;
                    Assert.AreEqual(3, expanderHeader.ColumnDefinitions.Count);
                    var aboutHeaderText = (StackPanel)expanderHeader.Children[1];
                    Assert.AreEqual("WPF Gallery", ((TextBlock)aboutHeaderText.Children[0]).Text);
                    Assert.AreEqual("\u00A9 2025 Microsoft. All rights reserved.", ((TextBlock)aboutHeaderText.Children[1]).Text);

                    var cloneCommand = (TextBox)page.FindName("CloneCommandTextBox");
                    Assert.IsFalse(cloneCommand.Focusable);
                    Assert.AreEqual("git clone https://github.com/Kinnara/ModernWpf.git", cloneCommand.Text);

                    var openIssues = (Button)page.FindName("OpenIssuesButton");
                    Assert.AreEqual("Open Issues", AutomationProperties.GetName(openIssues));
                    Assert.AreEqual(new Thickness(8), openIssues.Padding);
                    Assert.IsTrue(FocusManager.GetIsFocusScope(openIssues));

                    var dependencies = (GroupBox)page.FindName("DependenciesGroupBox");
                    var warranty = (GroupBox)page.FindName("WarrantyGroupBox");
                    Assert.AreEqual("Dependencies and References", AutomationProperties.GetName(dependencies));
                    Assert.AreEqual("THIS CODE AND INFORMATION IS PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.", AutomationProperties.GetName(warranty));
                    Assert.AreEqual(new Thickness(0), dependencies.BorderThickness);
                    Assert.AreEqual(new Thickness(0), warranty.BorderThickness);
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
        public void WhatsNewPageAccentSwatchesUseSystemAccentResources()
        {
            WpfTestHost.Run(() =>
            {
                var page = new WhatsNewPage();
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

                    var swatches = (StackPanel)page.FindName("AccentColorSwatches");
                    var borders = swatches.Children.OfType<Border>().ToArray();
                    var expectedBrushKeys = new[]
                    {
                        "SystemAccentColorDark3Brush",
                        "SystemAccentColorDark2Brush",
                        "SystemAccentColorDark1Brush",
                        "SystemControlBackgroundAccentBrush",
                        "SystemAccentColorLight1Brush",
                        "SystemAccentColorLight2Brush",
                        "SystemAccentColorLight3Brush"
                    };

                    Assert.AreEqual(expectedBrushKeys.Length, borders.Length);

                    for (var i = 0; i < expectedBrushKeys.Length; i++)
                    {
                        Assert.AreSame(page.FindResource(expectedBrushKeys[i]), borders[i].Background, expectedBrushKeys[i]);
                    }
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
                listViewPage.ViewModel.ListViewSelectionModeComboBoxSelectedIndex = 1;
                Assert.AreEqual(SelectionMode.Multiple, listViewPage.ViewModel.ListViewSelectionMode);
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
                AssertWpfGalleryPageViewModel<CanvasPage, CanvasPageViewModel>("Canvas", "Canvas", string.Empty);
                AssertWpfGalleryPageViewModel<ImagePage, ImagePageViewModel>("Image", "Image", string.Empty);
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
                AssertWpfGalleryPageRoot<CanvasPage>("Canvas");
                AssertWpfGalleryPageRoot<ImagePage>("Image");
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
                AssertWpfGalleryPageViewModel<RichTextEditPage, RichTextEditPageViewModel>("RichTextEdit", "RichTextEdit", string.Empty);
                AssertWpfGalleryPageViewModel<PasswordBoxPage, PasswordBoxPageViewModel>("PasswordBox", "PasswordBox", string.Empty);

                var textBoxPage = (TextBoxPage)new ItemPage(GalleryCatalog.FindItem("TextBox")).DirectPageContent;
                Assert.AreEqual(string.Empty, textBoxPage.ViewModel.ValidatedText);
                textBoxPage.ViewModel.ValidatedText = "abc";
                Assert.AreEqual("abc", textBoxPage.ViewModel.ValidatedText);
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
                AssertWpfGalleryPageRoot<RichTextEditPage>("RichTextEdit");
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
                AssertWpfGalleryPageViewModel<ColorPage, ColorsPageViewModel>(
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

                var iconographyPage = (IconographyPage)new ItemPage(GalleryCatalog.FindItem("Iconography")).DirectPageContent;
                Assert.IsInstanceOfType(iconographyPage.ViewModel, typeof(IconographyPageViewModel));
                Assert.AreEqual("Icons", iconographyPage.ViewModel.PageTitle);
                Assert.AreEqual("Guide showing how to use icons in your application.", iconographyPage.ViewModel.PageDescription);
            });
        }

        [TestMethod]
        public void DesignGuidanceItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<ColorPage>("Color", "ColorsPage");
                AssertWpfGalleryPageRoot<IconographyPage>("Iconography", "IconsPage");
                AssertWpfGalleryPageRoot<TypographyPage>("Typography");
                AssertWpfGalleryPageRoot<SpacingPage>("Spacing");
                AssertWpfGalleryPageRoot<GeometryPage>("Geometry");
            });
        }

        [TestMethod]
        public void SamplesPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                var page = (UserDashboardPage)new ItemPage(GalleryCatalog.FindItem("UserDashboard")).DirectPageContent;

                Assert.IsInstanceOfType(page.ViewModel, typeof(UserDashboardPageViewModel));
                Assert.AreEqual(20, page.ViewModel.Users.Count);
                Assert.IsNull(page.ViewModel.SelectedUser);
            });
        }

        [TestMethod]
        public void SamplesItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<UserDashboardPage>("UserDashboard");
            });
        }

        [TestMethod]
        public void SystemPagesUseOfficialPageSpecificViewModels()
        {
            WpfTestHost.Run(() =>
            {
                AssertSystemViewModel<FileAndFolderDialogsPage, FileAndFolderDialogsPageViewModel>(
                    "FileAndFolderDialogs",
                    "File and Folder Dialogs",
                    "Use the OpenFileDialog, SaveFileDialog, and OpenFolderDialog to let users select files and folders in a secure way.");
                AssertSystemViewModel<MessageBoxPage, MessageBoxPageViewModel>("MessageBox", "MessageBox", string.Empty);
                AssertSystemViewModel<ClipboardPage, ClipboardPageViewModel>("Clipboard", "Clipboard", string.Empty);

                var dialogsPage = (FileAndFolderDialogsPage)new ItemPage(GalleryCatalog.FindItem("FileAndFolderDialogs")).DirectPageContent;
                Assert.AreEqual("No file selected", dialogsPage.ViewModel.SingleFilePath);
                Assert.AreEqual("No files selected", dialogsPage.ViewModel.MultipleFilesPath);
                Assert.AreEqual("Enter text here to save to a file...", dialogsPage.ViewModel.FileContent);
                Assert.AreEqual("No file saved", dialogsPage.ViewModel.SavedFilePath);
                Assert.AreEqual("No folder selected", dialogsPage.ViewModel.SelectedFolderPath);

                var messageBoxPage = (MessageBoxPage)new ItemPage(GalleryCatalog.FindItem("MessageBox")).DirectPageContent;
                Assert.AreEqual("No message shown yet", messageBoxPage.ViewModel.DefaultMessageResult);
                Assert.AreEqual("No message shown yet", messageBoxPage.ViewModel.CustomTitleResult);
                Assert.AreEqual("No button clicked yet", messageBoxPage.ViewModel.DifferentButtonsResult);
                Assert.AreEqual("No image example shown yet", messageBoxPage.ViewModel.DifferentImagesResult);
                Assert.AreEqual("No common message shown yet", messageBoxPage.ViewModel.CommonMessagesResult);
                Assert.AreEqual("No selection made", messageBoxPage.ViewModel.CustomDefaultResult);
                Assert.AreEqual("<Button Content=\"Show MessageBox\" Click=\"ShowMessageBoxButton_Click\" />", messageBoxPage.ViewModel.DifferentButtonsXamlCode);
                Assert.AreEqual("<Button Content=\"Show MessageBox\" Click=\"ShowMessageButton_Click\" />", messageBoxPage.ViewModel.DifferentImagesXamlCode);
                StringAssert.Contains(messageBoxPage.ViewModel.DifferentButtonsCSharpCode, "MessageBoxButton.OK");
                StringAssert.Contains(messageBoxPage.ViewModel.DifferentImagesCSharpCode, "MessageBoxImage.None");
                messageBoxPage.ViewModel.SelectedButtonIndex = 1;
                StringAssert.Contains(messageBoxPage.ViewModel.DifferentButtonsCSharpCode, "MessageBoxButton.OKCancel");
                messageBoxPage.ViewModel.SelectedImageIndex = 4;
                StringAssert.Contains(messageBoxPage.ViewModel.DifferentImagesCSharpCode, "MessageBoxImage.Information");

                var clipboardPage = (ClipboardPage)new ItemPage(GalleryCatalog.FindItem("Clipboard")).DirectPageContent;
                Assert.AreEqual(string.Empty, clipboardPage.ViewModel.CopyStatus);
                Assert.AreEqual(string.Empty, clipboardPage.ViewModel.PastedText);
                Assert.AreEqual(string.Empty, clipboardPage.ViewModel.ClearStatus);
                Assert.AreEqual(string.Empty, clipboardPage.ViewModel.FormatsInfo);
                Assert.AreEqual(string.Empty, clipboardPage.ViewModel.CopyImageStatus);
                Assert.AreEqual(string.Empty, clipboardPage.ViewModel.PasteImageStatus);
            });
        }

        [TestMethod]
        public void SystemItemPagesUseOfficialPageRoots()
        {
            WpfTestHost.Run(() =>
            {
                AssertWpfGalleryPageRoot<FileAndFolderDialogsPage>("FileAndFolderDialogs", "File and Folder Dialogs");
                AssertWpfGalleryPageRoot<MessageBoxPage>("MessageBox", "MessageBox");
                AssertWpfGalleryPageRoot<ClipboardPage>("Clipboard", "ClipboardPage");
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
                    Assert.AreSame(
                        Application.Current.FindResource("BodyStrongTextBlockStyle"),
                        textBlocks[0].Style);
                    Assert.AreEqual(1, Grid.GetRow(textBlocks[1]));
                    Assert.AreEqual(1, Grid.GetColumn(textBlocks[1]));
                    Assert.AreEqual(0.7, textBlocks[1].Opacity);

                    var controls = selectionGrid.Children.OfType<StackPanel>().Single();
                    Assert.AreEqual(120.0, controls.MinWidth);
                    Assert.AreEqual(new Thickness(12, 0, 0, 0), controls.Margin);
                    Assert.AreEqual(VerticalAlignment.Top, controls.VerticalAlignment);

                    var label = (Label)controls.Children[0];
                    var comboBox = (ComboBox)controls.Children[1];
                    Assert.AreEqual("Selection mode", label.Content);
                    Assert.AreSame(comboBox, label.Target);
                    Assert.AreEqual(0.7, label.Opacity);
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
                AssertExampleMargins("Canvas", new Thickness(10));
                AssertExampleMargins("Expander", new Thickness(10));
                AssertExampleMargins("GridSplitter", new Thickness(10));
                AssertExampleMargins("GroupBox", new Thickness(10));
                AssertExampleMargins("Image", new Thickness(10));
                AssertExampleMargins("ResizeGrip", new Thickness(10));
                AssertExampleMargins("Calendar", new Thickness(10));
                AssertExampleMargins("DatePicker", new Thickness(10));
                AssertExampleMargins("ProgressBar", new Thickness(10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("ToolTip", new Thickness(10));
                AssertExampleMargins("Clipboard", new Thickness(10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("FileAndFolderDialogs", new Thickness(10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10));
                AssertExampleMargins("MessageBox", new Thickness(10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10), new Thickness(10, 32, 10, 10));
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

                    AssertSliderExample(sliderExamples[0], "Simple", 0, 100, 0, 1, TickPlacement.None, Orientation.Horizontal);
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
                Assert.IsTrue(inlines.OfType<LineBreak>().Any());
                var nestedInlines = inlines.OfType<Span>().SelectMany(span => span.Inlines.Cast<Inline>()).ToArray();
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
            });
        }

        [TestMethod]
        public void LayoutAndMediaPagesMatchWpfGalleryReference()
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

                var roundedBorder = (Border)borderExamples[1].ExampleContent;
                Assert.AreEqual(Brushes.LightBlue, roundedBorder.Background);
                Assert.AreEqual(Brushes.CornflowerBlue, roundedBorder.BorderBrush);
                Assert.AreEqual(new CornerRadius(10), roundedBorder.CornerRadius);
                Assert.AreEqual(new Thickness(15), roundedBorder.Padding);

                var variedBorder = (Border)borderExamples[2].ExampleContent;
                Assert.AreEqual(Brushes.DarkSlateGray, variedBorder.BorderBrush);
                Assert.AreEqual(new Thickness(1, 2, 4, 8), variedBorder.BorderThickness);

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

                var canvasPage = new ItemPage(GalleryCatalog.FindItem("Canvas"));
                Assert.IsTrue(canvasPage.HasDirectPageContent);
                Assert.AreEqual(0, canvasPage.Examples.Count);
                var canvasHeader = FindDescendant<PageHeader>((DependencyObject)canvasPage.DirectPageContent);
                Assert.IsNotNull(canvasHeader);
                Assert.AreEqual("Canvas", canvasHeader.Title);
                Assert.AreEqual(string.Empty, canvasHeader.Description);
                var canvasExample = FindDescendant<ControlExample>((DependencyObject)canvasPage.DirectPageContent);
                Assert.IsNotNull(canvasExample);
                Assert.AreEqual("A basic Canvas inside the ViewBox", canvasExample.HeaderText);
                var viewbox = (Viewbox)canvasExample.ExampleContent;
                Assert.AreEqual(200.0, viewbox.Width);
                Assert.AreEqual(200.0, viewbox.Height);
                var canvas = (Canvas)viewbox.Child;
                Assert.AreEqual(47.0, canvas.Width);
                Assert.AreEqual(123.0, canvas.Height);
                Assert.AreEqual(2, canvas.Children.OfType<Path>().Count());

                var imagePage = new ItemPage(GalleryCatalog.FindItem("Image"));
                Assert.IsTrue(imagePage.HasDirectPageContent);
                Assert.AreEqual(0, imagePage.Examples.Count);
                var imageHeader = FindDescendant<PageHeader>((DependencyObject)imagePage.DirectPageContent);
                Assert.IsNotNull(imageHeader);
                Assert.AreEqual("Image", imageHeader.Title);
                Assert.AreEqual(string.Empty, imageHeader.Description);
                var imageExample = FindDescendant<ControlExample>((DependencyObject)imagePage.DirectPageContent);
                Assert.IsNotNull(imageExample);
                Assert.AreEqual("Standand Image from a local file.", imageExample.HeaderText);
                var image = (Image)imageExample.ExampleContent;
                Assert.AreEqual(200.0, image.Height);
                Assert.AreEqual(HorizontalAlignment.Left, image.HorizontalAlignment);
                var imageSource = (BitmapSource)image.Source;
                Assert.IsTrue(imageSource.PixelWidth > 0);
                Assert.IsTrue(imageSource.PixelHeight > 0);
                StringAssert.Contains(imageExample.XamlCode, "Assets\\MyImage.jpg");

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
        public void DateStatusAndSystemPagesMatchWpfGalleryReference()
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
                Assert.AreEqual("Simple ToolTip", ToolTipService.GetToolTip(toolTipButton));

                var clipboardPage = new ItemPage(GalleryCatalog.FindItem("Clipboard"));
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
                    Assert.AreEqual(string.Empty, ((TextBlock)copyClipboardStack.Children[2]).Text);

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

                    AssertButtonResultExample((StackPanel)clipboardExamples[2].ExampleContent, "Clear Clipboard", string.Empty);
                    AssertButtonResultExample((StackPanel)clipboardExamples[3].ExampleContent, "Check Clipboard Formats", string.Empty);

                    var copyImageStack = (StackPanel)clipboardExamples[4].ExampleContent;
                    var sourceImage = (Image)copyImageStack.Children[0];
                    Assert.AreEqual("SourceImage", sourceImage.Name);
                    Assert.AreEqual(100.0, sourceImage.Width);
                    Assert.AreEqual(100.0, sourceImage.Height);
                    Assert.AreEqual(HorizontalAlignment.Left, sourceImage.HorizontalAlignment);
                    Assert.IsInstanceOfType(sourceImage.Source, typeof(BitmapSource));
                    StringAssert.Contains(sourceImage.Source.ToString(), "ControlImages/Clipboard.png");
                    Assert.AreEqual("Copy Image to Clipboard", ((Button)copyImageStack.Children[1]).Content);

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
                });

                var dialogsPage = new ItemPage(GalleryCatalog.FindItem("FileAndFolderDialogs"));
                WithRenderedPage(dialogsPage, () =>
                {
                    Assert.IsTrue(dialogsPage.HasDirectPageContent);
                    var dialogsExamples = GetRenderedExamples(dialogsPage);
                    Assert.AreEqual(4, dialogsExamples.Count);
                    CollectionAssert.AreEqual(
                        new[] { "Pick Single File", "Pick Multiple Files", "Save File", "Pick Folder" },
                        dialogsExamples.Select(example => example.HeaderText).ToArray());
                    AssertButtonResultExample((StackPanel)dialogsExamples[0].ExampleContent, "Pick a single file", "No file selected");
                    AssertButtonResultExample((StackPanel)dialogsExamples[1].ExampleContent, "Pick multiple files", "No files selected");
                    var saveFileStack = (StackPanel)dialogsExamples[2].ExampleContent;
                    var saveTextBox = (TextBox)saveFileStack.Children[0];
                    Assert.AreEqual("Enter text here to save to a file...", saveTextBox.Text);
                    Assert.IsTrue(saveTextBox.AcceptsReturn);
                    Assert.AreEqual(TextWrapping.Wrap, saveTextBox.TextWrapping);
                    Assert.AreEqual(80.0, saveTextBox.MinHeight);
                    Assert.AreEqual(ScrollBarVisibility.Auto, saveTextBox.VerticalScrollBarVisibility);
                    Assert.AreEqual("Save File Text Box", AutomationProperties.GetName(saveTextBox));
                    Assert.AreEqual("The text in the textbox will be saved to a file on button click", AutomationProperties.GetHelpText(saveTextBox));
                    Assert.AreEqual("Save a file", ((Button)saveFileStack.Children[1]).Content);
                    Assert.AreEqual("No file saved", ((TextBlock)saveFileStack.Children[2]).Text);
                    AssertButtonResultExample((StackPanel)dialogsExamples[3].ExampleContent, "Pick a folder", "No folder selected");
                });

                var messageBoxPage = new ItemPage(GalleryCatalog.FindItem("MessageBox"));
                WithRenderedPage(messageBoxPage, () =>
                {
                    Assert.IsTrue(messageBoxPage.HasDirectPageContent);
                    var messageBoxExamples = GetRenderedExamples(messageBoxPage);
                    Assert.AreEqual(6, messageBoxExamples.Count);
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

                    AssertButtonResultExample((StackPanel)messageBoxExamples[0].ExampleContent, "Simple MessageBox", "No message shown yet");
                    AssertButtonResultExample((StackPanel)messageBoxExamples[1].ExampleContent, "Custom MessageBox", "No message shown yet");
                    AssertMessageBoxSelectorExample((Grid)messageBoxExamples[2].ExampleContent, "Button Type:", "MessageBox with Different Buttons", "MessageBox Button Selector", "No button clicked yet", new[] { "OK", "OK/Cancel", "Abort/Retry/Ignore", "Yes/No/Cancel", "Yes/No", "Retry/Cancel", "Cancel/Try/Continue" });
                    AssertMessageBoxSelectorExample((Grid)messageBoxExamples[3].ExampleContent, "Icon Type:", "MessageBox with different images", "MessageBox Image Selector", "No image example shown yet", new[] { "None", "Error", "Question", "Warning", "Information" });

                    var commonMessagesStack = (StackPanel)messageBoxExamples[4].ExampleContent;
                    var commonButtons = ((WrapPanel)commonMessagesStack.Children[0]).Children.OfType<Button>().ToArray();
                    CollectionAssert.AreEqual(new[] { "Information", "Error", "Warning" }, commonButtons.Select(button => (string)button.Content).ToArray());
                    Assert.AreEqual(new Thickness(0, 0, 5, 0), commonButtons[0].Margin);
                    Assert.AreEqual(new Thickness(0, 0, 5, 0), commonButtons[1].Margin);
                    Assert.AreEqual(new Thickness(0), commonButtons[2].Margin);
                    Assert.AreEqual("No common message shown yet", ((TextBlock)commonMessagesStack.Children[1]).Text);

                    AssertButtonResultExample((StackPanel)messageBoxExamples[5].ExampleContent, "Show with 'No' as default", "No selection made");
                });
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
                Assert.AreEqual("A Hyperlink", hyperlinkExamples[0].HeaderText);
                var hyperlinkTextBlock = (TextBlock)hyperlinkExamples[0].ExampleContent;
                Assert.AreEqual(new Thickness(20), hyperlinkTextBlock.Margin);
                var hyperlink = hyperlinkTextBlock.Inlines.OfType<Hyperlink>().Single();
                Assert.AreEqual(new System.Uri("https://www.microsoft.com"), hyperlink.NavigateUri);
                Assert.AreEqual("Hyperlink", hyperlink.Inlines.OfType<Run>().Single().Text);
            });
        }

        [TestMethod]
        public void DesignGuidancePagesMatchWpfGalleryReferenceLayoutDetails()
        {
            WpfTestHost.Run(() =>
            {
                var spacingPage = new ItemPage(GalleryCatalog.FindItem("Spacing"));
                Assert.IsTrue(spacingPage.HasDirectPageContent);
                var spacingBody = GetDirectPageBodyStack(spacingPage);
                var images = (StackPanel)spacingBody.Children[2];
                Assert.AreEqual(Orientation.Horizontal, images.Orientation);
                Assert.AreEqual(new Thickness(0, 0, 0, 16), images.Margin);
                Assert.AreEqual(2, images.Children.Count);

                var cardsFrame = (Grid)images.Children[0];
                var dialogFrame = (Grid)images.Children[1];
                Assert.AreEqual(VerticalAlignment.Top, cardsFrame.VerticalAlignment);
                Assert.AreEqual(VerticalAlignment.Top, dialogFrame.VerticalAlignment);
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
                var typographyBody = GetDirectPageBodyStack(typographyPage);
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
                var geometryBody = GetDirectPageBodyStack(geometryPage);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), geometryBody.Margin);
                Assert.AreEqual(5, geometryBody.Children.Count);
                Assert.AreEqual("Geometry describes the shape, size and position of UI elements on screen.", ((TextBlock)geometryBody.Children[0]).Text);
                Assert.AreEqual("These fundamental design elements help experiences feel coherent across the entire design system.", ((TextBlock)geometryBody.Children[1]).Text);
                var geometryUsage = (TextBlock)geometryBody.Children[2];
                Assert.AreEqual("You can reference built-in corner radii styles using: CornerRadius=\"{StaticResource ControlCornerRadius}\" .", geometryUsage.Text);
                Assert.AreEqual(new Thickness(0, 0, 0, 12), geometryUsage.Margin);

                var geometryImageHost = (Border)geometryBody.Children[3];
                Assert.AreEqual(500.0, geometryImageHost.Width);
                Assert.AreEqual(300.0, geometryImageHost.Height);
                Assert.AreEqual(HorizontalAlignment.Left, geometryImageHost.HorizontalAlignment);
                var geometryImage = (Image)geometryImageHost.Child;
                Assert.AreEqual(500.0, geometryImage.Width);
                Assert.AreEqual(300.0, geometryImage.Height);
                Assert.AreEqual(Stretch.Uniform, geometryImage.Stretch);
                Assert.AreEqual("Example of corner radius.", AutomationProperties.GetName(geometryImage));
                StringAssert.Contains(((BitmapImage)geometryImage.Source).UriSource.ToString(), "Geometry.light.png");

                var geometryContent = (GeometryPage)geometryPage.DirectPageContent;
                geometryContent.ApplyImageResources(ElementTheme.Dark);
                StringAssert.Contains(((BitmapImage)geometryImage.Source).UriSource.ToString(), "Geometry.dark.png");

                var geometryExample = (ControlExample)geometryBody.Children[4];
                Assert.IsNull(geometryExample.HeaderText);
                StringAssert.Contains(geometryExample.XamlCode, "OverlayCornerRadius");
                var cornerRadiusTable = (Grid)geometryExample.ExampleContent;
                AssertCornerRadiusTable(cornerRadiusTable);
            });
        }

        [TestMethod]
        public void ColorPageUsesWpfGallerySelectorAndTextSectionLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Color"));
                Assert.IsTrue(page.HasDirectPageContent);

                var body = GetDirectPageBodyStack(page);
                Assert.AreEqual(3, body.Children.Count);
                var selector = (ComboBox)body.Children[1];
                var sectionHost = (ContentControl)body.Children[2];

                CollectionAssert.AreEqual(
                    new[] { "Text", "Fill", "Stroke", "Background", "Signal", "HighContrast" },
                    selector.Items.Cast<string>().ToArray());
                Assert.AreEqual(200.0, selector.Width);
                Assert.AreEqual("Page Selector", AutomationProperties.GetName(selector));

                selector.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, selector));
                WpfTestHost.DoEvents();

                Assert.AreEqual("TextSection", sectionHost.Content.GetType().Name);
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
        public void IconographyPageUsesWpfGalleryIconLibraryLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Iconography"));
                Assert.IsTrue(page.HasDirectPageContent);

                var directPage = (FrameworkElement)page.DirectPageContent;
                directPage.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent, directPage));
                WpfTestHost.DoEvents();

                var body = (Grid)((UserControl)page.DirectPageContent).Content;
                Assert.AreEqual(6, body.RowDefinitions.Count);

                var instructions = (Expander)body.Children.Cast<UIElement>().Single(child => Grid.GetRow(child) == 1);
                Assert.AreEqual("Instructions on how to use Segoe Fluent Icons", instructions.Header);
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
                WpfTestHost.DoEvents();
                Assert.AreEqual(Visibility.Hidden, searchPlaceholder.Visibility);
                Assert.AreEqual("GlobalNavButton", selectedName.Text);
                Assert.IsTrue(iconsListView.Items.Count > 0);
                Assert.IsTrue(iconsListView.Items.Count < 250);

                searchBox.Text = string.Empty;
                pageSizeComboBox.SelectedIndex = 0;
                WpfTestHost.DoEvents();
                Assert.AreEqual(Visibility.Visible, searchPlaceholder.Visibility);
                Assert.AreEqual(100, iconsListView.Items.Count);
                Assert.AreEqual("Page 1 of", currentPageText.Text);
                Assert.AreEqual("15", totalPagesText.Text);
            });
        }

        [TestMethod]
        public void UserDashboardPageMatchesWpfGalleryReferenceLayoutAndBehavior()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("UserDashboard"));
                var directPage = (UserDashboardPage)page.DirectPageContent;
                var directPageHost = (ContentControl)page.FindName("DirectPageContentHost");
                Assert.AreEqual(new Thickness(0), directPageHost.Margin);
                Assert.AreEqual("UserDashboardPage", directPage.Title);
                Assert.IsInstanceOfType(directPage.ViewModel, typeof(UserDashboardPageViewModel));

                var root = (Grid)directPage.FindName("ContentRootGrid");
                var window = new Window
                {
                    Width = 900,
                    Height = 720,
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

                    Assert.AreEqual(2, root.ColumnDefinitions.Count);
                    Assert.AreEqual(GridLength.Auto, root.ColumnDefinitions[0].Width);
                    Assert.AreEqual(2, root.RowDefinitions.Count);
                    Assert.AreEqual(280.0, root.RowDefinitions[0].MaxHeight);
                    Assert.AreEqual(new GridLength(2, GridUnitType.Star), root.RowDefinitions[1].Height);

                    var userListGrid = root.Children.OfType<Grid>().Single(child => Grid.GetColumn(child) == 0);
                    var userList = userListGrid.Children.OfType<ListView>().Single();
                    Assert.AreEqual("Users", AutomationProperties.GetName(userList));
                    Assert.AreEqual(300.0, userList.Width);
                    Assert.AreEqual(SelectionMode.Single, userList.SelectionMode);
                    Assert.AreEqual(20, userList.Items.Count);
                    Assert.AreEqual(-1, userList.SelectedIndex);
                    var firstUser = (UserDashboardUser)userList.Items[0];
                    var firstUserItem = (ListViewItem)userList.ItemContainerGenerator.ContainerFromIndex(0);
                    Assert.IsNotNull(firstUserItem);
                    Assert.AreEqual(firstUser.Name, AutomationProperties.GetName(firstUserItem));
                    var firstUserName = FindTextBlock(firstUserItem, firstUser.Name);
                    Assert.AreEqual(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(firstUserName));

                    var addUserButton = userListGrid.Children.OfType<Button>().Single();
                    Assert.AreEqual("Add New User", addUserButton.Content);
                    Assert.AreEqual(new Thickness(10), addUserButton.Margin);
                    Assert.AreEqual(HorizontalAlignment.Center, addUserButton.HorizontalAlignment);

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
                    Assert.AreEqual("Saved!", savedStatus.Text);
                    Assert.AreEqual(Visibility.Collapsed, savedStatus.Visibility);
                    Assert.AreEqual("User " + selectedUser.Name + " Deleted!", deletedStatus.Text);
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
            public RenderedExample(string headerText, object exampleContent, string xamlCode, Thickness margin)
            {
                HeaderText = headerText;
                ExampleContent = exampleContent;
                XamlCode = xamlCode;
                Margin = margin;
            }

            public string HeaderText { get; }

            public object ExampleContent { get; }

            public string XamlCode { get; }

            public Thickness Margin { get; }
        }

        private static IReadOnlyList<RenderedExample> GetRenderedExamples(ItemPage page)
        {
            if (page.HasDirectPageContent)
            {
                return FindDescendants<ControlExample>((DependencyObject)page.DirectPageContent)
                    .Select(example => new RenderedExample(example.HeaderText, example.ExampleContent, example.XamlCode, example.Margin))
                    .ToArray();
            }

            return page.Examples
                .Select(example => new RenderedExample(example.HeaderText, example.ExampleContent, example.XamlCode, example.Margin))
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

            Assert.AreEqual(expectedTitle, viewModel.PageTitle, uniqueId);
            Assert.AreEqual(string.Empty, viewModel.PageDescription, uniqueId);
            Assert.AreEqual(uniqueId + "PageViewModel", viewModel.GetType().Name, uniqueId);
        }

        private static void AssertCollectionsViewModel<TPage, TViewModel>(string uniqueId, string expectedTitle)
            where TPage : FrameworkElement
            where TViewModel : CollectionsPageViewModelBase
        {
            var itemPage = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            var directPage = (TPage)itemPage.DirectPageContent;
            var viewModel = (TViewModel)directPage.GetType().GetProperty("ViewModel").GetValue(directPage, null);

            Assert.AreEqual(expectedTitle, viewModel.PageTitle, uniqueId);
            Assert.AreEqual(string.Empty, viewModel.PageDescription, uniqueId);
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

            Assert.AreEqual(expectedTitle, viewModel.PageTitle, uniqueId);
            Assert.AreEqual(expectedDescription, viewModel.PageDescription, uniqueId);
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

            Assert.AreEqual(expectedTitle, viewModel.PageTitle, uniqueId);
            Assert.AreEqual(expectedDescription, viewModel.PageDescription, uniqueId);
            Assert.AreEqual(expectedViewModelTypeName ?? uniqueId + "PageViewModel", viewModel.GetType().Name, uniqueId);
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

        private static void AssertBindingPath(DependencyObject target, DependencyProperty property, string expectedPath)
        {
            var expression = BindingOperations.GetBindingExpression(target, property);
            Assert.IsNotNull(expression);
            Assert.AreEqual(expectedPath, expression.ParentBinding.Path.Path);
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

        private static void AssertSliderExample(RenderedExample example, string automationName, double minimum, double maximum, double value, double tickFrequency, TickPlacement tickPlacement, Orientation orientation)
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

        private static void AssertGridViewColumn(GridViewColumn column, string header, double width, string path)
        {
            Assert.AreEqual(header, column.Header);
            Assert.AreEqual(width, column.Width);
            var binding = (Binding)column.DisplayMemberBinding;
            Assert.AreEqual(path, binding.Path.Path);
        }
    }
}
