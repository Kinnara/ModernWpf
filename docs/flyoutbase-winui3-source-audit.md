# FlyoutBase WinUI 3 Source Audit

Date: 2026-05-17

This audit treats the local WinUI 3 checkout at `D:\repos\microsoft-ui-xaml`
as the behavioral source of truth for the ModernWpf `FlyoutBase`, `Flyout`,
and `MenuFlyout` port. The old guessed implementation is now tracked as a
source-backed WPF adaptation with explicit platform substitutions.

## WinUI 3 Source Inputs

- `src\dxaml\xcp\dxaml\lib\FlyoutBase_partial.cpp`
- `src\dxaml\xcp\dxaml\lib\FlyoutBase_partial.h`
- `src\dxaml\xcp\dxaml\lib\MenuFlyout_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\FlyoutBaseClosingEventArgs.g.cpp`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\FlyoutShowOptions.g.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.Primitives.cs`

## ModernWpf Artifacts

- `ModernWpf.Controls\Flyout\FlyoutBase.cs`
- `ModernWpf.Controls\Flyout\FlyoutBaseClosingEventArgs.cs`
- `ModernWpf.Controls\Flyout\FlyoutPlacementMode.cs`
- `ModernWpf.Controls\Flyout\FlyoutShowMode.cs`
- `ModernWpf.Controls\Flyout\FlyoutShowOptions.cs`
- `ModernWpf.Controls\MenuFlyout\MenuFlyout.cs`
- `ModernWpf.Controls\MenuFlyout\MenuFlyoutPresenter.cs`
- `ModernWpf\Controls\Primitives\PopupEx.cs`
- `ModernWpf\Controls\Primitives\CustomPopupPlacementHelper.cs`
- `test\ModernWpf.WinUI.Tests\Flyout\FlyoutBaseApiTests.cs`
- `test\ModernWpf.WinUI.Tests\MenuFlyout\MenuFlyoutApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf status |
| --- | --- |
| `Target` is a read-only dependency property set while the flyout is open and cleared on close. | Matched for `Flyout` and `MenuFlyout`, including canceled close preservation. |
| `ShouldConstrainToRootBounds` defaults to true and `IsConstrainedToRootBounds` reports the popup constraint state. | Matched as app-visible WPF state; WPF popups are windowed, so enforcement is a placement substitute. |
| `ShowAtWithOptionsImpl` reads position, exclusion rect, show mode, placement, and applies state before the open/no-op decision. | Matched; same-target no-op now still updates source-shaped show mode and placement override state. |
| `FlyoutShowOptions` defaults `ShowMode` and `Placement` to `Auto`, accepts null target only when `Position` is supplied, and rejects empty null-target options. | Matched with WPF root-target substitution for positioned null-target opens. |
| Opening one flyout closes the previous open flyout and stages the latest show request. | Matched with a top-level WPF open/staged flyout path shared by `Flyout` and `MenuFlyout`. |
| Placement-target `Unloaded` hides the flyout. | Matched for `Flyout` and `MenuFlyout`. |
| `Closing` is cancelable and `MenuFlyout::OnClosing` delegates to the base path. | Matched through WPF popup/context-menu cancellation bridges. |
| Placement fallback order tries requested major placement, opposite major placement, then the remaining axis while preserving justification. | Matched in `CustomPopupPlacementHelper`, including full-placement single-choice behavior. |
| `ShowMode.Auto` normalizes to `Standard`; transient modes do not take focus; pointer-move-away dismisses beyond the WinUI 80px threshold. | Matched with a WPF root `MouseMove` substitute. |

## WPF Substitutions

- WPF popup/menu `IsOpen` changes before cancellation can be observed, so canceled close restores `IsOpen` and suppresses the synthetic reopen notification.
- WPF has no direct `XamlRoot` content lookup on `FlyoutBase`; positioned null-target opens use `Application.Current.MainWindow.Content` or the main window as the root placement target.
- WPF does not expose WinUI popup offsets or targetless placement directly; target-point placement uses a zero-size `PlacementRectangle`, and exclusion-rect avoidance is applied in custom-placement coordinates.
- WPF popups are windowed, so `IsConstrainedToRootBounds` is represented by the requested constraint state rather than WinUI's internal popup implementation.
- WPF does not expose WinUI overlay pass-through, popup-root pointer events, child-flyout metadata, target theme forwarding, exact `ActualPlacement`, compositor transitions, or system backdrop directly.

## Current Validation

Run after FlyoutBase-family changes:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~FlyoutBaseApiTests|FullyQualifiedName~MenuFlyoutApiTests" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1
```

Latest verified result on 2026-05-17: FlyoutBase/MenuFlyout API tests passed 24/24.
