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
