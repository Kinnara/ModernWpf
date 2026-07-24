using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.InfoBadge;

[TestClass]
public class InfoBadgeSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3InfoBadgeParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "infobadge-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "InfoBadge", "InfoBadge.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "InfoBadge", "InfoBadge.xaml");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "InfoBadge", "InfoBadgeApiTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "StatusInfoSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "a2b399aa8b2bbf21c74fefce299de9d7ecc55150");
        StringAssert.Contains(audit, "453a2655ed4dd3c78e33c9f08b149b25600db5bf");
        StringAssert.Contains(audit, "1f6ab40e7cc23c5bb7311a85006ec780e6a27d4f");
        StringAssert.Contains(audit, "b09b56572ac8a2159bf9ba2dda7f063ec5460c6a");
        StringAssert.Contains(audit, "6b32b0d1d5964655fbd520597d5693381332102f");
        StringAssert.Contains(audit, "fadb132ac965d2bf7ec435edf011a19a6338de2c");
        StringAssert.Contains(audit, "ca07acb962a7c018a174bf28dcd0e945b13ddb4d");
        StringAssert.Contains(audit, "0615fb945616d35d6fb5082c2e9029cfca167d74");
        StringAssert.Contains(audit, "6a97ca72fc8e5cf65e15a525df82a34d05142ee3");
        StringAssert.Contains(audit, "f80193f2d2b06931a0c2009f93f51bff36e064cc");
        StringAssert.Contains(audit, "db5b7520cf7d98697ec5303c9ede9d66992c7cf9");
        StringAssert.Contains(audit, "5c00eef2c46cbe4ae3b40b41558bd8ea98b108a2");
        StringAssert.Contains(audit, "f1ac3d32b20fd740d1492c573b31ce436a211709");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-190042-656-45992/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-190124-362-45024/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-190209-312/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-190250-370/report.md");
        StringAssert.Contains(audit, "| `16x16` / `16x16` | `4.44` |");
        StringAssert.Contains(audit, "| `16x16` / `16x16` | `3.73` |");
        StringAssert.Contains(audit, "`4.072` local delta");
        StringAssert.Contains(audit, "`3.393` local delta");
        StringAssert.Contains(audit, "controls\\dev\\InfoBadge\\InfoBadge.cpp");

        StringAssert.Contains(control, "TemplateSettings.IconElement = iconSource.CreateIconElement();");
        Assert.IsFalse(control.Contains("TemplateSettings.IconElement = null;", StringComparison.Ordinal));
        StringAssert.Contains(control, "stateName = \"Dot\";");
        StringAssert.Contains(control, "UpdateCornerRadius(arrangeBounds.Height);");
        StringAssert.Contains(control, "return new Size(desiredSize.Height, desiredSize.Height);");
        Assert.IsFalse(control.Contains("OnCreateAutomationPeer", StringComparison.Ordinal));

        StringAssert.Contains(template, "x:Name=\"RootGrid\"");
        StringAssert.Contains(template, "x:Name=\"DisplayKindStates\"");
        StringAssert.Contains(template, "x:Name=\"ValueTextBlock\"");
        StringAssert.Contains(template, "x:Name=\"IconPresenter\"");
        StringAssert.Contains(template, "SystemFillColorSolidNeutralBrush");

        StringAssert.Contains(apiTests, "InfoBadgeRetainsLastIconElementWhenReturningToDotLikeCurrentWinUI");
        StringAssert.Contains(apiTests, "InfoBadgeHasNoStandaloneAutomationPeerLikeCurrentWinUI");
        StringAssert.Contains(apiTests, "Assert.IsNull(UIElementAutomationPeer.CreatePeerForElement(infoBadge));");

        StringAssert.Contains(galleryFactory, "InfoBadge embedded in NavigationView ");
        StringAssert.Contains(galleryFactory, "Different InfoBadge Styles");
        StringAssert.Contains(galleryFactory, "Placing an InfoBadge Inside Another Control");
        StringAssert.Contains(galleryFactory, "InfoBadge with Dynamic Value");
        StringAssert.Contains(galleryFactory, "Inbox, 5 notifications");
        StringAssert.Contains(galleryFactory, "DynamicInfoBadge.Value = (int)args.NewValue;");
        StringAssert.Contains(galleryTests, "Assert.IsNull(UIElementAutomationPeer.CreatePeerForElement(infoBadge1));");

        StringAssert.Contains(harness, "function New-InfoBadgeReferencePrimaryCrop");
        StringAssert.Contains(harness, "\"InfoBadge\" { return 5.0 }");
        StringAssert.Contains(harness, "\"InfoBadge\" { return 0 }");
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
