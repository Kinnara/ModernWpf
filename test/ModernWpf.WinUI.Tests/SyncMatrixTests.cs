using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests;

[TestClass]
public class SyncMatrixTests
{
    [TestMethod]
    public void WinUI287SyncMatrixDocumentsSourceAndTestPolicy()
    {
        var repoRoot = FindRepoRoot();
        var matrixPath = Path.Combine(repoRoot, "docs", "winui2-2.8.7-sync.md");

        Assert.IsTrue(File.Exists(matrixPath), "Missing WinUI 2.8.7 sync matrix.");

        var matrix = File.ReadAllText(matrixPath);
        StringAssert.Contains(matrix, "v2.8.7");
        StringAssert.Contains(matrix, "232a16e5ddfc22c9a1b79a2c51abeb9a39a94494");
        StringAssert.Contains(matrix, "ModernWpf.WinUI.Tests");
        StringAssert.Contains(matrix, "Retired Local Tests");
        Assert.IsFalse(matrix.Contains("| Pending |"), "Sync matrix still has a table row whose status is Pending.");
        Assert.IsFalse(matrix.Contains("remain pending", StringComparison.OrdinalIgnoreCase), "Sync matrix still has unresolved pending wording.");
        Assert.IsFalse(matrix.Contains("pending/excluded", StringComparison.OrdinalIgnoreCase), "Sync matrix should document exclusions directly instead of using pending/excluded wording.");
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
}
