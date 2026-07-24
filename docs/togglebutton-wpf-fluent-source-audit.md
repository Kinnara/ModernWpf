# ToggleButton Current-Source and WinUI Gallery Audit

Date: 2026-07-18

`ToggleButton` uses current official dotnet/wpf Fluent as the stock
control/template authority and current microsoft/WinUI-Gallery as the example,
checked-state, output, and accessibility authority.

## Current Official WPF Fluent Source

The official `dotnet/wpf` `main` revision audited was
`83e6cbda760818a2ab885c4aa3fc7e3a39eedf58` (2026-07-16). The local checkout
is `7f005faa89e79b0b1fa1cb2c21283bab7916c092` (2026-04-30), but all Fluent
theme blobs used by ToggleButton are byte-identical to current `main`:

| Source | Current/local blob |
| --- | --- |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.xaml` | `1c021505727a0f1011525e6b1512e770b2bf4044` |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.Light.xaml` | `b58c45efda2d74cded8732f04210ccd2a959456d` |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.Dark.xaml` | `44fd798a76c9c1dc08e8446ac1e94a43924c764a` |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.HC.xaml` | `78db46a8a9b4b78e07b29073320ecb6ed89f715f` |

The latest substantive Fluent-file revision remains
`732f215ba8c2d79d10b9c51f2b80833847de94df` (2025-12-30). No current stock
ToggleButton source drift remains to port.

## Stock Control and Template Mapping

ModernWpf maps the source through:

- `ModernWpf/Styles/ToggleButton.xaml`
- `ModernWpf/ThemeResources/Light.xaml`
- `ModernWpf/ThemeResources/Dark.xaml`
- `ModernWpf/ThemeResources/HighContrast.xaml`
- `test/ModernWpf.WinUI.Tests/CommonStyles/ToggleButtonVisualStateTests.cs`
- `test/ModernWpf.WinUI.Tests/LayoutCompatibility/LayoutCompatibilityApiTests.cs`
- `test/ModernWpf.WinUI.Tests/TemplateParityTests.cs`

The port keeps the source `11,5,11,6` padding, one-pixel border, natural 32px
height, `ContentBorder`, stock WPF `ContentPresenter`, normal font weight,
content alignment, corner radius, and seven native `MultiTrigger` paths for
enabled/disabled, checked/unchecked, pointer-over, and pressed combinations.
The stock WPF control owns nullable `IsChecked`, click/toggle input, and the
standard `ToggleButtonAutomationPeer` Toggle provider.

Documented substitutions remain intentional:

- the downlevel-compatible system focus visual plus `FocusVisualHelper`
  replaces current WPF's `DefaultControlFocusVisualStyle` key;
- public ModernWpf ToggleButton brush aliases retain the WinUI Light, Dark,
  and High Contrast resource surface;
- `ToggleButtonForegroundCheckedDisabled` maps to the brush-valued
  `TextOnAccentFillColorDisabledBrush` required by the source foreground
  setter;
- `Stylus.IsPressAndHoldEnabled=False` preserves the existing WinUI-style
  activation path on older WPF targets;
- source has no separate indeterminate visual branch, so `IsChecked=null`
  intentionally follows the official WPF Fluent fallback.

## Current WinUI Gallery Source

The current Gallery authority is official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13):

- `WinUIGallery/Samples/ToggleButton/ToggleButtonPage.xaml`
- `WinUIGallery/Samples/ToggleButton/ToggleButtonPage.xaml.cs`
- `WinUIGallery/Samples/ToggleButton/ToggleButtonSimple.txt`

The local Gallery checkout is
`1d490ef14f96d5c52de253b94063168eecde08e9`. Commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` moved the sample to the current
folder shape. Commit `18bdae8f30ddb1df6d6d25b9a2eb3d41a6590e56` only enabled
nullable reference types, and `673179befaa1477459ac6e93ab8a3473a6bf7a07` was
code cleanup; neither changes the live ToggleButton contract.

The ModernWpf sample matches current source:

- `Toggle1` with text content `ToggleButton` and the disable option;
- initial unchecked state and output `Off`;
- checked output `On` and unchecked output `Off`;
- source header and `ToggleButtonSimple.txt` snippet;
- output node `Control1Output` and the current ControlExample output layout.

ModernWpf adds a stable `GallerySample_ToggleButton_Output` automation ID for
the harness while WinUI exposes the source ID `Control1Output`.

## Pixel, Behavior, and Accessibility Lock

The harness already toggled both applications through UIA and compared the
checked button, but the comparison was diagnostic. It now also reacquires each
application's output Text peer and requires `On`, so a visual state change can
no longer pass while the source click-output behavior is broken.

Strict gates require:

- exact official reference source `Toggle1`;
- resting delta `<=3.0` and exact `107x32` geometry;
- checked-state delta `<=7.0`, common UIA crop source, and exact `127x52`
  padded interaction geometry;
- UIA state transition Off to On and output transition Off to On in both apps.

Final strict evidence:

- Light: `artifacts/visual-checks/20260718-041149-473-44704/report.md` —
  resting `2.98`, checked `6.31`, exact `107x32` / `127x52` sizes.
- Dark: `artifacts/visual-checks/20260718-041222-924-22376/report.md` —
  resting `2.62`, checked `6.98`, exact `107x32` / `127x52` sizes.

The bounded checked-state remainder is native WPF/WinUI text and rounded-edge
rasterization; background, border, geometry, accent state, and text position
align.

## Regression Coverage

- `ToggleButtonVisualStateTests`, `LayoutCompatibilityApiTests`, and
  `TemplateParityTests` pin the stock Fluent template, resources, checked
  states, presenter slot, nullable-state fallback, and runtime rendering.
- `GalleryAutomationHookTests.ToggleButtonSampleMatchesWinUIGalleryExample`
  pins source content/output behavior, Button/Text roles, and Toggle provider.
- `ToggleButtonSourceAuditTests` pins both current source identities and the
  strict visual/behavior gates.
- `WpfGallerySourceShapeTests` pins official reference selection, app-specific
  output lookup, required output transition, and exact-size thresholds.
