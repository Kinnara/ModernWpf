# CommandBar WinUI 3 Source Audit

Date: 2026-05-17

This audit treats the local WinUI 3 checkout at `D:\repos\microsoft-ui-xaml`
as the source of truth for the ModernWpf `CommandBar` port. The old WPF
`ToolBar`-hosted implementation has been deleted for normal `CommandBar`; the
control now owns its command panels, overflow popup, template settings, and
available-command visual states directly.

## WinUI 3 Source Inputs

- `src\dxaml\xcp\dxaml\lib\CommandBar_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\CommandBar_Partial.h`
- `src\dxaml\xcp\dxaml\lib\CommandBarOverflowPresenter_Partial.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\Modules\Controls\CommandBar.cs`
- `src\controls\dev\CommonStyles\CommandBar_themeresources.xaml`
- `src\dxaml\test\native\external\controls\commandbar\CommandBarIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf.Controls\CommandBar\CommandBar.cs`
- `ModernWpf.Controls\CommandBar\CommandBar.xaml`
- `ModernWpf.Controls\CommandBar\CommandBarTemplateSettings.cs`
- `ModernWpf.Controls\CommandBar\CommandBarOverflowPanel.cs`
- `ModernWpf.Controls\CommandBar\CommandBarOverflowPresenter.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementProperties.cs`
- `test\ModernWpf.WinUI.Tests\CommandBar\CommandBarApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf status |
| --- | --- |
| `CommandBar` owns primary and secondary command collections and exposes source-shaped `CommandBarTemplateSettings`. | Matched with WPF observable collections, direct template settings, and tests for default/setter behavior. |
| The template uses `PrimaryItemsControl`, `SecondaryItemsControl`, `MoreButton`, and `OverflowPopup` directly rather than an outer platform toolbar. | Matched with WPF panels inside the `CommandBar` template; `CommandBarToolBar` and `CommandBarPanel` were deleted. |
| Primary commands render in the compact bar and secondary commands render in overflow with `UseOverflowStyle` state. | Matched without WPF `ToolBar.OverflowMode` or `ToolBar.IsOverflowItem`. |
| `ChangeVisualState` chooses `BothCommands`, `PrimaryCommandsOnly`, or `SecondaryCommandsOnly` from visible dynamic commands. | Matched by running available-command states on `CommandBar` itself. |
| `IsDynamicOverflowEnabled` switches the source dynamic-overflow visual state and moves primary commands into overflow when measured width is constrained. | Matched with a WPF measurement pass that rebuilds dynamic primary/secondary command lists from the original collections. |
| Auto overflow-button visibility accounts for secondary commands and visible bottom-label primary commands. | Matched through `CommandBarTemplateSettings.EffectiveOverflowButtonVisibility` and focused tests. |
| Overflow presenter open-up/down display states are driven from popup placement. | Matched with WPF popup geometry, reusing the existing `CommandBarOverflowPresenter` visual states. |
| Source drop-shadow mode applies elevation to `SecondaryItemsControlShadowWrapper`, and the source template keeps `OverflowContentRoot` as the measured popup root around that wrapper. | Matched with a WPF `OverflowContentRoot` grid containing `SecondaryItemsControlShadowWrapper`, a `ThemeShadowChrome` at depth `32` with `WindowedPopupInsetMode=Medium`, and focused template-shape tests. |
| Command execution closes the parent command bar and visibility/property changes refresh command-bar state. | Matched through parent ownership tracking and `AppBarElementProperties` callbacks. |
| `CommandBar::UpdateInputDeviceTypeUsedToOpen` captures the input device used to open overflow and applies that input mode to secondary AppBar commands while primary commands remain in `InputModeDefault`. | Matched for WPF touch/default input with `CommandBar` input-mode tracking, `CommandBarOverflowPanel` owner propagation, and focused tests that move commands between primary and secondary collections while overflow is open. |

## WPF Substitutions

- WinUI `CommandBar` derives from `AppBar`; ModernWpf has no separate WPF `AppBar`
  base, so app-bar closed display modes and sticky light-dismiss policy remain
  outside the current public surface.
- WinUI uses generated `CommandBarElementCollection` and `ItemsControl`
  plumbing; ModernWpf uses WPF observable collections and direct panel children
  to avoid `ToolBar` container behavior while preserving command element reuse.
- WinUI dynamic overflow supports `DynamicOverflowOrder` and the
  `DynamicOverflowItemsChanging` event; those APIs are not present in the
  existing ModernWpf surface, so this slice ports measured movement of primary
  commands but does not add new API.
- WPF popup placement does not expose WinUI `Popup.ActualPlacement`, so
  `CommandBarOverflowPresenter` derives open-up/down state from measured popup
  position.
- WinUI compositor `ThemeShadow` and popup-root shadow animation are represented
  by the shared WPF `ThemeShadowChrome` renderer on the source shadow-wrapper
  template part. Root-bounds, gamepad/remote input mode, and WinRT automation
  details remain platform substitutions shared with the CommandBarFlyout port.
  WPF touch input is mapped to WinUI's overflow `TouchInputMode`; WPF has no
  equivalent gamepad/remote input-device service in this control path.

## Current Validation

Run after CommandBar changes:

```powershell
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~CommandBarApiTests
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~CommandBarFlyoutApiTests
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~SyncMatrixTests
```

Latest verified result on 2026-05-19: the combined `CommandBarApiTests`,
`LayoutCompatibilityApiTests.ThemeShadow`, and `TemplateParityTests` filter
passed 57/57. `CommandBarApiTests.CommandBarOverflowShadowUsesSourceWrapper`
guards the source shadow-wrapper target.
