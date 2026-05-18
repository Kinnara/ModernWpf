using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            Path.Combine("ModernWpf", "Styles", "Calendar.xaml"),
            Path.Combine("ModernWpf", "Styles", "ComboBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "DatePicker.xaml"),
            Path.Combine("ModernWpf", "Styles", "Expander.xaml"),
            Path.Combine("ModernWpf", "Styles", "Pivot.xaml"),
            Path.Combine("ModernWpf", "Styles", "ScrollBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "TabControl.xaml"),
            Path.Combine("ModernWpf", "Styles", "MenuItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "NavigationBackButton.xaml"),
            Path.Combine("ModernWpf", "Styles", "TreeView.xaml"),
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
            Path.Combine("ModernWpf", "Styles", "CheckBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "GridSplitter.xaml"),
            Path.Combine("ModernWpf", "Styles", "GroupBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "GroupItem.xaml"),
            Path.Combine("ModernWpf", "Styles", "Hyperlink.xaml"),
            Path.Combine("ModernWpf", "Styles", "Label.xaml"),
            Path.Combine("ModernWpf", "Styles", "RadioButton.xaml"),
            Path.Combine("ModernWpf", "Styles", "RepeatButton.xaml"),
            Path.Combine("ModernWpf", "Styles", "ResizeGrip.xaml"),
            Path.Combine("ModernWpf", "Styles", "RichTextBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "Slider.xaml"),
            Path.Combine("ModernWpf", "Styles", "StatusBar.xaml"),
            Path.Combine("ModernWpf", "Styles", "ToolTip.xaml"),
            Path.Combine("ModernWpf", "Styles", "ToggleButton.xaml")
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
            Path.Combine("ModernWpf", "Styles", "AutoSuggestBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "ComboBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "DatePicker.xaml"),
            Path.Combine("ModernWpf", "Styles", "PasswordBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "TextBox.xaml")
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
    public void CoreItemSourceBackedPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "ListBox.xaml"),
            Path.Combine("ModernWpf", "Styles", "ListView.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These core item template files should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreMenuSourceBackedPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "MenuItem.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "The core menu template file should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void CoreNavigationHeaderPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "Pivot.xaml"),
            Path.Combine("ModernWpf", "Styles", "TabControl.xaml")
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
    public void CoreNavigationViewAndTreePresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var sourceBackedTemplateFiles = new[]
        {
            Path.Combine("ModernWpf", "Styles", "NavigationView.xaml"),
            Path.Combine("ModernWpf", "Styles", "TreeView.xaml")
        };

        var offenders = sourceBackedTemplateFiles
            .Select(path => Path.Combine(repoRoot, path))
            .SelectMany(path => FindPlainContentPresenterElementUses(repoRoot, path)
                .Concat(FindTextElementForegroundUses(repoRoot, path)))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "These NavigationView/TreeView template files should use ContentPresenterEx and direct Foreground routing. Offenders: " + string.Join("; ", offenders));
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
            Path.Combine("ModernWpf", "Navigation", "Page.xaml"),
            Path.Combine("ModernWpf", "Styles", "Expander.xaml"),
            Path.Combine("ModernWpf", "Styles", "Window.xaml")
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
    public void CalendarPresenterSlotsUseWinUIPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var templateFile = Path.Combine(repoRoot, "ModernWpf", "Styles", "Calendar.xaml");

        var offenders = FindPlainContentPresenterElementUses(repoRoot, templateFile)
            .Concat(FindTextElementForegroundUses(repoRoot, templateFile))
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "Calendar navigation and weekday text slots should use ContentPresenterEx/direct foreground routing instead of WPF ContentPresenter/TextElement.Foreground. Offenders: " + string.Join("; ", offenders));
    }

    [TestMethod]
    public void DataGridWpfSpecificPresenterSlotsUseModernPresenterShape()
    {
        var repoRoot = FindRepoRoot();
        var templateFile = Path.Combine(repoRoot, "ModernWpf", "Styles", "DataGrid.xaml");

        var offenders = FindPlainContentPresenterElementUses(repoRoot, templateFile)
            .ToArray();

        Assert.IsFalse(
            offenders.Any(),
            "WPF-specific DataGrid template content slots should use ContentPresenterEx for consistent template compatibility. Offenders: " + string.Join("; ", offenders));
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
