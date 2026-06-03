# Gallery Control Recording Audit

This document tracks the recording-first Gallery audit. A control is not counted
as verified here unless there is a live recording for the relevant interaction
path and the recording has been reviewed or decoded into nonblank poster frames.

## Acceptance Bar

- Launch the Gallery route for each control in visual-test mode.
- Record the live window while driving the primary interaction for interactive
  controls.
- For popup and flyout controls, record open, close, and second open in the same
  clip so flicker, stale visual state, and repeat-open crashes are visible.
- Extract poster frames from each recording and reject blank recordings.
- Fix issues in substantial rounds and record the post-fix interaction before
  committing.

## Scope

Initial scope is the 46-control ModernWpf visual-check inventory from
`tools/visual-checks/Run-GalleryVisualChecks.ps1`, plus the Gallery shell
NavigationView pane because earlier user-reported failures were in the shell.
The broader official WPF Gallery page catalog is a separate expansion and is not
silently included in this matrix yet.

## Recorder

Use the per-control recorder:

```powershell
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls CommandBarFlyout -Theme Dark -DurationSeconds 8 -FrameRate 10 -Build
```

For broad sweeps, run in batches and review `report.md` plus the MP4 clips under
`artifacts/gallery-recordings/<stamp>/`.

The default recorder is rendered `PrintWindow` composition for the Gallery
process plus popup HWNDs. `-CaptureMode Screen` is available for diagnostics but
is not accepted as proof in the current Codex desktop session because it can
record the Windows background instead of the Gallery window.

## Control Matrix

