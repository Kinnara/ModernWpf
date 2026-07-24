# CommandBarFlyout WinUI 3 Source Audit

Date: 2026-07-19

This audit treats official `microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885` as the behavioral, template,
and accessibility source of truth for the ModernWpf `CommandBarFlyout` port.
The current Gallery source authority is commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`; live comparison continues to use
Microsoft WinUI 3 Controls Gallery `2.9.3.0` with Microsoft Windows App Runtime
`2.2.3.0.0`.

## WinUI 3 Source Inputs

- `controls\dev\CommandBarFlyout\CommandBarFlyout.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyout.h`
- `controls\dev\CommandBarFlyout\CommandBarFlyout.idl`
- `controls\dev\CommandBarFlyout\CommandBarFlyout.xaml`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.h`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.idl`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarAutomationProperties.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarAutomationProperties.h`
- `controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml`
- `controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources_perf2026.xaml`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.cpp`
- `controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBarTemplateSettings.h`
- `controls\dev\Generated\CommandBarFlyout.properties.cpp`
- `controls\dev\Generated\CommandBarFlyoutCommandBar.properties.cpp`
- `controls\dev\Generated\CommandBarFlyoutCommandBarAutomationProperties.properties.cpp`
- `controls\dev\Generated\CommandBarFlyoutCommandBarTemplateSettings.properties.cpp`
- `controls\dev\CommandBarFlyout\APITests\CommandBarFlyoutTests.cs`
- `controls\dev\CommandBarFlyout\InteractionTests\CommandBarFlyoutTests.cs`
- `controls\dev\CommandBarFlyout\Strings\en-us\Resources.resw`
- `dxaml\xcp\dxaml\lib\AppBarButtonAutomationPeer_Partial.cpp`
- `dxaml\xcp\dxaml\lib\AppBarToggleButtonAutomationPeer_Partial.cpp`
- `dxaml\xcp\dxaml\dllsrv\winrt\Microsoft.UI.Xaml.Common.rc`
- `dxaml\test\native\external\controls\commanding\CommandingIntegrationTests.cpp`

Current product blob pins are `7c8b6a6bc8c43d413160e5f7cce076c023ae4e0d`
(flyout runtime), `239ece554112abbd13f123e11d1e0d6df45bd390`
(command-bar runtime), `bcc9d0171251f1130c61004d5d5856e19b31a1ea`
(classic theme), `e761a9f9a000dd93caaefdbde7d1b99c089f3519`
(perf2026 theme), `201c6dae052c4291bafcc9566f5934005acbbcd5`
(API tests), and `35de6ec45045e44da36bfa21877fcbf45a3cd417`
(interaction tests). Relative to previous product pin
`c70471c511a0168b61dcca13af9556465f26b673`, the only control-runtime change is
commit `5c0970f013029ad4e343ff073fc764f1b49088fe`: inserted/replaced commands now
receive their localized control type individually instead of rescanning both
vectors. The root move remains commit
`8463f45162149de0ec3ad7df752596893fe3e13e`. Perf2026 was added by
`55f99cde0`; compared with classic it removes only the redundant
`IconAndLabelPanel.VerticalAlignment=Center` setter from the toggle-button
overflow-with-icons state, which is the shape ModernWpf already uses.

Current Gallery blobs are `e6bf03057f7ad1e661c179a98899368e02208b6b`
(page), `1de584d1e79bf0966827c0b605b79121100c4d36` (code-behind),
`6d2ce0768d78a0b3c5462a44e92f035ce5e43a1e` (displayed snippet),
`73a010396dec3c55ed0c5054776674f0526a548f` (sample XAML), and
`d1c31b098e57e5c0d29fa2ec2041dfab37149d1f` (sample C#). There is no
CommandBarFlyout page/sample change after Gallery conversion commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`.

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
| Adding or replacing a primary/secondary command updates that command's localized MenuItem identity without rescanning every existing command. | Behavior matched through the scoped `IsInCommandBarFlyout` flag and peer lookup: inserted commands immediately expose MenuItem / `menu item` without localized-resource rescans. A current-source regression covers live insertion after the flyout is open. |
| `AlwaysExpanded` forces `ShowMode=Standard`, opens the command bar, and hides the overflow button. | Matched with WPF command-bar state tests. |
| `AlwaysExpanded` rejects attempts to collapse the internal command bar while the owning flyout remains open. | Matched by restoring `IsOpen` from the internal Closing path; the regression explicitly requests a collapse and proves the overflow remains open. |
| Close animation cancels the first `FlyoutBase.Closing`, plays the command-bar close storyboard, and calls `Hide` again for the real close. | Matched through `FlyoutBaseClosingEventArgs.Cancel` and a WPF storyboard completion callback. |
| Current WinUI source prefers `OpeningOpacityStoryboard` / `ClosingOpacityStoryboard` for outer flyout open/close and only falls back to the older clip `OpeningStoryboard` / `ClosingStoryboard` resources when opacity resources are absent. | Matched. ModernWpf uses opacity-only outer open/close storyboards and keeps a fallback lookup for custom templates still using the legacy resource names. Tag checks show this source behavior is present in the WinUI 1.6 line and later, absent from WinUI 1.5. |
| `CommandBarFlyoutCommandBar` owns template settings, open/close animation state, overflow placement visual states, command focus routing, and tab-stop uniqueness. | Matched with WPF template settings, visual states, and focused API coverage. |
| Current primary AppBar styles use `Width=NaN` and no fixed height; the content root has `MinWidth=40` but no fixed `MinHeight`. The primary panel is 40px high with `3,3,0,3` margin, while `HasPrimaryLabels` changes it to `MinHeight=52` and `Height=NaN`. | Matched. The stale fixed 60px width, 55px item height, content-root minimum height, and command-bar height were removed. The Gallery example measures 60x52 primary buttons from content, not template constants. |
| The flyout ellipsis is 36x54 and the overflow presenter has `MinWidth=136`. | Matched by `CommandBarFlyoutEllipsisButtonStyle` and the source-shaped overflow template. The live expanded surface measures 229x136 including shadow in both apps; the raw command union is exactly 217x124. |
| Primary commands that exceed the 440-DIP flyout maximum move from right to left into the secondary presenter, followed by an automatic separator before declared secondary commands. | Matched. ModernWpf reserves the 36-DIP ellipsis, 3-DIP grid spacer, and primary-panel margin before measuring source-order commands. The official 20-primary/5-secondary case now produces exactly 9 primary children and 17 overflow children: 11 moved commands, one generated separator, and five secondary commands. |
| The flyout command bar avoids the WPF `ToolBar` secondary panel path. | Matched by deleting `CommandBarFlyoutToolBar` and using `CommandBarFlyoutOverflowPanel`. |
| `CommandBar::UpdateInputDeviceTypeUsedToOpen` snapshots the input device used to open secondary commands and applies source input-mode visual states to secondary AppBar entries. | Matched for WPF touch/default input. `CommandBarFlyoutCommandBar` snapshots the last WPF key/mouse/touch input before opening, `CommandBarFlyoutOverflowPanel` preserves the owner back-reference during layout updates, and secondary `AppBarButton` / `AppBarToggleButton` entries enter `TouchInputMode` when opened by touch. |
| Presenter shadow is disabled by default, added on flyout open when primary commands exist, removed during close, removed while secondary open/close animations run, then restored after those secondary storyboards complete. | Matched through `FlyoutPresenter.IsDefaultShadowEnabled` toggling. The presenter template renders the WPF `ThemeShadowChrome` depth-32 shadow with WinUI non-tooltip popup insets. |
| `OuterOverflowContentRootShadow` / `NoOuterOverflowContentRootShadow` visual states set or clear the overflow root `ThemeShadow` at `Translation.Z=32`, with no-primary-command flyouts always using the overflow shadow and primary-command flyouts using it when the overflow opens downward. | Matched with an `OuterOverflowContentRootShadowChrome` wrapper around the WPF overflow root, depth `32`, WinUI non-tooltip popup insets, source-shaped `ClearShadow`, and source-shaped `UpdateShadow` state selection. |
| A flyout command bar exposes the `Menu` control type and its AppBar commands expose `MenuItem`; the expanded ellipsis automation name is the localized `Less app bar` string. | Matched with a dedicated command-bar peer, a scoped `IsInCommandBarFlyout` flag for AppBar peers, and open-state automation-name updates. AppBar controls outside CommandBarFlyout keep their existing button roles. |

