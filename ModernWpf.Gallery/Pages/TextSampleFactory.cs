using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
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
                case "NumberBox":
                    return CreateNumberBoxSample();
                default:
                    return null;
            }
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
            GalleryAutomation.WithAutomationId(output, GalleryAutomation.SampleElementId("AutoSuggestBox", "SuggestionOutput"));

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
