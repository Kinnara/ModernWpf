using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class TextSampleFactory
    {
        private const string AutoSuggestBoxBasicXaml =
@"<AutoSuggestBox TextChanged=""AutoSuggestBox_TextChanged""
                SuggestionChosen=""AutoSuggestBox_SuggestionChosen""
                Width=""300"" AutomationProperties.Name=""Basic AutoSuggestBox""/>";

        private const string AutoSuggestBoxBasicCSharp =
@"// List of cats
private List<string> Cats = new List<string>()
{
    ""Abyssinian"",
    ""Aegean"",
    ""American Bobtail"",
    ...
};

// Handle text change and present suitable items
private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
{
    // Since selecting an item will also change the text,
    // only listen to changes caused by user entering text.
    if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
    {
        var suitableItems = new List<string>();
        var splitText = sender.Text.ToLower().Split("" "");
        foreach(var cat in Cats)
        {
            var found = splitText.All((key)=>
            {
                return cat.ToLower().Contains(key);
            });
            if(found)
            {
                suitableItems.Add(cat);
            }
        }
        if(suitableItems.Count == 0)
        {
            suitableItems.Add(""No results found"");
        }
        sender.ItemsSource = suitableItems;
    }
}

// Handle user selecting an item, in our case just output the selected item.
private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
{
    SuggestionOutput.Text = args.SelectedItem.ToString();
}";

        private const string AutoSuggestBoxSearchXaml =
@"<AutoSuggestBox PlaceholderText=""Type a control name""
        TextChanged=""Control2_TextChanged""
        QueryIcon=""Find""
        QuerySubmitted=""Control2_QuerySubmitted""
        SuggestionChosen=""Control2_SuggestionChosen""/>";

        private const string RichTextBlockSimpleXaml =
@"<RichTextBlock>
    <Paragraph>I am a RichTextBlock.</Paragraph>
</RichTextBlock>";

        private const string RichTextBlockSelectionXaml =
@"<RichTextBlock SelectionHighlightColor=""Green"">
    <Paragraph>RichTextBlock provides a rich text display container that supports
        <Run FontStyle=""Italic"" FontWeight=""Bold"">formatted text</Run>,
        <Hyperlink NavigateUri=""https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.Documents.Hyperlink"">hyperlinks</Hyperlink>, inline images, and other rich content.</Paragraph>
    <Paragraph>RichTextBlock also supports a built-in overflow model.</Paragraph>
</RichTextBlock>";

        private const string RichTextBlockOverflowXaml =
@"<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition/>
        <ColumnDefinition/>
        <ColumnDefinition/>
    </Grid.ColumnDefinitions>
    <RichTextBlock Grid.Column=""0"" OverflowContentTarget=""{x:Bind firstOverflowContainer}"" TextAlignment=""Justify"" Margin=""12,0"">
        <Paragraph>
            Linked text containers allow text which does not fit in one element to overflow into a different element on the page.
            Creative use of linked text containers enables basic multicolumn support and other advanced page layouts.
        </Paragraph>
    <!-- Additional content not shown. -->
    </RichTextBlock>
    <RichTextBlockOverflow x:Name=""firstOverflowContainer"" OverflowContentTarget=""{x:Bind secondOverflowContainer}"" Grid.Column=""1"" Margin=""12,0""/>
    <RichTextBlockOverflow x:Name=""secondOverflowContainer"" Grid.Column=""2"" Margin=""12,0""/>
</Grid>";

        private const string RichTextBlockHighlightXaml =
@"<RichTextBlock x:Name=""TextHighlightingRichTextBlock"">
    <Paragraph>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua
    </Paragraph>
</RichTextBlock>";

        private const string RichTextBlockHighlightCSharp =
@"private void HighlightColorCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    // Get color to use
    var selectedItem = (sender as ComboBox).SelectedItem as ComboBoxItem;
    var color = Colors.Yellow;
    switch (selectedItem.Content as string)
    {
        case ""Yellow"":
            color = Colors.Yellow;
            break;
        case ""Red"":
            color = Colors.Red;
            break;
        case ""Blue"":
            color = Colors.Blue;
            break;
    }

    // Get text range and highlighter
    TextRange textRange = new TextRange()
    {
        StartIndex = 28,
        Length = 11
    };
    TextHighlighter highlighter = new TextHighlighter()
    {
        Background = new SolidColorBrush(color),
        Ranges = { textRange }
    };

    // Switch texthighlighter
    TextHighlightingRichTextBlock.TextHighlighters.Clear();
    TextHighlightingRichTextBlock.TextHighlighters.Add(highlighter);
}";

        private const string RichEditBoxSimpleXaml =
@"<RichEditBox x:Name=""editor"" AutomationProperties.Name=""simple text editor""/>";

        private const string RichEditBoxMathModeXaml =
@"<RichEditBox x:Name=""mathEditor"" FontSize=""16"" />";

        private const string RichEditBoxMathModeCSharp =
@"mathEditor.TextDocument.SetMathMode(RichEditMathMode.MathOnly);";

        private const string RichEditBoxMathMLXaml =
