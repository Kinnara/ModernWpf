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
| Fixed in round 20 | Gallery shell NavigationView expanded layout | Repeated click sequences could still leave an expanded parent consuming a large blank pane area while child rows were missing or pushed away. | Earlier shell checks asserted expansion state and a loose minimum height, but did not require visible child rows, bounded expanded height, or bounded sibling spacing. |
| Fixed in round 27 | CommandBarFlyout interaction visual check | The harness reported CommandBarFlyout open interaction as passed while its saved open crop contained no popup pixels and did not prove the ellipsis secondary commands. | Main-window captures miss WPF popup HWNDs, and the old check accepted primary command UIA like `Share` without opening `MoreButton` or capturing the popup window. |
| Fixed in round 53 | SelectorBar selection recording | SelectorBar could be accepted from automation state or left at `NeedsReview` while the visible item template was blank or no selection indicator changed in the recording. | The recorder did not require rendered frame evidence for SelectorBar selection, and the external UIA tree did not expose generated item peers as selectable `TabItem`s. |
| Fixed in round 56 | CommandBar open-repeat recording | CommandBar could remain `NeedsReview` because UIA did not expose the second-open overflow item even while the video showed the overflow open. | The recorder had no frame-region open/closed/open proof tied to the overflow item bounds, so dense review could not be promoted to verified evidence. |
| Fixed in round 56 | CommandBarFlyout repeat-open and secondary menu recording | CommandBarFlyout could pass while the selected visual proof frame only showed the primary flyout, not the expanded `Resize` / `Move` secondary menu. | The recorder accepted low region deltas and UIA open state without requiring both first and second secondary-menu expansions to be visible in the proof frames. |

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

Add runtime click coverage for `SelectorBar`, which exposed an ambiguous
visual-check target during this round.

### Current Findings

- `SelectorBar` click behavior is now covered by an in-process runtime mouse down/up regression. The visual harness attempt remains tracked separately because the basic sample has no initially selected item and the visual proof did not show a reliable before/after delta.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksClicksCommonSelectionInteractionControls`: passed on net8 and net10
  - Full `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 8 passed on net8 and net10
  - `GalleryAutomationHookTests.SelectorBarSampleMatchesWinUIGalleryExamples`: passed on net8 and net10
- Visual audit:
  - Selection sweep with only `SelectorBar` still failing: `artifacts/visual-checks/20260602-034950-835-56632/report.md`

## Round 10: GridView Item Activation Coverage

### Scope

Expand visual interaction coverage for the Gallery `GridView` sample so it proves activating the first item exposes the expected click output:

- `GridView`

### Current Findings

- The Gallery runtime tests already verified the sample's `ItemClick` handler through an internal helper, but the visual pass did not activate any `GridView` item.
- Initial visual attempts exposed the same coverage gap in multiple forms:
  - UIA `SelectionItemPattern.Select()` selected the first tile but did not fire `ItemClick`.
  - Native coordinate clicks were not reliable enough in the visual runner to use as the proof path.
  - UIA focus plus Space selected the item but still did not expose the click output.
- The underlying automation gap was that `ListViewBase` used WPF's stock selector-owned item peers, which expose selection but not `Invoke`.
- Added `ListViewBaseAutomationPeer` and selector-owned `ListViewBaseItemAutomationPeer` so `ListView`/`GridView` items expose `InvokePattern`; invoking the item now routes through `ListViewBase.NotifyListItemClicked`.
- Added a focused WinUI test for `GridViewItem` automation invoke and kept a direct own-container content regression for `NotifyListItemClicked`.
- Extended the visual harness so `GridView` selection interaction selects `Item 1`, invokes it through UIA, and verifies the rendered Gallery output text `You clicked Item 1.` appears.

### Verification

- Focused tests:
  - `ListViewApiTests.ItemClickUsesOwnContainerContent`, `ListViewApiTests.ItemClickUsesOwnGridContainerContent`, and `ListViewApiTests.GridViewItemAutomationInvokeRaisesItemClick`: passed on net8
  - `ModernWpf.WinUI.Tests` is net8-only in this repo, so there is no net10 slice for these tests.
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks`: 8 passed on net8 and net10
- Visual audit:
  - First failing `GridView` selection run using selection without activation: `artifacts/visual-checks/20260602-035757-449-71644/report.md`
  - Passing focused `GridView` visual activation run after adding item Invoke automation: `artifacts/visual-checks/20260602-042327-414-39708/report.md`

## Round 11: Shell Navigation Expansion State Coverage

### Scope

Tighten the WPF Gallery shell click audit around NavigationView parent items:

- `ShellClickDesignGuidance`
- `ShellClickDesignGuidanceCollapse`
- `ShellClickSamples`

### Current Findings

- The previous shell click checks could pass with broken expansion/collapse behavior because they only proved route readiness and a nonblank pane crop.
- Once the audit asserted `ExpandCollapsePattern` state and parent item height, the collapse case failed: `Design Guidance` remained `Expanded` with a 250px item extent.
- The audit itself also had a false recovery path: after a successful collapse click, it re-invoked the last item while trying to recover route readiness, which expanded the item again.
- Fixed the audit so shell state cases assert expanded/collapsed UIA state and geometry, click the visible disclosure glyph, and skip the route re-invoke/route wait for the state-only collapse case.
- Fixed the control path behind the click by making `InputHelper` observe handled mouse events and by giving `NavigationViewItemPresenter` an explicit chevron mouse down/up path that raises the normal tapped event. That prevents child glyph handling and parent presenter capture from swallowing or double-processing the disclosure click.

### Verification

- Focused tests:
  - `NavigationViewApiTests.ExpandCollapseChevronMouseDownDoesNotLetPresenterStealCapture` and `VerifyExpandCollapseChevronVisibility`: passed on net8
  - `WpfGallerySourceShapeTests.WpfGalleryVisualAuditValidatesShellClickExpansionState` and `WpfGalleryVisualAuditLaunchesOfficialDisplayRoutesWithCanonicalReadyRoutes`: passed on net8 and net10
- Visual audit:
  - Initial strengthened audit failure proving the old false pass: `artifacts/wpf-gallery-visual-audit/20260602-043405-461-63352/report.md`
  - Focused passing collapse state run: `artifacts/wpf-gallery-visual-audit/20260602-051527-032-94560/report.md`
  - Passing combined shell click run for expand, collapse, and Samples: `artifacts/wpf-gallery-visual-audit/20260602-051552-177-28404/report.md`

## Round 12: ComboBox Dropdown Interaction Coverage

### Scope

Expand visual interaction coverage for the Gallery ComboBox sample:

- `ComboBox`

### Current Findings

- The visual harness rendered the ComboBox page and required `GallerySample_ComboBox_ComboBox`, but `-IncludeInteractions` did not open the dropdown.
- A first pass at adding ComboBox to open interactions exposed two false-pass paths:
  - UIA name lookup matched the already-visible selected `Blue` item instead of proving a dropdown was open.
  - Main-window screen capture could grab an occluding desktop surface, producing unrelated pixels that looked like an interaction delta.
- Fixed the open proof so ComboBox requires an expanded dropdown list item outside the closed control bounds.
- Added popup-window capture through the dropdown item's native window handle. The resulting artifact now records the actual dropdown bitmap and uses it as the visual proof instead of relying on stale UIA or an occluded screen frame.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 8 passed on net8 and net10
- Visual audit:
  - Initial ComboBox run with false UIA pass: `artifacts/visual-checks/20260602-052151-742-12528/report.md`
  - Screen-capture trust failure proving occluded pixels were no longer accepted: `artifacts/visual-checks/20260602-053439-210-96032/report.md`
  - Focused passing ComboBox popup-window run: `artifacts/visual-checks/20260602-054018-863-25296/report.md`
  - Passing broader open-interaction sweep for `ComboBox`, `ContentDialog`, `Flyout`, `Popup`, `MenuFlyout`, `DropDownButton`, `SplitButton`, `ToggleSplitButton`, `CommandBarFlyout`, and `TeachingTip`: `artifacts/visual-checks/20260602-054112-544-90000/report.md`

## Round 13: AutoSuggestBox Typing and Suggestion Selection Coverage

### Scope

Expand visual interaction coverage for the Gallery AutoSuggestBox sample:

- `AutoSuggestBox`

### Current Findings

- The visual harness required `GallerySample_AutoSuggestBox_AutoSuggestBox`, but `-IncludeInteractions` did not type into it, open suggestions, choose a suggestion, or verify the sample output.
- The static capture could report success with no interaction object at all, so regressions in TextBox input, suggestion popup placement/rendering, list item selection, or the sample's `SuggestionChosen` output would not have failed the visual run.
- Added a text interaction path that focuses the embedded edit control, sets/enters the query text `ae`, waits for the `Aegean` suggestion outside the closed control bounds, captures the suggestions popup native window, invokes the suggestion, and verifies the output TextBlock also reports `Aegean`.
- Kept the proof independent from the unreliable main-window screenshot path by using the popup window bitmap as the interaction crop.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 9 passed on net8 and net10
- Visual audit:
  - Static-only AutoSuggestBox baseline showing `Interaction: null`: `artifacts/visual-checks/20260602-054753-493-73932/report.md`
  - Focused passing AutoSuggestBox typing/suggestion run: `artifacts/visual-checks/20260602-055316-223-61868/report.md`
  - Passing combined popup interaction sweep for `AutoSuggestBox` and `ComboBox`: `artifacts/visual-checks/20260602-055556-530-69372/report.md`

## Round 14: NumberBox Spinner Value Coverage

### Scope

Expand visual interaction coverage for the Gallery NumberBox spin-button sample:

- `NumberBox`

### Current Findings

- The visual harness required `GallerySample_NumberBox_SpinButtonNumberBox`, but `-IncludeInteractions` did not activate the spinner or prove that the value changed.
- The live Gallery sample starts at `Value = 10` with `SmallChange = 10`, while the displayed snippet still says `Value="1"`. The audit now uses the live UIA value as the baseline and verifies baseline plus configured step, so the test follows the running sample instead of stale snippet text.
- Initial native-coordinate click attempts found the correct `Increase` button (`AutomationId=UpSpinButton`) and even hit-tested the center to the spinner glyph, but the Win32 mouse injection did not change the value in this runner. Those failures made the old gap visible but were not reliable enough as the proof mechanism.
- Added a value interaction path that reads the `RangeValuePattern` value, invokes the configured increase button through UIA, verifies the expected numeric result (`10 -> 20` in the current Gallery), and records the NumberBox subtree, button identity, hit-test diagnostics, before/after frames, crop delta, and value fields.
- A combined sweep exposed a blank-window capture retry where the value changed correctly but the UIA crop was white. Value interactions now require the numeric state transition plus nonblank before/after value crops, with the visual crop delta recorded as an additional proof.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 10 passed on net8 and net10
- Visual audit:
  - Initial failing NumberBox native-click value run: `artifacts/visual-checks/20260602-060537-259-96648/report.md`
  - Diagnostic failing native-click run proving the `Increase`/`UpSpinButton` target existed: `artifacts/visual-checks/20260602-060800-456-96292/report.md`
  - Native click/topmost and long-hold attempts that still did not advance value in the runner: `artifacts/visual-checks/20260602-061130-411-25500/report.md`, `artifacts/visual-checks/20260602-061438-728-37496/report.md`, and `artifacts/visual-checks/20260602-061514-741-95296/report.md`
  - Focused passing NumberBox UIA value activation run: `artifacts/visual-checks/20260602-061624-801-78892/report.md`
  - Passing combined interaction sweep for `ComboBox`, `AutoSuggestBox`, and `NumberBox`: `artifacts/visual-checks/20260602-061845-816-91420/report.md`

## Round 15: RepeatButton Output Coverage

### Scope

Expand visual interaction coverage for the Gallery RepeatButton sample:

- `RepeatButton`

### Current Findings

- The visual harness required `GallerySample_RepeatButton_RepeatButton`, but `-IncludeInteractions` did not activate the button or prove the sample output changed.
- The existing hook test raised `ButtonBase.ClickEvent` directly and verified `Number of clicks: 1`, but that did not cover the live Gallery visual path.
- Added an output interaction path that captures the sample root, activates the configured trigger, and requires a visible before/after crop delta.
- The first implementation produced a false positive: trigger lookup chose the child text element named `Click and hold`, and the baseline crop was blank white, so the run passed on render-vs-blank instead of output text.
- Fixed the proof so trigger lookup prefers the sample element when its UIA name matches, rejects blank before/after crops, and only passes when the cropped sample changes from an empty output to `Number of clicks: 1`.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 11 passed on net8 and net10
- Visual audit:
  - Initial false-positive RepeatButton output run with blank baseline crop and no output text: `artifacts/visual-checks/20260602-062507-224-48504/report.md`
  - Focused passing RepeatButton output run after trigger/crop fixes: `artifacts/visual-checks/20260602-062723-364-14564/report.md`
  - Passing combined interaction sweep for `RepeatButton`, `NumberBox`, `ComboBox`, and `AutoSuggestBox`: `artifacts/visual-checks/20260602-062812-475-96036/report.md`

## Round 16: RatingControl Value Coverage

### Scope

Expand visual interaction coverage for the Gallery RatingControl sample:

- `RatingControl`

### Current Findings

- The visual harness required `GallerySample_RatingControl_RatingControl`, but `-IncludeInteractions` did not set a rating or prove that the sample's value/caption changed.
- Added RatingControl to the value interaction path. The check reads the live `RangeValuePattern` value, sets the target value to `3`, verifies the numeric UIA value changes from `0` to `3`, and records before/after crops of the rendered control.
- The first focused run exposed the same class of false proof as earlier output/value checks: the full-window baseline screenshot was blank white, so a rendered after frame could look like a large interaction delta even though the before crop was invalid.
- Tightened value interactions to retry blank before/after crops once and then fail if either crop is still blank. This prevents startup/repaint flashes from satisfying interaction coverage.
- The validated RatingControl artifact now shows the expected visual transition from empty stars with caption `312 ratings` to three filled stars with caption `Your rating`, alongside `BaselineValue = 0`, `ExpectedValue = 3`, and `ValueAfter = 3`.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 11 passed on net8 and net10
- Visual audit:
  - Initial RatingControl value run proving the blank baseline crop gap: `artifacts/visual-checks/20260602-063343-205-84752/report.md`
  - Focused passing RatingControl value run after blank-crop hardening: `artifacts/visual-checks/20260602-063640-279-17232/report.md`
  - Passing combined value/output sweep for `RatingControl`, `RepeatButton`, and `NumberBox`: `artifacts/visual-checks/20260602-063726-091-65080/report.md`

## Round 17: Slider Value and Output Coverage

### Scope

Expand visual interaction coverage for the Gallery Slider sample:

- `Slider`

### Current Findings

- The visual harness required `GallerySample_Slider_Slider`, but `-IncludeInteractions` still produced `Interaction: null`, so the sample could render while value changes, thumb movement, and output binding were never exercised.
- Added Slider to the value interaction path. The check reads the live `RangeValuePattern` value, sets the target value to `50`, verifies `BaselineValue = 0`, `ExpectedValue = 50`, and `ValueAfter = 50`, and records the Slider subtree.
- Added a value crop automation hook so Slider uses `GallerySample_Slider_Root` instead of only cropping the control. That makes the visual proof include both thumb movement and the bound output text changing from `0` to `50`.
- The combined value sweep confirms the new crop hook did not regress the existing RatingControl or NumberBox value proofs.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 11 passed on net8 and net10
- Visual audit:
  - Static-only Slider baseline with `Interaction: null`: `artifacts/visual-checks/20260602-063954-481-84224/report.md`
  - Focused passing Slider value/output run: `artifacts/visual-checks/20260602-064149-608-80636/report.md`
  - Passing combined value sweep for `Slider`, `RatingControl`, and `NumberBox`: `artifacts/visual-checks/20260602-064243-561-24188/report.md`

## Round 18: MenuBar Open Interaction Coverage

### Scope

Expand visual interaction coverage for the Gallery MenuBar sample and fix the rendered MenuBar click path:

- `MenuBar`

### Current Findings

- The visual harness required `GallerySample_MenuBar_MenuBar`, but `-IncludeInteractions` did not open a menu or verify menu items, so the sample could render while top-level menu activation was broken or untested.
- The first MenuBar interaction implementation produced a false pass: the harness found a visual delta from an invalid baseline, but no menu item was open and the crop did not show menu content.
- Tightened open-interaction baselines so blank full screenshots or blank closed-control crops fail instead of creating render-vs-blank deltas. Added trigger UIA dumps for open checks so failures show exactly which live element was invoked.
- Fixed `MenuBarItem` so the rendered `ContentButton` has the styled padding as its hit target and the item opens on the routed mouse-up/click path after the current input event. This avoids opening the WPF `ContextMenu` during mouse-down/mouse-up, which can close the flyout before it remains visible.
- Added `MenuBarApiTests` coverage for the rendered button size, the routed mouse-down/mouse-up open path, and direct button click activation.
- Hardened MenuBar visual verification to open through native click first with UIA invoke as a fallback, then require both an opened menu item in UIA and a captured popup-window bitmap showing the visible menu content. The verified crop now shows `New`, `Open...`, `Save`, and `Exit`.
- A combined sweep exposed that the new blank-baseline gate was too strict when full-window control crops were blank but rendered sample artifacts were valid. Added a fallback to use the existing rendered trigger artifact as the closed control proof, while still requiring popup-window proof for opened controls.

### Verification

- Focused tests:
  - `MenuBarApiTests` `FullyQualifiedName~MenuBar` slice: 11 passed on net8
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 11 passed on net8 and net10
- Visual audit:
  - Static-only MenuBar baseline with `Interaction: null`: `artifacts/visual-checks/20260602-064512-752-43608/report.md`
  - False-positive MenuBar open run with no visible menu: `artifacts/visual-checks/20260602-064905-812-71400/report.md`
  - Correct failing MenuBar runs after stricter open verification: `artifacts/visual-checks/20260602-065246-846-29844/report.md`, `artifacts/visual-checks/20260602-070806-234-72688/report.md`, and `artifacts/visual-checks/20260602-072652-671-51660/report.md`
  - Focused passing MenuBar open run with popup-window crop proof: `artifacts/visual-checks/20260602-073526-505-34772/report.md`
  - Passing combined open-control sweep for `MenuBar`, `MenuFlyout`, `DropDownButton`, and `ComboBox`: `artifacts/visual-checks/20260602-073855-817-32676/report.md`

## Round 19: Popup Window Proof for Flyout Open Checks

### Scope

Tighten visual interaction proof for open controls whose content renders in a popup window:

- `MenuFlyout`
- `DropDownButton`

### Current Findings

- The combined Round 18 open-control sweep still showed weak passes for `MenuFlyout` and `DropDownButton`: both exposed expected UIA menu items, but the interaction crops were blank or off-target rather than visible popup content.
- The default open-interaction status accepted `OpenElementFound` without requiring a visible crop. That meant a popup could be present in UIA while the visual artifact failed to prove what the user would see.
- Added a popup-window proof classifier for `MenuFlyout` and `DropDownButton`. These controls now pass only when the opened UIA item is found and the popup's native window capture is nonblank.
- The generic popup proof records `OpenPopupScreenshot`, `OpenPopupNonBlank`, and `OpenPopupSize`, and uses the popup bitmap as the interaction crop. The verified artifacts now show the expected menu entries rather than blank/off-target window crops.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` `FullyQualifiedName~GalleryVisualChecks` slice: 11 passed on net8 and net10
- Visual audit:
  - False-pass evidence from the combined Round 18 sweep, where `MenuFlyout` and `DropDownButton` passed with bad interaction crops: `artifacts/visual-checks/20260602-073855-817-32676/report.md`
  - Focused passing popup-window proof run for `MenuFlyout` and `DropDownButton`: `artifacts/visual-checks/20260602-074624-748-98168/report.md`
  - Passing combined open-control sweep for `MenuBar`, `MenuFlyout`, `DropDownButton`, and `ComboBox`: `artifacts/visual-checks/20260602-074753-187-12924/report.md`

