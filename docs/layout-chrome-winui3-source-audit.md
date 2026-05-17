# Layout Chrome WinUI 3 Source Audit

Date: 2026-05-17

This audit treats the local WinUI 3 checkout at `D:\repos\microsoft-ui-xaml`
as the behavioral source of truth for ModernWpf layout chrome helpers. These
files are not deleteable guessed control implementations in the same way as
old `CommandBarFlyoutToolBar` or earlier control-specific guesses. WPF `Border`,
`Grid`, `StackPanel`, and `ContentPresenter` do not expose the WinUI 3 chrome,
spacing, snap-point, or text-formatting surface directly, so ModernWpf keeps
small WPF substitute controls that map the WinUI behavior onto WPF layout and
rendering primitives.

## WinUI 3 Source Inputs

- `src\dxaml\xcp\core\core\elements\Border.cpp`
- `src\dxaml\xcp\core\core\elements\Grid.cpp`
- `src\dxaml\xcp\core\core\elements\StackPanel.cpp`
- `src\dxaml\xcp\core\core\elements\ContentPresenter.cpp`
- `src\dxaml\xcp\core\core\elements\framework.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs`
- `src\dxaml\test\native\external\controls\grid\GridIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf\Controls\BorderEx.cs`
- `ModernWpf\Controls\GridEx.cs`
- `ModernWpf\Controls\StackPanelEx.cs`
- `ModernWpf\Controls\ContentPresenterEx.cs`
- `ModernWpf\Controls\ContentControlEx.cs`
- `ModernWpf\Controls\LayoutChromeHelper.cs`
- `ModernWpf\Controls\Primitives\ControlHelper.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf status |
| --- | --- |
| `CBorder::MeasureOverride` and `ArrangeOverride` use the combined border thickness and padding, then arrange content inside `HelperGetInnerRect`. | `LayoutChromeDecorator`, `ContentPresenterEx`, `GridEx`, and `StackPanelEx` measure through the combined chrome thickness and arrange content inside the deflated chrome rect. |
| `BackgroundSizing` chooses whether background paint extends to the outer border edge or starts at the inner border edge. | `LayoutChromeHelper.DrawChrome` builds outer and inner rounded geometries and selects the background geometry from `BackgroundSizing`; tests render edge pixels and outer-corner geometry. |
| WinUI rounded border geometry draws border as the difference between outer and inner rounded rectangles. | `LayoutChromeHelper.DrawChrome` draws the WPF border with `CombinedGeometry(Exclude, outer, inner)`, matching the source geometry model with WPF primitives. |
| WinUI rounded corners affect compositor clipping and hit testing through framework/layout infrastructure. | ModernWpf represents the app-visible behavior with WPF layout clips and rounded hit-test geometry on `BorderEx`, `GridEx`, `StackPanelEx`, `ContentPresenterEx`, and `LayoutChromeDecorator`. |
| `CGrid::MeasureOverride` subtracts combined row/column spacing from the inner available size and adds it back to desired size. | `GridEx` subtracts spacing from effective definition size and adds negative spacing back to desired size when WPF's stock grid cannot represent the source math directly. |
| `CGrid::ArrangeOverride` offsets child cells by `columnSpacing * columnIndex` / `rowSpacing * rowIndex`, and spans include spacing through `GetFinalSizeForRange`. | `GridEx.ItemsHost.ArrangeSpacingChildren` computes source-shaped offsets and range sizes, including positive and negative spacing and multi-column/multi-row spans. |
| WinUI native grid tests cover negative `RowSpacing` and `ColumnSpacing`. | ModernWpf allows negative spacing, rejects `NaN`, and has focused WPF tests for negative spacing, auto/star tracks, and span distribution. |
| `CStackPanel::MeasureOverride` counts spacing only between visible children while still measuring all children. | `StackPanelEx.ItemsHost` measures every child, counts spacing only for visible children, and subtracts the trailing spacing once, matching the source shape. |
| `CStackPanel::ArrangeOverride` arranges every child and advances spacing only after visible children. | `StackPanelEx.ItemsHost` arranges every child and only adds spacing for non-collapsed children; tests cover collapsed-child spacing. |
| WinUI `StackPanel` exposes regular and irregular snap-point APIs with errors when callers request the wrong mode. | `StackPanelEx` implements `IScrollSnapPointsInfo` with matching mode checks, regular/irregular APIs, orientation routing, and change notifications. |
| `CContentPresenter::MeasureOverride` / `ArrangeOverride` share the same border chrome model and align content within the inner rect. | `ContentPresenterEx` deflates by border plus padding, arranges child visuals according to `HorizontalContentAlignment` / `VerticalContentAlignment`, and tests chrome offsets and alignment. |
| `CContentPresenter::OnPropertyChanged` and `ApplyTemplate` push `TextWrapping`, `LineStackingStrategy`, `LineHeight`, and `MaxLines` into the default `TextBlock`. | `ContentPresenterEx` finds WPF default `TextBlock` / `AccessText` children and applies the representable text properties. `MaxLines` uses a WPF `MaxHeight` / clipping substitute because WPF `TextBlock` has no `MaxLines` dependency property. |
| WinUI XamlOM marks `Control.CharacterSpacing`, `Control.IsTextScaleFactorEnabled`, `ContentPresenter.CharacterSpacing`, and `ContentPresenter.IsTextScaleFactorEnabled` as inherited text-formatting properties that affect measure. | `ControlHelper`, `ContentControlEx`, and `ContentPresenterEx` share the same WPF dependency-property identities with `Inherits`, `AffectsMeasure`, and `AffectsRender` metadata; tests cover inheritance and local precedence. |
| WinUI `ContentControl` is not itself the chrome renderer; templates normally put the chrome and text surface on `ContentPresenter`. | ModernWpf keeps `ContentControlEx` as the WPF helper surface for template compatibility, but templates should prefer `ContentPresenterEx` where WinUI uses a `ContentPresenter`. Tests assert the direct presenter usage in migrated templates. |

## Why These Files Stay

`BorderEx`, `GridEx`, `StackPanelEx`, `ContentPresenterEx`, and
`LayoutChromeHelper` are the WPF adaptation layer for native WinUI behavior,
not independent replacement controls with a directly reusable C# WinUI
implementation. Deleting them would drop template features that WinUI 3 uses
heavily: `BackgroundSizing`, `CornerRadius` on panel/content surfaces, border
and padding chrome on layout panels, grid/stack spacing, snap-point APIs, and
ContentPresenter text-property forwarding.

The clean-port rule still applies to these files: future changes should come
from a concrete WinUI source behavior and should be tested as a layout-control
slice, not added as guessed compatibility behavior.

## WPF Substitutions

- WinUI layout and render code is native C++ (`CBorder`, `CGrid`,
  `CStackPanel`, and `CContentPresenter`), while ModernWpf must express the
  behavior through WPF `Panel`, `Border`, `ContentPresenter`, `Geometry`, and
  dependency-property metadata.
- WinUI rounded-corner layout clipping uses framework/composition state such
  as `RequiresCompNodeForRoundedCorners`; ModernWpf uses WPF `Geometry` clips
  and hit-test geometry so the app-visible template behavior is preserved.
- WinUI `framework.cpp` layout clips are rectangular `LayoutClipGeometry`
  values; ModernWpf intersects WPF's base layout clip with a rounded WPF
  geometry because that is the available WPF clipping model.
- WPF `TextBlock` has no `MaxLines` dependency property in the target
  frameworks, so `ContentPresenterEx.MaxLines` is represented by calculated
  `MaxHeight` plus `ClipToBounds` for the generated default `TextBlock`.
- Native WinUI text scaling is only surfaced by `IsTextScaleFactorEnabled`;
  WPF has no equivalent text scale-factor pipeline in these controls.

## Current Validation

Run after layout chrome changes:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~LayoutCompatibilityApiTests" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
```

Latest verified result on 2026-05-17: layout compatibility tests passed 89/89,
and `ModernWpf.Controls` built successfully with existing warnings.
