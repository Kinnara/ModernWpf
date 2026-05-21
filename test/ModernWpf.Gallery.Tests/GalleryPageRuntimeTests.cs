using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void WhatsNewPageHeaderMatchesWpfGalleryReference()
        {
            WpfTestHost.Run(() =>
            {
                var page = new WhatsNewPage();
                var title = (TextBlock)page.FindName("WhatsNewTitleTextBlock");
                var description = (TextBlock)page.FindName("WhatsNewDescriptionTextBlock");

                Assert.AreEqual("What's new in WPF", title.Text);
                Assert.AreEqual("Discover all the new features, enhancements and APIs introduced in WPF", description.Text);
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
        public void ColorPageUsesWpfGallerySelectorAndTextSectionLayout()
        {
            WpfTestHost.Run(() =>
            {
                var page = new ItemPage(GalleryCatalog.FindItem("Color"));
                var body = (StackPanel)page.PageBodyContent;
                var colorContent = (StackPanel)body.Children[1];
                var selector = (ComboBox)colorContent.Children[0];
                var sectionHost = (ContentControl)colorContent.Children[1];

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
                Assert.AreEqual("Accent Fill", GetColorPageExampleTitle((StackPanel)sectionHost.Content, 0));

                selector.SelectedIndex = 4;
                WpfTestHost.DoEvents();
                Assert.AreEqual("System Fill", GetColorPageExampleTitle((StackPanel)sectionHost.Content, 0));

                selector.SelectedIndex = 5;
                WpfTestHost.DoEvents();
                Assert.AreEqual("High Contrast", GetColorPageExampleTitle((StackPanel)sectionHost.Content, 0));
            });
        }

        private static string GetColorPageExampleTitle(StackPanel section, int childIndex)
        {
            var example = (Border)section.Children[childIndex];
            var grid = (Grid)example.Child;
            return ((TextBlock)grid.Children[0]).Text;
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
