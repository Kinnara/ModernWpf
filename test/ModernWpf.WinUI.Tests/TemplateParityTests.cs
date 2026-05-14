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
}
