using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.SplitButton;

[TestClass]
public class SplitButtonSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3SplitButtonFamilyParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "splitbutton-winui3-source-audit.md");
        var splitButton = Read(repoRoot, "ModernWpf.Controls", "SplitButton", "SplitButton.cs");
        var toggleSplitButton = Read(repoRoot, "ModernWpf.Controls", "SplitButton", "ToggleSplitButton.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "SplitButton", "SplitButton.xaml");
        var splitPeer = Read(repoRoot, "ModernWpf.Controls", "SplitButton", "SplitButtonAutomationPeer.cs");
        var togglePeer = Read(repoRoot, "ModernWpf.Controls", "SplitButton", "ToggleSplitButtonAutomationPeer.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "BasicInputSampleFactory.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "beabd047460bf5d43a41fcf8bddf7730188bd5a7");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2");
        StringAssert.Contains(audit, "c7731506ca9aa201e5c933034c72dad883743650");
        StringAssert.Contains(audit, "0be293a87cd5d944267906e159fbcc4d1d85911b");
        StringAssert.Contains(audit, "e9601d1f29cd59aa5fe7b7f0ebb49ef9c836ac19");
        StringAssert.Contains(audit, "001d9a515e58cf123bad59ad91a94c322407a222");
        StringAssert.Contains(audit, "a6612c92349e2eae8b1bf938bb8f3d7f9dcf4d22");
        StringAssert.Contains(audit, "a4c5ace66cb8f7245e35c4462ffbd473fdbc6d43");
        StringAssert.Contains(audit, "994e5ec0a4b495d6ddf865a6a5a4f661be3facd1");
        StringAssert.Contains(audit, "310135a3734eda7c1c102f0b71f368befca4261c");
        StringAssert.Contains(audit, "fb3a0abdd1e507e00899ac258dfe613fc8614bf2");
        StringAssert.Contains(audit, "56a69700ec9ca2c0276a1fb1e529d93077b183d0");
        StringAssert.Contains(audit, "8ae8e1f4998170efc977ee0b4af6a9064d9fc135");
        StringAssert.Contains(audit, "99f1cd62ff09071bf55444d9a68c0bd287e51ae9");
        StringAssert.Contains(audit, "5d50f69123b88ea2f0aacd36923cf74fe76e6968");
        StringAssert.Contains(audit, "2acbcfa36ca1ca1b4ae0f871b4a37f2738d79be6");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-113331-468-93712/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260718-113651-452-2372/report.md");
        StringAssert.Contains(audit, "| `71x32` / `71x32` | `0.46` |");
        StringAssert.Contains(audit, "| `71x32` / `71x32` | `0.37` |");
        StringAssert.Contains(audit, "| `78x33` / `78x33` | `1.62` |");
        StringAssert.Contains(audit, "| `78x33` / `78x33` | `0.98` |");

        StringAssert.Contains(splitButton, "RegisterFlyoutEvents();");
        StringAssert.Contains(splitButton, "Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft");
        StringAssert.Contains(splitButton, "OnClickPrimary(null, null);");
        StringAssert.Contains(splitButton, "ExecuteCommand();");
        StringAssert.Contains(splitButton, "VisualStateManager.GoToState(this, \"CheckedTouchPressed\", useTransitions);");
        StringAssert.Contains(toggleSplitButton, "Toggle();");
        StringAssert.Contains(toggleSplitButton, "base.OnClickPrimary(sender, e);");
        StringAssert.Contains(toggleSplitButton, "peer.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty");

        StringAssert.Contains(template, "x:Name=\"PrimaryBackgroundGrid\"");
        StringAssert.Contains(template, "x:Name=\"PrimaryButtonBorder\"");
        StringAssert.Contains(template, "x:Name=\"SecondaryButtonBorder\"");
        StringAssert.Contains(template, "x:Name=\"CheckedFlyoutOpen\"");
        StringAssert.Contains(template, "x:Name=\"CheckedTouchPressed\"");
        StringAssert.Contains(template, "x:Name=\"SecondaryButtonRight\"");
        StringAssert.Contains(template, "FontIconFallback");

        StringAssert.Contains(splitPeer, "PatternInterface.ExpandCollapse ||");
        StringAssert.Contains(splitPeer, "patternInterface == PatternInterface.Invoke");
        StringAssert.Contains(splitPeer, "return AutomationControlType.SplitButton;");
        StringAssert.Contains(togglePeer, "PatternInterface.ExpandCollapse ||");
        StringAssert.Contains(togglePeer, "patternInterface == PatternInterface.Toggle");
        StringAssert.Contains(togglePeer, "return AutomationControlType.SplitButton;");

        StringAssert.Contains(galleryFactory, "A SplitButton controlling text color in a RichEditBox");
        StringAssert.Contains(galleryFactory, "Name = \"myColorButton\"");
        StringAssert.Contains(galleryFactory, "Name = \"myColorButtonReveal\"");
        StringAssert.Contains(galleryFactory, "Name = \"myListButton\"");
        StringAssert.Contains(galleryFactory, "command.Execute(null, richTextBox);");
        StringAssert.Contains(galleryFactory, "if (isChecked &&");
        StringAssert.Contains(galleryFactory, "list.MarkerStyle = markerStyle;");
        StringAssert.Contains(harness, "\"SplitButton\" { return 1.0 }");
        StringAssert.Contains(harness, "\"ToggleSplitButton\" { return 2.0 }");
        StringAssert.Contains(harness, "\"SplitButton\" { return 0 }");
        StringAssert.Contains(harness, "\"ToggleSplitButton\" { return 0 }");
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
