# RepeatButton Official WPF Fluent Source Audit

Date: 2026-05-18

Source snapshot: `D:\repos\wpf`

## Official WPF Fluent Source Files

- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

## ModernWpf Files

- `ModernWpf\Styles\RepeatButton.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\RepeatButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpfTestApp\ApiTests\BaselineResources.cs`

## Ported Behavior

`RepeatButton` is a stock WPF control, so ModernWpf now treats official WPF
Fluent as the primary source instead of WinUI 3 CommonStyles:

- `DefaultRepeatButtonStyle` now uses the official WPF Fluent template shape:
  `ContentBorder`, WPF `ContentPresenter`, and native
  `ControlTemplate.Triggers`.
- Pointer-over, pressed, and disabled chrome is driven by WPF triggers for
  `IsMouseOver`, `IsPressed`, and `IsEnabled` instead of
  `VisualStateEx.Setters`.
- The previous WinUI-shaped `ButtonHelper.VisualStateSettersEnabled`,
  `VisualStateManagerEx`, `ContentPresenterEx`, and background transition path
  was removed from the stock `RepeatButton` template.
- `RepeatButtonPadding` and `RepeatButtonBorderThemeThickness` are now the
  control-specific resources used by the default style, matching official WPF
  Fluent resource shape.

## WPF Backport Substitutions

- Official WPF Fluent uses `DefaultControlFocusVisualStyle`; ModernWpf keeps
  the existing `{x:Static SystemParameters.FocusVisualStyleKey}` dynamic
  resource and `FocusVisualHelper` settings so the older-target focus visual
  bridge remains active.
- Official WPF Fluent uses `Border.CornerRadius`; ModernWpf keeps
  `ControlHelper.CornerRadius` in the backported template because older WPF
  targets do not expose the same source property on `RepeatButton`.
- Existing ModernWpf theme-resource aliases for repeat button brushes are
  retained because they already map to the same Fluent concepts and remain
  part of the public resource surface.
- WPF's platform `RepeatButton` still owns repeat timing, `Delay`, `Interval`,
  input handling, and automation behavior.

## Tests And Validation

- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter RepeatButtonVisualStateTests`
- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~LayoutCompatibilityApiTests.RepeatButtonTemplateUsesOfficialWpfFluentPresenterSlot`
- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "RepeatButtonVisualStateTests|TemplateParityTests|SyncMatrixTests"`
- `dotnet build ModernWpf.sln --no-restore`
- `git diff --check`
