# StatusBar Official WPF Fluent Source Audit

ModernWpf maps `StatusBar` and `StatusBarItem` to WPF's platform
`System.Windows.Controls.Primitives.StatusBar` and
`System.Windows.Controls.Primitives.StatusBarItem`. For these stock WPF
controls, the primary source is official WPF Fluent rather than WinUI 3 common
styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\StatusBar.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\StatusBarItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\StatusBar.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\StatusBarVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- `StatusBar` now follows official WPF Fluent setters for foreground,
  background, border brush, border thickness, padding, and margin.
- The status-bar separator style now follows official WPF Fluent chrome:
  transparent background, `ControlElevationBorderBrush`, `6,0` margin,
  `1,1,0,0` border thickness, and an override-default-style `Border`
  template.
- `StatusBarItemPadding` now matches official WPF Fluent's `4` value.
- `StatusBarItem` now follows the official WPF Fluent template shape: a WPF
  `Border`, a plain WPF `ContentPresenter`, left/center content alignment, and
  WPF triggers for disabled foreground/background.
- The old `ContentPresenterEx` StatusBarItem slot and old
  `SystemControlDisabledBaseMediumLowBrush` disabled foreground path were
  removed for this stock WPF control.
- Theme dictionaries now expose the official `StatusBarItemBackground`,
  `StatusBarItemBackgroundDisabled`, and `StatusBarItemForegroundDisabled`
  aliases.

## WPF Substitutions

- Official WPF Fluent defines `StatusBar` only as an implicit style. ModernWpf
  keeps a `DefaultStatusBarStyle` wrapper key so existing resource lookup
  remains stable while the implicit style is based on the official setter set.
- Official WPF Fluent defines dedicated `StatusBarItem` brush resources.
  ModernWpf keeps its theme-resource alias model by pointing Light and Dark at
  `SystemControlTransparentBrush`, `ControlFillColorDisabledBrush`, and
  `TextFillColorDisabledBrush`, and HighContrast at equivalent system brushes.

## Tests

- `StatusBarVisualStateTests.DefaultStatusBarStyleUsesOfficialWpfFluentStyleSurface`
  covers the official StatusBar setter shape and runtime values.
- `StatusBarVisualStateTests.StatusBarItemUsesOfficialWpfFluentTemplateShape`
  covers official StatusBarItem setters, runtime values, disabled trigger
  resources, plain WPF presenter usage, and deletion of `ContentPresenterEx`.
- `StatusBarVisualStateTests.SeparatorStyleUsesOfficialWpfFluentShape` covers
  the official separator style surface.
- `StatusBarVisualStateTests.ThemeDictionariesExposeOfficialStatusBarItemAliases`
  verifies the official theme aliases across Light, Dark, and HighContrast.
- `LayoutCompatibilityApiTests.StatusBarTemplateUsesOfficialWpfFluentPresenterShape`
  protects the stock WPF presenter shape in the broader layout compatibility
  suite.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\StatusBar.xaml` as an official WPF Fluent stock
  control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter StatusBarVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "StatusBarVisualStateTests|StatusBarTemplateUsesOfficialWpfFluentPresenterShape|TemplateParityTests|SyncMatrixTests"
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter LayoutCompatibilityApiTests
dotnet build ModernWpf.sln --no-restore
```
