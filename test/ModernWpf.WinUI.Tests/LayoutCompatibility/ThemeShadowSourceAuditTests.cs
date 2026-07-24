using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.LayoutCompatibility;

[TestClass]
public class ThemeShadowSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ThemeShadowParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "theme-shadow-winui3-parity.md");
        var coverage = Read(repoRoot, "docs", "theme-shadow-source-coverage.md");
        var chrome = Read(repoRoot, "ModernWpf", "Controls", "Primitives", "ThemeShadowChrome.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "StylesSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");
        var templateParity = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "TemplateParityTests.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "b695e427d8ae09616d03ed69530af1a08c46ae22");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "7165ba8601fff6c659e3fcef43bc34b534af57fe");
        StringAssert.Contains(audit, "e0a6dcfee7027abc006c53f52d62902327237187");
        StringAssert.Contains(audit, "310de7ceda1bf92d987aeec735247d6fc290b98d");
        StringAssert.Contains(audit, "28612e3f793b0e7359eae37ea6d5c2b2b1f9af82");
        StringAssert.Contains(audit, "505ff2514950ef5277b3f682d09226055f46c0c1");
        StringAssert.Contains(audit, "3eb777c950e0418fc5a25c258f8d313df0ee3c37");
        StringAssert.Contains(audit, "b5425fcc7b819e907acbbd756371f8eaa43c4fb9");
        StringAssert.Contains(audit, "b734f335f88dd2a3ee071c64973cce7b83487e1b");
        StringAssert.Contains(audit, "958fddf5359835ee0be26f3d0100545b579b3e24");
        StringAssert.Contains(audit, "7056132b065739ffc1fe8055a9a17bedc78235b5");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-140015-132-64340/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-140046-304-39564/report.md");
        StringAssert.Contains(audit, "| `272x272` / `272x272` | `0.22` |");
        StringAssert.Contains(audit, "| `272x272` / `272x272` | `0.12` |");

        StringAssert.Contains(coverage, "dxaml\\xcp\\components\\graphics\\ProjectedShadowManager.cpp");
        StringAssert.Contains(coverage, "walks open popups newest-first");
        Assert.IsFalse(coverage.Contains("`src\\", StringComparison.Ordinal));

        StringAssert.Contains(chrome, "new Thickness(4, 1, 4, 8)");
        StringAssert.Contains(chrome, "new Thickness(10, 2, 10, 18)");
        StringAssert.Contains(chrome, "double elevation = Math.Min(64, Math.Max(0, depth) / 2);");
        StringAssert.Contains(chrome, "directionalYOffset = elevation * 0.5;");
        StringAssert.Contains(chrome, "ambientOpacity = 0.37;");
        StringAssert.Contains(chrome, "directionalOpacity = 0.19;");
        StringAssert.Contains(chrome, "IsHitTestVisible = false");
        StringAssert.Contains(chrome, "_background.Opacity = opacity;");
        StringAssert.Contains(chrome, "_background.InvalidateVisual();");
        StringAssert.Contains(chrome, "Theme = ThemeManager.GetActualTheme(this)");
        StringAssert.Contains(chrome, "_shadow.Theme = ThemeManager.GetActualTheme(this);");
        StringAssert.Contains(chrome, "public ElementTheme Theme");
        StringAssert.Contains(chrome, "private ElementTheme _theme = ElementTheme.Light;");

        StringAssert.Contains(galleryFactory, "ThemeShadow applied to a Border");
        StringAssert.Contains(galleryFactory, "Name = \"ShadowCastGrid\"");
        StringAssert.Contains(galleryFactory, "Name = \"ShadowRect\"");
        StringAssert.Contains(galleryFactory, "Depth = 32");
        StringAssert.Contains(galleryFactory, "TranslationZ = 32");
        StringAssert.Contains(galleryFactory, "Padding = new Thickness(36)");
        StringAssert.Contains(galleryFactory, "MinWidth = 272");
        StringAssert.Contains(galleryFactory, "Maximum = 64");

        StringAssert.Contains(harness, "function New-ThemeShadowReferencePrimaryCrop");
        StringAssert.Contains(harness, "\"ThemeShadow\" { return 0.3 }");
        StringAssert.Contains(harness, "\"ThemeShadow\" { return 0 }");
        StringAssert.Contains(templateParity, "@\"dxaml\\xcp\\components\\graphics\\ThemeShadow.cpp\"");
        Assert.IsFalse(templateParity.Contains("@\"src\\dxaml\\xcp\\components\\graphics\\ThemeShadow.cpp\"", StringComparison.Ordinal));

        var layoutCompatibility = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "LayoutCompatibility", "LayoutCompatibilityApiTests.cs");
        StringAssert.Contains(layoutCompatibility, "Directory.Exists(Path.Combine(sourceRoot, \"dxaml\"))");
        StringAssert.Contains(layoutCompatibility, "Path.Combine(sourceRoot, \"src\")");
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