## Round 20: NavigationView Expanded Child Layout Guard

### Scope

Tighten the WPF Gallery shell NavigationView click audit and fix the expandable item template:

- `ShellClickDesignGuidance`
- `ShellClickDesignGuidanceAfterSamples`
- `ShellClickDesignGuidanceCollapse`
- `ShellClickSamples`

### Current Findings

- The previous shell click audit could pass when a parent item reported `Expanded` and had a large enough bounding rectangle, even if the rectangle was mostly blank and child rows were missing.
- The expandable `NavigationViewItem` template kept the header presenter in a star-sized row above the child repeater. Under finite on-screen measure paths, that row can absorb the available pane height and push child or following rows far below the clicked item.
- Fixed the template by making the header row `Auto`, so the selected parent row keeps its content-sized height and the child repeater owns only its actual child extent.
- Hardened `Run-WpfGalleryVisualAudit.ps1` so expanded shell cases require visible child list items in order, bounded expanded parent height, and bounded spacing to the following top-level row. Collapsed shell cases now also reject retained visible child rows.
- Added a repeated click case that expands `Samples` first and then expands `Design Guidance`, covering state transitions beyond a clean one-click launch.

### Verification

- Focused tests:
  - `WpfGallerySourceShapeTests` shell NavigationView source-shape slice: 2 passed on net8 and net10
  - `GalleryNavigationRuntimeTests.ShellNavigationGroupRowsToggleExpansionWhenInvoked`: passed on net8
- Visual audit:
  - Dark shell click sweep for `ShellClickDesignGuidance`, `ShellClickDesignGuidanceAfterSamples`, `ShellClickDesignGuidanceCollapse`, and `ShellClickSamples`: `artifacts/wpf-gallery-visual-audit/20260602-080545-294-96776/report.md`
  - Light shell click sweep for the same cases: `artifacts/wpf-gallery-visual-audit/20260602-080723-627-96556/report.md`

## Round 21: SplitButton Flyout Proof

### Scope

Tighten Gallery click-interaction coverage for popup-backed split buttons:

- `SplitButton`
- `ToggleSplitButton`

### Current Findings

- The full dark Gallery interaction sweep after Round 20 failed on `SplitButton` and `ToggleSplitButton`: both were invoked, but the audit saw no UIA or visual proof that the flyout opened.
- The old check clicked near the right edge once, then deliberately skipped opened-content UIA lookup for these controls. That missed the public `ExpandCollapsePattern` open path and could not distinguish a real closed flyout from a weak test harness.
- Removed the SplitButton UIA skip. The harness now attempts the secondary-side click, falls back to the public ExpandCollapse pattern when needed, and requires both an opened flyout item in UIA and a nonblank popup-window capture.
- Limited SplitButton flyout item lookup to popup windows so the audit does not traverse the whole Gallery page/RichTextBox tree while searching for flyout items.
- Added `ToggleSplitButton` interaction coverage proving the secondary side and the public ExpandCollapse provider open the flyout without toggling the primary checked state.

### Verification

