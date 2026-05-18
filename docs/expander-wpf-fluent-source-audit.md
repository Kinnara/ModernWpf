# Expander Official WPF Fluent Source Audit

ModernWpf `Expander` is a stock WPF control, so official WPF Fluent is the primary source for its template, trigger behavior, presenter shape, focus visuals, animations, and theme resources.

## Source Files

Official WPF Fluent source references:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Expander.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Controls\AnimationFactorToValueConverter.cs`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

ModernWpf files:

- `ModernWpf\Styles\Expander.xaml`
- `ModernWpf\Controls\Primitives\AnimationFactorToValueConverter.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\Expander\ExpanderApiTests.cs`
- `test\ModernWpf.WinUI.Tests\Expander\ExpanderInteractionTests.cs`

## Ported Source Shape

- The default and implicit stock `Expander` styles now follow official WPF Fluent, including `DefaultExpanderToggleButtonDownStyle`, `DefaultExpanderToggleButtonUpStyle`, `DefaultExpanderToggleButtonLeftStyle`, `DefaultExpanderToggleButtonRightStyle`, and `DefaultExpanderStyle`.
- The template uses official WPF `ControlTemplate.Triggers` and storyboards for expand/collapse behavior instead of the previous WinUI-shaped `VisualStateEx.Setters` layer.
- Header and content slots use plain WPF `ContentPresenter` elements, matching official WPF Fluent. The old `ContentPresenterEx` and `FontIconFallback` template guesses were removed from the stock Expander path.
- The copied style keeps official local resources for padding, border thickness, chevron size, chevron glyphs, and `AnimationFactorToValueConverter`.
- Theme dictionaries expose the official Expander keys consumed by the copied style: `ExpanderHeaderBackground`, `ExpanderHeaderForeground`, `ExpanderHeaderBorderBrush`, `ExpanderHeaderBorderPointerOverBrush`, `ExpanderHeaderDisabledForeground`, `ExpanderHeaderDisabledBorderBrush`, and `ExpanderContentBackground`.
- Tests cover the official default style, header/content template parts, direction-trigger templates for down/up/left/right, delayed collapse animation, high-contrast aliases, and WPF automation visibility.

## WPF Substitutions

- Official WPF Fluent uses `System.Runtime` for `system` resources; ModernWpf uses `mscorlib` to keep older target frameworks working.
- Official WPF Fluent uses `Border.CornerRadius` attached setters/template bindings; ModernWpf maps that to `primitives:ControlHelper.CornerRadius`.
- Official WPF Fluent's `Fluent.Controls.AnimationFactorToValueConverter` is localized as `ModernWpf.Controls.Primitives.AnimationFactorToValueConverter` so the copied style can resolve the converter without referencing the platform Fluent theme assembly.
- Historical ModernWpf Expander resource aliases that are no longer consumed by the official style remain in the theme dictionaries to avoid unnecessary resource-surface churn.
