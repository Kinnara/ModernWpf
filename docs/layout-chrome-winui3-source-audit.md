# Layout Chrome WinUI 3 Source Audit

Date: 2026-07-19

Scope: the WPF adapters for WinUI Border, Grid, StackPanel,
ContentPresenter, ContentControl text surface, rounded chrome, spacing, layout
clipping/hit testing, and StackPanel snap points.

## Current Product Baseline

The source of truth is official `microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`. Current authoritative inputs are:

| Official source | Current blob |
| --- | --- |
| `dxaml\xcp\core\core\elements\Border.cpp` | `3f662f6ce1158b1fccae058a5b825467b598ec5b` |
| `dxaml\xcp\core\core\elements\Grid.cpp` | `4a50cd9ff83c35528008882cadbf69c3ded571bd` |
| `dxaml\xcp\core\core\elements\StackPanel.cpp` | `0b9e007cec1ce9bd6951da7b56ec017fb4175ce1` |
| `dxaml\xcp\core\core\elements\ContentPresenter.cpp` | `63ed1d27c9575ef452b8c1275b059a55ef7c1a89` |
| `dxaml\xcp\core\core\elements\framework.cpp` | `08cf1b3e1a94cf5d07c696f34977ac357c548e61` |
| `dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs` | `ad1199a7ff9c253e38c4fb922accbe0afffbf432` |
| Border integration tests | `1cf9aa4a290f5c37d7934529f166c76275a4e082` |
| Grid integration tests | `6081798c3acda10eb2396f16c93a72b1093355bd` |
| StackPanel integration tests | `22d5c4696e9b81675bb33088518bed08ab22f656` |
| ContentPresenter integration tests | `0d03248799283623615e12916585e11e30868adc` |

Relative to previous product baseline
`c70471c511a0168b61dcca13af9556465f26b673`, Border, Grid, StackPanel,
ContentPresenter, XamlOM, and all four integration suites are byte-identical.
Commit `8463f45162149de0ec3ad7df752596893fe3e13e` only moves the source root.

`framework.cpp` changed from blob
`b2d1553db8cb78bfcd8d23d81f402aebbf3b5469` to the current blob through
framework-wide performance/lifecycle work: cached type bits, vector-backed
event/template-binding lists, no-ref resource access, resource-lookup tracing,
safer ApplyTemplate error cleanup, and the `OptimizeApplyStyles` opt-in that
defers or skips unnecessary style/setter realization. Those commits do not
change border/background geometry, rounded layout clips, Grid spacing,
StackPanel spacing/snap points, ContentPresenter text forwarding, or the
app-visible layout contract mapped by these adapters. WPF owns corresponding
style/resource/template lifecycle through its platform engine; no ModernWpf
layout-chrome patch is justified.

## ModernWpf Port Surface

- `ModernWpf\Controls\BorderEx.cs`
- `ModernWpf\Controls\GridEx.cs`
- `ModernWpf\Controls\StackPanelEx.cs`
- `ModernWpf\Controls\ContentPresenterEx.cs`
- `ModernWpf\Controls\ContentControlEx.cs`
- `ModernWpf\Controls\LayoutChromeHelper.cs`
- `ModernWpf\Controls\Primitives\ControlHelper.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutChromeSourceAuditTests.cs`

## Behavior Mapping

