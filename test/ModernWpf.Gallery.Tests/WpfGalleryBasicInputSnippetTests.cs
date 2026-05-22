using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Pages.WpfGallery.BasicInput;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryBasicInputSnippetTests
    {
        [TestMethod]
        public void BasicInputControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new ButtonPage(),
                    new ExpectedExample(
                        "Simple Button",
                        "<Button Content=\"Standard WPF button\" />"),
                    new ExpectedExample(
                        "WPF Accent Button",
                        "<Button Style=\"{DynamicResource AccentButtonStyle}\" Content=\"WPF Accent Button\" />"));

                AssertExamples(
                    new CheckBoxPage(),
                    new ExpectedExample(
                        "A 2-state CheckBox.",
                        "<CheckBox Content=\"Two-state CheckBox\" />"),
                    new ExpectedExample(
                        "A 3-state CheckBox.",
                        "<CheckBox IsThreeState=\"True\" Content=\"Three-state CheckBox\" IsChecked=\"{x:Null}\" />"),
                    new ExpectedExample(
                        "Using a 3-state CheckBox.",
                        Lines(
                            "<StackPanel>",
                            "<CheckBox Content=\"Select all\" IsThreeState=\"True\" />",
                            "<CheckBox Margin=\"24,0,0,0\" Content=\"Option 1\" />",
                            "<CheckBox Margin=\"24,0,0,0\" Content=\"Option 2\" />",
                            "<CheckBox Margin=\"24,0,0,0\" Content=\"Option 3\" />",
                            "</StackPanel>")));

                AssertExamples(
                    new ComboBoxPage(),
                    new ExpectedExample(
                        "A ComboBox with items defined inline.",
                        Lines(
                            "<ComboBox MinWidth=\"200\" HorizontalAlignment=\"Left\" SelectedIndex=\"0\">",
                            "<ComboBoxItem Content=\"Blue\" />",
                            "<ComboBoxItem Content=\"Green\" />",
                            "<ComboBoxItem Content=\"Red\" />",
                            "<ComboBoxItem Content=\"Yellow\" />",
                            "</ComboBox>")),
                    new ExpectedExample(
                        "A ComboBox with ItemsSource set.",
                        "<ComboBox MinWidth=\"200\" HorizontalAlignment=\"Left\" ItemsSource=\"{Binding FontFamilies}\" SelectedIndex=\"0\" />"),
                    new ExpectedExample(
                        "An editable ComboBox.",
                        "<ComboBox MinWidth=\"200\" HorizontalAlignment=\"Left\" ItemsSource=\"{Binding FontSizes}\" SelectedIndex=\"0\" IsEditable=\"True\" />"));

                AssertExamples(
                    new RadioButtonPage(),
                    new ExpectedExample(
                        "Standard RadioButton.",
                        Lines(
                            "<StackPanel>",
                            "<RadioButton Content=\"Option 1\" GroupName=\"radio_group_one\" IsChecked=\"True\"/>",
                            "<RadioButton Content=\"Option 2\" GroupName=\"radio_group_one\" />",
                            "<RadioButton Content=\"Option 3\" GroupName=\"radio_group_one\" />",
                            "</StackPanel>")),
                    new ExpectedExample(
                        "RadioButton with right to left flow direction.",
                        Lines(
                            "<StackPanel>",
                            "<RadioButton Content=\"Option 1\" FlowDirection=\"RightToLeft\" GroupName=\"radio_group_one\" IsChecked=\"True\"/>",
                            "<RadioButton Content=\"Option 2\" FlowDirection=\"RightToLeft\" GroupName=\"radio_group_one\" />",
                            "<RadioButton Content=\"Option 3\" FlowDirection=\"RightToLeft\" GroupName=\"radio_group_one\" />",
                            "</StackPanel>")));

                AssertExamples(
                    new SliderPage(),
                    new ExpectedExample(
                        "A simple slider.",
                        "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" Maximum=\"100\" Minimum=\"0\"/>"),
                    new ExpectedExample(
                        "A slider with steps and range specified.",
                        "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" IsSnapToTickEnabled=\"True\" Maximum=\"1000\" Minimum=\"500\" TickFrequency=\"50\"/>"),
                    new ExpectedExample(
                        "A slider with tick marks.",
                        "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" IsSnapToTickEnabled=\"True\" Maximum=\"100\" Minimum=\"0\" TickFrequency=\"20\" TickPlacement=\"Both\"/>"),
                    new ExpectedExample(
                        "A vertical slider with range and tick marks specified.",
                        "<Slider Width=\"200\" Margin=\"0\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" IsSnapToTickEnabled=\"True\" Maximum=\"100\" Minimum=\"0\" Orientation=\"Vertical\" TickFrequency=\"20\" TickPlacement=\"Both\"/>"));
            });
        }

    }
}
