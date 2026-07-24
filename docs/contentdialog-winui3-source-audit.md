# ContentDialog WinUI 3 Source Audit

Audit refreshed 2026-07-18 against official `microsoft-ui-xaml` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17) and WinUI Gallery
commit `29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13).

The relevant upstream history after the previous May audit contains only commit
`8463f45162149de0ec3ad7df752596893fe3e13e` (2026-05-30), which moved the
mirrored source tree from `src/` to the repository root. No later substantive
ContentDialog template, native behavior, XamlOM, or automation change was found.

## Current official sources

- `controls/dev/CommonStyles/ContentDialog_themeresources.xaml`
- `dxaml/xcp/dxaml/lib/ContentDialog_Partial.cpp`
- `dxaml/xcp/dxaml/lib/ContentDialog_Partial.h`
- `dxaml/xcp/dxaml/lib/ContentDialogMetadata.cpp`
- `dxaml/xcp/dxaml/lib/ContentDialogMetadata.h`
- `dxaml/xcp/dxaml/lib/ContentDialogClosingEventArgs_Partial.h`
- `dxaml/xcp/dxaml/lib/ContentDialogClosingDeferral_Partial.h`
- `dxaml/xcp/dxaml/lib/ContentDialogButtonClickEventArgs_Partial.h`
- `dxaml/xcp/dxaml/lib/ContentDialogButtonClickDeferral_Partial.h`
- `dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.Controls.cs`
- `dxaml/test/native/external/controls/contentdialog/ContentDialogIntegrationTests.cpp`
- `dxaml/test/native/external/controls/contentdialog/ContentDialogAutomationIntegrationTests.cpp`

Current authoritative product blobs are:

| Product source | Current blob |
| --- | --- |
| `controls/dev/CommonStyles/ContentDialog_themeresources.xaml` | `9cd8150fff8c216319e2d5515b538cfab5b397d5` |
| `dxaml/xcp/dxaml/lib/ContentDialog_Partial.cpp` | `a42b69a40ffca4c801cf83216c35a286436328d1` |
| `dxaml/xcp/dxaml/lib/ContentDialogMetadata.cpp` | `0e1d4117de9271e25653ac24390a93ae1106bb8a` |
| `dxaml/test/native/external/controls/contentdialog/ContentDialogIntegrationTests.cpp` | `d432348ab848421e8c2350c1e7fbb495a01d1772` |
| `dxaml/test/native/external/controls/contentdialog/ContentDialogAutomationIntegrationTests.cpp` | `6785715ca1152930f64b64700cc485e1c0f6607f` |

## Current Gallery baseline

No ContentDialog sample change is present after the current Gallery conversion.
The page retains exactly two examples: the `Save your work?` dialog with
Primary default button and the `Replace file?` dialog with no default button.
Both retain primary/secondary/cancel results, shared custom content, current
headers, button labels, and displayed definitions.

| Gallery source | Current blob |
| --- | --- |
| `WinUIGallery/Samples/ContentDialog/ContentDialogPage.xaml` | `e41cb2e8a95bbe230596c174fbf98dabbbf72800` |
| `WinUIGallery/Samples/ContentDialog/ContentDialogPage.xaml.cs` | `32c49c197540e22d968c354118258d8acd2fa175` |
| `WinUIGallery/Samples/ContentDialog/ContentDialogExample.xaml` | `2def02663d79db9cff46ff873314991aec618f47` |
| `WinUIGallery/Samples/ContentDialog/ContentDialogExample.xaml.cs` | `30909f938a028473b74d44818fd7e48619d1a7a8` |
| `WinUIGallery/Samples/ContentDialog/ContentDialogContent.xaml` | `f69a01bef1fd209a99e4ce1c9039d7629c25c886` |
| `WinUIGallery/Samples/ContentDialog/ContentDialogContent.xaml.cs` | `f0751752369919846287041618ce2cd6f9685927` |
| `WinUIGallery/Samples/ContentDialog/BasicContentDialogContent.txt` | `0054cf566faecfbd40d04ee03de2c33a52cab17a` |
| `WinUIGallery/Samples/ContentDialog/ContentDialogWithoutDefault.txt` | `05d976c076c69edfdf38f51fbb362f7fb751ff51` |

## ModernWpf mapping

- `ModernWpf.Controls/ContentDialog/ContentDialog.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialog.xaml`
- `ModernWpf.Controls/ContentDialog/ContentDialogAutomationPeer.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialogButtonClickEventArgs.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialogButtonClickDeferral.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialogClosingEventArgs.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialogClosingDeferral.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialogClosedEventArgs.cs`
- `ModernWpf.Controls/ContentDialog/ContentDialogOpenedEventArgs.cs`
- `test/ModernWpf.WinUI.Tests/ContentDialog/ContentDialogApiTests.cs`
- `test/ModernWpf.WinUI.Tests/ContentDialog/ContentDialogSourceAuditTests.cs`

## Source parity

- The public XamlOM surface is represented: title/template, full-size mode,
  corner radius, all three button text/command/parameter/style properties,
  enablement, default button, show/hide methods, opened/closing/closed and
  button-click events, results, cancelable event args, and deferrals.
- The template retains the source resources (`320`/`548` width,
  `184`/`756` height, `24` padding, `8` button spacing, `12` title margin,
  and the `1`-DIP separator), visual-state groups, three command columns,
  theme aliases, and elevation depth `128`.
- The detached WPF dialog now places the current WinUI control font defaults on
  the ContentDialog style. This makes arbitrary logical content inherit
  `ContentControlThemeFontFamily` and `ControlContentThemeFontSize=14`, just as
  it does through WinUI's popup tree. The title uses the source font family and
  20-DIP size; its `27`-DIP minimum is the WPF line-box bridge for the measured
  WinUI title height. Command buttons explicitly consume the same source font
  family and size because ModernWpf's stock Button style intentionally follows
  WPF Fluent rather than WinUI's `DefaultButtonStyle`.
- Result flow, cancelation/deferrals, command execution, default-button focus
  semantics, Enter, Escape/back handling, sibling-dialog rules, and shadow
  behavior remain pinned by `ContentDialogApiTests`.
- Current native source forwards `AutomationProperties.Name` and
  `AutomationId` to the popup, derives its default name from title/plain text,
  and marks the popup as a dialog. `ContentDialogAutomationPeer` now represents
  that source shape in WPF: Window control type and pattern, `ContentDialog`
  class name, explicit-name/title/content fallback, automation ID forwarding,
  modal state, and provider close semantics.

## Live visual proof

The visual harness now requires both parts of the installed-Gallery proof:

- closed `ShowDialog` button: mean delta at most `4.0`, exact crop size;
- open dialog surface: a common `ContentDialogSurface` crop derived from the
  title, content, checkbox, and command-button UIA evidence, then snapped to the
  source surface edges; mean delta at most `7.0`, aggregate size delta at most
  `2` pixels.

This replaces the old mismatched evidence where ModernWpf cropped only the
title while the WinUI run cropped nearly the entire Gallery window. The source
crop also records all six UIA element rectangles, which exposed and now guards
the former 12-DIP content inheritance and 28-DIP command-button rendering.

- Light: `artifacts/visual-checks/20260718-231822-208-52364/report.md`, closed
  `3.59` at `101x32`, open `6.59` at `320x218` versus `320x219`.
- Dark: `artifacts/visual-checks/20260718-231917-224-79972/report.md`, closed
  `2.62` at `101x32`, open `6.15` at `320x220` versus `320x221`.

Fresh Light OpenRepeat recording
`artifacts/gallery-recordings/20260718-232008-366/report.md` passes in `8.5s`
with `45.688` maximum frame delta and `73.122` maximum local delta. Fresh Dark
recording `artifacts/gallery-recordings/20260718-232151-395/report.md` passes in
`7.9s` with `8.495` / `27.032`. Both prove open, verified Cancel close, reopen,
and generate dense-transition review sheets.

Focused product/source coverage passes 18/18 on net8; Gallery sample and gate
coverage passes 2/2 on net8 and net10. `ModernWpf.Gallery` builds for net462,
net8, and net10 with zero errors; current target builds retain existing
unrelated warnings.

## WPF substitutions

- WinUI transplants ContentDialog into XamlRoot popup infrastructure, tracks
  per-root metadata, handles SIP/input-pane positioning, and supports native
  popup/windowed placement. ModernWpf uses WPF adorner/popup hosting and
  window-scoped ownership.
- WinUI compositor shadows and DComp validation are represented through
  `ThemeShadowChrome` with the source depth rather than native composition.
- WinUI `XamlUICommand` label, keyboard-accelerator, description, and tooltip
  binding has no direct ModernWpf command type; WPF `ICommand` execution and
  parameters are preserved.
- Native access-key/gamepad routes and `VisualState.Setters` are represented by
  WPF keyboard/focus routing and `VisualStateEx.Setters`.

## 2026-07-21 Screen-Origin Rounding Follow-up

Moving the dialog between physical screen origins exposed a WPF star-column
rounding split that could make the primary command four pixels narrower and a
command button one pixel short. The primary command column now has source-sized
`MinWidth=85`, and all three command buttons have `MinHeight=32`; wider dialogs
still use star sizing. Template tests pin both values.

Final Light
`artifacts/visual-checks/popup-fixes-light-v1/20260721-181431-286-73964/report.md`
passes the exact `101x32` launcher and `320x218` versus `320x219` open surface
at delta `5.75`. Final Dark
`artifacts/visual-checks/popup-family-dark-v1/20260721-181859-976-74480/report.md`
passes the exact launcher and `320x220` versus `320x221` open surface at delta
`6.31`.
