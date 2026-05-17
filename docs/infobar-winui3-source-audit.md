# InfoBar WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\controls\dev\InfoBar\InfoBar.cpp`
- `src\controls\dev\InfoBar\InfoBar.h`
- `src\controls\dev\InfoBar\InfoBar.xaml`
- `src\controls\dev\InfoBar\InfoBar.idl`
- `src\controls\dev\InfoBar\InfoBarAutomationPeer.cpp`
- `src\controls\dev\InfoBar\InfoBarPanel.cpp`
- `src\controls\dev\InfoBar\InfoBar_themeresources.xaml`
- `src\controls\dev\Generated\InfoBar.properties.cpp`
- `src\controls\dev\Generated\InfoBarTemplateSettings.properties.cpp`
- `src\controls\dev\InfoBar\InteractionTests\InfoBarTests.cs`

ModernWpf files:

- `ModernWpf.Controls\InfoBar\InfoBar.cs`
- `ModernWpf.Controls\InfoBar\InfoBar.xaml`
- `ModernWpf.Controls\InfoBar\InfoBarAutomationPeer.cs`
- `ModernWpf.Controls\InfoBar\InfoBarPanel.cs`
- `ModernWpf.Controls\InfoBar\InfoBarTemplateSettings.cs`
- `ModernWpf\Resources\Strings.resx`
- `ModernWpf\Resources\Strings.Designer.cs`
- `test\ModernWpf.WinUI.Tests\InfoBar\InfoBarApiTests.cs`

## Ported Source Behavior

- `UpdateContentPosition` now uses the source `BannerContent` / `NoBannerContent` visual-state path instead of manually setting `Grid.Row` from code.
- The template now has the source `NoBannerContent` setter targeting `ContentArea.(Grid.Row)`.
- `OnApplyTemplate` now follows source ordering for close-button hookup, standard icon discovery, pending open-visibility notification, and state refresh.
- Close-button automation name and tooltip now come from source resource strings instead of hard-coded WPF strings.
- Standard severity icon automation names now update from the source severity-icon string table.
- `UpdateSeverity` now uses the source switch shape, including informational fallback for invalid enum values.
- Visibility state is now tracked through source-shaped `_applyTemplateCalled`, `_notifyOpen`, and `_isVisible` fields.
- The inner layout root now uses `GridEx` to carry the source `Grid` `CornerRadius` and `Padding` behavior that WPF `Grid` lacks.
- The close button now uses source-shaped `Viewbox` + `SymbolIcon Cancel` content and the source AppBar button resource aliases.
- The action-button slot now carries the source `HyperlinkButton` margin and foreground override.
- Source InfoBar localized strings were added to the shared ModernWpf resource table.

## WPF Substitutions

- WPF has no WinUI `AutomationProperties.AccessibilityView` equivalent in the target surface, so `InfoBarAutomationPeer.IsControlElementCore` remains the control-view substitute for open vs. closed InfoBars.
- WPF/net462 does not expose WinUI notification automation APIs, `LocalizedLandmarkType`, or `IsDialog`; the WPF peer invalidates on open/close instead of raising WinUI notification events.
- WPF `Grid` does not have `CornerRadius` or `Padding`, so `GridEx` represents the source inner root.
- WPF `Viewbox.Child` template binding to `IconElement` is represented with `ContentPresenterEx` for the user icon slot.
- WPF `TextWrapping` does not have WinUI `WrapWholeWords`, so the template keeps WPF `Wrap`.
- The source `InfoBarCloseButtonStyle BasedOn="{StaticResource DefaultButtonStyle}"` is not directly reliable from this resource dictionary scope; ModernWpf keeps an explicit WPF close-button style while preserving source size, margin, content, and AppBar resource aliases.
- The source action-slot `DefaultHyperlinkButtonStyle` base style is represented by a local WPF `HyperlinkButton` style that only applies the InfoBar-specific margin and foreground.

## Verification

Focused tests cover source defaults, close/cancel events, source close-button automation text and tooltip, source close-button `SymbolIcon`, icon/close visual states, `NoBannerContent` and `BannerContent` state routing, severity icon text and automation names, foreground setter binding, source `GridEx` root chrome/padding, action presenter margins, HyperlinkButton action margin, automation peer control-view visibility, and `InfoBarPanel` layout.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~InfoBar" --no-restore`
  - Passed 8/8.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with existing repository warnings.
- `rg -n 'ContentAreaName|ContentRootName|_contentArea|_contentRoot|Severity\.ToString|Grid\.SetRow' .\ModernWpf.Controls\InfoBar .\test\ModernWpf.WinUI.Tests\InfoBar`
  - No stale manual content-position or old template-field symbols remain.
