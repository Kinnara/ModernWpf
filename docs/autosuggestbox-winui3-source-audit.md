# AutoSuggestBox WinUI 3 Source Audit

ModernWpf `AutoSuggestBox` is now treated as a source-backed WPF port of the local WinUI 3 implementation rather than the old compact WPF-written behavior.

## WinUI 3 Source

Local source snapshot:

```text
D:\repos\microsoft-ui-xaml
```

Mapped source files:

- `src\dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.h`
- `src\dxaml\xcp\dxaml\lib\AutoSuggestBoxTextChangedEventArgs_Partial.h`
- `src\dxaml\xcp\dxaml\lib\AutoSuggestBoxAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ListViewBase_Partial_Interaction.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\Modules\AutoSuggestBox\AutoSuggestBox.cs`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\Modules\AutoSuggestBox\AutoSuggestBoxAutomationPeer.cs`
- `src\controls\dev\AutoSuggestBox\AutoSuggestBox_themeresources.xaml`
- `src\controls\dev\AutoSuggestBox\APITests\AutoSuggestBoxTests.cs`
- `src\controls\dev\AutoSuggestBox\InteractionTests\AutoSuggestBoxTests.cs`

## Ported Shape

- Added the source XamlOM property surface that existed in WinUI 3 but not in the old ModernWpf control: `AutoMaximizeSuggestionArea`, `LightDismissOverlayMode`, and `HeaderPlacement`.
- Replaced the old text-change bookkeeping with WinUI's source-shaped text-change counter. `AutoSuggestBoxTextChangedEventArgs.CheckCurrent()` now compares the event counter captured when the delayed event was created, matching `AutoSuggestBoxTextChangedEventArgs_Partial.h`, instead of comparing a captured string value.
- Reworked text-change flow toward `AutoSuggestBox_Partial.cpp`: the `Text` property updates the inner `TextBox` through a programmatic-change path, inner `TextBox.TextChanged` creates the delayed event args, user input stores `m_userTypedText`, user input opens the suggestion list, and text changes clear stale suggestion selection through the source ignore-selection guard.
- Replaced the old `m_searchText` restoration path with source `m_userTypedText` behavior for Up/Down, Tab, Escape, and suggestion-list keyboard handling.
- Reworked query submission toward source `ProgrammaticSubmitQuery` / `SubmitQuery`: query-button and automation invoke clear the current suggestion selection and submit only the text-box text; suggestion item click is deferred so `SuggestionChosen` and text updates can happen before `QuerySubmitted`.
- Deleted the old suggestion-list `SelectionMode.Single`-only guess that threw for every other WPF selection mode. `AutoSuggestBoxListView` now follows WinUI `ListViewBase` item-interaction ordering: raise `ItemClick` first, then apply primary/secondary selection behavior for single, multiple, and extended selection.
- Added `AutoSuggestBoxAutomationPeer` with source class name, `AutomationControlType.Group`, and invoke-pattern routing to `ProgrammaticSubmitQuery`.
- The suggestions popup maps WinUI's `ApplyElevationEffect(m_tpPopupPart.AsOrNull<IUIElement>().Get())` popup-child elevation path to a WPF `ThemeShadowChrome` wrapping `SuggestionsContainer`, using source depth `32`, `WindowedPopupInsetMode=Medium`, and a corner-radius binding to the popup surface.
- Kept the WinUI CommonStyles-derived `AutoSuggestBoxTextBoxStyle`, query-button `ContentPresenterEx` state setter shape, source theme resources, and corner-radius popup/textbox update behavior already present in the WPF port.

## WPF Substitutions

- WinUI's `InputPane`, SIP candidate-window bounds, `LayoutTransitionElement`, full-window light-dismiss overlay, popup-root placement, and `ShouldConstrainToRootBounds=false` plumbing have no direct WPF equivalent in this control. ModernWpf keeps WPF `Popup` placement plus `PopupRepositionHelper` and documents `LightDismissOverlayMode` as app-visible source API surface without WinUI overlay infrastructure.
- WinUI reverses the suggestion item source when the popup is placed above the text box. ModernWpf continues to use WPF popup placement and normal item ordering because WPF does not expose the same root-bounds placement model in this control path.
- WinUI forwards validation context/command and IME candidate-window positioning to the inner `TextBox`. WPF has no matching ModernWpf-owned input-validation or candidate-window API here.
- WinUI's `HeaderPlacement` is exposed for source API parity. The WPF `TextBox` header helper used by `AutoSuggestBoxTextBoxStyle` remains top-header only until a broader WPF text-control header-placement port is done.
- WinUI's primary/secondary item gestures are mapped onto WPF `MouseButton` input, `Keyboard.Modifiers`, and `ListView.SelectedItems`. WPF has no `ListViewBase`-owned `SelectionModel`, so range selection updates the WPF selected-item collection directly when item containers are not realized.

## Test Coverage

- `AutoSuggestBoxApiTests.VerifyAutoSuggestBoxDefaultStyleAndWinUI2Resources` covers source defaults/resources including the newly exposed source properties.
- `AutoSuggestBoxApiTests.TextChangedArgsUseSourceCounterSemantics` covers the delayed event counter and `CheckCurrent()` behavior.
- `AutoSuggestBoxApiTests.AutomationPeerInvokesProgrammaticSubmitQuery` covers source automation peer class/control type/invoke routing and no-chosen-suggestion query-button semantics.
- `AutoSuggestBoxApiTests.SuggestionListItemClickUsesWinUISourceEventBeforeSelectionOrder` and `SuggestionListPrimarySelectionUsesWinUISourceSelectionModes` cover source `ItemClick` ordering and primary selection behavior for WPF multiple/extended modes.
- `AutoSuggestBoxApiTests.SuggestionsPopupUsesSourceThemeShadow` covers the source popup-child shadow target, depth `32`, medium windowed-popup insets, and the `SuggestionsContainer.CornerRadius` binding.
- Existing tests continue covering query-button `ContentPresenterEx` state setters, corner-radius popup/textbox filtering, and suggestion selection/query behavior.
