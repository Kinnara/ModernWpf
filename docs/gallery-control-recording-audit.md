# Gallery Control Recording Audit

This document tracks the recording-first Gallery audit. A control is not counted
as verified here unless there is a live recording for the relevant interaction
path and the recording has been reviewed or decoded into nonblank poster frames.

## Active Goal

Find and fix obvious ModernWpf Gallery visual and interaction defects across
the control inventory, with recordings as blocking evidence. The audit is not
only proving that a route opens or UIA state changes; it must catch the
user-visible failure classes shown in recordings, including open/close flicker,
misaligned or clipped popups and menus, missing expanded content, blank or
stale expanded regions, and repeat-open crashes.

If a user-provided video or manual review exposes a visible defect that an
automated run accepted, the next audit round treats that as a recorder/parity
harness defect as well as a control defect. The harness must be tightened so
the same failure class is reviewable or rejected before the affected control is
marked verified again.

User-provided recordings are authoritative regression evidence. When a supplied
clip shows multiple visible problems, each problem must be enumerated in the
round notes before implementation starts, then closed by product code,
recorder/parity detection, or an explicit still-open defect. A green recording
report that did not sample or analyze the visible failure window is a false
pass, not verification.

The effective goal is therefore stricter than "record and pass": every obvious
issue visible during real interaction must either be fixed in the product or
kept as an open tracked defect. A recording pass, parity pass, or UIA pass is
not accepted when it misses visible flicker, wrong placement, missing content,
blank/stale regions, broken navigation expansion, or a crash that can be seen
in the source clip.

No control can move from `NeedsReview` to verified while a source clip for the
same interaction contains an unmapped visible defect. Each visible defect must
be linked to a product fix, a recorder/parity guard that fails on that class of
issue, or an explicit remaining follow-up item in the audit.

## Acceptance Bar

- Launch the Gallery route for each control in visual-test mode.
- Record the live window while driving the primary interaction for interactive
  controls.
- When the user provides a recording, review it as source evidence and add the
  visible defects to the active control audit before accepting any automated
  pass.
- For every user-video defect, record the detection plan: which frame sheet,
  geometry check, crash check, or parity assertion would catch it on a rerun.
  Unmapped defects block verification.
- For popup and flyout controls, record open, close, and second open in the same
  clip so flicker, stale visual state, and repeat-open crashes are visible.
- Extract poster frames from each recording and reject blank recordings.
- For popup, flyout, menu, and navigation-pane interactions, dense frames around
  open and close transitions must be reviewed or analyzed. A pass must reject
  obvious transient defects such as clipped command strips, missing menu items,
  bad popup/menu alignment, stale pixels, blank expanded regions, app crashes,
  and open or close flicker.
- A recording pass is invalid if its sampled poster frames, UIA state, or
  parity checks would miss an obvious defect visible in the source clip. Add
  dense transition evidence or a control-specific frame/geometry assertion
  before accepting that control again.
- Low-delta interactive recordings must include local rendered evidence inside
  the recorded interaction bounds. UIA state changes can no longer make
  state/value/selection/option/text/output/scroll/navigation interactions pass
  when the cropped control region shows no pixel change.
- A fix round is incomplete until the defect inventory says how each visible
  issue would now be caught: automated fail-fast check, dense frame sheet,
  geometry/parity assertion, crash detection, or explicit still-open follow-up.
- UIA success alone is not accepted as visual proof for popup/flyout/menu
  interactions. The recording report must include either automated geometry or
  frame evidence, or the control remains `NeedsReview`.
- Require a control-specific exposed automation anchor before accepting a route
  capture as proof.
- Fix issues in substantial rounds and record the post-fix interaction before
  committing.

## Scope

Scope now includes the current ModernWpf visual-check inventory from
`tools/visual-checks/Run-GalleryVisualChecks.ps1`, the Gallery shell
NavigationView pane because earlier user-reported failures were in the shell,
and the active official WPF Gallery All Controls catalog pages. Official WPF
catalog pages without page-specific `GallerySample_*` anchors are accepted for
static route proof only when the recorder captures a nonblank rendered
`ContentPagePane` or `GalleryItemPageRoot` artifact.

## Recorder

Use the per-control recorder:

```powershell
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls CommandBarFlyout -Theme Dark -DurationSeconds 8 -FrameRate 10 -Build
```

For broad sweeps, run in batches and review `report.md` plus the MP4 clips under
`artifacts/gallery-recordings/<stamp>/`.

The default recorder is rendered `PrintWindow` composition for the Gallery
process plus popup HWNDs. Popup HWND captures strip edge-connected near-black
pixels because layered-window transparency can otherwise show up as black
backplates in rendered recordings. `-CaptureMode Screen` is available for
diagnostics but is not accepted as proof in the current Codex desktop session
because it can record the Windows background instead of the Gallery window.
Controls that require motion proof can opt into preserved animations and record
`AnimationEvidence` in the manifest while the normal visual-test artifact path
keeps indeterminate visuals stabilized.

## Current Focused Fix Round

Round 49 tightened the recorder after manual review found failures that earlier
passes accepted:

- Mostly blank screen recordings are now rejected unless at least 75% of
  extracted poster frames are nonblank.
- Open/reopen popup, flyout, menu, and split-button interactions now record
  trigger and opened-element bounds, and fail when the opened content is
  detached from the trigger.
- `ExpandCollapsePattern.Expand()` is no longer treated as proof by itself.
  The recorder checks `ExpandCollapseState == Expanded` before returning
  success; no-op expand attempts fall through to invoke/click paths and fail
  if expected opened content is still missing.
