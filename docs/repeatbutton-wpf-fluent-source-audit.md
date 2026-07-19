# RepeatButton Current-Source and WinUI Gallery Audit

Date: 2026-07-18

`RepeatButton` has two authorities in this port:

- the stock control/template authority is current official dotnet/wpf Fluent;
- the example, interaction, and accessibility authority is current
  microsoft/WinUI-Gallery.

## Current Official WPF Fluent Source

The official `dotnet/wpf` `main` revision audited was
`83e6cbda760818a2ab885c4aa3fc7e3a39eedf58` (2026-07-16). The local source
checkout is `7f005faa89e79b0b1fa1cb2c21283bab7916c092` (2026-04-30), but every
Fluent theme blob used by RepeatButton is byte-identical to current `main`:

| Source | Current/local blob |
| --- | --- |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.xaml` | `1c021505727a0f1011525e6b1512e770b2bf4044` |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.Light.xaml` | `b58c45efda2d74cded8732f04210ccd2a959456d` |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.Dark.xaml` | `44fd798a76c9c1dc08e8446ac1e94a43924c764a` |
| `src/Microsoft.DotNet.Wpf/src/Themes/PresentationFramework.Fluent/Themes/Fluent.HC.xaml` | `78db46a8a9b4b78e07b29073320ecb6ed89f715f` |

The latest substantive Fluent-file revision remains
`732f215ba8c2d79d10b9c51f2b80833847de94df` (2025-12-30). There is no
unported current-source drift after the earlier stock-control conversion.

## Stock Control and Template Mapping

ModernWpf maps the official Fluent control through:

- `ModernWpf/Styles/RepeatButton.xaml`
- `ModernWpf/ThemeResources/Light.xaml`
- `ModernWpf/ThemeResources/Dark.xaml`
- `ModernWpf/ThemeResources/HighContrast.xaml`
- `test/ModernWpf.WinUI.Tests/CommonStyles/RepeatButtonVisualStateTests.cs`
- `test/ModernWpf.WinUI.Tests/LayoutCompatibility/LayoutCompatibilityApiTests.cs`

The port keeps the source `11,5,11,6` padding, one-pixel border, 32px natural
button height, `ContentBorder`, stock WPF `ContentPresenter`, normal font
weight, content alignment, corner radius, and native `IsEnabled`,
`IsMouseOver`, and `IsPressed` trigger model. WPF continues to own `Delay`,
`Interval`, repeat timing, keyboard/stylus input, and the standard
`RepeatButtonAutomationPeer` Invoke provider.

Documented port substitutions remain intentional:

- the downlevel-compatible system focus visual and `FocusVisualHelper` bridge
  replace current WPF's `DefaultControlFocusVisualStyle` resource;
- `RepeatButtonBorderBrush` remains a public WinUI resource alias to
  `ControlElevationBorderBrush` in Light and Dark and carries the existing
  WinUI High Contrast token mapping;
- trigger chrome setters target `ContentBorder` directly while foreground
  targets `ContentPresenter`, retaining the WinUI resource surface without
  changing rendered Fluent states;
- `Stylus.IsPressAndHoldEnabled=False` preserves the existing WinUI-style
  activation path on older WPF targets.

## Current WinUI Gallery Source

The current Gallery authority is official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13):

- `WinUIGallery/Samples/RepeatButton/RepeatButtonPage.xaml`
- `WinUIGallery/Samples/RepeatButton/RepeatButtonPage.xaml.cs`
- `WinUIGallery/Samples/RepeatButton/RepeatButtonSimple.txt`

The local Gallery checkout is
`1d490ef14f96d5c52de253b94063168eecde08e9`. Commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` moved the sample to the current
path. Commit `b97ceb1ef7504631a9d2a7d5b46292f6f6a0e47a` added the current polite
live-region announcement; that behavior remains present on current `main`.

The port matches the live example:

- one horizontal row containing `Click and hold`;
- `Control1Output` with an 8px left margin and centered vertical alignment;
- automation name `Control output` and polite live setting;
- click text `Number of clicks: N`;
- a `LiveRegionChanged` automation event after each update;
- the source `RepeatButtonSimple.txt` snippet and disable-option binding.

The WPF harness additionally mirrors the rendered output into HelpText so the
cross-framework recorder can read the update reliably; this does not replace
the visible text or live-region event.

## Pixel and Interaction Lock

The former output comparison used
`GallerySample_RepeatButton_Output` for both applications. WinUI exposes that
node as `Control1Output`, so its lookup failed and the reference crop silently
fell back to the button. The reported `158x59` versus `152x72` images therefore
measured different elements.

The harness now resolves each application's real output ID and captures the
same source row as `RepeatButtonOutputRow`: a fixed `240x32` viewport anchored
to the exact `112x32` button and covering the source 8px gap plus click count.
This avoids the two-pixel native UIA text-bound difference while retaining all
rendered button and output pixels.

Strict gates require:

- official reference trigger `Click and hold`;
- static delta `<=4.0` and exact `112x32` geometry;
- output-row delta `<=11.0`, exact `240x32` geometry, and common crop source;
- a visible click-output change in both applications.

The larger output-row allowance is bounded native WPF/WinUI text rasterization:
the backgrounds, row origin, button geometry, 8px gap, and text baseline match.
An explicit WPF `Display` formatting experiment increased Light delta from
`8.79` to `10.63`, so it was rejected.

Final strict evidence:

- Light: `artifacts/visual-checks/20260718-035917-557-37488/report.md` —
  static `3.23`, output row `8.79`, exact `112x32` / `240x32` sizes.
- Dark: `artifacts/visual-checks/20260718-035943-186-33836/report.md` —
  static `2.55`, output row `10.71`, exact `112x32` / `240x32` sizes.

## Regression Coverage

- `RepeatButtonVisualStateTests` pins the stock Fluent template, resources,
  state triggers, content slot, and disabled rendering.
- `LayoutCompatibilityApiTests` pins the stock WPF presenter/template shape.
- `GalleryAutomationHookTests.RepeatButtonSampleMatchesWinUIGalleryExample`
  pins source content, output updates, live settings, Text/Button automation
  roles, and the Invoke pattern.
- `RepeatButtonSourceAuditTests` pins both current source identities and the
  strict visual/interaction contract.
- `WpfGallerySourceShapeTests` pins the app-specific output lookup, common row
  crop, strict thresholds, and exact-size gates.
