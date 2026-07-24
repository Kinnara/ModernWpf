using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.CommandBars;

[TestClass]
public class AppBarSourceAuditTests
{
    [TestMethod]
    public void CurrentWinUI3AppBarFamilyParityIsPinnedByAudit()
    {
        var repoRoot = FindRepoRoot();
        var audit = Read(repoRoot, "docs", "appbarbutton-winui3-source-audit.md");
        var sharedResources = Read(repoRoot, "ModernWpf", "Styles", "CommandBar.xaml");
        var buttonTemplate = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarButton.xaml");
        var toggleTemplate = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarToggleButton.xaml");
        var separatorTemplate = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarSeparator.xaml");
        var elementContainer = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarElementContainer.xaml");
        var commandBar = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "CommandBar.cs");
        var elementInterface = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "ICommandBarElement.cs");
        var elementProperties = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarElementProperties.properties.g.cs");
        var buttonProperties = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarButton.properties.g.cs");
        var toggleProperties = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarToggleButton.properties.g.cs");
        var separatorProperties = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarSeparator.properties.g.cs");
        var elementContainerProperties = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarElementContainer.properties.g.cs");
        var publicDocumentation = Read(repoRoot, "ModernWpf.Controls", "ModernWpf.Controls.xml");
        var buttonPeer = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarButtonAutomationPeer.cs");
        var togglePeer = Read(repoRoot, "ModernWpf.Controls", "CommandBar", "AppBarToggleButtonAutomationPeer.cs");
        var light = Read(repoRoot, "ModernWpf", "ThemeResources", "Light.xaml");
        var dark = Read(repoRoot, "ModernWpf", "ThemeResources", "Dark.xaml");
        var highContrast = Read(repoRoot, "ModernWpf", "ThemeResources", "HighContrast.xaml");
        var gallery = Read(repoRoot, "ModernWpf.Gallery", "Pages", "MenusToolbarsSampleFactory.cs");
        var galleryTests = Read(repoRoot, "test", "ModernWpf.Gallery.Tests", "GalleryAutomationHookTests.cs");
        var commandBarTests = Read(repoRoot, "test", "ModernWpf.WinUI.Tests", "CommandBar", "CommandBarApiTests.cs");
        var recorder = Read(repoRoot, "tools", "visual-checks", "Record-GalleryControlInteractions.ps1");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "controls/dev/CommonStyles/AppBarButton_themeresources.xaml");
        StringAssert.Contains(audit, "controls/dev/CommonStyles/AppBarToggleButton_themeresources.xaml");
        StringAssert.Contains(audit, "controls/dev/CommonStyles/AppBarSeparator_themeresources.xaml");
        StringAssert.Contains(audit, "c132a7bfd76806c1eff80e1072176d8d16fdf7d6");
        StringAssert.Contains(audit, "a7420f339171d12ac575be68c68036ac2561e349");
        StringAssert.Contains(audit, "dxaml/xcp/dxaml/lib/AppBarElementContainer_Partial.cpp");
        StringAssert.Contains(audit, "8463f45162149de0ec3ad7df752596893fe3e13e");
        Assert.IsFalse(audit.Contains("`src\\controls\\dev\\CommonStyles", StringComparison.Ordinal));

        StringAssert.Contains(sharedResources, "<sys:Double x:Key=\"AppBarButtonContentHeight\">16</sys:Double>");
        StringAssert.Contains(sharedResources, "<sys:Double x:Key=\"AppBarThemeMinHeight\">64</sys:Double>");
        StringAssert.Contains(sharedResources, "<sys:Double x:Key=\"AppBarThemeCompactHeight\">48</sys:Double>");
        StringAssert.Contains(sharedResources, "<Thickness x:Key=\"AppBarButtonContentViewboxCollapsedMargin\">0,16,0,2</Thickness>");
        StringAssert.Contains(sharedResources, "<Thickness x:Key=\"AppBarButtonTextLabelMargin\">2,0,2,8</Thickness>");
        StringAssert.Contains(sharedResources, "<Thickness x:Key=\"AppBarToggleButtonTextLabelMargin\">2,0,2,8</Thickness>");
        StringAssert.Contains(sharedResources, "<Thickness x:Key=\"AppBarButtonInnerBorderMargin\">2,6,2,6</Thickness>");

        StringAssert.Contains(buttonTemplate, "<Setter Property=\"Width\" Value=\"68\" />");
        StringAssert.Contains(buttonTemplate, "MinHeight=\"{DynamicResource AppBarThemeMinHeight}\"");
        StringAssert.Contains(buttonTemplate, "Margin=\"{DynamicResource AppBarButtonContentViewboxCollapsedMargin}\"");
        StringAssert.Contains(buttonTemplate, "Margin=\"{DynamicResource AppBarButtonTextLabelMargin}\"");
        StringAssert.Contains(toggleTemplate, "<Setter Property=\"Width\" Value=\"68\" />");
        StringAssert.Contains(toggleTemplate, "x:Name=\"Checked\"");
        StringAssert.Contains(toggleTemplate, "x:Name=\"OverflowCheckedPointerOver\"");
        StringAssert.Contains(toggleTemplate, "Margin=\"{DynamicResource AppBarToggleButtonTextLabelMargin}\"");

        StringAssert.Contains(separatorTemplate, "<Thickness x:Key=\"AppBarSeparatorMargin\">2,8,2,8</Thickness>");
        StringAssert.Contains(separatorTemplate, "<Thickness x:Key=\"AppBarOverflowSeparatorMargin\">0,4,0,4</Thickness>");
        StringAssert.Contains(separatorTemplate, "<sys:Double x:Key=\"AppBarSeparatorWidth\">1</sys:Double>");
        StringAssert.Contains(separatorTemplate, "<sys:Double x:Key=\"AppBarOverflowSeparatorHeight\">1</sys:Double>");
        StringAssert.Contains(separatorTemplate, "<sys:Double x:Key=\"AppBarSeparatorCornerRadius\">0.5</sys:Double>");
        StringAssert.Contains(elementContainer, "<ui:ContentPresenterEx");
        StringAssert.Contains(elementContainer, "ContentTransitions=\"{TemplateBinding ContentTransitions}\"");
        StringAssert.Contains(elementContainerProperties, "public bool IsInOverflow");
        StringAssert.Contains(elementContainerProperties, "public bool IsCompact");
        StringAssert.Contains(elementContainerProperties, "public int DynamicOverflowOrder");

        StringAssert.Contains(elementInterface, "bool IsInOverflow { get; }");
        StringAssert.Contains(elementInterface, "int DynamicOverflowOrder { get; set; }");
        StringAssert.Contains(elementProperties, "new PropertyMetadata(0, OnDynamicOverflowOrderChanged)");
        StringAssert.Contains(buttonProperties, "AppBarElementProperties.DynamicOverflowOrderProperty.AddOwner(typeof(AppBarButton))");
        StringAssert.Contains(toggleProperties, "AppBarElementProperties.DynamicOverflowOrderProperty.AddOwner(typeof(AppBarToggleButton))");
        StringAssert.Contains(separatorProperties, "AppBarElementProperties.DynamicOverflowOrderProperty.AddOwner(typeof(AppBarSeparator))");
        StringAssert.Contains(publicDocumentation, "P:ModernWpf.Controls.AppBarButton.DynamicOverflowOrder");
        StringAssert.Contains(publicDocumentation, "P:ModernWpf.Controls.AppBarToggleButton.DynamicOverflowOrder");
        StringAssert.Contains(publicDocumentation, "P:ModernWpf.Controls.AppBarSeparator.DynamicOverflowOrder");
        StringAssert.Contains(publicDocumentation, "P:ModernWpf.Controls.AppBarElementContainer.DynamicOverflowOrder");
        StringAssert.Contains(commandBar, ".Where(order => order > 0)");
        StringAssert.Contains(commandBar, ".OrderBy(order => order)");
        StringAssert.Contains(commandBar, "primaryCommands[i].DynamicOverflowOrder == 0");
        StringAssert.Contains(commandBar, "MoveAdjacentSeparators(primaryCommands, i, movedIndices, ref primaryWidth)");
        StringAssert.Contains(commandBarTests, "AppBarElementsExposeCurrentWinUIDynamicOverflowOrderContract");
        StringAssert.Contains(commandBarTests, "CommandBarDynamicOverflowUsesSourceOrderGroupsAndReactsToOrderChanges");
        StringAssert.Contains(commandBarTests, "CommandBarDynamicOverflowMovesWholeOrderGroupsAndAdjacentSeparators");

        StringAssert.Contains(buttonPeer, "return nameof(AppBarButton);");
        StringAssert.Contains(buttonPeer, "AutomationControlType.Button");
        StringAssert.Contains(buttonPeer, "PatternInterface.ExpandCollapse");
        StringAssert.Contains(buttonPeer, "return null;");
        StringAssert.Contains(togglePeer, "return nameof(AppBarToggleButton);");
        StringAssert.Contains(togglePeer, "AutomationControlType.Button");
        StringAssert.Contains(togglePeer, "PatternInterface.Toggle");
        StringAssert.Contains(togglePeer, "return null;");

        StringAssert.Contains(light, "x:Key=\"AppBarSeparatorForeground\" ResourceKey=\"DividerStrokeColorDefaultBrush\"");
        StringAssert.Contains(dark, "x:Key=\"AppBarSeparatorForeground\" ResourceKey=\"DividerStrokeColorDefaultBrush\"");
        StringAssert.Contains(highContrast, "x:Key=\"AppBarSeparatorForeground\" ResourceKey=\"SystemControlForegroundBaseMediumLowBrush\"");

        StringAssert.Contains(audit, "29f62479d5c046a0b854a5868e5a7cd484572d87");
        StringAssert.Contains(audit, "40a22976f78e63d5480afa8b49d5f3f7d5860dc6");
        StringAssert.Contains(audit, "7d8953077dcd776cd7fb1c17a473a2ab4fdacf80");
        StringAssert.Contains(gallery, "private const string AppBarButtonFlyoutXaml");
        StringAssert.Contains(gallery, "        <Flyout>");
        StringAssert.Contains(gallery, "        </Flyout>");
        StringAssert.Contains(gallery, "    </AppBarButton.Flyout>");
        Assert.IsFalse(gallery.Contains("<Flyout/>", StringComparison.Ordinal));
        StringAssert.Contains(galleryTests, "Assert.IsFalse(appBarButtonPage.Examples[5].XamlCode.Contains(\"<Flyout/>\"");
        StringAssert.Contains(recorder, "\"AppBarButton\" { return \"You clicked: Button1\" }");
        StringAssert.Contains(recorder, "\"AppBarToggleButton\" { return $true }");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260719-012054-235-104188/report.md");
        StringAssert.Contains(audit, "artifacts/visual-checks/20260719-012207-886-65184/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-012259-999/report.md");
        StringAssert.Contains(audit, "artifacts/gallery-recordings/20260719-012334-172/report.md");
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
