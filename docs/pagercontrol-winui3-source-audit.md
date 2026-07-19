# PagerControl WinUI 3 Source Audit

Date: 2026-07-18

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
de3e767333c2f0717a6a70cb22bd192ced5ad885
winui3/main
```

## Source Files

- `controls/dev/PagerControl/PagerControl.cpp`
- `controls/dev/PagerControl/PagerControl.h`
- `controls/dev/PagerControl/PagerControl.idl`
- `controls/dev/PagerControl/PagerControl.xaml`
- `controls/dev/PagerControl/PagerControl_themeresources.xaml`
- `controls/dev/PagerControl/PagerControl_themeresources_perf2026.xaml`
- `controls/dev/PagerControl/PagerControlTemplateSettings.cpp`
- `controls/dev/PagerControl/PagerControlTemplateSettings.h`
- `controls/dev/PagerControl/PagerControlAutomationPeer.cpp`
- `controls/dev/PagerControl/PagerControlAutomationPeer.h`
- `controls/dev/PagerControl/Strings/en-us/Resources.resw`
- `controls/dev/PagerControl/APITests/PagerControlTests.cs`
- `controls/dev/PagerControl/InteractionTests/PagerControlTests.cs`

## Current Source Identity

The current runtime, public API, automation, test, resource, and legacy theme
payloads are byte-identical to the 2026-05-17 audit. The only PagerControl tree
changes since `c70471c511a0168b61dcca13af9556465f26b673` are build packaging and a
perf2026 theme dictionary:

- `8463f45162149de0ec3ad7df752596893fe3e13e` moved the WinUI mirror from
  `src/controls/...` to `controls/...` without changing these payloads.
- `beabd047460bf5d43a41fcf8bddf7730188bd5a7` and the subsequent perf work add
  `PagerControl_themeresources_perf2026.xaml` to the built dictionaries.
- The perf2026 dictionary replaces the navigation and number-button
  `ObjectAnimationUsingKeyFrames` transitions with equivalent
  `VisualState.Setters`. ModernWpf already expresses those same state values
  through `VisualStateEx.Setters`; there is no visual or behavioral delta to
  port.

Current authoritative blob identities:

| Upstream file | Git blob |
| --- | --- |
| `PagerControl.cpp` | `80adea00055e7fad37e7e0f08770a80d831fdd11` |
| `PagerControl.h` | `de1cf3473f7b7e3db186e6fb1f4701e2b0c5654c` |
| `PagerControl.idl` | `d9f2b1d1384c43bafd0c1f7e7628f9507e0ee23e` |
| `PagerControl.xaml` | `4606b2611013238bf0f5f557a79681feba7eac3a` |
| `PagerControl_themeresources.xaml` | `33a364a33dd5ab24d2c2c3f5b7c1f660d40951cc` |
| `PagerControl_themeresources_perf2026.xaml` | `51164cb404d23f4b61b76fd9b0c8059bfb4b0f04` |
| `PagerControlTemplateSettings.cpp` | `2c2f42b400a41218d9522e6f773f735e0d902044` |
| `PagerControlAutomationPeer.cpp` | `58f344b290396459f230f5c72b330e7a9caf8dda` |
| `Strings/en-us/Resources.resw` | `5462881f8587d00fd0e98867af4464d06a807a9c` |
| `APITests/PagerControlTests.cs` | `c8cfcd319e85e90eeb9cfa72e87a7d6cc812061c` |
| `InteractionTests/PagerControlTests.cs` | `43b7e8d20e3f5e2f38956aac852cde69107a4f4b` |

## Current WinUI Gallery Coverage

The current official WinUI Gallery snapshot is
`29f62479d5c046a0b854a5868e5a7cd484572d87`. Its complete tree contains no PagerControl sample or page; the similarly named current surface is PipsPager,
which is a different control. PagerControl therefore has no truthful current
live-Gallery comparison target. This row is gated by current product source,
behavior, accessibility, template/resource regression tests, and multi-target
builds rather than by a substituted or historical Gallery page.

## ModernWpf Port

- `ModernWpf.Controls\PagerControl\PagerControl.cs`
- `ModernWpf.Controls\PagerControl\PagerControl.xaml`
- `ModernWpf.Controls\PagerControl\PagerControlTemplateSettings.cs`
- `ModernWpf.Controls\PagerControl\PagerControlAutomationPeer.cs`
- `ModernWpf.Controls\PagerControl\Strings\Resources.resx`
- `test\ModernWpf.WinUI.Tests\PagerControl\PagerControlApiTests.cs`

## Ported Source Behavior

- Deleted the old guessed `PART_RootPanel` / `PART_NumberPanel` / manual page-button map implementation. The default template now uses WinUI's `RootGrid`, `NumberBoxDisplay`, `ComboBoxDisplay`, `NumberPanelItemsRepeater`, `NumberPanelCurrentPageIndicator`, and source navigation-button part names.
- Replaced the hand-built number panel with the source `TemplateSettings.NumberPanelItems` collection. It now emits integer page buttons plus ellipsis icons through the WinUI start/end/center ellipsis algorithms.
- Replaced the old pages list with the source `TemplateSettings.Pages` collection. Finite mode sizes the combo-box entries to `NumberOfPages`, while infinite mode grows in `100` item increments.
- Ported source display-mode routing: `Auto` chooses ComboBox below the ten-page threshold and NumberBox at or above it (and in infinite mode), while explicit `NumberBox`, `ComboBox`, and `ButtonPanel` modes drive the source visual states.
- Ported source edge-button state handling for finite versus infinite page counts, first/previous/next/last visibility states, enabled states, and localized automation names.
- The navigation-button and number-panel button styles now use source-shaped button templates instead of falling back to the generic default button style. Pointer-over, pressed, and disabled chrome is driven by `VisualStateEx.Setters`; navigation glyphs use `FontIcon` with right-to-left mirroring, and page-number buttons use `ContentPresenterEx`.
- Generated number-panel buttons now resolve a template-local alias for `PagerControlNumberPanelButtonStyle`, matching the source intent of applying the pager-owned style to buttons created from code while avoiding WPF theme-dictionary lookup loss.
- Ported source `SelectedPageIndex` change ordering, including clamping to `NumberOfPages - 1`, raising selection invalidation from the automation peer, updating number-panel collections, and synchronizing NumberBox/ComboBox display values.
- Ported the source keyboard hook on `RootGrid` for left/right focus movement across pager children.
- Added the source English resource strings for prefix/suffix text and navigation-button automation names.
- Updated the automation peer to match source shape: control type `Menu`, selection pattern support, required single selection, and empty `GetSelection()` result.

## WPF Substitutions

- WPF has no native WinUI `VisualState.Setters`; the template uses `VisualStateEx.Setters`.
- WPF `Grid` lacks WinUI `CornerRadius`, `BorderBrush`, and `BorderThickness`, so the template root uses `GridEx`.
- WPF does not expose WinUI `AutomationProperties.LandmarkType` or `AutomationProperties.AccessibilityView=Raw` in this target surface, so PagerControl keeps the source automation peer shape and omits those XAML-only automation annotations.
- WinUI `RepositionThemeTransition` on the current-page indicator is not available in WPF; the indicator position is updated directly when the selected page changes.
- WinUI uses `FontIcon` for the ellipsis icon; ModernWpf uses `SymbolIcon(Symbol.More)` as the local icon substitute.
- WinUI repeater items can be primitive values and icons; the WPF port stores `object` values in the template-setting collections and lets `ItemsRepeater` host both buttons and icon elements.
- WinUI key navigation uses `FocusManager.FindNextElement`; WPF uses `MoveFocus` with left/right traversal requests from the root key handler.
- Localized resource strings are represented by the English WinUI strings until localized ModernWpf resource packs add PagerControl translations.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --framework net8.0-windows7.0 --filter FullyQualifiedName~PagerControl --no-restore -m:1`
  - Passed 14/14, including current source-identity and Auto-threshold gates.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --framework <net462|net8.0-windows7.0|net10.0-windows7.0> --no-restore -m:1`
  - Passed all three targets with zero warnings and zero errors. The modern
    targets retain the repository's informational `Failed to resolve
    WinRT.Runtime.dll.` message without a build warning or error.
- `rg -n "PART_RootPanel|PART_NumberPanel|PART_FirstButton|PART_PreviousButton|PART_NextButton|PART_LastButton|UpdateNumberPanelButtons|_pageButtonsByPageIndex|DefaultPagerControlNavigationButtonStyle" .\ModernWpf.Controls\PagerControl .\test\ModernWpf.WinUI.Tests\PagerControl`
  - No stale guessed PagerControl template parts, old number-panel rebuild path, or old navigation-button style remain in source or focused tests.
