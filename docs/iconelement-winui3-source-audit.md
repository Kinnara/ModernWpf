# IconElement WinUI 3 Source Audit

Date: 2026-07-19

Scope: `IconElement`, `FontIcon`, `SymbolIcon`, `PathIcon`, and `BitmapIcon`.
`IconSource` and `ImageIcon` remain covered by
`docs\iconsource-imageicon-winui3-source-audit.md`; this audit cross-checks
`FontIconSource` only where its defaults create a `FontIcon`.

## WinUI 3 Source Baseline

The product source of truth is official `microsoft-ui-xaml` `winui3/main`
commit `de3e767333c2f0717a6a70cb22bd192ced5ad885`. The current Gallery authority
is official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`; live comparison uses installed
Microsoft WinUI 3 Controls Gallery `2.9.3.0` with Microsoft Windows App Runtime
`2.2.3.0.0`.

Audited product sources:

- `dxaml\xcp\core\core\elements\icon.cpp`
- `dxaml\xcp\core\inc\icon.h`
- `dxaml\xcp\dxaml\lib\SymbolIcon_Partial.cpp`
- `dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs`
- `dxaml\xcp\dxaml\lib\winrtgeneratedclasses\IconElement.g.cpp`
- `dxaml\xcp\dxaml\lib\winrtgeneratedclasses\FontIcon.g.cpp`
- `dxaml\xcp\dxaml\lib\winrtgeneratedclasses\SymbolIcon.g.cpp`
- `dxaml\xcp\dxaml\lib\winrtgeneratedclasses\PathIcon.g.cpp`
- `dxaml\xcp\dxaml\lib\winrtgeneratedclasses\BitmapIcon.g.cpp`
- `dxaml\test\native\external\controls\bitmapicon\BitmapIconIntegrationTests.cpp`
- `dxaml\test\native\external\controls\fonticon\FontIconIntegrationTests.cpp`
- `dxaml\test\native\external\controls\pathicon\PathIconIntegrationTests.cpp`
- `dxaml\test\native\external\controls\symbolicon\SymbolIconIntegrationTests.cpp`
- `dxaml\test\native\external\controls\iconsourceelement\IconSourceElementIntegrationTests.cpp`

Current product blob pins are `7ae8225654f1d98d0da2e0a7535c32b35384ebe6`
(core runtime), `bf5816e78627acf0cb70febd8dcb3dc07df0703f` (header),
`e229584245b82f1b4977fc72799ad85725f5a138` (SymbolIcon partial),
`ad1199a7ff9c253e38c4fb922accbe0afffbf432` (XamlOM),
`2072b77aebb2ba7ecc7c467248bb1af8316d9dd8` / `9a3c8a1294ef37f0570ca0d9d718eec0359d4b52`
/ `f0be781e8dfffcb964c575c88084327ec1cdf8eb` /
`4ac500d69242e635ad9f51b2b957461a1a790daf` /
`18d035e8f53659724da4de75012300a23dd3c1d0` (generated IconElement, FontIcon,
SymbolIcon, PathIcon, and BitmapIcon), and
`13f932583c1330db3774ea5b6658b636e7071e01` /
`34bc5d1a7cfdb9d45b17763c738cba88ce9b55d0` /
`b184c5548d001a7c70fe18fcda1b04e2c2dc49bf` /
`2892f233766aa4c7fa223c28e16f04cce352221d` (the four integration suites).

Relative to previous product pin
`c70471c511a0168b61dcca13af9556465f26b673`, commit
`16737fe3a48cd8fc9b337a13e1b04e17afd97882` is the only substantive family
change. It opt-in optimizes current WinUI's `FontIcon` TextBlock and
`BitmapIcon` Image to be direct children instead of children of an extra Grid.
Commit `8463f45162149de0ec3ad7df752596893fe3e13e` only moves the source root.

Current Gallery sources are:

- `WinUIGallery\Samples\IconElement\IconElementPage.xaml`
- `WinUIGallery\Samples\IconElement\IconElementPage.xaml.cs`
- the six `WinUIGallery\Samples\IconElement\IconElement*.txt` definitions

Their blobs are `9f9e42eb762032186daf4781ec3a67db514517e9` (page),
`c1e7032d52401d1e433ec94d405af5f6a927fe91` (code-behind), and
`4f149a7754aafc5cc7fb0e7c498199a15200619d` /
`433e47644730e1d2ad5cfbd151376ee5c9448575` /
`aa1a7827935acb68b401fe4f940f2299f677e4fe` /
`43805bdca2aecf9a6fc49e6f27d02aa2a525bf81` /
`bdefb4e767635ddcc99e4f02836ce57c35b07121` /
`4c096a248c4a42b32829a16fd4557608746c1696` (BitmapIcon, FontIcon, bitmap
ImageIcon, SVG ImageIcon, PathIcon, and SymbolIcon definitions). There is no
IconElement page/sample change after Gallery conversion commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`.

## ModernWpf Port Surface

