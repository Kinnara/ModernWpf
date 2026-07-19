using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.InfoBar;

[TestClass]
public class InfoBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3InfoBarParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "infobar-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "InfoBar", "InfoBar.cs");
        var peer = Read(repoRoot, "ModernWpf.Controls", "InfoBar", "InfoBarAutomationPeer.cs");
        var panel = Read(repoRoot, "ModernWpf.Controls", "InfoBar", "InfoBarPanel.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "InfoBar", "InfoBar.xaml");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "InfoBar", "InfoBarApiTests.cs");
        var galleryFactory = Read(repoRoot, "ModernWpf.Gallery", "Pages", "StatusInfoSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var harness = Read(repoRoot, "tools", "visual-checks", "Run-GalleryVisualChecks.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7");
        StringAssert.Contains(audit, "4229e91b7384b94539f0897822dac2a320ca07aa");
        StringAssert.Contains(audit, "b78bf1a698b1c38169fa9a60f389c1ef2cd9b3db");
        StringAssert.Contains(audit, "9cb2a37cb70060d9ab4aa3a3e499169f26eb85a8");
        StringAssert.Contains(audit, "1f036889684c7af85187811a1ef776d059eed2a8");
        StringAssert.Contains(audit, "1056af57d0340a7e9aa2d5f9e2f2f177b841897c");
        StringAssert.Contains(audit, "65cd8db3b914853c2cc02d5ec7201c072d2e8be1");
        StringAssert.Contains(audit, "be47e83aaa7456db470130bd5154276534cc40f7");
        StringAssert.Contains(audit, "ad60e9e14ad011fc33cc9983557f84b1a9372874");
        StringAssert.Contains(audit, "b3110c62d4d76f7909f102ade44150ac779fe449");
        StringAssert.Contains(audit, "b5e4033a811f2d6201a5541355f64ce1803a8658");
        StringAssert.Contains(audit, "ace93fb54ac65eb82352fa85fd717daad2c085cf");
        StringAssert.Contains(audit, "controls\\dev\\InfoBar\\InfoBar.cpp");
        Assert.IsFalse(audit.Contains("`src\\controls\\dev\\InfoBar", StringComparison.Ordinal));

        StringAssert.Contains(control, "return new InfoBarAutomationPeer(this);");
        StringAssert.Contains(control, "TemplateSettings.IconElement = IconSource?.CreateIconElement();");
        StringAssert.Contains(control, "_lastCloseReason = InfoBarCloseReason.CloseButton;");
        StringAssert.Contains(control, "args.Cancel");
        StringAssert.Contains(control, "IsOpen = true;");

        StringAssert.Contains(peer, "return AutomationControlType.StatusBar;");
        StringAssert.Contains(peer, "return nameof(InfoBar);");
        StringAssert.Contains(peer, "infoBar.IsOpen");

        StringAssert.Contains(panel, "heightOfTallestInHorizontal > minHeight");
        StringAssert.Contains(panel, "Math.Ceiling(desiredSize.Width * dpi.DpiScaleX)");
        StringAssert.Contains(panel, "GetWinUITextLayoutRoundingHeightAdjustment");

        StringAssert.Contains(template, "x:Name=\"InfoBarVisibility\"");
        StringAssert.Contains(template, "x:Name=\"ContentStates\"");
        StringAssert.Contains(template, "Target=\"ContentArea.(Grid.Row)\"");
        StringAssert.Contains(template, "Command=\"{TemplateBinding CloseButtonCommand}\"");

        StringAssert.Contains(apiTests, "InfoBarCloseEventsTest");
        StringAssert.Contains(apiTests, "Assert.IsNull(infoBar.TemplateSettings.IconElement);");
        StringAssert.Contains(apiTests, "InfoBarAutomationPeerTest");
        StringAssert.Contains(apiTests, "InfoBarPanelUsesWinUITextBlockLayoutRounding");

        StringAssert.Contains(galleryFactory, "A closable InfoBar with options to change its Severity.");
        StringAssert.Contains(galleryFactory, "A closable InfoBar with a long or short message and various buttons");
        StringAssert.Contains(galleryFactory, "A closable InfoBar with options to display the close button and icon");
        StringAssert.Contains(galleryFactory, "InfoBarActionButtonComboBox");
        StringAssert.Contains(galleryTests, "Assert.AreEqual(ModernWpf.Controls.InfoBarSeverity.Error, infoBar.Severity);");
        StringAssert.Contains(galleryTests, "Assert.IsFalse(iconAndCloseInfoBar.IsClosable);");

        StringAssert.Contains(harness, "\"InfoBar\" { return 2.0 }");
        StringAssert.Contains(harness, "\"InfoBar\" { return 0 }");
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
