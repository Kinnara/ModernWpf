# ModernWPF 1.0.0-preview.7

`1.0.0-preview.7` adds the complete current-WinUI-shaped `ItemsView` control
family. It is the final planned feature preview in the fixed ModernWPF 1.0
roadmap; the next milestone is the API/resource-key freeze and release-candidate
soak, not another compressed preview.

## Preview compatibility

- `1.0.0-preview.6` is the active package-validation baseline for this
  development cycle.
- `1.0.0-preview.1` remains the immutable historical audit and migration
  baseline rather than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the compatibility and
migration policies.

## ItemsView

- `ModernWpf.Controls.ItemsView` combines `ItemsRepeater` virtualization,
  `ItemContainer` interaction and automation, current-item tracking, item
  invocation, and `None`, `Single`, `Multiple`, and `Extended` selection.
- The public surface includes item source/template/layout, selection state and
  methods, viewport lookup, `StartBringItemIntoView`, vertical scroll-controller
  integration, `ItemTransitionProvider`, and an `ISelectionProvider` automation
  peer.
- The default template uses `PART_ScrollView`, `PART_ItemsRepeater`, and
  `StackLayout`. Applications can switch to `UniformGridLayout` or
  `LinedFlowLayout` without replacing the control.
- Pointer, double-click, Enter, Space, spatial arrow/Home/End/Page navigation,
  and `Ctrl+A` in the two multiselect modes have focused WPF-hosted coverage.

## WPF adaptations

- WinUI `ScrollView` maps to the public WPF `ScrollViewer` returned by
  `ItemsView.ScrollView`; no incomplete `ScrollView` or `ScrollPresenter` type
  is published.
- `ModernWpf.BringIntoViewOptions` carries source-shaped alignment, offset,
  target rectangle, and animation intent into the Preview 6 scroll bridge.
- `ItemTemplate` accepts WPF data templates/selectors and the existing
  `IElementFactory` abstraction. Realized roots must be `ItemContainer` so
  selection, invocation, and automation stay coherent.
- WPF `Control` has no inherited `CornerRadius`, so ItemsView declares the
  compatible dependency property used by its ScrollViewer-backed template.
- WPF focus navigation, native scrolling, render transforms, and opacity
  clocks replace WinUI XYFocus/gamepad, InteractionTracker, and compositor
  services.

## Layouts and transitions

`ItemTransitionProvider` is forwarded to the internal `ItemsRepeater`. The
Preview 6 `LinedFlowLayoutItemCollectionTransitionProvider` can therefore
animate collection and layout changes directly inside ItemsView while
respecting the WPF system animation setting. Selection and current-item state
remain synchronized across collection changes and layout switches.

## Gallery and validation

- The Gallery adds basic selection/invocation, live layout switching with
  add/remove transitions, and selection/invocation-mode examples.
- Focused product tests cover API defaults, templates, realization,
  collection changes, selection, invocation, keyboard navigation, automation,
  viewport lookup, bring-into-view, scrolling-cache timing, and transition
  forwarding.
- Theme and Gallery coverage runs on .NET 8 and .NET 10. Package and executable
  consumer validation covers `net462`, `net8.0-windows7.0`, and
  `net10.0-windows7.0` with both resource entries.
- Publication still requires the complete serialized gate, three downstream
  canaries, and final-tip Light, Dark, and real OS High Contrast Gallery
  evidence on all three targets.

Exact source pins and substitutions are recorded in the
[ItemsView WinUI 3 source audit](itemsview-winui3-source-audit.md), with its
foundations in the [ItemContainer audit](itemcontainer-winui3-source-audit.md),
[LinedFlowLayout audit](linedflowlayout-winui3-source-audit.md), and
[scrolling adaptation](itemsview-scrolling-wpf-adaptation.md).

## Breaking changes and migration

Preview 7 makes no intentional breaking change to the Preview 6 CLR API or
public resource-key surface. Existing Preview 6 applications require no
source migration beyond updating their package version. ItemsView and
`ModernWpf.BringIntoViewOptions` are additive preview APIs, and Preview 7 adds
no ItemsView-specific public resource keys.

## Known preview limitations

- The public CLR API and resource-key inventories do not freeze until the
  release candidate. Any accepted pre-RC correction still requires an audit,
  inventory update, tests, and migration guidance.
- Native WinUI compositor, InteractionTracker, gamepad, private test-hook, and
  telemetry internals are represented by the documented WPF adaptations, not
  by new public compatibility facades.
- `PipsPager` remains deferred to 1.1.
