using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class TrackerPriorityTests
    {
        [TestMethod]
        public void TrackerHardOrderKeepsVisualAndHarnessRowsAheadOfSourceShapeCleanup()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");

            AssertContainsInOrder(
                tracker,
                "## Non-Negotiable Execution Gate",
                "A lower row is selectable only when every row above it is recorded for the",
                "Source-shape, resource-key, naming, selector, test cleanup,",
                "and tracker cleanup are never selectable merely because they are small.",
                "Hard order:",
                "1. User-reported priority/order conflict",
                "2. Real OS High Contrast visual and harness evidence.",
                "3. Visible drift and visual-harness stability.",
                "4. Retained ModernWpf/WinUI high-drift visual triage.",
                "5. P2 row 2 visual and high-drift freshness.",
                "6. P2 row 3 asset, thumbnail, and visual-reference parity.",
                "7. P2 row 4 measurement, interaction, automation, and harness-impacting parity.",
                "8. P2 row 5.1 sample panes and runtime-visible example content.",
                "9. P2 row 5.2 source-backed visible/runtime structure.",
                "10. P2 row 5.3 resource-key, naming, selector, and source-hook parity tied to",
                "11. P2 row 5.4 non-visible copied/adapted source-shape guards.",
                "12. P2 row 5.5 row-5 bookkeeping and stale-status cleanup.",
                "13. P2 row 6 and final closeout cleanup.");
        }

        [TestMethod]
        public void TrackerUsesCurrentOrderLockAsSingleScheduler()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");

            AssertContainsInOrder(
                tracker,
                "## Current Order Lock",
                "This section is the single scheduler for current work.",
                "If any lower section conflicts with this block, this block wins.",
                "Front-door priority selector:",
                "Run this selector before any edit. Pick the first true item below;",
                "1. User-requested priority/order conflict or scheduler ambiguity:",
                "2. Real OS High Contrast visual/harness evidence:",
                "3. Visible drift and visual-harness stability:",
                "4. Retained ModernWpf/WinUI high-drift visual triage:",
                "5. P2 row 2 visual/high-drift freshness.",
                "6. P2 row 3 asset, thumbnail, and visual-reference parity.",
                "7. P2 row 4 measurement, interaction, automation, and harness-impacting parity.",
                "11. P2 row 5.4 non-visible copied/adapted source-shape guards, only for a",
                "12. P2 row 5.5 row-5 bookkeeping and stale-status cleanup, only after 5.1-5.4",
                "13. P2 row 6/final closeout cleanup, last, with a fresh verification sweep.",
                "Current distance to completion:",
                "The next lower bucket is **global order 13 / P2 row 6**",
                "Current active selection snapshot:",
                "Current completion-audit/status pass, 2026-06-01:");
        }

        [TestMethod]
        public void TrackerImmediateStatusAnswersDistanceBeforeHistoricalQueues()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");

            AssertContainsInOrder(
                tracker,
                "## Immediate Status and Next Selection",
                "Milestone 1 is not complete.",
                "for the current branch tip, not closed permanently.",
                "Distance to completion, using executable buckets:",
                "If a new real OS High Contrast, visible drift, high-drift retained-control,",
                "row 5 source cleanup",
                "Row 5 is recorded for the current branch tip through the row 5.5 inventory.",
                "Row 6/final verification has run for the current branch tip, but completion",
                "Do not mark the goal complete while `Goal tracker status in Codex` remains",
                "Fail closed: if any lower tracker section, stale `Current` note, historical",
                "the lower note and run the higher-ranked evidence first.",
                "Current pointer:",
                "Latest completion-audit/status pass, 2026-06-01:",
                "Previous committed",
                "branch tip before this row 6 pass was `f9d76b15`",
                "Current Order Lock and Immediate",
                "Status both point to row 6");

            Assert.IsFalse(
                tracker.Contains("The branch tip is", StringComparison.Ordinal),
                "Immediate Status should not describe a historical commit as the current branch tip.");
        }

        [TestMethod]
        public void TrackerKeepsCompletionAuditActiveAfterRowFiveCloseout()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");

            AssertContainsInOrder(
                tracker,
                "Current allowed lower row remains **global order 13 / P2 row 6**",
                "completion-audit/status consistency",
                "Row 5.4 reopens only if",
                "Any new High Contrast, visible-drift, high-drift retained-control, asset,",
                "immediately preempts row 6 and reopens the higher global order.",
                "Goal tracker status in Codex: active, not complete.",
                "Completion audit/status consistency pass:",
                "The completion audit did **not** prove the goal complete.");
        }

        [TestMethod]
        public void TrackerKeepsCompletionAuditActiveUntilWorkingChecklistStatusesClose()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");
            var workingChecklistRows = ReadWorkingChecklistRows(tracker);
            var nonDoneRows = workingChecklistRows
                .Where(row => row.StructuralTests != "Done" || row.ExactSourceAudit != "Done" || row.VisualChecked != "Done")
                .ToList();

            Assert.AreEqual(
                14,
                nonDoneRows.Count,
                string.Join(", ", nonDoneRows.Select(row => row.Name + " " + row.Status)));

            foreach (var closedRow in new[] { "What's New", "User Dashboard", "DataGrid", "Expander", "ResizeGrip", "GridSplitter", "GroupBox", "Color", "Iconography" })
            {
                Assert.AreEqual("Done/Done/Done", workingChecklistRows.Single(row => row.Name == closedRow).Status, closedRow);
                Assert.IsFalse(nonDoneRows.Any(row => row.Name == closedRow), closedRow + " should not remain a completion-audit blocker.");
            }

            Assert.AreEqual("Partial/Partial/Done", workingChecklistRows.Single(row => row.Name == "Design Guidance section").Status);
            CollectionAssert.Contains(nonDoneRows.Select(row => row.Name).ToList(), "Design Guidance section");
            CollectionAssert.Contains(nonDoneRows.Select(row => row.Name).ToList(), "All Controls");
            CollectionAssert.Contains(nonDoneRows.Select(row => row.Name).ToList(), "Status & Info section");
            CollectionAssert.Contains(nonDoneRows.Select(row => row.Name).ToList(), "System section");

            AssertContainsInOrder(
                tracker,
                "Working Checklist page/status completion audit:",
                "table still has 14 rows with at least one non-`Done` status",
                "`Home`",
                "`System section`",
                "These are",
                "row 6 completion-audit blockers, not permission to run lower-priority source",
                "cleanup ahead of reopened visual, High Contrast, high-drift, asset,",
                "measurement, automation, harness, or row 5 evidence.");
        }

        [TestMethod]
        public void TrackerNamesLocalOfficialWpfGallerySourceAsAuthority()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");

            AssertContainsInOrder(
                tracker,
                "Official WPF Gallery source authority:",
                @"`D:\repos\WPF-Samples\Sample Applications\WPFGallery` as the comparison",
                "root before using remote source, old memory, or inferred parity.",
                "If a comparison cites official WPF Gallery source, name the local file or",
                "folder used for that comparison in the work notes or verification section.");

            Assert.IsFalse(
                tracker.Contains("use remote source before local official source", StringComparison.OrdinalIgnoreCase),
                "Tracker should not allow remote WPF Gallery source to outrank the local official source checkout.");
        }

        private static IReadOnlyList<WorkingChecklistRow> ReadWorkingChecklistRows(string tracker)
        {
            var rows = new List<WorkingChecklistRow>();
            var inTable = false;
            var lines = tracker.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.StartsWith("| Page or group | Structural tests | Exact source audit | Visual checked | Notes |", StringComparison.Ordinal))
                {
                    inTable = true;
                    continue;
                }

                if (!inTable)
                {
                    continue;
                }

                if (line.StartsWith("| ---", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!line.StartsWith("|", StringComparison.Ordinal))
                {
                    break;
                }

                var cells = line.Split('|').Select(cell => cell.Trim()).ToArray();
                if (cells.Length >= 6 && cells[1].Length > 0)
                {
                    rows.Add(new WorkingChecklistRow(cells[1], cells[2], cells[3], cells[4]));
                }
            }

            return rows;
        }

        private sealed class WorkingChecklistRow
        {
            public WorkingChecklistRow(string name, string structuralTests, string exactSourceAudit, string visualChecked)
            {
                Name = name;
                StructuralTests = structuralTests;
                ExactSourceAudit = exactSourceAudit;
                VisualChecked = visualChecked;
            }

            public string Name { get; }

            public string StructuralTests { get; }

            public string ExactSourceAudit { get; }

            public string VisualChecked { get; }

            public string Status => StructuralTests + "/" + ExactSourceAudit + "/" + VisualChecked;
        }
    }
}
