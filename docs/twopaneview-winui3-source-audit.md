# TwoPaneView WinUI 3 Source Audit

Date: 2026-08-08

ModernWpf `TwoPaneView` is a source-backed WPF adaptation of official
`microsoft-ui-xaml` `winui3/main` commit
`6a556bb28fc227acd2ec8fe67ee64853f559084b` (2026-08-08). The Gallery
boundary is WinUI Gallery commit
`3669519356c67f1376152c33ed8ea45003a91f3a` (2026-08-06). The official
Gallery tree has no current `TwoPaneView` page, so the ModernWpf Gallery page
is explicitly product-source-backed rather than presented as an official
Gallery port.

## Product source baseline

| Source | Current blob |
| --- | --- |
| `controls/dev/TwoPaneView/TwoPaneView.idl` | `b609b7af6c0bf707e9428c415f3b5422d2997808` |
| `controls/dev/TwoPaneView/TwoPaneView.cpp` | `609166c6a1537b2bb213c10ba3f47c2bf7dbda79` |
| `controls/dev/TwoPaneView/TwoPaneView.h` | `9c81354814cc82d10c10e82ad95c95fb090ca7e8` |
| `controls/dev/TwoPaneView/TwoPaneView.xaml` | `c7821c2691543cf5854ae4306c24ecf1345fc1a6` |
| `controls/dev/Generated/TwoPaneView.properties.cpp` | `24171b6c8443ac7af71c4e98728b3f33b62ce234` |
| `controls/dev/Generated/TwoPaneView.properties.h` | `21e76072e07d47e75b660e0f1f3eeed4878b06e2` |
| `controls/dev/TwoPaneView/DisplayRegionHelper.cpp` | `1b4fcfeb7fab71e5ed9c10a16c1f50e0e9b57ed2` |
| `controls/dev/TwoPaneView/DisplayRegionHelper.h` | `75abbb3614e260a75c23a968088aec170423516e` |
| `controls/dev/TwoPaneView/APITests/TwoPaneViewTests.cs` | `916b6b9052141395a4f8e9c0e3e7863fa53d6d64` |
| `controls/dev/TwoPaneView/InteractionTests/TwoPaneViewTests.cs` | `d87a0b003405753e75734b1e6b0fdbc679430906` |
| `controls/dev/TwoPaneView/TestUI/TwoPaneViewPage.xaml` | `f6169a907f5baf14c514e1f0a66874225249199e` |
| `controls/dev/TwoPaneView/TestUI/TwoPaneViewPage.xaml.cs` | `d8e0dfe6ed4f7fad84237ab32b7eb185d7646795` |

## Public and behavioral contract

- `Pane1` and `Pane2` are `UIElement` dependency properties.
- `Pane1Length` defaults to `Auto`; `Pane2Length` defaults to `1*`.
- `PanePriority` defaults to `Pane1`; `WideModeConfiguration` defaults to
  `LeftRight`; `TallModeConfiguration` defaults to `TopBottom`.
- Read-only `Mode` starts at `SinglePane`. `MinWideModeWidth` and
  `MinTallModeHeight` both default to 641 and clamp negative or NaN input to 0.
- Width is tested before height and uses the source strict-greater-than
  threshold. Configuration can force single-pane behavior independently for
  wide and tall layouts.
- The six internal layout modes map to source visual states
  `ViewMode_OneOnly`, `ViewMode_TwoOnly`, `ViewMode_LeftRight`,
  `ViewMode_RightLeft`, `ViewMode_TopBottom`, and `ViewMode_BottomTop`.
- Row and column lengths refresh even when the public `Mode` does not change.
  `ModeChanged` fires only when `SinglePane`, `Wide`, or `Tall` changes and,
  matching current source, passes the view as both sender and event argument.

The public enums remain source-shaped: `TwoPaneViewPriority`,
`TwoPaneViewMode`, `TwoPaneViewWideModeConfiguration`, and
`TwoPaneViewTallModeConfiguration`, with their current names and numeric
values.

## WPF adaptations

- WPF has no `ApplicationView`, `XamlRoot` display-region API, or portable
  hinge/occlusion contract. ModernWpf therefore applies the official
  single-region width/height threshold path and does not invent fake spanning
  regions, middle-gap values, or device APIs.
- WinUI's display-region-specific pixel column/row calculations are documented
  but intentionally omitted. A future WPF platform capability can add an
  internal adapter without changing the Preview 3 public contract.
- `ScrollContentPresenter.SizesContentToTemplatedParent` is a WinUI-only
  implementation detail. WPF `ScrollViewer` measurement supplies the matching
  content constraint without a public substitute.
- Current WinUI visual-state setters are represented through
  `VisualStateEx.Setters`; ordinary WPF `GridLength`, `ScrollViewer`, and
  `ContentPresenter` types retain the source part/state tree.

## Gallery contract

Because the pinned Gallery has no `WinUIGallery/Samples/TwoPaneView` tree,
ModernWpf labels its page as a WPF adaptation. The page demonstrates the
source defaults and exposes width/height, pane priority, wide/tall
configuration, and live `Mode` output. It does not claim dual-screen hinge
support.

## Regression guards

- `TwoPaneViewSourceAuditTests` pins the immutable product/Gallery revisions,
  source blobs, public enum/DP/event shape, state names, and the no-fake-hinge
  adaptation.
- `TwoPaneViewApiTests` covers defaults, setters/clamping, strict thresholds,
  pane order, row/column lengths, single-pane priority, and public-mode event
  transitions.
- Gallery tests cover page metadata, options, live mode output, resize-driven
  wide/tall/single behavior, automation anchors, and theme rendering.

## Current validation

The focused API/layout and source-audit slice passes 5/5 on
`net8.0-windows7.0`. The complete Gallery suite passes 727/727 on both Gallery
targets, and the serialized Release solution build succeeds for all supported
package targets with zero warnings or errors. Final Preview 3 acceptance still
requires the hosted complete WinUI suite three consecutive times, package
verification and consumers, downstream canaries, and final-tip Light, Dark,
and real OS High Contrast Gallery evidence.