| Current WinUI behavior | ModernWpf WPF mapping |
| --- | --- |
| Border and ContentPresenter measure/arrange within border plus padding. BackgroundSizing chooses the outer border edge or deflated inner edge. Rounded border paint is outer geometry minus inner geometry. | The shared layout decorator deflates/inflates measure and arrange. `LayoutChromeHelper` selects outer/inner background geometry and builds the border with `CombinedGeometry(Exclude)`. Deterministic rendered edge/corner tests cover both modes. |
| Rounded corner state participates in layout clipping and hit testing while preserving a pre-existing layout clip. | Each adapter intersects its base clip with the rounded inner geometry and uses the same rounded boundary for hit testing. Tests cover nonuniform radii, child clipping, changing radii, fully rounded pixels, and base-clip preservation. |
| Grid removes combined RowSpacing/ColumnSpacing from available definition space, adds spacing to desired size, offsets each cell by index times spacing, and includes spacing within spans. Negative spacing is legal. | GridEx applies source-shaped effective definition sizes and explicit child arrangement for positive/negative gaps. Tests cover Auto/Pixel/Star tracks, row/column gaps, multi-track spans, negative desired size, and definition invalidation. |
| StackPanel measures all children, counts Spacing only between visible children, arranges collapsed children without advancing visible spacing, and exposes regular/irregular snap points with orientation/alignment rules. | StackPanelEx matches the realized-child measure/arrange and snap-point contracts. Tests cover both orientations, negative spacing, collapsed children, Near/Center/Far points, regular/irregular errors, change notifications, and chrome. |
| ContentPresenter owns BackgroundSizing, CornerRadius, border/padding chrome, CharacterSpacing, IsTextScaleFactorEnabled, TextWrapping, LineHeight, LineStackingStrategy, and MaxLines forwarding into its generated text child. | ContentPresenterEx owns the WPF-equivalent inherited metadata, pushes representable properties into TextBlock/AccessText, and uses a measured MaxHeight clip for MaxLines. It is now the preferred template text/chrome surface. |
| ContentControl itself is not the normal chrome renderer; its template generally delegates to ContentPresenter. | ContentControlEx keeps API/template compatibility and transition/alignment forwarding, while migrated templates put rendering/text properties on ContentPresenterEx. |

## WPF Substitutions

- Native WinUI layout/composition code becomes WPF Panel, Border,
  ContentPresenter, Geometry, clip, and dependency-property metadata. Visible
  measure, arrange, render, clipping, and hit-test outcomes are the parity
  boundary.
- WinUI rounded-corner composition state and rectangular framework layout clips
  have no direct WPF equivalent. The adapters intersect WPF geometry clips so
  existing clips are retained and rounded content cannot paint/hit outside.
- WPF Grid has no row/column spacing. GridEx temporarily adapts definition
  constraints and explicitly arranges realized cells; this is why its span and
  negative-spacing tests are source-critical.
- WPF TextBlock has no MaxLines property. ContentPresenterEx converts the
  effective line height and line limit to MaxHeight/clipping. CharacterSpacing
  and per-element WinUI text-scale service behavior remain metadata/API
  compatible where WPF lacks matching glyph-spacing and OS text-scale hooks.
- WPF has no WinUI IScrollSnapPointsInfo integration in its stock StackPanel.
  StackPanelEx exposes the source-compatible query/events for ModernWpf scroll
  consumers, backed by realized WPF child geometry.
- The new native OptimizeApplyStyles/resource lookup optimizations are internal
  engine work, not a WPF control API to emulate. WPF's own style/resource engine
  remains authoritative for adapter lifecycle.

## Verification

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~LayoutChrome|FullyQualifiedName~BorderEx|FullyQualifiedName~GridEx|FullyQualifiedName~StackPanelEx|FullyQualifiedName~ContentPresenterEx|FullyQualifiedName~ContentControlEx" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
git diff --check
```

There is no one installed-Gallery target that isolates these private WPF
adapters. They are instead exercised by the retained controls whose exact-size
strict comparisons already pass, including current DropDownButton Light/Dark
`artifacts/visual-checks/20260719-034918-186-31168/report.md` /
`artifacts/visual-checks/20260719-035013-820-67352/report.md`, BreadcrumbBar
`artifacts/visual-checks/20260718-205617-796-43088/report.md` /
`artifacts/visual-checks/20260718-205659-478-24012/report.md`, and InfoBar
`artifacts/visual-checks/20260718-192111-466-94688/report.md` /
`artifacts/visual-checks/20260718-192130-115-16160/report.md`. Those shared
consumer checks complement, but do not replace, the deterministic per-adapter
geometry/layout tests.

The focused current-source/layout slice passes 56/56. During this refresh, its
exact 96-DPI elevation regression was found to leave a manually rendered,
windowless visual connected to WPF's render channel; that contaminated later
live-window invalidation tests in the same process. The regression now detaches
that visual after sampling. The paired repro and complete slice pass, including
dynamic corner-radius clipping, StackPanel orientation/snap notifications, and
Grid definition invalidation. This is test isolation only, not a product-pixel
or layout change. Controls continue to build on net462/net8/net10 with zero
warnings and zero errors.
