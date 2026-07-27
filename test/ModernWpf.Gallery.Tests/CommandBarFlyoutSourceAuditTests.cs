using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class CommandBarFlyoutSourceAuditTests
    {
        [TestMethod]
        public void CurrentCommandBarFlyoutSourcesBehaviorAccessibilityAndPixelGatesArePinnedByAudit()
        {
            var root = FindRepoRoot();
            var audit = Read(root, "docs", "commandbarflyout-winui3-source-audit.md");
            var flyout = Read(root, "ModernWpf.Controls", "CommandBarFlyout", "CommandBarFlyout.cs");
            var commandBar = Read(root, "ModernWpf.Controls", "CommandBarFlyout", "CommandBarFlyoutCommandBar.cs");
            var template = Read(root, "ModernWpf.Controls", "CommandBarFlyout", "CommandBarFlyout.xaml");
            var productTests = Read(root, "test", "ModernWpf.WinUI.Tests", "CommandBarFlyout", "CommandBarFlyoutApiTests.cs");
            var sample = Read(root, "ModernWpf.Gallery", "Pages", "MenusToolbarsSampleFactory.cs");
            var sampleTests = Read(root, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(root, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(root, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");
            var publicDocumentation = Read(root, "ModernWpf.Controls", "ModernWpf.Controls.xml");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "7c8b6a6bc8c43d413160e5f7cce076c023ae4e0d");
            StringAssert.Contains(audit, "239ece554112abbd13f123e11d1e0d6df45bd390");
            StringAssert.Contains(audit, "bcc9d0171251f1130c61004d5d5856e19b31a1ea");
            StringAssert.Contains(audit, "e761a9f9a000dd93caaefdbde7d1b99c089f3519");
            StringAssert.Contains(audit, "5c0970f013029ad4e343ff073fc764f1b49088fe");
            StringAssert.Contains(audit, "55f99cde0");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "e6bf03057f7ad1e661c179a98899368e02208b6b");
            StringAssert.Contains(audit, "1de584d1e79bf0966827c0b605b79121100c4d36");
            StringAssert.Contains(audit, "6d2ce0768d78a0b3c5462a44e92f035ce5e43a1e");
            StringAssert.Contains(audit, "73a010396dec3c55ed0c5054776674f0526a548f");
            StringAssert.Contains(audit, "d1c31b098e57e5c0d29fa2ec2041dfab37149d1f");

            StringAssert.Contains(commandBar, "UpdateDynamicOverflow()");
            StringAssert.Contains(commandBar, "maximumWidth - Math.Max(36, m_moreButton.DesiredSize.Width) - panelMargin - 3");
            StringAssert.Contains(commandBar, "m_dynamicOverflowSeparator ??= new AppBarSeparator");
            StringAssert.Contains(commandBar, "GetDisplayedSecondaryCommands()");
            StringAssert.Contains(flyout, "if (AlwaysExpanded && IsOpen && !m_isClosingAfterCloseAnimation)");
            StringAssert.Contains(flyout, "CommandBarFlyoutCommandBar.IsOpenProperty, true");
            StringAssert.Contains(template, "CommandBarFlyoutEllipsisButtonStyle");
            StringAssert.Contains(template, "<Setter Property=\"MinWidth\" Value=\"136\" />");
            StringAssert.Contains(productTests, "WidePrimaryCommandStripMovesExcessCommandsIntoOverflowLikeWinUISource");
            StringAssert.Contains(productTests, "DynamicallyInsertedCommandsKeepCurrentFlyoutMenuItemAutomationRoles");
            StringAssert.Contains(productTests, "AlwaysExpanded must reject an overflow-collapse request");
            StringAssert.Contains(productTests, "HighContrastRestBackgroundMatchesCurrentWinUISource");
            StringAssert.Contains(publicDocumentation, "P:ModernWpf.Controls.CommandBarFlyout.AlwaysExpanded");

            StringAssert.Contains(sample, "CreateCommandBarFlyoutExamples(sampleSnippets)");
            StringAssert.Contains(sample, "CommandBarFlyoutSample1_xaml.txt");
            StringAssert.Contains(sample, "CommandBarFlyoutSample1_cs.txt");
            StringAssert.Contains(sample, "Click or right click the image to open a CommandBarFlyout");
            StringAssert.Contains(sample, "AutomationProperties.SetName(button, \"mountain\")");
            StringAssert.Contains(sampleTests, "CommandBarFlyoutSampleMatchesWinUIGalleryExample");
            StringAssert.Contains(harness, "\"CommandBarFlyout\" { return 6.0 }");
            StringAssert.Contains(harness, "\"CommandBarFlyout\" { return 9.0 }");
            StringAssert.Contains(recorder, "\"CommandBarFlyout\" { 800; break }");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-022802-704-69300/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-022908-765-38068/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-023035-553/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-023154-586/report.md");
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
