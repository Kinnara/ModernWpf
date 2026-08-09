# ModernWPF 1.0.0-preview.6

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.6` adds the source-audited `ItemContainer` and
`LinedFlowLayout` foundations plus the WPF-native scrolling bridge needed by
Preview 7 `ItemsView`. This is the sixth milestone in the fixed ModernWPF 1.0
roadmap.

## Preview compatibility

- The latest published preview becomes the active package-validation baseline
  when this development branch is rebased for release.
- `1.0.0-preview.1` remains the historical audit and migration baseline rather
  than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md).

## New controls and prerequisites

- `ItemContainer` provides arbitrary child content, selected state, current
  WinUI item chrome, focus/pointer/pressed states, and SelectionItem/Invoke
  automation semantics.
- `LinedFlowLayout` provides equal-height, variable-width virtualized lines;
  aspect-ratio and width information requests; all six justification modes;
  Fill stretch; line locking; and item-info invalidation.
- The internal ItemsView scroll host connects the existing public
  `IScrollController` contract to WPF `ScrollViewer` offsets and composes with
  `ItemsRepeaterScrollHost` realization and recycling.

The pinned source and WPF adaptations are documented in the
[ItemContainer audit](itemcontainer-winui3-source-audit.md),
[LinedFlowLayout audit](linedflowlayout-winui3-source-audit.md), and
[scrolling adaptation](itemsview-scrolling-wpf-adaptation.md).

## Gallery, accessibility, and validation

- New Gallery pages use the real ItemContainer and LinedFlowLayout controls,
  including live selection, corner-radius, justification, stretch, and line
  height options.
- Focused tests cover public API shape, XAML, selected and multi-select
  visuals, automation patterns, requested item information, exact layout,
  virtualization/recycling, lock invalidation, and scroll-controller request
  completion.
- Light, Dark, High Contrast, compact resources, CLR/resource inventories,
  all supported package targets, and the complete serialized release gate
  remain required before publication.

## Breaking changes and migration

Preview 6 plans no intentional break to the latest published preview's CLR or
public resource-key surface. ItemContainer, LinedFlowLayout, their automation
peer, and their theme resources are additive preview surfaces. Applications
can adopt them independently; complete ItemsView selection and navigation
arrive in Preview 7.

## Known preview limitations

- Until the draft marker is removed and the tagged workflow completes, this
  file describes an unpublished development package.
- LinedFlowLayout does not expose WinUI compositor transition providers.
- ModernWPF does not publish partial `ScrollView` or `ScrollPresenter` shells;
  Preview 7 documents the WPF `ScrollViewer` substitution on ItemsView.
- `ItemsView` remains assigned to Preview 7, and `PipsPager` remains deferred
  to 1.1.
