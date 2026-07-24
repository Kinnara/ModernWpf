# VariableSizedWrapGrid / WrapGrid / ItemsWrapGrid WinUI 3 Source Audit

Date: 2026-07-19

## Current Source Baseline

The product source of truth is official `microsoft-ui-xaml` `winui3/main`
commit `de3e767333c2f0717a6a70cb22bd192ced5ad885`. Relative to the previous
product baseline `c70471c511a0168b61dcca13af9556465f26b673`, every audited
family blob is byte-identical; commit
`8463f45162149de0ec3ad7df752596893fe3e13e` only moves the repository source
root. There is no current layout, API, virtualization, input, accessibility,
or test change to port.

Current product inputs and blob pins:

| Official source | Current blob |
| --- | --- |
| `dxaml\xcp\dxaml\lib\VariableSizedWrapGrid_Partial.cpp` | `ef4a8c6e956c6b94e77ba7fa8dc86295a8031d21` |
| `dxaml\xcp\dxaml\lib\VariableSizedWrapGrid_Partial.h` | `915109c529888f4c18ddf869adc3563e119b4e7b` |
| `dxaml\xcp\dxaml\lib\VariableSizedWrapGrid_OccupancyMap.cpp` | `420130ae5500d902656caac9c09801f4570e1e84` |
| `dxaml\xcp\dxaml\lib\WrapGrid_Partial.cpp` | `15ccea7e686bcd117851e5b4fb32cef7f5ecd87d` |
| `dxaml\xcp\dxaml\lib\WrapGrid_Partial.h` | `e3c255eab6e9dbdd06e9bb41afc654d94a035a86` |
| `dxaml\xcp\dxaml\lib\ItemsWrapGrid_Partial.cpp` | `6e3be4f2d11754b7f8b05c427109c4b4a2530a09` |
| `dxaml\xcp\dxaml\lib\ItemsWrapGrid_Partial.h` | `1b563ed4d4fd6f51de4a0881f989fc8ae319c52e` |
| generated VariableSizedWrapGrid / WrapGrid / ItemsWrapGrid WinRT implementations | `7c8e7064fe16b6dc3d9250d2811e5143f55b8f9b` / `a258ee6a9ba09a8b26467bda08d0ae400fc70adf` / `07f15d044ca187ba7c667fe8f9ad5f75ab632f0d` |
| generated core headers | `6704c1ae4c7a519be15bb7e14e8a6d2332c63a4e` / `317359469a3e049f38f2ab2cd11328495263d2fc` / `d8a722805f7914c4f9a1aa35983d8b308d6eafcc` |
| `dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs` | `ad1199a7ff9c253e38c4fb922accbe0afffbf432` |
| native VariableSizedWrapGrid / WrapGrid / ItemsWrapGrid integration tests | `fad977b91352ef07c78365436b04c71bd9559fbf` / `ed736a383be9cb9eb7cf5e1be1450f6e93e527fd` / `45c7df191c4863b31d92b43b2b3ae4db11f98d25` |

