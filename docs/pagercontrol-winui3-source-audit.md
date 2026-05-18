# PagerControl WinUI 3 Source Audit

Date: 2026-05-17

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
c70471c511a0168b61dcca13af9556465f26b673
reference/winui3-current
```

## Source Files

- `src\controls\dev\PagerControl\PagerControl.cpp`
- `src\controls\dev\PagerControl\PagerControl.h`
- `src\controls\dev\PagerControl\PagerControl.idl`
- `src\controls\dev\PagerControl\PagerControl.xaml`
- `src\controls\dev\PagerControl\PagerControl_themeresources.xaml`
- `src\controls\dev\PagerControl\PagerControlTemplateSettings.cpp`
- `src\controls\dev\PagerControl\PagerControlTemplateSettings.h`
- `src\controls\dev\PagerControl\PagerControlAutomationPeer.cpp`
- `src\controls\dev\PagerControl\PagerControlAutomationPeer.h`
- `src\controls\dev\PagerControl\Strings\en-us\Resources.resw`
- `src\controls\dev\PagerControl\APITests\PagerControlTests.cs`
- `src\controls\dev\PagerControl\InteractionTests\PagerControlTests.cs`

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
- Ported source display-mode routing: `Auto` chooses NumberBox below the ten-page threshold and ComboBox at or above it, while explicit `NumberBox`, `ComboBox`, and `ButtonPanel` modes drive the source visual states.
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

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter FullyQualifiedName~PagerControl --no-restore`
  - Passed 12/12.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed.
- `rg -n "PART_RootPanel|PART_NumberPanel|PART_FirstButton|PART_PreviousButton|PART_NextButton|PART_LastButton|UpdateNumberPanelButtons|_pageButtonsByPageIndex|DefaultPagerControlNavigationButtonStyle" .\ModernWpf.Controls\PagerControl .\test\ModernWpf.WinUI.Tests\PagerControl`
  - No stale guessed PagerControl template parts, old number-panel rebuild path, or old navigation-button style remain in source or focused tests.