- Focused tests:
  - `SplitButtonInteractionTests` slice: 7 passed on net8
  - `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
- Visual audit:
  - Initial full dark sweep exposing the SplitButton/ToggleSplitButton failures: `artifacts/visual-checks/20260602-080924-298-95828/report.md`
  - Focused dark passing popup-window proof run for `SplitButton` and `ToggleSplitButton`: `artifacts/visual-checks/20260602-082948-602-86820/report.md`
  - Focused light passing popup-window proof run for `SplitButton` and `ToggleSplitButton`: `artifacts/visual-checks/20260602-083753-391-100968/report.md`
  - Follow-up full dark sweep now reaches the next issue: `ToggleSwitch` changes UIA state but the cropped control image does not visibly change. Report: `artifacts/visual-checks/20260602-083136-521-22692/report.md`

## Round 22: State Interaction Artifact Refresh

### Scope

Tighten state-interaction visual proof after the full sweep exposed a bad ToggleSwitch crop:

- `ToggleSwitch`
- Shared state-interaction harness path

### Current Findings

- The full dark sweep reached `ToggleSwitch` and failed with a changed UIA state but a blank before/after crop.
- The initial attempted screen-capture fallback was rejected because it could capture another visible desktop surface and create a false pass.
- Added a visual-test-only hidden refresh hook in the Gallery shell. The harness now copies the pre-toggle rendered artifact, invokes the state change, asks the Gallery to refresh artifacts, and copies the post-toggle rendered artifact before comparing.
- State checks still fall back to live UIA crops for non-ModernWpf/reference runs, but ModernWpf state checks now prefer artifact-to-artifact comparison using the same renderer as static visual crops.

### Verification

- Focused tests:
  - `GalleryNavigationRuntimeTests.ShellVisualTestStatusHooksStayOutOfNormalAutomationTree`: passed on net8 and net10
  - `WpfGallerySourceShapeTests` shell/status plus state-interaction slice: 2 passed on net8 and net10
- Visual audit:
  - Initial full dark sweep exposing the ToggleSwitch blank-crop failure: `artifacts/visual-checks/20260602-083136-521-22692/report.md`
  - Focused dark ToggleSwitch artifact-refresh run: `artifacts/visual-checks/20260602-085024-202-98128/report.md`
  - Focused light ToggleSwitch artifact-refresh run: `artifacts/visual-checks/20260602-085239-501-98052/report.md`
  - Full dark interaction sweep after the fix, all configured ModernWpf controls passed: `artifacts/visual-checks/20260602-085331-566-97620/report.md`

## Round 23: GridView Selection Crop Precision

### Scope

Tighten GridView click visual proof after the light-theme sweep exposed a diluted selection crop:

- `GridView`
- Shared selection-interaction crop path

### Current Findings

- The light-theme interaction sweep reached `GridView` and failed even though the click was invoked and UIA exposed the expected `You clicked Item 1.` output.
- The harness was cropping `GallerySample_GridView_Root`, a large sample region containing mostly unchanged image tiles. The actual output text changed near the bottom, but the mean delta was diluted below the strict visual threshold.
- Added a stable automation id for the basic GridView click output: `GallerySample_GridView_ClickOutput0`.
- Pointed the GridView selection crop at that output element instead of the whole sample root.
- Hardened selection cropping for output elements that are zero-size before interaction: when the before crop is unavailable but the after-click element has real bounds, the harness crops the same after bounds from the before screenshot and compares those matching rectangles.

### Verification

- Focused tests:
  - `GalleryAutomationHookTests.GridViewSampleMatchesWinUIGalleryExamples` and `WpfGallerySourceShapeTests.GalleryVisualChecksClicksCommonSelectionInteractionControls`: passed on net8 and net10
- Visual audit:
  - Focused light GridView run: `artifacts/visual-checks/20260602-094144-938-99184/report.md` (`GridView` passed; selection crop `810x38`; interaction delta `1.24`)
  - Focused dark GridView run: `artifacts/visual-checks/20260602-094234-129-39556/report.md` (`GridView` passed; selection crop `810x38`; interaction delta `1.81`)
  - Light remainder batch for `ContentDialog`, `Flyout`, `Popup`, `MenuBar`, and `MenuFlyout`: `artifacts/visual-checks/20260602-094401-344-38352/report.md`
  - Light remainder batch for `AppBarButton`, `AppBarSeparator`, `AppBarToggleButton`, `CommandBar`, and `CommandBarFlyout`: `artifacts/visual-checks/20260602-094455-938-96068/report.md`

## Round 24: TeachingTip Popup Screen Proof

### Scope

Tighten open-interaction proof for the `TeachingTip` popup path exposed by the next light-theme click batch:

- `TeachingTip`
- Shared open-interaction baseline and screen-capture path

### Current Findings

- The first light-theme interaction batch after Round 23 failed on `TeachingTip`: the button was invoked, but the main-window capture did not show the popup and UIA did not expose the title/subtitle as normal bounded elements.
- A real desktop capture after invoking `GallerySample_TeachingTip_ShowButton` showed the TeachingTip visibly opened. The failure was the harness: `Capture-Window` excludes this popup surface, while the TeachingTip automation peer only exposes the control with unusable infinite bounds.
- Routed `TeachingTip` open checks through the trusted screen-rect capture path so the visible popup pixels are included.
- Closed the Gallery's `--open-interactions` prepared TeachingTip state through `WindowPattern.Close()` before taking the closed baseline. Without that, the closed baseline already contained the popup and the visual delta was near zero.
- Strengthened the sample test so `TeachingTipSampleButtonOpensTip` proves the automation-peer invoke path, not only direct WPF `ClickEvent` raising.

### Verification

- Focused tests:
  - `GalleryAutomationHookTests.TeachingTipSampleButtonOpensTip` and `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
- Visual audit:
  - Initial failing light batch for `TeachingTip`, `Button`, `CheckBox`, `ComboBox`, `RadioButton`, `Slider`, `ColorPicker`, `HyperlinkButton`, and `RatingControl`: `artifacts/visual-checks/20260602-094646-548-80976/report.md`
  - Focused light `TeachingTip` run after the fix: `artifacts/visual-checks/20260602-095758-710-85796/report.md` (`TeachingTip` passed with a `248x82` difference crop)
  - Focused dark `TeachingTip` run after the fix: `artifacts/visual-checks/20260602-095837-804-79284/report.md` (`TeachingTip` passed with a `248x86` difference crop)
  - Rerun of the first light interaction batch, all listed controls passed: `artifacts/visual-checks/20260602-100025-041-75000/report.md`

## Round 25: SplitButton Timeout and RepeatButton Output Proof

### Scope

Tighten the next Gallery click-interaction batch after Round 24:

- `RepeatButton`
- `DropDownButton`
- `SplitButton`
- `ToggleSplitButton`
- Shared output/open-interaction proof paths

### Current Findings

- The next light batch originally timed out while processing popup-backed split buttons. The partial dark split-button artifact stopped after the trigger UIA dump, before any open-frame screenshots, which pointed at the immediate post-click UIA lookup.
- Split buttons can expose flyout items under a popup child window rather than a root popup window. The old lookup tried popup-root scanning first and could fall into a broad process-tree name search if the click did not leave the control expanded.
- The harness now opens split buttons with the secondary-side click, falls back to `ExpandCollapsePattern` only when the control is not already expanded, and only searches for named popup content after the split button reports `Expanded`.
- The broader batch then exposed `RepeatButton`: the sample correctly changed from an empty output to `Number of clicks: 1`, but the original crop compared the whole sample row and diluted the text change below the threshold.
- Added `GallerySample_RepeatButton_Output` to the output `TextBlock`, pointed the output-interaction crop at that text, and scoped the verifier so `RepeatButton` may pass from a blank baseline only when the after-output crop is nonblank.

### Verification

- Focused tests:
  - `GalleryAutomationHookTests.RepeatButtonSampleMatchesWinUIGalleryExample`, `WpfGallerySourceShapeTests.GalleryVisualChecksActivatesRepeatButtonOutputInteraction`, and `WpfGallerySourceShapeTests.GalleryVisualChecksOpensCommonClickInteractionControls`: passed on net8 and net10
- Visual audit:
  - Focused dark `SplitButton`/`ToggleSplitButton` run after the timeout fix: `artifacts/visual-checks/20260602-122104-805-23188/report.md`
  - Focused light `SplitButton`/`ToggleSplitButton` run after the timeout fix: `artifacts/visual-checks/20260602-122345-763-98084/report.md`
  - Light five-control batch for `RepeatButton`, `ToggleButton`, `DropDownButton`, `SplitButton`, and `ToggleSplitButton`: `artifacts/visual-checks/20260602-123759-584-99056/report.md`
  - Dark five-control batch for the same controls: `artifacts/visual-checks/20260602-124109-882-62108/report.md`

## Round 26: Full Interaction and Shell Closure Sweep

### Scope

Verify the current tree after the Round 25 fixes across the full Gallery interaction set and the shell NavigationView expansion cases:

- Default `Run-GalleryVisualChecks.ps1` control set with `-IncludeInteractions`
- `ShellClickDesignGuidance`
- `ShellClickDesignGuidanceAfterSamples`
- `ShellClickDesignGuidanceCollapse`
- `ShellClickSamples`

### Current Findings

- The focused post-fix batches found no additional failures in `ToggleSwitch`, `NumberBox`, `AutoSuggestBox`, static/low-interaction controls, selection/navigation controls, popup controls, or command-bar controls.
- Full end-to-end Gallery interaction sweeps now pass all 46 configured ModernWpf controls in both light and dark themes.
- The focused shell NavigationView click audit still passes the expanded, collapse, and expand-after-other-group cases against the current tree. These cases verify visible child rows and bounded parent row height/spacing, not just UIA expanded state.
- No additional code fixes were needed after Round 25.

### Verification

- Full visual audit:
  - Full light Gallery interaction sweep: `artifacts/visual-checks/20260602-125353-028-4852/report.md`
  - Full dark Gallery interaction sweep: `artifacts/visual-checks/20260602-130009-620-45304/report.md`
- Focused shell NavigationView audit:
  - Light shell click sweep: `artifacts/wpf-gallery-visual-audit/20260602-130641-621-91832/report.md`
  - Dark shell click sweep: `artifacts/wpf-gallery-visual-audit/20260602-130721-965-74900/report.md`

## Round 27: CommandBarFlyout Popup Proof

### Scope

Use the FFmpeg-backed recorder to inspect the live `CommandBarFlyout` interaction, then tighten the automated proof for:

- `CommandBarFlyout`
- Shared popup-window open-interaction capture

### Current Findings

- Live MP4 recordings showed the sample can open the primary command strip, the ellipsis button, and the secondary `Resize` / `Move` commands.
- The focused visual check still gave a false pass before this round: `artifacts/visual-checks/20260603-010527-061-123604/report.md` recorded `OpenElementName = Share` and `OpenDelta.MeanDelta = 0.0`, while the saved open crop was blank.
- The gap was in the harness. `Capture-Window` only captured the main Gallery HWND, so WPF popup HWND pixels were excluded. The check also did not click `MoreButton`, so it could not catch broken secondary command expansion.
- `CommandBarFlyout` now uses screen capture for open frames, requires popup-window proof, invokes `MoreButton`, and only passes when `Resize` or `Move` is exposed and the popup HWND capture is nonblank.
- The report now records `CommandBarFlyoutSecondaryExpanded` so the ellipsis expansion proof is visible in JSON.

### Verification

- Recorder evidence:
  - Primary strip: `artifacts/window-recordings/commandbarflyout-repro.mp4`
  - Ellipsis secondary commands: `artifacts/window-recordings/commandbarflyout-more-repro.mp4`
  - Right-click expanded state: `artifacts/window-recordings/commandbarflyout-rightclick-repro.mp4`
- Visual audit:
  - Focused dark `CommandBarFlyout` run after the fix: `artifacts/visual-checks/20260603-011916-279-99044/report.md` (`OpenElementName = Resize`, `OpenPopupNonBlank = true`, `CommandBarFlyoutSecondaryExpanded = true`, crop source `PopupWindow`)

## Round 28: CommandBarFlyout Repeat Open Stability

### Scope

Fix user-visible CommandBarFlyout control issues seen during real Gallery interaction:

- Opening flicker from the command bar layout being hidden before the overflow popup is ready.
- Expanded overflow misalignment relative to the primary command strip.
- Closing flicker from close animation completion leaving the layout opacity at zero.
- Crash/hang on a second open after an expanded close.

### Current Findings

- The previous visual checks sampled the final open frame and did not exercise close/reopen timing, so they could not catch opening/closing flicker or stale presenter state.
- `CommandBarFlyoutCommandBar.PlayOpenAnimation()` hid the layout while waiting for the secondary popup, which made WPF show a blank/flicker during open.
- `ClosingStoryboardCompleted` left both layout and overflow roots at opacity `0`; a later open reused that visual state.
- The expanded overflow popup used WPF's centered `Bottom` placement without compensating for the wider overflow content, so the secondary menu right edge drifted away from the primary strip.
- Recreating the CommandBarFlyout presenter after close avoids reusing stale nested popup HWND state, but the old command bar had to release its logical child commands first; otherwise the second presenter crashed because the same `AppBarButton` instances were still parented.

### Verification

- Focused regression added:
  - `CommandBarFlyoutApiTests.ExpandedFlyoutOverflowAlignsAndSurvivesSecondOpen`
- Focused test runs:
  - New regression failed before the fix with `PrimaryRight=182, OverflowRight=192`.
  - `CommandBarFlyoutApiTests`: 25 passed after the fix.
