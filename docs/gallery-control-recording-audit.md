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
  InfoBadge, and AppBarSeparator remained static route captures in this run.
  The run proved nonblank routed pages against the recorder's required
  automation anchor mapping, not interaction proof. HyperlinkButton's
  static-only status is superseded by Round 70; TitleBar's is superseded by
  Round 71.

Round 64 refreshes official WPF interaction coverage and demotes the ToolTip
prepared-open false pass:

- `artifacts/gallery-recordings/20260605-050718-351/report.md` passed for
  Expander, TreeView, TabControl, TextBox, PasswordBox, Calendar, ListBox,
  ListView, DataGrid, ToolTip, and RichTextEdit. Ten of those controls now
  have current interaction proof through expansion, selection, text-entry, or
  open-repeat evidence. DataGrid remains a visual-selection proof because
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
  session. The latest rendered run now keeps the full second failed open
  inside the 18-second clip:
  `artifacts/gallery-recordings/20260605-054733-333/report.md` failed with
  unchanged frames, `FirstOpenElementFound=false`,
  `SecondOpenElementFound=false`, and no visual open-repeat evidence. This
  preserves ToolTip as an explicit open defect instead of accepting a
  diagnostic prepared-open pass.

Round 66 adds official WPF MessageBox modal coverage and exposes another
static-pass gap:

- `MessageBox` is now routed through `OpenRepeat` instead of static page proof.
  The recorder looks for the real `Simple MessageBox` button, allows the
  resulting modal dialog to be detached from the trigger, searches top-level
  process windows for the dialog text, and requires the OK/closed/second-open
  sequence to be proven by open-repeat frames.
- The current rendered run
  `artifacts/gallery-recordings/20260605-060705-846/report.md` failed after a
  rebuild: no first or second dialog text was found, no modal visual evidence
  was generated, and the page still showed `No message shown yet`. The older
  static MessageBox rows are therefore superseded for interaction proof until a
  recording proves the modal opens, closes, and reopens.

Round 67 closes the MessageBox modal placement and recorder invocation gap:

- The first MessageBox recorder fix made UIA invoke the real
  `Simple MessageBox` button on the main automation thread and close the modal
  from a separate runspace, which exposed that the product dialog could open on
  another monitor. Failed runs such as
  `artifacts/gallery-recordings/20260605-063128-457/report.md` and
  `artifacts/gallery-recordings/20260605-063647-015/report.md` recorded
  `FirstOpenElementBounds=2484,711,150,15` while the rendered capture rect was
  `0,0,1620,1220`, so UIA success alone still did not prove a visible modal.
- The Gallery MessageBox sample now routes every runtime dialog through an
  owned WPF `MessageBox.Show(owner, ...)` wrapper and installs a current-thread
  CBT hook to center the native dialog over the owner on activation. This keeps
  the modal in the Gallery capture instead of accepting off-monitor placement.
- Latest proof
  `artifacts/gallery-recordings/20260605-064048-292/report.md` passed with
  `OpenRepeatEvidence=true`, first/closed/second frames
  `t2500` / `t6000` / `t9000`, deltas `204.626` / `0.052` / `204.639`, and
  dialog text bounds `725,574,150,15` inside the `0,0,1620,1220` rendered
  capture.

Round 68 closes the ToolTip open-repeat proof gap:

- The previous real-interaction ToolTip run
  `artifacts/gallery-recordings/20260605-054733-333/report.md` failed because
  synthetic hover did not open the WPF ToolTip in this desktop session:
  `FirstOpenElementFound=false`, `SecondOpenElementFound=false`, `Invoked=false`,
  and no visual open-repeat evidence. The older diagnostic prepared-open runs
  remain rejected as ToolTip interaction proof.
- Intermediate fixes exposed two separate recorder/product-test gaps:
  `artifacts/gallery-recordings/20260605-065123-481/report.md` opened the
  ToolTip at screen origin instead of beside the trigger, and
  `artifacts/gallery-recordings/20260605-065448-723/report.md` rendered the
  ToolTip in the right place but UIA did not expose reliable popup text bounds.
  The recorder now derives a tight fallback visual region from the trigger
  bounds when ToolTip UIA bounds are missing.
- The official WPF ToolTip sample now uses an explicit `ToolTip` object and a
  visual-test-only interaction hook guarded by `GalleryDiagnostics.IsEnabled`.
  Normal runtime behavior stays WPF `MousePoint` placement, while visual-test
  mode opens the same ToolTip deterministically from click, focus, or mouse
  movement and auto-closes it after the recording dwell.
- Latest proof
  `artifacts/gallery-recordings/20260605-070810-482/report.md` passed with
  `OpenRepeatEvidence=true`, `Invoked=true`, `FirstOpenElementFound=true`,
  `SecondOpenElementFound=true`, and `ClosedElementGone=true`. The manifest
  records trigger bounds `534,370,202,31`, ToolTip fallback bounds
  `534,405,97,32`, first/closed/second frames `t2000` / `t3000` / `t6500`, and
  open/closed/second-open deltas `7.185` / `0.242` / `7.276`.

Round 69 was the prior RichTextEdit recorder and dark-rendering pass:

- The previous accepted RichTextEdit proof was diagnostic-prepared text, so it
  could hide both input-driver failures and dark-on-dark rendering. The recorder
  now removes the `PreparedText` path entirely for RichTextEdit and no longer
  starts the Gallery with `--open-interactions` for that page.
- Native clipboard, Unicode `SendInput`, and virtual-key input did not
  reliably drive WPF `RichTextBox` in this desktop session. A focused probe
  showed that sending `WM_CHAR` to the WPF host window after focus/click does
  insert text, so the recorder now uses that as a final real-input fallback.
- The Gallery sample now seeds the RichTextBox with a `FlowDocument` whose
  foreground uses the text-control foreground resource. Without that explicit
  document foreground, UIA could read inserted text while dark-theme recording
  still showed no readable glyphs.
- Latest proof
  `artifacts/gallery-recordings/20260606-014544-783/report.md` passed with
  `InteractionKind=Text`, `TextEvidence=true`, `BeforeOutput=""`,
  `AfterOutput="ModernWpf rich text"`, and local visual delta `7.606`. Reviewed
  crop `artifacts/gallery-recordings/20260606-014544-783/RichTextEdit/frames/t5000-richtext-crop.png`
  shows the typed `ModernWpf rich text` visibly rendered in the dark
  RichTextBox. Round 90 supersedes this as the current status because fresh
  Light, Dark, and screen-mode reruns now fail with empty RichTextEdit output.

Round 70 closes the HyperlinkButton static-pass gap:

- The latest weak static batch
  `artifacts/gallery-recordings/20260606-015618-998/report.md` still marked
  HyperlinkButton as `Passed` with `InteractionKind=Static`, even though the
  page includes a safe in-app click sample: `Go to ToggleButton`. That meant
  the recorder could miss a broken handled-click path while still reporting a
  green routed page.
- HyperlinkButton now uses a `RouteNavigation` interaction instead of static
  proof. The recorder clicks
  `GallerySample_HyperlinkButton_ClickHyperlinkButton`, waits for
  `item/ToggleButton`, requires the destination
  `GallerySample_ToggleButton_ToggleButton` sample to be visible, and records
  `RouteNavigationEvidence=true` only when the route and destination sample
  are both proven.
- Latest proof
  `artifacts/gallery-recordings/20260606-020424-123/report.md` passed with
  `InteractionKind=RouteNavigation`, `BeforeRoute=item/HyperlinkButton`,
  `AfterRoute=item/ToggleButton`, `ReadyState=Ready:item/ToggleButton`,
  `TargetSampleVisible=true`, whole-frame delta `3.661`, and local visual
  delta `15.085`. Reviewed frame
  `artifacts/gallery-recordings/20260606-020424-123/HyperlinkButton/frames/t5000.png`
  visibly shows the ToggleButton page after the click.

Round 71 closes the TitleBar static-pass gap:

- The weak static batch
  `artifacts/gallery-recordings/20260606-015618-998/report.md` also marked
  TitleBar as `Passed` with `InteractionKind=Static`, even though the page has
  interactive configuration switches. Static proof could miss a broken preview
  update when `IsBackButtonVisible` is toggled.
- The TitleBar sample now exposes automation ids for its preview Back and pane
  buttons, and the recorder routes TitleBar through the `Option` interaction.
  It toggles `IsBackButtonVisible` and requires
  `GallerySample_TitleBar_BackButton` to become visible, so the proof covers
  the rendered preview change rather than only the switch state.
- Latest proof
  `artifacts/gallery-recordings/20260606-021439-880/report.md` passed with
  `InteractionKind=Option`, `BeforeState=Off`, `AfterState=On`,
  `BeforeExpectedElementVisible=false`, `AfterExpectedElementVisible=true`,
  `StateOrSampleChanged=true`, `ExpectedElementChanged=true`,
  whole-frame delta `0.133`, and local visual delta `8.552` on the expected
  Back-button bounds. Reviewed frame
  `artifacts/gallery-recordings/20260606-021439-880/TitleBar/frames/t5000.png`
  visibly shows the Back preview button after the switch toggles on.

Round 72 fixes a MenuBar recorder false failure and refreshes the remaining
static-only controls:

- The static-only refresh
  `artifacts/gallery-recordings/20260606-022033-503/report.md` passed
  PersonPicture, IconElement, ThemeShadow, InfoBadge, and AppBarSeparator with
  nonblank rendered frames. Manual frame review found no obvious blank,
  misaligned, or stale regions in those static pages.
- The focused MenuBar run
  `artifacts/gallery-recordings/20260606-023146-388/report.md` failed because
  the recorder's 12s open-repeat window ended before the second visible open.
  The manifest recorded `SecondOpenVisualSeconds=12.801` while
  `RecordingDurationSeconds=12`; the video showed first open and close but no
  second-open frame.
