using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.TeachingTip;

[TestClass]
public class TeachingTipSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3TeachingTipParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "teachingtip-winui3-source-audit.md");
        var template = Read(repoRoot, "ModernWpf.Controls", "TeachingTip", "TeachingTip.xaml");
        var control = Read(repoRoot, "ModernWpf.Controls", "TeachingTip", "TeachingTip.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "TeachingTip", "TeachingTipAutomationPeer.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "controls/dev/TeachingTip/TeachingTip_themeresources.xaml");
        StringAssert.Contains(audit, "controls/dev/TeachingTip/TeachingTip_themeresources_perf2026.xaml");
        StringAssert.Contains(audit, "c7e2f98d978c81c2b7b0054eb042a6f8f816ec9c");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        Assert.IsFalse(audit.Contains("`src\\controls\\dev\\TeachingTip", StringComparison.Ordinal));

        StringAssert.Contains(template, "<sys:Double x:Key=\"TeachingTipMinHeight\">40</sys:Double>");
        StringAssert.Contains(template, "<sys:Double x:Key=\"TeachingTipMinWidth\">320</sys:Double>");
        StringAssert.Contains(template, "<sys:Double x:Key=\"TeachingTipMaxWidth\">336</sys:Double>");
        StringAssert.Contains(template, "<Thickness x:Key=\"TeachingTipContentMargin\">12</Thickness>");
        StringAssert.Contains(template, "<GridLength x:Key=\"TeachingTipTailMargin\">10</GridLength>");
        StringAssert.Contains(template, "AutomationProperties.AutomationId=\"ContentRootGrid\"");
        StringAssert.Contains(template, "FontWeight=\"SemiBold\"");

        StringAssert.Contains(control, "return new TeachingTipAutomationPeer(this);");
        StringAssert.Contains(control, "private void SetPopupAutomationProperties()");
        StringAssert.Contains(control, "e.Property == System.Windows.Automation.AutomationProperties.NameProperty");
        StringAssert.Contains(control, "e.Property == System.Windows.Automation.AutomationProperties.AutomationIdProperty");
        StringAssert.Contains(control, "System.Windows.Automation.AutomationProperties.SetName(_popup");
        StringAssert.Contains(control, "System.Windows.Automation.AutomationProperties.SetAutomationId(");

        StringAssert.Contains(peer, "AutomationControlType.Window");
        StringAssert.Contains(peer, "AutomationControlType.Pane");
        StringAssert.Contains(peer, "PatternInterface.Window ? this");
        StringAssert.Contains(peer, "public bool IsModal => GetImpl().IsLightDismissEnabled;");
        StringAssert.Contains(peer, "GetImpl().SetCurrentValue(TeachingTip.IsOpenProperty, false);");

        StringAssert.Contains(harness, "function Save-TeachingTipOpenSurfaceCrop(");
        StringAssert.Contains(harness, "Source = \"TeachingTipSurface\"");
        StringAssert.Contains(harness, "$control -eq \"TeachingTip\"");
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