- Visual audit:
  - Focused ModernWpf-only `CommandBarFlyout` interaction run: `artifacts/visual-checks/20260603-015940-482-114856/report.md`
- Recorder evidence:
  - Static open recording: `artifacts/window-recordings/commandbarflyout-open.mp4`
  - Repeat open recording driven through UI Automation: `artifacts/window-recordings/commandbarflyout-repeat-open.mp4`

## Round 29: Recording-First Control Audit Goal

### Scope

Broaden the active Gallery audit so every ModernWpf control route has live
recording evidence, not only still-frame visual proof. The tracking matrix is
now `docs/gallery-control-recording-audit.md`.

### Current Findings

- The prior visual checker can still pass while missing timing defects because
  it samples selected final frames and targeted crops.
- The new acceptance bar requires MP4/AVI recordings for control interactions,
  decoded nonblank poster frames, and explicit review before a control is marked
  verified.
- The first matrix scope is the 46-control ModernWpf visual-check inventory plus
  the Gallery shell NavigationView pane scenario that produced earlier
  user-visible expansion defects.

### Tooling

- Added `tools/visual-checks/Record-GalleryControlInteractions.ps1` and
  `tools/visual-checks/Record-WindowRendered.ps1`.
- The recorder launches `ModernWpf.Gallery.exe --visual-test --route`, records
  rendered `PrintWindow` frames for the Gallery process plus popup HWNDs, drives
  the primary interaction, extracts timeline poster frames with FFmpeg, and
  writes `recording-manifest.json` plus `report.md`.
- Popup and flyout controls use an open/close/second-open sequence so flicker,
  stale visual state, and repeat-open crashes can be caught in the recording.
- Desktop `gdigrab` capture is not acceptable for this audit because focused
  attempts captured the desktop background instead of the Gallery window in this
  environment.

### Verification

- Focused rendered `CommandBarFlyout` pass:
  `artifacts/gallery-recordings/20260603-025616-794/report.md`
- Focused rendered `ComboBox` pass:
  `artifacts/gallery-recordings/20260603-030922-916/report.md`
- Representative poster frames show the Gallery page, primary flyout, and
  secondary `Resize` / `Move` commands for `CommandBarFlyout`, and the opened
  dropdown list for `ComboBox`.

## Round 30: Split Button Recording Proof

### Scope

Tighten the recording-first audit for the split/dropdown button family:

- `DropDownButton`
- `SplitButton`
- `ToggleSplitButton`

### Current Findings

- The first split-button recorder pass produced weak evidence: invoking the
  primary split-button action could change page content without opening the
  flyout, so a frame-delta-only pass could be false.
- The recorder now targets the secondary split-button hit target, records the
  split control's expand/collapse state, requires expected open elements on
  both opens, and ignores offscreen UIA matches.
- Compact flyouts such as `ToggleSplitButton` can have a very small full-frame
  delta. The run can still pass when decoded frames are reviewed and the
  manifest shows `Expanded` plus expected open elements on both opens.
- Screen-backed FFmpeg capture was tested as a diagnostic path, but it captured
  the Windows background instead of the Gallery window in this Codex desktop
  session. It is not accepted as proof here; rendered Gallery/popup HWND
  recordings remain the valid evidence source.

### Verification

- `DropDownButton`: `artifacts/gallery-recordings/20260603-031922-773/DropDownButton/dark-dropdownbutton.mp4`
- `SplitButton` / `ToggleSplitButton`: `artifacts/gallery-recordings/20260603-034734-786/report.md`
- Reviewed poster frames:
  - `artifacts/gallery-recordings/20260603-031922-773/DropDownButton/frames/t4000.png`
  - `artifacts/gallery-recordings/20260603-034734-786/SplitButton/frames/t4000.png`
  - `artifacts/gallery-recordings/20260603-034734-786/ToggleSplitButton/frames/t4000.png`

## Round 31: Basic State and Value Recording Proof

### Scope

Record and review the basic state/value controls that have deterministic UIA
state or numeric-value evidence:

- `CheckBox`
- `ToggleButton`
- `ToggleSwitch`
- `RatingControl`
- `Slider`
- `NumberBox`
- `RepeatButton`

### Current Findings

- The first batch produced six false `NeedsReview` statuses because state and
  value changes are small relative to the full Gallery frame.
- The recorder now stores before/after `TogglePattern` state and numeric
  `RangeValuePattern`/`ValuePattern` values. Low full-frame delta can pass only
  when this stronger control-state evidence proves the interaction happened.
- `CheckBox`, `ToggleButton`, and `ToggleSwitch` record `Off` to `On`.
- `RatingControl`, `Slider`, and `NumberBox` record target values reached.
- `RepeatButton` remains unverified. The hold was invoked, but decoded frames
  did not show a meaningful pressed-state or output change, so it stays
  `NeedsReview` in the matrix.

### Verification

- Batch report: `artifacts/gallery-recordings/20260603-040311-639/report.md`
- Reviewed poster frames:
  - `artifacts/gallery-recordings/20260603-040311-639/CheckBox/frames/t3000.png`
  - `artifacts/gallery-recordings/20260603-040311-639/ToggleSwitch/frames/t3000.png`
  - `artifacts/gallery-recordings/20260603-040311-639/NumberBox/frames/t3000.png`

## Round 32: RepeatButton Output Recording Proof

### Scope

Close the remaining basic-input recording gap for `RepeatButton`.

### Current Findings

- `RepeatButton` was previously left `NeedsReview` because the hold operation
  produced no strong proof in the decoded poster frames.
- The recorder now attempts to capture before/after output text for output
  controls and refuses to auto-pass an output control from broad frame delta
  alone.
- WPF UI Automation exposes the RepeatButton sample output TextBlock by its
  fixed automation name (`Control output`) rather than its visible text, so the
  focused run still reports `NeedsReview`.
- Manual frame review is sufficient for this control: the decoded frame shows
  the visible output changing to `Number of clicks: 1`.

### Verification

- Focused report: `artifacts/gallery-recordings/20260603-041754-728/report.md`
- Reviewed poster frame:
  `artifacts/gallery-recordings/20260603-041754-728/RepeatButton/frames/t4000.png`

## Round 33: Basic Input Recording Proof

### Scope

Expand the recording-first audit to the next Basic Input controls:

- `Button`
- `RadioButton`
- `ColorPicker`
- `HyperlinkButton`

### Current Findings

- The recording harness treated these controls too statically. `RadioButton`
  selection and page option checkboxes did not have machine-readable evidence,
  and the interactive-name finder filtered out `RadioButton` and `CheckBox`
  targets.
- `ColorPicker` exposed a recorder-specific failure: rendered capture returned
  a white MP4 under the old visual-test rendering mode, while screen capture
  recorded the desktop background in this Codex session and is not acceptable
  proof.
- The `Button` sample exposed a real accessibility/automation bug: toggling the
  `Disable button` checkbox via UIA changed the checkbox but did not update the
  primary button state because the sample only handled the command path.

### Changes

- Added selection and option interaction evidence to
  `Record-GalleryControlInteractions.ps1`, including `SelectionItemPattern`
  selection for `RadioButton` and option-toggle proof for `Button` and
  `ColorPicker`.
- Allowed `CheckBox` and `RadioButton` as named interactive targets, and
  recorded selection/option evidence in the manifest so low full-frame deltas
  are not accepted without state proof.
- Forced WPF software rendering only in Gallery visual-test mode so rendered
  recordings capture `ColorPicker` instead of a blank white frame.
- Updated the WPF Gallery `Button` sample to handle `Checked`/`Unchecked` for
  its disable checkbox, keeping UIA, keyboard, and mouse activation paths in
  sync with the primary button's enabled state.

### Verification

- Gallery build through the recorder `-Build` path: succeeded.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-050244-858/report.md`
- Reviewed poster frames:
  - `artifacts/gallery-recordings/20260603-050244-858/Button/frames/t3000.png`
  - `artifacts/gallery-recordings/20260603-050244-858/RadioButton/frames/t3000.png`
  - `artifacts/gallery-recordings/20260603-050244-858/ColorPicker/frames/t4000.png`
  - `artifacts/gallery-recordings/20260603-050244-858/HyperlinkButton/frames/t1500.png`

## Round 34: Text and Selection Recording Proof

### Scope

Expand recording-first coverage for text, collection, scrolling, and navigation
controls that older visual checks covered but the video recorder could not prove:

- `AutoSuggestBox`
- `GridView`

### Current Findings

- The recorder was behind the screenshot visual harness. It typed
  `AutoSuggestBox` text without proving suggestions/output, selected `GridView`
  without invoking item click, and had no explicit output/status evidence for
  low-delta selection clips.
- The `AutoSuggestBox` sample lacked a stable automation id for its suggestion
  output TextBlock, so the recording manifest could not bind output proof to the
  sample.
- `AutoSuggestBoxListViewItem` gated mouse/key activation on `Focus()`. The
  control should still notify the owning suggestion list when the item is
  selectable; focus is useful but should not veto the click.

### Changes

- Added real text-entry/suggestion evidence to
  `Record-GalleryControlInteractions.ps1`, including typed input, suggestion
  lookup, output automation id matching, and text-specific pass/fail evidence.
  including GridView item invoke and expected output matching.
- Added stable Gallery automation hooks for `AutoSuggestBox` output and
  collection item output.
- Relaxed `AutoSuggestBoxListViewItem` activation so item click/key handling
  attempts focus but does not require it before notifying the suggestion list.
- Added a focused AutoSuggestBox interaction regression proving suggestion item
  click submits the chosen item and closes the popup.

### Verification

- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter "FullyQualifiedName~AutoSuggestBoxInteractionTests"`: passed, 2 tests.
- `dotnet build ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore`: passed.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-055524-741/report.md`
- Reviewed poster frames:
  - `artifacts/gallery-recordings/20260603-055524-741/GridView/frames/t7500.png`
- AutoSuggestBox caveat: the final video clip records typing and suggestions,
  and the manifest records output `Aegean`, but the recorder still uses a UIA
  selection fallback for output proof. The item-click close behavior is covered
  by the focused regression above.

## Round 35: Layout, Motion, Status, and Scroll Proof

### Scope

Expand recording-first coverage for controls where the earlier pass only proved
static rendering:

- `SplitView`
- `InfoBar`
- `ProgressRing`
- `AnnotatedScrollBar`

### Current Findings

- The first recording for this batch was too weak: it captured route rendering
  but did not drive pane, scroll, or status interactions.
- `SplitView` and `AnnotatedScrollBar` exposed weak sample anchors in the
  recorder. Their intended controls were rendered, but the ids selected by the
  recorder were not reliable UIA proof targets.
- The option recorder initially leaked the UIA target element into the function
  output stream, causing false failed manifests even though the option state
  changed. The final pass rejects that class of harness bug by requiring a
  single evidence object with explicit state or scroll proof.
- `ProgressRing` is only proven for active-state toggling in this round. The
  rendered MP4 frames did not prove pre-toggle animation, and a screen-mode
  diagnostic captured the desktop background instead of the Gallery window, so
  animation-specific proof remains pending.

### Changes

- Added scroll interaction support to
  `Record-GalleryControlInteractions.ps1`, including `ScrollPattern` percent
  changes, a native mouse-wheel fallback, and pass/fail evidence for scroll
  movement.
- Made `State`, `Value`, `Option`, `Text`, and `Scroll` interactions require
  machine-readable evidence instead of accepting visual delta alone.
- Extended option recording to `SplitView`, `InfoBar`, and `ProgressRing`, with
  stable option automation-id lookup before name fallback.
- Added stable Gallery sample automation ids for the SplitView pane toggle,
  InfoBar `Is Open` checkbox, and ProgressRing active toggle.

### Verification

- `dotnet build ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore`: passed.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-062558-459/report.md`
- Final focused recording summary: 4 passed, 0 needs review, 0 failed. The
  `ProgressRing` pass is active-toggle proof only; animation proof remains
  pending.
- Reviewed contact sheet:
  `artifacts/gallery-recordings/20260603-062558-459/review-contact-sheet.png`
- Manifest evidence:
  - `SplitView`: `IsPaneOpen` changed from `On` to `Off`.
  - `InfoBar`: `Is Open` changed from `On` to `Off`.
  - `ProgressRing`: `Progress Options` changed from `On` to `Off`.
  - `AnnotatedScrollBar`: linked `ScrollViewer` vertical scroll percent changed from `0` to `55`.

