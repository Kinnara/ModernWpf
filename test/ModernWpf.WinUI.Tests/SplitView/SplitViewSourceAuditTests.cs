using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.SplitView;

[TestClass]
public class SplitViewSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3SplitViewParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "splitview-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "SplitView", "SplitView.cs");
        var properties = Read(repoRoot, "ModernWpf.Controls", "SplitView", "SplitView.properties.cs");
        var peers = Read(repoRoot, "ModernWpf.Controls", "SplitView", "SplitViewAutomationPeers.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "SplitView", "SplitView.xaml");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "LayoutSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "49b4d5326b4deba8c036e63a7e676715a5de4f3a");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "d58aff859c2e10d97e0b02a9ec35d65c4337d719");
        StringAssert.Contains(audit, "d1dc0f4603b4046fed9cdb53b262e73237992fa8");
        StringAssert.Contains(audit, "09123d961298baa7903bef3f7f5d31b01354281e");
        StringAssert.Contains(audit, "bba15283aa2ab3d4fa58124854f595bd636c1d0f");
        StringAssert.Contains(audit, "d793197acda192b7bc01f3251f86cee3e7c30dd6");
        StringAssert.Contains(audit, "84cc5a983fc6af7f8ea8466887692095471b6c23");
        StringAssert.Contains(audit, "d1622c4c92506535152ea7fee723f8ffeb3b941a");
        StringAssert.Contains(audit, "44bf3e3c701e5ed27648db602c5f84b2ca5236fc");
        StringAssert.Contains(audit, "5920731bab10d2398e0736bdddcf922585744f6d");
        StringAssert.Contains(audit, "cf10f87348f1791055abf3bce11bdcc6c9b3fdfc");
        StringAssert.Contains(audit, "919101bde98ecf5d74a56e5196fe9e69cac0c2a7");
        StringAssert.Contains(audit, "f0dbec14b1e20557d0ae4429555aac3b8410c561");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-130514-205-39096/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-130409-201-56932/report.md");
        StringAssert.Contains(audit, "| `400x300` / `400x300` | `3.23` |");
        StringAssert.Contains(audit, "| `400x300` / `400x300` | `3.37` |");

        StringAssert.Contains(control, "private static readonly string[,,] s_visualStateTable");
        StringAssert.Contains(control, "return double.IsNaN(openPaneLength) ? _paneMeasuredLength : openPaneLength;");
        StringAssert.Contains(control, "PaneClosing?.Invoke(this, args);");
        StringAssert.Contains(control, "internal bool IsLightDismissEnabledForAutomation => CanLightDismiss();");
        StringAssert.Contains(control, "internal void InvokeLightDismissForAutomation()");
        StringAssert.Contains(properties, "RestoreSavedFocusElement();");
        StringAssert.Contains(properties, "UpdateVisualState();");

        StringAssert.Contains(peers, "public sealed class SplitViewPaneRoot : Border");
        StringAssert.Contains(peers, "patternInterface == PatternInterface.Window && IsWindowContextEnabled");
        StringAssert.Contains(peers, "return AutomationControlType.Window;");
        StringAssert.Contains(peers, "public bool IsModal => true;");
        StringAssert.Contains(peers, "WindowInteractionState.Running");
        StringAssert.Contains(peers, "public sealed class SplitViewLightDismissLayer : FrameworkElement");
        StringAssert.Contains(peers, "patternInterface == PatternInterface.Invoke && IsLightDismissEnabled");
        StringAssert.Contains(peers, "return AutomationControlType.Button;");
        StringAssert.Contains(peers, "return \"Close\";");
        StringAssert.Contains(peers, "return \"LightDismiss\";");
        StringAssert.Contains(peers, "GetSplitView().InvokeLightDismissForAutomation();");

        StringAssert.Contains(template, "<local:SplitViewPaneRoot");
        StringAssert.Contains(template, "<local:SplitViewLightDismissLayer");
        StringAssert.Contains(template, "x:Name=\"PaneRoot\"");
        StringAssert.Contains(template, "x:Name=\"LightDismissLayer\"");
        StringAssert.Contains(template, "x:Name=\"OpenInlineRight\"");

        StringAssert.Contains(galleryFactory, "A basic SplitView.");
        StringAssert.Contains(galleryFactory, "Name = \"splitView\"");
        StringAssert.Contains(galleryFactory, "MaxWidth = 400");
        StringAssert.Contains(galleryFactory, "Height = 300");
        StringAssert.Contains(galleryFactory, "Name = \"togglePaneButton\"");
        StringAssert.Contains(galleryFactory, "OffContent = \"Left\"");
        StringAssert.Contains(galleryFactory, "OnContent = \"Right\"");
        StringAssert.Contains(galleryFactory, "UpdateSplitViewNavLinkLayout");

        StringAssert.Contains(harness, "\"SplitView\" { return 4.0 }");
        StringAssert.Contains(harness, "\"SplitView\" { return 0 }");
        StringAssert.Contains(harness, "function New-SplitViewReferencePrimaryCrop");
        StringAssert.Contains(harness, "$paneRoot = Find-DescendantByAutomationId $sampleElement \"PaneRoot\"");
        StringAssert.Contains(harness, "$content = Find-DescendantByAutomationId $sampleElement \"content\"");
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