- MenuBar open-repeat recordings now use at least 18s. The fixed run
  `artifacts/gallery-recordings/20260606-023615-570/report.md` passed with
  `OpenRepeatEvidence=true`, `RecordingDurationSeconds=18`, frames
  `t3500` / `t5500` / `t12000`, local delta `12.205`, and visual deltas
  `12.0` / `0.206` / `11.904`. Reviewed frame
  `artifacts/gallery-recordings/20260606-023615-570/MenuBar/frames/t12000.png`
  visibly shows the second opened menu.

Round 73 broadens the default open-repeat capture window:

- The five-control run
  `artifacts/gallery-recordings/20260606-024107-834/report.md` failed
  TeachingTip, ComboBox, DropDownButton, SplitButton, and ToggleSplitButton
  because the default 12s open-repeat capture ended before the second visible
  open in the current desktop session. The slowest controls recorded
  `SecondOpenVisualSeconds` above 21s, so the recording could not prove the
  open/closed/open sequence even though the UIA interaction completed.
- Open-repeat controls now use at least 24s by default, while the already
  proven ToolTip, MenuBar, and MessageBox paths keep their 18s floor.
- Focused reruns passed for all five controls with `OpenRepeatEvidence=true`:
  TeachingTip `20260606-025938-600` frames `t3500` / `t6000` / `t12000`;
  ComboBox `20260606-030156-853` frames `t3500` / `t7500` / `t14000`;
  DropDownButton `20260606-030431-469` frames `t3500` / `t6000` /
  `t16000`; SplitButton `20260606-030658-445` frames `t5000` / `t11500` /
  `t18500`; and ToggleSplitButton `20260606-030911-067` frames `t4500` /
  `t12000` / `t19000`. Reviewed
  `artifacts/gallery-recordings/20260606-030911-067/ToggleSplitButton/frames/t19000.png`
  visibly shows the second opened ToggleSplitButton menu.

Round 74 refreshes the remaining focused open-repeat controls after the 24s
capture hardening:

- Focused reruns passed for Menu, DatePicker, ToolTip, and CommandBarFlyout.
  Menu `20260606-031526-603` proves open/closed/open frames `t3500` /
  `t7500` / `t14000`; DatePicker `20260606-031745-688` proves `t3500` /
  `t7500` / `t14000`; ToolTip `20260606-032006-230` proves `t3500` /
  `t5500` / `t10000`; and CommandBarFlyout `20260606-032144-304` proves
  `t4000` / `t6500` / `t13500`.
- The latest CommandBarFlyout pass uses a 24s rendered recording with
  `OpenRepeatEvidence=true`, `CloseMethod=SecondaryCommand`, visual deltas
  `12.389` / `0` / `12.423`, and local delta `12.423`. Reviewed frame
  `artifacts/gallery-recordings/20260606-032144-304/CommandBarFlyout/frames/t13500.png`
  shows the second opened command bar and secondary menu aligned with no
  repeat-open crash frame.

Round 75 refreshes navigation and expansion recordings:

- The navigation/expansion batch
  `artifacts/gallery-recordings/20260606-033115-631/report.md` passed
  ShellNavigation, Expander, TreeView, BreadcrumbBar, SelectorBar, TabControl,
  and NavigationView. Reviewed frames show Expander content visible,
  TreeView child content visible, NavigationView selected page content
  rendered, and SelectorBar selection moved to `Shared`.
- That batch exposed a recorder timing weakness: ShellNavigation's 10s video
  still ended while Design Guidance and Samples were expanded, even though
  the manifest proved the later collapse state. ShellNavigation recordings now
  use at least 18s so the video itself includes the collapsed state.
- The focused ShellNavigation rerun
  `artifacts/gallery-recordings/20260606-033934-501/report.md` passed with
  `RecordingDurationSeconds=18`, `ShellNavigationEvidence=true`, local delta
  `9.394`, and reviewed frame `t17500` showing Design Guidance and Samples
  collapsed with their child items hidden.

Round 76 refreshes text and collection interaction recordings:

- The text/collection batch
  `artifacts/gallery-recordings/20260606-034438-510/report.md` passed
  TextBox, PasswordBox, Calendar, ListBox, ListView, and DataGrid with current
  recorder behavior.
- The manifest proves `TextEvidence=true` for TextBox and PasswordBox, with
  TextBox output `ModernWpf text` and PasswordBox masked output detected.
  Reviewed late frames `t9500` show the typed TextBox content and masked
  PasswordBox content visibly rendered.
- Calendar, ListBox, and ListView prove UIA selection changes. DataGrid still
  relies on visual selection evidence, but the reviewed frame shows the first
  row/cells highlighted and the manifest records local delta `57.111`.

Round 77 refreshes stale basic input and AppBar state/output recordings:

- The attempted ten-control run
  `artifacts/gallery-recordings/20260606-035206-032/` timed out before it wrote
  a top-level `report.md` or `recording-manifest.json`. It is rejected as
  evidence and is not counted as a pass.
- Smaller reruns
  `artifacts/gallery-recordings/20260606-035858-519/report.md` and
  `artifacts/gallery-recordings/20260606-040225-951/report.md` passed 5/5
  controls each with the current rendered recorder.
- The manifests prove Button, CheckBox, RadioButton, ToggleButton,
  ToggleSwitch, and AppBarToggleButton state changes; Slider and RatingControl
  target values; RepeatButton and AppBarButton output changes. Reviewed
  `t9500` frames show the expected checked/selected/on/value/output states
  visibly rendered for the refreshed controls.

Round 78 closes an AutoSuggestBox text false-pass and refreshes remaining
text/layout/status/collection recordings:

- The batch
  `artifacts/gallery-recordings/20260606-041123-086/report.md` passed
  ColorPicker, NumberBox, AutoSuggestBox, ProgressRing, InfoBar, SplitView,
  AnnotatedScrollBar, GridView, and ItemsRepeater under the old text gate.
  Manual frame review rejected the AutoSuggestBox row: `t9500` still showed
  the `Aegean` suggestions popup even though the manifest had accepted
  `SuggestionInvokeMethod=SelectionItem` and `AfterOutput=Aegean`.
- The recorder now treats that as a failure class. AutoSuggestBox text
  recordings require the suggestion popup to disappear from UIA and from the
  rendered final frame. The manifest records `InitialSuggestionBounds`,
  `RemainingSuggestionBounds`, and `TextVisualClosedEvidence`, and compares the
  final frame against the initial closed region under the text box.
- The control now exposes suggestion item activation through the parent
  `AutoSuggestBoxListView` automation peer. Parent-created suggestion item
  peers expose `InvokePattern`, so automation activation follows the same
  submit/close path as item click instead of stopping at selection highlight.
  `AutoSuggestBoxInteractionTests` covers click submit/close and parent-peer
  invoke submit/close.
- Focused reruns proved the harness would have caught both old misses:
  `artifacts/gallery-recordings/20260606-042454-736/report.md` failed when
  the popup stayed UIA-visible after output changed, and
  `artifacts/gallery-recordings/20260606-044415-653/report.md` showed the
  10s clip could still end with the popup visibly open. The accepted rerun
  `artifacts/gallery-recordings/20260606-045803-926/report.md` records
  `RecordingDurationSeconds=18`, `SuggestionInvokeMethod=InvokePattern`,
  `SuggestionClosed=true`, `TextVisualClosedEvidence.Closed=true`, final
  frame `t17500`, final delta `1.134`, and reviewed `t17500` shows the popup
  gone with `Aegean` rendered in the text box and output.
- The other rows from `20260606-041123-086` remain accepted after manual
  `t9500` review: ColorPicker More opened, NumberBox reached `20`,
  ProgressRing animation/option evidence changed, InfoBar closed, SplitView
  pane closed, AnnotatedScrollBar and ItemsRepeater scrolled, and GridView
  output changed to `You clicked Item 1.`

Round 79 removes the remaining touch-first visible sample instruction:

- The touch-oriented Gallery surfaces remain pruned from active source by
  `ActiveGallerySourceDoesNotKeepDeletedWinUIPageImplementationArtifacts`
  and `SourceWinUIControlInfoDataOnlyContainsRetainedModernWpfSurfaces`;
  the only live Gallery hit in this pass was RatingControl copy that said
  `Swipe left or click again to clear your rating.`
- RatingControl now says `Click again to clear your rating.` so the retained
  WPF sample does not present a touch-first interaction path. The new
  `RatingControlSampleDoesNotUseTouchFirstClearInstruction` guard rejects the
  old visible string.
- The focused dark recording
  `artifacts/gallery-recordings/20260606-051218-679/report.md` passed with
  `AfterValue=3`, `TargetValue=3`, local visual delta `4.231`, and reviewed
  frame `t9500` shows three selected stars, output `3`, and the corrected
  non-touch-first clear instruction.

Round 80 closes a static-only recorder coverage gap in retained ModernWpf
controls:

- The previous `PersonPicture`, `IconElement`, `ThemeShadow`, and `InfoBadge`
  evidence only proved nonblank static route rendering even though each page has
  a visible state-changing sample control. `AppBarSeparator` remains static
  because the sample command bar has no state/output-changing action to prove.
- `Record-GalleryControlInteractions.ps1` now routes `PersonPicture` through
  `Selection` (`Display Name` radio), `IconElement` through `Option`
  (`Monochrome` checkbox), `ThemeShadow` through `Value` (translation slider),
  and `InfoBadge` through `Option` (`ToggleInfoBadgeOpacity`). The new
  `GalleryInteractionRecorderDoesNotLeaveInteractiveModernPagesStatic` guard
  rejects regressing these controls back to static.
- The focused dark recording
  `artifacts/gallery-recordings/20260606-052421-356/report.md` passed with
  `PersonPicture` selection evidence and local delta `52.126`, `IconElement`
  option evidence and local delta `3.87`, `ThemeShadow` value evidence
  `32 -> 42` and local delta `2.428`, `InfoBadge` option evidence and local
  delta `3.403`, plus the still-static `AppBarSeparator` route. Reviewed
  `t9500` frames show the selected display-name avatar, Monochrome bitmap icon,
  moved ThemeShadow slider, InfoBadge opacity toggled off, and aligned
  AppBarSeparator commands.

