using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.DropDownButton;

[TestClass]
public class DropDownButtonSourceAuditTests
{
    [TestMethod]
    public void CurrentProductSourcesRuntimeTemplateAndAccessibilityArePinned()
    {
        var root = FindRepoRoot();
        var audit = Read(root, "docs", "dropdownbutton-winui3-source-audit.md");
        var control = Read(root, "ModernWpf.Controls", "DropDownButton", "DropDownButton.cs");
        var template = Read(root, "ModernWpf.Controls", "DropDownButton", "DropDownButton.xaml");
        var peer = Read(root, "ModernWpf.Controls", "DropDownButton", "DropDownButtonAutomationPeer.cs");
        var interaction = Read(root, "test", "ModernWpf.WinUI.Tests", "DropDownButton", "DropDownButtonInteractionTests.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "a8baaecbec5bc5e6d73d398bdc27c1beee0f426c");
        StringAssert.Contains(audit, "aa07b40c6f40b28c347ab25fd73a8ba36524505f");
        StringAssert.Contains(audit, "a8f3afd7123954780a40d8e6c1229b00ee512291");
        StringAssert.Contains(audit, "7e521cd12666090e241b90f79cfef712e011111c");
        StringAssert.Contains(audit, "50691799f2a5b29eff8360dc47953c6bbdcf15ab");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");

        StringAssert.Contains(control, "OnFlyoutPropertyChanged");
        StringAssert.Contains(control, "RegisterFlyoutEvents();");
        StringAssert.Contains(control, "m_registeredFlyout.Opened -= OnFlyoutOpened");
        StringAssert.Contains(control, "m_registeredFlyout.Closed -= OnFlyoutClosed");
        StringAssert.Contains(control, "flyout.Opened += OnFlyoutOpened");
        StringAssert.Contains(control, "flyout.Closed += OnFlyoutClosed");
        StringAssert.Contains(control, "Flyout?.ShowAt(this)");
        StringAssert.Contains(control, "Flyout?.Hide()");

        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"PointerOver\">");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"Pressed\">");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"Disabled\">");
        StringAssert.Contains(template, "Target=\"ChevronIcon.(ui:AnimatedIcon.State)\"");
        StringAssert.Contains(template, "x:Name=\"ChevronIcon\"");
        StringAssert.Contains(template, "Width=\"12\"");
        StringAssert.Contains(template, "Height=\"12\"");

        StringAssert.Contains(peer, "IExpandCollapseProvider");
        StringAssert.Contains(peer, "PatternInterface.ExpandCollapse");
        StringAssert.Contains(peer, "nameof(DropDownButton)");
        StringAssert.Contains(peer, "dropDownButton.OpenFlyout()");
        StringAssert.Contains(peer, "dropDownButton.CloseFlyout()");

        StringAssert.Contains(interaction, "ReplacementFlyout");
        StringAssert.Contains(interaction, "Assert.AreEqual(1, firstFlyout.OpenedCount)");
        StringAssert.Contains(interaction, "Assert.AreEqual(1, secondFlyout.OpenedCount)");
        StringAssert.Contains(interaction, "ExpandCollapseState.Expanded");
        StringAssert.Contains(interaction, "ExpandCollapseState.Collapsed");
    }

    private static string Read(string root, params string[] parts)
    {
        var path = root;
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
