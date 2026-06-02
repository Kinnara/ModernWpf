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
| Fixed in round 5 | Click-open visual checks | `ContentDialog`, `Flyout`, `Popup`, `MenuFlyout`, and `DropDownButton` had stable static rendering checks but no actual open-state verification under `-IncludeInteractions`. | The harness only opened `TeachingTip` and `CommandBarFlyout`; static route checks could not catch broken click/open paths for common popup controls. |
| Fixed in round 6 | SplitButton and ToggleSplitButton click-open checks | Opening the secondary flyout target needs a dedicated path; a naive center click invokes the primary action, while recursive UIA popup searches hang. | Round 5 intentionally left these out; round 6 uses a bounded secondary-segment click and visual-delta verification without walking the popup UIA tree. |

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

## Round 5: Click-Open Interaction Coverage

### Scope

Expand `Run-GalleryVisualChecks.ps1 -IncludeInteractions` beyond the two existing open-state cases:

- `ContentDialog`
- `Flyout`
- `Popup`
- `MenuFlyout`
- `DropDownButton`
- `CommandBarFlyout`
- `TeachingTip`

### Current Findings

- The harness previously only opened `TeachingTip` and `CommandBarFlyout`; other popup/flyout pages could pass from route readiness and static crops alone.
- Added a supported-control list and expected open-content names, then made the ModernWpf capture path keep the actual UIA trigger element even when rendered artifacts exist.
- The opener now tries a real click first, checks for opened content, then falls back to `ExpandCollapsePattern`, `InvokePattern`, and keyboard space only when needed.
- Installed WinUI Gallery reference trigger lookup was updated to use the same interaction-name table, but reference capture remains blocked in this environment by the `winui3gallery://...` access-denied issue noted in round 3.
- `SplitButton` and `ToggleSplitButton` were intentionally not included in the committed open-state list. An attempted generic path exposed two harness limitations: center-clicking invokes the primary action rather than opening the flyout, and recursive popup UIA search can hang; screen-rect capture also failed with an invalid-handle error in this environment.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksRetriesCommandBarFlyoutOpenThroughInvokePattern`: passed on net8 and net10
  - `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
  - `WpfGallerySourceShapeTests.GalleryVisualChecksCaptureInteractionFramesWithoutReactivatingWindow`: passed on net8 and net10
- Visual audit:
  - Dark `ContentDialog`, `Flyout`, `Popup`, `MenuFlyout`, `DropDownButton`, `CommandBarFlyout`, and `TeachingTip` with `Reference=None` and `IncludeInteractions`: `artifacts/visual-checks/20260602-025315-628-56592/report.md`
  - Full Dark `Reference=None` sweep with `IncludeInteractions` for the supported round-5 open controls and all static controls: `artifacts/visual-checks/20260602-025637-208-12020/report.md`

## Round 6: SplitButton Secondary Flyout Coverage

### Scope

Add click-open interaction coverage for controls where the center of the control is not the flyout opener:

- `SplitButton`
- `ToggleSplitButton`

### Current Findings

- `SplitButton` and `ToggleSplitButton` have separate primary and secondary regions. Center-clicking the control invokes the primary action, so the round-5 generic opener was intentionally not applied to them.
- A generic UIA name search after opening these flyouts can hang in this environment, so using popup UIA traversal as the proof would make the visual check flaky.
- Added a bounded secondary-segment click that targets the right edge of the control.
- For these two controls, the harness now verifies opening from captured frame deltas and difference crops instead of walking the opened popup UIA subtree.
- The first attempted two-control run hung before producing open frames: `artifacts/visual-checks/20260602-030901-942-76244`.

### Verification

- Focused test:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
- Visual audit:
  - Dark `SplitButton` and `ToggleSplitButton` with `Reference=None` and `IncludeInteractions`: `artifacts/visual-checks/20260602-031424-328-18236/report.md`
  - Dark combined supported open-interaction sweep for `ContentDialog`, `Flyout`, `Popup`, `MenuFlyout`, `DropDownButton`, `SplitButton`, `ToggleSplitButton`, `CommandBarFlyout`, and `TeachingTip`: `artifacts/visual-checks/20260602-031623-475-52824/report.md`

## Round 7: Basic Input Runtime Guard Cleanup

### Scope

Fix stale Basic Input runtime/source-shape assertions after the visual-anchor work from round 3:

- `Button`
- `CheckBox`
- `ComboBox`
- `RadioButton`
- `Slider`

### Current Findings

- The gallery pages had the intended `GallerySample_*` automation IDs, but two runtime parity tests still asserted that the first direct WPF Basic Input examples had empty automation IDs.
- The broader `FullyQualifiedName~BasicInput` slice also caught a source-shape guard that still matched the pre-anchor one-line CheckBox `ControlExample` tag.
- Updated the runtime tests to verify the stable sample-root and primary-control anchors instead of accepting empty IDs.
- Updated the source-shape guard to keep checking the official sample headers, XAML snippets, and ordering while also pinning the intentional `GallerySample_*` anchor attributes.

