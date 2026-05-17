# Expander WinUI 3 Source Audit

ModernWpf `Expander` is tracked as a source-backed WPF platform mapping. The repo does not add a new `ModernWpf.Controls.Expander` in this phase; it styles and augments WPF `System.Windows.Controls.Expander` where WinUI 3 behavior can be represented safely.

## Source Files

Primary WinUI 3 source references:

- `src\controls\dev\Expander\Expander.idl`
- `src\controls\dev\Expander\Expander.h`
- `src\controls\dev\Expander\Expander.cpp`
- `src\controls\dev\Expander\Expander.xaml`
- `src\controls\dev\Expander\ExpanderAutomationPeer.cpp`
- `src\controls\dev\Expander\ExpanderTemplateSettings.cpp`
- `src\controls\dev\Generated\Expander.properties.cpp`
- `src\controls\dev\Generated\ExpanderTemplateSettings.properties.cpp`
- `src\controls\dev\Expander\APITests\ExpanderTests.cs`
- `src\controls\dev\Expander\InteractionTests\ExpanderTests.cs`

ModernWpf files:

- `ModernWpf\Styles\Expander.xaml`
- `ModernWpf\Controls\Primitives\ToggleButtonHelper.cs`
- `ModernWpf\Controls\Primitives\CornerRadiusFilterConverter.cs`
- `test\ModernWpf.WinUI.Tests\Expander\ExpanderApiTests.cs`
- `test\ModernWpf.WinUI.Tests\Expander\ExpanderInteractionTests.cs`

## Ported Source Shape

- WinUI's default `Expander` is not tab-stoppable; focus goes to the template toggle button. ModernWpf now makes the WPF owner `Focusable=false` in the default style while keeping the `HeaderSite` toggle focusable.
- The WPF template maps source header/content resource keys, minimum width/height, content padding, down/up border thickness, header alignment, and source visual-state setter behavior through `VisualStateEx`.
- The header toggle uses source-shaped pointer/pressed/disabled and checked-state setters, with `AnimatedIcon.State` values represented by `FontIconFallback`.
- The WPF template preserves WPF `ExpandDirection.Left` and `ExpandDirection.Right` because ModernWpf is styling the platform WPF control. WinUI 3 source currently drives only the `Down` and `Up` visual states; left/right remain documented WPF platform behavior rather than guessed WinUI parity.
- Tests cover default resources, header/content template parts, source-style owner/header focus routing, visual-state setters, up/down/left/right WPF expand direction mapping, and WPF automation visibility for collapsed content.

## WPF Substitutions

- WinUI owns a custom `Microsoft.UI.Xaml.Controls.Expander`, generated dependency properties, `Expanding` / `Collapsed` events, `TemplateSettings.ContentHeight`, composition clip animation, and a custom automation peer. ModernWpf does not add that new control surface under the current no-new-controls rule.
- WPF `Expander` already owns `IsExpanded`, `ExpandDirection`, content/header hosting, and expand/collapse automation. ModernWpf maps source behavior through the WPF style, resource keys, `VisualStateEx`, and existing WPF automation peer.
- WinUI's `ExpanderHeader` automation peer event-source redirection and touch `GetPeerFromPointCore` behavior have no direct WPF style-only equivalent. WPF automation remains the platform substitute and is covered by visibility/name/class tests.
- WinUI content height template settings and compositor animations are represented by immediate WPF layout/visibility changes instead of adding a custom control implementation.
