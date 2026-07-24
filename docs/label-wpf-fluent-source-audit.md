# Label Official WPF Fluent Source Audit

ModernWpf maps `Label` to WPF's platform `System.Windows.Controls.Label`.
For this stock WPF control, the primary source is official WPF Fluent rather
than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Label.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\Label.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\LabelVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default style now follows official WPF Fluent's setter-only `Label`
  style: `Padding=0,0,0,4`, `Focusable=False`,
  `Foreground={DynamicResource LabelForeground}`, and
  `SnapsToDevicePixels=True`.
- The previous ModernWpf template with a `Border`, `ContentPresenterEx`,
  WinUI-like foreground forwarding, and disabled-state trigger was removed for
  this stock WPF control.
- ModernWpf no longer sets `OverridesDefaultStyle`, `Background`,
  `HorizontalContentAlignment`, `VerticalContentAlignment`, `FontSize`, or a
  custom template for `Label`; those now remain WPF platform behavior like the
  official Fluent style.
- Theme dictionaries now expose the official `LabelForeground` alias.

## WPF Substitutions

- Official WPF Fluent defines `LabelForeground` as a dedicated brush resource.
  ModernWpf keeps its theme-resource alias model by pointing Light and Dark at
  `TextFillColorPrimaryBrush`, and HighContrast at
  `SystemColorGrayTextColorBrush`.
- The official WPF Fluent source does not define a custom `Label` template, so
  there is no WinUI presenter surface to preserve. The WPF platform template
  remains responsible for access-key rendering and target behavior.

## Tests

- `LabelVisualStateTests.DefaultLabelStyleUsesOfficialWpfFluentStyleSurface`
  covers the official setter-only style shape, absence of custom template and
  `OverridesDefaultStyle` setters, runtime property values, and deletion of the
  old `ContentPresenterEx` visual surface.
- `LabelVisualStateTests.ThemeDictionariesExposeOfficialLabelForegroundAlias`
  verifies the official theme alias across Light, Dark, and HighContrast.
- `LayoutCompatibilityApiTests.LabelStyleUsesOfficialWpfFluentStyleSurface`
  protects the stock WPF style surface in the broader layout compatibility
  suite.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\Label.xaml` as an official WPF Fluent stock
  control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter LabelVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "LabelVisualStateTests|LabelStyleUsesOfficialWpfFluentStyleSurface|TemplateParityTests|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```
