# NumberBox WinUI 3 Source Audit

Current product snapshot: `D:\repos\microsoft-ui-xaml`, official
`microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`.

Current Gallery snapshot: official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`.

Parity refresh: 2026-07-18.

## Current Product Source Pins

| Current source | Git blob |
| --- | --- |
| `controls\dev\Generated\NumberBox.properties.cpp` | `d63f83da318fa6665e104d029c67fe227ed69c28` |
| `controls\dev\Generated\NumberBox.properties.h` | `faefba4d6963b5bf03a679c08ecb10a28077f2ac` |
| `controls\dev\Generated\NumberBoxAutomationPeer.properties.cpp` | `91899385043eb457bb2f2c48b66b514088a52c56` |
| `controls\dev\NumberBox\NumberBox.cpp` | `4bf79c2f673991f328230e60b218df60b3cabddb` |
| `controls\dev\NumberBox\NumberBox.h` | `34086e68af28d2bf220c0787395f87b18815bc68` |
| `controls\dev\NumberBox\NumberBox.idl` | `41a8067e45121eaa0856088bbbbee77138f10816` |
| `controls\dev\NumberBox\NumberBox.xaml` | `d70025ee3655bbb03c2580470d596a31b3e07bcc` |
| `controls\dev\NumberBox\NumberBox_perf2026.xaml` | `ac318960b35b1aad35892c4b85043fd7b29cad66` |
| `controls\dev\NumberBox\NumberBox_themeresources.xaml` | `3b532d4b76b2ac9f2d06c8c0d473fa7d8f9a32b2` |
| `controls\dev\NumberBox\NumberBoxAutomationPeer.cpp` | `41e8febeaa1015777756586a05e0bef45ce21b59` |
| `controls\dev\NumberBox\NumberBoxAutomationPeer.h` | `d5cd65d6ce8e2cd48c9e92da9a96d2f962620637` |
| `controls\dev\NumberBox\NumberBoxParser.cpp` | `9158e28b1bbbb736c57d00f468b4af87b8dfdb3e` |
| `controls\dev\NumberBox\NumberBoxParser.h` | `4520885ff3473a3b4fa7f955880a0cea37864206` |
| `controls\dev\NumberBox\APITests\NumberBoxTests.cs` | `b880e1c66ff99c2b711329a382a51cf22a859ea9` |
| `controls\dev\NumberBox\InteractionTests\NumberBoxTests.cs` | `b7a63cfa89404f604300a79c679b4b395a977dcb` |
| `controls\dev\NumberBox\Strings\en-us\Resources.resw` | `4402051f8497cdabcce8f6885491177fad1c1d1f` |
| `controls\dev\NumberBox\NumberBox.vcxitems` | `1c8b8f9ded0a8f6b65c31c5d232d071f54c4e20e` |

The previous audit pinned product commit
`c70471c511a0168b61dcca13af9556465f26b673`. Rename-aware comparison to the
current snapshot shows all generated files, headers, IDL, parser, peer,
classic XAML/theme resources, API/interaction tests, and English resources as
byte-identical 100% renames after commit
`8463f45162149de0ec3ad7df752596893fe3e13e` moved the source mirror to the
repository root. `NumberBox.cpp` is a 99% rename: commit
`c7e2f98d978c81c2b7b0054eb042a6f8f816ec9c` removes a DLL dynamic initializer
by moving the unchanged whitespace characters from a file-global `wstring`
into a function-local `constexpr wchar_t[]`. ModernWpf already uses
`TextBox.Text.Trim()`, so this load-time native implementation cleanup does not
change the port's behavior.

The separately packaged perf2026 style introduced by the current performance
work retains the classic dictionary's resources, metrics, part tree, states,
and behavior. Its only changes replace discrete object animations for brush,
foreground, border, requested-theme, and visibility assignments with
`VisualState.Setters`. ModernWpf uses `VisualStateEx.Setters` for NumberBox
states and documents its WPF TextBox trigger substitute below; no product
change is justified by the equivalent perf style. Commit
`beabd047460bf5d43a41fcf8bddf7730188bd5a7` enables perf2026 build/runtime
selection, while `49b4d5326b4deba8c036e63a7e676715a5de4f3a` carries the setter-style work.

## Current Gallery Source Pins

Current commit `29f62479d5c046a0b854a5868e5a7cd484572d87` carries the samples converted
by `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` (`Convert other samples`,
2026-05-22):

| Current Gallery source | Git blob |
| --- | --- |
| `WinUIGallery\Samples\NumberBox\NumberBoxPage.xaml` | `101a73a394586a997e5211486b6b4c78ccc7c0fb` |
| `WinUIGallery\Samples\NumberBox\NumberBoxPage.xaml.cs` | `6d7100f3a5ac67ecdef3ad6ddc183e10346e060d` |
| `WinUIGallery\Samples\NumberBox\NumberboxEvaluatesExpressions.txt` | `ad12b22405f5537a69c5731b68336af45a6d835f` |
| `WinUIGallery\Samples\NumberBox\NumberboxSpinButton.txt` | `d76405d99f51fa57220753f0c8f5775c2b3f3fcb` |
| `WinUIGallery\Samples\NumberBox\FormattedNumberboxRoundsNearest.txt` | `c621e68d731f9e807f3e4e58cb35850ca90b4f69` |
| `WinUIGallery\Samples\SampleCode\NumberBox\NumberBoxSample3_cs.txt` | `83f06ada54b15cb4d7debbace7de08edc42b3b3f` |
| `WinUIGallery\Samples\SampleCode\NumberBox\NumberBoxSample3_xaml.txt` | `df38c7a189f7aac42f2ba6767aa72d541edafa43` |

The current page retains three examples:

- An expression NumberBox with header `Enter an expression:`, placeholder
  `1 + 2^2`, `Value=NaN`, and `AcceptsExpression=True`.
- `NumberBoxSpinButtonPlacementExample` with accessible name
  `NumberBox with spin button`, header `Enter an integer:`, value `10`, changes
  `10` / `100`, and an `Inline` / `Compact` RadioButtons option. The source
  snippet's substituted default is Inline even though the page XAML declares
  Compact before the selected option is applied.
- `FormattedNumberBox`, whose quarter-increment formatter rounds half-up with
  two fraction digits. The live page correctly says `Enter a dollar amount:`;
  the current source snippet still contains its historical `an dollar` typo,
  which ModernWpf preserves in the displayed source code while keeping the
  live control text correct.

ModernWpf's programmatic WPF page uses explicit widths to reproduce the live
WinUI sample measurements; the primary spin-button example resolves to the
same exact 132x59 resting crop. The factory retains the current names, headers,
options, formatter behavior, accessible name, automation IDs, and source text.

## Ported Product Behavior

- The property surface includes AcceptsExpression, Description, Header and
  HeaderTemplate, IsWrapEnabled, Minimum/Maximum, Small/LargeChange,
  NumberFormatter, PlaceholderText, SelectionBrush, SpinButtonPlacementMode,
  Text, TextAlignment, ValidationMode, Value, and ValueChanged.
- Template application unhooks prior part handlers and popup helper, discovers
  the new buttons/text box/popup, localizes spin-button names, registers live
  handlers, establishes the source popup helper, selects the current spin
  placement and enabled states, forwards UIA properties, and resolves initial
  Text-versus-Value precedence.
- Input validation trims the current source whitespace set through WPF
  `String.Trim`, supports the source parser when expressions are enabled,
  handles invalid-input overwrite/disabled modes, coerces min/max/value, and
  uses ten significant digits to suppress floating-point display artifacts.
- Up/Down and PageUp/PageDown step on key down, Enter validates, Escape restores
  formatted text, mouse wheel steps only while the inner text box is focused,
  wrapping crosses between Minimum/Maximum, and the caret returns to the end.
- Inline/compact/hidden placement routes the source states through
  `VisualStateEx`; the WPF non-FrameworkElement fallback explicitly sets the
  inner spin column to 72 DIPs for Inline and Auto otherwise.
- The peer exposes only RangeValue, reports class `NumberBox` and Spinner
  control type, derives a missing name from Header, exposes source min/max/
  value/change values, and raises RangeValue.Value property changes.
- The owner forwards explicit AutomationProperties.Name or string Header plus
  bounded range status to the inner text box and forwards LabeledBy, matching
  current source `ReevaluateForwardedUIAProperties`.

## Template and Resource Parity

- The active template retains source `InputBox`, `InputEater`, inline and popup
  repeat buttons, compact popup indicator, header/description presenters,
  three-column layout, source spin margins, and source glyphs E70E/E70D/EC8F.
- Source SpinButtonsCollapsed/Visible/Popup, up/down enabled states, and common
  enabled state own visibility, minimum width, style, and button enabledness.
- Style defaults and resources retain the 120-DIP minimum, 32-DIP inline
  buttons, 36x36 popup buttons, popup offsets, border metrics, corner radii,
  secondary foreground, acrylic/background, and High Contrast aliases.
- Compact popup shadow depth 16 uses the shared source-backed ThemeShadow
  recipe with source padding `8,4,8,12`.

## WPF Substitutions

- WPF Popup, `ThemeShadowChrome`, and `PopupRepositionHelper` represent WinUI
  popup hosting, compositor shadow/translation, and root-bounds behavior.
- WPF TextBox requires `PART_ContentHost`; the embedded input/delete-button
  template uses WPF triggers and `FontIconFallback` where WinUI uses its native
  content element, perf setters, theme request, and AccessibilityView metadata.
- Popup spin buttons add `Focusable=False` because WPF focus movement can close
  the popup; source `IsTabStop=False` is retained.
- The local popup-indicator margin and explicit ColumnDefinition width bridge
  account for WPF resource scope and non-FrameworkElement setter limitations.
- The WPF formatter/parser interfaces and `DefaultNumberRounder` replace WinRT
  globalization formatter interfaces while preserving source behavior.
- Dark inline/popup glyph foreground derives from the resolved NumberBox
  foreground; High Contrast keeps the source system resource authoritative.

## Regression Coverage

- `NumberBoxApiTests` covers style/template/resources, part tree and metrics,
  placement/enabled state setters, compact popup/shadow, Light/Dark/High
  Contrast foregrounds, propagated text properties, input scope, forwarded
  UIA Name/LabeledBy/range text, and source control defaults.
- `NumberBoxInteractionTests` covers stepping, enabled boundaries, whitespace
  trimming, significant-digit preservation, validation modes, focus-only wheel
  input, custom formatting, NaN event suppression, keyboard behavior,
  expressions, automation RangeValue, and header/description lifecycle.
- `GalleryAutomationHookTests` pins all three current examples, current live
  values/names/options, formatter behavior, accessible name, sample source,
  and Inline/Compact option reaction.
- `WpfGallerySourceShapeTests` pins real ModernWpf/WinUI identifiers, the `2.5`
  resting and `2.0` interaction gates, and zero size tolerance for both crops.
- `NumberBoxSourceAuditTests` pins current commits/blobs, product/template/peer/
  Gallery implementation shape, strict report values, and this audit.

## Live Installed-Gallery Evidence

The harness captures the immutable resting value `10`, then uses the real
RangeValue provider to move both implementations to `20.00` and captures the
separate interaction crop.

| Theme | Report | State | Crop sizes | Mean delta | Gate |
| --- | --- | --- | --- | ---: | --- |
| Light | `artifacts/visual-checks/20260718-121201-546-28252/report.md` | Resting | `132x59` / `132x59` | `1.84` | `2.5`, size `0` |
| Dark | `artifacts/visual-checks/20260718-121231-516-71104/report.md` | Resting | `132x59` / `132x59` | `1.74` | `2.5`, size `0` |
| Light | `artifacts/visual-checks/20260718-121201-546-28252/report.md` | Value `20.00` | `152x79` / `152x79` | `1.69` | `2.0`, size `0` |
| Dark | `artifacts/visual-checks/20260718-121231-516-71104/report.md` | Value `20.00` | `152x79` / `152x79` | `1.02` | `2.0`, size `0` |

The remaining non-zero pixels are WPF/WinUI text, glyph, curve, and edge
antialiasing; state, geometry, layout, resources, value behavior, and
accessibility match.

## Verification

- The refreshed NumberBox product/source slice passes 18/18 on
  `net8.0-windows7.0`.
- Focused Gallery runtime/source tests pass 2/2 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- The focused `net462` Controls build is warning-free with zero errors; the
  product/Gallery test builds also refresh the net8/net10 Controls outputs.
- Both final strict Light and Dark installed-Gallery runs pass the tightened
  resting/interaction mean and exact-size gates.

## 2026-07-21 Header-Line Rounding Follow-up

The first and third Gallery cards could render one pixel shorter depending on
WPF text-line rounding. `HeaderContentPresenter` now has `MinHeight=19`, which
matches the WinUI header line without changing headerless or wrapping behavior;
template and source-audit tests pin the value.

Final Light
`artifacts/visual-checks/numberbox-rounding-light-v1/20260721-175252-350-69552/report.md`
passes all three cards, exact `132x59` rest and `152x79` value-state crops, at
deltas `1.83` / `1.75`. Final Dark
`artifacts/visual-checks/interaction-inputs-dark-v2/20260721-180019-720-41456/report.md`
passes the same exact geometry at `1.73` / `1.09`.
