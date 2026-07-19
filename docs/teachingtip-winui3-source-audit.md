# TeachingTip WinUI 3 Source Audit

Current official source: `microsoft/microsoft-ui-xaml` `winui3/main` at
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17).

The local full-source mirror remains at
`c70471c511a0168b61dcca13af9556465f26b673` (2026-05-11). This audit queried
the official repository for the current commit, file blobs, and path history
instead of treating that older checkout as current.

Current WinUI 3 source inspected:

- `controls/dev/TeachingTip/TeachingTip.idl`
- `controls/dev/TeachingTip/TeachingTip.h`
- `controls/dev/TeachingTip/TeachingTip.cpp`
- `controls/dev/TeachingTip/TeachingTip.xaml`
- `controls/dev/TeachingTip/TeachingTip_themeresources.xaml`
- `controls/dev/TeachingTip/TeachingTip_themeresources_perf2026.xaml`
- `controls/dev/TeachingTip/TeachingTipTemplateSettings.cpp`
- `controls/dev/TeachingTip/TeachingTipOpenedEventArgs.cpp`
- `controls/dev/TeachingTip/TeachingTipClosingEventArgs.cpp`
- `controls/dev/TeachingTip/TeachingTipClosedEventArgs.cpp`
- `controls/dev/TeachingTip/TeachingTipAutomationPeer.cpp`
- `controls/dev/TeachingTip/APITests/TeachingTipTests.cs`
- `controls/dev/TeachingTip/InteractionTests/TeachingTipTests.cs`

ModernWpf implementation and proof:

- `ModernWpf.Controls/TeachingTip/TeachingTip.cs`
- `ModernWpf.Controls/TeachingTip/TeachingTip.xaml`
- `ModernWpf.Controls/TeachingTip/TeachingTipTemplateSettings.cs`
- `ModernWpf.Controls/TeachingTip/TeachingTipOpenedEventArgs.cs`
- `ModernWpf.Controls/TeachingTip/TeachingTipClosingEventArgs.cs`
- `ModernWpf.Controls/TeachingTip/TeachingTipClosedEventArgs.cs`
- `ModernWpf.Controls/TeachingTip/TeachingTipAutomationPeer.cs`
- `test/ModernWpf.WinUI.Tests/TeachingTip/TeachingTipApiTests.cs`
- `test/ModernWpf.WinUI.Tests/TeachingTip/TeachingTipSourceAuditTests.cs`
- `tools/visual-checks/Run-GalleryVisualChecks.ps1`

## Current-source history

- `8463f45162149de0ec3ad7df752596893fe3e13e` (2026-05-30) moved the source
  mirror from the old `src/` layout to the current repository-root layout. It
  did not change TeachingTip behavior or pixels.
- `c7e2f98d978c81c2b7b0054eb042a6f8f816ec9c` (2026-06-07) compiled
  `TeachingTipTestHooks` and its notification calls only in Debug builds. The
  change removes production DLL test-hook code; it does not change the control
  template, resources, public behavior, placement, or automation peer.
- The current `TeachingTip.xaml`, IDL, automation peer, template-settings file,
  and standard theme-resource blobs are byte-identical to the May local
  mirror. `TeachingTip.cpp` differs only by the Debug guards above.
- The current source also carries
  `TeachingTip_themeresources_perf2026.xaml`. Its TeachingTip geometry contract
  remains 40/520 minimum/maximum height, 320/336 minimum/maximum width, 12px
  content/button spacing, a 40px alternate close button with 16px symbol, and
  the 10px tail.

## Visual and template mapping

ModernWpf retains the source template structure and values:

- `Popup`, `Container`, `TailOcclusionGrid`, `ContentRootGrid`,
  `HeroContentBorder`, `MainContentPresenter`, title/subtitle/icon presenters,
  action/close slots, alternate close button, and `TailPolygon` parts.
- Source size resources: minimum height `40`, maximum height `520`, minimum
  width `320`, maximum width `336`.
- Source spacing: 12px content and body margins, 12px button-panel top margin,
  4px inter-button split, 28px title reservation for the header close button,
  and 12px icon-to-title spacing.
