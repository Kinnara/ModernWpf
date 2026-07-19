using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class RepeatButtonSourceAuditTests
    {
        [TestMethod]
        public void CurrentRepeatButtonSourcesAndPixelGatesArePinnedByAudit()
        {
            var repoRoot = FindRepoRoot();
            var audit = Read(repoRoot, "docs", "repeatbutton-wpf-fluent-source-audit.md");
            var style = Read(repoRoot, "ModernWpf", "Styles", "RepeatButton.xaml");
            var sample = Read(repoRoot, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
            var sampleTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

            StringAssert.Contains(audit, "83e6cbda760818a2ab885c4aa3fc7e3a39eedf58");
            StringAssert.Contains(audit, "src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.xaml");
            StringAssert.Contains(audit, "1c021505");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "WinUIGallery/Samples/RepeatButton/RepeatButtonPage.xaml");
            StringAssert.Contains(audit, "WinUIGallery/Samples/RepeatButton/RepeatButtonPage.xaml.cs");
            StringAssert.Contains(audit, "WinUIGallery/Samples/RepeatButton/RepeatButtonSimple.txt");
            StringAssert.Contains(audit, "b97ceb1ef7504631a9d2a7d5b46292f6f6a0e47a");

            StringAssert.Contains(style, "<Thickness x:Key=\"RepeatButtonPadding\">11,5,11,6</Thickness>");
            StringAssert.Contains(style, "<Style x:Key=\"DefaultRepeatButtonStyle\" TargetType=\"{x:Type RepeatButton}\">");
            StringAssert.Contains(style, "x:Name=\"ContentBorder\"");
            StringAssert.Contains(style, "x:Name=\"ContentPresenter\"");
            StringAssert.Contains(style, "<Trigger Property=\"IsEnabled\" Value=\"False\">");
            StringAssert.Contains(style, "<Trigger Property=\"IsMouseOver\" Value=\"True\">");
            StringAssert.Contains(style, "<Trigger Property=\"IsPressed\" Value=\"True\">");

            StringAssert.Contains(sample, "Content = \"Click and hold\"");
            StringAssert.Contains(sample, "Name = \"Control1Output\"");
            StringAssert.Contains(sample, "Margin = new Thickness(8, 0, 0, 0)");
            StringAssert.Contains(sample, "AutomationProperties.SetName(output, \"Control output\")");
            StringAssert.Contains(sample, "AutomationProperties.SetLiveSetting(output, AutomationLiveSetting.Polite)");
            StringAssert.Contains(sample, "output.Text = \"Number of clicks: \" + clicks;");
            StringAssert.Contains(sample, "peer?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)");

            StringAssert.Contains(sampleTests, "new RepeatButtonAutomationPeer(button)");
            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(output)");
            StringAssert.Contains(sampleTests, "PatternInterface.Invoke");

            StringAssert.Contains(harness, "function Save-RepeatButtonOutputSurfaceCrop");
            StringAssert.Contains(harness, "Source = \"RepeatButtonOutputRow\"");
            StringAssert.Contains(harness, "\"RepeatButton\" { return 4.0 }");
            StringAssert.Contains(harness, "\"RepeatButton\" { return 11.0 }");
            StringAssert.Contains(harness, "\"RepeatButton\" { return 0 }");
        }

        private static string Read(string repoRoot, params string[] parts)
        {
            var path = repoRoot;
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
