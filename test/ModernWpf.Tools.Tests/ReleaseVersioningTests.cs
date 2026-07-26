using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests
{
    [TestClass]
    public class ReleaseVersioningTests
    {
        [TestMethod]
        public void PackageValidationUsesCentralCompatibilityBaseline()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var baseline = props
                .Descendants("ModernWpfCompatibilityBaselineVersion")
                .Select(element => element.Value)
                .SingleOrDefault() ?? string.Empty;
            var packageProject = File.ReadAllText(
                Path.Combine(repoRoot, "ModernWpf.Controls", "ModernWpf.Controls.csproj"));

            Assert.IsFalse(string.IsNullOrWhiteSpace(baseline));
            StringAssert.Contains(
                packageProject,
                "'$(Version)' != '$(ModernWpfCompatibilityBaselineVersion)'");
            StringAssert.Contains(
                packageProject,
                ">$(ModernWpfCompatibilityBaselineVersion)</PackageValidationBaselineVersion>");
            Assert.IsFalse(packageProject.Contains(baseline, StringComparison.Ordinal));
        }

        [TestMethod]
        public void ReleaseWorkflowDerivesNotesAndUsesDisplayBrand()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var version = props
                .Descendants("Version")
                .Select(element => element.Value)
                .Single();
            var releaseNotesPath = Path.Combine(
                repoRoot,
                "docs",
                $"release-notes-{version}.md");
            var workflow = File.ReadAllText(
                Path.Combine(repoRoot, ".github", "workflows", "release.yml"));

            Assert.IsTrue(
                File.Exists(releaseNotesPath),
                $"Release notes are missing for version '{version}'.");
            var releaseNotes = File.ReadAllText(releaseNotesPath);
            StringAssert.StartsWith(releaseNotes, $"# ModernWPF {version}");
            StringAssert.Contains(
                workflow,
                "RELEASE_VERSION: ${{ steps.validate.outputs.version }}");
            StringAssert.Contains(
                workflow,
                "$releaseNotesPath = \"docs\\release-notes-$env:RELEASE_VERSION.md\"");
            StringAssert.Contains(
                workflow,
                "Test-Path -LiteralPath $releaseNotesPath -PathType Leaf");
            StringAssert.Contains(
                workflow,
                "Copy-Item -LiteralPath $releaseNotesPath");
            StringAssert.Contains(
                workflow,
                "--title \"ModernWPF ${{ needs.build.outputs.version }}\"");
            Assert.IsFalse(
                workflow.Contains(
                    "Copy-Item docs\\release-notes-1.0.0-preview.1.md",
                    StringComparison.Ordinal));
            Assert.IsFalse(
                workflow.Contains("--title \"ModernWpf ", StringComparison.Ordinal));
            Assert.IsFalse(
                workflow.Contains("Select-Object -Single", StringComparison.Ordinal));
            Assert.AreEqual(
                2,
                CountOccurrences(
                    workflow,
                    "Expected exactly one ModernWpfUI package, found $($packages.Count)."));
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
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
}
