using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.ContentDialogs;

[TestClass]
public class ContentDialogSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ContentDialogParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "contentdialog-winui3-source-audit.md");
        var template = Read(repoRoot, "ModernWpf.Controls", "ContentDialog", "ContentDialog.xaml");
        var control = Read(repoRoot, "ModernWpf.Controls", "ContentDialog", "ContentDialog.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "ContentDialog", "ContentDialogAutomationPeer.cs");
        var gallery = Read(repoRoot, "ModernWpf.Gallery", "Pages", "DialogsFlyoutsSampleFactory.cs");
        var recorder = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "controls/dev/CommonStyles/ContentDialog_themeresources.xaml");
        StringAssert.Contains(audit, "dxaml/xcp/dxaml/lib/ContentDialog_Partial.cpp");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "9cd8150fff8c216319e2d5515b538cfab5b397d5");
        StringAssert.Contains(audit, "a42b69a40ffca4c801cf83216c35a286436328d1");
        StringAssert.Contains(audit, "e41cb2e8a95bbe230596c174fbf98dabbbf72800");
        StringAssert.Contains(audit, "05d976c076c69edfdf38f51fbb362f7fb751ff51");
        Assert.IsFalse(audit.Contains("`src\\controls\\dev\\CommonStyles", StringComparison.Ordinal));

        StringAssert.Contains(template, "<sys:Double x:Key=\"ContentDialogMinWidth\">320</sys:Double>");
        StringAssert.Contains(template, "<Thickness x:Key=\"ContentDialogPadding\">24</Thickness>");
        StringAssert.Contains(template, "<Setter Property=\"FontFamily\" Value=\"{DynamicResource ContentControlThemeFontFamily}\" />");
        StringAssert.Contains(template, "<Setter Property=\"FontSize\" Value=\"{DynamicResource ControlContentThemeFontSize}\" />");
        StringAssert.Contains(template, "FontSize=\"20\"");
        StringAssert.Contains(template, "FontFamily=\"{DynamicResource ContentControlThemeFontFamily}\"");
        StringAssert.Contains(template, "MinHeight=\"27\"");
        StringAssert.Contains(template, "FontSize=\"{DynamicResource ControlContentThemeFontSize}\"");

        StringAssert.Contains(control, "return new ContentDialogAutomationPeer(this);");
        StringAssert.Contains(peer, "AutomationControlType.Window");
        StringAssert.Contains(peer, "PatternInterface.Window ? this");
        StringAssert.Contains(peer, "AutomationProperties.GetName(dialog)");
        StringAssert.Contains(peer, "GetPlainText(dialog.Title)");
        StringAssert.Contains(peer, "public bool IsModal => true;");

        StringAssert.Contains(gallery, "\"A basic content dialog with content.\"");
        StringAssert.Contains(gallery, "defaultButton: Mux.ContentDialogButton.Primary");
        StringAssert.Contains(gallery, "\"A content dialog without a default button.\"");
        StringAssert.Contains(gallery, "defaultButton: Mux.ContentDialogButton.None");
        StringAssert.Contains(gallery, "CloseButtonText = \"Cancel\"");

        StringAssert.Contains(recorder, "\"ContentDialog\" { return \"GallerySample_ContentDialog_ShowButton\" }");
        StringAssert.Contains(recorder, "\"ContentDialog\" { return @(\"Save your work?\", \"Upload your content to the cloud.\", \"Save\", \"Don't Save\", \"Cancel\") }");
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
