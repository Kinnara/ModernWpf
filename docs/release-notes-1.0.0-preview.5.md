# ModernWPF 1.0.0-preview.5

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.5` adds the complete current-WinUI-shaped `TabView` control
family. This is the fifth milestone in the fixed ModernWPF 1.0 roadmap.

## Preview compatibility

- `1.0.0-preview.4` is the active package-validation baseline for this
  development cycle.
- `1.0.0-preview.1` remains the immutable historical audit and migration
  baseline rather than an API freeze across later previews.
- Stable `1.0.0` will establish the SemVer compatibility boundary for
  subsequent 1.x releases.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the compatibility and
migration policies.

## TabView

- `ModernWpf.Controls.TabView`, `TabViewItem`, template settings, event
  arguments, and automation peers mirror the applicable current WinUI API
  shape while remaining separate from WPF's stock `TabControl` and `TabItem`.
- Explicit items and templated observable data sources support container
  lookup, application-owned selection/content/removal, header and footer
  content, add commands, and live collection notifications.
- Equal, content-sized, and compact tab widths are supported. Overflow buttons
  scroll the strip and bring the selected tab into view; close buttons support
  the current Auto, pointer-over, and Always modes.
- Ctrl+Tab, Ctrl+Shift+Tab, Ctrl+F4, arrow-key focus traversal, middle-click
  close, right-click without selection, localized button labels, and a
  connected required single-selection Tab/TabItem automation tree are covered
  by focused tests.
- Drag start/completion, mutable-source reorder, external drop, drop-outside,
  and source-shaped public resource keys are included across Light, Dark, High
  Contrast, and compact resources.

## WPF tear-out adaptation

WinUI's current tear-out implementation depends on `WindowId`, `AppWindow`,
content islands, and native move-size integration. ModernWPF instead asks the
application to create a `System.Windows.Window` and move its item through the
source-shaped tear-out and rejoin event sequence. The new window appears after
the WPF drag is released outside the source strip; rejoining is a subsequent
ordinary WPF tab drag. Application code retains ownership of window type,
view-model lifetime, and collection mutation.

## Gallery and validation

- The Gallery replaces the retired generated `TabControl` facsimile with all
  ten current source-facing examples using the real `TabView` control.
- Examples cover add/close/context moves, markup and bound items, keyboarding,
  strip header/footer, width and close-overlay modes, color icons, accent
  styling, overflow, and a real WPF window tear-out/rejoin flow.
- Product, automation, input, resource, source-audit, Gallery, and visual-route
  tests accompany the new control. The complete release gate and final-tip
  Light, Dark, and real OS High Contrast matrix on all supported targets remain
  required before this draft is published.

Detailed source pins and WPF substitutions are recorded in the
[TabView current-source audit](tabview-winui3-source-audit.md).

## Breaking changes and migration

Preview 5 makes no intentional breaking change to the Preview 4 CLR API or
existing public resource-key surface. Existing Preview 4 applications require
no source migration beyond updating their package version. `TabView`, its
related types, and its new public resource keys are additive preview surfaces.
Do not mechanically replace stock WPF `TabControl`; choose the new control only
where its close, overflow, reorder, or tear-out model is required.

## Known preview limitations

- Until this draft marker is removed, this file describes an unpublished
  development package.
- `ItemContainer`, `LinedFlowLayout`, and the adapted scrolling prerequisites
  remain assigned to Preview 6; `ItemsView` remains assigned to Preview 7.
- `PipsPager` remains deferred to 1.1.
