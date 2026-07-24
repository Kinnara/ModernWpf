# Foundation Navigation WPF Fluent Source Audit

Date: 2026-05-18

## Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ContentControl.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\HeaderedContentControl.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ItemsControl.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\UserControl.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Page.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Frame.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\NavigationWindow.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\TextBlock.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Thumb.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\ContentControl.xaml`
- `ModernWpf\Styles\HeaderedContentControl.xaml`
- `ModernWpf\Styles\ItemsControl.xaml`
- `ModernWpf\Styles\UserControl.xaml`
- `ModernWpf\Styles\Page.xaml`
- `ModernWpf\Styles\Frame.xaml`
- `ModernWpf\Styles\NavigationWindow.xaml`
- `ModernWpf\Styles\TextStyles.xaml`
- `ModernWpf\Styles\Thumb.xaml`
- `ModernWpf\StockControlsResources.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\FoundationNavigationVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Summary

The stock WPF foundation and navigation-shell styles now follow official WPF
Fluent where ModernWpf did not previously merge a stock style dictionary.

Added official WPF Fluent-backed styles:

- `ContentControl`
- `HeaderedContentControl`
- `ItemsControl`
- `UserControl`
- `Page`
- `Frame`
- `NavigationWindow`

`TextStyles.xaml` now follows the official `TextBlock.xaml` style shape for the
core text styles. ModernWpf keeps the legacy `HeaderTextBlockStyle` and
`SubheaderTextBlockStyle` aliases as public compatibility resources, but the
official `BodyStrongTextBlockStyle` relationship and base text setters are now
source-backed.

`Thumb.xaml` was already an official WPF Fluent-shaped import with only the
older-target `Border.CornerRadius` substitution; this audit adds it to
the foundation/navigation coverage because it is a generic stock primitive.

## Backport Substitutions

| Official WPF Fluent source | ModernWpf value | Reason |
| --- | --- | --- |
| `System.Runtime` system namespace | `mscorlib` | Keeps copied resources compatible with ModernWpf's older target frameworks. |
| `Border.CornerRadius` in `Thumb.xaml` | `Border.CornerRadius` | Older ModernWpf targets do not expose the official attached property surface. |
| `TextBlock.xaml` file name | `TextStyles.xaml` | Preserve the existing ModernWpf merge path and public legacy text-style aliases. |
| Official shell theme resources | Added as Light, Dark, and HighContrast aliases | Required by `Page`, `Frame`, `NavigationWindow`, and the separate audited `Window` shell substitution. |

## Intentional Differences

ModernWpf's `Styles\Window.xaml` is not replaced in this slice. The official
WPF Fluent `Window.xaml` is a plain content-window style with platform backdrop
guards; ModernWpf's `Window` style owns custom title-bar chrome,
`WindowChrome`, `TitleBarControl`, and `WindowHelper.FixMaximizedWindow`.
That deliberate shell feature is covered separately by
`docs\window-wpf-fluent-source-audit.md`.

ModernWpf also keeps `HeaderTextBlockStyle` and `SubheaderTextBlockStyle` as
legacy resource aliases even though official WPF Fluent no longer defines them.

## Test Evidence

- `FoundationNavigationVisualStateTests` covers official style keys, WPF
  presenter slots, text-style shape, shell theme aliases, and source-shape
  file checks.
- `TemplateParityTests` classifies the new stock foundation/navigation style
  files as official WPF Fluent-backed files that must not use `VisualStateEx`
  or `ContentPresenterEx`.
