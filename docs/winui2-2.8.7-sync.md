# WinUI 2.8.7 Sync Matrix

Source of truth: `D:\repos\microsoft-ui-xaml-v2.8.7`, a detached worktree at tag `v2.8.7`

Verified tag:

```text
232a16e5ddfc22c9a1b79a2c51abeb9a39a94494
2025-01-31 18:58:01 +0000
Merged PR 12217986: WebView2 test fixes and update TSA options area path
```

This file tracks the ModernWpf parity plan against final WinUI 2.8.7. Every upstream area should end in one of these states:

- `Ported`: implemented in ModernWpf with upstream-derived tests.
- `Mapped`: covered by an existing WPF or ModernWpf equivalent with tests.
- `Optional`: supported through documentation/sample integration, not a core dependency.
- `Excluded`: not feasible or not appropriate for WPF; reason required.
- `Pending`: not completed yet.

## Stage 1 Status

Stage 1 retires the previous ad hoc tests from the solution and introduces the upstream-derived WPF test harness:

- `test\ModernWpf.WinUI.TestInfra`: dispatcher, window-host, input, and visual-tree helpers for ported WinUI tests.
- `test\ModernWpf.WinUI.TestApp`: WPF TestUI host assembly that will receive ported upstream TestUI pages.
- `test\ModernWpf.WinUI.Tests`: MSTest project for ported upstream API and interaction tests.

The old test projects remain on disk for reference while porting, but are no longer authoritative solution gates.

## Control And Feature Matrix

