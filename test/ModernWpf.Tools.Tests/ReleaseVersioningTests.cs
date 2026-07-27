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
        public void PreviewBaselineShipsEveryPublicContractEntry()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var version = GetPropertyValue(props, "Version");
            var baseline = GetPropertyValue(
                props,
                "ModernWpfCompatibilityBaselineVersion");

            if (!string.Equals(version, baseline, StringComparison.Ordinal))
            {
                return;
            }

            foreach (var projectDirectory in new[] { "ModernWpf", "ModernWpf.Controls" })
            {
                foreach (var fileName in new[]
                {
                    "PublicAPI.Unshipped.txt",
                    "PublicResourceKeys.Unshipped.txt"
                })
                {
                    var path = Path.Combine(repoRoot, projectDirectory, fileName);
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    var entries = File.ReadLines(path)
                        .Where(line =>
                            !string.IsNullOrWhiteSpace(line) &&
                            !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                        .ToArray();

                    Assert.AreEqual(
                        0,
                        entries.Length,
                        $"{projectDirectory}/{fileName} contains release-baseline entries: " +
                        string.Join(", ", entries));
                }
            }
        }

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
        public void PackageDependencyVersionsAreCentralizedAndVerified()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var systemValueTupleVersion = GetPropertyValue(
                props,
                "ModernWpfSystemValueTupleVersion");
            var windowsSdkContractsVersion = GetPropertyValue(
                props,
                "ModernWpfWindowsSdkContractsVersion");
            var modernWpfProject = File.ReadAllText(
                Path.Combine(repoRoot, "ModernWpf", "ModernWpf.csproj"));
            var controlsProject = File.ReadAllText(
                Path.Combine(repoRoot, "ModernWpf.Controls", "ModernWpf.Controls.csproj"));
            var nuspec = File.ReadAllText(
                Path.Combine(repoRoot, "ModernWpf.Controls", "ModernWpfUI.nuspec"));
            var verifier = File.ReadAllText(
                Path.Combine(repoRoot, "tools", "release", "Verify-ModernWpfPackage.ps1"));
            var smoke = File.ReadAllText(
                Path.Combine(repoRoot, "tools", "release", "Test-ModernWpfPackageSmoke.ps1"));

            Assert.IsFalse(string.IsNullOrWhiteSpace(systemValueTupleVersion));
            Assert.IsFalse(string.IsNullOrWhiteSpace(windowsSdkContractsVersion));
            StringAssert.Contains(
                modernWpfProject,
                "Version=\"$(ModernWpfSystemValueTupleVersion)\"");
            StringAssert.Contains(
                modernWpfProject,
                "Version=\"$(ModernWpfWindowsSdkContractsVersion)\"");
            StringAssert.Contains(
                controlsProject,
                "Version=\"$(ModernWpfSystemValueTupleVersion)\"");
            StringAssert.Contains(
                controlsProject,
                "<PackageReadmeFile>readme.md</PackageReadmeFile>");
            StringAssert.Contains(
                controlsProject,
                "<None Update=\"readme.md\" Pack=\"true\" PackagePath=\"\" />");
            StringAssert.Contains(
                controlsProject,
                "systemValueTupleVersion=$(ModernWpfSystemValueTupleVersion)");
            StringAssert.Contains(
                controlsProject,
                "windowsSdkContractsVersion=$(ModernWpfWindowsSdkContractsVersion)");
            StringAssert.Contains(nuspec, "version=\"$systemValueTupleVersion$\"");
            StringAssert.Contains(nuspec, "version=\"$windowsSdkContractsVersion$\"");
            Assert.IsFalse(nuspec.Contains(
                $"version=\"{systemValueTupleVersion}\"",
                StringComparison.Ordinal));
            Assert.IsFalse(nuspec.Contains(
                $"version=\"{windowsSdkContractsVersion}\"",
                StringComparison.Ordinal));
            StringAssert.Contains(verifier, "ModernWpfSystemValueTupleVersion");
            StringAssert.Contains(verifier, "ModernWpfWindowsSdkContractsVersion");
            StringAssert.Contains(smoke, "--warnaserror:MSB3277");
        }

        [TestMethod]
        public void WorkflowsDeclareLeastPrivilegePermissions()
        {
            var repoRoot = FindRepoRoot();
            var buildWorkflow = File.ReadAllText(
                    Path.Combine(repoRoot, ".github", "workflows", "build.yml"))
                .Replace("\r\n", "\n", StringComparison.Ordinal);
            var labelWorkflow = File.ReadAllText(
                    Path.Combine(repoRoot, ".github", "workflows", "label.yml"))
                .Replace("\r\n", "\n", StringComparison.Ordinal);

            StringAssert.Contains(buildWorkflow, "permissions:\n  contents: read");
            StringAssert.Contains(labelWorkflow, "permissions:\n  issues: write");
            StringAssert.Contains(labelWorkflow, "GH_TOKEN: ${{ github.token }}");
            StringAssert.Contains(labelWorkflow, "gh issue edit");
            Assert.IsFalse(
                labelWorkflow.Contains("andymckay/labeler", StringComparison.Ordinal));
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

        private static string GetPropertyValue(XDocument props, string name)
        {
            return props
                .Descendants(name)
                .Select(element => element.Value)
                .Single();
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
