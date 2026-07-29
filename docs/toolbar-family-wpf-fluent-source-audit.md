# ToolBar Family Official WPF Fluent Source Audit

Date: 2026-05-18

## Source

Primary source:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Separator.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Thumb.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ToolBar.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

ModernWpf files:

- `ModernWpf\Styles\Separator.xaml`
- `ModernWpf\Styles\Thumb.xaml`
- `ModernWpf\Styles\ToolBar.xaml`
- `ModernWpf\StockControlsResources.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ToolBarFamilyVisualStateTests.cs`

## Result

ModernWpf now includes the official WPF Fluent stock `Separator`, generic `Thumb`, `ToolBar`, and `ToolBarTray` styles through `StockControlsResources`.

This is a stock WPF control-family port, so official WPF Fluent is the primary source. No new ModernWpf control was added.

## Synced Surface

- `DefaultSeparatorStyle` now follows official WPF Fluent: transparent background, `SeparatorBorderBrush`, `Focusable=false`, `BorderThickness=1,1,0,0`, and a WPF `Border` template.
- Generic `DefaultThumbStyle` now follows official WPF Fluent: `ThumbBackground`, disabled `ThumbBackgroundDisabled`, non-focusable/non-tab-stop behavior, and a WPF `Border` template.
- `ToolBarButtonBaseStyle`, `ToolBar.ButtonStyleKey`, `ToolBar.ToggleButtonStyleKey`, `ToolBar.CheckBoxStyleKey`, `ToolBar.RadioButtonStyleKey`, `ToolBar.ComboBoxStyleKey`, `ToolBar.MenuStyleKey`, `ToolBar.SeparatorStyleKey`, `ToolBar.TextBoxStyleKey`, `ToolBarThumbStyle`, `ToolBarOverflowButtonStyle`, `{x:Type ToolBar}`, and `{x:Type ToolBarTray}` now come from official WPF Fluent.
- Theme aliases were added for `SeparatorBorderBrush`, `ThumbBackgroundDisabled`, and `MenuBorderColorDefaultBrush`. `ThumbBackground` was moved to the official strong-fill concept.

## ModernWpf Substitutions

- `System.Runtime` string namespaces were changed to `mscorlib` for older target compatibility.
- Official WPF Fluent's generic `Thumb` uses `Border.CornerRadius`; ModernWpf uses `Border.CornerRadius` for older target compatibility, matching the existing stock-control backport pattern.
- Official WPF Fluent's generated monolithic dictionary can resolve `ToolBar.xaml` `StaticResource BasedOn` references from sibling style sections. ModernWpf loads style files as split dictionaries, so `ToolBar.xaml` locally merges the stock style dependencies it needs.
- Official `ToolBar.xaml` references `MenuBorderColorDefaultBrush`, but the local official theme resource files do not define that exact key. ModernWpf exposes it as an alias to the same flyout border concept used by the menu/context menu family.
- ModernWpf maps the toolbar TextBox `CaretBrush` to
  `TextControlForeground`. The official style leaves the WPF caret fallback in
  place, which derives from the system window background and can render a dark
  caret over a transparent toolbar in Dark theme.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\ToolBarFamilyVisualStateTests.cs` verifies the official separator, thumb, toolbar, toolbar item-style keys, the theme-aware TextBox caret adaptation, theme aliases, and absence of ModernWpf-specific template guesses.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies `Separator.xaml`, `Thumb.xaml`, and `ToolBar.xaml` as official WPF Fluent stock templates that should not use `VisualStateEx`.
