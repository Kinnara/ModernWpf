# ItemsView WinUI 3 source audit

## Scope

Preview 7 adds the complete current-WinUI-shaped `ItemsView` control family on
top of the Preview 6 `ItemContainer`, `LinedFlowLayout`, item-transition, and
scrolling foundations. `ItemsView` is a new ModernWPF control; it does not
replace WPF `ItemsControl`, `ListBox`, `ListView`, or `GridView`.

The audit uses the already adopted Preview 6 synchronization epoch. No moving
branch is used as build input.

| Source | Accepted revision | Use |
| --- | --- | --- |
| WinUI stable | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Latest stable boundary at the adopted epoch. |
| WinUI `winui3/main` | `23a73be03d194ea0ece97da71de98b6b53021b70` | Current product, API, template, automation, test, and TestUI authority. |
| WinUI Gallery `main` | `b78c440193aab788215888561e45adf72da848cb` | Current three-example Gallery authority. |
| Historical ModernWPF prototype | `78488c1735dbce861d3dcf57ff2666e522201492` | Selective implementation input only; it is not the compatibility or source authority. |

The finite epoch disposition remains in
[`winui3-sync-2026-08-10-preview6.md`](winui3-sync-2026-08-10-preview6.md).
Later upstream movement belongs to the next review interval and follows that
document's critical-fix policy.

## Pinned product and test inputs

The complete `controls/dev/ItemsView` directory was enumerated through the
GitHub API at the accepted product revision. The primary inputs are:

| Upstream path | Git blob |
| --- | --- |
| `controls/dev/ItemsView/ItemsView.idl` | `9237bfe51b411d4c5b498bf93f064270c76e3bf3` |
| `controls/dev/ItemsView/ItemsView.cpp` | `65acb3f3eda381808c855ceeafd3ecf0e86a34f9` |
| `controls/dev/ItemsView/ItemsView.h` | `ea35d98765c3620e55332955faf2a865943bc8b4` |
| `controls/dev/ItemsView/ItemsView.xaml` | `f8ac105313c64b831e8debdeff39bc2a305fe815` |
| `controls/dev/ItemsView/ItemsViewAutomationPeer.cpp` | `27c6248d055005ad27477ffe957ffb79ae155b47` |
| `controls/dev/ItemsView/ItemsViewInteractions.cpp` | `b682abfb745b09b1e792eecb0eedc90ef023c3cb` |
| `controls/dev/ItemsView/ItemsViewItemInvokedEventArgs.cpp` | `16a0b6694fe27fe379b4379a834301e73087120a` |
| `controls/dev/ItemsView/ItemsViewSelectionChangedEventArgs.cpp` | `9c6fb57658ac0fb3056157db647af340ae2b9c93` |
| `controls/dev/ItemsView/ItemsView_themeresources.xaml` | `a9381d8d76a4bb401ace5fce98bb63723873d02f` |
| `controls/dev/ItemsView/APITests/ItemsViewTests.cs` | `e99e927f6942248eb81d6d6e5a018e879713e2c3` |
| `controls/dev/ItemsView/InteractionTests/ItemsViewTestsWithInputHelper.cs` | `bbb693eb23ed098023c1794cf44390f534e63ebb` |
| `controls/dev/ItemsView/TestUI/ItemsViewPage.xaml` | `647723cfb8d012b084278ebfbd9bb9cfa25f9194` |
| `controls/dev/ItemsView/TestUI/ItemsViewTransitionPage.xaml` | `b63d6a3e388c50784d6b2ae1ca1b2c0bba8f2167` |
| `controls/dev/Generated/ItemsView.properties.cpp` | `02e3056f8efd7d4414addd22cc38403e09971c4f` |
| `docs/design-notes/ItemsView_spec.md` | `541dc3aecaa4a2bde243c550d5bc2b63ea0f0b33` |

The selector implementations (`NullSelector`, `SingleSelector`,
`MultipleSelector`, and `ExtendedSelector`), test hooks, headers, project
items, and remaining TestUI pages at the same directory revision were also
classified. Test hooks, tracing, C++ build files, and WinRT-only diagnostics
remain internal and do not become package API.

The Gallery inputs are pinned independently:

| Upstream path | Git blob |
| --- | --- |
| `WinUIGallery/Samples/ItemsView/ItemsViewPage.xaml` | `c1c77b2be151de1b03376a8339b3d7881efb137b` |
| `WinUIGallery/Samples/ItemsView/ItemsViewPage.xaml.cs` | `8fe7c8fb0ce0bb9e6c64d92d3d7d25b3e5fa73ac` |
| `WinUIGallery/Samples/ItemsView/BasicItemsview.txt` | `36f27f326076677ef2da671410bad69a755004e7` |
| `WinUIGallery/Samples/ItemsView/ItemsviewSwappableLayouts.txt` | `25037e9ded023e6061dbbfa1efc89045064f682d` |
| `WinUIGallery/Samples/ItemsView/ItemsviewItemInvocationSelection.txt` | `bdc115a977c7b32b25a59f0b7b525bea78338117` |

