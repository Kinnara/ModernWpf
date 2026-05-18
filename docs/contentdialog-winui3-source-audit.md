# ContentDialog WinUI 3 Source Audit

ModernWpf `ContentDialog` is now treated as a source-backed WPF port of the local WinUI 3 implementation instead of the older WPF-written dialog surface.

## Source Files

Primary WinUI 3 source references:

- `src\dxaml\xcp\dxaml\lib\ContentDialog_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ContentDialog_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ContentDialogMetadata.cpp`
- `src\dxaml\xcp\dxaml\lib\ContentDialogMetadata.h`
- `src\dxaml\xcp\dxaml\lib\ContentDialogClosingEventArgs_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ContentDialogClosingDeferral_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ContentDialogButtonClickEventArgs_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ContentDialogButtonClickDeferral_Partial.h`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs`
- `src\controls\dev\CommonStyles\ContentDialog_themeresources.xaml`
- `src\dxaml\test\native\external\controls\contentdialog\ContentDialogIntegrationTests.cpp`
- `src\dxaml\test\native\external\controls\contentdialog\ContentDialogAutomationIntegrationTests.cpp`

ModernWpf files:

- `ModernWpf.Controls\ContentDialog\ContentDialog.cs`
- `ModernWpf.Controls\ContentDialog\ContentDialog.xaml`
- `ModernWpf.Controls\ContentDialog\ContentDialogButtonClickEventArgs.cs`
- `ModernWpf.Controls\ContentDialog\ContentDialogButtonClickDeferral.cs`
- `ModernWpf.Controls\ContentDialog\ContentDialogClosingEventArgs.cs`
- `ModernWpf.Controls\ContentDialog\ContentDialogClosingDeferral.cs`
- `ModernWpf.Controls\ContentDialog\ContentDialogClosedEventArgs.cs`
- `ModernWpf.Controls\ContentDialog\ContentDialogOpenedEventArgs.cs`
- `test\ModernWpf.WinUI.Tests\ContentDialog\ContentDialogApiTests.cs`

## Ported Source Shape

- The public XamlOM surface is present: `Title`, `TitleTemplate`, `FullSizeDesired`, `CornerRadius`, button text/command/parameter/style properties, button enablement, `DefaultButton`, `ShowAsync`, `ShowAsync(ContentDialogPlacement)`, `Hide`, opened/closing/closed events, button-click events, result/placement/default-button enums, cancelable button-click args, cancelable closing args, and deferrals. The WPF control metadata also carries the source template-part contract for the represented parts.
- The template uses the source visual-state groups for dialog showing, dialog sizing, button visibility, default button styling, and border state. These are represented by `VisualStateEx.Setters` where WinUI uses native `VisualState.Setters`.
- Show/close behavior follows source result flow: button clicks execute commands only when click cancellation/deferrals allow the default action, `Closing` can cancel or defer, `Closed` reports the final `ContentDialogResult`, and same-host in-place dialogs are blocked while separate host branches may show independently.
- Default-button state now follows source command-area focus semantics: when focus is outside the command area, the `DefaultButton` property selects the accent/default visual state; when focus is inside the command area, the accent state is shown only if the focused command button is the default.
- Keyboard behavior now follows `ContentDialog_Partial.cpp`: Enter invokes only the explicit `DefaultButton` when it is enabled, rather than falling back to the first visible button; Escape and back requests route through source-shaped `ExecuteCloseAction`, so a visible/enabled close button is programmatically clicked and `CloseButtonClick` cancellation/deferral is honored before falling back to `Hide(None)`.
- Source drop-shadow mode applies `ApplyElevationEffect` to the background element with `baseElevation=128`. ModernWpf maps that to `ThemeShadowChrome.Depth=128`, preserving the existing WPF template host while using the shared WinUI recipe renderer.
- `ContentDialogApiTests` covers the source defaults, setters, template resources, visual-state setters, source dialog shadow depth, default-button state styling and command-area focus suppression, opened/closing/closed result flow, button command/cancel/deferral paths, source Enter behavior, source Escape close-action behavior, and in-place sibling rules.

## WPF Substitutions

- WinUI transplants the dialog into XamlRoot popup infrastructure, tracks per-XamlRoot `ContentDialogMetadata`, supports unconstrained/windowed popup placement, positions around SIP/input-pane bounds, and reacts to popup child unload events. ModernWpf uses WPF adorner/popup hosting and window-scoped ownership instead.
- WinUI compositor shadows, DComp validation, popup root automation properties, native access-key/gamepad/focus paths, and visual-tree master verification are not directly portable. ModernWpf represents the source shadow depth through `ThemeShadowChrome` and keeps WPF keyboard/focus routing plus focused tests for behavior that can be represented in-process.
- WinUI `XamlUICommand` label/keyboard-accelerator/description binding from `SetButtonPropertiesFromCommand` has no current ModernWpf command type equivalent, so command parity is limited to WPF `ICommand` execution and command parameters.
- WinUI `VisualState.Setters` are represented by `VisualStateEx.Setters`, and platform-only popup/windowing behavior is documented as WPF substitution rather than preserved guessed behavior.
