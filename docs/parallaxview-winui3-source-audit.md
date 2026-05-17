# ParallaxView WinUI 3 Source Audit

ModernWpf `ParallaxView` is now treated as a source-backed WPF port of the local WinUI 3 implementation rather than a legacy WPF-only ScrollViewer mapping.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\ParallaxView\ParallaxView.idl`
- `src\controls\dev\ParallaxView\ParallaxView.h`
- `src\controls\dev\ParallaxView\ParallaxView.cpp`
- `src\controls\dev\ParallaxView\ScrollInputHelper.h`
- `src\controls\dev\ParallaxView\ScrollInputHelper.cpp`
- `src\controls\dev\Generated\ParallaxView.properties.cpp`
- `src\controls\dev\Generated\ParallaxView.properties.h`
- `src\controls\dev\ParallaxView\APITests\ParallaxViewTests.cs`
- `src\controls\dev\ParallaxView\InteractionTests\ParallaxViewTests.cs`

ModernWpf files:

- `ModernWpf.Controls\ParallaxView\ParallaxView.cs`
- `ModernWpf.Controls\ParallaxView\ParallaxSourceOffsetKind.cs`
- `test\ModernWpf.WinUI.Tests\ParallaxView\ParallaxViewApiTests.cs`

## Ported Source Shape

- The public IDL surface is present: `Child`, `Source`, horizontal and vertical shifts, source start/end offsets, relative/absolute offset kinds, clamping flags, max shift ratios, `RefreshAutomaticHorizontalOffsets`, and `RefreshAutomaticVerticalOffsets`.
- Measure and arrange follow the source layout contract: the child is measured with the absolute shift added to the available size, the control reports the available viewport size when finite, the child arrange rect expands to cover the viewport plus shift, stretched cross-axis dimensions scale with the same ratio, and child alignment follows source Border-like alignment behavior.
- The rectangular viewport clip now follows the source object-lifetime shape: a `RectangleGeometry` clip is created once when needed, reused across arrange passes, and only its `Rect` is updated when the arranged viewport changes.
- `RefreshAutomaticHorizontalOffsets` and `RefreshAutomaticVerticalOffsets` now follow source guards: they are no-ops unless the corresponding source offset kind is `Relative` and the corresponding shift is nonzero.
- WPF `ScrollViewer` source tracking keeps the source offset calculation local to layout and uses the same clamped/unclamped parallax expression math as WinUI's compositor expression path. Tests cover default values, setters, content property parsing, child expansion and clipping, clip reuse, refresh guards, relative ScrollViewer offsets, and absolute unclamped offset behavior.

## WPF Substitutions

- WinUI derives from `FrameworkElement` with a private panel child collection. WPF uses `Decorator` and a single `Child` property because WPF already has a single-child layout primitive and no WinRT generated panel helper.
- WinUI applies parallax with compositor translation expression animations, `ElementCompositionPreview`, and `ScrollInputHelper` support for `ScrollViewer`, `ScrollPresenter`, zoom factor, overpan, and target-in-source offset calculation. ModernWpf represents the feasible WPF subset by arranging the child with a layout offset derived from WPF `ScrollViewer` offsets.
- WinUI refreshes animated source start/end offset expressions. ModernWpf has no composition property set, so refresh methods invalidate arrange under the same source guard conditions.
- WinUI can observe scroll content, viewport, zoom, overpan, and source property-set changes independently. ModernWpf tracks WPF `ScrollViewer.ScrollChanged`, source load, and child alignment changes.
- WinUI registers child alignment property callbacks and invalidates the child arrange path. ModernWpf uses WPF `DependencyPropertyDescriptor` hooks and invalidates arrange through the WPF layout system.
