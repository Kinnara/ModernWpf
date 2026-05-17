# Slider WinUI 3 Source Audit

ModernWpf target: WPF platform `Slider` styled through `ModernWpf\Styles\Slider.xaml`.

WinUI 3 source snapshot: `D:\repos\microsoft-ui-xaml`.

## Source Files

- `src\controls\dev\CommonStyles\Slider_themeresources.xaml`
- `src\dxaml\xcp\dxaml\lib\Slider_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\Slider_Partial.h`
- `src\dxaml\test\native\external\controls\slider\SliderIntegrationTests.cpp`
- `src\controls\dev\CommonStyles\TestUI\SliderPage.xaml`

## Ported Shape

ModernWpf keeps the existing WPF platform `Slider` instead of adding a new control. The old trigger-driven WPF visual implementation is deleted from the horizontal and vertical control templates.

The WPF port now follows the WinUI 3 source shape for the parts that have WPF equivalents:

- `Normal`, `PointerOver`, `Pressed`, and `Disabled` are source-named `CommonStates`.
- Pointer-over, pressed, and disabled chrome changes are represented as `VisualStateEx.Setters` instead of `ControlTemplate.Triggers`.
- `SliderHelper.VisualStateSettersEnabled` drives the source state names for the WPF platform `Slider` and its current `Thumb`, including the pressed state while the thumb is dragging.
- Tick visibility moved out of template triggers into `SliderHelper`, matching WinUI's `OnTickPlacementChanged` source flow.
- Source style metrics are restored: `SliderPreContentMargin=14`, `SliderPostContentMargin=14`, 18px horizontal/vertical thumbs, `SliderThumbStyle` border margin `-2`, and source `ControlFastAnimationDuration` for normal/disabled thumb scale transitions.

## WPF Substitutions

- WinUI has one template containing both `HorizontalTemplate` and `VerticalTemplate`; WPF `Slider` requires a platform `PART_Track`, so ModernWpf keeps separate horizontal and vertical `ControlTemplate` resources selected by the existing orientation style trigger.
- WinUI lays out the track and thumb in `Slider_Partial.cpp`; WPF keeps platform `Track` layout, value coercion, keyboard handling, automation, and auto-tooltip behavior.
- WinUI has `TickPlacement.Inline` and `TickPlacement.Outside`; WPF exposes `TopLeft`, `BottomRight`, and `Both`. ModernWpf maps `Both` to the source outside-ticks behavior and does not add a new API.
- WinUI focus engagement/gamepad states and `FocusBorder` behavior remain platform-only. ModernWpf keeps its WPF focus visual helper.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\SliderVisualStateTests.cs`
  - verifies both horizontal and vertical templates have no `ControlTemplate.Triggers`;
  - verifies source metrics and source-named setter-backed common states;
  - verifies pointer-over and pressed states apply source resources;
  - verifies `SliderHelper` drives disabled state and tick placement.

Command:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter SliderVisualStateTests
```

Result: passed, 3 tests.
