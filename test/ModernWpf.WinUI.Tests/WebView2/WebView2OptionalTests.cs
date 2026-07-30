using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using MuxBreadcrumbBar = ModernWpf.Controls.BreadcrumbBar;

namespace ModernWpf.WinUI.Tests.WebView2;

[TestClass]
public class WebView2OptionalTests
{
    [TestMethod]
    public void CoreLibrariesDoNotReferenceWebView2()
    {
        var coreAssemblies = new[]
        {
            typeof(XamlControlsResources).Assembly,
            typeof(MuxBreadcrumbBar).Assembly
        };

        foreach (var assembly in coreAssemblies)
        {
            AssertDoesNotReferenceWebView2(assembly);
        }
    }

    [TestMethod]
    public void GalleryDoesNotShipWebView2FallbackPage()
    {
        var repoRoot = FindRepoRoot();
        var sampleFactoryPath = Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "MediaSampleFactory.cs");
        var catalogPath = Path.Combine(repoRoot, "ModernWpf.Gallery", "Samples", "Data", "ControlInfoData.json");

        Assert.IsTrue(File.Exists(sampleFactoryPath), "Missing gallery media sample factory.");
        Assert.IsTrue(File.Exists(catalogPath), "Missing gallery control catalog.");

        var sampleFactory = File.ReadAllText(sampleFactoryPath);
        Assert.IsFalse(sampleFactory.Contains("WebView2", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(sampleFactory.Contains("CreateWebView2Surface", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(sampleFactory.Contains("Microsoft.Web.WebView2", StringComparison.OrdinalIgnoreCase));

        var catalog = File.ReadAllText(catalogPath);
        Assert.IsFalse(catalog.Contains("\"UniqueId\": \"WebView2\"", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(catalog.Contains("\"SourcePath\": \"/WebView2\"", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void HistoricalSyncMatrixDocumentsWebView2Exclusion()
    {
        var repoRoot = FindRepoRoot();
        var matrixPath = Path.Combine(repoRoot, "docs", "winui2-2.8.7-sync.md");

        Assert.IsTrue(File.Exists(matrixPath), "Missing historical WinUI 2.8.7 sync matrix.");

        var matrix = File.ReadAllText(matrixPath);
        StringAssert.Contains(matrix, "Historical snapshot only");
        StringAssert.Contains(matrix, "| WebView2 | Core excluded; gallery page pruned | No ModernWpf-owned control |");
        StringAssert.Contains(matrix, "WebView2 is not a ModernWpf-implemented WinUI control surface in this gallery scope");
    }

    private static void AssertDoesNotReferenceWebView2(Assembly assembly)
    {
        var webViewReferences = assembly
            .GetReferencedAssemblies()
            .Where(reference => reference.Name?.Contains("WebView2", StringComparison.OrdinalIgnoreCase) == true)
            .Select(reference => reference.FullName)
            .ToArray();

        Assert.AreEqual(
            0,
            webViewReferences.Length,
            $"{assembly.GetName().Name} should not reference WebView2: {string.Join(", ", webViewReferences)}");
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
