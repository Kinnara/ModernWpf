# Stock ProgressBar Official WPF Fluent Source Audit

ModernWpf now uses the official WPF Fluent `ProgressBar` style as the source
for the stock `System.Windows.Controls.ProgressBar` template.

## Source

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ProgressBar.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ProgressBar.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\StockProgressBarVisualStateTests.cs`

## Synced Behavior

- `DefaultProgressBarStyle` now targets stock WPF `ProgressBar` directly.
- The implicit stock `ProgressBar` style is based on `DefaultProgressBarStyle`.
- The template uses the official `TemplateRoot`, `TrackBorder`, `PART_Track`,
  `PART_Indicator`, `Indicator`, and `Animation` parts.
- The official WPF `Determinate` and `Indeterminate` visual states are present,
  including the forever-repeating scale/origin animation.
- The official WPF triggers are present for vertical orientation and
  indeterminate mode.
- The stock style no longer instantiates `ModernWpf.Controls.ProgressBar`.

## ModernWpf Substitutions

- The existing `DefaultProgressBarStyle` key is retained so existing apps can
  reference the stock style explicitly.
- Existing `ProgressBarForeground`, `ProgressBarBackground`, and
  `ProgressBarBorderBrush` aliases are retained because they are also used by
  the separate WinUI-source-backed `ModernWpf.Controls.ProgressBar`.
- `ProgressBarIndeterminateBackground` and
  `ProgressBarIndeterminateBorderBrush` were added to all ModernWpf theme
  dictionaries and map to transparent Fluent brush aliases.
- The separate `ModernWpf.Controls.ProgressBar` control remains governed by
  `docs\progressbar-winui3-source-audit.md`; this audit is only for the stock
  WPF `ProgressBar` style in `ModernWpf\Styles`.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\StockProgressBarVisualStateTests.cs`
  covers the official style setter surface, visual-state names, animation
  shape, template parts, orientation and indeterminate triggers, deleted
  ModernWpf wrapper guess, and new indeterminate theme aliases.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies
  `ModernWpf\Styles\ProgressBar.xaml` as an official WPF Fluent stock template
  that should not use `VisualStateEx`.
