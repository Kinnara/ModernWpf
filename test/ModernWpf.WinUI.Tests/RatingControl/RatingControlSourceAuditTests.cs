using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.WinUI.Tests.RatingControl;

[TestClass]
public class RatingControlSourceAuditTests
{
    [TestMethod]
    public void AutomationPeerKeepsSourceBehaviorWithoutUnusedCppLocal()
    {
        var repoRoot = FindRepoRoot();
        var automationPeerPath = Path.Combine(
            repoRoot,
            "ModernWpf.Controls",
            "RatingControl",
            "RatingControlAutomationPeer.cs");
        var auditPath = Path.Combine(repoRoot, "docs", "ratingcontrol-winui3-source-audit.md");

        var automationPeer = File.ReadAllText(automationPeerPath);
        var audit = File.ReadAllText(auditPath);

        StringAssert.Contains(automationPeer, "GenerateValue_ValueString");
        StringAssert.Contains(automationPeer, "DetermineFractionDigits");
        StringAssert.Contains(automationPeer, "SR_CommunityRatingString");
        StringAssert.Contains(automationPeer, "SR_BasicRatingString");
        StringAssert.Contains(automationPeer, "SR_RatingUnset");
        StringAssert.Contains(automationPeer, "ValuePatternIdentifiers.ValueProperty");
        StringAssert.Contains(automationPeer, "RangeValuePatternIdentifiers.ValueProperty");
        Assert.IsFalse(
            automationPeer.Contains("string ratingString;", StringComparison.Ordinal),
            "The C# automation-peer port should omit the unused WinUI C++ local so source-backed builds stay warning-free.");

        StringAssert.Contains(audit, "unused `ratingString` local");
    }

    [TestMethod]
    public void CurrentPlaceholderCoercionFixAndSourcePinsAreGuarded()
    {
        var repoRoot = FindRepoRoot();
        var ratingControl = File.ReadAllText(Path.Combine(
            repoRoot,
            "ModernWpf.Controls",
            "RatingControl",
            "RatingControl.cs"));
        var apiTests = File.ReadAllText(Path.Combine(
            repoRoot,
            "test",
            "ModernWpf.WinUI.Tests",
            "RatingControl",
            "RatingControlApiTests.cs"));
        var audit = File.ReadAllText(Path.Combine(
            repoRoot,
            "docs",
            "ratingcontrol-winui3-source-audit.md"));
        var publicDocumentation = File.ReadAllText(Path.Combine(
            repoRoot,
            "ModernWpf.Controls",
            "ModernWpf.Controls.xml"));

        StringAssert.Contains(audit, "de3e767333c2f0717a6a70cb22bd192ced5ad885");
        StringAssert.Contains(audit, "27701085117b84f49936435a38c55a63a1e5d8b7");
        StringAssert.Contains(audit, "cf28ae58357cedc734fc92b30b5e84cb7922d88b");
        StringAssert.Contains(audit, "61143cf16f5c0627153ecb1ad0ca1657f02135a7");

        StringAssert.Contains(ratingControl, "CoercePlaceholderValueBetweenMinAndMax");
        StringAssert.Contains(ratingControl, "Math.Max(1, MaxRating)");
        StringAssert.Contains(ratingControl, "property == PlaceholderValueProperty");
        StringAssert.Contains(ratingControl, "SetValue(property, coercedValue);");
        StringAssert.Contains(apiTests, "VerifyMaxRatingCoercionWhileLoadedDoesNotCrash");
        StringAssert.Contains(apiTests, "Assert.AreEqual(0.5, ratingControl.PlaceholderValue)");
        StringAssert.Contains(apiTests, "Assert.AreEqual(0.0, ratingControl.PlaceholderValue)");
        StringAssert.Contains(publicDocumentation, "Zero and fractional values are valid");
        StringAssert.Contains(publicDocumentation, "The default is -1");
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
