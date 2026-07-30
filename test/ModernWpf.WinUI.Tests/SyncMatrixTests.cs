using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests;

[TestClass]
public class SyncMatrixTests
{
    [TestMethod]
    public void WinUI287SyncMatrixIsAPinnedHistoricalSnapshot()
    {
        var repoRoot = FindRepoRoot();
        var matrixPath = Path.Combine(repoRoot, "docs", "winui2-2.8.7-sync.md");

        Assert.IsTrue(File.Exists(matrixPath), "Missing historical WinUI 2.8.7 sync matrix.");

        var matrix = File.ReadAllText(matrixPath);
        StringAssert.Contains(matrix, "Historical snapshot only");
        StringAssert.Contains(matrix, "must not be used as the current behavior, API-shape, resource,");
        StringAssert.Contains(matrix, "docs/winui3-source-parity.md");
        StringAssert.Contains(matrix, "docs/winui3-control-source-coverage.md");
        StringAssert.Contains(matrix, "v2.8.7");
        StringAssert.Contains(matrix, "232a16e5ddfc22c9a1b79a2c51abeb9a39a94494");
        StringAssert.Contains(matrix, "ModernWpf.WinUI.Tests");
        StringAssert.Contains(matrix, "Retired Local Tests");
        Assert.IsFalse(matrix.Contains("| Pending |"), "Sync matrix still has a table row whose status is Pending.");
        Assert.IsFalse(matrix.Contains("remain pending", StringComparison.OrdinalIgnoreCase), "Sync matrix still has unresolved pending wording.");
        Assert.IsFalse(matrix.Contains("pending/excluded", StringComparison.OrdinalIgnoreCase), "Sync matrix should document exclusions directly instead of using pending/excluded wording.");

        AssertControlStatus(matrix, "AutoSuggestBox", "Source-backed WPF port");
        AssertControlStatus(matrix, "TeachingTip", "Source-backed WPF port");
        AssertControlStatus(matrix, "ColorPicker / ColorSpectrum", "Source-backed WPF port");
        AssertControlStatus(matrix, "ComboBox", "Official WPF Fluent-backed stock control");
        AssertControlStatus(matrix, "DataGrid", "Official WPF Fluent-backed stock control");
        AssertControlStatus(matrix, "Foundation navigation stock styles", "Official WPF Fluent-backed stock control family");
        AssertControlStatus(matrix, "Window shell", "Official WPF Fluent-backed shell substitution");
        AssertControlStatus(matrix, "TabView / stock WPF TabControl", "Official WPF Fluent-backed stock control");
        AssertControlStatus(matrix, "AnnotatedScrollBar", "Source-backed WPF port");
        AssertControlStatus(matrix, "RadioButtons", "Source-backed WPF port");
        AssertControlStatus(matrix, "SplitView", "Source-backed WPF port");
        AssertControlStatus(matrix, "CommandBar / AppBarButton / AppBarToggleButton / AppBarSeparator", "Source-backed WPF port");
        AssertControlStatus(matrix, "ContentDialog", "Source-backed WPF port");
        AssertControlStatus(matrix, "WrapPanel", "Source-backed WPF port");
        AssertControlStatus(matrix, "RadioMenuFlyoutItem", "Source-backed WPF port");
        AssertControlStatus(matrix, "LayoutPanel", "Source-backed WPF port");
        AssertControlStatus(matrix, "ProgressBar resources", "Source-backed WPF port");
        AssertControlStatus(matrix, "IconSource / ImageIcon", "Source-backed WPF port");
        AssertControlStatus(matrix, "MenuBar", "Source-backed WPF port");
        AssertControlStatus(matrix, "NavigationView", "Source-backed WPF port");
        AssertControlStatus(matrix, "Repeater / ItemsRepeater layouts", "Source-backed WPF port");
        AssertControlStatus(matrix, "Expander", "Official WPF Fluent-backed stock control");
    }

    [TestMethod]
    public void PreviewApiPolicyTreatsWinUIAsAuthorityWithoutFreezingPreviewOne()
    {
        var repoRoot = FindRepoRoot();
        var contractPath = Path.Combine(repoRoot, "docs", "public-api-contract-1x.md");

        Assert.IsTrue(File.Exists(contractPath), "Missing 1.x public API contract.");

        var contract = File.ReadAllText(contractPath);
        StringAssert.Contains(contract, "first audit, migration, and package-comparison");
        StringAssert.Contains(contract, "does not freeze later 1.0 previews");
        StringAssert.Contains(contract, "current applicable WinUI API shape is");
        StringAssert.Contains(contract, "Stable `1.0.0` establishes the SemVer compatibility baseline");
        StringAssert.Contains(contract, "ModernWpfPackageValidationBaselineVersion");
        StringAssert.Contains(contract, "ModernWpfPreviewAuditBaselineVersion");
    }

    public TestContext? TestContext { get; set; }

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

    private static void AssertControlStatus(string matrix, string controlName, string expectedStatus)
    {
        var expectedPrefix = $"| {controlName} | {expectedStatus} |";
        StringAssert.Contains(matrix, expectedPrefix, $"{controlName} should use the audited parity status.");
    }
}
