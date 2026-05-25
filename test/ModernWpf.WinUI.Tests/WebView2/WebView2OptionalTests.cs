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
    public void GalleryDocumentsWpfSafeWebView2Fallback()
    {
        var repoRoot = FindRepoRoot();
        var sampleFactoryPath = Path.Combine(repoRoot, "ModernWpf.Gallery", "Pages", "MediaSampleFactory.cs");
        var catalogPath = Path.Combine(repoRoot, "ModernWpf.Gallery", "Samples", "Data", "ControlInfoData.json");

        Assert.IsTrue(File.Exists(sampleFactoryPath), "Missing gallery media sample factory.");
        Assert.IsTrue(File.Exists(catalogPath), "Missing gallery control catalog.");

        var sampleFactory = File.ReadAllText(sampleFactoryPath);
        StringAssert.Contains(sampleFactory, "CreateWebView2Surface");
        StringAssert.Contains(sampleFactory, "https://learn.microsoft.com/windows/apps/winui/winui3/");
        StringAssert.Contains(sampleFactory, "GalleryAutomation.SampleElementId(\"WebView2\", \"WebView2\")");
        Assert.IsFalse(
            sampleFactory.Contains("Microsoft.Web.WebView2", StringComparison.OrdinalIgnoreCase),
            "The gallery fallback should not add a WebView2 package/runtime dependency.");

        var catalog = File.ReadAllText(catalogPath);
        StringAssert.Contains(catalog, "\"UniqueId\": \"WebView2\"");
        StringAssert.Contains(catalog, "\"SourcePath\": \"/WebView2\"");
    }

    [TestMethod]
    public void SyncMatrixDocumentsWebView2AsOptional()
    {
        var repoRoot = FindRepoRoot();
        var matrixPath = Path.Combine(repoRoot, "docs", "winui2-2.8.7-sync.md");

        Assert.IsTrue(File.Exists(matrixPath), "Missing WinUI 2.8.7 sync matrix.");

        var matrix = File.ReadAllText(matrixPath);
        StringAssert.Contains(matrix, "| WebView2 | Optional WPF gallery fallback; core excluded | Ported optional dependency/catalog guard |");
        StringAssert.Contains(matrix, "Edge/WebView2 runtime, registry, native CoreWebView2, network/install, and process-failure coverage");
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
