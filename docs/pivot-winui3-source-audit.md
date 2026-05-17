# Pivot WinUI 3 Source Audit

Date: 2026-05-17

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`

## WinUI Source Files

- `src\dxaml\phone\lib\Pivot_Partial.cpp`
- `src\dxaml\phone\lib\Pivot_Partial.h`
- `src\dxaml\phone\lib\PivotPanel_Partial.cpp`
- `src\dxaml\phone\lib\PivotHeaderPanel_Partial.cpp`
- `src\dxaml\phone\lib\PivotHeaderItem.cpp`
- `src\dxaml\phone\lib\PivotItem_Partial.cpp`
- `src\dxaml\phone\lib\PivotAutomationPeer_Partial.cpp`
- `src\controls\dev\CommonStyles\Pivot_themeresources.xaml`
- `src\dxaml\test\native\external\controls\pivot\PivotIntegrationTests.cpp`

## ModernWpf Files

- `ModernWpf\Styles\Pivot.xaml`
- `ModernWpf\Controls\Primitives\PivotHelper.cs`
- `ModernWpf\Controls\Primitives\PivotHeaderScrollViewer.cs`
- `test\ModernWpf.WinUI.Tests\CommonStyles\PivotVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Behavior

ModernWpf keeps mapping Pivot to WPF `TabControl` / `TabItem`, but the style/helper layer no longer uses WPF trigger guesses for source-owned visual states:

- Deleted all `ControlTemplate.Triggers` from `TabItemPivotStyle`, the previous/next navigation button templates, and `TabControlPivotStyle`.
- Pivot header item `SelectionStates` now carry the source foreground/background/selected-pipe targets in `VisualStateEx.Setters`, including source `SelectedPressed`.
- `PivotHelper` now drives selected pressed state instead of treating selected pointer-over as the only selected interactive state.
- Previous/next navigation button templates now use source `PointerOver` and `Pressed` `CommonStates` with `VisualStateEx.Setters`, driven by `ButtonHelper.VisualStateSettersEnabled`.
- `TabControlPivotStyle` now has source `NavigationButtonsVisibility` states: `NavigationButtonsHidden`, `NavigationButtonsVisible`, `PreviousButtonVisible`, and `NextButtonVisible`.
- `PivotHelper.NavigationButtonsVisualStateSettersEnabled` maps WPF header hover and `PivotHeaderScrollViewer.CanScrollLeft` / `CanScrollRight` into the WinUI navigation-button state names, including the source fallback from one-sided button states to `NavigationButtonsVisible` if a retemplate lacks those states.
- `PivotHelper.TitleVisibility` follows WinUI `UpdateTitleControlVisibility`: the title presenter is visible when either `Title` or `TitleTemplate` is non-null, including an empty-string title.

## WPF Substitutions

- This slice does not add `Pivot`, `PivotItem`, `PivotPanel`, or `PivotHeaderPanel` controls. WPF `TabControl`, `TabItem`, `StackPanel`, and `PivotHeaderScrollViewer` remain the platform substitute under the no-new-controls rule.
- WinUI `VisualState.Setters` / object animations are represented by `VisualStateEx.Setters`.
- WinUI pointer-enter/exit handling is represented by WPF mouse enter/leave on the header, content panel, and navigation buttons. Touch/pen filtering is not represented.
- WinUI static headers, header carousel, manipulation pivot state machine, drag curves, gamepad behavior, automation peers, and content transition internals remain WPF substitution gaps.
- WinUI `FontIcon` navigation glyphs remain represented by `FontIconFallback` and geometry resources.
- WinUI `ContentPresenter.OpticalMarginAlignment` and `ContentTransitions` have no direct WPF equivalent in this template.

## Tests And Validation

- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~PivotVisualStateTests`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter "FullyQualifiedName~LayoutCompatibilityApiTests.CorePivotTemplatesUseWinUIPresenterSlots|FullyQualifiedName~LayoutCompatibilityApiTests.CorePivotHeaderItemSelectionStatesUseVisualStateSetters"`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.CoreResidualPresenterSlotsUseWinUIPresenterShape`
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests.ProductTemplatesDoNotContainRawWinUIVisualStateSetters`
- `git diff --check`
