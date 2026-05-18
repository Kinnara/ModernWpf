# CommandBarFlyout WinUI 3 Source Audit

Date: 2026-05-18

This audit treats the local WinUI 3 checkout at `D:\repos\microsoft-ui-xaml`
as the behavioral source of truth for the ModernWpf `CommandBarFlyout` port.
The old WPF `ToolBar`-hosted guess has been deleted from this control path and
the remaining implementation is tracked as a source-backed WPF adaptation.

## WinUI 3 Source Inputs

- `src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp`
- `src\controls\dev\CommandBarFlyout\CommandBarFlyout.h`
- `src\controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.cpp`
- `src\controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.h`
- `src\controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml`
- `src\controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.cpp`
- `src\controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.h`
- `src\dxaml\test\native\external\controls\commanding\CommandingIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBar.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutOverflowPanel.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml`
- `ModernWpf.Controls\CommandBar\AppBarButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementProperties.cs`
- `test\ModernWpf.WinUI.Tests\CommandBarFlyout\CommandBarFlyoutApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf status |
| --- | --- |
| Constructor sets `ShouldConstrainToRootBounds=false`, disables default open/close animations, and owns primary/secondary command vectors. | Matched with WPF observable collections and FlyoutBase properties. |
| Primary and secondary command collection changes are mirrored into the internal command bar. | Matched with collection-forwarding tests. |
| Secondary command execution closes the flyout, except `AppBarButton` entries with an associated flyout. | Matched with WPF routed-event revokers. |
| Secondary `AppBarButton` / `AppBarToggleButton` changes to `Icon`, `Label`, and `KeyboardAcceleratorTextOverride` refresh open flyout sizing. | Matched; all three dependency properties are now tracked, including keyboard accelerator text. |
| `AlwaysExpanded` forces `ShowMode=Standard`, opens the command bar, and hides the overflow button. | Matched with WPF command-bar state tests. |
| Close animation cancels the first `FlyoutBase.Closing`, plays the command-bar close storyboard, and calls `Hide` again for the real close. | Matched through `FlyoutBaseClosingEventArgs.Cancel` and a WPF storyboard completion callback. |
| `CommandBarFlyoutCommandBar` owns template settings, open/close animation state, overflow placement visual states, command focus routing, and tab-stop uniqueness. | Matched with WPF template settings, visual states, and focused API coverage. |
| The flyout command bar avoids the WPF `ToolBar` secondary panel path. | Matched by deleting `CommandBarFlyoutToolBar` and using `CommandBarFlyoutOverflowPanel`. |
| `CommandBar::UpdateInputDeviceTypeUsedToOpen` snapshots the input device used to open secondary commands and applies source input-mode visual states to secondary AppBar entries. | Matched for WPF touch/default input. `CommandBarFlyoutCommandBar` snapshots the last WPF key/mouse/touch input before opening, `CommandBarFlyoutOverflowPanel` preserves the owner back-reference during layout updates, and secondary `AppBarButton` / `AppBarToggleButton` entries enter `TouchInputMode` when opened by touch. |
| Presenter shadow is disabled by default, added on flyout open when primary commands exist, removed during close, removed while secondary open/close animations run, then restored after those secondary storyboards complete. | Matched through `FlyoutPresenter.IsDefaultShadowEnabled` toggling. The presenter template renders the WPF `ThemeShadowChrome` depth-32 shadow with WinUI non-tooltip popup insets. |

## WPF Substitutions

- WPF has no WinUI compositor `ThemeShadow` or system backdrop equivalent. Presenter shadow is represented by `ThemeShadowChrome`; exact compositor rasterization and backdrop material remain platform gaps.
- WPF popups do not expose WinUI `Popup.ActualPlacement`; ModernWpf uses measured overflow location and combined placement visual states instead.
- WPF automation does not expose WinRT `AutomationEvents.MenuOpened` / `MenuClosed`; app-visible command-bar behavior and focus routing are covered by WPF tests.
- WPF template binding can lag dependency-property callbacks during measurement, so ModernWpf keeps a narrow deferred size refresh after source-tracked secondary command property changes.
- WPF has no WinUI gamepad/remote input-mode service in this control path. ModernWpf wires the source-shaped touch/default subset through WPF key/mouse/touch events and leaves gamepad/remote selection as a platform gap.

## Current Validation

Run after CommandBarFlyout changes:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~CommandBarFlyoutApiTests" --no-restore
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter "FullyQualifiedName~CommandBarFlyoutApiTests|FullyQualifiedName~CommandBarApiTests|FullyQualifiedName~TemplateParityTests|FullyQualifiedName~SyncMatrixTests"
dotnet build .\ModernWpf.sln --no-restore
git diff --check
```

Latest verified result on 2026-05-18: CommandBarFlyout API tests passed 23/23.
