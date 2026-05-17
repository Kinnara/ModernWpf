# RadioMenuFlyoutItem WinUI 3 Source Audit

ModernWpf does not expose a separate `RadioMenuFlyoutItem` type. The existing WPF surface is `RadioMenuItem`, which maps the WinUI radio-menu behavior onto WPF `MenuItem` and `Menu` infrastructure.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\RadioMenuFlyoutItem\RadioMenuFlyoutItem.idl`
- `src\controls\dev\RadioMenuFlyoutItem\RadioMenuFlyoutItem.h`
- `src\controls\dev\RadioMenuFlyoutItem\RadioMenuFlyoutItem.cpp`
- `src\controls\dev\RadioMenuFlyoutItem\RadioMenuFlyoutItem_themeresources.xaml`
- `src\controls\dev\RadioMenuFlyoutItem\InteractionTests\RadioMenuFlyoutItemTests.cs`

ModernWpf files:

- `ModernWpf.Controls\RadioMenuItem\RadioMenuItem.cs`
- `ModernWpf.Controls\RadioMenuItem\RadioMenuItem.xaml`
- `test\ModernWpf.WinUI.Tests\RadioMenuFlyoutItem\RadioMenuFlyoutItemApiTests.cs`
- `test\ModernWpf.WinUI.Tests\RadioMenuFlyoutItem\RadioMenuFlyoutItemInteractionTests.cs`

## Ported Source Shape

- `RadioMenuItem` now treats the WinUI 3 `RadioMenuFlyoutItem` implementation as the behavior source of truth rather than a WPF-only checked-menu-item helper.
- Checked items are tracked in a weak group selection map, matching the source one-checked-item-per-group model.
- User or direct interaction cannot uncheck the currently checked item unless another item in the group safely replaces it, matching the source `InternalIsChecked` guard.
- Loaded checked items refresh the active selection map, and unloaded checked items are removed from the active map without being forced unchecked, matching WinUI's `OnLoaded` / `OnUnloaded` lifecycle.
- The default template keeps WinUI radio glyph, icon, keyboard-accelerator text, and `VisualState.Setters` behavior through WPF `VisualStateEx.Setters`.
- Tests cover the upstream basic interaction behavior, submenu checked-state visual behavior, source loaded/unloaded group-map lifetime, template state setters, glyph/icon layout, and WinUI radio-menu resource mappings.

## WPF Substitutions

- WinUI's public type derives from `MenuFlyoutItem` but privately composes `ToggleMenuFlyoutItem` through a source helper. WPF represents the same user behavior by deriving `RadioMenuItem` from `MenuItem`, coercing `IsCheckable` to `true`, and guarding `OnUnchecked`.
- WinUI's `AreCheckStatesEnabled` attached property targets `MenuFlyoutSubItem` and drives visual states directly on load. WPF targets `MenuItem`, tracks child `RadioMenuItem` changes, and mirrors the visual state through `MenuItem.IsChecked` because WPF menu templates already key checked visuals from that property.
- WinUI uses thread-local group storage. ModernWpf uses process-local WPF UI-thread storage because WPF controls must be used on their owning dispatcher.
- WinUI has automation and popup input coverage through TestUI. ModernWpf covers the WPF-feasible group-selection and template behavior in dispatcher tests, while raw popup automation remains represented by WPF menu semantics.
