# ModernWPF 1.0.0-preview.4

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.4` adds the source-audited WinUI-shaped `TitleBar` control and
WPF-native Mica and Desktop Acrylic window materials. This is the fourth
milestone in the fixed ModernWPF 1.0 roadmap.

## Preview compatibility

- `1.0.0-preview.3` becomes the active package-validation baseline for this
  development cycle after Preview 3 is published.
- `1.0.0-preview.1` remains the immutable historical audit and migration
  baseline rather than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the compatibility and
migration policies.

## TitleBar

- `ModernWpf.Controls.TitleBar` carries the current WinUI title, subtitle,
  icon, left/content/right slots, back and pane-toggle events and state,
  template settings, nullable drag-region override, automatic refresh, and
  explicit recompute surface.
- It is distinct from the retained `WindowTitleBar` attached-property facade
  and `WindowTitleBarControl` shell chrome. Existing applications do not need
  to rename or replace either WPF window-shell type.
- The 32/48-DIP template includes compact layout, activation states,
  localized automation names and tooltips, keyboard focus visuals, Light,
  Dark, and High Contrast resources, and 76 recorded public resource keys.

## Window materials

- `WindowBackdrop.Kind` opts a WPF `Window` into `Mica` or
  `DesktopAcrylic`; `FallbackBrush` supplies the solid fallback and the
  read-only `EffectiveKind` reports what is actually active.
- Native material requires Windows 11 22H2 or newer, DWM composition, a
  non-layered window, and compatible full-glass or adapter-owned chrome.
  Older systems, High Contrast, disabled composition, partial custom glass,
  layered windows, and native failures use the theme-aware fallback.
- The adapter preserves application-owned background, composition-target,
  and frame state and reacts to system theme/composition changes without
  retaining closed windows through process-wide events.

## WPF adaptations and Gallery

- WPF has no WinUI `InputNonClientPointerSource`, `XamlRoot`, or rectangular
  drag-region API. TitleBar therefore classifies the live WPF visual tree at
  input time and bridges dragging and maximize/restore through `WindowChrome`
  and `Window.DragMove`.
- The Gallery replaces its old hand-built title-bar picture with all three
  current source-shaped examples: configuration, drag regions, and the
  end-to-end navigation/pane sample.
- The SystemBackdrop page opens real Mica and Desktop Acrylic windows, reports
  requested versus effective material, keeps one material window active, and
  updates its status when the effective result changes.
- Focused API, template, input, automation, chrome, fallback, source-audit,
  Gallery, theme, and public-contract tests accompany the new surfaces. The
  complete release gate and final-tip visual/manual matrix remain required
  before this draft is published.

## Upstream cutoff

The finite Preview 4 source boundary is documented in the
[2026-08-08 synchronization disposition](winui3-sync-2026-08-08-preview4.md).
The product head moves one commit beyond Preview 3; all four changed paths are
TableView sample files and are non-applicable to the ModernWPF 1.0 roadmap.
Stable WinUI and WinUI Gallery did not move.

Detailed implementation decisions and immutable source pins are in the
[TitleBar audit](titlebar-winui3-gallery-parity.md) and
[window-backdrop audit](window-backdrop-wpf-source-audit.md).

## Breaking changes and migration

Preview 4 makes no intentional breaking change to the Preview 3 CLR API or
existing public resource-key surface. Existing Preview 3 applications require
no source migration beyond updating their package version. TitleBar,
WindowBackdrop, their APIs, and the TitleBar resource keys are additive
preview surfaces and may be adopted independently.

## Known preview limitations

- Until Preview 3 is published, the development version and package baseline
  cannot advance to Preview 4; this draft must not be used for publication.
- `TabView`, `ItemContainer`, `LinedFlowLayout`, and `ItemsView` remain
  assigned to later 1.0 previews.
- `PipsPager` remains deferred to 1.1.