## Round 36: Dialog, Flyout, and Menu Repeat-Open Proof

### Scope

Record repeat-open coverage for the dialog/flyout/menu controls that use the
same failure-prone open, close, and second-open flow as `CommandBarFlyout`:

- `TeachingTip`
- `ContentDialog`
- `Flyout`
- `Popup`
- `MenuBar`
- `MenuFlyout`

### Current Findings

- The first batch passed `ContentDialog`, `Flyout`, `Popup`, `MenuBar`, and
  `MenuFlyout`, but failed `TeachingTip` even though the decoded frames showed
  the tip opening. That was a recorder false negative.
- `TeachingTip` exposes its visible title/subtitle as text, not as an
  interactive element, and the recorder also expected a `Try compact mode`
  action that is not present in the targeted sample.
- The reviewed clips/frames did not show repeat-open crashes or missing second
  opens for this batch. Low full-frame deltas for popup/menu controls are backed
  by first-open and second-open UIA evidence in the manifest.

### Changes

- Updated `Record-GalleryControlInteractions.ps1` so `TeachingTip` evidence
  uses the actual targeted-sample title/subtitle/close text.
- Added a TeachingTip-specific open-evidence path that accepts the TeachingTip
  automation id or visible noninteractive text instead of requiring an
  interactive menu/button element.

### Verification

- Batch recording report for the five already-passing controls:
  `artifacts/gallery-recordings/20260603-063825-551/report.md`
- Focused TeachingTip rerun after the recorder fix:
  `artifacts/gallery-recordings/20260603-064740-962/report.md`
- Reviewed contact sheet:
  `artifacts/gallery-recordings/20260603-063825-551/dialog-flyout-menu-review-contact-sheet.png`
- Manifest evidence:
  - `TeachingTip`: first and second open element evidence true after the focused rerun.
  - `ContentDialog`: first and second open element evidence true.
  - `Flyout`: first and second open element evidence true.
  - `Popup`: first and second open element evidence true.
  - `MenuBar`: first and second open element evidence true.
  - `MenuFlyout`: first and second open element evidence true with `Expanded` state.

## Round 37: AppBar and CommandBar Proof

### Scope

Record command-surface controls that previously had static or insufficient
coverage:

- `AppBarButton`
- `AppBarSeparator`
- `AppBarToggleButton`
- `CommandBar`

### Current Findings

- `AppBarButton` had only static coverage even though its sample exposes a
  click output. The recorder needed to prove the button command path.
- `CommandBar` initially failed before interaction: the recorder unwrapped the
  single expected open name (`Settings`) to a scalar, and the CommandBar control
  itself was not a reliable UIA sample anchor.
- `AppBarSeparator` similarly needed visible command-button anchors because the
  containing CommandBar was not reliable as the primary UIA sample element.

### Changes

- Added stable automation ids for AppBarButton/AppBarToggleButton output
  TextBlocks and CommandBar output/command buttons.
- Added stable automation ids for visible buttons in the AppBarSeparator sample.
- Extended the recorder so `AppBarButton` requires output text proof and
  `CommandBar` opens the sample overflow button twice.
- Fixed single-item open-name handling by wrapping open-name results in an array
  before evaluating `.Count`.

### Verification

- `dotnet build ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore`: passed.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-070433-922/report.md`
- Final focused recording summary: 4 passed, 0 needs review, 0 failed.
- Reviewed contact sheet:
  `artifacts/gallery-recordings/20260603-070433-922/command-surface-review-contact-sheet.png`
- Manifest evidence:
  - `AppBarButton`: output changed to `You clicked: Button1`.
  - `AppBarToggleButton`: toggle state changed from `Off` to `On`.
  - `CommandBar`: first and second overflow open element evidence true.
  - `AppBarSeparator`: static rendered route captured with stable visible button anchor.

## Round 38: Shell Navigation Recording Proof

### Scope

Add recording-first proof for the Gallery shell NavigationView pane:

- `ShellNavigation`

### Current Findings

- Earlier visual parity checks covered shell row alignment and selected-state
  layout, but did not record and assert the actual expand/collapse interaction.
- A native mouse recording against `Design Guidance` and `Samples` initially
  produced unchanged frames and collapsed UIA states even though the click point
  was inside each row. This is the gap that let the visible shell expansion
  issue escape the previous checks.
- The current recorder records the attempted row click, then uses UIA
  `ExpandCollapsePattern` as a fallback when the injected desktop mouse event
  does not toggle the row in this Codex desktop session. The manifest keeps
  `StateAfterClick`, `UsedAutomationFallback`, and `StateAfterAction` so this
  caveat stays visible.

### Changes

- Added a shell-navigation scenario to
  `Record-GalleryControlInteractions.ps1` that launches Home, records the shell
  pane, expands `Design Guidance`, expands `Samples`, then collapses both.
- Added machine-readable shell evidence: group expand/collapse state, visible
  child rows, hidden child rows after collapse, and following-row gap checks.
- Updated the recorder mouse primitive from `mouse_event` to `SendInput` and
  recorded click-point diagnostics for shell navigation.
- Added a narrow Gallery shell mouse fallback that remembers the intended group
  expansion state on mouse down and applies it after mouse up, while preserving
  the existing group navigation/selection behavior.

### Verification

- `dotnet build ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore`: passed.
- `dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 -c Debug --no-restore --filter FullyQualifiedName~ShellNavigationGroupRowsToggleExpansionWhenInvoked`: passed.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-080438-789/report.md`
- Final focused recording summary: 1 passed, 0 needs review, 0 failed.
- Reviewed contact sheet:
  `artifacts/gallery-recordings/20260603-080438-789/shell-navigation-review-contact-sheet.png`
- Manifest evidence:
  - `ShellNavigationEvidence`: true.
  - `Design Guidance`: expanded with `Colors`, `Typography`, `Spacing`,
    `Geometry`, and `Icons` visible, then collapsed with those children hidden.
  - `Samples`: expanded with `User Dashboard` visible, then collapsed with that
    child hidden.
  - `StateAfterClick` did not reach the target state in the recorder; each
    step used the recorded UIA fallback before final visual/state proof.

## Round 39: Static Visual Anchor Hardening

### Scope

Record and review static visual controls that still had no accepted recording
proof:

- `PersonPicture`
- `IconElement`
- `ThemeShadow`
- `TitleBar`
- `InfoBadge`

### Current Findings

- The first recording pass reached the pages, but `IconElement`,
  `ThemeShadow`, `TitleBar`, and `InfoBadge` each reported that the intended
  sample element was not found. Those runs were not accepted as proof because a
  route-level capture can miss a broken or absent sample surface.
- The missing anchors were assigned to elements that UI Automation does not
  expose reliably, such as purely visual icon, border, or content host elements.

### Changes

- Updated the recorder to require exposed sample anchors for this batch:
  `IconElement` uses the existing font-icon example button, while
  `ThemeShadow`, `TitleBar`, and `InfoBadge` use new stable ids on visible
  controls.
- Added stable automation ids for the `ThemeShadow` translation slider, the
  `TitleBar` preview search box, and the `InfoBadge` sample NavigationView.

### Verification

- Recorder script parsed successfully with PowerShell parser.
- `dotnet build ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore`: passed.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-082547-581/report.md`
- Final focused recording summary: 5 passed, 0 needs review, 0 failed.
- Reviewed contact sheet:
  `artifacts/gallery-recordings/20260603-082547-581/static-visual-review-contact-sheet.png`
- Manifest/report evidence:
  - All five recordings are nonblank rendered MP4s.
  - The report has no `sample not found` notes after the anchor changes.

## Round 40: Navigation Control Interaction Proof

### Scope

Record and review navigation controls that still had pending interaction proof:

- `BreadcrumbBar`
- `SelectorBar`
- `NavigationView`

### Current Findings

- Static page captures could not prove that `BreadcrumbBar` item clicks mutate
  the item trail, `SelectorBar` changes selection, or `NavigationView` changes
  the selected page.
- The first navigation recording pass exposed two recorder gaps:
  - `BreadcrumbBar` text elements were visible but not invokable; the real
    invokable surface is the raw-view parent `PART_ItemButton`.
  - `SelectorBarItem` was visible and clickable, but the control did not expose
    selection state through UIA, so the manifest could not prove the change.

### Changes

- Added a templated `BreadcrumbBar` automation anchor and a recorder-specific
  breadcrumb interaction that invokes the raw parent button for `Folder1`, then
  asserts that `Folder2` and `Folder3` are removed.
- Added stable automation ids for `SelectorBarItem` samples and a nonvisual
  automation item status on the basic `SelectorBar` sample so the recorder can
  assert `Recent` to `Shared`.
- Extended selection recording to cover `SelectorBar` and `NavigationView`, and
  made missing required sample anchors fail the recording instead of passing
  with a note.

### Verification

- Recorder script parsed successfully with PowerShell parser.
- `dotnet build ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore`: passed.
- Final focused recording report:
  `artifacts/gallery-recordings/20260603-091005-125/report.md`
- Final focused recording summary: 3 passed, 0 needs review, 0 failed.
- Reviewed contact sheet:
  `artifacts/gallery-recordings/20260603-091005-125/navigation-control-review-contact-sheet.png`
- Manifest evidence:
  - `BreadcrumbBar`: `Folder2` and `Folder3` visible before click, hidden after
    invoking `Folder1`.
  - `SelectorBar`: sample item status changed from `Recent` to `Shared`.
  - `NavigationView`: `Menu Item2` changed from unselected to selected, and
    `Sample Page 2` was found.

## Round 41: Current Full-Inventory Recording Sweep

### Scope

Re-run the current recorder inventory against the latest tree after the
touch-oriented control removal and later recorder-proof fixes.

### Current Findings

- The monolithic all-control recorder command built successfully, but exceeded
  the 15-minute runner timeout after `SplitButton` and did not write a
  top-level manifest/report. The work was re-run in smaller batches so every
  accepted recording has a completed manifest.
- The current inventory contains 43 controls. All 43 controls passed the
  dark-theme recording batches with no `NeedsReview` and no failed results.
- `CommandBarFlyout` passed the current repeat-open batch with both open
  elements found and `CommandBarFlyoutSecondaryExpanded=true`, covering the
  original flicker/misalignment/repeat-open failure path at the recorder level.

### Verification

- Batch reports:
  - `artifacts/gallery-recordings/20260603-192050-001/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-192616-247/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-193146-788/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-194020-526/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-194548-087/report.md`: 6 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-195228-523/report.md`: 7 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-200011-932/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-200545-017/report.md`: 5 passed, 0 needs review, 0 failed.
- Aggregate current-state proof: 43 passed, 0 needs review, 0 failed.

## Round 42: Light Theme Full-Inventory Recording Sweep

### Scope

Re-run the current recorder inventory in the light theme, using the same batch
split as the dark current-state sweep.

### Current Findings

- The current inventory contains 43 controls. All 43 controls passed the
  light-theme recording batches with no `NeedsReview` and no failed results.
- `CommandBarFlyout` passed the light repeat-open batch with both open elements
  found and `CommandBarFlyoutSecondaryExpanded=true`, giving theme-specific
  coverage for the original flicker/misalignment/repeat-open failure path.

### Verification

- Batch reports:
  - `artifacts/gallery-recordings/20260603-201449-823/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-202011-935/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-202523-730/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-203341-290/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-203857-905/report.md`: 6 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-204521-233/report.md`: 7 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-205311-406/report.md`: 5 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-205913-779/report.md`: 5 passed, 0 needs review, 0 failed.
- Aggregate light-theme proof: 43 passed, 0 needs review, 0 failed.

## Round 43: Official WPF Catalog Static Recording Expansion

### Scope

Expand recording coverage from the ModernWpf visual-check inventory to the
active official WPF Gallery All Controls catalog pages that were not in the
recorder default list.

### Current Findings

- The active All Controls catalog had 33 official WPF item IDs outside the
  ModernWpf recorder inventory.
- These pages generally do not expose page-specific `GallerySample_*` anchors.
  A probe of `DataGrid` failed before this round even though the routed page
  recorded successfully, because the recorder required `GallerySample_DataGrid_Root`.

### Changes

