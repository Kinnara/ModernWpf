# GroupBox Official WPF Fluent Source Audit

ModernWpf maps `GroupBox` to WPF's platform
`System.Windows.Controls.GroupBox`. For this stock WPF control, the primary
source is official WPF Fluent rather than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GroupBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\GroupBox.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\GroupBoxVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default style now follows official WPF Fluent's `Border` / `Grid`
  template with two plain WPF `ContentPresenter` slots.
- The old `ContentPresenterEx` header and content slots were removed for this
  stock WPF control.
- `GroupBoxPadding`, `GroupBoxHeaderFontSize`, `GroupBoxHeaderMargin`, and
  `GroupBoxBorderThickness` now match official WPF Fluent values.
- `DefaultGroupBoxStyle` now sets `BorderThickness`, `Background`,
  `BorderBrush`, and `Padding` through the official GroupBox resource keys.
- The header presenter uses `TextElement.FontSize`,
  `TextElement.Foreground`, `ContentSource=Header`, and
  `RecognizesAccessKey=True`, matching the official template shape.
- Theme dictionaries now expose the official `GroupBoxBackground`,
  `GroupBoxBorderBrush`, and `GroupBoxHeaderForeground` aliases.

## WPF Substitutions

- Official WPF Fluent uses `System.Runtime` for `system:Double`. ModernWpf
  keeps `mscorlib` as the `sys` namespace assembly so the style loads on older
  supported target frameworks.
- Official WPF Fluent defines dedicated brush resources for GroupBox theme
  aliases. ModernWpf keeps its theme-resource alias model by pointing Light and
  Dark at `SystemControlTransparentBrush` / `TextFillColorPrimaryBrush`, and
  HighContrast at the equivalent system color brushes.
- The body presenter intentionally remains a plain WPF `ContentPresenter`
  without explicit `ContentTemplate` forwarding, matching the official WPF
  Fluent template.

## Tests

- `GroupBoxVisualStateTests.DefaultGroupBoxStyleUsesOfficialWpfFluentTemplateShape`
  covers the official style setters, runtime values, `Border` chrome, plain
  WPF presenter slots, header text attached properties, access-key recognition,
  and removal of `ContentPresenterEx`.
- `GroupBoxVisualStateTests.ThemeDictionariesExposeOfficialGroupBoxAliases`
  verifies the official theme aliases across Light, Dark, and HighContrast.
- `LayoutCompatibilityApiTests.GroupBoxTemplateUsesOfficialWpfFluentPresenterShape`
  protects the stock WPF presenter shape in the broader layout compatibility
  suite.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\GroupBox.xaml` as an official WPF Fluent stock
  control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter GroupBoxVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "GroupBoxVisualStateTests|GroupBoxTemplateUsesOfficialWpfFluentPresenterShape|TemplateParityTests|SyncMatrixTests"
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter LayoutCompatibilityApiTests
dotnet build ModernWpf.sln --no-restore
```
