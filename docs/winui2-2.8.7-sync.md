# WinUI 2.8.7 Sync Matrix

Source of truth: `D:\repos\microsoft-ui-xaml` tag `v2.8.7`

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
| AutoSuggestBox | Pending | Pending | Existing WPF port; sync 2.8.7 resources, behavior fixes, API and interaction tests. |
| BreadcrumbBar | Pending | Pending | Add feasible WPF control port. |
| ColorPicker / ColorSpectrum | Pending | Pending | Add feasible WPF control port and color math tests. |
| ComboBox helper/styles | Pending | Pending | Map to WPF ComboBox style/resource parity rather than a new control. |
| CommandBar / AppBarButton / AppBarToggleButton / AppBarSeparator | Pending | Pending | Existing WPF port; sync resources and tests. |
| CommandBarFlyout / TextCommandBarFlyout | Pending | Pending | Existing WPF port; sync command bar flyout behavior and tests. |
| CommonStyles and compact density resources | Pending | Pending | Sync resource keys across Light, Dark, and HighContrast. |
| ContentDialog | Pending | Pending | Existing WPF port; sync resources and behavior tests. |
| DropDownButton | Pending | Pending | Existing WPF port; sync resources and tests. |
| Expander | Pending | Pending | WPF has stock Expander; align ModernWpf styling and WinUI resource/API expectations where feasible. |
| IconSource / ImageIcon | Pending | Pending | Existing IconElement/IconSource surface; sync WinUI 2.8.7 API/resource behavior where feasible. |
| InfoBadge | Pending | Pending | Add feasible WPF control port. |
| InfoBar | Pending | Pending | Add feasible WPF control port. |
| LayoutPanel | Pending | Pending | Existing WPF port; sync tests. |
| Materials / Acrylic / Reveal / Lights / Effects | Excluded | Excluded | UWP/WinUI compositor material system; keep ModernWpf's existing WPF material behavior separate. |
| MenuBar | Pending | Pending | Add feasible WPF control/style port or map to WPF Menu when API parity is not practical. |
| NavigationView | Pending | Pending | Existing WPF port; sync 2.8.7 behavior, top mode, selection, pane, and resource tests. |
| NumberBox | Pending | Pending | Existing WPF port; sync parser, validation, spin button, and resource behavior. |
| PagerControl | Pending | Pending | Add feasible WPF control port if distinct from PipsPager. |
| ParallaxView | Pending | Pending | Feasibility depends on WPF scroll/transform equivalent; document exclusions for compositor-only behavior. |
| PersonPicture | Pending | Pending | Existing WPF port; sync initials/badge behavior and tests. |
| PipsPager | Pending | Pending | Add feasible WPF control port. |
| ProgressBar resources | Pending | Pending | Map to WPF ProgressBar style/resource parity. |
| ProgressRing | Pending | Pending | Existing WPF port; sync determinate/indeterminate behavior and tests. |
| PullToRefresh / RefreshContainer | Pending | Pending | Add feasible WPF interaction port where input semantics can be represented. |
| RadioButtons | Pending | Pending | Existing WPF port; sync layout/focus tests. |
| RadioMenuFlyoutItem | Pending | Pending | Existing RadioMenuItem maps this surface; sync API/resource tests. |
| RatingControl | Pending | Pending | Existing WPF port; sync precision, placeholder, automation, and input behavior. |
| Repeater / ItemsRepeater layouts | Pending | Pending | Existing WPF port; sync layout, recycle, selection, and viewport tests. |
| ScrollPresenter / ScrollView | Excluded | Excluded | Large WinUI scrolling primitive; WPF ScrollViewer remains the platform primitive. Port only resource/style implications. |
| SplitButton | Pending | Pending | Existing WPF port; sync behavior and tests. |
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
| `test/MUXControlsTestApp/ThemeResourcesTests.cs` | `ModernWpf.WinUI.Tests` | Pending | Must prove Light/Dark/HighContrast key parity. |
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
