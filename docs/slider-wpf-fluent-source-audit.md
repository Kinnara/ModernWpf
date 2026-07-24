# Slider Official WPF Fluent Source Audit

ModernWpf target: WPF platform `Slider` styled through `ModernWpf\Styles\Slider.xaml`.

Official WPF Fluent source snapshot: `D:\repos\wpf`.

## Source Files

- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Light.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.Dark.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.HC.xaml`
- `src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Themes\Fluent.xaml`

## Ported Shape

`Slider` is a stock WPF control, so official WPF Fluent is the primary source for the backport. The ModernWpf style now follows the official WPF Fluent Slider block rather than the previous WinUI 3 visual-state adaptation.

- `SliderButtonStyle`, `SliderThumbStyle`, horizontal template, vertical template, and default style now use the official WPF Fluent structure.
- The helper-driven WinUI state port was deleted: `ModernWpf\Controls\Primitives\SliderHelper.cs` and `SliderHelper.VisualStateSettersEnabled` are gone.
- The WPF auto-tooltip placement helper was deleted from this style slice because official WPF Fluent does not carry a custom Slider tooltip helper.
- The Slider root templates use WPF `ControlTemplate.Triggers` for `TickPlacement`, `IsMouseOver`, and `IsSelectionRangeEnabled`, matching official WPF Fluent.
- Thumb common states use WPF names `Normal`, `MouseOver`, and `Pressed`. The WinUI `PointerOver` and `Disabled` thumb states were removed.
- The official WPF Fluent 20px thumb sizing is restored. Existing ModernWpf resource keys such as `DefaultSliderStyle`, `SliderHorizontal`, `SliderVertical`, and the thumb-size resources remain as backport aliases.

## Intentional Differences

- Resource references point to existing ModernWpf aliases where they already map to the same Fluent concepts, for example `SliderThumbBorderBrush` instead of directly referencing `ControlElevationBorderBrush`.
- The resource keys `SliderHorizontal`, `SliderVertical`, `DefaultSliderStyle`, and the Slider metric keys remain to avoid unnecessary resource-surface churn while the template internals follow official WPF Fluent.
- Theme dictionaries still provide ModernWpf/WinUI-compatible Slider aliases. This slice changes the stock WPF control style source, not the broader resource alias policy.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\SliderVisualStateTests.cs`
  - verifies horizontal and vertical templates use the official WPF Fluent trigger shape;
  - verifies WPF `MouseOver` thumb state naming and absence of WinUI `PointerOver` / `Disabled` states;
  - verifies tick placement and selection-range trigger behavior;
  - verifies official 14px track gutters and 20px thumb metrics.

Command:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter SliderVisualStateTests
```

Result: passed, 2 tests.
