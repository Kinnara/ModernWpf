using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.Win32;

namespace ModernWpf.Gallery.Pages
{
    internal static class WpfGalleryExampleFactory
    {
        public static IReadOnlyList<GalleryExample> Create(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Border":
                    return CreateBorderExamples();
                case "Button":
                    return CreateButtonExamples();
                case "Calendar":
                case "CalendarView":
                    return CreateCalendarExamples();
                case "Canvas":
                    return CreateCanvasExamples();
                case "CheckBox":
                    return CreateCheckBoxExamples();
                case "ComboBox":
                    return CreateComboBoxExamples();
                case "Clipboard":
                    return CreateClipboardExamples();
                case "Color":
                    return CreateColorExamples();
                case "DataGrid":
                    return CreateDataGridExamples();
                case "DatePicker":
                    return CreateDatePickerExamples();
                case "Expander":
                    return CreateExpanderExamples();
                case "FileAndFolderDialogs":
                    return CreateFileAndFolderDialogsExamples();
                case "Frame":
                    return CreateFrameExamples();
                case "Grid":
                    return CreateGridExamples();
                case "GridSplitter":
                    return CreateGridSplitterExamples();
                case "GroupBox":
                    return CreateGroupBoxExamples();
                case "Hyperlink":
                    return CreateHyperlinkExamples();
                case "Geometry":
                    return CreateGeometryExamples();
                case "Iconography":
                    return CreateIconographyExamples();
                case "Image":
                    return CreateImageExamples();
                case "Label":
                    return CreateLabelExamples();
                case "ListBox":
                    return CreateListBoxExamples();
                case "ListView":
                    return CreateListViewExamples();
                case "Menu":
                    return CreateMenuExamples();
                case "MessageBox":
                    return CreateMessageBoxExamples();
                case "NavigationWindow":
                    return CreateNavigationWindowExamples();
                case "PasswordBox":
                    return CreatePasswordBoxExamples();
                case "ProgressBar":
                    return CreateProgressBarExamples();
                case "RadioButton":
                    return CreateRadioButtonExamples();
                case "ResizeGrip":
                    return CreateResizeGripExamples();
                case "RichEditBox":
                case "RichTextEdit":
                    return CreateRichEditBoxExamples();
                case "Slider":
                    return CreateSliderExamples();
                case "Spacing":
                    return CreateSpacingExamples();
                case "StackPanel":
                    return CreateStackPanelExamples();
                case "TabControl":
                    return CreateTabControlExamples();
                case "TextBox":
                    return CreateTextBoxExamples();
                case "TextBlock":
                    return CreateTextBlockExamples();
                case "ToolTip":
                    return CreateToolTipExamples();
                case "TreeView":
                    return CreateTreeViewExamples();
                case "Typography":
                    return CreateTypographyExamples();
                default:
                    return Array.Empty<GalleryExample>();
            }
        }

        public static object CreatePageContent(string uniqueId)
        {
            switch (uniqueId)
            {
                case "UserDashboard":
                    return CreateUserDashboardPageContent();
                default:
                    return null;
            }
        }

        public static object CreateNotice(string uniqueId)
        {
            switch (uniqueId)
            {
                case "Clipboard":
                    return CreateClipboardNotice();
                case "Color":
                    return CreateDesignNotice(
                        "Color provides an intuitive way of communicating information to users in your app: it can be used to indicate interactivity, give feedback to user actions, and give your interface a sense of visual continuity.",
                        "The colors below are provided as part of WPF .NET 9 onwards. You can reference them in your app using DynamicResource bindings. For example: Color=\"{DynamicResource CardBackgroundFillColorDefault}\"");
                case "Geometry":
                    return CreateDesignNotice(
                        "Geometry describes the shape, size and position of UI elements on screen.",
                        "These fundamental design elements help experiences feel coherent across the entire design system. You can reference built-in corner radii styles using CornerRadius=\"{StaticResource ControlCornerRadius}\".");
                case "Iconography":
                    return CreateDesignNotice(
                        "Segoe Fluent Icons is the system icon font used by Windows.",
                        "Use icon font sizes such as 16, 20, 24, 32, 40, 48, and 64 to keep glyphs crisp and aligned.");
                case "Spacing":
                    return CreateDesignNotice(
                        "Consistent spacing helps create visual harmony and improves the readability and usability of your application.",
                        "Use the following spacing values to maintain a consistent layout throughout your app.");
                case "Typography":
                    return CreateDesignNotice(
                        "Type helps provide structure and hierarchy to UI. The default font for Windows is Segoe UI Variable.",
                        "Best practice is to use Regular weight for most text and Semibold for titles. The minimum values should be 12px Regular, 14px Semibold.");
                default:
                    return null;
            }
        }