| Upstream WinUI 2.8.7 area | ModernWpf status | Test status | Notes |
| --- | --- | --- | --- |
| AnnotatedScrollBar | Pending | Pending | No current ModernWpf surface. Evaluate a WPF ScrollBar/adorner-style port before adding tests. |
| AnimatedIcon | Excluded | Excluded | Depends on WinUI animated icon source infrastructure and compositor animation semantics not present in WPF. |
| AnimatedVisualPlayer | Excluded | Excluded | Depends on WinUI visual/lottie animation pipeline; do not add as ModernWpf core surface. |
| AutoSuggestBox | Pending | Ported APITest + interaction slices | Existing WPF port; upstream suggestion-popup corner-radius behavior and suggestion-selection interaction are covered. Visual-tree snapshots, resource refresh, and accessibility scan coverage remain pending. |
| BreadcrumbBar | Pending | Pending | Add feasible WPF control port. |
| ButtonInteraction / SliderInteraction | Mapped to WPF platform controls | Excluded | Upstream tests exercise WinUI interaction helper infrastructure for platform Button/Slider behavior. ModernWpf relies on WPF Button/Slider semantics rather than owning a separate control surface. |
| CalendarView common styles | Mapped to WPF Calendar/DatePicker styles | Excluded upstream snapshot | ModernWpf carries CalendarView/CalendarDatePicker resource keys through WPF Calendar and DatePicker styles. The final WinUI APITest is disabled upstream and only verifies a visual-tree baseline snapshot. |
| ColorPicker / ColorSpectrum | Pending | Pending | Add feasible WPF control port and color math tests. |
| ComboBox helper/styles | Pending | Ported APITest slice | WPF ComboBox style/helper maps this surface; upstream open-dropdown corner-radius behavior is covered for standard and editable modes. Broader visual-tree/resource snapshots remain pending. |
| CommandBar / AppBarButton / AppBarToggleButton / AppBarSeparator | Pending | Pending | Existing WPF port; sync resources and tests. |
| CommandBarFlyout / TextCommandBarFlyout | Pending | Ported APITest slices | Existing WPF port; default command collection, command propagation, and command-bar/overflow sizing API coverage are ported. Primary-command overflow and popup interaction tests remain pending for WPF popup-host adaptation. |
| CommonStyles and compact density resources | Pending | Ported initial API + resource override slices | `VerifyAllThemesContainSameResourceKeys`, `VerifyUseCompactResourcesAPI`, `CornerRadiusFilterConverterTest`, and `ThemeResourcesTests.VerifyOverrides` are ported; visual-tree and baseline resource tests remain pending. |
| ContentDialog | Pending | Pending | Existing WPF port; sync resources and behavior tests. |
| DropDownButton | Pending | Ported interaction slice | Existing WPF port; upstream accessibility/expand-collapse interaction coverage is mapped through WPF automation peer and flyout open/close tests. Resource and broader popup/input tests remain pending. |
| Expander | Pending | Ported APITest + interaction slices | WPF has stock Expander; WPF automation peer expand/collapse behavior is covered. Header/content accessibility intent from upstream APITests is mapped through WPF `AccessibilityView` and collapsed content visibility. ModernWpf styling and resource checks remain pending. |
| IconSource / ImageIcon | Ported IconSource + ImageIcon; AnimatedIcon excluded | Ported WPF-feasible APITests | Symbol/Font/Bitmap/Path/Image IconSource API propagation, created element types, foreground propagation, and image rendering smoke coverage are ported. `AnimatedIconSource`, text-scale/mirroring APIs, and WinUI XAML metadata provider tests are excluded because the WinUI animated/compositor and XAML metadata provider surfaces are not present in WPF. |
| InfoBadge | Pending | Pending | Add feasible WPF control port. |
| InfoBar | Pending | Pending | Add feasible WPF control port. |
| ItemsView / ItemContainer | Pending | Pending | No current ModernWpf surface. These are newer WinUI items primitives distinct from the existing ListView/ItemsRepeater ports. |
| LayoutPanel | Pending | Ported APITests | Existing WPF port; upstream padding/border layout-offset, dynamic layout switching, and custom non-virtualizing layout APITests are ported. Interaction with broader ItemsRepeater layout coverage remains pending. |
| MapControl | Excluded | Excluded | Upstream API test class is empty; the control itself is a provider-dependent map surface outside ModernWpf core. |
| Materials / Acrylic / Reveal / Lights / Effects | Excluded | Excluded | UWP/WinUI compositor material system; keep ModernWpf's existing WPF material behavior separate. |
| MenuBar | Pending | Pending | Add feasible WPF control/style port or map to WPF Menu when API parity is not practical. |
| NavigationView | Pending | Ported APITest slices | Existing WPF port; upstream defaults/basic setters, value coercion, pane-property API checks, pane/display-mode mapping, pane launch interplay, selected-item clearing, navigation item UIA type, expand/collapse pattern availability, settings item tag/tooltip, footer-item no-crash, menu item/container mapping, top-mode collection clearing, top-mode hierarchy, item tooltip, item-outlives-parent, closed-compact state, chevron visibility, and overflow tooltip checks are ported. Visual-tree, header margin, top-mode layout details, and resource tests remain pending. |
| NumberBox | Pending | Ported APITests + interaction slices | WinUI 2.8.7 API tests for text alignment, input scope, enabled visual state, and UIA name forwarding are ported. Interaction coverage now includes spin buttons, spin-button enabled state, value/text sync, value-changed events, min/max coercion, validation-disabled range behavior, keyboard commit/cancel/step behavior, mouse-wheel stepping, expression parsing, custom formatter behavior, and header presenter behavior. Right-click selection and accessibility scan coverage remain pending. |
| PagerControl | Pending | Pending | Add feasible WPF control port if distinct from PipsPager. |
| ParallaxView | Pending | Pending | Feasibility depends on WPF scroll/transform equivalent; document exclusions for compositor-only behavior. |
| PersonPicture | Pending | Ported WPF-feasible APITests + interaction slices | Existing WPF port; upstream defaults, automation name, small-size safety, initials/group visual states, rendered initials/badge behavior, image priority, image clear behavior, and visual-tree smoke coverage are ported. WinUI `Contact`, `PreferSmallImage`, XAML metadata provider, list scrolling, and pixel-baseline verification remain excluded/pending for WPF feasibility review. |
| PipsPager | Pending | Pending | Add feasible WPF control port. |
| ProgressBar resources | Pending | Ported APITests + interaction slices | Upstream `ProgressBarTrackHeight` resource overridability test is ported. Interaction coverage now includes range automation, value/min/max updates, indicator-width recalculation, padding offset, visual-state transitions, retemplate width/state behavior, and indeterminate range-pattern suppression. Broader visual rendering parity remains pending. |
| ProgressRing | Pending | Ported APITests + interaction automation/state parity | Existing WPF port; upstream inactive accessibility/raw-view behavior is mapped through the WPF automation peer. Final WinUI 2.8 `IsIndeterminate`, `Value`, `Minimum`, and `Maximum` API/automation parity is ported, with tests for defaults, active/determinate/inactive states, determinate range automation, value/min/max coercion, and indeterminate RangeValue suppression. Lottie/custom animated visual source behavior remains excluded for WPF. |
| PullToRefresh / RefreshContainer | Pending | Pending | Add feasible WPF interaction port where input semantics can be represented. |
| RadioButtons | Pending | Ported APITests + selection/keyboard/focus/layout/UIA interaction tests | Existing WPF port; upstream custom item-template wrapping, IsEnabled visual-state, selected-index, selected-item, focus handoff, checked-item insertion, column-layout test-hook coverage, UIA position/size updates, basic keyboard, multi-column keyboard, single-row keyboard, and disabled-item keyboard coverage are now ported. Access-key, control-modifier, and scroll/focus regression tests remain pending. |
| RadioMenuFlyoutItem | Ported group selection logic + submenu check state | Ported interaction slice | Existing RadioMenuItem maps this surface; upstream basic and submenu selection behavior is covered. The `AreCheckStatesEnabled` submenu attached-property behavior and check-glyph visual state are ported. Broader resource snapshot coverage remains pending. |
| RadialGradientBrush | Mapped to WPF platform brush | Excluded | WPF already provides `System.Windows.Media.RadialGradientBrush`; upstream coverage is TestUI interaction/rendering for the WinUI platform brush rather than a ModernWpf-owned control. |
| RatingControl | Pending | Ported APITests + interaction/resource slices | Existing WPF port; upstream API defaults, image item assignment, collapsed value set, value coercion, resource-size overrides, keyboard input, two-way value binding, read-only input suppression, max-rating UIA text, value/range text, and automation property coverage are ported. Pointer and item-info visual fallback behavior remain pending. |
| Repeater / ItemsRepeater layouts | Pending | Ported initial APITest slices | Existing WPF port; upstream `IndexPath`, `ItemsSourceView`, `RecyclePool`, broad `SelectionModel` API/event/range/nested/mutation/property/regression/children-requested coverage, direct `ItemTemplate` / `RecyclingElementFactory` API coverage, direct `ElementAnimator` API coverage, and basic `ItemsRepeater` API/element-mapping coverage are ported. WinRT-only `ICustomPropertyProvider` selection metadata is excluded; layout, advanced selection helpers, viewport, phasing, focus, data-source integration, and visual tests remain pending. |
| ScrollPresenter / ScrollView | Excluded | Excluded | Large WinUI scrolling primitive; WPF ScrollViewer remains the platform primitive. Port only resource/style implications. |
| SelectorBar | Pending | Pending | No current ModernWpf surface. Add a WPF selector command-strip port before upstream API/interaction tests can be mapped. |
| SplitButton | Pending | Ported APITests + interaction slices | Existing WPF port; upstream default/setter coverage and ToggleSplitButton IsChecked API are now covered. Interaction coverage now includes primary/secondary button behavior, command enablement, automation invoke/expand/collapse, keyboard Space/F4 routing, and ToggleSplitButton automation. Broader visual-state matrix and touch-specific behavior remain pending. |
| SplitView | Pending | Pending | Existing WPF port; sync resource/style behavior and TestUI coverage. |
| SwipeControl | Pending | Pending | Add feasible WPF control port. |
| SystemBackdropElement | Excluded | Excluded | Depends on WinUI `SystemBackdrop`, Mica, DesktopAcrylic, and compositor-backed lifecycle semantics. Keep ModernWpf material behavior separate. |
| TabView | Pending | Pending | Add feasible WPF control port or complete WPF TabControl-compatible API mapping. |
| TeachingTip | Pending | Pending | Add feasible WPF popup/teaching surface port. |
| TitleBar | Mapped | Pending | ModernWpf has WPF title bar helpers; map WinUI TitleBar tests to WPF window chrome behavior. |
| TreeView | Pending | Pending | WPF has stock TreeView; add WinUI-style node/control parity where feasible. |
| TwoPaneView | Pending | Pending | Add feasible WPF layout control port. |
| WebView2 | Optional | Pending | Use `Microsoft.Web.WebView2.Wpf` only in docs/gallery samples; no core ModernWpf dependency. |
| WrapPanel | Ported | Ported APITests | Dedicated WPF-compatible port added for WinUI Padding, ItemSpacing, LineSpacing, Orientation, and ItemsStretch behavior. Upstream layout tests for padding offset, horizontal/vertical wrapping, spacing, collapsed children, dynamic orientation changes, variable child sizes, and last-item stretch are ported. |

