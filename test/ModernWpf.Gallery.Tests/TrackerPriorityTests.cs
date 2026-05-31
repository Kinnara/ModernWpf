using System;
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
                "The next substantive row is not open-ended row 5.4.",
                "Current active selection snapshot:");
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
                "Current pointer:");
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
    }
}
