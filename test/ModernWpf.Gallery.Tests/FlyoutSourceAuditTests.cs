using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class FlyoutSourceAuditTests
    {
        [TestMethod]
        public void CurrentFlyoutSourcesBehaviorAndPixelGatesArePinnedByAudit()
        {
            var repoRoot = FindRepoRoot();
            var audit = Read(repoRoot, "docs", "flyoutbase-winui3-source-audit.md");
            var presenter = Read(repoRoot, "ModernWpf.Controls", "Flyout", "FlyoutPresenter.xaml");
            var flyoutBase = Read(repoRoot, "ModernWpf.Controls", "Flyout", "FlyoutBase.cs");
            var productTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "Flyout", "FlyoutBaseApiTests.cs");
            var sample = Read(repoRoot, "ModernWpf.Gallery", "Pages", "DialogsFlyoutsSampleFactory.cs");
            var sampleTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "6c5a80dd3fc09043b4d2a801d7b8c991a7d4a320");
            StringAssert.Contains(audit, "20997cd1d1771eab20b9760496e18766ae1e38ab");
            StringAssert.Contains(audit, "23a52887e7f284970574ba80746aeca3b0857cfd");
            StringAssert.Contains(audit, "621bf7d16825ae37cdd0b0ad05b7b5a49ddcd4c4");
            StringAssert.Contains(audit, "2db27f71f857363d6a9a4485e01c8b8fdbe02499");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "WinUIGallery/Samples/Flyout/FlyoutPage.xaml");
            StringAssert.Contains(audit, "WinUIGallery/Samples/Flyout/FlyoutPage.xaml.cs");
            StringAssert.Contains(audit, "WinUIGallery/Samples/Flyout/ButtonFlyout.txt");

            StringAssert.Contains(presenter, "<Thickness x:Key=\"FlyoutContentPadding\">16,15,16,17</Thickness>");
            StringAssert.Contains(presenter, "WindowedPopupInsetMode=\"Medium\"");
            StringAssert.Contains(presenter, "IsShadowEnabled=\"{TemplateBinding IsDefaultShadowEnabled}\"");
            StringAssert.Contains(presenter, "BackgroundSizing=\"InnerBorderEdge\"");
            StringAssert.Contains(presenter, "Margin=\"{TemplateBinding Padding}\"");

            StringAssert.Contains(flyoutBase, "ClampSidePlacementVerticalOffset(");
            StringAssert.Contains(flyoutBase, "anchorBottom - Math.Min(");
            StringAssert.Contains(flyoutBase, "MonitorFromRect(ref nativeTargetRect, MONITOR_DEFAULTTONEAREST)");
            StringAssert.Contains(productTests, "SidePlacementNearMonitorBottomAlignsToAnchorBottomLikeCurrentWinUISource");

            StringAssert.Contains(sample, "Name = \"Control1\"");
            StringAssert.Contains(sample, "Content = \"Empty cart\"");
            StringAssert.Contains(sample, "Text = \"All items will be removed. Do you want to continue?\"");
            StringAssert.Contains(sample, "Margin = new Thickness(0, 0, 0, 12)");
            StringAssert.Contains(sample, "Content = \"Yes, empty my cart\"");
            StringAssert.Contains(sample, "flyout.Hide();");

            StringAssert.Contains(sampleTests, "new ButtonAutomationPeer(button)");
            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(flyoutText)");
            StringAssert.Contains(sampleTests, "new ButtonAutomationPeer(confirmButton)");
            StringAssert.Contains(sampleTests, "PatternInterface.Invoke");
            StringAssert.Contains(sampleTests, "Assert.IsFalse(flyout.IsOpen)");

            StringAssert.Contains(harness, "function Save-FlyoutOpenSurfaceCrop");
            StringAssert.Contains(harness, "Source = \"FlyoutOpenSurface\"");
            StringAssert.Contains(harness, "\"Flyout\" { return 3.0 }");
            StringAssert.Contains(harness, "\"Flyout\" { return 11.0 }");
            StringAssert.Contains(harness, "\"Flyout\" { return 1 }");

            StringAssert.Contains(recorder, "\"Flyout\" { return \"GallerySample_Flyout_Button\" }");
            StringAssert.Contains(recorder, "\"Flyout\" { return @(\"All items will be removed. Do you want to continue?\", \"Yes, empty my cart\") }");
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
