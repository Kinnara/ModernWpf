using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void IconographyLoadedHandlerUsesWpfGalleryLoadDataCommandPath()
        {
            var pageSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconographyPage.xaml.cs");

            StringAssert.Contains(pageSource, "ViewModel.LoadDataCommand.Execute(null);");
            Assert.IsFalse(
                pageSource.Contains("ViewModel.LoadData();"),
                "The copied Iconography page should preserve the official WPF Gallery load-command path; the retained Loaded handler only adapts the XAML behavior trigger.");

            var viewModelSource = ReadRepoFile(
                "ModernWpf.Gallery",
                "Pages",
                "WpfGallery",
                "DesignGuidance",
                "IconographyPageViewModel.cs");

            StringAssert.Contains(viewModelSource, "private void LoadData()");
        }
    }
}
