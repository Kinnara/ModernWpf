# ModernWPF 1.0.0-preview.3

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.3` is under development. This draft tracks the source-audited
`TimePicker` and `TwoPaneView` milestone in the fixed ModernWPF 1.0 roadmap.

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

## Planned milestone

- Add source-audited `TimePicker` and `TwoPaneView` controls using current
  applicable WinUI API shape, with documented WPF adaptations.
- Add Gallery pages, focused behavior and automation coverage, Light, Dark,
  High Contrast, and compact-resource validation, and public API and resource
  inventories for both controls.
- Run the complete release, package, consumer, and downstream compatibility
  gates before publication.

## Breaking changes

No Preview 3 breaking changes have been finalized. Any intentional preview
change will update this section with explicit migration guidance before the
draft marker is removed.

## Known preview limitations

- This draft does not represent a published package.
- `TitleBar`, window materials, `TabView`, `ItemContainer`,
  `LinedFlowLayout`, and `ItemsView` remain assigned to later 1.0 previews.
- `PipsPager` remains deferred to 1.1.
