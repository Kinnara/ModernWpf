using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class PopupSourceAuditTests
    {
        [TestMethod]
        public void CurrentWinUIGalleryPopupParityIsPinnedByAudit()
        {
            var repoRoot = FindRepoRoot();
            var audit = Read(repoRoot, "docs", "popup-winui-gallery-source-audit.md");
            var sample = Read(repoRoot, "ModernWpf.Gallery", "Pages", "DialogsFlyoutsSampleFactory.cs");
            var sampleTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "WinUIGallery/Samples/Popup/PopupPage.xaml");
            StringAssert.Contains(audit, "WinUIGallery/Samples/Popup/PopupPage.xaml.cs");
            StringAssert.Contains(audit, "WinUIGallery/Samples/Popup/PopupOffsetPositioning.txt");
            StringAssert.Contains(audit, "6a5d35d9acf43067bb7cb9317e2d1ad863dbade2");
            StringAssert.Contains(audit, "ea3a3be6beecbaecb15f9a9612767d0e74c9b992");
            StringAssert.Contains(audit, "711eea6ce1ead50ef91bcf5eba7c011ddf892af9");
            StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
            StringAssert.Contains(audit, "177e25c1f537e54ce071c271d4486c81e6db1a3b");
            StringAssert.Contains(audit, "34d541896423996c22e5b80c5af2b2737be965d6");
            StringAssert.Contains(audit, "b9b7348c209836fe69c03cd8c5732baa77f46462");

            StringAssert.Contains(sample, "MinWidth = 240");
            StringAssert.Contains(sample, "Padding = new Thickness(16)");
            StringAssert.Contains(sample, "BorderThickness = new Thickness(1)");
            StringAssert.Contains(sample, "FontSize = 16");
            StringAssert.Contains(sample, "MinHeight = 22");
            StringAssert.Contains(sample, "Margin = new Thickness(0, 8, 0, 0)");
            StringAssert.Contains(sample, "HorizontalOffset = horizontalOffset.Value");
            StringAssert.Contains(sample, "VerticalOffset = verticalOffset.Value");
            StringAssert.Contains(sample, "popup.StaysOpen = !lightDismiss.IsOn;");

            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(heading)");
            StringAssert.Contains(sampleTests, "new ButtonAutomationPeer(closeButton)");
            StringAssert.Contains(sampleTests, "PatternInterface.Invoke");

            StringAssert.Contains(harness, "function Save-PopupOpenSurfaceCrop($window, $openElement, [string]$path)");
            StringAssert.Contains(harness, "Source = \"PopupSurface\"");
            StringAssert.Contains(harness, "\"Popup\" { return 3.0 }");
            StringAssert.Contains(harness, "\"Popup\" { return 0 }");

            StringAssert.Contains(recorder, "\"Popup\" { return \"GallerySample_Popup_Button\" }");
            StringAssert.Contains(recorder, "\"Popup\" { return $true }");
            StringAssert.Contains(recorder, "\"Popup\" { return @(\"Simple Popup\", \"Close\") }");
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
