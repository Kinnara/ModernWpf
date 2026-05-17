# SwipeControl WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI 3 source inspected:

- `src\controls\dev\SwipeControl\SwipeControl.idl`
- `src\controls\dev\SwipeControl\SwipeControl.h`
- `src\controls\dev\SwipeControl\SwipeControl.cpp`
- `src\controls\dev\SwipeControl\SwipeControl.xaml`
- `src\controls\dev\SwipeControl\SwipeControl_themeresources.xaml`
- `src\controls\dev\SwipeControl\SwipeItem.cpp`
- `src\controls\dev\SwipeControl\SwipeItems.cpp`
- `src\controls\dev\SwipeControl\SwipeItemInvokedEventArgs.cpp`

## Ported Shape

The old ModernWpf guessed implementation used four permanent WPF item panels and generated plain `Button` elements. That path has been deleted from the control implementation. The WPF port now follows the WinUI 3 source structure:

- Template parts are source-shaped: `RootGrid`, `SwipeContentRoot`, `SwipeContentStackPanel`, `ContentRoot`, `ContentPresenter`, and `InputEater`.
- Swipe item content is created dynamically into the single `SwipeContentStackPanel` instead of keeping `PART_LeftItemsPanel`, `PART_RightItemsPanel`, `PART_TopItemsPanel`, and `PART_BottomItemsPanel`.
- `SwipeItem.GenerateControl` now creates/configures `AppBarButton` instances with `Style`, `Background`, `Foreground`, `Icon`, `Label`, and click invocation, matching the WinUI source path.
- `SwipeItems.Mode=Execute` keeps the source one-item validation and now notifies the owning control when mode changes.
- Drag side selection follows the source sign semantics: negative horizontal motion creates `LeftItems`, positive horizontal motion creates `RightItems`, negative vertical motion creates `TopItems`, and positive vertical motion creates `BottomItems`.
- Open threshold logic follows the source `min(effectiveStackPanelSize, 100)` shape.
- Execute swipe invocation is source-shaped: after threshold open, the first item is invoked, and `BehaviorOnInvoked=RemainOpen` keeps the execute item open.
- Reveal background selection follows source near/far rules: left/top use the last item background, right/bottom use the first item background.
- Execute threshold colors use `SwipeItemPreThresholdExecute*` and `SwipeItemPostThresholdExecute*` resources.
- `MeasureOverride` follows the source ListViewItem-fill behavior by returning finite available width/height after measuring the root.

## WPF Substitutions

WinUI's implementation is driven by `InteractionTracker`, `VisualInteractionSource`, composition expression animations, clip animations, and XamlRoot-level dismiss handlers. The WPF port substitutes:

- WPF mouse capture and direct delta handling for `InteractionTracker`.
- A `TranslateTransform` on `ContentRoot` for source tracker translation.
- WPF root/window preview mouse/key handlers for XamlRoot dismissing.
- WPF `ClipToBounds` for WinUI inset clip animation.
- Existing `ContentPresenterEx` for WinUI `ContentPresenter` chrome and `ContentTransitions` API forwarding.

The remaining platform gaps are exact touch inertia, composition animation timing, WinUI test hooks, pointer-device-specific touch suppression, XamlRoot metadata, and raw WinUI TestUI automation.

## Tests

Focused WPF coverage:

- API defaults and markup construction for `SwipeControl`, `SwipeItems`, and `SwipeItem`.
- Source template part shape and deletion of the old `PART_*ItemsPanel` model.
- Dynamic `AppBarButton` generation and source `SwipeItemStyle` visual-state setter shape.
- Source drag sign semantics, `Close()`, outside-tap dismissal, reveal-item invocation, execute threshold invocation, and execute `RemainOpen`.

Validation:

```text
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter SwipeControlApiTests
```

Result: 16 passed.
