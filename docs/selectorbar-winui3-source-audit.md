# SelectorBar WinUI 3 Source Audit

Date: 2026-07-17

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
c70471c511a0168b61dcca13af9556465f26b673
reference/winui3-current
```

## Source Files

- `src\controls\dev\SelectorBar\SelectorBar.cpp`
- `src\controls\dev\SelectorBar\SelectorBarItem.cpp`
- `src\controls\dev\SelectorBar\SelectorBar.xaml`
- `src\controls\dev\SelectorBar\SelectorBar.idl`
- `src\controls\dev\SelectorBar\SelectorBar_themeresources.xaml`
- `src\controls\dev\SelectorBar\SelectorBarItemAutomationPeer.cpp`
- `src\controls\dev\SelectorBar\APITests\SelectorBarTests.cs`
- `src\controls\dev\SelectorBar\InteractionTests\SelectorBarTests.cs`

## ModernWpf Port

- `ModernWpf.Controls\SelectorBar\SelectorBar.cs`
- `ModernWpf.Controls\SelectorBar\SelectorBarItem.cs`
- `ModernWpf.Controls\SelectorBar\SelectorBar.xaml`
- `ModernWpf.Controls\SelectorBar\SelectorBarAutomationPeer.cs`
- `ModernWpf.Controls\SelectorBar\SelectorBarItemAutomationPeer.cs`
- `test\ModernWpf.WinUI.Tests\SelectorBar\SelectorBarApiTests.cs`

## Ported Source Behavior

- Deleted the old guessed `PART_ItemsPanel` manual child-injection path. The default template now exposes a source-shaped `PART_ItemsView` and binds it to `SelectorBar.Items`.
- Deleted the old `SelectorBarItem` `PART_Button` wrapper and direct button click path. Item input now lives on the item, with WinUI-style selected/unselected pointer and disabled visual states.
- Replaced the old child-presenter template with the WinUI `PART_IconVisual`, `PART_TextVisual`, `PART_SelectionVisual`, and `PART_CommonVisual` shape. `Child` stays as the WPF substitute for the WinUI inherited `ItemContainer.Child` API, but the default template follows the current WinUI source placeholder and does not render it.
- Ported `SelectorBarItem::UpdatePartsVisibility`: icon/text parts collapse independently and the shared icon/text parent collapses when both are absent.
- Added source-style `SelectorBar.CornerRadius`, `SelectorBarItem.UseSystemFocusVisuals`, and `SelectorBarItem.FocusVisualMargin` surfaces used by the source default styles.
- Ported `SelectorBarItemAutomationPeer.GetNameCore` fallback order: explicit automation name, item `Text`, `Child` string representation, then the localized default source string `SelectorBarItem`.
- Added source theme resources local to `SelectorBar.xaml`, including selector/item foreground/background aliases, pill metrics, icon scale, spacing, padding, and border thickness.
- Preserved the source item padding, icon scale, spacing, and 48-pixel Gallery geometry while applying a render-only `Y=-1` WPF text baseline adjustment. The icon row remains on the source position; this isolates WPF text raster placement without changing measurement, selection-pill geometry, focus bounds, or hit targets.

## WPF Substitutions

- WinUI `ItemsView` is not present in ModernWpf. The WPF port uses an `ItemsControl` named `PART_ItemsView` inside a horizontal `ScrollViewer`; item selection remains owned by `SelectorBarItem` input and `SelectorBar.SelectedItem`.
- WinUI `ItemContainer` is not present as a standalone ModernWpf control. `SelectorBarItem` directly exposes the needed inherited `Child` and `IsSelected` surface on a WPF `Control` subclass.
- WinUI `StackLayout`, `Grid.CornerRadius`, and `Grid.BackgroundSizing` are represented by `StackPanel`, `GridEx`, and source-shaped resource bindings.
- WinUI `PointerUpThemeAnimation`, compositor transforms, XY focus, and `ItemsView.CurrentItemIndex` have no direct WPF equivalent. The WPF substitute uses source visual-state setters, WPF mouse capture, and keyboard left/right focus movement across focusable items.
- WPF and WinUI place the same Segoe UI Variable Text run on adjacent device-pixel baselines in this template. ModernWpf applies a one-pixel render translation to `PART_TextVisual`; the Gallery's source-backed local item template mirrors that platform adjustment while leaving the icon glyphs unshifted.
- The source English automation default `SelectorBarItem` is used until localized ModernWpf resource packs are added for this control.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter FullyQualifiedName~SelectorBar --no-restore`
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
- Light installed-WinUI Gallery proof: `artifacts/visual-checks/20260717-022122-441-38908/report.md`, exact `284x48` crops, primary delta `1.99`.
- Dark installed-WinUI Gallery proof: `artifacts/visual-checks/20260717-022213-202-80524/report.md`, exact `284x48` crops, primary delta `2.58`.
- `Run-GalleryVisualChecks.ps1` now enforces a strict SelectorBar primary-crop threshold of `3.0`.
