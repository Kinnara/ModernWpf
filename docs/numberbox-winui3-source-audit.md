# NumberBox WinUI 3 Source Audit

Date: 2026-07-17

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\controls\dev\NumberBox\NumberBox.cpp`
- `src\controls\dev\NumberBox\NumberBox.h`
- `src\controls\dev\NumberBox\NumberBox.xaml`
- `src\controls\dev\NumberBox\NumberBox_themeresources.xaml`
- `src\controls\dev\NumberBox\NumberBoxAutomationPeer.cpp`
- `src\controls\dev\NumberBox\NumberBoxParser.cpp`
- `src\controls\dev\NumberBox\NumberBoxParser.h`
- `src\controls\dev\Generated\NumberBox.properties.cpp`
- `src\controls\dev\Generated\NumberBox.properties.h`
- `src\controls\dev\NumberBox\APITests\NumberBoxTests.cs`
- `src\controls\dev\NumberBox\InteractionTests\NumberBoxTests.cs`

ModernWpf files:

- `ModernWpf.Controls\NumberBox\NumberBox.cs`
- `ModernWpf.Controls\NumberBox\NumberBox.properties.cs`
- `ModernWpf.Controls\NumberBox\DefaultNumberRounder.cs`
- `ModernWpf.Controls\NumberBox\NumberBox.xaml`
- `ModernWpf.Controls\NumberBox\NumberBoxAutomationPeer.cs`
- `ModernWpf.Controls\NumberBox\NumberBoxParser.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\NumberBox\NumberBoxApiTests.cs`
- `test\ModernWpf.WinUI.Tests\NumberBox\NumberBoxInteractionTests.cs`

## Ported Source Behavior

- Template application now follows the WinUI revoker shape: old part handlers are removed before applying the new template, the popup helper is disposed, and the new parts are re-registered from the fresh template.
- The inner text box now gets a loaded-time spin-placement refresh, matching the WinUI path that re-enters placement after the inner template exists.
- The display rounder now uses source significant-digit behavior (`10`) instead of the old guessed `.NET G12` rounding path.
- The generated `Value` setter now follows WinUI's `NaN` guard so repeated `NaN` assignment does not create a useless binding/update cycle.
- UIA forwarding now follows source `ReevaluateForwardedUIAProperties`: explicit `AutomationProperties.Name`, string `Header`, range status text, and `AutomationProperties.LabeledBy` are forwarded to the inner text box.
- Spin-button enabled state is now owned by source-shaped visual states instead of direct imperative `IsEnabled` writes from code.
- The NumberBox template now uses the source `InputEater`, input text box column span, inline and popup spin glyphs, source spin margins, source repeat-button resource aliases, and source popup indicator glyph.
- Light and Dark theme dictionaries now publish the missing source `TextControlButtonBackground` key used by NumberBox and other text-control button chrome.

## WPF Substitutions

- WPF `Popup`, `ThemeShadowChrome`, and `PopupRepositionHelper` remain the substitute for WinUI popup hosting, theme shadow, translation, and root-bounds behavior. `ThemeShadowChrome` now renders through the shared depth-driven software ThemeShadow renderer rather than the old WPF `BlurEffect` border pair, while preserving the source `NumberBoxPopupShadowDepth=16` resource path and WinUI `GetDropShadowRecipe` padding of `8,4,8,12` for that depth.
- WPF `TextBox` requires `PART_ContentHost`; ModernWpf cannot exactly use the WinUI `ContentElement` scroll viewer or `AutomationProperties.AccessibilityView=Raw` paths.
- Popup spin buttons keep `Focusable=False` in addition to source `IsTabStop=False` because WPF focus movement can close the popup.
- The local `NumberBoxPopupIndicatorMargin` resource remains in the NumberBox dictionary so WPF template-scope `StaticResource` lookup resolves reliably.
- WPF can retain the Light `TextControlButtonForeground` alias when the source-shaped repeat-button style is instantiated inside the NumberBox template. The inline buttons therefore derive the Dark secondary foreground from the resolved NumberBox foreground, and popup buttons receive it through the popup root's inherited text foreground; the substitution is disabled in High Contrast so the system resource alias remains authoritative.
- ModernWpf explicitly sets the inner text-box spin-button column width after entering the visual state because the current WPF `VisualStateEx` setter path does not reliably apply to the non-`FrameworkElement` `ColumnDefinition.Width` target.
- The inner delete-button/TextBox common-state template still uses WPF trigger and `FontIconFallback` substitutions until the shared TextBox source-style port is done.

## Verification

Focused tests cover source template shape, spin-button visual-state setters, inline and popup glyphs and metrics, repeat-button resource aliases, Light/Dark foreground resolution for both button hosts, popup indicator glyph, UIA `LabeledBy` forwarding, source `NaN` setter behavior through existing property tests, and significant-digit input preservation.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~NumberBox" --no-restore`
  - Passed 17/17.
- `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --filter "FullyQualifiedName~NumberBoxSampleMatchesWinUIGalleryExamples|FullyQualifiedName~GalleryVisualChecksEnforceNumberBoxPixelParityThreshold" --no-restore`
  - Passed 2/2 on `net8.0-windows7.0` and `net10.0-windows7.0`.

Strict installed WinUI 3 Gallery comparisons use exact `132x59` resting crops
and separately prove the value interaction from `10` to `20.00`. Both themes
pass the enforced `2.5` primary-crop gate:

- Light: `artifacts\visual-checks\20260717-075724-633-8816\report.md`, primary delta `1.84`, interaction crop delta `1.69`.
- Dark: `artifacts\visual-checks\20260717-075742-058-68924\report.md`, primary delta `1.74`, interaction crop delta `1.02`.

The Dark foreground correction reduced the refreshed resting delta from `2.21`
to `1.74`; more importantly, both chevrons now use WinUI's light secondary
foreground instead of the visually incorrect Light-theme black alias.
