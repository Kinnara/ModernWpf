# Repeater WinUI 3 Source Audit

Date: 2026-07-17

ModernWpf `ItemsRepeater` and Repeater layouts are tracked as a source-backed
WPF port of official `microsoft-ui-xaml` `winui3/main` commit
`3cae15f071f1ab8565f9a7592dbf27f04bafe651` (2026-07-13), rather than as a
WinUI 2-era WPF-feasible layout surface. Live comparison uses WinUI 3 Controls
Gallery `2.9.3.0` with Windows App Runtime `2.2.3.0.0`.

## Source Files

Primary WinUI 3 source references:

- `controls\dev\Repeater\ItemsRepeater.h`
- `controls\dev\Repeater\ItemsRepeater.cpp`
- `controls\dev\Repeater\ViewportManager.h`
- `controls\dev\Repeater\ViewportManager.cpp`
- `controls\dev\Repeater\StackLayout.h`
- `controls\dev\Repeater\StackLayout.cpp`
- `controls\dev\Repeater\FlowLayout.h`
- `controls\dev\Repeater\FlowLayout.cpp`
- `controls\dev\Repeater\UniformGridLayout.h`
- `controls\dev\Repeater\UniformGridLayout.cpp`
- `controls\dev\Repeater\ItemsRepeaterScrollHost.*`
- `controls\dev\Repeater\ItemsSourceView.*`
- `controls\dev\Repeater\RecyclePool.*`
- `controls\dev\Repeater\SelectionModel.*`
- `controls\dev\Repeater\APITests\*.cs`

Current Gallery sample inputs:

- `D:\repos\WinUI-Gallery\WinUIGallery\Samples\ControlPages\ItemsRepeaterPage.xaml`
- `D:\repos\WinUI-Gallery\WinUIGallery\Samples\ControlPages\ItemsRepeaterPage.xaml.cs`

ModernWpf files:

- `ModernWpf.Controls\Repeater\ItemsRepeater\ItemsRepeater.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ItemsRepeater.wpf.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ViewportManager.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ViewportManagerDownLevel.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ViewManager.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ItemsRepeaterScrollHost.cs`
- `ModernWpf.Controls\Repeater\Layouts\StackLayout\*.cs`
- `ModernWpf.Controls\Repeater\Layouts\FlowLayout\*.cs`
- `ModernWpf.Controls\Repeater\Layouts\UniformGridLayout\*.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ItemsSource\*.cs`
- `ModernWpf.Controls\Repeater\ItemsRepeater\ItemTemplate\*.cs`
- `ModernWpf.Controls\Repeater\SelectionModel\*.cs`
- `ModernWpf.Gallery\Pages\CollectionsSampleFactory.cs`
- `test\ModernWpf.WinUI.Tests\Repeater\*.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Ported Source Shape

- The existing Repeater files are already Microsoft source-derived rather than a disposable WPF wrapper. This slice does not preserve old guessed behavior; it reclassifies the family against WinUI 3 source and ports the remaining concrete source delta found during the audit.
- `ModernWpf.Controls\Repeater\GlobalSuppressions.cs` now uses explicit source-parity justifications instead of stale `<Pending>` analyzer suppressions. The suppressions preserve source-shaped field names, method signatures, animation hooks, viewport hooks, and WPF substitute signatures that would otherwise be mechanically renamed or deleted by style analyzers.
- `ItemsRepeater` owns the source-shaped `AnimationManager`, `ViewManager`, viewport manager, layout context, element mapping, recycle/pin flow, item-template wrapping, and layout replacement hooks.
- `ViewportManagerDownLevel` exposes the source `GetLayoutExtent` hook used by `ItemsRepeater` measure-cycle protection.
- `ItemsRepeater.MeasureOverride` now follows WinUI 3's `StackLayout` cycle guard: after 60 consecutive `StackLayout` measure passes without layout settling, it shortcuts to the last layout extent instead of re-entering layout indefinitely.
- `ItemsRepeater` resets that counter from source-equivalent layout-settled points: `LayoutUpdated`, `Unloaded`, and layout replacement.
- Current `FlowLayout::OnItemsChangedCore` and `UniformGridLayout::OnItemsChangedCore` guard a missing `LayoutState` before forwarding collection changes, while always invalidating layout. ModernWpf now mirrors this June 2026 source fix so a stray notification against an uninitialized or unloaded context does not dereference a missing state.
- The WPF `ScrollViewer.ChangeView` substitute now follows source return semantics for Repeater calls: it clamps requested offsets to the current WPF scrollable range, returns `true` only when an offset request is actually applied, returns `false` for already-current/no-op requests, and rejects NaN/infinite offset or zoom values like WinUI's invalid numeric path.
- `StackLayout`, `FlowLayout`, and `UniformGridLayout` carry the source layout surface and WPF-feasible layout algorithms, including virtualization toggles, item spacing, wrapping, uniform item slots, and index-based orientation.
- `IndexPath`, `ItemsSourceView`, `RecyclePool`, `ElementFactory`, `SelectionModel`, and `ItemsRepeaterScrollHost` retain the existing source-shaped API/test coverage.
- The Gallery's horizontal, vertical, and circular bar templates now map WinUI `SystemChromeLowColor` to `SystemControlPageBackgroundChromeLowBrush`. The older medium-chrome substitution rendered Light bars `#E6E6E6` instead of source `#F2F2F2`.

