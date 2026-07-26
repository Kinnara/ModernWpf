# AutoSuggestBox WinUI 3 Source Audit

Current product snapshot: `D:\repos\microsoft-ui-xaml`, official
`microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`.

Current Gallery snapshot: official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`.

Parity refresh: 2026-07-18.

## Current Product Source Pins

| Current source | Git blob |
| --- | --- |
| `dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.cpp` | `2b8474321ebfcadf268039d6fc4f24ea96276c7d` |
| `dxaml\xcp\dxaml\lib\AutoSuggestBox_Partial.h` | `0a40cb737425fd5fb31cc07f4fb35fb921f7fd73` |
| `dxaml\xcp\dxaml\lib\AutoSuggestBoxTextChangedEventArgs_Partial.h` | `57119bacd035f7e466ecb19d964d6c4250ccc4a9` |
| `dxaml\xcp\dxaml\lib\AutoSuggestBoxAutomationPeer_Partial.cpp` | `6e3443b581675f79c833226a41e2a531472579ec` |
| `dxaml\xcp\dxaml\lib\AutoSuggestBoxAutomationPeer_Partial.h` | `329293ba85808476ebd4bb99e7aeb0a2c93411e7` |
| `dxaml\xcp\dxaml\lib\ListViewBase_Partial_Interaction.cpp` | `7758954dccfead1b0f2ce7873c000c50390ca17f` |
| `dxaml\xcp\tools\XCPTypesAutoGen\Modules\AutoSuggestBox\AutoSuggestBox.cs` | `177fca6665b16fe29bd745768475e6f479fda394` |
| `dxaml\xcp\tools\XCPTypesAutoGen\Modules\AutoSuggestBox\AutoSuggestBoxAutomationPeer.cs` | `02b40b3877dec02600d1a4b5962901705aa9d179` |
| `controls\dev\AutoSuggestBox\AutoSuggestBox_themeresources.xaml` | `7cd762ebf3f870d238c428a8ca551d3cd311ee4a` |
| `controls\dev\AutoSuggestBox\AutoSuggestBox_themeresources_perf2026.xaml` | `39e8d87fee36da16ea8a9cb439400821ae55d703` |
| `controls\dev\AutoSuggestBox\AutoSuggestBoxHelper.cpp` | `ee0e163b3d815a149db9190d668c216c1aad64a3` |
| `controls\dev\AutoSuggestBox\AutoSuggestBoxHelper.h` | `7f03ba398ca7ee1bb25bd989bffd93741f725af6` |
| `controls\dev\AutoSuggestBox\AutoSuggestBoxHelper.idl` | `4ba18910a118263a40a65bc9e709500d962bfb1d` |
| `controls\dev\Generated\AutoSuggestBoxHelper.properties.cpp` | `8620837c661959a514350aed2551a7e265876854` |
| `controls\dev\AutoSuggestBox\APITests\AutoSuggestBoxTests.cs` | `16d9d90037e46fdf5a16c5685bd905be0d257b0c` |
| `controls\dev\AutoSuggestBox\InteractionTests\AutoSuggestBoxTests.cs` | `e6397a4aeacac6d1f3b026d50e489b7dfb5e545d` |
| `dxaml\test\native\external\controls\autosuggestbox\AutoSuggestBoxIntegrationTests.cpp` | `bb1e6fda90b12819a33eda7cdcd0afb866990513` |
| `dxaml\test\native\external\controls\autosuggestbox\AutoSuggestBoxAutomationPeerIntegrationTests.cpp` | `3b627d5f1710cdd1dad7515189aafffb273d8d3e` |
| `controls\test\MUXControlsTestApp\verification\AutoSuggestBox.xml` | `a70b11545d05af6883c6076204de3b26470e8c05` |
| `controls\dev\AutoSuggestBox\AutoSuggestBox.vcxitems` | `5775d9306b8235be6b323d994388caea52cb0aec` |

The previous audit pinned product commit
`c70471c511a0168b61dcca13af9556465f26b673`. Rename-aware comparison to the
current snapshot shows every classic runtime/event-args/peer/list-interaction,
XamlOM, helper, theme, API/interaction/native-test, and verification source as
a byte-identical 100% rename. Commit
`8463f45162149de0ec3ad7df752596893fe3e13e` only moves the mirror to the
repository-root layout.

The new perf2026 dictionary preserves resources, metrics, part tree, visual
states, popup shape, and behavior. It replaces discrete object animations for
button/text-box brush, foreground, border, requested-theme, and visibility
assignments with `VisualState.Setters`. ModernWpf already uses
`VisualStateEx.Setters` for its AutoSuggestBox query-button state assignments
and documents the shared WPF TextBox trigger substitute below, so the
equivalent performance style does not justify a product change. Commit
`beabd047460bf5d43a41fcf8bddf7730188bd5a7` enables perf2026 build/runtime
selection, while `49b4d5326b4deba8c036e63a7e676715a5de4f3a` carries the setter conversion.

## Current Gallery Source Pins

Current commit `29f62479d5c046a0b854a5868e5a7cd484572d87` carries the page converted by
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` (`Convert other samples`,
2026-05-22):

