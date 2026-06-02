# WPF Gallery Interaction Audit

## Goal

Find and fix WPF Gallery issues that appear during real user interaction, with emphasis on bugs in the gallery shell and ModernWpf controls surfaced by clicking through the app.

## Process

- Prefer actual UI Automation or mouse-driven reproduction over route-only visual captures.
- Keep route-driven visual parity checks as secondary evidence.
- Commit after each verified fix round, grouping related fixes into meaningful commits.
- Record each round here with reproduction, fix, coverage, and verification.

## Known Failures

| Status | Area | Symptom | Reproduction Gap |
| --- | --- | --- | --- |
| Fixed in round 1 | Gallery shell NavigationView | Clicking an expandable group can show a selected group row with a large blank gap before children/following items. | Existing visual parity cases used initial routes, not real clicks. |
| Fixed in round 1 | Gallery shell NavigationView | Clicking `Samples` can hide the `User Dashboard` child or push it out of the expected position above `All Controls`. | Existing tests did not assert child visibility through a real clicked expansion path. |
| Fixed in round 1 | NavigationView control automation | `NavigationViewItemAutomationPeer` implemented `IInvokeProvider` but did not expose `PatternInterface.Invoke`, so UI Automation could not invoke nav items. | Existing NavigationView API test explicitly expected no Invoke pattern. |

## Round 1: NavigationView Click Expansion

### Scope

Fix gallery shell group expansion through actual user-style clicks:

- `Design Guidance` expands with `Colors` directly under the selected row.
- `Samples` expands with `User Dashboard` visible above `All Controls`.
- Selected group background remains row-sized.

### Current Findings

- Route-driven parity captures can show the correct final state while real click paths still fail.
- The gallery shell uses `NavigationViewItem` with an expanded children host in row 1; layout changes must preserve both row-sized selection and child host measurement.
- `NavigationViewItemAutomationPeer` implemented `IInvokeProvider.Invoke`, but `GetPattern` did not return it for `PatternInterface.Invoke`. The click audit now uses native click first and falls back to UIA Invoke when OS-level click injection is not delivered by the test environment.
- Added click-driven visual audit cases:
  - `ShellClickDesignGuidance`: launches at Home, clicks `Design Guidance`, waits for `category/DesignGuidance`, then captures the navigation pane.
  - `ShellClickSamples`: launches at Home, clicks `Samples`, waits for `category/Samples`, then captures the navigation pane.

### Verification

- Focused tests:
  - `GalleryNavigationRuntimeTests.ShellNavigationGroupRowsToggleExpansionWhenInvoked`
  - `WpfGallerySourceShapeTests.WpfGalleryVisualAuditLaunchesOfficialDisplayRoutesWithCanonicalReadyRoutes`
  - `NavigationViewApiTests.VerifyNavigationItemUIAType`
- Broad tests:
  - `ModernWpf.Gallery.Tests` net8: 545 passed
  - `ModernWpf.Gallery.Tests` net10: 545 passed
  - `NavigationViewApiTests`: 48 passed
  - Full `ModernWpf.WinUI.Tests` was attempted with `--no-build`, but timed out before reporting results; no failure was reported before the timeout.
- Visual audit:
  - Dark `ShellClickDesignGuidance`, `ShellClickSamples`: `artifacts/wpf-gallery-visual-audit/20260602-013810-532-52600/report.md`
  - Light `ShellClickDesignGuidance`, `ShellClickSamples`: `artifacts/wpf-gallery-visual-audit/20260602-013908-551-21820/report.md`
