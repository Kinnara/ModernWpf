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
        var control = Read(repoRoot, "ModernWpf.Controls", "TitleBar", "TitleBar.cs");
        var template = Read(repoRoot, "ModernWpf.Controls", "TitleBar", "TitleBar.xaml");
        var templateSettings = Read(
            repoRoot,
            "ModernWpf.Controls",
            "TitleBar",
            "TitleBarTemplateSettings.cs");
        var strings = Read(
            repoRoot,
            "ModernWpf.Controls",
            "TitleBar",
            "Strings",
            "Resources.resx");
        var peer = Read(
            repoRoot,
            "ModernWpf.Controls",
            "TitleBar",
            "TitleBarAutomationPeer.cs");
        var generic = Read(repoRoot, "ModernWpf.Controls", "Themes", "Generic.xaml");
        var publicApi = Read(repoRoot, "ModernWpf.Controls", "PublicAPI.Unshipped.txt");
        var publicResourceKeys = Read(repoRoot, "ModernWpf", "PublicResourceKeys.Unshipped.txt");
        var controlsResources = Read(repoRoot, "ModernWpf", "ModernWpfControlsResources.xaml");
        var highContrastResources = Read(repoRoot, "ModernWpf", "ThemeResources", "HighContrast.xaml");
        var tests = Read(
            repoRoot,
            "test",
            "ModernWpf.WinUI.Tests",
            "TitleBar",
            "TitleBarControlApiTests.cs");
        var galleryFactory = Read(
            repoRoot,
            "ModernWpf.Gallery",
            "Pages",
            "WindowingSampleFactory.cs");
        var visualHarness = Read(
            repoRoot,
            "tools",
            "visual-checks",
            "Run-GalleryVisualChecks.ps1");
        var interactionHarness = Read(
            repoRoot,
            "tools",
            "visual-checks",
            "Record-GalleryControlInteractions.ps1");

        StringAssert.Contains(audit, "e1aa8f64df98d6229f6cd4074d59b654616254da");
        StringAssert.Contains(audit, "a97562621a1d1ea397a38a3f512c9eef99db52d8");
        StringAssert.Contains(audit, "3669519356c67f1376152c33ed8ea45003a91f3a");
        StringAssert.Contains(audit, "f540ca36b93e557b6b9f1221fc7c08b988ca6fd0");
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
        StringAssert.Contains(audit, "WPF has no `InputNonClientPointerSource`");
        StringAssert.Contains(audit, "final clean tip");
        StringAssert.Contains(audit, "real OS High Contrast");
        Assert.IsFalse(
            audit.Contains("does not currently ship a WinUI `TitleBar` clone", StringComparison.Ordinal));

        StringAssert.Contains(control, "public class TitleBar : Control");
        StringAssert.Contains(control, "public static readonly DependencyProperty AutoRefreshDragRegionsProperty");
        StringAssert.Contains(control, "typeof(bool?)");
        StringAssert.Contains(control, "public static bool? GetIsDragRegion(UIElement element)");
        StringAssert.Contains(control, "element.ClearValue(IsDragRegionProperty);");
        StringAssert.Contains(control, "public void RecomputeDragRegions()");
        StringAssert.Contains(control, "WindowChrome.SetIsHitTestVisibleInChrome(this, true);");
        StringAssert.Contains(control, "window.DragMove();");
        StringAssert.Contains(control, "window.WindowState == WindowState.Maximized");
        StringAssert.Contains(control, "IsBackButtonVisible == IsPaneToggleButtonVisible");
        StringAssert.Contains(control, "private bool IsDragTarget(DependencyObject originalSource)");
        StringAssert.Contains(control, "current is Control control && control.IsEnabled");
        StringAssert.Contains(control, "ReferenceEquals(current, _leftHeaderPresenter)");
        StringAssert.Contains(control, "ReferenceEquals(current, _rightHeaderPresenter)");
        StringAssert.Contains(control, "SR_NavigationButtonToggleName");
        StringAssert.Contains(control, "InitializeButtonAccessibility();");
        StringAssert.Contains(control, "return new TitleBarAutomationPeer(this);");
        StringAssert.Contains(templateSettings, "public class TitleBarTemplateSettings : DependencyObject");
        StringAssert.Contains(peer, "return AutomationControlType.TitleBar;");
        StringAssert.Contains(peer, "return nameof(TitleBar);");
        StringAssert.Contains(strings, "<data name=\"NavigationButtonToggleName\"");
        StringAssert.Contains(strings, "<value>Toggle Navigation</value>");

        StringAssert.Contains(template, "x:Name=\"PART_BackButton\"");
        StringAssert.Contains(template, "x:Name=\"PART_PaneToggleButton\"");
        StringAssert.Contains(template, "AutomationProperties.AutomationId=\"TitleBarBackButton\"");
        StringAssert.Contains(template, "AutomationProperties.AutomationId=\"TitleBarPaneToggleButton\"");
        StringAssert.Contains(template, "TitleBarPaneToggleButtonBackgroundPointerOver");
        StringAssert.Contains(template, "TitleBarPaneToggleButtonForegroundPressed");
        StringAssert.Contains(template, "{x:Static SystemParameters.FocusVisualStyleKey}");
        StringAssert.Contains(template, "x:Name=\"ExpandedHeight\"");
        StringAssert.Contains(template, "x:Name=\"Compact\"");
        StringAssert.Contains(template, "x:Name=\"PART_MinDragRegion\"");
        StringAssert.Contains(template, "Width=\"{DynamicResource TitleBarMinDragRegionWidth}\"");
        Assert.IsFalse(template.Contains("TitleBarMinDragRegionGridLength", StringComparison.Ordinal));
        StringAssert.Contains(generic, "Source=\"/ModernWpf.Controls;component/TitleBar/TitleBar.xaml\"");

        StringAssert.Contains(publicApi, "ModernWpf.Controls.TitleBar");
        StringAssert.Contains(publicApi, "ModernWpf.Controls.TitleBarTemplateSettings");
        StringAssert.Contains(publicApi, "ModernWpf.Automation.Peers.TitleBarAutomationPeer");
        StringAssert.Contains(publicApi, "ModernWpf.Controls.TitleBar.AutoRefreshDragRegions.get -> bool");
        StringAssert.Contains(publicApi, "~static ModernWpf.Controls.TitleBar.GetIsDragRegion(System.Windows.UIElement element) -> bool?");
        StringAssert.Contains(publicResourceKeys, "ModernWpfControlsResources.xaml|TitleBarMinDragRegionWidth");
        StringAssert.Contains(publicResourceKeys, "ThemeResources/Light.xaml|TitleBarBackButtonBackgroundPointerOver");
        StringAssert.Contains(publicResourceKeys, "ThemeResources/Dark.xaml|TitleBarPaneToggleButtonForegroundPressed");
        StringAssert.Contains(publicResourceKeys, "ThemeResources/HighContrast.xaml|TitleBarSubtitleForegroundBrush");
        StringAssert.Contains(controlsResources, "x:Key=\"TitleBarContentHorizontalAlignment\"");
        StringAssert.Contains(highContrastResources, "x:Key=\"TitleBarBackButtonBackgroundPointerOver\" ResourceKey=\"SystemControlHighlightListLowBrush\"");

        StringAssert.Contains(tests, "DefaultsAndSettersMatchCurrentWinUIV11Contract");
        StringAssert.Contains(tests, "DragRegionOverridesInteractiveControlDefaults");
        StringAssert.Contains(tests, "WindowTitleIsAppliedAndRestoredWithoutOverwritingExternalChanges");
        StringAssert.Contains(tests, "AutomationPeerUsesTitleBarRoleAndTitleFallback");
        StringAssert.Contains(tests, "BackAndPaneButtonsUseTheirOwnStateResourceContracts");
        StringAssert.Contains(tests, "PublicLayoutResourcesOverrideTheLiveTemplate");
        StringAssert.Contains(tests, "LeftHeaderSpacingMatchesTheCurrentWinUIButtonCombinationRule");
        StringAssert.Contains(tests, "ExtendedChromeTreatsTheTitleBarTreeAsClientInput");

        StringAssert.Contains(galleryFactory, "new Mux.TitleBar");
        StringAssert.Contains(galleryFactory, "TitleBarContentHorizontalAlignment");
        StringAssert.Contains(galleryFactory, "MaxWidth = 580");
        StringAssert.Contains(galleryFactory, "PlaceholderText = \"Search...\"");
        StringAssert.Contains(galleryFactory, "TitleBarDragRegionsXaml");
        StringAssert.Contains(galleryFactory, "CreateTitleBarDragRegionsWindowBody");
        StringAssert.Contains(galleryFactory, "titleBar.RecomputeDragRegions();");
        StringAssert.Contains(galleryFactory, "WindowTitleBar.SetExtendsContentIntoTitleBar(this, true);");
        Assert.IsFalse(galleryFactory.Contains("this.SetTitleBar(titleBar);", StringComparison.Ordinal));

        StringAssert.Contains(visualHarness, "TitleBar = 3");
        StringAssert.Contains(interactionHarness, "\"TitleBar\" { return \"TitleBarBackButton\" }");
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
