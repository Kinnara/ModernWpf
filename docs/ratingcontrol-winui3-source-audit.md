# RatingControl WinUI 3 Source Audit

Date: 2026-07-17

Scope: existing `RatingControl` and its item-info helpers only. This audit
maps the WPF implementation to local WinUI 3 source and records the WPF
substitutions that remain because the WinUI implementation depends on platform
services that WPF does not expose.

## WinUI 3 Source Baseline

- `src\controls\dev\RatingControl\RatingControl.cpp`
- `src\controls\dev\RatingControl\RatingControl.h`
- `src\controls\dev\RatingControl\RatingControl.xaml`
- `src\controls\dev\RatingControl\RatingControl_themeresources.xaml`
- `src\controls\dev\RatingControl\RatingControlAutomationPeer.cpp`
- `src\controls\dev\RatingControl\RatingItemInfo.cpp`
- `src\controls\dev\RatingControl\RatingItemFontInfo.cpp`
- `src\controls\dev\RatingControl\RatingItemImageInfo.cpp`
- `src\controls\dev\RatingControl\APITests\RatingControlTests.cs`
- `src\controls\dev\RatingControl\InteractionTests\RatingControlTests.cs`

The current `microsoft-ui-xaml` `winui3/main` source was rechecked on
2026-07-17 at `controls\dev\RatingControl\RatingControl.cpp`,
`controls\dev\RatingControl\RatingControl.xaml`,
`controls\dev\RatingControl\RatingControl_themeresources.xaml`, and
`controls\dev\CommonStyles\TextBlock_themeresources.xaml`. The current
caption style still uses the 12px `CaptionTextBlockStyle` based on
`XamlAutoFontFamily`, and the control still computes its width from rating
width, 12px caption spacing, and the caption text block's actual width.

## ModernWpf Port Surface

- `ModernWpf.Controls\RatingControl\RatingControl.cs`
- `ModernWpf.Controls\RatingControl\RatingControl.properties.cs`
- `ModernWpf.Controls\RatingControl\RatingControl.xaml`
- `ModernWpf.Controls\RatingControl\RatingControlAutomationPeer.cs`
- `ModernWpf.Controls\RatingControl\RatingItemImageInfo.cs`
- `ModernWpf\Controls\RatingItemInfo.cs`
- `ModernWpf\Controls\RatingItemFontInfo.cs`
- `ModernWpf\Styles\RatingControl.xaml`
- `ModernWpf\ModernWpfControlsResources.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\RatingControl\RatingControlApiTests.cs`
- `test\ModernWpf.WinUI.Tests\RatingControl\RatingControlInteractionTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| Template sets `MinHeight=32`, binds `LayoutRoot.Background`, names `CaptionStackPanel`, and uses named translate transforms on the foreground/background rating item panels. | Matched with WPF template bindings and `StackPanelEx` for the star panels so source `Spacing` behavior can be represented. |
| Caption uses source margin `4,0,20,0`, `FontSize=12`, `CaptionTextBlockStyle` / `XamlAutoFontFamily`, template-bound foreground, and no fixed height. | Matched with the Segoe UI Variable Small optical face used by the 12px WinUI caption style, exposed as `CaptionControlThemeFontFamily`; tests assert source shape and live template output. |
| Source supports `RatingItemFontInfo` and `RatingItemImageInfo`; there is no `RatingItemPathInfo` in local WinUI 3 source. | Deleted `RatingItemPathInfo`, removed path data templates/resources, and removed the path-specific render branch/tests. |
| `StampOutRatingItems` measures a representative text glyph to compute `m_scaledFontSizeForRendering`; image items use the configured rendering size directly. | Matched with a WPF `TextBlock.Measure` substitute and source-shaped cached resource fields. |
| Source reads `RatingControlFontSizeForRendering`, `RatingControlItemSpacing`, and `RatingControlCaptionTopMargin` once into control fields. | Matched with WPF resource lookup from control resources first, then app resources, including restored `RatingControlCaptionTopMargin` resource keys. |
| Source computes built-in item spacing from the first generated item and applies net spacing to both rating stack panels. | Matched with `StackPanelEx.Spacing`; this is the WPF substitute for WinUI `StackPanel.Spacing`. |
| Source computes total width as rating width plus 12px caption spacing and caption width when caption text is non-empty. | Matched; ModernWpf no longer uses the item spacing resource as caption spacing. A DPI-aware one-physical-pixel correction compensates for WPF rounding the Small-face caption one pixel wider than WinUI at 100% DPI. |
| Source tracks the first item offset on pointer enter and subtracts it during pointer move. | Matched with WPF `TransformToVisual` and mouse event coordinates. |
| Source tracks pointer capture separately from pointer-down state. | Matched with WPF `CaptureMouse`, `LostMouseCapture`, and guarded release. |
| `RatingControlAutomationPeer::IValueProvider_Value` chooses unset, community placeholder, or basic value strings using source resources. | Matched. The WinUI C++ source contains an unused `ratingString` local in this method; the C# port intentionally omits that local so the source-backed build remains warning-free without changing behavior. |

## WPF Substitutions

- WinUI composition expression animation has no direct WPF equivalent here.
  ModernWpf keeps a WPF `ScaleTransform` on each generated item with source
  center-point constants.
- WinUI pointer events include `PointerCanceled` and pointer-device details.
  WPF mouse events do not expose the same model, so cancellation remains a
  platform gap while capture-lost follows the source cleanup shape.
- WinUI gamepad focus engagement and element sounds are platform services.
  ModernWpf keeps keyboard arrow/home/end behavior and documents gamepad/audio
  as WPF gaps rather than guessed behavior.
- WinUI XAML has `TextLineBounds="Tight"` and
  `AutomationProperties.AccessibilityView="Raw"` on template elements. WPF
  does not expose those exact properties in this control template, so they are
  omitted.
- WinUI resolves `XamlAutoFontFamily` to its caption-size optical face. WPF
  does not expose that automatic font selection, so the template uses
  `Segoe UI Variable Small` with `Segoe UI` fallback. WPF rounds the sample
  caption one physical pixel wider; the source-shaped total-width calculation
  removes that framework-only rounding excess in a DPI-aware way.
- WinUI `RatingControlCaptionTopMargin` is loaded by source code but is not
  consumed by the current local WinUI implementation. ModernWpf restores the
  resource key and loads it for source parity, but does not invent new behavior
  for it.

## Validation

Run after the RatingControl source port:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~RatingControl" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
rg -n "RatingItemPathInfo|RatingControlDefaultPathInfo|BackgroundPathDefaultTemplate|ForegroundPathDefaultTemplate" ModernWpf ModernWpf.Controls test\ModernWpf.WinUI.Tests
git diff --check
```

Latest visual verification on 2026-07-17 uses exact `183x32` primary crops:
Light `artifacts\visual-checks\20260717-011121-730-29640\report.md` passes at
`6.23`, and Dark
`artifacts\visual-checks\20260717-011145-649-64488\report.md` passes at `6.90`.
The strict Gallery harness gate is `7.0`; focused API and source-shape tests
pin the Small optical face, DPI-aware width metric, and gate.
