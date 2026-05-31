using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Controls;
using ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance;
using GeometryPage = ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.GeometryPage;
using TypographyPage = ModernWpf.Gallery.Pages.WpfGallery.DesignGuidance.TypographyPage;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryDesignGuidanceSnippetTests
    {
        [TestMethod]
        public void DesignGuidanceControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new TypographyPage(new TypographyPageViewModel()),
                    new ExpectedExample(
                        "Type ramp",
                        Lines(
                            "<TextBlock Text=\"Caption\" Style=\"{StaticResource CaptionTextBlockStyle}\" />",
                            "<TextBlock Text=\"Body\" Style=\"{StaticResource BodyTextBlockStyle}\" />",
                            "<TextBlock Text=\"Body Strong\" Style=\"{StaticResource BodyStrongTextBlockStyle}\" />",
                            "<TextBlock Text=\"Subtitle\" Style=\"{StaticResource SubtitleTextBlockStyle}\" />",
                            "<TextBlock Text=\"Title\" Style=\"{StaticResource TitleTextBlockStyle}\" />",
                            "<TextBlock Text=\"Title Large\" Style=\"{StaticResource TitleLargeTextBlockStyle}\" />",
                            "<TextBlock Text=\"Display\" Style=\"{StaticResource DisplayTextBlockStyle}\" />")));

                AssertExamples(
                    new GeometryPage(new GeometryPageViewModel()),
                    new ExpectedExample(
                        null,
                        Lines(
                            "<Border CornerRadius=\"{StaticResource OverlayCornerRadius}\" />",
                            "<Border CornerRadius=\"{StaticResource ControlCornerRadius}\" />")));
            });
        }

        [TestMethod]
        public void IconographyLoadedBehaviorUsesWpfGalleryLoadDataCommandPath()
        {
            var pageXaml = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconsPage.xaml");

            StringAssert.Contains(pageXaml, "xmlns:i=\"http://schemas.microsoft.com/xaml/behaviors\"");
            StringAssert.Contains(pageXaml, "<i:InvokeCommandAction Command=\"{Binding ViewModel.LoadDataCommand}\" />");

            var pageSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconsPage.xaml.cs");

            StringAssert.Contains(
                pageSource,
                "CommandManager.RegisterClassCommandBinding(typeof(IconsPage), new CommandBinding(ApplicationCommands.Copy, Copy_Content));");
            StringAssert.Contains(
                pageSource,
                "public static void Copy_Content(object sender, RoutedEventArgs e)");
            AssertContainsInOrder(
                pageSource,
                "public static void Copy_Content(object sender, RoutedEventArgs e)",
                "if (!string.IsNullOrEmpty(((ExecutedRoutedEventArgs)e).Parameter.ToString()))",
                "Clipboard.SetText(((ExecutedRoutedEventArgs)e).Parameter.ToString());",
                "MessageBox.Show(\"Error copying to clipboard: \" + ex.Message);");
            Assert.IsFalse(
                pageSource.Contains("var text = ((ExecutedRoutedEventArgs)e).Parameter as string", StringComparison.Ordinal),
                "The copied Iconography copy command should keep the official Parameter.ToString() behavior instead of narrowing the command parameter to string.");
            Assert.IsFalse(
                pageSource.Contains("OnLoaded"),
                "The copied Iconography page should use the official WPF Gallery behavior trigger instead of a local Loaded handler.");

            var viewModelSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconsPageViewModel.cs");

            StringAssert.Contains(viewModelSource, "private void LoadData()");
        }

        [TestMethod]
        public void ColorSubsectionExamplesMatchOfficialWpfGalleryVisibleContent()
        {
            WpfTestHost.Run(() =>
            {
                AssertColorExamples(
                    new TextSection(),
                    new ExpectedColorExample("Text", "For UI labels and static text", typeof(TextBlock)),
                    new ExpectedColorExample("Accent Text", "Recommended for links", typeof(TextBlock)),
                    new ExpectedColorExample("Text On Accent", "Used for text on accent colored controls or fills", typeof(TextBlock)));

                AssertColorExamples(
                    new FillSection(),
                    new ExpectedColorExample("Control Fill", "Fill used for standard controls", typeof(Button)),
                    new ExpectedColorExample("Control Alt Fill", "Fill used for the 'off' states of toggle controls", typeof(CheckBox)),
                    new ExpectedColorExample("Control Solid", "Fills used for Sliders thumb control to cover the track beneath it.", typeof(Slider)),
                    new ExpectedColorExample("Control Strong Fill", "Used for controls that must meet contrast ratio requirements of 3:1.", typeof(ScrollBar)),
                    new ExpectedColorExample("Subtle Fill", "Used for list items and fills that are transparent at rest and appear upon interaction.", typeof(StackPanel)),
                    new ExpectedColorExample("Control On Image Fill", "Used for controls living on top of imagery.", typeof(Border)),
                    new ExpectedColorExample("Accent Fill", "Used for accent fills on controls", typeof(StackPanel)));

                AssertColorExamples(
                    new StrokeSection(),
                    new ExpectedColorExample("Control Elevation (gradient strokes)", "Used for standard control strokes and stroke states.", typeof(Button)),
                    new ExpectedColorExample("Control Stroke", "Used for gradient stops in elevation borders, and for control states.", typeof(Button)),
                    new ExpectedColorExample("Card Stroke", "Used for card and layer colors.", typeof(Button)),
                    new ExpectedColorExample("Control Strong Stroke", "Used for control strokes that must meet contrast ratio requirements of 3:1.", typeof(CheckBox)),
                    new ExpectedColorExample("Surface Stroke", "Used for strokes on background surfaces, ie: flyouts, windows, dialogs.", typeof(Border)),
                    new ExpectedColorExample("Divider Stroke", "Used for divider and graphic lines. Theme inverse; dark in light theme and light in dark theme.", typeof(Border)),
                    new ExpectedColorExample("Focus Stroke", "Used for divider and graphic lines. Theme inverse; dark in light theme and light in dark theme..", typeof(Border)));

                AssertColorExamples(
                    new BackgroundSection(),
                    new ExpectedColorExample("Card Background", "Used to create 'cards' - content blocks that live on page and layer backgrounds", typeof(Border)),
                    new ExpectedColorExample("Smoke Background", "Used over windows and desktop to block them out as inaccessible.", typeof(Border)),
                    new ExpectedColorExample("Layer", "Used on background colors of any material to create layering", typeof(Border)),
                    new ExpectedColorExample("Layer on Acrylic", "Used on background colors of any material to create layering.", typeof(Border)),
                    new ExpectedColorExample("Layer on Mica Base Alt", "Used for fills on Tab control.", typeof(Border)),
                    new ExpectedColorExample("Solid Background", "Solid background colors to place layers, cards or controls on.", typeof(Border)),
                    new ExpectedColorExample("Mica Background", "Mica background colors to place layers, cards, or controls on.", typeof(Border)),
                    new ExpectedColorExample("Acrylic Background", "Acrylic background colors to place layers, cards, or controls on.", typeof(Border)),
                    new ExpectedColorExample("Accent Acrylic Background", "Acrylic background colors to place layers, cards, or controls on.", typeof(Border)));

                AssertColorExamples(
                    new SignalSection(),
                    new ExpectedColorExample("System", "Used for accent fills on controls", null));
            });
        }

        private static void AssertColorExamples(Page section, params ExpectedColorExample[] expectedExamples)
        {
            var stack = section.Content as StackPanel;
            Assert.IsNotNull(stack, section.GetType().Name);

            var actualExamples = stack.Children.OfType<ColorPageExample>().ToArray();
            Assert.AreEqual(expectedExamples.Length, actualExamples.Length, section.GetType().Name);

            for (var i = 0; i < expectedExamples.Length; i++)
            {
                var expected = expectedExamples[i];
                var actual = actualExamples[i];
                var context = section.GetType().Name + " color example " + i;

                Assert.AreEqual(expected.Title, actual.Title, context);
                Assert.AreEqual(expected.Description, actual.Description, context);

                if (expected.ExampleContentType == null)
                {
                    Assert.IsNull(actual.ExampleContent, context);
                }
                else
                {
                    Assert.IsInstanceOfType(actual.ExampleContent, expected.ExampleContentType, context);
                }
            }
        }

        private sealed class ExpectedColorExample
        {
            public ExpectedColorExample(string title, string description, Type exampleContentType)
            {
                Title = title;
                Description = description;
                ExampleContentType = exampleContentType;
            }

            public string Title { get; }

            public string Description { get; }

            public Type ExampleContentType { get; }
        }
    }
}
