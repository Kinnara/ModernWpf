# ToolTip Official WPF Fluent Source Audit

ModernWpf maps `ToolTip` to WPF's platform `System.Windows.Controls.ToolTip`.
For this stock WPF control, the primary source is official WPF Fluent rather
than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ToolTip.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ToolTip.xaml`
- `ModernWpf\StockControlsResources.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ToolTipVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default style now follows the official WPF Fluent `Border` plus plain WPF
  `ContentPresenter` template shape.
- The previous WinUI-like `ThemeShadowChrome` and `ContentPresenterEx` chrome
  surface was removed for this stock WPF control.
- The template now uses the official WPF `DropShadowEffect` with a 30px blur,
  0 direction, 0.4 opacity, 0 shadow depth, and a fixed 4px corner radius.
- The style now uses WPF system status font family, size, style, and weight
  resources, matching official WPF Fluent, instead of the old
  `ToolTipContentThemeFontSize` setter.
- The official TextBlock wrapping style is present inside the template
  `ContentPresenter`.
- Theme dictionaries now expose official aliases `ToolTipForeground` and
  `ToolTipBackground` in addition to ModernWpf's existing brush aliases.

## WPF Substitutions

- No new control or WinUI compatibility helper is used. The template is a stock
  WPF `ToolTip` template.
- Existing ModernWpf resource aliases such as `ToolTipContentThemeFontSize`,
  `ToolTipForegroundBrush`, `ToolTipBackgroundBrush`, and
  `ToolTipBorderBrush` remain available for compatibility, but the official
  template shape does not consume `ToolTipContentThemeFontSize`.
- ModernWpf keeps its theme-resource alias model by pointing
  `ToolTipForeground` and `ToolTipBackground` at the existing brush tokens
  rather than duplicating brush instances.

## Tests

- `ToolTipVisualStateTests.DefaultToolTipStyleUsesOfficialWpfFluentTemplateShape`
  covers the default/implicit style shape, official setters, WPF border and
  presenter template shape, drop shadow, TextBlock wrapping style, and removal
  of `ContentPresenterEx` / `ThemeShadowChrome`.
- `ToolTipVisualStateTests.ThemeDictionariesExposeOfficialToolTipAliases`
  verifies official alias resources across Light, Dark, and HighContrast.
- `LayoutCompatibilityApiTests.ToolTipTemplateUsesOfficialWpfFluentPresenterShape`
  protects the stock WPF presenter shape in the layout compatibility suite.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\ToolTip.xaml` as an official WPF Fluent
  stock-control template.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter ToolTipVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "ToolTipVisualStateTests|ToolTipTemplateUsesOfficialWpfFluentPresenterShape|TemplateParityTests|SyncMatrixTests"
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter LayoutCompatibilityApiTests
dotnet build ModernWpf.sln --no-restore
```
