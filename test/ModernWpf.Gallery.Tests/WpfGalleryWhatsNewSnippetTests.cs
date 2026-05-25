using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Pages;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryWhatsNewSnippetTests
    {
        [TestMethod]
        public void WhatsNewControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new WhatsNewPage(),
                    new ExpectedExample(
                        "Grid Shorthand Syntax Sample",
                        Lines(
                            "<Grid RowDefinitions=\"Auto,Auto,Auto\" ColumnDefinitions=\"Auto 80 *\" HorizontalAlignment=\"Left\">",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"0\" FontWeight=\"Bold\" Margin=\"0 0 10 0\">Sl. No.</TextBlock>",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"1\" FontWeight=\"Bold\">Name</TextBlock>",
                            "<TextBlock Grid.Row=\"0\" Grid.Column=\"2\" FontWeight=\"Bold\">Description</TextBlock>",
                            "<TextBlock Grid.Row=\"1\" Grid.Column=\"0\">1</TextBlock>",
                            "<TextBlock Grid.Row=\"1\" Grid.Column=\"1\">Rectangle</TextBlock>",
                            "<TextBlock Grid.Row=\"1\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Quadrilateral where all the adjacent sides form a right angle.</TextBlock>",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"0\">2</TextBlock>",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"1\">Circle</TextBlock>",
                            "<TextBlock Grid.Row=\"2\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Set of all points that are equidistant from a fixed point.</TextBlock>",
                            "</Grid>")),
                    new ExpectedExample(
                        "AccentColor API",
                        Lines(
                            "<StackPanel Orientation=\"Horizontal\" Height=\"50\">",
                            "<StackPanel.Resources>",
                            "<Style TargetType=\"Border\">",
                            "<Setter Property=\"Height\" Value=\"50\" />",
                            "<Setter Property=\"Width\" Value=\"30\" />",
                            "</Style>",
                            "</StackPanel.Resources>",
                            "<Border CornerRadius=\"2 0 0 2\" Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark3BrushKey}}\" />",
                            "<Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark2BrushKey}}\" />",
                            "<Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark1BrushKey}}\" />",
                            "<Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorBrushKey}}\" />",
                            "<Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight1BrushKey}}\" />",
                            "<Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight2BrushKey}}\" />",
                            "<Border CornerRadius=\"0 2 2 0\" Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight3BrushKey}}\" />",
                            "</StackPanel>")),
                    new ExpectedExample(
                        "Hyphen based ligature example",
                        Lines(
                            "<StackPanel Orientation=\"Horizontal\">",
                            "<TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"-->\" />",
                            "<TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"&lt;!--\" />",
                            "<TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"&lt;--\" />",
                            "</StackPanel>")));
            });
        }

        [TestMethod]
        public void WhatsNewLinkHandlersUseOfficialProcessStartShape()
        {
            var source = ReadRepoFile("ModernWpf.Gallery", "Pages", "WhatsNewPage.xaml.cs");

            Assert.IsFalse(source.Contains("OpenUri("), "Copied WhatsNew link handlers should keep the official direct Process.Start source shape.");
            StringAssert.Contains(source, "Process.Start(new ProcessStartInfo(\"https://learn.microsoft.com/en-in/dotnet/desktop/wpf/whats-new/net100\") { UseShellExecute = true });");
            StringAssert.Contains(source, "Process.Start(new ProcessStartInfo(\"https://learn.microsoft.com/en-in/dotnet/desktop/wpf/whats-new/net90\") { UseShellExecute = true });");
            StringAssert.Contains(source, "Process.Start(new ProcessStartInfo(\"https://github.com/dotnet/wpf/issues/9613\") { UseShellExecute = true });");
            StringAssert.Contains(source, "Process.Start(new ProcessStartInfo(\"https://aka.ms/wpf-fluentdoc\") { UseShellExecute = true });");
            StringAssert.Contains(source, "ViewModel.NavigateCommand.Execute(\"MessageBox\");");
        }
    }
}
