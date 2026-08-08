# Window backdrop official WPF and Windows source audit

Date: 2026-08-08

ModernWPF Preview 4 adds a WPF-native window material adapter. It does not
project Windows App SDK runtime classes into the package and does not emulate a
material with a custom blur brush. DWM remains the authority for the native
effect; ModernWPF owns the WPF lifecycle and deterministic fallback.

## Pinned sources

| Source | Commit or page | Purpose |
| --- | --- | --- |
| dotnet/wpf `main` | `7f005faa89e79b0b1fa1cb2c21283bab7916c092` | Current WPF window-backdrop implementation cutoff. |
| `WindowBackdropManager.cs` | blob `d28ae573fc2ef5af2e03a768ca1017ad0f647ffb` | WPF support guards, composition surface, glass frame, and DWM application sequence. |
| `WindowBackdropType.cs` | blob `3204ce020ec702a1d1fd77ee31e09e1602b660f0` | WPF's internal backdrop categories. |
| `Window.cs` | blob `aedc66a4e76a260a2158528fa51f9cc87ae58d99` | Source initialization and property-change integration. |
| `NativeMethods.cs` | blob `8497ff393dbb568b73f7d1976bcf874fe8dda11a` | `DWMWA_SYSTEMBACKDROP_TYPE`, native values, composition query, frame extension, and `DwmSetWindowAttribute`. |
| [DWM system backdrop values](https://learn.microsoft.com/windows/win32/api/dwmapi/ne-dwmapi-dwm_systembackdrop_type) | Windows 11 desktop API | `MAINWINDOW` is the long-lived-window material (Mica on Windows 11); `TRANSIENTWINDOW` is the transient-window material (Desktop Acrylic on Windows 11); minimum supported build is 22621. |
| [DwmSetWindowAttribute](https://learn.microsoft.com/windows/win32/api/dwmapi/nf-dwmapi-dwmsetwindowattribute) | Win32 desktop API | HRESULT contract used to detect native success or failure. |
| [System backdrops](https://learn.microsoft.com/windows/apps/windows-app-sdk/system-backdrop-controller) | Windows design and platform guidance | Mica is the foundation material for a primary window; Acrylic is intended chiefly for transient surfaces. |
| [Acrylic material](https://learn.microsoft.com/windows/apps/design/style/acrylic) | Windows design guidance | High Contrast and disabled transparency require a solid fallback. |

The source cutoff contains these exact native constants:

- `DWMWA_SYSTEMBACKDROP_TYPE = 38`
- `DWMSBT_NONE = 1`
- `DWMSBT_MAINWINDOW = 2`
- `DWMSBT_TRANSIENTWINDOW = 3`

## ModernWPF surface

`ModernWpf.Controls.WindowBackdrop` is a WPF attached-property adapter for
`System.Windows.Window`:

- `Kind` requests `None`, `Mica`, or `DesktopAcrylic`.
- `FallbackBrush` optionally supplies the solid fallback.
- read-only `EffectiveKind` reports the native material actually in use and is
  `None` whenever the fallback is active.

The enum deliberately uses user-facing material names rather than exposing
the broader internal WPF/DWM category list. Mica maps to the long-lived main
window value; Desktop Acrylic maps to the transient-window value. Mica Alt and
an automatic DWM choice are not part of Preview 4.

## WPF adaptation

Official WPF Fluent owns this capability internally and does not expose its
`WindowBackdropType` or `WindowBackdropManager` to applications on all three
ModernWPF target frameworks. ModernWPF therefore performs the same platform
work behind a small package-owned adapter:

1. Require Windows 11 build 22621 or newer, DWM composition, High Contrast
   off, and `AllowsTransparency=false`.
2. Resolve the HWND after `SourceInitialized` and make the WPF composition
   target transparent while retaining its exact previous color.
3. If no `WindowChrome` owns the glass frame, extend the DWM frame across the
   client area. An existing `WindowChrome` is accepted only when its
   `GlassFrameThickness` is `GlassFrameCompleteThickness`; a partial or zero
   glass frame falls back rather than reporting a native material that cannot
   fill the client area. Existing ModernWPF chrome already uses the complete
   glass frame and remains authoritative.
4. Set DWM attribute 38 to value 2 for Mica or 3 for Desktop Acrylic.
5. Make the WPF `Window.Background` transparent only after native setup
   succeeds.
6. On removal or fallback, request DWM value 1, undo only a frame extension
   owned by this adapter, restore the composition-target color, and restore the
   application background.

The adapter uses direct `dwmapi.dll` entry points already present on supported
Windows versions. It does not add a Windows App SDK runtime or deployment
dependency. Native failures are contained and become a fallback, rather than
an application startup failure.

The original WPF background is retained. If an application replaces the
background while a material is active, that newer application value becomes
the restoration value instead of being overwritten when `Kind` returns to
`None`.

## Policy and fallback matrix

| Condition | Requested material | Effective kind | Visible background |
| --- | --- | --- | --- |
| Windows 11 build 22621+, composition on, High Contrast off, native call succeeds | Mica or Desktop Acrylic | Requested kind | Native DWM material through transparent WPF content gaps. |
| Older Windows | Any native material | `None` | `FallbackBrush`, `WindowBackground`, or `SystemColors.WindowBrush`. |
| Composition disabled | Any native material | `None` | Same solid fallback chain. |
| real OS High Contrast | Any native material | `None` | Same solid fallback chain, preserving system legibility. |
| `AllowsTransparency=true` | Any native material | `None` | Same solid fallback chain; layered WPF windows are not passed to DWM backdrops. |
| Existing `WindowChrome` without a complete glass frame | Any native material | `None` | Same solid fallback chain; the adapter does not overwrite application-owned partial chrome margins. |
| Frame preparation or DWM attribute fails | Any native material | `None` | Same solid fallback chain. |
| `Kind=None` | None | `None` | The application background captured before the adapter took ownership. |

The state refreshes for source creation, activation/deactivation, DWM
composition changes, Windows theme/setting changes, and relevant WPF system
parameter changes. The process-wide system-parameter subscription begins only
after an HWND source exists, so setting a backdrop on a never-shown Window
does not root it. Event and HWND hooks are removed when the window closes.

## Gallery mapping

The Gallery `SystemBackdrop` page is a WPF adaptation of the Windows system
backdrop concept, not a copied WinUI control page. Its single durable example:

- opens real top-level WPF windows for Mica and Desktop Acrylic;
- displays requested and effective kinds;
- uses `WindowBackground` as the explicit fallback;
- places ordinary WPF card content above the window material; and
- exposes stable automation identifiers for the page, buttons, status, and
  opened windows.

The package README and normal quick start continue to use ordinary theme
resources. A window material is optional presentation, not a package startup
requirement.

## Automated evidence

`WindowBackdropTests` covers:

- defaults, validation, and Window-only attached-property ownership;
- Mica and Desktop Acrylic native success;
- composition-target/frame preparation and restoration;
- unsupported OS, disabled composition, real-High-Contrast policy, layered
  windows, frame failure, and DWM failure;
- explicit and resource-based fallback brushes;
- native-to-fallback-to-native refresh; and
- preservation of a background changed by the application while active.

Gallery tests open both material windows, verify the attached and effective
states, check the solid fallback contract, and close the windows. Source-shape
tests pin this audit, native constants, platform guards, Gallery route, stable
automation IDs, and release-tool coverage.

## Final visual and manual acceptance

The automated platform abstraction proves deterministic state transitions but
cannot manufacture the Windows compositor's pixels. Before Preview 4 is
tagged, the final clean tip must be exercised on all three supported targets:

- `net462`
- `net8.0-windows7.0`
- `net10.0-windows7.0`

For Light and Dark, capture both Mica and Desktop Acrylic windows, activate and
deactivate them, resize/maximize/restore them, and verify that content gaps
show the requested material without blank or opaque client regions. For real
OS High Contrast, verify `SystemParameters.HighContrast=true`, confirm
`EffectiveKind=None`, inspect the solid `WindowBackground` fallback and chrome
legibility, and restore the exact prior Contrast theme afterward. Older-OS and
disabled-composition behavior remain automated because the release host is a
current Windows 11 machine.

No successful historical or pre-final-tip image is accepted as Preview 4
release evidence.
