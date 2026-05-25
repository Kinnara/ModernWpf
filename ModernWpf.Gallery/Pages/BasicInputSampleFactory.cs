using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Gallery.Models;
using ModernWpf.Controls.Primitives;
using Mux = ModernWpf.Controls;

namespace ModernWpf.Gallery.Pages
{
    internal static class BasicInputSampleFactory
    {
        private const double SwatchSize = 32;

        private const string ToggleSwitchSimpleXaml =
@"<ToggleSwitch AutomationProperties.Name=""simple ToggleSwitch""/>";

        private const string HyperlinkButtonNavigateUriXaml =
@"<HyperlinkButton Content=""Microsoft home page"" NavigateUri=""https://www.microsoft.com"" $(IsEnabled)/>";

        private const string HyperlinkButtonClickXaml =
@"<HyperlinkButton Content=""ToggleButton"" Click=""HyperlinkButton_Click""/>";

        private const string RepeatButtonSimpleXaml =
@"<RepeatButton Content=""Click and hold"" Click=""RepeatButton_Click"" $(IsEnabled)/>";

        private const string RatingControlSimpleXaml =
@"<RatingControl AutomationProperties.Name=""Simple RatingControl"" IsClearEnabled=""$(IsClearEnabled)""
    IsReadOnly=""$(IsReadOnly)"" Caption=""$(Caption)""/>";

        private const string RatingControlPlaceholderXaml =
@"<RatingControl AutomationProperties.Name=""RatingControl with placeholder"" PlaceholderValue=""$(Slider)"" />";

        private const string ColorPickerPropertiesXaml =
@"<ColorPicker
      ColorSpectrumShape=""$(ColorSpectrumShape)""
      IsMoreButtonVisible=""$(IsMoreButtonVisible)""
      IsColorSliderVisible=""$(IsColorSliderVisible)""
      IsColorChannelTextInputVisible=""$(IsColorChannelTextInputVisible)""
      IsHexInputVisible=""$(IsHexInputVisible)""
      IsAlphaEnabled=""$(IsAlphaEnabled)""
      IsAlphaSliderVisible=""$(IsAlphaSliderVisible)""
      IsAlphaTextInputVisible=""$(IsAlphaTextInputVisible)"" />";

        private const string ToggleButtonSimpleXaml =
@"<ToggleButton Content=""ToggleButton"" Click=""Button_Click"" $(IsEnabled)/>";

        private const string ToggleSwitchWithProgressXaml =
@"<StackPanel Orientation=""Horizontal"">
    <ToggleSwitch Header=""Toggle work"" OffContent=""Do work"" OnContent=""Working"" IsOn=""$(isOn)$(isOff)"" />
    <ProgressRing IsActive=""{x:Bind ToggleSwitch2.IsOn, Mode=OneWay}"" Width=""32""/>
</StackPanel>";

        public static IReadOnlyList<GalleryExample> CreateExamples(string uniqueId, IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            switch (uniqueId)
            {
                case "HyperlinkButton":
                    return CreateHyperlinkButtonExamples();
                case "ColorPicker":
                    return CreateColorPickerExamples();
                case "RatingControl":
                    return CreateRatingControlExamples();
                case "RepeatButton":
                    return CreateRepeatButtonExamples();
                case "ToggleButton":
                    return CreateToggleButtonExamples();
                case "DropDownButton":
                    return CreateDropDownButtonExamples(sampleSnippets);
                case "SplitButton":
                    return CreateSplitButtonExamples(sampleSnippets);
                case "ToggleSplitButton":
                    return CreateToggleSplitButtonExamples(sampleSnippets);
                case "ToggleSwitch":
                    return CreateToggleSwitchExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static UIElement Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "DropDownButton":
                    return CreateDropDownButtonSample();
                case "HyperlinkButton":
                    return CreateHyperlinkButtonSample();
                case "RepeatButton":
                    return CreateRepeatButtonSample();
                case "ToggleButton":
                    return CreateToggleButtonSample();
                case "SplitButton":
                    return CreateSplitButtonSample();
                case "ToggleSplitButton":
                    return CreateToggleSplitButtonSample();
                case "ColorPicker":
                    return CreateColorPickerSample();
                case "RatingControl":
                    return CreateRatingControlSample();
                case "ToggleSwitch":
                    return CreateToggleSwitchSample();
                default:
                    return null;
            }
        }