- Official WPF pages now have interaction coverage for high-risk route types:
  `Expander` and `TreeView` expansion, `Menu` and `DatePicker` repeat-open,
  `TabControl` selection, and `TextBox` / `PasswordBox` text entry.
- `MenuFlyout` and shared `FlyoutBase` popup HWNDs now receive explicit
  absolute placement when WPF opens the underlying popup at screen origin.

Round 53 closed the SelectorBar false-pass gap:

- `SelectorBar` now exposes its item host through a `SelectorBarItemsControl`
  automation peer, so external UIA can select the generated `SelectorBarItem`
  peers by `SelectionItemPattern` and report `TabItem` control type.
- The Gallery SelectorBar sample keeps its visible adapted item template; tests
  assert the local template renders the icon, text, and selection pill instead
  of relying on UIA state only.
- SelectorBar selection recordings now fail if automation state changes but no
  rendered poster-frame delta is detected. The earlier `NeedsReview` run had
  `MaxFrameDelta=0`, empty before/after target selection, empty sample status,
  and `SelectionChanged=false`; the hardened path converted that class to a
  failed result before the product fix was accepted.

Round 56 closes the CommandBar and CommandBarFlyout repeat-open gap:

- Previous recordings missed obvious CommandBarFlyout defects because the
  pass condition accepted UIA state or low-delta frames that did not actually
  prove the expanded secondary menu. Runs such as
  `artifacts/gallery-recordings/20260604-191251-778/report.md` and
  `artifacts/gallery-recordings/20260604-192051-160/report.md` are treated as
  superseded false positives for secondary-menu proof.
- CommandBar and CommandBarFlyout now run for at least 24 seconds and must
  prove first-open, closed, and second-open states from opened-content frame
  regions. For CommandBarFlyout, the opened-content region is retargeted to
  the `Resize` / `Move` secondary commands and both opens must expand the
  secondary menu.
- Product fixes keep the nested overflow popup state synchronized with
  `IsOpen`, disable WPF `PopupAnimation`, suppress CommandBarFlyout
  transitions when the owning flyout disables open/close animations, and hide
  the owning flyout on Escape.

Round 57 closes one MenuFlyout repeat-open guard and keeps Flyout open as a
tracked failure:

- `MenuFlyout.ShowAtCore` now treats an already-open presenter at
  `AbsolutePoint` as the same target when its tracked `Target` matches the
  requested placement target, and treats requested `Custom` placement as
  equivalent to the current absolute-point presenter. This prevents same-target
  repeat opens from closing and reopening the menu after the absolute-placement
  conversion.
- `FlyoutBaseApiTests.HideDisconnectsPopupVisualSource` covers the old blind
  spot where `Hide()` updated logical `IsOpen` but a popup visual source could
  remain connected.
- The recorder no longer starts Flyout/Popup/MenuFlyout close attempts by
  hold-clicking the first open element. It uses named sample actions first,
  falls back to Escape/dismiss, and still rejects the run unless rendered
  frames prove first-open, closed, and second-open states.
- Latest Flyout proof
  `artifacts/gallery-recordings/20260605-020217-228/report.md` is intentionally
  failed: `CloseMethod=DismissPoint2`, `ClosedElementGone=false`,
  `VisualOpenRepeatEvidence.Generated=false`, and the same opened-element
  bounds remained present between first and second open. Earlier green Flyout
  rows are superseded for close/reopen proof until this product failure is
  fixed.

Round 58 closes the Flyout verifier gap exposed while investigating that
failure:

- The Flyout blocker was not a product close failure in the latest rendered
  evidence; the recorder was sampling the wrong proof. UIA could report the
  named flyout button gone while the chosen poster frame still showed a
  visible flyout, and stopwatch-derived visual timestamps were not aligned
  tightly enough with the rendered video frame stream.
- Popup-style open-repeat closes now carry a pixel-backed live close context.
  The close path captures a baseline from the rendered recorder's live frames,
  requires the named open region to return to baseline before accepting a
  close, and records `CloseVisualChecked`, `CloseVisualClosed`,
  `CloseVisualDelta`, and the close snapshot in the manifest.
- Open-repeat visual proof no longer trusts a single stopwatch-derived closed
  timestamp. It scans the extracted poster frames for the actual
  baseline -> open -> closed -> second-open transition and accepts the closed
  state only when the opened-content region returns close to baseline. The
  closed threshold is `1.0` luminance delta; the latest Flyout run measured
  open/closed/second-open deltas of `22.728`, `0.901`, and `19.984`.
- Latest Flyout proof
  `artifacts/gallery-recordings/20260605-030028-982/report.md` passed with
  `Detection=BaselineDeltaScan`, frames `t2500` / `t6500` / `t11500`, and
  `CloseVisualChecked=true`. Earlier failed Flyout runs remain useful
  evidence of the recorder defect but are superseded for current close/reopen
  status.

Round 59 closes the pending MenuFlyout focused rerun and tightens the same
visual-proof class:

- The first MenuFlyout rerun under the new baseline scan exposed another
  false-pass edge: a low `0.714` closed-state drift was high enough to satisfy
  the old `0.5` open threshold, so the scan could pick a frame before the real
  second open.
- Open-repeat visual evidence now requires a `5.0` luminance delta for both
  open states while keeping the closed threshold at `1.0`. This prevents
  normal rendered drift in a closed region from counting as an open popup/menu.
- Latest MenuFlyout proof
  `artifacts/gallery-recordings/20260605-031711-696/report.md` passed with
  `Detection=BaselineDeltaScan`, `CloseMethod=LeafMenuItem:Invoke`,
  `CloseVisualChecked=true`, frames `t2000` / `t6500` / `t12000`, and
  open/closed/second-open deltas of `15.058`, `0.679`, and `14.044`.

