# IconSource / ImageIcon WinUI 3 Source Audit

ModernWpf treats the IconSource family and ImageIcon as a WinUI 3
source-backed WPF port.

Date: 2026-07-18

Source of truth: `D:\repos\microsoft-ui-xaml`

Snapshot used:

```text
de3e767333c2f0717a6a70cb22bd192ced5ad885
winui3/main
```

## WinUI 3 Source Files

- `dxaml/xcp/dxaml/lib/IconSource_Partial.cpp`
- `dxaml/xcp/dxaml/lib/BitmapIconSource_Partial.cpp`
- `dxaml/xcp/dxaml/lib/FontIconSource_Partial.cpp`
- `dxaml/xcp/dxaml/lib/PathIconSource_Partial.cpp`
- `dxaml/xcp/dxaml/lib/SymbolIconSource_Partial.cpp`
- `controls/dev/IconSource/IconSource.idl`
- `controls/dev/IconSource/ImageIconSource.cpp`
- `controls/dev/IconSource/APITests/IconSourceApiTests.cs`
- `controls/dev/ImageIcon/ImageIcon.cpp`
- `controls/dev/ImageIcon/ImageIcon.h`
- `controls/dev/ImageIcon/ImageIcon.idl`
- `controls/dev/ImageIcon/ImageIcon.xaml`
- `controls/dev/ImageIcon/ImageIcon_themeresources.xaml`
- `controls/dev/ImageIcon/APITests/ImageIconTests.cs`
- `controls/dev/ImageIcon/InteractionTests/ImageIconTests.cs`
- `controls/dev/Generated/ImageIcon.properties.cpp`
- `controls/dev/Generated/ImageIconSource.properties.cpp`
- `controls/dev/DllHost/SharedHelpers.cpp`

