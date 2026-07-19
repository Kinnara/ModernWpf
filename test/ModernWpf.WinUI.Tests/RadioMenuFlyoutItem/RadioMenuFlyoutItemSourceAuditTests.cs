using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.RadioMenuFlyoutItem;

[TestClass]
public class RadioMenuFlyoutItemSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3RadioMenuFlyoutItemParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "radiomenuflyoutitem-winui3-source-audit.md");
        var control = Read(repoRoot, "ModernWpf.Controls", "RadioMenuItem", "RadioMenuItem.cs");
        var properties = Read(repoRoot, "ModernWpf.Controls", "RadioMenuItem", "RadioMenuItem.properties.g.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "RadioMenuItem", "RadioMenuItem.xaml");
        var apiTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "RadioMenuFlyoutItem", "RadioMenuFlyoutItemApiTests.cs");
        var gallery = Read(repoRoot, "ModernWpf.Gallery", "Pages", "MenusToolbarsSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "c70471c511a0168b61dcca13af9556465f26b673");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        StringAssert.Contains(audit, "4d827b32f1b6eaca140f4d0298caa2a3ef47c542");
        StringAssert.Contains(audit, "078fadf4058d7c6b269b335350a075d6c079ab03");
        StringAssert.Contains(audit, "4f03dc205b6cd0271015076444cc40bbe183f345");
        StringAssert.Contains(audit, "18a783d06015f40c1e21bf0eb4870162b99fd066");
        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "834f7da9d1a314fa30c7a92e8814f2bc4aa889c0");
        StringAssert.Contains(audit, "099d74fd883a104cbd45e364f0d5fd1a361b8935");
        StringAssert.Contains(audit, "not an isolated radio-item pixel proof");

        StringAssert.Contains(properties, "nameof(GroupName)");
        StringAssert.Contains(properties, "new FrameworkPropertyMetadata(string.Empty, OnGroupNameChanged)");
        StringAssert.Contains(control, "new WeakReference<RadioMenuItem>(this)");
        StringAssert.Contains(control, "previousCheckedItem.m_isSafeUncheck = true");
        StringAssert.Contains(control, "SetCurrentValue(IsCheckedProperty, true)");
        StringAssert.Contains(control, "RemoveCheckedItemFromGroup(m_groupName)");
        StringAssert.Contains(control, "menuItem.SetCurrentValue(IsCheckedProperty, isAnyItemChecked)");

        StringAssert.Contains(template, "x:Name=\"CheckGlyph\"");
        StringAssert.Contains(template, "Data=\"{StaticResource RadioBullet}\"");
        StringAssert.Contains(template, "FontSize=\"12\"");
        StringAssert.Contains(template, "Margin=\"0,0,16,0\"");
        StringAssert.Contains(template, "Target=\"CheckGlyph.Opacity\" Value=\"1\"");
        StringAssert.Contains(template, "x:Name=\"KeyboardAcceleratorTextBlock\"");

        StringAssert.Contains(apiTests, "AutomationControlType.MenuItem");
        StringAssert.Contains(apiTests, "PatternInterface.Toggle");
        StringAssert.Contains(apiTests, "ToggleState.On");
        StringAssert.Contains(apiTests, "does not allow the selected item to toggle itself off");

        StringAssert.Contains(gallery, "CreateMenuFlyoutRadioExampleContent()");
        StringAssert.Contains(gallery, "CreateRadioMenuItem(\"Portrait\", \"OrientationGroup\", isChecked: true");
        StringAssert.Contains(gallery, "CreateRadioMenuItem(\"Medium icons\", \"SizeGroup\", isChecked: true");
        StringAssert.Contains(galleryTests, "var landscapePeer = new MenuItemAutomationPeer(landscape)");
        StringAssert.Contains(galleryTests, "var portraitToggle = portraitPeer.GetPattern(PatternInterface.Toggle) as IToggleProvider");
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
