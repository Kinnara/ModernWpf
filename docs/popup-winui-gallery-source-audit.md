# Popup WinUI Gallery Source Audit

Current official source: `microsoft/WinUI-Gallery` `main` at
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13).

The local Gallery source checkout remains at
`1d490ef14f96d5c52de253b94063168eecde08e9` (2026-04-30). This audit queried
the official repository for the current commit, current blobs, and file
history instead of treating that older checkout as current.

Current official files inspected:

- `WinUIGallery/Samples/Popup/PopupPage.xaml` — blob
  `6a5d35d9acf43067bb7cb9317e2d1ad863dbade2`
- `WinUIGallery/Samples/Popup/PopupPage.xaml.cs` — blob
  `ea3a3be6beecbaecb15f9a9612767d0e74c9b992`
- `WinUIGallery/Samples/Popup/PopupOffsetPositioning.txt` — blob
  `711eea6ce1ead50ef91bcf5eba7c011ddf892af9`

ModernWpf implementation and proof:

- `ModernWpf.Gallery/Pages/DialogsFlyoutsSampleFactory.cs`
- `test/ModernWpf.Gallery.Tests/GalleryAutomationHookTests.cs`
- `test/ModernWpf.Gallery.Tests/PopupSourceAuditTests.cs`
- `test/ModernWpf.Gallery.Tests/WpfGallerySourceShapeTests.cs`
- `tools/visual-checks/Run-GalleryVisualChecks.ps1`
- `tools/visual-checks/Record-GalleryControlInteractions.ps1`

## Current-source history

- `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` (2026-05-22) converted the
  remaining Gallery samples to the current per-sample layout. Popup moved from
  `WinUIGallery/Samples/ControlPages/PopupPage.xaml` to the current paths above;
  the runtime example was not substantively changed.
- `177e25c1f537e54ce071c271d4486c81e6db1a3b` (2025-07-30) removed the fixed
  live-surface width and established the current `MinWidth="240"` layout.
- `34d541896423996c22e5b80c5af2b2737be965d6` (2025-07-25) made the popup's
  Close button keyboard accessible.
- `b9b7348c209836fe69c03cd8c5732baa77f46462` (2025-04-17) moved the stroke
  properties from Popup to the rendered Border where they belong.
- No later Popup runtime, interaction, or accessibility change exists between
  the May local checkout and the audited current commit.

The current `PopupOffsetPositioning.txt` display snippet still demonstrates an
older fixed `200x160` Border with 20px padding and centered content. The actual
current `PopupPage.xaml` is authoritative for runtime parity: a minimum-width
surface with natural height. ModernWpf preserves the historical snippet for
source-code display while its live sample follows the current page.

## Visual mapping

The current live WinUI Gallery surface defines:

- a `Grid` with `MinWidth="240"`, `Padding="16"`, a 1-DIP surface stroke,
  `OverlayCornerRadius`, and the default acrylic fill;
- a vertical StackPanel with 8-DIP spacing;
- a 16-DIP `Simple Popup` heading and a standard 32-DIP `Close` button;
- a natural measured height of 96 DIPs.

ModernWpf uses a WPF `Border` and `StackPanel` with the same resources and
metrics. WPF's 16px Segoe UI line box measures 21 DIPs while WinUI's measures
22, so the heading has a 22-DIP minimum-height bridge. That produces the exact
current `240x96` surface without changing its glyph position or content.

The visual harness anchors on the unique `Simple Popup` heading. It captures
the complete surface from screen coordinates so the comparison works for both
WinUI's in-window Popup and WPF's separate transparent popup HWND. Both emit a
common `PopupSurface` crop and must satisfy:

- official closed trigger source `Show Popup (using Offset)`, mean delta at
  most `4.0`, exact `189x32` size;
- open surface mean delta at most `3.0`, exact `240x96` size.

Final strict installed-Gallery evidence:

- Light: `artifacts/visual-checks/20260718-234231-942-70740/report.md` — closed
  `3.33` at exact `189x32`; open surface `2.63` at exact `240x96`.
- Dark: `artifacts/visual-checks/20260718-234257-459-77704/report.md` — closed
  `3.13` at exact `189x32`; open surface `2.66` at exact `240x96`.

Fresh `OpenRepeat` recordings exercise two successful opens and verified close
paths while retaining the unique popup content bounds:

- Light: `artifacts/gallery-recordings/20260718-234329-029/report.md` — passed,
  `11.4s`, maximum frame/local deltas `0.265` / `23.164`.
- Dark: `artifacts/gallery-recordings/20260718-234435-353/report.md` — passed,
  `11.4s`, maximum frame/local deltas `0.367` / `28.313`.

## Behavior and accessibility mapping

- The source's horizontal/vertical offsets bind one-way to NumberBoxes with
  the same `-100..100` and `-100..500` ranges, 10 small change, 100 large
  change, and initial `0`/`200` values. ModernWpf mirrors updates into WPF
  `Popup.VerticalOffset` and `HorizontalOffset`.
- The source's `IsLightDismissEnabled` is represented by the inverse WPF
  `Popup.StaysOpen` value. Showing disables the option; the Close button and
  `Closed` event restore it.
- `PopupSampleMatchesWinUIGalleryExample` covers placement, transparency,
  offsets, option state, opening, explicit Close behavior, and restoration.
- The unique heading remains an automation Text peer named `Simple Popup`.
  The Close control remains an automation Button peer named `Close` and
  exposes the Invoke pattern. The live visual proof also requires that unique
  heading to appear after invocation, avoiding false matches with Gallery
  title-bar Close buttons.

## WPF substitution

The sample uses the platform `System.Windows.Controls.Primitives.Popup`; it is
not a new ModernWpf control. WPF hosts an `AllowsTransparency` popup in a
separate native window, while WinUI renders its Popup within the Gallery
window. The source-facing placement, dismissal, offsets, rendered surface,
keyboard-invokable Close action, and exposed content semantics are preserved.

## Verification

The focused sample/source/gate slice passes 3/3 on both
`net8.0-windows7.0` and `net10.0-windows7.0`. The PowerShell parser accepts
both harnesses. Both strict installed-Gallery commands and both `OpenRepeat`
recordings pass, and `ModernWpf.Gallery` builds for net462, net8, and net10.
