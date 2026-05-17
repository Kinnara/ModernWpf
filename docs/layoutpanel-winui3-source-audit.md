# LayoutPanel WinUI 3 Source Audit

ModernWpf `LayoutPanel` is now tracked as a source-backed WPF port of the local WinUI 3 control rather than only a WinUI 2-era WPF-feasible layout surface.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\LayoutPanel\LayoutPanel.idl`
- `src\controls\dev\LayoutPanel\LayoutPanel.h`
- `src\controls\dev\LayoutPanel\LayoutPanel.cpp`
- `src\controls\dev\LayoutPanel\LayoutPanelLayoutContext.h`
- `src\controls\dev\LayoutPanel\LayoutPanelLayoutContext.cpp`
- `src\controls\dev\LayoutPanel\APITests\LayoutPanelTests.cs`

ModernWpf files:

- `ModernWpf.Controls\LayoutPanel\LayoutPanel.cs`
- `ModernWpf.Controls\LayoutPanel\LayoutPanelLayoutContext.cs`
- `test\ModernWpf.WinUI.Tests\LayoutPanel\LayoutPanelApiTests.cs`

## Ported Source Shape

- The public source surface is present: `Layout`, `BorderBrush`, `BorderThickness`, `Padding`, and `CornerRadius`.
- Measure and arrange follow the WinUI source algorithm: available/final size is reduced by padding and border thickness, clamped to zero, delegated to `Layout` when present, and otherwise applied to every child as a single fill rect.
- Source layout replacement semantics are preserved: the old layout is uninitialized and detached from measure/arrange invalidation, the new layout is initialized and hooked, and `LayoutPanel` invalidates measure after the change.
- `LayoutPanelLayoutContext` exposes the panel children and `LayoutState` to non-virtualizing layouts through the WPF `UIElementCollection` substitute.
- Tests cover upstream padding/border layout offsets, dynamic layout switching, custom non-virtualizing layout, source invalidation handler revocation, WPF chrome rendering, rounded clipping/hit testing, and XAML parsing of chrome properties.

## WPF Substitutions

- WinUI uses generated dependency-property metadata and `OnPropertyChanged`. ModernWpf uses WPF dependency-property registration and metadata flags to drive measure, arrange, and render invalidation.
- WinUI panel protected APIs own `BorderBrush`, `BorderThickness`, and `CornerRadius` under the internal SDK. ModernWpf renders equivalent chrome through `LayoutChromeHelper`.
- WinUI layout clips are platform layout geometry. ModernWpf represents rounded layout clipping and hit testing through WPF `Geometry` and `HitTestCore`.
- WinUI `Children().GetView()` is represented by a lightweight WPF `IReadOnlyList<UIElement>` wrapper over `UIElementCollection`.
