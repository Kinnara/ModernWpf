# DropDownButton WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

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

## Ported Source Behavior

- Flyout event registration now follows the source current-flyout registration shape. `OnApplyTemplate` and `Flyout` changes both re-register the active flyout and keep `IsFlyoutOpen` plus expand/collapse automation state in sync.
- The template no longer uses WPF `ControlTemplate.Triggers` for pointer/pressed/disabled colors. Those state values now live in `VisualStateEx.Setters`, matching WinUI's visual-state-owned template state model.
- The chevron icon now uses source-sized 12x12 layout with the existing WPF `FontIconFallback` substitution.
- DropDownButton automation remains source-shaped: the peer exposes `ExpandCollapse`, reports class name `DropDownButton`, and expands/collapses through the owner.

## WPF Substitutions

- WinUI `Grid` supports `BackgroundSizing`, `CornerRadius`, `BorderBrush`, and `BorderThickness`; WPF `Grid` does not. ModernWpf keeps `GridEx` for the root chrome.
- WinUI `AnimatedIcon` uses `AnimatedChevronDownSmallVisualSource`; ModernWpf keeps `FontIconFallback` because the repo does not carry the animated source.
- WPF has no direct `AutomationProperties.AccessibilityView=Raw` equivalent.
- WPF template values use `VisualStateEx.Setters` instead of native WinUI `VisualState.Setters`.

## Verification

Focused tests cover property defaults, template shape, visual-state setter targets and application, chevron sizing, and expand/collapse flyout state tracking.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~DropDownButton" --no-restore`
  - Passed 6/6.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with existing warnings.
- `git diff --check`
  - Only CRLF normalization warnings.