## Test Port Matrix

| Upstream test category | ModernWpf target | Status | Notes |
| --- | --- | --- | --- |
| `dev/*/APITests` | `ModernWpf.WinUI.Tests` | Pending | Port all tests for implemented or mapped controls. |
| `dev/*/InteractionTests` | `ModernWpf.WinUI.Tests` + `ModernWpf.WinUI.TestApp` | Pending | Replace UWP input helpers with WPF dispatcher/window/input helpers. |
| `dev/*/TestUI` pages | `ModernWpf.WinUI.TestApp` | Pending | Port pages for implemented controls; document excluded pages. |
| `test/MUXControlsTestApp/ThemeResourcesTests.cs` | `ModernWpf.WinUI.Tests` | Ported WPF-feasible slice | `VerifyOverrides` is ported for `RatingControlCaptionForeground` application-resource overrides; Light/Dark/HighContrast key parity is covered by `CommonStylesResourceTests`. |
| Localization/resource tests | `ModernWpf.WinUI.Tests` | Pending | Port for resources that remain in ModernWpf. |
| Leak and compositor tests | WPF equivalent or exclusion | Pending | Add concrete exclusions when no WPF behavior exists. |

## Retired Local Tests

These projects were useful during earlier migration work but are not sufficient as final WinUI parity proof:

- `test\ModernWpf.Test`
- `test\ModernWpfTestApp`
- `test\NavigationView_TestUI`
- `test\TestAppUtils`
- `test\ModernWpf.Gallery.Tests`
- `test\ItemsRepeaterTestApp`

They may be consulted while porting but should not be treated as completion evidence.
