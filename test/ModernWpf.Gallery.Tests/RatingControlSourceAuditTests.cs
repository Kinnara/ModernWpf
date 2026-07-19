using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class RatingControlSourceAuditTests
    {
        [TestMethod]
        public void CurrentRatingControlSourcesSampleAndPixelGatesArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "ratingcontrol-winui3-source-audit.md");
            var ratingControl = Read(root, "ModernWpf.Controls", "RatingControl", "RatingControl.cs");
            var productTests = Read(root, "test", "ModernWpf.WinUI.Tests", "RatingControl", "RatingControlApiTests.cs");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(root, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "61143cf16f5c0627153ecb1ad0ca1657f02135a7");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "7c4a293639c43105aa8d9526ce97beadc34ea8c1");
            StringAssert.Contains(audit, "dc55def2ac916fbf9464df9fbd9679b78cc772a3");
            StringAssert.Contains(audit, "15618bfa3921d191216af4a3db3fa527cf7ca190");
            StringAssert.Contains(audit, "085aed50dfcf3459ddcdc36a81ba359a4c4717b7");

            StringAssert.Contains(ratingControl, "CoercePlaceholderValueBetweenMinAndMax");
            StringAssert.Contains(productTests, "VerifyMaxRatingCoercionWhileLoadedDoesNotCrash");
            StringAssert.Contains(sample, "CreateRatingControlExamples()");
            StringAssert.Contains(sample, "A simple RatingControl");
            StringAssert.Contains(sample, "PlaceholderValue of RatingControl");
            StringAssert.Contains(sample, "Swipe left or click again to clear your rating.");
            StringAssert.Contains(sample, "PlaceholderValue = 0");
            StringAssert.Contains(sample, "rating.PlaceholderValue = slider.Value;");
            StringAssert.Contains(sampleTests, "RatingControlSampleMatchesWinUIGalleryExamples");

            StringAssert.Contains(harness, "\"RatingControl\" { return 7.0 }");
            StringAssert.Contains(harness, "\"RatingControl\" { return 5.0 }");
            StringAssert.Contains(recorder, "\"RatingControl\" { 3.0 }");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-025609-047-47084/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-025640-167-62492/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-025706-701/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-025724-041/report.md");
        }

        private static string Read(string root, params string[] parts)
        {
            var path = root;
            foreach (var part in parts)
            {
                path = Path.Combine(path, part);
            }

            return File.ReadAllText(path);
        }

        private static string FindRepoRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
            return string.Empty;
        }
    }
}
