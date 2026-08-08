# TitleBar current WinUI 3 and Gallery parity audit

Date: 2026-07-18
Updated: 2026-08-08

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml | `e1aa8f64df98d6229f6cd4074d59b654616254da` | Preview 4 `winui3/main` product, API, template, test, and resource cutoff. |
| microsoft-ui-xaml | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Latest stable `winui3/release/2.3.1` snapshot content-reconciled with the main epoch. |
| WinUI Gallery | `3669519356c67f1376152c33ed8ea45003a91f3a` | Preview 4 Gallery source and sample cutoff. |
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

### Preview 4 disposition

Main-line commit `54c81dcacb9d6e01a30da7c5299bfd4bf661d43e`
changes `AutoRefreshDragRegions`, nullable attached `IsDragRegion`, and
`RecomputeDragRegions` in `TitleBar.idl` from `MUX_PREVIEW` to
`MUX_PUBLIC_V11`. These are current public V11 APIs, not preview APIs. The
change is API-status metadata only; it does not change TitleBar runtime,
template, resources, or Gallery behavior.

Preview 4 adds the source-shaped `ModernWpf.Controls.TitleBar` control and
keeps the older WPF window-shell APIs separate. `WindowTitleBar` remains the
attached-property facade for `WindowChrome`, and
`WindowTitleBarControl` remains the shell-owned caption control. The new
`TitleBar` name is therefore not an alias or rename of either existing type.
All V11 members are public in the first preview that contains the new control.

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
| `controls\dev\TitleBar\TitleBar.idl` | `f540ca36b93e557b6b9f1221fc7c08b988ca6fd0` |
| `controls\dev\TitleBar\TitleBar.xaml` | `acd14c7c6f242d99a0467d69f701b8599d8dd9c5` |
| `controls\dev\TitleBar\TitleBarAutomationPeer.cpp` | `f3a0717c2aeb1cc056f57138876206cf920c280d` |
| `controls\dev\TitleBar\TitleBarAutomationPeer.h` | `fafa6fe28a11a74aa405e69d13ce0841ab91b1cd` |
| `controls\dev\TitleBar\TitleBarTemplateSettings.cpp` | `acd296138d7ba7a4d0a03cf3f9d51be2680e81e3` |
| `controls\dev\TitleBar\TitleBarTemplateSettings.h` | `77fef2adea98a66903fb28b1feec34aa8e48ec7d` |
| `controls\dev\TitleBar\TitleBar_themeresources.xaml` | `b22068a7909c99426a1f1811e227db4ad11baa1c` |
| `controls\dev\TitleBar\TitleBar_themeresources_perf2026.xaml` | `a093c18518d257b87bea607cdb5b6ef6310ee73d` |
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

Preview 4 records 76 source-shaped TitleBar resource entries in
`ModernWpf/PublicResourceKeys.Unshipped.txt`: 18 Light, Dark, and High Contrast
brush aliases per theme plus 22 shared dimensions, margins, and alignments.
The shared width resources drive live WPF spacer elements rather than private
`GridLength` copies, so application overrides affect the rendered template.
`TitleBarButtonBaseStyle` and the template itself remain unlisted
implementation resources under the repository's documented resource-contract
policy.

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

`ModernWpf.Controls.TitleBar`, `TitleBarTemplateSettings`, and
`TitleBarAutomationPeer` now carry the current WinUI-shaped control contract.
The template keeps the source 32/48-DIP height states, icon/title/subtitle,
left/content/right slots, separate back and pane-toggle resources, compact
layout, activation states, Light/Dark resources, and High Contrast aliases.

WPF has no `InputNonClientPointerSource`, `XamlRoot`, or WinUI rectangular
non-client region API. The adaptation classifies the live WPF visual/logical
tree at pointer time: ordinary enabled controls and the complete left/right
header regions stay clickable,
non-interactive title-bar space calls `Window.DragMove`, and the nearest
nullable `TitleBar.IsDragRegion` override wins. A double-click on a drag region
maximizes or restores the WPF `Window`. `AutoRefreshDragRegions` listens to the
content's `LayoutUpdated` event; `RecomputeDragRegions` updates layout and
invalidates the control without manufacturing native rectangles.

The back and pane-toggle buttons use the package's high-visibility WPF system
focus visual, matching WinUI's `UseSystemFocusVisuals` intent in Light, Dark,
and High Contrast. Their automation names and tooltips come from the
control-local resource table (`Back` and `Toggle Navigation` in the neutral
English resources), rather than hard-coded template text.

The control marks its inherited WPF chrome subtree as client-hit-testable.
This is required when it overlaps `WindowChrome.CaptionHeight`: buttons and
other interactive content receive normal WPF input, while the control's own
preview handler turns only classified drag space back into `DragMove` and
double-click maximize/restore behavior.

The control synchronizes a non-empty `Title` to the containing WPF
`Window.Title` for shell/UIA consistency and restores the pre-control title
when it detaches. It does not overwrite a title that the application changed
after the control's last update. The automation peer reports TitleBar,
`TitleBar`, and the control title as the fallback accessible name.

The pre-existing WPF-specific attached-property facade remains
`ModernWpf.Controls.WindowTitleBar`, and
`ModernWpf.Controls.Primitives.WindowTitleBarControl` remains the shell-owned
caption control. Their attached window state, icon handling, and
`WindowChrome` integration are not redirected through the new content control.

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

`ModernWpf.Gallery\Pages\WindowingSampleFactory.cs` mirrors all three current
Gallery examples with the real control rather than a hand-built title-bar
facsimile. The configuration preview uses the stretch resource,
`MaxWidth="580"`, and `Search...`. The drag-region window demonstrates unset,
true, and false attached values plus dynamic content and explicit recompute.
The end-to-end window uses WPF `Binding`, `WindowTitleBar` to extend content
into the existing ModernWpf chrome, and the new control for back/pane events.

## Validation and release gate

Focused product coverage verifies the complete dependency-property/event
surface, template height, button states and focus visuals, independent
back/pane resource keys, live public layout-resource overrides, window-title
synchronization, live drag classification, dynamic content, nullable attached
overrides, header passthrough, and the automation peer. Focused Gallery
coverage opens both real TitleBar windows and verifies the three-example
order, snippets, real control identity, option updates, all drag-region modes,
dynamic recomputation, pane toggling, bound back navigation, and exhaustive
visual-artifact registration.

The prior July `470x48` pixel reports describe the retired hand-built preview
and are historical evidence only. They are not accepted as Preview 4 proof.
Before the Preview 4 tag, the final clean tip must pass the complete serialized
release gate and new Light, Dark, and real OS High Contrast Gallery checks on
`net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`. Manual evidence must
exercise both title-bar buttons, interactive versus draggable content,
double-click maximize/restore, dynamic attached-property changes, activation,
keyboard focus, and window chrome. No pixel or manual result is claimed in
this audit until that final-tip evidence exists.

The unavailable native primitive is explicitly bounded: WPF has no direct
equivalent of WinUI's `InputNonClientPointerSource` rectangular non-client
region API. That affects implementation mechanics, not the public control
shape, accessible role, interactive exclusions, or Gallery-demonstrated
outcomes.
