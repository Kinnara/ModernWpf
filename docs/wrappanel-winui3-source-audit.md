# WrapPanel WinUI 3 Source Audit

ModernWpf `WrapPanel` is now treated as a source-backed WPF port of the local WinUI 3 implementation instead of a legacy WPF-compatible layout approximation.

Date: 2026-07-18

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
de3e767333c2f0717a6a70cb22bd192ced5ad885
winui3/main
```

## Source Files

Primary WinUI 3 source references:

- `controls/dev/WrapPanel/WrapPanel.idl`
- `controls/dev/WrapPanel/WrapPanel.h`
- `controls/dev/WrapPanel/WrapPanel.cpp`
- `controls/dev/Generated/WrapPanel.properties.cpp`
- `controls/dev/Generated/WrapPanel.properties.h`
- `controls/dev/WrapPanel/APITests/WrapPanelTests.cs`

ModernWpf files:

- `ModernWpf.Controls\WrapPanel\WrapPanel.cs`
- `test\ModernWpf.WinUI.Tests\WrapPanel\WrapPanelApiTests.cs`

## Current Source Identity

The entire current WrapPanel runtime, generated-property, build-item, and API
test payload is byte-identical to snapshot
`c70471c511a0168b61dcca13af9556465f26b673`. Its only intervening path history
is `8463f45162149de0ec3ad7df752596893fe3e13e`, which moved the WinUI mirror
from `src/controls/...` to `controls/...`. No product patch is justified.

Current authoritative blob identities:

| Upstream file | Git blob |
| --- | --- |
| `WrapPanel.idl` | `18e1e0a870b9eae3f21336e63b9185c303ed50f2` |
| `WrapPanel.h` | `2eeef0dfd2cac4bcef559c4f1dd2026b3f107d2b` |
| `WrapPanel.cpp` | `885377d69ebf34437f260498269ced9c8f1abd81` |
| `Generated/WrapPanel.properties.cpp` | `10a449d4c68314591545c38ed2d427535c9d41fe` |
| `Generated/WrapPanel.properties.h` | `e3a2db1058ea5fde4131d2ffbf1a8256b0677358` |
| `APITests/WrapPanelTests.cs` | `74252f6a4875ebb840f219e9419a61239c1f9b97` |

## Current WinUI Gallery Coverage

The complete official WinUI Gallery tree at
`29f62479d5c046a0b854a5868e5a7cd484572d87` contains no WrapPanel sample or page. WrapPanel therefore has no truthful current live-Gallery comparison target.
Current product-source identity, source-derived layout regressions, and
multi-target builds are the appropriate gates for this source-only panel.

## Ported Source Shape

- The public surface matches the WinUI 3 IDL shape: `Padding`, `ItemSpacing`, `LineSpacing`, `Orientation`, and `ItemsStretch` with `WrapPanelItemsStretch.None` / `Last`.
- WPF dependency-property metadata directly invalidates measure and arrange for the source property set, replacing WinUI's generated property callbacks plus `OnPropertyChanged`.
- Measure follows the source row-building algorithm: children are measured with the padded available size, rows are built in UV coordinates, collapsed children are skipped without adding spacing, row height is the maximum child cross size, and `ItemsStretch.Last` stretches the final child to the remaining line space.
- Arrange now follows the source cache rule: rows are refreshed during arrange only when the arranged primary-axis size is smaller than the measured desired size. When a parent measures at a constrained width and later arranges wider, ModernWpf keeps the measured row breaks like WinUI instead of reflowing into the larger arrange slot.
- `WrapPanelApiTests` ports the upstream layout scenarios for padding, horizontal and vertical wrapping, spacing, dynamic orientation changes, variable child sizes, collapsed children, `ItemsStretch`, and source row-cache behavior.

## WPF Substitutions

- WPF uses `System.Windows.Controls.Orientation`; WinUI uses `Microsoft.UI.Xaml.Controls.Orientation`.
- WPF `FrameworkElement.Measure` clamps `DesiredSize` to the available size, so the WinUI source branch that can push an empty row before an oversized first child is represented in implementation but not directly observable through normal WPF layout tests.
- WPF collapsed elements are explicitly arranged to `Rect.Empty` when encountered after the source row walk, preserving WPF layout hygiene while keeping collapsed children out of row calculations.
- WPF dependency-property metadata replaces WinUI generated dependency-property wrappers and factory plumbing.

WrapPanel is a layout container and the current WinUI product source defines no
control-specific automation peer. ModernWpf likewise relies on the platform
panel/child automation tree rather than inventing a control-specific provider.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --framework net8.0-windows7.0 --filter FullyQualifiedName~WrapPanel --no-restore -m:1`
  - Passed 20/20, including current source-identity, public-surface,
    horizontal/vertical layout, spacing, padding, collapse, stretch,
    invalidation, and arrange-cache gates.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --framework <net462|net8.0-windows7.0|net10.0-windows7.0> --no-restore -m:1`
  - Passed all three targets with zero warnings and zero errors. The modern
    targets retain the repository's informational `Failed to resolve
    WinRT.Runtime.dll.` message without a build warning or error.
