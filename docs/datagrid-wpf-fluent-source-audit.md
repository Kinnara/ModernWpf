# DataGrid WPF Fluent Source Audit

Date: 2026-05-18

## Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\DataGrid.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Controls\FallbackBrushConverter.cs`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\DataGrid.xaml`
- `ModernWpf\Controls\Primitives\FallbackBrushConverter.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\DataGridVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Summary

Stock WPF `DataGrid` now follows official WPF Fluent instead of the previous
ModernWpf-specific template and helper layer.

The previous guessed layer was deleted:

- `ModernWpf\Controls\Primitives\DataGridCellPresenter.cs`
- `ModernWpf\Controls\Primitives\DataGridHelper.cs`
- `ModernWpf\Controls\Primitives\DataGridRowHelper.cs`
- `DataGridCellExpanded`, `DataGridRowGroupHeaderStyle`, and
  `DataGridRowGroupContainerStyle` style branches in `DataGrid.xaml`

The copied style now uses official WPF Fluent structures:

- `DataGridCellFocusVisual`
- `DefaultDataGridCellStyle`
- `DefaultDataGridRowStyle`
- `DefaultDataGridRowHeaderStyle`
- `DefaultDataGridColumnHeaderStyle`
- `DefaultDataGridColumnHeadersPresenterStyle`
- `DefaultDataGridCellsPresenterStyle`
- `DefaultDataGridColumnFloatingHeaderStyle`
- `DefaultDataGridHeaderDropSeparatorStyle`
- `DefaultDataGridStyle`
- `DataGridCheckBoxElementDefaultStyle`
- `DataGridCheckBoxEditingElementDefaultStyle`

## Backport Substitutions

| Official WPF Fluent source | ModernWpf value | Reason |
| --- | --- | --- |
| `System.Runtime` system namespace | `mscorlib` | Keeps the style compatible with ModernWpf's older target frameworks. |
| `Border.CornerRadius` attached setter/template binding | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official attached property surface. |
| `Fluent.Controls.FallbackBrushConverter` | `ModernWpf.Controls.Primitives.FallbackBrushConverter` | Keeps the copied style self-contained inside ModernWpf. |
| Official DataGrid theme aliases | Added to Light, Dark, and HighContrast dictionaries | Required by the copied official template. |

## Removed Guesswork

The old template used `ContentPresenterEx`, `FontIconFallback`,
`DataGridCellPresenter`, `DataGridHelper`, and `DataGridRowHelper` to simulate a
WinUI-like DataGrid shape. Those are not part of official WPF Fluent's stock
DataGrid implementation, so they were removed from the stock DataGrid path.

The DataGrid-specific adapter styles retained in other stock style files, such
as `DataGridTextBoxStyle`, `DataGridComboBoxStyle`, and
`DataGridCheckBoxStyle`, remain separate compatibility resources. The stock
`DataGrid.xaml` no longer wires them through a ModernWpf helper.

## Test Evidence

- `DataGridVisualStateTests` covers the official style keys, resource aliases,
  WPF presenter slots, converter localization, and deleted helper/template
  branches.
- `LayoutCompatibilityApiTests` covers the DataGrid cell, column header, and
  row header presenter shape.
- `TemplateParityTests` classifies `DataGrid.xaml` as an official WPF
  Fluent-backed stock template file that should not use `VisualStateEx`,
  `ContentPresenterEx`, or old helper branches.