The current official WinUI Gallery authority is commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`. It has no standalone VariableSizedWrapGrid, WrapGrid, or ItemsWrapGrid page. Its current GridView
layout-customization example does consume ItemsWrapGrid through
`MaxItemsWrapGrid`; the page blob is
`248a2341c9c876a398715723ac6b7924d1271e3d` and the definition blob is
`6114d9dfab83f1359254083b9dd277ae55707eea`.

## ModernWpf Port Surface

- `ModernWpf\Controls\VariableSizedWrapGrid.cs`
- `ModernWpf\Controls\WrapGrid.cs`
- `ModernWpf\Controls\ItemsWrapGrid.cs`
- `ModernWpf.Gallery\Pages\CollectionsSampleFactory.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\WrapGridSourceAuditTests.cs`
- `test\ModernWpf.Gallery.Tests\ItemsWrapGridSourceAuditTests.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`

## Behavior Mapping

| Current WinUI behavior | ModernWpf WPF mapping |
| --- | --- |
| VariableSizedWrapGrid exposes ItemHeight, ItemWidth, Orientation, horizontal/vertical child alignment, MaximumRowsOrColumns, and attached RowSpan/ColumnSpan. | Matched through WPF dependency properties with measure/arrange invalidation, XAML parsing coverage, and span-change parent invalidation. |
| Unspecified item dimensions come from the first child; explicit dimensions and first-child spans shape the measure constraint. | Matched by `ComputeItemSize`; tests cover fixed/default sizes and horizontal/vertical layout. |
| The occupancy map derives direct-axis capacity from available size and MaximumRowsOrColumns, normalizes nonpositive spans to one, scans for the first available cell, and stops when a finite indirect map is full. Oversized spans can start at the first cell and retain their requested arranged size. | Matched by the managed occupancy set, positive-span normalization, direct/indirect limits, first-fit scan, finite-map stop regression, and span-aware arranged slots. |
| HorizontalChildrenAlignment and VerticalChildrenAlignment distribute remaining row/column space according to Start/Center/End/Stretch semantics. | Matched for realized WPF children, including span-sized slots and inter-cell stretch gaps. |
| WrapGrid exposes fixed/default item sizing, Orientation, child alignment, and MaximumRowsOrColumns on an oriented virtualizing panel. | The visible realized-child layout contract is matched by the shared WPF wrap implementation; focused tests cover both orientations, constraints, XAML, and alignment. Virtualization is a documented platform boundary. |
| ItemsWrapGrid exposes GroupPadding, Orientation, MaximumRowsOrColumns, item sizing, GroupHeaderPlacement, CacheLength, AreStickyGroupHeadersEnabled, realized/cache index properties, and ScrollingDirection. Property changes feed the native wrapping/virtualization strategy. | The public WPF-feasible surface is present. Layout and live MaximumRowsOrColumns updates are real; realized index properties describe the currently materialized WPF child set. Native cache realization, grouped sticky headers, and scrolling-direction telemetry are not claimed. |
| The current Gallery GridView layout-customization sample initializes horizontal MaxItemsWrapGrid to three and changes it live from the NumberBox option. | Matched in CollectionsSampleFactory and guarded by a rendered page test that changes three to four and reads the live ItemsWrapGrid instance. |

## WPF Substitutions And Explicit Boundaries

- WinUI VariableSizedWrapGrid uses native OccupancyBlock storage. ModernWpf
  uses managed cells while preserving visible first-fit placement, finite-map
  exhaustion, span normalization, alignment, and arranged geometry.
- WinUI WrapGrid and ItemsWrapGrid integrate with native item-generation,
  recycling, effective viewport, cache buffers, insertion indexes, group
  headers, keyboard-navigation-panel services, and oriented-panel contracts.
  WPF supplies ItemsControl/ItemContainerGenerator/VirtualizingPanel services
  through a different protocol. ModernWpf lays out the children WPF actually
  realizes; it does not fabricate native recycling, insertion-index,
  effective-viewport, sticky-header, or gamepad-navigation services.
- For compatibility and implementation reuse, WPF WrapGrid derives from the
  VariableSizedWrapGrid panel and ItemsWrapGrid derives from WrapGrid. This
  exposes harmless inherited compatibility members beyond WinUI's nominal
  type declarations; source-owned members retain their current names/defaults.
- FirstVisibleIndex/FirstCacheIndex and their last-index counterparts describe
  the full materialized WPF child range, and ScrollingDirection remains None.
  They must not be interpreted as native WinUI viewport/cache telemetry.
- BackgroundTransition and ChildrenTransitions are inherited Panel surface in
  WinUI. The WPF controls expose equivalent transition properties through the
  repository's managed transition adapter.

## Validation

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~VariableSizedWrapGrid|FullyQualifiedName~WrapGrid" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~ItemsWrapGrid" --no-restore
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net10.0-windows7.0 --filter "FullyQualifiedName~ItemsWrapGrid" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
git diff --check
```

There is no standalone current Gallery page or installed-Gallery primary
target for this family, so no substituted direct pixel comparison is claimed.
The shared current GridView consumer remains covered by Light
`artifacts/visual-checks/20260718-200820-485-79776/report.md` and Dark
`artifacts/visual-checks/20260718-200857-428-3920/report.md`, but those strict
primary crops isolate the basic GridView rather than MaxItemsWrapGrid. Direct
layout fidelity is therefore proven by source-derived deterministic geometry,
finite-occupancy, span, alignment, XAML, live option, and realized-range tests.
