# ItemsView current WinUI 3 source audit

Date: 2026-08-09

ModernWPF Preview 7 completes the WinUI-shaped `ItemsView` milestone on top
of Preview 6 `ItemContainer`, `LinedFlowLayout`, `ItemsRepeater`, selection,
and scrolling foundations. The control is additive and does not replace WPF
`ListBox`, `ListView`, or their stock Fluent styles.

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml `winui3/main` | `e1aa8f64df98d6229f6cd4074d59b654616254da` | Product, generated properties, public IDL, template, selectors, automation, API tests, and interaction cutoff. |
| microsoft-ui-xaml `winui3/release/2.3.1` | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Stable reconciliation. This older stable cutoff does not contain ItemsView, so the accepted current main snapshot is authoritative. |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | Basic, layout-switching, selection, and item-invocation examples. |

The exact upstream product inputs are:

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls/dev/Generated/ItemsView.properties.cpp` | `02e3056f8efd7d4414addd22cc38403e09971c4f` |
| `controls/dev/Generated/ItemsView.properties.h` | `fab53302989845667ca30823a96ce499892aebd0` |
| `controls/dev/Generated/ItemsViewAutomationPeer.properties.cpp` | `7b7caed228695debd675feb032144971948f8392` |
| `controls/dev/ItemsView/ItemsView.cpp` | `65acb3f3eda381808c855ceeafd3ecf0e86a34f9` |
| `controls/dev/ItemsView/ItemsView.h` | `ea35d98765c3620e55332955faf2a865943bc8b4` |
| `controls/dev/ItemsView/ItemsView.idl` | `9237bfe51b411d4c5b498bf93f064270c76e3bf3` |
| `controls/dev/ItemsView/ItemsView.xaml` | `f8ac105313c64b831e8debdeff39bc2a305fe815` |
| `controls/dev/ItemsView/ItemsView_themeresources.xaml` | `a9381d8d76a4bb401ace5fce98bb63723873d02f` |
| `controls/dev/ItemsView/ItemsViewAutomationPeer.cpp` | `27c6248d055005ad27477ffe957ffb79ae155b47` |
| `controls/dev/ItemsView/ItemsViewAutomationPeer.h` | `aaa6999306104de99f534628f3d84919705d9fed` |
| `controls/dev/ItemsView/ItemsViewInteractions.cpp` | `b682abfb745b09b1e792eecb0eedc90ef023c3cb` |
| `controls/dev/ItemsView/ItemsViewItemInvokedEventArgs.cpp` | `16a0b6694fe27fe379b4379a834301e73087120a` |
| `controls/dev/ItemsView/ItemsViewSelectionChangedEventArgs.cpp` | `9c6fb57658ac0fb3056157db647af340ae2b9c93` |
| `controls/dev/ItemsView/NullSelector.cpp` | `d2e0fb41908098bc8c52f857311f99b1420d8e0a` |
| `controls/dev/ItemsView/SingleSelector.cpp` | `454c9b9b39ec1ef41415110b702f68f7866f94a0` |
| `controls/dev/ItemsView/MultipleSelector.cpp` | `9269742b53846bcce3d4a29dc7a85065500acee2` |
| `controls/dev/ItemsView/ExtendedSelector.cpp` | `590c86de186abbcea8b0436839a62b4fa6615564` |
| `controls/dev/ItemsView/SelectorBase.cpp` | `18a0536ae88de2434a19822df88f868a147626e9` |
| `controls/dev/ItemsView/APITests/ItemsViewTests.cs` | `e99e927f6942248eb81d6d6e5a018e879713e2c3` |
| `controls/dev/ItemsView/InteractionTests/ItemsViewTestsWithInputHelper.cs` | `bbb693eb23ed098023c1794cf44390f534e63ebb` |
| `controls/dev/Repeater/SelectionModel.cpp` | `711448f2708ae07d25cbab118560962bcd8deaca` |
| `controls/dev/Repeater/ItemsRepeaterScrollHost.cpp` | `fdd2b26086742f271e9e872ada409faa6befc800` |
| `controls/dev/Repeater/ItemsRepeaterScrollHost.h` | `c3fcaba0f931b2093ae129a2cf1131c68a5ac1d5` |
| `dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.cs` | `749eedc63ca3237e01c1a02fed2a5bd1120c9807` |
| `dxaml/xcp/dxaml/lib/BringIntoViewOptions_Partial.cpp` | `548de8a38c849e6b2fbf65c73cb3d37cc2ba8b2d` |
| `dxaml/xcp/dxaml/lib/BringIntoViewOptions_Partial.h` | `022aa109ce90cb56838402219b8bf36082616e9a` |

The exact Gallery inputs are:

| File under WinUI Gallery | Blob |
| --- | --- |
| `WinUIGallery/Samples/ItemsView/BasicItemsview.txt` | `36f27f326076677ef2da671410bad69a755004e7` |
| `WinUIGallery/Samples/ItemsView/ItemsViewPage.xaml` | `c1c77b2be151de1b03376a8339b3d7881efb137b` |
| `WinUIGallery/Samples/ItemsView/ItemsViewPage.xaml.cs` | `8fe7c8fb0ce0bb9e6c64d92d3d7d25b3e5fa73ac` |
| `WinUIGallery/Samples/ItemsView/ItemsviewItemInvocationSelection.txt` | `bdc115a977c7b32b25a59f0b7b525bea78338117` |
| `WinUIGallery/Samples/ItemsView/ItemsviewSwappableLayouts.txt` | `25037e9ded023e6061dbbfa1efc89045064f682d` |

The `items-view` entry in `tools/upstream/upstream-sync.json` owns these
product and Gallery paths. Later upstream movement therefore creates a new
review interval instead of being hidden under the SelectorBar audit.

## Public surface

The WPF port follows the current IDL for `ItemsSource`, `ItemTemplate`,
`Layout`, `IsItemInvokedEnabled`, `SelectionMode`, `CurrentItemIndex`,
`SelectedItem`, `SelectedItems`, `VerticalScrollController`,
`TryGetItemIndex`, `StartBringItemIntoView`, selection methods,
`ItemInvoked`, `SelectionChanged`, and the public automation peer.
`ItemsViewSelectionMode` preserves `None`, `Single`, `Multiple`, and
`Extended`; `Single` and disabled invocation remain the defaults.

`ModernWpf.BringIntoViewOptions` carries the current animation, target-rect,
alignment-ratio, and offset surface. Alignment ratios preserve WinUI's NaN
sentinel and clamp finite values to `[0, 1]`. The WPF port also supplies the
source event-argument shapes and read-only selection/current-item results.

WinUI's `ItemTransitionProvider` is deliberately omitted. ModernWPF does not
publish the compositor-driven item-transition family, and a property with no
faithful implementation would create a misleading public contract. This is
the only omitted current ItemsView property.

## Item creation, selection, and input

The default factory wraps ordinary data in `ItemContainer`; WPF
`DataTemplate`, `DataTemplateSelector`, and the existing `IElementFactory`
are accepted through an `object`-typed `ItemTemplate` adaptation. A custom
template must still realize an `ItemContainer` root so ItemsView can own
selection, invocation, focus, and automation consistently. UIElement source
items must already be ItemContainer roots rather than being reparented into a
second visual owner.

The four source selector policies are retained on the audited
`SelectionModel`. Single selection replaces the previous item; Multiple
toggles independent items; Extended uses Shift ranges and Ctrl toggles; None
keeps selection empty. The flat SelectAll path is explicit so string and
other enumerable data items are not mistaken for nested selection sources.
Collection changes update realized state, current item, selected results,
and automation set metadata.

WPF preview mouse and keyboard input replaces WinUI pointer, gamepad, and
focus-manager services. Arrow keys, Home/End, PageUp/PageDown, Enter, Space,
Ctrl+A, Ctrl+Space, and Shift/Ctrl range modifiers feed the same selection,
current-item, invocation, and bring-into-view policies. The application still
owns its data after `ItemInvoked`; the control does not navigate or mutate the
source implicitly.

## Scrolling and layout adaptations

The source `ScrollView` property is represented by WPF
`System.Windows.Controls.ScrollViewer`. The default template composes
`ItemsRepeaterScrollHost`, the internal `ItemsViewScrollHost`, and
`ItemsRepeater`, while its public `Layout` remains the existing ModernWPF
layout family and defaults through style to `StackLayout`.

`StartBringItemIntoView` realizes the target, honors target rectangles,
alignment ratios, and offsets, and uses WPF scroll offsets. NaN alignment
ratios request minimum scrolling rather than accidental numeric propagation.
`TryGetItemIndex` resolves the realized item intersecting the requested
viewport ratios. The optional vertical scroll controller continues through
Preview 6's WPF-native controller bridge. Full rationale is in
[the scrolling adaptation](itemsview-scrolling-wpf-adaptation.md).

## Accessibility and theme behavior

`ItemsViewAutomationPeer` reports List and implements WPF
`ISelectionProvider`; its selection-required and multi-select flags follow
the active mode. Realized `ItemContainerAutomationPeer` instances remain the
ListItem/SelectionItem/conditional Invoke children, and position/size
metadata follows collection indexes. Programmatic, pointer, keyboard, and
automation selection share the same model and raise selection-property
changes.

WPF's PositionInSet and SizeOfSet attached properties are unavailable in the
net462 reference assembly. As with the repository's existing collection
controls, those optional attached values are guarded by `NET48_OR_NEWER`;
net462 still exposes List, Selection, SelectionItem, Invoke, selected state,
and child order through its available automation contracts.

The upstream ItemsView theme dictionary contains no control-specific public
resource keys. ModernWPF therefore adds no ItemsView keys: the template uses
the Preview 6 ItemContainer resources and existing control/chrome resources
in Light, Dark, High Contrast, standard, and compact entries. WPF adds a
control-level `CornerRadius` dependency property because WinUI inherits that
property from its base Control while WPF does not.

## Gallery and acceptance coverage

The Gallery page uses the real control for basic item invocation, swappable
StackLayout/UniformGridLayout/LinedFlowLayout arrangements, and live
selection/invocation modes. Focused product tests cover defaults, templates,
all programmatic selection operations, pointer/keyboard invocation,
automation, invalid template roots, viewport lookup, and bring-into-view.
Theme tests cover Light, Dark, High Contrast, standard, and compact entries;
Gallery tests cover catalog, examples, source pins, and runtime automation.

Publication still requires the complete serialized release gate, package/API
and resource verification, executable consumers on all supported targets,
downstream canaries, and final Gallery Light/Dark/real-OS-High-Contrast
evidence.