- `ModernWpf\IconElement\IconElement.cs`
- `ModernWpf\IconElement\IconElementConverter.cs`
- `ModernWpf\IconElement\FontIcon.cs`
- `ModernWpf\IconElement\SymbolIcon.cs`
- `ModernWpf\IconElement\Symbol.cs`
- `ModernWpf\IconElement\PathIcon.cs`
- `ModernWpf\IconElement\BitmapIcon.cs`
- `ModernWpf\IconSource\FontIconSource.cs`
- `ModernWpf.Gallery\Pages\StylesSampleFactory.cs`
- `test\ModernWpf.WinUI.Tests\IconElement\IconElementApiTests.cs`
- `test\ModernWpf.WinUI.Tests\IconElement\IconElementSourceAuditTests.cs`
- `test\ModernWpf.Gallery.Tests\IconElementSourceAuditTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`
- `tools\visual-checks\Record-GalleryControlInteractions.ps1`

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| `IconElement` inherits Foreground, owns one rendered child, and measures/arranges to that child. Parsing an icon from a string creates a `SymbolIcon`. | Matched with the inherited WPF text Foreground property, a private layout host, one logical icon child, and `IconElementConverter`. |
| The public `Symbol` enum retains legacy values, but `ConvertSymbolValueToGlyph` remaps all 197 legacy entries to current Segoe Fluent Icons codepoints and falls back to the raw value for newer entries. | Matched exactly. A mechanical audit compares all 197 source/local pairs, while API tests pin representative low, high, unchanged, default, and fallback cases. ModernWpf no longer casts the legacy enum value directly to a character. |
| `FontIcon` defaults to `Segoe Fluent Icons,Segoe MDL2 Assets`, 20px, normal style/weight, text scaling enabled, and mirroring disabled. FontStyle and FontWeight inherit from parent text formatting when not locally set. | Matched. `FontIconSource` uses the same exact fallback so creating a default FontIcon cannot replace the current default with MDL2-only behavior. WPF AddOwner metadata supplies FontStyle/FontWeight inheritance. |
| RTL mirroring creates a scale transform on `FontIcon` itself. Once created, the transform remains and its X scale changes between -1 and 1 as flow/mirroring changes. | Matched. The internal TextBlock is not transformed independently and the retained-transform lifetime is covered by API tests. |
| `PathIcon` creates a Path with horizontal/vertical Stretch alignment but does not locally set `Path.Stretch`; the platform default therefore remains `None`. | Matched. The old forced `Stretch.Uniform` assignment is removed and a rendered test guards the source shape. |
| `BitmapIcon` defaults `ShowAsMonochrome=true`, tracks URI/foreground changes, shows the original image in color mode, and recolors visible source pixels in monochrome mode. | Matched at the public/property/rendering boundary through WPF Image and opacity-mask primitives. The Gallery toggle and live recordings guard both modes. |
| IconElement's internal glyph TextBlocks are `AccessibilityView.Raw`, and IconElement does not create its own standalone automation peer. | The WPF controls intentionally create no standalone peer; enclosing buttons/items own accessible names. WPF has no exact Raw accessibility-view property for these private children. |

## WPF Substitutions

- Current WinUI can opt in to direct TextBlock/Image children for FontIcon and
  BitmapIcon. ModernWpf retains one transparent private Grid because WPF's
  custom `FrameworkElement` visual-child contract and the existing
  foreground-inheritance bridge use that host. It is not exposed through the
  public API; exact-size live crops prove no pixel/layout regression.
- WinUI recolors a decoded writable bitmap in place. WPF uses the original
  Image plus an ImageBrush opacity mask and foreground Rectangle. This keeps
  URI changes, alpha, inherited foreground, color/monochrome switching, and
  rendered output without reproducing WinUI's native image pipeline.
- WinUI's `IsTextScaleFactorEnabled` integrates with the OS text-scale service.
  ModernWpf preserves and propagates the property, but WPF has no matching
  per-element WinUI text-scale hook.
- WinUI sets private glyph TextBlocks to AccessibilityView Raw. WPF lacks that
  property; the public IconElement peer boundary is matched and host controls
  remain responsible for accessible names.
- Segoe Fluent Icons availability and fallback selection ultimately remain
  platform font-resolution behavior. The exact WinUI fallback family string
  is used so supported Windows versions resolve Fluent first and MDL2 second.

## Validation

Run after the IconElement current-source port:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~IconElement|FullyQualifiedName~IconSource" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~IconElement" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
dotnet build .\ModernWpf.Gallery\ModernWpf.Gallery.csproj --no-restore -m:1
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls IconElement -Reference InstalledWinUI3Gallery -Theme Light -IncludeInteractions -FailOnDifference
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls IconElement -Reference InstalledWinUI3Gallery -Theme Dark -IncludeInteractions -FailOnDifference
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls IconElement -Theme Light
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls IconElement -Theme Dark
git diff --check
```

Fresh fully live Light
`artifacts/visual-checks/20260719-032510-192-22356/report.md` and Dark
`artifacts/visual-checks/20260719-032607-022-99256/report.md` both pass at
`0.02`, with exact `50x51` rendered BitmapIcon crops under the strict `0.1`
gate and zero size tolerance. Fresh Light/Dark monochrome-option recordings
`artifacts/gallery-recordings/20260719-032648-459/report.md` and
`artifacts/gallery-recordings/20260719-032713-648/report.md` pass, with local
render deltas `4.336` / `3.582` proving the option changed pixels. Shared
SymbolIcon consumers are spot-checked by AppBarButton: Light
`artifacts/visual-checks/20260719-032741-289-92820/report.md` and Dark
`artifacts/visual-checks/20260719-032800-286-98924/report.md` pass their strict
static and interaction gates after the mapping correction.
Focused product/source coverage passes 23/23. Focused current Gallery/sample/
gate coverage passes 4/4 on both net8 and net10. Controls and Gallery build on
net462/net8/net10 with zero errors; Controls reports 20 existing unrelated
net462 warnings and Gallery is warning-free. The dependency-property generator,
both edited PowerShell parsers, the exact 197-entry source/local mapping check,
and the scoped diff check pass.
