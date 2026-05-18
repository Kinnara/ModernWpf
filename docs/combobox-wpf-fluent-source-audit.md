# ComboBox Official WPF Fluent Source Audit

## Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ComboBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ComboBox.xaml`
- `test\ModernWpf.WinUI.Tests\ComboBox\ComboBoxApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`
- `test\ModernWpf.WinUI.Tests\SyncMatrixTests.cs`

## Summary

ModernWpf now treats WPF `ComboBox` as a stock WPF control whose primary
source is official WPF Fluent. The previous WinUI-shaped helper/style layer was
deleted instead of preserved as a compatibility baseline.

The active `ComboBox.xaml` is copied from official WPF Fluent and keeps the
official `DefaultComboBoxTextBoxStyle`, `DefaultComboBoxToggleButtonStyle`,
`DefaultComboBoxItemStyle`, `DefaultComboBoxTemplate`,
`EditableComboBoxTemplate`, `DefaultComboBoxStyle`, and implicit `ComboBox` /
`ComboBoxItem` styles.

## Deleted Guessed Layer

- `ModernWpf\Controls\Primitives\ComboBoxHelper.cs` was removed.
- The stock ComboBox template no longer uses `VisualStateEx`,
  `VisualStateManagerEx`, `ContentPresenterEx`, or `FontIconFallback`.
- The old editable-only split-corner runtime helper, WPF-only `TextBoxStyle`
  attached property, and animated-glyph state driver were removed for this stock
  control.

## Substitutions

| Official WPF Fluent surface | ModernWpf substitution | Reason |
| --- | --- | --- |
| `System.Runtime` in the XAML namespace | `mscorlib` | Keeps copied resources compatible with ModernWpf's older target frameworks. |
| `Border.CornerRadius` attached setters and template bindings | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the newer official WPF attached property. |
| Unified official Fluent resource layout | Existing `ModernWpf\Styles\ComboBox.xaml` split dictionary | Keeps ModernWpf's current stock-control resource entry point. |
| No official DataGrid ComboBox adapter styles in `ComboBox.xaml` | Retained `DataGridComboBoxStyle` and `DataGridTextBlockComboBoxStyle` as WPF adapter resources based on `DefaultComboBoxStyle` | These remain compatibility resources for callers that reference them directly. The stock DataGrid template now follows official WPF Fluent and no longer wires them through `DataGridHelper`. |

## Test Evidence

- `ComboBoxApiTests` checks the default ComboBox and ComboBoxItem style shape,
  WPF presenter slots, editable TextBox style, theme resource aliases, DataGrid
  adapter style resolution, and deletion of the old WinUI helper/template layer.
- `TemplateParityTests` classifies `Styles\ComboBox.xaml` as an official WPF
  Fluent stock template file that should not use `VisualStateEx` or
  `ContentPresenterEx`.
- `SyncMatrixTests` records `ComboBox` as an official WPF Fluent-backed stock
  control rather than a WinUI-source helper/style port.