Round 60 closes the ContentDialog and Popup stale-automation verifier gap:

- The failed focused run
  `artifacts/gallery-recordings/20260605-032318-214/report.md` exposed two
  recorder defects. ContentDialog was treated like a light-dismiss popup even
  though the sample is modal, so the dialog stayed open between first and
  second open. Popup visually closed and reopened, but stale UIA for the
  named popup element kept `ClosedElementGone=false` and blocked the pass.
- Popup-style close verification now consults the live pixel close context
  even while UIA still reports an opened element. A stale automation element
  can no longer override a close when the opened-content region has returned
  to baseline. If pixels still show the opened region, the close still fails.
- ContentDialog is now in the 24-second live-visual close bucket, keeps the
  dialog visible long enough for poster-frame extraction, and closes through
  the named `Cancel` dialog button before falling back to generic dismiss
  paths.
- Latest ContentDialog/Popup proof
  `artifacts/gallery-recordings/20260605-033404-923/report.md` passed with
  `CloseVisualChecked=true` for both controls. ContentDialog used
  `CloseMethod=DialogCancelButton:Invoke`, frames `t2000` / `t9000` /
  `t14000`, and open/closed/second-open deltas of `12.379`, `0.756`, and
  `26.644`. Popup used `CloseMethod=SampleCloseButton:Invoke`, frames
  `t2000` / `t8500` / `t11000`, and deltas of `28.867`, `0.937`, and
  `28.846`.

Round 61 hardens the remaining dropdown/menu open-repeat controls:

- The focused run
  `artifacts/gallery-recordings/20260605-034335-817/report.md` failed all
  eight retested open-repeat controls because the generic close path left
  dropdowns, menus, and calendars open through the close interval. This
  exposed another false-pass class in older evidence: first and second open
  elements could be found while the recording never proved a visible closed
  interval.
- The recorder now uses control-specific close actions before generic dismiss:
  leaf item invocation for TeachingTip, DropDownButton, SplitButton,
  ToggleSplitButton, MenuBar, and Menu; `ExpandCollapsePattern.Collapse()` for
  ComboBox and DatePicker; focused keyboard/direct-click fallbacks for the WPF
  popup controls that do not respond to normal mouse dismissal in this desktop
  session.
- Live pixel close proof now covers these dropdown/menu controls, with a
  1500ms first-open dwell so the extracted frames reliably sample open,
  closed, and second-open states. DatePicker keeps the `5.0` open threshold
  but uses a `1.2` closed threshold because the expanded calendar proof region
  includes margin and returned to `1.016` delta in the verified closed frame.
- Latest proof under the final recorder code:
  `artifacts/gallery-recordings/20260605-043648-914/report.md` passed for
  ComboBox and DropDownButton, `20260605-042758-748` passed for DatePicker,
  and `20260605-042951-643` passed for TeachingTip, SplitButton,
  ToggleSplitButton, MenuBar, and Menu. The accepted frame/delta triples are
  ComboBox `t2500` / `t5000` / `t9500` with `9.482` / `0.544` / `11.141`,
  DropDownButton `t1500` / `t3000` / `t10500` with `12.183` / `0.397` /
  `12.406`, DatePicker `t2000` / `t4000` / `t8500` with `11.026` / `1.016` /
  `10.707`, TeachingTip `t2500` / `t3500` / `t8500` with `12.768` / `0.697` /
  `12.818`, SplitButton `t2500` / `t5500` / `t10000` with `21.394` / `0.013` /
  `21.441`, ToggleSplitButton `t2500` / `t6000` / `t10500` with `10.864` /
  `0.025` / `18.001`, MenuBar `t2500` / `t4000` / `t9000` with `11.896` /
  `0.258` / `12.3`, and Menu `t2500` / `t5000` / `t10000` with `13.701` /
  `0.797` / `13.843`.

Round 62 refreshes older state, value, output, scroll, and navigation
interaction evidence under the current recorder:

- `artifacts/gallery-recordings/20260605-044321-949/report.md` passed for
  CheckBox, RadioButton, Slider, RatingControl, ToggleButton, ToggleSwitch,
  NumberBox, InfoBar, and AppBarToggleButton. Every low whole-frame-delta
  state/value interaction included local rendered evidence inside the
  interaction bounds, so UIA state alone did not carry the pass.
- `artifacts/gallery-recordings/20260605-044806-923/report.md` passed for
  Button, ColorPicker, RepeatButton, SplitView, AnnotatedScrollBar, GridView,
  ItemsRepeater, BreadcrumbBar, NavigationView, and AppBarButton. This run
  refreshes older output, option, selection, breadcrumb, and scroll evidence
  with local visual deltas or large scroll-frame deltas from the current
  recorder.
- The current recorder still treats these passes as interaction proof, not
  static route proof: e.g. Button local delta `4.268`, RepeatButton local delta
  `1.034`, AnnotatedScrollBar frame/local deltas `4.521` / `95.226`,
  ItemsRepeater frame/local deltas `4.456` / `11.471`, BreadcrumbBar local
  delta `4.267`, NavigationView local delta `5.808`, and AppBarButton local
  delta `0.795`.

Round 63 refreshes shell navigation, text, animation, and static-route
evidence under the current recorder:

- `artifacts/gallery-recordings/20260605-045636-525/report.md` passed for
  ShellNavigation, AutoSuggestBox, ProgressRing, HyperlinkButton,
  PersonPicture, IconElement, ThemeShadow, TitleBar, InfoBadge, and
  AppBarSeparator with no needs-review or failed entries.
