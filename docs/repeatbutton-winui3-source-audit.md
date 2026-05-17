# RepeatButton WinUI 3 Source Audit

Date: 2026-05-17

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`

## WinUI Source Files

- `src\dxaml\xcp\dxaml\lib\RepeatButton_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\RepeatButton_Partial.h`
- `src\dxaml\xcp\dxaml\lib\RepeatButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\RepeatButtonAutomationPeer_Partial.h`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\RepeatButton.g.cpp`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\RepeatButtonAutomationPeer.g.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.Primitives.cs`
- `src\controls\dev\CommonStyles\RepeatButton_themeresources.xaml`
- `src\dxaml\test\native\external\controls\primitives\repeatbutton\RepeatButtonIntegrationTests.cpp`
- `src\controls\dev\CommonStyles\APITests\BaselineResources2dot5stable.cs`

## ModernWpf Files

- `ModernWpf\Styles\RepeatButton.xaml`
- `ModernWpf\Controls\Primitives\ButtonHelper.cs`
- `test\ModernWpf.WinUI.Tests\CommonStyles\RepeatButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Behavior

ModernWpf keeps mapping WinUI `RepeatButton` to WPF's platform `RepeatButton`, but the default style no longer relies on guessed WPF trigger combinations:

- Deleted the `ControlTemplate.Triggers` visual matrix from `DefaultRepeatButtonStyle`.
- Added source-shaped `CommonStates` for `Normal`, `PointerOver`, `Pressed`, and `Disabled`.
- Moved foreground/background/border changes into `VisualStateEx.Setters`.
- Enabled `ButtonHelper.VisualStateSettersEnabled` so the WPF platform `RepeatButton` drives WinUI's `PointerOver` state name instead of WPF's older `MouseOver` name.
- Set the default style `ClickMode` to `Press`, matching WinUI `RepeatButton::Initialize`.

## WPF Substitutions

- WPF's platform `RepeatButton` still owns repeat timing, `Delay`, `Interval`, pointer capture, keyboard handling, and automation peer behavior.
- WinUI's `IgnoreTouchInput` internal hook has no public WPF equivalent in the stock platform control.
- WinUI focus states `Focused`, `PointerFocused`, and `Unfocused` are represented by WPF focus visuals rather than a source focus-state group in this template.
- WinUI `ContentPresenter.AutomationProperties.AccessibilityView=Raw` has no direct WPF template equivalent here.
- WinUI `ThemeResource` lookup is represented by ModernWpf `DynamicResource`.
- `VisualState.Setters` is represented by `VisualStateEx.Setters`.

## Tests And Validation

- `dotnet build .\ModernWpf\ModernWpf.csproj --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~RepeatButtonVisualStateTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~LayoutCompatibilityApiTests.RepeatButtonTemplateUsesContentPresenterExDirectly`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.ProductTemplatesUseVisualStateExForConvertedStateSetters`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.ProductTemplatesDoNotContainRawWinUIVisualStateSetters`
- `git diff --check`
