using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class MenuFlyoutSourceAuditTests
    {
        [TestMethod]
        public void CurrentMenuFlyoutSourcesBehaviorAccessibilityAndPixelGatesArePinnedByAudit()
        {
            var repoRoot = FindRepoRoot();
            var audit = Read(repoRoot, "docs", "menuflyout-winui3-source-audit.md");
            var presenter = Read(repoRoot, "ModernWpf.Controls", "MenuFlyout", "MenuFlyout.xaml");
            var menuItem = Read(repoRoot, "ModernWpf", "Styles", "MenuItem.xaml");
            var radioMenuItem = Read(repoRoot, "ModernWpf.Controls", "RadioMenuItem", "RadioMenuItem.cs");
            var light = Read(repoRoot, "ModernWpf", "ThemeResources", "Light.xaml");
            var highContrast = Read(repoRoot, "ModernWpf", "ThemeResources", "HighContrast.xaml");
            var sample = Read(repoRoot, "ModernWpf.Gallery", "Pages", "MenusToolbarsSampleFactory.cs");
            var sampleTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
            var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
            var recorder = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

            StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
            StringAssert.Contains(audit, "6f9f3fd322583d2d537dc30c439e62accf1efbd4");
            StringAssert.Contains(audit, "f5cfea5b8bd4b3bccb8a24c8f4fbc165c1cc69b9");
            StringAssert.Contains(audit, "49b4d5326b4deba8c036e63a7e676715a5de4f3a");
            StringAssert.Contains(audit, "569d6084ab4a5800d18971bc9eefa99d543c355c");
            StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
            StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
            StringAssert.Contains(audit, "834f7da9d1a314fa30c7a92e8814f2bc4aa889c0");
            StringAssert.Contains(audit, "569c61d81cb4b9e0c2aece909358ec78071e02df");
            StringAssert.Contains(audit, "AppbarbuttonMenuflyout.txt");
            StringAssert.Contains(audit, "MenuflyoutTogglemenuflyoutitemsMenuflyoutseparator.txt");

            StringAssert.Contains(presenter, "Property=\"FontFamily\" Value=\"{DynamicResource ContentControlThemeFontFamily}\"");
            StringAssert.Contains(presenter, "Property=\"FontSize\" Value=\"{DynamicResource ControlContentThemeFontSize}\"");
            StringAssert.Contains(presenter, "<Thickness x:Key=\"MenuItemSubmenuContentMargin\">7,4,7,5</Thickness>");
            StringAssert.Contains(presenter, "Padding=\"0,0,0,1\"");
            StringAssert.Contains(menuItem, "<Thickness x:Key=\"MenuItemSubmenuContentMargin\">8,6,8,6</Thickness>");
            StringAssert.Contains(menuItem, "Margin=\"{DynamicResource MenuItemSubmenuContentMargin}\"");
            StringAssert.Contains(menuItem, "<system:String x:Key=\"MenuItemRadioBulletGlyph\">&#xE915;</system:String>");
            StringAssert.Contains(menuItem, "<Setter TargetName=\"CheckGlyph\" Property=\"Opacity\" Value=\"1\" />");
            StringAssert.Contains(radioMenuItem, "menuItem.SetCurrentValue(IsCheckableProperty, true)");
            StringAssert.Contains(light, "<Thickness x:Key=\"MenuFlyoutItemThemePadding\">11,8,11,9</Thickness>");
            StringAssert.Contains(light, "<Thickness x:Key=\"MenuFlyoutItemThemePaddingNarrow\">11,4,11,5</Thickness>");
            StringAssert.Contains(highContrast, "x:Key=\"MenuFlyoutItemBackgroundPointerOver\" ResourceKey=\"SystemColorHighlightColorBrush\"");
            StringAssert.Contains(highContrast, "x:Key=\"MenuFlyoutItemForegroundDisabled\" ResourceKey=\"SystemColorGrayTextColorBrush\"");

            StringAssert.Contains(sample, "CreateMenuFlyoutExamples(sampleSnippets)");
            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"AppbarbuttonMenuflyout.txt\")");
            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"MenuflyoutIconsKeyboardAccelerators.txt\")");
            StringAssert.Contains(sample, "FindSnippetText(sampleSnippets, \"MenuflyoutRadiomenuflyoutitems.txt\")");
            StringAssert.Contains(sample, "new FontFamily(\"Consolas\")");
            StringAssert.Contains(sample, "CreateAppBarOutput(\"Control1Output\")");
            StringAssert.Contains(sample, "CreateAppBarOutput(\"Control3bOutput\")");

            StringAssert.Contains(sampleTests, "new ModernWpf.Automation.Peers.AppBarButtonAutomationPeer(sortButton)");
            StringAssert.Contains(sampleTests, "PatternInterface.ExpandCollapse");
            StringAssert.Contains(sampleTests, "new MenuItemAutomationPeer(ratingItem)");
            StringAssert.Contains(sampleTests, "new MenuItemAutomationPeer(repeat)");
            StringAssert.Contains(sampleTests, "new TextBlockAutomationPeer(control1Output)");

            StringAssert.Contains(harness, "function Save-MenuFlyoutOpenSurfaceCrop");
            StringAssert.Contains(harness, "\"MenuFlyoutOpenSurface\"");
            StringAssert.Contains(harness, "\"MenuFlyout\" { return 1.0 }");
            StringAssert.Contains(harness, "\"MenuFlyout\" { return 8.0 }");
            StringAssert.Contains(harness, "\"MenuFlyout\" { return 0 }");

            StringAssert.Contains(recorder, "\"MenuFlyout\" { return \"GallerySample_MenuFlyout_AppBarButton\" }");
            StringAssert.Contains(recorder, "\"MenuFlyout\" { return $true }");
            StringAssert.Contains(recorder, "\"MenuFlyout\" { return @(\"By rating\", \"By match\", \"By distance\") }");
            StringAssert.Contains(recorder, "if ($control -eq \"MenuFlyout\")");
            StringAssert.Contains(recorder, "\"By rating\" \"LeafMenuItem\"");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-004427-629-26672/report.md");
            StringAssert.Contains(audit, "artifacts/visual-checks/20260719-004459-735-37204/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-004531-064/report.md");
            StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-004623-068/report.md");
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
