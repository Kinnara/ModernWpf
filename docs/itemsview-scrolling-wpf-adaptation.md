# ItemsView scrolling prerequisites: WPF adaptation

Date: 2026-08-09

Preview 6 establishes the scrolling boundary required by Preview 7 ItemsView.
The goal is to preserve observable scrolling and controller behavior without
publishing a partial imitation of WinUI's compositor-only `ScrollView` and
`ScrollPresenter` families.

## Source boundary

The product authority is microsoft-ui-xaml `winui3/main`
`e1aa8f64df98d6229f6cd4074d59b654616254da`. The relevant public designs are:

- `controls/dev/ScrollView/ScrollView.idl`, blob
  `2848163f939b4725ac7b9279b5b228622b1db39a`; and
- `docs/design-notes/ItemsView_spec.md`, blob
  `541dc3aecaa4a2bde243c550d5bc2b63ea0f0b33`.

ModernWPF already ships WinUI-shaped `IScrollController`, scrolling options,
request event arguments, correlation IDs, and panning-information contracts in
`ModernWpf.Controls.Primitives`. It also ships `ItemsRepeaterScrollHost`, whose
WPF scrolling-surface implementation supplies ItemsRepeater visible and
realization windows, anchoring, and recycling.

## Accepted WPF substitution

WPF has no InteractionTracker, CompositionAnimation, expression-animation
property set, or portable content-island scroll presenter. Publishing public
`ScrollPresenter`/`ScrollView` shells that silently lack those contracts would
create a larger, misleading API and make the eventual stable boundary harder
to correct.

Preview 6 therefore adds only an internal `ItemsViewScrollHost : ScrollViewer`:

- it reports minimum, maximum, offset, viewport, and scrollability to an
  external vertical `IScrollController`;
- it accepts absolute, relative, and velocity requests, assigns correlation
  IDs, clamps WPF offsets, and completes requests after the dispatcher applies
  the change;
- it hides the native vertical bar only while an external controller is
  attached and restores the exact prior visibility when detached; and
- it composes inside the existing public `ItemsRepeaterScrollHost`, so the
  repeater retains real visible-window virtualization rather than measuring
  every item.

WPF has no compositor velocity transaction. The velocity request is adapted
to one 60 Hz display-frame delta and completed deterministically. The panning
information and animation-object hooks remain controller contracts but are not
fabricated by the WPF host.

## Preview 7 public boundary

Preview 7 will expose the ItemsView scroll host as
`System.Windows.Controls.ScrollViewer` from its source-named `ScrollView`
property. That is an explicit WPF type substitution, not a new ModernWPF
`ScrollView` class. `VerticalScrollController` continues to use the already
public `IScrollController` contract. The ItemsView source audit must list any
members whose types depend on unavailable WinUI composition primitives and
their exact WPF disposition.

Focused Preview 6 tests prove values, scrollability, all three request paths,
correlation completion, clamping, detach behavior, and native-scrollbar
restoration. Preview 7 adds the end-to-end template, selection, keyboard,
bring-into-view, and external-controller tests.
