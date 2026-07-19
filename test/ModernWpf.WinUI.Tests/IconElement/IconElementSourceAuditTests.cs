using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.IconElements;

[TestClass]
public class IconElementSourceAuditTests
{
    [TestMethod]
    public void CurrentIconElementSourcePinsAndBehaviorAreGuarded()
    {
        var root = FindRepoRoot();
        var audit = Read(root, "docs", "iconelement-winui3-source-audit.md");
        var fontIcon = Read(root, "ModernWpf", "IconElement", "FontIcon.cs");
        var fontIconSource = Read(root, "ModernWpf", "IconSource", "FontIconSource.cs");
        var symbolIcon = Read(root, "ModernWpf", "IconElement", "SymbolIcon.cs");
        var pathIcon = Read(root, "ModernWpf", "IconElement", "PathIcon.cs");

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "7ae8225654f1d98d0da2e0a7535c32b35384ebe6");
        StringAssert.Contains(audit, "e229584245b82f1b4977fc72799ad85725f5a138");
        StringAssert.Contains(audit, "16737fe3a48cd8fc9b337a13e1b04e17afd97882");
        StringAssert.Contains(audit, "197 legacy entries");

        StringAssert.Contains(fontIcon, "Segoe Fluent Icons,Segoe MDL2 Assets");
        StringAssert.Contains(fontIconSource, "Segoe Fluent Icons,Segoe MDL2 Assets");
        StringAssert.Contains(fontIcon, "TextElement.FontStyleProperty.AddOwner");
        StringAssert.Contains(fontIcon, "TextElement.FontWeightProperty.AddOwner");
        Assert.AreEqual(
            2,
            Regex.Matches(fontIcon, "FrameworkPropertyMetadataOptions.Inherits").Count,
            "FontStyle and FontWeight should both inherit exactly once.");
        StringAssert.Contains(fontIcon, "RenderTransform = _mirroringTransform;");
        Assert.IsFalse(fontIcon.Contains("_textBlock.RenderTransform", StringComparison.Ordinal));
        StringAssert.Contains(fontIcon, "_mirroringTransform.ScaleX");

        var mappings = Regex.Matches(symbolIcon, @"0x[0-9A-F]+ => 0x[0-9A-F]+");
        Assert.AreEqual(197, mappings.Count, "The complete current WinUI Symbol remapping table must remain present.");
        foreach (var pair in new[]
        {
            "0xE10B => 0xE8FB",
            "0xE11D => 0xE899",
            "0xE14C => 0xEA37",
            "0xE191 => 0xE620",
            "0xE1D2 => 0xF5F0",
            "0xE700 => 0xE700",
            "0xE72D => 0xE72D"
        })
        {
            StringAssert.Contains(symbolIcon, pair);
        }
        StringAssert.Contains(symbolIcon, "_ => (int)symbol");

        StringAssert.Contains(pathIcon, "HorizontalAlignment = HorizontalAlignment.Stretch");
        StringAssert.Contains(pathIcon, "VerticalAlignment = VerticalAlignment.Stretch");
        Assert.IsFalse(pathIcon.Contains("Stretch = Stretch.Uniform", StringComparison.Ordinal));
        StringAssert.Contains(audit, "transparent private Grid");
        StringAssert.Contains(audit, "AccessibilityView Raw");
    }

    private static string Read(string root, params string[] parts)
    {
        return File.ReadAllText(parts.Aggregate(root, Path.Combine));
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
