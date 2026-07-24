using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Markup;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class ThemeResourceExtensionTests
{
    [TestMethod]
    public void SystemColorBindingsRefreshWhenHighContrastToggles()
    {
        Assert.IsTrue(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.HighContrast),
            isHighContrast: true));
        Assert.IsTrue(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.HighContrast),
            isHighContrast: false));
    }

    [TestMethod]
    public void SystemColorBindingsRefreshForSystemParameterChangesWhileHighContrastIsActive()
    {
        Assert.IsTrue(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.WindowGlassColor),
            isHighContrast: true));
        Assert.IsFalse(ThemeResourceExtension.ShouldRefreshSystemColorBindings(
            nameof(SystemParameters.WindowGlassColor),
            isHighContrast: false));
    }

    [TestMethod]
    public void HighContrastSystemColorKeysAreBackedBySystemColorsSource()
    {
        var highContrastXaml = ReadRepoFile(
            "ModernWpf",
            "ThemeResources",
            "HighContrast.xaml");

        var referencedKeys = Regex
            .Matches(highContrastXaml, @"\bSystemColor[A-Za-z]+Color\b")
            .Cast<Match>()
            .Select(match => match.Value)
            .Distinct()
            .OrderBy(key => key)
            .ToArray();

        Assert.IsTrue(
            referencedKeys.Length > 0,
            "HighContrast.xaml should reference system color keys.");

        var sourceType = typeof(ThemeResourceExtension).GetNestedType(
            "SystemColorsSource",
            BindingFlags.NonPublic);
        Assert.IsNotNull(sourceType);

        var sourceKeys = sourceType!
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.PropertyType == typeof(Color))
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        var missingKeys = referencedKeys
            .Where(key => !sourceKeys.Contains(key))
            .ToArray();

        Assert.AreEqual(
            0,
            missingKeys.Length,
            "HighContrast.xaml references system color keys without ThemeResourceExtension source properties: " +
            string.Join(", ", missingKeys));
    }

    private static string ReadRepoFile(params string[] relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                var candidate = Path.Combine(
                    new[] { directory.FullName }.Concat(relativePath).ToArray());
                if (File.Exists(candidate))
                {
                    return File.ReadAllText(candidate);
                }

                Assert.Fail(
                    $"Could not find repository file '{string.Join(Path.DirectorySeparatorChar.ToString(), relativePath)}'.");
            }

            directory = directory.Parent;
        }

        Assert.Fail($"Could not find repository root from '{AppContext.BaseDirectory}'.");
        return string.Empty;
    }
}