## Public surface and WPF substitutions

The local public contract follows the accepted IDL: an unsealed `ItemsView`,
`ItemsViewSelectionMode`, empty selection-changed event args, invoked-item
event args, and a public `ItemsViewAutomationPeer`. It includes the source
properties, read-only selection/current-item state, selection methods,
viewport lookup, bring-into-view method, and events.

WPF requires these explicit type substitutions:

- WinUI `ScrollView` maps to `System.Windows.Controls.ScrollViewer` through the
  read-only `ScrollView` property. Preview 7 does not publish an incomplete
  WPF `ScrollView` or `ScrollPresenter` facade.
- WinUI `Microsoft.UI.Xaml.BringIntoViewOptions` maps to
  `ModernWpf.BringIntoViewOptions`, retaining target rectangle, alignment
  ratios, offsets, and animation intent. The existing Preview 6 scroll bridge
  applies those values to WPF scrolling.
- WinUI `IElementFactory` remains supported. The WPF-facing `ItemTemplate` is
  typed as `object` so it can also accept WPF `DataTemplate` and
  `DataTemplateSelector` inputs already understood by `ItemsRepeater`.
  Realized template roots must be `ItemContainer`, matching ItemsView's item
  interaction contract.
- WinUI `Control` supplies `CornerRadius`; WPF `Control` does not. ModernWPF
  therefore declares a compatible `CornerRadius` dependency property and
  binds it through the WPF `ScrollViewer` template boundary.
- `SelectedItems` is exposed as `IReadOnlyList<object>` instead of a WinRT
  vector view. `VerticalScrollController` retains the existing WPF-portable
  `IScrollController` contract.

## Behavior carried into WPF

- The default mode is `Single`; `None`, `Multiple`, and `Extended` use the
  corresponding selection policies. Programmatic select, deselect, select
  all, clear, invert, and source-collection changes keep realized
  `ItemContainer` state and selection events synchronized.
- Pointer release updates selection, double-click/double-tap and Enter invoke
  when enabled, and Space updates selection. `Ctrl+A` selects all only in
  `Multiple` and `Extended`, matching current source behavior.
- Home, End, arrow, Page Up, and Page Down navigation use realized layout
  geometry across `StackLayout`, `UniformGridLayout`, and `LinedFlowLayout`.
  WPF keyboard focus and `BringIntoView` replace WinUI XYFocus/gamepad and
  InteractionTracker services.
- `ItemsViewAutomationPeer` exposes List identity and `ISelectionProvider`;
  realized `ItemContainerAutomationPeer` instances supply SelectionItem and
  opt-in Invoke behavior.
- `ItemTransitionProvider` is a first-class dependency property and is
  forwarded to the template's `ItemsRepeater`. The Preview 6
  `LinedFlowLayoutItemCollectionTransitionProvider` supplies WPF-native add,
  remove, move, and layout animations.
- The template keeps source part names `PART_ScrollView` and
  `PART_ItemsRepeater`, a default `StackLayout`, `IsTabStop=false`, one-pass
  tab navigation, zero horizontal realization cache, Padding forwarding, and
  the source `NaN` horizontal-anchor setting. Realization, recycling, and
  vertical viewport anchoring remain owned by the Preview 6 WPF scroll bridge.

WPF can delay the `ScrollViewer` dependency-property cache update until
`LayoutUpdated`, especially in an off-screen automated host. The internal
adapter uses live `ScrollContentPresenter` viewport, extent, and offset values
only when the owning `ScrollViewer` cache is stale, then calls
`InvalidateScrollInfo`. Normal on-screen behavior continues to use the public
`ScrollViewer` values. Focused regressions cover both ordinary content and a
factory-generated virtualizing `ItemsRepeater` subtree.

WinUI compositor expressions, native InteractionTracker state, private test
hooks, telemetry, and gamepad-specific plumbing are not copied. Their WPF
substitutes are tested through visible layout, native scrolling, keyboard,
pointer, automation, and transition outcomes.

## Resources, Gallery, and validation

The source template consumes existing control, scroll, focus, and
`ItemContainer` resources.

Preview 7 adds no ItemsView-specific public resource keys. The CLR additions
are listed in the unshipped public API inventories and remain preview contract
input for the RC freeze.

The Gallery adds three durable examples: basic selection/invocation; switching
between Stack, UniformGrid, and LinedFlow layouts with live add/remove
transitions; and selection/invocation mode controls. Focused product coverage
locks defaults, XAML/template constraints, collection changes, selection,
input, spatial keyboard navigation, automation, bring-into-view, viewport
lookup, transition forwarding, and all three supported layout families.
Theme coverage loads the template under Light, Dark, and High Contrast
resources, while final release validation exercises the real Gallery on every
supported target.

`PipsPager` remains deferred to 1.1 and is not part of this audit.
