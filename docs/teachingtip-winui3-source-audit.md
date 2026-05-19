# TeachingTip WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI 3 source inspected:

- `src\controls\dev\TeachingTip\TeachingTip.idl`
- `src\controls\dev\TeachingTip\TeachingTip.h`
- `src\controls\dev\TeachingTip\TeachingTip.cpp`
- `src\controls\dev\TeachingTip\TeachingTip.xaml`
- `src\controls\dev\TeachingTip\TeachingTip_themeresources.xaml`
- `src\controls\dev\TeachingTip\TeachingTipTemplateSettings.cpp`
- `src\controls\dev\TeachingTip\TeachingTipOpenedEventArgs.cpp`
- `src\controls\dev\TeachingTip\TeachingTipClosingEventArgs.cpp`
- `src\controls\dev\TeachingTip\TeachingTipClosedEventArgs.cpp`
- `src\controls\dev\TeachingTip\TeachingTipAutomationPeer.cpp`
- `src\controls\dev\TeachingTip\APITests\TeachingTipTests.cs`
- `src\controls\dev\TeachingTip\InteractionTests\TeachingTipTests.cs`

ModernWpf files:

- `ModernWpf.Controls\TeachingTip\TeachingTip.cs`
- `ModernWpf.Controls\TeachingTip\TeachingTip.xaml`
- `ModernWpf.Controls\TeachingTip\TeachingTipTemplateSettings.cs`
- `ModernWpf.Controls\TeachingTip\TeachingTipOpenedEventArgs.cs`
- `ModernWpf.Controls\TeachingTip\TeachingTipClosingEventArgs.cs`
- `ModernWpf.Controls\TeachingTip\TeachingTipClosedEventArgs.cs`
- `ModernWpf.Controls\TeachingTip\TeachingTipAutomationPeer.cs`
- `test\ModernWpf.WinUI.Tests\TeachingTip\TeachingTipApiTests.cs`

## Ported Shape

The old ModernWpf row treated `TeachingTip` as a functional subset. The control is now mapped as a source-backed WPF port against the local WinUI 3 implementation:

- Added source-shaped `TeachingTipOpenedEventArgs` and the `Opened` event from WinUI's preview IDL surface.
- Open timing now follows WinUI source behavior: `Opened` is raised after the expand animation completes, or immediately when animations are disabled. Reopening raises a new event; canceled/deferred close restore paths do not synthesize a duplicate open.
- Added `TeachingTipAutomationPeer` with WinUI's class name, `Pane` versus `Window` control-type switch based on `IsLightDismissEnabled`, and `IWindowProvider` shape.
- Automation window provider state follows the source `m_isIdle` model through WPF open/close animation state: closed idle tips report `BlockedByModalWindow`, open idle tips report `ReadyForUserInteraction`, and active open/close animations report running/closing states.
- Automation `Close()` routes through the owner `IsOpen` property like WinUI source.
- Existing WPF template parts remain source-shaped: `Popup`, `Container`, `TailOcclusionGrid`, `ContentRootGrid`, `HeroContentBorder`, `MainContentPresenter`, title/subtitle/icon presenters, source button slots, and `TailPolygon`.
- WinUI source applies a `ThemeShadow` to `ContentRootGrid` when `m_tipShouldHaveShadow=true`, with default `m_contentElevation=32`, and animates content translation Z from `0.01` to `32` during expand/contract. ModernWpf now wraps `ContentRootGrid` in `ContentRootGridShadowChrome`, uses the shared WPF `ThemeShadowChrome` renderer at depth `32` with `WindowedPopupInsetMode=Medium`, and animates the WPF shadow depth alongside the existing scale animation.
- Existing tests already cover source-shaped visual-state setters, title/subtitle/content/hero/icon states, open popup placement, close cancellation/deferral, light dismiss, target-unload close, scale animation, and final WinUI 2 resource mappings. This slice adds tests for `Opened` and automation peer shape.

## WPF Substitutions

- WinUI's popup is XamlRoot-aware and integrates with WinRT automation notification/window events. ModernWpf keeps WPF `Popup`; WPF `AutomationPeer` exposes `IWindowProvider` but does not expose WinUI's notification event or window opened/closed peer events, so those calls are documented no-ops.
- WinUI placement solves against XamlRoot, screen bounds, flow direction, and composition metrics. ModernWpf keeps the existing WPF custom-popup placement substitute and bounds-aware fallback tests.
- WinUI light-dismiss focus management, F6 handling, gamepad/XYFocus, Axe scans, and full TestUI process input automation remain platform gaps.
- WinUI compositor scale/elevation animations are represented by WPF scale transforms, `ThemeShadowChrome.Depth` animation, and storyboards.

## Verification

Focused validation:

```text
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TeachingTip
```

Result: 21 passed.

Controls build validation:

```text
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
```

Result: passed with existing warnings.
