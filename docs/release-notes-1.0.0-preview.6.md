# ModernWPF 1.0.0-preview.6

`1.0.0-preview.6` adds the current-WinUI-shaped `ItemContainer`,
`LinedFlowLayout`, item-collection transition family, and the adapted scrolling
prerequisites needed by Preview 7 `ItemsView`. This is the sixth milestone in
the fixed ModernWPF 1.0 roadmap.

## Preview compatibility

- `1.0.0-preview.5` is the active package-validation baseline for this
  development cycle.
- `1.0.0-preview.1` remains the immutable historical audit and migration
  baseline rather than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the compatibility and
migration policies.

## ItemContainer

- `ModernWpf.Controls.ItemContainer` is an unsealed content control with the
  current public `Child` and `IsSelected` dependency properties and a public
  `ItemContainerAutomationPeer`.
- The template includes selected, pointer-over, pressed, disabled,
  multi-select, focus, and arbitrary-child states. Its semantic resources are
  available through Light, Dark, real OS High Contrast, and compact-resource
  entries.
- Pointer, double-click, Enter, Space, SelectionItem, and conditional Invoke
  paths follow the source behavior used by ItemsView while leaving collection
  selection policy to the owning control.
- WPF `Control` has no inherited `CornerRadius`, so the port explicitly owns
  WPF's compatible `Border.CornerRadiusProperty` and binds it through every
  rounded template layer.

## LinedFlowLayout and item transitions

- `LinedFlowLayout` exposes the current justification/stretch enums, line and
  item spacing, explicit or measured line height, requested item-information
  range, per-item aspect/min/max width data, line locks, and unlock events.
- Its WPF implementation builds a deterministic metadata line plan while
  realizing and recycling only the viewport, cache, and recommended anchor.
  Deep scrolling and unbounded non-scrolling hosts are covered explicitly.
- `ItemsRepeater.ItemTransitionProvider`,
  `Layout.CreateDefaultItemTransitionProvider`, the complete public
  `ItemCollectionTransition` family, and
  `LinedFlowLayoutItemCollectionTransitionProvider` are included.
- WPF render-transform/opacity clocks replace WinUI Composition animations.
  The provider respects the system client-area animation setting, completes
  interrupted transitions exactly once, restores application transforms, and
  has a bounded completion fallback when rendering stalls.

## Adapted scrolling prerequisite

WPF has no portable equivalent of WinUI's InteractionTracker-backed
`ScrollPresenter`. Preview 6 therefore adds an internal ScrollViewer bridge
that supplies offsets, ranges, controller requests, correlation completion,
velocity adaptation, and exact native-scrollbar restoration. It works inside
the existing virtualizing `ItemsRepeaterScrollHost`; it does not freeze an
incomplete public `ScrollView` or `ScrollPresenter` API.

## Gallery and validation

- The Gallery adds real ItemContainer and LinedFlowLayout foundation samples
  with selection, enabled/corner-radius controls, all justification and
  stretch modes, live add/remove transitions, requested-range diagnostics,
  and deep scrolling.
- Focused product tests cover public API defaults, XAML, input, automation,
  event-argument validation, realization/recycling, line locks, all
  justification modes, transitions, and the scrolling bridge.
- Theme and Gallery tests run on .NET 8 and .NET 10; package and executable
  consumer validation cover all three supported targets and both resource
  entries. Publication still requires the complete release gate, downstream
  canaries, and final-tip Light, Dark, and real OS High Contrast Gallery
  evidence.

Detailed source pins and WPF substitutions are recorded in the
[ItemContainer source audit](itemcontainer-winui3-source-audit.md),
[LinedFlowLayout source audit](linedflowlayout-winui3-source-audit.md), and
[ItemsView scrolling adaptation](itemsview-scrolling-wpf-adaptation.md).

## Breaking changes and migration

Preview 6 makes no intentional breaking change to the Preview 5 CLR API or
existing public resource-key surface. Existing Preview 5 applications require
no source migration beyond updating their package version. ItemContainer,
LinedFlowLayout, item transitions, and their new public resource keys are
additive preview surfaces. They do not replace stock WPF item containers,
panels, or scrolling controls.

## Known preview limitations

- The complete `ItemsView` control family and its end-to-end selection,
  invocation, keyboard, automation, layout, and Gallery surface remain assigned
  to Preview 7.
- `PipsPager` remains deferred to 1.1.
