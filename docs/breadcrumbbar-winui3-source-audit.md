# BreadcrumbBar WinUI 3 Source Audit

Date: 2026-07-18

ModernWpf `BreadcrumbBar`, `BreadcrumbBarItem`, element factory, iterable, and
layout are tracked as a source-backed WPF port of official
`microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17). The current Gallery
contract is pinned to WinUI Gallery commit
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13). Live comparison uses
the installed WinUI 3 Controls Gallery `2.9.3.0` with Windows App Runtime
`2.2.3.0.0`.

## Product Source Baseline

The product repository moved its mirrored tree from `src\...` to the repository
root in `8463f45162149de0ec3ad7df752596893fe3e13e`. The last audited pre-move
product baseline is `c70471c511a0168b61dcca13af9556465f26b673`; the only
Breadcrumb history after that baseline is the root move, so all substantive
runtime, theme, peer, resource, API-test, and interaction-test blobs remain
byte-current. Current paths below intentionally omit `src\`.

Primary current WinUI 3 inputs and blob IDs:

| Source | Current blob |
| --- | --- |
| `controls\dev\Breadcrumb\APITests\BreadcrumbTests.cs` | `c3abf33ba71e396341a165183ab8e4a6202b4bfb` |
| `controls\dev\Breadcrumb\BreadcrumbBar.cpp` | `2c84e298b228b8455143b3eaa84d847782ab99eb` |
| `controls\dev\Breadcrumb\BreadcrumbBar.h` | `fa1653ac70e1e0bf78b255ef0167b4032d9b321a` |
| `controls\dev\Breadcrumb\BreadcrumbBar.idl` | `3cf9f2d6b8119a94c057d97a5755541430885f8f` |
| `controls\dev\Breadcrumb\BreadcrumbBar.xaml` | `d7d0b16aae852dc792122bcfc1d788237f784209` |
| `controls\dev\Breadcrumb\BreadcrumbBarElementFactory.cpp` | `04a88235546743e6ff8a9157658240975bcfebbc` |
| `controls\dev\Breadcrumb\BreadcrumbBarItem.cpp` | `eddccd5245273d209389a7039795d472d72f2777` |
| `controls\dev\Breadcrumb\BreadcrumbBarItemAutomationPeer.cpp` | `10e9f14396d84c7e5f59c38a7a57c5d97cec66a7` |
| `controls\dev\Breadcrumb\BreadcrumbBar_perf2026.xaml` | `52e69acc2ba289e1a075b8fce343b6014b1c2eec` |
| `controls\dev\Breadcrumb\BreadcrumbBar_themeresources.xaml` | `af094a1594bb76d73034e91d8c6e9677d7c67e15` |
| `controls\dev\Breadcrumb\BreadcrumbLayout.cpp` | `e9bcb9f1fe7fe2e6f9e67e7bc3048de96237aa62` |
| `controls\dev\Breadcrumb\InteractionTests\BreadcrumbBarTests.cs` | `d91cbb50e9eabd9be224962f77f905dfc7c621c7` |
| `controls\dev\Breadcrumb\Strings\en-us\Resources.resw` | `7dee4b0304407292f7badf8742373802d22a5a17` |

The new current `BreadcrumbBar_perf2026.xaml` packages the same setter-driven
item states, item-button states, geometry, focus targets, and resource aliases
as the classic dictionary. ModernWpf's `VisualStateEx.Setters` port therefore
already represents both current upstream template variants.

## Current Gallery Baseline

WinUI Gallery converted this page to `SampleDefinition` files in
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`; no later BreadcrumbBar commit is
present through the current pin. Current paths and blobs are:

| Gallery source | Current blob |
| --- | --- |
| `WinUIGallery\Samples\BreadcrumbBar\BreadcrumbBarPage.xaml` | `ac1587dfbe112353cedef00f96b42b994390b836` |
| `WinUIGallery\Samples\BreadcrumbBar\BreadcrumbBarPage.xaml.cs` | `5b13ec7b35fe35851b4c717f66fc17858f1cc976` |
| `WinUIGallery\Samples\BreadcrumbBar\BreadcrumbbarControl.txt` | `088a1e244ed5054068a08fc3d56a378588cc7ce4` |
| `WinUIGallery\Samples\BreadcrumbBar\BreadcrumbbarControlCustomDatatemplate.txt` | `d88e0ad3598f977c4b40d349bd5a67d5ebc8dae7` |

The page still has exactly two examples: the eight-string breadcrumb used by
the primary pixel proof, and a four-folder custom-template breadcrumb whose
`Folder1` invocation removes `Folder2` and `Folder3`. Its reset button restores
the original collection. `ModernWpf.Gallery\Pages\NavigationSampleFactory.cs`
keeps those headers, names, item sets, templates, snippets, click behavior, and
reset behavior.

## Ported Runtime and Visual Shape

- The default template uses source-shaped `PART_ItemsRepeater`; the older
  guessed `PART_RootPanel` and manual `StackPanel` rebuild path remain deleted.
- `BreadcrumbIterable` inserts the leading ellipsis item at repeater index `0`,
  and `BreadcrumbElementFactory` wraps data items while forwarding the item
  template.
- `BreadcrumbLayout` measures all items, renders the ellipsis only on overflow,
  hides the earliest items, arranges the visible suffix, and asks the owner to
  re-index visible items for accessibility.
