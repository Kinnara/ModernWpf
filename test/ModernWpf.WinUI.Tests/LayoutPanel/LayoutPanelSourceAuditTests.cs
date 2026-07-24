using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.LayoutPanels;

[TestClass]
public class LayoutPanelSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3LayoutPanelParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "layoutpanel-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "LayoutPanel", "LayoutPanel.cs");
        var context = Read(repoRoot, "ModernWpf.Controls", "LayoutPanel", "LayoutPanelLayoutContext.cs");
        var properties = Read(repoRoot, "ModernWpf.Controls", "LayoutPanel", "LayoutPanel.properties.g.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "controls/dev/LayoutPanel/LayoutPanel.cpp");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "no LayoutPanel sample or page");
        StringAssert.Contains(audit, "185fabd426b2d246185ff5c9b90bcd5447655d18");
        StringAssert.Contains(audit, "037d7e4028b6f03f91f7f6442e0b20dfa7e4b249");

        StringAssert.Contains(properties, "public static readonly DependencyProperty BorderBrushProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty BorderThicknessProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty CornerRadiusProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty LayoutProperty");
        StringAssert.Contains(properties, "public static readonly DependencyProperty PaddingProperty");

        StringAssert.Contains(control, "adjustedSize.Width = Math.Max(0.0, adjustedSize.Width);");
        StringAssert.Contains(control, "layout.Measure(m_layoutContext, adjustedSize)");
        StringAssert.Contains(control, "layout.Arrange(m_layoutContext, adjustedSize)");
        StringAssert.Contains(control, "oldValue.UninitializeForContext(m_layoutContext);");
        StringAssert.Contains(control, "oldValue.MeasureInvalidated -= InvalidateMeasureForLayout;");
        StringAssert.Contains(control, "newValue.InitializeForContext(m_layoutContext);");
        StringAssert.Contains(control, "newValue.ArrangeInvalidated += InvalidateArrangeForLayout;");
        StringAssert.Contains(control, "LayoutChromeHelper.DrawChrome(");
        StringAssert.Contains(control, "LayoutChromeHelper.CreateRoundedLayoutClip(");
        StringAssert.Contains(control, "LayoutChromeHelper.FillContainsRoundedRectangle(");

        StringAssert.Contains(context, "protected override IReadOnlyList<UIElement> ChildrenCore");
        StringAssert.Contains(context, "protected override object LayoutStateCore");
        StringAssert.Contains(context, "new UIElementCollectionView(GetOwner().Children)");
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
