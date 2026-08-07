using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests
{
    [TestClass]
    public class DownstreamCanaryTests
    {
        [TestMethod]
        public void ManifestPinsReviewedConsumersAndMinimalMigrations()
        {
            var repoRoot = FindRepoRoot();
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                repoRoot,
                "tools",
                "downstream-canaries",
                "downstream-canaries.json")));
            var root = document.RootElement;

            Assert.AreEqual(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.AreEqual("ModernWpfUI", RequiredString(root, "packageId"));
            Assert.AreEqual(
                "docs/migrating-from-0.9.md",
                RequiredString(root, "migrationGuidePath"));

            var repositories = root.GetProperty("repositories")
                .EnumerateArray()
                .ToArray();
            Assert.AreEqual(3, repositories.Length);
            Assert.AreEqual(
                repositories.Length,
                repositories.Select(item => RequiredString(item, "id")).Distinct().Count());

            AssertCanary(
                repositories,
                "bilibili-live-record-downloader",
                "HMBSbige/BilibiliLiveRecordDownLoader",
                "77113a04d631715abc48368da450ed4c4205ae32",
                "BilibiliLiveRecordDownLoader/BilibiliLiveRecordDownLoader.csproj",
                "net10.0-windows10.0.26100.0",
                "dotnet",
                1,
                Array.Empty<string>(),
                "0.9.6",
                "GPL-3.0",
                4);
            AssertCanary(
                repositories,
                "bililive-recorder",
                "BililiveRecorder/BililiveRecorder",
                "d263506c9ae97370e88f27620014cddb6e8c3e58",
                "BililiveRecorder.WPF/BililiveRecorder.WPF.csproj",
                "net472",
                "msbuild",
                0,
                Array.Empty<string>(),
                "0.9.4",
                "GPL-3.0",
                4);
            AssertCanary(
                repositories,
                "openkh-kh2-object-editor",
                "OpenKH/OpenKh",
                "5153c6752e0855444aca88572068f73ad349de29",
                "OpenKh.Tools.Kh2ObjectEditor/OpenKh.Tools.Kh2ObjectEditor.csproj",
                "net8.0-windows",
                "dotnet",
                1,
                new[]
                {
                    "ModelingToolkit",
                    "Simple3DViewport",
                    "XeEngine.Tools.Public",
                    "nQuant"
                },
                "0.9.6",
                "Apache-2.0",
                0);

            AssertTextMigration(
                repositories,
                "bilibili-live-record-downloader",
                "BilibiliLiveRecordDownLoader/Views/Dialogs/RoomDialog.xaml",
                4);
            AssertTextMigration(
                repositories,
                "bilibili-live-record-downloader",
                "BilibiliLiveRecordDownLoader/Views/FFmpegCommandView.xaml",
                8);
            AssertTextMigration(
                repositories,
                "bilibili-live-record-downloader",
                "BilibiliLiveRecordDownLoader/Views/SettingView.xaml",
                6);
            AssertTextMigration(
                repositories,
                "bilibili-live-record-downloader",
                "BilibiliLiveRecordDownLoader/Views/UserSettingsView.xaml",
                6);
            AssertTextMigration(
                repositories,
                "bililive-recorder",
                "BililiveRecorder.WPF/Controls/PerRoomSettingsDialog.xaml",
                2);
            AssertTextMigration(
                repositories,
                "bililive-recorder",
                "BililiveRecorder.WPF/Pages/AdvancedSettingsPage.xaml",
                8);
            AssertTextMigration(
                repositories,
                "bililive-recorder",
                "BililiveRecorder.WPF/Pages/SettingsPage.xaml",
                2);
            AssertTitleBarMigration(
                repositories,
                "bililive-recorder",
                "BililiveRecorder.WPF/NewMainWindow.xaml",
                3);
        }

        [TestMethod]
        public void ManifestValidatesAgainstCheckedInSchema()
        {
            var repoRoot = FindRepoRoot();
            var result = RunPowerShell(
                repoRoot,
                Path.Combine(
                    repoRoot,
                    "tools",
                    "downstream-canaries",
                    "Invoke-DownstreamCanary.ps1"),
                "-ValidateManifestOnly");

            Assert.AreEqual(
                0,
                result.ExitCode,
                result.StandardOutput + Environment.NewLine + result.StandardError);
            StringAssert.Contains(result.StandardOutput, "Validated 3 downstream canaries.");
        }

        [TestMethod]
        public void PackageMigrationHandlesAttributeAndElementVersionsWithoutReformatting()
        {
            var repoRoot = FindRepoRoot();
            var script = Path.Combine(
                repoRoot,
                "tools",
                "downstream-canaries",
                "Set-DownstreamCanaryPackageVersion.ps1");
            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"modernwpf-canary-migration-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);

            try
            {
                var cases = new[]
                {
                    "<Project>\r\n  <ItemGroup>\r\n    <PackageReference Include=\"ModernWpfUI\" Version=\"0.9.6\" />\r\n  </ItemGroup>\r\n</Project>\r\n",
                    "<Project>\r\n  <ItemGroup>\r\n    <PackageReference Include=\"ModernWpfUI\">\r\n      <Version>0.9.6</Version>\r\n    </PackageReference>\r\n  </ItemGroup>\r\n</Project>\r\n"
                };

                for (var index = 0; index < cases.Length; index++)
                {
                    var path = Path.Combine(temporaryDirectory, $"case-{index}.csproj");
                    var includeBom = index == 1;
                    File.WriteAllText(path, cases[index], new UTF8Encoding(includeBom));
                    var before = File.ReadAllText(path);
                    var result = RunPowerShell(
                        repoRoot,
                        script,
                        "-ProjectPath",
                        path,
                        "-PackageId",
                        "ModernWpfUI",
                        "-FromVersion",
                        "0.9.6",
                        "-ToVersion",
                        "1.0.0-preview.2");

                    Assert.AreEqual(
                        0,
                        result.ExitCode,
                        result.StandardOutput + Environment.NewLine + result.StandardError);
                    var after = File.ReadAllText(path);
                    Assert.AreEqual(
                        before.Replace("0.9.6", "1.0.0-preview.2", StringComparison.Ordinal),
                        after);
                    var bytes = File.ReadAllBytes(path);
                    Assert.AreEqual(includeBom, bytes.Length >= 3 &&
                        bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
                }
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }

        [TestMethod]
        public void TextMigrationRequiresExactReviewedOccurrenceCount()
        {
            var repoRoot = FindRepoRoot();
            var script = Path.Combine(
                repoRoot,
                "tools",
                "downstream-canaries",
                "Set-DownstreamCanaryTextReplacement.ps1");
            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"modernwpf-canary-text-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);
            var path = Path.Combine(temporaryDirectory, "Page.xaml");
            const string source =
                "<ui:SimpleStackPanel>\r\n</ui:SimpleStackPanel>\r\n";
            File.WriteAllText(path, source, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            try
            {
                var rejected = RunPowerShell(
                    repoRoot,
                    script,
                    "-Path",
                    path,
                    "-From",
                    "SimpleStackPanel",
                    "-To",
                    "StackPanelEx",
                    "-ExpectedOccurrences",
                    "1");
                Assert.AreNotEqual(0, rejected.ExitCode);
                StringAssert.Contains(rejected.StandardError, "found 2");
                Assert.AreEqual(source, File.ReadAllText(path));

                var migrated = RunPowerShell(
                    repoRoot,
                    script,
                    "-Path",
                    path,
                    "-From",
                    "SimpleStackPanel",
                    "-To",
                    "StackPanelEx",
                    "-ExpectedOccurrences",
                    "2");
                Assert.AreEqual(
                    0,
                    migrated.ExitCode,
                    migrated.StandardOutput + Environment.NewLine + migrated.StandardError);
                Assert.AreEqual(
                    source.Replace("SimpleStackPanel", "StackPanelEx", StringComparison.Ordinal),
                    File.ReadAllText(path));
                var bytes = File.ReadAllBytes(path);
                Assert.IsTrue(bytes.Length >= 3 &&
                    bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }

        [TestMethod]
        public void WorkflowIsManualReadOnlyIsolatedAndUsesLocalCandidateFeed()
        {
            var repoRoot = FindRepoRoot();
            var workflow = File.ReadAllText(Path.Combine(
                    repoRoot,
                    ".github",
                    "workflows",
                    "downstream-canaries.yml"))
                .Replace("\r\n", "\n", StringComparison.Ordinal);
            var runner = File.ReadAllText(Path.Combine(
                repoRoot,
                "tools",
                "downstream-canaries",
                "Invoke-DownstreamCanary.ps1"));

            StringAssert.Contains(workflow, "on:\n  workflow_dispatch:");
            StringAssert.Contains(workflow, "permissions:\n  contents: read");
            StringAssert.Contains(workflow, "runs-on: windows-2022");
            StringAssert.Contains(workflow, "fail-fast: false");
            StringAssert.Contains(workflow, "DOWNSTREAM_MSBUILD_SDK_VERSION: 8.0.423");
            StringAssert.Contains(workflow, "Setup isolated .NET SDK for full MSBuild");
            StringAssert.Contains(
                workflow,
                "DOTNET_INSTALL_DIR: ${{ runner.temp }}\\dotnet-msbuild");
            StringAssert.Contains(workflow, "global-json-file: global.json");
            StringAssert.Contains(
                workflow,
                "dotnet-version: ${{ env.DOWNSTREAM_MSBUILD_SDK_VERSION }}");
            StringAssert.Contains(
                workflow,
                "-MSBuildSdkVersion $env:DOWNSTREAM_MSBUILD_SDK_VERSION");
            StringAssert.Contains(
                workflow,
                "-MSBuildDotNetRoot (Join-Path $env:RUNNER_TEMP 'dotnet-msbuild')");
            StringAssert.Contains(
                workflow,
                "-WorkPath (Join-Path $env:RUNNER_TEMP 'mw')");
            var canaryJobStart = workflow.IndexOf("\n  canary:\n", StringComparison.Ordinal);
            Assert.IsTrue(canaryJobStart > 0);
            var packageJob = workflow[..canaryJobStart];
            var canaryJob = workflow[canaryJobStart..];
            Assert.IsFalse(
                packageJob.Contains(
                    "Setup isolated .NET SDK for full MSBuild",
                    StringComparison.Ordinal));
            Assert.IsTrue(
                canaryJob.IndexOf(
                    "Setup isolated .NET SDK for full MSBuild",
                    StringComparison.Ordinal) <
                canaryJob.IndexOf("\n      - name: Setup .NET\n", StringComparison.Ordinal));
            StringAssert.Contains(workflow, "persist-credentials: false");
            StringAssert.Contains(workflow, "continue-on-error: true");
            StringAssert.Contains(workflow, "if: always()");
            StringAssert.Contains(workflow, "uses: actions/upload-artifact@v7");
            StringAssert.Contains(workflow, "uses: actions/download-artifact@v8");
            StringAssert.Contains(
                workflow,
                @".\tools\downstream-canaries\Invoke-DownstreamCanary.ps1");
            StringAssert.Contains(workflow, "-CandidatePackagePath $packages[0].FullName");
            StringAssert.Contains(workflow, "timeout-minutes: 75");
            Assert.IsFalse(workflow.Contains("schedule:", StringComparison.Ordinal));
            Assert.IsFalse(workflow.Contains("pull_request:", StringComparison.Ordinal));
            Assert.IsFalse(workflow.Contains("push:", StringComparison.Ordinal));
            Assert.IsFalse(workflow.Contains("id-token:", StringComparison.Ordinal));
            Assert.IsFalse(workflow.Contains("secrets.", StringComparison.Ordinal));

            StringAssert.Contains(runner, "<package pattern=\"$PackageId\" />");
            StringAssert.Contains(runner, "modernwpf-candidate");
            StringAssert.Contains(runner, ".nupkg.metadata");
            StringAssert.Contains(runner, "Verified local candidate source");
            StringAssert.Contains(runner, "NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED");
            StringAssert.Contains(runner, "DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR");
            StringAssert.Contains(runner, "DOTNET_MSBUILD_SDK_RESOLVER_SDKS_DIR");
            StringAssert.Contains(runner, "DOTNET_MSBUILD_SDK_RESOLVER_SDKS_VER");
            StringAssert.Contains(runner, "DOTNET_MULTILEVEL_LOOKUP = '0'");
            StringAssert.Contains(runner, "$startInfo.Environment.Clear()");
            StringAssert.Contains(runner, "New-CleanProcessEnvironment");
            StringAssert.Contains(runner, "GIT_CONFIG_KEY_0 = 'credential.helper'");
            StringAssert.Contains(runner, "repositoryCommit");
            StringAssert.Contains(runner, "guideRevision");
            StringAssert.Contains(runner, "/blob/$($package.RepositoryCommit)/");
            StringAssert.Contains(runner, "could not resolve host");
            Assert.IsFalse(runner.Contains("|could not resolve|", StringComparison.Ordinal));
            Assert.IsFalse(runner.Contains("'GITHUB_ENV'", StringComparison.Ordinal));
            Assert.IsFalse(runner.Contains("'ACTIONS_RUNTIME_TOKEN'", StringComparison.Ordinal));
            StringAssert.Contains(runner, "credential.helper=");
            StringAssert.Contains(runner, "--no-hardlinks");
            StringAssert.Contains(runner, "'modernwpf-canary'");
            StringAssert.Contains(runner, "'-b'");
            StringAssert.Contains(runner, "$canary.fetchDepth -eq 0");
            StringAssert.Contains(runner, "$cloneFetchArguments += '--tags'");
            StringAssert.Contains(runner, "$cloneFetchArguments += '--no-tags'");
            StringAssert.Contains(runner, ".Substring(0, 8)");
            StringAssert.Contains(runner, "\"c-$runId\"");
            Assert.IsFalse(runner.Contains("'--branch'", StringComparison.Ordinal));
            Assert.IsFalse(runner.Contains("'--detach'", StringComparison.Ordinal));
            StringAssert.Contains(runner, "baseline-submodules");
            StringAssert.Contains(runner, "candidate-submodules");
            StringAssert.Contains(runner, "--jobs=1");
            StringAssert.Contains(runner, "submoduleWorktree.Name)-status");
            StringAssert.Contains(runner, "'status'");
            StringAssert.Contains(runner, "migration.patch");
            StringAssert.Contains(runner, "Set-DownstreamCanaryTextReplacement.ps1");
            Assert.IsFalse(runner.Contains("dotnet run", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(runner.Contains("Start-Process", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(runner.Contains("gh pr", StringComparison.OrdinalIgnoreCase));
        }

        private static void AssertCanary(
            JsonElement[] repositories,
            string id,
            string repository,
            string commit,
            string project,
            string targetFramework,
            string buildTool,
            int fetchDepth,
            string[] submodules,
            string baselineVersion,
            string license,
            int expectedTextMigrations)
        {
            var canary = repositories.Single(item => RequiredString(item, "id") == id);
            Assert.AreEqual(repository, RequiredString(canary, "repository"));
            Assert.AreEqual(commit, RequiredString(canary, "commit"));
            Assert.AreEqual(40, commit.Length);
            Assert.AreEqual(project, RequiredString(canary, "project"));
            Assert.AreEqual(targetFramework, RequiredString(canary, "targetFramework"));
            Assert.AreEqual(buildTool, RequiredString(canary, "buildTool"));
            Assert.AreEqual("Debug", RequiredString(canary, "configuration"));
            Assert.AreEqual(fetchDepth, canary.GetProperty("fetchDepth").GetInt32());
            CollectionAssert.AreEqual(
                submodules,
                canary.GetProperty("submodules")
                    .EnumerateArray()
                    .Select(item => item.GetString()!)
                    .ToArray());
            Assert.AreEqual(baselineVersion, RequiredString(canary, "baselinePackageVersion"));
            Assert.AreEqual("XamlControlsResources", RequiredString(canary, "resourceEntry"));
            Assert.AreEqual(license, RequiredString(canary, "license"));

            var migrations = canary.GetProperty("migrations").EnumerateArray().ToArray();
            var packageMigration = migrations.Single(item =>
                RequiredString(item, "kind") == "package-version");
            Assert.AreEqual(project, RequiredString(packageMigration, "path"));
            Assert.AreEqual("ModernWpfUI", RequiredString(packageMigration, "packageId"));
            Assert.AreEqual(baselineVersion, RequiredString(packageMigration, "fromVersion"));
            Assert.AreEqual(
                expectedTextMigrations,
                migrations.Count(item => RequiredString(item, "kind") == "text-replacement"));
        }

        private static void AssertTextMigration(
            JsonElement[] repositories,
            string id,
            string path,
            int expectedOccurrences)
        {
            var canary = repositories.Single(item => RequiredString(item, "id") == id);
            var migration = canary.GetProperty("migrations")
                .EnumerateArray()
                .Single(item =>
                    RequiredString(item, "kind") == "text-replacement" &&
                    RequiredString(item, "path") == path);
            Assert.AreEqual("SimpleStackPanel", RequiredString(migration, "from"));
            Assert.AreEqual("StackPanelEx", RequiredString(migration, "to"));
            Assert.AreEqual(
                expectedOccurrences,
                migration.GetProperty("expectedOccurrences").GetInt32());
        }

        private static void AssertTitleBarMigration(
            JsonElement[] repositories,
            string id,
            string path,
            int expectedOccurrences)
        {
            var canary = repositories.Single(item => RequiredString(item, "id") == id);
            var migration = canary.GetProperty("migrations")
                .EnumerateArray()
                .Single(item =>
                    RequiredString(item, "kind") == "text-replacement" &&
                    RequiredString(item, "path") == path);
            Assert.AreEqual("TitleBar.", RequiredString(migration, "from"));
            Assert.AreEqual("WindowTitleBar.", RequiredString(migration, "to"));
            Assert.AreEqual(
                expectedOccurrences,
                migration.GetProperty("expectedOccurrences").GetInt32());
        }

        private static PowerShellRun RunPowerShell(
            string workingDirectory,
            string script,
            params string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };
            foreach (var argument in new[]
            {
                "-NoProfile",
                "-NonInteractive",
                "-File",
                script
            }.Concat(arguments))
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);
            Assert.IsNotNull(process);
            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new PowerShellRun(process.ExitCode, standardOutput, standardError);
        }

        private static string RequiredString(JsonElement element, string propertyName)
        {
            var value = element.GetProperty(propertyName).GetString();
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(value),
                $"Property '{propertyName}' must be a non-empty string.");
            return value!;
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

        private sealed record PowerShellRun(
            int ExitCode,
            string StandardOutput,
            string StandardError);
    }
}