        private static IReadOnlyList<GalleryExample> CreateBorderExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A basic Border",
                    new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(2),
                        Padding = new Thickness(10),
                        Child = new TextBlock { Text = "Content inside a Border" }
                    },
                    "<Border BorderBrush=\"Gray\" BorderThickness=\"2\" Padding=\"10\">\n    <TextBlock Text=\"Content inside a Border\" />\n</Border>",
                    null),
                new GalleryExample(
                    "A Border with rounded corners",
                    new Border
                    {
                        Background = Brushes.LightBlue,
                        BorderBrush = Brushes.CornflowerBlue,
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(15),
                        Child = new TextBlock { Text = "Rounded Border", Foreground = Brushes.Black }
                    },
                    "<Border BorderBrush=\"CornflowerBlue\" BorderThickness=\"2\" CornerRadius=\"10\" Padding=\"15\" Background=\"LightBlue\">\n    <TextBlock Text=\"Rounded Border\" />\n</Border>",
                    null),
                new GalleryExample(
                    "A Border with different thickness on each side",
                    new Border
                    {
                        BorderBrush = Brushes.DarkSlateGray,
                        BorderThickness = new Thickness(1, 2, 4, 8),
                        Padding = new Thickness(10),
                        Child = new TextBlock { Text = "Different border thickness (Left=1, Top=2, Right=4, Bottom=8)" }
                    },
                    "<Border BorderBrush=\"DarkSlateGray\" BorderThickness=\"1,2,4,8\" Padding=\"10\">\n    <TextBlock Text=\"Different border thickness\" />\n</Border>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateButtonExamples()
        {
            var simpleButton = new Button
            {
                Content = "Standard WPF button"
            };
            AutomationProperties.SetName(simpleButton, "Standard WPF");
            GalleryAutomation.WithAutomationId(simpleButton, GalleryAutomation.SampleElementId("Button", "PrimaryButton"));
            var disableButton = new CheckBox
            {
                Content = "Disable button",
                VerticalAlignment = VerticalAlignment.Center
            };
            disableButton.Checked += delegate { simpleButton.IsEnabled = false; };
            disableButton.Unchecked += delegate { simpleButton.IsEnabled = true; };

            var simpleGrid = new Grid();
            simpleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            simpleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(disableButton, 1);
            simpleGrid.Children.Add(simpleButton);
            simpleGrid.Children.Add(disableButton);

            var accentButton = new Button
            {
                Style = (Style)Application.Current.TryFindResource("AccentButtonStyle")
            };
            AutomationProperties.SetName(accentButton, "WPF Accent");
            accentButton.Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock { Text = "WPF Accent Button" }
                }
            };

            return new[]
            {
                new GalleryExample(
                    "Simple Button",
                    CreateSampleRoot("Button", simpleGrid),
                    "<Button Content=\"Standard WPF button\" />",
                    null),
                new GalleryExample(
                    "WPF Accent Button",
                    accentButton,
                    "<Button Style=\"{DynamicResource AccentButtonStyle}\" Content=\"WPF Accent Button\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateCalendarExamples()
        {
            var calendar = new Calendar
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                IsTabStop = false
            };
            AutomationProperties.SetName(calendar, "Default");

            return new[]
            {
                new GalleryExample(
                    "A basic Calendar control.",
                    calendar,
                    "<Calendar/>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateCanvasExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A basic Canvas inside the ViewBox",
                    CreateCanvasViewboxExample(),
                    "<Viewbox Width=\"200\" Height=\"200\" >\n    <Canvas Width=\"47\" Height=\"123\">\n        <Path Data=\"M0,19H18V84h29v15H0V19Z\" Fill=\"White\" />\n        <Path Data=\"M46,80H29V15H0V0H46V80Z\" Fill=\"White\" />\n    </Canvas>\n</Viewbox>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateCheckBoxExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A 2-state CheckBox.",
                    CreateCheckBox("Two-state CheckBox", false, false, "Sample Two State"),
                    "<CheckBox Content=\"Two-state CheckBox\" />",
                    null),
                new GalleryExample(
                    "A 3-state CheckBox.",
                    CreateCheckBox("Three-state CheckBox", true, null, "Sample Three State"),
                    "<CheckBox IsThreeState=\"True\" Content=\"Three-state CheckBox\" IsChecked=\"{x:Null}\" />",
                    null),
                new GalleryExample(
                    "Using a 3-state CheckBox.",
                    CreateThreeStateCheckBoxGroup(),
                    "<StackPanel>\n    <CheckBox Content=\"Select all\" IsThreeState=\"True\" />\n    <CheckBox Margin=\"24,0,0,0\" Content=\"Option 1\" />\n    <CheckBox Margin=\"24,0,0,0\" Content=\"Option 2\" />\n    <CheckBox Margin=\"24,0,0,0\" Content=\"Option 3\" />\n</StackPanel>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateClipboardExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Copy text to Clipboard",
                    CreateCopyTextToClipboardExample(),
                    "<Button Content=\"Copy to Clipboard\" Click=\"CopyToClipboard_Click\" />",
                    "private void CopyToClipboard_Click(object sender, RoutedEventArgs e)\n{\n    Clipboard.SetText(textToCopy);\n}"),
                new GalleryExample(
                    "Paste text from Clipboard",
                    CreatePasteTextFromClipboardExample(),
                    "<Button Content=\"Paste from Clipboard\" Click=\"PasteFromClipboard_Click\" />",
                    "private void PasteFromClipboard_Click(object sender, RoutedEventArgs e)\n{\n    if (Clipboard.ContainsText())\n    {\n        string text = Clipboard.GetText();\n    }\n}"),
                new GalleryExample(
                    "Clear Clipboard",
                    CreateClearClipboardExample(),
                    "<Button Content=\"Clear Clipboard\" Click=\"ClearClipboard_Click\" />",
                    "private void ClearClipboard_Click(object sender, RoutedEventArgs e)\n{\n    Clipboard.Clear();\n}"),
                new GalleryExample(
                    "Check Clipboard data formats",
                    CreateCheckClipboardFormatsExample(),
                    "<Button Content=\"Check Formats\" Click=\"CheckFormats_Click\" />",
                    "private void CheckFormats_Click(object sender, RoutedEventArgs e)\n{\n    bool hasText = Clipboard.ContainsText();\n    bool hasImage = Clipboard.ContainsImage();\n    bool hasFileDropList = Clipboard.ContainsFileDropList();\n}"),
                new GalleryExample(
                    "Copy image to Clipboard",
                    CreateCopyImageToClipboardExample(),
                    "<Button Content=\"Copy Image\" Click=\"CopyImageToClipboard_Click\" />",
                    "private void CopyImageToClipboard_Click(object sender, RoutedEventArgs e)\n{\n    Clipboard.SetImage(bitmapSource);\n}"),
                new GalleryExample(
                    "Paste image from Clipboard",
                    CreatePasteImageFromClipboardExample(),
                    "<Button Content=\"Paste Image\" Click=\"PasteImageFromClipboard_Click\" />",
                    "private void PasteImageFromClipboard_Click(object sender, RoutedEventArgs e)\n{\n    if (Clipboard.ContainsImage())\n    {\n        BitmapSource image = Clipboard.GetImage();\n    }\n}")
            };
        }

        private static IReadOnlyList<GalleryExample> CreateComboBoxExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A ComboBox with items defined inline.",
                    CreateSampleRoot("ComboBox", CreateInlineComboBoxExample()),
                    "<ComboBox MinWidth=\"200\" HorizontalAlignment=\"Left\" SelectedIndex=\"0\">\n    <ComboBoxItem Content=\"Blue\" />\n    <ComboBoxItem Content=\"Green\" />\n    <ComboBoxItem Content=\"Red\" />\n    <ComboBoxItem Content=\"Yellow\" />\n</ComboBox>",
                    null),
                new GalleryExample(
                    "A ComboBox with ItemsSource set.",
                    CreateFontFamilyComboBoxExample(),
                    "<ComboBox MinWidth=\"200\" HorizontalAlignment=\"Left\" ItemsSource=\"{Binding FontFamilies}\" SelectedIndex=\"0\" />",
                    null),
                new GalleryExample(
                    "An editable ComboBox.",
                    CreateEditableFontSizeComboBoxExample(),
                    "<ComboBox MinWidth=\"200\" HorizontalAlignment=\"Left\" ItemsSource=\"{Binding FontSizes}\" SelectedIndex=\"0\" IsEditable=\"True\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateColorExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Text color resources",
                    CreateColorResourcesExample(),
                    "<TextBlock Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\" Text=\"Primary text\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateGeometryExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Corner radius",
                    CreateGeometryExample(),
                    "<Border CornerRadius=\"{StaticResource OverlayCornerRadius}\" />\n<Border CornerRadius=\"{StaticResource ControlCornerRadius}\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateIconographyExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Fluent Icons Library",
                    CreateIconographyExample(),
                    "<TextBlock FontFamily=\"{StaticResource SymbolThemeFontFamily}\" Text=\"&#xE8A7;\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateSpacingExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Spacing values",
                    CreateSpacingExample(),
                    "<Border Padding=\"16\" Margin=\"0,0,0,24\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateTypographyExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Type ramp",
                    CreateTypographyRampExample(),
                    "<TextBlock Text=\"Caption\" Style=\"{StaticResource CaptionTextBlockStyle}\" />\n<TextBlock Text=\"Body\" Style=\"{StaticResource BodyTextBlockStyle}\" />\n<TextBlock Text=\"Body Strong\" Style=\"{StaticResource BodyStrongTextBlockStyle}\" />\n<TextBlock Text=\"Subtitle\" Style=\"{StaticResource SubtitleTextBlockStyle}\" />\n<TextBlock Text=\"Title\" Style=\"{StaticResource TitleTextBlockStyle}\" />\n<TextBlock Text=\"Title Large\" Style=\"{StaticResource TitleLargeTextBlockStyle}\" />\n<TextBlock Text=\"Display\" Style=\"{StaticResource DisplayTextBlockStyle}\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateDataGridExamples()
        {
            var dataGrid = new DataGrid
            {
                Height = 400,
                ItemsSource = CreateProducts()
            };
            AutomationProperties.SetName(dataGrid, "Sample Data Grid");

            return new[]
            {
                new GalleryExample(
                    "Default DataGrid with ItemsSource.",
                    dataGrid,
                    "<DataGrid ItemsSource=\"{Binding ViewModel.ProductsCollection, Mode=TwoWay}\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateDatePickerExamples()
        {
            var datePicker = new DatePicker
            {
                MinWidth = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(datePicker, "Pick a date");

            return new[]
            {
                new GalleryExample(
                    "A basic DatePicker control.",
                    datePicker,
                    "<DatePicker />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateExpanderExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "An Expander with text in the header and content areas",
                    CreateExpanderExample(),
                    "<Expander Header=\"This text is in the header\" Content=\"This is in the content\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateGridExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple 3x3 Grid",
                    CreateSimpleGridExample(),
                    "<Grid ShowGridLines=\"True\">\n    <Grid.RowDefinitions>\n        <RowDefinition Height=\"*\" />\n        <RowDefinition Height=\"*\" />\n        <RowDefinition Height=\"*\" />\n    </Grid.RowDefinitions>\n    <Grid.ColumnDefinitions>\n        <ColumnDefinition Width=\"*\" />\n        <ColumnDefinition Width=\"*\" />\n        <ColumnDefinition Width=\"*\" />\n    </Grid.ColumnDefinitions>\n    <TextBlock Grid.Row=\"0\" Grid.Column=\"0\" Text=\"Cell 1\" />\n    <TextBlock Grid.Row=\"0\" Grid.Column=\"1\" Text=\"Cell 2\" />\n    <TextBlock Grid.Row=\"0\" Grid.Column=\"2\" Text=\"Cell 3\" />\n    <TextBlock Grid.Row=\"1\" Grid.Column=\"0\" Text=\"Cell 4\" />\n    <TextBlock Grid.Row=\"1\" Grid.Column=\"1\" Text=\"Cell 5\" />\n    <TextBlock Grid.Row=\"1\" Grid.Column=\"2\" Text=\"Cell 6\" />\n    <TextBlock Grid.Row=\"2\" Grid.Column=\"0\" Text=\"Cell 7\" />\n    <TextBlock Grid.Row=\"2\" Grid.Column=\"1\" Text=\"Cell 8\" />\n    <TextBlock Grid.Row=\"2\" Grid.Column=\"2\" Text=\"Cell 9\" />\n</Grid>",
                    null),
                new GalleryExample(
                    "A Grid with custom sizing and spanning",
                    CreateCustomGridExample(),
                    "<Grid>\n    <Grid.RowDefinitions>\n        <RowDefinition Height=\"Auto\" />\n        <RowDefinition Height=\"*\" />\n        <RowDefinition Height=\"Auto\" />\n    </Grid.RowDefinitions>\n    <Grid.ColumnDefinitions>\n        <ColumnDefinition Width=\"*\" />\n        <ColumnDefinition Width=\"2*\" />\n        <ColumnDefinition Width=\"*\" />\n    </Grid.ColumnDefinitions>\n    <Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Row 0, Column 0\" />\n    </Border>\n    <Border Grid.Row=\"0\" Grid.Column=\"1\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Row 0, Column 1 (2x width)\" />\n    </Border>\n    <Border Grid.Row=\"0\" Grid.Column=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Row 0, Column 2\" />\n    </Border>\n    <Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Row 1, Spans all columns\" />\n    </Border>\n    <Border Grid.Row=\"2\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Row 2, Spans 2 columns\" />\n    </Border>\n    <Border Grid.Row=\"2\" Grid.Column=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Row 2, Column 2\" />\n    </Border>\n</Grid>",
                    null),
                new GalleryExample(
                    "Grid using XAML shorthand syntax",
                    CreateShorthandGridExample(),
                    "<Grid RowDefinitions=\"Auto,*,Auto\" ColumnDefinitions=\"100,2*,*\">\n    <Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Header (100px)\" />\n    </Border>\n    <Border Grid.Row=\"0\" Grid.Column=\"1\" Background=\"{DynamicResource ControlFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Title (2*)\" />\n    </Border>\n    <Border Grid.Row=\"0\" Grid.Column=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Actions (*)\" />\n    </Border>\n    <Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlAltFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Main Content Area (fills available space)\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" />\n    </Border>\n    <Border Grid.Row=\"2\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">\n        <TextBlock Text=\"Footer (Auto height, spans all columns)\" />\n    </Border>\n</Grid>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateGridSplitterExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A GridSplitter",
                    CreateGridSplitterExample(),
                    "<Grid Height=\"400\">\n    <Grid.RowDefinitions>\n        <RowDefinition Height=\"Auto\" />\n        <RowDefinition Height=\"*\" />\n    </Grid.RowDefinitions>\n    <TextBlock Style=\"{DynamicResource TitleTextBlockStyle}\" Text=\"Grid Splitter\" Margin=\"0 0 0 10\" />\n    <Border BorderBrush=\"{DynamicResource ControlElevationBorderBrush}\" BorderThickness=\"2\" Grid.Row=\"1\" Padding=\"10\" CornerRadius=\"4\">\n        <Grid Background=\"{DynamicResource ControlAltFillColorSecondaryBrush}\">\n            <Grid.RowDefinitions>\n                <RowDefinition Height=\"*\" />\n                <RowDefinition Height=\"Auto\" />\n                <RowDefinition Height=\"*\" />\n                <RowDefinition Height=\"Auto\" />\n                <RowDefinition Height=\"*\" />\n                <RowDefinition Height=\"Auto\" />\n                <RowDefinition Height=\"*\" />\n            </Grid.RowDefinitions>\n            <Grid.ColumnDefinitions>\n                <ColumnDefinition Width=\"*\" />\n                <ColumnDefinition Width=\"Auto\" />\n                <ColumnDefinition Width=\"*\" />\n            </Grid.ColumnDefinitions>\n            <TextBlock TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\" />\n            <TextBlock Grid.Row=\"0\" Grid.Column=\"2\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText2}\" />\n            <TextBlock Grid.Row=\"2\" Grid.Column=\"2\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\" />\n            <TextBlock Grid.Row=\"2\" Grid.Column=\"0\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText2}\" />\n            <TextBlock Grid.Row=\"4\" Grid.Column=\"2\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText2}\" />\n            <TextBlock Grid.Row=\"4\" Grid.Column=\"0\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\" />\n            <GridSplitter Grid.RowSpan=\"5\" Grid.Column=\"1\" ResizeDirection=\"Columns\" />\n            <GridSplitter Grid.Row=\"1\" Grid.ColumnSpan=\"3\" ResizeDirection=\"Rows\" />\n            <GridSplitter Grid.Row=\"3\" Grid.ColumnSpan=\"1\" ResizeDirection=\"Rows\" />\n        </Grid>\n    </Border>\n</Grid>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateGroupBoxExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A GroupBox",
                    CreateUserInformationGroupBox(),
                    "<GroupBox Header=\"User Information\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Width=\"400\">\n    <StackPanel>\n        <StackPanel Orientation=\"Horizontal\">\n            <TextBlock Width=\"100\" Text=\"Name:\" />\n            <TextBox Name=\"NameTextBox\" Width=\"280\" Margin=\"10,0,0,20\" />\n        </StackPanel>\n        <StackPanel Orientation=\"Horizontal\">\n            <TextBlock Width=\"100\" Text=\"Gender:\" Margin=\"0,10,0,0\" />\n            <TextBox Name=\"GenderTextBox\" Width=\"280\" Margin=\"10,0,0,20\" />\n        </StackPanel>\n        <Button Content=\"Submit\" HorizontalAlignment=\"Right\" Margin=\"0,10,0,0\" />\n    </StackPanel>\n</GroupBox>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateTextBlockExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple TextBlock.",
                    new TextBlock { Text = "I am a text block." },
                    "<TextBlock Text=\"I am a text block.\" />",
                    null),
                new GalleryExample(
                    "A TextBlock with style applied.",
                    new TextBlock
                    {
                        FontFamily = new FontFamily("Comic Sans MS"),
                        FontStyle = FontStyles.Italic,
                        Text = "I am a styled TextBlock."
                    },
                    "<TextBlock Text=\"I am a styled TextBlock.\" FontFamily=\"Comic Sans MS\" FontStyle=\"Italic\" />",
                    null),
                new GalleryExample(
                    "A TextBlock with inline text elements.",
                    CreateInlineTextBlock(),
                    "<TextBlock FontSize=\"14\">\n    <Run FontFamily=\"Times New Roman\" Foreground=\"DarkGray\">\n        Text in a TextBlock doesn't have to be a simple string.\n    </Run>\n    <LineBreak />\n    <Span>\n        Text can be <Bold>bold</Bold>, <Italic>italic</Italic>, or <Underline>underlined</Underline>.\n    </Span>\n</TextBlock>",
                    null),
                new GalleryExample(
                    "A TextBlock with wrap property.",
                    new TextBlock
                    {
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap,
                        Text = "The TextBlock control provides flexible text support for WPF applications. The element is targeted primarily toward basic UI scenarios that do not require more than one paragraph of text. It supports a number of properties that enable precise control of presentation, such as FontFamily, FontSize, FontWeight, TextEffects, and TextWrapping."
                    },
                    "<TextBlock FontSize=\"14\" TextWrapping=\"Wrap\">\n    The TextBlock control provides flexible text support for WPF applications.\n    The element is targeted primarily toward basic UI scenarios that do not require more than one paragraph of text.\n    It supports a number of properties that enable precise control of presentation, such as FontFamily, FontSize, FontWeight, TextEffects, and TextWrapping.\n</TextBlock>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateFileAndFolderDialogsExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Pick Single File",
                    CreatePickSingleFileExample(),
                    "<Button Content=\"Pick Single File\" Click=\"PickSingleFileButton_Click\" />",
                    "private void PickSingleFileButton_Click(object sender, RoutedEventArgs e)\n{\n    var openFileDialog = new OpenFileDialog\n    {\n        Title = \"Select a file\",\n        Filter = \"All files (*.*)|*.*|Text files (*.txt)|*.txt\",\n        Multiselect = false\n    };\n\n    if (openFileDialog.ShowDialog() == true)\n    {\n        string filePath = openFileDialog.FileName;\n    }\n}"),
                new GalleryExample(
                    "Pick Multiple Files",
                    CreatePickMultipleFilesExample(),
                    "<Button Content=\"Pick Multiple Files\" Click=\"PickMultipleFilesButton_Click\" />",
                    "private void PickMultipleFilesButton_Click(object sender, RoutedEventArgs e)\n{\n    var openFileDialog = new OpenFileDialog\n    {\n        Title = \"Select multiple files\",\n        Filter = \"All files (*.*)|*.*|Text files (*.txt)|*.txt\",\n        Multiselect = true\n    };\n\n    if (openFileDialog.ShowDialog() == true)\n    {\n        string[] files = openFileDialog.FileNames;\n    }\n}"),
                new GalleryExample(
                    "Save File",
                    CreateSaveFileExample(),
                    "<Button Content=\"Save File\" Click=\"SaveFileButton_Click\" />",
                    "private void SaveFileButton_Click(object sender, RoutedEventArgs e)\n{\n    var saveFileDialog = new SaveFileDialog\n    {\n        Title = \"Save file\",\n        Filter = \"Text files (*.txt)|*.txt|All files (*.*)|*.*\",\n        DefaultExt = \"txt\"\n    };\n\n    if (saveFileDialog.ShowDialog() == true)\n    {\n        File.WriteAllText(saveFileDialog.FileName, fileContent);\n    }\n}"),
                new GalleryExample(
                    "Pick Folder",
                    CreatePickFolderExample(),
                    "<Button Content=\"Pick Folder\" Click=\"PickFolderButton_Click\" />",
                    "private void PickFolderButton_Click(object sender, RoutedEventArgs e)\n{\n    var folderBrowserDialog = new OpenFolderDialog\n    {\n        Title = \"Select a folder\"\n    };\n\n    if (folderBrowserDialog.ShowDialog() == true)\n    {\n        string folderPath = folderBrowserDialog.FolderName;\n    }\n}")
            };
        }

        private static IReadOnlyList<GalleryExample> CreateFrameExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A Frame",
                    CreateOpenFrameWindowButton(),
                    "<Frame Source=\"FramePage1.xaml\" NavigationUIVisibility=\"Visible\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateHyperlinkExamples()
        {
            var textBlock = new TextBlock
            {
                Margin = new Thickness(20)
            };
            var hyperlink = new Hyperlink(new Run("Hyperlink"))
            {
                NavigateUri = new Uri("https://www.microsoft.com")
            };
            hyperlink.RequestNavigate += OnRequestNavigate;
            textBlock.Inlines.Add(hyperlink);

            return new[]
            {
                new GalleryExample(
                    "A Hyperlink",
                    textBlock,
                    "<TextBlock Margin=\"20\">\n    <Hyperlink NavigateUri=\"https://www.microsoft.com\" RequestNavigate=\"Hyperlink_RequestNavigate\">\n        Lorem Ipsum link\n    </Hyperlink>\n</TextBlock>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateImageExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Standand Image from a local file.",
                    new Image
                    {
                        Height = 200,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Source = new BitmapImage(new Uri("pack://application:,,,/ModernWpf.Gallery;component/Assets/win11-dashboard.light.png"))
                    },
                    "<Image Height=\"100\" Source=\"Assets\\MyImage.jpg\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateLabelExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple Label.",
                    CreateSimpleLabelExample(),
                    "<Label Content=\"I am a Label.\" />",
                    null),
                new GalleryExample(
                    "A Label for TextBox.",
                    CreateLabelForTextBoxExample(),
                    "<Grid>\n    <Grid.RowDefinitions>\n        <RowDefinition Height=\"Auto\" />\n        <RowDefinition Height=\"Auto\" />\n    </Grid.RowDefinitions>\n    <Label Grid.Row=\"0\" Content=\"I am a Label of the TextBox below.\" />\n    <TextBox Grid.Row=\"1\" />\n</Grid>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateListBoxExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "ListBox with items defined inline.",
                    CreateInlineListBox(),
                    "<ListBox SelectedIndex=\"0\">\n    <ListBoxItem>Blue</ListBoxItem>\n    <ListBoxItem>Green</ListBoxItem>\n    <ListBoxItem>Red</ListBoxItem>\n    <ListBoxItem>Yellow</ListBoxItem>\n</ListBox>",
                    null),
                new GalleryExample(
                    "A ListBox with its ItemsSource and Height set.",
                    CreateFontListBox(),
                    "<ListBox Height=\"100\" ItemsSource=\"{Binding ViewModel.MyItems}\" SelectedIndex=\"2\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateListViewExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Basic ListView with Simple DataTemplate.",
                    CreateBasicListView(),
                    "<ListView Height=\"200\" ItemsSource=\"{Binding ViewModel.BasicListViewItems}\" SelectedIndex=\"2\" SelectionMode=\"Single\">\n    <ListView.ItemTemplate>\n        <DataTemplate>\n            <TextBlock Margin=\"8,4\" Text=\"{Binding Name}\" />\n        </DataTemplate>\n    </ListView.ItemTemplate>\n</ListView>",
                    null),
                new GalleryExample(
                    "ListView with Selection Support.",
                    CreateSelectionListView(),
                    "<Grid>\n    <ListView Height=\"200\" ItemsSource=\"{Binding BasicListViewItems}\" SelectedIndex=\"1\" SelectionMode=\"{Binding ListViewSelectionMode}\" />\n    <ComboBox SelectedIndex=\"{Binding ListViewSelectionModeComboBoxSelectedIndex}\">\n        <ComboBoxItem Content=\"Single\" />\n        <ComboBoxItem Content=\"Multiple\" />\n        <ComboBoxItem Content=\"Extended\" />\n    </ComboBox>\n</Grid>",
                    null),
                new GalleryExample(
                    "ListView with GridView.",
                    CreateGridViewListView(),
                    "<ListView Height=\"280\" ItemsSource=\"{Binding ViewModel.GridViewItems}\">\n    <ListView.View>\n        <GridView>\n            <GridViewColumn Header=\"First Name\" Width=\"150\" DisplayMemberBinding=\"{Binding FirstName}\" />\n            <GridViewColumn Header=\"Last Name\" Width=\"150\" DisplayMemberBinding=\"{Binding LastName}\" />\n            <GridViewColumn Header=\"Company\" Width=\"200\" DisplayMemberBinding=\"{Binding Company}\" />\n        </GridView>\n    </ListView.View>\n</ListView>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateMenuExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Standard Menu.",
                    CreateStandardMenuExample(),
                    "<Menu>\n    <MenuItem Header=\"File\">\n        <MenuItem Header=\"New\" />\n        <MenuItem Header=\"New window\" />\n        <MenuItem Header=\"Open...\" />\n        <MenuItem Header=\"Save\" />\n        <MenuItem Header=\"Save As...\" />\n        <Separator />\n        <MenuItem Header=\"Exit\" />\n    </MenuItem>\n    <MenuItem Header=\"Edit\">\n        <MenuItem Header=\"Undo\" />\n        <Separator />\n        <MenuItem Header=\"Cut\" />\n        <MenuItem Header=\"Copy\" />\n        <MenuItem Header=\"Paste\" />\n        <MenuItem IsEnabled=\"False\" />\n        <Separator />\n        <MenuItem Header=\"Search with browser\" />\n        <MenuItem Header=\"Find...\" />\n        <MenuItem Header=\"Find Next\" />\n        <Separator />\n        <MenuItem Header=\"Select All\" />\n    </MenuItem>\n</Menu>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateMessageBoxExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Simple MessageBox",
                    CreateSimpleMessageBoxExample(),
                    "<Button Content=\"Simple MessageBox\" Click=\"ShowDefaultMessageButton_Click\" />",
                    "private void ShowDefaultMessageButton_Click(object sender, RoutedEventArgs e)\n{\n    var result = MessageBox.Show(\"This is a simple message box!\");\n}"),
                new GalleryExample(
                    "MessageBox with Custom Title and Description",
                    CreateCustomMessageBoxExample(),
                    "<Button Content=\"Show MessageBox\" Click=\"ShowCustomTitleButton_Click\" />",
                    "private void ShowCustomTitleButton_Click(object sender, RoutedEventArgs e)\n{\n    var result = MessageBox.Show(\n        \"This is a detailed description of what happened or what action is needed.\",\n        \"Custom Title\");\n}"),
                new GalleryExample(
                    "MessageBox with Different Buttons",
                    CreateMessageBoxButtonsExample(),
                    "<Button Content=\"Show MessageBox\" Click=\"ShowButtonFromComboBox_Click\" />",
                    "private void ShowButtonFromComboBox_Click(object sender, RoutedEventArgs e)\n{\n    var result = MessageBox.Show(\n        \"Choose one of the available responses.\",\n        \"MessageBox buttons\",\n        MessageBoxButton.YesNoCancel);\n}"),
                new GalleryExample(
                    "MessageBox with Different Images",
                    CreateMessageBoxImagesExample(),
                    "<Button Content=\"Show MessageBox\" Click=\"ShowImageFromComboBox_Click\" />",
                    "private void ShowImageFromComboBox_Click(object sender, RoutedEventArgs e)\n{\n    var result = MessageBox.Show(\n        \"This MessageBox displays the Information icon.\",\n        \"Information Icon\",\n        MessageBoxButton.OK,\n        MessageBoxImage.Information);\n}"),
                new GalleryExample(
                    "Information, Error, and Warning MessageBox",
                    CreateCommonMessageBoxExample(),
                    "<WrapPanel Margin=\"0,0,0,10\">\n    <Button Content=\"Information\" Click=\"ShowInformationButton_Click\" />\n    <Button Content=\"Error\" Click=\"ShowErrorButton_Click\" />\n    <Button Content=\"Warning\" Click=\"ShowWarningButton_Click\" />\n</WrapPanel>",
                    "private void ShowInformationButton_Click(object sender, RoutedEventArgs e)\n{\n    MessageBox.Show(\"Operation completed successfully.\", \"Information\", MessageBoxButton.OK, MessageBoxImage.Information);\n}\n\nprivate void ShowErrorButton_Click(object sender, RoutedEventArgs e)\n{\n    MessageBox.Show(\"An error occurred!\", \"Error\", MessageBoxButton.OK, MessageBoxImage.Error);\n}\n\nprivate void ShowWarningButton_Click(object sender, RoutedEventArgs e)\n{\n    MessageBox.Show(\"This action cannot be undone!\", \"Warning\", MessageBoxButton.OKCancel, MessageBoxImage.Warning);\n}"),
                new GalleryExample(
                    "MessageBox with Custom Default Button",
                    CreateCustomDefaultMessageBoxExample(),
                    "<Button Content=\"Show with 'No' as default\" Click=\"ShowCustomDefaultButton_Click\" />",
                    "private void ShowCustomDefaultButton_Click(object sender, RoutedEventArgs e)\n{\n    var result = MessageBox.Show(\n        \"Do you want to save changes? Press Enter to select the default 'No' button.\",\n        \"Save Changes\",\n        MessageBoxButton.YesNoCancel,\n        MessageBoxImage.Question,\n        MessageBoxResult.No);\n}")
            };
        }

        private static IReadOnlyList<GalleryExample> CreateNavigationWindowExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A Navigation Window",
                    CreateOpenNavigationWindowButton(),
                    "<NavigationWindow Width=\"800\" Height=\"450\" Source=\"/Views/Navigation/Page1.xaml\" />",
                    "private void OpenNavigationWindow_Click(object sender, RoutedEventArgs e)\n{\n    NavigationWindow window = new NavigationWindow()\n    {\n        Width = 800,\n        Height = 450,\n        Source = new Uri(\"/Views/Navigation/Page1.xaml\", UriKind.Relative)\n    };\n    window.Show();\n}")
            };
        }

        private static IReadOnlyList<GalleryExample> CreatePasswordBoxExamples()
        {
            var passwordBox = new PasswordBox();
            AutomationProperties.SetName(passwordBox, "Simple Password Box");

            return new[]
            {
                new GalleryExample(
                    "A simple PasswordBox.",
                    passwordBox,
                    "<PasswordBox />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateProgressBarExamples()
        {
            var determinate = new ProgressBar
            {
                Margin = new Thickness(24),
                Value = 40
            };
            AutomationProperties.SetName(determinate, "A determinate");

            var indeterminate = new ProgressBar
            {
                Margin = new Thickness(24),
                IsIndeterminate = true
            };
            AutomationProperties.SetName(indeterminate, "An indeterminate");

            return new[]
            {
                new GalleryExample(
                    "A simple progress bar.",
                    determinate,
                    "<ProgressBar Value=\"40\" />",
                    null),
                new GalleryExample(
                    "An indeterminate progress bar.",
                    indeterminate,
                    "<ProgressBar IsIndeterminate=\"True\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateRadioButtonExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Standard RadioButton.",
                    CreateRadioButtonGroup("radio_group_one", FlowDirection.LeftToRight, true),
                    "<StackPanel>\n    <RadioButton Content=\"Option 1\" GroupName=\"radio_group_one\" IsChecked=\"True\" />\n    <RadioButton Content=\"Option 2\" GroupName=\"radio_group_one\" />\n    <RadioButton Content=\"Option 3\" GroupName=\"radio_group_one\" />\n</StackPanel>",
                    null),
                new GalleryExample(
                    "RadioButton with right to left flow direction.",
                    CreateRadioButtonGroup("radio_group_two", FlowDirection.RightToLeft, false),
                    "<StackPanel>\n    <RadioButton Content=\"Option 1\" FlowDirection=\"RightToLeft\" GroupName=\"radio_group_one\" IsChecked=\"True\" />\n    <RadioButton Content=\"Option 2\" FlowDirection=\"RightToLeft\" GroupName=\"radio_group_one\" />\n    <RadioButton Content=\"Option 3\" FlowDirection=\"RightToLeft\" GroupName=\"radio_group_one\" />\n</StackPanel>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateResizeGripExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A ResizeGrip",
                    CreateResizeGripWindowExample(),
                    "<Window Width=\"500\" Height=\"300\" ResizeMode=\"CanResizeWithGrip\">\n    <TextBlock Text=\"ResizeGrip is present at the bottom right corner of the window\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" FontSize=\"16\" />\n</Window>",
                    "private void OpenResizeGripWindow_Click(object sender, RoutedEventArgs e)\n{\n    Window window = new Window()\n    {\n        Width = 500,\n        Height = 300,\n        ResizeMode = ResizeMode.CanResizeWithGrip,\n        Content = new TextBlock\n        {\n            Text = \"ResizeGrip is present at the bottom right corner of the window\",\n            HorizontalAlignment = HorizontalAlignment.Center,\n            VerticalAlignment = VerticalAlignment.Center,\n            FontSize = 16\n        }\n    };\n    window.Show();\n}")
            };
        }

        private static IReadOnlyList<GalleryExample> CreateRichEditBoxExamples()
        {
            var richTextBox = new RichTextBox
            {
                Width = 480,
                Height = 160
            };
            AutomationProperties.SetName(richTextBox, "simple rich text editor");

            return new[]
            {
                new GalleryExample(
                    "A simple RichTextBox",
                    richTextBox,
                    "<RichTextBox />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateSliderExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A simple slider.",
                    CreateSliderWithOutput(0, 100, 0, 0, TickPlacement.None, Orientation.Horizontal, "Simple"),
                    "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Maximum=\"100\" Minimum=\"0\" />",
                    null),
                new GalleryExample(
                    "A slider with steps and range specified.",
                    CreateSliderWithOutput(500, 1000, 500, 50, TickPlacement.None, Orientation.Horizontal, "Range and steps specified"),
                    "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" IsSnapToTickEnabled=\"True\" Maximum=\"1000\" Minimum=\"500\" TickFrequency=\"50\" />",
                    null),
                new GalleryExample(
                    "A slider with tick marks.",
                    CreateSliderWithOutput(0, 100, 0, 20, TickPlacement.Both, Orientation.Horizontal, "Tick marks"),
                    "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" IsSnapToTickEnabled=\"True\" Maximum=\"100\" Minimum=\"0\" TickFrequency=\"20\" TickPlacement=\"Both\" />",
                    null),
                new GalleryExample(
                    "A vertical slider with range and tick marks specified.",
                    CreateSliderWithOutput(0, 100, 0, 20, TickPlacement.Both, Orientation.Vertical, "Vertical"),
                    "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" IsSnapToTickEnabled=\"True\" Maximum=\"100\" Minimum=\"0\" Orientation=\"Vertical\" TickFrequency=\"20\" TickPlacement=\"Both\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateStackPanelExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "A basic vertical StackPanel",
                    CreateStackPanel(Orientation.Vertical),
                    "<StackPanel Orientation=\"Vertical\">\n    <Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" />\n    <Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" />\n    <Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" />\n</StackPanel>",
                    null),
                new GalleryExample(
                    "A horizontal StackPanel",
                    CreateStackPanel(Orientation.Horizontal),
                    "<StackPanel Orientation=\"Horizontal\">\n    <Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" />\n    <Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" />\n    <Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" />\n</StackPanel>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateTabControlExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Standard TabControl.",
                    CreateStandardTabControl(),
                    "<TabControl Margin=\"0,8,0,0\">\n    <TabItem>\n        <TabItem.Header>\n            <StackPanel Orientation=\"Horizontal\">\n                <TextBlock Text=\"Hello\" />\n            </StackPanel>\n        </TabItem.Header>\n        <Grid>\n            <TextBlock Margin=\"12\" Text=\"World\" />\n        </Grid>\n    </TabItem>\n    <TabItem IsSelected=\"True\">\n        <TabItem.Header>\n            <StackPanel Orientation=\"Horizontal\">\n                <TextBlock Text=\"The cake\" />\n            </StackPanel>\n        </TabItem.Header>\n        <Grid>\n            <TextBlock Margin=\"12\" Text=\"Is a lie.\" />\n        </Grid>\n    </TabItem>\n</TabControl>",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateTextBoxExamples()
        {
            var simpleTextBox = new TextBox();
            AutomationProperties.SetName(simpleTextBox, "simple TextBox");

            var multiLineTextBox = new TextBox
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap
            };
            AutomationProperties.SetName(multiLineTextBox, "multi-line TextBox");

            return new[]
            {
                new GalleryExample(
                    "A simple TextBox.",
                    simpleTextBox,
                    "<TextBox />",
                    null),
                new GalleryExample(
                    "A TextBox with input validation.",
                    CreateValidatedTextBoxExample(),
                    "<TextBox>\n    <TextBox.Text>\n        <Binding Path=\"ViewModel.ValidatedText\" UpdateSourceTrigger=\"PropertyChanged\">\n            <Binding.ValidationRules>\n                <helpers:AlphabeticValidationRule />\n            </Binding.ValidationRules>\n        </Binding>\n    </TextBox.Text>\n</TextBox>",
                    null),
                new GalleryExample(
                    "A multi-line TextBox.",
                    multiLineTextBox,
                    "<TextBox TextWrapping=\"Wrap\" AcceptsReturn=\"True\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateToolTipExamples()
        {
            var button = new Button
            {
                Content = "Button with a simple ToolTip."
            };
            ToolTipService.SetInitialShowDelay(button, 100);
            ToolTipService.SetPlacement(button, PlacementMode.MousePoint);
            ToolTipService.SetToolTip(button, "Simple ToolTip");
            AutomationProperties.SetName(button, "TooltipButton");

            return new[]
            {
                new GalleryExample(
                    "A button with a simple ToolTip.",
                    button,
                    "<Button Content=\"Button with a simple ToolTip.\" ToolTipService.InitialShowDelay=\"100\" ToolTipService.Placement=\"MousePoint\" ToolTipService.ToolTip=\"Simple ToolTip\" />",
                    null)
            };
        }

        private static IReadOnlyList<GalleryExample> CreateTreeViewExamples()
        {
            return new[]
            {
                new GalleryExample(
                    "Simple TreeView.",
                    CreateTreeView(),
                    "<TreeView AllowDrop=\"True\" ScrollViewer.CanContentScroll=\"False\">\n    <TreeViewItem Header=\"Work Documents\" IsExpanded=\"True\" IsSelected=\"True\">\n        <TreeViewItem Header=\"Feature Schedule\" />\n        <TreeViewItem Header=\"Overall Project Plan\" />\n    </TreeViewItem>\n    <TreeViewItem Header=\"Personal Documents\">\n        <TreeViewItem Header=\"Contractor contact info\" />\n        <TreeViewItem Header=\"Home Remodel\">\n            <TreeViewItem Header=\"Paint Color Scheme\" />\n            <TreeViewItem Header=\"Flooring Woodgrain Type\" />\n            <TreeViewItem Header=\"Kitchen Cabinet Style\" />\n        </TreeViewItem>\n    </TreeViewItem>\n</TreeView>",
                    null)
            };
        }

        private static TextBlock CreateInlineTextBlock()
        {
            var textBlock = new TextBlock { FontSize = 14 };
            textBlock.Inlines.Add(new Run("Text in a TextBlock doesn't have to be a simple string.")
            {
                FontFamily = new FontFamily("Times New Roman"),
                Foreground = (Brush)Application.Current.TryFindResource("TextFillColorPrimaryBrush")
            });
            textBlock.Inlines.Add(new LineBreak());
            textBlock.Inlines.Add(new Run("Text can be "));
            textBlock.Inlines.Add(new Bold(new Run("bold")));
            textBlock.Inlines.Add(new Run(", "));
            textBlock.Inlines.Add(new Italic(new Run("italic")));
            textBlock.Inlines.Add(new Run(", or "));
            textBlock.Inlines.Add(new Underline(new Run("underlined")));
            textBlock.Inlines.Add(new Run("."));
            return textBlock;
        }

        private static Label CreateSimpleLabelExample()
        {
            var label = new Label
            {
                Content = "I am a Label.",
                Opacity = 0.7
            };
            label.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush");
            return label;
        }

        private static Grid CreateLabelForTextBoxExample()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new Label
            {
                Content = "I am a Label of the TextBox below.",
                Opacity = 0.7
            };
            label.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush");

            var textBox = new TextBox();
            AutomationProperties.SetName(textBox, "Simple Text Box");
            Grid.SetRow(textBox, 1);

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            return grid;
        }

        private static TextBox CreateValidatedTextBoxExample()
        {
            var textBox = new TextBox();
            AutomationProperties.SetName(textBox, "validated TextBox");

            var binding = new Binding("Text")
            {
                Source = new ValidatedTextBoxState(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            binding.ValidationRules.Add(new AlphabeticValidationRule());
            textBox.SetBinding(TextBox.TextProperty, binding);
            return textBox;
        }

        private static StackPanel CreateButtonResultExample(Button button, TextBlock output)
        {
            var stack = new StackPanel();
            stack.Children.Add(button);
            stack.Children.Add(output);
            return stack;
        }

        private static Grid CreateExpanderExample()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var expander = new Expander
            {
                Header = "This text is in the header",
                Content = "This is in the content"
            };
            Grid.SetColumn(expander, 0);

            grid.Children.Add(expander);
            return grid;
        }

        private static Border CreateClipboardNotice()
        {
            var border = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 16)
            };
            border.SetResourceReference(Border.BackgroundProperty, "SubtleFillColorSecondaryBrush");
            border.SetResourceReference(Border.BorderBrushProperty, "AccentFillColorDefaultBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var icon = new TextBlock
            {
                FontSize = 16,
                Text = "\uE946",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 12, 0)
            };
            icon.SetResourceReference(TextBlock.FontFamilyProperty, "SymbolThemeFontFamily");
            icon.SetResourceReference(TextBlock.ForegroundProperty, "AccentFillColorDefaultBrush");
            AutomationProperties.SetName(icon, "Info");

            var text = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            text.Inlines.Add(new Run("Note: ") { FontWeight = FontWeights.SemiBold });
            text.Inlines.Add(new Run("Since .NET 9, few of the Clipboard APIs have become obsolete. For more information please read "));
            var link = new Hyperlink(new Run("here"))
            {
                NavigateUri = new Uri("https://learn.microsoft.com/en-us/dotnet/desktop/winforms/migration/clipboard-dataobject-net10")
            };
            link.RequestNavigate += OnRequestNavigate;
            text.Inlines.Add(link);
            text.Inlines.Add(new Run("."));

            Grid.SetColumn(text, 1);
            grid.Children.Add(icon);
            grid.Children.Add(text);
            border.Child = grid;
            return border;
        }

        private static StackPanel CreateDesignNotice(params string[] paragraphs)
        {
            var stack = new StackPanel
            {
                Margin = new Thickness(10, 0, 10, 24)
            };

            foreach (var paragraph in paragraphs)
            {
                var text = new TextBlock
                {
                    Margin = new Thickness(0, 0, 0, 8),
                    Text = paragraph,
                    TextWrapping = TextWrapping.Wrap
                };
                text.SetResourceReference(FrameworkElement.StyleProperty, "BodyTextBlockStyle");
                stack.Children.Add(text);
            }

            return stack;
        }

        private static StackPanel CreateColorResourcesExample()
        {
            var root = new StackPanel();
            var selector = new ComboBox
            {
                Width = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = new[] { "Text", "Fill", "Stroke", "Background", "Signal", "HighContrast" },
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 0, 12)
            };
            AutomationProperties.SetName(selector, "Page Selector");
            root.Children.Add(selector);

            var tiles = new UniformGrid
            {
                Columns = 2
            };
            tiles.Children.Add(CreateColorResourceTile("Primary text", "TextFillColorPrimaryBrush", "Primary body text and page titles."));
            tiles.Children.Add(CreateColorResourceTile("Secondary text", "TextFillColorSecondaryBrush", "Supplementary text and descriptions."));
            tiles.Children.Add(CreateColorResourceTile("Tertiary text", "TextFillColorTertiaryBrush", "Lower-emphasis supporting text."));
            tiles.Children.Add(CreateColorResourceTile("Disabled text", "TextFillColorDisabledBrush", "Unavailable commands and inactive text."));
            tiles.Children.Add(CreateColorResourceTile("Accent", "AccentFillColorDefaultBrush", "Primary accent actions and highlights."));
            tiles.Children.Add(CreateColorResourceTile("Card background", "CardBackgroundFillColorDefaultBrush", "Cards and elevated content surfaces."));
            root.Children.Add(tiles);

            return root;
        }

        private static Border CreateColorResourceTile(string colorName, string brushName, string explanation)
        {
            var border = new Border
            {
                Width = 330,
                MinHeight = 118,
                Margin = new Thickness(0, 0, 8, 8),
                Padding = new Thickness(12),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            border.SetResourceReference(Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var swatch = new Border
            {
                Width = 48,
                Height = 48,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 12, 0)
            };
            swatch.SetResourceReference(Border.BackgroundProperty, brushName);
            swatch.SetResourceReference(Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");
            grid.Children.Add(swatch);

            var textStack = new StackPanel();
            Grid.SetColumn(textStack, 1);
            var title = new TextBlock { Text = colorName, TextWrapping = TextWrapping.Wrap };
            title.SetResourceReference(FrameworkElement.StyleProperty, "BodyStrongTextBlockStyle");
            textStack.Children.Add(title);
            var body = new TextBlock
            {
                Text = explanation,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 8)
            };
            body.SetResourceReference(FrameworkElement.StyleProperty, "CaptionTextBlockStyle");
            textStack.Children.Add(body);
            var brush = new TextBlock { Text = brushName, TextWrapping = TextWrapping.Wrap };
            brush.SetResourceReference(FrameworkElement.StyleProperty, "CaptionTextBlockStyle");
            textStack.Children.Add(brush);
            grid.Children.Add(textStack);

            border.Child = grid;
            return border;
        }

        private static StackPanel CreateGeometryExample()
        {
            var root = new StackPanel();
            root.Children.Add(CreateDesignImage("Geometry.dark.png", "Example of corner radius.", 500, 300));
            root.Children.Add(CreateCornerRadiusTable());
            return root;
        }

        private static Grid CreateCornerRadiusTable()
        {
            var grid = CreateTableGrid(new[] { 148.0, 400.0, 180.0 }, 4);
            AddHeaderRow(grid, new[] { "Corner radius", "Usage", "Style" });
            AddCornerRadiusRow(grid, 1, "8px", 8, "Top-level containers such as app windows, flyouts, cards and dialogs.", "OverlayCornerRadius", true);
            AddCornerRadiusRow(grid, 2, "4px", 4, "In-page elements such as controls and list backplates.", "ControlCornerRadius", false);
            AddCornerRadiusRow(grid, 3, "0px", 0, "Straight edges that intersect with other straight edges.", "N/A", true);
            return grid;
        }

        private static void AddCornerRadiusRow(Grid grid, int row, string value, double radius, string usage, string styleName, bool shaded)
        {
            var rowGrid = CreateShadedRow(60, shaded);
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(148) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(400) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });

            var sample = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(16, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var shape = new Border
            {
                Width = 20,
                Height = 20,
                CornerRadius = new CornerRadius(radius),
                VerticalAlignment = VerticalAlignment.Center
            };
            shape.SetResourceReference(Border.BackgroundProperty, "AccentFillColorDefaultBrush");
            sample.Children.Add(shape);
            var label = CreateTableText(value, "BodyTextBlockStyle");
            label.Margin = new Thickness(16, 0, 0, 0);
            sample.Children.Add(label);

            rowGrid.Children.Add(sample);
            AddTableCell(rowGrid, CreateTableText(usage, "CaptionTextBlockStyle"), 0, 1, new Thickness(16, 0, 16, 0));
            AddTableCell(rowGrid, CreateTableText(styleName, "CaptionTextBlockStyle"), 0, 2, new Thickness(16, 0, 16, 0));
            AddTableCell(grid, rowGrid, row, 0, new Thickness(0));
            Grid.SetColumnSpan(rowGrid, 3);
        }

        private static StackPanel CreateIconographyExample()
        {
            var root = new StackPanel();
            var expander = new Expander
            {
                Header = "Instructions on how to use Segoe Fluent Icons",
                Margin = new Thickness(0, 0, 0, 24),
                Content = new TextBlock
                {
                    Text = "On Windows 11, the Segoe Fluent Icons font comes with Windows. Use TextBlock with SymbolThemeFontFamily and a glyph value such as &#xE8A7; for predictable 16, 20, 24, 32, 40, 48, and 64 pixel sizing.",
                    TextWrapping = TextWrapping.Wrap
                }
            };
            root.Children.Add(expander);

            var searchBox = new TextBox
            {
                Width = 500,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 16)
            };
            AutomationProperties.SetName(searchBox, "Search Icons by Name, Tag");
            root.Children.Add(searchBox);

            var icons = new WrapPanel();
            icons.Children.Add(CreateIconGlyphCard("Open in new window", "\uE8A7", "E8A7"));
            icons.Children.Add(CreateIconGlyphCard("Copy", "\uE8C8", "E8C8"));
            icons.Children.Add(CreateIconGlyphCard("Accept", "\uE73E", "E73E"));
            icons.Children.Add(CreateIconGlyphCard("Search", "\uE721", "E721"));
            icons.Children.Add(CreateIconGlyphCard("Settings", "\uE713", "E713"));
            icons.Children.Add(CreateIconGlyphCard("Back", "\uE72B", "E72B"));
            root.Children.Add(icons);
            return root;
        }

        private static Border CreateIconGlyphCard(string name, string glyph, string code)
        {
            var border = new Border
            {
                Width = 150,
                Height = 124,
                Margin = new Thickness(0, 0, 12, 12),
                Padding = new Thickness(12),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8)
            };
            border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            border.SetResourceReference(Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");

            var stack = new StackPanel();
            var glyphText = new TextBlock
            {
                Text = glyph,
                FontSize = 32,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 8)
            };
            glyphText.SetResourceReference(TextBlock.FontFamilyProperty, "SymbolThemeFontFamily");
            stack.Children.Add(glyphText);
            var nameText = CreateTableText(name, "CaptionTextBlockStyle");
            nameText.FontWeight = FontWeights.SemiBold;
            stack.Children.Add(nameText);
            stack.Children.Add(CreateTableText("&#x" + code + ";", "CaptionTextBlockStyle"));
            border.Child = stack;
            return border;
        }

        private static StackPanel CreateSpacingExample()
        {
            var root = new StackPanel();
            var images = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 16)
            };
            images.Children.Add(CreateDesignImage("Cards.dark.png", "Page with cards layout", 280, 280));
            images.Children.Add(CreateDesignImage("Dialog.dark.png", "Form layout", 280, 280));
            root.Children.Add(images);
            root.Children.Add(CreateSpacingTable());
            return root;
        }

        private static Grid CreateSpacingTable()
        {
            var grid = CreateTableGrid(new[] { 90.0, 100.0, 400.0 }, 8);
            AddHeaderRow(grid, new[] { "Value", "", "Usage" });
            AddSpacingRow(grid, 1, "4px", 4, "Spacing used for compact sizing.", true);
            AddSpacingRow(grid, 2, "8px", 8, "Spacing between UI controls, control + label.", false);
            AddSpacingRow(grid, 3, "12px", 12, "Spacing between control + header, surface and edge text, text sections.", true);
            AddSpacingRow(grid, 4, "16px", 16, "Padding used in list styles, cards.", false);
            AddSpacingRow(grid, 5, "24px", 24, "Spacing between content sections.", true);
            AddSpacingRow(grid, 6, "32px", 32, "Padding on pages.", false);
            AddSpacingRow(grid, 7, "48px", 48, "Spacing between page sections with title.", true);
            return grid;
        }

        private static void AddSpacingRow(Grid grid, int row, string value, double width, string usage, bool shaded)
        {
            var rowGrid = CreateShadedRow(60, shaded);
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(400) });

            AddTableCell(rowGrid, CreateTableText(value, "BodyTextBlockStyle"), 0, 0, new Thickness(16, 0, 0, 0));
            var bar = new Border
            {
                Width = width,
                Height = 24,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            bar.SetResourceReference(Border.BackgroundProperty, "AccentFillColorDefaultBrush");
            AddTableCell(rowGrid, bar, 0, 1, new Thickness(0));
            AddTableCell(rowGrid, CreateTableText(usage, "CaptionTextBlockStyle"), 0, 2, new Thickness(0, 0, 16, 0));

            AddTableCell(grid, rowGrid, row, 0, new Thickness(0));
            Grid.SetColumnSpan(rowGrid, 3);
        }

        private static Grid CreateTypographyRampExample()
        {
            var grid = CreateTableGrid(new[] { 272.0, 136.0, 112.0, 164.0 }, 8);
            AddHeaderRow(grid, new[] { "Example", "Variable Font", "Size/Line height", "Style" });
            AddTypographyRow(grid, 1, "Caption", "CaptionTextBlockStyle", "Small, Regular", "12/16 epx", "CaptionTextBlockStyle", true);
            AddTypographyRow(grid, 2, "Body", "BodyTextBlockStyle", "Text, Regular", "14/20 epx", "BodyTextBlockStyle", false);
            AddTypographyRow(grid, 3, "Body Strong", "BodyStrongTextBlockStyle", "Text, SemiBold", "14/20 epx", "BodyStrongTextBlockStyle", true);
            AddTypographyRow(grid, 4, "Subtitle", "SubtitleTextBlockStyle", "Display, SemiBold", "20/28 epx", "SubtitleTextBlockStyle", false);
            AddTypographyRow(grid, 5, "Title", "TitleTextBlockStyle", "Display, SemiBold", "28/36 epx", "TitleTextBlockStyle", true);
            AddTypographyRow(grid, 6, "Title Large", "TitleLargeTextBlockStyle", "Display, SemiBold", "40/52 epx", "TitleLargeTextBlockStyle", false);
            AddTypographyRow(grid, 7, "Display", "DisplayTextBlockStyle", "Display, SemiBold", "68/92 epx", "DisplayTextBlockStyle", true);
            return grid;
        }

        private static void AddTypographyRow(Grid grid, int row, string example, string exampleStyle, string variableFont, string size, string styleName, bool shaded)
        {
            var rowGrid = CreateShadedRow(row == 7 ? 96 : 68, shaded);
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(272) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(136) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(112) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(164) });

            AddTableCell(rowGrid, CreateTableText(example, exampleStyle), 0, 0, new Thickness(16, 0, 0, 0));
            AddTableCell(rowGrid, CreateTableText(variableFont, "CaptionTextBlockStyle"), 0, 1, new Thickness(0));
            AddTableCell(rowGrid, CreateTableText(size, "CaptionTextBlockStyle"), 0, 2, new Thickness(0));
            AddTableCell(rowGrid, CreateTableText(styleName, "CaptionTextBlockStyle"), 0, 3, new Thickness(0));

            AddTableCell(grid, rowGrid, row, 0, new Thickness(0));
            Grid.SetColumnSpan(rowGrid, 4);
        }

        private static Grid CreateTableGrid(double[] columnWidths, int rowCount)
        {
            var grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 10)
            };

            foreach (var width in columnWidths)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width) });
            }

            for (var i = 0; i < rowCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            return grid;
        }

        private static void AddHeaderRow(Grid grid, string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                var text = CreateTableText(headers[i], "CaptionTextBlockStyle");
                text.Opacity = 0.7;
                AddTableCell(grid, text, 0, i, new Thickness(16, 0, 0, 24));
            }
        }

        private static Grid CreateShadedRow(double minHeight, bool shaded)
        {
            var rowGrid = new Grid
            {
                MinHeight = minHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            if (shaded)
            {
                rowGrid.SetResourceReference(Panel.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            }

            return rowGrid;
        }

        private static TextBlock CreateTableText(string text, string styleKey)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            textBlock.SetResourceReference(FrameworkElement.StyleProperty, styleKey);
            return textBlock;
        }

        private static void AddTableCell(Grid grid, UIElement element, int row, int column, Thickness margin)
        {
            var frameworkElement = element as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.Margin = margin;
            }

            Grid.SetRow(element, row);
            Grid.SetColumn(element, column);
            grid.Children.Add(element);
        }

        private static Grid CreateDesignImage(string fileName, string title, double width, double height)
        {
            var grid = new Grid
            {
                Width = width,
                Margin = new Thickness(0, 0, 16, 16),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var titleText = new TextBlock
            {
                Text = title,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            titleText.SetResourceReference(FrameworkElement.StyleProperty, "SubtitleTextBlockStyle");
            grid.Children.Add(titleText);

            var image = new Image
            {
                Width = width,
                Height = height,
                Stretch = Stretch.Uniform,
                Source = new BitmapImage(new Uri("pack://application:,,,/ModernWpf.Gallery;component/Assets/Design/" + fileName))
            };
            AutomationProperties.SetName(image, title);
            Grid.SetRow(image, 1);
            grid.Children.Add(image);

            return grid;
        }

        private static Grid CreateUserDashboardPageContent()
        {
            var users = CreateDashboardUsers();
            var selectedUser = users[0];

            var root = new Grid
            {
                MinHeight = 620
            };
            AutomationProperties.SetAutomationId(root, "ContentRootGrid");
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MaxHeight = 280 });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

            var userListGrid = new Grid();
            userListGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            userListGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRowSpan(userListGrid, 2);
            root.Children.Add(userListGrid);

            var userList = new ListView
            {
                Width = 300,
                SelectionMode = SelectionMode.Single
            };
            userList.SetResourceReference(Control.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
            AutomationProperties.SetName(userList, "Users");
            foreach (var user in users)
            {
                var item = new ListViewItem
                {
                    Content = CreateUserListItemContent(user)
                };
                AutomationProperties.SetName(item, user.Name);
                userList.Items.Add(item);
            }
            userList.SelectedIndex = 0;
            userListGrid.Children.Add(userList);

            var addButton = new Button
            {
                Content = "Add New User",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };
            addButton.SetResourceReference(FrameworkElement.StyleProperty, "AccentButtonStyle");
            Grid.SetRow(addButton, 1);
            userListGrid.Children.Add(addButton);

            var detailsGrid = new Grid();
            detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            detailsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(detailsGrid, 1);
            Grid.SetRowSpan(detailsGrid, 2);
            root.Children.Add(detailsGrid);

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 10, 20, 10)
            };
            header.Children.Add(new Ellipse
            {
                Width = 96,
                Height = 96,
                Margin = new Thickness(12),
                Fill = CreateDashboardAvatarBrush(selectedUser.ImageId)
            });
            var headerText = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            headerText.Children.Add(new TextBlock { Text = selectedUser.Name, FontSize = 24 });
            headerText.Children.Add(new TextBlock { Text = selectedUser.Company, FontSize = 16 });
            header.Children.Add(headerText);
            detailsGrid.Children.Add(header);

            var formGrid = new Grid
            {
                Margin = new Thickness(20, 10, 20, 10)
            };
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(formGrid, 1);
            detailsGrid.Children.Add(formGrid);

            var scrollViewer = new ScrollViewer
            {
                Focusable = false
            };
            Grid.SetColumn(scrollViewer, 1);
            formGrid.Children.Add(scrollViewer);

            var form = new StackPanel
            {
                Margin = new Thickness(20, 0, 20, 0)
            };
            scrollViewer.Content = form;

            var nameGrid = new Grid();
            nameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            nameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            nameGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            nameGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            form.Children.Add(nameGrid);

            var firstNamePanel = CreateFormField("First Name", selectedUser.FirstName);
            firstNamePanel.Margin = new Thickness(0, 0, 10, 0);
            nameGrid.Children.Add(firstNamePanel);
            var lastNamePanel = CreateFormField("Last Name", selectedUser.LastName);
            Grid.SetColumn(lastNamePanel, 1);
            nameGrid.Children.Add(lastNamePanel);

            form.Children.Add(CreateFormLabel("Company"));
            form.Children.Add(CreateFormTextBox(selectedUser.Company));
            form.Children.Add(CreateFormLabel("Address"));
            form.Children.Add(CreateFormTextBox(selectedUser.Address));

            var ageHeader = new Grid();
            ageHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ageHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            ageHeader.Children.Add(CreateFormLabel("Age"));
            var ageValue = new TextBlock
            {
                Text = selectedUser.Age.ToString(),
                Padding = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(ageValue, 1);
            ageHeader.Children.Add(ageValue);
            form.Children.Add(ageHeader);

            form.Children.Add(new Slider
            {
                Minimum = 21,
                Maximum = 62,
                IsSnapToTickEnabled = true,
                IsEnabled = false,
                Value = selectedUser.Age,
                Margin = new Thickness(0, 5, 0, 15)
            });

            form.Children.Add(CreateFormLabel("Date of Joining"));
            form.Children.Add(new DatePicker
            {
                SelectedDate = selectedUser.DateOfJoining,
                IsEnabled = false,
                Margin = new Thickness(0, 5, 0, 15)
            });

            var graduatePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 15)
            };
            var graduateLabel = CreateFormLabel("Is user a new graduate ?");
            graduateLabel.Margin = new Thickness(0, 0, 10, 0);
            graduatePanel.Children.Add(graduateLabel);
            graduatePanel.Children.Add(new CheckBox
            {
                IsChecked = selectedUser.IsNewGraduate,
                IsEnabled = false,
                VerticalAlignment = VerticalAlignment.Center
            });
            form.Children.Add(graduatePanel);

            var commands = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Horizontal
            };
            commands.Children.Add(new Button { Content = "Edit", Margin = new Thickness(10) });
            commands.Children.Add(new Button { Content = "Delete", Margin = new Thickness(10) });
            form.Children.Add(commands);

            return root;
        }

        private static Grid CreateUserListItemContent(DashboardUser user)
        {
            var grid = new Grid
            {
                Margin = new Thickness(8, 0, 8, 0)
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatar = new Ellipse
            {
                Width = 32,
                Height = 32,
                Margin = new Thickness(6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = CreateDashboardAvatarBrush(user.ImageId)
            };
            Grid.SetRowSpan(avatar, 2);
            grid.Children.Add(avatar);

            var name = new TextBlock
            {
                Text = user.Name,
                Margin = new Thickness(12, 6, 0, 0)
            };
            Grid.SetColumn(name, 1);
            grid.Children.Add(name);

            var company = new TextBlock
            {
                Text = user.Company,
                Margin = new Thickness(12, 0, 0, 6),
                Opacity = 0.8
            };
            company.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            Grid.SetRow(company, 1);
            Grid.SetColumn(company, 1);
            grid.Children.Add(company);

            return grid;
        }

        private static StackPanel CreateFormField(string label, string value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            panel.Children.Add(CreateFormLabel(label));
            panel.Children.Add(CreateFormTextBox(value));
            return panel;
        }

        private static Label CreateFormLabel(string content)
        {
            var label = new Label
            {
                Content = content,
                FontWeight = FontWeights.SemiBold,
                Opacity = 0.67,
                Padding = new Thickness(0)
            };
            label.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush");
            return label;
        }

        private static TextBox CreateFormTextBox(string text)
        {
            return new TextBox
            {
                Text = text,
                IsReadOnly = true,
                Margin = new Thickness(0, 5, 0, 15)
            };
        }

        private static ImageBrush CreateDashboardAvatarBrush(string imageId)
        {
            return new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri("pack://application:,,,/ModernWpf.Gallery;component/Assets/UserDashboard/" + imageId + "-100x100.jpg")),
                Stretch = Stretch.UniformToFill
            };
        }

        private static DashboardUser[] CreateDashboardUsers()
        {
            return new[]
            {
                new DashboardUser("91", "John", "Doe", "Luminary Nexus", "Room 1450, 9819 Rutledge Parkway, Saint Louis, Missouri, United States", 37, new DateTime(2022, 8, 15), false),
                new DashboardUser("65", "Adrianna", "Cisneros", "CrestWave Dynamics", "18th Floor, 3631 Manitowish Point, Mobile, Alabama, United States", 29, new DateTime(2021, 3, 22), true),
                new DashboardUser("64", "Spencer", "Lynch", "Horizon Ventures", "Apt 1145, Kansas, United States", 44, new DateTime(2020, 11, 4), false),
                new DashboardUser("103", "Phoebe", "Munoz", "Sapphire Pulse Technologies", "PO Box 54647, 252 Derek Way, Flushing, New York, United States", 25, new DateTime(2024, 1, 9), true),
                new DashboardUser("177", "Lucas", "Marsh", "EmberLight Industries", "20th Floor, 5524 Badeau Pass, Glendale, Arizona, United States", 32, new DateTime(2023, 6, 5), false),
                new DashboardUser("334", "Marissa", "Bartlett", "StellarEdge Ventrues", "Room 1121, 9 Kipling Terrace, Winston Salem, North Carolina, United States", 41, new DateTime(2021, 9, 18), false),
                new DashboardUser("338", "Brandon", "Gregory", "Luminary Nexus", "16th Floor, Odessa, Texas, United States", 28, new DateTime(2024, 4, 12), true),
                new DashboardUser("342", "Antoine", "Banks", "CrestWave Dynamics", "Suite 82, 44 Shasta Terrace, Las Cruces, United States", 36, new DateTime(2022, 12, 1), false),
                new DashboardUser("349", "Winston", "Tapia", "Horizon Ventures", "Room 1930, 45779 Anhalt Junction, Detroit, Michigan, United States", 52, new DateTime(2020, 5, 29), false),
                new DashboardUser("366", "Carl", "Hudson", "Sapphire Pulse Technologies", "PO Box 54206, 14 Waubesa Street, Greenville, South Carolina, United States", 34, new DateTime(2023, 2, 20), true),
                new DashboardUser("367", "Arielle", "Hood", "EmberLight Industries", "1st Floor, 78 Barby Park, South Dakota, United States", 31, new DateTime(2021, 7, 11), false),
                new DashboardUser("373", "Clara", "Fry", "StellarEdge Ventrues", "Room 1426, 7394 Welch Alley, Huntsville, Alabama, United States", 49, new DateTime(2020, 10, 8), false),
                new DashboardUser("375", "Elliot", "Carroll", "Luminary Nexus", "20th Floor, 11 Eastwood Road, El Paso, Texas, United States", 27, new DateTime(2024, 3, 6), true),
                new DashboardUser("378", "Amelia", "Doe", "CrestWave Dynamics", "Suite 92, 9 Hermina Point, Bakersfield, United States", 39, new DateTime(2022, 1, 24), false),
                new DashboardUser("399", "Grant", "Tapia", "Horizon Ventures", "Apt 687, 47182 Superior Avenue, Kansas City, Missouri", 46, new DateTime(2021, 12, 19), false),
                new DashboardUser("447", "Nora", "Cisneros", "Sapphire Pulse Technologies", "Room 1450, 9819 Rutledge Parkway, Saint Louis, Missouri, United States", 26, new DateTime(2024, 5, 3), true),
                new DashboardUser("453", "Milo", "Lynch", "EmberLight Industries", "18th Floor, 3631 Manitowish Point, Mobile, Alabama, United States", 43, new DateTime(2020, 4, 17), false),
                new DashboardUser("469", "Leah", "Munoz", "StellarEdge Ventrues", "Apt 1145, Kansas, United States", 35, new DateTime(2022, 9, 27), false),
                new DashboardUser("473", "Theo", "Marsh", "Luminary Nexus", "PO Box 54647, 252 Derek Way, Flushing, New York, United States", 58, new DateTime(2021, 5, 14), false),
                new DashboardUser("505", "Iris", "Banks", "CrestWave Dynamics", "20th Floor, 5524 Badeau Pass, Glendale, Arizona, United States", 30, new DateTime(2023, 11, 2), true)
            };
        }

        private sealed class AlphabeticValidationRule : ValidationRule
        {
            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                var text = value as string;
                if (string.IsNullOrEmpty(text))
                {
                    return ValidationResult.ValidResult;
                }

                foreach (var character in text)
                {
                    if (!IsEnglishAlphabetic(character))
                    {
                        return new ValidationResult(false, "Only English alphabetic characters (a-z, A-Z) are allowed.");
                    }
                }

                return ValidationResult.ValidResult;
            }

            private static bool IsEnglishAlphabetic(char character)
            {
                return (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z');
            }
        }

        private sealed class ValidatedTextBoxState
        {
            public string Text { get; set; }
        }

        private sealed class DashboardUser
        {
            public DashboardUser(string imageId, string firstName, string lastName, string company, string address, int age, DateTime dateOfJoining, bool isNewGraduate)
            {
                ImageId = imageId;
                FirstName = firstName;
                LastName = lastName;
                Company = company;
                Address = address;
                Age = age;
                DateOfJoining = dateOfJoining;
                IsNewGraduate = isNewGraduate;
            }

            public string ImageId { get; }
            public string FirstName { get; }
            public string LastName { get; }
            public string Name
            {
                get { return FirstName + " " + LastName; }
            }
            public string Company { get; }
            public string Address { get; }
            public int Age { get; }
            public DateTime DateOfJoining { get; }
            public bool IsNewGraduate { get; }
        }

        private static StackPanel CreateCheckClipboardFormatsExample()
        {
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Check Clipboard Formats",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                try
                {
                    output.Text = "Clipboard contains:" + Environment.NewLine +
                        "  Text: " + Clipboard.ContainsText() + Environment.NewLine +
                        "  Image: " + Clipboard.ContainsImage() + Environment.NewLine +
                        "  File Drop List: " + Clipboard.ContainsFileDropList() + Environment.NewLine +
                        "  Audio: " + Clipboard.ContainsAudio();
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard read failed: " + ex.Message;
                }
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreateClearClipboardExample()
        {
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Clear Clipboard",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                try
                {
                    Clipboard.Clear();
                    output.Text = "Clipboard cleared!";
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard clear failed: " + ex.Message;
                }
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreateCopyImageToClipboardExample()
        {
            var source = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/ModernWpf.Gallery;component/Assets/ControlImages/Clipboard.png")),
                Width = 100,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Copy Image to Clipboard",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var bitmapSource = source.Source as BitmapSource;
                if (bitmapSource == null)
                {
                    output.Text = "Failed to copy image.";
                    return;
                }

                try
                {
                    Clipboard.SetImage(bitmapSource);
                    output.Text = "Image copied to clipboard!";
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard image write failed: " + ex.Message;
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(source);
            stack.Children.Add(button);
            stack.Children.Add(output);
            return stack;
        }

        private static StackPanel CreateCopyTextToClipboardExample()
        {
            var input = new TextBox
            {
                Text = "Hello, Clipboard!",
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };
            AutomationProperties.SetName(input, "Copy To Clipboard TextBox");
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Copy to Clipboard",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                if (string.IsNullOrEmpty(input.Text))
                {
                    output.Text = "Nothing to copy - text box is empty.";
                    return;
                }

                try
                {
                    Clipboard.SetText(input.Text);
                    output.Text = "Copied \"" + input.Text + "\" to clipboard!";
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard write failed: " + ex.Message;
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(input);
            stack.Children.Add(button);
            stack.Children.Add(output);
            return stack;
        }

        private static StackPanel CreatePasteImageFromClipboardExample()
        {
            var image = new Image
            {
                Stretch = Stretch.Uniform,
                Visibility = Visibility.Collapsed
            };
            var imageHost = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Width = 200,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = image
            };
            var output = new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Paste Image from Clipboard",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                try
                {
                    if (Clipboard.ContainsImage())
                    {
                        var bitmapSource = Clipboard.GetImage();
                        image.Source = bitmapSource;
                        image.Visibility = Visibility.Visible;
                        output.Text = "Image pasted! Size: " + bitmapSource.PixelWidth + "x" + bitmapSource.PixelHeight;
                    }
                    else
                    {
                        image.Source = null;
                        image.Visibility = Visibility.Hidden;
                        output.Text = "No image in clipboard.";
                    }
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard image read failed: " + ex.Message;
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(button);
            stack.Children.Add(new TextBlock { Text = "Pasted Image:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            stack.Children.Add(imageHost);
            stack.Children.Add(output);
            return stack;
        }

        private static StackPanel CreatePasteTextFromClipboardExample()
        {
            var output = new TextBox
            {
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 60,
                Width = 300,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(output, "Paste content textbox");
            var button = new Button
            {
                Content = "Paste from Clipboard",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                try
                {
                    output.Text = Clipboard.ContainsText() ? Clipboard.GetText() : "(No text in clipboard)";
                }
                catch (Exception ex)
                {
                    output.Text = "Clipboard read failed: " + ex.Message;
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(button);
            stack.Children.Add(new TextBlock { Text = "Pasted Content:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            stack.Children.Add(output);
            return stack;
        }

        private static Viewbox CreateCanvasViewboxExample()
        {
            var canvas = new Canvas
            {
                Width = 47,
                Height = 123
            };
            canvas.Children.Add(CreateCanvasLogoPath("M0,19H18V84h29v15H0V19Z"));
            canvas.Children.Add(CreateCanvasLogoPath("M46,80H29V15H0V0H46V80Z"));

            return new Viewbox
            {
                Width = 200,
                Height = 200,
                Child = canvas
            };
        }

        private static System.Windows.Shapes.Path CreateCanvasLogoPath(string data)
        {
            var path = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(data)
            };
            path.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "TextFillColorSecondaryBrush");
            return path;
        }

        private static StackPanel CreateCommonMessageBoxExample()
        {
            var output = new TextBlock
            {
                Text = "No common message shown yet",
                TextWrapping = TextWrapping.Wrap
            };
            var row = new WrapPanel
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            row.Children.Add(CreateMessageBoxButton("Information", "The operation completed successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information, output, "Type: Information"));
            row.Children.Add(CreateMessageBoxButton("Error", "An error occurred! The operation could not be completed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error, output, "Type: Error"));
            row.Children.Add(CreateMessageBoxButton("Warning", "This action cannot be undone! Do you want to continue?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning, output, "Type: Warning"));

            var stack = new StackPanel();
            stack.Children.Add(row);
            stack.Children.Add(output);
            return stack;
        }

        private static Grid CreateGridSplitterExample()
        {
            const string sampleText = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.";
            const string sampleText2 = "When an unknown printer took a galley of type and scrambled it to make a type specimen book.";

            var root = new Grid
            {
                Height = 400
            };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var title = new TextBlock
            {
                Text = "Grid Splitter",
                Margin = new Thickness(0, 0, 0, 10)
            };
            title.SetResourceReference(FrameworkElement.StyleProperty, "TitleTextBlockStyle");
            root.Children.Add(title);

            var border = new Border
            {
                BorderThickness = new Thickness(2),
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(4)
            };
            border.SetResourceReference(Border.BorderBrushProperty, "ControlElevationBorderBrush");
            Grid.SetRow(border, 1);

            var grid = new Grid();
            grid.SetResourceReference(Panel.BackgroundProperty, "ControlAltFillColorSecondaryBrush");
            for (var i = 0; i < 4; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                if (i < 3)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
            }
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            AddGridSplitterText(grid, sampleText, 0, 0);
            AddGridSplitterText(grid, sampleText2, 0, 2);
            AddGridSplitterText(grid, sampleText2, 2, 0);
            AddGridSplitterText(grid, sampleText, 2, 2);
            AddGridSplitterText(grid, sampleText, 4, 0);
            AddGridSplitterText(grid, sampleText2, 4, 2);

            var columnSplitter = new GridSplitter
            {
                Width = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ResizeDirection = GridResizeDirection.Columns
            };
            Grid.SetRowSpan(columnSplitter, 5);
            Grid.SetColumn(columnSplitter, 1);
            grid.Children.Add(columnSplitter);

            AddRowSplitter(grid, 1, 3);
            AddRowSplitter(grid, 3, 1);

            border.Child = grid;
            root.Children.Add(border);
            return root;
        }

        private static GroupBox CreateUserInformationGroupBox()
        {
            var nameTextBox = new TextBox { Width = 280, Margin = new Thickness(10, 0, 0, 20) };
            AutomationProperties.SetName(nameTextBox, "Name Field");
            var genderTextBox = new TextBox { Width = 280, Margin = new Thickness(10, 0, 0, 20) };
            AutomationProperties.SetName(genderTextBox, "Gender Field");

            var stack = new StackPanel();
            var nameRow = new StackPanel { Orientation = Orientation.Horizontal };
            nameRow.Children.Add(new TextBlock { Width = 100, Text = "Name:" });
            nameRow.Children.Add(nameTextBox);
            var genderRow = new StackPanel { Orientation = Orientation.Horizontal };
            genderRow.Children.Add(new TextBlock { Width = 100, Text = "Gender:", Margin = new Thickness(0, 10, 0, 0) });
            genderRow.Children.Add(genderTextBox);
            stack.Children.Add(nameRow);
            stack.Children.Add(genderRow);
            stack.Children.Add(new Button
            {
                Content = "Submit",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            });

            return new GroupBox
            {
                Header = "User Information",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 400,
                Content = stack
            };
        }

        private static StackPanel CreateCustomMessageBoxExample()
        {
            var output = new TextBlock
            {
                Text = "No message shown yet",
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Custom MessageBox",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var result = MessageBox.Show(
                    Window.GetWindow(button),
                    "This is a detailed description of what happened or what action is needed.",
                    "Custom Title");
                output.Text = "Result: " + result;
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreateMessageBoxButtonsExample()
        {
            var output = new TextBlock
            {
                Text = "No button clicked yet",
                TextWrapping = TextWrapping.Wrap
            };
            var comboBox = new ComboBox
            {
                MinWidth = 150,
                SelectedIndex = 0
            };
            AutomationProperties.SetName(comboBox, "MessageBox Button Selector");
            foreach (var item in new[] { "OK", "OK/Cancel", "Abort/Retry/Ignore", "Yes/No/Cancel", "Yes/No", "Retry/Cancel", "Cancel/Try/Continue" })
            {
                comboBox.Items.Add(item);
            }

            var button = new Button
            {
                Content = "Show MessageBox",
                Margin = new Thickness(0, 0, 0, 10)
            };
            AutomationProperties.SetName(button, "MessageBox with Different Buttons");
            button.Click += delegate
            {
                var buttonName = comboBox.SelectedItem as string ?? "OK";
                var messageBoxButton = GetMessageBoxButton(comboBox.SelectedIndex);
                var result = MessageBox.Show(
                    Window.GetWindow(button),
                    "This MessageBox has " + buttonName + " button(s).",
                    buttonName + " Button(s)",
                    messageBoxButton);
                output.Text = "Result: " + result;
            };

            var left = new StackPanel();
            left.Children.Add(button);
            left.Children.Add(output);
            var right = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0)
            };
            right.Children.Add(new TextBlock { Text = "Button Type:", Margin = new Thickness(0, 0, 0, 5) });
            right.Children.Add(comboBox);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(right, 1);
            grid.Children.Add(left);
            grid.Children.Add(right);

            var stack = new StackPanel();
            stack.Children.Add(grid);
            return stack;
        }

        private static StackPanel CreateMessageBoxImagesExample()
        {
            var output = new TextBlock
            {
                Text = "No image example shown yet",
                TextWrapping = TextWrapping.Wrap
            };
            var comboBox = new ComboBox
            {
                MinWidth = 150,
                SelectedIndex = 0
            };
            AutomationProperties.SetName(comboBox, "MessageBox Image Selector");
            foreach (var item in new[] { "None", "Error", "Question", "Warning", "Information" })
            {
                comboBox.Items.Add(item);
            }

            var button = new Button
            {
                Content = "Show MessageBox",
                Margin = new Thickness(0, 0, 0, 10)
            };
            AutomationProperties.SetName(button, "MessageBox with different images");
            button.Click += delegate
            {
                var imageName = comboBox.SelectedItem as string ?? "None";
                var result = MessageBox.Show(
                    Window.GetWindow(button),
                    "This MessageBox displays the " + imageName + " icon.",
                    imageName + " Icon",
                    MessageBoxButton.OK,
                    GetMessageBoxImage(comboBox.SelectedIndex));
                output.Text = "Result: " + result;
            };

            var left = new StackPanel();
            left.Children.Add(button);
            left.Children.Add(output);
            var right = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0)
            };
            right.Children.Add(new TextBlock { Text = "Icon Type:", Margin = new Thickness(0, 0, 0, 5) });
            right.Children.Add(comboBox);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(right, 1);
            grid.Children.Add(left);
            grid.Children.Add(right);

            var stack = new StackPanel();
            stack.Children.Add(grid);
            return stack;
        }

        private static StackPanel CreateOpenFrameWindowButton()
        {
            var button = new Button
            {
                Content = "Open window to view Frame",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            button.Click += delegate
            {
                var frame = new Frame
                {
                    NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Visible,
                    Content = CreateNavigationPageContent("Frame page")
                };
                var window = new Window
                {
                    Title = "Frame sample",
                    Width = 640,
                    Height = 420,
                    Content = frame
                };
                var owner = Window.GetWindow(button);
                if (owner != null)
                {
                    window.Owner = owner;
                }
                window.Show();
            };

            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(button);
            return stack;
        }

        private static StackPanel CreateOpenNavigationWindowButton()
        {
            var button = new Button
            {
                Content = "Open window to view NavigationWindow",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            button.Click += delegate
            {
                var window = new System.Windows.Navigation.NavigationWindow
                {
                    Title = "NavigationWindow sample",
                    Width = 800,
                    Height = 450,
                    Content = CreateNavigationPageContent("NavigationWindow page")
                };
                var owner = Window.GetWindow(button);
                if (owner != null)
                {
                    window.Owner = owner;
                }
                window.Show();
            };

            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(button);
            return stack;
        }

        private static StackPanel CreatePickFolderExample()
        {
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Pick a folder",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                output.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreatePickMultipleFilesExample()
        {
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Pick multiple files",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Select multiple files",
                    Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt",
                    Multiselect = true
                };
                if (ShowDialog(dialog, button) == true)
                {
                    output.Text = string.Join(Environment.NewLine, dialog.FileNames);
                }
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreatePickSingleFileExample()
        {
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Pick a single file",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Select a file",
                    Filter = "All files (*.*)|*.*|Text files (*.txt)|*.txt",
                    Multiselect = false
                };
                if (ShowDialog(dialog, button) == true)
                {
                    output.Text = dialog.FileName;
                }
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreateResizeGripWindowExample()
        {
            var button = new Button
            {
                Content = "Open window with resize grip",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            button.Click += delegate
            {
                var window = new Window
                {
                    Width = 500,
                    Height = 300,
                    ResizeMode = ResizeMode.CanResizeWithGrip,
                    Content = new TextBlock
                    {
                        Text = "ResizeGrip is present at the bottom right corner of the window",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 16
                    }
                };
                var owner = Window.GetWindow(button);
                if (owner != null)
                {
                    window.Owner = owner;
                }
                window.Show();
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            stack.Children.Add(new TextBlock
            {
                Margin = new Thickness(0, 10, 0, 40),
                TextWrapping = TextWrapping.Wrap,
                Text = "Resize grip control is only used along with a Window in WPF.\nIt can not be used in other places as such.\nTo see the resize grip in action click the button."
            });
            stack.Children.Add(button);
            return stack;
        }

        private static StackPanel CreateSaveFileExample()
        {
            var textBox = new TextBox
            {
                Text = "ModernWpf Gallery file content",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 80,
                Margin = new Thickness(0, 0, 0, 10),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            AutomationProperties.SetName(textBox, "Save File Text Box");
            var output = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Save a file",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Save file",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt"
                };
                if (ShowDialog(dialog, button) == true)
                {
                    File.WriteAllText(dialog.FileName, textBox.Text ?? string.Empty);
                    output.Text = dialog.FileName;
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(textBox);
            stack.Children.Add(button);
            stack.Children.Add(output);
            return stack;
        }

        private static StackPanel CreateSimpleMessageBoxExample()
        {
            var output = new TextBlock
            {
                Text = "No message shown yet",
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Simple MessageBox",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var result = MessageBox.Show(Window.GetWindow(button), "This is a simple message box!");
                output.Text = "Result: " + result;
            };
            return CreateButtonResultExample(button, output);
        }

        private static StackPanel CreateStandardMenuExample()
        {
            var output = new TextBlock();
            var menu = new Menu();

            var file = new MenuItem { Header = "File" };
            file.Items.Add(CreateMenuItem("New", output));
            file.Items.Add(CreateMenuItem("New window", output));
            file.Items.Add(CreateMenuItem("Open", output));
            file.Items.Add(CreateMenuItem("Save", output));
            file.Items.Add(CreateMenuItem("Save As", output));
            file.Items.Add(new Separator());
            file.Items.Add(CreateMenuItem("Exit", output));

            var edit = new MenuItem { Header = "Edit" };
            edit.Items.Add(CreateMenuItem("Undo", output));
            edit.Items.Add(new Separator());
            edit.Items.Add(CreateMenuItem("Cut", output));
            edit.Items.Add(CreateMenuItem("Copy", output));
            edit.Items.Add(CreateMenuItem("Paste", output));
            edit.Items.Add(new MenuItem { IsEnabled = false });
            edit.Items.Add(new Separator());
            edit.Items.Add(CreateMenuItem("Search with browser", output));
            edit.Items.Add(CreateMenuItem("Find", output));
            edit.Items.Add(CreateMenuItem("Find next", output));
            edit.Items.Add(new Separator());
            edit.Items.Add(CreateMenuItem("Select All", output));

            menu.Items.Add(file);
            menu.Items.Add(edit);
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateGlyphMenuItem("Bold", "\uE8DD", output));
            menu.Items.Add(CreateGlyphMenuItem("Italic", "\uE8DB", output));
            menu.Items.Add(CreateGlyphMenuItem("Underlined", "\uE8DC", output));

            var stack = new StackPanel();
            stack.Children.Add(output);
            stack.Children.Add(menu);
            return stack;
        }

        private static TabControl CreateStandardTabControl()
        {
            var tabControl = new TabControl
            {
                Margin = new Thickness(0, 8, 0, 0)
            };
            var hello = CreateTabItem("Hello", "World");
            AutomationProperties.SetName(hello, "Hello Tab");
            var cake = CreateTabItem("The cake", "Is a lie.");
            AutomationProperties.SetName(cake, "The cake Tab");
            cake.IsSelected = true;
            tabControl.Items.Add(hello);
            tabControl.Items.Add(cake);
            return tabControl;
        }

        private static CheckBox CreateCheckBox(string content, bool isThreeState, bool? isChecked, string automationName)
        {
            var checkBox = new CheckBox
            {
                Content = content,
                IsThreeState = isThreeState,
                IsChecked = isChecked
            };
            AutomationProperties.SetName(checkBox, automationName);
            return checkBox;
        }

        private static GallerySamplePanel CreateSampleRoot(string uniqueId, UIElement content)
        {
            var panel = new GallerySamplePanel();
            GalleryAutomation.WithAutomationId(panel, GalleryAutomation.SampleRootId(uniqueId));
            panel.Children.Add(content);
            return panel;
        }

        private static ComboBox CreateInlineComboBoxExample()
        {
            var comboBox = CreateGalleryComboBox("Sample defined inline");
            GalleryAutomation.WithAutomationId(comboBox, GalleryAutomation.SampleElementId("ComboBox", "ComboBox"));
            comboBox.Items.Add(new ComboBoxItem { Content = "Blue" });
            comboBox.Items.Add(new ComboBoxItem { Content = "Green" });
            comboBox.Items.Add(new ComboBoxItem { Content = "Red" });
            comboBox.Items.Add(new ComboBoxItem { Content = "Yellow" });
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private static ComboBox CreateFontFamilyComboBoxExample()
        {
            var comboBox = CreateGalleryComboBox("Sample item source set");
            comboBox.ItemsSource = CreateComboBoxFontFamilies();
            comboBox.ItemTemplate = CreateFontFamilyComboBoxItemTemplate();
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private static ComboBox CreateEditableFontSizeComboBoxExample()
        {
            var comboBox = CreateGalleryComboBox("Editable");
            comboBox.ItemsSource = CreateComboBoxFontSizes();
            comboBox.IsEditable = true;
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private static ComboBox CreateGalleryComboBox(string automationName)
        {
            var comboBox = new ComboBox
            {
                MinWidth = 200,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AutomationProperties.SetName(comboBox, automationName);
            return comboBox;
        }

        private static string[] CreateComboBoxFontFamilies()
        {
            return new[]
            {
                "Arial",
                "Comic Sans MS",
                "Segoe UI",
                "Times New Roman"
            };
        }

        private static int[] CreateComboBoxFontSizes()
        {
            return new[]
            {
                8,
                9,
                10,
                11,
                12,
                14,
                16,
                18,
                20,
                24,
                28,
                36,
                48,
                72
            };
        }

        private static DataTemplate CreateFontFamilyComboBoxItemTemplate()
        {
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));
            textBlock.SetBinding(TextBlock.FontFamilyProperty, new Binding());
            textBlock.SetBinding(TextBlock.TextProperty, new Binding());
            return new DataTemplate { VisualTree = textBlock };
        }

        private static void AddGridSplitterText(Grid grid, string text, int row, int column)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(8)
            };
            Grid.SetRow(textBlock, row);
            Grid.SetColumn(textBlock, column);
            grid.Children.Add(textBlock);
        }

        private static void AddRowSplitter(Grid grid, int row, int columnSpan)
        {
            var splitter = new GridSplitter
            {
                Height = 8,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ResizeDirection = GridResizeDirection.Rows
            };
            Grid.SetRow(splitter, row);
            Grid.SetColumnSpan(splitter, columnSpan);
            grid.Children.Add(splitter);
        }

        private static StackPanel CreateThreeStateCheckBoxGroup()
        {
            var selectAll = new CheckBox
            {
                Content = "Select all",
                IsThreeState = true
            };
            var options = new[]
            {
                new CheckBox { Content = "Option 1", Margin = new Thickness(24, 0, 0, 0) },
                new CheckBox { Content = "Option 2", Margin = new Thickness(24, 0, 0, 0) },
                new CheckBox { Content = "Option 3", Margin = new Thickness(24, 0, 0, 0) }
            };
            selectAll.Checked += delegate
            {
                foreach (var option in options)
                {
                    option.IsChecked = true;
                }
            };
            selectAll.Unchecked += delegate
            {
                foreach (var option in options)
                {
                    option.IsChecked = false;
                }
            };

            var stack = new StackPanel();
            stack.Children.Add(selectAll);
            foreach (var option in options)
            {
                stack.Children.Add(option);
            }

            return stack;
        }

        private static StackPanel CreateCustomDefaultMessageBoxExample()
        {
            var output = new TextBlock
            {
                Text = "No selection made",
                TextWrapping = TextWrapping.Wrap
            };
            var button = new Button
            {
                Content = "Show with 'No' as default",
                Margin = new Thickness(0, 0, 0, 10)
            };
            button.Click += delegate
            {
                var result = MessageBox.Show(
                    Window.GetWindow(button),
                    "Do you want to save changes? Press Enter to select the default 'No' button.",
                    "Save Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);
                output.Text = "User selected: " + result;
            };
            return CreateButtonResultExample(button, output);
        }

        private static Button CreateMessageBoxButton(string content, string message, string title, MessageBoxButton buttons, MessageBoxImage image, TextBlock output, string resultPrefix)
        {
            var button = new Button
            {
                Content = content,
                Margin = new Thickness(0, 0, 5, 0)
            };
            button.Click += delegate
            {
                var result = MessageBox.Show(Window.GetWindow(button), message, title, buttons, image);
                output.Text = resultPrefix + " | Result: " + result;
            };
            return button;
        }

        private static MenuItem CreateGlyphMenuItem(string name, string glyph, TextBlock output)
        {
            var item = new MenuItem
            {
                Header = new TextBlock
                {
                    Text = glyph,
                    Focusable = false,
                    FontSize = 12
                },
                Tag = name
            };
            ((TextBlock)item.Header).SetResourceReference(TextBlock.FontFamilyProperty, "SymbolThemeFontFamily");
            AutomationProperties.SetName(item, name);
            item.Click += delegate { output.Text = "Selected " + name; };
            return item;
        }

        private static MenuItem CreateMenuItem(string header, TextBlock output)
        {
            var item = new MenuItem { Header = header };
            item.Click += delegate { output.Text = "Selected " + header; };
            return item;
        }

        private static Border CreateGridCell(string text, Brush background, int row, int column)
        {
            var border = new Border
            {
                Background = background,
                Margin = new Thickness(4),
                Child = new TextBlock
                {
                    Text = text,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                }
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            return border;
        }

        private static Grid CreateCustomGridExample()
        {
            var grid = new Grid
            {
                Height = 300
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(CreateResourceGridBorder("Row 0, Column 0", 0, 0, 1, 1, "ControlFillColorDefaultBrush"));
            grid.Children.Add(CreateResourceGridBorder("Row 0, Column 1 (2x width)", 0, 1, 1, 1, "ControlFillColorDefaultBrush"));
            grid.Children.Add(CreateResourceGridBorder("Row 0, Column 2", 0, 2, 1, 1, "ControlFillColorDefaultBrush"));
            grid.Children.Add(CreateResourceGridBorder("Row 1, Spans all columns", 1, 0, 1, 3, "ControlFillColorSecondaryBrush"));
            grid.Children.Add(CreateResourceGridBorder("Row 2, Spans 2 columns", 2, 0, 1, 2, "ControlFillColorDefaultBrush"));
            grid.Children.Add(CreateResourceGridBorder("Row 2, Column 2", 2, 2, 1, 1, "ControlFillColorDefaultBrush"));
            return grid;
        }

        private static Border CreateResourceGridBorder(string text, int row, int column, int rowSpan, int columnSpan, string backgroundResourceKey, bool centerContent = false)
        {
            var textBlock = new TextBlock { Text = text };
            if (centerContent)
            {
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
            }

            var border = new Border
            {
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Child = textBlock
            };
            border.SetResourceReference(Border.BackgroundProperty, backgroundResourceKey);
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            if (rowSpan > 1)
            {
                Grid.SetRowSpan(border, rowSpan);
            }
            if (columnSpan > 1)
            {
                Grid.SetColumnSpan(border, columnSpan);
            }
            return border;
        }

        private static Grid CreateShorthandGridExample()
        {
            var grid = new Grid
            {
                Height = 300
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(CreateResourceGridBorder("Header (100px)", 0, 0, 1, 1, "ControlFillColorDefaultBrush"));
            grid.Children.Add(CreateResourceGridBorder("Title (2*)", 0, 1, 1, 1, "ControlFillColorSecondaryBrush"));
            grid.Children.Add(CreateResourceGridBorder("Actions (*)", 0, 2, 1, 1, "ControlFillColorDefaultBrush"));
            grid.Children.Add(CreateResourceGridBorder("Main Content Area (fills available space)", 1, 0, 1, 3, "ControlAltFillColorSecondaryBrush", true));
            grid.Children.Add(CreateResourceGridBorder("Footer (Auto height, spans all columns)", 2, 0, 1, 3, "ControlFillColorDefaultBrush"));
            return grid;
        }

        private static Grid CreateSimpleGridExample()
        {
            var grid = new Grid
            {
                Height = 250,
                ShowGridLines = true
            };
            for (var i = 0; i < 3; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            var cell = 1;
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var text = new TextBlock
                    {
                        Text = "Cell " + cell++
                    };
                    Grid.SetRow(text, row);
                    Grid.SetColumn(text, column);
                    grid.Children.Add(text);
                }
            }

            return grid;
        }

        private static MessageBoxButton GetMessageBoxButton(int index)
        {
            switch (index)
            {
                case 1:
                    return MessageBoxButton.OKCancel;
                case 3:
                    return MessageBoxButton.YesNoCancel;
                case 4:
                    return MessageBoxButton.YesNo;
                default:
                    return MessageBoxButton.OK;
            }
        }

        private static MessageBoxImage GetMessageBoxImage(int index)
        {
            switch (index)
            {
                case 1:
                    return MessageBoxImage.Error;
                case 2:
                    return MessageBoxImage.Question;
                case 3:
                    return MessageBoxImage.Warning;
                case 4:
                    return MessageBoxImage.Information;
                default:
                    return MessageBoxImage.None;
            }
        }

        private static Page CreateNavigationPageContent(string title)
        {
            return new Page
            {
                Content = new Border
                {
                    Padding = new Thickness(18),
                    Background = Brushes.LightGray,
                    Child = new TextBlock
                    {
                        Text = title,
                        FontSize = 22,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }

        private static string[] CreateFontNames()
        {
            return new[]
            {
                "Arial",
                "Comic Sans MS",
                "Courier New",
                "Segoe UI",
                "Times New Roman"
            };
        }

        private static ListBox CreateInlineListBox()
        {
            var listBox = new ListBox
            {
                SelectedIndex = 0
            };
            AutomationProperties.SetName(listBox, "Color ListBox");
            listBox.Items.Add(new ListBoxItem { Content = "Blue" });
            listBox.Items.Add(new ListBoxItem { Content = "Green" });
            listBox.Items.Add(new ListBoxItem { Content = "Red" });
            listBox.Items.Add(new ListBoxItem { Content = "Yellow" });
            return listBox;
        }

        private static ListBox CreateFontListBox()
        {
            var listBox = new ListBox
            {
                Height = 164,
                ItemsSource = CreateFontNames(),
                SelectedIndex = 2
            };
            AutomationProperties.SetName(listBox, "Font ListBox");
            return listBox;
        }

        private static ListView CreateBasicListView()
        {
            var listView = new ListView
            {
                Height = 200,
                ItemsSource = CreatePeople(),
                SelectedIndex = 2,
                SelectionMode = SelectionMode.Single,
                ItemTemplate = CreatePersonTemplate()
            };
            AutomationProperties.SetName(listView, "Basic ListView");
            return listView;
        }

        private static DataTemplate CreatePersonTemplate()
        {
            var name = new FrameworkElementFactory(typeof(TextBlock));
            name.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            name.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 4, 8, 4));
            return new DataTemplate { VisualTree = name };
        }

        private static DataTemplate CreateDetailedPersonTemplate()
        {
            var row = new FrameworkElementFactory(typeof(StackPanel));
            row.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            row.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 8, 0));

            var ellipse = new FrameworkElementFactory(typeof(Ellipse));
            ellipse.SetValue(FrameworkElement.WidthProperty, 32.0);
            ellipse.SetValue(FrameworkElement.HeightProperty, 32.0);
            ellipse.SetValue(FrameworkElement.MarginProperty, new Thickness(6));
            ellipse.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            ellipse.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            ellipse.SetResourceReference(Shape.FillProperty, "SystemAccentColorPrimaryBrush");

            var textStack = new FrameworkElementFactory(typeof(StackPanel));
            textStack.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            var name = new FrameworkElementFactory(typeof(TextBlock));
            name.SetValue(FrameworkElement.MarginProperty, new Thickness(12, 6, 0, 0));
            name.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
            name.SetBinding(TextBlock.TextProperty, new Binding("Name"));

            var company = new FrameworkElementFactory(typeof(TextBlock));
            company.SetValue(FrameworkElement.MarginProperty, new Thickness(12, 0, 0, 6));
            company.SetValue(UIElement.OpacityProperty, 0.7);
            company.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            company.SetBinding(TextBlock.TextProperty, new Binding("Company"));

            textStack.AppendChild(name);
            textStack.AppendChild(company);
            row.AppendChild(ellipse);
            row.AppendChild(textStack);

            return new DataTemplate { VisualTree = row };
        }

        private static ListView CreateGridViewListView()
        {
            var listView = new ListView
            {
                Height = 280,
                ItemsSource = CreatePeople()
            };
            AutomationProperties.SetName(listView, "ListView with GridView");
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn { Header = "First Name", DisplayMemberBinding = new Binding("FirstName"), Width = 150 });
            gridView.Columns.Add(new GridViewColumn { Header = "Last Name", DisplayMemberBinding = new Binding("LastName"), Width = 150 });
            gridView.Columns.Add(new GridViewColumn { Header = "Company", DisplayMemberBinding = new Binding("Company"), Width = 200 });
            listView.View = gridView;
            return listView;
        }

        private static FrameworkElement CreateRadioButtonGroup(string groupName, FlowDirection flowDirection, bool includeDisableControl)
        {
            var stack = new StackPanel();
            KeyboardNavigation.SetTabNavigation(stack, KeyboardNavigationMode.Once);
            KeyboardNavigation.SetDirectionalNavigation(stack, KeyboardNavigationMode.Cycle);
            var options = new[]
            {
                new RadioButton { GroupName = groupName, Content = "Option 1", FlowDirection = flowDirection, IsChecked = true },
                new RadioButton { GroupName = groupName, Content = "Option 2", FlowDirection = flowDirection },
                new RadioButton { GroupName = groupName, Content = "Option 3", FlowDirection = flowDirection }
            };
            for (var i = 0; i < options.Length; i++)
            {
                AutomationProperties.SetName(options[i], (flowDirection == FlowDirection.RightToLeft ? "Left Flow" : "Default") + " Radio Option " + (i + 1));
                stack.Children.Add(options[i]);
            }

            if (!includeDisableControl)
            {
                return stack;
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var disable = new CheckBox
            {
                Content = "Disable RadioButton's"
            };
            disable.Checked += delegate
            {
                foreach (var option in options)
                {
                    option.IsEnabled = false;
                }
            };
            disable.Unchecked += delegate
            {
                foreach (var option in options)
                {
                    option.IsEnabled = true;
                }
            };
            Grid.SetColumn(disable, 1);
            grid.Children.Add(stack);
            grid.Children.Add(disable);
            return grid;
        }

        private static Grid CreateSelectionListView()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var listView = new ListView
            {
                Height = 200,
                ItemsSource = CreatePeople(),
                SelectedIndex = 1,
                SelectionMode = SelectionMode.Single,
                ItemTemplate = CreateDetailedPersonTemplate()
            };
            AutomationProperties.SetName(listView, "ListView with Selection Support.");

            var controls = new StackPanel
            {
                MinWidth = 120,
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            var label = new Label
            {
                Content = "Selection mode",
                Opacity = 0.7
            };
            label.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush");
            var comboBox = new ComboBox();
            AutomationProperties.SetName(comboBox, "Selection Mode");
            comboBox.Items.Add(new ComboBoxItem { Content = "Single" });
            comboBox.Items.Add(new ComboBoxItem { Content = "Multiple" });
            comboBox.Items.Add(new ComboBoxItem { Content = "Extended" });
            comboBox.SelectedIndex = 0;
            comboBox.SelectionChanged += delegate
            {
                listView.SelectionMode = (SelectionMode)comboBox.SelectedIndex;
            };
            label.Target = comboBox;
            controls.Children.Add(label);
            controls.Children.Add(comboBox);

            Grid.SetColumn(controls, 1);
            grid.Children.Add(listView);
            grid.Children.Add(controls);
            return grid;
        }

        private static Grid CreateSliderWithOutput(double minimum, double maximum, double value, double tickFrequency, TickPlacement tickPlacement, Orientation orientation, string automationName)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var slider = new Slider
            {
                Width = 200,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Minimum = minimum,
                Maximum = maximum,
                Value = value,
                IsSnapToTickEnabled = true,
                TickFrequency = tickFrequency,
                TickPlacement = tickPlacement,
                Orientation = orientation
            };
            if (orientation == Orientation.Vertical)
            {
                slider.Height = 200;
            }
            AutomationProperties.SetName(slider, automationName);

            var outputValue = new TextBlock();
            outputValue.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            Action updateOutput = delegate { outputValue.Text = slider.Value.ToString("0"); };
            slider.ValueChanged += delegate { updateOutput(); };
            updateOutput();

            var outputStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            var outputLabel = new TextBlock { Text = "Output:" };
            outputLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorPrimaryBrush");
            outputStack.Children.Add(outputLabel);
            outputStack.Children.Add(outputValue);
            Grid.SetColumn(outputStack, 1);

            grid.Children.Add(slider);
            grid.Children.Add(outputStack);
            return grid;
        }

        private static StackPanel CreateStackPanel(Orientation orientation)
        {
            var stack = new StackPanel
            {
                Orientation = orientation
            };
            stack.Children.Add(CreateStackPanelRectangle(Brushes.CornflowerBlue));
            stack.Children.Add(CreateStackPanelRectangle(Brushes.LightCoral));
            stack.Children.Add(CreateStackPanelRectangle(Brushes.MediumSeaGreen));
            return stack;
        }

        private static Rectangle CreateStackPanelRectangle(Brush fill)
        {
            return new Rectangle
            {
                Width = 100,
                Height = 30,
                Fill = fill,
                Margin = new Thickness(5)
            };
        }

        private static bool? ShowDialog(FileDialog dialog, FrameworkElement ownerElement)
        {
            var owner = Window.GetWindow(ownerElement);
            return owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        }

        private static void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private static TabItem CreateTabItem(string header, string text)
        {
            var headerStack = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            headerStack.Children.Add(new TextBlock { Text = header });
            return new TabItem
            {
                Header = headerStack,
                Content = new Grid
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Margin = new Thickness(12),
                            Text = text
                        }
                    }
                }
            };
        }

        private static TreeView CreateTreeView()
        {
            var treeView = new TreeView
            {
                AllowDrop = true
            };
            AutomationProperties.SetName(treeView, "Sample TreeView");
            ScrollViewer.SetCanContentScroll(treeView, false);

            var workDocuments = new TreeViewItem
            {
                Header = "Work Documents",
                IsExpanded = true,
                IsSelected = true
            };
            workDocuments.Items.Add(new TreeViewItem { Header = "Feature Schedule" });
            workDocuments.Items.Add(new TreeViewItem { Header = "Overall Project Plan" });

            var homeRemodel = new TreeViewItem { Header = "Home Remodel" };
            homeRemodel.Items.Add(new TreeViewItem { Header = "Paint Color Scheme" });
            homeRemodel.Items.Add(new TreeViewItem { Header = "Flooring Woodgrain Type" });
            homeRemodel.Items.Add(new TreeViewItem { Header = "Kitchen Cabinet Style" });

            var personalDocuments = new TreeViewItem { Header = "Personal Documents" };
            personalDocuments.Items.Add(new TreeViewItem { Header = "Contractor contact info" });
            personalDocuments.Items.Add(homeRemodel);

            treeView.Items.Add(workDocuments);
            treeView.Items.Add(personalDocuments);
            return treeView;
        }

        private static object[] CreatePeople()
        {
            var random = new Random(0);
            var people = new object[50];
            var names = new[]
            {
                "John",
                "Winston",
                "Adrianna",
                "Spencer",
                "Phoebe",
                "Lucas",
                "Carl",
                "Marissa",
                "Brandon",
                "Antoine",
                "Arielle",
                "Arielle",
                "Jamie",
                "Alexander"
            };
            var surnames = new[]
            {
                "Doe",
                "Tapia",
                "Cisneros",
                "Lynch",
                "Munoz",
                "Marsh",
                "Hudson",
                "Bartlett",
                "Gregory",
                "Banks",
                "Hood",
                "Fry",
                "Carroll"
            };
            var companies = new[]
            {
                "Luminary Nexus",
                "CrestWave Dynamics",
                "Horizon Ventures",
                "Sapphire Pulse Technologies",
                "EmberLight Industries",
                "StellarEdge Ventrues",
                "Elysium Crest Holdings"
            };

            for (var i = 0; i < people.Length; i++)
            {
                var firstName = names[random.Next(0, names.Length)];
                var lastName = surnames[random.Next(0, surnames.Length)];
                people[i] = new
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Name = firstName + " " + lastName,
                    Company = companies[random.Next(0, companies.Length)]
                };
            }

            return people;
        }

        private static object[] CreateProducts()
        {
            var random = new Random(0);
            var products = new object[50];
            var adjectives = new[] { "Red", "Blueberry" };
            var names = new[] { "Marmalade", "Dumplings", "Soup" };

            for (var i = 0; i < products.Length; i++)
            {
                products[i] = new
                {
                    ProductId = i,
                    ProductCode = i,
                    ProductName = adjectives[random.Next(0, adjectives.Length)] + " " + names[random.Next(0, names.Length)],
                    UnitPrice = Math.Round(random.NextDouble() * 20.0, 3),
                    UnitsInStock = random.Next(0, 100)
                };
            }

            return products;
        }
    }
}
