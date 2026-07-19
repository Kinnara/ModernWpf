using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class DropDownButtonSourceAuditTests
    {
        [TestMethod]
        public void CurrentGallerySamplesAndLiveGatesArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "dropdownbutton-winui3-source-audit.md");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(root, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "dc5ff6d936836168db93e3c85245df24760bac1b");
            StringAssert.Contains(audit, "0f0479b8ea3456007f20bc930aab88341910eadd");
            StringAssert.Contains(audit, "376c922b56e8d4be1679c2683ba64cfa7b8da432");
            StringAssert.Contains(audit, "0c55bd9e3fd82a397dd4180f20c2672e891f37eb");
            StringAssert.Contains(audit, "417bb4c1a8ed0e266fc1c143bdb30e973bcf7062");

            StringAssert.Contains(sample, "CreateDropDownButtonExamples");
            StringAssert.Contains(sample, "Simple DropDownButton");
            StringAssert.Contains(sample, "DropDownButton with Icons");
            StringAssert.Contains(sample, "Content = \"Email\"");
            StringAssert.Contains(sample, "Content = new Mux.FontIcon { Glyph = \"\\uE715\" }");
            StringAssert.Contains(sample, "AutomationProperties.SetName(button, \"Email\")");
            StringAssert.Contains(sample, "Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft");
            StringAssert.Contains(sample, "CreateEmailMenuItem(\"Send\", includeIcons ? \"\\uE725\" : null)");
            StringAssert.Contains(sample, "CreateEmailMenuItem(\"Reply\", includeIcons ? \"\\uE8CA\" : null)");
            StringAssert.Contains(sample, "CreateEmailMenuItem(\"Reply All\", includeIcons ? \"\\uE8C2\" : null)");
            StringAssert.Contains(sampleTests, "DropDownButtonSampleMatchesWinUIGalleryExamples");

            StringAssert.Contains(harness, "\"DropDownButton\" { return 4.0 }");
            StringAssert.Contains(harness, "\"DropDownButton\" { return \"Email\" }");
            StringAssert.Contains(harness, "\"DropDownButton\" { return @(\"Send\", \"Reply\", \"Reply All\") }");
            StringAssert.Contains(recorder, "\"DropDownButton\" { return $true }");
            StringAssert.Contains(recorder, "\"DropDownButton\" { return @(\"Send\", \"Reply\", \"Reply All\") }");
            StringAssert.Contains(recorder, "\"DropDownButton\" { return \"Send\" }");

            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-034918-186-31168/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-035013-820-67352/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-035113-704/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-035158-155/report.md");
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
