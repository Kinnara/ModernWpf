using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests
{
    [TestClass]
    public class ReleaseRoadmapTests
    {
        [TestMethod]
        public void OneZeroRoadmapKeepsTheReviewedMilestoneSequence()
        {
            var repoRoot = FindRepoRoot();
            var roadmap = File.ReadAllText(
                Path.Combine(repoRoot, "docs", "roadmap-1.0.md"));
            var milestones = new[]
            {
                "`1.0.0-preview.2`",
                "`1.0.0-preview.3`",
                "`1.0.0-preview.4`",
                "`1.0.0-preview.5`",
                "`1.0.0-preview.6`",
                "`1.0.0-preview.7`",
                "`1.0.0-rc.1`",
                "`1.0.0`"
            };

            var previousIndex = -1;
            foreach (var milestone in milestones)
            {
                var index = roadmap.IndexOf(milestone, StringComparison.Ordinal);
                Assert.IsTrue(index > previousIndex, $"Missing or reordered {milestone}.");
                previousIndex = index;
            }

            StringAssert.Contains(roadmap, "`PipsPager` is deferred to the 1.1 line");
            StringAssert.Contains(roadmap, "unchanged 14-day soak");
            StringAssert.Contains(roadmap, "No download threshold");
        }

        [TestMethod]
        public void ReleaseReadinessRequiresTheReviewedStableEvidence()
        {
            var repoRoot = FindRepoRoot();
            var readiness = File.ReadAllText(
                Path.Combine(repoRoot, "docs", "release-readiness-1x.md"));
            var readme = File.ReadAllText(Path.Combine(repoRoot, "README.md"));

            StringAssert.Contains(readiness, "## Finite upstream milestone cutoff");
            StringAssert.Contains(readiness, "## Downstream compatibility canaries");
            StringAssert.Contains(readiness, "BililiveRecorder on .NET Framework 4.7.2");
            StringAssert.Contains(readiness, "OpenKh on .NET 8");
            StringAssert.Contains(readiness, "BilibiliLiveRecordDownLoader on .NET 10");
            StringAssert.Contains(readiness, "## Visual and manual Gallery gate");
            StringAssert.Contains(readiness, "Every preview, release candidate, and stable release");
            StringAssert.Contains(readiness, "a real OS High");
            StringAssert.Contains(readiness, "Contrast theme");
            StringAssert.Contains(readiness, "## RC and stable graduation");
            StringAssert.Contains(readiness, "Any CLR API or public resource-key change");
            StringAssert.Contains(readiness, "Download counts are informational");
            StringAssert.Contains(readiness, "After publication:");
            StringAssert.Contains(readiness, "mark every listed 0.9.x version as **Legacy**");
            StringAssert.Contains(readiness, "`1.0.0-preview.3`");
            StringAssert.Contains(readiness, "`1.0.0-preview.2` the active package-validation");
            StringAssert.Contains(readme, "[1.0 roadmap](docs/roadmap-1.0.md)");
        }

        [TestMethod]
        public void BuildWorkflowEnforcesThreeConsecutiveFullWinUIRuns()
        {
            var repoRoot = FindRepoRoot();
            var workflow = File.ReadAllText(
                    Path.Combine(repoRoot, ".github", "workflows", "build.yml"))
                .Replace("\r\n", "\n", StringComparison.Ordinal);

            StringAssert.Contains(
                workflow,
                "name: Test complete WinUI suite three consecutive times");
            StringAssert.Contains(workflow, "foreach ($run in 1..3)");
            StringAssert.Contains(workflow, "winui-net8-run$run.trx");
            StringAssert.Contains(workflow, "if ($LASTEXITCODE -ne 0)");
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

            Assert.Fail("Could not locate the ModernWpf repository root.");
            return string.Empty;
        }
    }
}
