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
- [Microsoft WPF Gallery `MainWindow.xaml.cs`](https://github.com/microsoft/WPF-Samples/blob/30ee5948dd92d2a81ef6a54d25b1b921463da107/Sample%20Applications/WPFGallery/MainWindow.xaml.cs)

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
- On Windows 11 outside High Contrast, the left, right, and bottom resize
  borders remain non-client edges. This follows the current Microsoft WPF
  Gallery shell policy and prevents full-client glass from exposing an
  unpainted compositor surface while a dark window is resized.
- The custom minimize button honors the native `WS_MINIMIZEBOX` window-style
  bit. Applications can remove that bit during `SourceInitialized` while
  retaining `WindowHelper.UseModernWindowStyle`; the custom command is then
  disabled and cannot minimize the window.

## WPF Substitutions

| Official WPF Fluent source | ModernWpf value | Reason |
| --- | --- | --- |
| Plain content-only `DefaultWindowStyle` | `BaseWindowStyle` plus `DefaultWindowStyle` | ModernWpf exposes a custom title bar and window chrome as part of its public WPF shell behavior. |
| Transparent backdrop default with `MS.Internal.FrameworkAppContextSwitches.DisableFluentThemeWindowBackdrop` and `Standard.Utility.IsOSWindows11OrNewer` guards | Always resolve the visible window background through `WindowBackground` | ModernWpf does not own .NET WPF Fluent's platform backdrop implementation; using the fallback resource is the stable WPF substitute. |
| Plain `AdornerDecorator` / `ContentPresenter` content host | Same plain WPF `ContentPresenter` inside ModernWpf's title-bar grid | The content presenter is source-compatible while the surrounding custom chrome remains ModernWpf-owned. |
| Stock `WindowTemplateKey` template swap for resize grip | Existing resize-grip trigger inside the custom shell template | Preserves ModernWpf's title bar, high-contrast border, and maximized-window handling while keeping the source `ResizeMode=CanResizeWithGrip` / `WindowState=Normal` visibility rule. |
| Stock content window with platform-owned non-client chrome | Full-glass custom title bar with Windows 11 left, right, and bottom `NonClientFrameEdges` | Matches the Microsoft WPF Gallery shell adaptation, preserves the top client title bar and snap-layout hit testing, and prevents resize-time white flashes reported in issue #683. High Contrast and pre-Windows 11 retain `NonClientFrameEdges.None`. |

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
- `WindowVisualStateTests` verifies the guarded Windows 11 resize-edge policy,
  the legacy and High Contrast fallbacks, the complete-glass requirement, and
  the High Contrast chrome-resource override.
- `TitleBarApiTests` verifies that removing `WS_MINIMIZEBOX` during
  `SourceInitialized` disables the ModernWpf minimize button and its routed
  command.
- `WindowVisualStateTests` statically guards `Styles\Window.xaml` against
  `ContentPresenterEx`, `MS.Internal`, `Fluent.Controls`, and `System.Runtime`
  source markers.
- `TemplateParityTests` no longer classifies `Styles\Window.xaml` as a WinUI
  presenter template that must use `ContentPresenterEx`.
