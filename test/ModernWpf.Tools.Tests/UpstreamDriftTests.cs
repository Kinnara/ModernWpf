using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests
{
    [TestClass]
    public class UpstreamDriftTests
    {
        private const string ProductReviewedRevision =
            "d5bdbb190cdba0b7f1baec4b3981208a9685a360";
        private const string ProductEpochRevision =
            "6a556bb28fc227acd2ec8fe67ee64853f559084b";
        private const string StableEpochRevision =
            "a97562621a1d1ea397a38a3f512c9eef99db52d8";
        private const string GalleryReviewedRevision =
            "3669519356c67f1376152c33ed8ea45003a91f3a";
        private const string GalleryEpochRevision =
            "3669519356c67f1376152c33ed8ea45003a91f3a";

        [TestMethod]
        public void ManifestSchemaCoversAuditedFamiliesAndGenericResources()
        {
            var repoRoot = FindRepoRoot();
            var upstreamDirectory = Path.Combine(repoRoot, "tools", "upstream");
            var schemaPath = Path.Combine(upstreamDirectory, "upstream-sync.schema.json");
            var manifestPath = Path.Combine(upstreamDirectory, "upstream-sync.json");

            using var schema = JsonDocument.Parse(File.ReadAllText(schemaPath));
            Assert.AreEqual(
                "https://json-schema.org/draft/2020-12/schema",
                schema.RootElement.GetProperty("$schema").GetString());
            Assert.AreEqual(
                "object",
                schema.RootElement.GetProperty("type").GetString());
            Assert.IsTrue(schema.RootElement.GetProperty("$defs").TryGetProperty(
                "family",
                out _));
            Assert.IsTrue(schema.RootElement.GetProperty("$defs").TryGetProperty(
                "ignorePath",
                out _));
            Assert.IsTrue(schema.RootElement.GetProperty("$defs").TryGetProperty(
                "epochAdoption",
                out _));
            Assert.AreEqual(
                "adopted",
                schema.RootElement
                    .GetProperty("$defs")
                    .GetProperty("epochAdoption")
                    .GetProperty("properties")
                    .GetProperty("status")
                    .GetProperty("const")
                    .GetString());
            var epochAdoptionRequired = schema.RootElement
                .GetProperty("$defs")
                .GetProperty("epochAdoption")
                .GetProperty("required")
                .EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty)
                .ToArray();
            CollectionAssert.Contains(epochAdoptionRequired, "milestone");
            CollectionAssert.Contains(epochAdoptionRequired, "cutoffDate");

            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var root = manifest.RootElement;
            Assert.AreEqual("./upstream-sync.schema.json", root.GetProperty("$schema").GetString());
            Assert.AreEqual(1, root.GetProperty("schemaVersion").GetInt32());

            var repositories = root.GetProperty("repositories")
                .EnumerateArray()
                .ToArray();
            var repositoryIds = repositories
                .Select(repository => RequiredString(repository, "id"))
                .ToArray();
            CollectionAssert.AreEquivalent(
                new[] { "winui-product", "winui-gallery" },
                repositoryIds);
            Assert.AreEqual(
                repositoryIds.Length,
                repositoryIds.Distinct(StringComparer.Ordinal).Count());

            foreach (var repository in repositories)
            {
                var ignorePaths = repository.GetProperty("ignorePaths")
                    .EnumerateArray()
                    .ToArray();
                Assert.IsTrue(ignorePaths.Length > 0);
                foreach (var ignorePath in ignorePaths)
                {
                    Assert.IsFalse(RequiredString(ignorePath, "path").Contains('\\'));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(
                        RequiredString(ignorePath, "justification")));
                }

                var tracks = repository.GetProperty("tracks")
                    .EnumerateArray()
                    .ToArray();
                var trackIds = tracks
                    .Select(track => RequiredString(track, "id"))
                    .ToArray();
                Assert.AreEqual(
                    trackIds.Length,
                    trackIds.Distinct(StringComparer.Ordinal).Count());
                foreach (var track in tracks)
                {
                    var epochAdoption = track.GetProperty("epochAdoption");
                    Assert.AreEqual(
                        "adopted",
                        RequiredString(epochAdoption, "status"));
                    Assert.AreEqual(
                        "1.0.0-preview.3",
                        RequiredString(epochAdoption, "milestone"));
                    Assert.AreEqual(
                        "2026-08-08",
                        RequiredString(epochAdoption, "cutoffDate"));
                    var dispositionDocument = RequiredString(
                        epochAdoption,
                        "dispositionDocument");
                    Assert.IsFalse(dispositionDocument.Contains('\\'));
                    Assert.IsTrue(File.Exists(Path.Combine(
                        repoRoot,
                        dispositionDocument.Replace(
                            '/',
                            Path.DirectorySeparatorChar))));
                }
            }

            var product = GetRepository(root, "winui-product");
            var productIgnorePaths = product.GetProperty("ignorePaths")
                .EnumerateArray()
                .Select(ignorePath => RequiredString(ignorePath, "path"))
                .ToArray();
            CollectionAssert.Contains(productIgnorePaths, "src/XamlCompiler/**");
            CollectionAssert.Contains(productIgnorePaths, "XamlCompilerPublic.csproj");
            CollectionAssert.Contains(productIgnorePaths, "build/**");
            CollectionAssert.Contains(productIgnorePaths, "scripts/**");
            CollectionAssert.Contains(productIgnorePaths, "eng/**");
            CollectionAssert.Contains(productIgnorePaths, "packaging/**");
            CollectionAssert.Contains(productIgnorePaths, "BuildTransportPackage.ps1");
            CollectionAssert.Contains(
                productIgnorePaths,
                "BuildTransportPackageCmdWrapper.cmd");
            CollectionAssert.Contains(productIgnorePaths, "buildsamples.cmd");
            var stable = GetTrack(product, "stable");
            AssertRevision(stable, "reviewedBaseline", StableEpochRevision);
            AssertRevision(stable, "epochTarget", StableEpochRevision);
            Assert.AreEqual(
                StableEpochRevision,
                RequiredString(stable.GetProperty("latestStableAtEpoch"), "revision"));
            Assert.AreEqual(
                "winui3/release/2.3.1",
                RequiredString(stable.GetProperty("latestStableAtEpoch"), "tag"));
            Assert.AreEqual(
                "latestStableRelease",
                RequiredString(stable.GetProperty("observedHead"), "kind"));

            var productMain = GetTrack(product, "main");
            AssertRevision(productMain, "reviewedBaseline", ProductReviewedRevision);
            AssertRevision(productMain, "epochTarget", ProductEpochRevision);
            Assert.AreEqual(
                "ref",
                RequiredString(productMain.GetProperty("observedHead"), "kind"));
            Assert.AreEqual(
                "winui3/main",
                RequiredString(productMain.GetProperty("observedHead"), "ref"));

            var gallery = GetRepository(root, "winui-gallery");
            var galleryMain = GetTrack(gallery, "main");
            AssertRevision(galleryMain, "reviewedBaseline", GalleryReviewedRevision);
            AssertRevision(galleryMain, "epochTarget", GalleryEpochRevision);
            Assert.AreEqual(
                "main",
                RequiredString(galleryMain.GetProperty("observedHead"), "ref"));

            var families = root.GetProperty("families")
                .EnumerateArray()
                .ToArray();
            var familyIds = families
                .Select(family => RequiredString(family, "id"))
                .ToArray();
            Assert.AreEqual(
                familyIds.Length,
                familyIds.Distinct(StringComparer.Ordinal).Count());

            var manifestAuditDocuments = new List<string>();
            foreach (var family in families)
            {
                var auditDocuments = family.GetProperty("auditDocuments")
                    .EnumerateArray()
                    .Select(value => value.GetString() ?? string.Empty)
                    .ToArray();
                Assert.IsTrue(auditDocuments.Length > 0);
                foreach (var auditDocument in auditDocuments)
                {
                    Assert.IsFalse(auditDocument.Contains('\\'));
                    Assert.IsTrue(
                        File.Exists(Path.Combine(
                            repoRoot,
                            auditDocument.Replace(
                                '/',
                                Path.DirectorySeparatorChar))),
                        $"Missing audit document '{auditDocument}'.");
                    manifestAuditDocuments.Add(auditDocument);
                }

                var watches = family.GetProperty("watches")
                    .EnumerateArray()
                    .ToArray();
                Assert.IsTrue(watches.Length > 0);
                foreach (var watch in watches)
                {
                    CollectionAssert.Contains(
                        repositoryIds,
                        RequiredString(watch, "repository"));
                    var paths = watch.GetProperty("paths")
                        .EnumerateArray()
                        .Select(value => value.GetString() ?? string.Empty)
                        .ToArray();
                    Assert.IsTrue(paths.Length > 0);
                    Assert.IsFalse(paths.Any(path =>
                        string.IsNullOrWhiteSpace(path) ||
                        path.Contains('\\')));
                }
            }
            CollectionAssert.Contains(
                manifestAuditDocuments,
                "docs/custom-popup-source-findings.md",
                "Popup platform changes must route to the governing WindowedPopup source record.");
            CollectionAssert.Contains(
                manifestAuditDocuments,
                "docs/titlebar-winui3-gallery-parity.md",
                "TitleBar API metadata must route to the WPF-facade parity record.");

            var sourceAudits = Directory
                .GetFiles(Path.Combine(repoRoot, "docs"), "*winui3-source-audit.md")
                .Select(path => Path.GetRelativePath(repoRoot, path).Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Assert.IsTrue(sourceAudits.Length >= 30);
            foreach (var sourceAudit in sourceAudits)
            {
                Assert.AreEqual(
                    1,
                    manifestAuditDocuments.Count(path =>
                        string.Equals(path, sourceAudit, StringComparison.Ordinal)),
                    $"Current WinUI source audit '{sourceAudit}' must map to exactly one family.");
            }

            var expectedResources = XDocument
                .Load(Path.Combine(
                    repoRoot,
                    "ModernWpf.Controls",
                    "Themes",
                    "Generic.xaml"))
                .Descendants()
                .Where(element => element.Name.LocalName == "ResourceDictionary")
                .Select(element => element.Attribute("Source")?.Value)
                .OfType<string>()
                .Where(source => source.StartsWith(
                    "/ModernWpf.Controls;component/",
                    StringComparison.OrdinalIgnoreCase))
                .Select(source => source.Substring(
                    "/ModernWpf.Controls;component/".Length))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var coverageRows = ParseControlCoverageRows(Path.Combine(
                repoRoot,
                "docs",
                "winui3-control-source-coverage.md"));
            var coveredResources = coverageRows
                .Select(row => row.Resource)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            CollectionAssert.AreEqual(expectedResources, coveredResources);
            Assert.AreEqual(
                coverageRows.Count,
                coverageRows
                    .Select(row => row.Resource)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                "Generic resource coverage rows must be unique.");
            foreach (var row in coverageRows)
            {
                CollectionAssert.Contains(
                    manifestAuditDocuments,
                    row.AuditDocument,
                    $"Generic resource '{row.Resource}' is not mapped by the upstream manifest.");
            }
        }

        [TestMethod]
        public void ReporterValidatesManifestAgainstSchemaAndUsesSemanticStableOrder()
        {
            var repoRoot = FindRepoRoot();
            var upstreamDirectory = Path.Combine(repoRoot, "tools", "upstream");
            var script = File.ReadAllText(Path.Combine(
                upstreamDirectory,
                "Get-UpstreamDriftReport.ps1"));

            StringAssert.Contains(script, "Test-Json");
            StringAssert.Contains(script, "-SchemaFile $resolvedSchemaPath");
            StringAssert.Contains(script, "Get-StableTagVersion");
            StringAssert.Contains(script, "Select-LatestStableTag");
            StringAssert.Contains(script, "ConvertTo-StableTags");
            StringAssert.Contains(script, "/tags?per_page=100&page=$pageNumber");
            StringAssert.Contains(script, "while ($pageTags.Count -eq 100)");
            Assert.IsFalse(script.Contains(
                "$tags = @(" + Environment.NewLine + "        Invoke-GitHubApi",
                StringComparison.Ordinal),
                "Invoke-RestMethod JSON arrays must be assigned before @() enumeration.");
            StringAssert.Contains(
                script,
                "$encodedBase...${encodedHead}?per_page=100",
                "The compare URI must delimit encodedHead because '?' is valid in a PowerShell variable name.");
            StringAssert.Contains(script, "Sort-Object -Property version -Descending");
            StringAssert.Contains(script, "$script:GitHubCompareFileLimit = 300");
            StringAssert.Contains(script, "$files.Count -ge $script:GitHubCompareFileLimit");
            Assert.IsFalse(script.Contains(
                "Sort-Object -Property published_at",
                StringComparison.Ordinal));

            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"modernwpf-upstream-schema-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);
            try
            {
                File.Copy(
                    Path.Combine(upstreamDirectory, "upstream-sync.schema.json"),
                    Path.Combine(temporaryDirectory, "upstream-sync.schema.json"));
                var invalidManifest = File
                    .ReadAllText(Path.Combine(upstreamDirectory, "upstream-sync.json"))
                    .Replace(
                        "\"schemaVersion\": 1,",
                        "\"schemaVersion\": 1,\n  \"unexpectedProperty\": true,",
                        StringComparison.Ordinal);
                var invalidManifestPath = Path.Combine(
                    temporaryDirectory,
                    "upstream-sync.json");
                File.WriteAllText(invalidManifestPath, invalidManifest);

                var result = RunReport(
                    repoRoot,
                    Path.Combine(
                        repoRoot,
                        "test",
                        "ModernWpf.Tools.Tests",
                        "Fixtures",
                        "Upstream",
                        "upstream-clean.fixture.json"),
                    "-ManifestPath",
                    invalidManifestPath);

                Assert.AreNotEqual(0, result.ExitCode);
                Assert.IsTrue(string.IsNullOrWhiteSpace(result.Json));
                StringAssert.Contains(result.StandardError, "unexpectedProperty");
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }

        [TestMethod]
        public void OfflineFixtureSeparatesEpochObservedIgnoredAndUnmappedChanges()
        {
            var repoRoot = FindRepoRoot();
            var fixturePath = Path.Combine(
                repoRoot,
                "test",
                "ModernWpf.Tools.Tests",
                "Fixtures",
                "Upstream",
                "upstream-drift.fixture.json");
            var result = RunReport(
                repoRoot,
                fixturePath,
                "-IncludeEpochComparison");

            Assert.AreEqual(
                0,
                result.ExitCode,
                result.StandardOutput + Environment.NewLine + result.StandardError);
            using var report = JsonDocument.Parse(result.Json);
            var root = report.RootElement;
            Assert.IsTrue(root.GetProperty("hasEpochDrift").GetBoolean());
            Assert.IsTrue(root.GetProperty("hasObservedDrift").GetBoolean());
            Assert.IsTrue(root.GetProperty("hasEpochUnmappedDrift").GetBoolean());
            Assert.IsTrue(root.GetProperty("hasObservedUnmappedDrift").GetBoolean());
            Assert.IsFalse(root.GetProperty("hasIncompleteComparison").GetBoolean());
            Assert.AreEqual(
                DateTimeOffset.Parse("2026-08-08T12:34:56Z"),
                DateTimeOffset.Parse(RequiredString(root, "generatedAt")));

            var stable = GetReportTrack(root, "winui-product", "stable");
            Assert.AreEqual(
                "winui3/release/2.3.2",
                RequiredString(stable.GetProperty("observedHead"), "label"));
            Assert.AreEqual(
                1,
                stable
                    .GetProperty("observed")
                    .GetProperty("comparison")
                    .GetProperty("watchedChangedFileCount")
                    .GetInt32());
            var stableFile = stable
                .GetProperty("observed")
                .GetProperty("comparison")
                .GetProperty("families")[0]
                .GetProperty("files")[0];
            Assert.AreEqual(
                "src/controls/dev/NumberBox/NumberBox.cpp",
                RequiredString(stableFile, "filename"));

            var productMain = GetReportTrack(root, "winui-product", "main");
            Assert.AreEqual(
                "adopted",
                RequiredString(productMain.GetProperty("epochAdoption"), "status"));
            Assert.AreEqual(
                "docs/winui3-sync-2026-08-08-preview3.md",
                RequiredString(
                    productMain.GetProperty("epochAdoption"),
                    "dispositionDocument"));
            var epoch = productMain
                .GetProperty("epoch")
                .GetProperty("comparison");
            Assert.IsTrue(epoch.GetProperty("isEvaluated").GetBoolean());
            Assert.AreEqual(4, epoch.GetProperty("watchedChangedFileCount").GetInt32());
            Assert.AreEqual(1, epoch.GetProperty("ignoredChangedFileCount").GetInt32());
            Assert.AreEqual(2, epoch.GetProperty("unmappedChangedFileCount").GetInt32());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "command-bar",
                    "popup-gallery",
                    "theme-shadow",
                    "xaml-core-api-surface"
                },
                epoch
                    .GetProperty("families")
                    .EnumerateArray()
                    .Select(family => RequiredString(family, "id"))
                    .ToArray());
            Assert.AreEqual(
                ".github/**",
                RequiredString(epoch.GetProperty("ignoredFiles")[0], "ignorePattern"));
            CollectionAssert.AreEquivalent(
                new[]
                {
                    ".github/workflows/moved.yml",
                    "controls/dev/NewControl/NewControl.idl"
                },
                epoch
                    .GetProperty("unmappedFiles")
                    .EnumerateArray()
                    .Select(file => RequiredString(file, "filename"))
                    .ToArray());

            var observed = productMain
                .GetProperty("observed")
                .GetProperty("comparison");
            Assert.AreEqual(2, observed.GetProperty("actionableChangedFileCount").GetInt32());
            Assert.AreEqual(
                "number-box",
                RequiredString(observed.GetProperty("families")[0], "id"));
            Assert.AreEqual(
                "controls/dev/FutureControl/FutureControl.idl",
                RequiredString(observed.GetProperty("unmappedFiles")[0], "filename"));

            var galleryMain = GetReportTrack(root, "winui-gallery", "main");
            var galleryEpoch = galleryMain
                .GetProperty("epoch")
                .GetProperty("comparison");
            Assert.IsTrue(galleryEpoch.GetProperty("isEvaluated").GetBoolean());
            Assert.AreEqual("identical", RequiredString(galleryEpoch, "status"));
            Assert.AreEqual(
                0,
                galleryEpoch.GetProperty("actionableChangedFileCount").GetInt32());
            Assert.AreEqual(0, galleryEpoch.GetProperty("families").GetArrayLength());

            StringAssert.Contains(
                result.Markdown,
                "Reviewed baseline → finite epoch target");
            StringAssert.Contains(
                result.Markdown,
                "Finite epoch target → moving observed head");
            StringAssert.Contains(result.Markdown, "Explicitly ignored files");
            StringAssert.Contains(result.Markdown, "Unmapped files (action required)");
            StringAssert.Contains(
                result.Markdown,
                "dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.cs");
            StringAssert.Contains(
                result.Markdown,
                "dxaml/xcp/dxaml/lib/FrameworkElement_Partial.cpp");
            StringAssert.Contains(
                result.Markdown,
                "Popup platform primitive and Gallery sample");
            StringAssert.Contains(result.Markdown, "ThemeShadow");
            StringAssert.Contains(
                result.Markdown,
                "controls/dev/NewControl/NewControl.idl");
            StringAssert.Contains(result.Markdown, ".github/workflows/moved.yml");
            StringAssert.Contains(
                result.Markdown,
                "src/controls/dev/NumberBox/NumberBox.cpp");
            StringAssert.Contains(
                result.Markdown,
                "does not port, merge, or advance any baseline");
            StringAssert.Contains(
                result.Markdown,
                "Epoch adoption: `adopted` for `1.0.0-preview.3` at cutoff date `2026-08-08`; disposition `docs/winui3-sync-2026-08-08-preview3.md`.");
        }

        [TestMethod]
        public void ScheduledModeSkipsHistoricalComparisonAndFailsOnNewDrift()
        {
            var repoRoot = FindRepoRoot();
            var fixtureDirectory = Path.Combine(
                repoRoot,
                "test",
                "ModernWpf.Tools.Tests",
                "Fixtures",
                "Upstream");
            var clean = RunReport(
                repoRoot,
                Path.Combine(fixtureDirectory, "upstream-clean.fixture.json"),
                "-FailOnObservedDrift",
                "-FailOnIncompleteComparison");
            Assert.AreEqual(
                0,
                clean.ExitCode,
                clean.StandardOutput + Environment.NewLine + clean.StandardError);
            using (var cleanReport = JsonDocument.Parse(clean.Json))
            {
                Assert.IsFalse(cleanReport.RootElement
                    .GetProperty("hasObservedDrift")
                    .GetBoolean());
                foreach (var track in cleanReport.RootElement
                    .GetProperty("tracks")
                    .EnumerateArray())
                {
                    Assert.IsFalse(track
                        .GetProperty("epoch")
                        .GetProperty("comparison")
                        .GetProperty("isEvaluated")
                        .GetBoolean());
                }
                var cleanStable = GetReportTrack(
                    cleanReport.RootElement,
                    "winui-product",
                    "stable");
                Assert.AreEqual(
                    "winui3/release/2.3.1",
                    RequiredString(cleanStable.GetProperty("observedHead"), "label"),
                    "Stable selection must use highest SemVer even when an older train was published later.");
            }
            StringAssert.Contains(
                clean.Markdown,
                "The historical epoch comparison was not queried.");

            var drift = RunReport(
                repoRoot,
                Path.Combine(fixtureDirectory, "upstream-drift.fixture.json"),
                "-FailOnObservedDrift");
            Assert.AreNotEqual(0, drift.ExitCode);
            Assert.AreEqual(2, drift.ExitCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(drift.Json));
            using var driftReport = JsonDocument.Parse(drift.Json);
            Assert.IsTrue(driftReport.RootElement
                .GetProperty("hasObservedDrift")
                .GetBoolean());
        }

        [TestMethod]
        public void IncompleteComparisonEmitsReportAndExitsThree()
        {
            var repoRoot = FindRepoRoot();
            var result = RunReport(
                repoRoot,
                Path.Combine(
                    repoRoot,
                    "test",
                    "ModernWpf.Tools.Tests",
                    "Fixtures",
                    "Upstream",
                    "upstream-incomplete.fixture.json"),
                "-FailOnIncompleteComparison");

            Assert.AreEqual(3, result.ExitCode);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Json));
            using var report = JsonDocument.Parse(result.Json);
            Assert.IsTrue(report.RootElement
                .GetProperty("hasIncompleteComparison")
                .GetBoolean());
            StringAssert.Contains(
                result.StandardError,
                "At least one upstream comparison was incomplete.");
        }

        [TestMethod]
        public void ComparisonAtGitHubFileLimitFailsClosedEvenWhenMarkedComplete()
        {
            var repoRoot = FindRepoRoot();
            var sourceFixturePath = Path.Combine(
                repoRoot,
                "test",
                "ModernWpf.Tools.Tests",
                "Fixtures",
                "Upstream",
                "upstream-incomplete.fixture.json");
            var fixture = JsonNode.Parse(File.ReadAllText(sourceFixturePath))!
                .AsObject();
            var comparison = fixture["comparisons"]!
                .AsArray()[0]!
                .AsObject();
            comparison["isComplete"] = true;
            var files = new JsonArray();
            for (var index = 0; index < 300; index++)
            {
                files.Add(new JsonObject
                {
                    ["filename"] = $"unmapped/file-{index:D3}.txt",
                    ["status"] = "modified"
                });
            }
            comparison["files"] = files;

            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"modernwpf-upstream-cap-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);
            var fixturePath = Path.Combine(temporaryDirectory, "fixture.json");
            File.WriteAllText(fixturePath, fixture.ToJsonString());

            try
            {
                var result = RunReport(
                    repoRoot,
                    fixturePath,
                    "-FailOnIncompleteComparison");

                Assert.AreEqual(3, result.ExitCode);
                using var report = JsonDocument.Parse(result.Json);
                var observed = GetReportTrack(
                        report.RootElement,
                        "winui-product",
                        "main")
                    .GetProperty("observed")
                    .GetProperty("comparison");
                Assert.IsFalse(observed.GetProperty("isComplete").GetBoolean());
                Assert.IsTrue(observed.GetProperty("fileLimitReached").GetBoolean());
                Assert.AreEqual(300, observed.GetProperty("fileLimit").GetInt32());
                Assert.AreEqual(300, observed.GetProperty("returnedFileCount").GetInt32());
                Assert.AreEqual(300, observed.GetProperty("unmappedChangedFileCount").GetInt32());
                StringAssert.Contains(result.Markdown, "**Partial classification:**");
                StringAssert.Contains(
                    result.Markdown,
                    "This is not a complete changed-path inventory.");
                StringAssert.Contains(
                    result.Markdown,
                    "GitHub returned its 300-file comparison limit");
                StringAssert.Contains(
                    result.StandardError,
                    "At least one upstream comparison was incomplete.");
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
        }

        [TestMethod]
        public void MonitoringWorkflowIsScheduledManualReadOnlyAndNonPorting()
        {
            var repoRoot = FindRepoRoot();
            var workflow = File.ReadAllText(Path.Combine(
                    repoRoot,
                    ".github",
                    "workflows",
                    "upstream-drift.yml"))
                .Replace("\r\n", "\n", StringComparison.Ordinal);

            StringAssert.Contains(workflow, "on:\n  schedule:");
            StringAssert.Contains(workflow, "  workflow_dispatch:");
            StringAssert.Contains(workflow, "permissions:\n  contents: read");
            StringAssert.Contains(
                workflow,
                @".\tools\upstream\Get-UpstreamDriftReport.ps1");
            StringAssert.Contains(workflow, "-FailOnObservedDrift");
            StringAssert.Contains(workflow, "-FailOnIncompleteComparison");
            StringAssert.Contains(workflow, "uses: actions/upload-artifact@v7");
            Assert.IsFalse(workflow.Contains(
                "-IncludeEpochComparison",
                StringComparison.Ordinal));
            Assert.IsFalse(Regex.IsMatch(
                workflow,
                @"(?m)^  (push|pull_request):"));
            Assert.IsFalse(workflow.Contains("issues: write", StringComparison.Ordinal));
            Assert.IsFalse(workflow.Contains("pull-requests: write", StringComparison.Ordinal));
            Assert.IsFalse(workflow.Contains("git commit", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(workflow.Contains("git merge", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(workflow.Contains("gh pr", StringComparison.OrdinalIgnoreCase));
        }

        private static void AssertRevision(
            JsonElement track,
            string propertyName,
            string expectedRevision)
        {
            Assert.AreEqual(
                expectedRevision,
                RequiredString(track.GetProperty(propertyName), "revision"));
        }

        private static JsonElement GetRepository(JsonElement root, string id)
        {
            return root.GetProperty("repositories")
                .EnumerateArray()
                .Single(repository => RequiredString(repository, "id") == id);
        }

        private static JsonElement GetTrack(JsonElement repository, string id)
        {
            return repository.GetProperty("tracks")
                .EnumerateArray()
                .Single(track => RequiredString(track, "id") == id);
        }

        private static JsonElement GetReportTrack(
            JsonElement root,
            string repository,
            string track)
        {
            return root.GetProperty("tracks")
                .EnumerateArray()
                .Single(item =>
                    RequiredString(item, "repository") == repository &&
                    RequiredString(item, "track") == track);
        }

        private static string RequiredString(JsonElement element, string propertyName)
        {
            var value = element.GetProperty(propertyName).GetString();
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(value),
                $"Property '{propertyName}' must be a non-empty string.");
            return value!;
        }

        private static IReadOnlyList<CoverageRow> ParseControlCoverageRows(
            string path)
        {
            var rows = new List<CoverageRow>();
            foreach (var line in File.ReadLines(path))
            {
                if (!line.StartsWith("| `", StringComparison.Ordinal) ||
                    !line.Contains("docs\\", StringComparison.Ordinal))
                {
                    continue;
                }

                var values = Regex.Matches(line, @"`([^`]+)`")
                    .Cast<Match>()
                    .Select(match => match.Groups[1].Value)
                    .ToArray();
                Assert.IsTrue(
                    values.Length >= 2,
                    $"Malformed WinUI control coverage row: {line}");
                rows.Add(new CoverageRow(
                    values[0],
                    values[^1].Replace('\\', '/')));
            }

            return rows;
        }

        private static ReportRun RunReport(
            string repoRoot,
            string fixturePath,
            params string[] additionalArguments)
        {
            var temporaryDirectory = Path.Combine(
                Path.GetTempPath(),
                $"modernwpf-upstream-{Guid.NewGuid():N}");
            Directory.CreateDirectory(temporaryDirectory);
            var markdownPath = Path.Combine(temporaryDirectory, "report.md");
            var jsonPath = Path.Combine(temporaryDirectory, "report.json");

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
                    Path.Combine(
                        repoRoot,
                        "tools",
                        "upstream",
                        "Get-UpstreamDriftReport.ps1"),
                    "-FixturePath",
                    fixturePath,
                    "-OutputPath",
                    markdownPath,
                    "-JsonOutputPath",
                    jsonPath
                }.Concat(additionalArguments))
                {
                    startInfo.ArgumentList.Add(argument);
                }

                using var process = Process.Start(startInfo);
                Assert.IsNotNull(process);
                var standardOutput = process.StandardOutput.ReadToEnd();
                var standardError = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return new ReportRun(
                    process.ExitCode,
                    standardOutput,
                    standardError,
                    File.Exists(markdownPath)
                        ? File.ReadAllText(markdownPath)
                        : string.Empty,
                    File.Exists(jsonPath)
                        ? File.ReadAllText(jsonPath)
                        : string.Empty);
            }
            finally
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
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

        private sealed record CoverageRow(string Resource, string AuditDocument);

        private sealed record ReportRun(
            int ExitCode,
            string StandardOutput,
            string StandardError,
            string Markdown,
            string Json);
    }
}
