# Hyperlink Official WPF Fluent Source Audit

ModernWpf maps `Hyperlink` to WPF's platform
`System.Windows.Documents.Hyperlink`. For this stock WPF text element, the
primary source is official WPF Fluent rather than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Hyperlink.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\Hyperlink.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\HyperlinkVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default style now follows official WPF Fluent's stock `Hyperlink`
  style: `Foreground={DynamicResource HyperlinkForeground}`,
  `TextDecorations=Underline`, a pointer-over trigger that switches
  foreground and removes the underline, and a disabled trigger that switches
  to `HyperlinkForegroundDisabled`.
- The previous ModernWpf `HyperlinkHelper` pressed-state hook,
  `HyperlinkForegroundPressed` trigger, enabled `Cursor=Hand` trigger, and
  unused `HyperlinkUnderlineVisible` resource were removed from the stock
  `Hyperlink` style.
- Light and Dark `HyperlinkForegroundPointerOver` now map to the same primary
  accent text brush as `HyperlinkForeground`, matching official WPF Fluent's
  same-color pointer-over resource.
- Theme dictionaries now expose the official `HyperlinkForegroundDisabled`
  alias.

## WPF Substitutions

- Official WPF Fluent defines dedicated brush resources. ModernWpf keeps its
  theme-resource alias model by pointing Light and Dark at the same Fluent
  brush concepts, and HighContrast at the same system-color concepts.
- `HyperlinkForegroundPressed` remains in the theme dictionaries as an
  existing ModernWpf resource for compatibility with older custom styles, but
  the official WPF Fluent stock `Hyperlink` style no longer consumes it.
- `HyperlinkHelper` remains available for WPF-specific styles that still use a
  pressed-state substitute, such as the current DataGrid text-block hyperlink
  style. It is no longer part of the stock `Hyperlink` style.

## Tests

- `HyperlinkVisualStateTests.DefaultHyperlinkStyleUsesOfficialWpfFluentStyleSurface`
  covers the official setter and trigger shape, removal of the pressed helper
  and cursor triggers, and removal of the old underline-visibility resource.
- `HyperlinkVisualStateTests.ThemeDictionariesExposeOfficialHyperlinkAliases`
  verifies the official theme aliases across Light, Dark, and HighContrast.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\Hyperlink.xaml` as an official WPF Fluent stock
  control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter HyperlinkVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "HyperlinkVisualStateTests|TemplateParityTests|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```
