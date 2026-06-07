# CommandBarFlyout WinUI 3 Source Audit

Date: 2026-06-07

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
| Current WinUI source prefers `OpeningOpacityStoryboard` / `ClosingOpacityStoryboard` for outer flyout open/close and only falls back to the older clip `OpeningStoryboard` / `ClosingStoryboard` resources when opacity resources are absent. | Matched. ModernWpf uses opacity-only outer open/close storyboards and keeps a fallback lookup for custom templates still using the legacy resource names. Tag checks show this source behavior is present in the WinUI 1.6 line and later, absent from WinUI 1.5. |
| `CommandBarFlyoutCommandBar` owns template settings, open/close animation state, overflow placement visual states, command focus routing, and tab-stop uniqueness. | Matched with WPF template settings, visual states, and focused API coverage. |
| The flyout command bar avoids the WPF `ToolBar` secondary panel path. | Matched by deleting `CommandBarFlyoutToolBar` and using `CommandBarFlyoutOverflowPanel`. |
| `CommandBar::UpdateInputDeviceTypeUsedToOpen` snapshots the input device used to open secondary commands and applies source input-mode visual states to secondary AppBar entries. | Matched for WPF touch/default input. `CommandBarFlyoutCommandBar` snapshots the last WPF key/mouse/touch input before opening, `CommandBarFlyoutOverflowPanel` preserves the owner back-reference during layout updates, and secondary `AppBarButton` / `AppBarToggleButton` entries enter `TouchInputMode` when opened by touch. |
| Presenter shadow is disabled by default, added on flyout open when primary commands exist, removed during close, removed while secondary open/close animations run, then restored after those secondary storyboards complete. | Matched through `FlyoutPresenter.IsDefaultShadowEnabled` toggling. The presenter template renders the WPF `ThemeShadowChrome` depth-32 shadow with WinUI non-tooltip popup insets. |
| `OuterOverflowContentRootShadow` / `NoOuterOverflowContentRootShadow` visual states set or clear the overflow root `ThemeShadow` at `Translation.Z=32`, with no-primary-command flyouts always using the overflow shadow and primary-command flyouts using it when the overflow opens downward. | Matched with an `OuterOverflowContentRootShadowChrome` wrapper around the WPF overflow root, depth `32`, WinUI non-tooltip popup insets, source-shaped `ClearShadow`, and source-shaped `UpdateShadow` state selection. |

## WPF Substitutions

- WPF has no WinUI compositor `ThemeShadow` or system backdrop equivalent. Presenter and overflow-root shadows are represented by `ThemeShadowChrome`; exact compositor rasterization and backdrop material remain platform gaps.
- WPF's built-in `Popup` does not expose WinUI `Popup.ActualPlacement`; ModernWpf uses `WindowedPopup` for the CommandBarFlyout overflow surface so placement is reported and the visible popup can align to the primary command strip. Because `WindowedPopup` rehosts its child in a separate `HwndSource`, opacity storyboards target the hosted `OuterOverflowContentRootShadowChrome` instead of the placeholder popup element.
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

Latest verified result on 2026-06-07: targeted CommandBarFlyout API tests passed 26/26; ModernWpf Gallery build passed; cached WinUI static parity passed at `artifacts/visual-checks/20260607-201010-303-80700/report.md`; ModernWpf interaction visual check passed at `artifacts/visual-checks/20260607-201033-613-84848/report.md`; rendered MP4 recording passed at `artifacts/gallery-recordings/20260607-201107-062/report.md`.
