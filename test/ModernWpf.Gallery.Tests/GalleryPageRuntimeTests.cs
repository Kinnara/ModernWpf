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
                Assert.AreEqual(4, textBlockPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[]
                    {
                        "A simple TextBlock.",
                        "A TextBlock with style applied.",
                        "A TextBlock with inline text elements.",
                        "A TextBlock with wrap property."
                    },
                    textBlockPage.Examples.Select(example => example.HeaderText).ToArray());

                var simpleTextBlock = (TextBlock)textBlockPage.Examples[0].ExampleContent;
                Assert.AreEqual("I am a text block.", simpleTextBlock.Text);

                var styledTextBlock = (TextBlock)textBlockPage.Examples[1].ExampleContent;
                Assert.AreEqual("I am a styled TextBlock.", styledTextBlock.Text);
                Assert.AreEqual("Comic Sans MS", styledTextBlock.FontFamily.Source);
                Assert.AreEqual(FontStyles.Italic, styledTextBlock.FontStyle);

                var inlineTextBlock = (TextBlock)textBlockPage.Examples[2].ExampleContent;
                Assert.AreEqual(14.0, inlineTextBlock.FontSize);
                var inlines = inlineTextBlock.Inlines.ToArray();
                Assert.AreEqual(9, inlines.Length);
                var firstRun = (Run)inlines[0];
                Assert.AreEqual("Text in a TextBlock doesn't have to be a simple string.", firstRun.Text);
                Assert.AreEqual("Times New Roman", firstRun.FontFamily.Source);
                Assert.IsInstanceOfType(inlines[1], typeof(LineBreak));
                Assert.AreEqual("bold", ((Bold)inlines[3]).Inlines.OfType<Run>().Single().Text);
                Assert.AreEqual("italic", ((Italic)inlines[5]).Inlines.OfType<Run>().Single().Text);
                Assert.AreEqual("underlined", ((Underline)inlines[7]).Inlines.OfType<Run>().Single().Text);

                var wrappedTextBlock = (TextBlock)textBlockPage.Examples[3].ExampleContent;
                Assert.AreEqual(14.0, wrappedTextBlock.FontSize);
                Assert.AreEqual(TextWrapping.Wrap, wrappedTextBlock.TextWrapping);
                StringAssert.StartsWith(wrappedTextBlock.Text, "The TextBlock control provides flexible text support");

                var textBoxPage = new ItemPage(GalleryCatalog.FindItem("TextBox"));
                Assert.AreEqual(3, textBoxPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple TextBox.", "A TextBox with input validation.", "A multi-line TextBox." },
                    textBoxPage.Examples.Select(example => example.HeaderText).ToArray());

                var simpleTextBox = (TextBox)textBoxPage.Examples[0].ExampleContent;
                Assert.AreEqual("simple TextBox", AutomationProperties.GetName(simpleTextBox));

                var validatedTextBox = (TextBox)textBoxPage.Examples[1].ExampleContent;
                Assert.AreEqual("validated TextBox", AutomationProperties.GetName(validatedTextBox));
                var textBinding = BindingOperations.GetBinding(validatedTextBox, TextBox.TextProperty);
                Assert.IsNotNull(textBinding);
                Assert.AreEqual(UpdateSourceTrigger.PropertyChanged, textBinding.UpdateSourceTrigger);
                Assert.AreEqual(1, textBinding.ValidationRules.Count);
                Assert.AreEqual("AlphabeticValidationRule", textBinding.ValidationRules[0].GetType().Name);

                var multilineTextBox = (TextBox)textBoxPage.Examples[2].ExampleContent;
                Assert.IsTrue(multilineTextBox.AcceptsReturn);
                Assert.AreEqual(TextWrapping.Wrap, multilineTextBox.TextWrapping);
                Assert.AreEqual("multi-line TextBox", AutomationProperties.GetName(multilineTextBox));

                var labelPage = new ItemPage(GalleryCatalog.FindItem("Label"));
                Assert.AreEqual(2, labelPage.Examples.Count);
                CollectionAssert.AreEqual(
                    new[] { "A simple Label.", "A Label for TextBox." },
                    labelPage.Examples.Select(example => example.HeaderText).ToArray());

                var simpleLabel = (Label)labelPage.Examples[0].ExampleContent;
                Assert.AreEqual("I am a Label.", simpleLabel.Content);
                Assert.AreEqual(0.7, simpleLabel.Opacity);

                var labelGrid = (Grid)labelPage.Examples[1].ExampleContent;
                Assert.AreEqual(2, labelGrid.RowDefinitions.Count);
                var textBoxLabel = labelGrid.Children.OfType<Label>().Single();
                var labelledTextBox = labelGrid.Children.OfType<TextBox>().Single();
                Assert.AreEqual(0, Grid.GetRow(textBoxLabel));
                Assert.AreEqual("I am a Label of the TextBox below.", textBoxLabel.Content);
                Assert.AreEqual(0.7, textBoxLabel.Opacity);
                Assert.AreEqual(1, Grid.GetRow(labelledTextBox));
                Assert.AreEqual("Simple Text Box", AutomationProperties.GetName(labelledTextBox));

                var passwordBoxPage = new ItemPage(GalleryCatalog.FindItem("PasswordBox"));
                Assert.AreEqual(1, passwordBoxPage.Examples.Count);
                Assert.AreEqual("A simple PasswordBox.", passwordBoxPage.Examples[0].HeaderText);
                Assert.AreEqual("Simple Password Box", AutomationProperties.GetName((PasswordBox)passwordBoxPage.Examples[0].ExampleContent));

                var richTextPage = new ItemPage(GalleryCatalog.FindItem("RichTextEdit"));
                Assert.AreEqual(1, richTextPage.Examples.Count);
                Assert.AreEqual("A simple RichTextBox", richTextPage.Examples[0].HeaderText);
                var richTextBox = (RichTextBox)richTextPage.Examples[0].ExampleContent;
                Assert.AreEqual("simple rich text editor", AutomationProperties.GetName(richTextBox));
                Assert.IsTrue(double.IsNaN(richTextBox.Width));
                Assert.IsTrue(double.IsNaN(richTextBox.Height));
            });
        }

        [TestMethod]
        public void DesignGuidancePagesMatchWpfGalleryReferenceLayoutDetails()
        {
            WpfTestHost.Run(() =>
            {
                var spacingPage = new ItemPage(GalleryCatalog.FindItem("Spacing"));
                var spacingBody = (StackPanel)spacingPage.PageBodyContent;
                var images = (Grid)spacingBody.Children[2];
                Assert.AreEqual(2, images.ColumnDefinitions.Count);
                Assert.AreEqual(2, images.Children.Count);

                var cardsFrame = (Grid)images.Children[0];
                var dialogFrame = (Grid)images.Children[1];
                Assert.AreEqual(0, Grid.GetColumn(cardsFrame));
                Assert.AreEqual(1, Grid.GetColumn(dialogFrame));
                Assert.AreEqual(HorizontalAlignment.Stretch, cardsFrame.HorizontalAlignment);
                Assert.AreEqual(new Thickness(0, 0, 0, 8), ((TextBlock)cardsFrame.Children[0]).Margin);
                Assert.AreEqual(500.0, ((Border)cardsFrame.Children[1]).Height);
                Assert.AreEqual(HorizontalAlignment.Stretch, ((Border)cardsFrame.Children[1]).HorizontalAlignment);

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

        private static TextBlock GetTableText(Grid row, int column)
        {
            return row.Children.OfType<TextBlock>().Single(textBlock => Grid.GetColumn(textBlock) == column);
        }

        private static void AssertExampleMargins(string uniqueId, params Thickness[] expectedMargins)
        {
            var page = new ItemPage(GalleryCatalog.FindItem(uniqueId));
            Assert.AreEqual(expectedMargins.Length, page.Examples.Count, uniqueId);
            for (var i = 0; i < expectedMargins.Length; i++)
            {
                Assert.AreEqual(expectedMargins[i], page.Examples[i].Margin, uniqueId + " example " + i);
            }
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
