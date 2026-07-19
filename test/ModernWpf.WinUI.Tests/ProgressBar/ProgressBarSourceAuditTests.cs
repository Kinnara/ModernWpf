using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.ProgressBars;

[TestClass]
public class ProgressBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ProgressBarParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "progressbar-winui3-source-audit.md");
        var progressBar = Read(repoRoot, "ModernWpf", "ProgressBar", "ProgressBar.cs");
        var template = Read(repoRoot, "ModernWpf", "ProgressBar", "ProgressBar.xaml");
        var rasterOverlay = Read(repoRoot, "ModernWpf", "Controls", "Primitives", "ProgressBarIndicatorRasterOverlay.cs");
        var peer = Read(repoRoot, "ModernWpf", "ProgressBar", "ProgressBarAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "StatusInfoSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "beabd047460bf5d43a41fcf8bddf7730188bd5a7");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "7ab65894f91ca59504729240d044cdf468d266cc");
        StringAssert.Contains(audit, "5f188ab8b3d45632d74a90b742299e0412e45feb");
        StringAssert.Contains(audit, "f2cf8b8edbc50ab21d0f2d5f460f30b92d662a36");
        StringAssert.Contains(audit, "cca6b40d87c1ab0e473b108132a13b434c6de9a3");
        StringAssert.Contains(audit, "9ccf87853c457992f7f08a5b7cac71bb3e51315a");
        StringAssert.Contains(audit, "60f20a78ea4a4f320d1844b70f6026f4463c3801");
        StringAssert.Contains(audit, "dbaba364fe27de13c460db24e7824bbeb2645c54");
        StringAssert.Contains(audit, "5812f9901151066051c65f46a1bf98967c7ab0e4");
        StringAssert.Contains(audit, "dcf4e7e0bf4bf5252b5b12c74fe23b2f18a1c670");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-105525-867-12132/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-105621-848-29676/report.md");
        StringAssert.Contains(audit, "| `130x3` / `130x3` | `0.43` |");

        StringAssert.Contains(progressBar, "if (IsIndeterminate && Visibility == Visibility.Visible)");
        StringAssert.Contains(progressBar, "double paddingAndBorderWidth = padding.Left + padding.Right + roundedBorderWidth;");
        StringAssert.Contains(progressBar, "m_indeterminateProgressBarIndicator2.Width = maxIndicatorWidth;");
        StringAssert.Contains(progressBar, "templateSettings.ContainerAnimationMidPosition = 0;");
        StringAssert.Contains(template, "x:Name=\"DeterminateProgressBarIndicator\"");
        StringAssert.Contains(template, "x:Name=\"IndeterminateProgressBarIndicator2\"");
        StringAssert.Contains(template, "Target=\"DeterminateProgressBarIndicator.(Shape.Fill).(SolidColorBrush.Color)\"");
        StringAssert.Contains(template, "x:Name=\"DeterminateProgressBarIndicatorRasterOverlay\"");
        StringAssert.Contains(template, "RenderTransform=\"{Binding RenderTransform, ElementName=DeterminateProgressBarIndicator}\"");
        StringAssert.Contains(rasterOverlay, "var inset = 2.0 * physicalPixelWidth;");
        StringAssert.Contains(rasterOverlay, "drawingContext.PushGuidelineSet(guidelines);");
        StringAssert.Contains(peer, "return ResourceAccessor.GetLocalizedStringResource(SR_ProgressBarIndeterminateStatus) + name;");

        StringAssert.Contains(galleryFactory, "case \"WinUIProgressBar\":");
        StringAssert.Contains(galleryFactory, "GalleryAutomation.SampleElementId(\"WinUIProgressBar\", \"DeterminateProgressBar\")");
        StringAssert.Contains(harness, "function Set-ProgressBarDeterminateValue");
        StringAssert.Contains(harness, "if ($control -eq \"WinUIProgressBar\" -and $app -eq \"WinUI3\")");
        StringAssert.Contains(harness, "\"WinUIProgressBar\" { return 2.0 }");
        StringAssert.Contains(harness, "\"WinUIProgressBar\" { return 0 }");
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