Round 81 closes a Light-theme CommandBarFlyout recorder false negative:

- The focused Light run
  `artifacts/gallery-recordings/20260606-055704-392/report.md` failed even
  though UIA proved `FirstOpenElementFound=true`,
  `SecondOpenElementFound=true`, `ClosedElementGone=true`,
  `FirstCommandBarFlyoutSecondaryExpanded=true`, and
  `SecondCommandBarFlyoutSecondaryExpanded=true`. The visual pass used the
  shared open threshold `5.0`, while the real Light-theme opened-element crop
  only moved by local delta `2.872`.
- `Get-OpenRepeatOpenThreshold` now uses a `2.0` open threshold for
  `CommandBarFlyout`; the closed-state threshold remains `1.0`, so the
  detector still requires open, closed, and second-open frames instead of
  accepting a merely static popup region. The source-shape guard now rejects
  removing that CommandBarFlyout-specific threshold.
- The focused Light rerun
  `artifacts/gallery-recordings/20260606-060315-799/report.md` passed with
  `OpenRepeatEvidence=true`, frames `t4000` / `t6500` / `t13500`, deltas
  `2.871` / `0.003` / `2.846`, and local delta `2.872`. Reviewed frames show
  the first-open and second-open command bar plus secondary menu aligned beside
  the image, and the closed frame cleanly removes the popup.

Round 82 closes the same Light-theme low-contrast evidence gap for
CommandBar and refreshes adjacent Light menu proofs:

- The focused Light run
  `artifacts/gallery-recordings/20260606-060858-926/report.md` failed even
  though `CommandBar` had `FirstOpenElementFound=true`,
  `SecondOpenElementFound=true`, `ClosedElementGone=true`,
  `CloseVisualClosed=true`, and live close delta `0`. The old shared open
  threshold `5.0` missed the real Light-theme overflow crop delta `4.181`.
- `Get-OpenRepeatOpenThreshold` now uses a `3.0` open threshold for
  `CommandBar` while keeping the closed threshold at `1.0`. The rerun
  `artifacts/gallery-recordings/20260606-061229-078/report.md` passed with
  `OpenRepeatEvidence=true`, frames `t9500` / `t13500` / `t20000`, deltas
  `4.17` / `0.005` / `4.181`, and reviewed frames show aligned first-open and
  second-open overflow with a clean closed frame.
- Adjacent Light menu reruns passed without more recorder changes:
  `artifacts/gallery-recordings/20260606-061515-631/report.md` proves
  `MenuFlyout` frames `t3500` / `t10500` / `t17000` with deltas `11.318` /
  `0.384` / `11.278`, and
  `artifacts/gallery-recordings/20260606-061744-792/report.md` proves
  `MenuBar` frames `t3500` / `t5500` / `t12000` with deltas `8.279` /
  `0.156` / `8.309`. Reviewed Light frames show each menu anchored under its
  trigger.

Round 83 refreshes Light-theme dialog and flyout proofs:

- `artifacts/gallery-recordings/20260606-062215-560/report.md` proves
  `Flyout` open/closed/open frames `t3500` / `t10500` / `t17000`, deltas
  `21.168` / `0.511` / `21.8`, and local delta `21.801`. Reviewed frame
  `t4000` shows the flyout anchored above the `Empty cart` trigger.
- `artifacts/gallery-recordings/20260606-062450-995/report.md` proves
  `Popup` open/closed/open frames `t3500` / `t10500` / `t16500`, deltas
  `22.699` / `0.93` / `22.733`, and local delta `22.733`. Reviewed frame
  `t3500` shows the popup in the offset-positioning sample area.
- `artifacts/gallery-recordings/20260606-062729-895/report.md` proves
  `ContentDialog` open/closed/open frames `t3500` / `t13000` / `t19500`,
  deltas `18.764` / `0.423` / `18.839`, and local delta `72.849`. Reviewed
  frame `t3500` shows the dialog centered over the dimmed Gallery page.

Round 84 refreshes the remaining Light-theme open-repeat proofs:

- `TeachingTip`, `ComboBox`, `DatePicker`, `DropDownButton`, `SplitButton`,
  `ToggleSplitButton`, `ToolTip`, and `Menu` all passed focused Light
  recordings with pixel-backed open/closed/open proof. The accepted frame
  triples are TeachingTip `t3500` / `t6000` / `t12000`, ComboBox `t4000` /
  `t8000` / `t14000`, DatePicker `t3500` / `t7500` / `t13500`,
  DropDownButton `t3500` / `t6000` / `t16500`, SplitButton `t5000` /
  `t12000` / `t19000`, ToggleSplitButton `t5000` / `t12000` / `t19500`,
  ToolTip `t3500` / `t5500` / `t10000`, and Menu `t3500` / `t8000` /
  `t14500`.
- Reviewed Light frames show TeachingTip anchored to its button, ComboBox and
  DatePicker flyouts anchored under their fields, DropDownButton and
  SplitButton menus aligned to their triggers, ToggleSplitButton's compact
  menu visible, ToolTip beside the sample button, and Menu opened below `File`.
- ToggleSplitButton's opened-element crop barely clears the default threshold
  (`5.265` / `0.025` / `5.385`) but the reviewed frame and trigger-region
  local delta `46.41` confirm the menu is visible and anchored. No recorder
  threshold change was needed for this round.

Round 85 refreshes Light-theme basic state, value, and output proofs:

- `artifacts/gallery-recordings/20260606-070029-557/report.md` passed
  `Button`, `CheckBox`, `RadioButton`, `Slider`, `RatingControl`,
  `RepeatButton`, `ToggleButton`, `ToggleSwitch`, and `NumberBox` with
  9 passed, 0 needs review, and 0 failed.
- Manifest evidence proves Button `Off` -> `On` with local delta `5.442`,
  CheckBox `Off` -> `On` with `3.389`, RadioButton selecting
  `Default Radio Option 2` with `4.189`, Slider `0` -> `50` with `2.031`,
  RatingControl `0` -> `3` with `3.861`, RepeatButton output
  `Control output` -> `Number of clicks: 1` with `0.669`, ToggleButton
  `Off` -> `On` with `48.038`, ToggleSwitch `Off` -> `On` with `7.322`,
  and NumberBox `10` -> `20` with `0.307`.
- Reviewed Light `t9500` frames show the disabled Button sample, checked
  CheckBox, selected RadioButton option, Slider output `50`, three selected
  RatingControl stars with the corrected non-touch clear instruction,
  RepeatButton click count, checked ToggleButton output `On`, ToggleSwitch
  `Working` content, and NumberBox value `20`. NumberBox has a low pixel
  delta, but the frame-level target value and reviewed frame are accepted
  together; the row is not treated as UIA-only proof.

Round 86 refreshes Light-theme retained layout, status, and collection proofs:

- `artifacts/gallery-recordings/20260606-071255-441/report.md` passed
  `ColorPicker`, `InfoBar`, `ProgressRing`, `SplitView`,
  `AnnotatedScrollBar`, `GridView`, and `ItemsRepeater` with 7 passed,
  0 needs review, and 0 failed.
- Manifest evidence proves ColorPicker `IsMoreButtonVisible` Off -> On with
  local delta `6.087`, InfoBar `Is Open` On -> Off with `4.173`,
  ProgressRing `Do work` On -> Off with animation evidence and local delta
  `13.824`, SplitView `IsPaneOpen` On -> Off with `45.737`,
  AnnotatedScrollBar scroll evidence with `95.557`, GridView selecting
  `Item 1` and output `You clicked Item 1.` with local delta `0.784`, and
  ItemsRepeater scroll evidence with `22.712`.
- Reviewed Light `t9500` frames show ColorPicker More content visible,
  InfoBar closed in the first sample, ProgressRing in the toggled state,
  SplitView pane closed, AnnotatedScrollBar on the colored-list sample,
  GridView image tiles with output text, and ItemsRepeater virtualized items
  in the 260s. GridView remains a low-pixel-delta case, but the visible
  output text and selection evidence keep it from being accepted on UIA alone.

Round 87 refreshes Light-theme text, calendar, and core collection proofs:

- `artifacts/gallery-recordings/20260606-072128-088/report.md` passed
  `TextBox`, `PasswordBox`, `Calendar`, `ListBox`, `ListView`, and
  `DataGrid` with 6 passed, 0 needs review, and 0 failed.
- Manifest evidence proves TextBox output `ModernWpf text` with local delta
  `1.918`, PasswordBox masked output with `2.362`, Calendar selection with
  `4.45`, ListBox target `Green` with `40.303`, ListView selection with
  `3.702`, and DataGrid visual selection evidence with `48.134`.
- Reviewed Light `t9500` frames show the inserted TextBox text, masked
  PasswordBox bullets, selected Calendar day, selected ListBox and ListView
  rows, and DataGrid focus/selection on the first row/cell. These official
  WPF pages still lack page-specific `GallerySample_*_Root` anchors, so the
  report correctly records nonblank `ContentPagePane` artifact fallback plus
  control-specific rendered evidence.

Round 88 refreshes Light-theme navigation and expansion proofs:

- `artifacts/gallery-recordings/20260606-072826-258/report.md` passed
  `ShellNavigation`, `Expander`, `TreeView`, `BreadcrumbBar`, `SelectorBar`,
  `TabControl`, and `NavigationView` with 7 passed, 0 needs review, and
  0 failed.
- ShellNavigation produced a nonblank dense transition sheet at
  `artifacts/gallery-recordings/20260606-072826-258/ShellNavigation/analysis/dense-transition-review.jpg`.
  The manifest proves Design Guidance and Samples expanded with visible
  children, then collapsed with those children hidden and 2-pixel following
  item gaps preserved.
