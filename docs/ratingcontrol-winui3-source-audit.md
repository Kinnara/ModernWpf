# RatingControl WinUI 3 Source Audit

Date: 2026-07-19

Scope: existing `RatingControl` and its item-info helpers only. This audit
maps the WPF implementation to local WinUI 3 source and records the WPF
substitutions that remain because the WinUI implementation depends on platform
services that WPF does not expose.

## WinUI 3 Source Baseline

The product source of truth is official `microsoft-ui-xaml` `winui3/main`
commit `de3e767333c2f0717a6a70cb22bd192ced5ad885`. The current Gallery authority
is official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`; live comparison uses installed
Microsoft WinUI 3 Controls Gallery `2.9.3.0` with Microsoft Windows App Runtime
`2.2.3.0.0`.

- `controls\dev\RatingControl\RatingControl.cpp`
- `controls\dev\RatingControl\RatingControl.h`
- `controls\dev\RatingControl\RatingControl.idl`
- `controls\dev\RatingControl\RatingControl.xaml`
- `controls\dev\RatingControl\RatingControl_themeresources.xaml`
- `controls\dev\RatingControl\RatingControlAutomationPeer.cpp`
- `controls\dev\RatingControl\RatingItemInfo.cpp`
- `controls\dev\RatingControl\RatingItemFontInfo.cpp`
- `controls\dev\RatingControl\RatingItemImageInfo.cpp`
- `controls\dev\Generated\RatingControl.properties.cpp`
- `controls\dev\Generated\RatingItemFontInfo.properties.cpp`
- `controls\dev\Generated\RatingItemImageInfo.properties.cpp`
- `controls\dev\RatingControl\APITests\RatingControlTests.cs`
- `controls\dev\RatingControl\InteractionTests\RatingControlTests.cs`
- `WinUIGallery\Samples\RatingControl\RatingControlPage.xaml`
- `WinUIGallery\Samples\RatingControl\RatingControlPage.xaml.cs`
- `WinUIGallery\Samples\RatingControl\RatingControlSimple.txt`
- `WinUIGallery\Samples\RatingControl\RatingControlPlaceholder.txt`

Current product blob pins are `27701085117b84f49936435a38c55a63a1e5d8b7`
(runtime), `d3c2ed42a3af30f85cde0afbdb3ff3b04cd49acd` (header),
`b2569922e5e0a86b3ecbf54f6f01b3da922856c5` (IDL),
`6229cfe469d53d5d466f0baead99c19a5cc7eb6b` (template),
`0d9fccdf29a38836bd89fabbc222f6605d104b59` (theme),
`29d7a49d042d56ae4b8e89ad96529ad3b8d14822` (automation peer),
`4dfb0f2e37ae323a694bb5beeb19c34152b290ca` / `3a74cfc06a50a200257aa14ef5dfa0666cb4c96c`
/ `d6caf6dd8688090cfe251163586c0be248e0c302` (item-info runtimes),
`6f185fc8b1644ea5a897e891b5123d352e9c3fc8` / `a0c91f38d45f69ca90d4dbdf5122dacf78c601f9`
/ `4108f8f1cd6726c401562ef5024e202927ada408` (generated properties),
`cf28ae58357cedc734fc92b30b5e84cb7922d88b` (API tests), and
`c5067d1fa63de058feae97891effc033b89dea24` (interaction tests).

Relative to previous product pin
`c70471c511a0168b61dcca13af9556465f26b673`, the only substantive RatingControl
change is `61143cf16f5c0627153ecb1ad0ca1657f02135a7`, which fixes PlaceholderValue
coercion and invalid-MaxRating loaded layout. Commit
`8463f45162149de0ec3ad7df752596893fe3e13e` only moves the source root. There
is no RatingControl perf2026 dictionary. The current caption style still uses
the 12px `CaptionTextBlockStyle` based on `XamlAutoFontFamily`, and the control
still computes width from rating width, 12px caption spacing, and the caption
text block's actual width.

Current Gallery blobs are `7c4a293639c43105aa8d9526ce97beadc34ea8c1`
(page), `dc55def2ac916fbf9464df9fbd9679b78cc772a3` (code-behind),
`085aed50dfcf3459ddcdc36a81ba359a4c4717b7` (simple snippet), and
`15618bfa3921d191216af4a3db3fa527cf7ca190` (placeholder snippet). There is
no RatingControl page/sample change after Gallery conversion commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`.

## ModernWpf Port Surface

