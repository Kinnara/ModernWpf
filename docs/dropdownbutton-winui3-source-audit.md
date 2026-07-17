# DropDownButton WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

Last verified: 2026-07-17.

WinUI source files:

- `src\controls\dev\DropDownButton\DropDownButton.cpp`
- `src\controls\dev\DropDownButton\DropDownButton.xaml`
- `src\controls\dev\DropDownButton\DropDownButton_themeresources.xaml`
- `src\controls\dev\DropDownButton\DropDownButtonAutomationPeer.cpp`

ModernWpf files:

- `ModernWpf.Controls\DropDownButton\DropDownButton.cs`
- `ModernWpf.Controls\DropDownButton\DropDownButton.xaml`
- `ModernWpf.Controls\DropDownButton\DropDownButtonAutomationPeer.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\DropDownButton\DropDownButtonApiTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Ported Source Behavior

- Flyout event registration now follows the source current-flyout registration shape. `OnApplyTemplate` and `Flyout` changes both re-register the active flyout and keep `IsFlyoutOpen` plus expand/collapse automation state in sync.
- The template no longer uses WPF `ControlTemplate.Triggers` for pointer/pressed/disabled colors. Those state values now live in `VisualStateEx.Setters`, matching WinUI's visual-state-owned template state model.
- The chevron icon now uses source-sized 12x12 layout with the existing WPF `FontIconFallback` substitution.
- DropDownButton automation remains source-shaped: the peer exposes `ExpandCollapse`, reports class name `DropDownButton`, and expands/collapses through the owner.
- The source `ButtonBorderBrush` elevation gradient is retained without a capture-specific color substitution. A 96-DPI renderer regression test proves the declared WinUI Light and Dark bottom endpoints composite to exact `#CCCCCC` and `#303030` pixels.
- The generated Gallery page retains the official two-example shape, and its primary `Email` sample has the same `78x32` control bounds as the installed WinUI Gallery in both themes.

## WPF Substitutions

- WinUI `Grid` supports `BackgroundSizing`, `CornerRadius`, `BorderBrush`, and `BorderThickness`; WPF `Grid` does not. ModernWpf keeps `GridEx` for the root chrome.
- WinUI `AnimatedIcon` uses `AnimatedChevronDownSmallVisualSource`; ModernWpf keeps `FontIconFallback` because the repo does not carry the animated source.
- WPF has no direct `AutomationProperties.AccessibilityView=Raw` equivalent.
- WPF template values use `VisualStateEx.Setters` instead of native WinUI `VisualState.Setters`.
- The live installed-Gallery comparison traverses different WPF and WinUI display rasterizers. At the current desktop scale, WPF gives the trailing elevation scanline fractional coverage even though the same source brush renders the exact WinUI endpoint at 96 DPI. A trial capture-specific alpha correction improved the live crop but produced `#8E8E8E` instead of `#CCCCCC` in the 96-DPI renderer test, so it was rejected. The strict live gate therefore allows the measured antialiasing residual while keeping product rendering correct across DPI contexts.

## Verification

Focused tests cover property defaults, template shape, visual-state setter targets and application, chevron sizing, expand/collapse flyout state tracking, exact 96-DPI elevation endpoints, Gallery sample structure, and the live comparison gate.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~GridExElevationBorderMatchesWinUIEndpointsAt96Dpi|FullyQualifiedName~DropDownButtonApiTests" --no-restore`
  - Passed 9/9.
- `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~DropDownButtonSampleMatchesWinUIGalleryExamples|FullyQualifiedName~GalleryVisualChecksEnforceDropDownButtonPixelParityThreshold" --no-restore`
  - Passed 2/2.
- The same Gallery filter passed 2/2 for `net10.0-windows7.0`.
- `Run-GalleryVisualChecks.ps1 -Controls DropDownButton -Theme Light -Build -FailOnDifference`
  - Passed the strict `4.0` gate with exact `78x32` crops and primary delta `3.68`: `artifacts/visual-checks/20260717-064508-588-80056/report.md`.
- `Run-GalleryVisualChecks.ps1 -Controls DropDownButton -Theme Dark -FailOnDifference`
  - Passed the strict `4.0` gate with exact `78x32` crops and primary delta `2.69`: `artifacts/visual-checks/20260717-064534-981-29992/report.md`.
- `git diff --check`
  - Only CRLF normalization warnings.
