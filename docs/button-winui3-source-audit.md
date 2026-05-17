# Button WinUI 3 Source Audit

Date: 2026-05-17

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`

## WinUI Source Files

- `src\dxaml\xcp\dxaml\lib\Button_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\Button_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ButtonBase_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ButtonBase_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ButtonBaseAutomationPeer_Partial.cpp`
- `src\controls\dev\CommonStyles\Button_themeresources.xaml`

## ModernWpf Files

- `ModernWpf\Styles\Button.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `ModernWpf\Controls\Primitives\ButtonHelper.cs`
- `test\ModernWpf.WinUI.Tests\CommonStyles\ButtonVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpfTestApp\ApiTests\BaselineResources.cs`

## Ported Behavior

ModernWpf keeps using the WPF platform `Button` type, but the style layer now follows the WinUI 3 source templates instead of WPF trigger guesses:

- The old `ControlTemplate.Triggers` matrices were deleted from the default and accent templates.
- Default button pointer-over, pressed, and disabled chrome now uses `VisualStateEx.Setters` for `ContentPresenter.Background`, `ContentPresenter.BorderBrush`, and `ContentPresenter.Foreground`.
- Accent button pointer-over, pressed, and disabled chrome now uses the same setter-backed source shape, with `BackgroundSizing=OuterBorderEdge`.
- Default button keeps WinUI source `AnimatedIcon.State` fallback setters on `ContentPresenter`.
- Accent button intentionally does not set `AnimatedIcon.State`, matching the current WinUI 3 `AccentButtonStyle` source template.
- `SubtleButtonStyle` and its theme resources were added from the WinUI 3 source template/resource set.
- `ButtonHelper` remains the WPF driver for the source common-state names: `Disabled`, `Pressed`, `PointerOver`, and `Normal`.

## WPF Substitutions

- WinUI's native `Button` and `ButtonBase` implementation is mapped to WPF's platform `Button` plus ModernWpf style/helper behavior; this slice does not introduce a new `ModernWpf.Controls.Button`.
- WinUI `VisualState.Setters` are represented by `VisualStateEx.Setters`.
- WinUI `ContentPresenter` is represented by `ContentPresenterEx` so WPF can carry WinUI chrome properties and animated icon state compatibility.
- WinUI `AutomationProperties.AccessibilityView=Raw` on template content has no direct WPF template equivalent.
- WinUI `Flyout` / `OpenAssociatedFlyout`, keyboard accelerator, and WinRT automation internals are platform gaps for WPF `Button`.
- Focus visuals remain WPF focus visuals.

## Tests And Validation

- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~ButtonVisualStateTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~CommonStylesResourceTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter "FullyQualifiedName~LayoutCompatibilityApiTests.ButtonTemplateForwardsControlHelperLayoutSurface|FullyQualifiedName~LayoutCompatibilityApiTests.AccentButtonStyleUsesOuterBackgroundSizing"`
