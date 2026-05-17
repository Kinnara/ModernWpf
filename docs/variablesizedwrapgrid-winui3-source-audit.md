# VariableSizedWrapGrid WinUI 3 Source Audit

ModernWpf `VariableSizedWrapGrid`, `WrapGrid`, and `ItemsWrapGrid` are now tracked as a source-backed WPF layout port. The WPF code keeps the existing controls because WPF has no platform `VariableSizedWrapGrid`, but the layout behavior is mapped against the local WinUI 3 source rather than treated as a guessed helper panel.

## WinUI 3 Source Inputs

- `src\dxaml\xcp\dxaml\lib\VariableSizedWrapGrid_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\VariableSizedWrapGrid_Partial.h`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\VariableSizedWrapGrid.g.cpp`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\VariableSizedWrapGrid.g.h`
- `src\dxaml\xcp\core\controls\generated\CVariableSizedWrapGrid.g.h`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs`
- `src\dxaml\test\native\external\controls\variablesizedwrapgrid\VariableSizedWrapGridIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf\Controls\VariableSizedWrapGrid.cs`
- `ModernWpf\Controls\WrapGrid.cs`
- `ModernWpf\Controls\ItemsWrapGrid.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf status |
| --- | --- |
| XamlOM surface: `ItemHeight`, `ItemWidth`, `Orientation`, `HorizontalChildrenAlignment`, `VerticalChildrenAlignment`, `MaximumRowsOrColumns`, and attached `RowSpan` / `ColumnSpan`. | Matched with WPF dependency properties, XAML parsing coverage, and WPF invalidation metadata. |
| `ComputeBounds` uses fixed item sizes when provided, otherwise measures the first item with span-aware constraints and uses its desired size. | Matched by the WPF `ComputeItemSize` path. |
| `CalculateOccupancyMap` computes direct `itemsPerLine` from available direct size and `MaximumRowsOrColumns`, then bounds the occupancy map by finite indirect available size. | Matched. The WPF port now stops placing later children when the finite indirect-axis occupancy map is full instead of growing past the available map. |
| Source row/column spans <= 0 normalize to 1. | Matched through `GetPositiveSpan`. |
| Source horizontal and vertical integration tests verify instantiation, live-tree entry/leave, horizontal wrapping, vertical wrapping, and row/column spans. | Covered by focused WPF layout tests for API surface, XAML parsing, horizontal/vertical wrapping, spans, and source occupancy-map-full behavior. |
| `WrapGrid` and `ItemsWrapGrid` share the same variable-sized wrap layout base. | Matched by the WPF inheritance model. `ItemsWrapGrid` keeps WPF-feasible realized-range properties and source-compatible surface aliases. |

## WPF Substitutions

- WinUI implements `IKeyboardNavigationPanel` and `IOrientedPanel` for selector navigation. WPF does not expose the same selector-to-panel hook, so ModernWpf documents this as a platform substitution instead of a guessed public API.
- WinUI stores its occupancy map in native `OccupancyBlock` linked blocks. The WPF port uses managed collections but preserves the visible placement, finite-map stop, span normalization, and alignment behavior.
- WinUI generated dependency-property metadata owns invalidation. WPF uses dependency-property metadata flags and child-property callbacks for equivalent layout invalidation.
- `ItemsWrapGrid` virtualization and sticky group-header behavior are represented only as WPF-feasible compatibility state; ModernWpf still arranges the realized WPF child collection directly.

## Validation

Run after this slice:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~LayoutCompatibilityApiTests
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
```
