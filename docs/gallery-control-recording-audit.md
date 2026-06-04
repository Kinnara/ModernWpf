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

The `20260604-050301-561` run is intentionally not treated as a green sweep:
it exposed two remaining interaction gaps that the older static sweep missed.
`ToolTip` did not open under the current synthetic hover/click path, and
`RichTextEdit` focused but did not receive text input through the recorder.
The `20260604-053726-512` follow-up closes those two controls with
diagnostics-prepared visual evidence: `--open-interactions` opens the WPF
tooltip in-process and populates the WPF `RichTextBox` before recording. This
is accepted as visual proof of the rendered open/text states, not as proof that
external synthetic hover or keyboard injection works in this desktop session.

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
| Shell | Navigation pane | Home, Design Guidance, Samples expand/collapse | Recorded | Fixed | `artifacts/gallery-recordings/20260603-080438-789/ShellNavigation/dark-shellnavigation.mp4` | Manifest proves Design Guidance and Samples expanded with visible children, then collapsed with children hidden; reviewed contact sheet shows initial collapsed, expanded, and final collapsed states. Recorder's injected mouse event still required UIA ExpandCollapse fallback in this desktop session. |
| Dialogs & flyouts | TeachingTip | `item/TeachingTip` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260603-064740-962/TeachingTip/dark-teachingtip.mp4` | Manifest records first and second open evidence; reviewed contact sheet shows the TeachingTip visible after repeat open. |
| Basic input | Button | `item/Button` | Recorded | Fixed | `artifacts/gallery-recordings/20260603-050244-858/Button/dark-button.mp4` | Recorder toggles `Disable button`; manifest records the primary button changing from `Enabled` to `Disabled`, and reviewed frame `t3000.png` shows the disabled button. |
| Basic input | CheckBox | `item/CheckBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/CheckBox/dark-checkbox.mp4` | Manifest records `Off` to `On`; reviewed frame shows checked state. |
| Basic input | ComboBox | `item/ComboBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-030922-916/ComboBox/dark-combobox.mp4` | Rendered MP4 shows dropdown open and second-open path. |
| Basic input | RadioButton | `item/RadioButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-050244-858/RadioButton/dark-radiobutton.mp4` | Manifest records `Option 1` from selected to unselected and `Option 2` from unselected to selected; reviewed frame `t3000.png` shows `Option 2` selected. |
| Basic input | Slider | `item/Slider` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/Slider/dark-slider.mp4` | Manifest records `0` to `50`; reviewed frame shows output `50`. |
| Basic input | ColorPicker | `item/ColorPicker` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-050244-858/ColorPicker/dark-colorpicker.mp4` | Manifest records `IsMoreButtonVisible` from `Off` to `On`; reviewed frame `t4000.png` shows the `More` button visible. |
| Basic input | HyperlinkButton | `item/HyperlinkButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-050244-858/HyperlinkButton/dark-hyperlinkbutton.mp4` | Static route capture only; external URI navigation is intentionally not invoked in this pass. |
| Basic input | RatingControl | `item/RatingControl` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/RatingControl/dark-ratingcontrol.mp4` | Manifest records `0` to `3`; reviewed frame shows three selected stars and value `3`. |
| Basic input | RepeatButton | `item/RepeatButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-185724-938/RepeatButton/dark-repeatbutton.mp4` | Manifest records output from `Control output` to `Number of clicks: 1` with `OutputEvidence=true`, `OutputMatched=true`, and `OutputChanged=true`. |
| Basic input | ToggleButton | `item/ToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/ToggleButton/dark-togglebutton.mp4` | Manifest records `Off` to `On`; reviewed frame shows `On`. |
| Basic input | DropDownButton | `item/DropDownButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-031922-773/DropDownButton/dark-dropdownbutton.mp4` | Rendered MP4 frame shows `Send`, `Reply`, and `Reply All` flyout on repeat-open path. |
| Basic input | SplitButton | `item/SplitButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-034734-786/SplitButton/dark-splitbutton.mp4` | Rendered MP4 frame shows color flyout; manifest verifies both opens reached `Expanded`. |
| Basic input | ToggleSplitButton | `item/ToggleSplitButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-034734-786/ToggleSplitButton/dark-togglesplitbutton.mp4` | Rendered MP4 frame shows the compact two-button flyout; full-frame delta is small, so the manifest also requires expanded-state and open-element proof on both opens. |
| Basic input | ToggleSwitch | `item/ToggleSwitch` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/ToggleSwitch/dark-toggleswitch.mp4` | Manifest records `Off` to `On`; reviewed frame shows switch and `Working` state on. |
| Text | NumberBox | `item/NumberBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/NumberBox/dark-numberbox.mp4` | Manifest records `10` to `20`; reviewed frame shows spin-button value `20`. |
| Text | AutoSuggestBox | `item/AutoSuggestBox` | Recorded | Fixed | `artifacts/gallery-recordings/20260603-055524-741/AutoSuggestBox/dark-autosuggestbox.mp4` | Manifest records typed input `ae`, suggestion `Aegean`, and output `Aegean`. Recorder still falls back to UIA selection for output proof; `AutoSuggestBoxInteractionTests` covers item-click submit/close behavior. |
| Layout | SplitView | `item/SplitView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/SplitView/dark-splitview.mp4` | Manifest records `IsPaneOpen` from `On` to `Off`; reviewed contact sheet shows the pane collapsed after the option toggle. |
| Media | PersonPicture | `item/PersonPicture` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-082547-581/PersonPicture/dark-personpicture.mp4` | Static rendered route with `GallerySample_PersonPicture_PersonPicture` anchor; reviewed contact sheet shows the portrait options and API/catalog content. |
| Styles | IconElement | `item/IconElement` | Recorded | Recorder anchor fixed | `artifacts/gallery-recordings/20260603-082547-581/IconElement/dark-iconelement.mp4` | Static rendered route now requires the exposed `GallerySample_IconElement_ExampleButton1` anchor; reviewed contact sheet shows bitmap, font, and image icon samples. |
| Styles | ThemeShadow | `item/ThemeShadow` | Recorded | Sample anchor fixed | `artifacts/gallery-recordings/20260603-082547-581/ThemeShadow/dark-themeshadow.mp4` | Added and required `GallerySample_ThemeShadow_TranslationSlider`; reviewed contact sheet shows the shadow surface and slider. |
| Windowing | TitleBar | `item/TitleBar` | Recorded | Sample anchor fixed | `artifacts/gallery-recordings/20260603-082547-581/TitleBar/dark-titlebar.mp4` | Added and required `GallerySample_TitleBar_SearchBox`; reviewed contact sheet shows the preview title bar and options. |
| Status & info | InfoBadge | `item/InfoBadge` | Recorded | Sample anchor fixed | `artifacts/gallery-recordings/20260603-082547-581/InfoBadge/dark-infobadge.mp4` | Added and required `GallerySample_InfoBadge_NavigationView`; reviewed contact sheet shows the NavigationView badge and style samples. |
| Status & info | InfoBar | `item/InfoBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/InfoBar/dark-infobar.mp4` | Manifest records `Is Open` from `On` to `Off`; reviewed contact sheet shows the first InfoBar closed. |
| Status & info | ProgressRing | `item/ProgressRing` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260603-182335-320/ProgressRing/dark-progressring.mp4` | Manifest records `AnimationEvidence: true` with early-frame delta `0.069`, plus `Progress Options` from `On` to `Off`; reviewed frames show the indeterminate arc at different angles before deactivation. |
| Scrolling | AnnotatedScrollBar | `item/AnnotatedScrollBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/AnnotatedScrollBar/dark-annotatedscrollbar.mp4` | Manifest records the linked ScrollViewer vertical scroll percent from `0` to `55`; reviewed contact sheet shows the list scrolled to the magenta section. |
| Collections | GridView | `item/GridView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-055524-741/GridView/dark-gridview.mp4` | Manifest records `You clicked Item 1.` after item activation; reviewed frame `t7500.png` shows the output text. |
| Collections | ItemsRepeater | `item/ItemsRepeater` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260603-181238-897/ItemsRepeater/dark-itemsrepeater.mp4` | Manifest records the virtualizing `ScrollViewer` vertical scroll percent from `0` to `55`; reviewed contact sheet shows the visible item range move from `0`-`9` to `265`-`279`. |
| Navigation | BreadcrumbBar | `item/BreadcrumbBar` | Recorded | Recorder/sample anchor fixed | `artifacts/gallery-recordings/20260603-091005-125/BreadcrumbBar/dark-breadcrumbbar.mp4` | Manifest proves clicking `Folder1` in the templated breadcrumb removed `Folder2` and `Folder3`; reviewed contact sheet shows the before/after item collection. |
| Navigation | SelectorBar | `item/SelectorBar` | Recorded | Fixed + recorder hardened | `artifacts/gallery-recordings/20260604-064455-670/SelectorBar/dark-selectorbar.mp4` | Manifest records `Shared` changing from `Unselected` to `Selected`, sample status changing to `Shared`, and `VisualSelectionEvidence=true` with frame delta `0.003`; reviewed `t2000.png`/`t3000.png` shows the selected pill moving from no basic item to `Shared`. |
| Navigation | NavigationView | `item/NavigationView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-091005-125/NavigationView/dark-navigationview.mp4` | Manifest proves `Menu Item2` changed from unselected to selected and `Sample Page 2` appeared; reviewed contact sheet shows the selected item/header change. |
| Dialogs & flyouts | ContentDialog | `item/ContentDialog` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/ContentDialog/dark-contentdialog.mp4` | Manifest records first and second dialog opens; reviewed contact sheet shows the dialog visible on sampled frames. |
| Dialogs & flyouts | Flyout | `item/Flyout` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/Flyout/dark-flyout.mp4` | Manifest records first and second flyout opens; reviewed contact sheet shows the confirmation flyout on sampled frames. |
| Dialogs & flyouts | Popup | `item/Popup` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/Popup/dark-popup.mp4` | Manifest records first and second popup opens despite low full-frame delta; reviewed contact sheet shows `Simple Popup`. |
| Menus & toolbars | MenuBar | `item/MenuBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/MenuBar/dark-menubar.mp4` | Manifest records first and second menu opens despite low full-frame delta; reviewed contact sheet shows the `File` menu. |
| Menus & toolbars | MenuFlyout | `item/MenuFlyout` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/MenuFlyout/dark-menuflyout.mp4` | Manifest records first and second flyout opens with `Expanded` state; reviewed contact sheet shows the sort menu. |
| Menus & toolbars | AppBarButton | `item/AppBarButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-070433-922/AppBarButton/dark-appbarbutton.mp4` | Manifest records output changing to `You clicked: Button1`; reviewed contact sheet shows the click output. |
| Menus & toolbars | AppBarSeparator | `item/AppBarSeparator` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-070433-922/AppBarSeparator/dark-appbarseparator.mp4` | Static rendered route with stable button anchors; reviewed contact sheet shows separated command buttons. |
| Menus & toolbars | AppBarToggleButton | `item/AppBarToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-070433-922/AppBarToggleButton/dark-appbartogglebutton.mp4` | Manifest records toggle state from `Off` to `On`; reviewed contact sheet shows the selected toggle output. |
| Menus & toolbars | CommandBar | `item/CommandBar` | Recorded | Fixed | `artifacts/gallery-recordings/20260604-183855-055/CommandBar/dark-commandbar.mp4` | Product popup state is now synchronized with `IsOpen` and the recorder no longer depends on UIA exposing the second-open overflow item. Manifest records `ClosedElementGone=true`, `CloseMethod=SampleCloseButton`, `OpenRepeatEvidence=true`, and visual open/closed/open frames `t0500` / `t5000` / `t7500` / `t11000` with deltas `9.921`, `0.001`, and `9.839`. |
| Menus & toolbars | CommandBarFlyout | `item/CommandBarFlyout` | Recorded | Fixed | `artifacts/gallery-recordings/20260604-194134-079/CommandBarFlyout/dark-commandbarflyout.mp4` | Product popup state is now synchronized with `IsOpen`, WPF popup animation is disabled, Escape hides the owning flyout, and secondary transitions respect the owning flyout animation gate. The latest 24s run requires both first and second secondary-menu expansions; reviewed frames `t3000`, `t4000`, and `t8000` show `Resize` / `Move`, closed state after `Resize`, and second-open `Resize` / `Move` with no repeat-open crash. |
