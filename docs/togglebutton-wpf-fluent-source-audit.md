# ToggleButton Official WPF Fluent Source Audit

Date: 2026-05-18

Source snapshot: `D:\repos\wpf`

## Official WPF Fluent Source Files

- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ToggleButton.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ToggleButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`
- `test\ModernWpfTestApp\ApiTests\BaselineResources.cs`

## Ported Behavior

`ToggleButton` is a stock WPF control, so ModernWpf now treats official WPF
Fluent as the primary source instead of WinUI 3 CommonStyles:

- `DefaultToggleButtonStyle` now uses the official WPF Fluent template shape:
  `ContentBorder`, WPF `ContentPresenter`, and native `MultiTrigger` entries.
- Unchecked, checked, pressed, pointer-over, and disabled chrome is driven by
  WPF trigger conditions on `IsEnabled`, `IsChecked`, `IsMouseOver`, and
  `IsPressed` instead of `VisualStateEx.Setters`.
- The previous WinUI-shaped `ToggleButtonHelper.VisualStateSettersEnabled`,
  `VisualStateManagerEx`, `ContentPresenterEx`, checked-state background
  sizing, indeterminate visual states, and background transition path were
  removed from the stock `ToggleButton` template.
- `ToggleButtonPadding` and `ToggleButtonBorderThemeThickness` are now the
  control-specific resources used by the default style, matching official WPF
  Fluent resource shape.

## WPF Backport Substitutions

- Official WPF Fluent uses `DefaultControlFocusVisualStyle`; ModernWpf keeps
  the existing `{x:Static SystemParameters.FocusVisualStyleKey}` dynamic
  resource and `FocusVisualHelper` settings so the older-target focus visual
  bridge remains active.
- Official WPF Fluent uses `Border.CornerRadius`; ModernWpf keeps
  `ControlHelper.CornerRadius` in the backported template because older WPF
  targets do not expose the same source property on `ToggleButton`.
- Existing ModernWpf theme-resource aliases for toggle button brushes are
  retained because they already map to the same Fluent concepts and remain
  part of the public resource surface.
- `ToggleButtonForegroundCheckedDisabled` now resolves to
  `TextOnAccentFillColorDisabledBrush` in Light and Dark themes so the official
  WPF `TextElement.Foreground` setter receives a brush-valued resource.
- Official WPF Fluent does not define a separate indeterminate visual branch for
  the stock `ToggleButton`; ModernWpf now follows that fallback behavior for
  `IsChecked=null` instead of carrying the previous WinUI indeterminate visual
  states in this stock style.

## Tests And Validation

- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter ToggleButtonVisualStateTests`
- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~LayoutCompatibilityApiTests.ToggleButtonTemplateUsesOfficialWpfFluentPresenterSlot`
- `dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "ToggleButtonVisualStateTests|TemplateParityTests|SyncMatrixTests"`
- `dotnet build ModernWpf.sln --no-restore`
- `git diff --check`
