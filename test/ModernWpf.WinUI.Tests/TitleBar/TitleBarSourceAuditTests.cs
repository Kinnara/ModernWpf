using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.TitleBar;

[TestClass]
public class TitleBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3TitleBarParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "titlebar-winui3-gallery-parity.md");
        var control = Read(repoRoot, "ModernWpf", "TitleBar", "TitleBarControl.cs");
        var peer = Read(repoRoot, "ModernWpf", "TitleBar", "TitleBarControlAutomationPeer.cs");
        var windowTests = Read(
            repoRoot,
            "test",
            "ModernWpf.WinUI.Tests",
            "CommonStyles",
            "WindowVisualStateTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "WindowingSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "9a14fa563584b19c06e3baccf10664a12f84fad5");
        StringAssert.Contains(audit, "5134559ff2de382759847e60c6797e16837db1a9");
        StringAssert.Contains(audit, "e6885f5fb8c7deb5f6e552c7e88b3614742c2969");
        StringAssert.Contains(audit, "acd14c7c6f242d99a0467d69f701b8599d8dd9c5");
        StringAssert.Contains(audit, "f3a0717c2aeb1cc056f57138876206cf920c280d");
        StringAssert.Contains(audit, "acd296138d7ba7a4d0a03cf3f9d51be2680e81e3");
        StringAssert.Contains(audit, "b22068a7909c99426a1f1811e227db4ad11baa1c");
        StringAssert.Contains(audit, "a093c18518d257b87bea607cdb5b6ef6310ee73d");
        StringAssert.Contains(audit, "bc2dca716306280040390a3d446e95aae93ca904");
        StringAssert.Contains(audit, "25714311aaf20f8450eb6aa0f116d8ec6ac556e9");
        StringAssert.Contains(audit, "af520bb8b5124280f607608bf242d8b39cd401dc");
        StringAssert.Contains(audit, "809fd3df59b5383279de02be9eefe76fd61fd5cc");
        StringAssert.Contains(audit, "a63138f1d89beee02b4ffb8b7626e398b557e8c0");
        StringAssert.Contains(audit, "6e2fb83489d8c0df9b08758bceec24afe401c595");
        StringAssert.Contains(audit, "ec23c47c91b0c164d875c449df8246f085350aec");
        StringAssert.Contains(audit, "e138e59bc558add94bfb98fcbc1dd094e8d67b87");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-174511-923-56972/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-174542-034-92644/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-184435-809/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260718-184510-347/report.md");
        StringAssert.Contains(audit, "| `470x48` / `470x48` | `0.74` |");
        StringAssert.Contains(audit, "| `470x48` / `470x48` | `0.82` |");
        StringAssert.Contains(audit, "`6.819` local delta");
        StringAssert.Contains(audit, "`7.897` local delta");
        StringAssert.Contains(audit, "controls\\dev\\TitleBar\\TitleBar.cpp");
        StringAssert.Contains(audit, "TitleBar.HeightKey");
        StringAssert.Contains(audit, "WindowChrome.CaptionHeight");
        StringAssert.Contains(audit, "WM_NCHITTEST");
        Assert.IsFalse(audit.Contains("`src\\controls\\dev\\TitleBar", StringComparison.Ordinal));

        StringAssert.Contains(control, "return new TitleBarControlAutomationPeer(this);");
        StringAssert.Contains(control, "UpdateWindowChromeCaptionHeight");
        StringAssert.Contains(control, "WindowChrome.WindowChromeProperty");
        StringAssert.Contains(control, "CloneCurrentValue");
        StringAssert.Contains(peer, "return AutomationControlType.TitleBar;");
        StringAssert.Contains(peer, "return \"TitleBar\";");
        StringAssert.Contains(peer, "name = ((TitleBarControl)Owner).Title;");

        StringAssert.Contains(windowTests, "TitleBarHeightResourceControlsRenderedAndDraggableHeight");
        StringAssert.Contains(windowTests, "WmNcHitTest");
        StringAssert.Contains(windowTests, "Assert.AreSame(replacementChrome, synchronizedReplacement);");

        StringAssert.Contains(galleryFactory, "TitleBarContentHorizontalAlignment");
        StringAssert.Contains(galleryFactory, "MaxWidth=\"\"580\"\"");
        StringAssert.Contains(galleryFactory, "PlaceholderText=\"\"Search...\"\"");
        StringAssert.Contains(galleryFactory, "TitleBarDragRegionsXaml");
        StringAssert.Contains(galleryFactory, "CreateTitleBarDragRegionsWindowBody");
        StringAssert.Contains(galleryFactory, "GalleryAutomation.SampleElementId(\"TitleBar\", \"DragRegionsShowWindowButton\")");
        StringAssert.Contains(galleryFactory, "Mux.TitleBar.SetExtendViewIntoTitleBar(window, true);");
        StringAssert.Contains(galleryFactory, "WPF updates its live drag/input tree automatically.");

        StringAssert.Contains(harness, "\"TitleBar\" { return 1.0 }");
        StringAssert.Contains(harness, "\"TitleBar\" { return 0 }");
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