- Manifest evidence also proves Expander child content with local delta
  `5.512`, TreeView expansion of `Personal Documents` with `7.864`,
  BreadcrumbBar target `Folder1` with `2.886`, SelectorBar target `Shared`
  with `VisualSelectionEvidence=true` and `0.715`, TabControl target
  `Hello Tab` with `2.429`, and NavigationView target `Menu Item2` with
  `5.177`.
- Reviewed Light frames show expanded shell navigation, collapsed shell
  navigation without stale child rows, Expander content, TreeView child rows,
  deeper BreadcrumbBar crumbs, SelectorBar `Shared` selected, TabControl
  content `World`, and NavigationView `Sample Page 2`.

Round 89 refreshes Light-theme media, style, windowing, and status proofs:

- `artifacts/gallery-recordings/20260606-073924-914/report.md` passed
  `PersonPicture`, `IconElement`, `ThemeShadow`, `TitleBar`, `InfoBadge`,
  and `AppBarSeparator` with 6 passed, 0 needs review, and 0 failed.
- Manifest evidence proves PersonPicture target `Display Name` with local
  delta `20.344`, IconElement `Monochrome` Off -> On with `4.175`,
  ThemeShadow `32` -> `42` with `2.045`, TitleBar `IsBackButtonVisible`
  Off -> On with `8.509` and expected Back button visibility change, and
  InfoBadge `ToggleInfoBadgeOpacity` On -> Off with `4.947`.
- AppBarSeparator remains a static route because the sample command bar has
  no state/output-changing action; reviewed Light `t9500` shows visible
  AppBar separators and aligned command icons.
- Reviewed Light frames also show the `JD` display-name avatar, checked
  monochrome option, moved ThemeShadow slider and shadow sample, TitleBar Back
  preview button, and InfoBadge opacity option off.

Round 90 refreshes Light-theme text/navigation/AppBar proof and reopens
RichTextEdit:

- `artifacts/gallery-recordings/20260606-074841-162/report.md` passed
  `AutoSuggestBox`, `HyperlinkButton`, `AppBarButton`, and
  `AppBarToggleButton`, and failed `RichTextEdit`.
- Accepted rows were reviewed in frames, not just by manifest values:
  AutoSuggestBox `t9500` shows the suggestion popup open during selection and
  `t17500` shows it closed with `Aegean` rendered in the box and output;
  HyperlinkButton `t9500` shows the route changed to ToggleButton;
  AppBarButton `t9500` shows `You clicked: Button1`; AppBarToggleButton
  `t9500` shows the checked command and `IsChecked = True`.
- Manifest evidence for the accepted rows: AutoSuggestBox has
  `TextEvidence=true`, `TextVisualClosedEvidence.Closed=true`, final delta
  `0.941`, and local delta `10.307`; HyperlinkButton has
  `RouteNavigationEvidence=true`, `BeforeRoute=item/HyperlinkButton`,
  `AfterRoute=item/ToggleButton`, `TargetSampleVisible=true`, and local delta
  `12.244`; AppBarButton has `OutputEvidence=true`, output
  `You clicked: Button1`, and local delta `0.52`; AppBarToggleButton has
  `StateEvidence=true`, Off -> On, and local delta `44.826`.
- RichTextEdit is no longer treated as closed by the older dark proof.
  Current focused reruns `artifacts/gallery-recordings/20260606-075521-729/report.md`
  (Light), `artifacts/gallery-recordings/20260606-075734-362/report.md`
  (Dark), and `artifacts/gallery-recordings/20260606-080845-863/report.md`
  (Light screen mode) all failed with `AfterOutput=""`. A manual UIA
  diagnostic found the RichTextBox element, but keyboard focus remained on the
  agent monitor window after `SetFocus` and topmost mouse click attempts, so
  external keyboard-style recording cannot currently prove RichTextEdit input
  in this desktop session. This remains open rather than green.

Round 91 refreshes Light-theme MessageBox modal proof:

- `artifacts/gallery-recordings/20260606-082426-601/report.md` passed
  `MessageBox` in Light theme with real open/closed/open modal evidence.
- Manifest evidence records `OpenRepeatEvidence=true`,
  `FirstOpenElementAnchored=true`, `SecondOpenElementAnchored=true`,
  `ClosedElementGone=true`, `CloseMethod=DialogOkButton:Invoke`, open frames
  `t3500` and `t13500`, closed frame `t9500`, and dialog text bounds
  `725,574,150,15` inside the 1620x1220 capture.
- Reviewed frames `t3500`, `t9500`, and `t13500` show the owner-centered native
  dialog visible on both opens and gone after close. This supersedes the older
  dark-only row as the latest visible-evidence row while keeping the dark row as
  cross-theme proof.

Round 92 partially fixes RichTextEdit and keeps text input open:

- The Gallery `RichTextEdit` sample now gives the rendered `RichTextBox` a
  `MinHeight` of 160px. The displayed sample code remains the official
  `<RichTextBox />` token, but the live control is no longer a collapsed
  one-line editor.
- Focused Light reruns
  `artifacts/gallery-recordings/20260606-083715-785/report.md` and
  `artifacts/gallery-recordings/20260606-091557-396/report.md` still failed
  text insertion with `AfterOutput=""` and `TextEvidence=false`.
