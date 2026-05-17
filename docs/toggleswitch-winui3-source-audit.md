# ToggleSwitch WinUI 3 Source Audit

Date: 2026-05-17

This audit treats the local WinUI 3 checkout at `D:\repos\microsoft-ui-xaml`
as the behavioral source of truth for the existing ModernWpf `ToggleSwitch`.
It is an evidence map, not a completed source replacement. ToggleSwitch still
counts as guessed implementation debt until the control is replaced/adapted as
a whole-control WinUI 3 port; this note only identifies the source-aligned
pieces, WPF substitutions, and platform gaps known so far.

## WinUI 3 Source Inputs

- `src\dxaml\xcp\dxaml\lib\ToggleSwitch_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ToggleSwitch_Partial.h`
- `src\dxaml\xcp\dxaml\lib\ToggleSwitchAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\components\controls\KeyDownUp\inc\ToggleSwitchKeyProcess.h`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.Primitives.cs`
- `src\controls\dev\CommonStyles\ToggleSwitch_themeresources.xaml`
- `src\dxaml\test\native\external\controls\toggleswitch\ToggleSwitchIntegrationTests.cpp`
- `src\dxaml\test\native\external\controls\toggleswitch\ToggleSwitchAutomationIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf.Controls\ToggleSwitch\ToggleSwitch.cs`
- `ModernWpf.Controls\ToggleSwitch\ToggleSwitch.xaml`
- `ModernWpf.Controls\ToggleSwitch\ToggleSwitchAutomationPeer.cs`
- `ModernWpf.Controls\ToggleSwitch\ToggleSwitchTemplateSettings.cs`
- `ModernWpf.Controls\ToggleSwitch\Strings\Resources.resx`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\ToggleSwitch\ToggleSwitchApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf status |
| --- | --- |
| Constructor state fields initialize pointer, drag, translation, and pending-key state. | Matched with WPF fields in `ToggleSwitch.cs`. |
| `PrepareState` creates read-only `ToggleSwitchTemplateSettings`. | Matched by constructor initialization and read-only WPF dependency property. |
| XamlOM property surface: `IsOn`, `Header`, `HeaderTemplate`, `OnContent`, `OnContentTemplate`, `OffContent`, `OffContentTemplate`, `TemplateSettings`, `HeaderPlacement`, `Toggled`, and protected callback hooks. | Matched with WPF dependency properties, routed `Toggled`, and protected virtual callbacks. |
| `GetDefaultValue2` supplies localized On/Off defaults while preserving default-value detection. | Matched with resource-backed default values and tests for default-vs-custom automation naming. |
| `OnPropertyChanged2` updates visual state, header visibility, protected callbacks, and automation toggle-state notifications. | Matched through WPF property callbacks and automation peer notification path. |
| `ChangeVisualState` selects Common, Focus, Toggle, Content, and Header visual states from source state fields. | Matched with WPF `VisualStateManager.GoToState`; owner focus state is used rather than child focus. |
| Template part discovery, drag/tap hookup, part size updates, and header presenter visibility. | Matched with WPF template parts, `Thumb` drag events, a bubbling mouse-up tap bridge, part `SizeChanged`, and null/header-template visibility rules. |
| `OnPointerCaptureLost` clears `PointerOver` after vertical-pan drag completion when dragging has finished. | Matched through the WPF thumb `LostMouseCapture` path plus the owner fallback. |
| `GetTranslations`, `SetTranslations`, `ClearTranslations`, `MoveDelta`, `MoveCompleted`, and size-derived knob/curtain bounds. | Matched, including current-to-on/off and on/off-to-current template setting offsets. |
| `ToggleSwitchKeyProcess` handles source key-down/up sequencing using `OriginalKey`. | Matched for WPF keyboard input; WPF system/IME/dead-char keys now normalize back to the exposed original key before processing. |
| WinUI native tests for live-tree entry/leave, tap, horizontal drag, vertical pan no-toggle, keyboard space, directional-key no-toggle, footprint, visual tree, and automation shape. | Covered by focused WPF tests where platform input can be represented; touch pan and compositor-only behavior are documented substitutions. |
| Automation peer class name, localized control type, toggle pattern, clickable point, name construction, default On/Off filtering, and hidden template children. | Matched with WPF automation APIs and source-shaped string extraction. |
| CommonStyles template dimensions, style setters, state names, knob animations, On/Off content presenters, and WinUI resource keys. | Matched with WPF template equivalents, `VisualStateEx.Setters`, and theme resource aliases; the previous WPF-only `VerticalContentAlignment=Center` setter has been removed because WinUI CommonStyles does not set it. |

## WPF Substitutions

- WinUI `VirtualKey.GamepadA` has no WPF `Key` equivalent in the target framework, so this remains a documented platform gap.
- WinUI `ManipulationMode="System,TranslateX"` is represented by WPF `Thumb` drag handling and focused drag tests; exact touch pan routing is a platform gap.
- WinUI `RepositionThemeAnimation`, compositor behavior, and element sounds have no direct WPF equivalent.
- WinUI `Grid.CornerRadius` is represented by the WPF `Border` used for `SwitchAreaGrid`.
- WinUI `AutomationProperties.AccessibilityView="Raw"` and WinRT automation internals are represented by WPF automation peer child filtering and WPF provider APIs.
- The WinUI framework `dxaml` generic template remains in the source tree, but packaged WinUI 3 CommonStyles overrides it; ModernWpf targets the packaged CommonStyles template shape.

## Current Validation

Run after this audit:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~ToggleSwitchApiTests" --no-restore
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
```
