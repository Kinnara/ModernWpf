# LinedFlowLayout current WinUI 3 source audit

Date: 2026-08-09

ModernWPF Preview 6 adds the current WinUI-shaped `LinedFlowLayout` on top of
the already shipped ItemsRepeater virtualization and recycling stack. It is
the variable-width, equal-line-height layout required by Preview 7 ItemsView.

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml `winui3/main` | `e1aa8f64df98d6229f6cd4074d59b654616254da` | Product, IDL, algorithm, generated properties, API tests, TestUI, and design-spec cutoff. |
| microsoft-ui-xaml `winui3/release/2.3.1` | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Stable reconciliation. Public IDL/API tests remain byte-identical; main is the implementation authority. |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | Gallery cutoff. It contains no standalone LinedFlowLayout page, so the source TestUI demo and design specification are the sample authority. |

The exact upstream inputs are:

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls/dev/Generated/LinedFlowLayout.properties.cpp` | `26831834ca41272d6899f9435c65580a8eecf6b1` |
| `controls/dev/Generated/LinedFlowLayout.properties.h` | `f43fca1d79376c47e199326aa6eea3e44c4337d4` |
| `controls/dev/Repeater/LinedFlowLayout.cpp` | `862ec26d51942f7f47c4290ab21638af079c4129` |
| `controls/dev/Repeater/LinedFlowLayout.h` | `918aab45899e8597eab97781136a4a1ca22f714e` |
| `controls/dev/Repeater/LinedFlowLayoutItemAspectRatios.cpp` | `0392fac7ed47e620a5b70ee7e86bf5c7d54a3526` |
| `controls/dev/Repeater/LinedFlowLayoutItemAspectRatios.h` | `12c309349bd7f642d9166d74d50ef9a3e9107858` |
| `controls/dev/Repeater/LinedFlowLayoutItemsInfoRequestedEventArgs.cpp` | `576c94369d51a5afccb355d1151b86acf3b4ea40` |
| `controls/dev/Repeater/LinedFlowLayoutItemsInfoRequestedEventArgs.h` | `324eeb0069cf87d58f3f43164311f5ad5c1d89e8` |
| `controls/dev/Generated/LinedFlowLayoutItemCollectionTransitionProvider.properties.cpp` | `7623be07b187d9f193543bdb4f895237a1ef39cc` |
| `controls/dev/Repeater/LinedFlowLayoutItemCollectionTransitionProvider.cpp` | `efc5a62df9fcb25805215bdd56cee76a2b060cff` |
| `controls/dev/Repeater/LinedFlowLayoutItemCollectionTransitionProvider.h` | `deedec1c9f2842bd94faf4f7cb169da6eff7f8b4` |
| `controls/dev/Repeater/LayoutsTestHooksLinedFlowLayoutInvalidatedEventArgs.cpp` | `f7f3f13a447966b1c3cdfd30aa60725bc39d6dbe` |
| `controls/dev/Repeater/LayoutsTestHooksLinedFlowLayoutInvalidatedEventArgs.h` | `622916a6b6d12f0da14a4ad499e7159b5ac6e1a7` |
| `controls/dev/Repeater/LayoutsTestHooksLinedFlowLayoutItemLockedEventArgs.cpp` | `f6aec18ccad24e312be83dcbe867f5927ba403d4` |
| `controls/dev/Repeater/LayoutsTestHooksLinedFlowLayoutItemLockedEventArgs.h` | `b7dd50dc00c06e1f574b54e445bab8c1ab2f7b58` |
| `controls/dev/Repeater/LinedFlowLayoutTrace.h` | `930be6ad5d726e8641194637529fc798aa4dc587` |
| `controls/dev/Repeater/APITests/LinedFlowLayoutTests.cs` | `13b362d15338fe8d1fc314a31c91784298ba2099` |
| `controls/dev/Repeater/TestUI/Samples/LinedFlowLayoutDemo.xaml` | `21c8673d8813166f68377349fc4b17448f74c818` |
| `controls/dev/Repeater/TestUI/Samples/LinedFlowLayoutDemo.cs` | `edabe967fdb67090a1f627b927aa642dd0a283af` |
| `docs/design-notes/LinedFlowLayout_spec.md` | `cf8659f59e35c680aed5ed6f0e66061429304645` |

The public declarations are in the pinned
`controls/dev/Repeater/ItemsRepeater.idl` blob
`22e70d759e8b93cf2581515f1efdb1d91f183bff`.

## Public surface

The WPF projection preserves the unsealed `VirtualizingLayout` type; the six
`ItemsJustification` values; `None`/`Fill` stretch values; dependency
properties for justification, stretch, minimum item spacing, line spacing,
line height, and read-only actual line height; requested range properties;
`ItemsInfoRequested` and `ItemsUnlocked`; `InvalidateItemsInfo`; and
`LockItemToLine`.

`LinedFlowLayoutItemsInfoRequestedEventArgs` preserves the mutable range start,
requested length, uniform minimum/maximum widths, and array setters for
desired aspect ratios and per-item minimum/maximum widths. It clones supplied
arrays and enforces the source rule that every array has the same length and
covers the requested range.

## WPF algorithm adaptation

The upstream C++ implementation has a compositor-era fast path, timer-driven
progressive sizing, transition-provider integration, and internal diagnostic
hooks. Those are not public behavior and cannot be copied literally onto WPF.
ModernWPF instead reuses its audited `FlowLayoutAlgorithm` and
`ElementManager`, which already provide the source-shaped anchor,
realization-window, recycling, collection-change, and line-alignment behavior
for every supported target.

Before each measure, LinedFlowLayout builds a deterministic line plan from
the supplied aspect ratios, line height, spacing, and per-item width bounds.
Missing hints fall back to measured aspect ratios. `Fill` distributes
remaining width while respecting per-item maxima. Arrange delegates all six
justification modes to `FlowLayoutAlgorithm`. The computed line table supplies
the exact vertical extent and target/viewport anchors without realizing every
item.

The requested item-info window covers roughly two viewports before and three
after the visible area, mirroring the source five-viewport data window while
retaining WPF's existing incremental cache growth. A supplied covering range
is reused until invalidated or the viewport leaves it. `LockItemToLine`
records the current line; property, data, or replanning changes clear locks
and raise `ItemsUnlocked` when the guarantee can no longer be retained.

The source's private `LinedFlowLayoutItemAspectRatios` store maps to the local
per-layout state dictionaries. Native trace and LayoutsTestHooks types remain
test diagnostics rather than package API. This adaptation deliberately omits
the upstream `ItemCollectionTransition` provider after auditing its generated
property, runtime, and header inputs: ModernWPF does not expose that
compositor-dependent family, and the Preview 6 requirement is realization,
recycling, selection prerequisites, layout, accessibility, and
supported-target behavior needed by ItemsView.

## Gallery and acceptance coverage

The Gallery page hosts the real layout in a real ItemsRepeater under
`ItemsRepeaterScrollHost` and WPF `ScrollViewer`. Eighty variable-aspect items
exercise requested-range data, Fill, all justification values, line-height
changes, vertical scrolling, and recycling.

Focused tests cover defaults and XAML parsing, argument coverage and cloning,
aspect-ratio sizing, spacing, exact line slots, Fill, every alignment mapping,
virtualization/recycling after deep scrolling, item locking/unlocking, and
collection invalidation. The same tests and package consumers are built for
`net462`, `net8.0-windows7.0`, and `net10.0-windows7.0` by the release gate.
