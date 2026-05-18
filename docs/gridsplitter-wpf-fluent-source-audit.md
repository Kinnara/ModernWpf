# GridSplitter Official WPF Fluent Source Audit

ModernWpf maps `GridSplitter` to WPF's platform
`System.Windows.Controls.GridSplitter`. For this stock WPF control, the
primary source is official WPF Fluent rather than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GridSplitter.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\GridSplitter.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\GridSplitterVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default style now follows official WPF Fluent's thumb template using a
  WPF `Border` root and `Rectangle` named `PART_Thumb`.
- The old solid-bar template and `GridSplitterPreviewStyle` guess were removed
  for this stock WPF control.
- `GridsplitterThumbHeight`, `GridsplitterThumbWidth`,
  `GridsplitterThumbRadius`, `GridsplitterMinHeight`,
  `GridsplitterMinWidth`, and `GridsplitterPadding` now match official WPF
  Fluent values.
- The style now uses official WPF triggers for pointer-over, dragging,
  disabled, and `Cursor=SizeNS` thumb orientation.
- Theme dictionaries now expose the official `GridsplitterBackground`,
  `GridsplitterBackgroundPointerOver`, `GridsplitterBackgroundPressed`,
  `GridsplitterBackgroundDisabled`, and `GridsplitterForeground` aliases.

## WPF Substitutions

- Official WPF Fluent uses `System.Runtime` for `system:Double`. ModernWpf
  keeps `mscorlib` as the `sys` namespace assembly so the style loads on older
  supported target frameworks.
- Official WPF Fluent defines dedicated brush resources. ModernWpf keeps its
  theme-resource alias model by pointing Light and Dark at the same Fluent
  brush concepts, and HighContrast at the same system-color concepts.
- Official WPF Fluent defines `GridsplitterMinWidth` but the style's
  `MinWidth` setter references `GridsplitterMinHeight`. ModernWpf keeps this
  exact style shape for parity while still exposing the `GridsplitterMinWidth`
  resource.

## Tests

- `GridSplitterVisualStateTests.DefaultGridSplitterStyleUsesOfficialWpfFluentTemplateShape`
  covers the official setter shape, runtime metric values, thumb template, and
  trigger matrix.
- `GridSplitterVisualStateTests.ThemeDictionariesExposeOfficialGridSplitterAliases`
  verifies the official theme aliases across Light, Dark, and HighContrast.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\GridSplitter.xaml` as an official WPF Fluent
  stock control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter GridSplitterVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "GridSplitterVisualStateTests|TemplateParityTests|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```
