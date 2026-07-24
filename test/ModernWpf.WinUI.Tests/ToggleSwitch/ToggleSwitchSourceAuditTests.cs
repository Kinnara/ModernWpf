using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.ToggleSwitchControl;

[TestClass]
public class ToggleSwitchSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3ToggleSwitchParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "toggleswitch-winui3-source-audit.md");
        var toggleSwitch = Read(repoRoot, "ModernWpf.Controls", "ToggleSwitch", "ToggleSwitch.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "ToggleSwitch", "ToggleSwitch.xaml");
        var peer = Read(repoRoot, "ModernWpf.Controls", "ToggleSwitch", "ToggleSwitchAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "beabd047460bf5d43a41fcf8bddf7730188bd5a7");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2");
        StringAssert.Contains(audit, "182d9d9af8547bab338f0e3375ccd50b7871b7ec");
        StringAssert.Contains(audit, "653b96141170459f5ec43ca43999c89b0be23972");
        StringAssert.Contains(audit, "8363db47b29b8747da03756fe98d39ab5b1bc170");
        StringAssert.Contains(audit, "50f290fbe053170d943f390d78bb4f8d6ba48140");
        StringAssert.Contains(audit, "09daa82259a14070ea12d243ae0080e9e7c3d9da");
        StringAssert.Contains(audit, "5c5346bb857e34ecb584f9fd225d9639a8d61a9e");
        StringAssert.Contains(audit, "f5ce988ad3009837ba15fa23bc30657e835d57f9");
        StringAssert.Contains(audit, "929eeb0566891197877a9684d5a2a97ced390868");
        StringAssert.Contains(audit, "1de3beec353704e20f6df7a85ad1c5a4a961b537");
        StringAssert.Contains(audit, "d9505228d70ab1f946e1696908d73742a8ca6b6c");
        StringAssert.Contains(audit, "d7af70b72b67b49a9dbd9b151cd0610a8c05d73b");
        StringAssert.Contains(audit, "1f84b100bce89d1caa528d4fed4341bf5a393d8d");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-115333-533-40948/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-115425-504-74736/report.md");
        StringAssert.Contains(audit, "| `72x40` / `72x40` | `0.92` |");
        StringAssert.Contains(audit, "| `72x40` / `72x40` | `1.06` |");
        StringAssert.Contains(audit, "| `92x60` / `92x60` | `0.53` |");
        StringAssert.Contains(audit, "| `92x60` / `92x60` | `0.48` |");

        StringAssert.Contains(toggleSwitch, "SwitchThumb.DragStarted += DragStartedHandler;");
        StringAssert.Contains(toggleSwitch, "SwitchThumb.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(TapHandler), true);");
        StringAssert.Contains(toggleSwitch, "ToggleSwitchKeyProcess.KeyDown(GetOriginalKey(e), this, ref isHandled);");
        StringAssert.Contains(toggleSwitch, "AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged)");
        StringAssert.Contains(toggleSwitch, "MoveCompleted(_wasDragged);");
        StringAssert.Contains(toggleSwitch, "private void GetTranslations()");

        StringAssert.Contains(template, "<Setter Property=\"VerticalContentAlignment\" Value=\"Center\" />");
        StringAssert.Contains(template, "<ui:VisualStateEx x:Name=\"Normal\">");
        StringAssert.Contains(template, "Target=\"SwitchKnobBounds.Fill\"");
        StringAssert.Contains(template, "x:Name=\"SwitchAreaGrid\"");
        StringAssert.Contains(template, "x:Name=\"OuterBorder\"");
        StringAssert.Contains(template, "x:Name=\"SwitchKnobBounds\"");
        StringAssert.Contains(template, "x:Name=\"SwitchThumb\"");
        StringAssert.Contains(template, "Width=\"40\"");
        StringAssert.Contains(template, "Height=\"20\"");

        StringAssert.Contains(peer, "patternInterface == PatternInterface.Toggle");
        StringAssert.Contains(peer, "return AutomationControlType.Button;");
        StringAssert.Contains(peer, "return ResourceAccessor.GetLocalizedStringResource(SR_ToggleSwitchLocalizedControlType);");
        StringAssert.Contains(peer, "protected override System.Collections.Generic.List<AutomationPeer> GetChildrenCore()");
        StringAssert.Contains(peer, "GetImpl().AutomationToggleSwitchOnToggle();");

        StringAssert.Contains(galleryFactory, "A simple ToggleSwitch.");
        StringAssert.Contains(galleryFactory, "AutomationProperties.SetName(toggleSwitch, \"simple ToggleSwitch\");");
        StringAssert.Contains(galleryFactory, "Name = \"ToggleSwitch2\"");
        StringAssert.Contains(galleryFactory, "Header = \"Toggle work\"");
        StringAssert.Contains(galleryFactory, "progressRing.IsActive = toggleSwitch.IsOn;");
        StringAssert.Contains(harness, "\"ToggleSwitch\" { return 1.5 }");
        StringAssert.Contains(harness, "\"ToggleSwitch\" { return 0 }");
        StringAssert.Contains(harness, "\"ToggleSwitch\" { return $true }");
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
