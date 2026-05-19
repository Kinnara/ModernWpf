using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests;

[TestClass]
public class TemplateParityTests
{
    [TestMethod]
    public void ProductTemplatesDoNotUseContentControlExAsPresenterSlot()
    {
        var repoRoot = FindRepoRoot();
        var productTemplateRoots = new[]
        {
            Path.Combine(repoRoot, "ModernWpf"),
            Path.Combine(repoRoot, "ModernWpf.Controls")
        };

        var offenders = productTemplateRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
            .Where(path => !Path.GetRelativePath(repoRoot, path)
                .Equals(Path.Combine("ModernWpf", "Themes", "ContentControlEx.xaml"), StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => FindContentControlExElementUses(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Template content slots should use ContentPresenterEx, matching WinUI presenter usage. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void ProductTemplatesUseExLayoutControlsForWinUILayoutAttributes()
    {
        var repoRoot = FindRepoRoot();
        var productTemplateRoots = new[]
        {
            Path.Combine(repoRoot, "ModernWpf"),
            Path.Combine(repoRoot, "ModernWpf.Controls")
        };

        var offenders = productTemplateRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
            .SelectMany(path => FindNativeLayoutElementsWithWinUILayoutAttributes(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Native WPF layout elements cannot host WinUI layout/chrome attributes; use the matching *Ex layout control. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void ProductTemplatesDoNotContainRawWinUIVisualStateSetters()
    {
        var repoRoot = FindRepoRoot();
        var productTemplateRoots = new[]
        {
            Path.Combine(repoRoot, "ModernWpf"),
            Path.Combine(repoRoot, "ModernWpf.Controls")
        };

        var offenders = productTemplateRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
            .SelectMany(path => FindRawWinUIVisualStateSetterUses(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Raw WinUI <VisualState.Setters> syntax does not parse in WPF; use ui:VisualStateEx.Setters after normalization. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void ProductTemplatesUseVisualStateExForConvertedStateSetters()
    {
        var repoRoot = FindRepoRoot();
        var convertedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "ProgressBar", "ProgressBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "AutoSuggestBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "Pivot.xaml"),
            Path.Combine("ModernWpf", "Styles", "NavigationBackButton.xaml"),
            Path.Combine("ModernWpf", "TitleBar", "TitleBarControl.xaml"),
            Path.Combine("ModernWpf.Controls", "BreadcrumbBar", "BreadcrumbBar.xaml"),
            Path.Combine("ModernWpf.Controls", "ColorPicker", "ColorPicker.xaml"),
            Path.Combine("ModernWpf.Controls", "ContentDialog", "ContentDialog.xaml"),
            Path.Combine("ModernWpf.Controls", "DropDownButton", "DropDownButton.xaml"),
            Path.Combine("ModernWpf.Controls", "CommandBar", "AppBarButton.xaml"),
            Path.Combine("ModernWpf.Controls", "CommandBar", "AppBarSeparator.xaml"),
            Path.Combine("ModernWpf.Controls", "CommandBar", "AppBarToggleButton.xaml"),
            Path.Combine("ModernWpf.Controls", "CommandBar", "CommandBar.xaml"),
            Path.Combine("ModernWpf.Controls", "HyperlinkButton", "HyperlinkButton.xaml"),
            Path.Combine("ModernWpf.Controls", "InfoBar", "InfoBar.xaml"),
            Path.Combine("ModernWpf.Controls", "InfoBadge", "InfoBadge.xaml"),
            Path.Combine("ModernWpf.Controls", "NavigationView", "NavigationView.xaml"),
            Path.Combine("ModernWpf.Controls", "NumberBox", "NumberBox.xaml"),
            Path.Combine("ModernWpf.Controls", "PagerControl", "PagerControl.xaml"),
            Path.Combine("ModernWpf.Controls", "PersonPicture", "PersonPicture.xaml"),
            Path.Combine("ModernWpf.Controls", "PipsPager", "PipsPager.xaml"),
            Path.Combine("ModernWpf.Controls", "ProgressRing", "ProgressRing.xaml"),
            Path.Combine("ModernWpf.Controls", "RadioButtons", "RadioButtons.xaml"),
            Path.Combine("ModernWpf.Controls", "RadioMenuItem", "RadioMenuItem.xaml"),
            Path.Combine("ModernWpf.Controls", "RatingControl", "RatingControl.xaml"),
            Path.Combine("ModernWpf.Controls", "SplitButton", "SplitButton.xaml"),
            Path.Combine("ModernWpf.Controls", "SplitView", "SplitView.xaml"),
            Path.Combine("ModernWpf.Controls", "SwipeControl", "SwipeControl.xaml"),
            Path.Combine("ModernWpf.Controls", "TeachingTip", "TeachingTip.xaml"),
            Path.Combine("ModernWpf.Controls", "ToggleSwitch", "ToggleSwitch.xaml"),
            Path.Combine("ModernWpf.Controls", "TwoPaneView", "TwoPaneView.xaml")
        };

        var offenders = convertedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .Where(path => !File.ReadAllText(path).Contains("VisualStateEx.Setters", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Templates with converted WinUI VisualState.Setters should keep using VisualStateEx.Setters. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx()
    {
        var repoRoot = FindRepoRoot();
        var officialWpfFluentTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "Button.xaml"),
            Path.Combine("ModernWpf", "Styles", "Calendar.xaml"),
            Path.Combine("ModernWpf", "Styles", "CheckBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "ComboBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "ContentControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "DataGrid.xaml"),
            Path.Combine("ModernWpf", "Styles", "DatePicker.xaml"),
            Path.Combine("ModernWpf", "Styles", "Expander.xaml"),
            Path.Combine("ModernWpf", "Styles", "Frame.xaml"),
            Path.Combine("ModernWpf", "Styles", "GridSplitter.xaml"),
            Path.Combine("ModernWpf", "Styles", "GroupBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "GroupItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "HeaderedContentControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "Hyperlink.xaml"),
            Path.Combine("ModernWpf", "Styles", "ItemsControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "Label.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListBoxItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "GridView.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListView.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListViewItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "Menu.xaml"),
            Path.Combine("ModernWpf", "Styles", "ContextMenu.xaml"),
            Path.Combine("ModernWpf", "Styles", "MenuItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "NavigationWindow.xaml"),
            Path.Combine("ModernWpf", "Styles", "Page.xaml"),
            Path.Combine("ModernWpf", "Styles", "ProgressBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "RadioButton.xaml"),
            Path.Combine("ModernWpf", "Styles", "RepeatButton.xaml"),
            Path.Combine("ModernWpf", "Styles", "ResizeGrip.xaml"),
            Path.Combine("ModernWpf", "Styles", "RichTextBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "Separator.xaml"),
            Path.Combine("ModernWpf", "Styles", "Slider.xaml"),
            Path.Combine("ModernWpf", "Styles", "ScrollBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "ScrollViewer.xaml"),
            Path.Combine("ModernWpf", "Styles", "StatusBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "TabControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "TextBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "TextStyles.xaml"),
            Path.Combine("ModernWpf", "Styles", "PasswordBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "Thumb.xaml"),
            Path.Combine("ModernWpf", "Styles", "ToolTip.xaml"),
            Path.Combine("ModernWpf", "Styles", "ToolBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "ToggleButton.xaml"),
            Path.Combine("ModernWpf", "Styles", "TreeView.xaml"),
            Path.Combine("ModernWpf", "Styles", "TreeViewItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "UserControl.xaml")
        };

        var offenders = officialWpfFluentTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .Where(path => File.ReadAllText(path).Contains("VisualStateEx", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Stock controls aligned to official WPF Fluent should use WPF template mechanisms instead of VisualStateEx. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void ProductTemplatesDoNotUseDropShadowEffectOutsideOfficialWpfFluentStockShadows()
    {
        var repoRoot = FindRepoRoot();
        var productTemplateRoots = new[]
        {
            Path.Combine(repoRoot, "ModernWpf"),
            Path.Combine(repoRoot, "ModernWpf.Controls")
        };
        var officialWpfFluentShadowFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "ComboBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "DatePicker.xaml"),
            Path.Combine("ModernWpf", "Styles", "MenuItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "ToolTip.xaml")
        };

        var offenders = productTemplateRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
            .Select(path => Path.GetRelativePath(repoRoot, path))
            .Where(path => File.ReadAllText(Path.Combine(repoRoot, path)).Contains("DropShadowEffect", StringComparison.Ordinal))
            .Except(officialWpfFluentShadowFiles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var missingAllowedShadows = officialWpfFluentShadowFiles
            .Where(path => !File.ReadAllText(Path.Combine(repoRoot, path)).Contains("DropShadowEffect", StringComparison.Ordinal))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "WinUI-source-backed templates should use ThemeShadowChrome instead of raw WPF DropShadowEffect. Offenders: " + string.Join("; ", offenders));
        Assert.IsFalse(
            missingAllowedShadows.Any(),
            "Update this guard when an official WPF Fluent stock-control shadow no longer uses DropShadowEffect. Missing: " + string.Join("; ", missingAllowedShadows));
    }

    [TestMethod]
    public void ProductTemplatesUseOnlyKnownThemeShadowChromeHosts()
    {
        var repoRoot = FindRepoRoot();
        var productTemplateRoots = new[]
        {
            Path.Combine(repoRoot, "ModernWpf"),
            Path.Combine(repoRoot, "ModernWpf.Controls")
        };
        var expectedHosts = new[]
        {
            Path.Combine("ModernWpf.Controls", "AutoSuggestBox", "AutoSuggestBox.xaml"),
            Path.Combine("ModernWpf.Controls", "CommandBar", "CommandBar.xaml"),
            Path.Combine("ModernWpf.Controls", "CommandBarFlyout", "CommandBarFlyout.xaml"),
            Path.Combine("ModernWpf.Controls", "ContentDialog", "ContentDialog.xaml"),
            Path.Combine("ModernWpf.Controls", "Flyout", "FlyoutPresenter.xaml"),
            Path.Combine("ModernWpf.Controls", "MenuFlyout", "MenuFlyout.xaml"),
            Path.Combine("ModernWpf.Controls", "NavigationView", "NavigationView.xaml"),
            Path.Combine("ModernWpf.Controls", "NumberBox", "NumberBox.xaml"),
            Path.Combine("ModernWpf.Controls", "TeachingTip", "TeachingTip.xaml")
        };

        var actualHostEntries = productTemplateRoots
            .Where(Directory.Exists)
            .SelectMany(root => Directory.EnumerateFiles(root, "*.xaml", SearchOption.AllDirectories))
            .SelectMany(path => FindThemeShadowChromeElementUses(repoRoot, path))
            .ToArray();
        var actualHostPaths = actualHostEntries
            .Select(GetEntryPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var missing = expectedHosts
            .Except(actualHostPaths, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var extra = actualHostPaths
            .Except(expectedHosts, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var duplicateHosts = actualHostEntries
            .GroupBy(GetEntryPath, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({group.Count()} hosts: {string.Join(", ", group)})")
            .ToArray();

        Assert.AreEqual(expectedHosts.Length, actualHostEntries.Length, "Unexpected ThemeShadowChrome host count.");
        Assert.IsFalse(missing.Any(), "Missing source-backed ThemeShadowChrome hosts: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "New ThemeShadowChrome hosts need source evidence, rendered-pixel coverage, and docs: " + string.Join("; ", extra));
        Assert.IsFalse(duplicateHosts.Any(), "Unexpected duplicate ThemeShadowChrome host rows: " + string.Join("; ", duplicateHosts));
    }

    [TestMethod]
    public void ThemeShadowSourceCoverageAuditCoversKnownWinUIShadowInputs()
    {
        var repoRoot = FindRepoRoot();
        var auditFile = Path.Combine(repoRoot, "docs", "theme-shadow-source-coverage.md");
        var expectedSources = new[]
        {
            @"src\controls\dev\CommonStyles\Common_themeresources.xaml",
            @"src\dxaml\xcp\dxaml\lib\ElevationHelper.cpp",
            @"src\dxaml\xcp\components\graphics\ThemeShadow.cpp",
            @"src\dxaml\xcp\components\graphics\ProjectedShadowManager.cpp",
            @"src\dxaml\test\native\external\foundation\graphics\rendering\ThemeShadowTests.cpp",
            @"src\controls\dev\NumberBox\NumberBox.cpp",
            @"src\dxaml\xcp\dxaml\lib\CommandBar_Partial.cpp",
            @"src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp",
            @"src\controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml",
            @"src\controls\dev\NavigationView\NavigationView.cpp",
            @"src\controls\dev\TeachingTip\TeachingTip.cpp",
            @"src\controls\dev\TeachingTip\TeachingTip.cpp",
            @"src\dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.cpp",
            @"src\dxaml\xcp\dxaml\lib\ContentDialog_Partial.cpp",
            @"src\dxaml\xcp\dxaml\lib\FlyoutPresenter_partial.cpp",
            @"src\dxaml\xcp\dxaml\lib\MenuFlyoutPresenter_Partial.cpp",
            @"src\dxaml\xcp\dxaml\lib\ComboBox_Partial.cpp",
            @"src\dxaml\xcp\dxaml\lib\ToolTip_Partial.cpp",
            @"src\dxaml\phone\lib\DatePickerFlyoutPresenter_Partial.cpp",
            @"src\dxaml\phone\lib\TimePickerFlyoutPresenter_Partial.cpp",
            @"src\dxaml\xcp\dxaml\lib\UIElement_Partial.cpp"
        };
        var allowedStatuses = new[]
        {
            "Source-backed renderer recipe",
            "Source-backed ThemeShadowChrome",
            "Official WPF Fluent stock exception",
            "Documented WPF substitution"
        };

        Assert.IsTrue(File.Exists(auditFile), "Missing ThemeShadow source coverage audit.");

        var rows = ParseThemeShadowSourceCoverageRows(auditFile);
        var rowSources = rows.Select(row => row.SourceFile).ToArray();
        var missing = expectedSources
            .Except(rowSources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var extra = rowSources
            .Except(expectedSources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var duplicateStatusRows = rows
            .GroupBy(row => $"{row.SourceFile}\0{row.Status}", StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.Replace("\0", ":", StringComparison.Ordinal))
            .ToArray();
        var badStatuses = rows
            .Where(row => !allowedStatuses.Contains(row.Status, StringComparer.Ordinal))
            .Select(row => $"{row.SourceFile}:{row.LineNumber} {row.Status}")
            .ToArray();
        var missingEvidence = rows
            .Where(row => row.ArtifactPaths.Length == 0 ||
                row.ArtifactPaths.Any(path => !File.Exists(Path.Combine(repoRoot, path))))
            .Select(row => $"{row.SourceFile}:{row.LineNumber}")
            .ToArray();

        Assert.AreEqual(expectedSources.Length, rows.Length, "Unexpected ThemeShadow source coverage row count.");
        Assert.IsFalse(missing.Any(), "Missing ThemeShadow source rows: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "Unexpected ThemeShadow source rows: " + string.Join("; ", extra));
        Assert.IsFalse(duplicateStatusRows.Any(), "Duplicate ThemeShadow source/status rows: " + string.Join("; ", duplicateStatusRows));
        Assert.IsFalse(badStatuses.Any(), "Invalid ThemeShadow source statuses: " + string.Join("; ", badStatuses));
        Assert.IsFalse(missingEvidence.Any(), "ThemeShadow source rows should point at existing repo evidence: " + string.Join("; ", missingEvidence));

        AssertCoverageStatus(rows, @"src\controls\dev\NumberBox\NumberBox.cpp", "Source-backed ThemeShadowChrome");
        AssertCoverageStatus(rows, @"src\dxaml\xcp\components\graphics\ThemeShadow.cpp", "Source-backed renderer recipe");
        AssertCoverageStatus(rows, @"src\dxaml\xcp\dxaml\lib\ComboBox_Partial.cpp", "Official WPF Fluent stock exception");
        AssertCoverageStatus(rows, @"src\dxaml\xcp\dxaml\lib\ToolTip_Partial.cpp", "Official WPF Fluent stock exception");
        AssertCoverageStatus(rows, @"src\dxaml\phone\lib\TimePickerFlyoutPresenter_Partial.cpp", "Documented WPF substitution");
    }

    [TestMethod]
    public void ThemeShadowReferenceCaptureManifestCoversRenderedSnapshotTargets()
    {
        var repoRoot = FindRepoRoot();
        var manifestFile = Path.Combine(repoRoot, "docs", "theme-shadow-reference-captures.json");
        var expectedTargets = GetExpectedThemeShadowReferenceCaptureTargets();

        Assert.IsTrue(File.Exists(manifestFile), "Missing ThemeShadow reference capture manifest.");

        using var document = JsonDocument.Parse(File.ReadAllText(manifestFile));
        var root = document.RootElement;
        Assert.AreEqual(1, root.GetProperty("version").GetInt32());
        Assert.AreEqual("MODERNWPF_SHADOW_SNAPSHOT_DIR", root.GetProperty("snapshotDirectoryEnvVar").GetString());
        Assert.AreEqual("MODERNWPF_SHADOW_REFERENCE_DIR", root.GetProperty("referenceDirectoryEnvVar").GetString());
        Assert.AreEqual("shadow-only", root.GetProperty("snapshotKind").GetString());

        var targets = root.GetProperty("targets")
            .EnumerateArray()
            .Select(ParseThemeShadowReferenceCaptureTarget)
            .ToArray();
        var actualFileBases = targets.Select(target => target.ReferenceFileBase).ToArray();
        var expectedFileBases = expectedTargets.Select(target => target.ReferenceFileBase).ToArray();
        var missing = expectedFileBases.Except(actualFileBases, StringComparer.Ordinal).ToArray();
        var extra = actualFileBases.Except(expectedFileBases, StringComparer.Ordinal).ToArray();
        var duplicateTargets = actualFileBases
            .GroupBy(fileBase => fileBase, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key} ({group.Count()})")
            .ToArray();
        var invalidEvidence = targets
            .Where(target => target.WinUIEvidence.Length == 0 ||
                target.WinUIEvidence.Any(path => !path.StartsWith("src/", StringComparison.Ordinal)))
            .Select(target => target.ReferenceFileBase)
            .ToArray();

        Assert.AreEqual(expectedTargets.Length, targets.Length, "Unexpected ThemeShadow reference capture target count.");
        Assert.IsFalse(missing.Any(), "Missing ThemeShadow reference capture targets: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "Unexpected ThemeShadow reference capture targets: " + string.Join("; ", extra));
        Assert.IsFalse(duplicateTargets.Any(), "Duplicate ThemeShadow reference capture targets: " + string.Join("; ", duplicateTargets));
        Assert.IsFalse(invalidEvidence.Any(), "ThemeShadow reference targets need WinUI source evidence: " + string.Join("; ", invalidEvidence));

        foreach (var expected in expectedTargets)
        {
            var actual = targets.Single(target => target.ReferenceFileBase == expected.ReferenceFileBase);
            Assert.AreEqual(expected.Name, actual.Name, expected.ReferenceFileBase);
            Assert.AreEqual(expected.Width, actual.Width, expected.ReferenceFileBase);
            Assert.AreEqual(expected.Height, actual.Height, expected.ReferenceFileBase);
            Assert.AreEqual(expected.ModernWpfTest, actual.ModernWpfTest, expected.ReferenceFileBase);
        }
    }

    [TestMethod]
    public void OfficialWpfFluentStyleCoverageAuditCoversSourceFolderInventory()
    {
        var repoRoot = FindRepoRoot();
        var auditFile = Path.Combine(repoRoot, "docs", "official-fluent-style-coverage.md");
        var expectedSourceFiles = new[]
        {
            "Button.xaml",
            "Calendar.xaml",
            "CheckBox.xaml",
            "CollectionViewGroup.xaml",
            "ComboBox.xaml",
            "ContentControl.xaml",
            "ContextMenu.xaml",
            "DataGrid.xaml",
            "DatePicker.xaml",
            "DocumentViewer.xaml",
            "Expander.xaml",
            "Frame.xaml",
            "GridSplitter.xaml",
            "GridView.xaml",
            "GroupBox.xaml",
            "GroupItem.xaml",
            "HeaderedContentControl.xaml",
            "Hyperlink.xaml",
            "ItemsControl.xaml",
            "Label.xaml",
            "ListBox.xaml",
            "ListBoxItem.xaml",
            "ListView.xaml",
            "ListViewItem.xaml",
            "Menu.xaml",
            "MenuItem.xaml",
            "NavigationWindow.xaml",
            "Page.xaml",
            "PasswordBox.xaml",
            "ProgressBar.xaml",
            "RadioButton.xaml",
            "RepeatButton.xaml",
            "ResizeGrip.xaml",
            "RichTextBox.xaml",
            "ScrollBar.xaml",
            "ScrollViewer.xaml",
            "Separator.xaml",
            "Slider.xaml",
            "StatusBar.xaml",
            "StatusBarItem.xaml",
            "TabControl.xaml",
            "TextBlock.xaml",
            "TextBox.xaml",
            "Thumb.xaml",
            "ToggleButton.xaml",
            "ToolBar.xaml",
            "ToolTip.xaml",
            "TreeView.xaml",
            "TreeViewItem.xaml",
            "UserControl.xaml",
            "Window.xaml"
        };
        var allowedStatuses = new[]
        {
            "Backported",
            "Folded",
            "Substituted",
            "Excluded"
        };

        Assert.IsTrue(File.Exists(auditFile), "Missing official WPF Fluent style coverage audit.");

        var rows = ParseOfficialWpfFluentCoverageRows(auditFile);
        var rowSourceFiles = rows.Select(row => row.SourceFile).ToArray();
        var missing = expectedSourceFiles
            .Except(rowSourceFiles, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var extra = rowSourceFiles
            .Except(expectedSourceFiles, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var duplicates = rows
            .GroupBy(row => row.SourceFile, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        var badStatuses = rows
            .Where(row => !allowedStatuses.Contains(row.Status, StringComparer.Ordinal))
            .Select(row => $"{row.SourceFile}:{row.LineNumber} {row.Status}")
            .ToArray();
        var missingArtifacts = rows
            .Where(row => row.Status != "Excluded")
            .Where(row => row.ArtifactPaths.Length == 0 ||
                row.ArtifactPaths.Any(path => !File.Exists(Path.Combine(repoRoot, path))))
            .Select(row => $"{row.SourceFile}:{row.LineNumber}")
            .ToArray();

        Assert.AreEqual(expectedSourceFiles.Length, rows.Length, "Unexpected official WPF Fluent source coverage row count.");
        Assert.IsFalse(missing.Any(), "Missing official WPF Fluent source rows: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "Unexpected official WPF Fluent source rows: " + string.Join("; ", extra));
        Assert.IsFalse(duplicates.Any(), "Duplicate official WPF Fluent source rows: " + string.Join("; ", duplicates));
        Assert.IsFalse(badStatuses.Any(), "Invalid official WPF Fluent coverage statuses: " + string.Join("; ", badStatuses));
        Assert.IsFalse(missingArtifacts.Any(), "Non-excluded official WPF Fluent coverage rows should point at existing ModernWpf artifacts: " + string.Join("; ", missingArtifacts));

        AssertCoverageStatus(rows, "Window.xaml", "Substituted");
        AssertCoverageStatus(rows, "TextBlock.xaml", "Folded");
        AssertCoverageStatus(rows, "StatusBarItem.xaml", "Folded");
        AssertCoverageStatus(rows, "CollectionViewGroup.xaml", "Folded");
        AssertCoverageStatus(rows, "DocumentViewer.xaml", "Excluded");
    }

    [TestMethod]
    public void WinUIControlSourceCoverageAuditCoversGenericResourceInventory()
    {
        var repoRoot = FindRepoRoot();
        var genericFile = Path.Combine(repoRoot, "ModernWpf.Controls", "Themes", "Generic.xaml");
        var auditFile = Path.Combine(repoRoot, "docs", "winui3-control-source-coverage.md");
        var allowedStatuses = new[]
        {
            "WinUI 3 source-backed WPF port",
            "WinUI 3 source-backed WPF family"
        };

        Assert.IsTrue(File.Exists(auditFile), "Missing WinUI 3 control source coverage audit.");

        var expectedResources = XDocument.Load(genericFile)
            .Descendants()
            .Where(element => element.Name.LocalName == "ResourceDictionary")
            .Select(element => element.Attribute("Source")?.Value)
            .Where(source => source != null &&
                source.StartsWith("/ModernWpf.Controls;component/", StringComparison.OrdinalIgnoreCase))
            .Select(source => source!.Substring("/ModernWpf.Controls;component/".Length))
            .ToArray();

        var rows = ParseWinUIControlSourceCoverageRows(auditFile);
        var rowResources = rows.Select(row => row.SourceFile).ToArray();
        var missing = expectedResources
            .Except(rowResources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var extra = rowResources
            .Except(expectedResources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var duplicates = rows
            .GroupBy(row => row.SourceFile, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        var badStatuses = rows
            .Where(row => !allowedStatuses.Contains(row.Status, StringComparer.Ordinal))
            .Select(row => $"{row.SourceFile}:{row.LineNumber} {row.Status}")
            .ToArray();
        var missingEvidence = rows
            .Where(row => row.ArtifactPaths.Length == 0 ||
                row.ArtifactPaths.Any(path => !File.Exists(Path.Combine(repoRoot, path))))
            .Select(row => $"{row.SourceFile}:{row.LineNumber}")
            .ToArray();

        Assert.AreEqual(expectedResources.Length, rows.Length, "Unexpected WinUI 3 control source coverage row count.");
        Assert.IsFalse(missing.Any(), "Missing WinUI 3 control resource rows: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "Unexpected WinUI 3 control resource rows: " + string.Join("; ", extra));
        Assert.IsFalse(duplicates.Any(), "Duplicate WinUI 3 control resource rows: " + string.Join("; ", duplicates));
        Assert.IsFalse(badStatuses.Any(), "Invalid WinUI 3 control coverage statuses: " + string.Join("; ", badStatuses));
        Assert.IsFalse(missingEvidence.Any(), "WinUI 3 control coverage rows should point at existing source-audit evidence: " + string.Join("; ", missingEvidence));

        AssertCoverageStatus(rows, "CommandBar/AppBarButton.xaml", "WinUI 3 source-backed WPF family");
        AssertCoverageStatus(rows, "Flyout/FlyoutPresenter.xaml", "WinUI 3 source-backed WPF family");
        AssertCoverageStatus(rows, "ToggleSwitch/ToggleSwitch.xaml", "WinUI 3 source-backed WPF port");
    }

    [TestMethod]
    public void ModernWpfCoreSourceCoverageAuditCoversGenericResourceInventory()
    {
        var repoRoot = FindRepoRoot();
        var genericFile = Path.Combine(repoRoot, "ModernWpf", "Themes", "Generic.xaml");
        var auditFile = Path.Combine(repoRoot, "docs", "modernwpf-core-resource-source-coverage.md");
        var allowedStatuses = new[]
        {
            "WinUI 3 source-backed WPF port",
            "WinUI 3 source-backed WPF compatibility layer",
            "Official WPF Fluent shell substitution",
            "ModernWpf compatibility resource"
        };

        Assert.IsTrue(File.Exists(auditFile), "Missing ModernWpf core resource source coverage audit.");

        var expectedResources = XDocument.Load(genericFile)
            .Descendants()
            .Where(element => element.Name.LocalName == "ResourceDictionary")
            .Select(element => element.Attribute("Source")?.Value)
            .Where(source => source != null &&
                source.StartsWith("/ModernWpf;component/", StringComparison.OrdinalIgnoreCase))
            .Select(source => source!.Substring("/ModernWpf;component/".Length))
            .ToArray();

        var rows = ParseWinUIControlSourceCoverageRows(auditFile);
        var rowResources = rows.Select(row => row.SourceFile).ToArray();
        var missing = expectedResources
            .Except(rowResources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var extra = rowResources
            .Except(expectedResources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var duplicates = rows
            .GroupBy(row => row.SourceFile, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        var badStatuses = rows
            .Where(row => !allowedStatuses.Contains(row.Status, StringComparer.Ordinal))
            .Select(row => $"{row.SourceFile}:{row.LineNumber} {row.Status}")
            .ToArray();
        var missingEvidence = rows
            .Where(row => row.ArtifactPaths.Length == 0 ||
                row.ArtifactPaths.Any(path => !File.Exists(Path.Combine(repoRoot, path))))
            .Select(row => $"{row.SourceFile}:{row.LineNumber}")
            .ToArray();

        Assert.AreEqual(expectedResources.Length, rows.Length, "Unexpected ModernWpf core resource source coverage row count.");
        Assert.IsFalse(missing.Any(), "Missing ModernWpf core resource rows: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "Unexpected ModernWpf core resource rows: " + string.Join("; ", extra));
        Assert.IsFalse(duplicates.Any(), "Duplicate ModernWpf core resource rows: " + string.Join("; ", duplicates));
        Assert.IsFalse(badStatuses.Any(), "Invalid ModernWpf core resource coverage statuses: " + string.Join("; ", badStatuses));
        Assert.IsFalse(missingEvidence.Any(), "ModernWpf core resource coverage rows should point at existing source-audit evidence: " + string.Join("; ", missingEvidence));

        AssertCoverageStatus(rows, "ProgressBar/ProgressBar.xaml", "WinUI 3 source-backed WPF port");
        AssertCoverageStatus(rows, "Themes/ContentControlEx.xaml", "WinUI 3 source-backed WPF compatibility layer");
        AssertCoverageStatus(rows, "TitleBar/TitleBarControl.xaml", "Official WPF Fluent shell substitution");
    }

    [TestMethod]
    public void ModernWpfCoreControlSourceCoverageAuditCoversControlsResourcesInventory()
    {
        var repoRoot = FindRepoRoot();
        var controlsResourcesFile = Path.Combine(repoRoot, "ModernWpf", "ModernWpfControlsResources.xaml");
        var auditFile = Path.Combine(repoRoot, "docs", "modernwpf-core-control-source-coverage.md");
        var allowedStatuses = new[]
        {
            "WinUI 3 source-backed WPF port",
            "WinUI 3 source-backed WPF family",
            "WinUI 3 source-backed WPF platform mapping",
            "Shared WinUI resource compatibility layer"
        };

        Assert.IsTrue(File.Exists(auditFile), "Missing ModernWpf core control source coverage audit.");

        var expectedResources = XDocument.Load(controlsResourcesFile)
            .Descendants()
            .Where(element => element.Name.LocalName == "ResourceDictionary")
            .Select(element => element.Attribute("Source")?.Value)
            .Where(source => source != null)
            .Select(source => source!.StartsWith("/ModernWpf;component/", StringComparison.OrdinalIgnoreCase)
                ? source.Substring("/ModernWpf;component/".Length)
                : source)
            .ToArray();

        var rows = ParseWinUIControlSourceCoverageRows(auditFile);
        var rowResources = rows.Select(row => row.SourceFile).ToArray();
        var missing = expectedResources
            .Except(rowResources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var extra = rowResources
            .Except(expectedResources, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var duplicates = rows
            .GroupBy(row => row.SourceFile, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();
        var badStatuses = rows
            .Where(row => !allowedStatuses.Contains(row.Status, StringComparer.Ordinal))
            .Select(row => $"{row.SourceFile}:{row.LineNumber} {row.Status}")
            .ToArray();
        var missingEvidence = rows
            .Where(row => row.ArtifactPaths.Length == 0 ||
                row.ArtifactPaths.Any(path => !File.Exists(Path.Combine(repoRoot, path))))
            .Select(row => $"{row.SourceFile}:{row.LineNumber}")
            .ToArray();

        Assert.AreEqual(expectedResources.Length, rows.Length, "Unexpected ModernWpf core control source coverage row count.");
        Assert.IsFalse(missing.Any(), "Missing ModernWpf core control resource rows: " + string.Join("; ", missing));
        Assert.IsFalse(extra.Any(), "Unexpected ModernWpf core control resource rows: " + string.Join("; ", extra));
        Assert.IsFalse(duplicates.Any(), "Duplicate ModernWpf core control resource rows: " + string.Join("; ", duplicates));
        Assert.IsFalse(badStatuses.Any(), "Invalid ModernWpf core control coverage statuses: " + string.Join("; ", badStatuses));
        Assert.IsFalse(missingEvidence.Any(), "ModernWpf core control coverage rows should point at existing source-audit evidence: " + string.Join("; ", missingEvidence));

        AssertCoverageStatus(rows, "Styles/Common.xaml", "Shared WinUI resource compatibility layer");
        AssertCoverageStatus(rows, "Styles/Pivot.xaml", "WinUI 3 source-backed WPF platform mapping");
        AssertCoverageStatus(rows, "Styles/NavigationView.xaml", "WinUI 3 source-backed WPF family");
    }

    [TestMethod]
    public void VisualStateSetterAuditUsesExplicitStatusBuckets()
    {
        var repoRoot = FindRepoRoot();
        var auditFile = Path.Combine(repoRoot, "docs", "winui-visualstate-setters-audit.md");
        var allowedStatuses = new[]
        {
            "Converted",
            "StructuralGap",
            "RuntimeGap",
            "Pending",
            "Unsupported",
            "Excluded"
        };
        var rowPattern = new Regex(@"^\|\s+`dev\\[^`]+`\s+\|\s+\d+\s+\|[^|]+\|\s+(?<status>[A-Za-z]+)\s+\|");

        var statuses = File.ReadAllLines(auditFile)
            .Select((line, index) => (
                LineNumber: index + 1,
                Status: rowPattern.Match(line).Groups["status"].Value))
            .Where(entry => !string.IsNullOrEmpty(entry.Status))
            .ToArray();

        Assert.IsTrue(statuses.Any(), "The WinUI VisualState.Setters audit should contain source-mapped status rows.");

        var offenders = statuses
            .Where(entry => entry.Status == "Partial" || !allowedStatuses.Contains(entry.Status, StringComparer.Ordinal))
            .Select(entry => $"docs{Path.DirectorySeparatorChar}winui-visualstate-setters-audit.md:{entry.LineNumber} {entry.Status}")
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "VisualState setter audit rows should use explicit actionable statuses, not the ambiguous Partial bucket. Offenders: " + string.Join("; ", offenders));

        var pendingRows = statuses
            .Where(entry => entry.Status == "Pending")
            .Select(entry => $"docs{Path.DirectorySeparatorChar}winui-visualstate-setters-audit.md:{entry.LineNumber}")
            .ToArray();

        Assert.IsFalse(
            pendingRows.Any(),
            "Audited WinUI VisualState.Setters rows should be converted or carry an explicit compatibility gap decision. Pending rows: " + string.Join("; ", pendingRows));
    }

    [TestMethod]
    public void BatchedSourceBackedPresenterSlotsDoNotUsePlainContentPresenter()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf.Controls", "ListView", "ListView.xaml"),
            Path.Combine("ModernWpf.Controls", "ListView", "GridView.xaml"),
            Path.Combine("ModernWpf.Controls", "NavigationView", "NavigationView.xaml"),
            Path.Combine("ModernWpf.Controls", "RadioMenuItem", "RadioMenuItem.xaml"),
            Path.Combine("ModernWpf.Controls", "TeachingTip", "TeachingTip.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These source-backed template files should use ContentPresenterEx for presenter slots. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreInputSourceBackedPresenterSlotsDoNotUsePlainContentPresenter()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "AutoSuggestBox.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These core input template files should use ContentPresenterEx for source-backed presenter slots. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreOfficialWpfFluentPresenterSlotsUseWpfPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var officialWpfFluentTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "ContentControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "HeaderedContentControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "ItemsControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "UserControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "Page.xaml"),
            Path.Combine("ModernWpf", "Styles", "Frame.xaml"),
            Path.Combine("ModernWpf", "Styles", "NavigationWindow.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListBoxItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "ComboBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "DataGrid.xaml"),
            Path.Combine("ModernWpf", "Styles", "GridView.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListView.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListViewItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "Expander.xaml"),
            Path.Combine("ModernWpf", "Styles", "TabControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "TreeView.xaml"),
            Path.Combine("ModernWpf", "Styles", "TreeViewItem.xaml")
        };

        var offenders = officialWpfFluentTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .Where(path => File.ReadAllText(path).Contains("ContentPresenterEx", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repoRoot, path))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Stock controls aligned to official WPF Fluent should use WPF presenters, not ContentPresenterEx. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreNavigationHeaderPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "Pivot.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These core navigation header template files should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreNavigationViewPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "NavigationView.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "NavigationView template files should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreResidualPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Themes", "ListViewHeaderItem.xaml"),
            Path.Combine("ModernWpf", "TitleBar", "TitleBarButton.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These residual presenter template files should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void SimpleShellPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Navigation", "Frame.xaml"),
            Path.Combine("ModernWpf", "Navigation", "Page.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These simple shell template files should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void RefreshVisualizerTemplateUsesWinUIRootContentHosting()
    {
        var repoRoot = FindRepoRoot();
        var templateFile = Path.Combine(repoRoot, "ModernWpf.Controls", "PullToRefresh", "RefreshVisualizer.xaml");

        var offenders = FindPlainContentPresenterElementUses(repoRoot, templateFile)
            .Concat(FindTextElementForegroundUses(repoRoot, templateFile))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "RefreshVisualizer should keep content hosting in code like WinUI3, not in a template ContentPresenter. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void DataGridOfficialWpfFluentPresenterSlotsUseWpfPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var templateFile = Path.Combine(repoRoot, "ModernWpf", "Styles", "DataGrid.xaml");
        var text = File.ReadAllText(templateFile);

        Assert.IsTrue(text.Contains("<ContentPresenter", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FontIconFallback", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridHelper", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridRowHelper", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("DataGridCellPresenter", StringComparison.Ordinal));
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

    private static string[] FindContentControlExElementUses(string repoRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repoRoot, path);
        var lines = File.ReadAllLines(path);

        return lines
            .Select((line, index) => (Line: line, LineNumber: index + 1))
            .Where(entry => Regex.IsMatch(entry.Line, @"<\s*(?:[A-Za-z_][\w.-]*:)?ContentControlEx(?=[\s>/])"))
            .Select(entry => $"{relativePath}:{entry.LineNumber}")
            .ToArray();
    }

    private static CoverageRow[] ParseOfficialWpfFluentCoverageRows(string path)
    {
        var rowPattern = new Regex(
            @"^\|\s+`(?<source>[^`]+\.xaml)`\s+\|\s+(?<artifact>[^|]+)\|\s+(?<status>[A-Za-z]+)\s+\|",
            RegexOptions.Compiled);

        return File.ReadAllLines(path)
            .Select((line, index) => (
                Match: rowPattern.Match(line),
                LineNumber: index + 1))
            .Where(entry => entry.Match.Success)
            .Select(entry => new CoverageRow(
                entry.Match.Groups["source"].Value,
                entry.Match.Groups["status"].Value,
                ExtractArtifactPaths(entry.Match.Groups["artifact"].Value),
                entry.LineNumber))
            .ToArray();
    }

    private static CoverageRow[] ParseWinUIControlSourceCoverageRows(string path)
    {
        var rowPattern = new Regex(
            @"^\|\s+`(?<source>[^`]+\.xaml)`\s+\|\s+(?<status>[^|]+?)\s+\|\s+(?<evidence>[^|]+)\|",
            RegexOptions.Compiled);

        return File.ReadAllLines(path)
            .Select((line, index) => (
                Match: rowPattern.Match(line),
                LineNumber: index + 1))
            .Where(entry => entry.Match.Success)
            .Select(entry => new CoverageRow(
                entry.Match.Groups["source"].Value,
                entry.Match.Groups["status"].Value.Trim(),
                ExtractArtifactPaths(entry.Match.Groups["evidence"].Value),
                entry.LineNumber))
            .ToArray();
    }

    private static CoverageRow[] ParseThemeShadowSourceCoverageRows(string path)
    {
        var rowPattern = new Regex(
            @"^\|\s+`(?<source>[^`]+)`\s+\|\s+[^|]+\|\s+(?<status>[^|]+?)\s+\|\s+(?<evidence>[^|]+)\|",
            RegexOptions.Compiled);

        return File.ReadAllLines(path)
            .Select((line, index) => (
                Match: rowPattern.Match(line),
                LineNumber: index + 1))
            .Where(entry => entry.Match.Success)
            .Select(entry => new CoverageRow(
                entry.Match.Groups["source"].Value,
                entry.Match.Groups["status"].Value.Trim(),
                ExtractArtifactPaths(entry.Match.Groups["evidence"].Value),
                entry.LineNumber))
            .ToArray();
    }

    private static string[] ExtractArtifactPaths(string artifactCell)
    {
        return Regex.Matches(artifactCell, @"`(?<path>[^`]+)`")
            .Cast<Match>()
            .Select(match => match.Groups["path"].Value)
            .Where(path => path.Contains("\\", StringComparison.Ordinal) ||
                path.Contains("/", StringComparison.Ordinal))
            .ToArray();
    }

    private static void AssertCoverageStatus(CoverageRow[] rows, string sourceFile, string expectedStatus)
    {
        var row = rows.SingleOrDefault(entry => entry.SourceFile.Equals(sourceFile, StringComparison.OrdinalIgnoreCase));

        Assert.IsNotNull(row.SourceFile, $"Missing {sourceFile} coverage row.");
        Assert.AreEqual(expectedStatus, row.Status, $"{sourceFile} should use the expected source coverage status.");
    }

    private static string[] FindPlainContentPresenterElementUses(string repoRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repoRoot, path);
        var lines = File.ReadAllLines(path);

        return lines
            .Select((line, index) => (Line: line, LineNumber: index + 1))
            .Where(entry => Regex.IsMatch(entry.Line, @"<\s*ContentPresenter(?=[\s>/])"))
            .Select(entry => $"{relativePath}:{entry.LineNumber}")
            .ToArray();
    }

    private static string[] FindThemeShadowChromeElementUses(string repoRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repoRoot, path);
        var lines = File.ReadAllLines(path);

        return lines
            .Select((line, index) => (Line: line, LineNumber: index + 1))
            .Where(entry => Regex.IsMatch(entry.Line, @"<\s*ui:ThemeShadowChrome(?=$|[\s>/])"))
            .Select(entry => $"{relativePath}:{entry.LineNumber}")
            .ToArray();
    }

    private static string GetEntryPath(string entry)
    {
        var separatorIndex = entry.LastIndexOf(':');
        return separatorIndex < 0 ? entry : entry.Substring(0, separatorIndex);
    }

    private static string[] FindTextElementForegroundUses(string repoRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repoRoot, path);
        var lines = File.ReadAllLines(path);

        return lines
            .Select((line, index) => (Line: line, LineNumber: index + 1))
            .Where(entry => entry.Line.Contains("TextElement.Foreground", StringComparison.Ordinal))
            .Select(entry => $"{relativePath}:{entry.LineNumber}")
            .ToArray();
    }

    private static string[] FindNativeLayoutElementsWithWinUILayoutAttributes(string repoRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repoRoot, path);
        var text = File.ReadAllText(path);
        var checks = new[]
        {
            (Element: "Grid", Attributes: "BackgroundSizing|BackgroundTransition|BorderBrush|BorderThickness|ChildrenTransitions|ColumnSpacing|CornerRadius|Padding|RowSpacing"),
            (Element: "StackPanel", Attributes: "BackgroundSizing|BackgroundTransition|BorderBrush|BorderThickness|ChildrenTransitions|CornerRadius|Padding|Spacing"),
            (Element: "Border", Attributes: "BackgroundSizing|BackgroundTransition|ChildTransitions"),
            (Element: "ContentPresenter", Attributes: "BackgroundSizing|BackgroundTransition|ContentTransitions|CornerRadius|HorizontalContentAlignment|LineHeight|MaxLines|Padding|TextWrapping|VerticalContentAlignment")
        };

        return checks
            .SelectMany(check => FindNativeLayoutElementWithAttributes(relativePath, text, check.Element, check.Attributes))
            .ToArray();
    }

    private static string[] FindNativeLayoutElementWithAttributes(string relativePath, string text, string element, string attributes)
    {
        var elementPattern = $@"<\s*(?<name>(?:[A-Za-z_][\w.-]*:)?{element})(?=[\s>/])(?<body>[^>]*)>";
        var attributePattern = $@"(?:^|\s)(?<attribute>(?:[A-Za-z_][\w.-]*:)?(?:{attributes}))\s*=";

        return Regex.Matches(text, elementPattern, RegexOptions.Singleline)
            .Cast<Match>()
            .Select(match => (
                LineNumber: text.Take(match.Index).Count(ch => ch == '\n') + 1,
                Attribute: Regex.Match(match.Groups["body"].Value, attributePattern).Groups["attribute"].Value))
            .Where(match => !string.IsNullOrEmpty(match.Attribute))
            .Select(match => $"{relativePath}:{match.LineNumber} {element}.{match.Attribute}")
            .ToArray();
    }

    private static ShadowReferenceCaptureTarget[] GetExpectedThemeShadowReferenceCaptureTargets()
    {
        return new[]
        {
            new ShadowReferenceCaptureTarget("FlyoutPresenter", "FlyoutPresenter-shadow-only", 140, 140, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("NumberBox compact popup", "NumberBox-compact-popup-shadow-only", 140, 140, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("AutoSuggestBox suggestions popup", "AutoSuggestBox-suggestions-popup-shadow-only", 280, 220, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("CommandBar overflow popup", "CommandBar-overflow-popup-shadow-only", 320, 180, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("CommandBarFlyout overflow root", "CommandBarFlyout-overflow-root-shadow-only", 320, 180, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("MenuFlyoutPresenter", "MenuFlyoutPresenter-shadow-only", 200, 140, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("TeachingTip content root", "TeachingTip-content-root-shadow-only", 320, 220, "LayoutCompatibilityApiTests.SourceBackedShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("ContentDialog background shadow", "ContentDialog-background-shadow-shadow-only", 260, 260, "LayoutCompatibilityApiTests.SourceBackedChildlessShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>()),
            new ShadowReferenceCaptureTarget("NavigationView pane overlay shadow", "NavigationView-pane-overlay-shadow-shadow-only", 130, 130, "LayoutCompatibilityApiTests.SourceBackedChildlessShadowTemplatesRenderVisibleShadowPixels", Array.Empty<string>())
        };
    }

    private static ShadowReferenceCaptureTarget ParseThemeShadowReferenceCaptureTarget(JsonElement element)
    {
        var canvasSize = element.GetProperty("canvasSize");
        var evidence = element.GetProperty("winuiEvidence")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();

        return new ShadowReferenceCaptureTarget(
            element.GetProperty("name").GetString() ?? string.Empty,
            element.GetProperty("referenceFileBase").GetString() ?? string.Empty,
            canvasSize.GetProperty("width").GetInt32(),
            canvasSize.GetProperty("height").GetInt32(),
            element.GetProperty("modernWpfTest").GetString() ?? string.Empty,
            evidence);
    }

    private readonly struct ShadowReferenceCaptureTarget
    {
        public ShadowReferenceCaptureTarget(
            string name,
            string referenceFileBase,
            int width,
            int height,
            string modernWpfTest,
            string[] winUIEvidence)
        {
            Name = name;
            ReferenceFileBase = referenceFileBase;
            Width = width;
            Height = height;
            ModernWpfTest = modernWpfTest;
            WinUIEvidence = winUIEvidence;
        }

        public string Name { get; }

        public string ReferenceFileBase { get; }

        public int Width { get; }

        public int Height { get; }

        public string ModernWpfTest { get; }

        public string[] WinUIEvidence { get; }
    }

    private readonly struct CoverageRow
    {
        public CoverageRow(string sourceFile, string status, string[] artifactPaths, int lineNumber)
        {
            SourceFile = sourceFile;
            Status = status;
            ArtifactPaths = artifactPaths;
            LineNumber = lineNumber;
        }

        public string SourceFile { get; }

        public string Status { get; }

        public string[] ArtifactPaths { get; }

        public int LineNumber { get; }
    }

    private static string[] FindRawWinUIVisualStateSetterUses(string repoRoot, string path)
    {
        var relativePath = Path.GetRelativePath(repoRoot, path);
        var lines = File.ReadAllLines(path);

        return lines
            .Select((line, index) => (Line: line, LineNumber: index + 1))
            .Where(entry => entry.Line.Contains("<VisualState.Setters", StringComparison.Ordinal) ||
                entry.Line.Contains("</VisualState.Setters", StringComparison.Ordinal))
            .Select(entry => $"{relativePath}:{entry.LineNumber}")
            .ToArray();
    }
}
