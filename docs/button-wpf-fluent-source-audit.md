# Button Official WPF Fluent Source Audit

Date: 2026-05-18

Source snapshot: `D:\repos\wpf`

## Official WPF Fluent Source Files

- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

## ModernWpf Files

- `ModernWpf\Styles\Button.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpfTestApp\ApiTests\BaselineResources.cs`

## Ported Behavior

`Button` is a stock WPF control, so ModernWpf now treats official WPF Fluent as
the primary source instead of WinUI 3 CommonStyles:

- `DefaultButtonStyle` now targets `ButtonBase`, with the implicit `Button`
  style based on it like official WPF Fluent.
- Default and accent button templates use a `ContentBorder` plus WPF
  `ContentPresenter`, matching official WPF Fluent template structure.
- Pointer-over, pressed, and disabled chrome is driven by
  `ControlTemplate.Triggers` for `IsMouseOver`, `IsPressed`, and `IsEnabled`
  instead of `VisualStateEx.Setters`.
- `AccentButtonStyle` is self-contained rather than based on
  `DefaultButtonStyle`, matching the official WPF Fluent style shape.
- `ButtonHelper.VisualStateSettersEnabled`, `VisualStateManagerEx`, and the
  Button-specific `AnimatedIcon.State` fallback setters are no longer part of
  stock `Button` templates.

`SubtleButtonStyle` is still a ModernWpf resource because official WPF Fluent
does not expose a matching style. Its theme-resource aliases remain the prior
WinUI-derived values, but its template now follows the same official WPF Fluent
trigger structure as the stock default/accent button templates.

## WPF Backport Substitutions

- Official WPF Fluent uses `DefaultControlFocusVisualStyle`; ModernWpf keeps
  the existing `{x:Static SystemParameters.FocusVisualStyleKey}` dynamic
  resource and `FocusVisualHelper` settings so the older-target focus visual
  bridge remains active.
- Official WPF Fluent uses `Border.CornerRadius`; ModernWpf keeps
  `Border.CornerRadius` in the backported templates because older WPF
  targets do not expose the same source property on `ButtonBase`.
- Existing ModernWpf theme-resource aliases for button brushes are retained
  because they already map to the same Fluent concepts and remain public
  resource keys.

## Tests And Validation

- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter ButtonVisualStateTests`
