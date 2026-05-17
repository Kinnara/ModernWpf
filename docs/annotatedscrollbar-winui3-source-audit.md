# AnnotatedScrollBar WinUI 3 Source Audit

Date: 2026-05-17

ModernWpf now treats `AnnotatedScrollBar` as a source-backed WPF port of the local WinUI 3 implementation rather than a guessed subset.

## WinUI 3 Source Files

- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.idl`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.h`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.cpp`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBar.xaml`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBar_themeresources.xaml`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBarLabel.cpp`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBarScrollingEventArgs.cpp`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBarDetailLabelRequestedEventArgs.cpp`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBarPanningInfo.h`
- `src\controls\dev\AnnotatedScrollBar\AnnotatedScrollBarPanningInfo.cpp`
- `src\controls\dev\AnnotatedScrollBar\APITests\AnnotatedScrollBarTests.cs`
- `src\controls\dev\AnnotatedScrollBar\InteractionTests\AnnotatedScrollBarInteractionTests.cs`
- `src\controls\dev\ScrollPresenter\ScrollPresenterPrimitives.idl`

## ModernWpf Files

- `ModernWpf.Controls\AnnotatedScrollBar\AnnotatedScrollBar.cs`
- `ModernWpf.Controls\AnnotatedScrollBar\AnnotatedScrollBar.xaml`
- `ModernWpf.Controls\AnnotatedScrollBar\AnnotatedScrollBarLabel.cs`
- `ModernWpf.Controls\AnnotatedScrollBar\AnnotatedScrollBarScrollingEventArgs.cs`
- `ModernWpf.Controls\AnnotatedScrollBar\AnnotatedScrollBarDetailLabelRequestedEventArgs.cs`
- `ModernWpf.Controls\AnnotatedScrollBar\AnnotatedScrollBarPanningInfo.cs`
- `ModernWpf.Controls\AnnotatedScrollBar\ScrollControllerPrimitives.cs`
- `test\ModernWpf.WinUI.Tests\AnnotatedScrollBar\AnnotatedScrollBarApiTests.cs`

## Source Behavior Ported

- Deleted the old simple `PART_Rail` / `PART_LabelsHost` / `ItemsControl` shape. The default template now uses source part names: `PART_VerticalThumb`, `PART_VerticalThumbGhost`, increment/decrement repeat buttons, `PART_VerticalGrid`, `PART_LabelsGrid`, `PART_TooltipContentPresenter`, and `PART_DetailLabelToolTip`.
- `ScrollController` now exposes a source-shaped `IScrollController` instead of an opaque object. The WPF port includes `SetValues`, `SetIsScrollable`, `CanScroll`, `IsScrollingWithMouse`, `PanningInfo`, `ScrollToRequested`, `ScrollByRequested`, and `AddScrollVelocityRequested`.
- Added WPF substitutes for WinUI scroll-controller primitive types: `ScrollingScrollOptions`, animation/snap-point mode enums, scroll request event args, pan request event args, `IScrollController`, and `IScrollControllerPanningInfo`.
- Ported source `SetValues` validation, offset clamping, viewport-to-small-change default, requested-scroll operation tracking, and source request options using disabled animation with ignored snap points for scroll-to/by requests.
- Ported source increment/decrement semantics: the increment repeat button requests a negative small-change delta and the decrement repeat button requests a positive small-change delta.
- Ported cancelable `Scrolling`: a canceled event suppresses the scroll-controller request.
- Removed the guessed nearest-label detail fallback. The detail tooltip content now comes from `DetailLabelRequested`, matching the source event-driven model.
- Ported source label container generation into `PART_LabelsGrid`, source label offset scaling, collision/out-of-bounds collapse, hover tooltip positioning, and thumb ghost positioning.
- Removed `AnnotatedScrollBarLabel.ToString()` content fallback. WinUI source only exposes `Content` and `ScrollOffset`.

## WPF Substitutions

- WinUI binds `AnnotatedScrollBar` to `ScrollPresenter` through `IScrollController`. WPF has no platform `ScrollPresenter` or scroll-controller primitive, so ModernWpf adds source-shaped WPF primitives under `ModernWpf.Controls.Primitives`; a WPF scroll host can consume the request events and call `SetValues`.
- WinUI composition expression animation sources in `AnnotatedScrollBarPanningInfo` are represented by no-op WPF methods. Thumb and label positions are updated by layout and `SetValues`.
- WinUI independent touch pan and pointer capture are represented by WPF mouse capture and input events. The WPF test coverage verifies the source request semantics rather than raw pointer-device automation.
- WinUI `ScrollView` / `ScrollPresenter` TestUI coverage is represented by focused WPF API/template/request tests. Full TestUI process input, touch inertia, and compositor timing remain platform gaps.
- Theme resources are mapped through existing ModernWpf brush aliases where WinUI uses newer resource keys that do not exist in this WPF resource stack.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter FullyQualifiedName~AnnotatedScrollBar --no-restore`
  - Passed: 12 AnnotatedScrollBar tests.
- Focused tests cover source-shaped defaults, template parts, label containers, `SetValues` validation, `CanScroll`, scroll-to request options, cancel suppression, small-change direction, panning info, and removal of the guessed detail-label fallback.
- Stale implementation search:
  - `PART_Rail`, `PART_LabelsHost`, and the old label-host `ItemsControl` expectations are removed from AnnotatedScrollBar implementation and focused tests.