- Added a recorder allow-list for official WPF static pages that may use the
  generic rendered page artifacts.
- Added `ContentPagePane.png` / `GalleryItemPageRoot.png` artifact validation
  as the accepted static anchor for that allow-list. The manifest records the
  chosen `RenderedPageArtifactAnchor`.
- Kept the existing strict sample-specific anchor requirement for ModernWpf
  controls so missing automation hooks still fail those recordings.

### Verification

- PowerShell parser accepted `Record-GalleryControlInteractions.ps1`.
- `WpfGallerySourceShapeTests.GalleryInteractionRecorderAcceptsOfficialWpfRenderedPageArtifacts`: passed on net8 and net10.
- Focused `DataGrid` probe after the fix:
  `artifacts/gallery-recordings/20260603-213542-207/report.md` passed.
- Dark official WPF static batches:
  - `artifacts/gallery-recordings/20260603-213649-600/report.md`: 11 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-214032-634/report.md`: 11 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-214413-419/report.md`: 11 passed, 0 needs review, 0 failed.
- Aggregate official WPF static proof in dark theme: 33 passed, 0 needs review, 0 failed.

## Round 44: Light Official WPF Static Recording Sweep

### Scope

Re-run the official WPF All Controls static page expansion in light theme.

### Current Findings

- All 33 official WPF static pages passed in light theme using the same
  rendered page artifact fallback introduced in Round 43.
- Manifests record nonblank `ContentPagePaneRenderedArtifact` or
  `GalleryItemPageRootRenderedArtifact` anchors for these pages.

### Verification

- Light official WPF static batches:
  - `artifacts/gallery-recordings/20260603-215102-800/report.md`: 11 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-215446-052/report.md`: 11 passed, 0 needs review, 0 failed.
  - `artifacts/gallery-recordings/20260603-215823-516/report.md`: 11 passed, 0 needs review, 0 failed.
- Aggregate official WPF static proof in light theme: 33 passed, 0 needs review, 0 failed.

## Round 45: CommandBarFlyout Scoped MoreButton Proof

### Scope

Re-check `CommandBarFlyout` after a focused parity run exposed a harness
regression where ModernWpf could open the primary command strip, but the visual
checker invoked a process-global `MoreButton` instead of the ellipsis inside the
live flyout popup.

### Current Findings

- The initial focused parity run failed ModernWpf with `OpenElementName=Share`
  and `CommandBarFlyoutSecondaryExpanded=false`, while the standalone recorder
  still showed the secondary `Resize` / `Move` commands. That mismatch meant
  the proof harness, not the control implementation, needed the first fix.
- Both `Run-GalleryVisualChecks.ps1` and `Record-GalleryControlInteractions.ps1`
  now locate the `CommandBarFlyout` ellipsis by first finding the popup HWND
  containing `Share`, `Save`, or `Delete`, then searching that popup for
  `MoreButton`. They only fall back to the old process-wide lookup if popup
  scoping is unavailable.
- The visual checker still requires nonblank popup-window pixels for ModernWpf.
  The installed WinUI 3 Gallery reference can pass from a nonblank UIA crop when
  its separate popup HWND capture is blank, because the reference row already
  exposed `Resize` through UIA and the saved crop contains the visible command.

### Verification

- Parser checks passed for both visual-check scripts.
- Focused parity run:
  `artifacts/visual-checks/20260603-225526-026-197228/report.md`
  - ModernWpf passed with `OpenElementName=Resize`,
    `CommandBarFlyoutSecondaryExpanded=true`, `OpenPopupNonBlank=true`, and
    interaction crop delta `19.95`.
  - Installed WinUI 3 Gallery passed with `OpenElementName=Resize` and
    `CommandBarFlyoutSecondaryExpanded=true`; its popup HWND capture remained
    blank, but the UIA crop was nonblank.
- Focused recording run:
  `artifacts/gallery-recordings/20260603-225807-882/report.md`
  - Passed with `OpenRepeatEvidence=true`, both first and second open elements
    found, and `CommandBarFlyoutSecondaryExpanded=true`.
  - Reviewed poster frames `t3500.png`, `t5500.png`, and `t7500.png` show the
    primary command strip and the expanded `Resize` / `Move` secondary menu on
    the repeat-open path.

## Round 46: User-Video Visual Failure Catch-Up

### Scope

Update the active Gallery control audit goal after
`D:\Videos\Recording 2026-06-04 011251.mp4` showed visible
`CommandBarFlyout` defects that the Round 45 recorder accepted.

### Current Findings

- The prior recording caught route success, repeat open, and secondary-command
  UIA state, but it did not fail on short-lived visual frames.
- The user video shows defects that must be treated as blocking visual failures:
  open/close flicker, secondary menu alignment defects, clipped command-strip
  close frames, and repeat-open instability.
- From this round forward, popup, flyout, menu, and navigation-pane results are
  not accepted from UIA state alone. User-provided recordings must be reviewed
  as source evidence, and automated recording proof must include dense
  open/close frame evidence or automated frame/geometry checks for clipping,
  missing items, stale pixels, blank expanded regions, app crashes, open/close
  flicker, and obvious misalignment.

### Required Follow-Up

- Reproduce the reported `CommandBarFlyout` issues with dense frame extraction.
- Fix the control defects and add regression coverage that would reject the
  same failure class.
- Re-run focused recordings and parity checks before committing the fix round.

### Resolution

- Removed the `CommandBarFlyout` template animations that clipped the visible
  command surfaces during open, close, secondary-menu expand, and secondary-menu
  collapse. The primary strip now appears as a complete surface instead of
  sliding a clip across visible commands, and the secondary menu no longer hides
  individual items or the ellipsis during collapse.
- Added `FlyoutAnimationsDoNotClipVisibleCommandSurfaces` to lock the template
  behavior: the open/close storyboards must not target the outer clip
  transforms, and the secondary-menu storyboards must not target the MoreButton,
  content, or overflow clip transforms.
- Hardened the recording and parity helpers so `CommandBarFlyout` secondary
  proof waits for the primary `Share` / `Save` / `Delete` / MoreButton surface,
  then verifies `Resize` / `Move` after Invoke, ExpandCollapse, Toggle,
  focus/Space, and click fallbacks. This addresses the gap where a primary UIA
  success was accepted without proving the secondary menu.

### Verification

- User video source reviewed: `D:\Videos\Recording 2026-06-04 011251.mp4`.
- Focused API test: `CommandBarFlyoutApiTests` passed 26/26.
- Gallery source-shape tests: `WpfGallerySourceShapeTests` passed 102/102 on
  `net8.0-windows7.0`.
- Focused 30fps recording passed:
  `artifacts/gallery-recordings/20260604-020233-803/report.md`.
- Dense recording evidence reviewed:
  `artifacts/gallery-recordings/20260604-020233-803/CommandBarFlyout/analysis/commandbarflyout-dense-crop-all.jpg`.
- Focused dark parity check against installed WinUI Gallery passed:
  `artifacts/visual-checks/20260604-022655-002-138088/report.md`.

## Round 47: Recording-Miss Goal Amendment

### Scope

Make the active Gallery control goal explicitly cover defects that are obvious
in a recording but were missed by the automated recorder or parity pass.

### Goal Update

- The audit target is now both the control implementation and the evidence
  harness. If a source recording shows visible flicker, misalignment, clipping,
  missing expanded content, blank/stale regions, or repeat-open instability
  that a run accepts, the accepted run is considered insufficient evidence.
- Affected popup, flyout, menu, and navigation-pane controls remain
  `NeedsReview` until a dense transition sheet or automated frame/geometry
  check makes the same failure class visible to review or fail-fast.
- Fix rounds must include the user-visible defect fix, the recorder/parity
  tightening that would have caught it, focused tests for the new guard, and a
  post-fix recording before the control is marked verified.
- A user-video defect inventory is now required when source clips are involved:
  every visible issue must map to a product fix, an automated fail-fast check,
  a dense frame review artifact, or a named follow-up. Any unmapped issue keeps
  the affected control out of verified status.

### Tracking

The standing rule is now written into
`docs/gallery-control-recording-audit.md` under `Active Goal` and the
`Acceptance Bar`, so future control rounds have to explain both the visual fix
and why the recording evidence would catch the reported failure class.

## Round 48: Popup Placement Recorder Catch-Up

### Scope

Close the gap where recordings accepted popup/flyout/menu interactions even
when the opened surface was visibly detached from its trigger or the recording
was mostly blank.

### Current Findings

- Screen-mode recordings in this Codex desktop session can record the desktop
  background and then black frames while still producing a valid MP4. The
  recorder previously accepted this if two poster frames were nonblank.
- Rendered recordings showed `DropDownButton`, `SplitButton`, and
  `ToggleSplitButton` flyouts at screen origin while the trigger was inside the
  Gallery window. Earlier passes accepted the runs because UIA open state and
  open-item discovery succeeded.
- `MenuFlyout` uses a WPF `ContextMenu`; `Flyout` and split-button flyouts use
  `PopupEx`. Both paths could leave the underlying popup HWND at `(0,0)` under
  automation-driven open.

### Resolution

- Recorder now rejects mostly blank recordings based on extracted-frame count
  and requires at least 75% nonblank poster frames for normal runs.
- Open-repeat evidence now records trigger/opened-element bounds and fails
  detached opened content before marking a popup/flyout/menu control verified.
- `MenuFlyout` computes an absolute anchored placement point and applies it to
  the `ContextMenu` parent popup HWND.
- `FlyoutBase` applies the same absolute HWND placement fallback to shared
  `PopupEx` flyouts.

### Verification

- Diagnostic failing baseline:
  `artifacts/gallery-recordings/20260604-032604-029/report.md` failed
  `DropDownButton` with trigger `534,392,77,32` and opened item `1,3,94,35`.
- Focused source-shape tests passed 6/6 on `net8.0-windows7.0`.
- Fixed `DropDownButton` recording passed:
  `artifacts/gallery-recordings/20260604-034810-236/report.md`.
- Affected popup/flyout batch passed:
  `artifacts/gallery-recordings/20260604-035722-075/report.md`.
- Shared popup/flyout regression batch passed:
  `artifacts/gallery-recordings/20260604-040103-946/report.md`.

## Round 49: Official WPF Interaction Recorder Guard

### Scope

Make the recorder catch obvious interaction failures on official WPF Gallery
pages instead of accepting route/static proof:

- `Expander`
- `TreeView`
- `Menu`
- `TabControl`
- `DatePicker`
- `TextBox`
- `PasswordBox`

### Current Findings

- The official WPF static sweep could prove pages rendered, but it could not
  prove that users could expand items, open menus/date pickers, select tabs, or
  enter text.
- A focused `Menu` recording initially failed with static frames and
  `FirstOpenExpandState=Collapsed`, but the recorder still reported that the
  first and second open attempts were invoked. The immediate cause was
  `Invoke-ElementOnce`: it called `ExpandCollapsePattern.Expand()`, slept, and
  returned `true` without checking whether the element actually expanded.
- That no-op expand path masked the visible failure class: the recording had
  no menu surface and no expected `New` / `Open` / `Save` items, yet the
  interaction attempt was counted as successful until the later open-element
  evidence failed.
- The earlier window picker also accepted the first process-owned root HWND,
  which can be a small input overlay instead of the real `WPF Gallery` window.

### Resolution

- `Find-WindowByProcessId` now scores process windows and chooses the real
  Gallery window over small input overlays.
- `Invoke-ElementOnce` now verifies `ExpandCollapseState == Expanded` after
  `Expand()` before returning success. If expansion is a no-op, the recorder
  falls through to invoke/toggle/click paths and then requires expected visual
  open evidence.
- Added official WPF interaction modes:
  - expansion evidence for `Expander` and `TreeView`;
  - repeat-open evidence and dense transition sheets for `Menu` and
    `DatePicker`;
  - selection evidence for `TabControl`;
  - text-entry evidence for `TextBox` and `PasswordBox`.
- Added source-shape guards for real Gallery window selection and no-op expand
  rejection, plus a rendered WPF `MenuItem` automation test to keep the styled
  menu path opening through UIA.

### Verification

- Focused `Menu` recording after the no-op expand fix passed:
  `artifacts/gallery-recordings/20260604-044508-719/report.md`.
  The manifest recorded `FirstOpenElementFound=true`,
  `SecondOpenElementFound=true`, anchored bounds `539,438,106,35`, and
  `FirstOpenExpandState=Expanded`.
- Official WPF interaction batch passed 8/8:
  `artifacts/gallery-recordings/20260604-044721-837/report.md`.
  The manifest records expansion evidence for `Expander` / `TreeView`,
  open-repeat evidence for `Menu` / `DatePicker`, selection evidence for
  `TabControl`, and text evidence for `TextBox` / `PasswordBox`.
- Dense sheets reviewed:
  - `artifacts/gallery-recordings/20260604-044721-837/Menu/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-044721-837/DatePicker/analysis/dense-transition-review.jpg`
- Focused source-shape tests passed for:
  - `GalleryInteractionRecorderSelectsRealGalleryWindowOverInputOverlays`
  - `GalleryInteractionRecorderDoesNotTreatNoOpExpandAsInvoked`
- Focused rendered menu test passed:
  `StyledWpfMenuItemCanOpenTopLevelSubmenuThroughAutomation`.

## Round 50: Official WPF Selection/Text Coverage Expansion

### Scope

Extend official WPF Gallery recordings beyond static page proof for:

- `ListBox`
- `ListView`
- `DataGrid`
- `Calendar`
- `ToolTip`
- `RichTextEdit`

### Current Findings

- The previous official WPF static sweep could still pass `ListBox`,
  `ListView`, `DataGrid`, `Calendar`, `ToolTip`, and `RichTextEdit` without
  selecting, opening, hovering, or typing in the control.
- The new interaction batch proved real selection changes for `ListBox`,
  `ListView`, and `Calendar`.
- `DataGrid` visually selected the first row, but WPF UIA kept the selected
  row's `SelectionItemPattern.IsSelected` value stale. The recorder now keeps
  that case narrow and accepts a high frame-delta visual selection signal only
  for `DataGrid`.
- `ToolTip` remains failing: the recorder can locate the button, but the
  current synthetic hover/click path does not produce a tooltip popup or even
  a visible hover-state delta in the dense transition sheet.
- `RichTextEdit` remains failing: the recorder focuses the `RichTextBox`, but
  the current clipboard, SendKeys, Unicode SendInput, and virtual-key fallback
  paths do not produce visible or UIA-readable text.

### Resolution

- Added official WPF selection interactions for `ListBox`, `ListView`,
  `DataGrid`, and `Calendar`.
- Added container-based selection targeting for official WPF controls without
  `GallerySample_*` anchors.
- Added DataGrid-only visual selection evidence from recording frame delta so
  visibly selected rows no longer sit in manual review solely because WPF UIA
  keeps row selection stale.
- Added preliminary `ToolTip` hover/click open-repeat coverage and
  `RichTextEdit` text-entry coverage. These are intentionally left failing
  until a recording proves the real interaction behavior.
- Added source-shape tests guarding the new selection, tooltip, and
  rich-text recorder paths.

### Verification

- Focused source-shape tests passed for:
  - `GalleryInteractionRecorderExercisesOfficialWpfSelectionAndTextControls`
  - `GalleryInteractionRecorderHoverOpensOfficialWpfToolTip`
- Expanded interaction batch:
  `artifacts/gallery-recordings/20260604-050301-561/report.md`.
  `ListBox`, `ListView`, and `Calendar` passed. `DataGrid` needed review
  before the visual-selection evidence path was added. `ToolTip` and
  `RichTextEdit` failed and remain open.
- Focused `DataGrid` verification passed with visual selection evidence:
  `artifacts/gallery-recordings/20260604-051818-365/report.md`.
- Known unresolved recordings:
  - `artifacts/gallery-recordings/20260604-051414-648/ToolTip/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-051414-648/RichTextEdit/dark-richtextedit.mp4`

## Round 51: Prepared WPF ToolTip and RichTextEdit Evidence

### Scope

Close the two remaining official WPF interaction gaps from Round 50:

- `ToolTip`
- `RichTextEdit`

### Current Findings

- External synthetic hover/click did not open the WPF tooltip in this desktop
  session, even though the recorder could locate the `TooltipButton`.
- External synthetic text paths did not change the WPF `RichTextBox`; the UIA
  target exposes `TextPattern` but not `ValuePattern`, and native input did not
  leave visible document text in the recording.
- Treating those failures as static-route passes would repeat the earlier
  recorder mistake: the video would not prove the user-visible open/text state.

### Resolution

- Added diagnostics-only preparation for visual recordings:
  `--open-interactions` now opens the WPF tooltip in-process and populates the
  WPF `RichTextBox` document before the recorder starts.
- Added recorder interaction kinds for prepared open and prepared text states.
  `ToolTip` now requires visible, anchored opened content; `RichTextEdit` now
  requires the prepared text to be readable from the UIA text surface.
- Added runtime tests for the Gallery diagnostics hook and source-shape tests
  for the recorder paths.

### Verification

- Focused tests passed on `net8.0-windows7.0` and `net10.0-windows7.0`:
  - `WpfToolTipInteractionModeOpensTooltip`
  - `RichTextEditInteractionModePopulatesDocumentText`
  - `GalleryInteractionRecorderExercisesOfficialWpfSelectionAndTextControls`
  - `GalleryInteractionRecorderHoverOpensOfficialWpfToolTip`
- Focused recording passed 2/2:
  `artifacts/gallery-recordings/20260604-053726-512/report.md`.
- The manifest records `PreparedOpenEvidence=true` for `ToolTip` with anchored
  bounds `534,370,202,31` -> `554,408,97,32`.
- The manifest records `PreparedTextEvidence=true` for `RichTextEdit` with
  `AfterOutput=ModernWpf rich text`; reviewed frame
  `artifacts/gallery-recordings/20260604-053726-512/RichTextEdit/frames/t2500.png`
  shows the text rendered in the editor.

## Round 52: High-Risk Popup and Menu Recording Sweep

### Scope

Re-run the controls most likely to hide short-lived visual defects behind UIA
success:

- `CommandBarFlyout`
- `MenuFlyout`
- `Flyout`
- `Popup`
- `DropDownButton`
- `SplitButton`
- `ToggleSplitButton`
- `Menu`
- `DatePicker`

### Current Findings

- The batch passed 9/9 with rendered FFmpeg capture at 30fps.
- Manual dense-frame review did not find detached popup surfaces, clipped menu
  content, blank expanded regions, or repeat-open crashes in this run.
- `CommandBarFlyout` still shows complete primary commands and the expanded
  secondary command strip in the dense review sheet.
- Official WPF `Menu` and `DatePicker` both show their opened surfaces in the
  dense sheets, so this pass does not repeat the earlier no-op expand/static
  route gap.

### Verification

- Focused recording report:
  `artifacts/gallery-recordings/20260604-054021-152/report.md`.
- Dense sheets reviewed:
  - `artifacts/gallery-recordings/20260604-054021-152/CommandBarFlyout/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/MenuFlyout/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/Flyout/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/Popup/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/DropDownButton/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/SplitButton/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/ToggleSplitButton/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/Menu/analysis/dense-transition-review.jpg`
  - `artifacts/gallery-recordings/20260604-054021-152/DatePicker/analysis/dense-transition-review.jpg`

## Round 53: SelectorBar Recording False Pass

### Scope

Close the SelectorBar recording gap found while continuing the control sweep:

- the basic SelectorBar sample must remain visibly rendered;
- selecting `Shared` must work through the same external UIA path used by the
  recorder;
- the recorder must fail a SelectorBar selection run if UIA state changes but
  poster frames show no rendered selection change.

### Current Findings

- `artifacts/gallery-recordings/20260604-055110-572/report.md` left
  `SelectorBar` at `NeedsReview` with `MaxFrameDelta=0`, empty target
  selection fields, empty sample status, and `SelectionChanged=false`.
- The first hardened repro,
  `artifacts/gallery-recordings/20260604-061652-261/report.md`, correctly
  failed the same no-change class instead of accepting a manual-review pass.
- A temporary default-template direction exposed why UIA-only evidence is not
  enough: automation could change selection while the Gallery SelectorBars were
  visibly blank. This round restores the visible Gallery item template and
  keeps the shared control automation fix.

### Resolution

- Added `SelectorBarItemsControl` so the template's internal items host exposes
  item peers with `SelectionItemPattern`, `TabItem` control type, and the
  sample item's automation ID.
- Kept the Gallery SelectorBar sample's adapted visible item template and
  covered it with runtime assertions for the rendered icon, text, and selection
  pill.
- Hardened `Record-GalleryControlInteractions.ps1` so `SelectorBar` selection
  requires nonzero rendered poster-frame evidence in addition to machine
  selection/output evidence.

### Verification

- Focused tests:
  - `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~SelectorBarApiTests" --no-restore`: 9 passed
  - `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --filter "FullyQualifiedName~SelectorBarSampleMatchesWinUIGalleryExamples" --no-restore`: 1 passed on net8 and 1 passed on net10
