using Microsoft.VisualStudio.TestTools.UnitTesting;
using FramePage = ModernWpf.Gallery.Pages.WpfGallery.Navigation.FramePage;
using MenuPage = ModernWpf.Gallery.Pages.WpfGallery.Navigation.MenuPage;
using NavigationWindowPage = ModernWpf.Gallery.Pages.WpfGallery.Navigation.NavigationWindowPage;
using TabControlPage = ModernWpf.Gallery.Pages.WpfGallery.Navigation.TabControlPage;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryNavigationSnippetTests
    {
        [TestMethod]
        public void NavigationControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new MenuPage(),
                    new ExpectedExample(
                        "Standard Menu.",
                        Lines(
                            "<Menu>",
                            "<MenuItem Header=\"File\">",
                            "<MenuItem Header=\"New\" />",
                            "<MenuItem Header=\"New window\" />",
                            "<MenuItem Header=\"Open...\" />",
                            "<MenuItem Header=\"Save\" />",
                            "<MenuItem Header=\"Save As...\" />",
                            "<Separator />",
                            "<MenuItem Header=\"Exit\" />",
                            "</MenuItem>",
                            "<MenuItem Header=\"Edit\">",
                            "<MenuItem Header=\"Undo\" />",
                            "<Separator />",
                            "<MenuItem Header=\"Cut\" />",
                            "<MenuItem Header=\"Copy\" />",
                            "<MenuItem Header=\"Paste\" />",
                            "<MenuItem IsEnabled=\"False\" />",
                            "<Separator />",
                            "<MenuItem Header=\"Search with browser\" />",
                            "<MenuItem Header=\"Find...\" />",
                            "<MenuItem Header=\"Find Next\" />",
                            "<Separator />",
                            "<MenuItem Header=\"Select All\" />",
                            "</MenuItem>",
                            "</Menu>")));

                AssertExamples(
                    new TabControlPage(),
                    new ExpectedExample(
                        "Standard TabControl.",
                        Lines(
                            "<TabControl Margin=\"0,8,0,0\">",
                            "<TabItem>",
                            "<TabItem.Header>",
                            "<StackPanel Orientation=\"Horizontal\">",
                            "<TextBlock Text=\"Hello\" />",
                            "</StackPanel>",
                            "</TabItem.Header>",
                            "<Grid>",
                            "<TextBlock Margin=\"12\" Text=\"World\" />",
                            "</Grid>",
                            "</TabItem>",
                            "<TabItem IsSelected=\"True\">",
                            "<TabItem.Header>",
                            "<StackPanel Orientation=\"Horizontal\">",
                            "<TextBlock Text=\"The cake\" />",
                            "</StackPanel>",
                            "</TabItem.Header>",
                            "<Grid>",
                            "<TextBlock Margin=\"12\" Text=\"Is a lie.\" />",
                            "</Grid>",
                            "</TabItem>",
                            "</TabControl>")));

                AssertExamples(
                    new FramePage(),
                    new ExpectedExample(
                        "A Frame",
                        "<Frame Source=\"FramePage1.xaml\" NavigationUIVisibility=\"Visible\"/>"));

                AssertExamples(
                    new NavigationWindowPage(),
                    new ExpectedExample(
                        "A Navigation Window",
                        Lines(
                            "<NavigationWindow",
                            "Width=\"800\"",
                            "Height=\"450\"",
                            "Source=\"/Views/Navigation/Page1.xaml\" />"),
                        Lines(
                            "private void OpenNavigationWindow_Click(object sender, RoutedEventArgs e)",
                            "{",
                            "NavigationWindow window = new NavigationWindow()",
                            "{",
                            "Width = 800,",
                            "Height = 450,",
                            "Source = new Uri(\"/Views/Navigation/Page1.xaml\", UriKind.Relative)",
                            "};",
                            "window.Show();",
                            "}")));
            });
        }
    }
}
