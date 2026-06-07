# Custom Popup Source Findings

Date: 2026-06-07

This note captures the source pass for replacing WPF `Popup` usage in ModernWpf flyout surfaces that need WinUI-like placement. The immediate motivator is CommandBarFlyout, but the primitive should be reusable.

## Sources Checked

- WPF popup source:
  - `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\PresentationFramework\System\Windows\Controls\Primitives\Popup.cs`
  - `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\PresentationFramework\System\Windows\Controls\Primitives\PopupRoot.cs`
- WinUI popup source and API spec:
  - `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\lib\Popup_Partial.cpp`
  - `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\lib\Popup_Partial.h`
  - `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\idl\winrt\core\microsoft.ui.xaml.coretypes.idl`
  - `D:\repos\microsoft-ui-xaml\specs\Popup-AdditionalLayoutProperties-Spec.md`
  - `D:\repos\microsoft-ui-xaml\src\controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.cpp`

## WPF Popup Behavior To Avoid

WPF `Popup` is not only an HWND host. It owns creation timing, logical-parent routing, popup-root measurement, monitor constraints, fallback selection, animation offsets, capture, close timers, DPI handling, and z-order behavior as one coupled control.

Important source points:

- `Popup.OnIsOpenChanged` creates and destroys the HWND through `CreateWindow`, `HideWindow`, async create, and async destroy timers.
- `PopupRoot.MeasureOverride` can restrict child size based on placement constraints. This means popup sizing is not a neutral "measure desired content and host it" operation.
- `Popup.UpdatePosition` ranks candidate placements by visible area, nudges the selected window, and applies `SetWindowPos`. This is WPF placement policy, not WinUI placement policy.
- `GetChildInterestPoints` uses the WPF popup child render size and animation offsets. This makes shadow-host padding and animated translations affect placement unless patched around.
- `GetScreenBounds` sometimes uses monitor work area and sometimes monitor bounds, with special cases for menus/tooltips and child popups.
- `BuildWindow` creates either child or top-level popup windows, applies topmost/no-activate/toolwindow styles, and treats transparency only at creation time.
- `HideWindow` can leave the HWND alive until a timer fires so close animations and routed events complete.

Conclusion: subclassing `Popup` or extending `PopupEx : Popup` would keep us dependent on WPF's sizing, fallback, timing, and close behavior. That is likely to repeat the same CommandBarFlyout bugs under different patches.

## WinUI Popup Behavior To Copy

WinUI's relevant API is `PopupPlacementMode` plus `DesiredPlacement`, `ActualPlacement`, `PlacementTarget`, and `ActualPlacementChanged`.

Important source points:

- `PopupPlacementMode` values are ordered as `Auto`, `Top`, `Bottom`, `Left`, `Right`, then edge-aligned variants. This differs from ModernWpf `FlyoutPlacementMode` because `FlyoutPlacementMode` includes `Full`.
- If `PlacementTarget` is null or `DesiredPlacement` is `Auto`, `ActualPlacement` remains `Auto` and `ActualPlacementChanged` is not raised.
- `SetPositionFromPlacement` calculates target bounds, available window rect, child size, major placement, and justification.
- If the desired placement is out of bounds, WinUI flips the major placement or justification, recalculates, and reports the final `ActualPlacement`.
- `ActualPlacementChanged` is raised when the calculated placement changes, before the screen refresh path completes.
- CommandBarFlyout uses `PlacementTarget = PrimaryItemsRoot`, `DesiredPlacement = BottomEdgeAlignedLeft`, and `ActualPlacement` to choose expanded-up versus expanded-down visual states.

## Design Consequences

The custom ModernWpf popup should start fresh instead of inheriting from WPF `Popup`.

Required first version:

- Own its HWND, probably through `HwndSource`, with no WPF `Popup` placement or `PopupRoot`.
- Expose `Child`, `IsOpen`, `PlacementTarget`, `DesiredPlacement`, `ActualPlacement`, `ActualPlacementChanged`, `HorizontalOffset`, and `VerticalOffset`.
- Add a ModernWpf `PopupPlacementMode` enum matching WinUI numeric order. Do not reuse `FlyoutPlacementMode`.
- Measure the child directly, then size the host HWND to the measured content bounds.
- Calculate placement from target screen bounds, child host bounds, flow direction, and monitor available rect.
- Flip major placement and justification using the WinUI algorithm, then set `ActualPlacement`.
- Raise `ActualPlacementChanged` before showing/moving the HWND for the frame.
- For `ThemeShadowChrome`, distinguish host bounds from content-placement bounds so reserved shadow space expands the HWND but does not move the visible content edge.
- Reposition on placement target layout changes, child size changes, DPI/source changes, offsets, and desired placement changes.

Explicit non-goals for the first version:

- Do not port WPF mouse placement modes.
- Do not emulate WPF popup root size restriction.
- Do not route through WPF `CustomPopupPlacementCallback`.
- Do not depend on private WPF `Popup` reflection.

## Open Implementation Questions

- Whether the first host should use `HwndSource` directly or a borderless `Window`. `HwndSource` is closer to WPF internals and avoids normal `Window` activation/chrome behavior.
- Whether light-dismiss and capture should live in the primitive or in flyout layers. WPF `Popup` couples them, but a fresh primitive may be cleaner if it only hosts and positions.
- How much parent-popup adjustment from WinUI is needed for nested popup scenarios. CommandBarFlyout likely needs placement and actual-placement reporting first.
- Which controls should migrate first after the primitive exists. CommandBarFlyout is the immediate target; MenuFlyout/FlyoutBase should be evaluated after focused tests.