@"<RichEditBox x:Name=""mathEditor2"" FontSize=""16"" />";

        private static readonly string[] Cats =
        {
            "Abyssinian",
            "Aegean",
            "American Bobtail",
            "American Curl",
            "American Ringtail",
            "American Shorthair",
            "American Wirehair",
            "Aphrodite Giant",
            "Arabian Mau",
            "Asian cat",
            "Asian Semi-longhair",
            "Australian Mist",
            "Balinese",
            "Bambino",
            "Bengal",
            "Birman",
            "Brazilian Shorthair",
            "British Longhair",
            "British Shorthair",
            "Burmese",
            "Burmilla",
            "California Spangled",
            "Chantilly-Tiffany",
            "Chartreux",
            "Chausie",
            "Colorpoint Shorthair",
            "Cornish Rex",
            "Cymric",
            "Cyprus",
            "Devon Rex",
            "Donskoy",
            "Dragon Li",
            "Dwelf",
            "Egyptian Mau",
            "European Shorthair",
            "Exotic Shorthair",
            "Foldex",
            "German Rex",
            "Havana Brown",
            "Highlander",
            "Himalayan",
            "Japanese Bobtail",
            "Javanese",
            "Kanaani",
            "Khao Manee",
            "Kinkalow",
            "Korat",
            "Korean Bobtail",
            "Korn Ja",
            "Kurilian Bobtail",
            "Lambkin",
            "LaPerm",
            "Lykoi",
            "Maine Coon",
            "Manx",
            "Mekong Bobtail",
            "Minskin",
            "Napoleon",
            "Munchkin",
            "Nebelung",
            "Norwegian Forest Cat",
            "Ocicat",
            "Ojos Azules",
            "Oregon Rex",
            "Persian (modern)",
            "Persian (traditional)",
            "Peterbald",
            "Pixie-bob",
            "Ragamuffin",
            "Ragdoll",
            "Raas",
            "Russian Blue",
            "Russian White",
            "Sam Sawet",
            "Savannah",
            "Scottish Fold",
            "Selkirk Rex",
            "Serengeti",
            "Serrade Petit",
            "Siamese",
            "Siberian or\u00b4Siberian Forest Cat",
            "Singapura",
            "Snowshoe",
            "Sokoke",
            "Somali",
            "Sphynx",
            "Suphalak",
            "Thai",
            "Thai Lilac",
            "Tonkinese",
            "Toyger",
            "Turkish Angora",
            "Turkish Van",
            "Turkish Vankedisi",
            "Ukrainian Levkoy",
            "Wila Krungthep",
            "York Chocolate"
        };

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "AutoSuggestBox":
                    return CreateAutoSuggestBoxExamples();
                case "NumberBox":
                    return CreateNumberBoxExamples(sampleSnippets);
                case "RichEditBox":
                    return CreateRichEditBoxExamples(sampleSnippets);
                case "RichTextBlock":
                    return CreateRichTextBlockExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "AutoSuggestBox":
                    return CreateAutoSuggestBoxSample();
                case "Hyperlink":
                    return CreateHyperlinkSample();
                case "Label":
                    return CreateLabelSample();
                case "NumberBox":
                    return CreateNumberBoxSample();
                case "PasswordBox":
                    return CreatePasswordBoxSample();
                case "RichEditBox":
                case "RichTextEdit":
                    return CreateRichEditBoxSample();
                case "RichTextBlock":
                    return CreateRichTextBlockSample();
                case "TextBlock":
                    return CreateTextBlockSample();
                case "TextBox":
                    return CreateTextBoxSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateHyperlinkSample()
        {
            var panel = CreateSamplePanel("Hyperlink is an inline text element that can raise navigation requests from text content.");
            var output = CreateOutput("Click a link.");
            var text = new TextBlock
            {
                Width = 460,
                TextWrapping = TextWrapping.Wrap
            };
            text.Inlines.Add(new Run("Open the "));
            var docs = new Hyperlink(new Run("WPF documentation"));
            docs.Click += delegate { output.Text = "Hyperlink clicked: WPF documentation"; };
            text.Inlines.Add(docs);
            text.Inlines.Add(new Run(" or "));
            var account = new Hyperlink(new Run("account page"));
            account.Click += delegate { output.Text = "Hyperlink clicked: account page"; };
            text.Inlines.Add(account);
            text.Inlines.Add(new Run(" from inline content."));

            panel.Children.Add(text);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateLabelSample()
        {
            var panel = CreateSamplePanel("Label identifies another control and can move focus to it through an access key.");
            var textBox = new TextBox
            {
                Width = 320,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var label = new Label
            {
                Content = "_Name",
                Target = textBox,
                Padding = new Thickness(0, 0, 0, 4)
            };

            panel.Children.Add(label);
            panel.Children.Add(textBox);
            panel.Children.Add(CreateOutput("Press Alt+N to focus the text box."));
            return panel;
        }

        private static UIElement CreateAutoSuggestBoxSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("AutoSuggestBox"));
            panel.Children.Add(CreateBasicAutoSuggestBoxExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateAutoSuggestBoxExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A basic autosuggest box.",
                    CreateBasicAutoSuggestBoxExampleContent(assignRootAutomationId: true),
                    AutoSuggestBoxBasicXaml,
                    AutoSuggestBoxBasicCSharp),
                new GalleryExample(
                    "An AutoSuggestBox that provides a SearchBox experience",
                    CreateSearchAutoSuggestBoxExampleContent(),
                    AutoSuggestBoxSearchXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateBasicAutoSuggestBoxExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("AutoSuggestBox"));
            }

            var output = new TextBlock
            {
                Name = "SuggestionOutput",
                FontFamily = new FontFamily("Global User Interface")
            };

            var box = new Mux.AutoSuggestBox
            {
                Name = "Control1",
                Width = 300
            };
            AutomationProperties.SetName(box, "Basic AutoSuggestBox");
            GalleryAutomation.WithAutomationId(box, GalleryAutomation.SampleElementId("AutoSuggestBox", "AutoSuggestBox"));
            box.TextChanged += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxTextChangedEventArgs args)
            {
                if (args.Reason == Mux.AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    var suitableItems = new List<string>();
                    var splitText = sender.Text.ToLowerInvariant().Split(new[] { ' ' });
                    foreach (var cat in Cats)
                    {
                        var found = splitText.All(key => cat.ToLowerInvariant().Contains(key));
                        if (found)
                        {
                            suitableItems.Add(cat);
                        }
                    }

                    if (suitableItems.Count == 0)
                    {
                        suitableItems.Add("No results found");
                    }

                    sender.ItemsSource = suitableItems;
                }
            };
            box.SuggestionChosen += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxSuggestionChosenEventArgs args)
            {
                output.Text = args.SelectedItem == null ? string.Empty : args.SelectedItem.ToString();
            };

            panel.Children.Add(box);
            panel.Children.Add(output);
            return panel;
        }

        private static Grid CreateSearchAutoSuggestBoxExampleContent()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var details = new Grid
            {
                Name = "ControlDetails",
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Visibility = Visibility.Collapsed
            };
            details.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            details.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            details.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var image = new Image
            {
                Name = "ControlImage",
                Height = 75
            };
            details.Children.Add(image);
            Grid.SetRowSpan(image, 2);

            var title = new TextBlock
            {
                Name = "ControlTitle",
                Margin = new Thickness(8, 0, 0, 0)
            };
            Grid.SetColumn(title, 1);
            details.Children.Add(title);

            var subtitle = new TextBlock
            {
                Name = "ControlSubtitle",
                Margin = new Thickness(8, 0, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(subtitle, 1);
            Grid.SetRow(subtitle, 1);
            details.Children.Add(subtitle);

            var box = new Mux.AutoSuggestBox
            {
                Name = "Control2",
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left,
                PlaceholderText = "Type a control name",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find)
            };
            GalleryAutomation.WithAutomationId(box, GalleryAutomation.SampleElementId("AutoSuggestBox", "SearchBox"));
            box.TextChanged += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxTextChangedEventArgs args)
            {
                if (args.Reason == Mux.AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    var suggestions = SearchControls(sender.Text);
                    if (suggestions.Count > 0)
                    {
                        sender.ItemsSource = suggestions;
                    }
                    else
                    {
                        sender.ItemsSource = new[] { "No results found" };
                    }
                }
            };
            box.QuerySubmitted += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxQuerySubmittedEventArgs args)
            {
                if (args.ChosenSuggestion is GalleryItem chosenSuggestion)
                {
                    SelectControl(chosenSuggestion, details, image, title, subtitle);
                }
                else if (!string.IsNullOrEmpty(args.QueryText))
                {
                    var suggestions = SearchControls(sender.Text);
                    var firstItem = suggestions.FirstOrDefault();
                    if (firstItem != null)
                    {
                        SelectControl(firstItem, details, image, title, subtitle);
                    }
                }
            };
            box.SuggestionChosen += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxSuggestionChosenEventArgs args)
            {
                if (args.SelectedItem is GalleryItem control)
                {
                    sender.Text = control.Title;
                }
            };

            grid.Children.Add(box);
            Grid.SetRow(details, 1);
            grid.Children.Add(details);
            return grid;
        }

        private static void SelectControl(GalleryItem control, UIElement details, Image image, TextBlock title, TextBlock subtitle)
        {
            details.Visibility = Visibility.Visible;
            image.Source = control.ImageSource == null ? null : new BitmapImage(control.ImageSource);
            title.Text = control.Title;
            subtitle.Text = control.Subtitle;
        }

        private static List<GalleryItem> SearchControls(string query)
        {
            return GalleryCatalog.Search(query).ToList();
        }

        private static UIElement CreateNumberBoxSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("NumberBox"));
            panel.Children.Add(CreateExpressionNumberBoxExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateNumberBoxExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A NumberBox that evaluates expressions.",
                    CreateExpressionNumberBoxExampleContent(assignRootAutomationId: true),
                    "<NumberBox Header=\"Enter an expression:\" Value=\"NaN\" PlaceholderText=\"1 + 2^2\" AcceptsExpression=\"True\" />",
                    null),
                new GalleryExample(
                    "A NumberBox with a spin button.",
                    CreateSpinButtonNumberBoxExampleContent(),
                    "<NumberBox\r\n    x:Name=\"NumberBoxSpinButtonPlacementExample\"\r\n    Header=\"Enter an integer:\"\r\n    Value=\"1\"\r\n    SpinButtonPlacementMode=\"Inline\"\r\n    SmallChange=\"10\"\r\n    LargeChange=\"100\" />",
                    null),
                new GalleryExample(
                    "A formatted NumberBox that rounds to the nearest 0.25.",
                    CreateFormattedNumberBoxExampleContent(),
                    FindSampleCodeText(sampleSnippets, "NumberBoxSample3_xaml.txt"),
                    FindSampleCodeText(sampleSnippets, "NumberBoxSample3_cs.txt"))
            };
        }

        private static GallerySamplePanel CreateExpressionNumberBoxExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("NumberBox"));
            }

            var numberBox = new Mux.NumberBox
            {
                Name = "ExpressionNumberBox",
                Width = 124,
                AcceptsExpression = true,
                Header = "Enter an expression:",
                PlaceholderText = "1 + 2^2",
                Value = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(numberBox, GalleryAutomation.SampleElementId("NumberBox", "ExpressionNumberBox"));
            panel.Children.Add(numberBox);
            return panel;
        }

        private static GallerySamplePanel CreateSpinButtonNumberBoxExampleContent()
        {
            var panel = new GallerySamplePanel();
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var numberBox = new Mux.NumberBox
            {
                Name = "NumberBoxSpinButtonPlacementExample",
                Width = 132,
                VerticalAlignment = VerticalAlignment.Top,
                Header = "Enter an integer:",
                LargeChange = 100,
                SmallChange = 10,
                SpinButtonPlacementMode = Mux.NumberBoxSpinButtonPlacementMode.Inline,
                Value = 10
            };
            AutomationProperties.SetName(numberBox, "NumberBox with spin button");
            GalleryAutomation.WithAutomationId(numberBox, GalleryAutomation.SampleElementId("NumberBox", "SpinButtonNumberBox"));

            var radioButtons = new Mux.RadioButtons
            {
                Name = "SpinButtonPlacementGroup",
                Header = "SpinButton placement",
                SelectedIndex = 0
            };
            radioButtons.Items.Add("Inline");
            radioButtons.Items.Add("Compact");
            radioButtons.SelectionChanged += delegate
            {
                numberBox.SpinButtonPlacementMode = radioButtons.SelectedIndex == 0
                    ? Mux.NumberBoxSpinButtonPlacementMode.Inline
                    : Mux.NumberBoxSpinButtonPlacementMode.Compact;
            };

            layout.Children.Add(numberBox);
            Grid.SetColumn(radioButtons, 2);
            layout.Children.Add(radioButtons);
            panel.Children.Add(layout);
            return panel;
        }

        private static GallerySamplePanel CreateFormattedNumberBoxExampleContent()
        {
            var panel = new GallerySamplePanel();
            var numberBox = new Mux.NumberBox
            {
                Name = "FormattedNumberBox",
                Width = 137,
                Header = "Enter a dollar amount:",
                PlaceholderText = "0.00",
                NumberFormatter = new QuarterIncrementNumberFormatter(),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(numberBox, GalleryAutomation.SampleElementId("NumberBox", "FormattedNumberBox"));
            panel.Children.Add(numberBox);
            return panel;
        }

        private static UIElement CreatePasswordBoxSample()
        {
            var panel = CreateSamplePanel("PasswordBox masks user input and supports ModernWpf password reveal modes.");
            var passwordBox = new PasswordBox
            {
                Width = 260,
                Password = "modernwpf",
                MaxLength = 24
            };
            ControlHelper.SetHeader(passwordBox, "Password");
            ControlHelper.SetPlaceholderText(passwordBox, "Enter password");
            PasswordBoxHelper.SetIsEnabled(passwordBox, true);
            PasswordBoxHelper.SetPasswordRevealMode(passwordBox, Mux.PasswordRevealMode.Peek);

            var output = CreateOutput("Password length: " + passwordBox.Password.Length);
            passwordBox.PasswordChanged += delegate
            {
                output.Text = "Password length: " + passwordBox.Password.Length;
            };

            var mode = new ComboBox
            {
                Width = 220,
                Margin = new Thickness(0, 12, 0, 0),
                ItemsSource = new[] { Mux.PasswordRevealMode.Peek, Mux.PasswordRevealMode.Hidden, Mux.PasswordRevealMode.Visible },
                SelectedItem = Mux.PasswordRevealMode.Peek
            };
            ControlHelper.SetHeader(mode, "Reveal mode");
            mode.SelectionChanged += delegate
            {
                if (mode.SelectedItem is Mux.PasswordRevealMode)
                {
                    PasswordBoxHelper.SetPasswordRevealMode(passwordBox, (Mux.PasswordRevealMode)mode.SelectedItem);
                }
            };

            panel.Children.Add(passwordBox);
            panel.Children.Add(mode);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateRichEditBoxSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RichEditBox"));
            panel.Children.Add(CreateSimpleRichEditBox(assignSampleAutomationId: true));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateRichEditBoxExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A simple text editor using RichEditBox.",
                    CreateSimpleRichEditBoxExampleContent(assignRootAutomationId: true),
                    RichEditBoxSimpleXaml,
                    null),
                new GalleryExample(
                    "Customizing RichEditBox's CommandBarFlyout - Adding 'Share'",
                    CreateCustomCommandFlyoutRichEditBoxExampleContent(),
                    FindSampleCodeText(sampleSnippets, "RichEditBoxSample4_xaml.txt", System.IO.Path.Combine("Text", "RichEditBox")),
                    FindSampleCodeText(sampleSnippets, "RichEditBoxSample4_cs.txt", System.IO.Path.Combine("Text", "RichEditBox"))),
                new GalleryExample(
                    "A custom editor with RichEditBox.",
                    CreateCustomRichEditBoxExampleContent(),
                    FindSampleCodeText(sampleSnippets, "RichEditBoxSample3_xaml.txt", System.IO.Path.Combine("Text", "RichEditBox")),
                    FindSampleCodeText(sampleSnippets, "RichEditBoxSample3_cs.txt", System.IO.Path.Combine("Text", "RichEditBox"))),
                new GalleryExample(
                    "Rich edit box in math mode",
                    CreateMathModeRichEditBoxExampleContent(),
                    RichEditBoxMathModeXaml,
                    RichEditBoxMathModeCSharp),
                new GalleryExample(
                    "Working with MathML in RichEditBox",
                    CreateMathMLRichEditBoxExampleContent(),
                    RichEditBoxMathMLXaml,
                    FindSampleCodeText(sampleSnippets, "RichEditBoxSample6_cs.txt", System.IO.Path.Combine("Text", "RichEditBox")))
            };
        }

        private static GallerySamplePanel CreateSimpleRichEditBoxExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RichEditBox"));
            }

            panel.Children.Add(CreateSimpleRichEditBox(assignSampleAutomationId: true));
            return panel;
        }

        private static RichTextBox CreateSimpleRichEditBox(bool assignSampleAutomationId)
        {
            var richTextBox = new RichTextBox
            {
                Name = "editor",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 300,
                MinHeight = 32,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            AutomationProperties.SetName(richTextBox, "simple text editor");
            if (assignSampleAutomationId)
            {
                GalleryAutomation.WithAutomationId(richTextBox, GalleryAutomation.SampleElementId("RichEditBox", "RichEditBox"));
            }

            return richTextBox;
        }

        private static RichTextBox CreateRichEditBoxEditor(string name, string automationName, double width, double height)
        {
            var richTextBox = new RichTextBox
            {
                Name = name,
                Width = width,
                Height = height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            AutomationProperties.SetName(richTextBox, automationName);
            return richTextBox;
        }

        private static GallerySamplePanel CreateCustomCommandFlyoutRichEditBoxExampleContent()
        {
            var panel = new GallerySamplePanel();
            var richTextBox = CreateRichEditBoxEditor("REBCustom", "editor with custom menu", 800, 200);
            richTextBox.ContextMenu = new ContextMenu();
            richTextBox.ContextMenu.Items.Add(new MenuItem { Header = "Cut", Command = ApplicationCommands.Cut });
            richTextBox.ContextMenu.Items.Add(new MenuItem { Header = "Copy", Command = ApplicationCommands.Copy });
            richTextBox.ContextMenu.Items.Add(new MenuItem { Header = "Paste", Command = ApplicationCommands.Paste });
            richTextBox.ContextMenu.Items.Add(new Separator());
            richTextBox.ContextMenu.Items.Add(new MenuItem { Header = "Share" });
            panel.Children.Add(richTextBox);
            return panel;
        }

        private static Grid CreateCustomRichEditBoxExampleContent()
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var editor = new RichTextBox
            {
                Name = "editor",
                Height = 200,
                MinWidth = 300,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            AutomationProperties.SetName(editor, "Custom editor");

            var openFileButton = CreateIconButton("openFileButton", "Open file", "\uE8E5");
            openFileButton.Click += delegate { InsertRichEditBoxStatus(editor, "Open file"); };
            grid.Children.Add(openFileButton);

            var saveFileButton = CreateIconButton(null, "Save file", "\uE74E");
            saveFileButton.Click += delegate { InsertRichEditBoxStatus(editor, "Save file"); };
            Grid.SetColumn(saveFileButton, 1);
            grid.Children.Add(saveFileButton);

            var boldButton = CreateIconButton(null, "Bold", "\uE8DD");
            boldButton.Click += delegate
            {
                EditingCommands.ToggleBold.Execute(null, editor);
                editor.Focus();
            };
            Grid.SetColumn(boldButton, 3);
            grid.Children.Add(boldButton);

            var italicButton = CreateIconButton("italicButton", "Italic", "\uE8DB");
            italicButton.Click += delegate
            {
                EditingCommands.ToggleItalic.Execute(null, editor);
                editor.Focus();
            };
            Grid.SetColumn(italicButton, 4);
            grid.Children.Add(italicButton);

            var fontColorButton = new Mux.DropDownButton
            {
                Name = "fontColorButton",
                Content = CreateIconGlyph("\uE8D3"),
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Top
            };
            AutomationProperties.SetName(fontColorButton, "Font color");
            fontColorButton.Flyout = CreateRichEditBoxColorFlyout(delegate(string colorName, SolidColorBrush brush)
            {
                ApplyRichTextBoxForeground(editor, brush);
                fontColorButton.Flyout.Hide();
                editor.Focus();
            });
            Grid.SetColumn(fontColorButton, 5);
            grid.Children.Add(fontColorButton);

            Grid.SetRow(editor, 1);
            Grid.SetColumnSpan(editor, 6);
            grid.Children.Add(editor);

            var findPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };
            findPanel.Children.Add(new TextBlock
            {
                Name = "findBoxLabel",
                Text = "Find:",
                VerticalAlignment = VerticalAlignment.Center
            });
            var findBox = new TextBox
            {
                Name = "findBox",
                Width = 224,
                Margin = new Thickness(10, 0, 0, 0)
            };
            ControlHelper.SetPlaceholderText(findBox, "Enter search text");
            findBox.TextChanged += delegate
            {
                HighlightFindMatches(editor, findBox.Text);
            };
            findPanel.Children.Add(findBox);
            Grid.SetRow(findPanel, 2);
            Grid.SetColumnSpan(findPanel, 6);
            grid.Children.Add(findPanel);

            return grid;
        }

        private static GallerySamplePanel CreateMathModeRichEditBoxExampleContent()
        {
            var panel = new GallerySamplePanel();
            panel.Children.Add(CreateRichEditBoxDescription(
                "Math mode enables users to have input automatically recognized and converted to math expressions while being received.\n\n" +
                "It uses Unicode Nearly Plain-Text Encoding of Mathematics, which allows mathematical notation to be represented in a linear format and automatically converted into proper math equations.\n\n" +
                "For example, \"4^2\" is converted to a squared expression, and \"\\pi\" is converted to pi."));
            var editor = CreateRichEditBoxEditor("MathEditor", "Math editor", 724, 80);
            editor.FontSize = 16;
            editor.Document.Blocks.Clear();
            editor.Document.Blocks.Add(new Paragraph(new Run("4^2 + \\pi")));
            panel.Children.Add(editor);
            return panel;
        }

        private static GallerySamplePanel CreateMathMLRichEditBoxExampleContent()
        {
            var panel = new GallerySamplePanel();
            panel.Children.Add(CreateRichEditBoxDescription(
                "The SetMathML method takes a MathML string and displays the equation in the RichEditBox. It replaces any existing equation with the new one.\n\n" +
                "The GetMathML method retrieves the MathML string of the equation from the RichEditBox when the equation is in a single line."));

            var editor = CreateRichEditBoxEditor("mathEditor2", "MathML editor", 724, 80);
            editor.FontSize = 16;
            panel.Children.Add(editor);
            panel.Children.Add(new TextBlock
            {
                Text = "MathML Code",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 16, 0, 8)
            });

            var mathmlPresenter = new TextBox
            {
                Name = "MathmlPresenter",
                Text = "<!-- No MathML content -->",
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = new FontFamily("Consolas"),
                MinHeight = 96,
                MaxHeight = 220,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            panel.Children.Add(mathmlPresenter);

            var setFormulaButton = CreateButton("Set sample formula");
            setFormulaButton.Name = "SetMathmlFormulaBtn";
            setFormulaButton.Margin = new Thickness(0, 12, 0, 0);
            setFormulaButton.Click += delegate
            {
                var formulaText = "x in P(A) <=> x subset A";
                editor.Document.Blocks.Clear();
                editor.Document.Blocks.Add(new Paragraph(new Run(formulaText)));
                mathmlPresenter.Text = CreateSampleMathML();
            };
            panel.Children.Add(setFormulaButton);
            return panel;
        }

        private static TextBlock CreateRichEditBoxDescription(string text)
        {
            return new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private static Button CreateIconButton(string name, string automationName, string glyph)
        {
            var button = new Button
            {
                Content = CreateIconGlyph(glyph),
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(8),
                MinWidth = 32,
                MinHeight = 32
            };
            if (name != null)
            {
                button.Name = name;
            }

            AutomationProperties.SetName(button, automationName);
            ToolTipService.SetToolTip(button, automationName);
            return button;
        }

        private static TextBlock CreateIconGlyph(string glyph)
        {
            return new TextBlock
            {
                Text = glyph,
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private static Mux.Flyout CreateRichEditBoxColorFlyout(Action<string, SolidColorBrush> colorSelected)
        {
            var grid = new Mux.VariableSizedWrapGrid
            {
                MaximumRowsOrColumns = 3,
                Orientation = Orientation.Horizontal
            };

            foreach (var colorName in new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray" })
            {
                grid.Children.Add(CreateRichEditBoxColorButton(colorName, colorSelected));
            }

            return new Mux.Flyout
            {
                Placement = FlyoutPlacementMode.Bottom,
                Content = grid
            };
        }

        private static Button CreateRichEditBoxColorButton(string colorName, Action<string, SolidColorBrush> colorSelected)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
            var rectangle = new System.Windows.Shapes.Rectangle
            {
                Width = 32,
                Height = 32,
                RadiusX = 4,
                RadiusY = 4,
                Fill = brush
            };
            var button = new Button
            {
                Content = rectangle,
                Padding = new Thickness(0),
                MinWidth = 0,
                MinHeight = 0,
                Margin = new Thickness(6)
            };
            AutomationProperties.SetName(button, colorName);
            button.Click += delegate { colorSelected(colorName, brush); };
            return button;
        }

        private static void ApplyRichTextBoxForeground(RichTextBox richTextBox, Brush brush)
        {
            if (richTextBox.Selection == null || richTextBox.Selection.IsEmpty)
            {
                richTextBox.SelectAll();
            }

            richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        private static void HighlightFindMatches(RichTextBox richTextBox, string textToFind)
        {
            if (string.IsNullOrEmpty(textToFind))
            {
                return;
            }

            var documentRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            if (documentRange.Text.IndexOf(textToFind, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                richTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, SystemColors.HighlightBrush);
            }
        }

        private static void InsertRichEditBoxStatus(RichTextBox richTextBox, string status)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(status)));
        }

        private static string CreateSampleMathML()
        {
            return "<mml:math xmlns:mml=\"http://www.w3.org/1998/Math/MathML\" display=\"block\">\r\n" +
                "  <mml:mi mathcolor=\"#000000\">x</mml:mi>\r\n" +
                "  <mml:mo mathcolor=\"#000000\">&#x2208;</mml:mo>\r\n" +
                "  <mml:mi mathcolor=\"#000000\">P</mml:mi>\r\n" +
                "</mml:math>";
        }

        private static UIElement CreateRichTextBlockSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RichTextBlock"));
            panel.Children.Add(CreateSimpleRichTextBlock());
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateRichTextBlockExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple RichTextBlock.",
                    CreateSimpleRichTextBlockExampleContent(assignRootAutomationId: true),
                    RichTextBlockSimpleXaml,
                    null),
                new GalleryExample(
                    "A RichTextBlock with a custom selection highlight color.",
                    CreateSelectionRichTextBlockExampleContent(),
                    RichTextBlockSelectionXaml,
                    null),
                new GalleryExample(
                    "A RichTextBlock with overflow.",
                    CreateOverflowRichTextBlockExampleContent(),
                    RichTextBlockOverflowXaml,
                    null),
                new GalleryExample(
                    "RichTextBlock with custom TextHighlighting",
                    CreateHighlightedRichTextBlockExampleContent(),
                    RichTextBlockHighlightXaml,
                    RichTextBlockHighlightCSharp)
            };
        }

        private static GallerySamplePanel CreateSimpleRichTextBlockExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RichTextBlock"));
            }

            panel.Children.Add(CreateSimpleRichTextBlock());
            return panel;
        }

        private static TextBlock CreateSimpleRichTextBlock()
        {
            var textBlock = CreateRichTextBlockText();
            textBlock.Name = "SimpleRichTextBlock";
            textBlock.Text = "I am a RichTextBlock.";
            GalleryAutomation.WithAutomationId(textBlock, GalleryAutomation.SampleElementId("RichTextBlock", "RichTextBlock"));
            return textBlock;
        }

        private static TextBlock CreateSelectionRichTextBlockExampleContent()
        {
            var textBlock = CreateRichTextBlockText();
            textBlock.Name = "SelectionHighlightRichTextBlock";
            textBlock.Inlines.Add(new Run("RichTextBlock provides a rich text display container that supports "));
            textBlock.Inlines.Add(new Italic(new Bold(new Run("formatted text"))));
            textBlock.Inlines.Add(new Run(", "));
            textBlock.Inlines.Add(new Hyperlink(new Run("hyperlinks"))
            {
                NavigateUri = new Uri("https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.Documents.Hyperlink")
            });
            textBlock.Inlines.Add(new Run(", inline images, and other rich content."));
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new Run("RichTextBlock also supports a built-in overflow model."));
            return textBlock;
        }

        private static Grid CreateOverflowRichTextBlockExampleContent()
        {
            var grid = new Grid
            {
                Height = 300
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            AddOverflowColumn(grid, 0, "Linked text containers allow text which does not fit in one element to overflow into a different element on the page. Creative use of linked text containers enables basic multicolumn support and other advanced page layouts.");
            AddOverflowColumn(grid, 1, "Duis sed nulla metus, id hendrerit velit. Curabitur dolor purus, bibendum eu cursus lacinia, interdum vel augue. Aenean euismod eros et sapien vehicula dictum. Duis ullamcorper, turpis nec feugiat tincidunt, dui erat luctus risus, aliquam accumsan lacus est vel quam.");
            AddOverflowColumn(grid, 2, "Nunc lacus massa, varius eget accumsan id, congue sed orci. Duis dignissim hendrerit egestas. Proin ut turpis magna, sit amet porta erat. Nunc semper metus nec magna imperdiet nec vestibulum dui fringilla.");
            return grid;
        }

        private static GallerySamplePanel CreateHighlightedRichTextBlockExampleContent()
        {
            var panel = new GallerySamplePanel();
            var highlightedRun = new Run("consectetur")
            {
                Background = Brushes.Yellow
            };

            var textBlock = CreateRichTextBlockText();
            textBlock.Name = "TextHighlightingRichTextBlock";
            textBlock.Inlines.Add(new Run("Lorem ipsum dolor sit amet, "));
            textBlock.Inlines.Add(highlightedRun);
            textBlock.Inlines.Add(new Run(" adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua"));

            var colorComboBox = new ComboBox
            {
                MinWidth = 160,
                Margin = new Thickness(0, 12, 0, 0)
            };
            ControlHelper.SetHeader(colorComboBox, "Text highlighting color");
            colorComboBox.Items.Add("Yellow");
            colorComboBox.Items.Add("Red");
            colorComboBox.Items.Add("Blue");
            colorComboBox.SelectedIndex = 0;
            colorComboBox.SelectionChanged += delegate
            {
                highlightedRun.Background = CreateHighlightBrush(colorComboBox.SelectedItem as string);
            };

            panel.Children.Add(textBlock);
            panel.Children.Add(colorComboBox);
            return panel;
        }

        private static void AddOverflowColumn(Grid grid, int column, string text)
        {
            var textBlock = CreateRichTextBlockText();
            textBlock.Margin = new Thickness(12, 0, 12, 0);
            textBlock.TextAlignment = TextAlignment.Justify;
            textBlock.Text = text;
            if (column == 1)
            {
                textBlock.Name = "firstOverflowContainer";
            }
            else if (column == 2)
            {
                textBlock.Name = "secondOverflowContainer";
            }

            Grid.SetColumn(textBlock, column);
            grid.Children.Add(textBlock);
        }

        private static TextBlock CreateRichTextBlockText()
        {
            return new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static Brush CreateHighlightBrush(string color)
        {
            switch (color)
            {
                case "Red":
                    return Brushes.Red;
                case "Blue":
                    return Brushes.Blue;
                default:
                    return Brushes.Yellow;
            }
        }

        private static UIElement CreateTextBlockSample()
        {
            var panel = CreateSamplePanel("TextBlock displays read-only text with wrapping, trimming, and inline formatting.");
            var textBlock = new TextBlock
            {
                Width = 430,
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            textBlock.Inlines.Add(new Run("TextBlock can combine "));
            textBlock.Inlines.Add(new Bold(new Run("formatted")));
            textBlock.Inlines.Add(new Run(" inline runs while remaining lightweight."));

            var fontSize = new Slider
            {
                Width = 220,
                Minimum = 12,
                Maximum = 32,
                Value = textBlock.FontSize,
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ControlHelper.SetHeader(fontSize, "FontSize");
            fontSize.ValueChanged += delegate { textBlock.FontSize = Math.Round(fontSize.Value); };

            panel.Children.Add(textBlock);
            panel.Children.Add(fontSize);
            return panel;
        }

        private static UIElement CreateTextBoxSample()
        {
            var panel = CreateSamplePanel("TextBox accepts plain text input and can switch between single-line and multi-line editing.");
            var textBox = new TextBox
            {
                Width = 360,
                Text = "The quick brown fox jumps over the lazy dog.",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = false,
                SpellCheck = { IsEnabled = true }
            };
            ControlHelper.SetHeader(textBox, "Message");
            ControlHelper.SetPlaceholderText(textBox, "Enter text");
            TextBoxHelper.SetIsEnabled(textBox, true);

            var output = CreateOutput("Characters: " + textBox.Text.Length);
            textBox.TextChanged += delegate
            {
                output.Text = "Characters: " + textBox.Text.Length;
            };

            var multiline = new ToggleButton
            {
                Content = "Multi-line",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 12, 8, 0)
            };
            multiline.Checked += delegate
            {
                textBox.AcceptsReturn = true;
                textBox.Height = 130;
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            };
            multiline.Unchecked += delegate
            {
                textBox.AcceptsReturn = false;
                textBox.ClearValue(FrameworkElement.HeightProperty);
                textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            };
            var clear = CreateButton("Clear");
            clear.Margin = new Thickness(0, 12, 8, 0);
            clear.Click += delegate { textBox.Clear(); };

            var commands = new StackPanel { Orientation = Orientation.Horizontal };
            commands.Children.Add(multiline);
            commands.Children.Add(clear);

            panel.Children.Add(textBox);
            panel.Children.Add(commands);
            panel.Children.Add(output);
            return panel;
        }

        private static Button CreateCommandButton(string text, RoutedCommand command, RichTextBox target)
        {
            var button = CreateButton(text);
            button.Click += delegate
            {
                command.Execute(null, target);
                target.Focus();
            };
            return button;
        }

        private static StackPanel CreateCommandRow()
        {
            return new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private static StackPanel CreateSamplePanel(string description)
        {
            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            panel.Children.Add(new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72,
                Margin = new Thickness(0, 0, 0, 12)
            });
            return panel;
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Content = text,
                Padding = new Thickness(16, 6, 16, 6),
                Margin = new Thickness(0, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }

        private static TextBlock CreateOutput(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string relativePath)
        {
            return FindSampleCodeText(snippets, relativePath, "NumberBox");
        }

        private static string FindSampleCodeText(IReadOnlyList<SampleSnippet> snippets, string relativePath, string fallbackFolder)
        {
            var fileName = System.IO.Path.GetFileName(relativePath);
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, fileName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(snippets[i].Title, relativePath, StringComparison.OrdinalIgnoreCase))
                {
                    return snippets[i].Text;
                }
            }

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", fallbackFolder, relativePath);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        }

        private sealed class QuarterIncrementNumberFormatter : Mux.INumberBoxNumberFormatter
        {
            public string FormatDouble(double value)
            {
                var roundedValue = Math.Floor((value / 0.25) + 0.5) * 0.25;
                return roundedValue.ToString("0.00", CultureInfo.InvariantCulture);
            }

            public double? ParseDouble(string text)
            {
                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
                    double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                {
                    return value;
                }

                return null;
            }
        }

        private static SolidColorBrush CreateBrush(string color)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(color);
        }
    }
}
