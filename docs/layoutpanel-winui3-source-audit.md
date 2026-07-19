# LayoutPanel WinUI 3 Source Audit

ModernWpf `LayoutPanel` is now tracked as a source-backed WPF port of the local WinUI 3 control rather than only a WinUI 2-era WPF-feasible layout surface.

Date: 2026-07-18

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
de3e767333c2f0717a6a70cb22bd192ced5ad885
winui3/main
```

## Source Files

Primary WinUI 3 source references:

- `controls/dev/LayoutPanel/LayoutPanel.idl`
- `controls/dev/LayoutPanel/LayoutPanel.h`
- `controls/dev/LayoutPanel/LayoutPanel.cpp`
- `controls/dev/LayoutPanel/LayoutPanelLayoutContext.h`
- `controls/dev/LayoutPanel/LayoutPanelLayoutContext.cpp`
- `controls/dev/LayoutPanel/APITests/LayoutPanelTests.cs`

ModernWpf files:

- `ModernWpf.Controls\LayoutPanel\LayoutPanel.cs`
- `ModernWpf.Controls\LayoutPanel\LayoutPanelLayoutContext.cs`
- `test\ModernWpf.WinUI.Tests\LayoutPanel\LayoutPanelApiTests.cs`

## Current Source Identity

The entire current LayoutPanel upstream tree is byte-identical to snapshot
`c70471c511a0168b61dcca13af9556465f26b673`. Its only intervening path history
is `8463f45162149de0ec3ad7df752596893fe3e13e`, which moved the WinUI source
mirror from `src/controls/...` to `controls/...`. No runtime, API, layout
context, build-item, or upstream test payload changed, so no product patch is
justified.

Current authoritative blob identities:

| Upstream file | Git blob |
| --- | --- |
| `LayoutPanel.idl` | `42ba19ac3dd510a0bd366eef964a2d0d644df84f` |
| `LayoutPanel.h` | `c9dcff481cdec2d6294af6290377d2352fa6221d` |
| `LayoutPanel.cpp` | `185fabd426b2d246185ff5c9b90bcd5447655d18` |
| `LayoutPanelLayoutContext.h` | `c9048301b1fd383db9637212c94699f2ef4919db` |
| `LayoutPanelLayoutContext.cpp` | `1d0c0a052d87e6d5af4522dc012b3110a027b009` |
| `APITests/LayoutPanelTests.cs` | `037d7e4028b6f03f91f7f6442e0b20dfa7e4b249` |

## Current WinUI Gallery Coverage

The complete official WinUI Gallery tree at
`29f62479d5c046a0b854a5868e5a7cd484572d87` contains no LayoutPanel sample or page. LayoutPanel therefore has no truthful current live-Gallery comparison
target. Current product-source identity, source-derived layout/chrome/lifecycle
tests, and multi-target builds are the appropriate gates for this source-only
surface.

## Ported Source Shape

- The public source surface is present: `Layout`, `BorderBrush`, `BorderThickness`, `Padding`, and `CornerRadius`.
- Measure and arrange follow the WinUI source algorithm: available/final size is reduced by padding and border thickness, clamped to zero, delegated to `Layout` when present, and otherwise applied to every child as a single fill rect.
- Source layout replacement semantics are preserved: the old layout is uninitialized and detached from measure/arrange invalidation, the new layout is initialized and hooked, and `LayoutPanel` invalidates measure after the change.
- `LayoutPanelLayoutContext` exposes the panel children and `LayoutState` to non-virtualizing layouts through the WPF `UIElementCollection` substitute.
- Tests cover upstream padding/border layout offsets, dynamic layout switching, custom non-virtualizing layout, source invalidation handler revocation, WPF chrome rendering, rounded clipping/hit testing, and XAML parsing of chrome properties.

## WPF Substitutions

- WinUI uses generated dependency-property metadata and `OnPropertyChanged`. ModernWpf uses WPF dependency-property registration and metadata flags to drive measure, arrange, and render invalidation.
- WinUI panel protected APIs own `BorderBrush`, `BorderThickness`, and `CornerRadius` under the internal SDK. ModernWpf renders equivalent chrome through `LayoutChromeHelper`.
- WinUI layout clips are platform layout geometry. ModernWpf represents rounded layout clipping and hit testing through WPF `Geometry` and `HitTestCore`.
- WinUI `Children().GetView()` is represented by a lightweight WPF `IReadOnlyList<UIElement>` wrapper over `UIElementCollection`.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --framework net8.0-windows7.0 --filter FullyQualifiedName~LayoutPanel --no-restore -m:1`
  - Passed 9/9, including current source-identity, source API-layout,
    layout-replacement lifecycle, chrome, clipping, hit-testing, and XAML gates.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --framework <net462|net8.0-windows7.0|net10.0-windows7.0> --no-restore -m:1`
  - Passed all three targets with zero warnings and zero errors. The modern
    targets retain the repository's informational `Failed to resolve
    WinRT.Runtime.dll.` message without a build warning or error.
