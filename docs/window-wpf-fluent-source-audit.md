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
- [DWM colorization overview](https://learn.microsoft.com/windows/win32/dwm/composition-ovw)
- [Windows accent-color system information](https://learn.microsoft.com/openspecs/windows_protocols/ms-rdperp/bc6975ee-c630-4414-ba10-04eecbb6fccc)

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
- A window-scoped `WindowTitleBar.HeightKey` override keeps the rendered
  `WindowTitleBarControl`, the read-only `WindowTitleBar.Height` value, and
  `WindowChrome.CaptionHeight` synchronized. The synchronization also follows
  runtime resource changes and replacement chrome objects, so the entire
  visible title-bar height remains draggable instead of retaining the default
  32-DIP caption boundary.
- Disabled custom caption buttons remain inert when Windows 11 routes their
  input through non-client messages. In particular, `ResizeMode=CanMinimize`
  keeps the disabled maximize button from invoking its command.
- The rendered client area immediately below `WindowTitleBarControl` is explicitly
  returned as `HTCLIENT`, avoiding WPF `WindowChrome`'s resize-border addition
  to `CaptionHeight`. Caption-button bounds use half-open right/bottom edges,
  while title dragging, side/bottom resize borders, and explicit resize grips
  retain their native hit-test codes.
- A maximized custom-chrome window leaves a two-device-pixel reveal strip on
  the primary auto-hide taskbar edge, including when Windows proposes bounds
  that already exactly match the monitor. The shell state check tests the
  `ABS_AUTOHIDE` flag instead of treating `ABS_ALWAYSONTOP` as auto-hide.
- `WindowStyle=None` suppresses the ModernWpf title bar, detaches the custom
  `WindowChrome`, and lets content fill the captionless client area.
- The default Light/Dark `WindowBorder` follows the current per-user DWM
  `ColorPrevalence` preference: it uses the system accent color while Windows'
  "show accent color on title bars and window borders" option is enabled and
  retains `#707070` while disabled. The built-in maximized-window hook refreshes
  the value on Windows setting and DWM colorization messages. Application- and
  window-scoped `WindowBorder` overrides still take precedence.

## WPF Substitutions

| Official WPF Fluent source | ModernWpf value | Reason |
| --- | --- | --- |
| Plain content-only `DefaultWindowStyle` | `BaseWindowStyle` plus `DefaultWindowStyle` | ModernWpf exposes a custom title bar and window chrome as part of its public WPF shell behavior. |
| Internal `WindowBackdropManager` selected by WPF Fluent | Opt-in public `WindowBackdrop` attached adapter; ordinary windows still resolve `WindowBackground` | ModernWPF cannot consume WPF's internal manager across all supported targets. Preview 4 mirrors the guarded DWM path without making a material the default. See `window-backdrop-wpf-source-audit.md`. |
| Plain `AdornerDecorator` / `ContentPresenter` content host | Same plain WPF `ContentPresenter` inside ModernWpf's title-bar grid | The content presenter is source-compatible while the surrounding custom chrome remains ModernWpf-owned. |
| Stock `WindowTemplateKey` template swap for resize grip | Existing resize-grip trigger inside the custom shell template | Preserves ModernWpf's title bar, high-contrast border, and maximized-window handling while keeping the source `ResizeMode=CanResizeWithGrip` / `WindowState=Normal` visibility rule. |
| Stock content window with platform-owned non-client chrome | Full-glass custom title bar with Windows 11 left, right, and bottom `NonClientFrameEdges` | Matches the Microsoft WPF Gallery shell adaptation, preserves the top client title bar and snap-layout hit testing, and prevents resize-time white flashes reported in issue #683. High Contrast and pre-Windows 11 retain `NonClientFrameEdges.None`. |
| Platform-owned border colorization | Dynamic Light/Dark `WindowBorder` color selected from the per-user DWM `ColorPrevalence` preference | ModernWpf paints its own client border, so it must mirror the Windows preference explicitly. The existing system-accent palette and window message hook provide the WPF adaptation without adding a package dependency. |

## Intentional Differences

ModernWpf does not copy official `Window.xaml` wholesale. Doing so would remove
the ModernWpf `WindowTitleBarControl`, `WindowChrome`, attached title-bar properties,
high-contrast caption border, and `WindowHelper.FixMaximizedWindow`. Those are
custom WPF shell features rather than guessed control internals.

`TitleBarBackButtonStyle` still uses `FontIconFallback` because it belongs to
the ModernWpf title-bar chrome, not to the official WPF Fluent content-window
surface.

## Test Evidence

- `WindowVisualStateTests` verifies that `BaseWindowStyle` resolves
  `WindowForeground`, `WindowBackground`, `DefaultWindowChrome`, and
  `WindowHelper.FixMaximizedWindow`, and that the applied window template keeps
  `WindowTitleBarControl`, `ResizeGrip`, and a plain WPF content presenter.
- `WindowVisualStateTests` verifies the guarded Windows 11 resize-edge policy,
  the legacy and High Contrast fallbacks, the complete-glass requirement, and
  the High Contrast chrome-resource override.
- `TitleBarApiTests` verifies that removing `WS_MINIMIZEBOX` during
  `SourceInitialized` disables the ModernWpf minimize button and its routed
  command, and that the non-client click bridge cannot invoke the disabled
  maximize button when `ResizeMode=CanMinimize`.
- `WindowVisualStateTests` scopes `WindowTitleBar.HeightKey` to one window, verifies
  rendered/read-only/chrome heights at 56 DIPs, sends `WM_NCHITTEST` through
  the area below the former 32-DIP boundary, changes the resource to 64 DIPs,
  and verifies that an explicitly replaced `WindowChrome` is synchronized
  without replacing the caller-owned instance.
- `WindowVisualStateTests` sends native `WM_NCHITTEST` messages across the
  first four content pixels below the title bar and verifies `HTCLIENT`, while
  separately retaining `HTCAPTION` for draggable title space and
  `HTBOTTOMRIGHT` for the explicit resize grip.
- `WindowVisualStateTests` also covers the documented extended-title-bar
  opt-in: a control marked with
  `WindowChrome.IsHitTestVisibleInChrome=True` receives `HTCLIENT`, unmarked
  title-bar space remains `HTCAPTION`, and the explicit resize grip remains
  `HTBOTTOMRIGHT`.
- `WindowVisualStateTests` verifies that full-monitor maximized bounds are
  reduced once at the auto-hide taskbar edge and that the Win32 state parser
  distinguishes `ABS_AUTOHIDE` from `ABS_ALWAYSONTOP`.
- `WindowVisualStateTests` verifies that `WindowStyle=None` collapses the
  ModernWpf title bar, removes custom chrome/maximized-window compensation, and
  places content at the top of the client area.
- `ColorsHelperTests` verifies that Light and Dark `WindowBorder` brushes carry
  the dynamic system-preference color key, refresh from that key, select the
  system accent only while the preference is enabled, and retain the fallback
  for disabled or unusable system colors.
- `WindowVisualStateTests` statically guards `Styles\Window.xaml` against
  `ContentPresenterEx`, `MS.Internal`, `Fluent.Controls`, and `System.Runtime`
  source markers, and guards the maximized-window hook's DWM colorization
  refresh path.
- `TemplateParityTests` no longer classifies `Styles\Window.xaml` as a WinUI
  presenter template that must use `ContentPresenterEx`.