- ShellNavigation now has dense transition proof plus manifest-level expansion
  checks for Design Guidance and Samples. The manifest records visible child
  items while expanded, hidden child items after collapse, following-item gaps
  of `2.0`, `ShellNavigationChanged=true`, and local visual delta `9.407`.
- AutoSuggestBox and ProgressRing were moved off older evidence: AutoSuggestBox
  records text proof with local delta `2.816` and expected output despite low
  whole-frame delta `0.111`; ProgressRing records `AnimationEvidence=true`
  with early-frame delta `0.075`, local delta `13.759`, and option-state
  change.
- HyperlinkButton, PersonPicture, IconElement, ThemeShadow, TitleBar,
  InfoBadge, and AppBarSeparator remain static route captures. The current
  run proves nonblank routed pages against the recorder's required automation
  anchor mapping; it is not interaction proof for those controls.

Round 64 refreshes official WPF interaction coverage and demotes the ToolTip
prepared-open false pass:

- `artifacts/gallery-recordings/20260605-050718-351/report.md` passed for
  Expander, TreeView, TabControl, TextBox, PasswordBox, Calendar, ListBox,
  ListView, DataGrid, ToolTip, and RichTextEdit. Ten of those controls now
  have current interaction proof through expansion, selection, text-entry, or
  prepared text evidence. DataGrid remains a visual-selection proof because
  UIA selection did not report a changed item, but the rendered row/cell
  highlight produced `VisualSelectionEvidence=true` and local delta `57.125`.
- The ToolTip row in that run is no longer accepted as full interaction proof.
  It used the diagnostic `PreparedOpen` path, so it proved only that the WPF
  ToolTip can render when opened in-process. The same class of static
  prepared-open pass previously hid the fact that synthetic hover did not
  prove real open/close/reopen behavior.
- The recorder now routes ToolTip through `OpenRepeat`, removes ToolTip from
  diagnostic pre-open preparation, and sends explicit cursor movement and
  window mouse-move messages before waiting for the hover delay. Source-shape
  tests reject any return to `PreparedOpen` for ToolTip.
- Current ToolTip hover proof is still failed and tracked open:
  `artifacts/gallery-recordings/20260605-052434-715/report.md` failed under
  rendered capture with unchanged frames, no first or second opened element,
  and no visual open-repeat evidence. A diagnostic screen-mode attempt at
  `artifacts/gallery-recordings/20260605-053026-959/report.md` also failed in
  this desktop session because most captured frames were black, so screen mode
  is not accepted as proof here.

Round 65 strengthens the ToolTip hover probe and keeps the defect open:

- The ToolTip `OpenRepeat` path now forces focus away from the trigger, sets
  focus back to the button, walks the pointer into the button over multiple
  move messages, then sends both synchronous and queued WPF window mouse-move
  and mouse-hover messages before the hover dwell.
- The stronger probe still does not open the WPF ToolTip in this desktop
  session. The latest rendered run
  `artifacts/gallery-recordings/20260605-054214-762/report.md` failed with
  unchanged frames, `FirstOpenElementFound=false`,
  `SecondOpenElementFound=false`, and no visual open-repeat evidence. This
  preserves ToolTip as an explicit open defect instead of accepting a
  diagnostic prepared-open pass.

Latest focused evidence:

