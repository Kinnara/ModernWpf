# AppBarButton / AppBarToggleButton WinUI 3 Source Audit

Date: 2026-05-17

Scope: existing `AppBarButton` and `AppBarToggleButton` controls only. This
audit maps the WPF implementation to local WinUI 3 source and records the WPF
substitutions that remain because the WinUI implementation depends on XAML
platform services that do not exist in WPF.

## WinUI 3 Source Baseline

- `src\dxaml\xcp\dxaml\lib\AppBarButton_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\AppBarToggleButton_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\AppBarButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\AppBarToggleButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\AppBarButtonHelpers.h`
- `src\dxaml\xcp\dxaml\lib\CommandBar_Partial.cpp`
- `src\controls\dev\CommonStyles\AppBarButton_themeresources.xaml`
- `src\controls\dev\CommonStyles\AppBarToggleButton_themeresources.xaml`
- `src\dxaml\test\native\external\foundation\input\Focus\AllowFocusOnInteractionTests.cpp`
- `src\dxaml\test\native\external\controls\commandbar\CommandBarAutomationIntegrationTests.cpp`
- `src\dxaml\test\native\external\controls\appbarbutton\AppBarButtonAutomationIntegrationTests.cpp`
- `src\dxaml\test\native\external\controls\appbartogglebutton\AppBarToggleButtonAutomationIntegrationTests.cpp`

## ModernWpf Port Surface

- `ModernWpf.Controls\CommandBar\AppBarButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarButtonAutomationPeer.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButtonAutomationPeer.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementProperties.cs`
- `ModernWpf.Controls\CommandBar\AppBarButton.xaml`
- `ModernWpf.Controls\CommandBar\AppBarToggleButton.xaml`
- `ModernWpf.Controls\CommandBar\CommandBar.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBar.cs`
- `test\ModernWpf.WinUI.Tests\CommandBar\CommandBarApiTests.cs`

## Ported Source Behavior

| WinUI 3 behavior | ModernWpf WPF port |
| --- | --- |
| `AppBarButton::OnClick` calls `CommandBar::OnCommandExecutionStatic` only when there is no associated flyout, then invokes the base click path and opens the associated flyout. | Matched. WPF tests cover command execution close, flyout preservation, click-open, automation expand, and automation collapse. |
| `AppBarToggleButton::OnClick` always routes through `CommandBar::OnCommandExecutionStatic` before the base toggle click. | Matched. WPF tests cover close behavior, command execution, and toggle state. |
| `AppBarButtonHelpers::OnPropertyChanged` updates internal styles and visual states for `IsCompact`, `UseOverflowStyle`, and `LabelPosition`. | Matched through WPF dependency-property callbacks, `SetDefaultLabelPosition`, `UpdateApplicationViewState`, width coercion, tooltip coercion, and visual-state refresh. |
| `SetOverflowStyleParams` records peer icon/toggle/keyboard-accelerator presence and refreshes visual states. | Matched for `CommandBarOverflowPanel` and `CommandBarFlyoutOverflowPanel`, including shared keyboard-accelerator text width. |
| `GetEffectiveLabelPosition` treats `LabelPosition=Collapsed` as collapsed and otherwise uses the propagated `CommandBarDefaultLabelPosition`. | Matched. Empty labels are treated as present where WinUI checks non-null `HSTRING`. |
| `UpdateInternalStyles` applies the label-on-right auto-width adjustment only for right labels, non-overflow style, and no local `Width`. | Matched with WPF `WidthProperty` coercion so local widths remain authoritative. |
| WinUI creates explicit AppBar automation peers with source class names, localized control-type strings, label-based name fallback, trimmed keyboard accelerator text, no template children, command-bar hosted keyboard focusability, AppBarButton expand/collapse, and AppBarToggleButton toggle routing through the owner. | Matched with WPF automation peers and tests. |
| WinUI `AllowFocusOnInteraction` tests keep AppBar buttons keyboard-focusable while suppressing pointer-origin focus. | Matched with WPF mouse-origin focus cancellation and tests. |
| `AppBarButtonHelpers::CloseSubMenusOnPointerEntered` closes peer overflow submenus on pointer entry, leaving the hovered AppBarButton submenu open and closing all peers for AppBarToggleButton. | Matched with WPF `MouseEnter` routing through `CommandBar.ClosePeerSubMenusOnPointerEntered`; peer `FlyoutBase` submenus are hidden for regular `CommandBar` and `CommandBarFlyoutCommandBar`. |
| CommonStyles AppBar templates use setter-backed application-view, common, input-mode, keyboard-accelerator, chevron, and checked states. | Matched through `VisualStateEx.Setters` in WPF templates with tests for setter presence and active state effects. |

## WPF Substitutions

- WinUI `CascadingMenuHelper`, `ISubMenuOwner`, popup submenu direction, delayed
  close timers, and popup-root tracking do not exist in WPF. ModernWpf uses
  associated `FlyoutBase` instances as the submenu representation and closes
  peer flyouts on WPF `MouseEnter`.
- WinUI can query `DXamlCore::GetIsKeyboardPresent`; ModernWpf approximates
  accelerator visibility from overflow state plus source-shaped peer
  `KeyboardAcceleratorTextOverride` presence.
- WinUI `TextTrimming="Clip"` is not a WPF enum value.
- WinUI touch and gamepad input-mode state selection is not wired to a WPF
  platform input-mode service. The source visual states exist in templates and
  can be selected, but automatic runtime selection remains a platform gap.
- WinUI disabled `AllowFocusWhenDisabled` has no direct WPF equivalent here, so
  disabled AppBar controls remain non-keyboard-focusable.
- Normal `CommandBar` still uses WPF `ToolBar` hosting for overflow layout. The
  AppBar controls themselves no longer depend on WPF `ToolBar.IsOverflowItem`
  triggers; overflow state flows through the explicit `UseOverflowStyle` model.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~CommandBarApiTests.AppBarOverflowPointerEnterClosesPeerSubMenusLikeWinUISource" --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~CommandBarApiTests" --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~CommandBarFlyoutApiTests" --no-restore`
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
- `git diff --check`