## WPF Substitutions

- WinUI uses platform effective viewport, `ScrollPresenter`, phasing, focus/gamepad navigation, raw TestUI automation, and WinRT data-source metadata. ModernWpf maps the feasible behavior through WPF `ScrollViewer`, `IRepeaterScrollingSurface`, WPF layout invalidation, and direct unit/integration tests.
- Current WinUI collapsed its old platform/downlevel viewport subclasses into one value-owned `ViewportManager` in June 2026. ModernWpf deliberately retains the abstract `ViewportManager` plus `ViewportManagerDownLevel` WPF substitute because WPF has neither WinUI effective-viewport nor `ScrollPresenter` services; this is a platform adaptation, not a stale ownership model.
- WinUI's invalid-rect sentinel is `{-1,-1,-1,-1}`. WPF `Rect` cannot represent a negative width/height sentinel, so ModernWpf keeps `Rect.Empty` as the WPF invalid-arrange substitute.
- WinUI resets default layout state lazily from `OnLayoutUpdated`. ModernWpf eagerly installs and initializes the default `StackLayout` in the constructor, so the WPF `LayoutUpdated` substitute only resets the source measure-cycle counter.
- WinUI automation and visual/TestUI coverage remains platform-owned. ModernWpf tests the WPF-feasible API, layout, element mapping, recycle, selection, item-template, and scroll-host behavior.
- WPF `ScrollViewer` has no WinUI `ZoomFactor` surface in this Repeater substitute. Valid zoom requests are ignored and report `false` unless an offset request was also applied; invalid zoom values still throw to preserve the source numeric guard.

## Regression Guards

- `test\ModernWpf.WinUI.Tests\Repeater\RepeaterSourceAuditTests.cs` verifies that Repeater analyzer suppressions remain explicit, carry source-audit justifications, and do not fall back to `<Pending>` wording.
- `RepeaterLayoutTests.FlowLayoutsIgnoreCollectionChangesWhenContextStateIsUnavailableLikeCurrentWinUI` covers the current null-state collection-change guard for both layouts.
- `GalleryAutomationHookTests.ItemsRepeaterSampleMatchesWinUIGalleryExamples` covers the six-example source shape, add/remove behavior, layout switching, and Low chrome resource.
- `WpfGallerySourceShapeTests.GalleryVisualChecksCropVisibleItemsRepeaterSourceBarRows` pins the live 425x88 bar-row crop, required reference source, and strict `1.0`/exact-size gates.

## Current Validation

- The complete Repeater product slice passes 70/70 on net8.
- The ItemsRepeater sample/crop slice passes 2/2 on net8 and net10.
- `ModernWpf.Gallery` builds for net462, net8, and net10.
- Final Light `artifacts/visual-checks/20260717-220356-944-25124/report.md` passes at `0.53`; final Dark `artifacts/visual-checks/20260717-220423-221-77876/report.md` passes at `0.42`. Both compare exact `425x88` source bar rows under a `1.0` mean-delta gate with exact size parity.
