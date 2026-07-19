# SelectorBar WinUI 3 Source Audit

Date: 2026-07-18

ModernWpf `SelectorBar` and `SelectorBarItem` are tracked as a source-backed WPF
port of official `microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17). The current Gallery
contract is pinned to WinUI Gallery commit
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13). Live comparison uses
the installed WinUI 3 Controls Gallery `2.9.3.0` with Windows App Runtime
`2.2.3.0.0`.

## Product Source Baseline

The product repository moved its mirrored tree from `src\...` to the repository
root in `8463f45162149de0ec3ad7df752596893fe3e13e`. The last audited pre-move
product baseline is `c70471c511a0168b61dcca13af9556465f26b673`; the only
SelectorBar history after that baseline is the root move, so all substantive
runtime, item, peer, classic-template/theme, resource, API-test, and interaction
test blobs remain byte-current. Current paths intentionally omit `src\`.

Primary current WinUI 3 inputs and blob IDs:

| Source | Current blob |
| --- | --- |
| `controls\dev\SelectorBar\APITests\SelectorBarTests.cs` | `4a26ec257cf6ec5b03d494d489c95abe3d887af2` |
| `controls\dev\SelectorBar\InteractionTests\SelectorBarTests.cs` | `ff3f546f8f22580dbcbb5dcbcc5689a15e543109` |
| `controls\dev\SelectorBar\SelectorBar.cpp` | `03fb32f3f7ded885ead07735139848adc8882197` |
| `controls\dev\SelectorBar\SelectorBar.h` | `c4c2bf7dec892002631b7d96ace9a707d9ed00da` |
| `controls\dev\SelectorBar\SelectorBar.idl` | `5de9e849b5f3b678dde121b75b4983f352cbb803` |
| `controls\dev\SelectorBar\SelectorBar.xaml` | `b928003fb521d2901aa1dc3edef3f16a2fd869f3` |
| `controls\dev\SelectorBar\SelectorBarItem.cpp` | `205c8df3caecd96237136a347d428d73723a6d88` |
| `controls\dev\SelectorBar\SelectorBarItem.h` | `0bbc42853e410432d5ccc7eecc51f3f0fc767e72` |
| `controls\dev\SelectorBar\SelectorBarItemAutomationPeer.cpp` | `54d37c95f97f6d366333a7996301f86b2bb57bba` |
| `controls\dev\SelectorBar\SelectorBar_perf2026.xaml` | `01951cdbb3af6802e9cf6d457e62bbd9e02f286d` |
| `controls\dev\SelectorBar\SelectorBar_themeresources.xaml` | `194182902a7df8b134b1eeeda17c0bf354464c44` |
| `controls\dev\SelectorBar\Strings\en-us\Resources.resw` | `6019fa505d3a1f90bf22c89138470c03c994056e` |
| `controls\dev\ItemsView\ItemsViewAutomationPeer.cpp` | `27c6248d055005ad27477ffe957ffb79ae155b47` |
| `controls\dev\ItemContainer\ItemContainerAutomationPeer.cpp` | `abf79f19728820b2d4db4649fcfd698500d939ba` |

The current `SelectorBar_perf2026.xaml` preserves the classic part tree,
metrics, resource aliases, animations, and state semantics while representing
discrete brush changes as setters. ModernWpf already expresses that current
variant through `VisualStateEx.Setters`.

## Current Gallery Baseline

WinUI Gallery converted this page to `SampleDefinition` files in
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`; no later SelectorBar sample commit
is present through the current pin. Current paths and blobs are:

| Gallery source | Current blob |
| --- | --- |
| `WinUIGallery\Samples\SelectorBar\SelectorBarPage.xaml` | `08c77fd4eb5e3105e210166a00e0f0082cf8d873` |
| `WinUIGallery\Samples\SelectorBar\SelectorBarPage.xaml.cs` | `710711c1daf70fb6914a46fb6c8f0f90600f5040` |
| `WinUIGallery\Samples\SelectorBar\BasicSelectorbar.txt` | `34b1d24fda342e274202c33bd4131ac93b48d873` |
| `WinUIGallery\Samples\SelectorBar\SelectorbarDisplayingDifferentCollections.txt` | `9ece269a599518155ff6b686f2bdd296fde166e2` |
| `WinUIGallery\Samples\SelectorBar\SelectorbarFrameSlideTransitions.txt` | `c52043ced6a16955cfa56b06218c003e08a34c85` |
| `WinUIGallery\Styles\SelectorBar.xaml` | `af62f7f6df5c54bd51e8c0db2ece59aa6114c7cb` |

The page still contains exactly three examples: the `Recent`/`Shared`/`Favorites`
bar; a five-page Frame example with directional slide transitions; and a
Pink/Plum/PowderBlue collection switcher. ModernWpf Gallery keeps the current
headers, source-facing names, displayed definitions (including the current
third-definition `UniformGridLayout` text), runtime item counts, selection
flows, frame content, and color collections. WPF substitutes a `Frame` content
change and an `ItemsControl` for the unavailable Gallery `ItemsView` surface.

## Ported Runtime and Visual Shape

- The default template exposes source `PART_ItemsView`; the former guessed
  `PART_ItemsPanel` manual child-injection path remains deleted.
- The WPF `SelectorBarItemsControl` binds to `Items`, lays out horizontally in a
  `ScrollViewer`, and owns the single-selection bridge that current WinUI owns
  through `ItemsView`.
- `SelectedItem` must belong to `Items`; item changes synchronize both ways,
  removal clears selection, preselected items initialize on load, and every
  change raises `SelectionChanged` with source-shaped empty event args.
