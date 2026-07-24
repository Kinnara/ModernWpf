using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalendarPage = ModernWpf.Gallery.Pages.WpfGallery.DateAndTime.CalendarPage;
using CalendarPageViewModel = ModernWpf.Gallery.Pages.WpfGallery.DateAndTime.CalendarPageViewModel;
using CanvasPage = ModernWpf.Gallery.Pages.WpfGallery.Media.CanvasPage;
using CanvasPageViewModel = ModernWpf.Gallery.Pages.WpfGallery.Media.CanvasPageViewModel;
using DatePickerPage = ModernWpf.Gallery.Pages.WpfGallery.DateAndTime.DatePickerPage;
using DatePickerPageViewModel = ModernWpf.Gallery.Pages.WpfGallery.DateAndTime.DatePickerPageViewModel;
using ImagePage = ModernWpf.Gallery.Pages.WpfGallery.Media.ImagePage;
using ImagePageViewModel = ModernWpf.Gallery.Pages.WpfGallery.Media.ImagePageViewModel;
using ProgressBarPage = ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo.ProgressBarPage;
using ProgressBarPageViewModel = ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo.ProgressBarPageViewModel;
using ToolTipPage = ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo.ToolTipPage;
using ToolTipPageViewModel = ModernWpf.Gallery.Pages.WpfGallery.StatusAndInfo.ToolTipPageViewModel;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGallerySmallSectionSnippetTests
    {
        [TestMethod]
        public void DateMediaAndStatusControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new CalendarPage(new CalendarPageViewModel()),
                    new ExpectedExample(
                        "A basic Calendar control.",
                        "<Calendar/>"));

                AssertExamples(
                    new DatePickerPage(new DatePickerPageViewModel()),
                    new ExpectedExample(
                        "A basic DatePicker control.",
                        "<DatePicker/>"));

                AssertExamples(
                    new CanvasPage(new CanvasPageViewModel()),
                    new ExpectedExample(
                        "A basic Canvas inside the ViewBox",
                        Lines(
                            "<Viewbox Width=\"200\" Height=\"200\" >",
                            "<Canvas Width=\"47\" Height=\"123\">",
                            "<Path Data=\"M0,19H18V84h29v15H0V19Z\" Fill=\"White\" />",
                            "<Path Data=\"M46,80H29V15H0V0H46V80Z\" Fill=\"White\" />",
                            "</Canvas>",
                            "</Viewbox>")));

                AssertExamples(
                    new ImagePage(new ImagePageViewModel()),
                    new ExpectedExample(
                        "Standand Image from a local file.",
                        "<Image Height=\"100\" Source=\"Assets\\MyImage.jpg\" />"));

                AssertExamples(
                    new ProgressBarPage(new ProgressBarPageViewModel()),
                    new ExpectedExample(
                        "A simple progress bar.",
                        "<ProgressBar Value=\"40\" />"),
                    new ExpectedExample(
                        "An indeterminate progress bar.",
                        "<ProgressBar IsIndeterminate=\"True\" />"));

                AssertExamples(
                    new ToolTipPage(new ToolTipPageViewModel()),
                    new ExpectedExample(
                        "A button with a simple ToolTip.",
                        Lines(
                            "<Button",
                            "Content=\"Button with a simple ToolTip.\"",
                            "ToolTipService.InitialShowDelay=\"100\"",
                            "ToolTipService.Placement=\"MousePoint\"",
                            "ToolTipService.ToolTip=\"Simple ToolTip\"/>")));
            });
        }
    }
}
