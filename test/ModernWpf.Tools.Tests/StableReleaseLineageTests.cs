using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests
{
    [TestClass]
    public class StableReleaseLineageTests
    {
        private const string StableTag = "v1.0.0";
        private const string RcTag = "v1.0.0-rc.1";
        private static readonly DateTimeOffset PublishedAt =
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        [TestMethod]
        public void AcceptsAllowedDeltaAtExactFourteenDayBoundary()
        {
            using var repository = TestRepository.Create();

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "Validated stable lineage");
        }

        [TestMethod]
        public void RejectsIncompletePublishedReleaseSoak()
        {
            using var repository = TestRepository.Create();

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14).AddSeconds(-1));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "has not completed the 14-day soak");
        }

        [TestMethod]
        [DataRow("ModernWpf.Controls/Control.xaml")]
        [DataRow(".github/workflows/release.yml")]
        [DataRow("ModernWpf/PublicAPI.Shipped.txt")]
        [DataRow("docs/images/Gallery.Light.png")]
        public void RejectsProductWorkflowContractAndImageChanges(string relativePath)
        {
            using var repository = TestRepository.Create(
                mutateStableTree: root => WriteFile(root, relativePath, "changed"));

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "outside the allowed version and release-document delta");
        }

        [TestMethod]
        public void RejectsHiddenDirectoryBuildPropsChange()
        {
            using var repository = TestRepository.Create(stableExtraValue: "changed");

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "differs from 'v1.0.0-rc.1' outside Version");
        }

        [TestMethod]
        public void RejectsRenameIntoAllowedDocumentationPath()
        {
            using var repository = TestRepository.Create(
                mutateStableTree: root =>
                {
                    File.Delete(Path.Combine(root, "ModernWpf.Controls", "readme.md"));
                    RunGit(
                        root,
                        "mv",
                        "ModernWpf.Controls/Control.xaml",
                        "ModernWpf.Controls/readme.md");
                });

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "outside the allowed version and release-document delta");
            StringAssert.Contains(result.Output, "ModernWpf.Controls/Control.xaml");
        }

        [TestMethod]
        public void RejectsDeletion()
        {
            using var repository = TestRepository.Create(
                mutateStableTree: root =>
                    File.Delete(Path.Combine(root, "ModernWpf.Controls", "Control.xaml")));

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "outside the allowed version and release-document delta");
            StringAssert.Contains(result.Output, "ModernWpf.Controls/Control.xaml");
        }

        [TestMethod]
        public void RejectsLightweightRcTag()
        {
            using var repository = TestRepository.Create(annotatedRcTag: false);

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "must be annotated");
        }

        [TestMethod]
        public void RejectsMismatchedRcPackageVersion()
        {
            using var repository = TestRepository.Create(rcPackageVersion: "1.0.0-rc.2");

            var result = RunValidator(
                repository.Root,
                RcTag,
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "contains package version '1.0.0-rc.2'");
        }

        [TestMethod]
        public void RejectsNonAncestorRc()
        {
            using var repository = TestRepository.Create();
            RunGit(repository.Root, "checkout", "--orphan", "unrelated-rc");
            WriteFile(
                repository.Root,
                "Directory.Build.props",
                CreateProps("1.0.0-rc.2", "1.0.0-preview.7", "same"));
            RunGit(repository.Root, "add", "-A");
            RunGit(repository.Root, "commit", "-m", "Unrelated RC");
            RunGit(repository.Root, "tag", "-a", "v1.0.0-rc.2", "-m", "Unrelated RC");

            var result = RunValidator(
                repository.Root,
                "v1.0.0-rc.2",
                PublishedAt.AddDays(14));

            Assert.AreNotEqual(0, result.ExitCode, result.Output);
            StringAssert.Contains(result.Output, "is not an ancestor");
        }

        private static ProcessResult RunValidator(
            string repositoryRoot,
            string acceptedRcTag,
            DateTimeOffset now)
        {
            var scriptPath = Path.Combine(
                FindRepoRoot(),
                "tools",
                "release",
                "Assert-StableReleaseLineage.ps1");
            var startInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            foreach (var argument in new[]
            {
                "-NoProfile",
                "-NonInteractive",
                "-File",
                scriptPath,
                "-StableTag",
                StableTag,
                "-AcceptedRcTag",
                acceptedRcTag,
                "-AcceptedRcPublishedAt",
                PublishedAt.ToString("O", CultureInfo.InvariantCulture),
                "-RepositoryRoot",
                repositoryRoot,
                "-NowUtc",
                now.ToString("O", CultureInfo.InvariantCulture)
            })
            {
                startInfo.ArgumentList.Add(argument);
            }

            using var process = Process.Start(startInfo);
            Assert.IsNotNull(process);
            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new ProcessResult(
                process.ExitCode,
                standardOutput + Environment.NewLine + standardError);
        }

        private static string CreateProps(
            string version,
            string packageBaseline,
            string extraValue)
        {
            return
                "<Project>\n" +
                "  <PropertyGroup>\n" +
                $"    <Version>{version}</Version>\n" +
                "    <ModernWpfPreviewAuditBaselineVersion>1.0.0-preview.1</ModernWpfPreviewAuditBaselineVersion>\n" +
                $"    <ModernWpfPackageValidationBaselineVersion>{packageBaseline}</ModernWpfPackageValidationBaselineVersion>\n" +
                $"    <StableLineageTestValue>{extraValue}</StableLineageTestValue>\n" +
                "  </PropertyGroup>\n" +
                "</Project>\n";
        }

        private static void WriteFile(string root, string relativePath, string contents)
        {
            var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, contents);
        }

        private static string RunGit(string workingDirectory, params string[] arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };
            foreach (var argument in arguments)
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
                $"git {string.Join(' ', arguments)} failed.{Environment.NewLine}" +
                standardOutput + Environment.NewLine + standardError);
            return standardOutput.Trim();
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

            throw new DirectoryNotFoundException("Could not find the ModernWpf repository root.");
        }

        private readonly record struct ProcessResult(int ExitCode, string Output);

        private sealed class TestRepository : IDisposable
        {
            private TestRepository(string root)
            {
                Root = root;
            }

            public string Root { get; }

            public static TestRepository Create(
                bool annotatedRcTag = true,
                string rcPackageVersion = "1.0.0-rc.1",
                string stableExtraValue = "same",
                Action<string>? mutateStableTree = null)
            {
                var root = Path.Combine(
                    Path.GetTempPath(),
                    $"ModernWpf-StableLineage-{Guid.NewGuid():N}");
                Directory.CreateDirectory(root);
                var repository = new TestRepository(root);

                RunGit(root, "init");
                RunGit(root, "config", "user.name", "ModernWpf Test");
                RunGit(root, "config", "user.email", "modernwpf-test@example.invalid");

                WriteFile(
                    root,
                    "Directory.Build.props",
                    CreateProps(rcPackageVersion, "1.0.0-preview.7", "same"));
                WriteFile(root, "README.md", "RC readme\n");
                WriteFile(root, "ModernWpf.Controls/readme.md", "RC package readme\n");
                WriteFile(root, "ModernWpf.Controls/Control.xaml", "<Control />\n");
                WriteFile(root, "samples/PackageConsumer/README.md", "RC consumer readme\n");
                RunGit(root, "add", ".");
                RunGit(root, "commit", "-m", "RC");
                if (annotatedRcTag)
                {
                    RunGit(root, "tag", "-a", RcTag, "-m", "RC");
                }
                else
                {
                    RunGit(root, "tag", RcTag);
                }

                WriteFile(
                    root,
                    "Directory.Build.props",
                    CreateProps("1.0.0", "1.0.0-rc.1", stableExtraValue));
                WriteFile(root, "README.md", "Stable readme\n");
                WriteFile(root, "ModernWpf.Controls/readme.md", "Stable package readme\n");
                WriteFile(root, "samples/PackageConsumer/README.md", "Stable consumer readme\n");
                WriteFile(root, "docs/release-notes-1.0.0.md", "# Stable release\n");
                mutateStableTree?.Invoke(root);
                RunGit(root, "add", "-A");
                RunGit(root, "commit", "-m", "Stable");
                RunGit(root, "tag", "-a", StableTag, "-m", "Stable");

                return repository;
            }

            public void Dispose()
            {
                if (Directory.Exists(Root))
                {
                    foreach (var file in Directory.EnumerateFiles(
                        Root,
                        "*",
                        SearchOption.AllDirectories))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                    }
                    foreach (var directory in Directory.EnumerateDirectories(
                        Root,
                        "*",
                        SearchOption.AllDirectories))
                    {
                        File.SetAttributes(directory, FileAttributes.Directory);
                    }

                    Directory.Delete(Root, recursive: true);
                }
            }
        }
    }
}
