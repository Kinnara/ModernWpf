# ModernWPF 1.0.0-preview.7

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.7` completes the source-audited `ItemsView` milestone on top
of the Preview 6 item-container, layout, selection, virtualization, and WPF
scrolling foundations. This is the final feature preview in the fixed
ModernWPF 1.0 roadmap; release-candidate stabilization follows without
compressing or renumbering the sequence.

## Preview compatibility

- The latest published preview becomes the active package-validation baseline
  when this development branch is rebased for release.
- `1.0.0-preview.1` remains the historical audit and migration baseline rather
  than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md).

## ItemsView

- `ItemsView` supplies source-shaped item creation, current item, item
  invocation, `None`/`Single`/`Multiple`/`Extended` selection, selected-item
  results, collection-change synchronization, and programmatic selection.
- Keyboard and pointer behavior includes navigation, range and toggle
  modifiers, select-all, page movement, invocation, and automatic
  bring-into-view.
- The public automation peer exposes List and Selection semantics while
  ItemContainer peers provide ListItem, SelectionItem, and conditional Invoke
  behavior with collection position metadata.
- `BringIntoViewOptions`, viewport-ratio lookup, target rectangles, offsets,
  alignment ratios, and optional vertical scroll-controller integration
  complete the public scrolling surface.
- The default factory, WPF DataTemplate/DataTemplateSelector adaptation, and
  existing `IElementFactory` support all realize ItemContainer roots so the
  selection and accessibility contract remains coherent.

The pinned product and Gallery inputs, public surface, and WPF adaptations are
documented in the [ItemsView source audit](itemsview-winui3-source-audit.md)
and [scrolling adaptation](itemsview-scrolling-wpf-adaptation.md).

## Gallery, themes, and validation

- The Gallery adds real ItemsView examples for item invocation, swappable
  StackLayout/UniformGridLayout/LinedFlowLayout arrangements, and live
  selection/invocation modes.
- Focused tests cover selection policies, collection changes, templates,
  input, automation, viewport lookup, bring-into-view, source pins, and
  catalog/runtime behavior.
- ItemsView uses the existing ItemContainer and shared control resources, so
  no new public resource keys are introduced. Light, Dark, High Contrast,
  standard, and compact resource paths remain covered.
- CLR/resource inventories, all supported package targets, the complete
  serialized release gate, downstream canaries, and final visual evidence
  remain required before publication.

## Breaking changes and migration

Preview 7 plans no intentional break to the latest published preview's CLR or
public resource-key surface. ItemsView, its event arguments and automation
peer, `ItemsViewSelectionMode`, and `BringIntoViewOptions` are additive
preview APIs. It is not an automatic replacement for WPF ListBox or ListView;
applications should choose it when they need the WinUI-shaped virtualized
layout, invocation, and selection model.

## Known preview limitations

- Until the draft marker is removed and the tagged workflow completes, this
  file describes an unpublished development package.
- `ScrollView` is exposed as WPF `System.Windows.Controls.ScrollViewer`; the
  package does not publish partial WinUI ScrollView or ScrollPresenter shells.
- WinUI `ItemTransitionProvider` is omitted because ModernWPF does not ship
  the compositor-driven item-transition family.
- Windowed drag/tear-out behavior remains application-owned; ItemsView does
  not mutate the source collection implicitly.
- `PipsPager` remains deferred to 1.1. The next 1.0 milestone is `rc.1`, not an
  additional feature preview.
