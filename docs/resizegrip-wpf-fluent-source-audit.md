# ResizeGrip Official WPF Fluent Source Audit

ModernWpf maps `ResizeGrip` to WPF's platform
`System.Windows.Controls.Primitives.ResizeGrip`. For this stock WPF control,
the primary source is official WPF Fluent rather than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ResizeGrip.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ResizeGrip.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ResizeGripVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default style now follows official WPF Fluent's glyph-based template
  using a plain WPF `TextBlock`.
- The old tiled `Path` / `DrawingBrush` resize-grip drawing was removed for
  this stock WPF control.
- `ResizeGripMinHeight`, `ResizeGripMinWidth`, `ResizeGripIconSize`, and
  `ResizeGripIconGlyph` now match official WPF Fluent values.
- The template now uses `SymbolThemeFontFamily`, `ResizeGripIconGlyph`,
  `ResizeGripIconSize`, and `ResizeGripForeground` like official WPF Fluent.
- Theme dictionaries now expose the official `ResizeGripForeground` aliases.

## WPF Substitutions

- Official WPF Fluent uses `System.Runtime` for `sys:Double` and `sys:String`.
  ModernWpf keeps `mscorlib` as the `sys` namespace assembly so the style loads
  on older supported target frameworks.
- Official WPF Fluent defines `ResizeGripForeground` as a dedicated brush
  resource. ModernWpf keeps its theme-resource alias model by pointing Light
  and Dark at `ControlStrongFillColorDefaultBrush`, and HighContrast at
  `SystemColorButtonTextColorBrush`.

## Tests

- `ResizeGripVisualStateTests.DefaultResizeGripStyleUsesOfficialWpfFluentTemplateShape`
  covers the official setter shape, runtime metric/glyph values, glyph
  `TextBlock` template, and deletion of the old `Path` template.
- `ResizeGripVisualStateTests.ThemeDictionariesExposeOfficialResizeGripForegroundAlias`
  verifies the official theme alias across Light, Dark, and HighContrast.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\ResizeGrip.xaml` as an official WPF Fluent stock
  control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter ResizeGripVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "ResizeGripVisualStateTests|TemplateParityTests|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```
