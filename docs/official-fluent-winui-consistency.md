# Official WPF Fluent And WinUI Consistency

This matrix tracks the 1.x theme strategy after the historical WinUI 2.8.7
sync and the WinUI 3 source-parity pivot. ModernWpf uses two active upstreams:

- official WPF Fluent for stock WPF controls where the target framework provides
  `PresentationFramework.Fluent`;
- WinUI 3 for ModernWpf-specific controls, WinUI-compatible resource keys,
  and element-level theme behavior.

The goal is not to replace one upstream with the other. During previews,
public-surface changes remain deliberate but current applicable WinUI API shape
is authoritative for WinUI-derived controls, including documented breaking
changes. Stable 1.x releases preserve their SemVer contract while the
stock-control layer continues to look and behave like the platform Fluent theme
on new WPF runtimes.

## Source Map

| Area | Source of truth | ModernWpf owner | Current state | Required guard |
| --- | --- | --- | --- | --- |
| Stock WPF control styles on `net10.0-windows7.0` | `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent` | `FluentControlsResources` entry-point composition | `FluentControlsResources` enables the platform ThemeMode bridge and layers `ModernWpfControlsResources.xaml`; WPF supplies the official Fluent stock-control dictionary. | Verify the official Fluent dictionary is active once and that ModernWpf-only control resources still resolve. |
| Stock WPF control styles on `net462` and `net8.0-windows7.0` | ModernWpf backport, periodically compared with official WPF Fluent XAML | `ControlsResources.xaml`, `StockControlsResources.xaml`, `ThemeResources/*.xaml`, `DensityStyles/Compact.xaml` | Older targets still use the existing ModernWpf controls/resources path. | `docs/official-fluent-style-coverage.md` must account for every official `Styles` source file as backported, folded, substituted, or excluded. |
| ModernWpf WinUI-derived controls | Current applicable WinUI 3 source and `docs/winui3-source-parity.md` | `ModernWpf.Controls` and `ModernWpf` control implementations | The WinUI-derived WPF test harness covers implemented controls and documented exclusions. | During previews, classify and test upstream API/resource changes and document migrations or WPF adaptations; after stable 1.0, preserve the SemVer contract or defer a break to the next major. |
| Shared theme tokens and aliases | Current applicable WinUI 3 resources plus official WPF Fluent values where they overlap; WinUI 2.8.7 is historical reference only | `ThemeResources`, `UISettingsResources`, `ModernWpfControlsResources.xaml` | Existing resources provide WinUI-compatible brush, typography, density, and accent aliases. | Audit aliases against current upstream; preview changes may be deliberately rebaselined, while stable 1.x keys follow the public contract. |
| Recommended resource entry | ModernWpf preview governance and stable compatibility contract | `<ui:ThemeResources />` plus `<ui:FluentControlsResources />` | README and Gallery use the new entry, and `ModernWpf.Theme.Tests` plus `ModernWpf.Gallery.Tests` exercise the entry on net8 and net10. | Keep focused net10 smoke coverage for the recommended entry and keep legacy entry behavior unchanged. |
| Legacy resource entry | ModernWpf 0.9.x compatibility | `<ui:ThemeResources />` plus `<ui:XamlControlsResources />` | Legacy path still merges `ControlsResources` and `UISettingsResources`. | Do not force official `ThemeMode` or platform Fluent behavior through this path. |
| Application theme preference | ModernWpf public API plus official WPF `Application.ThemeMode` on net10+ | `ThemeManager.ApplicationTheme`, `ThemeResources.RequestedTheme` | `FluentControlsResources` opts into `PlatformThemeModeBridge` on net10. ModernWpf still applies Light/Dark/HighContrast dictionaries for its own resource aliases. | Bridge only on `net10.0-windows7.0`; `null` maps to official `System`, Light/Dark map directly. |
| Window theme preference | ModernWpf `ThemeManager.RequestedTheme` attached property plus official WPF `Window.ThemeMode` on net10+ | `ThemeManager` window path | Window `RequestedTheme` is bridged through `PlatformThemeModeBridge` after the Fluent entry opts in. | Bridge Window Default to official `None`, Light/Dark to direct values. |
| Element theme islands | ModernWpf/WinUI compatibility | `ThemeManager.RequestedTheme`, `ThemeResources.ApplyElementTheme`, `ThemeManager.HasThemeResources` | Element-level islands already use ModernWpf dictionaries. | Keep this path ModernWpf-owned because official WPF `ThemeMode` is app/window scoped only. |
| High contrast | WPF system theme behavior plus ModernWpf resource dictionaries | `ThemeResources.ApplyApplicationTheme`, `ApplyElementTheme`, `ThemeManager` high-contrast listeners | ModernWpf swaps in `ThemeResources/HighContrast.xaml`. | Bridge must not block existing high-contrast dictionary updates. |

## Implementation Checklist

| Stage | Deliverable | Evidence to collect |
| --- | --- | --- |
| Stage 2 resource entry cleanup | `FluentControlsResources` has explicit, duplicate-safe net10+ platform Fluent composition and unchanged legacy `XamlControlsResources` behavior. | Unit tests for dictionary source shape, duplicate detection, compact resources, and legacy path shape. |
| Stage 3 ThemeMode bridge | A small net10+ platform adapter maps ModernWpf application/window theme state to official WPF `ThemeMode`. | Tests for ApplicationTheme null/Light/Dark, Window RequestedTheme Default/Light/Dark, and element RequestedTheme Light/Dark. |
| Stage 4 backport sync | Older target resources are compared with official WPF Fluent in source-backed batches. | `docs/official-fluent-backport-sync.md` records synced values and retained differences; `docs/official-fluent-style-coverage.md` guards the whole official `Styles` inventory; focused tests cover the synced style/resource shapes. |
| Stage 5 Gallery/test coverage | Gallery exercises the recommended resource entry. | `ModernWpf.Gallery.Tests` runs against `net8.0-windows7.0` and `net10.0-windows7.0`, including app resource-entry checks and runtime item-page smoke tests. |
| Stage 6 docs/final validation | README/roadmap document the layered model and ThemeMode scope. | Release build, full tests, Gallery smoke tests, and `git diff --check`. |

## Known Current Gaps

- `test\ModernWpf.WinUI.TestApp\App.xaml` intentionally still uses
  `<ui:XamlControlsResources />` because the broad WinUI-derived suite asserts
  ModernWpf backport stock-control styles. The recommended entry is covered by
  `ModernWpf.Theme.Tests` and `ModernWpf.Gallery.Tests` on net8 and net10.
- The official WPF Fluent coverage matrix is a snapshot of the local WPF source
  checkout. Refresh `docs/official-fluent-style-coverage.md` and the matching
  `TemplateParityTests` source-file list when the local official Fluent source
  adds or removes stock style files.
