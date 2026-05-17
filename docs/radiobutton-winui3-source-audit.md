# RadioButton WinUI 3 Source Audit

Date: 2026-05-17

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`

## WinUI Source Files

- `src\dxaml\xcp\dxaml\lib\RadioButton_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\RadioButton_Partial.h`
- `src\dxaml\xcp\dxaml\lib\RadioButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\RadioButtonAutomationPeer_Partial.h`
- `src\controls\dev\CommonStyles\RadioButton_themeresources.xaml`
- `src\dxaml\test\native\external\controls\radiobutton\RadioButtonIntegrationTests.cpp`

## ModernWpf Files

- `ModernWpf\Styles\RadioButton.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `ModernWpf\Controls\Primitives\ButtonHelper.cs`
- `test\ModernWpf.WinUI.Tests\CommonStyles\RadioButtonVisualStateTests.cs`

## Ported Behavior

ModernWpf keeps using the WPF platform `RadioButton`, but the style layer now follows the WinUI 3 CommonStyles source instead of WPF trigger guesses:

- The old `ControlTemplate.Triggers` visual matrix was deleted.
- `CommonStates` now use the source state name `PointerOver` instead of WPF `MouseOver`.
- Pointer-over, pressed, and disabled chrome now uses source-shaped `VisualStateEx.Setters` for the content presenter, root chrome, outer ellipse, checked outer ellipse, and glyph fill/stroke targets.
- `Checked` now carries the source glyph-stroke and pressed-glyph background object-animation targets through `VisualStateEx.Setters`.
- Light and Dark RadioButton theme resources were corrected from the old color-only animation workaround back to brush-valued WinUI source resources so WPF setters can apply them directly.
- `ButtonHelper.VisualStateSettersEnabled` drives the source common-state names: `Disabled`, `Pressed`, `PointerOver`, and `Normal`.
- WPF's native toggle path still drives the `Checked`, `Unchecked`, and `Indeterminate` check-state group.

## WPF Substitutions

- WinUI's native `RadioButton` implementation is mapped to WPF's platform `RadioButton`; this slice does not introduce a new `ModernWpf.Controls.RadioButton`.
- WPF owns group selection, `GroupName`, focus traversal, toggling, and the platform automation peer. WinUI's named-group registry, gamepad traversal, and WinRT automation event details remain platform gaps.
- WinUI `VisualState.Setters` / object animations are represented by `VisualStateEx.Setters`.
- WinUI `Grid` root chrome is represented by a WPF `Border` named `RootGrid`, because WPF `Grid` does not expose `CornerRadius`, `Background`, `BorderBrush`, and `BorderThickness` as the same chrome surface.
- WinUI `ContentPresenter` is represented by `ContentPresenterEx` so WPF can carry the WinUI foreground state target shape.
- WinUI `AutomationProperties.AccessibilityView=Raw` on template content has no direct WPF template equivalent.
- Focus visuals remain WPF focus visuals.

## Tests And Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~RadioButtonVisualStateTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.CoreResidualPresenterSlotsUseWinUIPresenterShape`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.ProductTemplatesDoNotContainRawWinUIVisualStateSetters`
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore`
- `git diff --check`
