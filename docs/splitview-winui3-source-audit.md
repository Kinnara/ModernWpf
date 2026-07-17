# SplitView WinUI 3 Source Audit

Date: 2026-07-17

ModernWpf `SplitView` is now treated as a whole-control WPF port of the local WinUI 3 source rather than the older guessed WPF surface.

Source snapshot:

- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\core\core\elements\SplitView.cpp`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\core\inc\SplitView.h`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\dxaml\lib\SplitView_Partial.cpp`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\tools\XCPTypesAutoGen\Modules\Controls\SplitView\SplitView.cs`
- `D:\repos\microsoft-ui-xaml\src\dxaml\xcp\tools\XCPTypesAutoGen\Modules\Controls\SplitView\SplitViewTemplateSettings.cs`
- `D:\repos\microsoft-ui-xaml\src\controls\dev\SplitView\SplitView_themeresources.xaml`

ModernWpf targets:

- `ModernWpf.Controls\SplitView\SplitView.cs`
- `ModernWpf.Controls\SplitView\SplitView.properties.cs`
- `ModernWpf.Controls\SplitView\SplitViewTemplateSettings.cs`
- `ModernWpf.Controls\SplitView\SplitView.xaml`
- `test\ModernWpf.WinUI.Tests\SplitView\SplitViewApiTests.cs`

## Ported Source Behavior

- Replaced the hand-written display-mode branching with the WinUI source visual-state table: `[DisplayMode][PanePlacement][IsPaneOpen]`.
- Ported source `OpenPaneLength=NaN` behavior by measuring the pane and using the measured desired width for `TemplateSettings.OpenPaneLength`, grid lengths, negative lengths, and pane clip geometry.
- Moved `Content`, `Pane`, `IsPaneOpen`, pane lengths, `DisplayMode`, and `PanePlacement` dependency properties to source-shaped measure-affecting metadata.
- Ported source light-dismiss close flow: light dismiss raises cancelable `PaneClosing` first, cancellation keeps the pane open, accepted dismiss sets `IsPaneOpen=false`, and the normal close path does not honor cancellation after the property has already changed.
- Added the template-owned `LightDismissLayer` pointer path, while keeping the WPF window-preview substitute for WinUI's outer dismiss layer outside the `SplitView` bounds.
- Added source-shaped focus save/restore for light-dismissible panes and Escape-key close behavior.
- Completed the missing right-side inline transition set from `SplitView_themeresources.xaml`: `Closed -> OpenInlineRight`, `OpenInlineRight -> Closed`, `ClosedCompactRight -> OpenInlineRight`, and `OpenInlineRight -> ClosedCompactRight`.

## WPF Substitutions

- WinUI uses `XamlRoot`, popup-hosted polygonal outer dismiss layers, back-button integration, gamepad XY focus, automation peer factory indexes, and `LightDismissOverlayMode.Auto` Xbox detection. ModernWpf maps these to WPF window preview input, normal WPF focus APIs, Escape handling, and `LightDismissOverlayMode.On` overlay visibility.
- WinUI `Grid` supports `BorderBrush`, `BorderThickness`, and `CornerRadius`; the WPF template keeps the existing `Border` pane root for chrome.
- WinUI `VisualState.Setters` remain represented by `VisualStateEx.Setters` where WPF lacks native setter support.

## Tests

- `SplitViewApiTests.TemplateSettingsUseMeasuredPaneLengthWhenOpenPaneLengthIsAuto`
- `SplitViewApiTests.LightDismissLayerRespectsPaneClosingCancellation`
- `SplitViewApiTests.EscapeClosesLightDismissiblePane`
- `SplitViewApiTests.RightInlineTransitionsMatchWinUISourceTemplate`

Validation run:

```text
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~SplitView
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~NavigationView
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~SyncMatrixTests
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
git diff --check
```

Latest focused result: `SplitView` product tests passed 14/14 on
`net8.0-windows7.0`. The Gallery sample plus visual-gate contract passed 2/2
on both `net8.0-windows7.0` and `net10.0-windows7.0`.

The installed WinUI 3 Gallery crop uses the sample-scoped `PaneRoot` and
`content` bounds so the Gallery shell's duplicate `PaneRoot` automation ID is
not mistaken for the control. Exact `400x300` pane+content crops pass the
enforced `4.0` gate:

- Light: `artifacts\visual-checks\20260717-081604-045-11108\report.md`, delta `3.23`.
- Dark: `artifacts\visual-checks\20260717-081618-744-58120\report.md`, delta `3.37`.

Pane width, divider, backgrounds, headers, item layout, and content placement
align; the remaining bounded delta is WPF text and symbol-font rasterization.
