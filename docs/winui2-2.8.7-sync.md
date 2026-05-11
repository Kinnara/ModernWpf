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
| AnimatedIcon | Excluded | Excluded | Depends on WinUI animated icon source infrastructure and compositor animation semantics not present in WPF. |
| AnimatedVisualPlayer | Excluded | Excluded | Depends on WinUI visual/lottie animation pipeline; do not add as ModernWpf core surface. |
| AutoSuggestBox | Pending | Ported APITest slice | Existing WPF port; upstream suggestion-popup corner-radius behavior is covered. Visual-tree snapshots, resource refresh, and broader interaction tests remain pending. |
| BreadcrumbBar | Pending | Pending | Add feasible WPF control port. |
| ColorPicker / ColorSpectrum | Pending | Pending | Add feasible WPF control port and color math tests. |
| ComboBox helper/styles | Pending | Ported APITest slice | WPF ComboBox style/helper maps this surface; upstream open-dropdown corner-radius behavior is covered for standard and editable modes. Broader visual-tree/resource snapshots remain pending. |
| CommandBar / AppBarButton / AppBarToggleButton / AppBarSeparator | Pending | Pending | Existing WPF port; sync resources and tests. |
| CommandBarFlyout / TextCommandBarFlyout | Pending | Ported APITest slices | Existing WPF port; default command collection and command propagation API coverage are ported. Sizing, overflow, and popup interaction tests remain pending for WPF popup-host adaptation. |
| CommonStyles and compact density resources | Pending | Ported initial API + resource override slices | `VerifyAllThemesContainSameResourceKeys`, `VerifyUseCompactResourcesAPI`, `CornerRadiusFilterConverterTest`, and `ThemeResourcesTests.VerifyOverrides` are ported; visual-tree and baseline resource tests remain pending. |
| ContentDialog | Pending | Pending | Existing WPF port; sync resources and behavior tests. |
| DropDownButton | Pending | Ported interaction slice | Existing WPF port; upstream accessibility/expand-collapse interaction coverage is mapped through WPF automation peer and flyout open/close tests. Resource and broader popup/input tests remain pending. |
| Expander | Pending | Ported APITest + interaction slices | WPF has stock Expander; WPF automation peer expand/collapse behavior is covered. Header/content accessibility intent from upstream APITests is mapped through WPF `AccessibilityView` and collapsed content visibility. ModernWpf styling and resource checks remain pending. |
| IconSource / ImageIcon | Pending | Ported WPF-feasible APITests | Existing Symbol/Font/Bitmap/Path IconSource API propagation is covered. `ImageIconSource`, `AnimatedIconSource`, text-scale/mirroring APIs, and WinUI XAML metadata provider tests are not present in the current WPF surface and remain pending/excluded for feasibility review. |
| InfoBadge | Pending | Pending | Add feasible WPF control port. |
| InfoBar | Pending | Pending | Add feasible WPF control port. |
| LayoutPanel | Pending | Ported APITests | Existing WPF port; upstream padding/border layout-offset, dynamic layout switching, and custom non-virtualizing layout APITests are ported. Interaction with broader ItemsRepeater layout coverage remains pending. |
| Materials / Acrylic / Reveal / Lights / Effects | Excluded | Excluded | UWP/WinUI compositor material system; keep ModernWpf's existing WPF material behavior separate. |
| MenuBar | Pending | Pending | Add feasible WPF control/style port or map to WPF Menu when API parity is not practical. |
| NavigationView | Pending | Pending | Existing WPF port; sync 2.8.7 behavior, top mode, selection, pane, and resource tests. |
| NumberBox | Pending | Ported APITests + interaction slices | WinUI 2.8.7 API tests for text alignment, input scope, enabled visual state, and UIA name forwarding are ported. Interaction coverage now includes spin buttons, spin-button enabled state, value/text sync, min/max coercion, and validation-disabled range behavior. Parser, keyboard, scroll, custom formatter, and accessibility scan coverage remain pending. |
| PagerControl | Pending | Pending | Add feasible WPF control port if distinct from PipsPager. |
| ParallaxView | Pending | Pending | Feasibility depends on WPF scroll/transform equivalent; document exclusions for compositor-only behavior. |
| PersonPicture | Pending | Ported WPF-feasible APITests | Existing WPF port; upstream defaults, automation name, small-size safety, initials/group visual states, and visual-tree smoke coverage are ported. WinUI `Contact`, `PreferSmallImage`, XAML metadata provider, and pixel-baseline verification remain excluded/pending for WPF feasibility review. |
| PipsPager | Pending | Pending | Add feasible WPF control port. |
| ProgressBar resources | Pending | Ported APITests + interaction slices | Upstream `ProgressBarTrackHeight` resource overridability test is ported. Interaction coverage now includes range automation, value/min/max updates, indicator-width recalculation, padding offset, and indeterminate range-pattern suppression. Broader visual-state and retemplate parity remains pending. |
| ProgressRing | Pending | Ported APITests + interaction automation parity | Existing WPF port; upstream inactive accessibility/raw-view behavior is mapped through the WPF automation peer. Ported the indeterminate automation test that ensures no RangeValue pattern is exposed. Visual behavior tests still pending. |
| PullToRefresh / RefreshContainer | Pending | Pending | Add feasible WPF interaction port where input semantics can be represented. |
| RadioButtons | Pending | Ported APITests | Existing WPF port; upstream custom item-template wrapping and IsEnabled visual-state coverage are now ported. Layout/focus tests still pending. |
| RadioMenuFlyoutItem | Ported group selection logic | Ported interaction slice | Existing RadioMenuItem maps this surface; upstream basic and submenu selection behavior is covered. Resource and submenu check-state visual attached-property coverage remain pending. |
| RatingControl | Pending | Ported APITests + interaction automation slices | Existing WPF port; upstream API defaults, image item assignment, collapsed value set, and value coercion are now covered. UIA value/range text and automation property coverage is ported. Pointer, keyboard, binding, item-info visual fallback, and input behavior remain pending. |
| Repeater / ItemsRepeater layouts | Pending | Ported initial APITest slices | Existing WPF port; upstream `IndexPath`, `ItemsSourceView`, `RecyclePool`, broad `SelectionModel` API/event/range/nested/mutation/property/regression/children-requested coverage, direct `ItemTemplate` / `RecyclingElementFactory` API coverage, direct `ElementAnimator` API coverage, and basic `ItemsRepeater` API/element-mapping coverage are ported. WinRT-only `ICustomPropertyProvider` selection metadata is excluded; layout, advanced selection helpers, viewport, phasing, focus, data-source integration, and visual tests remain pending. |
| ScrollPresenter / ScrollView | Excluded | Excluded | Large WinUI scrolling primitive; WPF ScrollViewer remains the platform primitive. Port only resource/style implications. |
| SplitButton | Pending | Ported APITests + interaction slices | Existing WPF port; upstream default/setter coverage and ToggleSplitButton IsChecked API are now covered. Interaction coverage now includes primary/secondary button behavior, command enablement, automation invoke/expand/collapse, keyboard Space/F4 routing, and ToggleSplitButton automation. Broader visual-state matrix and touch-specific behavior remain pending. |
| SplitView | Pending | Pending | Existing WPF port; sync resource/style behavior and TestUI coverage. |
| SwipeControl | Pending | Pending | Add feasible WPF control port. |
| TabView | Pending | Pending | Add feasible WPF control port or complete WPF TabControl-compatible API mapping. |
| TeachingTip | Pending | Pending | Add feasible WPF popup/teaching surface port. |
| TitleBar | Mapped | Pending | ModernWpf has WPF title bar helpers; map WinUI TitleBar tests to WPF window chrome behavior. |
| TreeView | Pending | Pending | WPF has stock TreeView; add WinUI-style node/control parity where feasible. |
| TwoPaneView | Pending | Pending | Add feasible WPF layout control port. |
| WebView2 | Optional | Pending | Use `Microsoft.Web.WebView2.Wpf` only in docs/gallery samples; no core ModernWpf dependency. |

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
