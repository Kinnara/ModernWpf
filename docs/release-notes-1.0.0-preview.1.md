# ModernWPF 1.0.0-preview.1

`1.0.0-preview.1` starts the actively maintained ModernWPF 1.x line. It is the
first audit and migration baseline for the CLR APIs and public resource keys
shipped by `ModernWpfUI`. It records the Preview 1 package shape rather than
freezing later previews: current WinUI parity may require documented breaking
changes before stable 1.0.

## Highlights

- Targets `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`.
- Adds `FluentControlsResources` as the recommended resource entry.
  `net10.0-windows7.0` uses the platform WPF Fluent theme for stock controls;
  the other targets use ModernWPF's Fluent backport.
- Retains `ThemeResources` plus `XamlControlsResources` as the legacy resource
  entry for applications migrating from 0.9.x.
- Aligns the WinUI-derived controls, layouts, templates, automation peers, and
  resource contracts with the checked-in 1.x parity baselines.
- Ships portable SourceLinked PDBs in `ModernWpfUI.1.0.0-preview.1.snupkg`.
  Package metadata records the exact repository commit used for the build.

## Framework and package changes

The package no longer contains assets for:

- `net45`
- `netcoreapp3.0`
- `net5.0-windows`

The 0.9.x `ModernWpfUI.MahApps` adapter is not part of the 1.x line. Its
historical package is frozen and unsupported. MahApps applications must own
their integration locally when moving to 1.x; the core `ModernWpfUI` package
does not reference MahApps.

## Intentional 0.9 API changes

Source and binary compatibility with 0.9.x is not promised. In particular:

- Replace `SimpleStackPanel` with `StackPanelEx`.
- Replace `IElementFactoryShim` implementations with `IElementFactory`.
- Replace the old WPF window-shell `TitleBar` facade and control names with
  `WindowTitleBar` and `WindowTitleBarControl`.
- Template-only types previously exposed from
  `ModernWpf.Controls.Primitives` are internal in 1.x.
- WinRT projection and ABI implementation types are no longer public package
  API.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the complete boundary.

## Recommended application resources

New and migrated applications should merge:

```xaml
<ui:ThemeResources />
<ui:FluentControlsResources UseCompactResources="False" />
```

The legacy entry remains available when a staged migration is preferable:

```xaml
<ui:ThemeResources />
<ui:XamlControlsResources />
```

Both entries are built and executed from the packed NuGet package on every
supported target as part of the release gate.

## Preview limitations

- Packages are not Authenticode-signed for this preview. The annotated tag,
  repository commit metadata, retained workflow artifact, SourceLink data, and
  published SHA-256 manifest provide build provenance.
- MahApps integration is excluded.
- 0.9.x binary compatibility is not provided.
- Compositor-only WinUI behavior and raw pixel parity are adapted to WPF and
  remain subject to the documented manual Light, Dark, and High Contrast
  visual gate.
- Nullable annotations are not yet part of the public compatibility contract.
