using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class PublicResourceKeyContractTests
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly string ContractRoot =
        Path.Combine(AppContext.BaseDirectory, "PublicResourceContract");

    [TestMethod]
    public void DeclaredPublicResourceKeysExistAsLiteralTopLevelKeys()
    {
        var entries = ReadContractEntries().ToArray();
        Assert.IsTrue(entries.Length > 0, "The public resource-key contract is empty.");

        foreach (var sourceGroup in entries.GroupBy(entry => entry.SourcePath))
        {
            var sourcePath = Path.Combine(
                ContractRoot,
                sourceGroup.Key.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(sourcePath), $"Contract source '{sourcePath}' does not exist.");

            var actualKeys = ReadLiteralTopLevelKeys(sourcePath);
            foreach (var entry in sourceGroup)
            {
                Assert.IsTrue(
                    actualKeys.Contains(entry.Key),
                    $"Public resource key '{entry.Key}' is missing from '{entry.SourcePath}'.");
            }
        }
    }

    [TestMethod]
    public void ContractFilesAreSortedUniqueAndUseApprovedSources()
    {
        var approvedSources = new HashSet<string>(StringComparer.Ordinal)
        {
            "DensityStyles/Compact.xaml",
            "ModernWpfControlsResources.xaml",
            "ThemeResources/Dark.xaml",
            "ThemeResources/HighContrast.xaml",
            "ThemeResources/Light.xaml"
        };
        var allEntries = new HashSet<string>(StringComparer.Ordinal);

        foreach (var contractFileName in ContractFileNames)
        {
            var contractPath = Path.Combine(ContractRoot, contractFileName);
            var lines = ReadDataLines(contractPath).ToArray();
            var sortedLines = lines.OrderBy(line => line, StringComparer.Ordinal).ToArray();

            CollectionAssert.AreEqual(
                sortedLines,
                lines,
                $"'{contractFileName}' must be sorted using ordinal order.");

            foreach (var line in lines)
            {
                Assert.IsTrue(
                    allEntries.Add(line),
                    $"Duplicate public resource-key contract entry '{line}'.");

                var entry = ParseEntry(line);
                Assert.IsTrue(
                    approvedSources.Contains(entry.SourcePath),
                    $"Resource contract source '{entry.SourcePath}' is not approved.");
                Assert.IsFalse(
                    entry.Key.StartsWith("{", StringComparison.Ordinal),
                    $"Markup-extension key '{entry.Key}' must not be a public literal-key contract.");
            }
        }
    }

    private static readonly string[] ContractFileNames =
    {
        "PublicResourceKeys.Shipped.txt",
        "PublicResourceKeys.Unshipped.txt"
    };

    private static IEnumerable<(string SourcePath, string Key)> ReadContractEntries()
    {
        foreach (var contractFileName in ContractFileNames)
        {
            var contractPath = Path.Combine(ContractRoot, contractFileName);
            foreach (var line in ReadDataLines(contractPath))
            {
                yield return ParseEntry(line);
            }
        }
    }

    private static IEnumerable<string> ReadDataLines(string path)
    {
        Assert.IsTrue(File.Exists(path), $"Contract file '{path}' does not exist.");

        return File.ReadLines(path)
            .Select(line => line.Trim())
            .Where(line => line.Length != 0 && !line.StartsWith("#", StringComparison.Ordinal));
    }

    private static (string SourcePath, string Key) ParseEntry(string line)
    {
        var separatorIndex = line.IndexOf('|');
        Assert.IsTrue(
            separatorIndex > 0 && separatorIndex < line.Length - 1,
            $"Invalid resource-key contract entry '{line}'.");

        return (line[..separatorIndex], line[(separatorIndex + 1)..]);
    }

    private static HashSet<string> ReadLiteralTopLevelKeys(string path)
    {
        var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        Assert.IsNotNull(document.Root, $"Resource dictionary '{path}' has no root element.");

        var keyName = XName.Get("Key", XamlNamespace);
        return document.Root!
            .Elements()
            .Select(element => (string?)element.Attribute(keyName))
            .Where(key => !string.IsNullOrWhiteSpace(key) &&
                          !key.StartsWith("{", StringComparison.Ordinal))
            .Select(key => key!)
            .ToHashSet(StringComparer.Ordinal);
    }
}
