# Official WPF Fluent Backport Sync

This file records the audited batches where the ModernWpf backport is compared
with the official WPF Fluent resources.

## 2026-05-12 Batch 1

Source inspected:

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent`
- `Resources\Fonts.xaml`
- `Resources\Variables.xaml`
- `Themes\Fluent.xaml`

### Synced Values

| Resource key | Official WPF Fluent value | ModernWpf value after sync | Reason |
| --- | --- | --- | --- |
| `SymbolThemeFontFamily` | `Segoe Fluent Icons, Segoe MDL2 Assets` | `Segoe Fluent Icons, Segoe MDL2 Assets` | Align shared icon glyph rendering with the platform Fluent theme while retaining MDL2 fallback for existing glyph contracts. |
| `ControlCornerRadius` | `4,4,4,4` | `4` | Already equivalent for WPF corner radius use. |
| `OverlayCornerRadius` | `8,8,8,8` | `8` | Already equivalent for WPF corner radius use. |
| `ControlContentThemeFontSize` | `14` | `14` | Already aligned. |
| `ContentControlFontSize` | `14` | `14` | Already aligned. |
| `TextControlThemeMinHeight` | `32` | `32` | Already aligned. |
| `TextControlThemePadding` | `10,5,6,6` | `10,5,6,6` | Already aligned. |

### Intentional Differences

| Resource key | Official WPF Fluent value | ModernWpf backport value | Reason retained |
| --- | --- | --- | --- |
| `TextControlThemeMinWidth` | `0` | `64` | ModernWpf inherited the WinUI-compatible text-control width contract; changing it would be a visible backport layout break. |
| `TreeViewItemPresenterMargin` | `0` | `4,2` | The default backport TreeView follows existing ModernWpf/WinUI-derived layout expectations. Compact mode already provides the official-style `0` value. |
| `TreeViewItemPresenterPadding` | `0` | `0,3,0,5` | Same as TreeView margin; keep default compatibility and use compact resources for denser layout. |
| `TreeViewItemMultiSelectCheckBoxMinHeight` | `24` | `28` | Retained for default ModernWpf/WinUI backport behavior; compact resources already use `24`. |
| Stock WPF control templates | Official WPF Fluent templates | ModernWpf backport templates on `net462` and `net8.0-windows7.0` | Template replacement is intentionally batched. `net10.0-windows7.0` uses official templates through `FluentControlsResources`. |

### Test Evidence

- `ModernWpf.Theme.Tests` verifies that the recommended resource entry resolves
  `SymbolThemeFontFamily` to the official WPF Fluent fallback chain.
- `ModernWpf.Gallery.Tests` verifies that Gallery uses
  `FluentControlsResources`, resolves the synced icon font, and uses the
  official platform Fluent dictionary on `net10.0-windows7.0`.