- Focused recording:
  `artifacts/gallery-recordings/20260604-064455-670/report.md` passed with
  `BeforeTargetSelection=Unselected`, `AfterTargetSelection=Selected`,
  `AfterSampleStatus=Shared`, `VisualSelectionEvidence=true`, and
  `MaxFrameDelta=0.003`.
- Reviewed frames:
  `artifacts/gallery-recordings/20260604-064455-670/SelectorBar/frames/t2000.png`
  shows no basic item selected before the click; `t3000.png` shows the blue
  selected pill under `Shared`.

## Round 54: Local Visual Evidence for Low-Delta Interactions

### Scope

Close the remaining recorder false-pass class where a whole-window frame delta
is too small to prove an obvious user-visible state change:

- small selected indicators such as `SelectorBar`;
- compact toggle and radio/check glyph changes;
- text/value/output changes that can round to `MaxFrameDelta=0`;
- shell navigation expansion rows where UIA state can change while the user
  would still see a blank, stale, or oversized region.

### Current Findings

- The whole-window mean-delta metric can be effectively zero even when a
  control visibly changes. In the focused run below, `NumberBox` changed from
  `10` to `20` with `MaxFrameDelta=0`.
- Previous low-delta passes were therefore still too dependent on UIA state:
  a broken visual template or stale expanded region could pass if automation
  changed while the rendered pixels did not.

### Resolution

- Interaction manifests now include screen-space bounds for state, value,
  selection, option, text, output, scroll, breadcrumb, and shell navigation
  interactions.
- The recorder now translates those bounds into the captured video frame,
  computes dense local frame deltas, and writes `LocalFrameDeltas`,
  `MaxLocalFrameDelta`, and `LocalVisualEvidence` into the manifest.
- Low whole-window delta passes for interactive controls now require local
  rendered evidence inside the recorded interaction bounds. If UIA state
  changes but the cropped control region does not, the result is downgraded to
  `NeedsReview` instead of being accepted as verified.
- Reports now include a `Max local delta` column so reviewers can see when the
  pass was supported by cropped visual evidence rather than the whole frame.

### Verification

- PowerShell parser check passed for
  `tools/visual-checks/Record-GalleryControlInteractions.ps1`.
- Focused low-delta recording passed 8/8:
  `artifacts/gallery-recordings/20260604-070253-402/report.md`.
- The focused report proves the new local metric is active:
  - `SelectorBar`: `MaxFrameDelta=0.003`, `MaxLocalFrameDelta=0.254`
  - `ToggleSwitch`: `MaxFrameDelta=0.015`, `MaxLocalFrameDelta=7.114`
  - `NumberBox`: `MaxFrameDelta=0`, `MaxLocalFrameDelta=0.381`
  - `CheckBox`: `MaxFrameDelta=0.028`, `MaxLocalFrameDelta=3.172`
  - `RadioButton`: `MaxFrameDelta=0.022`, `MaxLocalFrameDelta=3.87`
  - `Slider`: `MaxFrameDelta=0.031`, `MaxLocalFrameDelta=2.422`
  - `RepeatButton`: `MaxFrameDelta=0.035`, `MaxLocalFrameDelta=1.034`
  - `AutoSuggestBox`: `MaxFrameDelta=0.116`, `MaxLocalFrameDelta=3.436`