| Run | Controls | Result |
| --- | --- | --- |
| `artifacts/gallery-recordings/20260604-034810-236/report.md` | DropDownButton | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-035722-075/report.md` | DropDownButton, MenuFlyout, SplitButton, ToggleSplitButton | 4 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-040103-946/report.md` | ContentDialog, Flyout, Popup, CommandBarFlyout | 4 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-044508-719/report.md` | Menu | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-044721-837/report.md` | Expander, TreeView, Menu, TabControl, DatePicker, Calendar, TextBox, PasswordBox | 8 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-050301-561/report.md` | ListBox, ListView, DataGrid, Calendar, ToolTip, RichTextEdit | 3 passed, 1 needs review, 2 failed |
| `artifacts/gallery-recordings/20260604-051818-365/report.md` | DataGrid | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-053726-512/report.md` | ToolTip, RichTextEdit | 2 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-054021-152/report.md` | CommandBarFlyout, MenuFlyout, Flyout, Popup, DropDownButton, SplitButton, ToggleSplitButton, Menu, DatePicker | 9 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-061652-261/report.md` | SelectorBar | 0 passed, 0 needs review, 1 failed |
| `artifacts/gallery-recordings/20260604-064455-670/report.md` | SelectorBar | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-183855-055/report.md` | CommandBar | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260604-194134-079/report.md` | CommandBarFlyout | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-020217-228/report.md` | Flyout | 0 passed, 0 needs review, 1 failed |
| `artifacts/gallery-recordings/20260605-030028-982/report.md` | Flyout | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-031711-696/report.md` | MenuFlyout | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-033404-923/report.md` | ContentDialog, Popup | 2 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-034335-817/report.md` | TeachingTip, ComboBox, DropDownButton, SplitButton, ToggleSplitButton, MenuBar, Menu, DatePicker | 0 passed, 0 needs review, 8 failed |
| `artifacts/gallery-recordings/20260605-043648-914/report.md` | ComboBox, DropDownButton | 2 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-042758-748/report.md` | DatePicker | 1 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-042951-643/report.md` | TeachingTip, SplitButton, ToggleSplitButton, MenuBar, Menu | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-044321-949/report.md` | CheckBox, RadioButton, Slider, RatingControl, ToggleButton, ToggleSwitch, NumberBox, InfoBar, AppBarToggleButton | 9 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-044806-923/report.md` | Button, ColorPicker, RepeatButton, SplitView, AnnotatedScrollBar, GridView, ItemsRepeater, BreadcrumbBar, NavigationView, AppBarButton | 10 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-045636-525/report.md` | ShellNavigation, AutoSuggestBox, ProgressRing, HyperlinkButton, PersonPicture, IconElement, ThemeShadow, TitleBar, InfoBadge, AppBarSeparator | 10 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260605-050718-351/report.md` | Expander, TreeView, TabControl, TextBox, PasswordBox, Calendar, ListBox, ListView, DataGrid, ToolTip, RichTextEdit | 11 passed, 0 needs review, 0 failed; ToolTip was diagnostic prepared-open only and is superseded by the failed real-hover run |
| `artifacts/gallery-recordings/20260605-052434-715/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed |
| `artifacts/gallery-recordings/20260605-053026-959/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed; screen-mode diagnostic rejected because most frames were black |
| `artifacts/gallery-recordings/20260605-054214-762/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed; focus, stepped pointer movement, and queued hover messages still did not open the WPF ToolTip |

The `20260604-050301-561` run is intentionally not treated as a green sweep:
it exposed two remaining interaction gaps that the older static sweep missed.
`ToolTip` did not open under the current synthetic hover/click path, and
`RichTextEdit` focused but did not receive text input through the recorder.
The `20260604-053726-512` follow-up closes RichTextEdit with
diagnostics-prepared text evidence and shows that a WPF ToolTip can render when
opened in-process. It is no longer accepted as ToolTip hover interaction proof:
the current real-hover ToolTip runs failed and must stay open until a recording
proves open, close, and second open from actual interaction.

## Current Full-Inventory Sweep

The one-shot all-control recorder command exceeded the 15-minute runner timeout
after `SplitButton` before it could write a top-level report. The baseline
inventory proof uses smaller dark-theme batches against the same built tree.
Later recorder hardening supersedes any older pass that did not meet the
current visible-evidence bar; the focused evidence table and Control Matrix
are authoritative for controls retested after the broad batches.

### Dark Theme

| Run | Controls | Result |
| --- | --- | --- |
| `artifacts/gallery-recordings/20260603-192050-001/report.md` | TeachingTip, Button, CheckBox, ComboBox, RadioButton | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-192616-247/report.md` | Slider, ColorPicker, HyperlinkButton, RatingControl, RepeatButton | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-193146-788/report.md` | ToggleButton, DropDownButton, SplitButton, ToggleSplitButton, ToggleSwitch | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-194020-526/report.md` | NumberBox, AutoSuggestBox, SplitView, PersonPicture | 4 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-194548-087/report.md` | IconElement, ThemeShadow, TitleBar, InfoBadge, InfoBar, ProgressRing | 6 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-195228-523/report.md` | AnnotatedScrollBar, GridView, ItemsRepeater, BreadcrumbBar, SelectorBar, NavigationView | 6 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-200011-932/report.md` | ContentDialog, Flyout, Popup, MenuBar, MenuFlyout | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-200545-017/report.md` | AppBarButton, AppBarSeparator, AppBarToggleButton, CommandBar, CommandBarFlyout | 5 passed, 0 needs review, 0 failed |

### Light Theme

| Run | Controls | Result |
| --- | --- | --- |
| `artifacts/gallery-recordings/20260603-201449-823/report.md` | TeachingTip, Button, CheckBox, ComboBox, RadioButton | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-202011-935/report.md` | Slider, ColorPicker, HyperlinkButton, RatingControl, RepeatButton | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-202523-730/report.md` | ToggleButton, DropDownButton, SplitButton, ToggleSplitButton, ToggleSwitch | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-203341-290/report.md` | NumberBox, AutoSuggestBox, SplitView, PersonPicture | 4 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-203857-905/report.md` | IconElement, ThemeShadow, TitleBar, InfoBadge, InfoBar, ProgressRing | 6 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-204521-233/report.md` | AnnotatedScrollBar, GridView, ItemsRepeater, BreadcrumbBar, SelectorBar, NavigationView | 6 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-205311-406/report.md` | ContentDialog, Flyout, Popup, MenuBar, MenuFlyout | 5 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-205913-779/report.md` | AppBarButton, AppBarSeparator, AppBarToggleButton, CommandBar, CommandBarFlyout | 5 passed, 0 needs review, 0 failed |

## Official WPF All Controls Static Sweep

The ModernWpf recorder now accepts nonblank rendered page artifacts for official
WPF Gallery pages that do not expose page-specific `GallerySample_*` anchors.
The expansion covers the 33 official All Controls catalog pages that were
missing from the ModernWpf control recorder inventory. This sweep is no longer
accepted as sufficient by itself for controls where a user interaction is
available; newer rounds add selection, open-repeat, expansion, and text-entry
proof on top of these static route captures.

### Dark Theme

| Run | Controls | Result |
| --- | --- | --- |
| `artifacts/gallery-recordings/20260603-213649-600/report.md` | Color, Typography, Spacing, Geometry, Iconography, DataGrid, ListBox, ListView, TreeView, Calendar, DatePicker | 11 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-214032-634/report.md` | Expander, Grid, ResizeGrip, GridSplitter, GroupBox, StackPanel, Border, Menu, TabControl, Frame, NavigationWindow | 11 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-214413-419/report.md` | ProgressBar, ToolTip, Label, TextBox, TextBlock, RichTextEdit, PasswordBox, Hyperlink, FileAndFolderDialogs, MessageBox, Clipboard | 11 passed, 0 needs review, 0 failed |

### Light Theme

| Run | Controls | Result |
| --- | --- | --- |
| `artifacts/gallery-recordings/20260603-215102-800/report.md` | Color, Typography, Spacing, Geometry, Iconography, DataGrid, ListBox, ListView, TreeView, Calendar, DatePicker | 11 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-215446-052/report.md` | Expander, Grid, ResizeGrip, GridSplitter, GroupBox, StackPanel, Border, Menu, TabControl, Frame, NavigationWindow | 11 passed, 0 needs review, 0 failed |
| `artifacts/gallery-recordings/20260603-215823-516/report.md` | ProgressBar, ToolTip, Label, TextBox, TextBlock, RichTextEdit, PasswordBox, Hyperlink, FileAndFolderDialogs, MessageBox, Clipboard | 11 passed, 0 needs review, 0 failed |