- Source alternate close button: `40x40`, 16px symbol, 4px padding, and `-3`
  focus-visual margin.
- Source tail: 20px long side, 10px short side/margin, placement-specific
  points and one-pixel overlap margins.
- Source typography: inherited control font family, semibold title, and normal
  subtitle/body text. Light/Dark/High Contrast aliases remain pinned by the
  existing resource tests.
- WinUI establishes a depth-32 `ThemeShadow` and animates translation Z during
  expand/contract. ModernWpf uses `ContentRootGridShadowChrome` at depth `32`
  and animates WPF shadow depth with the source-shaped scale storyboards.

The Gallery intentionally overrides `TeachingTipMinWidth` to `48`, matching
the official Gallery example. With the same title, subtitle, Refresh icon, and
header close button, both rendered content roots measure exactly `224x64`.

## Behavior and accessibility mapping

- `Opened`, `Closing`, and `Closed` timing, close cancellation/deferral,
  reopening, light dismiss, target unload, button invocation, placement
  fallback, hero placement, and scale/shadow animation are covered by the
  existing API tests.
- `TeachingTipAutomationPeer` exposes the source class name, switches between
  Pane and Window by `IsLightDismissEnabled`, implements the Window pattern,
  reports modal/topmost/idle state from the owner, and closes through
  `IsOpen=false`.
- Current native `SetPopupAutomationProperties()` forwards the owner's
  `AutomationProperties.Name` and `AutomationId` to the popup, falling back to
  `Title` for the name, and refreshes those values when title/name/ID changes.
  ModernWpf now performs the same forwarding during template application and
  from `OnPropertyChanged`. `TeachingTipForwardsAutomationNameAndIdToPopup`
  covers initial values, live explicit-name/ID changes, and the title fallback.
- The native localized landmark, notification, and window-open/window-closed
  events have no fully equivalent downlevel WPF surface. ModernWpf keeps the
  peer/event methods as documented no-ops where WPF does not expose those
  WinRT-specific event APIs.

## Live visual proof and strict gates

The older TeachingTip interaction proof used a difference crop (`248x82`
Light and `248x86` Dark). It included unrelated page pixels and ModernWpf's
transparent WPF popup does not expose a separately bounded UIA root, so that
crop could not support cross-app pixel comparison.

The refreshed harness:

- requires the official `TestButton1` closed control crop and exact static
  crop size under a `4.0` delta gate;
- finds the WinUI `ContentRootGrid` through UIA and derives the corresponding
  WPF bounds from stable surface-edge transitions around the invoked target;
- writes the same `TeachingTipSurface` source for both applications;
- compares the actual `224x64` content root, excluding unrelated Gallery page
  pixels behind the source 10px tail and the platform-specific shadow;
- requires the open surface under a `10.0` mean-delta gate with exact size.

Final strict installed-Gallery evidence:

- Light: `artifacts/visual-checks/20260718-031359-694-76316/report.md` — closed
  `3.43` at exact `135x32`; open surface `9.30` at exact `224x64`.
- Dark: `artifacts/visual-checks/20260718-031438-291-94484/report.md` — closed
  `2.94` at exact `135x32`; open surface `8.28` at exact `224x64`.

The installed Gallery Light route retains a dark surrounding shell while the
tip itself is Light. The surface crop removes those surrounding pixels; the
remaining bounded difference is concentrated in WPF/WinUI glyph
antialiasing and the installed Gallery's icon/border resource resolution. The
current upstream resource aliases remain pinned rather than changing product
resources to reproduce that host-specific mixed-theme result.

## Verification

Focused product/source validation:

```text
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter "FullyQualifiedName~TeachingTip"
```

Result: 25 passed on `net8.0-windows7.0`.

The focused Gallery gate passes 1/1 on both net8 and net10. The strict
Light/Dark commands above pass with required primary/open crop sources and
exact size gates. `ModernWpf.Controls` and `ModernWpf.Gallery` both build for
net462, net8, and net10.
