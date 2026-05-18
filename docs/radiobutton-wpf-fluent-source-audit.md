# RadioButton Official WPF Fluent Source Audit

Date: 2026-05-18

Source snapshot: `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent`

## Official WPF Fluent Source Files

- `Themes\Fluent.Light.xaml`
- `Themes\Fluent.Dark.xaml`
- `Themes\Fluent.HC.xaml`
- `Themes\Fluent.xaml`

## ModernWpf Files

- `ModernWpf\Styles\RadioButton.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\RadioButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

ModernWpf now treats stock WPF `RadioButton` as an official WPF Fluent-backed stock control:

- The previous WinUI `VisualStateEx` / `ButtonHelper.VisualStateSettersEnabled` stock-style path was deleted.
- The template now uses the official WPF Fluent `RootBorder`, `RootGrid`, WPF `ContentPresenter`, `OuterEllipse`, `CheckOuterEllipse`, `CheckGlyph`, and `PressedCheckGlyph` structure.
- `CommonStates` uses official WPF state names `Normal`, `MouseOver`, and `Pressed`, with scale-transform animations on `CheckGlyph`.
- Chrome, checked, pointer-over, pressed, disabled, and right-to-left behavior now uses WPF `Trigger` / `MultiTrigger` entries instead of `VisualStateEx.Setters`.
- `RadioButtonPadding`, `RadioButtonStrokeThickness`, `RadioButtonCheckGlyphSize`, and the official checked outer-ellipse resource keys are present for the stock template.

## WPF Substitutions

- Official WPF Fluent uses `DefaultControlFocusVisualStyle`; ModernWpf keeps its existing cross-target system focus visual bridge with `FocusVisualHelper`.
- Official WPF Fluent uses the platform `Border.CornerRadius` property; ModernWpf uses `ControlHelper.CornerRadius` for older target-framework compatibility.
- Existing ModernWpf brush aliases are retained where they map to the same Fluent concepts, and the official `RadioButtonCheckOuterEllipseChecked*` keys are added as aliases for the trigger targets.
- Platform `RadioButton` grouping, focus traversal, toggle behavior, and automation remain owned by WPF.

## Tests And Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter RadioButtonVisualStateTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~LayoutCompatibilityApiTests.CoreResidualTemplatesUseExpectedPresenterSlots`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "RadioButtonVisualStateTests|TemplateParityTests|SyncMatrixTests"`
- `dotnet build .\ModernWpf.sln --no-restore`
- `git diff --check`