## Control Matrix

| Area | Control | Route or Scenario | Recording Status | Fix Status | Latest Evidence | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Shell | Navigation pane | Home, Design Guidance, Samples expand/collapse | Recorded | Fixed | `artifacts/gallery-recordings/20260605-045636-525/ShellNavigation/dark-shellnavigation.mp4` | Latest manifest proves Design Guidance and Samples expanded with visible children, then collapsed with children hidden; following-item gaps remain `2.0`, `ShellNavigationChanged=true`, and local visual delta is `9.407`. Dense transition review sheet: `artifacts/gallery-recordings/20260605-045636-525/ShellNavigation/analysis/dense-transition-review.jpg`. |
| Dialogs & flyouts | TeachingTip | `item/TeachingTip` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-042951-643/TeachingTip/dark-teachingtip.mp4` | Latest rendered run closes through the named TeachingTip close button and requires baseline-delta open/closed/open proof. Manifest records `CloseMethod=LeafCloseItem:Invoke`, `Detection=BaselineDeltaScan`, frames `t2500` / `t3500` / `t8500`, and deltas `12.768` / `0.697` / `12.818`. |
| Basic input | Button | `item/Button` | Recorded | Fixed | `artifacts/gallery-recordings/20260605-044806-923/Button/dark-button.mp4` | Latest rendered run toggles `Disable button` and records local visual delta `4.268`; option state changed despite low whole-frame delta `0.048`. |
| Basic input | CheckBox | `item/CheckBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/CheckBox/dark-checkbox.mp4` | Latest rendered run records local visual delta `3.174`; before/after toggle state changed despite low whole-frame delta `0.028`. |
| Basic input | ComboBox | `item/ComboBox` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-043648-914/ComboBox/dark-combobox.mp4` | Latest rendered run closes through `ExpandCollapsePattern.Collapse()` and requires baseline-delta open/closed/open proof. Manifest records frames `t2500` / `t5000` / `t9500`, deltas `9.482` / `0.544` / `11.141`, and local delta `11.141`. |
| Basic input | RadioButton | `item/RadioButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/RadioButton/dark-radiobutton.mp4` | Latest rendered run records local visual delta `3.87`; selection/output evidence changed despite low whole-frame delta `0.022`. |
| Basic input | Slider | `item/Slider` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/Slider/dark-slider.mp4` | Latest rendered run records local visual delta `2.422`; target value was reached despite low whole-frame delta `0.031`. |
| Basic input | ColorPicker | `item/ColorPicker` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/ColorPicker/dark-colorpicker.mp4` | Latest rendered run records option interaction with local visual delta `6.413` and whole-frame delta `0.37`. |
| Basic input | HyperlinkButton | `item/HyperlinkButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-045636-525/HyperlinkButton/dark-hyperlinkbutton.mp4` | Static route capture only; the current run records nonblank frames and routes against required anchor `GallerySample_HyperlinkButton_HyperlinkButton`. External URI navigation is intentionally not invoked. |
| Basic input | RatingControl | `item/RatingControl` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/RatingControl/dark-ratingcontrol.mp4` | Latest rendered run records local visual delta `4.198`; target value was reached despite low whole-frame delta `0.21`. |
| Basic input | RepeatButton | `item/RepeatButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/RepeatButton/dark-repeatbutton.mp4` | Latest rendered run records local visual delta `1.034`; output text changed despite low whole-frame delta `0.035`. |
| Basic input | ToggleButton | `item/ToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/ToggleButton/dark-togglebutton.mp4` | Latest rendered run records local visual delta `38.487`; before/after toggle state changed despite low whole-frame delta `0.234`. |
| Basic input | DropDownButton | `item/DropDownButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-043648-914/DropDownButton/dark-dropdownbutton.mp4` | Latest rendered run closes through the `Send` leaf item and requires baseline-delta open/closed/open proof. Manifest records frames `t1500` / `t3000` / `t10500` and deltas `12.183` / `0.397` / `12.406`. |
| Basic input | SplitButton | `item/SplitButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-042951-643/SplitButton/dark-splitbutton.mp4` | Latest rendered run closes through the `Red` leaf item and requires baseline-delta open/closed/open proof. Manifest records frames `t2500` / `t5500` / `t10000` and deltas `21.394` / `0.013` / `21.441`. |
| Basic input | ToggleSplitButton | `item/ToggleSplitButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-042951-643/ToggleSplitButton/dark-togglesplitbutton.mp4` | Latest rendered run closes through the `Bulleted list` leaf item and requires baseline-delta open/closed/open proof. Manifest records frames `t2500` / `t6000` / `t10500`, deltas `10.864` / `0.025` / `18.001`, and local delta `38.571`. |
| Basic input | ToggleSwitch | `item/ToggleSwitch` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/ToggleSwitch/dark-toggleswitch.mp4` | Latest rendered run records local visual delta `7.034`; before/after toggle state changed despite low whole-frame delta `0.014`. |
| Text | NumberBox | `item/NumberBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/NumberBox/dark-numberbox.mp4` | Latest rendered run reaches the target value with local visual delta `0.381`; whole-frame delta remains `0`, so the proof is intentionally cropped to the interaction bounds. |
| Text | AutoSuggestBox | `item/AutoSuggestBox` | Recorded | Fixed | `artifacts/gallery-recordings/20260605-045636-525/AutoSuggestBox/dark-autosuggestbox.mp4` | Latest rendered run records text interaction with local visual delta `2.816`; expected output was detected despite low whole-frame delta `0.111`. `AutoSuggestBoxInteractionTests` covers item-click submit/close behavior. |
| Text | TextBox | `item/TextBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/TextBox/dark-textbox.mp4` | Latest rendered run records text entry with local visual delta `5.519`; expected `ModernWpf text` output was detected. |
| Text | PasswordBox | `item/PasswordBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/PasswordBox/dark-passwordbox.mp4` | Latest rendered run records text entry with local visual delta `5.524`; the password output remains masked while the expected input state is detected. |
| Text | RichTextEdit | `item/RichTextEdit` | Recorded | Recorder gap tracked | `artifacts/gallery-recordings/20260605-050718-351/RichTextEdit/dark-richtextedit.mp4` | Current proof is diagnostic prepared text, not external keyboard injection proof. It remains accepted only as rendered text-state evidence until the recorder can drive WPF RichTextBox input directly. |
| Layout | SplitView | `item/SplitView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/SplitView/dark-splitview.mp4` | Latest rendered run records the option interaction with local visual delta `36.476` and whole-frame delta `0.951`. |
| Layout | Expander | `item/Expander` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/Expander/dark-expander.mp4` | Latest rendered run records expansion evidence with whole-frame/local deltas `0.659` / `11.331`; reviewed frames show the expected content visible after expansion. |
| Media | PersonPicture | `item/PersonPicture` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-045636-525/PersonPicture/dark-personpicture.mp4` | Static rendered route with nonblank frames against required anchor `GallerySample_PersonPicture_PersonPicture`. |
| Styles | IconElement | `item/IconElement` | Recorded | Recorder anchor fixed | `artifacts/gallery-recordings/20260605-045636-525/IconElement/dark-iconelement.mp4` | Static rendered route with nonblank frames against required anchor `GallerySample_IconElement_ExampleButton1`. |
| Styles | ThemeShadow | `item/ThemeShadow` | Recorded | Sample anchor fixed | `artifacts/gallery-recordings/20260605-045636-525/ThemeShadow/dark-themeshadow.mp4` | Static rendered route with nonblank frames against required anchor `GallerySample_ThemeShadow_TranslationSlider`. |
| Windowing | TitleBar | `item/TitleBar` | Recorded | Sample anchor fixed | `artifacts/gallery-recordings/20260605-045636-525/TitleBar/dark-titlebar.mp4` | Static rendered route with nonblank frames against required anchor `GallerySample_TitleBar_SearchBox`. |
| Status & info | InfoBadge | `item/InfoBadge` | Recorded | Sample anchor fixed | `artifacts/gallery-recordings/20260605-045636-525/InfoBadge/dark-infobadge.mp4` | Static rendered route with nonblank frames against required anchor `GallerySample_InfoBadge_NavigationView`. |
| Status & info | InfoBar | `item/InfoBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/InfoBar/dark-infobar.mp4` | Latest rendered run records the option interaction with local visual delta `8.084` and whole-frame delta `0.379`. |
| Status & info | ProgressRing | `item/ProgressRing` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260605-045636-525/ProgressRing/dark-progressring.mp4` | Latest manifest records `AnimationEvidence=true` with early-frame delta `0.075`, local visual delta `13.759`, and option state change despite low whole-frame delta `0.086`. |
| Status & info | ToolTip | `item/ToolTip` | Failed recording | Open defect | `artifacts/gallery-recordings/20260605-054214-762/ToolTip/dark-tooltip.mp4` | ToolTip now uses real `OpenRepeat` hover proof instead of diagnostic `PreparedOpen`. Latest rendered run includes focus, stepped pointer movement, and queued hover messages, but still failed with no first/second opened element and no visual open-repeat evidence; prior prepared-open runs are superseded for hover proof. |
| Scrolling | AnnotatedScrollBar | `item/AnnotatedScrollBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/AnnotatedScrollBar/dark-annotatedscrollbar.mp4` | Latest rendered run records scroll interaction with whole-frame delta `4.521` and local visual delta `95.226`. |
| Collections | GridView | `item/GridView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/GridView/dark-gridview.mp4` | Latest rendered run records local visual delta `1.104`; selection/output evidence changed despite low whole-frame delta `0.162`. |
| Collections | ItemsRepeater | `item/ItemsRepeater` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260605-044806-923/ItemsRepeater/dark-itemsrepeater.mp4` | Latest rendered run records virtualized scroll interaction with whole-frame delta `4.456` and local visual delta `11.471`. |
| Collections | ListBox | `item/ListBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/ListBox/dark-listbox.mp4` | Latest rendered run records selection evidence with whole-frame/local deltas `1.122` / `22.966`. |
| Collections | ListView | `item/ListView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/ListView/dark-listview.mp4` | Latest rendered run records selection evidence with whole-frame/local deltas `0.379` / `5.654`. |
| Collections | DataGrid | `item/DataGrid` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/DataGrid/dark-datagrid.mp4` | UIA selection did not change, but visual selection evidence passed with whole-frame/local deltas `1.594` / `57.125`; reviewed frames show the selected row/cells highlighted. |
| Collections | TreeView | `item/TreeView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/TreeView/dark-treeview.mp4` | Latest rendered run records expansion evidence with whole-frame/local deltas `1.009` / `14.665`; expected child content is visible after expansion. |
| Navigation | BreadcrumbBar | `item/BreadcrumbBar` | Recorded | Recorder/sample anchor fixed | `artifacts/gallery-recordings/20260605-044806-923/BreadcrumbBar/dark-breadcrumbbar.mp4` | Latest rendered run records local visual delta `4.267`; breadcrumb item collection changed despite low whole-frame delta `0.028`. |
| Navigation | SelectorBar | `item/SelectorBar` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260604-064455-670/SelectorBar/dark-selectorbar.mp4` | Manifest records `Shared` changing from `Unselected` to `Selected`, sample status changing to `Shared`, and `VisualSelectionEvidence=true` with frame delta `0.003`; reviewed `t2000.png`/`t3000.png` shows the selected pill moving from no basic item to `Shared`. |
| Navigation | TabControl | `item/TabControl` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/TabControl/dark-tabcontrol.mp4` | Latest rendered run records selection evidence and expected `World` output; local delta `2.818` covers the low whole-frame delta `0.019`. |
| Navigation | NavigationView | `item/NavigationView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/NavigationView/dark-navigationview.mp4` | Latest rendered run records local visual delta `5.808`; selection/output evidence changed despite low whole-frame delta `0.043`. |
| Date & calendar | Calendar | `item/Calendar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-050718-351/Calendar/dark-calendar.mp4` | Latest rendered run records selection evidence with whole-frame/local deltas `0.475` / `4.178`. |
| Dialogs & flyouts | ContentDialog | `item/ContentDialog` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-033404-923/ContentDialog/dark-contentdialog.mp4` | Latest 24s rendered run treats modal close as a named `Cancel` button action and requires pixel-backed close proof. Manifest records `CloseMethod=DialogCancelButton:Invoke`, `CloseVisualChecked=true`, `Detection=BaselineDeltaScan`, frames `t2000` / `t9000` / `t14000`, and deltas `12.379` / `0.756` / `26.644`. |
| Dialogs & flyouts | Flyout | `item/Flyout` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260605-030028-982/Flyout/dark-flyout.mp4` | Latest 24s rendered run passes with pixel-backed close proof and baseline-delta transition scan: `CloseVisualChecked=true`, `CloseVisualClosed=true`, `Detection=BaselineDeltaScan`, frames `t2500` / `t6500` / `t11500`, and deltas `22.728` / `0.901` / `19.984`. |
| Dialogs & flyouts | Popup | `item/Popup` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-033404-923/Popup/dark-popup.mp4` | Latest 24s rendered run accepts the named `Close` button only after the opened-content region returns to baseline, so stale UIA cannot block or fake the close. Manifest records `CloseMethod=SampleCloseButton:Invoke`, `CloseVisualChecked=true`, `Detection=BaselineDeltaScan`, frames `t2000` / `t8500` / `t11000`, and deltas `28.867` / `0.937` / `28.846`. |
| Menus & toolbars | MenuBar | `item/MenuBar` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260605-042951-643/MenuBar/dark-menubar.mp4` | Latest rendered run closes through the `Exit` leaf item and requires baseline-delta open/closed/open proof. Manifest records frames `t2500` / `t4000` / `t9000`, deltas `11.896` / `0.258` / `12.3`, and local delta `12.485`. |
| Menus & toolbars | MenuFlyout | `item/MenuFlyout` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260605-031711-696/MenuFlyout/dark-menuflyout.mp4` | Same-target repeat-open guard now treats tracked absolute-point presenters as the same target to avoid close/reopen flicker. Latest 24s rendered rerun passes with `CloseMethod=LeafMenuItem:Invoke`, `CloseVisualChecked=true`, `Detection=BaselineDeltaScan`, frames `t2000` / `t6500` / `t12000`, and deltas `15.058` / `0.679` / `14.044`. |
| Menus & toolbars | AppBarButton | `item/AppBarButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044806-923/AppBarButton/dark-appbarbutton.mp4` | Latest rendered run records local visual delta `0.795`; output text changed despite low whole-frame delta `0.057`. |
| Menus & toolbars | AppBarSeparator | `item/AppBarSeparator` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-045636-525/AppBarSeparator/dark-appbarseparator.mp4` | Static rendered route with nonblank frames against required anchor `GallerySample_AppBarSeparator_AttachCameraButton`. |
| Menus & toolbars | AppBarToggleButton | `item/AppBarToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260605-044321-949/AppBarToggleButton/dark-appbartogglebutton.mp4` | Latest rendered run records local visual delta `41.626`; before/after toggle state changed despite low whole-frame delta `0.268`. |
| Menus & toolbars | CommandBar | `item/CommandBar` | Recorded | Fixed | `artifacts/gallery-recordings/20260604-183855-055/CommandBar/dark-commandbar.mp4` | Product popup state is now synchronized with `IsOpen` and the recorder no longer depends on UIA exposing the second-open overflow item. Manifest records `ClosedElementGone=true`, `CloseMethod=SampleCloseButton`, `OpenRepeatEvidence=true`, and visual open/closed/open frames `t0500` / `t5000` / `t7500` / `t11000` with deltas `9.921`, `0.001`, and `9.839`. |
| Menus & toolbars | CommandBarFlyout | `item/CommandBarFlyout` | Recorded | Fixed | `artifacts/gallery-recordings/20260604-194134-079/CommandBarFlyout/dark-commandbarflyout.mp4` | Product popup state is now synchronized with `IsOpen`, WPF popup animation is disabled, Escape hides the owning flyout, and secondary transitions respect the owning flyout animation gate. The latest 24s run requires both first and second secondary-menu expansions; reviewed frames `t3000`, `t4000`, and `t8000` show `Resize` / `Move`, closed state after `Resize`, and second-open `Resize` / `Move` with no repeat-open crash. |
