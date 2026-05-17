# ToggleButton WinUI 3 Source Audit

Date: 2026-05-17

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`

## WinUI Source Files

- `src\dxaml\xcp\dxaml\lib\ToggleButton_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ToggleButton_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ToggleButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ToggleButtonAutomationPeer_Partial.h`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\ToggleButton.g.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\Modules\Controls\ToggleButton.cs`
- `src\controls\dev\CommonStyles\ToggleButton_themeresources.xaml`
- `src\dxaml\test\native\external\controls\primitives\togglebutton\ToggleButtonIntegrationTests.cpp`
- `src\controls\dev\CommonStyles\APITests\CommonStylesTests.cs`

## ModernWpf Files

- `ModernWpf\Styles\ToggleButton.xaml`
- `ModernWpf\Controls\Primitives\ToggleButtonHelper.cs`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ToggleButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Behavior

ModernWpf keeps mapping WinUI `ToggleButton` to WPF's platform `ToggleButton`, but the default style no longer relies on guessed WPF trigger combinations:

- Deleted the `ControlTemplate.Triggers` visual matrix from `DefaultToggleButtonStyle`.
- Added source-shaped `CommonStates` for `Normal`, `PointerOver`, `Pressed`, `Disabled`, `Checked`, `CheckedPointerOver`, `CheckedPressed`, `CheckedDisabled`, `Indeterminate`, `IndeterminatePointerOver`, `IndeterminatePressed`, and `IndeterminateDisabled`.
- Moved foreground/background/border/background-sizing changes into `VisualStateEx.Setters`.
- Added `ToggleButtonCheckedStateBackgroundSizing`, matching the source resource used by checked states.
- Enabled `ToggleButtonHelper.VisualStateSettersEnabled` on the default style.
- Updated `ToggleButtonHelper` to drive source indeterminate pointer, pressed, and disabled states instead of collapsing all nullable `IsChecked` states to plain `Indeterminate`.
- `ToggleButtonHelper` reapplies the selected source state on the dispatcher because WPF's built-in `ToggleButton` visual-state pass can otherwise overwrite a combined source state with the older separate checked-state name.

## WPF Substitutions

- WPF's platform `ToggleButton` still owns `IsChecked`, `IsThreeState`, routed events, keyboard handling, focus behavior, and automation peer behavior.
- WinUI focus states `Focused`, `PointerFocused`, and `Unfocused` are represented by WPF focus visuals rather than a source focus-state group in this template.
- WinUI `ContentPresenter.AutomationProperties.AccessibilityView=Raw` has no direct WPF template equivalent here.
- WinUI `ThemeResource` lookup is represented by ModernWpf `DynamicResource`.
- `VisualState.Setters` is represented by `VisualStateEx.Setters`.

## Tests And Validation

- `dotnet build .\ModernWpf\ModernWpf.csproj --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~ToggleButtonVisualStateTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~LayoutCompatibilityApiTests.ToggleButtonCheckedStateUsesOuterBackgroundSizing`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.ProductTemplatesUseVisualStateExForConvertedStateSetters`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.ProductTemplatesDoNotContainRawWinUIVisualStateSetters`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~Expander`
- `git diff --check`
