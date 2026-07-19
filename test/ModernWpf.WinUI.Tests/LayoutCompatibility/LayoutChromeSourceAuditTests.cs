using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.LayoutCompatibility;

[TestClass]
public class LayoutChromeSourceAuditTests
{
    [TestMethod]
    public void CurrentBorderGridStackPanelAndContentPresenterSourcesArePinned()
    {
        var root = FindRepoRoot();
        var audit = Read(root, "docs", "layout-chrome-winui3-source-audit.md");
        var chrome = Read(root, "ModernWpf", "Controls", "LayoutChromeHelper.cs");
        var grid = Read(root, "ModernWpf", "Controls", "GridEx.cs");
        var stack = Read(root, "ModernWpf", "Controls", "StackPanelEx.cs");
        var presenter = Read(root, "ModernWpf", "Controls", "ContentPresenterEx.cs");
        var control = Read(root, "ModernWpf", "Controls", "ContentControlEx.cs");
        var apiTests = Read(root, "test", "ModernWpf.WinUI.Tests", "LayoutCompatibility", "LayoutCompatibilityApiTests.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "3f662f6ce1158b1fccae058a5b825467b598ec5b");
        StringAssert.Contains(audit, "4a50cd9ff83c35528008882cadbf69c3ded571bd");
        StringAssert.Contains(audit, "0b9e007cec1ce9bd6951da7b56ec017fb4175ce1");
        StringAssert.Contains(audit, "63ed1d27c9575ef452b8c1275b059a55ef7c1a89");
        StringAssert.Contains(audit, "08cf1b3e1a94cf5d07c696f34977ac357c548e61");
        StringAssert.Contains(audit, "ad1199a7ff9c253e38c4fb922accbe0afffbf432");
        StringAssert.Contains(audit, "1cf9aa4a290f5c37d7934529f166c76275a4e082");
        StringAssert.Contains(audit, "6081798c3acda10eb2396f16c93a72b1093355bd");
        StringAssert.Contains(audit, "22d5c4696e9b81675bb33088518bed08ab22f656");
        StringAssert.Contains(audit, "0d03248799283623615e12916585e11e30868adc");
        StringAssert.Contains(audit, "OptimizeApplyStyles");
        Assert.IsFalse(audit.Contains("`src\\dxaml", StringComparison.Ordinal));

        StringAssert.Contains(chrome, "GeometryCombineMode.Exclude");
        StringAssert.Contains(chrome, "GeometryCombineMode.Intersect");
        StringAssert.Contains(chrome, "backgroundSizing == BackgroundSizing.OuterBorderEdge");
        StringAssert.Contains(chrome, "Deflate(outerRect, borderThickness)");

        StringAssert.Contains(grid, "nameof(ColumnSpacing)");
        StringAssert.Contains(grid, "nameof(RowSpacing)");
        StringAssert.Contains(grid, "GetCombinedColumnSpacing");
        StringAssert.Contains(grid, "GetCombinedRowSpacing");
        StringAssert.Contains(grid, "ArrangeSpacingChildren");
        StringAssert.Contains(grid, "GetRangeSize");

        StringAssert.Contains(stack, "class StackPanelEx : Panel, IScrollSnapPointsInfo");
        StringAssert.Contains(stack, "nameof(Spacing)");
        StringAssert.Contains(stack, "GetIrregularSnapPoints");
        StringAssert.Contains(stack, "GetRegularSnapPoints");
        StringAssert.Contains(stack, "child.Visibility != Visibility.Collapsed");

        StringAssert.Contains(presenter, "ControlHelper.CharacterSpacingProperty.AddOwner");
        StringAssert.Contains(presenter, "ControlHelper.IsTextScaleFactorEnabledProperty.AddOwner");
        StringAssert.Contains(presenter, "TextBlock.TextWrappingProperty.AddOwner");
        StringAssert.Contains(presenter, "Block.LineStackingStrategyProperty.AddOwner");
        StringAssert.Contains(presenter, "ApplyMaxLines(_textBlock)");
        StringAssert.Contains(control, "ControlHelper.CharacterSpacingProperty.AddOwner");
        StringAssert.Contains(control, "ControlHelper.IsTextScaleFactorEnabledProperty.AddOwner");

        StringAssert.Contains(apiTests, "BorderExOuterBackgroundSizingPaintsBehindBorder");
        StringAssert.Contains(apiTests, "RoundedLayoutClipPreservesBaseLayoutClip");
        StringAssert.Contains(apiTests, "ContentPresenterExPushesSupportedTextPropertiesToDefaultTextBlock");
        StringAssert.Contains(apiTests, "StackPanelExComputesWinUISnapPoints");
        StringAssert.Contains(apiTests, "StackPanelExHorizontalSpacingSkipsCollapsedChildren");
        StringAssert.Contains(apiTests, "GridExNegativeSpacingDistributesSpannedAutoDesiredSize");
        StringAssert.Contains(apiTests, "GridExPositiveSpacingHandlesStarSpans");
        StringAssert.Contains(apiTests, "root.Children.Clear();");
        StringAssert.Contains(audit, "windowless visual");
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