## ModernWpf Files

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
- `test\ModernWpf.WinUI.Tests\IconSource\IconSourceSourceAuditTests.cs`
- `ModernWpf.Gallery\Pages\StylesSampleFactory.cs`
- `ModernWpf.Gallery\Testing\GalleryDiagnostics.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Current Source Identity And Delta

The IconSource core, concrete source mappings, generated properties, ImageIcon
IDL/XAML/theme resources, and interaction test remain byte-identical to the
earlier `c70471c511a0168b61dcca13af9556465f26b673` audit. Current history adds
one substantive ImageIcon runtime change:

- `16737fe3a48cd8fc9b337a13e1b04e17afd97882` optimizes the native icon visual
  tree so the first child can be the Image directly instead of a Grid wrapping
  an Image. `ImageIcon::OnApplyTemplate` now accepts both shapes and the API
  test reads the direct Image child. ModernWpf already uses one code-built Image
  child, so no ImageIcon visual-tree product change was required.
- `132e2cdd30531603e613bb26b8139722e886a379` only corrects spelling in the
  ImageIcon API-test comment.
- `8463f45162149de0ec3ad7df752596893fe3e13e` moves the source mirror from
  `src/...` to the repository root.
- `9099866fccd3e83e68c5e3adef4e7de6a2fa69ae` moves the unchanged shared
  helper implementation from the former `dll` host to `DllHost` as part of the
  new binary layout. Its ImageIconSource conversion branch remains the source
  contract.

The refresh did expose two ModernWpf behavior gaps, both now fixed:

- WinUI's base `IconSource::CreateIconElement` applies a non-null Foreground to
  the created element after the virtual factory returns. ModernWpf now does the
  same, so a custom IconSource cannot accidentally omit its base Foreground.
- WinUI `SharedHelpers::MakeIconElementFrom` handles ImageIconSource before
  BitmapIconSource and copies ImageSource and Foreground. ModernWpf previously
  omitted ImageIconSource from this source-shaped helper; it now returns a
  fully populated ImageIcon.

Current authoritative blob identities:

| Upstream file | Git blob |
| --- | --- |
| `IconSource.idl` | `8d87db55e78abc2a250ef2c8779abc8512e7e80f` |
| `ImageIconSource.cpp` | `7c76a793ef6a9d4b35540935009366cf35a8f0ba` |
| `IconSourceApiTests.cs` | `54d685cab2057cbefe583951040880abded7b53b` |
| `ImageIcon.cpp` | `39cda55d953431ffa920239051af63a0723ccca7` |
| `ImageIcon.idl` | `c561a116b79220e4f2fb7397c206505e5f38af4b` |
| `ImageIcon.xaml` | `c38bc82ba0eb4d73ed4afcc9be81217b8b7dd554` |
| `ImageIcon_themeresources.xaml` | `50aab881e57c7cb332fd5a91f9a905cfdf9efd16` |
| `ImageIconTests.cs` (API) | `c5f7e4b226893edc836fa9a5c172ff664d0e0feb` |
| `ImageIconTests.cs` (interaction) | `54d6c157ac98a20d0ae481394873598f5510eac4` |
| `Generated/ImageIcon.properties.cpp` | `f6bde6d4ac8b767f1d976e9c544e197a58f0d65f` |
| `Generated/ImageIconSource.properties.cpp` | `a85416ddb82631f0dfa7cfd3bc772e5a0027e684` |
| `IconSource_Partial.cpp` | `e34d4401bf097635cfdc313cd838bbe94985dc64` |
| `BitmapIconSource_Partial.cpp` | `462c68685e47abf17fdb4706e91fe71eb1a1e3bc` |
| `FontIconSource_Partial.cpp` | `72b92a4e41c42db9e56c0d71d0a8bc3543995913` |
| `PathIconSource_Partial.cpp` | `285eb2e87b5317d19f9bb9c4df2022f7b5567a43` |
| `SymbolIconSource_Partial.cpp` | `7944ce05448e81e46833fac3a9d803aa0e2a08d4` |
| `DllHost/SharedHelpers.cpp` | `9eac297bd4547d04f056454a16ac98be405d59b9` |

## Source Alignment

- `IconSource.CreateIconElement()` creates the concrete IconElement, applies a
  non-null base Foreground, stores a weak reference, and propagates mapped
  source-property changes to every still-live created element.
- BitmapIconSource, FontIconSource, PathIconSource, SymbolIconSource, and
  ImageIconSource map their public properties to the matching IconElement
  dependency properties.
- FontIconSource carries Glyph, FontSize, FontFamily, FontWeight, FontStyle,
  IsTextScaleFactorEnabled, and MirroredWhenRightToLeft through creation and
  later property propagation.
- ImageIconSource creates ImageIcon and maps ImageSource to ImageIcon.Source.
- ImageIcon owns a single Image visual, applies Source when the visual is
  created, and refreshes that Image when Source changes. This matches current
  WinUI's optimized direct-Image path while avoiding a compatibility-only Grid.
- `SharedHelpers.MakeIconElementFrom` now includes the WinUI ImageIconSource
  conversion in addition to its font, symbol, bitmap, and path conversions.

## WPF Substitutions

- WinUI generated property metadata maps to WPF dependency-property
  registration.
- FontIcon.IsTextScaleFactorEnabled is exposed and propagated through the WPF
  text-scale attached-property identity, but WPF has no equivalent OS text
  scaling pipeline beyond property inheritance and measure invalidation.
- MirroredWhenRightToLeft maps to a retained WPF `ScaleTransform` on FontIcon
  itself while FlowDirection is right-to-left, matching current WinUI owner
  transform placement and lifetime. The IconElement-family audit owns the
  detailed transform contract.
- AnimatedIconSource remains excluded because ModernWpf does not carry WinUI's
  compositor animated-icon pipeline as a core control.
- ImageIcon uses a code-built WPF Image child instead of a native WinUI control
  template. WPF has no built-in SVG ImageSource, so the Gallery's SVG example
  remains a source-shaped vector DrawingImage adaptation; the bitmap example
  is the cross-framework pixel reference.

## Current WinUI Gallery Coverage

The official Gallery at
`29f62479d5c046a0b854a5868e5a7cd484572d87` has no standalone IconSource page.
It does have the current IconElement page with bitmap and SVG ImageIcon button
examples:

- `WinUIGallery/Samples/IconElement/IconElementPage.xaml`
  (`9f9e42eb762032186daf4781ec3a67db514517e9`)
- `IconElementImageiconBitmapImageButton.txt`
  (`aa1a7827935acb68b401fe4f940f2299f677e4fe`)
- `IconElementImageiconSvgImageButton.txt`
  (`43805bdca2aecf9a6fc49e6f27d02aa2a525bf81`)
- `Assets/SampleMedia/Slices.png`
  (`8c793521c6fbb816483ecbbcc9f87a0acb9e5e7a`)

ModernWpf's generated IconElement page carries all six official examples and
both ImageIcon snippets. The ImageIcon-specific visual case routes both apps to
IconElement but requires the real `ImageExample1` button. It uses a 1000px
capture height so the full button is visible and crops the live WPF UIA button
instead of the detached VisualBrush artifact, whose nested parent offset clips
the lower half.

Strict bitmap ImageIcon evidence uses exact `100x89` button crops, a `2.0`
mean-delta gate, and zero size tolerance:

- Light: `artifacts\visual-checks\20260718-092908-566-20888\report.md` at
  `1.49`.
- Dark: `artifacts\visual-checks\20260718-093011-012-87368\report.md` at
  `1.17`.

The current IconElement BitmapIcon proof remains separate and is owned by
`docs\iconelement-winui3-source-audit.md`: Light
`artifacts\visual-checks\20260719-032510-192-22356\report.md` and Dark
`artifacts\visual-checks\20260719-032607-022-99256\report.md`, both exact
`50x51` crops at `0.02` under the `0.1` gate.

## Tests And Validation

- IconSourceApiTests covers all five implemented source types, defaults,
  source-to-element property transfer, later propagation, weak created-element
  handling, base Foreground application for a custom source, FontIcon RTL/text
  scale flags, ImageIcon loaded visual/source refresh, and shared-helper
  ImageIconSource conversion.
- IconSourceSourceAuditTests pins the current official commit, source blobs,
  base Foreground contract, direct Image child/source refresh, helper branch,
  and strict ImageIcon visual gate.
- Gallery automation tests cover the six-example IconElement page, both
  ImageIcon snippets, bitmap/SVG ImageIcon contents, and the ImageIcon live
  crop route.

Validation commands and final results are recorded in tracker row 8.39.
