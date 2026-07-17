# InfoBar WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

Current-source recheck: official `microsoft-ui-xaml` `winui3/main` on 2026-07-17, including the relocated `controls\dev\InfoBar\InfoBarPanel.cpp` and `dxaml\xcp\core\text\TextBlock\TextBlock.cpp`, plus the installed WinUI 3 Gallery `InfoBar` page.

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
- `src\dxaml\xcp\core\text\TextBlock\TextBlock.cpp`

ModernWpf files:

- `ModernWpf.Controls\InfoBar\InfoBar.cs`
- `ModernWpf.Controls\InfoBar\InfoBar.xaml`
- `ModernWpf.Controls\InfoBar\InfoBarAutomationPeer.cs`
- `ModernWpf.Controls\InfoBar\InfoBarPanel.cs`
- `ModernWpf.Controls\InfoBar\InfoBarTemplateSettings.cs`
- `ModernWpf\Resources\Strings.resx`
- `ModernWpf\Resources\Strings.Designer.cs`
- `test\ModernWpf.WinUI.Tests\InfoBar\InfoBarApiTests.cs`
- `ModernWpf.Gallery\Pages\StatusInfoSampleFactory.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Ported Source Behavior

- `UpdateContentPosition` now uses the source `BannerContent` / `NoBannerContent` visual-state path instead of manually setting `Grid.Row` from code.
- The template now has the source `NoBannerContent` setter targeting `ContentArea.(Grid.Row)`.
- `OnApplyTemplate` now follows source ordering for close-button hookup, standard icon discovery, pending open-visibility notification, and state refresh.
- Close-button automation name and tooltip now come from source resource strings instead of hard-coded WPF strings.
- Standard severity icon automation names now update from the source severity-icon string table.
- `UpdateSeverity` now uses the source switch shape, including informational fallback for invalid enum values.
- Visibility state is now tracked through source-shaped `_applyTemplateCalled`, `_notifyOpen`, and `_isVisible` fields.
- The inner layout root uses a standard WPF `Border` around the source-shaped `Grid` so source `CornerRadius`, `Padding`, background, and minimum-height behavior remain visible in rendered and offscreen captures.
- The close button now uses source-shaped `Viewbox` + `SymbolIcon Cancel` content and the source AppBar button resource aliases.
- The close button style now inlines the source default-button chrome shape as a WPF-scoped substitute, including `ContentBorder`, focus visual settings, button pointer/pressed/disabled resource aliases, 38px source sizing, and top-right source margin.
- The action-button slot now carries the source `HyperlinkButton` margin and foreground override.
- Source InfoBar localized strings were added to the shared ModernWpf resource table.
- The default style enables device-pixel snapping, matching the source framework's rounded layout boundary.
- `InfoBarPanel` now consumes physical-pixel-ceiled child sizes like current WinUI `TextBlock::MeasureOverride`. When arranging template `TextBlock` children it also applies WinUI's `m_layoutRoundingHeightAdjustment` render offset, preventing the last text line from sitting one pixel too high or clipping.
- The generated Gallery sample top-aligns each InfoBar inside its example/options row, matching WinUI Gallery `ControlExample.Example` desired-height hosting instead of stretching to the adjacent options column.

## WPF Substitutions

- WPF has no WinUI `AutomationProperties.AccessibilityView` equivalent in the target surface, so `InfoBarAutomationPeer.IsControlElementCore` remains the control-view substitute for open vs. closed InfoBars.
- WPF/net462 does not expose WinUI notification automation APIs, `LocalizedLandmarkType`, or `IsDialog`; the WPF peer invalidates on open/close instead of raising WinUI notification events.
- WPF `Grid` does not have `CornerRadius` or `Padding`, so a standard `Border` represents the source inner root chrome while retaining the source child `Grid` layout.
- WPF `Viewbox.Child` template binding to `IconElement` is represented with `ContentPresenterEx` for the user icon slot.
- WPF `TextWrapping` does not have WinUI `WrapWholeWords`, so the template keeps WPF `Wrap`.
- The source `InfoBarCloseButtonStyle BasedOn="{StaticResource DefaultButtonStyle}"` is not directly reliable from this resource dictionary scope. ModernWpf uses a self-contained WPF style that mirrors the relevant `DefaultButtonStyle` template/trigger shape and uses dynamic button resources where theme resources are resolved later.
- The source action-slot `DefaultHyperlinkButtonStyle` base style is not referenced directly from the InfoBar resource dictionary because it does not resolve reliably from this scope. ModernWpf represents the slot with a local WPF `HyperlinkButton` style that applies the InfoBar-specific margin and foreground.

## Verification

Focused tests cover source defaults, close/cancel events, source close-button automation text and tooltip, source close-button `SymbolIcon`, close-button WPF button chrome, icon/close visual states, `NoBannerContent` and `BannerContent` state routing, severity icon text and automation names, foreground setter binding, source root chrome/padding, action presenter margins, HyperlinkButton action margin, automation peer control-view visibility, `InfoBarPanel` layout, device-pixel snapping, child desired-size ceilings, and the WinUI TextBlock render offset.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --configuration Debug --framework net8.0-windows7.0 --filter "FullyQualifiedName~InfoBarApiTests" --no-restore`
  - Passed 11/11.
- `tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls InfoBar -Reference InstalledWinUI3Gallery -Theme Light -FailOnDifference`
  - Passed at `artifacts\visual-checks\20260717-004129-648-15600\report.md`: exact `560x95` primary crops and mean delta `1.33`.
- `tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls InfoBar -Reference InstalledWinUI3Gallery -Theme Dark -FailOnDifference`
  - Passed at `artifacts\visual-checks\20260717-004152-161-80752\report.md`: exact `560x95` primary crops and mean delta `1.46`.
- The InfoBar primary-crop threshold is `2.0`; Gallery source-shape coverage pins both automation IDs and the gate.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with existing repository warnings.
- `rg -n 'ContentAreaName|ContentRootName|_contentArea|_contentRoot|Severity\.ToString|Grid\.SetRow' .\ModernWpf.Controls\InfoBar .\test\ModernWpf.WinUI.Tests\InfoBar`
  - No stale manual content-position or old template-field symbols remain.
