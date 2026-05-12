# Official WPF Fluent And WinUI Consistency

This matrix tracks the 1.x theme strategy after the WinUI 2.8.7 sync. ModernWpf
uses two upstreams:

- official WPF Fluent for stock WPF controls where the target framework provides
  `PresentationFramework.Fluent`;
- WinUI 2.8.7 for ModernWpf-specific controls, WinUI-compatible resource keys,
  and element-level theme behavior.

The goal is not to replace one upstream with the other. The goal is to keep a
stable ModernWpf API while making the stock-control layer look and behave like
the platform Fluent theme on new WPF runtimes.

## Source Map

| Area | Source of truth | ModernWpf owner | Current state | Required guard |
| --- | --- | --- | --- | --- |
| Stock WPF control styles on `net10.0-windows7.0` | `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent` | `FluentControlsResources` entry-point composition | `FluentControlsResources` merges `PresentationFramework.Fluent;component/Themes/Fluent.xaml`, then `ModernWpfControlsResources.xaml`. | Verify the official Fluent dictionary is merged once and that ModernWpf-only control resources still resolve. |
| Stock WPF control styles on `net462` and `net8.0-windows7.0` | ModernWpf backport, periodically compared with official WPF Fluent XAML | `ControlsResources.xaml`, `StockControlsResources.xaml`, `ThemeResources/*.xaml`, `DensityStyles/Compact.xaml` | Older targets still use the existing ModernWpf controls/resources path. | Keep source-compatible resource keys; document intentional differences from official WPF Fluent. |
| ModernWpf WinUI-derived controls | WinUI 2.8.7 source and `docs/winui2-2.8.7-sync.md` | `ModernWpf.Controls` and `ModernWpf` control implementations | The WinUI-derived WPF test harness covers implemented controls and documented exclusions. | Do not remove WinUI resource aliases or template contracts while adopting official stock-control styles. |
| Shared theme tokens and aliases | WinUI 2.8.7 names plus official WPF Fluent values where they overlap | `ThemeResources`, `UISettingsResources`, `ModernWpfControlsResources.xaml` | Existing resources provide WinUI-compatible brush, typography, density, and accent aliases. | Alias keys remain stable; overlapping stock-control values may map to official Fluent values when feasible. |
| Recommended resource entry | ModernWpf compatibility contract | `<ui:ThemeResources />` plus `<ui:FluentControlsResources />` | README recommends the new entry, but Gallery and the WinUI test app still use `<ui:XamlControlsResources />`. | Add smoke coverage for the recommended entry and keep legacy entry behavior unchanged. |
| Legacy resource entry | ModernWpf 0.9.x compatibility | `<ui:ThemeResources />` plus `<ui:XamlControlsResources />` | Legacy path still merges `ControlsResources` and `UISettingsResources`. | Do not force official `ThemeMode` or platform Fluent behavior through this path. |
| Application theme preference | ModernWpf public API plus official WPF `Application.ThemeMode` on net10+ | `ThemeManager.ApplicationTheme`, `ThemeResources.RequestedTheme` | ModernWpf applies Light/Dark/HighContrast dictionaries itself. It does not currently set official WPF `Application.ThemeMode`. | Bridge only on `net10.0-windows7.0`; `null` maps to official `System`, Light/Dark map directly. |
| Window theme preference | ModernWpf `ThemeManager.RequestedTheme` attached property plus official WPF `Window.ThemeMode` on net10+ | `ThemeManager` window path | ModernWpf updates inherited `ActualTheme` and window resource dictionaries itself. It does not currently set official WPF `Window.ThemeMode`. | Bridge Window Default to official `None`, Light/Dark to direct values. |
| Element theme islands | ModernWpf/WinUI compatibility | `ThemeManager.RequestedTheme`, `ThemeResources.ApplyElementTheme`, `ThemeManager.HasThemeResources` | Element-level islands already use ModernWpf dictionaries. | Keep this path ModernWpf-owned because official WPF `ThemeMode` is app/window scoped only. |
| High contrast | WPF system theme behavior plus ModernWpf resource dictionaries | `ThemeResources.ApplyApplicationTheme`, `ApplyElementTheme`, `ThemeManager` high-contrast listeners | ModernWpf swaps in `ThemeResources/HighContrast.xaml`. | Bridge must not block existing high-contrast dictionary updates. |

## Implementation Checklist

| Stage | Deliverable | Evidence to collect |
| --- | --- | --- |
| Stage 2 resource entry cleanup | `FluentControlsResources` has explicit, duplicate-safe net10+ platform Fluent composition and unchanged legacy `XamlControlsResources` behavior. | Unit tests for dictionary source shape, duplicate detection, compact resources, and legacy path shape. |
| Stage 3 ThemeMode bridge | A small net10+ platform adapter maps ModernWpf application/window theme state to official WPF `ThemeMode`. | Tests for ApplicationTheme null/Light/Dark, Window RequestedTheme Default/Light/Dark, and element RequestedTheme Light/Dark. |
| Stage 4 backport sync | Older target resources are compared with official WPF Fluent in small batches. | Documented differences and focused resource tests for each synced batch. |
| Stage 5 Gallery/test coverage | Gallery and WinUI test app exercise the recommended resource entry. | Gallery runtime smoke tests and WinUI-derived control resource-resolution tests under `FluentControlsResources`. |
| Stage 6 docs/final validation | README/roadmap document the layered model and ThemeMode scope. | Release build, full tests, Gallery smoke tests, and `git diff --check`. |

## Known Current Gaps

- `ModernWpf.Gallery\App.xaml` still uses `<ui:XamlControlsResources />`; it
  should move to the recommended `<ui:FluentControlsResources />` entry after
  resource-entry tests are in place.
- `test\ModernWpf.WinUI.TestApp\App.xaml` still uses
  `<ui:XamlControlsResources />`; it should exercise the recommended path so
  WinUI-derived tests prove they work over the official Fluent stock layer.
- `ThemeManager` does not yet bridge to official WPF `Application.ThemeMode` or
  `Window.ThemeMode` on net10+.
- There is no focused test that detects duplicate
  `PresentationFramework.Fluent;component/Themes/Fluent.xaml` dictionaries.
- The older-framework backport has not yet been compared in a recorded batch
  against the current official WPF Fluent source.