| Current Gallery source | Git blob |
| --- | --- |
| `WinUIGallery\Samples\AutoSuggestBox\AutoSuggestBoxPage.xaml` | `0302c81d8dacc625f2cf88549f11149eaa65e5c6` |
| `WinUIGallery\Samples\AutoSuggestBox\AutoSuggestBoxPage.xaml.cs` | `e1b6d89711b84f3bbad36581c519f856a8144cf5` |
| `WinUIGallery\Samples\AutoSuggestBox\BasicAutosuggestBox.txt` | `35eb630681ab9dff5987d281a593f64e3ef45c3b` |
| `WinUIGallery\Samples\AutoSuggestBox\AutosuggestboxProvidesSearchboxExperience.txt` | `a3049872eba26e7ebac51528bb4fa2375e94e882` |

The current page retains two examples:

- `Control1` is a 300-DIP basic AutoSuggestBox with accessible name
  `Basic AutoSuggestBox`. User-input text filters the current cat-breed list by
  every space-separated token, returns `No results found` when empty, and
  writes a chosen item to `SuggestionOutput`.
- `Control2` is a 300-DIP search-box experience with placeholder
  `Type a control name`, Find query icon, current Gallery-catalog search,
  suggestion text completion, query submission, and a collapsed details area
  containing `ControlImage`, `ControlTitle`, and `ControlSubtitle`.
- The `.txt` files store current headers plus XAML/C# sections. ModernWpf keeps
  those same headers/source strings and reproduces the current two runtime
  examples through its WPF Gallery factory.

## Ported Product Behavior

- The property/event surface includes AutoMaximizeSuggestionArea,
  HeaderPlacement, LightDismissOverlayMode, IsSuggestionListOpen,
  MaxSuggestionListHeight, QueryIcon, Text/TextBoxStyle, TextMemberPath,
  UpdateTextOnSelect, ItemsSource, QuerySubmitted, SuggestionChosen, and
  source-reasoned TextChanged.
- Template application revokes old part handlers and popup helper, discovers
  the inner TextBox/popup/list/container, registers source-shaped handlers,
  applies the current text and popup state, and updates popup/text-box corners.
- Delayed text-change args capture the source event counter; `CheckCurrent()`
  compares that counter to the owner's current counter rather than comparing a
  stale string. User input stores `m_userTypedText`, opens suggestions, and
  clears stale selection under the source ignore-selection guard.
- Up/Down previews suggestions while retaining/restoring user text; Tab,
  Escape, Enter, query-button Invoke, suggestion choice, and programmatic submit
  preserve source ordering and chosen-suggestion semantics.
- Suggestion selection applies the default selected-item text before raising
  `SuggestionChosen`, so an event handler can replace `Text` with an
  application-specific value without the control overwriting it afterward.
- Suggestion item input raises ItemClick first and then applies primary or
  secondary selection behavior across WPF Single, Multiple, and Extended
  modes, matching the relevant ListViewBase source ordering.
- The automation peer exposes only Invoke, reports class `AutoSuggestBox` and
  Group control type, and routes Invoke through `ProgrammaticSubmitQuery`.
- The suggestion popup applies source depth-32 elevation to the popup child,
  uses medium windowed-popup insets, and dynamically filters popup/text-box
  corners through the current helper contract.

## Template and Resource Parity

