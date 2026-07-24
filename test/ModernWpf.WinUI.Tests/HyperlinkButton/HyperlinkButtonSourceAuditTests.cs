using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.HyperlinkButtonTests;

[TestClass]
public class HyperlinkButtonSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3HyperlinkButtonParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "hyperlinkbutton-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "HyperlinkButton", "HyperlinkButton.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "HyperlinkButton", "HyperlinkButtonAutomationPeer.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "HyperlinkButton", "HyperlinkButton.xaml");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "c0d59563ffb684a8f492715bb66c7bfa89a68313");
        StringAssert.Contains(audit, "4760194f6724e7335963ad60fa69e356ccc9c9a6");
        StringAssert.Contains(audit, "cc561812c862c252ab41c5ce5a4a47d11024f563");
        StringAssert.Contains(audit, "a120c5fb943ae7623a56cc738cb00f0bb3b8cf2b");
        StringAssert.Contains(audit, "08c4ff39ff0d1e3f185dc87a0a7d5388b47eaab4");
        StringAssert.Contains(audit, "ad1199a7ff9c253e38c4fb922accbe0afffbf432");
        StringAssert.Contains(audit, "93b5efd391803a229e63e55c315e5675fef4362e");
        StringAssert.Contains(audit, "3861ddde4574c2519b0e4f64d296db5d2dd2b5d5");
        StringAssert.Contains(audit, "b26959664f83a1a088954b21ad8f834411077f33");
        StringAssert.Contains(audit, "819170cc934226436706355221698507c04d8dba");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "89d2864e4545f894f6c80b0e2d41017112f348af");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-080523-554-71612/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-080545-679-80452/report.md");

        StringAssert.Contains(control, "AutomationEvents.InvokePatternOnInvoked");
        StringAssert.Contains(control, "base.OnClick()");
        StringAssert.Contains(control, "UseShellExecute = true");
        StringAssert.Contains(peer, "AutomationControlType.Hyperlink");
        StringAssert.Contains(peer, "return \"Hyperlink\"");
        StringAssert.Contains(peer, "PatternInterface.Invoke");
        StringAssert.Contains(peer, "throw new ElementNotEnabledException()");
        StringAssert.Contains(template, "x:Key=\"DefaultHyperlinkButtonStyle\"");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"PointerOver\"");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"Pressed\"");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"Disabled\"");
        StringAssert.Contains(template, "Target=\"ContentPresenter.Foreground\"");
        StringAssert.Contains(template, "Target=\"ContentPresenter.Background\"");
        StringAssert.Contains(template, "Target=\"ContentPresenter.BorderBrush\"");
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
