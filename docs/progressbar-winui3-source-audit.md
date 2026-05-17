# ProgressBar WinUI 3 Source Audit

Source of truth: `D:\repos\microsoft-ui-xaml`

WinUI 3 files audited:

- `src\controls\dev\ProgressBar\ProgressBar.cpp`
- `src\controls\dev\ProgressBar\ProgressBar.h`
- `src\controls\dev\ProgressBar\ProgressBar.idl`
- `src\controls\dev\ProgressBar\ProgressBar.xaml`
- `src\controls\dev\ProgressBar\ProgressBarAutomationPeer.cpp`
- `src\controls\dev\ProgressBar\ProgressBarTemplateSettings.cpp`
- `src\controls\dev\ProgressBar\APITests\ProgressBarTests.cs`

ModernWpf files:

- `ModernWpf\ProgressBar\ProgressBar.cs`
- `ModernWpf\ProgressBar\ProgressBar.xaml`
- `ModernWpf\ProgressBar\ProgressBarAutomationPeer.cs`
- `ModernWpf\ProgressBar\ProgressBarTemplateSettings.cs`
- `test\ModernWpf.WinUI.Tests\ProgressBar\ProgressBarApiTests.cs`
- `test\ModernWpf.WinUI.Tests\ProgressBar\ProgressBarInteractionTests.cs`

## Source-Backed Behavior

ModernWpf now follows the WinUI 3 ProgressBar implementation for the feasible WPF surface:

- `IsIndeterminate`, `ShowPaused`, `ShowError`, `Value`, `Minimum`, `Maximum`, `Padding`, and `Visibility` all drive source-shaped state and indicator-width refresh paths.
- Indeterminate visual states are selected only while the control is visible; hidden indeterminate bars fall back to determinate/error/paused states, matching the source `Visibility == Visible` guard.
- Indicator width subtracts horizontal padding and border thickness before applying determinate and indeterminate widths.
- The second indeterminate indicator uses the full available width while paused or error, and 60 percent otherwise.
- `ContainerAnimationMidPosition` is source-backed at `0`.
- WinUI 3 animation keyframes for the paused/error indeterminate paths are mirrored in the WPF template where WPF storyboard syntax can represent them.
- `ProgressBarAutomationPeer.GetNameCore()` prefixes the source status string to the base automation name for error, paused, and busy states.

## WPF Substitutions

- WinUI `RegisterPropertyChangedCallback` maps to WPF property overrides and `OnPropertyChanged` for `Visibility`.
- WinUI `XamlRoot.RasterizationScale` maps to `VisualTreeHelper.GetDpi(this).DpiScaleX` for layout-rounded border subtraction.
- WinUI permits early source clip geometry construction with dimensions that WPF `Rect` rejects. The WPF port clamps the clip width and height to zero during pre-layout property changes.
- The source template has an unused `Normal` state in `CommonStates`. WPF base control state management treats `Normal` specially and can override ProgressBar's source-owned determinate state, so the WPF template omits that state.
- WinUI `CompositeTransform` and `RepositionThemeAnimation` map to WPF `TranslateTransform` and key-frame storyboards.
- `ProgressBarTrackHeight` stays resource-overridable through a WPF `TemplateSettings.TrackHeight` binding to preserve the source APITest behavior under WPF resource lookup.

## Test Coverage

ModernWpf covers the source-backed WPF slice with:

- resource overridability from the upstream ProgressBar APITest;
- range automation, value/min/max updates, padding and border width recalculation;
- source indeterminate paused/error indicator-width behavior;
- hidden indeterminate fallback state selection;
- source visual-state setters for nested brush color paths;
- retemplate width/state behavior;
- indeterminate `RangeValue` suppression;
- status automation name prefix behavior.
