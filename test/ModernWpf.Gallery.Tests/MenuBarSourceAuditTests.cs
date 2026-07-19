using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class MenuBarSourceAuditTests
    {
        [TestMethod]
        public void CurrentMenuBarSourcesBehaviorAccessibilityAndPixelGatesArePinnedByAudit()
        {
            var repoRoot = FindRepoRoot();
            var audit = Read(repoRoot, "docs", "menubar-winui3-source-audit.md");
            var product = Read(repoRoot, "ModernWpf.Controls", "MenuBar", "MenuBar.xaml");
            var item = Read(repoRoot, "ModernWpf.Controls", "MenuBar", "MenuBarItem.cs");
            var itemFlyout = Read(repoRoot, "ModernWpf.Controls", "MenuBar", "MenuBarItemFlyout.cs");
            var sample = Read(repoRoot, "ModernWpf.Gallery", "Pages", "MenusToolbarsSampleFactory.cs");
            var simpleSnippet = Read(repoRoot, "ModernWpf.Gallery", "Samples", "SampleCode", "MenuBar", "SimpleMenubar.txt");
            var submenuSnippet = Read(repoRoot, "ModernWpf.Gallery", "Samples", "SampleCode", "MenuBar", "MenubarSubmenusSeparatorsRadio.txt");
            var sampleTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var productTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "MenuBar", "MenuBarApiTests.cs");
            var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "bde55725158ab922a102268b10726ab8f8450957");
            StringAssert.Contains(audit, "610d3249721d6be66eb202791cfd5b1dc7a67041");
            StringAssert.Contains(audit, "96cf1b1e788a327c5c00e07af0e3fc3cbf23e40b");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "e59e706022dabe27d01b05dbedb8bc86a286f343");
            StringAssert.Contains(audit, "6804bce2118957356c28897ef47635f89da89ba2");
            StringAssert.Contains(audit, "WinUIGallery/Samples/MenuBar/SimpleMenubar.txt");
            StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");

            StringAssert.Contains(product, "x:Name=\"LayoutRoot\"");
            StringAssert.Contains(product, "x:Name=\"ContentRoot\"");
            StringAssert.Contains(product, "x:Name=\"ContentButton\"");
            StringAssert.Contains(product, "<ui:VisualStateEx x:Name=\"Selected\"");
            StringAssert.Contains(product, "<ui:VisualStateEx x:Name=\"PointerOver\"");

            StringAssert.Contains(item, "_flyout.Presenter.AddHandler(MenuItem.ClickEvent");
            StringAssert.Contains(item, "!item.StaysOpenOnClick");
            StringAssert.Contains(item, "Dispatcher.BeginInvoke((Action)CloseMenuFlyout)");
            StringAssert.Contains(itemFlyout, "Presenter.FontFamily = System.Windows.SystemFonts.MenuFontFamily");
            StringAssert.Contains(itemFlyout, "Presenter.FontSize = System.Windows.SystemFonts.MenuFontSize");
            StringAssert.Contains(itemFlyout, "Presenter.Padding = new System.Windows.Thickness(0, 2, 0, 1)");
            StringAssert.Contains(itemFlyout, "Presenter.Resources[\"MenuItemSubmenuContentMargin\"] = new System.Windows.Thickness(8, 6, 8, 6)");
            StringAssert.Contains(productTests, "FourItemFlyoutKeepsCurrentWinUIOpenSurfaceHeight");

            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"SimpleMenubar.txt\")");
            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"MenubarKeyboardAccelerators.txt\")");
            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"MenubarSubmenusSeparatorsRadio.txt\")");
            StringAssert.Contains(sample, "CreateMenuItem(\"Open\", output)");
            StringAssert.Contains(sample, "CreateMenuItem(\"Other Formats\", output)");
            StringAssert.Contains(simpleSnippet, "Text=\"Open...\"");
            StringAssert.Contains(submenuSnippet, "Text=\"Other Formats...\"");

            StringAssert.Contains(sampleTests, "new ModernWpf.Automation.Peers.MenuBarAutomationPeer(simpleMenu)");
            StringAssert.Contains(sampleTests, "new ModernWpf.Automation.Peers.MenuBarItemAutomationPeer(simpleFile)");
            StringAssert.Contains(sampleTests, "PatternInterface.ExpandCollapse");
            StringAssert.Contains(sampleTests, "new MenuItemAutomationPeer((MenuItem)simpleFile.Items[0])");
            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(selectedOptionText)");
            StringAssert.Contains(sampleTests, "Assert.AreEqual(\"You clicked: Open\", selectedOptionText.Text)");

            StringAssert.Contains(harness, "function Save-MenuBarOpenSurfaceCrop");
            StringAssert.Contains(harness, "\"MenuBarOpenSurface\" 0 $window");
            StringAssert.Contains(harness, "\"MenuBar\" { return 3.0 }");
            StringAssert.Contains(harness, "\"MenuBar\" { return 9.0 }");
            StringAssert.Contains(harness, "\"MenuBar\" { return 2 }");

            StringAssert.Contains(recorder, "\"MenuBar\" { return \"GallerySample_MenuBar_MenuBar\" }");
            StringAssert.Contains(recorder, "\"MenuBar\" { return $true }");
            StringAssert.Contains(recorder, "\"MenuBar\" { return @(\"New\", \"Open\", \"Save\", \"Exit\") }");
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
