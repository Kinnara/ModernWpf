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
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class TextSampleFactory
    {
        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
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
            var panel = CreateSamplePanel("AutoSuggestBox filters suggestions while the user types and raises query events.");
            var suggestions = new[]
            {
                "Alpine Ski House",
                "Blue Yonder Airlines",
                "City Power and Light",
                "Contoso Suites",
                "Fabrikam Residences",
                "Graphic Design Institute",
                "Northwind Traders",
                "Tailspin Toys"
            };
            var output = CreateOutput("Type to filter suggestions.");
            var box = new Mux.AutoSuggestBox
            {
                Width = 360,
                Header = "Search",
                PlaceholderText = "Search businesses",
                QueryIcon = new Mux.SymbolIcon(Mux.Symbol.Find),
                ItemsSource = suggestions
            };
            box.TextChanged += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxTextChangedEventArgs args)
            {
                if (args.Reason == Mux.AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    sender.ItemsSource = suggestions
                        .Where(item => item.IndexOf(sender.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }
            };
            box.QuerySubmitted += delegate(Mux.AutoSuggestBox sender, Mux.AutoSuggestBoxQuerySubmittedEventArgs args)
            {
                output.Text = "Query submitted: " + args.QueryText;
            };

            panel.Children.Add(box);
            panel.Children.Add(output);
            return panel;
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
            var panel = CreateSamplePanel("RichEditBox maps to WPF RichTextBox for editable formatted content.");
            var richTextBox = new RichTextBox
            {
                Width = 470,
                Height = 220,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run("Select text and apply formatting with the toolbar.")));
            richTextBox.Document.Blocks.Add(new Paragraph(new Run("This WPF RichTextBox supports bold, italic, underline, and document flow.")));

            var toolbar = CreateCommandRow();
            toolbar.Children.Add(CreateCommandButton("Bold", EditingCommands.ToggleBold, richTextBox));
            toolbar.Children.Add(CreateCommandButton("Italic", EditingCommands.ToggleItalic, richTextBox));
            toolbar.Children.Add(CreateCommandButton("Underline", EditingCommands.ToggleUnderline, richTextBox));
            var readOnly = new ToggleButton
            {
                Content = "Read-only",
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(0, 0, 8, 0)
            };
            readOnly.Checked += delegate { richTextBox.IsReadOnly = true; };
            readOnly.Unchecked += delegate { richTextBox.IsReadOnly = false; };
            toolbar.Children.Add(readOnly);

            panel.Children.Add(toolbar);
            panel.Children.Add(richTextBox);
            return panel;
        }

        private static UIElement CreateRichTextBlockSample()
        {
            var panel = CreateSamplePanel("RichTextBlock maps to a WPF read-only rich text surface with inline formatting and links.");
            var rich = new TextBlock
            {
                Width = 470,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16
            };
            rich.Inlines.Add(new Run("Rich text supports "));
            rich.Inlines.Add(new Bold(new Run("bold")));
            rich.Inlines.Add(new Run(", "));
            rich.Inlines.Add(new Italic(new Run("italic")));
            rich.Inlines.Add(new Run(", "));
            rich.Inlines.Add(new Underline(new Run("underline")));
            rich.Inlines.Add(new Run(", inline symbols, and "));
            rich.Inlines.Add(new Hyperlink(new Run("links")) { NavigateUri = new Uri("https://learn.microsoft.com/windows/apps/design/controls/text-controls") });
            rich.Inlines.Add(new Run("."));

            var bordered = new Border
            {
                Width = 500,
                Padding = new Thickness(18),
                BorderThickness = new Thickness(1),
                BorderBrush = CreateBrush("#D8D8D8"),
                Child = rich
            };
            panel.Children.Add(bordered);
            return panel;
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
            var fileName = System.IO.Path.GetFileName(relativePath);
            for (var i = 0; i < snippets.Count; i++)
            {
                if (string.Equals(snippets[i].Title, fileName, StringComparison.Ordinal) ||
                    string.Equals(snippets[i].Title, relativePath, StringComparison.Ordinal))
                {
                    return snippets[i].Text;
                }
            }

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", "NumberBox", relativePath);
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
