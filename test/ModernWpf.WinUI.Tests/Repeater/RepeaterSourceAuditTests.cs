using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class RepeaterSourceAuditTests
{
    [TestMethod]
    public void RepeaterSuppressionsUseExplicitSourceBackedJustifications()
    {
        var repoRoot = FindRepoRoot();
        var suppressionsPath = Path.Combine(repoRoot, "ModernWpf.Controls", "Repeater", "GlobalSuppressions.cs");
        var auditPath = Path.Combine(repoRoot, "docs", "repeater-winui3-source-audit.md");

        var suppressions = File.ReadAllLines(suppressionsPath);
        var suppressionRows = suppressions
            .Where(line => line.Contains("[assembly:", StringComparison.Ordinal))
            .Where(line => line.Contains("SuppressMessage", StringComparison.Ordinal))
            .ToArray();
        var pendingRows = suppressionRows
            .Where(line => line.Contains("<Pending>", StringComparison.Ordinal))
            .ToArray();
        var undocumentedRows = suppressionRows
            .Where(line => !line.Contains("docs/repeater-winui3-source-audit.md", StringComparison.Ordinal))
            .ToArray();
        var auditText = File.ReadAllText(auditPath);

        Assert.IsTrue(suppressionRows.Any(), "Repeater source suppressions should remain explicit.");
        Assert.IsFalse(pendingRows.Any(), "Repeater source suppressions should not use pending analyzer justifications.");
        Assert.IsFalse(undocumentedRows.Any(), "Repeater source suppressions should point at the source audit.");
        StringAssert.Contains(auditText, "GlobalSuppressions.cs");
    }

    [TestMethod]
    public void CurrentFlowLayoutsGuardMissingLayoutStateLikeWinUISource()
    {
        var repoRoot = FindRepoRoot();
        var flowLayout = File.ReadAllText(Path.Combine(
            repoRoot,
            "ModernWpf.Controls",
            "Repeater",
            "Layouts",
            "FlowLayout",
            "FlowLayout.cs"));
        var uniformGridLayout = File.ReadAllText(Path.Combine(
            repoRoot,
            "ModernWpf.Controls",
            "Repeater",
            "Layouts",
            "UniformGridLayout",
            "UniformGridLayout.cs"));
        var audit = File.ReadAllText(Path.Combine(repoRoot, "docs", "repeater-winui3-source-audit.md"));

        StringAssert.Contains(flowLayout, "context.LayoutState is FlowLayoutState flowState");
        StringAssert.Contains(flowLayout, "flowState.FlowAlgorithm.OnItemsSourceChanged(source, args, context);");
        StringAssert.Contains(uniformGridLayout, "context.LayoutState is UniformGridLayoutState gridState");
        StringAssert.Contains(uniformGridLayout, "gridState.FlowAlgorithm.OnItemsSourceChanged(source, args, context);");
        StringAssert.Contains(audit, "3cae15f071f1ab8565f9a7592dbf27f04bafe651");
        StringAssert.Contains(audit, "ViewportManagerDownLevel");
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