        private static UIElement CreateDropDownButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("DropDownButton"));
            panel.Children.Add(CreateSimpleDropDownButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateDropDownButtonExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Simple DropDownButton",
                    CreateSimpleDropDownButtonExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "Buttons\\DropDown\\DropDownButton_Simple.txt"),
                    null),
                new GalleryExample(
                    "DropDownButton with Icons",
                    CreateIconDropDownButtonExampleContent(),
                    FindSampleCodeText(sampleSnippets, "Buttons\\DropDown\\DropDownButton_Icon.txt"),
                    null)
            };
        }

        private static GallerySamplePanel CreateSimpleDropDownButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = CreateDropDownButtonExampleRoot(assignRootAutomationId);
            var button = new Mux.DropDownButton
            {
                Content = "Email",
                Flyout = CreateEmailMenuFlyout(includeIcons: false),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("DropDownButton", "DropDownButton"));
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateIconDropDownButtonExampleContent()
        {
            var panel = CreateDropDownButtonExampleRoot(assignRootAutomationId: false);
            var button = new Mux.DropDownButton
            {
                Content = new Mux.FontIcon { Glyph = "\uE715" },
                Flyout = CreateEmailMenuFlyout(includeIcons: true),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(button, "Email");
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("DropDownButton", "IconDropDownButton"));
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateDropDownButtonExampleRoot(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("DropDownButton"));
            }

            return panel;
        }

        private static Mux.MenuFlyout CreateEmailMenuFlyout(bool includeIcons)
        {
            var flyout = new Mux.MenuFlyout
            {
                Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
            };
            flyout.Items.Add(CreateEmailMenuItem("Send", includeIcons ? "\uE725" : null));
            flyout.Items.Add(CreateEmailMenuItem("Reply", includeIcons ? "\uE8CA" : null));
            flyout.Items.Add(CreateEmailMenuItem("Reply All", includeIcons ? "\uE8C2" : null));
            return flyout;
        }

        private static MenuItem CreateEmailMenuItem(string text, string iconGlyph)
        {
            var item = new MenuItem
            {
                Header = text
            };
            if (iconGlyph != null)
            {
                item.Icon = new Mux.FontIcon { Glyph = iconGlyph };
            }

            return item;
        }

        private static UIElement CreateHyperlinkButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("HyperlinkButton"));
            panel.Children.Add(CreateUriHyperlinkButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateHyperlinkButtonExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A hyperlink button that navigates to a URI.",
                    CreateUriHyperlinkButtonExampleContent(assignRootAutomationId: true),
                    HyperlinkButtonNavigateUriXaml,
                    null),
                new GalleryExample(
                    "A hyperlink button that handles a Click event.",
                    CreateClickHyperlinkButtonExampleContent(),
                    HyperlinkButtonClickXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateUriHyperlinkButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("HyperlinkButton"));
            }

            var button = new Mux.HyperlinkButton
            {
                Name = "Control1",
                Content = "Microsoft home page",
                NavigateUri = new Uri("https://www.microsoft.com")
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("HyperlinkButton", "HyperlinkButton"));
            panel.Children.Add(button);
            return panel;
        }

        private static GallerySamplePanel CreateClickHyperlinkButtonExampleContent()
        {
            var panel = new GallerySamplePanel();
            var button = new Mux.HyperlinkButton
            {
                Name = "Control2",
                Content = "Go to ToggleButton"
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("HyperlinkButton", "ClickHyperlinkButton"));
            button.Click += delegate
            {
                RequestItemNavigation(button, "ToggleButton");
            };

            panel.Children.Add(button);
            return panel;
        }

        private static UIElement CreateRepeatButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RepeatButton"));
            panel.Children.Add(CreateSimpleRepeatButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateRepeatButtonExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple RepeatButton with text content.",
                    CreateSimpleRepeatButtonExampleContent(assignRootAutomationId: true),
                    RepeatButtonSimpleXaml,
                    null)
            };
        }

        private static StackPanel CreateSimpleRepeatButtonExampleContent(bool assignRootAutomationId)
        {
            var root = new GallerySamplePanel
            {
                Orientation = Orientation.Horizontal
            };
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(root, GalleryAutomation.SampleRootId("RepeatButton"));
            }

            var button = new RepeatButton
            {
                Name = "Control1",
                Content = "Click and hold"
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("RepeatButton", "RepeatButton"));

            var output = new TextBlock
            {
                Name = "Control1Output",
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            AutomationProperties.SetName(output, "Control output");
#if !NET462
            AutomationProperties.SetLiveSetting(output, AutomationLiveSetting.Polite);
#endif

            var clicks = 0;
            button.Click += delegate
            {
                clicks += 1;
                output.Text = "Number of clicks: " + clicks;

#if !NET462
                var peer = FrameworkElementAutomationPeer.FromElement(output) ??
                    FrameworkElementAutomationPeer.CreatePeerForElement(output);
                peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
#endif
            };

            root.Children.Add(button);
            root.Children.Add(output);
            return root;
        }

        private static UIElement CreateToggleButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ToggleButton"));
            panel.Children.Add(CreateSimpleToggleButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateToggleButtonExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple ToggleButton with text content.",
                    CreateSimpleToggleButtonExampleContent(assignRootAutomationId: true),
                    ToggleButtonSimpleXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateSimpleToggleButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ToggleButton"));
            }

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top
            };

            var button = new ToggleButton
            {
                Name = "Toggle1",
                Content = "ToggleButton"
            };
            GalleryAutomation.WithAutomationId(button, GalleryAutomation.SampleElementId("ToggleButton", "ToggleButton"));

            var output = new TextBlock
            {
                Name = "Control1Output",
                Margin = new Thickness(0, 12, 0, 0),
                Text = "Off"
            };

            button.Checked += delegate { output.Text = "On"; };
            button.Unchecked += delegate { output.Text = "Off"; };

            buttonRow.Children.Add(button);
            panel.Children.Add(buttonRow);
            panel.Children.Add(output);
            return panel;
        }

        private static UIElement CreateSplitButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("SplitButton"));
            panel.Children.Add(CreateColorSplitButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateSplitButtonExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "A SplitButton controlling text color in a RichEditBox",
                    CreateColorSplitButtonExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "Buttons\\SplitButton\\SplitButtonSample1.txt"),
                    null),
                new GalleryExample(
                    "A SplitButton with text",
                    CreateTextSplitButtonExampleContent(),
                    FindSampleCodeText(sampleSnippets, "Buttons\\SplitButton\\SplitButtonSample2.txt"),
                    null)
            };
        }

        private static GallerySamplePanel CreateColorSplitButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("SplitButton"));
            }

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var richTextBox = CreateSplitButtonRichTextBox();
            var currentColor = new Border
            {
                Name = "CurrentColor",
                Width = SwatchSize,
                Height = SwatchSize,
                Margin = new Thickness(0),
                Background = Brushes.Green,
                CornerRadius = new CornerRadius(4, 0, 0, 4)
            };

            var splitButton = new Mux.SplitButton
            {
                Name = "myColorButton",
                Content = currentColor,
                MinWidth = 0,
                MinHeight = 0,
                Padding = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Top
            };
            AutomationProperties.SetName(splitButton, "Font color");
            GalleryAutomation.WithAutomationId(splitButton, GalleryAutomation.SampleElementId("SplitButton", "SplitButton"));
            splitButton.Click += delegate
            {
                ApplyRichTextBoxForeground(richTextBox, ((SolidColorBrush)currentColor.Background).Color);
            };
            splitButton.Flyout = CreateColorSwatchFlyout(delegate(string colorName, SolidColorBrush brush)
            {
                currentColor.Background = brush;
                ApplyRichTextBoxForeground(richTextBox, brush.Color);
                splitButton.Flyout.Hide();
            }, includeBlack: false);

            layout.Children.Add(splitButton);
            Grid.SetColumn(richTextBox, 2);
            layout.Children.Add(richTextBox);
            panel.Children.Add(layout);
            return panel;
        }

        private static GallerySamplePanel CreateTextSplitButtonExampleContent()
        {
            var panel = new GallerySamplePanel();
            var splitButton = new Mux.SplitButton
            {
                Name = "myColorButtonReveal",
                Content = "Choose color",
                MinWidth = 0,
                MinHeight = 0,
                Padding = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(splitButton, "Font color with text");
            GalleryAutomation.WithAutomationId(splitButton, GalleryAutomation.SampleElementId("SplitButton", "TextSplitButton"));
            splitButton.Flyout = CreateColorSwatchFlyout(delegate
            {
                splitButton.Flyout.Hide();
            }, includeBlack: true);
            panel.Children.Add(splitButton);
            return panel;
        }

        private static RichTextBox CreateSplitButtonRichTextBox()
        {
            var richTextBox = new RichTextBox
            {
                Name = "myRichEditBox",
                Width = 240,
                MinHeight = 96,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, " +
                "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tempor commodo ullamcorper a lacus.")));
            ApplyRichTextBoxForeground(richTextBox, Colors.Green);
            return richTextBox;
        }

        private static Mux.Flyout CreateColorSwatchFlyout(Action<string, SolidColorBrush> colorSelected, bool includeBlack)
        {
            var grid = new Mux.VariableSizedWrapGrid
            {
                MaximumRowsOrColumns = 3,
                Orientation = Orientation.Horizontal
            };

            var colors = includeBlack
                ? new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray", "Black" }
                : new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet", "Gray" };
            foreach (var colorName in colors)
            {
                grid.Children.Add(CreateColorSwatchButton(colorName, colorSelected));
            }

            return new Mux.Flyout
            {
                Placement = FlyoutPlacementMode.Bottom,
                Content = grid
            };
        }

        private static Button CreateColorSwatchButton(string colorName, Action<string, SolidColorBrush> colorSelected)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
            var rectangle = new Rectangle
            {
                Width = SwatchSize,
                Height = SwatchSize,
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

        private static void ApplyRichTextBoxForeground(RichTextBox richTextBox, Color color)
        {
            richTextBox.SelectAll();
            richTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }

        private static UIElement CreateToggleSplitButtonSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ToggleSplitButton"));
            panel.Children.Add(CreateToggleSplitButtonExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateToggleSplitButtonExamples(IReadOnlyList<SampleSnippet> sampleSnippets)
        {
            return new[]
            {
                new GalleryExample(
                    "Using ToggleSplitButton to control bulleted list functionality in RichEditBox",
                    CreateToggleSplitButtonExampleContent(assignRootAutomationId: true),
                    FindSampleCodeText(sampleSnippets, "Buttons\\ToggleSplitButton\\ToggleSplitButtonSample1.txt"),
                    null)
            };
        }

        private static GallerySamplePanel CreateToggleSplitButtonExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ToggleSplitButton"));
            }

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var richTextBox = new RichTextBox
            {
                Name = "myRichEditBox",
                Width = 240,
                MinHeight = 96,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            AutomationProperties.SetName(richTextBox, "Text entry");

            var symbolIcon = new Mux.SymbolIcon(Mux.Symbol.List)
            {
                Name = "mySymbolIcon"
            };
            var toggleSplitButton = new Mux.ToggleSplitButton
            {
                Name = "myListButton",
                Content = symbolIcon,
                VerticalAlignment = VerticalAlignment.Top
            };
            AutomationProperties.SetName(toggleSplitButton, "Bullets");
            GalleryAutomation.WithAutomationId(toggleSplitButton, GalleryAutomation.SampleElementId("ToggleSplitButton", "ToggleSplitButton"));

            var currentMarkerStyle = TextMarkerStyle.Disc;
            toggleSplitButton.IsCheckedChanged += delegate
            {
                ApplyRichTextBoxListStyle(richTextBox, toggleSplitButton.IsChecked, currentMarkerStyle);
            };
            toggleSplitButton.Flyout = CreateToggleSplitButtonFlyout(delegate(Mux.Symbol symbol, string automationName, TextMarkerStyle markerStyle)
            {
                currentMarkerStyle = markerStyle;
                symbolIcon.Symbol = symbol;
                AutomationProperties.SetName(toggleSplitButton, automationName);
                var wasChecked = toggleSplitButton.IsChecked;
                toggleSplitButton.IsChecked = true;
                if (wasChecked)
                {
                    ApplyRichTextBoxListStyle(richTextBox, isChecked: true, currentMarkerStyle);
                }
                toggleSplitButton.Flyout.Hide();
                richTextBox.Focus();
            });

            layout.Children.Add(toggleSplitButton);
            Grid.SetColumn(richTextBox, 2);
            layout.Children.Add(richTextBox);
            panel.Children.Add(layout);
            return panel;
        }

        private static Mux.Flyout CreateToggleSplitButtonFlyout(Action<Mux.Symbol, string, TextMarkerStyle> markerSelected)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            stackPanel.Children.Add(CreateListMarkerButton("Bulleted list", Mux.Symbol.List, TextMarkerStyle.Disc, markerSelected));
            stackPanel.Children.Add(CreateListMarkerButton("Roman numerals list", Mux.Symbol.Bullets, TextMarkerStyle.UpperRoman, markerSelected));

            return new Mux.Flyout
            {
                Placement = FlyoutPlacementMode.Bottom,
                Content = stackPanel
            };
        }

        private static Button CreateListMarkerButton(
            string automationName,
            Mux.Symbol symbol,
            TextMarkerStyle markerStyle,
            Action<Mux.Symbol, string, TextMarkerStyle> markerSelected)
        {
            var button = new Button
            {
                Content = new Mux.SymbolIcon(symbol),
                Padding = new Thickness(4),
                MinWidth = 0,
                MinHeight = 0,
                Margin = new Thickness(6)
            };
            AutomationProperties.SetName(button, automationName);
            button.Click += delegate { markerSelected(symbol, symbol == Mux.Symbol.List ? "Bullets" : "Roman Numerals", markerStyle); };
            return button;
        }

        private static void ApplyRichTextBoxListStyle(RichTextBox richTextBox, bool isChecked, TextMarkerStyle markerStyle)
        {
            if (!isChecked)
            {
                return;
            }

            richTextBox.Focus();
            var command = markerStyle == TextMarkerStyle.Disc
                ? EditingCommands.ToggleBullets
                : EditingCommands.ToggleNumbering;
            if (command.CanExecute(null, richTextBox))
            {
                command.Execute(null, richTextBox);
            }
        }

        private static UIElement CreateColorPickerSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ColorPicker"));
            panel.Children.Add(CreateColorPickerPropertiesExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateColorPickerExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "ColorPicker Properties.",
                    CreateColorPickerPropertiesExampleContent(assignRootAutomationId: true),
                    ColorPickerPropertiesXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateColorPickerPropertiesExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ColorPicker"));
            }

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var colorPicker = new Mux.ColorPicker
            {
                Name = "colorPicker",
                IsAlphaEnabled = false,
                IsAlphaSliderVisible = true,
                IsAlphaTextInputVisible = true,
                IsColorChannelTextInputVisible = true,
                IsColorSliderVisible = true,
                IsHexInputVisible = true,
                IsMoreButtonVisible = false
            };
            GalleryAutomation.WithAutomationId(colorPicker, GalleryAutomation.SampleElementId("ColorPicker", "ColorPicker"));
            layout.Children.Add(colorPicker);

            var options = new StackPanel
            {
                Width = 250,
                Margin = new Thickness(0, -5, 0, 0)
            };
            Grid.SetColumn(options, 2);

            var moreButtonCheck = CreateOptionCheckBox("moreBtn", "IsMoreButtonVisible", isChecked: false, isEnabled: true, value => colorPicker.IsMoreButtonVisible = value);
            var colorSliderCheck = CreateOptionCheckBox("colorSlider", "IsColorSliderVisible", isChecked: true, isEnabled: true, value => colorPicker.IsColorSliderVisible = value);
            var colorChannelInputCheck = CreateOptionCheckBox("colorChannelInput", "IsColorChannelTextInputVisible", isChecked: true, isEnabled: true, value => colorPicker.IsColorChannelTextInputVisible = value);
            var hexInputCheck = CreateOptionCheckBox("hexInput", "IsHexInputVisible", isChecked: true, isEnabled: true, value => colorPicker.IsHexInputVisible = value);
            var alphaSliderCheck = CreateOptionCheckBox("alphaSlider", "IsAlphaSliderVisible", isChecked: true, isEnabled: false, value => colorPicker.IsAlphaSliderVisible = value);
            var alphaTextInputCheck = CreateOptionCheckBox("alphaTextInput", "IsAlphaTextInputVisible", isChecked: true, isEnabled: false, value => colorPicker.IsAlphaTextInputVisible = value);
            var alphaCheck = CreateOptionCheckBox("alpha", "Alpha Enabled", isChecked: false, isEnabled: true, value =>
            {
                colorPicker.IsAlphaEnabled = value;
                alphaSliderCheck.IsEnabled = value;
                alphaTextInputCheck.IsEnabled = value;
            });

            options.Children.Add(moreButtonCheck);
            options.Children.Add(colorSliderCheck);
            options.Children.Add(colorChannelInputCheck);
            options.Children.Add(hexInputCheck);
            options.Children.Add(alphaCheck);
            options.Children.Add(alphaSliderCheck);
            options.Children.Add(alphaTextInputCheck);

            var shapeRadioButtons = new Mux.RadioButtons
            {
                Name = "ColorSpectrumShapeRadioButtons",
                Header = "Colorspectrum shape",
                SelectedIndex = 0
            };
            shapeRadioButtons.Items.Add("Box");
            shapeRadioButtons.Items.Add("Ring");
            shapeRadioButtons.SelectionChanged += delegate
            {
                colorPicker.ColorSpectrumShape = shapeRadioButtons.SelectedIndex == 1
                    ? Mux.ColorSpectrumShape.Ring
                    : Mux.ColorSpectrumShape.Box;
            };
            options.Children.Add(shapeRadioButtons);

            var previewStack = new StackPanel
            {
                Margin = new Thickness(0, 12, 0, 0)
            };
            previewStack.Children.Add(new TextBlock
            {
                Text = "ColorPicker applied on a Rectangle"
            });
            var previewFill = new SolidColorBrush(colorPicker.Color);
            var previewRect = new Rectangle
            {
                Name = "previewRect",
                Height = 100,
                Margin = new Thickness(0, 12, 0, 0),
                StrokeThickness = 1,
                Fill = previewFill
            };
            previewRect.SetResourceReference(Shape.StrokeProperty, "TextControlBorderBrush");
            colorPicker.ColorChanged += delegate
            {
                previewFill.Color = colorPicker.Color;
            };
            previewStack.Children.Add(previewRect);
            options.Children.Add(previewStack);

            layout.Children.Add(options);
            panel.Children.Add(layout);
            return panel;
        }

        private static UIElement CreateRatingControlSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RatingControl"));
            panel.Children.Add(CreateSimpleRatingControlExampleContent(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateRatingControlExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple RatingControl",
                    CreateSimpleRatingControlExampleContent(assignRootAutomationId: true),
                    RatingControlSimpleXaml,
                    null),
                new GalleryExample(
                    "PlaceholderValue of RatingControl",
                    CreatePlaceholderRatingControlExampleContent(),
                    RatingControlPlaceholderXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateSimpleRatingControlExampleContent(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("RatingControl"));
            }

            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var ratingStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top
            };
            var rating = new Mux.RatingControl
            {
                Name = "RatingControl1",
                HorizontalAlignment = HorizontalAlignment.Left,
                Caption = "312 ratings",
                IsClearEnabled = false,
                IsReadOnly = false
            };
            AutomationProperties.SetName(rating, "Simple RatingControl");
            GalleryAutomation.WithAutomationId(rating, GalleryAutomation.SampleElementId("RatingControl", "RatingControl"));

            var output = new TextBlock
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 12, 0, 0),
                Text = FormatRatingValue(rating.Value)
            };

            rating.ValueChanged += delegate
            {
                UpdateSimpleRatingOutput(rating, output);
            };
            var ratingValueDescriptor = DependencyPropertyDescriptor.FromProperty(
                Mux.RatingControl.ValueProperty,
                typeof(Mux.RatingControl));
            ratingValueDescriptor?.AddValueChanged(rating, delegate
            {
                UpdateSimpleRatingOutput(rating, output);
            });

            ratingStack.Children.Add(rating);
            layout.Children.Add(ratingStack);
            Grid.SetRow(output, 1);
            layout.Children.Add(output);

            var options = new StackPanel
            {
                Width = 220
            };
            Grid.SetColumn(options, 2);
            Grid.SetRowSpan(options, 2);
            var clearEnabledCheck = new CheckBox
            {
                Name = "clearEnabledCheck",
                Content = "IsClearEnabled"
            };
            clearEnabledCheck.Checked += delegate { rating.IsClearEnabled = true; };
            clearEnabledCheck.Unchecked += delegate { rating.IsClearEnabled = false; };
            options.Children.Add(clearEnabledCheck);
            options.Children.Add(new TextBlock
            {
                Text = "Swipe left or click again to clear your rating.",
                TextWrapping = TextWrapping.Wrap
            });
            var readOnlyCheck = new CheckBox
            {
                Name = "readOnlyCheck",
                Content = "IsReadOnly",
                Margin = new Thickness(0, 12, 0, 0)
            };
            readOnlyCheck.Checked += delegate { rating.IsReadOnly = true; };
            readOnlyCheck.Unchecked += delegate { rating.IsReadOnly = false; };
            options.Children.Add(readOnlyCheck);
            layout.Children.Add(options);

            panel.Children.Add(layout);
            return panel;
        }

        private static GallerySamplePanel CreatePlaceholderRatingControlExampleContent()
        {
            var panel = new GallerySamplePanel();
            var layout = new Grid();
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var rating = new Mux.RatingControl
            {
                Name = "RatingControl2",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            AutomationProperties.SetName(rating, "RatingControl with placeholder");
            GalleryAutomation.WithAutomationId(rating, GalleryAutomation.SampleElementId("RatingControl", "PlaceholderRatingControl"));
            layout.Children.Add(rating);

            var options = new StackPanel
            {
                Width = 220
            };
            Grid.SetColumn(options, 2);
            options.Children.Add(new TextBlock
            {
                Text = "PlaceholderValue",
                Margin = new Thickness(0, 0, 0, 4),
                FontWeight = FontWeights.SemiBold
            });
            var slider = new Slider
            {
                Name = "slider",
                Minimum = 0,
                Maximum = 5,
                SmallChange = 0.5,
                TickFrequency = 0.5
            };
            slider.ValueChanged += delegate
            {
                rating.PlaceholderValue = slider.Value <= 0 ? -1 : slider.Value;
            };
            options.Children.Add(slider);
            layout.Children.Add(options);

            panel.Children.Add(layout);
            return panel;
        }

        private static UIElement CreateToggleSwitchSample()
        {
            var panel = new GallerySamplePanel
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ToggleSwitch"));
            panel.Children.Add(CreateSimpleToggleSwitch(assignRootAutomationId: false));
            return panel;
        }

        private static IReadOnlyList<GalleryExample> CreateToggleSwitchExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple ToggleSwitch.",
                    CreateSimpleToggleSwitch(assignRootAutomationId: true),
                    ToggleSwitchSimpleXaml,
                    null),
                new GalleryExample(
                    "A ToggleSwitch with custom header and content.",
                    CreateToggleSwitchWithProgressRing(),
                    ToggleSwitchWithProgressXaml,
                    null)
            };
        }

        private static GallerySamplePanel CreateSimpleToggleSwitch(bool assignRootAutomationId)
        {
            var panel = new GallerySamplePanel();
            if (assignRootAutomationId)
            {
                GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId("ToggleSwitch"));
            }

            var toggleSwitch = new Mux.ToggleSwitch
            {
                Width = 72,
                MinWidth = 0,
                OffContent = string.Empty,
                OnContent = string.Empty
            };
            AutomationProperties.SetName(toggleSwitch, "simple ToggleSwitch");
            GalleryAutomation.WithAutomationId(toggleSwitch, GalleryAutomation.SampleElementId("ToggleSwitch", "ToggleSwitch"));
            panel.Children.Add(toggleSwitch);
            return panel;
        }

        private static StackPanel CreateToggleSwitchWithProgressRing()
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var toggleSwitch = new Mux.ToggleSwitch
            {
                Name = "ToggleSwitch2",
                Header = "Toggle work",
                IsOn = true,
                OffContent = "Do work",
                OnContent = "Working"
            };
            GalleryAutomation.WithAutomationId(toggleSwitch, GalleryAutomation.SampleElementId("ToggleSwitch", "WorkToggleSwitch"));

            var progressRing = new Mux.ProgressRing
            {
                Name = "ToggleSwitchProgressRing",
                Width = 32,
                IsActive = toggleSwitch.IsOn
            };
            toggleSwitch.Toggled += delegate
            {
                progressRing.IsActive = toggleSwitch.IsOn;
            };

            panel.Children.Add(toggleSwitch);
            panel.Children.Add(progressRing);
            return panel;
        }

        private static Mux.MenuFlyout CreateCommandFlyout(TextBlock output, params string[] labels)
        {
            var flyout = new Mux.MenuFlyout();
            foreach (var label in labels)
            {
                var item = new MenuItem { Header = label };
                item.Click += delegate { output.Text = "Selected: " + label; };
                flyout.Items.Add(item);
            }

            return flyout;
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

            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "SampleCode", relativePath);
            return System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        }

        private static string FormatRatingValue(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static CheckBox CreateOptionCheckBox(
            string name,
            string content,
            bool isChecked,
            bool isEnabled,
            Action<bool> valueChanged)
        {
            var checkBox = new CheckBox
            {
                Name = name,
                Content = content,
                IsChecked = isChecked,
                IsEnabled = isEnabled
            };
            checkBox.Checked += delegate { valueChanged(true); };
            checkBox.Unchecked += delegate { valueChanged(false); };
            return checkBox;
        }

        private static void UpdateSimpleRatingOutput(Mux.RatingControl rating, TextBlock output)
        {
            rating.Caption = "Your rating";
            output.Text = FormatRatingValue(rating.Value);
        }

        private static void RequestItemNavigation(DependencyObject source, string uniqueId)
        {
            var page = FindVisualAncestor<ItemPage>(source);
            var item = GalleryCatalog.FindItem(uniqueId);
            if (page != null && item != null)
            {
                page.ItemRequested?.Invoke(item);
            }
        }

        private static T FindVisualAncestor<T>(DependencyObject source)
            where T : DependencyObject
        {
            var current = source;
            while (current != null)
            {
                var match = current as T;
                if (match != null)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

    }
}
