using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.SelectorBar;

[TestClass]
public class SelectorBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3SelectorBarParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "selectorbar-winui3-source-audit.md");
        var selector = Read(repoRoot, "ModernWpf.Controls", "SelectorBar", "SelectorBar.cs");
        var itemPeer = Read(repoRoot, "ModernWpf.Controls", "SelectorBar", "SelectorBarItemAutomationPeer.cs");
        var itemsPeer = Read(repoRoot, "ModernWpf.Controls", "SelectorBar", "SelectorBarItemsControl.cs");
        var selectorPeer = Read(repoRoot, "ModernWpf.Controls", "SelectorBar", "SelectorBarAutomationPeer.cs");
        var resources = Read(repoRoot, "ModernWpf.Controls", "SelectorBar", "Strings", "Resources.resx");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "SelectorBar", "SelectorBarApiTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "NavigationSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var visualHarness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
        var recordingHarness = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");

        StringAssert.Contains(audit, "4a26ec257cf6ec5b03d494d489c95abe3d887af2");
        StringAssert.Contains(audit, "ff3f546f8f22580dbcbb5dcbcc5689a15e543109");
        StringAssert.Contains(audit, "03fb32f3f7ded885ead07735139848adc8882197");
        StringAssert.Contains(audit, "c4c2bf7dec892002631b7d96ace9a707d9ed00da");
        StringAssert.Contains(audit, "5de9e849b5f3b678dde121b75b4983f352cbb803");
        StringAssert.Contains(audit, "b928003fb521d2901aa1dc3edef3f16a2fd869f3");
        StringAssert.Contains(audit, "205c8df3caecd96237136a347d428d73723a6d88");
        StringAssert.Contains(audit, "0bbc42853e410432d5ccc7eecc51f3f0fc767e72");
        StringAssert.Contains(audit, "54d37c95f97f6d366333a7996301f86b2bb57bba");
        StringAssert.Contains(audit, "01951cdbb3af6802e9cf6d457e62bbd9e02f286d");
        StringAssert.Contains(audit, "194182902a7df8b134b1eeeda17c0bf354464c44");
        StringAssert.Contains(audit, "6019fa505d3a1f90bf22c89138470c03c994056e");
        StringAssert.Contains(audit, "27c6248d055005ad27477ffe957ffb79ae155b47");
        StringAssert.Contains(audit, "abf79f19728820b2d4db4649fcfd698500d939ba");

        StringAssert.Contains(audit, "08c77fd4eb5e3105e210166a00e0f0082cf8d873");
        StringAssert.Contains(audit, "710711c1daf70fb6914a46fb6c8f0f90600f5040");
        StringAssert.Contains(audit, "34b1d24fda342e274202c33bd4131ac93b48d873");
        StringAssert.Contains(audit, "9ece269a599518155ff6b686f2bdd296fde166e2");
        StringAssert.Contains(audit, "c52043ced6a16955cfa56b06218c003e08a34c85");
        StringAssert.Contains(audit, "af62f7f6df5c54bd51e8c0db2ece59aa6114c7cb");
        StringAssert.Contains(audit, "WinUIGallery\\Samples\\SelectorBar\\SelectorBarPage.xaml");
        Assert.IsFalse(audit.Contains("`src\\controls", StringComparison.Ordinal));

        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-211850-273-87320/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-211931-717-99256/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-212208-592/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-212254-641/report.md");
        StringAssert.Contains(audit, "exact `284x48`");
        StringAssert.Contains(audit, "`1.99`");
        StringAssert.Contains(audit, "`2.58`");
        StringAssert.Contains(audit, "`0.711` maximum local delta");
        StringAssert.Contains(audit, "`0.251`");

        StringAssert.Contains(selector, "internal SelectorBarItemsControl ItemsView");
        StringAssert.Contains(itemPeer, "AutomationControlType.ListItem");
        StringAssert.Contains(itemPeer, "SR_SelectorBarItemDefaultControlName");
        StringAssert.Contains(itemPeer, "OwnerItem.Owner?.ItemsView");
        StringAssert.Contains(itemsPeer, "ISelectionProvider");
        StringAssert.Contains(itemsPeer, "return AutomationControlType.List;");
        StringAssert.Contains(itemsPeer, "return \"ItemsView\";");
        StringAssert.Contains(itemsPeer, "AutomationControlType.ListItem");
        Assert.IsFalse(itemsPeer.Contains("AutomationControlType.TabItem", StringComparison.Ordinal));
        StringAssert.Contains(selectorPeer, "return false;");
        StringAssert.Contains(resources, "SelectorBarItemDefaultControlName");
        StringAssert.Contains(resources, "<value>SelectorBarItem</value>");
        StringAssert.Contains(apiTests, "Assert.AreEqual(AutomationControlType.ListItem");
        StringAssert.Contains(apiTests, "Assert.IsFalse(selectorPeer.IsControlElement())");

        StringAssert.Contains(galleryFactory, "SelectorBar Displaying Different Collections Using ItemsView");
        StringAssert.Contains(galleryFactory, "Mux.Symbol.OutlineStar");
        StringAssert.Contains(galleryTests, "SelectorBarSampleMatchesWinUIGalleryExamples");
        StringAssert.Contains(visualHarness, "\"SelectorBar\" { return 3.0 }");
        StringAssert.Contains(visualHarness, "\"SelectorBar\" { return \"PART_ItemsView\" }");
        StringAssert.Contains(recordingHarness, "\"SelectorBar\" { return \"GallerySample_SelectorBar_SelectorBarItemShared\" }");
        StringAssert.Contains(recordingHarness, "\"SelectorBar\" { return [double]$maxLocalFrameDelta -ge 0.05 }");
    }

    [TestMethod]
    public void SelectorBarCurrentTemplateVariantsAndWpfAdaptersStayDocumented()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "selectorbar-winui3-source-audit.md");
        var template = Read(repoRoot, "ModernWpf.Controls", "SelectorBar", "SelectorBar.xaml");

        StringAssert.Contains(audit, "SelectorBar_perf2026.xaml");
        StringAssert.Contains(audit, "VisualStateEx.Setters");
        StringAssert.Contains(audit, "purpose-built WPF `ItemsControl`");
        StringAssert.Contains(audit, "Only the en-US SelectorBar resource pack is currently added");
        StringAssert.Contains(template, "PART_ItemsView");
        StringAssert.Contains(template, "ui:VisualStateSetter");
        StringAssert.Contains(template, "PART_SelectionVisual");
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
