# Window Official WPF Fluent Source Audit

ModernWpf maps `Window` to WPF's platform `System.Windows.Window`, but its
default style also owns ModernWpf-specific custom title-bar chrome. For this
stock WPF control, official WPF Fluent is the primary source for the compatible
content-window resource and presenter behavior; ModernWpf keeps its custom
shell pieces as documented WPF substitutions.

## Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Window.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\Window.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\WindowVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Summary

The official WPF Fluent `Window` style is a plain content-window style:

- `DefaultWindowStyle` uses `WindowForeground`.
- Fallback non-backdrop states use `WindowBackground`.
- The template hosts content through `AdornerDecorator` and a plain WPF
  `ContentPresenter`.
- `ResizeMode=CanResizeWithGrip` shows the stock `ResizeGrip` in the normal
  window state.

ModernWpf now follows the compatible parts of that source shape:

- `BaseWindowStyle.Foreground` uses `WindowForeground`.
- `BaseWindowStyle.Background` uses `WindowBackground`.
- The content host in `Styles\Window.xaml` is a plain WPF `ContentPresenter`
  instead of `ContentPresenterEx`.
- `DefaultWindowStyle` remains based on `BaseWindowStyle`, preserving the
  ModernWpf shell contract.

## WPF Substitutions

| Official WPF Fluent source | ModernWpf value | Reason |
| --- | --- | --- |
| Plain content-only `DefaultWindowStyle` | `BaseWindowStyle` plus `DefaultWindowStyle` | ModernWpf exposes a custom title bar and window chrome as part of its public WPF shell behavior. |
| Transparent backdrop default with `MS.Internal.FrameworkAppContextSwitches.DisableFluentThemeWindowBackdrop` and `Standard.Utility.IsOSWindows11OrNewer` guards | Always resolve the visible window background through `WindowBackground` | ModernWpf does not own .NET WPF Fluent's platform backdrop implementation; using the fallback resource is the stable WPF substitute. |
| Plain `AdornerDecorator` / `ContentPresenter` content host | Same plain WPF `ContentPresenter` inside ModernWpf's title-bar grid | The content presenter is source-compatible while the surrounding custom chrome remains ModernWpf-owned. |
| Stock `WindowTemplateKey` template swap for resize grip | Existing resize-grip trigger inside the custom shell template | Preserves ModernWpf's title bar, high-contrast border, and maximized-window handling while keeping the source `ResizeMode=CanResizeWithGrip` / `WindowState=Normal` visibility rule. |

## Intentional Differences

ModernWpf does not copy official `Window.xaml` wholesale. Doing so would remove
the ModernWpf `TitleBarControl`, `WindowChrome`, attached title-bar properties,
high-contrast caption border, and `WindowHelper.FixMaximizedWindow`. Those are
custom WPF shell features rather than guessed control internals.

`TitleBarBackButtonStyle` still uses `FontIconFallback` because it belongs to
the ModernWpf title-bar chrome, not to the official WPF Fluent content-window
surface.

## Test Evidence

- `WindowVisualStateTests` verifies that `BaseWindowStyle` resolves
  `WindowForeground`, `WindowBackground`, `DefaultWindowChrome`, and
  `WindowHelper.FixMaximizedWindow`, and that the applied window template keeps
  `TitleBarControl`, `ResizeGrip`, and a plain WPF content presenter.
- `WindowVisualStateTests` statically guards `Styles\Window.xaml` against
  `ContentPresenterEx`, `MS.Internal`, `Fluent.Controls`, and `System.Runtime`
  source markers.
- `TemplateParityTests` no longer classifies `Styles\Window.xaml` as a WinUI
  presenter template that must use `ContentPresenterEx`.
