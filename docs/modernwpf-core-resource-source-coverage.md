# ModernWpf Core Resource Source Coverage

This audit covers every resource dictionary merged by
`ModernWpf\Themes\Generic.xaml`. It complements
`docs\official-fluent-style-coverage.md`, which covers stock WPF styles under
`ModernWpf\Styles`, and `docs\winui3-control-source-coverage.md`, which covers
`ModernWpf.Controls\Themes\Generic.xaml`.

Rows in this file must remain one-to-one with the core ModernWpf generic
resource inventory. These resources are a mix of WinUI-source-backed custom
controls, compatibility layers required by source-backed templates, and custom
shell resources retained as documented WPF substitutions.

## Status

- `WinUI 3 source-backed WPF port`: the resource belongs to an existing
  ModernWpf control mapped to local WinUI 3 source, with WPF substitutions
  documented in the linked audit.
- `WinUI 3 source-backed WPF compatibility layer`: the resource is a WPF
  compatibility layer for WinUI template behavior that WPF stock controls do
  not expose directly.
- `Official WPF Fluent shell substitution`: the resource belongs to
  ModernWpf's custom shell around a stock WPF surface that is governed by
  official WPF Fluent for the compatible content-window behavior.
- `ModernWpf compatibility resource`: the resource is retained as a
  repo-specific support resource for existing source-backed templates or
  compatibility integration, with the retained scope documented in evidence.

## Generic Resource Inventory

| Generic resource | Status | Evidence |
| --- | --- | --- |
| `Navigation/Frame.xaml` | WinUI 3 source-backed WPF compatibility layer | `docs\winui3-source-parity.md` |
| `Navigation/Page.xaml` | WinUI 3 source-backed WPF compatibility layer | `docs\winui3-source-parity.md` |
| `ProgressBar/ProgressBar.xaml` | WinUI 3 source-backed WPF port | `docs\progressbar-winui3-source-audit.md` |
| `Themes/ContentControlEx.xaml` | WinUI 3 source-backed WPF compatibility layer | `docs\layout-chrome-winui3-source-audit.md` |
| `Themes/FontIconFallback.xaml` | ModernWpf compatibility resource | `docs\window-wpf-fluent-source-audit.md`, `docs\dropdownbutton-winui3-source-audit.md`, `docs\splitbutton-winui3-source-audit.md` |
| `Themes/ListViewHeaderItem.xaml` | ModernWpf compatibility resource | `docs\groupitem-wpf-fluent-source-audit.md`, `docs\winui3-source-parity.md` |
| `Themes/TextContextMenu.xaml` | ModernWpf compatibility resource | `docs\textbox-passwordbox-wpf-fluent-source-audit.md`, `docs\richtextbox-wpf-fluent-source-audit.md` |
| `TitleBar/TitleBarButton.xaml` | Official WPF Fluent shell substitution | `docs\window-wpf-fluent-source-audit.md`, `docs\winui-visualstate-setters-audit.md` |
| `TitleBar/TitleBarControl.xaml` | Official WPF Fluent shell substitution | `docs\window-wpf-fluent-source-audit.md`, `docs\winui-visualstate-setters-audit.md` |

## Dynamic System Colors

`DynamicColorExtension` tags mutable `SolidColorBrush` resources with their
source color key. `ColorsHelper` updates those brush instances when the Windows
accent palette changes so existing `DynamicResource` consumers keep their
resource identity.

Windows color-change notifications can produce a transient fully transparent
palette while the session is locking or resuming. ModernWpf rejects any such
incomplete system palette and retains the last valid colors; if the first
system read is invalid, it uses `DefaultAccentColor` until Windows supplies a
valid snapshot. Explicit application `ThemeManager.AccentColor` values still
flow through the separate `SetAccent` path.

`ColorsHelperTests.TransparentSystemAccentSnapshotDoesNotReplaceDynamicColorBrush`
guards the palette and brush behavior.
