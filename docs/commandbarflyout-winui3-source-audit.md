# CommandBarFlyout WinUI 3 Source Audit

Date: 2026-07-17

This audit treats official `microsoft-ui-xaml` `winui3/main` commit
`3cae15f071f1ab8565f9a7592dbf27f04bafe651` (2026-07-13) as the behavioral,
template, and accessibility source of truth for the ModernWpf
`CommandBarFlyout` port. Live comparison uses Microsoft WinUI 3 Controls
Gallery `2.9.3.0` with Microsoft Windows App Runtime `2.2.3.0.0`.

## WinUI 3 Source Inputs

- `controls\dev\CommandBarFlyout\CommandBarFlyout.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyout.h`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.h`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarAutomationProperties.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.h`
- `dxaml\xcp\dxaml\lib\AppBarButtonAutomationPeer_Partial.cpp`
- `dxaml\xcp\dxaml\lib\AppBarToggleButtonAutomationPeer_Partial.cpp`
- `dxaml\xcp\dxaml\dllsrv\winrt\Microsoft.UI.Xaml.Common.rc`
- `dxaml\test\native\external\controls\commanding\CommandingIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBar.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBarAutomationPeer.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutOverflowPanel.cs`
- `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml`
- `ModernWpf.Controls\CommandBar\AppBarButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButton.cs`
- `ModernWpf.Controls\CommandBar\AppBarElementProperties.cs`
- `ModernWpf.Controls\CommandBar\AppBarButtonAutomationPeer.cs`
- `ModernWpf.Controls\CommandBar\AppBarToggleButtonAutomationPeer.cs`
- `ModernWpf\Styles\CommandBar.xaml`
- `ModernWpf\Resources\Strings.resx`
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
| Current primary AppBar styles use `Width=NaN` and no fixed height; the content root has `MinWidth=40` but no fixed `MinHeight`. The primary panel is 40px high with `3,3,0,3` margin, while `HasPrimaryLabels` changes it to `MinHeight=52` and `Height=NaN`. | Matched. The stale fixed 60px width, 55px item height, content-root minimum height, and command-bar height were removed. The Gallery example measures 60x52 primary buttons from content, not template constants. |
| The flyout ellipsis is 36x54 and the overflow presenter has `MinWidth=136`. | Matched by `CommandBarFlyoutEllipsisButtonStyle` and the source-shaped overflow template. The live expanded surface measures 229x136 including shadow in both apps; the raw command union is exactly 217x124. |
| The flyout command bar avoids the WPF `ToolBar` secondary panel path. | Matched by deleting `CommandBarFlyoutToolBar` and using `CommandBarFlyoutOverflowPanel`. |
| `CommandBar::UpdateInputDeviceTypeUsedToOpen` snapshots the input device used to open secondary commands and applies source input-mode visual states to secondary AppBar entries. | Matched for WPF touch/default input. `CommandBarFlyoutCommandBar` snapshots the last WPF key/mouse/touch input before opening, `CommandBarFlyoutOverflowPanel` preserves the owner back-reference during layout updates, and secondary `AppBarButton` / `AppBarToggleButton` entries enter `TouchInputMode` when opened by touch. |
| Presenter shadow is disabled by default, added on flyout open when primary commands exist, removed during close, removed while secondary open/close animations run, then restored after those secondary storyboards complete. | Matched through `FlyoutPresenter.IsDefaultShadowEnabled` toggling. The presenter template renders the WPF `ThemeShadowChrome` depth-32 shadow with WinUI non-tooltip popup insets. |
| `OuterOverflowContentRootShadow` / `NoOuterOverflowContentRootShadow` visual states set or clear the overflow root `ThemeShadow` at `Translation.Z=32`, with no-primary-command flyouts always using the overflow shadow and primary-command flyouts using it when the overflow opens downward. | Matched with an `OuterOverflowContentRootShadowChrome` wrapper around the WPF overflow root, depth `32`, WinUI non-tooltip popup insets, source-shaped `ClearShadow`, and source-shaped `UpdateShadow` state selection. |
| A flyout command bar exposes the `Menu` control type and its AppBar commands expose `MenuItem`; the expanded ellipsis automation name is the localized `Less app bar` string. | Matched with a dedicated command-bar peer, a scoped `IsInCommandBarFlyout` flag for AppBar peers, and open-state automation-name updates. AppBar controls outside CommandBarFlyout keep their existing button roles. |

## WPF Substitutions

- WPF has no WinUI compositor `ThemeShadow` or system backdrop equivalent. Presenter and overflow-root shadows are represented by `ThemeShadowChrome`; exact compositor rasterization and backdrop material remain platform gaps.
- WPF's built-in `Popup` does not expose WinUI `Popup.ActualPlacement`; ModernWpf uses `WindowedPopup` for the CommandBarFlyout overflow surface. Its separate `HwndSource` needs a measured two-pixel platform-anchor compensation (`HorizontalOffset=2`; `VerticalOffset=-2` downward or `+2` upward) to produce the same raw union as WinUI. Opacity storyboards target the hosted `OuterOverflowContentRootShadowChrome` instead of the placeholder popup element.
- WPF automation does not expose WinRT `AutomationEvents.MenuOpened` / `MenuClosed`. Control types, localized type names, expanded ellipsis name, app-visible behavior, and focus routing are matched and covered; only the WinRT-specific event identifiers remain a platform gap.
- WPF template binding can lag dependency-property callbacks during measurement, so ModernWpf keeps a narrow deferred size refresh after source-tracked secondary command property changes.
- WPF has no WinUI gamepad/remote input-mode service in this control path. ModernWpf wires the source-shaped touch/default subset through WPF key/mouse/touch events and leaves gamepad/remote selection as a platform gap.

## Current Validation

Run after CommandBarFlyout changes:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~CommandBarFlyoutApiTests" --no-restore
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter "FullyQualifiedName~CommandBarFlyoutApiTests|FullyQualifiedName~CommandBarApiTests|FullyQualifiedName~TemplateParityTests|FullyQualifiedName~SyncMatrixTests"
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj -f net462 --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj -f net8.0-windows7.0 --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj -f net10.0-windows7.0 --no-restore
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls CommandBarFlyout -Theme Light -Reference InstalledWinUI3Gallery -IncludeInteractions -FailOnDifference
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls CommandBarFlyout -Theme Dark -Reference InstalledWinUI3Gallery -IncludeInteractions -FailOnDifference
git diff --check
```

Final gate-enforced Light `artifacts/visual-checks/20260717-214308-498-41228/report.md` and Dark `artifacts/visual-checks/20260717-214353-622-95916/report.md` runs both have static primary delta `4.99` with `454x302` versus `453x302` photo crops. Expanded interaction crops are exact `229x136` matches at delta `7.05` / `8.18`, and both raw UIA command unions are exact `217x124`. Live UIA reports Menu/MenuItem roles and `Less app bar` in both applications. The harness enforces static delta `<=6.0`, static size delta `<=2`, interaction delta `<=9.0`, and exact interaction size parity. The full focused API suite passes 27/27, the sample/theme/interaction/gate slice passes 4/4 on net8 and net10, and ModernWpf.Controls builds for net462, net8, and net10.