### Verification

- Focused runtime tests:
  - `GalleryPageRuntimeTests.BasicInputButtonAndCheckBoxPagesMatchWpfGalleryReference`: passed on net8 and net10
  - `GalleryPageRuntimeTests.BasicInputComboBoxRadioButtonAndSliderPagesMatchWpfGalleryReference`: passed on net8 and net10
- Broader guard slice:
  - `ModernWpf.Gallery.Tests` filter `FullyQualifiedName~BasicInput`: 9 passed on net8
  - `ModernWpf.Gallery.Tests` filter `FullyQualifiedName~BasicInput`: 9 passed on net10

## Round 8: Toggle State Interaction Coverage

### Scope

Expand `Run-GalleryVisualChecks.ps1 -IncludeInteractions` to cover click-state controls, not only controls that open flyouts/popups:

- `CheckBox`
- `ToggleButton`
- `ToggleSwitch`
- `AppBarToggleButton`

### Current Findings

- The harness already had UIA toggle helpers, but they were only used for setup flows such as resetting ProgressRing animation phase.
- The interaction pass therefore still allowed these controls to pass from static render readiness alone; a broken user click or a missing visual state change would not have been caught.
- Added state-interaction capture that records the initial toggle state, toggles the target to the opposite state, verifies the UIA state changed, and verifies the cropped control image visibly changed.
- Kept this separate from open-state interactions so flyouts still use the popup/open-content proof path and toggle-pattern controls use state-change proof.
- Found and fixed a related default-sweep gap: `CheckBox`, `RadioButton`, and `Slider` had direct-page visual anchors, but the script's default `Controls` list still omitted them.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksTogglesCommonStateInteractionControls`: passed on net8 and net10
  - `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
  - Full `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 7 passed on net8 and net10
- Visual audit:
  - Dark state sweep for `CheckBox`, `ToggleButton`, `ToggleSwitch`, `AppBarToggleButton`: `artifacts/visual-checks/20260602-033206-677-83284/report.md`
  - Light state sweep for `CheckBox`, `ToggleButton`, `ToggleSwitch`, `AppBarToggleButton`: `artifacts/visual-checks/20260602-033258-522-13052/report.md`
  - Dark combined interaction sweep for the four state controls plus `ContentDialog`, `Flyout`, `Popup`, `MenuFlyout`, `DropDownButton`, `SplitButton`, `ToggleSplitButton`, `CommandBarFlyout`, and `TeachingTip`: `artifacts/visual-checks/20260602-033344-262-37720/report.md`
  - Dark sanity sweep for newly defaulted direct Basic Input controls `CheckBox`, `RadioButton`, and `Slider`: `artifacts/visual-checks/20260602-033759-115-35152/report.md`

## Round 9: Selection Interaction Coverage

### Scope

Expand visual interaction coverage for selection-style controls with stable visible before/after states:

- `PipsPager`
- `Pivot`

Also add runtime click coverage for `SelectorBar`, which exposed an ambiguous visual-check target during this round.

### Current Findings

- `PipsPager` and `Pivot` previously passed static rendering without proving that a user click changed selection.
- The first `PipsPager` sample does not show previous/next buttons, so the reliable click target is the visible `Page 2` pip rather than `Next Page`.
- `Pivot` initially failed because the harness found the visible label but did not invoke the interactive tab item. Added a selection invoker that walks up to a `SelectionItemPattern`/`InvokePattern` ancestor and then uses a native click fallback.
- `SelectorBar` click behavior is now covered by an in-process runtime mouse down/up regression. The visual harness attempt remains tracked separately because the basic sample has no initially selected item and the visual proof did not show a reliable before/after delta.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksClicksCommonSelectionInteractionControls`: passed on net8 and net10
  - Full `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 8 passed on net8 and net10
  - `GalleryAutomationHookTests.SelectorBarSampleMatchesWinUIGalleryExamples`: passed on net8 and net10
- Visual audit:
  - Initial failing selection sweep for `PipsPager`, `Pivot`, and `SelectorBar`: `artifacts/visual-checks/20260602-034258-580-82964/report.md`
  - Selection sweep after scoped-target/pattern-invocation fixes, with `Pivot` passing and `PipsPager`/`SelectorBar` still failing: `artifacts/visual-checks/20260602-034618-480-30884/report.md`
  - Selection sweep after switching `PipsPager` to `Page 2` and adding native-click fallback, with only `SelectorBar` still failing: `artifacts/visual-checks/20260602-034950-835-56632/report.md`
  - Dark supported selection sweep for `PipsPager` and `Pivot`: `artifacts/visual-checks/20260602-035247-913-16492/report.md`
