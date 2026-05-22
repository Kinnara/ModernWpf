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
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;

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
                        Assert.AreEqual(expectedDescription.Key, pageHeader.Title, expectedDescription.Key);
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
                var titleLabel = (Label)page.FindName("TitleLabel");
                var descriptionLabel = (Label)page.FindName("DescriptionLabel");
                var title = (TextBlock)page.FindName("WhatsNewTitleTextBlock");
                var description = (TextBlock)page.FindName("WhatsNewDescriptionTextBlock");

                Assert.AreEqual("What's new in WPF Page", AutomationProperties.GetName(titleLabel));
                Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel(titleLabel));
                Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel(descriptionLabel));
                Assert.AreEqual(0, KeyboardNavigation.GetTabIndex(titleLabel));
                Assert.AreEqual(1, KeyboardNavigation.GetTabIndex(descriptionLabel));
                Assert.AreEqual("What's new in WPF", title.Text);
                Assert.AreEqual("Discover all the new features, enhancements and APIs introduced in WPF", description.Text);

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

                    var root = (Grid)page.FindName("ContentRootGrid");
                    Assert.AreEqual(2, root.RowDefinitions.Count);
                    Assert.AreEqual(GridUnitType.Auto, root.RowDefinitions[0].Height.GridUnitType);
                    Assert.AreEqual(GridUnitType.Star, root.RowDefinitions[1].Height.GridUnitType);

                    var titleLabel = (Label)page.FindName("TitleLabel");
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
                    Assert.AreEqual("ModernWpf Gallery", AutomationProperties.GetName(aboutExpander));
                    var expanderHeader = (Grid)aboutExpander.Header;
                    Assert.AreEqual(3, expanderHeader.ColumnDefinitions.Count);
                    Assert.AreEqual("ModernWpf Gallery", ((TextBlock)((StackPanel)expanderHeader.Children[1]).Children[0]).Text);

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
        public void ListViewPageExamplesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("ListView"));

                Assert.AreEqual(3, page.Examples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "Basic ListView with Simple DataTemplate.",
                        "ListView with Selection Support.",
                        "ListView with GridView."
                    },
                    page.Examples.Select(example => example.HeaderText).ToArray());

                var basicListView = (ListView)page.Examples[0].ExampleContent;
                Assert.AreEqual(200.0, basicListView.Height);
                Assert.AreEqual(2, basicListView.SelectedIndex);
                Assert.AreEqual(SelectionMode.Single, basicListView.SelectionMode);
                Assert.IsNotNull(basicListView.ItemTemplate);
                StringAssert.Contains(page.Examples[0].XamlCode, "ViewModel.BasicListViewItems, Mode=TwoWay");
                StringAssert.Contains(page.Examples[0].XamlCode, "Text=\"{Binding Name, Mode=OneWay}\"");

                var selectionGrid = (Grid)page.Examples[1].ExampleContent;
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
                Assert.AreEqual(FontWeights.Bold, textBlocks[0].FontWeight);
                Assert.AreEqual(0, Grid.GetRow(textBlocks[0]));
                Assert.AreEqual(1, Grid.GetColumn(textBlocks[0]));
                Assert.AreEqual(1, Grid.GetRow(textBlocks[1]));
                Assert.AreEqual(1, Grid.GetColumn(textBlocks[1]));
                Assert.AreEqual(1.0, textBlocks[1].Opacity);

                var controls = selectionGrid.Children.OfType<StackPanel>().Single();
                Assert.AreEqual(120.0, controls.MinWidth);
                Assert.AreEqual(new Thickness(12, 0, 0, 0), controls.Margin);
                Assert.AreEqual(VerticalAlignment.Top, controls.VerticalAlignment);

                var label = (Label)controls.Children[0];
                var comboBox = (ComboBox)controls.Children[1];
                Assert.AreEqual("Selection mode", label.Content);
                Assert.AreSame(comboBox, label.Target);
                CollectionAssert.AreEqual(
                    new[] { "Single", "Multiple", "Extended" },
                    comboBox.Items.Cast<ComboBoxItem>().Select(item => (string)item.Content).ToArray());

                comboBox.SelectedIndex = 1;
                Assert.AreEqual(SelectionMode.Multiple, selectionListView.SelectionMode);
                comboBox.SelectedIndex = 2;
                Assert.AreEqual(SelectionMode.Extended, selectionListView.SelectionMode);

                StringAssert.Contains(page.Examples[1].XamlCode, "<Grid.RowDefinitions>");
                StringAssert.Contains(page.Examples[1].XamlCode, "FontWeight=\"Bold\"");
                StringAssert.Contains(page.Examples[1].XamlCode, "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"");

                var gridViewListView = (ListView)page.Examples[2].ExampleContent;
                Assert.AreEqual(280.0, gridViewListView.Height);
                var gridView = (GridView)gridViewListView.View;
                Assert.AreEqual(3, gridView.Columns.Count);
                AssertGridViewColumn(gridView.Columns[0], "First Name", 150.0, "FirstName");
                AssertGridViewColumn(gridView.Columns[1], "Last Name", 150.0, "LastName");
                AssertGridViewColumn(gridView.Columns[2], "Company", 200.0, "Company");
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
        public void BasicInputButtonAndCheckBoxPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var buttonPage = new ItemPage(GalleryCatalog.FindItem("Button"));
                Assert.AreEqual(2, buttonPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "Simple Button", "WPF Accent Button" },
                    buttonPage.Examples.Select(example => example.HeaderText).ToArray());

                var simpleRoot = (Panel)buttonPage.Examples[0].ExampleContent;
                var simpleGrid = (Grid)simpleRoot.Children[0];
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
                Assert.IsFalse(simpleButton.IsEnabled);
                disableButton.IsChecked = false;
                Assert.IsTrue(simpleButton.IsEnabled);

                var accentButton = (Button)buttonPage.Examples[1].ExampleContent;
                Assert.AreEqual("WPF Accent", AutomationProperties.GetName(accentButton));
                var accentContent = (StackPanel)accentButton.Content;
                Assert.AreEqual(Orientation.Horizontal, accentContent.Orientation);
                Assert.AreEqual("WPF Accent Button", ((TextBlock)accentContent.Children[0]).Text);

                var checkBoxPage = new ItemPage(GalleryCatalog.FindItem("CheckBox"));
                Assert.AreEqual(3, checkBoxPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A 2-state CheckBox.", "A 3-state CheckBox.", "Using a 3-state CheckBox." },
                    checkBoxPage.Examples.Select(example => example.HeaderText).ToArray());

                var twoState = (CheckBox)checkBoxPage.Examples[0].ExampleContent;
                Assert.AreEqual("Two-state CheckBox", twoState.Content);
                Assert.IsFalse(twoState.IsThreeState);
                Assert.AreEqual(false, twoState.IsChecked);
                Assert.AreEqual("Sample Two State", AutomationProperties.GetName(twoState));

                var threeState = (CheckBox)checkBoxPage.Examples[1].ExampleContent;
                Assert.AreEqual("Three-state CheckBox", threeState.Content);
                Assert.IsTrue(threeState.IsThreeState);
                Assert.IsNull(threeState.IsChecked);
                Assert.AreEqual("Sample Three State", AutomationProperties.GetName(threeState));

                var group = (StackPanel)checkBoxPage.Examples[2].ExampleContent;
                Assert.AreEqual(4, group.Children.Count);
                var selectAll = (CheckBox)group.Children[0];
                var options = group.Children.OfType<CheckBox>().Skip(1).ToArray();
                Assert.AreEqual("Select all", selectAll.Content);
                Assert.IsTrue(selectAll.IsThreeState);
                CollectionAssert.AreEqual(new[] { "Option 1", "Option 2", "Option 3" }, options.Select(option => (string)option.Content).ToArray());
                CollectionAssert.AreEqual(Enumerable.Repeat(new Thickness(24, 0, 0, 0), 3).ToArray(), options.Select(option => option.Margin).ToArray());

                options[0].IsChecked = true;
                Assert.IsNull(selectAll.IsChecked);
                options[1].IsChecked = true;
                options[2].IsChecked = true;
                Assert.AreEqual(true, selectAll.IsChecked);
                options[1].IsChecked = false;
                Assert.IsNull(selectAll.IsChecked);
                selectAll.IsChecked = false;
                CollectionAssert.AreEqual(new bool?[] { false, false, false }, options.Select(option => option.IsChecked).ToArray());
            });
        }

        [TestMethod]
        public void BasicInputComboBoxRadioButtonAndSliderPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var comboBoxPage = new ItemPage(GalleryCatalog.FindItem("ComboBox"));
                Assert.AreEqual(3, comboBoxPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "A ComboBox with items defined inline.",
                        "A ComboBox with ItemsSource set.",
                        "An editable ComboBox."
                    },
                    comboBoxPage.Examples.Select(example => example.HeaderText).ToArray());

                var inlineComboBox = (ComboBox)((Panel)comboBoxPage.Examples[0].ExampleContent).Children[0];
                AssertGalleryComboBox(inlineComboBox, "Sample defined inline");
                CollectionAssert.AreEqual(
                    new[] { "Blue", "Green", "Red", "Yellow" },
                    inlineComboBox.Items.Cast<ComboBoxItem>().Select(item => (string)item.Content).ToArray());
                Assert.AreEqual(0, inlineComboBox.SelectedIndex);

                var fontFamilyComboBox = (ComboBox)comboBoxPage.Examples[1].ExampleContent;
                AssertGalleryComboBox(fontFamilyComboBox, "Sample item source set");
                CollectionAssert.AreEqual(
                    new[] { "Arial", "Comic Sans MS", "Segoe UI", "Times New Roman" },
                    fontFamilyComboBox.ItemsSource.Cast<string>().ToArray());
                Assert.IsNotNull(fontFamilyComboBox.ItemTemplate);
                Assert.AreEqual(0, fontFamilyComboBox.SelectedIndex);

                var editableComboBox = (ComboBox)comboBoxPage.Examples[2].ExampleContent;
                AssertGalleryComboBox(editableComboBox, "Editable");
                Assert.IsTrue(editableComboBox.IsEditable);
                CollectionAssert.AreEqual(
                    new[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 36, 48, 72 },
                    editableComboBox.ItemsSource.Cast<int>().ToArray());
                Assert.AreEqual(0, editableComboBox.SelectedIndex);

                var radioButtonPage = new ItemPage(GalleryCatalog.FindItem("RadioButton"));
                Assert.AreEqual(2, radioButtonPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "Standard RadioButton.", "RadioButton with right to left flow direction." },
                    radioButtonPage.Examples.Select(example => example.HeaderText).ToArray());

                var radioGrid = (Grid)radioButtonPage.Examples[0].ExampleContent;
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
                Assert.IsTrue(defaultRadios.All(radioButton => !radioButton.IsEnabled));
                disableRadioButtons.IsChecked = false;
                Assert.IsTrue(defaultRadios.All(radioButton => radioButton.IsEnabled));

                RaiseGotKeyboardFocus(defaultRadios[1]);
                Assert.AreEqual(true, defaultRadios[1].IsChecked);
                Assert.AreEqual(false, defaultRadios[0].IsChecked);

                var leftFlowStack = (StackPanel)radioButtonPage.Examples[1].ExampleContent;
                Assert.AreEqual(KeyboardNavigationMode.Once, KeyboardNavigation.GetTabNavigation(leftFlowStack));
                Assert.AreEqual(KeyboardNavigationMode.Cycle, KeyboardNavigation.GetDirectionalNavigation(leftFlowStack));
                AssertRadioButtons(leftFlowStack.Children.OfType<RadioButton>().ToArray(), "Left Flow", "radio_group_two", FlowDirection.RightToLeft);

                var sliderPage = new ItemPage(GalleryCatalog.FindItem("Slider"));
                Assert.AreEqual(4, sliderPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "A simple slider.",
                        "A slider with steps and range specified.",
                        "A slider with tick marks.",
                        "A vertical slider with range and tick marks specified."
                    },
                    sliderPage.Examples.Select(example => example.HeaderText).ToArray());

                AssertSliderExample(sliderPage.Examples[0], "Simple", 0, 100, 0, 0, TickPlacement.None, Orientation.Horizontal);
                AssertSliderExample(sliderPage.Examples[1], "Range and steps specified", 500, 1000, 500, 50, TickPlacement.None, Orientation.Horizontal);
                AssertSliderExample(sliderPage.Examples[2], "Tick marks", 0, 100, 0, 20, TickPlacement.Both, Orientation.Horizontal);
                AssertSliderExample(sliderPage.Examples[3], "Vertical", 0, 100, 0, 20, TickPlacement.Both, Orientation.Vertical);
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
                Assert.AreEqual(3, borderPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A basic Border", "A Border with rounded corners", "A Border with different thickness on each side" },
                    borderPage.Examples.Select(example => example.HeaderText).ToArray());

                var basicBorder = (Border)borderPage.Examples[0].ExampleContent;
                Assert.AreEqual(Brushes.Gray, basicBorder.BorderBrush);
                Assert.AreEqual(new Thickness(2), basicBorder.BorderThickness);
                Assert.AreEqual(new Thickness(10), basicBorder.Padding);
                Assert.AreEqual("Content inside a Border", ((TextBlock)basicBorder.Child).Text);

                var roundedBorder = (Border)borderPage.Examples[1].ExampleContent;
                Assert.AreEqual(Brushes.LightBlue, roundedBorder.Background);
                Assert.AreEqual(Brushes.CornflowerBlue, roundedBorder.BorderBrush);
                Assert.AreEqual(new CornerRadius(10), roundedBorder.CornerRadius);
                Assert.AreEqual(new Thickness(15), roundedBorder.Padding);

                var variedBorder = (Border)borderPage.Examples[2].ExampleContent;
                Assert.AreEqual(Brushes.DarkSlateGray, variedBorder.BorderBrush);
                Assert.AreEqual(new Thickness(1, 2, 4, 8), variedBorder.BorderThickness);

                var gridPage = new ItemPage(GalleryCatalog.FindItem("Grid"));
                Assert.AreEqual(3, gridPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple 3x3 Grid", "A Grid with custom sizing and spanning", "Grid using XAML shorthand syntax" },
                    gridPage.Examples.Select(example => example.HeaderText).ToArray());

                var simpleGrid = (Grid)gridPage.Examples[0].ExampleContent;
                Assert.AreEqual(250.0, simpleGrid.Height);
                Assert.IsTrue(simpleGrid.ShowGridLines);
                Assert.AreEqual(3, simpleGrid.RowDefinitions.Count);
                Assert.AreEqual(3, simpleGrid.ColumnDefinitions.Count);
                CollectionAssert.AreEqual(
                    Enumerable.Range(1, 9).Select(i => "Cell " + i).ToArray(),
                    simpleGrid.Children.OfType<TextBlock>().Select(textBlock => textBlock.Text).ToArray());

                var customGrid = (Grid)gridPage.Examples[1].ExampleContent;
                AssertGridExample(customGrid, 300.0, new[] { "Row 0, Column 0", "Row 1, Spans all columns", "Row 2, Spans 2 columns" });

                var shorthandGrid = (Grid)gridPage.Examples[2].ExampleContent;
                AssertGridExample(shorthandGrid, 300.0, new[] { "Header (100px)", "Main Content Area (fills available space)", "Footer (Auto height, spans all columns)" });
                Assert.AreEqual(100.0, shorthandGrid.ColumnDefinitions[0].Width.Value);
                Assert.AreEqual(2.0, shorthandGrid.ColumnDefinitions[1].Width.Value);

                var stackPanelPage = new ItemPage(GalleryCatalog.FindItem("StackPanel"));
                Assert.AreEqual(2, stackPanelPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A basic vertical StackPanel", "A horizontal StackPanel" },
                    stackPanelPage.Examples.Select(example => example.HeaderText).ToArray());
                AssertStackPanelExample((StackPanel)stackPanelPage.Examples[0].ExampleContent, Orientation.Vertical);
                AssertStackPanelExample((StackPanel)stackPanelPage.Examples[1].ExampleContent, Orientation.Horizontal);

                var expanderPage = new ItemPage(GalleryCatalog.FindItem("Expander"));
                Assert.AreEqual(1, expanderPage.Examples.Count);
                Assert.AreEqual("An Expander with text in the header and content areas", expanderPage.Examples[0].HeaderText);
                var expanderGrid = (Grid)expanderPage.Examples[0].ExampleContent;
                Assert.AreEqual(2, expanderGrid.ColumnDefinitions.Count);
                var expander = expanderGrid.Children.OfType<Expander>().Single();
                Assert.AreEqual(0, Grid.GetColumn(expander));
                Assert.AreEqual("This text is in the header", expander.Header);
                Assert.AreEqual("This is in the content", expander.Content);

                var gridSplitterPage = new ItemPage(GalleryCatalog.FindItem("GridSplitter"));
                Assert.AreEqual(1, gridSplitterPage.Examples.Count);
                Assert.AreEqual("A GridSplitter", gridSplitterPage.Examples[0].HeaderText);
                var gridSplitterRoot = (Grid)gridSplitterPage.Examples[0].ExampleContent;
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
                Assert.AreEqual(1, groupBoxPage.Examples.Count);
                Assert.AreEqual("A GroupBox", groupBoxPage.Examples[0].HeaderText);
                var groupBox = (GroupBox)groupBoxPage.Examples[0].ExampleContent;
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
                Assert.AreEqual(1, resizeGripPage.Examples.Count);
                Assert.AreEqual("A ResizeGrip", resizeGripPage.Examples[0].HeaderText);
                var resizeGripStack = (StackPanel)resizeGripPage.Examples[0].ExampleContent;
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
                Assert.AreEqual(1, calendarPage.Examples.Count);
                Assert.AreEqual("A basic Calendar control.", calendarPage.Examples[0].HeaderText);
                var calendar = (Calendar)calendarPage.Examples[0].ExampleContent;
                Assert.AreEqual(HorizontalAlignment.Left, calendar.HorizontalAlignment);
                Assert.AreEqual("Default", AutomationProperties.GetName(calendar));
                Assert.IsFalse(KeyboardNavigation.GetIsTabStop(calendar));

                var datePickerPage = new ItemPage(GalleryCatalog.FindItem("DatePicker"));
                Assert.AreEqual(1, datePickerPage.Examples.Count);
                Assert.AreEqual("A basic DatePicker control.", datePickerPage.Examples[0].HeaderText);
                var datePicker = (DatePicker)datePickerPage.Examples[0].ExampleContent;
                Assert.AreEqual(200.0, datePicker.MinWidth);
                Assert.AreEqual(HorizontalAlignment.Left, datePicker.HorizontalAlignment);
                Assert.AreEqual("Pick a date", AutomationProperties.GetName(datePicker));

                var progressBarPage = new ItemPage(GalleryCatalog.FindItem("ProgressBar"));
                Assert.AreEqual(2, progressBarPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple progress bar.", "An indeterminate progress bar." },
                    progressBarPage.Examples.Select(example => example.HeaderText).ToArray());
                var determinate = (ProgressBar)progressBarPage.Examples[0].ExampleContent;
                Assert.AreEqual(new Thickness(24), determinate.Margin);
                Assert.AreEqual(40.0, determinate.Value);
                Assert.IsFalse(determinate.IsIndeterminate);
                Assert.AreEqual("A determinate", AutomationProperties.GetName(determinate));
                var indeterminate = (ProgressBar)progressBarPage.Examples[1].ExampleContent;
                Assert.AreEqual(new Thickness(24), indeterminate.Margin);
                Assert.IsTrue(indeterminate.IsIndeterminate);
                Assert.AreEqual("An indeterminate", AutomationProperties.GetName(indeterminate));

                var toolTipPage = new ItemPage(GalleryCatalog.FindItem("ToolTip"));
                Assert.AreEqual(1, toolTipPage.Examples.Count);
                Assert.AreEqual("A button with a simple ToolTip.", toolTipPage.Examples[0].HeaderText);
                var toolTipButton = (Button)toolTipPage.Examples[0].ExampleContent;
                Assert.AreEqual("Button with a simple ToolTip.", toolTipButton.Content);
                Assert.AreEqual("TooltipButton", AutomationProperties.GetName(toolTipButton));
                Assert.AreEqual(100, ToolTipService.GetInitialShowDelay(toolTipButton));
                Assert.AreEqual(PlacementMode.MousePoint, ToolTipService.GetPlacement(toolTipButton));
                Assert.AreEqual("Simple ToolTip", ToolTipService.GetToolTip(toolTipButton));

                var clipboardPage = new ItemPage(GalleryCatalog.FindItem("Clipboard"));
                Assert.AreEqual(6, clipboardPage.Examples.Count);
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
                    clipboardPage.Examples.Select(example => example.HeaderText).ToArray());

                var copyClipboardStack = (StackPanel)clipboardPage.Examples[0].ExampleContent;
                var copyTextBox = (TextBox)copyClipboardStack.Children[0];
                Assert.AreEqual("CopyTextBox", copyTextBox.Name);
                Assert.AreEqual("Hello, Clipboard!", copyTextBox.Text);
                Assert.AreEqual(300.0, copyTextBox.Width);
                Assert.AreEqual(HorizontalAlignment.Left, copyTextBox.HorizontalAlignment);
                Assert.AreEqual("Copy To Clipboard TextBox", AutomationProperties.GetName(copyTextBox));
                Assert.AreEqual("Copy to Clipboard", ((Button)copyClipboardStack.Children[1]).Content);
                Assert.AreEqual(string.Empty, ((TextBlock)copyClipboardStack.Children[2]).Text);

                var pasteClipboardStack = (StackPanel)clipboardPage.Examples[1].ExampleContent;
                Assert.AreEqual("Paste from Clipboard", ((Button)pasteClipboardStack.Children[0]).Content);
                Assert.AreEqual("Pasted Content:", ((TextBlock)pasteClipboardStack.Children[1]).Text);
                var pasteTextBox = (TextBox)pasteClipboardStack.Children[2];
                Assert.AreEqual("PasteTextBox", pasteTextBox.Name);
                Assert.IsTrue(pasteTextBox.IsReadOnly);
                Assert.AreEqual(TextWrapping.Wrap, pasteTextBox.TextWrapping);
                Assert.AreEqual(60.0, pasteTextBox.MinHeight);
                Assert.AreEqual(300.0, pasteTextBox.Width);
                Assert.AreEqual("Paste content textbox", AutomationProperties.GetName(pasteTextBox));

                AssertButtonResultExample((StackPanel)clipboardPage.Examples[2].ExampleContent, "Clear Clipboard", string.Empty);
                AssertButtonResultExample((StackPanel)clipboardPage.Examples[3].ExampleContent, "Check Clipboard Formats", string.Empty);

                var copyImageStack = (StackPanel)clipboardPage.Examples[4].ExampleContent;
                var sourceImage = (Image)copyImageStack.Children[0];
                Assert.AreEqual("SourceImage", sourceImage.Name);
                Assert.AreEqual(100.0, sourceImage.Width);
                Assert.AreEqual(100.0, sourceImage.Height);
                Assert.AreEqual(HorizontalAlignment.Left, sourceImage.HorizontalAlignment);
                StringAssert.Contains(((BitmapImage)sourceImage.Source).UriSource.ToString(), "ControlImages/Clipboard.png");
                Assert.AreEqual("Copy Image to Clipboard", ((Button)copyImageStack.Children[1]).Content);

                var pasteImageStack = (StackPanel)clipboardPage.Examples[5].ExampleContent;
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

                var dialogsPage = new ItemPage(GalleryCatalog.FindItem("FileAndFolderDialogs"));
                Assert.AreEqual(4, dialogsPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "Pick Single File", "Pick Multiple Files", "Save File", "Pick Folder" },
                    dialogsPage.Examples.Select(example => example.HeaderText).ToArray());
                AssertButtonResultExample((StackPanel)dialogsPage.Examples[0].ExampleContent, "Pick a single file", "No file selected");
                AssertButtonResultExample((StackPanel)dialogsPage.Examples[1].ExampleContent, "Pick multiple files", "No files selected");
                var saveFileStack = (StackPanel)dialogsPage.Examples[2].ExampleContent;
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
                AssertButtonResultExample((StackPanel)dialogsPage.Examples[3].ExampleContent, "Pick a folder", "No folder selected");

                var messageBoxPage = new ItemPage(GalleryCatalog.FindItem("MessageBox"));
                Assert.AreEqual(6, messageBoxPage.Examples.Count);
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
                    messageBoxPage.Examples.Select(example => example.HeaderText).ToArray());

                AssertButtonResultExample((StackPanel)messageBoxPage.Examples[0].ExampleContent, "Simple MessageBox", "No message shown yet");
                AssertButtonResultExample((StackPanel)messageBoxPage.Examples[1].ExampleContent, "Custom MessageBox", "No message shown yet");
                AssertMessageBoxSelectorExample((Grid)messageBoxPage.Examples[2].ExampleContent, "Button Type:", "MessageBox with Different Buttons", "MessageBox Button Selector", "No button clicked yet", new[] { "OK", "OK/Cancel", "Abort/Retry/Ignore", "Yes/No/Cancel", "Yes/No", "Retry/Cancel", "Cancel/Try/Continue" });
                AssertMessageBoxSelectorExample((Grid)messageBoxPage.Examples[3].ExampleContent, "Icon Type:", "MessageBox with different images", "MessageBox Image Selector", "No image example shown yet", new[] { "None", "Error", "Question", "Warning", "Information" });

                var commonMessagesStack = (StackPanel)messageBoxPage.Examples[4].ExampleContent;
                var commonButtons = ((WrapPanel)commonMessagesStack.Children[0]).Children.OfType<Button>().ToArray();
                CollectionAssert.AreEqual(new[] { "Information", "Error", "Warning" }, commonButtons.Select(button => (string)button.Content).ToArray());
                Assert.AreEqual(new Thickness(0, 0, 5, 0), commonButtons[0].Margin);
                Assert.AreEqual(new Thickness(0, 0, 5, 0), commonButtons[1].Margin);
                Assert.AreEqual(new Thickness(0), commonButtons[2].Margin);
                Assert.AreEqual("No common message shown yet", ((TextBlock)commonMessagesStack.Children[1]).Text);

                AssertButtonResultExample((StackPanel)messageBoxPage.Examples[5].ExampleContent, "Show with 'No' as default", "No selection made");
            });
        }

        [TestMethod]
        public void NavigationCollectionsAndHyperlinkPagesMatchWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var menuPage = new ItemPage(GalleryCatalog.FindItem("Menu"));
                Assert.AreEqual(1, menuPage.Examples.Count);
                Assert.AreEqual("Standard Menu.", menuPage.Examples[0].HeaderText);
                var menuStack = (StackPanel)menuPage.Examples[0].ExampleContent;
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

                var framePage = new ItemPage(GalleryCatalog.FindItem("Frame"));
                Assert.AreEqual("A Frame", framePage.Examples[0].HeaderText);
                var frameButton = (Button)framePage.Examples[0].ExampleContent;
                Assert.AreEqual("OpenFrameWindow", frameButton.Name);
                Assert.AreEqual("Open window to view Frame", frameButton.Content);
                Assert.AreEqual(HorizontalAlignment.Center, frameButton.HorizontalAlignment);
                Assert.AreEqual(VerticalAlignment.Center, frameButton.VerticalAlignment);

                var navigationWindowPage = new ItemPage(GalleryCatalog.FindItem("NavigationWindow"));
                Assert.AreEqual("A Navigation Window", navigationWindowPage.Examples[0].HeaderText);
                var navigationWindowButton = (Button)navigationWindowPage.Examples[0].ExampleContent;
                Assert.AreEqual("OpenNavigationWindow", navigationWindowButton.Name);
                Assert.AreEqual("Open window to view NavigationWindow", navigationWindowButton.Content);
                Assert.AreEqual(HorizontalAlignment.Center, navigationWindowButton.HorizontalAlignment);
                Assert.AreEqual(VerticalAlignment.Center, navigationWindowButton.VerticalAlignment);

                var tabControlPage = new ItemPage(GalleryCatalog.FindItem("TabControl"));
                Assert.AreEqual(1, tabControlPage.Examples.Count);
                Assert.AreEqual("Standard TabControl.", tabControlPage.Examples[0].HeaderText);
                var tabControl = (TabControl)tabControlPage.Examples[0].ExampleContent;
                Assert.AreEqual(new Thickness(0, 8, 0, 0), tabControl.Margin);
                Assert.AreEqual(2, tabControl.Items.Count);
                AssertTabItem((TabItem)tabControl.Items[0], "Hello", "Hello Tab", "World", false);
                AssertTabItem((TabItem)tabControl.Items[1], "The cake", "The cake Tab", "Is a lie.", true);

                var listBoxPage = new ItemPage(GalleryCatalog.FindItem("ListBox"));
                Assert.AreEqual(2, listBoxPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "ListBox with items defined inline.", "A ListBox with its ItemsSource and Height set." },
                    listBoxPage.Examples.Select(example => example.HeaderText).ToArray());
                var colorListBox = (ListBox)listBoxPage.Examples[0].ExampleContent;
                Assert.AreEqual("Color ListBox", AutomationProperties.GetName(colorListBox));
                Assert.AreEqual(0, colorListBox.SelectedIndex);
                CollectionAssert.AreEqual(new[] { "Blue", "Green", "Red", "Yellow" }, colorListBox.Items.Cast<ListBoxItem>().Select(item => (string)item.Content).ToArray());
                var fontListBox = (ListBox)listBoxPage.Examples[1].ExampleContent;
                Assert.AreEqual(164.0, fontListBox.Height);
                Assert.AreEqual("Font ListBox", AutomationProperties.GetName(fontListBox));
                Assert.AreEqual(2, fontListBox.SelectedIndex);
                CollectionAssert.AreEqual(new[] { "Arial", "Comic Sans MS", "Courier New", "Segoe UI", "Times New Roman" }, fontListBox.ItemsSource.Cast<string>().ToArray());

                var treeViewPage = new ItemPage(GalleryCatalog.FindItem("TreeView"));
                Assert.AreEqual(1, treeViewPage.Examples.Count);
                Assert.AreEqual("Simple TreeView.", treeViewPage.Examples[0].HeaderText);
                var treeView = (TreeView)treeViewPage.Examples[0].ExampleContent;
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

                var dataGridPage = new ItemPage(GalleryCatalog.FindItem("DataGrid"));
                Assert.AreEqual(1, dataGridPage.Examples.Count);
                Assert.AreEqual("Default DataGrid with ItemsSource.", dataGridPage.Examples[0].HeaderText);
                var dataGrid = (DataGrid)dataGridPage.Examples[0].ExampleContent;
                Assert.AreEqual("SampleDataGrid", dataGrid.Name);
                Assert.AreEqual(400.0, dataGrid.Height);
                Assert.AreEqual("Sample Data Grid", AutomationProperties.GetName(dataGrid));
                Assert.AreEqual(50, dataGrid.ItemsSource.Cast<object>().Count());

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
                var spacingBody = (StackPanel)spacingPage.PageBodyContent;
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
                StringAssert.Contains(((BitmapImage)cardsImage.Source).UriSource.ToString(), "Cards.dark.png");

                var typographyPage = new ItemPage(GalleryCatalog.FindItem("Typography"));
                var typographyBody = (StackPanel)typographyPage.PageBodyContent;
                var typeRampExample = (ControlExample)typographyBody.Children[3];
                var typeRamp = (Grid)typeRampExample.ExampleContent;
                var rows = typeRamp.Children.OfType<Grid>().OrderBy(Grid.GetRow).ToArray();
                Assert.AreEqual(7, rows.Length);

                var bodyStrongStyleName = GetTableText(rows.Single(row => Grid.GetRow(row) == 3), 3);
                Assert.AreEqual("CaptionTextBlockStyle", bodyStrongStyleName.Text);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), rows.Single(row => Grid.GetRow(row) == 5).Margin);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), rows.Single(row => Grid.GetRow(row) == 6).Margin);
                var displayRow = rows.Single(row => Grid.GetRow(row) == 7);
                Assert.AreEqual(68.0, displayRow.MinHeight);
                Assert.AreEqual(new Thickness(0, 0, 0, 24), displayRow.Margin);
                Assert.AreEqual("Consolas", GetTableText(displayRow, 3).FontFamily.Source);

                var geometryPage = new ItemPage(GalleryCatalog.FindItem("Geometry"));
                var geometryBody = (StackPanel)geometryPage.PageBodyContent;
                Assert.AreEqual(new Thickness(0, 0, 0, 24), geometryBody.Margin);
                Assert.AreEqual(5, geometryBody.Children.Count);
                Assert.AreEqual("Geometry describes the shape, size and position of UI elements on screen.", ((TextBlock)geometryBody.Children[0]).Text);
                Assert.AreEqual("These fundamental design elements help experiences feel coherent across the entire design system.", ((TextBlock)geometryBody.Children[1]).Text);
                var geometryUsage = (TextBlock)geometryBody.Children[2];
                Assert.AreEqual("You can reference built-in corner radii styles using: CornerRadius=\"{StaticResource ControlCornerRadius}\".", geometryUsage.Text);
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
                var body = (StackPanel)page.PageBodyContent;
                Assert.AreEqual(3, body.Children.Count);
                var selector = (ComboBox)body.Children[1];
                var sectionHost = (ContentControl)body.Children[2];

                CollectionAssert.AreEqual(
                    new[] { "Text", "Fill", "Stroke", "Background", "Signal", "HighContrast" },
                    selector.Items.Cast<string>().ToArray());
                Assert.AreEqual(200.0, selector.Width);
                Assert.AreEqual("Page Selector", AutomationProperties.GetName(selector));

                var textSection = (StackPanel)sectionHost.Content;
                Assert.AreEqual("Text", GetColorPageExampleTitle(textSection, 0));
                Assert.AreEqual("Accent Text", GetColorPageExampleTitle(textSection, 2));
                Assert.AreEqual("Text On Accent", GetColorPageExampleTitle(textSection, 4));

                var firstTilesPanel = (Border)textSection.Children[1];
                var firstTilesGrid = (Grid)firstTilesPanel.Child;
                Assert.AreEqual(4, firstTilesGrid.ColumnDefinitions.Count);
                Assert.AreEqual("Text / Primary", AutomationProperties.GetName((UIElement)firstTilesGrid.Children[0]));
                Assert.AreEqual("Text / Disabled", AutomationProperties.GetName((UIElement)firstTilesGrid.Children[3]));

                var firstTextTile = (Border)firstTilesGrid.Children[0];
                Assert.AreEqual(new CornerRadius(8, 0, 0, 8), firstTextTile.CornerRadius);
                var lastTextTile = (Border)firstTilesGrid.Children[3];
                Assert.AreEqual(new CornerRadius(0, 8, 8, 0), lastTextTile.CornerRadius);

                selector.SelectedIndex = 1;
                WpfTestHost.DoEvents();
                var fillSection = (StackPanel)sectionHost.Content;
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
                Assert.AreEqual("Control / Default", AutomationProperties.GetName((UIElement)controlFillTiles.Children[0]));
                Assert.AreEqual("Control / Quartenary", AutomationProperties.GetName((UIElement)controlFillTiles.Children[3]));

                var controlFillSecondRow = GetColorTilesGrid(fillSection, 2);
                Assert.AreEqual(3, controlFillSecondRow.ColumnDefinitions.Count);
                Assert.AreEqual("Control / Disabled", AutomationProperties.GetName((UIElement)controlFillSecondRow.Children[0]));
                Assert.AreEqual("Control / Input Active", AutomationProperties.GetName((UIElement)controlFillSecondRow.Children[2]));

                var accentFillSecondRow = GetColorTilesGrid(fillSection, 16);
                Assert.AreEqual("Accent / Selected Text Background", AutomationProperties.GetName((UIElement)accentFillSecondRow.Children[1]));

                selector.SelectedIndex = 2;
                WpfTestHost.DoEvents();
                var strokeSection = (StackPanel)sectionHost.Content;
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
                Assert.AreEqual("Control / Border", AutomationProperties.GetName((UIElement)elevationTiles.Children[0]));
                Assert.AreEqual("Text Control / Border", AutomationProperties.GetName((UIElement)elevationTiles.Children[2]));

                var controlStrokeSecondRow = GetColorTilesGrid(strokeSection, 5);
                Assert.AreEqual(3, controlStrokeSecondRow.ColumnDefinitions.Count);
                Assert.AreEqual("Control Stroke / For Strong Fill When On Image", AutomationProperties.GetName((UIElement)controlStrokeSecondRow.Children[2]));

                var focusTiles = GetColorTilesGrid(strokeSection, 15);
                Assert.AreEqual("Focus / Outer", AutomationProperties.GetName((UIElement)focusTiles.Children[0]));
                Assert.AreEqual("Focus / Inner", AutomationProperties.GetName((UIElement)focusTiles.Children[1]));

                selector.SelectedIndex = 3;
                WpfTestHost.DoEvents();
                var backgroundSection = (StackPanel)sectionHost.Content;
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
                Assert.AreEqual("Card Background / Tertiary", AutomationProperties.GetName((UIElement)cardTiles.Children[2]));

                var micaTiles = GetColorTilesGrid(backgroundSection, 15);
                Assert.AreEqual("Mica Background / Base Alt", AutomationProperties.GetName((UIElement)micaTiles.Children[1]));

                var accentAcrylicTiles = GetColorTilesGrid(backgroundSection, 19);
                Assert.AreEqual("Accent Acrylic Background / Default", AutomationProperties.GetName((UIElement)accentAcrylicTiles.Children[1]));

                selector.SelectedIndex = 4;
                WpfTestHost.DoEvents();
                var signalSection = (StackPanel)sectionHost.Content;
                Assert.AreEqual(6, signalSection.Children.Count);
                Assert.AreEqual("System", GetColorPageExampleTitle(signalSection, 0));
                var signalStatusTiles = GetColorTilesGrid(signalSection, 1);
                Assert.AreEqual("System / Success", AutomationProperties.GetName((UIElement)signalStatusTiles.Children[0]));
                Assert.AreEqual("System / Critical", AutomationProperties.GetName((UIElement)signalStatusTiles.Children[2]));
                var signalNeutralTiles = GetColorTilesGrid(signalSection, 3);
                Assert.AreEqual("System / Solid Neutral", AutomationProperties.GetName((UIElement)signalNeutralTiles.Children[2]));
                var signalSolidAttentionTiles = GetColorTilesGrid(signalSection, 5);
                Assert.AreEqual(1, signalSolidAttentionTiles.ColumnDefinitions.Count);
                Assert.AreEqual("System / Solid Attention Background", AutomationProperties.GetName((UIElement)signalSolidAttentionTiles.Children[0]));

                selector.SelectedIndex = 5;
                WpfTestHost.DoEvents();
                var highContrastSection = (StackPanel)sectionHost.Content;
                Assert.AreEqual(9, highContrastSection.Children.Count);
                StringAssert.StartsWith(((TextBlock)highContrastSection.Children[0]).Text, "Below are the default highcontrast themes shown.");
                Assert.AreEqual("Aquatic", ((TextBlock)highContrastSection.Children[1]).Text);
                var aquaticTiles = (Grid)highContrastSection.Children[2];
                Assert.AreEqual(4, aquaticTiles.ColumnDefinitions.Count);
                Assert.AreEqual(2, aquaticTiles.RowDefinitions.Count);
                Assert.AreEqual(8, aquaticTiles.Children.Count);
                Assert.AreEqual("Window Text Color", AutomationProperties.GetName((UIElement)aquaticTiles.Children[0]));
                Assert.AreEqual("Grey Text Color / Disabled", AutomationProperties.GetName((UIElement)aquaticTiles.Children[7]));
                Assert.AreEqual("Night Sky", ((TextBlock)highContrastSection.Children[7]).Text);
                var nightSkyTiles = (Grid)highContrastSection.Children[8];
                Assert.AreEqual("Hotlight Color", AutomationProperties.GetName((UIElement)nightSkyTiles.Children[6]));
            });
        }

        [TestMethod]
        public void IconographyPageUsesWpfGalleryIconLibraryLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Iconography"));
                var body = (StackPanel)page.PageBodyContent;

                var instructions = (Expander)body.Children[0];
                Assert.AreEqual("Instructions on how to use Segoe Fluent Icons", instructions.Header);
                Assert.IsFalse(instructions.IsExpanded);
                Assert.AreEqual(new Thickness(2, -8, 0, 0), instructions.Margin);

                var libraryTitle = (TextBlock)body.Children[1];
                Assert.AreEqual("Fluent Icons Library", libraryTitle.Text);

                var searchHost = (Grid)body.Children[2];
                var searchBox = (TextBox)searchHost.Children[0];
                var searchPlaceholder = (TextBlock)searchHost.Children[1];
                Assert.AreEqual(500.0, searchBox.Width);
                Assert.AreEqual("Search Icons by Name, Tag", AutomationProperties.GetName(searchBox));
                Assert.AreEqual("Search Icons by Name, Tag", searchPlaceholder.Text);
                Assert.AreEqual(Visibility.Visible, searchPlaceholder.Visibility);

                var libraryGrid = (Grid)body.Children[3];
                Assert.AreEqual(2, libraryGrid.ColumnDefinitions.Count);
                Assert.AreEqual(300.0, libraryGrid.ColumnDefinitions[1].Width.Value);

                var iconsListView = libraryGrid.Children.OfType<ListView>().Single();
                Assert.AreEqual("Icons", AutomationProperties.GetName(iconsListView));
                Assert.AreEqual(520.0, iconsListView.Height);
                Assert.AreEqual(250, iconsListView.Items.Count);
                Assert.AreEqual(0, iconsListView.SelectedIndex);

                var detailsPane = libraryGrid.Children.OfType<Grid>().Single(child => Grid.GetColumn(child) == 1);
                Assert.AreEqual(300.0, detailsPane.Width);
                Assert.AreEqual(520.0, detailsPane.Height);
                var detailsStack = (StackPanel)((ScrollViewer)detailsPane.Children[0]).Content;
                var selectedName = (TextBlock)detailsStack.Children[0];
                var selectedGlyph = (TextBlock)detailsStack.Children[1];
                Assert.AreEqual("GlobalNavButton", selectedName.Text);
                Assert.AreNotEqual(string.Empty, selectedGlyph.Text);
                Assert.AreEqual("GlobalNavButton", GetIconDataValue(detailsStack, 2));
                Assert.AreEqual("E700", GetIconDataValue(detailsStack, 3));
                Assert.AreEqual("&#xE700;", GetIconDataValue(detailsStack, 4));
                Assert.AreEqual("\\xE700", GetIconDataValue(detailsStack, 5));

                var pagination = (Grid)body.Children[4];
                var navigation = (StackPanel)pagination.Children[0];
                var previousButton = (Button)navigation.Children[0];
                var pageText = (TextBlock)navigation.Children[1];
                var nextButton = (Button)navigation.Children[2];
                Assert.IsFalse(previousButton.IsEnabled);
                Assert.IsTrue(nextButton.IsEnabled);
                Assert.AreEqual("Page 1 of 7", pageText.Text);

                var pageSize = (StackPanel)pagination.Children[1];
                var pageSizeComboBox = (ComboBox)pageSize.Children[1];
                CollectionAssert.AreEqual(new[] { "100", "250", "500", "1000", "All" }, pageSizeComboBox.Items.Cast<string>().ToArray());
                Assert.AreEqual(1, pageSizeComboBox.SelectedIndex);

                searchBox.Text = "GlobalNavButton";
                WpfTestHost.DoEvents();
                Assert.AreEqual(Visibility.Collapsed, searchPlaceholder.Visibility);
                Assert.AreEqual("GlobalNavButton", selectedName.Text);
                Assert.IsTrue(iconsListView.Items.Count > 0);
                Assert.IsTrue(iconsListView.Items.Count < 250);

                searchBox.Text = string.Empty;
                pageSizeComboBox.SelectedIndex = 0;
                WpfTestHost.DoEvents();
                Assert.AreEqual(Visibility.Visible, searchPlaceholder.Visibility);
                Assert.AreEqual(100, iconsListView.Items.Count);
                Assert.AreEqual("Page 1 of 16", pageText.Text);
            });
        }

        [TestMethod]
        public void UserDashboardPageMatchesWpfGalleryReferenceLayoutAndBehavior()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("UserDashboard"));
                var root = (Grid)page.DirectPageContent;
                var window = new Window
                {
                    Width = 900,
                    Height = 720,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = root
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
                    Assert.AreEqual(0, userList.SelectedIndex);
                    var firstUserItem = (ListViewItem)userList.Items[0];
                    Assert.AreEqual("John Doe", AutomationProperties.GetName(firstUserItem));
                    var firstUserName = FindTextBlock((DependencyObject)firstUserItem.Content, "John Doe");
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
                    Assert.AreEqual("John", firstNameBox.Text);
                    Assert.IsTrue(firstNameBox.IsReadOnly);
                    Assert.AreEqual("Last Name", AutomationProperties.GetName(lastNameBox));
                    Assert.AreEqual("Doe", lastNameBox.Text);

                    var companyBox = (TextBox)form.Children[2];
                    var addressBox = (TextBox)form.Children[4];
                    Assert.AreEqual("Company", AutomationProperties.GetName(companyBox));
                    Assert.AreEqual("Address", AutomationProperties.GetName(addressBox));

                    var ageSlider = (Slider)form.Children[6];
                    Assert.AreEqual("Age", AutomationProperties.GetName(ageSlider));
                    Assert.AreEqual(21.0, ageSlider.Minimum);
                    Assert.AreEqual(62.0, ageSlider.Maximum);
                    Assert.IsTrue(ageSlider.IsSnapToTickEnabled);
                    Assert.IsFalse(ageSlider.IsEnabled);
                    Assert.AreEqual(37.0, ageSlider.Value);

                    var datePicker = (DatePicker)form.Children[8];
                    Assert.AreEqual("Date of Joining", AutomationProperties.GetName(datePicker));
                    Assert.IsFalse(datePicker.IsEnabled);

                    var graduatePanel = (StackPanel)form.Children[9];
                    var graduateCheckBox = (CheckBox)graduatePanel.Children[1];
                    Assert.AreEqual("Is user a new graduate ?", AutomationProperties.GetName(graduateCheckBox));
                    Assert.IsFalse(graduateCheckBox.IsEnabled);

                    var commands = (StackPanel)form.Children[10];
                    var savedStatus = (TextBlock)commands.Children[0];
                    var deletedStatus = (TextBlock)commands.Children[1];
                    var editButton = (Button)commands.Children[2];
                    var deleteButton = (Button)commands.Children[3];
                    var saveButton = (Button)commands.Children[4];
                    var cancelButton = (Button)commands.Children[5];
                    Assert.AreEqual("Saved!", savedStatus.Text);
                    Assert.AreEqual(Visibility.Collapsed, savedStatus.Visibility);
                    Assert.AreEqual("User John Doe Deleted!", deletedStatus.Text);
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

                    root.Width = 700;
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();
                    Assert.AreEqual(240.0, userList.Width);
                    Assert.AreEqual(new Thickness(-10, 0, -20, 0), detailsGrid.Margin);
                    Assert.AreEqual(Orientation.Vertical, header.Orientation);
                    Assert.AreEqual(1, Grid.GetRow(lastNamePanel));
                    Assert.AreEqual(2, Grid.GetColumnSpan(firstNamePanel));

                    root.Width = 500;
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
            var example = (Border)section.Children[childIndex];
            var grid = (Grid)example.Child;
            return ((TextBlock)grid.Children[0]).Text;
        }

        private static Grid GetColorTilesGrid(StackPanel section, int childIndex)
        {
            var tilesPanel = (Border)section.Children[childIndex];
            return (Grid)tilesPanel.Child;
        }

        private static string GetIconDataValue(StackPanel detailsStack, int rowIndex)
        {
            var row = (StackPanel)detailsStack.Children[rowIndex];
            var grid = (Grid)row.Children[1];
            return ((TextBlock)grid.Children[0]).Text;
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
            Assert.AreEqual(HorizontalAlignment.Left, table.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0, 10, 0, 10), table.Margin);
            Assert.AreEqual(3, table.ColumnDefinitions.Count);
            Assert.AreEqual(4, table.RowDefinitions.Count);
            CollectionAssert.AreEqual(new[] { 148.0, 400.0, 180.0 }, table.ColumnDefinitions.Select(column => column.Width.Value).ToArray());

            CollectionAssert.AreEqual(
                new[] { "Corner radius", "Usage", "Style" },
                table.Children.OfType<TextBlock>().Where(textBlock => Grid.GetRow(textBlock) == 0).OrderBy(Grid.GetColumn).Select(textBlock => textBlock.Text).ToArray());

            AssertCornerRadiusRow(table, 1, "8px", new CornerRadius(8), "Top-level containers such as app windows, flyouts, cards and dialogs.", "OverlayCornerRadius");
            AssertCornerRadiusRow(table, 2, "4px", new CornerRadius(4), "In-page elements such as controls and list backplates.", "ControlCornerRadius");
            AssertCornerRadiusRow(table, 3, "0px", new CornerRadius(0), "Straight edges that intersect with other straight edges.", "N/A");
        }

        private static void AssertCornerRadiusRow(Grid table, int row, string radiusText, CornerRadius radius, string usage, string styleName)
        {
            var rowGrid = table.Children.OfType<Grid>().Single(grid => Grid.GetRow(grid) == row);
            Assert.AreEqual(60.0, rowGrid.MinHeight);
            Assert.AreEqual(3, rowGrid.ColumnDefinitions.Count);
            Assert.AreEqual(3, Grid.GetColumnSpan(rowGrid));

            var sample = rowGrid.Children.OfType<StackPanel>().Single();
            Assert.AreEqual(Orientation.Horizontal, sample.Orientation);
            Assert.AreEqual(new Thickness(16, 0, 0, 0), sample.Margin);
            var shape = (Border)sample.Children[0];
            Assert.AreEqual(20.0, shape.Width);
            Assert.AreEqual(20.0, shape.Height);
            Assert.AreEqual(radius, shape.CornerRadius);
            Assert.AreEqual(radiusText, ((TextBlock)sample.Children[1]).Text);

            var usageText = rowGrid.Children.OfType<TextBlock>().Single(textBlock => Grid.GetColumn(textBlock) == 1);
            var styleText = rowGrid.Children.OfType<TextBlock>().Single(textBlock => Grid.GetColumn(textBlock) == 2);
            Assert.AreEqual(usage, usageText.Text);
            Assert.AreEqual(styleName, styleText.Text);
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

        private static void AssertSettingsSectionHeader(TextBlock header, string expectedText)
        {
            Assert.AreEqual(expectedText, header.Text);
            Assert.AreEqual(new Thickness(10), header.Margin);
            Assert.AreEqual(14.0, header.FontSize);
            Assert.AreEqual(FontWeights.SemiBold, header.FontWeight);
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
            CollectionAssert.AreEqual(expectedItems, comboBox.Items.Cast<object>().Select(item => item.ToString()).ToArray());
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

        private static void AssertSliderExample(GalleryExample example, string automationName, double minimum, double maximum, double value, double tickFrequency, TickPlacement tickPlacement, Orientation orientation)
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
