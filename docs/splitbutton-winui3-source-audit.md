# SplitButton / ToggleSplitButton WinUI 3 Source Audit

Current product snapshot: `D:\repos\microsoft-ui-xaml`, official
`microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`.

Current Gallery snapshot: official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`.

Parity refresh: 2026-07-18.

## Current Product Source Pins

| Current source | Git blob |
| --- | --- |
| `controls\dev\SplitButton\SplitButton.cpp` | `c7731506ca9aa201e5c933034c72dad883743650` |
| `controls\dev\SplitButton\SplitButton.h` | `8f9a2e5265ad470f7dff66f898678ddb2051cbee` |
| `controls\dev\SplitButton\SplitButton.idl` | `ae96ca1974fd23b80ac85947fb13cd62d93972dc` |
| `controls\dev\SplitButton\SplitButton.xaml` | `0be293a87cd5d944267906e159fbcc4d1d85911b` |
| `controls\dev\SplitButton\SplitButton_themeresources.xaml` | `e9601d1f29cd59aa5fe7b7f0ebb49ef9c836ac19` |
| `controls\dev\SplitButton\SplitButtonAutomationPeer.cpp` | `001d9a515e58cf123bad59ad91a94c322407a222` |
| `controls\dev\SplitButton\SplitButtonAutomationPeer.h` | `36242354ccb40292115153d19b438ffea3c00d03` |
| `controls\dev\SplitButton\ToggleSplitButton.cpp` | `a6612c92349e2eae8b1bf938bb8f3d7f9dcf4d22` |
| `controls\dev\SplitButton\ToggleSplitButton.h` | `d4c255959e6e0a9f85f20485057c979543758344` |
| `controls\dev\SplitButton\ToggleSplitButtonAutomationPeer.cpp` | `a4c5ace66cb8f7245e35c4462ffbd473fdbc6d43` |
| `controls\dev\SplitButton\ToggleSplitButtonAutomationPeer.h` | `45568f6fad82300319694a98b6f06b541c1dedb5` |
| `controls\dev\Generated\SplitButton.properties.cpp` | `994e5ec0a4b495d6ddf865a6a5a4f661be3facd1` |
| `controls\dev\Generated\ToggleSplitButton.properties.cpp` | `310135a3734eda7c1c102f0b71f368befca4261c` |
| `controls\dev\SplitButton\APITests\SplitButtonTests.cs` | `fb3a0abdd1e507e00899ac258dfe613fc8614bf2` |
| `controls\dev\SplitButton\InteractionTests\SplitButtonTests.cs` | `56a69700ec9ca2c0276a1fb1e529d93077b183d0` |
| `controls\dev\SplitButton\Strings\en-us\Resources.resw` | `e242fb3aea51cb61c0c4ad5ec5e3597befa3fc23` |
| `controls\dev\SplitButton\SplitButton.vcxitems` | `34ac2a620bd737b131ee553749124b9507c7a0a4` |

The previous audit pinned product commit
`c70471c511a0168b61dcca13af9556465f26b673`. A full rename-aware diff from
that snapshot to the current one shows every runtime, generated, template,
theme, peer, API-test, interaction-test, and resource blob as a 100% rename.
Commit `8463f45162149de0ec3ad7df752596893fe3e13e` moves the source mirror from
`src\controls\...` to `controls\...`. Commit
`beabd047460bf5d43a41fcf8bddf7730188bd5a7` adds six perf2026 packaging lines
to `SplitButton.vcxitems`; it does not change the shipped control dictionaries.

## Current Gallery Source Pins

Current commit `29f62479d5c046a0b854a5868e5a7cd484572d87` carries the samples converted
by `14a4a1a2` (`Convert other samples`, 2026-05-22):

| Current Gallery source | Git blob |
| --- | --- |
| `WinUIGallery\Samples\SplitButton\SplitButtonPage.xaml` | `8ae8e1f4998170efc977ee0b4af6a9064d9fc135` |
| `WinUIGallery\Samples\SplitButton\SplitButtonPage.xaml.cs` | `49646a64699a8a9c364771fb2109452941a90d64` |
| `WinUIGallery\Samples\SplitButton\SplitButtonColorPicker.txt` | `5d50f69123b88ea2f0aacd36923cf74fe76e6968` |
| `WinUIGallery\Samples\SplitButton\SplitButtonText.txt` | `4587fbf75d39d7d383a4e080908580c5e0f675d2` |
| `WinUIGallery\Samples\ToggleSplitButton\ToggleSplitButtonPage.xaml` | `99f1cd62ff09071bf55444d9a68c0bd287e51ae9` |
| `WinUIGallery\Samples\ToggleSplitButton\ToggleSplitButtonPage.xaml.cs` | `c6acb55c04b480265fb246ee6766d09d7aa8b7e0` |
| `WinUIGallery\Samples\ToggleSplitButton\ToggleSplitButtonBulletList.txt` | `2acbcfa36ca1ca1b4ae0f871b4a37f2738d79be6` |

The current pages retain the same visible sample contract:

- SplitButton example 1 is the zero-minimum, zero-padding, top-aligned
  `myColorButton` named `Font color`, with a 32x32 green swatch, an eight-color
  flyout, a 24-DIP gap, and a 240-DIP RichEditBox option.
- SplitButton example 2 is the left-aligned `Choose color` button with 5-DIP
  padding, accessible name `Font color with text`, and a nine-color flyout that
  adds Black.
- ToggleSplitButton has the top-aligned `myListButton`, `List` SymbolIcon,
  accessible name `Bullets`, two marker choices, and a 240-DIP text editor named
  `Text entry`. Selecting Roman numbering changes the icon/name, checks the
  control, applies uppercase-Roman list formatting, hides the flyout, and
  returns focus to the editor. Unchecking removes list formatting.
- Each current `.txt` file separates its header from its XAML. ModernWpf keeps
  the header in `GalleryExample.HeaderText` and the same XAML in its established
  sample-code asset, which is the WPF Gallery's equivalent storage contract.

## Ported Product Behavior

- `OnApplyTemplate` unregisters stale primary/secondary handlers, obtains the
  current `PrimaryButton` and `SecondaryButton`, registers click/pointer hooks,
  refreshes flyout revokers, updates states, and only then marks the control
  loaded.
- `Flyout` changes revoke the old flyout's opened/closed/placement handlers and
  register the new instance. `OpenFlyout` uses
  `FlyoutShowOptions.Placement = BottomEdgeAlignedLeft` exactly like source.
- Common, checked, disabled, flyout-open, touch/key, primary, secondary, and
  secondary-placement states follow the source state ordering. The default and
  CommandBar templates use setter-owned source state targets rather than the
  deleted WPF listener bridge.
- Space and Enter use the source key-down pressed state and key-up primary-click
  plus command ordering. Alt+Down and F4 open the secondary flyout.
- ToggleSplitButton toggles before raising the inherited primary click;
  `IsCheckedChanged` and the Toggle-pattern property event are withheld until
  the source-equivalent loaded flag is set.
- SplitButton exposes Invoke plus ExpandCollapse; ToggleSplitButton exposes
  Toggle plus ExpandCollapse and intentionally does not expose Invoke. Both
  peers report `AutomationControlType.SplitButton`, their concrete class name,
  collapsed/expanded state from the active flyout, and no template-part
  children in the WPF control view.
- The source split-border chrome is retained: separate primary and secondary
  borders, a spanning primary background, 35-DIP secondary column, source
  padding, corner radii, divider, foreground, border, pressed, checked, and High
  Contrast resources.

## WPF and Gallery Substitutions

- WinUI Grid supports corner radius and border properties. The WPF template
  uses `GridEx` for the root and split border layers while retaining the same
  names, geometry, bindings, and state setter targets.
- WinUI uses `AnimatedIcon` with `AnimatedChevronDownSmallVisualSource`.
  ModernWpf uses the repository's `FontIconFallback` chevron because that WinUI
  animated source type is unavailable to WPF.
- WPF has no `AutomationProperties.AccessibilityView=Raw` or WinUI GamepadA
  input path. The peer suppresses template-part children directly; keyboard
  coverage retains Space, Enter, Alt+Down, and F4.
- WPF `Button.CommandTarget` remains bound to ModernWpf's WPF-specific
  `SplitButton.CommandTarget`; source Command and CommandParameter semantics are
  otherwise preserved.
- The current Gallery's RichEditBox is represented by WPF RichTextBox. A
  24-DIP spacer column represents WinUI Grid `ColumnSpacing`. WPF flyout content
  uses clickable Rectangle-in-Button entries in a `VariableSizedWrapGrid` in
  place of the first WinUI sample's item-click GridView; the color list, names,
  selection behavior, and control crop remain equivalent.
- WPF's RichTextBox represents list formatting as `List` / `ListItem` objects.
  The current adapter executes the matching bullet/number editing command for
  both checked and unchecked transitions, then assigns the selected WPF
  `List.MarkerStyle`. This is the native WPF equivalent of WinUI's explicit
  RichEditBox `MarkerType` assignment and fixes the former unchecked early
  return that left list formatting active.

## Regression Coverage

- `SplitButtonApiTests` pins defaults, source template parts and geometry,
  Light/Dark resource aliases, High Contrast resources, all source state setter
  targets, CommandBar state ownership, active-flyout re-registration,
  BottomEdgeAlignedLeft placement, and removal of listener/trigger relays.
- `SplitButtonInteractionTests` covers primary and secondary clicks, commands,
  Space/Enter/F4, flyout replacement, toggle-before-click ordering, loaded event
  behavior, Invoke/Toggle/ExpandCollapse patterns, concrete class/control type,
  hidden template children, and the mutually exclusive Invoke/Toggle contract.
- `GalleryAutomationHookTests` pins the current two-plus-one sample structure,
  sizes, spacing, names, snippets, icons, flyout entries, Roman selection, and
  the newly locked list-format removal when unchecked.
- `WpfGallerySourceShapeTests` pins real ModernWpf and WinUI element IDs,
  normal-state-before-interaction capture ordering, secondary-segment popup
  proof, mean-delta limits, and explicit zero size tolerance for both controls.
- `SplitButtonSourceAuditTests` pins the current product/Gallery commits and
  blobs, implementation shape, current WPF substitutions, reports, thresholds,
  and this audit.

## Live Installed-Gallery Evidence

The harness moves the pointer away, captures static normal-state pixels first,
then separately invokes the real secondary segment and requires the expected
opened item (`Red` or `Bulleted list`). Static crops use the real current
elements: ModernWpf diagnostic IDs and official `myColorButton` / `myListButton`.

| Theme | Report | Control | Crop sizes | Mean delta | Gate |
| --- | --- | --- | --- | ---: | --- |
| Light | `artifacts/visual-checks/20260718-113331-468-93712/report.md` | SplitButton | `71x32` / `71x32` | `0.46` | `1.0`, size `0` |
| Dark | `artifacts/visual-checks/20260718-113651-452-2372/report.md` | SplitButton | `71x32` / `71x32` | `0.37` | `1.0`, size `0` |
| Light | `artifacts/visual-checks/20260718-113331-468-93712/report.md` | ToggleSplitButton | `78x33` / `78x33` | `1.62` | `2.0`, size `0` |
| Dark | `artifacts/visual-checks/20260718-113651-452-2372/report.md` | ToggleSplitButton | `78x33` / `78x33` | `0.98` | `2.0`, size `0` |

The remaining non-zero pixels are WPF/WinUI text and symbol antialiasing; the
control extents, segment geometry, backgrounds, borders, divider, glyph, and
chevron are aligned.

## Verification

- Product behavior/accessibility/source slice: 15/15 passed on
  `net8.0-windows7.0`.
- Focused Gallery sample/harness slice: 4/4 passed on both
  `net8.0-windows7.0` and `net10.0-windows7.0` after the list-format fix.
- `ModernWpf.Controls` builds pass net462, net8, and net10 with zero errors;
  the focused net462 build is warning-free.
- Both strict Light and Dark installed-Gallery runs passed with exact crop size
  and opened-flyout proof for both controls.