- Focus without a valid selection selects the current/focused item or the first
  enabled, visible, focusable item. Mouse, Enter/Space, and direction-aware
  Left/Right input update focus/selection through WPF equivalents of current
  ItemContainer/ItemsView behavior.
- `SelectorBarItem.UpdatePartsVisibility` independently collapses missing icon
  and text parts and collapses their common parent when both are absent.
- The item template retains `PART_IconVisual`, `PART_TextVisual`,
  `PART_SelectionVisual`, `PART_CommonVisual`, source resources and states,
  0.8 icon scale, eight-DIP spacing, pill geometry, focus bounds, and 48-DIP
  Gallery height.
- A render-only one-pixel WPF text translation aligns the glyph baseline while
  leaving icon origin, measurement, pill, focus, and hit geometry source-sized.
  Exact installed-Gallery primary crops remain `284x48`.

## Current Accessibility Contract

- Current WinUI gives `SelectorBar` no separate peer. Its internal `ItemsView`
  is the active List/Selection provider; each `SelectorBarItem` inherits the
  ItemContainer ListItem/SelectionItem contract and overrides class/name/type.
- ModernWpf now follows that visible tree: `SelectorBarItemsControlAutomationPeer`
  reports class `ItemsView`, List role, single optional selection, and the
  selected ListItem provider. Both direct and generated item peers report
  `SelectorBarItem`, ListItem, SelectionItem, and the internal List as their
  selection container. The former Tab/TabItem roles are removed.
- Item name fallback order is current source order: explicit automation name,
  `Text`, `Child.ToString()`, then `SelectorBarItemDefaultControlName`.
- The default name/localized control type is now resolved through the control
  resource pack instead of a hardcoded literal; the upstream en-US value is
  `SelectorBarItem`.
- WPF retains a raw, non-Control/non-Content `SelectorBarAutomationPeer` only so
  app/harness automation IDs on the outer WPF control remain addressable. It is
  excluded from the accessible Control and Content views; selection ownership
  stays on the source-equivalent internal List.

## WPF Substitutions

- WinUI `ItemsView`, `ItemContainer`, and `StackLayout` are represented by a
  purpose-built WPF `ItemsControl`, direct item control, horizontal
  `StackPanel`, and source-shaped selection peers.
- `Grid.CornerRadius`, `Grid.BackgroundSizing`, and content-presenter chrome use
  `GridEx`, `BackgroundSizing`, and `ContentPresenterEx`.
- WinUI `VisualState.Setters` use `VisualStateEx.Setters`; pointer animation,
  compositor transforms, XY focus, and `ItemsView.CurrentItemIndex` map to WPF
  state setters, mouse capture, focus, and directional navigation.
- Only the en-US SelectorBar resource pack is currently added. Other WinUI
  translations remain a localization follow-up.
- The Gallery maps source `Favorite` to ModernWpf's outline-star symbol because
  ModernWpf's `Symbol.Favorite` glyph is filled while the installed Gallery's
  current first example renders an outline star. Displayed source text remains
  unchanged.
- WPF and WinUI place the same Segoe UI Variable Text run on adjacent physical
  baselines; the documented one-pixel render translation is platform-specific.

## Regression Guards

- `SelectorBarSourceAuditTests` pins the current product/Gallery commits,
  root-move boundary, product/Gallery blobs, current paths, classic/perf mapping,
  accessibility roles/ownership/resources, strict artifacts, and selection
  recorder anchor.
- `SelectorBarApiTests` covers current defaults, Items collection/selection,
  invalid selected items, state/resource/template geometry, part visibility,
  mouse selection, List/ListItem peer shape, Selection/SelectionItem patterns,
  selection-container ownership, raw outer wrapper, and localized fallback.
- `GalleryAutomationHookTests.SelectorBarSampleMatchesWinUIGalleryExamples`
  covers all three current definitions, source names, exact first-example
  geometry, icons, selections, Frame page changes, and 5/7/4 color counts.
- `WpfGallerySourceShapeTests.GalleryVisualChecksEnforceSelectorBarPixelParityThreshold`
  pins the ModernWpf/reference targets and strict `3.0` primary gate.
- The recording harness anchors on the actual `Shared` ListItem, invokes its
  SelectionItem pattern, requires Unselected-to-Selected state evidence, and
  separately requires a rendered local delta.

## Current Validation

- Fresh Light comparison
  `artifacts/visual-checks/20260718-211850-273-87320/report.md` passes at `1.99`;
  fresh Dark comparison
  `artifacts/visual-checks/20260718-211931-717-99256/report.md` passes at `2.58`.
  Both compare exact `284x48` live primary controls under the `3.0` gate.
- Fresh Light selection recording
  `artifacts/gallery-recordings/20260718-212208-592/report.md` passes with
  `0.001` maximum frame delta and `0.711` maximum local delta. Fresh Dark
  recording `artifacts/gallery-recordings/20260718-212254-641/report.md` passes
  with `0` / `0.251`. Both use the real `Shared` SelectionItem provider, prove
  Unselected-to-Selected state, and finish within `2.7s` of the six-second
  maximum window.
- The complete SelectorBar product/source/accessibility slice passes 11/11 on
  `net8.0-windows7.0`.
- The SelectorBar Gallery sample/crop slice passes 2/2 on net8 and net10.
- `ModernWpf.Gallery` builds successfully for net462, net8, and net10 with zero
  errors; current target builds retain existing unrelated warnings and no
  SelectorBar warning.
