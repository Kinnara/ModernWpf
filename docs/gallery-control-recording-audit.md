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
| Dialogs & flyouts | TeachingTip | `item/TeachingTip` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260603-064740-962/TeachingTip/dark-teachingtip.mp4` | Manifest records first and second open evidence; reviewed contact sheet shows the TeachingTip visible after repeat open. |
| Basic input | Button | `item/Button` | Recorded | Fixed | `artifacts/gallery-recordings/20260603-050244-858/Button/dark-button.mp4` | Recorder toggles `Disable button`; manifest records the primary button changing from `Enabled` to `Disabled`, and reviewed frame `t3000.png` shows the disabled button. |
| Basic input | CheckBox | `item/CheckBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/CheckBox/dark-checkbox.mp4` | Manifest records `Off` to `On`; reviewed frame shows checked state. |
| Basic input | ComboBox | `item/ComboBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-030922-916/ComboBox/dark-combobox.mp4` | Rendered MP4 shows dropdown open and second-open path. |
| Basic input | RadioButton | `item/RadioButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-050244-858/RadioButton/dark-radiobutton.mp4` | Manifest records `Option 1` from selected to unselected and `Option 2` from unselected to selected; reviewed frame `t3000.png` shows `Option 2` selected. |
| Basic input | Slider | `item/Slider` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/Slider/dark-slider.mp4` | Manifest records `0` to `50`; reviewed frame shows output `50`. |
| Basic input | ColorPicker | `item/ColorPicker` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-050244-858/ColorPicker/dark-colorpicker.mp4` | Manifest records `IsMoreButtonVisible` from `Off` to `On`; reviewed frame `t4000.png` shows the `More` button visible. |
| Basic input | HyperlinkButton | `item/HyperlinkButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-050244-858/HyperlinkButton/dark-hyperlinkbutton.mp4` | Static route capture only; external URI navigation is intentionally not invoked in this pass. |
| Basic input | RatingControl | `item/RatingControl` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/RatingControl/dark-ratingcontrol.mp4` | Manifest records `0` to `3`; reviewed frame shows three selected stars and value `3`. |
| Basic input | RepeatButton | `item/RepeatButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-041754-728/RepeatButton/dark-repeatbutton.mp4` | Recorder report stays `NeedsReview` because UIA exposes only `Control output`; reviewed frame `t4000.png` shows `Number of clicks: 1`. |
| Basic input | ToggleButton | `item/ToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/ToggleButton/dark-togglebutton.mp4` | Manifest records `Off` to `On`; reviewed frame shows `On`. |
| Basic input | DropDownButton | `item/DropDownButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-031922-773/DropDownButton/dark-dropdownbutton.mp4` | Rendered MP4 frame shows `Send`, `Reply`, and `Reply All` flyout on repeat-open path. |
| Basic input | SplitButton | `item/SplitButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-034734-786/SplitButton/dark-splitbutton.mp4` | Rendered MP4 frame shows color flyout; manifest verifies both opens reached `Expanded`. |
| Basic input | ToggleSplitButton | `item/ToggleSplitButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-034734-786/ToggleSplitButton/dark-togglesplitbutton.mp4` | Rendered MP4 frame shows the compact two-button flyout; full-frame delta is small, so the manifest also requires expanded-state and open-element proof on both opens. |
| Basic input | ToggleSwitch | `item/ToggleSwitch` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/ToggleSwitch/dark-toggleswitch.mp4` | Manifest records `Off` to `On`; reviewed frame shows switch and `Working` state on. |
| Text | NumberBox | `item/NumberBox` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-040311-639/NumberBox/dark-numberbox.mp4` | Manifest records `10` to `20`; reviewed frame shows spin-button value `20`. |
| Text | AutoSuggestBox | `item/AutoSuggestBox` | Recorded | Fixed | `artifacts/gallery-recordings/20260603-055524-741/AutoSuggestBox/dark-autosuggestbox.mp4` | Manifest records typed input `ae`, suggestion `Aegean`, and output `Aegean`. Recorder still falls back to UIA selection for output proof; `AutoSuggestBoxInteractionTests` covers item-click submit/close behavior. |
| Layout | SplitView | `item/SplitView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/SplitView/dark-splitview.mp4` | Manifest records `IsPaneOpen` from `On` to `Off`; reviewed contact sheet shows the pane collapsed after the option toggle. |
| Media | PersonPicture | `item/PersonPicture` | Pending | Pending |  | Static route. |
| Motion | ParallaxView | `item/ParallaxView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/ParallaxView/dark-parallaxview.mp4` | Manifest records the ListView vertical scroll percent from `0` to `55`; reviewed contact sheet shows the scrolled parallax content. |
| Styles | IconElement | `item/IconElement` | Pending | Pending |  | Static route. |
| Styles | ThemeShadow | `item/ThemeShadow` | Pending | Pending |  | Static route plus shadow visibility review. |
| Windowing | TitleBar | `item/TitleBar` | Pending | Pending |  | Static route. |
| Status & info | InfoBadge | `item/InfoBadge` | Pending | Pending |  | Static route. |
| Status & info | InfoBar | `item/InfoBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/InfoBar/dark-infobar.mp4` | Manifest records `Is Open` from `On` to `Off`; reviewed contact sheet shows the first InfoBar closed. |
| Status & info | ProgressRing | `item/ProgressRing` | Recorded (active toggle) | Animation proof pending | `artifacts/gallery-recordings/20260603-062558-459/ProgressRing/dark-progressring.mp4` | Manifest records `Progress Options` from `On` to `Off`; reviewed contact sheet shows the indeterminate ring hidden after deactivation. Rendered frames did not prove pre-toggle animation, and screen capture diagnostics recorded the desktop background instead of Gallery. |
| Scrolling | PipsPager | `item/PipsPager` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-055524-741/PipsPager/dark-pipspager.mp4` | Manifest records page selection and pager item status changing to `LandscapeImage2.jpg`; reviewed frame `t7500.png` shows the second gallery image. |
| Scrolling | AnnotatedScrollBar | `item/AnnotatedScrollBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-062558-459/AnnotatedScrollBar/dark-annotatedscrollbar.mp4` | Manifest records the linked ScrollViewer vertical scroll percent from `0` to `55`; reviewed contact sheet shows the list scrolled to the magenta section. |
| Collections | PullToRefresh | `item/PullToRefresh` | Pending | Pending |  | Pull/refresh gesture required after first static pass. |
| Collections | GridView | `item/GridView` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-055524-741/GridView/dark-gridview.mp4` | Manifest records `You clicked Item 1.` after item activation; reviewed frame `t7500.png` shows the output text. |
| Collections | ItemsRepeater | `item/ItemsRepeater` | Pending | Pending |  | Scroll/virtualization interaction required after first static pass. |
| Navigation | BreadcrumbBar | `item/BreadcrumbBar` | Pending | Pending |  | Ellipsis/flyout interaction required after first static pass. |
| Navigation | Pivot | `item/Pivot` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-055524-741/Pivot/dark-pivot.mp4` | Manifest records `Unread` selected and expected content `unread emails go here.`; reviewed frame `t7500.png` shows the selected tab/content. |
| Navigation | SelectorBar | `item/SelectorBar` | Pending | Pending |  | Selection required after first static pass. |
| Navigation | NavigationView | `item/NavigationView` | Pending | Pending |  | Sample nav interaction required; shell pane tracked separately. |
| Dialogs & flyouts | ContentDialog | `item/ContentDialog` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/ContentDialog/dark-contentdialog.mp4` | Manifest records first and second dialog opens; reviewed contact sheet shows the dialog visible on sampled frames. |
| Dialogs & flyouts | Flyout | `item/Flyout` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/Flyout/dark-flyout.mp4` | Manifest records first and second flyout opens; reviewed contact sheet shows the confirmation flyout on sampled frames. |
| Dialogs & flyouts | Popup | `item/Popup` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/Popup/dark-popup.mp4` | Manifest records first and second popup opens despite low full-frame delta; reviewed contact sheet shows `Simple Popup`. |
| Menus & toolbars | MenuBar | `item/MenuBar` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/MenuBar/dark-menubar.mp4` | Manifest records first and second menu opens despite low full-frame delta; reviewed contact sheet shows the `File` menu. |
| Menus & toolbars | MenuFlyout | `item/MenuFlyout` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-063825-551/MenuFlyout/dark-menuflyout.mp4` | Manifest records first and second flyout opens with `Expanded` state; reviewed contact sheet shows the sort menu. |
| Menus & toolbars | SwipeControl | `item/SwipeControl` | Pending | Pending |  | Swipe gesture required after first static pass. |
| Menus & toolbars | AppBarButton | `item/AppBarButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-070433-922/AppBarButton/dark-appbarbutton.mp4` | Manifest records output changing to `You clicked: Button1`; reviewed contact sheet shows the click output. |
| Menus & toolbars | AppBarSeparator | `item/AppBarSeparator` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-070433-922/AppBarSeparator/dark-appbarseparator.mp4` | Static rendered route with stable button anchors; reviewed contact sheet shows separated command buttons. |
| Menus & toolbars | AppBarToggleButton | `item/AppBarToggleButton` | Recorded | No issue found in current pass | `artifacts/gallery-recordings/20260603-070433-922/AppBarToggleButton/dark-appbartogglebutton.mp4` | Manifest records toggle state from `Off` to `On`; reviewed contact sheet shows the selected toggle output. |
| Menus & toolbars | CommandBar | `item/CommandBar` | Recorded | Recorder fixed | `artifacts/gallery-recordings/20260603-070433-922/CommandBar/dark-commandbar.mp4` | Manifest records first and second overflow opens despite low full-frame delta; reviewed contact sheet shows the overflow menu. |
| Menus & toolbars | CommandBarFlyout | `item/CommandBarFlyout` | Recorded | Fixed | `artifacts/gallery-recordings/20260603-025616-794/CommandBarFlyout/dark-commandbarflyout.mp4` | Rendered MP4 shows open, MoreButton expansion, and `Resize` / `Move` secondary commands. |
