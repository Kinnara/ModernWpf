using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.LayoutCompatibility;

[TestClass]
public class WrapGridSourceAuditTests
{
    [TestMethod]
    public void CurrentVariableSizedWrapGridFamilySourcesAndWpfBoundariesArePinned()
    {
        var root = FindRepoRoot();
        var audit = Read(root, "docs", "variablesizedwrapgrid-winui3-source-audit.md");
        var variableGrid = Read(root, "ModernWpf", "Controls", "VariableSizedWrapGrid.cs");
        var wrapGrid = Read(root, "ModernWpf", "Controls", "WrapGrid.cs");
        var itemsWrapGrid = Read(root, "ModernWpf", "Controls", "ItemsWrapGrid.cs");
        var apiTests = Read(root, "test", "ModernWpf.WinUI.Tests", "LayoutCompatibility", "LayoutCompatibilityApiTests.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "ef4a8c6e956c6b94e77ba7fa8dc86295a8031d21");
        StringAssert.Contains(audit, "420130ae5500d902656caac9c09801f4570e1e84");
        StringAssert.Contains(audit, "15ccea7e686bcd117851e5b4fb32cef7f5ecd87d");
        StringAssert.Contains(audit, "6e3be4f2d11754b7f8b05c427109c4b4a2530a09");
        StringAssert.Contains(audit, "ad1199a7ff9c253e38c4fb922accbe0afffbf432");
        StringAssert.Contains(audit, "fad977b91352ef07c78365436b04c71bd9559fbf");
        StringAssert.Contains(audit, "ed736a383be9cb9eb7cf5e1be1450f6e93e527fd");
        StringAssert.Contains(audit, "45c7df191c4863b31d92b43b2b3ae4db11f98d25");
        Assert.IsFalse(audit.Contains("`src\\dxaml", StringComparison.Ordinal));

        StringAssert.Contains(variableGrid, "nameof(ItemHeight)");
        StringAssert.Contains(variableGrid, "nameof(ItemWidth)");
        StringAssert.Contains(variableGrid, "nameof(Orientation)");
        StringAssert.Contains(variableGrid, "nameof(HorizontalChildrenAlignment)");
        StringAssert.Contains(variableGrid, "nameof(VerticalChildrenAlignment)");
        StringAssert.Contains(variableGrid, "nameof(MaximumRowsOrColumns)");
        StringAssert.Contains(variableGrid, "RegisterAttached(");
        StringAssert.Contains(variableGrid, "GetPositiveSpan(GetRowSpan(child))");
        StringAssert.Contains(variableGrid, "GetPositiveSpan(GetColumnSpan(child))");
        StringAssert.Contains(variableGrid, "DetermineItemsPerLine");
        StringAssert.Contains(variableGrid, "DetermineLineLimit");
        StringAssert.Contains(variableGrid, "TryFindNextAvailableCell");

        StringAssert.Contains(wrapGrid, "class WrapGrid : VariableSizedWrapGrid");
        StringAssert.Contains(itemsWrapGrid, "class ItemsWrapGrid : WrapGrid");
        StringAssert.Contains(itemsWrapGrid, "nameof(GroupPadding)");
        StringAssert.Contains(itemsWrapGrid, "nameof(GroupHeaderPlacement)");
        StringAssert.Contains(itemsWrapGrid, "nameof(CacheLength)");
        StringAssert.Contains(itemsWrapGrid, "nameof(AreStickyGroupHeadersEnabled)");
        StringAssert.Contains(itemsWrapGrid, "FirstCacheIndex { get; private set; } = -1");
        StringAssert.Contains(itemsWrapGrid, "LastCacheIndex { get; private set; } = -1");
        StringAssert.Contains(itemsWrapGrid, "ScrollingDirection = PanelScrollingDirection.None");

        StringAssert.Contains(apiTests, "VariableSizedWrapGridStopsPlacementWhenSourceOccupancyMapIsFull");
        StringAssert.Contains(apiTests, "VariableSizedWrapGridSupportsRowAndColumnSpans");
        StringAssert.Contains(apiTests, "WrapGridWrapsHorizontallyAndVertically");
        StringAssert.Contains(apiTests, "ItemsWrapGridWrapsChildrenAndReportsRealizedRange");
        StringAssert.Contains(apiTests, "ItemsWrapGridParsesTemplateCompatibilityXaml");
    }

    private static string Read(string root, params string[] parts)
    {
        var path = root;
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
