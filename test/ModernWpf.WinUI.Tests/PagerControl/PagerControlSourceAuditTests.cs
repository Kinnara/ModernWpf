using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.PagerControl;

[TestClass]
public class PagerControlSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3PagerControlParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "pagercontrol-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "PagerControl", "PagerControl.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "PagerControl", "PagerControl.xaml");
        var peer = Read(repoRoot, "ModernWpf.Controls", "PagerControl", "PagerControlAutomationPeer.cs");
        var resources = Read(repoRoot, "ModernWpf.Controls", "PagerControl", "Strings", "Resources.resx");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "controls/dev/PagerControl/PagerControl.cpp");
        StringAssert.Contains(audit, "controls/dev/PagerControl/PagerControl_themeresources_perf2026.xaml");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "no PagerControl sample or page");
        StringAssert.Contains(audit, "80adea00055e7fad37e7e0f08770a80d831fdd11");
        StringAssert.Contains(audit, "51164cb404d23f4b61b76fd9b0c8059bfb4b0f04");

        StringAssert.Contains(control, "private const int AutoDisplayModeNumberOfPagesThreshold = 10;");
        StringAssert.Contains(control, "numberOfPages < AutoDisplayModeNumberOfPagesThreshold ? ComboBoxVisibleVisualState : NumberBoxVisibleVisualState");
        StringAssert.Contains(control, "UpdateNumberPanel(NumberOfPages);");
        StringAssert.Contains(control, "AutomationProperties.SetPositionInSet(button, pageNumber);");
        StringAssert.Contains(control, "AutomationProperties.SetSizeOfSet(button, numberOfPages);");

        StringAssert.Contains(template, "x:Name=\"RootGrid\"");
        StringAssert.Contains(template, "x:Name=\"NumberBoxDisplay\"");
        StringAssert.Contains(template, "x:Name=\"ComboBoxDisplay\"");
        StringAssert.Contains(template, "x:Name=\"NumberPanelItemsRepeater\"");
        StringAssert.Contains(template, "x:Name=\"NumberPanelCurrentPageIndicator\"");
        StringAssert.Contains(template, "<ui:VisualStateEx.Setters>");
        StringAssert.Contains(template, "x:Key=\"PagerControlTemplateNumberPanelButtonStyle\"");

        StringAssert.Contains(peer, "AutomationControlType.Menu");
        StringAssert.Contains(peer, "PatternInterface.Selection ? this");
        StringAssert.Contains(peer, "public bool CanSelectMultiple => false;");
        StringAssert.Contains(peer, "public bool IsSelectionRequired => true;");
        StringAssert.Contains(peer, "return new IRawElementProviderSimple[0];");

        StringAssert.Contains(resources, "<value>First page</value>");
        StringAssert.Contains(resources, "<value>Previous page</value>");
        StringAssert.Contains(resources, "<value>Next page</value>");
        StringAssert.Contains(resources, "<value>Last page</value>");
        StringAssert.Contains(resources, "<value>Page</value>");
        StringAssert.Contains(resources, "<value>of</value>");
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
