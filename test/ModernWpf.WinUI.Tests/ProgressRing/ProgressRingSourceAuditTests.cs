using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.ProgressRing;

[TestClass]
public class ProgressRingSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ProgressRingParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "progressring-winui3-source-audit.md");
        var progressRing = Read(repoRoot, "ModernWpf.Controls", "ProgressRing", "ProgressRing.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "ProgressRing", "ProgressRing.xaml");
        var indicator = Read(repoRoot, "ModernWpf.Controls", "ProgressRing", "ProgressRingIndicator.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "ProgressRing", "ProgressRingAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "StatusInfoSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "beabd047460bf5d43a41fcf8bddf7730188bd5a7");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "46aaf1a82be04175f7eb6a8f6f2481ac7db7be15");
        StringAssert.Contains(audit, "df15b1794bbd0cda30279b5ab0674cafa544a0cd");
        StringAssert.Contains(audit, "f3fa38ea83ea910200a6a539140a379b128bbf94");
        StringAssert.Contains(audit, "cd10c66d1322c5b4620bdcdadcfd5bb6c49fb856");
        StringAssert.Contains(audit, "7d5234db42e92d9d4411009bd0086e7a2a1f0e31");
        StringAssert.Contains(audit, "48b0ac924c6010a5bc4ba7aa4d588c200d807907");
        StringAssert.Contains(audit, "5a71dd173f31e68611a1fd05300c5c99c1a021d1");
        StringAssert.Contains(audit, "8b82ad49e3cdf29bb5f5906833168daf882de368");
        StringAssert.Contains(audit, "ec4263532cd9e028df8be2ca922d313b4a1d72d0");
        StringAssert.Contains(audit, "7c30cfaf363ecbfc49a399806b79bc8e5804e5ca");
        StringAssert.Contains(audit, "e68db7715d70a8f59ed66cf02bfb81ba9ffb02af");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-111542-017-86572/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-111628-842-2556/report.md");
        StringAssert.Contains(audit, "| `60x60` / `60x60` | `0.64` |");
        StringAssert.Contains(audit, "| `60x60` / `60x60` | `0.63` |");

        StringAssert.Contains(progressRing, "m_layoutRoot = GetTemplateChild(s_LayoutRootName) as FrameworkElement;");
        StringAssert.Contains(progressRing, "UpdateLottieProgress();");
        StringAssert.Contains(progressRing, "VisualStateManager.GoToState(this, IsIndeterminate ? s_ActiveStateName : s_DeterminateActiveStateName, true);");
        StringAssert.Contains(progressRing, "double width = ActualWidth;");
        StringAssert.Contains(progressRing, "if (!double.IsNaN(value) && !IsInBounds(value))");
        StringAssert.Contains(template, "x:Name=\"LayoutRoot\"");
        StringAssert.Contains(template, "x:Name=\"LottiePlayer\"");
        StringAssert.Contains(template, "Target=\"LayoutRoot.Opacity\"");
        StringAssert.Contains(template, "x:Name=\"DeterminateActive\"");
        StringAssert.Contains(template, "x:Name=\"Active\"");
        StringAssert.Contains(indicator, "private const double LottieShapeScale = 1.77;");
        StringAssert.Contains(indicator, "private const double LottieEllipseRadius = 8.0;");
        StringAssert.Contains(indicator, "private const double LottieStrokeThickness = 1.5;");
        StringAssert.Contains(peer, "return AutomationControlType.ProgressBar;");
        StringAssert.Contains(peer, "if (progressRing.IsActive && progressRing.IsIndeterminate)");
        StringAssert.Contains(peer, "if (Owner is ProgressRing progressRing && !progressRing.IsIndeterminate)");

        StringAssert.Contains(galleryFactory, "<ProgressRing IsActive=\"\"$(IsActive)\"\" $(Background)/>");
        StringAssert.Contains(galleryFactory, "Name = \"ProgressRing1\"");
        StringAssert.Contains(galleryFactory, "Name = \"ProgressRing2\"");
        StringAssert.Contains(galleryFactory, "sender.Value = 0;");
        StringAssert.Contains(harness, "function Set-ProgressRingDeterminateValue");
        StringAssert.Contains(harness, "\"ProgressRing\" { return 1.0 }");
        StringAssert.Contains(harness, "\"ProgressRing\" { return 0 }");
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
