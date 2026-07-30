using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests
{
    [TestClass]
    public class ReleaseVersioningTests
    {
        [TestMethod]
        public void ActivePackageBaselineShipsEveryPublicContractEntry()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var version = GetPropertyValue(props, "Version");
            var baseline = GetPropertyValue(
                props,
                "ModernWpfPackageValidationBaselineVersion");

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
        public void PackageValidationSeparatesHistoricalAuditFromActiveBaseline()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var auditBaseline = GetPropertyValue(
                props,
                "ModernWpfPreviewAuditBaselineVersion");
            var packageBaseline = GetPropertyValue(
                props,
                "ModernWpfPackageValidationBaselineVersion");
            var packageProject = File.ReadAllText(
                Path.Combine(repoRoot, "ModernWpf.Controls", "ModernWpf.Controls.csproj"));

            Assert.AreEqual("1.0.0-preview.1", auditBaseline);
            Assert.IsFalse(string.IsNullOrWhiteSpace(packageBaseline));
            StringAssert.Contains(
                packageProject,
                "'$(Version)' != '$(ModernWpfPackageValidationBaselineVersion)'");
            StringAssert.Contains(
                packageProject,
                ">$(ModernWpfPackageValidationBaselineVersion)</PackageValidationBaselineVersion>");
            Assert.IsFalse(packageProject.Contains(
                "ModernWpfPreviewAuditBaselineVersion",
                StringComparison.Ordinal));
            Assert.IsFalse(packageProject.Contains(packageBaseline, StringComparison.Ordinal));
        }

        [TestMethod]
        public void PreviewRebaselineRequiresBreakingChangeMigrationNotes()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var version = GetPropertyValue(props, "Version");
            var auditBaseline = GetPropertyValue(
                props,
                "ModernWpfPreviewAuditBaselineVersion");
            var packageBaseline = GetPropertyValue(
                props,
                "ModernWpfPackageValidationBaselineVersion");

            if (!version.Contains("-", StringComparison.Ordinal) ||
                string.Equals(packageBaseline, auditBaseline, StringComparison.Ordinal) ||
                !string.Equals(packageBaseline, version, StringComparison.Ordinal))
            {
                return;
            }

            var releaseNotesPath = Path.Combine(
                repoRoot,
                "docs",
                $"release-notes-{version}.md");
            Assert.IsTrue(
                File.Exists(releaseNotesPath),
                $"Release notes are missing for preview rebaseline '{version}'.");

            var releaseNotes = File.ReadAllText(releaseNotesPath);
            Assert.IsTrue(
                HasSubstantivePreviewRebaselineNotes(
                    releaseNotes,
                    auditBaseline,
                    packageBaseline),
                "An intentional preview rebaseline must identify the old and new " +
                "compatibility baselines and include a breaking-change bullet with " +
                "explicit consumer migration guidance.");
        }

        [TestMethod]
        public void PreviewRebaselineMigrationValidatorRejectsNoChangePlaceholder()
        {
            const string oldBaseline = "1.0.0-preview.1";
            const string newBaseline = "1.0.0-preview.2";
            const string placeholder = """
                ## Breaking changes

                This release requires no public CLR or resource-key change, so
                there is no consumer migration.
                """;
            const string substantive = """
                ## Breaking changes

                Preview compatibility baseline: `1.0.0-preview.1` → `1.0.0-preview.2`

                - `OldMember` was replaced by `NewMember`.
                  **Migration:** Replace calls to `OldMember` with `NewMember`.
                """;

            Assert.IsFalse(HasSubstantivePreviewRebaselineNotes(
                placeholder,
                oldBaseline,
                newBaseline));
            Assert.IsTrue(HasSubstantivePreviewRebaselineNotes(
                substantive,
                oldBaseline,
                newBaseline));
        }

        [TestMethod]
        public void StableOneXKeepsTheOneZeroSemVerBaseline()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var version = GetPropertyValue(props, "Version");

            if (version.Contains("-", StringComparison.Ordinal) ||
                !version.StartsWith("1.", StringComparison.Ordinal))
            {
                return;
            }

            Assert.AreEqual(
                "1.0.0",
                GetPropertyValue(props, "ModernWpfPackageValidationBaselineVersion"),
                "Stable 1.x must not advance its package baseline to hide a breaking change.");
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
                "<!-- RELEASE-NOTES: DRAFT -->");
            StringAssert.Contains(
                workflow,
                "are still marked as a draft");
            StringAssert.Contains(
                workflow,
                @".\tools\release\Prepare-GitHubReleaseNotes.ps1");
            StringAssert.Contains(
                workflow,
                "-Repository $env:GITHUB_REPOSITORY");
            StringAssert.Contains(
                workflow,
                "-Tag $env:RELEASE_TAG");
            StringAssert.Contains(
                workflow,
                "--title \"ModernWPF ${{ needs.build.outputs.version }}\"");
            StringAssert.Contains(
                workflow,
                "GH_REPO: ${{ github.repository }}");
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

        [TestMethod]
        public void GitHubReleaseNotesUseTagPinnedDocumentationLinks()
        {
            var repoRoot = FindRepoRoot();
            var props = XDocument.Load(Path.Combine(repoRoot, "Directory.Build.props"));
            var version = GetPropertyValue(props, "Version");
            var releaseNotesPath = Path.Combine(
                repoRoot,
                "docs",
                $"release-notes-{version}.md");
            var scriptPath = Path.Combine(
                repoRoot,
                "tools",
                "release",
                "Prepare-GitHubReleaseNotes.ps1");
            var outputPath = Path.Combine(
                Path.GetTempPath(),
                $"modernwpf-release-notes-{Guid.NewGuid():N}.md");

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = repoRoot
                };
                foreach (var argument in new[]
                {
                    "-NoProfile",
                    "-NonInteractive",
                    "-File",
                    scriptPath,
                    "-SourcePath",
                    releaseNotesPath,
                    "-DestinationPath",
                    outputPath,
                    "-Repository",
                    "Kinnara/ModernWpf",
                    "-Tag",
                    $"v{version}",
                    "-RepositoryRoot",
                    repoRoot
                })
                {
                    startInfo.ArgumentList.Add(argument);
                }

                using var process = Process.Start(startInfo);
                Assert.IsNotNull(process);
                var standardOutput = process.StandardOutput.ReadToEnd();
                var standardError = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Assert.AreEqual(
                    0,
                    process.ExitCode,
                    $"Release-note preparation failed.{Environment.NewLine}" +
                    $"{standardOutput}{Environment.NewLine}{standardError}");

                var preparedReleaseNotes = File.ReadAllText(outputPath);
                var tagUrlPrefix =
                    $"https://github.com/Kinnara/ModernWpf/blob/v{version}/docs/";
                StringAssert.Contains(
                    preparedReleaseNotes,
                    $"{tagUrlPrefix}migrating-from-0.9.md");
                StringAssert.Contains(
                    preparedReleaseNotes,
                    $"{tagUrlPrefix}public-api-contract-1x.md");
                Assert.IsFalse(
                    preparedReleaseNotes.Contains(
                        "](migrating-from-0.9.md)",
                        StringComparison.Ordinal));
                Assert.IsFalse(
                    preparedReleaseNotes.Contains(
                        "](public-api-contract-1x.md)",
                        StringComparison.Ordinal));
            }
            finally
            {
                File.Delete(outputPath);
            }
        }

        [TestMethod]
        public void ReleaseWorkflowUsesNuGetTrustedPublishing()
        {
            var repoRoot = FindRepoRoot();
            var workflow = File.ReadAllText(
                Path.Combine(repoRoot, ".github", "workflows", "release.yml"));

            StringAssert.Contains(workflow, "id-token: write");
            StringAssert.Contains(workflow, "uses: NuGet/login@v1");
            StringAssert.Contains(workflow, "user: kinnara");
            StringAssert.Contains(
                workflow,
                "NUGET_API_KEY: ${{ steps.nuget-login.outputs.NUGET_API_KEY }}");
            Assert.IsFalse(
                workflow.Contains(
                    "secrets.NUGET_API_KEY",
                    StringComparison.Ordinal));
        }

        private static string GetPropertyValue(XDocument props, string name)
        {
            return props
                .Descendants(name)
                .Select(element => element.Value)
                .Single();
        }

        private static bool HasSubstantivePreviewRebaselineNotes(
            string releaseNotes,
            string oldBaseline,
            string newBaseline)
        {
            var section = Regex.Match(
                releaseNotes,
                @"(?ms)^## Breaking changes\s*(?<body>.*?)(?=^## |\z)");
            if (!section.Success)
            {
                return false;
            }

            var body = section.Groups["body"].Value;
            var baselineMarker =
                $"Preview compatibility baseline: `{oldBaseline}` → `{newBaseline}`";
            return body.Contains(baselineMarker, StringComparison.Ordinal) &&
                Regex.IsMatch(body, @"(?m)^\s*[-*]\s+\S") &&
                body.Contains("**Migration:**", StringComparison.Ordinal) &&
                !Regex.IsMatch(
                    body,
                    @"(?i)\bno\s+public\s+(?:CLR|API|resource)|\bno\s+consumer\s+migration\b");
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
