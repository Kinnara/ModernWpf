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
            Path.Combine("ModernWpf", "Styles", "RichTextBox.xaml"),
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
}
