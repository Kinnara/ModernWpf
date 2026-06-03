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
  - Passing combined selection sweep for `GridView`, `PipsPager`, and `Pivot`: `artifacts/visual-checks/20260602-042541-677-12876/report.md`

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
  - Light remainder batch for `GridView`, `ItemsRepeater`, `BreadcrumbBar`, `Pivot`, `SelectorBar`, and `NavigationView`: `artifacts/visual-checks/20260602-094314-620-98676/report.md`
  - Light remainder batch for `ContentDialog`, `Flyout`, `Popup`, `MenuBar`, `MenuFlyout`, and `SwipeControl`: `artifacts/visual-checks/20260602-094401-344-38352/report.md`
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
