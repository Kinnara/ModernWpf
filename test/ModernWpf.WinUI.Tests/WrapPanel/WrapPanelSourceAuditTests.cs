using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.WrapPanels;

[TestClass]
public class WrapPanelSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3WrapPanelParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "wrappanel-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "WrapPanel", "WrapPanel.cs");
        var properties = Read(repoRoot, "ModernWpf.Controls", "WrapPanel", "WrapPanel.properties.g.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "controls/dev/WrapPanel/WrapPanel.cpp");
        StringAssert.Contains(audit, "controls/dev/Generated/WrapPanel.properties.cpp");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "no WrapPanel sample or page");
        StringAssert.Contains(audit, "885377d69ebf34437f260498269ced9c8f1abd81");
        StringAssert.Contains(audit, "74252f6a4875ebb840f219e9419a61239c1f9b97");

        StringAssert.Contains(properties, "public static readonly DependencyProperty PaddingProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty ItemSpacingProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty LineSpacingProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty OrientationProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty ItemsStretchProperty");
        StringAssert.Contains(properties, "FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange");

        StringAssert.Contains(control, "Math.Max(0, availableSize.Width - padding.Left - padding.Right)");
        StringAssert.Contains(control, "child.Measure(childAvailableSize);");
        StringAssert.Contains(control, "finalSize.Width < DesiredSize.Width");
        StringAssert.Contains(control, "finalSize.Height < DesiredSize.Height");
        StringAssert.Contains(control, "var child = GetNextVisibleChild(ref childIndex);");
        StringAssert.Contains(control, "child.Arrange(Rect.Empty);");
        StringAssert.Contains(control, "if (child.Visibility == Visibility.Collapsed)");
        StringAssert.Contains(control, "if (isLast && ItemsStretch == WrapPanelItemsStretch.Last)");
        StringAssert.Contains(control, "var newV = Math.Max(Size.V, size.V);");
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