| Area | Control | Route or Scenario | Recording Status | Fix Status | Latest Evidence | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| Shell | Navigation pane | Home, Design Guidance, Samples expand/collapse | Pending | Fixed earlier, needs new recording pass |  | Must cover repeated expand/collapse and child visibility. |
| Dialogs & flyouts | TeachingTip | `item/TeachingTip` | Pending | Pending |  | Open, close, second open required. |
| Basic input | Button | `item/Button` | Pending | Pending |  | Static route plus primary button click review. |
| Basic input | CheckBox | `item/CheckBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/CheckBox/dark-checkbox.mp4` | Manifest records `Off` to `On`; reviewed frame shows checked state. |
| Basic input | ComboBox | `item/ComboBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-030922-916/ComboBox/dark-combobox.mp4` | Rendered MP4 shows dropdown open and second-open path. |
| Basic input | RadioButton | `item/RadioButton` | Pending | Pending |  | Selection state required. |
| Basic input | Slider | `item/Slider` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/Slider/dark-slider.mp4` | Manifest records `0` to `50`; reviewed frame shows output `50`. |
| Basic input | ColorPicker | `item/ColorPicker` | Pending | Pending |  | Static first pass, then More/color interaction expansion. |
| Basic input | HyperlinkButton | `item/HyperlinkButton` | Pending | Pending |  | Static first pass; avoid external navigation during automation. |
| Basic input | RatingControl | `item/RatingControl` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/RatingControl/dark-ratingcontrol.mp4` | Manifest records `0` to `3`; reviewed frame shows three selected stars and value `3`. |
| Basic input | RepeatButton | `item/RepeatButton` | NeedsReview | Pending | `artifacts/gallery-recordings/20260603-040311-639/RepeatButton/dark-repeatbutton.mp4` | Hold was invoked, but decoded frames show no strong visual/output proof; keep unverified until the recorder captures pressed-state or output evidence. |
| Basic input | ToggleButton | `item/ToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/ToggleButton/dark-togglebutton.mp4` | Manifest records `Off` to `On`; reviewed frame shows `On`. |
| Basic input | DropDownButton | `item/DropDownButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-031922-773/DropDownButton/dark-dropdownbutton.mp4` | Rendered MP4 frame shows `Send`, `Reply`, and `Reply All` flyout on repeat-open path. |
| Basic input | SplitButton | `item/SplitButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-034734-786/SplitButton/dark-splitbutton.mp4` | Rendered MP4 frame shows color flyout; manifest verifies both opens reached `Expanded`. |
| Basic input | ToggleSplitButton | `item/ToggleSplitButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-034734-786/ToggleSplitButton/dark-togglesplitbutton.mp4` | Rendered MP4 frame shows the compact two-button flyout; full-frame delta is small, so the manifest also requires expanded-state and open-element proof on both opens. |
| Basic input | ToggleSwitch | `item/ToggleSwitch` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/ToggleSwitch/dark-toggleswitch.mp4` | Manifest records `Off` to `On`; reviewed frame shows switch and `Working` state on. |
| Text | NumberBox | `item/NumberBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/NumberBox/dark-numberbox.mp4` | Manifest records `10` to `20`; reviewed frame shows spin-button value `20`. |
| Text | AutoSuggestBox | `item/AutoSuggestBox` | Pending | Pending |  | Text entry and suggestion popup required. |
| Layout | SplitView | `item/SplitView` | Pending | Pending |  | Pane toggle required after first static pass. |
| Media | PersonPicture | `item/PersonPicture` | Pending | Pending |  | Static route. |
| Motion | ParallaxView | `item/ParallaxView` | Pending | Pending |  | Scroll interaction required after first static pass. |
| Styles | IconElement | `item/IconElement` | Pending | Pending |  | Static route. |
| Styles | ThemeShadow | `item/ThemeShadow` | Pending | Pending |  | Static route plus shadow visibility review. |
| Windowing | TitleBar | `item/TitleBar` | Pending | Pending |  | Static route. |
| Status & info | InfoBadge | `item/InfoBadge` | Pending | Pending |  | Static route. |
| Status & info | InfoBar | `item/InfoBar` | Pending | Pending |  | Close/action interaction required after first static pass. |
| Status & info | ProgressRing | `item/ProgressRing` | Pending | Pending |  | Animated recording required. |
| Scrolling | PipsPager | `item/PipsPager` | Pending | Pending |  | Page selection required. |
| Scrolling | AnnotatedScrollBar | `item/AnnotatedScrollBar` | Pending | Pending |  | Scroll interaction required after first static pass. |
| Collections | PullToRefresh | `item/PullToRefresh` | Pending | Pending |  | Pull/refresh gesture required after first static pass. |
| Collections | GridView | `item/GridView` | Pending | Pending |  | Item selection required. |
| Collections | ItemsRepeater | `item/ItemsRepeater` | Pending | Pending |  | Scroll/virtualization interaction required after first static pass. |
| Navigation | BreadcrumbBar | `item/BreadcrumbBar` | Pending | Pending |  | Ellipsis/flyout interaction required after first static pass. |
| Navigation | Pivot | `item/Pivot` | Pending | Pending |  | Tab selection required. |
| Navigation | SelectorBar | `item/SelectorBar` | Pending | Pending |  | Selection required after first static pass. |
| Navigation | NavigationView | `item/NavigationView` | Pending | Pending |  | Sample nav interaction required; shell pane tracked separately. |
| Dialogs & flyouts | ContentDialog | `item/ContentDialog` | Pending | Pending |  | Open, close, second open required. |
| Dialogs & flyouts | Flyout | `item/Flyout` | Pending | Pending |  | Open, close, second open required. |
| Dialogs & flyouts | Popup | `item/Popup` | Pending | Pending |  | Open, close, second open required. |
| Menus & toolbars | MenuBar | `item/MenuBar` | Pending | Pending |  | Open, close, second open required. |
| Menus & toolbars | MenuFlyout | `item/MenuFlyout` | Pending | Pending |  | Open, close, second open required. |
| Menus & toolbars | SwipeControl | `item/SwipeControl` | Pending | Pending |  | Swipe gesture required after first static pass. |
| Menus & toolbars | AppBarButton | `item/AppBarButton` | Pending | Pending |  | Static route plus primary click review. |
| Menus & toolbars | AppBarSeparator | `item/AppBarSeparator` | Pending | Pending |  | Static route. |
| Menus & toolbars | AppBarToggleButton | `item/AppBarToggleButton` | Pending | Pending |  | Toggle state required. |
| Menus & toolbars | CommandBar | `item/CommandBar` | Pending | Pending |  | Overflow interaction required after first static pass. |
| Menus & toolbars | CommandBarFlyout | `item/CommandBarFlyout` | Recorded | Fixed | `artifacts/gallery-recordings/20260603-025616-794/CommandBarFlyout/dark-commandbarflyout.mp4` | Rendered MP4 shows open, MoreButton expansion, and `Resize` / `Move` secondary commands. |
