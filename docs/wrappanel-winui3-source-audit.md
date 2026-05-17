# WrapPanel WinUI 3 Source Audit

ModernWpf `WrapPanel` is now treated as a source-backed WPF port of the local WinUI 3 implementation instead of a legacy WPF-compatible layout approximation.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\WrapPanel\WrapPanel.idl`
- `src\controls\dev\WrapPanel\WrapPanel.h`
- `src\controls\dev\WrapPanel\WrapPanel.cpp`
- `src\controls\dev\Generated\WrapPanel.properties.cpp`
- `src\controls\dev\Generated\WrapPanel.properties.h`
- `src\controls\dev\WrapPanel\APITests\WrapPanelTests.cs`

ModernWpf files:

- `ModernWpf.Controls\WrapPanel\WrapPanel.cs`
- `test\ModernWpf.WinUI.Tests\WrapPanel\WrapPanelApiTests.cs`

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
