# TitleBar current WinUI 3 and Gallery parity audit

Date: 2026-07-18
Updated: 2026-07-30

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml | `de3e767333c2f0717a6a70cb22bd192ced5ad885` | Detailed `winui3/main` product/template snapshot audited here. |
| microsoft-ui-xaml | `eb75504a1978df0d37a3ad4574d6f72bf4d21583` | Current-source epoch target; its TitleBar API-status delta is disposed below. |
| microsoft-ui-xaml | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Latest stable `winui3/release/2.3.1` snapshot content-reconciled with the main epoch. |
| microsoft-ui-xaml | `8463f45162149de0ec3ad7df752596893fe3e13e` | 2026-05-30 root-layout move from `src\controls` to `controls`; no TitleBar behavior or template change. |
| WinUI Gallery | `29f62479d5c046a0b854a5868e5a7cd484572d87` | Current Gallery source and installed-app comparison target. |
| WinUI Gallery | `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` | Gallery sample-folder conversion baseline. |
| WinUI Gallery | `9a14fa563584b19c06e3baccf10664a12f84fad5` | Stretchable search content and the drag-region example added by PR #2182. |

The prior product pin `c70471c511a0168b61dcca13af9556465f26b673`
and the current product pin are behaviorally equivalent for TitleBar. Existing
files moved from the former src-prefixed root to
`controls\dev\TitleBar`. The only implementation edit is a range-loop value
changing from `const auto` to `const auto&` in `UpdateDragRegion`, avoiding a
copy without changing behavior. The Performance2026 theme dictionary was also
added and expresses the same resources, geometry, and visual states with
setter-style assignments.

### 2026-07-29 epoch reconciliation

Main-line commit `54c81dcacb9d6e01a30da7c5299bfd4bf661d43e`
changes `AutoRefreshDragRegions`, nullable attached `IsDragRegion`, and
`RecomputeDragRegions` in `TitleBar.idl` from `MUX_PREVIEW` to
`MUX_PUBLIC_V11`. These are current public V11 APIs, not preview APIs. The
change is API-status metadata only; it does not change TitleBar runtime,
template, resources, or Gallery behavior.

ModernWpf does not currently ship a WinUI `TitleBar` clone. Its
`WindowTitleBar` attached-property surface and `WindowTitleBarControl` remain a
documented WPF window-shell facade that adapts title content, buttons, drag
behavior, and `WindowChrome`. Promoting WinUI's APIs therefore corrects the
authority/status record but does not add those members to the differently
scoped facade. A future source-shaped `TitleBar` port can use the unclaimed
`TitleBar` name and must treat these V11 members as public API from its first
preview.

## Current WinUI product inventory