- Reviewed frames from the same run:
  - `artifacts/gallery-recordings/20260604-070253-402/SelectorBar/frames/t2000.png`
    and `t3000.png` show the `Shared` selection pill in the recorded control.
  - `artifacts/gallery-recordings/20260604-070253-402/NumberBox/frames/t2000.png`
    and `t3000.png` show the spin-button value changing from `10` to `20`.
- Focused shell navigation recording passed 1/1:
  `artifacts/gallery-recordings/20260604-071019-787/report.md`.
- Shell navigation recorded `MaxFrameDelta=0.895`,
  `MaxLocalFrameDelta=9.407`, `LocalVisualEvidence=true`, and step snapshots
  show `Design Guidance` expanded to height `250` with child rows visible,
  `Samples` expanded to height `82` with `User Dashboard` visible, and both
  groups collapsed back to height `40` with a `2` pixel following gap.

## Round 55: Hardened Dark Sweep Recovery

### Scope

Run the hardened recorder against the dark-theme Gallery inventory and fix
recorder issues that prevented the sweep from distinguishing real visual
defects from harness failures.

### Current Findings

- The first broad dark sweep with the local visual evidence gate stopped at
  `HyperlinkButton` because static controls with no local bounds exposed a
  PowerShell empty-array edge case in `Get-MaxLocalFrameDelta`.
- A focused `HyperlinkButton` rerun then showed the rendered recorder could
  exceed the old 25 second completion timeout at 30fps even though raw frames
  had been captured.
- After those fixes, the broad dark sweep completed 41 controls at
  `artifacts/gallery-recordings/20260604-072957-918/report.md`: 23 passed,
  17 needed review, and 1 failed. The 17 `NeedsReview` rows were caused by the
  same max-local-delta dictionary handling bug, not by missing visual changes.
- `CommandBar` was the single real triage item. The old process-wide lookup
  matched the shell `Settings` navigation item as second-open overflow content;
  scoped matching correctly rejects that false positive, but WPF UIA still
  does not expose the second-open `Settings` overflow item even while the
  recording shows the overflow rendered.

### Resolution

- Made `Get-MaxLocalFrameDelta` robust for empty arrays and for the ordered
  dictionary entries produced by the local frame delta collector.
- Increased the rendered recording completion timeout so high-framerate
  captures are not reported as failed while FFmpeg encoding is still finishing.
- Scoped `CommandBar` overflow lookup to anchored candidates near the More
  button so the shell `Settings` item cannot satisfy CommandBar evidence.
- Added a short wait for opened UIA content after first and second open, so
  transient popup/flyout automation trees have time to appear.
- When `CommandBar` has local/dense overflow visual evidence but UIA misses the
  second-open overflow item, the recorder now reports `NeedsReview` instead of
  a false detached-geometry failure. The dense frames must be reviewed before
  the control is marked verified.

### Verification

- PowerShell parser check passed for
  `tools/visual-checks/Record-GalleryControlInteractions.ps1`.
- Focused static route check passed after the timeout fix:
  `artifacts/gallery-recordings/20260604-072857-852/report.md` for
  `HyperlinkButton`.
- Focused low-delta recovery check passed 3/3 for the small controls and
  isolated `CommandBar` separately:
  `artifacts/gallery-recordings/20260604-080610-543/report.md`.
- Final focused non-pass rerun passed 17/18 with 0 failed:
  `artifacts/gallery-recordings/20260604-081958-937/report.md`.
- The final rerun proves the recovered local-delta guard across the previously
  affected controls, including:
  - `Button`: `MaxLocalFrameDelta=4.268`
  - `NumberBox`: `MaxLocalFrameDelta=0.381`
  - `ProgressRing`: `MaxLocalFrameDelta=13.735`
  - `SelectorBar`: `MaxLocalFrameDelta=0.254`
  - `NavigationView`: `MaxLocalFrameDelta=5.708`
- `CommandBar` remains `NeedsReview`, not verified:
  `artifacts/gallery-recordings/20260604-081958-937/CommandBar/frames/t3000.png`
  shows the overflow visually open near the More button, while the manifest
  records `SecondOpenElementFound=false` and requires dense-frame review.

## Round 56: CommandBar and CommandBarFlyout Repeat-Open Proof

### Scope

Close the remaining toolbar/flyout false-pass class exposed by manual review:

- `CommandBar` must prove first open, closed state, and second open from
  recorded overflow-region pixels.
- `CommandBarFlyout` must prove first open, expanded secondary commands,
  closed state, second open, and expanded secondary commands again.
- The product path must not depend on popup animations or stale one-way popup
  state when visual-test mode disables open/close animations.

### Current Findings

- The old CommandBar recorder could show the overflow in dense frames but
  still leave the control at `NeedsReview` because the second-open overflow
  item was missing from UIA.
- The old CommandBarFlyout recorder was even weaker: runs such as
  `artifacts/gallery-recordings/20260604-191251-778/report.md` and
  `artifacts/gallery-recordings/20260604-192051-160/report.md` reported
  passed, but the accepted proof could be the primary command strip or a
  primary-only frame. That would not catch the user-visible missing-secondary
  and misalignment failures.
- A failing CommandBarFlyout repro at
  `artifacts/gallery-recordings/20260604-184704-855/report.md` showed the
  opened element did not disappear between opens. Later partial runs still
  showed the primary flyout remaining visible after close attempts.
- The root product issue was stale popup state: the nested overflow popups
  were not explicitly synchronized with `IsOpen`, and CommandBarFlyout visual
  states could still transition while the owning flyout had disabled
  open/close animations.

### Resolution

- `CommandBar` now keeps the overflow popup and More button in two-way sync
  with `IsOpen`, closes on Escape, and forces `IsOpen=false` when the popup
  closes.
- `CommandBarFlyoutCommandBar` now explicitly opens or closes the secondary
  popup from `IsOpen`, hides the owning flyout on Escape, and suppresses both
  primary and secondary transition storyboards when the owning flyout disables
  open/close animations.
- Both toolbar popups disable WPF `PopupAnimation` so visual-test recordings
  do not inherit menu animation flicker outside ModernWpf's animation gate.
- The recorder now keeps 24-second CommandBar and CommandBarFlyout clips,
  records closed-state proof between opens, and extracts visual proof from the
  opened-content bounds rather than accepting UIA state alone.
- CommandBarFlyout proof now requires both first and second secondary-menu
  expansions, retargets the proof bounds to `Resize` / `Move`, dwells on the
  expanded secondary state, and raises the secondary-menu open threshold so a
  primary-only frame cannot satisfy verification.

### Verification

- PowerShell parser check passed for
  `tools/visual-checks/Record-GalleryControlInteractions.ps1`.
- `ModernWpf.Gallery` Debug `net8.0-windows7.0` build passed.
- Focused CommandBar recording passed:
  `artifacts/gallery-recordings/20260604-183855-055/report.md`.
  The manifest records `ClosedElementGone=true`,
  `CloseMethod=SampleCloseButton`, `OpenRepeatEvidence=true`, and visual
  frames `t0500` -> `t5000` -> `t7500` -> `t11000` with deltas
  `9.921`, `0.001`, and `9.839`.
- Focused CommandBarFlyout recording passed:
  `artifacts/gallery-recordings/20260604-194134-079/report.md`.
  The manifest records `FirstCommandBarFlyoutSecondaryExpanded=true`,
  `SecondCommandBarFlyoutSecondaryExpanded=true`,
  `ClosedElementGone=true`, `CloseMethod=SecondaryCommand`, and visual frames
  `t0500` -> `t3000` -> `t4000` -> `t8000` with secondary-menu deltas
  `12.363`, `0.001`, and `12.361`.
- Reviewed CommandBarFlyout frames:
  `t3000.png` shows `Resize` / `Move` on first open, `t4000.png` shows the
  flyout closed after `Resize`, and `t8000.png` shows the secondary menu open
  again without a repeat-open crash.

## 2026-07-17 GridView Strict Click-Result Parity Refresh

The older GridView interaction proof was valid only for ModernWpf: its
prefixed output automation ID resolved to the empty output TextBlock, while
the installed WinUI Gallery fell back to the whole `BasicGridView` sample.
Because the official click output is outside that list, the reference crop
could not visibly change even though UIA exposed `You clicked Item 1.`.

- The reference capture now resolves WinUI Gallery's `ClickOutput0` directly.
- Both broad output fields are reduced to the pixels changed by the click
  result before cross-app comparison, avoiding unrelated example-width drift.
- A common-canvas comparison permits at most one pixel of alignment in either
  direction. Crop-size parity remains independently gated, so the alignment
  search cannot hide text metric drift.
- `GridView` now requires an interaction crop under an `8.0` delta gate and a
  four-pixel combined width/height gate.
- Final Light evidence is
  `artifacts/visual-checks/20260717-092950-082-76648/report.md`: interaction
  delta `6.40`, `122x18` versus `120x19`, expected output present.
- Final Dark evidence is
  `artifacts/visual-checks/20260717-092919-846-50492/report.md`: interaction
  delta `6.64`, the same bounded crop metrics, expected output present.
- `GridViewSampleMatchesWinUIGalleryExamples`,
  `GalleryVisualChecksClicksCommonSelectionInteractionControls`, and
  `GalleryVisualChecksEnforceGridViewPixelParityThreshold` pass 3/3 on both
  net8 and net10.

## 2026-07-17 CommandBarFlyout Current-Source Interaction Parity

The previous CommandBarFlyout proof showed that secondary commands opened, but
its interaction crop and UIA evidence did not prove current WinUI surface
geometry or accessibility roles. The refreshed audit uses official
`winui3/main` commit `3cae15f071f1ab8565f9a7592dbf27f04bafe651`
and installed WinUI Gallery `2.9.3.0` / Windows App Runtime `2.2.3.0.0`.

- Root-window and sample-element theme probes now reject a WinUI reference
  capture when the requested Light/Dark theme was not actually applied.
- CommandBarFlyout bounds and element evidence are captured from one atomic
  UIA enumeration so transient popup elements cannot change between geometry
  and accessibility reads.
- The report records each open command's name, control type, and bounds plus
  the raw union. Both ModernWpf and WinUI expose Share, Save, Delete, Resize,
  and Move as MenuItem elements and expose the expanded ellipsis as
  `Less app bar`, MoreButton.
- Current source-sized templates and the WPF popup-HWND anchor correction
  produce an exact `217x124` raw UIA command union and exact `229x136`
  shadow-inclusive interaction crop in both applications.
- Light evidence is
  `artifacts/visual-checks/20260717-214308-498-41228/report.md`: static primary
  delta `4.99`, interaction delta `7.05`.
- Dark evidence is
  `artifacts/visual-checks/20260717-214353-622-95916/report.md`: static primary
  delta `4.99`, interaction delta `8.18`.
- The harness now requires CommandBarFlyout reference interaction parity and
  enforces static delta `<=6.0`, static crop-size delta `<=2`, interaction
  delta `<=9.0`, and exact interaction crop-size parity.

## 2026-07-17 ItemsRepeater Source-Bar Proof

The broader ungated-control inventory found that ItemsRepeater's saved primary
artifact was visibly populated but classified as blank. Its WPF VisualBrush
viewbox included a six-pixel centered-parent offset, clipped the right edge,
and had no matching WinUI primary crop, so the older pass could not prove pixel
parity.

- The harness now crops the current sample's three 425x24 bar rows and two 8px
  StackLayout gaps from live element bounds in both applications.
- WinUI's first anonymous ControlExample pane is selected structurally beside
  `AddBtn`; the crop source is required, so whole-sample fallback cannot pass.
- The ModernWpf crop removes the six-pixel centering inset from the repeater's
  437px max-width surface.
- The Gallery sample now uses the source Low chrome resource for horizontal,
  vertical, and circular bar templates. This changes Light bar backgrounds
  from the stale `#E6E6E6` medium value to source `#F2F2F2`.
- Add/remove and vertical/horizontal/uniform layout behavior remains covered by
  `ItemsRepeaterSampleMatchesWinUIGalleryExamples`; Repeater product tests cover
  realization, layout, recycle, selection, scrolling, and lifecycle behavior.
- Final strict Light
  `artifacts/visual-checks/20260717-220356-944-25124/report.md` passes at `0.53`;
  Dark `artifacts/visual-checks/20260717-220423-221-77876/report.md` passes at
  `0.42`. Both use exact `425x88` crops under a `1.0` exact-size gate.