- The active template retains `TextBox`, `SuggestionsPopup`,
  `SuggestionsContainer`, `SuggestionsList`, query/delete-button presenter
  shape, current popup sizing, list padding/border/background, and focus/input
  bindings.
- Shared styles/resources retain source top-header and inner-button margins,
  delete/query metrics, icon size, popup foreground/background/border, overlay
  brush, TextBox chrome, and High Contrast mappings.
- Query-button common states use `ContentPresenterEx` and
  `VisualStateEx.Setters`; tests pin the source state targets and resource
  assignments.

## WPF Substitutions

- WPF Popup, `ThemeShadowChrome`, and `PopupRepositionHelper` represent WinUI
  popup root placement, compositor elevation, translation, and root-bounds
  behavior. WinUI InputPane/SIP candidate bounds, LayoutTransitionElement,
  overlay infrastructure, and `ShouldConstrainToRootBounds=false` plumbing have
  no direct control-owned WPF equivalent.
- WinUI reverses item source when the popup opens above the text box; WPF popup
  placement does not expose the same root-bounds model, so item ordering stays
  normal.
- Input-validation context/command and IME candidate positioning are not
  available on WPF TextBox. The shared inner TextBox also retains WPF triggers
  and `FontIconFallback` in place of native perf setters/AnimatedIcon paths.
- HeaderPlacement is exposed for API parity, while the shared WPF text-control
  header helper remains top-only pending a broader text-control port.
- WPF mouse buttons/modifiers and `ListView.SelectedItems` represent WinUI
  primary/secondary gestures and SelectionModel storage.

## Regression Coverage

- `AutoSuggestBoxApiTests` covers source defaults/resources/template parts,
  delayed counter semantics, automation Invoke and no-chosen-suggestion query,
  ItemClick-before-selection ordering, WPF selection modes, query-button state
  setters, popup shadow/insets, and corner filtering.
- `AutoSuggestBoxInteractionTests` covers choosing a suggestion by keyboard,
  preserving handler-assigned text after `SuggestionChosen`, Escape
  restoration, and query-button submission behavior.
- `GalleryAutomationHookTests` pins the two current examples, headers/source,
  names/IDs, cat filtering and chosen output, search suggestions, query/details
  behavior, and current catalog integration.
- `WpfGallerySourceShapeTests` pins real ModernWpf/WinUI identifiers, real
  keyboard input before UIA fallback, expected `Aegean` suggestion/output, the
  strict `0.1` static gate, and explicit zero size tolerance.
- `AutoSuggestBoxSourceAuditTests` pins current commits/blobs, product/template/
  peer/Gallery implementation shape, strict report values, and this audit.

## Live Installed-Gallery Evidence

| Theme | Report | State | Crop sizes | Mean delta | Gate |
| --- | --- | --- | --- | ---: | --- |
| Light | `artifacts/visual-checks/20260718-122824-866-56568/report.md` | Resting | `300x32` / `300x32` | `0.01` | `0.1`, size `0` |
| Dark | `artifacts/visual-checks/20260718-122854-442-15432/report.md` | Resting | `300x32` / `300x32` | `0.01` | `0.1`, size `0` |

Fresh interaction diagnostic
`artifacts/visual-checks/20260718-122944-036-93464/report.json` shows the
ModernWpf side accepting real-keyboard `ae`, exposing and invoking `Aegean`,
capturing a nonblank 300x50 popup HWND, and updating the output to `Aegean`.
The same run confirms the current installed Gallery accepts the text but still
does not expose its sample suggestion popup; therefore the overall diagnostic
is intentionally unavailable/failed as a cross-app interaction comparison and
is not presented as a passing visual gate. Deterministic source behavior is
instead enforced by the product and Gallery runtime tests above.

The remaining `0.01` resting pixels are sparse WPF/WinUI edge quantization;
state, geometry, layout, resources, and static accessibility match.

## Verification

- The refreshed AutoSuggestBox product/source slice passes 13/13 on
  `net8.0-windows7.0`.
- Focused Gallery runtime/source/interaction-shape tests pass 4/4 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- The focused `net462` Controls build is warning-free with zero errors; the
  product/Gallery test builds also refresh the net8/net10 Controls outputs.
- Both final strict Light and Dark installed-Gallery resting runs pass the
  `0.1` gate with exact crop sizes.