- Item states cover `Inline`, `EllipsisDropDown`, `Default`, `DefaultRTL`,
  `LastItem`, `Ellipsis`, and `EllipsisRTL`; common-state setters, focus targets,
  content presenters, chevrons, flyout chrome, and theme resources follow the
  current classic/perf-2026 template contract.
- Hidden elements are cloned in reverse order into the ellipsis flyout repeater;
  flyout indexes map back to original item indexes and route through
  `ItemClicked`.
- `Segoe UI Variable Text, Segoe UI`, WinUI-default layout rounding, and
  `ContentPresenterEx` physical-pixel text measurement ceilings produce the
  exact installed-Gallery item-width vector `56,89,61,84,63,63,65,49` and exact
  `530x26` primary geometry.

## Current Accessibility Contract

- `BreadcrumbBarItemAutomationPeer` exposes the source Button control type,
  `BreadcrumbBarItem` class name, Invoke pattern, and click routing.
- The peer now resolves `BreadcrumbBarItemLocalizedControlType` through the
  control resource pack; the en-US value is the upstream `breadcrumb bar item`.
- The leading ellipsis name now resolves
  `AutomationNameEllipsisBreadcrumbBarItem` through the resource pack instead
  of a hardcoded literal; the en-US value is the upstream `More`.
- Current WinUI sets a hidden ellipsis to `AccessibilityView.Raw` and a rendered
  ellipsis to `AccessibilityView.Content`. WPF has no `AccessibilityView`, so
  the item peer gates `IsControlElementCore` and `IsContentElementCore` on the
  same layout state. `ReIndexVisibleElementsForAccessibility` invalidates the
  peer when that state changes, making the UIA tree update immediately.
- Visible user items retain source position-in-set and size-of-set metadata on
  target frameworks where WPF exposes those attached properties.

## WPF Substitutions

- WinUI `Grid.CornerRadius`, `Grid.BackgroundSizing`, and `ContentPresenter`
  chrome are represented by `GridEx` and `ContentPresenterEx`.
- WinUI `VisualState.Setters` are represented by `VisualStateEx.Setters`.
- WinUI can name a `Flyout` directly in a template. WPF keeps the same resource
  key and instantiates the ellipsis repeater before opening its popup.
- WinUI pointer events, `FocusState`, gamepad navigation, access-key routing,
  XamlRoot focus services, and compositor behavior map to WPF mouse capture,
  focus/navigation, popup, and layout services where feasible.
- Only the en-US Breadcrumb resource pack is currently added. Other WinUI
  translations remain a localization follow-up, not an English accessibility
  or runtime parity gap.
- WPF and WinUI use different DirectWrite integration/rasterization paths. With
  geometry and colors exact, the remaining live delta is confined to glyph
  antialiasing; forcing WPF grayscale rendering was measured and rejected
  because it worsened the Dark score.

## Regression Guards

- `BreadcrumbBarSourceAuditTests` pins the current product/Gallery commits,
  root-move boundary, product and Gallery blobs, current paths, perf-2026
  mapping, resource-backed accessibility contract, strict artifact evidence,
  and Gallery interaction harness.
- `BreadcrumbBarApiTests.AutomationPeerMatchesWinUILocalizedTypeAndEllipsisAccessibilityView`
  proves class name, Button role, localized type, resource-backed `More`, hidden
  Control/Content exclusion, visible restoration, and overflow layout state.
- The existing product suite covers default API values, template/resources,
  state setters and focus targets, ItemsRepeater use, overflow arrangement,
  hidden-item ordering, flyout click mapping, item clicks, collection changes,
  and rendered pixels.
- `GalleryAutomationHookTests.BreadcrumbBarSampleMatchesWinUIGalleryExamples`
  covers both current examples, snippets, custom data template, click removal,
  and reset restoration.
- `WpfGallerySourceShapeTests.GalleryVisualChecksEnforceBreadcrumbBarPixelParityThreshold`
  pins the ModernWpf/reference element IDs and strict `3.0` primary gate.

## Current Validation

- Fresh Light comparison
  `artifacts/visual-checks/20260718-205617-796-43088/report.md` passes at `2.53`;
  fresh Dark comparison
  `artifacts/visual-checks/20260718-205659-478-24012/report.md` passes at `2.33`.
  Both compare exact `530x26` live primary controls under the `3.0` gate.
- Fresh Light breadcrumb recording
  `artifacts/gallery-recordings/20260718-205742-447/report.md` passes with
  `0.037` maximum frame delta and `2.432` maximum local delta. Fresh Dark
  recording `artifacts/gallery-recordings/20260718-205804-316/report.md` passes
  with `0.06` / `3.874`. Both invoke the Button peer for `Folder1`, prove
  `Folder2` and `Folder3` disappear, and finish within `1.9s` of the six-second
  maximum window.
- The complete BreadcrumbBar/ContentPresenterEx product slice passes 27/27 on
  `net8.0-windows7.0`.
- The BreadcrumbBar Gallery sample/crop slice passes on both net8 and net10.
- `ModernWpf.Gallery` builds successfully for net462, net8, and net10. The
  fresh net8 visual build is warning-free; net462 reports 20 existing unrelated
  warnings and net10 emits existing generated/source warnings, with zero errors
  and no BreadcrumbBar warning on either target.
