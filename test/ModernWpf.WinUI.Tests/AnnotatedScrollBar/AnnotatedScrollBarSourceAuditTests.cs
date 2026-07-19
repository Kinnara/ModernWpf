using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.AnnotatedScrollBar;

[TestClass]
public class AnnotatedScrollBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3AnnotatedScrollBarParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "annotatedscrollbar-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "AnnotatedScrollBar", "AnnotatedScrollBar.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "AnnotatedScrollBar", "AnnotatedScrollBar.xaml");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "AnnotatedScrollBar", "AnnotatedScrollBarApiTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "ScrollingSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "77360ea4bf813506ee75e1900c9f28f0b35d8495");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "c9ee884da938b6b4bdfa1ba76dd63f1de1f73751");
        StringAssert.Contains(audit, "5e84a8dfe4481d88b8fabec7602990ddb3e0a9e5");
        StringAssert.Contains(audit, "dd312445431351223d46afdf1006bd87de06ea0a");
        StringAssert.Contains(audit, "355e681a505ecb17c0a9dc174e33ef61868769e8");
        StringAssert.Contains(audit, "8cf9d20e67173d0afdc25829aebdd499bd87e2f5");
        StringAssert.Contains(audit, "e47aa7cdc0203cfee22763732fb51f287f4d1fb3");
        StringAssert.Contains(audit, "ac83f7bc2a60aa693075fa8349589fd55044f48d");
        StringAssert.Contains(audit, "0fc9f18000bc56ef5070d468c6dbfd9851bc7c6c");
        StringAssert.Contains(audit, "2048d004a267ae774862c406a278f697cf24fb44");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-193638-007-34996/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-193710-698-37208/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-193743-939/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-193759-788/report.md");
        StringAssert.Contains(audit, "| `52x500` / `52x500` | `1.20` |");
        StringAssert.Contains(audit, "| `52x500` / `52x500` | `1.21` |");
        StringAssert.Contains(audit, "`102.53` maximum local delta");
        StringAssert.Contains(audit, "`102.133` maximum local delta");
        StringAssert.Contains(audit, "controls\\dev\\AnnotatedScrollBar\\AnnotatedScrollBar.cpp");
        Assert.IsFalse(audit.Contains("`src\\controls\\dev\\AnnotatedScrollBar", StringComparison.Ordinal));

        StringAssert.Contains(control, "public partial class AnnotatedScrollBar : Control, IScrollController");
        StringAssert.Contains(control, "ScrollController => this;");
        StringAssert.Contains(control, "if (maxOffset < minOffset)");
        StringAssert.Contains(control, "if (viewportLength < 0.0)");
        StringAssert.Contains(control, "Scrolling?.Invoke(this, args);");
        StringAssert.Contains(control, "DetailLabelRequested?.Invoke(this, args);");
        StringAssert.Contains(control, "foreach (var label in Labels ?? Array.Empty<AnnotatedScrollBarLabel>())");
        Assert.IsFalse(control.Contains("OnCreateAutomationPeer", StringComparison.Ordinal));

        StringAssert.Contains(template, "x:Name=\"PART_VerticalThumb\"");
        StringAssert.Contains(template, "x:Name=\"PART_VerticalThumbGhost\"");
        StringAssert.Contains(template, "x:Name=\"PART_LabelsGrid\"");
        StringAssert.Contains(template, "x:Name=\"PART_DetailLabelToolTip\"");
        StringAssert.Contains(template, "FontFamily=\"{DynamicResource SymbolThemeFontFamily}\"");

        StringAssert.Contains(apiTests, "AnnotatedScrollBarHasNoStandaloneAutomationPeerLikeCurrentWinUI");
        StringAssert.Contains(apiTests, "VerifySetValuesValidation");
        StringAssert.Contains(apiTests, "VerifyCanceledScrollingSuppressesScrollRequest");
        StringAssert.Contains(apiTests, "VerifySmallChangeButtonDirectionMatchesWinUI");

        StringAssert.Contains(galleryFactory, "AnnotatedScrollBar linked to a ScrollView.");
        StringAssert.Contains(galleryFactory, "<ScrollView x:Name=\"\"scrollView\"\"");
        StringAssert.Contains(galleryFactory, "scrollView.ScrollPresenter.VerticalScrollController = annotatedScrollBar.ScrollController;");
        StringAssert.Contains(galleryFactory, "var scrollViewer = new ScrollViewer");
        StringAssert.Contains(galleryTests, "Assert.IsNull(UIElementAutomationPeer.CreatePeerForElement(annotatedScrollBar));");

        StringAssert.Contains(harness, "function New-AnnotatedScrollBarReferencePrimaryCrop");
        StringAssert.Contains(harness, "\"AnnotatedScrollBar\" { return 1.5 }");
        StringAssert.Contains(harness, "\"AnnotatedScrollBar\" { return 0 }");
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
