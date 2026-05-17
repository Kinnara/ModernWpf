# Repeater WinUI 3 Source Audit

ModernWpf `ItemsRepeater` and Repeater layouts are now tracked as a source-backed WPF port of the local WinUI 3 Repeater family instead of a WinUI 2-era WPF-feasible layout surface.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\Repeater\ItemsRepeater.h`
- `src\controls\dev\Repeater\ItemsRepeater.cpp`
- `src\controls\dev\Repeater\ViewportManager.h`
- `src\controls\dev\Repeater\ViewportManagerDownlevel.h`
- `src\controls\dev\Repeater\ViewportManagerDownlevel.cpp`
- `src\controls\dev\Repeater\StackLayout.h`
- `src\controls\dev\Repeater\StackLayout.cpp`
- `src\controls\dev\Repeater\FlowLayout.h`
- `src\controls\dev\Repeater\FlowLayout.cpp`
- `src\controls\dev\Repeater\UniformGridLayout.h`
- `src\controls\dev\Repeater\UniformGridLayout.cpp`
- `src\controls\dev\Repeater\ItemsRepeaterScrollHost.*`
- `src\controls\dev\Repeater\ItemsSourceView.*`
- `src\controls\dev\Repeater\RecyclePool.*`
- `src\controls\dev\Repeater\SelectionModel.*`
- `src\controls\dev\Repeater\APITests\*.cs`

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
- `test\ModernWpf.WinUI.Tests\Repeater\*.cs`

## Ported Source Shape

- The existing Repeater files are already Microsoft source-derived rather than a disposable WPF wrapper. This slice does not preserve old guessed behavior; it reclassifies the family against WinUI 3 source and ports the remaining concrete source delta found during the audit.
- `ItemsRepeater` owns the source-shaped `AnimationManager`, `ViewManager`, viewport manager, layout context, element mapping, recycle/pin flow, item-template wrapping, and layout replacement hooks.
- `ViewportManagerDownLevel` exposes the source `GetLayoutExtent` hook used by `ItemsRepeater` measure-cycle protection.
- `ItemsRepeater.MeasureOverride` now follows WinUI 3's `StackLayout` cycle guard: after 60 consecutive `StackLayout` measure passes without layout settling, it shortcuts to the last layout extent instead of re-entering layout indefinitely.
- `ItemsRepeater` resets that counter from source-equivalent layout-settled points: `LayoutUpdated`, `Unloaded`, and layout replacement.
- `StackLayout`, `FlowLayout`, and `UniformGridLayout` carry the source layout surface and WPF-feasible layout algorithms, including virtualization toggles, item spacing, wrapping, uniform item slots, and index-based orientation.
- `IndexPath`, `ItemsSourceView`, `RecyclePool`, `ElementFactory`, `SelectionModel`, and `ItemsRepeaterScrollHost` retain the existing source-shaped API/test coverage.

## WPF Substitutions

- WinUI uses platform effective viewport, `ScrollPresenter`, phasing, focus/gamepad navigation, raw TestUI automation, and WinRT data-source metadata. ModernWpf maps the feasible behavior through WPF `ScrollViewer`, `IRepeaterScrollingSurface`, WPF layout invalidation, and direct unit/integration tests.
- WinUI's invalid-rect sentinel is `{-1,-1,-1,-1}`. WPF `Rect` cannot represent a negative width/height sentinel, so ModernWpf keeps `Rect.Empty` as the WPF invalid-arrange substitute.
- WinUI resets default layout state lazily from `OnLayoutUpdated`. ModernWpf eagerly installs and initializes the default `StackLayout` in the constructor, so the WPF `LayoutUpdated` substitute only resets the source measure-cycle counter.
- WinUI automation and visual/TestUI coverage remains platform-owned. ModernWpf tests the WPF-feasible API, layout, element mapping, recycle, selection, item-template, and scroll-host behavior.
