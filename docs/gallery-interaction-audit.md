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
| Fixed in round 1 | Gallery shell NavigationView | Clicking an expandable group did not reliably expand and place its children directly under the selected group row. | Existing visual parity cases used initial routes, not real clicks. |
| Fixed in round 1 | Gallery shell NavigationView | Clicking `Samples` can hide the `User Dashboard` child or push it out of the expected position above `All Controls`. | Existing tests did not assert child visibility through a real clicked expansion path. |
| Fixed in round 1 | NavigationView control automation | `NavigationViewItemAutomationPeer` implemented `IInvokeProvider` but did not expose `PatternInterface.Invoke`, so UI Automation could not invoke nav items. | Existing NavigationView API test explicitly expected no Invoke pattern. |
| Fixed in round 2 | NavigationView control layout | Clicking an expanded group again collapsed the child repeater visually, but the parent item kept the old expanded height as a large blank gap before the next row. | Round 1 checked expansion and selected row height, but did not assert that collapsed child layout space was released. |
| Fixed in round 3 | Basic Input visual checks | `Button`, `CheckBox`, `ComboBox`, `RadioButton`, and `Slider` pages rendered successfully but exposed no stable `GallerySample_*` anchors, so visual checks could not crop or require their primary samples. | The visual check reported route-ready pages as failed/missing required sample elements; curated automation-ID coverage did not include these WPF Gallery pages. |
| Fixed in round 4 | Visual-check harness crops | `SplitView` and `PersonPicture` passed route readiness and rendered correct sample artifacts, but the visual check failed because their ModernWpf primary crops were taken from blank full-window screenshots. | ModernWpf primary crop mappings used inner option names instead of rendered `GallerySample_*` artifact IDs, so full-window capture failures looked like sample failures. |

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

## Round 2: NavigationView Collapse Layout

### Scope

Fix the second-click collapse state for expandable gallery navigation groups:

- `Design Guidance` expands and selects on the first user invocation.
- A second invocation collapses the children without leaving the old child area as blank space.
- The selected group page remains displayed after collapse.

### Current Findings

- The collapsed child item was already not visible, so this was not a catalog, selection, or route bug.
- The selected row background was row-sized, but the parent `NavigationViewItem` still reported the previous expanded height, so the following top-level row was arranged hundreds of pixels lower.
- `NavigationViewItem.ShowHideChildren` changed the nested repeater visibility but did not invalidate the item measure.
- The owning `ItemsRepeater` also needed a measure invalidation so its `StackLayout` stopped using the previous expanded extent.
- Added `ShellClickDesignGuidanceCollapse`: launches at Home, clicks `Design Guidance` twice, waits for `category/DesignGuidance`, then captures the collapsed selected group state.

### Verification

- Focused tests:
  - `GalleryNavigationRuntimeTests.ShellNavigationGroupRowsToggleExpansionWhenInvoked`
  - `WpfGallerySourceShapeTests.WpfGalleryVisualAuditLaunchesOfficialDisplayRoutesWithCanonicalReadyRoutes`
  - `NavigationViewApiTests`: 48 passed
- Visual audit:
  - Dark `ShellClickDesignGuidanceCollapse`: `artifacts/wpf-gallery-visual-audit/20260602-020044-354-88816/report.md`
  - Light `ShellClickDesignGuidanceCollapse`: `artifacts/wpf-gallery-visual-audit/20260602-020120-506-22984/report.md`

## Round 3: Basic Input Visual Anchors

### Scope

Restore visual-check coverage for the first WPF Gallery Basic Input pages users usually click:

- `Button`
- `CheckBox`
- `ComboBox`
- `RadioButton`
- `Slider`

### Current Findings

- These pages reached `Ready:item/...` and rendered nonblank content.
- The visual checker still failed them because it could not find a required sample automation element or rendered artifact crop.
- The pages are direct WPF Gallery XAML pages, not generated `GallerySamplePanel` pages, so they had not been included in the curated `GallerySample_*` automation-ID test matrix.
- Added sample-root and primary-control automation IDs directly to the first example on each page.
- Updated `Run-GalleryVisualChecks.ps1` so `CheckBox`, `RadioButton`, and `Slider` primary crops target the actual first control instead of defaulting to the example root.
- Updated the curated sample-ID test to include all five pages and to handle direct WPF pages with an embedded visible `PageHeader`.

### Verification

- Focused test:
  - `GalleryAutomationHookTests.CuratedSamplesExposeStableAutomationIds`: 46 passed on net8 and net10
- Visual audit:
  - Dark `Button`, `CheckBox`, `RadioButton`, `Slider`, `ToggleButton`, `RepeatButton`, `ToggleSwitch`, `NumberBox`, `ComboBox`, `AutoSuggestBox` with `Reference=None`: `artifacts/visual-checks/20260602-021355-602-86796/report.md`
- Note:
  - `Reference=InstalledWinUI3Gallery` is currently blocked in this environment by OS denial when starting `winui3gallery://...` URI routes, so this round verified ModernWpf coverage independently.

## Round 4: SplitView and PersonPicture Crop Coverage

### Scope

Fix visual-check false failures that appeared during the broader controls sweep:

- `SplitView`
- `PersonPicture`

### Current Findings

- The failed sweep reported `Primary crop 'NavLinksList' was blank` for `SplitView` and `Primary crop 'ProfileImageRadio' was blank` for `PersonPicture`.
- The full-window screenshots for both pages were blank, but the rendered sample artifacts were correct and nonblank.
- `Get-ModernPrimaryCropAutomationId` was using inner option names (`NavLinksList`, `ProfileImageRadio`) that have no ModernWpf rendered sample artifact, forcing the harness to crop from the unreliable blank full-window image.
- Updated the ModernWpf primary crop mappings to target `GallerySample_SplitView_SplitView` and `GallerySample_PersonPicture_PersonPicture`; left the installed WinUI Gallery reference mappings unchanged.
- Added a source-shape test so the ModernWpf artifact-backed mappings stay separate from the installed-reference UIA mappings.

### Verification

- Focused test:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksUseRenderedModernPrimaryArtifactsForSplitViewAndPersonPicture`: passed on net8 and net10
- Visual audit:
  - Initial failing broad sweep: `artifacts/visual-checks/20260602-021906-706-42352/report.md`
  - Focused `SplitView`, `PersonPicture` rerun: `artifacts/visual-checks/20260602-022318-767-63064/report.md`
  - Broad rerun for `ColorPicker`, `HyperlinkButton`, `RatingControl`, `DropDownButton`, `SplitButton`, `ToggleSplitButton`, `SplitView`, `PersonPicture`, `ParallaxView`, `IconElement`, `ThemeShadow`, `TitleBar`, `InfoBadge`, `InfoBar`, `ProgressRing`, `PipsPager`, `AnnotatedScrollBar`, `PullToRefresh`, `GridView`, `ItemsRepeater`, `BreadcrumbBar`, `Pivot`, `SelectorBar`, and `NavigationView` with `Reference=None`: `artifacts/visual-checks/20260602-022401-170-82512/report.md`
