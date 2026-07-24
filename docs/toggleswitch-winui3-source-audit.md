# ToggleSwitch WinUI 3 Source Audit

Current product snapshot: `D:\repos\microsoft-ui-xaml`, official
`microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`.

Current Gallery snapshot: official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`.

Parity refresh: 2026-07-18.

## Current Product Source Pins

| Current source | Git blob |
| --- | --- |
| `dxaml\xcp\dxaml\lib\ToggleSwitch_Partial.cpp` | `182d9d9af8547bab338f0e3375ccd50b7871b7ec` |
| `dxaml\xcp\dxaml\lib\ToggleSwitch_Partial.h` | `9c94df84b0f2420c646ae08e49e5c4c6d802be45` |
| `dxaml\xcp\dxaml\lib\ToggleSwitchAutomationPeer_Partial.cpp` | `653b96141170459f5ec43ca43999c89b0be23972` |
| `dxaml\xcp\dxaml\lib\ToggleSwitchAutomationPeer_Partial.h` | `d61500f69d777d9c092eee4cc76a698e6059ec42` |
| `dxaml\xcp\components\controls\KeyDownUp\inc\ToggleSwitchKeyProcess.h` | `8363db47b29b8747da03756fe98d39ab5b1bc170` |
| `dxaml\xcp\components\controls\KeyDownUp\unittests\toggleswitch\ToggleSwitchUnitTests.cpp` | `43d0aabfd55d046161a7d8e82c79983dd22564d1` |
| `dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs` | `ad1199a7ff9c253e38c4fb922accbe0afffbf432` |
| `dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.Primitives.cs` | `aa7fcdf28f0464bccbc47b85257cf3919983d52f` |
| `controls\dev\CommonStyles\ToggleSwitch_themeresources.xaml` | `50f290fbe053170d943f390d78bb4f8d6ba48140` |
| `controls\dev\CommonStyles\ToggleSwitch_themeresources_perf2026.xaml` | `09daa82259a14070ea12d243ae0080e9e7c3d9da` |
| `dxaml\test\native\external\controls\toggleswitch\ToggleSwitchIntegrationTests.cpp` | `5c5346bb857e34ecb584f9fd225d9639a8d61a9e` |
| `dxaml\test\native\external\controls\toggleswitch\ToggleSwitchAutomationIntegrationTests.cpp` | `f5ce988ad3009837ba15fa23bc30657e835d57f9` |
| `controls\test\MUXControlsTestApp\verification\ToggleSwitch.xml` | `d65e7b84f4e1eee952dbe848108bd16e0ee76452` |

The previous audit pinned product commit
`c70471c511a0168b61dcca13af9556465f26b673`. Rename-aware comparison to the
current snapshot shows the runtime partials, peer, key processor, unit/native
tests, and classic CommonStyles dictionary as byte-identical 100% renames.
Commit `8463f45162149de0ec3ad7df752596893fe3e13e` moves the source mirror from
`src\...` to the current repository-root layout and introduces the separately
named perf2026 dictionary into this mirror. Commit
`beabd047460bf5d43a41fcf8bddf7730188bd5a7` enables build/runtime consumption
of perf2026 dictionaries.

The perf2026 ToggleSwitch dictionary is an equivalent implementation variant,
not a new visual contract. It retains the same theme resources, 40x20 track,
12/14/17x14 knob sizes, margins, presenters, state names, transition timing,
translation endpoints, and part tree. It replaces discrete brush
`ObjectAnimationUsingKeyFrames` entries with `VisualState.Setters` and keeps
the color, size, opacity, and reposition animations. ModernWpf already uses
`VisualStateEx.Setters` for those brush/foreground assignments while preserving
the WPF-feasible animations, so no product template change is justified.

## Current Gallery Source Pins

Current commit `29f62479d5c046a0b854a5868e5a7cd484572d87` carries the samples converted
by `14a4a1a2` (`Convert other samples`, 2026-05-22):

| Current Gallery source | Git blob |
| --- | --- |
| `WinUIGallery\Samples\ToggleSwitch\ToggleSwitchPage.xaml` | `929eeb0566891197877a9684d5a2a97ced390868` |
| `WinUIGallery\Samples\ToggleSwitch\ToggleSwitchPage.xaml.cs` | `1de3beec353704e20f6df7a85ad1c5a4a961b537` |
| `WinUIGallery\Samples\ToggleSwitch\ToggleSwitchSimple.txt` | `d9505228d70ab1f946e1696908d73742a8ca6b6c` |
| `WinUIGallery\Samples\ToggleSwitch\ToggleSwitchCustom.txt` | `d7af70b72b67b49a9dbd9b151cd0610a8c05d73b` |
| `tests\WinUIGallery.UITests\Tests\ToggleSwitch.cs` | `1f84b100bce89d1caa528d4fed4341bf5a393d8d` |

The current page retains two examples:

- `Example1` is a default Off switch with accessible name
  `simple ToggleSwitch`.
- The second example is horizontal: `ToggleSwitch2` starts On, has header
  `Toggle work`, Off/On content `Do work` / `Working`, and drives a 32-DIP
  ProgressRing. The substitution text emits `IsOn="True"` or `False`.
- Current Gallery UI tests click `ToggleSwitch2` in both directions, require it
  displayed/enabled, require Button automation type, and require accessible
  text `Toggle work Working` in the On state.
- Each current `.txt` stores a header and XAML section. ModernWpf keeps those
  headers in `GalleryExample.HeaderText` and the same XAML strings in the
  factory, which is the equivalent WPF Gallery storage contract.

ModernWpf gives the default example an explicit 72-DIP width and zero MinWidth.
This is a documented sample-measurement adapter: the unqualified current WinUI
control resolves to the same 72x40 live extent, while WPF otherwise retains the
source style's 154-DIP minimum intended for default On/Off content.

## Ported Product Behavior

- The property surface includes IsOn, Header/HeaderTemplate,
  OnContent/OnContentTemplate, OffContent/OffContentTemplate, HeaderPlacement,
  read-only TemplateSettings, Toggled, and protected property/toggle hooks.
- Template application revokes old drag/tap/size handlers, discovers current
  parts and their live render transforms, clones frozen WPF transforms before
  mutation, registers current handlers, updates header visibility and
  translations, and enters current visual states.
- Common, focus, content, toggle, dragging, and header states follow the source
  state fields. Disabling or collapsing clears transient drag/pointer state.
- Thumb dragging and WPF horizontal manipulation share source GetTranslations,
  MoveDelta, MoveCompleted, and half-range decisions. Horizontal motion toggles;
  vertical-only manipulation does not. A bubbling tap bridge runs after drag
  cleanup and avoids post-drag double toggles.
- The private WPF `ToggleSwitchKeyProcess` ports source key down/up sequencing
  and OriginalKey normalization. Space toggles; directional keys remain
  non-toggle in both flow directions. GamepadA has no WPF Key equivalent.
- The peer exposes only Toggle, reports class `ToggleSwitch`, Button control
  type, the localized ToggleSwitch type string, a clickable point derived from
  the live thumb, and no template children. Explicit AutomationProperties.Name
  wins; otherwise Header plus custom current On/Off content forms the name,
  while default On/Off values are intentionally excluded.
- IsOn changes raise Toggled, protected callbacks, state changes, and—only when
  a property-changed listener exists—a get-or-create peer ToggleState event.
  The provider rejects disabled toggles and routes enabled toggles through
  `AutomationToggleSwitchOnToggle`.

## Template and Resource Parity

- The active template uses the source visual-state-hosting root Grid plus an
  inert WPF `BorderEx` chrome layer for WinUI Grid Background/Border/CornerRadius.
- Style defaults retain source foreground/alignment/font/focus/corner settings,
  40x20 switch geometry, 12-DIP knob, 10-DIP content margins, 4-DIP top-header
  margin, source focus margin, state resources, and High Contrast mappings.
- WPF Control metadata defaults VerticalContentAlignment to Top; WinUI Control
  metadata centers it. The explicit WPF `VerticalContentAlignment=Center`
  setter is the measured platform-default substitution that aligns current
  Off/On content.
- WinUI SwitchAreaGrid is a Grid/Panel; WPF uses Border for rounded chrome, so
  color animations target `Border.Background`. `SwitchKnobOn` uses `BorderEx`
  to represent WinUI `BackgroundSizing=OuterBorderEdge`.
- Legacy ToggleSwitchCurtain, Thumb, Track, foreground, header, and outer-border
  resources remain published across Light, Dark, and High Contrast.

## WPF Substitutions

- GamepadA, WinUI element sounds, RepositionThemeAnimation/compositor internals,
  and raw WinRT automation internals have no direct WPF equivalents.
- WPF manipulation events plus Thumb dragging represent WinUI
  `ManipulationMode="System,TranslateX"`; parent ScrollViewer arbitration
  remains platform-owned.
- Translation transitions use WPF spline/opacity animations with the same
  source endpoints and timings in place of RepositionThemeAnimation.
- AccessibilityView Raw is represented by peer child filtering.
- ContentPresenterEx, BorderEx, brush-to-color proxies, cloned Freezables, and
  the explicit centered-content setter are scoped WPF platform adapters.

## Regression Coverage

- `ToggleSwitchApiTests` covers live-tree lifecycle, tap/drag/manipulation,
  threshold and cancellation, pointer capture, key processing, flow direction,
  state resets, callbacks, property defaults, template settings/translations,
  part discovery, all state groups/setters/animations, 40x20/knob geometry,
  root chrome, centered content, resources/High Contrast, automation naming,
  class/type/localized type/click point/children, Toggle, disabled rejection,
  and listener-first peer creation.
- `GalleryAutomationHookTests` pins the current two examples, headers/snippets,
  default/local values, accessible names, 72-DIP measurement adapter, custom
  On content, and ProgressRing reaction when the switch turns Off.
- `WpfGallerySourceShapeTests` pins real ModernWpf/WinUI IDs, immutable resting
  artifacts before interaction, the `1.5` static gate, On-state comparison,
  and explicit zero size tolerance.
- `ToggleSwitchSourceAuditTests` pins current commits/blobs, product/template/
  peer/Gallery implementation shape, strict report values, and this audit.

## Live Installed-Gallery Evidence

The harness compares the immutable resting Off state before invoking the real
Toggle provider, then separately compares the resulting On state.

| Theme | Report | State | Crop sizes | Mean delta | Gate |
| --- | --- | --- | --- | ---: | --- |
| Light | `artifacts/visual-checks/20260718-115333-533-40948/report.md` | Off | `72x40` / `72x40` | `0.92` | `1.5`, size `0` |
| Dark | `artifacts/visual-checks/20260718-115425-504-74736/report.md` | Off | `72x40` / `72x40` | `1.06` | `1.5`, size `0` |
| Light | `artifacts/visual-checks/20260718-115333-533-40948/report.md` | On | `92x60` / `92x60` | `0.53` | interaction gate, exact size |
| Dark | `artifacts/visual-checks/20260718-115425-504-74736/report.md` | On | `92x60` / `92x60` | `0.48` | interaction gate, exact size |

The remaining non-zero pixels are WPF/WinUI text, curve, and edge
antialiasing; state, geometry, alignment, resources, and interaction match.

## Verification

- The refreshed ToggleSwitch product/source slice passes 55/55 on
  `net8.0-windows7.0`.
- Focused Gallery runtime/source tests pass 2/2 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- The focused `net462` Controls build is warning-free with zero errors; the
  product/Gallery test builds also refresh the net8/net10 Controls outputs.
- Both strict Light and Dark installed-Gallery runs pass with exact Off and On
  crop sizes.
