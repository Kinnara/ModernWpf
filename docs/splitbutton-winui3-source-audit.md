# SplitButton WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\controls\dev\SplitButton\SplitButton.cpp`
- `src\controls\dev\SplitButton\SplitButton.xaml`
- `src\controls\dev\SplitButton\SplitButton_themeresources.xaml`
- `src\controls\dev\SplitButton\SplitButtonAutomationPeer.cpp`
- `src\controls\dev\SplitButton\ToggleSplitButton.cpp`
- `src\controls\dev\SplitButton\ToggleSplitButtonAutomationPeer.cpp`

ModernWpf files:

- `ModernWpf.Controls\SplitButton\SplitButton.cs`
- `ModernWpf.Controls\SplitButton\ToggleSplitButton.cs`
- `ModernWpf.Controls\SplitButton\SplitButton.xaml`
- `ModernWpf.Controls\CommandBar\CommandBar.xaml`
- `ModernWpf.Controls\SplitButton\SplitButtonAutomationPeer.cs`
- `ModernWpf.Controls\SplitButton\ToggleSplitButtonAutomationPeer.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\SplitButton\SplitButtonApiTests.cs`
- `test\ModernWpf.WinUI.Tests\SplitButton\SplitButtonInteractionTests.cs`

## Ported Source Behavior

- Removed the old WPF `InputBinding` workaround for `Alt+Down` and ported the source key-up path directly: Space/Enter run primary click plus command execution, `Alt+Down` opens the flyout, and `F4` opens the flyout.
- `OpenFlyout` now matches WinUI by showing with `FlyoutShowOptions.Placement = BottomEdgeAlignedLeft` instead of the flyout's default placement.
- Flyout event registration now follows the source revoker shape: applying a template or changing `Flyout` re-registers the current flyout, tracks open/closed state, and keeps placement-state updates bound to the active flyout.
- Pointer-type tracking now distinguishes mouse and WPF touch input so touch/key activation can route through the source `TouchPressed` and `CheckedTouchPressed` states.
- `SplitButtonAutomationPeer.Invoke` now routes through the owner `SplitButton.Invoke`, which first tries the primary button invoke provider and falls back to the source primary-click path.
- The default template now uses the WinUI split-border shape: separate `PrimaryButtonBorder` and `SecondaryButtonBorder`, `PrimaryBackgroundGrid` spanning the separator, and source state setter targets for the two borders instead of the old single full-width `Border`.
- The default template no longer keeps the stale `VisualStateGroupListener` bridge; common states are represented directly by `VisualStateEx.Setters`, matching the WinUI template's setter-owned state model.
- `SplitButtonCommandBarStyle` now carries the source command-bar variant states with `VisualStateEx.Setters` instead of the old `VisualStateGroupListener` plus WPF `ControlTemplate.Triggers` relay.
- `SplitButtonPadding` and `SplitButtonBorderBrushPressed` now match the WinUI 3 source resource values.

## WPF Substitutions

- WinUI `Grid` supports `CornerRadius`, `BorderBrush`, and `BorderThickness`; WPF `Grid` does not. ModernWpf uses `GridEx` for the root and split border layers while keeping plain WPF `Grid` for the source background layers.
- WinUI `SplitButtonCommandBarStyle` uses `Grid` corner-radius bindings for the command-bar background surfaces. ModernWpf keeps WPF `Border` elements with the same source target names so rounded corners and setter targets stay functional.
- WinUI `AnimatedIcon` uses `AnimatedChevronDownSmallVisualSource`; ModernWpf keeps the existing `FontIconFallback` path because the repo does not carry the WinUI animated visual source.
- WPF has no direct `AutomationProperties.AccessibilityView=Raw` equivalent, so the template cannot express that source metadata.
- WPF has no WinUI `VirtualKey.GamepadA` path in this control. Keyboard parity is covered for Space/Enter, `Alt+Down`, and `F4`.
- WPF `Button.CommandTarget` remains a WPF-specific binding because ModernWpf exposes `SplitButton.CommandTarget`.

## Verification

Focused tests cover default style/resource values, the source split-border template shape, source and command-bar visual-state setter targets, removal of the stale default-template listener bridge, source flyout placement, and Space/Enter command execution.
