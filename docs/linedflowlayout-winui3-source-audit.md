# LinedFlowLayout current WinUI 3 source audit

Date: 2026-08-10

ModernWPF 1.0.0-preview.6 adds the current public `LinedFlowLayout` and item
collection transition family on the existing ItemsRepeater virtualization
stack. These are the variable-width equal-line-height layout and transition
prerequisites for Preview 7 ItemsView.

## Pinned source interval

| Source | Commit | Use |
| --- | --- | --- |
| microsoft-ui-xaml `winui3/main` | `23a73be03d194ea0ece97da71de98b6b53021b70` | Current product, IDL, generated properties, algorithm, transitions, tests, TestUI, and design cutoff. |
| microsoft-ui-xaml `winui3/release/2.3.1` | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Stable reconciliation; current main remains authoritative for the newer transition and layout work. |
| WinUI Gallery `main` | `b78c440193aab788215888561e45adf72da848cb` | Gallery cutoff. It has no standalone LinedFlowLayout page; the current ItemsView page and product TestUI are the sample authority. |

Exact principal inputs:

| Path | Blob |
| --- | --- |
| `controls/dev/Generated/LinedFlowLayout.properties.cpp` | `26831834ca41272d6899f9435c65580a8eecf6b1` |
| `controls/dev/Generated/LinedFlowLayout.properties.h` | `f43fca1d79376c47e199326aa6eea3e44c4337d4` |
| `controls/dev/Repeater/ItemsRepeater.idl` | `22e70d759e8b93cf2581515f1efdb1d91f183bff` |
| `controls/dev/Repeater/ItemsRepeater.cpp` | `792c35888d49fbfe5b27d97ef4206f71103dcc9e` |
| `controls/dev/Repeater/LinedFlowLayout.cpp` | `862ec26d51942f7f47c4290ab21638af079c4129` |
| `controls/dev/Repeater/LinedFlowLayout.h` | `918aab45899e8597eab97781136a4a1ca22f714e` |
| `controls/dev/Repeater/LinedFlowLayoutItemsInfoRequestedEventArgs.cpp` | `576c94369d51a5afccb355d1151b86acf3b4ea40` |
| `controls/dev/Repeater/LinedFlowLayoutItemCollectionTransitionProvider.cpp` | `efc5a62df9fcb25805215bdd56cee76a2b060cff` |
| `controls/dev/Repeater/ItemCollectionTransition.cpp` | `efb386575ae3f3252c6319fe1dc5a632aca9d775` |
| `controls/dev/Repeater/ItemCollectionTransitionProgress.cpp` | `24e5074153db9bd881059978b0ee213ddec1f2ee` |
| `controls/dev/Repeater/ItemCollectionTransitionProvider.cpp` | `f43211eb47a71506245e98f84895503c72070599` |
| `controls/dev/Generated/ItemCollectionTransitionProvider.properties.h` | `da4f149dd9484c90baca113391cd1da4308ad072` |
| `controls/dev/Repeater/APITests/LinedFlowLayoutTests.cs` | `13b362d15338fe8d1fc314a31c91784298ba2099` |
| `controls/dev/Repeater/APITests/ItemCollectionTransitionProviderTests.cs` | `38a2c32d216ed617eebe8b792bd95fba66a6abb9` |
| `controls/dev/Repeater/TestUI/Samples/LinedFlowLayoutDemo.xaml` | `21c8673d8813166f68377349fc4b17448f74c818` |
| `controls/dev/Repeater/TestUI/Samples/LinedFlowLayoutDemo.cs` | `edabe967fdb67090a1f627b927aa642dd0a283af` |
| `docs/design-notes/LinedFlowLayout_spec.md` | `cf8659f59e35c680aed5ed6f0e66061429304645` |
| `docs/design-notes/ItemCollectionTransitionProvider-spec.md` | `fa4d46511b7d6bfbe150bc8eefef1fccd06aa8bb` |

## Public surface

The WPF projection retains the unsealed layout type, all six justification
values, `None`/`Fill` stretching, spacing and line-height dependency
properties, read-only actual line height and requested-range values,
`ItemsInfoRequested`, `ItemsUnlocked`, `InvalidateItemsInfo`, and
`LockItemToLine`.

The event arguments retain the mutable range start, requested length, uniform
and per-item width bounds, and desired-aspect-ratio arrays. Supplied arrays are
cloned, must have equal lengths, and must cover the requested range.

Current WinUI also exposes `ItemTransitionProvider` on ItemsRepeater,
`Layout.CreateDefaultItemTransitionProvider`, the complete
`ItemCollectionTransition`/progress/provider/completed-arguments family, and
`LinedFlowLayoutItemCollectionTransitionProvider`. Preview 6 includes that
surface; it is not deferred or represented by inert placeholders.

## WPF layout adaptation

WinUI's implementation combines compositor-era fast and regular paths,
progressive sizing timers, native trace hooks, and a private aspect-ratio
store. Those internals cannot be copied literally to retained WPF targets.
ModernWPF builds a deterministic line plan from the requested item information
and cached measured ratios, walks metadata for the collection, and realizes
only the viewport plus cache/anchor lines through the existing
VirtualizingLayoutContext. `double.MaxValue`, used by ItemsRepeater for an
unbounded non-scrolling realization window, is handled explicitly.

The item-information request is centered on the estimated realization range
with a 32-item buffer on each side; an unbounded host requests the complete
collection. Missing ratios use a measured rolling average. Fill distributes
remaining line width while respecting per-item maxima, and arrange implements
all source justification values. Collection changes recycle/replan; deep
scrolling releases old elements and realizes the new viewport.

Line locks retain their source line while the effective unlocked
items-per-line average is unchanged. The layout computes that unlocked value
before enforcing locks, clears locks and raises `ItemsUnlocked` when it changes,
and also clears locks for source collection changes. `InvalidateItemsInfo`
alone retains locks; changed information unlocks only if it changes the
effective average.

## WPF transition adaptation

WinUI animates Composition scale and translation. WPF uses
`RenderTransform`, opacity, easing, and `SystemParameters.ClientAreaAnimation`.
The source ordering is retained: removals precede moves; adds follow both;
initial-layout moves are skipped; reset removals are skipped when reset adds
replace them; same-line moves translate; cross-line moves scale through the
midpoint; collection adds/removes scale; and every started or skipped
transition completes exactly once. WPF clocks are owned per element: a new
transition completes and restores an interrupted predecessor, and a bounded
dispatcher fallback completes a clock if rendering never reports completion.
An explicit provider takes precedence over the layout default. The older
ElementAnimator path remains as a compatibility fallback only when no
item-transition provider is active.

## Acceptance coverage

Focused tests cover public defaults and XAML, range validation and cloned
arrays, aspect/min/max width information, every justification value, Fill,
line locking/unlocking, unbounded hosts, deep-scroll realization/recycling,
provider queuing and completion, system animation policy, default-provider
selection, and source skip rules. The Gallery page adds live justification,
stretch, line height, collection add/remove transitions, requested-range
status, and deep scrolling.
