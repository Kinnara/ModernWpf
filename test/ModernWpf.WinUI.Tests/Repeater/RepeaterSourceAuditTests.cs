using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class RepeaterSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3RepeaterParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "repeater-winui3-source-audit.md");
        var repeater = Read(repoRoot, "ModernWpf.Controls", "Repeater", "ItemsRepeater", "ItemsRepeater.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "Repeater", "Automation", "RepeaterAutomationPeer.cs");
        var automationTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "Repeater", "RepeaterAutomationPeerTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "CollectionsSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "262cf0f1f5dcbaf366ac2cb426713e4a961fc7be");
        StringAssert.Contains(audit, "ac8c220bb148d4dc5d40b22ed7e1d1e393dbeb07");
        StringAssert.Contains(audit, "9018b87dc3f914f70aac40d324f2d49511e7e3a7");
        StringAssert.Contains(audit, "132e2cdd30531603e613bb26b8139722e886a379");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");

        StringAssert.Contains(audit, "792c35888d49fbfe5b27d97ef4206f71103dcc9e");
        StringAssert.Contains(audit, "c57ed88e9fc7b5a9132b2bfe5d4f9254c8fa6ad7");
        StringAssert.Contains(audit, "166f14597dc2936c60046dc0af2a0068a70a76ac");
        StringAssert.Contains(audit, "915e1a81e6ae380fcdb56ee197226c5cf3fead9f");
        StringAssert.Contains(audit, "f52dc50df137855a8670b2c2bbe3cd21f7671e65");
        StringAssert.Contains(audit, "998206abb7b8d3b8b8c71a63e3e0f1419ccf9ed1");
        StringAssert.Contains(audit, "7d2d456fd321c14b38e7650fd8fb856c37a7a58f");
        StringAssert.Contains(audit, "eca86cc6c25e988c86d03d7ea03cbc01c436872f");
        StringAssert.Contains(audit, "7e3db62efac2dede6ad24f09ef167a721c9ab30c");
        StringAssert.Contains(audit, "510cb4b1982308d57ce0d555985a194f69565d8e");
        StringAssert.Contains(audit, "b6b72d5f01e6458d58fcd3bd9ede4a44ee75f75d");
        StringAssert.Contains(audit, "0b85a7fa228ea40777533ccfbe1404bb5808e2ea");
        StringAssert.Contains(audit, "e454262a237d2c279f94fb36ac48ed437b12256a");
        StringAssert.Contains(audit, "9b7b51307fb845bf4c6a5b4b79ec1e2088f615b8");

        StringAssert.Contains(audit, "WinUIGallery\\Samples\\ItemsRepeater\\ItemsRepeaterPage.xaml");
        Assert.IsFalse(audit.Contains("Samples\\ControlPages\\ItemsRepeaterPage", StringComparison.Ordinal));
        Assert.IsFalse(audit.Contains("`src\\controls", StringComparison.Ordinal));
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-203128-999-95628/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-203156-243-103388/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-203226-944/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-203249-206/report.md");
        StringAssert.Contains(audit, "exact `425x88`");
        StringAssert.Contains(audit, "`0.53`");
        StringAssert.Contains(audit, "`0.42`");
        StringAssert.Contains(audit, "`22.657` maximum local delta");
        StringAssert.Contains(audit, "`11.4`");

        StringAssert.Contains(repeater, "KeyboardNavigationMode.Once");
        StringAssert.Contains(repeater, "MaxStackLayoutIterations = 60");
        StringAssert.Contains(repeater, "return new RepeaterAutomationPeer(this);");
        StringAssert.Contains(peer, "return AutomationControlType.Group;");
        StringAssert.Contains(peer, "if (virtInfo.IsRealized)");
        StringAssert.Contains(peer, "realizedPeers.OrderBy(x => x.Item1)");
        StringAssert.Contains(automationTests, "AutomationPeerReportsOnlyRealizedChildrenInItemIndexOrderLikeWinUI");
        StringAssert.Contains(automationTests, "context.RecycleElement(element1);");
        StringAssert.Contains(automationTests, "Assert.AreSame(group.Children[1], owners[2]);");

        StringAssert.Contains(galleryFactory, "Basic, non-interactive items laid out by ItemsRepeater");
        StringAssert.Contains(galleryFactory, "Virtualized, Content-Heavy Layout with Filtering and Sorting");
        StringAssert.Contains(galleryFactory, "SystemControlPageBackgroundChromeLowBrush");
        StringAssert.Contains(galleryTests, "ItemsRepeaterSampleMatchesWinUIGalleryExamples");
        StringAssert.Contains(harness, "\"ItemsRepeater\" { return 1.0 }");
        StringAssert.Contains(harness, "\"ItemsRepeater\" { return 0 }");
        StringAssert.Contains(harness, "ItemsRepeater source bar rows");
    }

    [TestMethod]
    public void RepeaterSuppressionsAndCurrentLayoutStateGuardsStaySourceBacked()
    {
        var repoRoot = FindRepoRoot();
        var suppressionsPath = Path.Combine(repoRoot, "ModernWpf.Controls", "Repeater", "GlobalSuppressions.cs");
        var suppressions = File.ReadAllLines(suppressionsPath)
            .Where(line => line.Contains("[assembly:", StringComparison.Ordinal))
            .Where(line => line.Contains("SuppressMessage", StringComparison.Ordinal))
            .ToArray();
        var flowLayout = Read(repoRoot, "ModernWpf.Controls", "Repeater", "Layouts", "FlowLayout", "FlowLayout.cs");
        var uniformGridLayout = Read(repoRoot, "ModernWpf.Controls", "Repeater", "Layouts", "UniformGridLayout", "UniformGridLayout.cs");
        var audit = Read(repoRoot, "docs", "repeater-winui3-source-audit.md");

        Assert.IsTrue(suppressions.Any(), "Repeater source suppressions should remain explicit.");
        Assert.IsFalse(suppressions.Any(line => line.Contains("<Pending>", StringComparison.Ordinal)));
        Assert.IsFalse(suppressions.Any(line => !line.Contains("docs/repeater-winui3-source-audit.md", StringComparison.Ordinal)));
        StringAssert.Contains(flowLayout, "context.LayoutState is FlowLayoutState flowState");
        StringAssert.Contains(flowLayout, "flowState.FlowAlgorithm.OnItemsSourceChanged(source, args, context);");
        StringAssert.Contains(uniformGridLayout, "context.LayoutState is UniformGridLayoutState gridState");
        StringAssert.Contains(uniformGridLayout, "gridState.FlowAlgorithm.OnItemsSourceChanged(source, args, context);");
        StringAssert.Contains(audit, "ViewportManagerDownLevel");
        StringAssert.Contains(audit, "WPF has neither effective-viewport services nor");
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
