# ModernWpf Core Control Source Coverage

This audit covers every resource dictionary merged by
`ModernWpf\ModernWpfControlsResources.xaml`. It complements:

- `docs\official-fluent-style-coverage.md` for stock WPF styles under
  `ModernWpf\Styles`.
- `docs\modernwpf-core-resource-source-coverage.md` for
  `ModernWpf\Themes\Generic.xaml`.
- `docs\winui3-control-source-coverage.md` for
  `ModernWpf.Controls\Themes\Generic.xaml`.

Rows in this file must remain one-to-one with the
`ModernWpfControlsResources.xaml` merged resource inventory. These resources
are the core ModernWpf WinUI-derived style layer that is still needed when
stock WPF controls come from official WPF Fluent.

## Status

- `WinUI 3 source-backed WPF port`: the resource belongs to an existing
  ModernWpf control mapped to local WinUI 3 source, with WPF substitutions
  documented in the linked audit.
- `WinUI 3 source-backed WPF family`: the resource is one entry in a larger
  source-backed control family documented by the linked audit.
- `WinUI 3 source-backed WPF platform mapping`: the resource maps WinUI source
  behavior onto an existing WPF platform control because this phase does not
  add new controls.
- `Shared WinUI resource compatibility layer`: the resource provides shared
  WinUI-compatible resource keys consumed by source-backed templates rather
  than a standalone control template.

## Resource Inventory

| Resource | Status | Evidence |
| --- | --- | --- |
| `Styles/Common.xaml` | Shared WinUI resource compatibility layer | `docs\official-fluent-winui-consistency.md`, `docs\winui3-source-parity.md` |
| `Styles/AutoSuggestBox.xaml` | WinUI 3 source-backed WPF port | `docs\autosuggestbox-winui3-source-audit.md` |
| `Styles/CommandBar.xaml` | WinUI 3 source-backed WPF family | `docs\commandbar-winui3-source-audit.md`, `docs\appbarbutton-winui3-source-audit.md`, `docs\commandbarflyout-winui3-source-audit.md` |
| `Styles/NavigationBackButton.xaml` | WinUI 3 source-backed WPF family | `docs\navigationview-winui3-source-audit.md` |
| `Styles/NavigationView.xaml` | WinUI 3 source-backed WPF family | `docs\navigationview-winui3-source-audit.md` |
| `Styles/RatingControl.xaml` | WinUI 3 source-backed WPF port | `docs\ratingcontrol-winui3-source-audit.md` |
