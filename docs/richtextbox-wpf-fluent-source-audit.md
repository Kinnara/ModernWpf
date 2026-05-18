# RichTextBox Official WPF Fluent Source Audit

ModernWpf maps `RichTextBox` to WPF's platform
`System.Windows.Controls.RichTextBox`. For this stock WPF control, the primary
source is official WPF Fluent rather than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

## ModernWpf Files

- `ModernWpf\Styles\RichTextBox.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\RichTextBoxVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Behavior

- The default style now follows the official WPF Fluent
  `ContentBorder` / `PART_ContentHost` template shape.
- The previous WinUI-style header, placeholder, and description presenter slots
  were deleted for this stock WPF control because official WPF Fluent does not
  support those RichTextBox template parts.
- The previous `RichTextBoxHelper` placeholder-state driver was deleted. The
  official WPF Fluent template does not need a custom helper or visual-state
  manager.
- Pointer-over, focused, and disabled states now use WPF
  `ControlTemplate.Triggers` against `ContentBorder` and `PART_ContentHost`.
- Official style setters are restored for focus visual suppression, caret
  brush, cursor, min size, padding, scroll bar visibility, panning mode,
  flick-disable behavior, selection brush, and `AllowDrop`.

## WPF Substitutions

- Official WPF Fluent uses `DefaultControlContextMenu`; ModernWpf keeps
  `TextControlContextMenu` plus `TextContextMenu.UsingTextContextMenu` so the
  existing text-control context menu integration remains available.
- Official WPF Fluent uses `Border.CornerRadius`; ModernWpf keeps
  `primitives:ControlHelper.CornerRadius` for older target-framework support.
- ModernWpf keeps `Validation.ErrorTemplate` and
  `ValidationHelper.IsTemplateValidationAdornerSite` so existing validation
  adorners continue to attach to the template chrome.
- The legacy `RichEditBoxTopHeaderMargin` resource remains as an unused public
  alias, but the official-WPF-backed RichTextBox template no longer has a
  header presenter that consumes it.

## Tests

- `RichTextBoxVisualStateTests.DefaultRichTextBoxStyleUsesOfficialWpfFluentTriggerShape`
  covers the default/implicit style shape, official setters, WPF trigger
  matrix, deleted header/placeholder/description slots, deleted
  `ContentPresenterEx` slot, disabled resource application, and retained
  ModernWpf substitutions.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\RichTextBox.xaml` as an official WPF Fluent
  stock-control template.
- `LayoutCompatibilityApiTests.CoreTextInputDescriptionPresentersUseWinUIPresenterSlot`
  no longer expects a WinUI-style description presenter from stock
  `RichTextBox`.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter RichTextBoxVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "RichTextBoxVisualStateTests|TemplateParityTests|CoreTextInputDescriptionPresentersUseWinUIPresenterSlot|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```
