using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class ToggleButtonSourceAuditTests
    {
        [TestMethod]
        public void CurrentToggleButtonSourcesAndPixelGatesArePinnedByAudit()
        {
            var repoRoot = FindRepoRoot();
            var audit = Read(repoRoot, "docs", "togglebutton-wpf-fluent-source-audit.md");
            var style = Read(repoRoot, "ModernWpf", "Styles", "ToggleButton.xaml");
            var sample = Read(repoRoot, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
            var sampleTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

            StringAssert.Contains(audit, "83e6cbda760818a2ab885c4aa3fc7e3a39eedf58");
            StringAssert.Contains(audit, "src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.xaml");
            StringAssert.Contains(audit, "1c021505727a0f1011525e6b1512e770b2bf4044");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "WinUIGallery/Samples/ToggleButton/ToggleButtonPage.xaml");
            StringAssert.Contains(audit, "WinUIGallery/Samples/ToggleButton/ToggleButtonPage.xaml.cs");
            StringAssert.Contains(audit, "WinUIGallery/Samples/ToggleButton/ToggleButtonSimple.txt");
            StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
            StringAssert.Contains(audit, "18bdae8f30ddb1df6d6d25b9a2eb3d41a6590e56");

            StringAssert.Contains(style, "<Thickness x:Key=\"ToggleButtonPadding\">11,5,11,6</Thickness>");
            StringAssert.Contains(style, "<Style x:Key=\"DefaultToggleButtonStyle\" TargetType=\"{x:Type ToggleButton}\">");
            StringAssert.Contains(style, "x:Name=\"ContentBorder\"");
            StringAssert.Contains(style, "x:Name=\"ContentPresenter\"");
            StringAssert.Contains(style, "<Condition Property=\"IsChecked\" Value=\"False\" />");
            StringAssert.Contains(style, "<Condition Property=\"IsChecked\" Value=\"True\" />");
            StringAssert.Contains(style, "{DynamicResource AccentControlElevationBorderBrush}");

            StringAssert.Contains(sample, "Name = \"Toggle1\"");
            StringAssert.Contains(sample, "Content = \"ToggleButton\"");
            StringAssert.Contains(sample, "Name = \"Control1Output\"");
            StringAssert.Contains(sample, "Text = \"Off\"");
            StringAssert.Contains(sample, "GalleryAutomation.SampleElementId(\"ToggleButton\", \"Output\")");
            StringAssert.Contains(sample, "button.Checked += delegate { outputText.Text = \"On\"; };");
            StringAssert.Contains(sample, "button.Unchecked += delegate { outputText.Text = \"Off\"; };");

            StringAssert.Contains(sampleTests, "new ToggleButtonAutomationPeer(button)");
            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(output)");
            StringAssert.Contains(sampleTests, "PatternInterface.Toggle");
            StringAssert.Contains(sampleTests, "typeof(IToggleProvider)");

            StringAssert.Contains(harness, "Get-StateInteractionOutputAutomationId");
            StringAssert.Contains(harness, "OutputMatched = $stateOutputMatched");
            StringAssert.Contains(harness, "\"ToggleButton\" { return 3.0 }");
            StringAssert.Contains(harness, "\"ToggleButton\" { return 7.0 }");
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
