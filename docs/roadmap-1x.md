# ModernWpf 1.x Roadmap Draft

This is a public draft for the 1.x maintenance reboot. It intentionally starts small: restore a buildable, testable, packable package before large control rewrites.

## Direction

- Keep `ModernWpfUI` as the package name.
- Maintain `0.9.x` as a legacy/security-only line.
- Use `1.x` as the active maintenance line.
- Start the reboot with `1.0.0-preview.1`.
- Target `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`.

## Theme Strategy

- Preserve the existing `<ui:ThemeResources />` and `<ui:XamlControlsResources />` entry for compatibility.
- Add `<ui:FluentControlsResources UseCompactResources="False" />` as the recommended 1.x entry.
- Use the official WPF `PresentationFramework.Fluent` theme for stock WPF controls on `net10.0-windows7.0`.
- Use the ModernWpf Fluent backport for stock WPF controls on `net462` and `net8.0-windows7.0`.
- Bridge ModernWpf application/window theme preferences to official WPF `ThemeMode` on `net10.0-windows7.0`.
- Keep ModernWpf-specific WinUI-like controls and compatibility helpers in ModernWpf.
- Keep element-level `RequestedTheme` scopes implemented through ModernWpf resource dictionaries for WinUI-compatible theme islands.
- Do not carry the MahApps adapter into the 1.x line; the old `ModernWpfUI.MahApps` package remains part of the 0.9.x legacy line.

## First Milestone

- Restore reliable restore/build/test/pack flows.
- Ship package assets for `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`.
- Remove package assets for `net45`, `netcoreapp3.0`, and `net5.0-windows`.
- Revalidate small pending fixes before larger control work.

## Initial Fix Candidates

- Revalidate #508, neutral Chinese resource cultures `zh-Hans` and `zh-Hant`.

## Not In The First Milestone

- Full rewrite of every control template.
- Breaking package rename.
- Dropping the compatibility resource entry.
- MahApps adapter support.