## WPF Substitutions

- WPF has no WinUI compositor `ThemeShadow` or system backdrop equivalent. Presenter and overflow-root shadows are represented by `ThemeShadowChrome`; exact compositor rasterization and backdrop material remain platform gaps.
- WPF's built-in `Popup` does not expose WinUI `Popup.ActualPlacement`; ModernWpf uses `WindowedPopup` for the CommandBarFlyout overflow surface. Its separate `HwndSource` needs a measured two-pixel platform-anchor compensation (`HorizontalOffset=2`; `VerticalOffset=-2` downward or `+2` upward) to produce the same raw union as WinUI. Opacity storyboards target the hosted `OuterOverflowContentRootShadowChrome` instead of the placeholder popup element.
- WPF automation does not expose WinRT `AutomationEvents.MenuOpened` / `MenuClosed`. Control types, localized type names, expanded ellipsis name, app-visible behavior, and focus routing are matched and covered; only the WinRT-specific event identifiers remain a platform gap.
- WPF AutomationProperties has no `FlowsTo` / `FlowsFrom` attached-property API. ModernWpf matches the source primary/secondary focus graph and tab-stop uniqueness, but cannot publish those two WinUI relationship properties through WPF UIA.
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

Fresh gate-enforced Light `artifacts/visual-checks/20260719-022802-704-69300/report.md` and Dark `artifacts/visual-checks/20260719-022908-765-38068/report.md` runs both retain static primary delta `4.99` with `454x302` versus `453x302` photo crops. Expanded interaction crops are exact `229x136` matches at delta `7.05` / `8.18`, and both raw UIA command unions remain exact `217x124`. Live UIA reports Menu/MenuItem roles and `Less app bar` in both applications. The harness enforces static delta `<=6.0`, static size delta `<=2`, interaction delta `<=9.0`, and exact interaction size parity. Fresh Light/Dark OpenRepeat recordings `artifacts/gallery-recordings/20260719-023035-553/report.md` and `artifacts/gallery-recordings/20260719-023154-586/report.md` pass, detect both opens, and provide dense transition review. The refreshed focused API suite passes 29/29; focused current-source/sample/interaction/gate Gallery coverage passes 5/5 on net8 and net10; Controls and Gallery build on net462, net8, and net10 with zero errors. Controls retains the repository's 18 unrelated net462 warnings, while Gallery is warning-free.

## 2026-07-21 Transition-State Follow-up

The open-surface gate above did not cover first measure, pointer-down, or
collapse transitions. A ten-state paired matrix exposed three WPF-hosting
problems: the first Popup HWND was measured before the shadow insets existed;
transparent AppBar roots did not consistently receive pointer input; and the
secondary `WindowedPopup` child HWND was treated as outside the owning Popup's
light-dismiss capture.

The presenter now enables its WPF shadow substitute before its child is
assigned, AppBar roots have a transparent hit-test background, and
CommandBarFlyout owns light dismiss across both related HWND surfaces while
preserving outside click and owner-deactivation dismissal. Cursor containment
is tested against both presenter bounds and the real `WindowedPopup` HWND.

Final Light
`artifacts/visual-checks/commandbar-state-gate-light-v5/20260721-025151-971-41592/report.md`
and Dark
`artifacts/visual-checks/commandbar-state-gate-dark-v2/20260721-025458-794-39604/report.md`
runs pass all ten states. Collapsed surfaces are exactly `228x66`; expanded
surfaces are exactly `229x136` in both applications. Pointer-over and pressed
states are also required to differ visibly from rest in each application, so a
future hit-test regression cannot pass on static geometry alone.