- `ModernWpf.Controls\RatingControl\RatingControl.cs`
- `ModernWpf.Controls\RatingControl\RatingControl.properties.cs`
- `ModernWpf.Controls\RatingControl\RatingControl.xaml`
- `ModernWpf.Controls\RatingControl\RatingControlAutomationPeer.cs`
- `ModernWpf.Controls\ModernWpf.Controls.xml`
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
- `test\ModernWpf.Gallery.Tests\RatingControlSourceAuditTests.cs`
- `ModernWpf.Gallery\Pages\BasicInputSampleFactory.cs`
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
| `PlaceholderValue` is a display hint: zero and fractional values from 0 through 1 remain valid, negative values become the `-1` unset sentinel, and values above `MaxRating` clamp to the maximum. `Value` retains its minimum non-sentinel value of 1. | Matched with separate `CoercePlaceholderValueBetweenMinAndMax` and `CoerceValueBetweenMinAndMax` paths. API tests pin `0`, `0.1`, `0.5`, negative sentinel, and upper-bound behavior. |
| Invalid `MaxRating` is committed as 1 before `Value` and `PlaceholderValue` are re-coerced, preventing a loaded control from briefly computing layout against a negative maximum. | Matched. A rendered `TestWindowHost` regression assigns `MaxRating=-2` while loaded, forces layout, and verifies all three values settle at 1 without an exception. |
| The current Gallery placeholder example two-way binds its zero-valued slider directly to `PlaceholderValue`. | Matched. The obsolete WPF adapter that converted slider zero back to `-1` is removed; the local sample initializes and preserves `PlaceholderValue=0`, then forwards every slider value directly. |
| Source tracks the first item offset on pointer enter and subtracts it during pointer move. | Matched with WPF `TransformToVisual` and mouse event coordinates. |
| Source tracks pointer capture separately from pointer-down state. | Matched with WPF `CaptureMouse`, `LostMouseCapture`, and guarded release. |
| `RatingControlAutomationPeer::IValueProvider_Value` chooses unset, community placeholder, or basic value strings using source resources. | Matched. The WinUI C++ source contains an unused `ratingString` local in this method; the C# port intentionally omits that local so the source-backed build remains warning-free without changing behavior. |

## WPF Substitutions

- WinUI composition expression animation has no direct WPF equivalent here.
  ModernWpf keeps a WPF `ScaleTransform` on each generated item with source
  center-point constants.
- WinUI pointer events include `PointerCanceled` and pointer-device details.
  WPF mouse events do not expose the same model, so cancellation remains a
  platform gap while capture-lost follows the source cleanup shape. For the
  same reason, the Gallery option text says `Click again to clear your rating.`
  instead of advertising WinUI's unsupported swipe-left gesture.
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
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~RatingControl" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
dotnet build .\ModernWpf.Gallery\ModernWpf.Gallery.csproj --no-restore -m:1
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls RatingControl -Reference InstalledWinUI3Gallery -Theme Light -IncludeInteractions -FailOnDifference
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls RatingControl -Reference InstalledWinUI3Gallery -Theme Dark -IncludeInteractions -FailOnDifference
rg -n "RatingItemPathInfo|RatingControlDefaultPathInfo|BackgroundPathDefaultTemplate|ForegroundPathDefaultTemplate" ModernWpf ModernWpf.Controls test\ModernWpf.WinUI.Tests
git diff --check
```

Fresh gate-enforced live verification on 2026-07-19 uses exact `183x32`
primary crops and exact `203x52` selected-value interaction crops. Light
`artifacts/visual-checks/20260719-025609-047-47084/report.md` passes at `6.23`
static / `2.80` selected; Dark
`artifacts/visual-checks/20260719-025640-167-62492/report.md` passes at `6.90`
/ `4.35`. The harness enforces static delta `<=7.0`, interaction delta
`<=5.0`, and zero size tolerance for both crops. Fresh Light/Dark value
recordings `artifacts/gallery-recordings/20260719-025706-701/report.md` and
`artifacts/gallery-recordings/20260719-025724-041/report.md` pass, reach value
3 through UIA RangeValue, and show local deltas `3.856` / `4.11`. Focused
product/source tests pass 16/16; focused Gallery current-source/sample/gate
tests pass 4/4 on net8 and net10. Controls and Gallery build on net462/net8/
net10 with zero errors; Controls retains 18 unrelated net462 warnings and
Gallery is warning-free. Generated dependency properties, both PowerShell
parsers, and the scoped diff check pass.