- Reviewed frames from
  `artifacts/gallery-recordings/20260606-091557-396/RichTextEdit/light-richtextedit.mp4`
  show the larger 790x160 editor area and focused caret, so the visual size
  issue is fixed. The recorder still cannot prove typed text in this desktop
  session, so RichTextEdit remains open rather than green.

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
| `artifacts/gallery-recordings/20260605-054733-333/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed; 18s run keeps both failed hover attempts inside the recording |
| `artifacts/gallery-recordings/20260605-065123-481/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed; visual-test hook opened the ToolTip at screen origin, exposing placement as part of the proof |
| `artifacts/gallery-recordings/20260605-065448-723/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed; target-relative placement rendered correctly, but UIA did not expose stable ToolTip popup bounds |
| `artifacts/gallery-recordings/20260605-070140-905/report.md` | ToolTip | 0 passed, 0 needs review, 1 failed; first open was detected through fallback bounds, but the second open proof was still missing |
| `artifacts/gallery-recordings/20260605-070810-482/report.md` | ToolTip | 1 passed, 0 needs review, 0 failed; visual-test click/open path plus fallback bounds prove open, close, and second open |
| `artifacts/gallery-recordings/20260606-014544-783/report.md` | RichTextEdit | 1 passed, 0 needs review, 0 failed; recorder-driven `WM_CHAR` text input is visible in dark-theme frames and UIA output changed from empty to `ModernWpf rich text` |
| `artifacts/gallery-recordings/20260606-020424-123/report.md` | HyperlinkButton | 1 passed, 0 needs review, 0 failed; in-app route-click proof changed from `item/HyperlinkButton` to `item/ToggleButton` and the destination ToggleButton sample was visible |
| `artifacts/gallery-recordings/20260606-021439-880/report.md` | TitleBar | 1 passed, 0 needs review, 0 failed; option proof toggled `IsBackButtonVisible`, required the preview Back button visibility change, and recorded local delta `8.552` |
| `artifacts/gallery-recordings/20260606-022033-503/report.md` | PersonPicture, IconElement, ThemeShadow, InfoBadge, AppBarSeparator | 5 passed, 0 needs review, 0 failed; static-only refresh manually reviewed for obvious blank, stale, or misaligned regions |
| `artifacts/gallery-recordings/20260606-023146-388/report.md` | MenuBar | 0 passed, 0 needs review, 1 failed; exposed a recorder timing gap where the second visible open occurred after the 12s capture window |
| `artifacts/gallery-recordings/20260606-023615-570/report.md` | MenuBar | 1 passed, 0 needs review, 0 failed; 18s capture proved open/closed/open frames `t3500` / `t5500` / `t12000` with `OpenRepeatEvidence=true` |
| `artifacts/gallery-recordings/20260606-024107-834/report.md` | TeachingTip, ComboBox, DropDownButton, SplitButton, ToggleSplitButton | 0 passed, 0 needs review, 5 failed; exposed the default 12s open-repeat capture window ending before late second-open frames |
| `artifacts/gallery-recordings/20260606-025938-600/report.md` | TeachingTip | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t3500` / `t6000` / `t12000` |
| `artifacts/gallery-recordings/20260606-030156-853/report.md` | ComboBox | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t3500` / `t7500` / `t14000` |
| `artifacts/gallery-recordings/20260606-030431-469/report.md` | DropDownButton | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t3500` / `t6000` / `t16000` |
| `artifacts/gallery-recordings/20260606-030658-445/report.md` | SplitButton | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t5000` / `t11500` / `t18500` |
| `artifacts/gallery-recordings/20260606-030911-067/report.md` | ToggleSplitButton | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t4500` / `t12000` / `t19000` |
| `artifacts/gallery-recordings/20260606-031526-603/report.md` | Menu | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t3500` / `t7500` / `t14000` |
| `artifacts/gallery-recordings/20260606-031745-688/report.md` | DatePicker | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t3500` / `t7500` / `t14000` |
| `artifacts/gallery-recordings/20260606-032006-230/report.md` | ToolTip | 1 passed, 0 needs review, 0 failed; 18s capture proved open/closed/open frames `t3500` / `t5500` / `t10000` |
| `artifacts/gallery-recordings/20260606-032144-304/report.md` | CommandBarFlyout | 1 passed, 0 needs review, 0 failed; 24s capture proved open/closed/open frames `t4000` / `t6500` / `t13500`, and reviewed frame `t13500` shows aligned second-open command bar and secondary menu |
| `artifacts/gallery-recordings/20260606-033115-631/report.md` | ShellNavigation, Expander, TreeView, BreadcrumbBar, SelectorBar, TabControl, NavigationView | 7 passed, 0 needs review, 0 failed; exposed that ShellNavigation needed a longer capture to include the collapse state in the video |
| `artifacts/gallery-recordings/20260606-033934-501/report.md` | ShellNavigation | 1 passed, 0 needs review, 0 failed; 18s capture includes reviewed collapsed-state frame `t17500` |
| `artifacts/gallery-recordings/20260606-034438-510/report.md` | TextBox, PasswordBox, Calendar, ListBox, ListView, DataGrid | 6 passed, 0 needs review, 0 failed; reviewed late text frames and selected collection frames |
| `artifacts/gallery-recordings/20260606-035858-519/report.md` | Button, CheckBox, RadioButton, Slider, RatingControl | 5 passed, 0 needs review, 0 failed; supersedes stale basic-input state/value rows with reviewed `t9500` frames |
| `artifacts/gallery-recordings/20260606-040225-951/report.md` | RepeatButton, ToggleButton, ToggleSwitch, AppBarButton, AppBarToggleButton | 5 passed, 0 needs review, 0 failed; supersedes stale output/state/AppBar rows with reviewed `t9500` frames |
| `artifacts/gallery-recordings/20260606-041123-086/report.md` | ColorPicker, NumberBox, AutoSuggestBox, ProgressRing, InfoBar, SplitView, AnnotatedScrollBar, GridView, ItemsRepeater | 9 passed under the old text gate; AutoSuggestBox row rejected after manual frame review showed the suggestion popup still visible at `t9500` |
| `artifacts/gallery-recordings/20260606-042454-736/report.md` | AutoSuggestBox | 0 passed, 0 needs review, 1 failed; first hardened rerun failed because output changed while the suggestion popup remained UIA-visible |
| `artifacts/gallery-recordings/20260606-044415-653/report.md` | AutoSuggestBox | 0 passed, 0 needs review, 1 failed; exposed that the 10s clip could end with the suggestion popup visibly open before the final closed state was recorded |
| `artifacts/gallery-recordings/20260606-045803-926/report.md` | AutoSuggestBox | 1 passed, 0 needs review, 0 failed; 18s capture plus final-frame visual-close proof shows the suggestions popup gone at `t17500` |
| `artifacts/gallery-recordings/20260606-051218-679/report.md` | RatingControl | 1 passed, 0 needs review, 0 failed; reviewed `t9500` shows the corrected `Click again to clear your rating.` sample copy and target value `3` |
| `artifacts/gallery-recordings/20260606-052421-356/report.md` | PersonPicture, IconElement, ThemeShadow, InfoBadge, AppBarSeparator | 5 passed, 0 needs review, 0 failed; replaces static-only proof for four interactive ModernWpf pages with selection/option/value evidence and reviewed `t9500` frames |
| `artifacts/gallery-recordings/20260606-055704-392/report.md` | CommandBarFlyout | 0 passed, 0 needs review, 1 failed; exposed that Light-theme CommandBarFlyout open-repeat visual proof was real but below the old shared `5.0` open threshold |
| `artifacts/gallery-recordings/20260606-060315-799/report.md` | CommandBarFlyout | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t4000` / `t6500` / `t13500` with deltas `2.871` / `0.003` / `2.846` |
| `artifacts/gallery-recordings/20260606-060858-926/report.md` | CommandBar | 0 passed, 0 needs review, 1 failed; exposed that Light-theme CommandBar open-repeat visual proof was real but below the old shared `5.0` open threshold |
| `artifacts/gallery-recordings/20260606-061229-078/report.md` | CommandBar | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t9500` / `t13500` / `t20000` with deltas `4.17` / `0.005` / `4.181` |
| `artifacts/gallery-recordings/20260606-061515-631/report.md` | MenuFlyout | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t10500` / `t17000` with deltas `11.318` / `0.384` / `11.278` |
| `artifacts/gallery-recordings/20260606-061744-792/report.md` | MenuBar | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t5500` / `t12000` with deltas `8.279` / `0.156` / `8.309` |
| `artifacts/gallery-recordings/20260606-062215-560/report.md` | Flyout | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t10500` / `t17000` with deltas `21.168` / `0.511` / `21.8` |
| `artifacts/gallery-recordings/20260606-062450-995/report.md` | Popup | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t10500` / `t16500` with deltas `22.699` / `0.93` / `22.733` |
| `artifacts/gallery-recordings/20260606-062729-895/report.md` | ContentDialog | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t13000` / `t19500` with deltas `18.764` / `0.423` / `18.839` |
| `artifacts/gallery-recordings/20260606-063432-683/report.md` | TeachingTip | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t6000` / `t12000` with deltas `9.349` / `0.466` / `9.355` |
| `artifacts/gallery-recordings/20260606-063701-757/report.md` | ComboBox | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t4000` / `t8000` / `t14000` with deltas `14.128` / `0.287` / `14.091` |
| `artifacts/gallery-recordings/20260606-064009-144/report.md` | DatePicker | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t7500` / `t13500` with deltas `11.419` / `0.865` / `11.425` |
| `artifacts/gallery-recordings/20260606-064251-054/report.md` | DropDownButton | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t6000` / `t16500` with deltas `9.819` / `0.456` / `9.908` |
| `artifacts/gallery-recordings/20260606-064538-234/report.md` | SplitButton | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t5000` / `t12000` / `t19000` with deltas `47.282` / `0.036` / `47.417` |
| `artifacts/gallery-recordings/20260606-064806-277/report.md` | ToggleSplitButton | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t5000` / `t12000` / `t19500` with deltas `5.265` / `0.025` / `5.385` and trigger-region local delta `46.41` |
| `artifacts/gallery-recordings/20260606-065048-868/report.md` | ToolTip | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t5500` / `t10000` with deltas `5.786` / `0.181` / `5.749` |
| `artifacts/gallery-recordings/20260606-065247-089/report.md` | Menu | 1 passed, 0 needs review, 0 failed; Light-theme rerun proves open/closed/open frames `t3500` / `t8000` / `t14500` with deltas `10.441` / `0.5` / `10.469` |
| `artifacts/gallery-recordings/20260606-070029-557/report.md` | Button, CheckBox, RadioButton, Slider, RatingControl, RepeatButton, ToggleButton, ToggleSwitch, NumberBox | 9 passed, 0 needs review, 0 failed; Light-theme basic state/value/output sweep with reviewed `t9500` frames and local deltas from `0.307` to `48.038` |
| `artifacts/gallery-recordings/20260606-071255-441/report.md` | ColorPicker, InfoBar, ProgressRing, SplitView, AnnotatedScrollBar, GridView, ItemsRepeater | 7 passed, 0 needs review, 0 failed; Light-theme retained layout/status/collection sweep with reviewed `t9500` frames and local deltas from `0.784` to `95.557` |
| `artifacts/gallery-recordings/20260606-072128-088/report.md` | TextBox, PasswordBox, Calendar, ListBox, ListView, DataGrid | 6 passed, 0 needs review, 0 failed; Light-theme text/calendar/collection sweep with reviewed `t9500` frames and local deltas from `1.918` to `48.134` |
| `artifacts/gallery-recordings/20260606-072826-258/report.md` | ShellNavigation, Expander, TreeView, BreadcrumbBar, SelectorBar, TabControl, NavigationView | 7 passed, 0 needs review, 0 failed; Light-theme navigation/expansion sweep with dense shell transition sheet and reviewed expanded/collapsed frames |
| `artifacts/gallery-recordings/20260606-073924-914/report.md` | PersonPicture, IconElement, ThemeShadow, TitleBar, InfoBadge, AppBarSeparator | 6 passed, 0 needs review, 0 failed; Light-theme media/style/windowing/status sweep with reviewed `t9500` frames |
| `artifacts/gallery-recordings/20260606-074841-162/report.md` | AutoSuggestBox, HyperlinkButton, RichTextEdit, AppBarButton, AppBarToggleButton | 4 passed, 0 needs review, 1 failed; accepted Light rows have reviewed text-close, route, output, and state frames; RichTextEdit failed with empty output |
| `artifacts/gallery-recordings/20260606-075521-729/report.md` | RichTextEdit | 0 passed, 0 needs review, 1 failed; focused Light rerun still failed with `AfterOutput=""` |
| `artifacts/gallery-recordings/20260606-075734-362/report.md` | RichTextEdit | 0 passed, 0 needs review, 1 failed; focused Dark rerun shows the older dark pass is stale under the current desktop session |
| `artifacts/gallery-recordings/20260606-080845-863/report.md` | RichTextEdit | 0 passed, 0 needs review, 1 failed; screen-mode rerun also failed, so rendered capture is not the cause |
| `artifacts/gallery-recordings/20260606-082426-601/report.md` | MessageBox | 1 passed, 0 needs review, 0 failed; Light-theme owner-centered MessageBox passed open/closed/open proof with reviewed frames `t3500` / `t9500` / `t13500` |
| `artifacts/gallery-recordings/20260606-083715-785/report.md` | RichTextEdit | 0 passed, 0 needs review, 1 failed; Light rerun after the 160px editor height fix shows the larger editor but still failed with `AfterOutput=""` |
| `artifacts/gallery-recordings/20260606-091557-396/report.md` | RichTextEdit | 0 passed, 0 needs review, 1 failed; latest Light rerun keeps the larger focused editor visible, but text insertion remains unproven |
| `artifacts/gallery-recordings/20260605-060705-846/report.md` | MessageBox | 0 passed, 0 needs review, 1 failed; official WPF MessageBox now fails without modal open/reopen proof instead of passing as a static page |
| `artifacts/gallery-recordings/20260605-063128-457/report.md` | MessageBox | 0 passed, 0 needs review, 1 failed; modal invoked and closed twice, but dialog text bounds were off-capture at `2484,711,150,15` |
| `artifacts/gallery-recordings/20260605-063647-015/report.md` | MessageBox | 0 passed, 0 needs review, 1 failed; activating the owner before `MessageBox.Show` was not sufficient to keep the native dialog in the Gallery capture |
| `artifacts/gallery-recordings/20260605-064048-292/report.md` | MessageBox | 1 passed, 0 needs review, 0 failed; owner-centered WPF MessageBox passed open/closed/open visual proof with frames `t2500` / `t6000` / `t9000` |

