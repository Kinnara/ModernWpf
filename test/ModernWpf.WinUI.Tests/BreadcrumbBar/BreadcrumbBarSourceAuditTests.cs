using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.BreadcrumbBar;

[TestClass]
public class BreadcrumbBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3BreadcrumbBarParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "breadcrumbbar-winui3-source-audit.md");
        var breadcrumb = Read(repoRoot, "ModernWpf.Controls", "BreadcrumbBar", "BreadcrumbBar.cs");
        var item = Read(repoRoot, "ModernWpf.Controls", "BreadcrumbBar", "BreadcrumbBarItem.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "BreadcrumbBar", "BreadcrumbBarItemAutomationPeer.cs");
        var resources = Read(repoRoot, "ModernWpf.Controls", "BreadcrumbBar", "Strings", "Resources.resx");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "BreadcrumbBar", "BreadcrumbBarApiTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "NavigationSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var visualHarness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
        var recordingHarness = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");

        StringAssert.Contains(audit, "c3abf33ba71e396341a165183ab8e4a6202b4bfb");
        StringAssert.Contains(audit, "2c84e298b228b8455143b3eaa84d847782ab99eb");
        StringAssert.Contains(audit, "fa1653ac70e1e0bf78b255ef0167b4032d9b321a");
        StringAssert.Contains(audit, "3cf9f2d6b8119a94c057d97a5755541430885f8f");
        StringAssert.Contains(audit, "d7d0b16aae852dc792122bcfc1d788237f784209");
        StringAssert.Contains(audit, "04a88235546743e6ff8a9157658240975bcfebbc");
        StringAssert.Contains(audit, "eddccd5245273d209389a7039795d472d72f2777");
        StringAssert.Contains(audit, "10e9f14396d84c7e5f59c38a7a57c5d97cec66a7");
        StringAssert.Contains(audit, "52e69acc2ba289e1a075b8fce343b6014b1c2eec");
        StringAssert.Contains(audit, "af094a1594bb76d73034e91d8c6e9677d7c67e15");
        StringAssert.Contains(audit, "e9bcb9f1fe7fe2e6f9e67e7bc3048de96237aa62");
        StringAssert.Contains(audit, "d91cbb50e9eabd9be224962f77f905dfc7c621c7");
        StringAssert.Contains(audit, "7dee4b0304407292f7badf8742373802d22a5a17");

        StringAssert.Contains(audit, "ac1587dfbe112353cedef00f96b42b994390b836");
        StringAssert.Contains(audit, "5b13ec7b35fe35851b4c717f66fc17858f1cc976");
        StringAssert.Contains(audit, "088a1e244ed5054068a08fc3d56a378588cc7ce4");
        StringAssert.Contains(audit, "d88e0ad3598f977c4b40d349bd5a67d5ebc8dae7");
        StringAssert.Contains(audit, "WinUIGallery\\Samples\\BreadcrumbBar\\BreadcrumbBarPage.xaml");
        Assert.IsFalse(audit.Contains("`src\\controls", StringComparison.Ordinal));

        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-205617-796-43088/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-205659-478-24012/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-205742-447/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-205804-316/report.md");
        StringAssert.Contains(audit, "exact `530x26`");
        StringAssert.Contains(audit, "`2.53`");
        StringAssert.Contains(audit, "`2.33`");
        StringAssert.Contains(audit, "`2.432` maximum local delta");
        StringAssert.Contains(audit, "`3.874`");

        StringAssert.Contains(breadcrumb, "internal bool IsEllipsisRendered");
        StringAssert.Contains(breadcrumb, "SR_AutomationNameEllipsisBreadcrumbBarItem");
        StringAssert.Contains(breadcrumb, "InvalidatePeer()");
        Assert.IsFalse(breadcrumb.Contains("AutomationProperties.SetName(item, \"More\")", StringComparison.Ordinal));
        StringAssert.Contains(item, "IsVisibleForAutomation");
        StringAssert.Contains(peer, "SR_BreadcrumbBarItemLocalizedControlType");
        StringAssert.Contains(peer, "IsControlElementCore");
        StringAssert.Contains(peer, "IsContentElementCore");
        StringAssert.Contains(resources, "AutomationNameEllipsisBreadcrumbBarItem");
        StringAssert.Contains(resources, "BreadcrumbBarItemLocalizedControlType");
        StringAssert.Contains(resources, "<value>More</value>");
        StringAssert.Contains(resources, "<value>breadcrumb bar item</value>");
        StringAssert.Contains(apiTests, "AutomationPeerMatchesWinUILocalizedTypeAndEllipsisAccessibilityView");
        StringAssert.Contains(apiTests, "Assert.IsFalse(peer.IsControlElement())");
        StringAssert.Contains(apiTests, "Assert.IsTrue(peer.IsContentElement())");

        StringAssert.Contains(galleryFactory, "A BreadcrumbBar control");
        StringAssert.Contains(galleryFactory, "BreadCrumbBar Control with Custom DataTemplate");
        StringAssert.Contains(galleryFactory, "items.RemoveAt(i)");
        StringAssert.Contains(galleryFactory, "if (!folders.Contains(folder))");
        StringAssert.Contains(galleryTests, "BreadcrumbBarSampleMatchesWinUIGalleryExamples");
        StringAssert.Contains(visualHarness, "\"BreadcrumbBar\" { return 3.0 }");
        StringAssert.Contains(visualHarness, "\"BreadcrumbBar\" { return \"BreadcrumbBar1\" }");
        StringAssert.Contains(recordingHarness, "if ($control -eq \"BreadcrumbBar\") { return \"Breadcrumb\" }");
        StringAssert.Contains(recordingHarness, "BreadcrumbChanged");
    }

    [TestMethod]
    public void BreadcrumbBarCurrentTemplateVariantsAndAccessibilitySubstitutionsStayDocumented()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "breadcrumbbar-winui3-source-audit.md");
        var template = Read(repoRoot, "ModernWpf.Controls", "BreadcrumbBar", "BreadcrumbBar.xaml");

        StringAssert.Contains(audit, "BreadcrumbBar_perf2026.xaml");
        StringAssert.Contains(audit, "VisualStateEx.Setters");
        StringAssert.Contains(audit, "WPF has no `AccessibilityView`");
        StringAssert.Contains(audit, "Only the en-US Breadcrumb resource pack is currently added");
        StringAssert.Contains(template, "ui:VisualStateSetter");
        StringAssert.Contains(template, "PART_ItemsRepeater");
        StringAssert.Contains(template, "PART_EllipsisFlyout");
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
