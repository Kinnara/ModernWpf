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
            var currentOrderLock = ReadMarkdownSection(tracker, "## Current Order Lock");

            AssertContainsInOrder(
                currentOrderLock,
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
                "Current completion state:",
                "The final lower bucket, **global order 13 / P2 row 6**, is complete",
                "Current active selection snapshot:",
                "Final completion audit, 2026-06-01:");
        }

        [TestMethod]
        public void TrackerImmediateStatusAnswersCompletionBeforeHistoricalQueues()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");
            var immediateStatus = ReadMarkdownSection(tracker, "## Immediate Status and Next Selection");

            AssertContainsInOrder(
                immediateStatus,
                "## Immediate Status and Next Selection",
                "Milestone 1 is complete for the current branch tip",
                "not closed permanently for future changes.",
                "Completion proof, using executable buckets:",
                "If a new real OS High Contrast, visible drift, high-drift retained-control,",
                "row 5 source cleanup",
                "Row 5 is recorded for the current branch tip through the row 5.5 inventory.",
                "Row 6/final verification has run for the current branch tip and proved no",
                "The goal may remain complete only while `Goal tracker status in Codex`",
                "Fail closed: if any lower tracker section, stale `Current` note, historical",
                "the lower note and run the higher-ranked evidence first.",
                "Current pointer:",
                "Latest final completion audit, 2026-06-01:",
                "Previous committed branch tip before",
                "`a4aeb46d`",
                "artifact report/json references existed",
                "all were `Done/Done/Done`",
                "full",
                "passed",
                "544 tests per target",
                "Post-visual-status commits");

            Assert.IsFalse(
                tracker.Contains("The branch tip is", StringComparison.Ordinal),
                "Immediate Status should not describe a historical commit as the current branch tip.");
        }

        [TestMethod]
        public void TrackerMarksFinalCompletionAfterRowSixAudit()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");
            var currentStatus = ReadMarkdownSection(tracker, "## Current Status");
            var currentOrderLock = ReadMarkdownSection(tracker, "## Current Order Lock");

            AssertContainsInOrder(
                currentStatus,
                "Goal tracker status in Codex: complete.",
                "Final row 6 completion audit:",
                "artifact-reference audit found 547 unique tracked",
                "0 missing files",
                "Current status tables were internally consistent",
                "## Working Checklist",
                "544 tests per",
                "`git diff --name-only e3e568e5..a4aeb46d` showed",
                "visual, real High Contrast,",
                "high-drift, and harness evidence",
                "remains current");

            AssertContainsInOrder(
                currentOrderLock,
                "Current completion state:",
                "The final lower bucket, **global order 13 / P2 row 6**, is complete",
                "Row 5.4 is not open-ended",
                "Current active selection snapshot:",
                "future visual,",
                "evidence still preempts any",
                "source-shape cleanup.",
                "No current allowed lower row remains.");
        }

        [TestMethod]
        public void TrackerKeepsWorkingChecklistClosedAfterFinalAudit()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");
            var workingChecklistRows = ReadWorkingChecklistRows(tracker);
            var nonDoneRows = workingChecklistRows
                .Where(row => row.StructuralTests != "Done" || row.ExactSourceAudit != "Done" || row.VisualChecked != "Done")
                .ToList();

            Assert.AreEqual(
                0,
                nonDoneRows.Count,
                string.Join(", ", nonDoneRows.Select(row => row.Name + " " + row.Status)));

            foreach (var closedRow in new[]
            {
                "What's New",
                "User Dashboard",
                "DataGrid",
                "Expander",
                "ResizeGrip",
                "GridSplitter",
                "GroupBox",
                "Color",
                "Iconography",
                "Samples section",
                "Basic Input section",
                "Collections section",
                "Date & Calendar section",
                "Layout section",
                "Media section",
                "Navigation section",
                "Text section",
                "Status & Info section",
                "System section",
                "Settings",
                "Design Guidance section",
                "All Controls",
                "Home"
            })
            {
                Assert.AreEqual("Done/Done/Done", workingChecklistRows.Single(row => row.Name == closedRow).Status, closedRow);
                Assert.IsFalse(nonDoneRows.Any(row => row.Name == closedRow), closedRow + " should not remain a completion-audit blocker.");
            }

            Assert.IsFalse(
                workingChecklistRows.Any(row => row.VisualChecked != "Done"),
                string.Join(", ", workingChecklistRows.Where(row => row.VisualChecked != "Done").Select(row => row.Name + " " + row.Status)));
            Assert.AreEqual("Done/Done/Done", workingChecklistRows.Single(row => row.Name == "Home").Status);
            Assert.IsFalse(nonDoneRows.Any(row => row.Name == "Home"), "Home should not remain a completion-audit blocker.");
            Assert.AreEqual("Done/Done/Done", workingChecklistRows.Single(row => row.Name == "Settings").Status);
            Assert.IsFalse(nonDoneRows.Any(row => row.Name == "Settings"), "Settings should not remain a completion-audit blocker.");
            Assert.AreEqual("Done/Done/Done", workingChecklistRows.Single(row => row.Name == "Design Guidance section").Status);
            Assert.IsFalse(nonDoneRows.Any(row => row.Name == "Design Guidance section"), "Design Guidance section should not remain a completion-audit blocker.");
            Assert.AreEqual("Done/Done/Done", workingChecklistRows.Single(row => row.Name == "All Controls").Status);
            Assert.IsFalse(nonDoneRows.Any(row => row.Name == "All Controls"), "All Controls should not remain a completion-audit blocker.");

            AssertContainsInOrder(
                tracker,
                "Working Checklist page/status completion audit:",
                "table now has 0 rows with at least one non-`Done` status",
                "The final row 6",
                "audit verified",
                "no current-state tracker section",
                "visual/High Contrast/high-drift record",
                "harness result",
                "explicit goal");
        }

        [TestMethod]
        public void TrackerCurrentStatusTablesDoNotCarryStaleOpenLabels()
        {
            var tracker = ReadRepoFile("docs", "wpf-gallery-milestone-1-tracker.md");
            var doneRows = ReadAreaStatusRows(tracker, "## Done", "| Area | Status | Notes |");
            var reopenOnlyRows = ReadAreaStatusRows(tracker, "## Recorded Reopen-Only Work", "| Area | Status | Next action |");
            var staleStatuses = new[] { "Mostly done", "Partial", "Open, gated" };

            Assert.IsFalse(
                tracker.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Any(line => line == "## Needs Work"),
                "Current status section should be reopen-only, not Needs Work.");

            Assert.IsFalse(
                doneRows.Any(row => staleStatuses.Contains(row.Status)),
                string.Join(", ", doneRows.Where(row => staleStatuses.Contains(row.Status)).Select(row => row.Name + " " + row.Status)));
            Assert.IsFalse(
                reopenOnlyRows.Any(row => staleStatuses.Contains(row.Status)),
                string.Join(", ", reopenOnlyRows.Where(row => staleStatuses.Contains(row.Status)).Select(row => row.Name + " " + row.Status)));

            foreach (var recordedRow in new[]
            {
                "Page-by-page exact XAML audit",
                "Home page",
                "All Controls page",
                "Section pages",
                "Sample code panes",
                "Theme behavior",
                "Shell high-contrast chrome",
                "Keyboard and automation details"
            })
            {
                Assert.IsTrue(
                    reopenOnlyRows.Single(row => row.Name == recordedRow).Status.StartsWith("Recorded", StringComparison.Ordinal),
                    recordedRow);
            }

            AssertContainsInOrder(
                tracker,
                "## Recorded Reopen-Only Work",
                "This section is supporting evidence, not the scheduler.",
                "Every row below is recorded or reopen-only for Milestone 1",
                "`Current Order Lock` global-order table");
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

        private static string ReadMarkdownSection(string markdown, string heading)
        {
            var start = markdown.StartsWith(heading, StringComparison.Ordinal)
                ? 0
                : markdown.IndexOf("\n" + heading, StringComparison.Ordinal);

            if (start > 0)
            {
                start++;
            }

            Assert.IsTrue(start >= 0, "Could not find markdown section " + heading);

            var next = markdown.IndexOf("\n## ", start + heading.Length, StringComparison.Ordinal);
            return next >= 0
                ? markdown.Substring(start, next - start)
                : markdown.Substring(start);
        }

        private static IReadOnlyList<AreaStatusRow> ReadAreaStatusRows(string tracker, string heading, string tableHeader)
        {
            var rows = new List<AreaStatusRow>();
            var lines = tracker.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var headingIndex = lines.ToList().FindIndex(line => line == heading);
            Assert.IsTrue(headingIndex >= 0, heading);
            var headerIndex = lines
                .Skip(headingIndex + 1)
                .TakeWhile(line => !line.StartsWith("## ", StringComparison.Ordinal))
                .Select((line, index) => new { Line = line, Index = headingIndex + 1 + index })
                .FirstOrDefault(item => item.Line == tableHeader)?.Index ?? -1;
            Assert.IsTrue(headerIndex >= 0, tableHeader);

            for (var i = headerIndex + 2; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!line.StartsWith("|", StringComparison.Ordinal))
                {
                    break;
                }

                var parts = line.Trim('|').Split('|').Select(part => part.Trim()).ToArray();
                if (parts.Length >= 2)
                {
                    rows.Add(new AreaStatusRow(parts[0], parts[1]));
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

        private sealed class AreaStatusRow
        {
            public AreaStatusRow(string name, string status)
            {
                Name = name;
                Status = status;
            }

            public string Name { get; }

            public string Status { get; }
        }
    }
}