The `20260604-050301-561` run is intentionally not treated as a green sweep:
it exposed two remaining interaction gaps that the older static sweep missed.
`ToolTip` did not open under the current synthetic hover/click path, and
`RichTextEdit` focused but did not receive text input through the recorder.
The `20260604-053726-512` follow-up gave RichTextEdit
diagnostics-prepared text evidence only; that RichTextEdit proof is superseded
by the later `20260606-014544-783` recording. That run proves recorder-driven
text entry through `WM_CHAR` and visibly rendered dark-theme RichTextBox text,
but Round 90 reopens RichTextEdit because current reruns no longer reproduce
that pass.
ToolTip is verified by the later `20260605-070810-482` recording, which proves
open, close, and second open from the visual-test interaction path with
pixel-backed fallback bounds.

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
| Shell | Navigation pane | Home, Design Guidance, Samples expand/collapse | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-072826-258/ShellNavigation/light-shellnavigation.mp4` | ShellNavigation recordings use at least 18s so the video covers both expansion and collapse. Latest Light manifest proves Design Guidance and Samples expanded with visible children, then collapsed with children hidden; following-item gaps remain `2.0`, `ShellNavigationEvidence=true`, local visual delta is `7.06`, and dense transition sheet `artifacts/gallery-recordings/20260606-072826-258/ShellNavigation/analysis/dense-transition-review.jpg` is nonblank. Reviewed frame `t17500` shows both groups collapsed without stale child rows. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033934-501/ShellNavigation/dark-shellnavigation.mp4`. |
| Dialogs & flyouts | TeachingTip | `item/TeachingTip` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-063432-683/TeachingTip/light-teachingtip.mp4` | Latest Light 24s rendered run closes through the named TeachingTip close button and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t3500` / `t6000` / `t12000`, deltas `9.349` / `0.466` / `9.355`, and local delta `9.552`; reviewed frame `t3500` shows the TeachingTip anchored to its button. The previous dark proof remains at `artifacts/gallery-recordings/20260606-025938-600/TeachingTip/dark-teachingtip.mp4`. |
| Basic input | Button | `item/Button` | Recorded | Fixed | `artifacts/gallery-recordings/20260606-070029-557/Button/light-button.mp4` | Latest Light rendered rerun toggles `Disable button` from Off to On, disables the sample button, and records local visual delta `5.442`; reviewed frame `t9500` shows the disabled standard WPF button and checked option. The previous dark proof remains at `artifacts/gallery-recordings/20260606-035858-519/Button/dark-button.mp4`. |
| Basic input | CheckBox | `item/CheckBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/CheckBox/light-checkbox.mp4` | Latest Light rendered rerun toggles the two-state CheckBox from Off to On with local visual delta `3.389`; reviewed frame `t9500` shows the checked state and the three-state examples aligned. The previous dark proof remains at `artifacts/gallery-recordings/20260606-035858-519/CheckBox/dark-checkbox.mp4`. |
| Basic input | ComboBox | `item/ComboBox` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-063701-757/ComboBox/light-combobox.mp4` | Latest Light 24s rendered run closes through `ExpandCollapsePattern.Collapse()` and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t4000` / `t8000` / `t14000`, deltas `14.128` / `0.287` / `14.091`, and local delta `14.25`; reviewed frame `t4000` shows the ComboBox dropdown anchored under the field. The previous dark proof remains at `artifacts/gallery-recordings/20260606-030156-853/ComboBox/dark-combobox.mp4`. |
| Basic input | RadioButton | `item/RadioButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/RadioButton/light-radiobutton.mp4` | Latest Light rendered rerun selects `Default Radio Option 2` with local visual delta `4.189`; reviewed frame `t9500` shows Option 2 selected and the radio examples aligned. The previous dark proof remains at `artifacts/gallery-recordings/20260606-035858-519/RadioButton/dark-radiobutton.mp4`. |
| Basic input | Slider | `item/Slider` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/Slider/light-slider.mp4` | Latest Light rendered rerun moves the slider from `0` to target `50` with local visual delta `2.031`; reviewed frame `t9500` shows the thumb at the new value and output `50`. The previous dark proof remains at `artifacts/gallery-recordings/20260606-035858-519/Slider/dark-slider.mp4`. |
| Basic input | ColorPicker | `item/ColorPicker` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-071255-441/ColorPicker/light-colorpicker.mp4` | Latest Light rendered rerun toggles `IsMoreButtonVisible` from Off to On with local visual delta `6.087` and whole-frame delta `0.247`; reviewed frame `t9500` shows the More color picker content visible. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/ColorPicker/dark-colorpicker.mp4`. |
| Basic input | HyperlinkButton | `item/HyperlinkButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-074841-162/HyperlinkButton/light-hyperlinkbutton.mp4` | Latest Light rendered run clicks the safe in-app `Go to ToggleButton` sample and requires route proof: `BeforeRoute=item/HyperlinkButton`, `AfterRoute=item/ToggleButton`, `TargetSampleVisible=true`, whole-frame delta `2.563`, and local visual delta `12.244`. Reviewed `t9500` shows the ToggleButton destination page. External URI navigation remains intentionally not invoked. The previous dark proof remains at `artifacts/gallery-recordings/20260606-020424-123/HyperlinkButton/dark-hyperlinkbutton.mp4`. |
| Basic input | RatingControl | `item/RatingControl` | Recorded | Touch-first sample copy removed | `artifacts/gallery-recordings/20260606-070029-557/RatingControl/light-ratingcontrol.mp4` | Latest Light rendered rerun changes the rating from `0` to target `3` with local visual delta `3.861`; reviewed frame `t9500` shows three selected stars, output `3`, and the corrected `Click again to clear your rating.` text with no `Swipe left` instruction. The previous dark proof remains at `artifacts/gallery-recordings/20260606-051218-679/RatingControl/dark-ratingcontrol.mp4`. |
| Basic input | RepeatButton | `item/RepeatButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/RepeatButton/light-repeatbutton.mp4` | Latest Light rendered rerun changes output from `Control output` to `Number of clicks: 1` with local visual delta `0.669`; reviewed frame `t9500` shows the click count. The previous dark proof remains at `artifacts/gallery-recordings/20260606-040225-951/RepeatButton/dark-repeatbutton.mp4`. |
| Basic input | ToggleButton | `item/ToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/ToggleButton/light-togglebutton.mp4` | Latest Light rendered rerun toggles Off to On with local visual delta `48.038`; reviewed frame `t9500` shows the checked ToggleButton and output text `On`. The previous dark proof remains at `artifacts/gallery-recordings/20260606-040225-951/ToggleButton/dark-togglebutton.mp4`. |
| Basic input | DropDownButton | `item/DropDownButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-064251-054/DropDownButton/light-dropdownbutton.mp4` | Latest Light 24s rendered run closes through the `Send` leaf item and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t3500` / `t6000` / `t16500`, deltas `9.819` / `0.456` / `9.908`, and local delta `9.934`; reviewed frame `t3500` shows the menu aligned under the trigger. The previous dark proof remains at `artifacts/gallery-recordings/20260606-030431-469/DropDownButton/dark-dropdownbutton.mp4`. |
| Basic input | SplitButton | `item/SplitButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-064538-234/SplitButton/light-splitbutton.mp4` | Latest Light 24s rendered run closes through the `Red` leaf item and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t5000` / `t12000` / `t19000`, deltas `47.282` / `0.036` / `47.417`, and local delta `47.417`; reviewed frame `t5000` shows the color menu aligned under the trigger. The previous dark proof remains at `artifacts/gallery-recordings/20260606-030658-445/SplitButton/dark-splitbutton.mp4`. |
| Basic input | ToggleSplitButton | `item/ToggleSplitButton` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-064806-277/ToggleSplitButton/light-togglesplitbutton.mp4` | Latest Light 24s rendered run closes through the `Bulleted list` leaf item and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t5000` / `t12000` / `t19500`, deltas `5.265` / `0.025` / `5.385`, opened-element local delta `5.385`, and trigger-region local delta `46.41`; reviewed frame `t5000` shows the compact menu visible and aligned. The previous dark proof remains at `artifacts/gallery-recordings/20260606-030911-067/ToggleSplitButton/dark-togglesplitbutton.mp4`. |
| Basic input | ToggleSwitch | `item/ToggleSwitch` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/ToggleSwitch/light-toggleswitch.mp4` | Latest Light rendered rerun toggles Off to On with local visual delta `7.322`; reviewed frame `t9500` shows the switches on with the `Working` content visible. The previous dark proof remains at `artifacts/gallery-recordings/20260606-040225-951/ToggleSwitch/dark-toggleswitch.mp4`. |
| Text | NumberBox | `item/NumberBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-070029-557/NumberBox/light-numberbox.mp4` | Latest Light rendered rerun reaches value `20` from `10` with local visual delta `0.307`; reviewed frame `t9500` shows the updated value in the spin-button sample, so this remains rendered value proof rather than UIA-only proof. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/NumberBox/dark-numberbox.mp4`. |
| Text | AutoSuggestBox | `item/AutoSuggestBox` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-074841-162/AutoSuggestBox/light-autosuggestbox.mp4` | Latest Light rendered rerun uses an 18s capture and requires both UIA and final-frame visual close proof. Manifest records `SuggestionInvokeMethod=InvokePattern`, `SuggestionClosed=true`, `TextVisualClosedEvidence.Closed=true`, final frame `t17500`, final delta `0.941`, local visual delta `10.307`, and output `Aegean`; reviewed `t9500` shows the popup open during selection and reviewed `t17500` shows it gone with `Aegean` rendered in the text box and output. The rejected `20260606-041123-086` row is kept only as false-pass evidence, and the previous dark proof remains at `artifacts/gallery-recordings/20260606-045803-926/AutoSuggestBox/dark-autosuggestbox.mp4`. |
| Text | TextBox | `item/TextBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072128-088/TextBox/light-textbox.mp4` | Latest Light rendered run records text entry with `TextEvidence=true`, `AfterOutput=ModernWpf text`, and local visual delta `1.918`; reviewed late frame `t9500` shows the text visibly rendered. The previous dark proof remains at `artifacts/gallery-recordings/20260606-034438-510/TextBox/dark-textbox.mp4`. |
| Text | PasswordBox | `item/PasswordBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072128-088/PasswordBox/light-passwordbox.mp4` | Latest Light rendered run records text entry with `TextEvidence=true`, masked `AfterOutput`, and local visual delta `2.362`; reviewed late frame `t9500` shows masked password bullets rendered. The previous dark proof remains at `artifacts/gallery-recordings/20260606-034438-510/PasswordBox/dark-passwordbox.mp4`. |
| Text | RichTextEdit | `item/RichTextEdit` | Recorded failure | Visual size fixed; text input open | `artifacts/gallery-recordings/20260606-091557-396/RichTextEdit/light-richtextedit.mp4` | The Gallery sample now renders the live RichTextBox as a 160px-tall editor instead of the previous 32px one-line field; reviewed Light frames show a 790x160 editor area and focused caret. Text input remains open: current Light and Dark focused reruns fail with `AfterOutput=""`, so the older `artifacts/gallery-recordings/20260606-014544-783/RichTextEdit/dark-richtextedit.mp4` proof is stale under the current desktop session. Failed evidence includes `20260606-075521-729` Light, `20260606-075734-362` Dark, `20260606-080845-863` Light screen mode, and post-height-fix Light runs `20260606-083715-785` / `20260606-091557-396`. |
| Layout | SplitView | `item/SplitView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-071255-441/SplitView/light-splitview.mp4` | Latest Light rendered rerun toggles `IsPaneOpen` from On to Off with local visual delta `45.737` and whole-frame delta `0.495`; reviewed frame `t9500` shows the pane closed with the option controls aligned. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/SplitView/dark-splitview.mp4`. |
| Layout | Expander | `item/Expander` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072826-258/Expander/light-expander.mp4` | Latest Light rendered run records expansion evidence with whole-frame/local deltas `0.305` / `5.512`; reviewed frame `t9500` shows the expected content visible after expansion. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033115-631/Expander/dark-expander.mp4`. |
| Media | PersonPicture | `item/PersonPicture` | Recorded | Recorder coverage hardened | `artifacts/gallery-recordings/20260606-073924-914/PersonPicture/light-personpicture.mp4` | Latest Light rendered rerun selects the `Display Name` radio instead of accepting a static route. Manifest records `SelectionEvidence=true`, `TargetName=Display Name`, local visual delta `20.344`, and reviewed `t9500` shows the avatar changed to the `JD` display-name state. The previous dark proof remains at `artifacts/gallery-recordings/20260606-052421-356/PersonPicture/dark-personpicture.mp4`. |
| Styles | IconElement | `item/IconElement` | Recorded | Recorder coverage hardened | `artifacts/gallery-recordings/20260606-073924-914/IconElement/light-iconelement.mp4` | Latest Light rendered rerun toggles the `Monochrome` checkbox instead of accepting a static route. Manifest records `BeforeState=Off`, `AfterState=On`, `OptionEvidence=true`, local visual delta `4.175`, and reviewed `t9500` shows the monochrome bitmap icon and checked option. The previous dark proof remains at `artifacts/gallery-recordings/20260606-052421-356/IconElement/dark-iconelement.mp4`. |
| Styles | ThemeShadow | `item/ThemeShadow` | Recorded | Recorder coverage hardened | `artifacts/gallery-recordings/20260606-073924-914/ThemeShadow/light-themeshadow.mp4` | Latest Light rendered rerun moves the translation slider instead of accepting a static route. Manifest records `BeforeValue=32`, `AfterValue=42`, `TargetReached=true`, local visual delta `2.045`, and reviewed `t9500` shows the slider and shadow sample in the changed position. The previous dark proof remains at `artifacts/gallery-recordings/20260606-052421-356/ThemeShadow/dark-themeshadow.mp4`. |
| Windowing | TitleBar | `item/TitleBar` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-073924-914/TitleBar/light-titlebar.mp4` | Latest Light rendered run toggles `IsBackButtonVisible` and requires the preview Back button to become visible. Manifest records `BeforeState=Off`, `AfterState=On`, `BeforeExpectedElementVisible=false`, `AfterExpectedElementVisible=true`, `ExpectedElementChanged=true`, whole-frame delta `0.127`, and local visual delta `8.509`; reviewed frame `t9500` shows the Back preview button. The previous dark proof remains at `artifacts/gallery-recordings/20260606-021439-880/TitleBar/dark-titlebar.mp4`. |
| Status & info | InfoBadge | `item/InfoBadge` | Recorded | Recorder coverage hardened | `artifacts/gallery-recordings/20260606-073924-914/InfoBadge/light-infobadge.mp4` | Latest Light rendered rerun toggles `ToggleInfoBadgeOpacity` instead of accepting a static route. Manifest records `BeforeState=On`, `AfterState=Off`, `OptionEvidence=true`, local visual delta `4.947`, and reviewed `t9500` shows the option off with the embedded NavigationView still rendered. The previous dark proof remains at `artifacts/gallery-recordings/20260606-052421-356/InfoBadge/dark-infobadge.mp4`. |
| Status & info | InfoBar | `item/InfoBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-071255-441/InfoBar/light-infobar.mp4` | Latest Light rendered rerun toggles `Is Open` from On to Off with local visual delta `4.173` and whole-frame delta `0.192`; reviewed frame `t9500` shows the first InfoBar sample closed while later samples remain aligned. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/InfoBar/dark-infobar.mp4`. |
| Status & info | ProgressRing | `item/ProgressRing` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260606-071255-441/ProgressRing/light-progressring.mp4` | Latest Light manifest records `AnimationEvidence=true` with early-frame delta `0.083`, local visual delta `13.824`, and option state changing from On to Off despite low whole-frame delta `0.085`; reviewed frame `t9500` shows the toggled ProgressRing sample and aligned controls. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/ProgressRing/dark-progressring.mp4`. |
| Status & info | ToolTip | `item/ToolTip` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-065048-868/ToolTip/light-tooltip.mp4` | ToolTip uses `OpenRepeat` proof instead of diagnostic `PreparedOpen`. Latest Light 18s rendered run passes with `OpenRepeatEvidence=true`, close method `Escape2`, frames `t3500` / `t5500` / `t10000`, deltas `5.786` / `0.181` / `5.749`, and local delta `43.935`; reviewed frame `t3500` shows the ToolTip beside the trigger. The previous dark proof remains at `artifacts/gallery-recordings/20260606-032006-230/ToolTip/dark-tooltip.mp4`. |
| Scrolling | AnnotatedScrollBar | `item/AnnotatedScrollBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-071255-441/AnnotatedScrollBar/light-annotatedscrollbar.mp4` | Latest Light rendered rerun records scroll evidence with whole-frame delta `4.548` and local visual delta `95.557`; reviewed frame `t9500` shows the colored list scrolled with annotated markers visible. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/AnnotatedScrollBar/dark-annotatedscrollbar.mp4`. |
| Collections | GridView | `item/GridView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-071255-441/GridView/light-gridview.mp4` | Latest Light rendered rerun selects `Item 1` and changes output to `You clicked Item 1.` with local visual delta `0.784` and whole-frame delta `0.085`; reviewed frame `t9500` shows populated GridView image tiles and the output text, so this remains visible rendered output proof rather than UIA-only proof. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/GridView/dark-gridview.mp4`. |
| Collections | ItemsRepeater | `item/ItemsRepeater` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260606-071255-441/ItemsRepeater/light-itemsrepeater.mp4` | Latest Light rendered rerun records scroll evidence with whole-frame delta `6.815` and local visual delta `22.712`; reviewed frame `t9500` shows virtualized items in the 260s rendered after scroll. The previous dark proof remains at `artifacts/gallery-recordings/20260606-041123-086/ItemsRepeater/dark-itemsrepeater.mp4`. |
| Collections | ListBox | `item/ListBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072128-088/ListBox/light-listbox.mp4` | Latest Light rendered run records UIA selection evidence for target `Green` with whole-frame/local deltas `1.929` / `40.303`; reviewed frame `t9500` shows selected ListBox rows. The previous dark proof remains at `artifacts/gallery-recordings/20260606-034438-510/ListBox/dark-listbox.mp4`. |
| Collections | ListView | `item/ListView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072128-088/ListView/light-listview.mp4` | Latest Light rendered run records UIA selection evidence with whole-frame/local deltas `0.247` / `3.702`; reviewed frame `t9500` shows the selected ListView item. The previous dark proof remains at `artifacts/gallery-recordings/20260606-034438-510/ListView/dark-listview.mp4`. |
| Collections | DataGrid | `item/DataGrid` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072128-088/DataGrid/light-datagrid.mp4` | UIA selection still does not change, but Light visual selection evidence passes with whole-frame/local deltas `1.344` / `48.134`; reviewed frame `t9500` shows the first row/cell highlighted. The previous dark proof remains at `artifacts/gallery-recordings/20260606-034438-510/DataGrid/dark-datagrid.mp4`. |
| Collections | TreeView | `item/TreeView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072826-258/TreeView/light-treeview.mp4` | Latest Light rendered run records expansion evidence for `Personal Documents` with whole-frame/local deltas `0.508` / `7.864`; reviewed frame `t9500` shows `Contractor contact info` visible after expansion. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033115-631/TreeView/dark-treeview.mp4`. |
| Navigation | BreadcrumbBar | `item/BreadcrumbBar` | Recorded | Recorder/sample anchor fixed | `artifacts/gallery-recordings/20260606-072826-258/BreadcrumbBar/light-breadcrumbbar.mp4` | Latest Light rendered run records local visual delta `2.886`; breadcrumb item collection changed despite low whole-frame delta `0.018`, and reviewed frame `t9500` shows the path advanced through `Folder1` to `Folder3`. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033115-631/BreadcrumbBar/dark-breadcrumbbar.mp4`. |
| Navigation | SelectorBar | `item/SelectorBar` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-072826-258/SelectorBar/light-selectorbar.mp4` | Latest Light rendered run records `Shared` changing to selected with `SelectionEvidence=true`, `VisualSelectionEvidence=true`, and local visual delta `0.715`; reviewed frame `t9500` shows the selected indicator under `Shared`. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033115-631/SelectorBar/dark-selectorbar.mp4`. |
| Navigation | TabControl | `item/TabControl` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072826-258/TabControl/light-tabcontrol.mp4` | Latest Light rendered run records target `Hello Tab`, selection evidence, and local delta `2.429`; reviewed frame `t9500` shows the selected `Hello` tab and content `World`. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033115-631/TabControl/dark-tabcontrol.mp4`. |
| Navigation | NavigationView | `item/NavigationView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072826-258/NavigationView/light-navigationview.mp4` | Latest Light rendered run records target `Menu Item2`, selection evidence, and local visual delta `5.177`; reviewed frame `t9500` shows `Sample Page 2` selected and rendered in the sample NavigationView. The previous dark proof remains at `artifacts/gallery-recordings/20260606-033115-631/NavigationView/dark-navigationview.mp4`. |
| Date & calendar | Calendar | `item/Calendar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-072128-088/Calendar/light-calendar.mp4` | Latest Light rendered run records UIA selection evidence with whole-frame/local deltas `0.466` / `4.45`; reviewed frame `t9500` shows the selected day highlighted in the calendar. The previous dark proof remains at `artifacts/gallery-recordings/20260606-034438-510/Calendar/dark-calendar.mp4`. |
| Date & calendar | DatePicker | `item/DatePicker` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-064009-144/DatePicker/light-datepicker.mp4` | Latest Light 24s rendered run closes through `ExpandCollapsePattern.Collapse()` and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t3500` / `t7500` / `t13500`, deltas `11.419` / `0.865` / `11.425`, local delta `11.428`, and `ClosedThreshold=1.2`; reviewed frame `t3500` shows the calendar flyout positioned under the DatePicker. The previous dark proof remains at `artifacts/gallery-recordings/20260606-031745-688/DatePicker/dark-datepicker.mp4`. |
| Dialogs & flyouts | ContentDialog | `item/ContentDialog` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-062729-895/ContentDialog/light-contentdialog.mp4` | Latest Light 24s rendered run treats modal close as a named `Cancel` button action and requires pixel-backed close proof. Manifest records `OpenRepeatEvidence=true`, `CloseMethod=DialogCancelButton:Invoke`, frames `t3500` / `t13000` / `t19500`, deltas `18.764` / `0.423` / `18.839`, and local delta `72.849`; reviewed frame `t3500` shows the dialog centered over the dimmed page. The previous dark proof remains at `artifacts/gallery-recordings/20260605-033404-923/ContentDialog/dark-contentdialog.mp4`. |
| Dialogs & flyouts | Flyout | `item/Flyout` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260606-062215-560/Flyout/light-flyout.mp4` | Latest Light 24s rendered run passes with pixel-backed close proof and baseline-delta transition scan. Manifest records `OpenRepeatEvidence=true`, `CloseMethod=SampleConfirmButton:Invoke`, frames `t3500` / `t10500` / `t17000`, deltas `21.168` / `0.511` / `21.8`, and local delta `21.801`; reviewed frame `t4000` shows the flyout anchored above the trigger. The previous dark proof remains at `artifacts/gallery-recordings/20260605-030028-982/Flyout/dark-flyout.mp4`. |
| Dialogs & flyouts | Popup | `item/Popup` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-062450-995/Popup/light-popup.mp4` | Latest Light 24s rendered run accepts the named `Close` button only after the opened-content region returns to baseline, so stale UIA cannot block or fake the close. Manifest records `OpenRepeatEvidence=true`, `CloseMethod=SampleCloseButton:Invoke`, frames `t3500` / `t10500` / `t16500`, deltas `22.699` / `0.93` / `22.733`, and local delta `22.733`; reviewed frame `t3500` shows the popup in the offset-positioning sample area. The previous dark proof remains at `artifacts/gallery-recordings/20260605-033404-923/Popup/dark-popup.mp4`. |
| System | MessageBox | `item/MessageBox` | Recorded | Fixed | `artifacts/gallery-recordings/20260606-082426-601/MessageBox/light-messagebox.mp4` | Runtime sample dialogs now use owned WPF `MessageBox.Show(owner, ...)` plus owner-centered native placement. Latest Light 18s rendered run passes with `OpenRepeatEvidence=true`, `FirstOpenElementAnchored=true`, `SecondOpenElementAnchored=true`, `ClosedElementGone=true`, `CloseMethod=DialogOkButton:Invoke`, frames `t3500` / `t9500` / `t13500`, deltas `16.981` / `0.034` / `16.966`, and dialog text bounds `725,574,150,15` inside the capture; reviewed frames show the dialog visible on both opens and gone after close. The previous dark proof remains at `artifacts/gallery-recordings/20260605-064048-292/MessageBox/dark-messagebox.mp4`. |
| Menus & toolbars | Menu | `item/Menu` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-065247-089/Menu/light-menu.mp4` | Latest Light 24s rendered run closes through the `Exit` leaf item and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t3500` / `t8000` / `t14500`, deltas `10.441` / `0.5` / `10.469`, and local delta `10.474`; reviewed frame `t3500` shows the File menu opened in place. The previous dark proof remains at `artifacts/gallery-recordings/20260606-031526-603/Menu/dark-menu.mp4`. |
| Menus & toolbars | MenuBar | `item/MenuBar` | Recorded | Recorder hardened | `artifacts/gallery-recordings/20260606-061744-792/MenuBar/light-menubar.mp4` | Latest Light rendered run uses an 18s capture, closes through the `Exit` leaf item, and requires baseline-delta open/closed/open proof. Manifest records `OpenRepeatEvidence=true`, frames `t3500` / `t5500` / `t12000`, deltas `8.279` / `0.156` / `8.309`, and local delta `8.345`; reviewed frame `t3500` shows the File menu anchored under the trigger. The previous dark proof remains at `artifacts/gallery-recordings/20260606-023615-570/MenuBar/dark-menubar.mp4`. |
| Menus & toolbars | MenuFlyout | `item/MenuFlyout` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-061515-631/MenuFlyout/light-menuflyout.mp4` | Same-target repeat-open guard now treats tracked absolute-point presenters as the same target to avoid close/reopen flicker. Latest Light 24s rendered rerun passes with `CloseMethod=LeafMenuItem:Invoke`, `Detection=BaselineDeltaScan`, frames `t3500` / `t10500` / `t17000`, deltas `11.318` / `0.384` / `11.278`, and local delta `11.354`; reviewed frame `t4000` shows the menu anchored under the trigger. The previous dark proof remains at `artifacts/gallery-recordings/20260605-031711-696/MenuFlyout/dark-menuflyout.mp4`. |
| Menus & toolbars | AppBarButton | `item/AppBarButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-074841-162/AppBarButton/light-appbarbutton.mp4` | Latest Light rendered rerun changes output to `You clicked: Button1` with local visual delta `0.52`; reviewed frame `t9500` shows the output text and aligned symbol, bitmap, and font-icon examples. The previous dark proof remains at `artifacts/gallery-recordings/20260606-040225-951/AppBarButton/dark-appbarbutton.mp4`. |
| Menus & toolbars | AppBarSeparator | `item/AppBarSeparator` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-073924-914/AppBarSeparator/light-appbarseparator.mp4` | Latest Light rendered static route remains appropriate because the sample command bar has no state/output-changing action. Reviewed `t9500` shows visible separators and aligned AppBar commands. The previous dark proof remains at `artifacts/gallery-recordings/20260606-052421-356/AppBarSeparator/dark-appbarseparator.mp4`. |
| Menus & toolbars | AppBarToggleButton | `item/AppBarToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260606-074841-162/AppBarToggleButton/light-appbartogglebutton.mp4` | Latest Light rendered rerun toggles Off to On with local visual delta `44.826`; reviewed frame `t9500` shows the first symbol AppBarToggleButton checked and output `IsChecked = True`. The previous dark proof remains at `artifacts/gallery-recordings/20260606-040225-951/AppBarToggleButton/dark-appbartogglebutton.mp4`. |
| Menus & toolbars | CommandBar | `item/CommandBar` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-061229-078/CommandBar/light-commandbar.mp4` | Product popup state is now synchronized with `IsOpen` and the recorder no longer depends on UIA exposing the second-open overflow item. Latest Light 24s rendered run passes with `OpenRepeatEvidence=true`, `CloseMethod=SampleCloseButton`, frames `t9500` / `t13500` / `t20000`, deltas `4.17` / `0.005` / `4.181`, and local delta `4.181`; reviewed frames show aligned first-open and second-open overflow plus a clean closed frame. The previous dark proof remains at `artifacts/gallery-recordings/20260604-183855-055/CommandBar/dark-commandbar.mp4`. |
| Menus & toolbars | CommandBarFlyout | `item/CommandBarFlyout` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260606-060315-799/CommandBarFlyout/light-commandbarflyout.mp4` | Product popup state is synchronized with `IsOpen`, WPF popup animation is disabled, Escape hides the owning flyout, and secondary transitions respect the owning flyout animation gate. Latest Light 24s rendered run passes with `OpenRepeatEvidence=true`, `CloseMethod=SecondaryCommand`, frames `t4000` / `t6500` / `t13500`, deltas `2.871` / `0.003` / `2.846`, and local delta `2.872`; reviewed first-open and second-open frames show the command bar and secondary menu aligned with no repeat-open crash frame. The previous dark proof remains at `artifacts/gallery-recordings/20260606-032144-304/CommandBarFlyout/dark-commandbarflyout.mp4`. |
