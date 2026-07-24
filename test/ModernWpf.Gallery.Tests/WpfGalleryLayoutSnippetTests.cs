using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Pages.WpfGallery.Layout;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryLayoutSnippetTests
    {
        [TestMethod]
        public void LayoutControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new BorderPage(new BorderPageViewModel()),
                    new ExpectedExample(
                        "A basic Border",
                        Lines(
                            "<Border BorderBrush=\"Gray\" BorderThickness=\"2\" Padding=\"10\">",
                            "<TextBlock Text=\"Content inside a Border\" />",
                            "</Border>")),
                    new ExpectedExample(
                        "A Border with rounded corners",
                        Lines(
                            "<Border BorderBrush=\"CornflowerBlue\" BorderThickness=\"2\" CornerRadius=\"10\" Padding=\"15\" Background=\"LightBlue\">",
                            "<TextBlock Text=\"Rounded Border\" />",
                            "</Border>")),
                    new ExpectedExample(
                        "A Border with different thickness on each side",
                        Lines(
                            "<Border BorderBrush=\"DarkSlateGray\" BorderThickness=\"1,2,4,8\" Padding=\"10\">",
                            "<TextBlock Text=\"Different border thickness\" />",
                            "</Border>")));

                AssertExamples(
                    new ExpanderPage(new ExpanderPageViewModel()),
                    new ExpectedExample(
                        "An Expander with text in the header and content areas",
                        "<Expander Header=\"This text is in the header\" Content=\"This is in the content\" />"));

                AssertExamples(
                    new GridPage(new GridPageViewModel()),
                    new ExpectedExample(
                        "A simple 3x3 Grid",
                        Lines(
                            "<Grid ShowGridLines=\"True\">",
                            "<Grid.RowDefinitions>",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"*\" />",
                            "</Grid.RowDefinitions>",
                            "<Grid.ColumnDefinitions>",
                            "<ColumnDefinition Width=\"*\" />",
                            "<ColumnDefinition Width=\"*\" />",
                            "<ColumnDefinition Width=\"*\" />",
                            "</Grid.ColumnDefinitions>",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"0\" Text=\"Cell 1\" />",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"1\" Text=\"Cell 2\" />",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"2\" Text=\"Cell 3\" />",
                            "<TextBlock Grid.Row=\"1\" Grid.Column=\"0\" Text=\"Cell 4\" />",
                            "<TextBlock Grid.Row=\"1\" Grid.Column=\"1\" Text=\"Cell 5\" />",
                            "<TextBlock Grid.Row=\"1\" Grid.Column=\"2\" Text=\"Cell 6\" />",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"0\" Text=\"Cell 7\" />",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"1\" Text=\"Cell 8\" />",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"2\" Text=\"Cell 9\" />",
                            "</Grid>")),
                    new ExpectedExample(
                        "A Grid with custom sizing and spanning",
                        Lines(
                            "<Grid>",
                            "<Grid.RowDefinitions>",
                            "<RowDefinition Height=\"Auto\" />",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"Auto\" />",
                            "</Grid.RowDefinitions>",
                            "<Grid.ColumnDefinitions>",
                            "<ColumnDefinition Width=\"*\" />",
                            "<ColumnDefinition Width=\"2*\" />",
                            "<ColumnDefinition Width=\"*\" />",
                            "</Grid.ColumnDefinitions>",
                            "<Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Row 0, Column 0\" />",
                            "</Border>",
                            "<Border Grid.Row=\"0\" Grid.Column=\"1\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Row 0, Column 1 (2x width)\" />",
                            "</Border>",
                            "<Border Grid.Row=\"0\" Grid.Column=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Row 0, Column 2\" />",
                            "</Border>",
                            "<Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Row 1, Spans all columns\" />",
                            "</Border>",
                            "<Border Grid.Row=\"2\" Grid.Column=\"0\" Grid.ColumnSpan=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Row 2, Spans 2 columns\" />",
                            "</Border>",
                            "<Border Grid.Row=\"2\" Grid.Column=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Row 2, Column 2\" />",
                            "</Border>",
                            "</Grid>")),
                    new ExpectedExample(
                        "Grid using XAML shorthand syntax",
                        Lines(
                            "<Grid RowDefinitions=\"Auto,*,Auto\" ColumnDefinitions=\"100,2*,*\">",
                            "<Border Grid.Row=\"0\" Grid.Column=\"0\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Header (100px)\" />",
                            "</Border>",
                            "<Border Grid.Row=\"0\" Grid.Column=\"1\" Background=\"{DynamicResource ControlFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Title (2*)\" />",
                            "</Border>",
                            "<Border Grid.Row=\"0\" Grid.Column=\"2\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Actions (*)\" />",
                            "</Border>",
                            "<Border Grid.Row=\"1\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlAltFillColorSecondaryBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Main Content Area (fills available space)\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Center\" />",
                            "</Border>",
                            "<Border Grid.Row=\"2\" Grid.Column=\"0\" Grid.ColumnSpan=\"3\" Background=\"{DynamicResource ControlFillColorDefaultBrush}\" Margin=\"5\" Padding=\"10\">",
                            "<TextBlock Text=\"Footer (Auto height, spans all columns)\" />",
                            "</Border>",
                            "</Grid>")));

                AssertExamples(
                    new GridSplitterPage(new GridSplitterPageViewModel()),
                    new ExpectedExample(
                        "A GridSplitter",
                        Lines(
                            "<Grid Height=\"400\">",
                            "<Grid.RowDefinitions>",
                            "<RowDefinition Height=\"Auto\" />",
                            "<RowDefinition Height=\"*\" />",
                            "</Grid.RowDefinitions>",
                            "<TextBlock Style=\"{DynamicResource TitleTextBlockStyle}\" Text=\"Grid Splitter\" Margin=\"0 0 0 10\"/>",
                            "<Border BorderBrush=\"{DynamicResource ControlElevationBorderBrush}\"",
                            "BorderThickness=\"2\"",
                            "Grid.Row=\"1\"",
                            "Padding=\"10\"",
                            "CornerRadius=\"4\">",
                            "<Grid Background=\"{DynamicResource ControlAltFillColorSecondaryBrush}\">",
                            "<Grid.RowDefinitions>",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"Auto\" />",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"Auto\" />",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"Auto\" />",
                            "<RowDefinition Height=\"*\" />",
                            "</Grid.RowDefinitions>",
                            "<Grid.ColumnDefinitions>",
                            "<ColumnDefinition Width=\"*\" />",
                            "<ColumnDefinition Width=\"Auto\" />",
                            "<ColumnDefinition Width=\"*\" />",
                            "</Grid.ColumnDefinitions>",
                            "<TextBlock TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\" />",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"2\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText2}\"/>",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"2\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\"/>",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"0\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText2}\"/>",
                            "<TextBlock Grid.Row=\"4\" Grid.Column=\"2\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText2}\"/>",
                            "<TextBlock Grid.Row=\"4\" Grid.Column=\"0\" TextWrapping=\"Wrap\" Text=\"{StaticResource SampleText}\"/>",
                            "<GridSplitter Grid.RowSpan=\"5\" Grid.Column=\"1\" />",
                            "<GridSplitter Grid.Row=\"1\" Grid.ColumnSpan=\"3\"/>",
                            "<GridSplitter Grid.Row=\"3\" Grid.ColumnSpan=\"1\"/>",
                            "</Grid>",
                            "</Border>",
                            "</Grid>")));

                AssertExamples(
                    new GroupBoxPage(new GroupBoxPageViewModel()),
                    new ExpectedExample(
                        "A GroupBox",
                        Lines(
                            "<GroupBox",
                            "Header=\"User Information\"",
                            "HorizontalAlignment=\"Left\"",
                            "VerticalAlignment=\"Center\"",
                            "Width=\"400\">",
                            "<StackPanel>",
                            "<StackPanel Orientation=\"Horizontal\">",
                            "<TextBlock Width=\"100\" Text=\"Name:\" />",
                            "<TextBox Name=\"NameTextBox\" Width=\"280\" Margin=\"10,0,0,20\"/>",
                            "</StackPanel>",
                            "<StackPanel Orientation=\"Horizontal\">",
                            "<TextBlock Width=\"100\" Text=\"Gender:\" Margin=\"0,10,0,0\"/>",
                            "<TextBox Name=\"GenderTextBox\" Width=\"280\" Margin=\"10,0,0,20\"/>",
                            "</StackPanel>",
                            "<Button Content=\"Submit\" HorizontalAlignment=\"Right\" Width=\"100\" Margin=\"0,10,0,0\" />",
                            "</StackPanel>",
                            "</GroupBox>")));

                AssertExamples(
                    new ResizeGripPage(new ResizeGripPageViewModel()),
                    new ExpectedExample(
                        "A ResizeGrip",
                        Lines(
                            "<Window",
                            "Width=\"500\"",
                            "Height=\"300\"",
                            "ResizeMode=\"CanResizeWithGrip\">",
                            "<TextBlock",
                            "Text=\"ResizeGrip is present at the bottom right corner of the window\"",
                            "HorizontalAlignment=\"Center\"",
                            "VerticalAlignment=\"Center\"",
                            "FontSize=\"16\" />",
                            "</Window>"),
                        Lines(
                            "private void OpenResizeGripWindow_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "Window window = new Window()",
                            "{",
                            "Width = 500,",
                            "Height = 300,",
                            "ResizeMode = ResizeMode.CanResizeWithGrip,",
                            "Content = new TextBlock",
                            "{",
                            "Text = \"ResizeGrip is present at the bottom right corner of the window\",",
                            "HorizontalAlignment = HorizontalAlignment.Center,",
                            "VerticalAlignment = VerticalAlignment.Center,",
                            "FontSize = 16",
                            "}",
                            "};",
                            "window.Show();",
                            "}")));

                AssertExamples(
                    new StackPanelPage(new StackPanelPageViewModel()),
                    new ExpectedExample(
                        "A basic vertical StackPanel",
                        Lines(
                            "<StackPanel Orientation=\"Vertical\">",
                            "<Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" />",
                            "<Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" />",
                            "<Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" />",
                            "</StackPanel>")),
                    new ExpectedExample(
                        "A horizontal StackPanel",
                        Lines(
                            "<StackPanel Orientation=\"Horizontal\">",
                            "<Rectangle Width=\"100\" Height=\"30\" Fill=\"CornflowerBlue\" />",
                            "<Rectangle Width=\"100\" Height=\"30\" Fill=\"LightCoral\" />",
                            "<Rectangle Width=\"100\" Height=\"30\" Fill=\"MediumSeaGreen\" />",
                            "</StackPanel>")));
            });
        }
    }
}