The following Git blob IDs pin the exact product inputs:

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls\dev\Generated\TitleBar.properties.cpp` | `5134559ff2de382759847e60c6797e16837db1a9` |
| `controls\dev\Generated\TitleBar.properties.h` | `10e7745bb9a60f2511e526ec2db787bdcea5b91d` |
| `controls\dev\Generated\TitleBarAutomationPeer.properties.cpp` | `567097eeb38ec79cbd9a3dc81e8d7d37f1915f44` |
| `controls\dev\Generated\TitleBarTemplateSettings.properties.cpp` | `71ca4c01e2445b4d8676d3385a268a7ba9cc1a30` |
| `controls\dev\Generated\TitleBarTemplateSettings.properties.h` | `bc110ebde46759b065e2d4a4f85b524cb7e58604` |
| `controls\dev\TitleBar\TitleBar.cpp` | `e6885f5fb8c7deb5f6e552c7e88b3614742c2969` |
| `controls\dev\TitleBar\TitleBar.h` | `ff669a51f2fb6be72d7e7cc5ef7dec1e9a2795ea` |
| `controls\dev\TitleBar\TitleBar.idl` | `1ee8f768affcef049c302f7723cdd5d80ad1dd5c` |
| `controls\dev\TitleBar\TitleBar.xaml` | `acd14c7c6f242d99a0467d69f701b8599d8dd9c5` |
| `controls\dev\TitleBar\TitleBarAutomationPeer.cpp` | `f3a0717c2aeb1cc056f57138876206cf920c280d` |
| `controls\dev\TitleBar\TitleBarAutomationPeer.h` | `fafa6fe28a11a74aa405e69d13ce0841ab91b1cd` |
| `controls\dev\TitleBar\TitleBarTemplateSettings.cpp` | `acd296138d7ba7a4d0a03cf3f9d51be2680e81e3` |
| `controls\dev\TitleBar\TitleBarTemplateSettings.h` | `77fef2adea98a66903fb28b1feec34aa8e48ec7d` |
| `controls\dev\TitleBar\TitleBar_themeresources.xaml` | `b22068a7909c99426a1f1811e227db4ad11baa1c` |
| `controls\dev\TitleBar\TitleBar_themeresources_performance2026.xaml` | `a093c18518d257b87bea607cdb5b6ef6310ee73d` |
| `controls\dev\TitleBar\InteractionTests\TitleBarTests.cs` | `bc2dca716306280040390a3d446e95aae93ca904` |
| `controls\dev\TitleBar\Strings\en-us\Resources.resw` | `a9dc89afc23e0c9d43f16463c76c3dab308e136b` |
| `controls\dev\TitleBar\TestUI\TitleBarPageWindow.xaml` | `bb3a6a49b91d0f35c95711d96d1b056a5cdac735` |
| `controls\dev\TitleBar\TestUI\TitleBarPageWindow.xaml.cs` | `f18ab2841ef76a47cadab67d0eef6bbd584bdb30` |

### API and behavior

The current control surface contains `Title`, `Subtitle`, `IconSource`,
`LeftHeader`, `Content`, `RightHeader`, `IsBackButtonVisible`,
`IsBackButtonEnabled`, `IsPaneToggleButtonVisible`, `TemplateSettings`,
`BackRequested`, and `PaneToggleRequested`. Public V11 APIs add
`AutoRefreshDragRegions`, nullable attached `IsDragRegion`, and
`RecomputeDragRegions`.

`OnApplyTemplate` updates height, padding, icon, header/content visibility,
button states, interactable elements, drag regions, and the icon region. A
content `LayoutUpdated` callback refreshes once by default and continuously
when `AutoRefreshDragRegions` is enabled. Interactive controls are excluded
from dragging automatically. `IsDragRegion=true` opts an element into drag;
`false` creates passthrough content. `RecomputeDragRegions` refreshes layout,
the interactable list, and the native region immediately.

The template preserves the expanded 48-DIP layout, 16-DIP icon, back and pane
button states, compact/expanded content states, high-contrast resources, and
the current classic and Performance2026 resource values.

### Accessibility

The current peer reports `AutomationControlType.TitleBar`, class name
`TitleBar`, and uses the control `Title` as its accessible name when no
explicit automation name is supplied. `WindowTitleBarControlAutomationPeer` now
provides the same contract for ModernWpf's retained WPF shell control, and
`WindowTitleBarControl.OnCreateAutomationPeer` exposes it.

## Current WinUI Gallery inventory

The current Gallery has three TitleBar examples. Exact inputs are pinned here:

| File under WinUI-Gallery | Blob |
| --- | --- |
| `WinUIGallery\Samples\TitleBar\TitleBarPage.xaml` | `25714311aaf20f8450eb6aa0f116d8ec6ac556e9` |
| `WinUIGallery\Samples\TitleBar\TitleBarPage.xaml.cs` | `af520bb8b5124280f607608bf242d8b39cd401dc` |
| `WinUIGallery\Samples\TitleBar\TitleBarPage.xaml` configuration snippet | `809fd3df59b5383279de02be9eefe76fd61fd5cc` |
| `WinUIGallery\Samples\TitleBar\TitleBarPage.xaml` drag-region snippet | `a63138f1d89beee02b4ffb8b7626e398b557e8c0` |
| `WinUIGallery\Samples\TitleBar\TitleBarPage.xaml` end-to-end snippet | `6e2fb83489d8c0df9b08758bceec24afe401c595` |
| `WinUIGallery\SampleSupport\SamplePages\TitleBarWindow.xaml` | `0ec513515ffb4e16c3ca0174b05f1783dd5c98ce` |
| `WinUIGallery\SampleSupport\SamplePages\TitleBarWindow.xaml.cs` | `77a4dbe135718139c68635cede89f2ca4745e61d` |
| `WinUIGallery\SampleSupport\SamplePages\TitleBarDragRegionsWindow.xaml` | `ec23c47c91b0c164d875c449df8246f085350aec` |
| `WinUIGallery\SampleSupport\SamplePages\TitleBarDragRegionsWindow.xaml.cs` | `e138e59bc558add94bfb98fcbc1dd094e8d67b87` |

The configuration example contains a title and subtitle, optional back and
pane buttons, an icon, a search box with `MaxWidth="580"`, stretch alignment,
and `PlaceholderText="Search..."`, plus a 30-DIP PersonPicture at the right.
The drag-region example opens a window demonstrating unset, true, and false
`IsDragRegion` states, automatic interactive-control exclusion, a dynamically
added button, and explicit `RecomputeDragRegions`. The third example opens the
complete custom-title-bar window. No later commit after `9a14fa56` changes
these paths at the current Gallery pin.

## ModernWpf mapping and platform boundary

ModernWpf predates the current WinUI control. Its WPF-specific attached-property
facade is named `ModernWpf.Controls.WindowTitleBar`, leaving `TitleBar`
available for a future port of the current WinUI control.
`ModernWpf.Controls.Primitives.WindowTitleBarControl` is the retained WPF
window-shell substitution. Its attached window state, icon handling, title
content, back/pane buttons, and WPF `WindowChrome` integration remain native
to WPF. The facade's `ExtendsContentIntoTitleBar` name follows the current
Windows app-window terminology; `CoreApplicationViewTitleBar` retains the
legacy UWP-shaped `ExtendViewIntoTitleBar` member.

The retained shell's public `WindowTitleBar.HeightKey` is a WPF-specific
customization point. A window-scoped override now drives the rendered
`WindowTitleBarControl`, read-only `WindowTitleBar.Height`, and
`WindowChrome.CaptionHeight` together, including runtime resource changes and
chrome replacement. Native `WM_NCHITTEST` coverage verifies that the region
below the former 32-DIP boundary remains draggable at a larger custom height.

`TitleBarButton` also retains normal WPF content-font inheritance. Its built-in
caption and back icons use `StreamGeometry` through `FontIconFallback`, so the
old control-wide `SymbolThemeFontFamily` setter is neither needed for those
icons nor appropriate for arbitrary content. A nested `TextBlock` now inherits
the host font unless the application explicitly gives it another font.

`ModernWpf.Gallery\Pages\WindowingSampleFactory.cs` now mirrors all three
current Gallery examples. The configuration code and visible preview use the
stretch resource, `MaxWidth="580"`, and `Search...`. The new drag-region window
adapts WinUI's `InputNonClientPointerSource` behavior to WPF: empty title-bar
space and explicitly opted-in content call `Window.DragMove`; ordinary WPF
buttons and the search box remain interactive; dynamically added controls are
recognized immediately by WPF's live visual and input trees; and the
recompute action documents that platform substitution. Runtime automation
tests open the child window and exercise the radio choices, status button,
dynamic button, and recompute status.

The installed-Gallery primary comparison intentionally remains the first
470x48 preview. With the back button, pane toggle, and left header collapsed,
the AppWindow inset is zero: the template places 14 DIPs before the 16x16 icon
and then its 16-DIP right margin. At the fixed comparison width the stretchable
search box measures 186x32, followed by the 30x30 right header. The Gallery
card owns the one-pixel rounded stroke; the WPF preview represents that parent
ownership with its existing negative one-pixel surface margin so the crop
does not alter control size or hit targets.

## Validation

| Theme | Installed Gallery / ModernWpf crop | Primary delta |
| --- | --- | --- |
| Light | `470x48` / `470x48` | `0.74` |
| Dark | `470x48` / `470x48` | `0.82` |

- Light: `artifacts/visual-checks/20260718-174511-923-56972/report.md`.
- Dark: `artifacts/visual-checks/20260718-174542-034-92644/report.md`.
- Both runs used the installed current WinUI Gallery and a freshly built
  ModernWpf Gallery; both passed the strict `1.0` primary-delta threshold and
  the exact-size threshold of `0`.
- Fresh option-interaction recordings passed by toggling
  `IsBackButtonVisible`: Light
  `artifacts/gallery-recordings/20260718-184435-809/report.md` detected a
  `6.819` local delta, and Dark
  `artifacts/gallery-recordings/20260718-184510-347/report.md` detected a
  `7.897` local delta.
- Focused product tests cover the dependency-property shell contract,
  title-bar automation peer, and this current-source audit.
- Focused Gallery tests cover the three-example order, source snippets,
  configuration preview, child-window drag-region adaptation, dynamic content,
  and visual-harness gates on `net8.0-windows7.0` and
  `net10.0-windows7.0`.
- `ModernWpf.Controls` and `ModernWpf.Gallery` remain build-checked for the
  retained `net462` target. The final Controls build completed with the
  repository's existing warnings and zero errors; Gallery completed with zero
  warnings and zero errors.

The unavailable native primitive is explicitly bounded: WPF has no direct
equivalent of WinUI's `InputNonClientPointerSource` rectangular non-client
region API. That affects implementation mechanics, not the visible preview,
accessible role, interactive exclusions, or Gallery-demonstrated outcomes.
