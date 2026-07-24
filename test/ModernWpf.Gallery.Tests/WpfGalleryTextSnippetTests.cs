using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Pages.WpfGallery.Text;
using HyperlinkPage = ModernWpf.Gallery.Pages.WpfGallery.Text.HyperlinkPage;
using LabelPage = ModernWpf.Gallery.Pages.WpfGallery.Text.LabelPage;
using PasswordBoxPage = ModernWpf.Gallery.Pages.WpfGallery.Text.PasswordBoxPage;
using RichTextEditPage = ModernWpf.Gallery.Pages.WpfGallery.Text.RichTextEditPage;
using TextBlockPage = ModernWpf.Gallery.Pages.WpfGallery.Text.TextBlockPage;
using TextBoxPage = ModernWpf.Gallery.Pages.WpfGallery.Text.TextBoxPage;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryTextSnippetTests
    {
        [TestMethod]
        public void TextControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new HyperlinkPage(new HyperlinkPageViewModel()),
                    new ExpectedExample(
                        "A Hyperlink with in-app navigation handling",
                        Lines(
                            "<TextBlock Margin=\"20\">",
                            "<Hyperlink NavigateUri=\"https://github.com/Kinnara/ModernWpf\" RequestNavigate=\"Hyperlink_RequestNavigate\">",
                            "ModernWPF repository",
                            "</Hyperlink>",
                            "</TextBlock>"),
                        Lines(
                            "private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)",
                            "{",
                            "NavigationStatusText.Text = $\"Navigation request: {e.Uri.AbsoluteUri}\";",
                            "NavigationStatusText.Visibility = Visibility.Visible;",
                            "e.Handled = true;",
                            "}")));

                AssertExamples(
                    new LabelPage(new LabelPageViewModel()),
                    new ExpectedExample(
                        "A simple Label.",
                        "<Label Content=\"I am a Label.\" />"),
                    new ExpectedExample(
                        "A Label for TextBox.",
                        Lines(
                            "<Grid>",
                            "<Grid.RowDefinitions>",
                            "<RowDefinition Height=\"Auto\" />",
                            "<RowDefinition Height=\"Auto\" />",
                            "</Grid.RowDefinitions>",
                            "<Label Grid.Row=\"0\" Content=\"I am a Label of the TextBox below.\" />",
                            "<TextBox Grid.Row=\"1\" />",
                            "</Grid>")));

                AssertExamples(
                    new PasswordBoxPage(new PasswordBoxPageViewModel()),
                    new ExpectedExample(
                        "A simple PasswordBox.",
                        "<PasswordBox />"));

                AssertExamples(
                    new RichTextEditPage(new RichTextEditPageViewModel()),
                    new ExpectedExample(
                        "A simple RichTextBox",
                        "<RichTextBox />"));

                AssertExamples(
                    new TextBoxPage(new TextBoxPageViewModel()),
                    new ExpectedExample(
                        "A simple TextBox.",
                        "<TextBox />"),
                    new ExpectedExample(
                        "A TextBox with input validation.",
                        Lines(
                            "<TextBox>",
                            "<TextBox.Text>",
                            "<Binding Path=\"ViewModel.ValidatedText\" UpdateSourceTrigger=\"PropertyChanged\">",
                            "<Binding.ValidationRules>",
                            "<helpers:AlphabeticValidationRule />",
                            "</Binding.ValidationRules>",
                            "</Binding>",
                            "</TextBox.Text>",
                            "</TextBox>")),
                    new ExpectedExample(
                        "A multi-line TextBox.",
                        "<TextBox TextWrapping=\"Wrap\" AcceptsReturn=\"True\" />"));

                AssertExamples(
                    new TextBlockPage(new TextBlockPageViewModel()),
                    new ExpectedExample(
                        "A simple TextBlock.",
                        "<TextBlock Text=\"I am a text block.\" />"),
                    new ExpectedExample(
                        "A TextBlock with style applied.",
                        "<TextBlock Text=\"I am a styled TextBlock.\" FontFamily=\"Comic Sans MS\" FontStyle=\"Italic\" />"),
                    new ExpectedExample(
                        "A TextBlock with inline text elements.",
                        Lines(
                            "<TextBlock FontSize=\"14\">",
                            "<Run FontFamily=\"Times New Roman\" Foreground=\"DarkGray\">",
                            "Text in a TextBlock doesn't have to be a simple string.",
                            "</Run>",
                            "<LineBreak />",
                            "<Span>",
                            "Text can be <Bold>bold</Bold>",
                            ", <Italic>italic</Italic>",
                            ", or <Underline>underlined</Underline>.",
                            "</Span>",
                            "</TextBlock>")),
                    new ExpectedExample(
                        "A TextBlock with wrap property.",
                        Lines(
                            "<TextBlock FontSize=\"14\" TextWrapping=\"Wrap\">",
                            "The TextBlock control provides flexible text support for WPF applications.",
                            "The element is targeted primarily toward basic UI scenarios that do not require more than one paragraph of text.",
                            "It supports a number of properties that enable precise control of presentation, such as FontFamily, FontSize, FontWeight, TextEffects, and TextWrapping.",
                            "</TextBlock>")));
            });
        }
    }
}
