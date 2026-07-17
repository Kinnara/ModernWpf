# IconSource / ImageIcon WinUI 3 Source Audit

Source of truth: `D:\repos\microsoft-ui-xaml`

WinUI 3 files audited:

- `src\dxaml\xcp\dxaml\lib\IconSource_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\BitmapIconSource_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\FontIconSource_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\PathIconSource_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\SymbolIconSource_Partial.cpp`
- `src\controls\dev\IconSource\ImageIconSource.cpp`
- `src\controls\dev\IconSource\IconSource.idl`
- `src\controls\dev\ImageIcon\ImageIcon.cpp`
- `src\controls\dev\ImageIcon\ImageIcon.xaml`
- `src\controls\dev\IconSource\APITests\IconSourceApiTests.cs`
- `src\controls\dev\ImageIcon\APITests\ImageIconTests.cs`

Current-source and Gallery visual behavior were rechecked on 2026-07-17.
The official Gallery's first IconElement example still renders the 50px
`SlicesIcon` `BitmapIcon` with `ShowAsMonochrome=False`, while its descriptive
paragraph uses `BodyTextBlockStyle` and is not part of the icon control.

ModernWpf files:

- `ModernWpf\IconSource\IconSource.cs`
- `ModernWpf\IconSource\BitmapIconSource.cs`
- `ModernWpf\IconSource\FontIconSource.cs`
- `ModernWpf\IconSource\PathIconSource.cs`
- `ModernWpf\IconSource\SymbolIconSource.cs`
- `ModernWpf\IconSource\ImageIconSource.cs`
- `ModernWpf\IconElement\FontIcon.cs`
- `ModernWpf\IconElement\ImageIcon.cs`
- `ModernWpf.Controls\Common\SharedHelpers.cs`
- `test\ModernWpf.WinUI.Tests\IconSource\IconSourceApiTests.cs`
- `ModernWpf.Gallery\Pages\StylesSampleFactory.cs`
- `ModernWpf.Gallery\Testing\GalleryDiagnostics.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Source-Backed Behavior

ModernWpf now follows the WinUI 3 IconSource / ImageIcon source shape for existing icon types:

- `IconSource.CreateIconElement()` creates the concrete icon element, applies a non-null `Foreground`, tracks created elements weakly, and propagates subsequent source-property changes to live created elements.
- `BitmapIconSource`, `PathIconSource`, `SymbolIconSource`, and `ImageIconSource` map source properties to the corresponding icon-element dependency properties.
- `FontIconSource` now carries WinUI's `IsTextScaleFactorEnabled` and `MirroredWhenRightToLeft` properties instead of leaving the source propagation commented out.
- Created `FontIcon` instances receive `Glyph`, `FontSize`, `FontFamily`, `FontWeight`, `FontStyle`, `IsTextScaleFactorEnabled`, and `MirroredWhenRightToLeft` from `FontIconSource`.
- Existing `FontIconSource` instances propagate later `IsTextScaleFactorEnabled` and `MirroredWhenRightToLeft` changes to already-created `FontIcon` instances.
- `SharedHelpers.MakeIconElementFrom(FontIconSource)` now copies the same source property set.
- `ImageIcon` applies its `Source` to the template image and refreshes that image when `Source` changes.

## WPF Substitutions

- WinUI generated property metadata maps to WPF dependency-property registration.
- WinUI `FontIcon.IsTextScaleFactorEnabled` is exposed and propagated through the existing WPF text-scale attached-property identity, but WPF has no OS text-scale factor pipeline to apply beyond property inheritance and measure invalidation.
- WinUI `MirroredWhenRightToLeft` maps to a WPF `ScaleTransform(-1, 1)` on the internal text block while `FlowDirection` is right-to-left.
- `AnimatedIconSource` remains excluded because ModernWpf does not carry WinUI's compositor animated-icon pipeline as a core control.
- `ImageIcon` uses the existing code-built WPF visual child instead of a WinUI control template, but keeps the source single-image behavior and update hook.

## Test Coverage

ModernWpf covers the source-backed slice with:

- upstream-derived IconSource API tests for symbol, font, bitmap, path, and image icon sources;
- source foreground and source-property propagation to created icon elements;
- `FontIconSource` propagation for `IsTextScaleFactorEnabled` and `MirroredWhenRightToLeft`;
- source `FontIcon` mirroring behavior under right-to-left flow direction;
- `SharedHelpers.MakeIconElementFrom(FontIconSource)` copying the full source property set;
- ImageIcon source application and loaded visual-child smoke coverage.

## Installed Gallery Pixel Verification

The visual harness now compares the rendered `SlicesIcon` control on both
sides, rather than treating the surrounding 590x118 sample paragraph as the
IconElement primary crop. `GalleryDiagnostics` uses the parent-offset viewbox
for the WPF BitmapIcon so its image pixels survive the isolated artifact
render; the reference crop locates the same 50px bitmap immediately below the
official example's body text. The ModernWpf paragraph also explicitly consumes
the source `BodyTextBlockStyle`.

Strict 2026-07-17 evidence uses matching `50x51` rendered crops and passes at
`0.02` in both themes:

- Light: `artifacts\visual-checks\20260717-012319-346-86980\report.md`
- Dark: `artifacts\visual-checks\20260717-012349-247-72408\report.md`

The primary-crop regression gate is `0.1`. The old 12-13 whole-example delta
was descriptive WPF/WinUI paragraph line-breaking, not IconElement pixels.
