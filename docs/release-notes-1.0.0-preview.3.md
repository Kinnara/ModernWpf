# ModernWPF 1.0.0-preview.3

`1.0.0-preview.3` adds source-audited `TimePicker` and `TwoPaneView` controls
and carries the applicable WinUI accessibility change for the visible
`NumberBox` placeholder. This is the third milestone in the fixed ModernWPF
1.0 roadmap.

## Preview compatibility

- `1.0.0-preview.2` is the active package-validation baseline for this
  development cycle.
- `1.0.0-preview.1` remains the immutable historical audit and migration
  baseline rather than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the compatibility and
migration policies.

## New controls

- `TimePicker` includes the current WinUI-shaped dependency properties,
  nullable `SelectedTime`, `Time` and `SelectedTime` change events,
  culture-aware automation values, resource-backed labels, 12/24-hour culture
  ordering, minute increments, and a WPF popup selector with explicit accept
  and dismiss behavior.
- `TwoPaneView` includes the current pane, length, priority, configuration,
  threshold, mode, and mode-change surface. Its width-first layout follows the
  source strict threshold and all six source visual states.
- Both controls are recorded in the CLR inventory, generic-resource coverage,
  upstream drift manifest, and dedicated source audits:
  [TimePicker](timepicker-winui3-source-audit.md) and
  [TwoPaneView](twopaneview-winui3-source-audit.md).

## WPF adaptations

- `TimePicker` uses a WPF `Popup` and finite `ListBox` selectors in place of
  WinUI's asynchronous flyout, `LoopingSelector`, and phone presenter stack.
  `LightDismissOverlayMode` is retained for API parity, but WPF does not create
  a synthetic window-wide overlay.
- WPF has no portable `XamlRoot` display-region or hinge contract.
  `TwoPaneView` therefore uses WinUI's official single-region width/height
  path and does not expose a guessed spanning API or artificial middle gap.

## Accessibility and Gallery

- A visible `NumberBox` placeholder now participates in the UI Automation
  Control view, matching the applicable post-Preview-2 WinUI accessibility
  change without adding public API.
- The Gallery adds the three pinned official `TimePicker` examples and a
  source-shaped WPF `TwoPaneView` adaptation with live mode output and controls
  for pane priority, wide/tall configuration, thresholds, and size.
- Focused API, layout, input, automation, source-audit, catalog, and theme
  coverage accompanies the new surfaces. The complete release gate remains
  required before publication.

## Upstream cutoff

The finite Preview 3 source boundary is documented in the
[2026-08-08 synchronization disposition](winui3-sync-2026-08-08-preview3.md).
It classifies all 35 product paths after the Preview 2 cutoff. Stable WinUI and
WinUI Gallery did not move. The only applicable existing-control change is the
`NumberBox` placeholder accessibility update described above.

## Breaking changes and migration

Preview 3 makes no intentional breaking change to the Preview 2 CLR API or
existing public resource-key surface. Existing Preview 2 applications require
no source migration beyond updating their package version. `TimePicker`,
`TwoPaneView`, their APIs, and the two current TimePicker foreground resource
keys are additive preview surfaces and may be adopted independently.

## Known preview limitations

- This preview remains unpublished until the tagged Trusted Publishing
  workflow completes.
- `TitleBar`, window materials, `TabView`, `ItemContainer`,
  `LinedFlowLayout`, and `ItemsView` remain assigned to later 1.0 previews.
- `PipsPager` remains deferred to 1.1.
