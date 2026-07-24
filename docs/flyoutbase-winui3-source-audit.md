# Flyout / FlyoutBase Current WinUI Source Audit

Date: 2026-07-18

This audit treats current official `microsoft-ui-xaml` and WinUI Gallery
`main` as the product and sample authorities for ModernWpf's `FlyoutBase`,
`FlyoutPresenter`, and retained Flyout Gallery example.

## Pinned authorities

Official `microsoft-ui-xaml` `main` is
`de3e767333c2f0717a6a70cb22bd192ced5ad885` (2026-07-17). The upstream
repository moved its mirrored sources to a root layout in
`8463f45162149de0ec3ad7df752596893fe3e13e`; current paths and blob identities
are:

- `dxaml/xcp/dxaml/lib/FlyoutBase_partial.cpp` —
  `6c5a80dd3fc09043b4d2a801d7b8c991a7d4a320`
- `dxaml/xcp/dxaml/lib/FlyoutPresenter_partial.cpp` —
  `20997cd1d1771eab20b9760496e18766ae1e38ab`
- `dxaml/xcp/dxaml/lib/Flyout_partial.cpp` —
  `23a52887e7f284970574ba80746aeca3b0857cfd`
- `controls/dev/CommonStyles/FlyoutPresenter_themeresources.xaml` —
  `621bf7d16825ae37cdd0b0ad05b7b5a49ddcd4c4`

The pre-move product baseline is
`c70471c511a0168b61dcca13af9556465f26b673`. Its presenter implementation and
theme-resource blobs are byte-identical to current official `main`; bounded
post-baseline history consists of the root move and current FlyoutBase fix
`2db27f71f857363d6a9a4485e01c8b8fdbe02499`, which fixes side-placement
bottom alignment near a monitor edge.

Official WinUI Gallery `main` is
`29f62479d5c046a0b854a5868e5a7cd484572d87` (2026-07-13). Current sample
inputs are:

- `WinUIGallery/Samples/Flyout/FlyoutPage.xaml` —
  `901a26130976d541f5e1a9144cf737d8a26e5289`
- `WinUIGallery/Samples/Flyout/FlyoutPage.xaml.cs` —
  `2e87143f4ff6ca263ea20517e2e944dd2a1ca4cc`
- `WinUIGallery/Samples/Flyout/ButtonFlyout.txt` —
  `e0beec869700cd5ee8d11cb73aae95e559bebf63`

The current Gallery path conversion is commit
`14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`. Relevant earlier history also
includes `0380525bbe7d9e92b80cd4a4abbb50fa04b207cc` for ControlExample stretching
and `2fbc60a8ba81a08e4af7c3044aefd55f833376c2` for sample-folder cleanup. No
Flyout sample change follows the conversion through the current Gallery pin.

## Product mapping

| Current WinUI source behavior | ModernWpf mapping |
| --- | --- |
| `Target` is set while open and cleared after a non-canceled close. Opening one top-level flyout stages the latest request behind the current flyout. | Matched in `ModernWpf.Controls/Flyout/FlyoutBase.cs`, including canceled-close restoration and placement-target unload handling. |
| `FlyoutShowOptions` applies position, exclusion rectangle, show mode, and placement before the already-open no-op decision. `Auto` normalizes to `Standard`; transient modes do not take focus. | Matched with WPF dependency-property and popup substitutions. Null-target positioned opens use the current WPF root. |
| Placement tries the requested major side, its opposite, then the other axis. The current source bottom-aligns an overflowing Left/Right flyout to the exclusion anchor before clamping it to the monitor work area. | Matched by `CustomPopupPlacementHelper` fallback order and `FlyoutBase.ClampSidePlacementVerticalOffset`. The latter is applied to top-aligned Left/Right popup HWND placement and is pinned by a focused product test. |
| `FlyoutPresenter::OnApplyTemplate` applies elevation to its first child when `IsDefaultShadowEnabled` is true. | Matched by `ThemeShadowChrome`, depth 32, medium windowed-popup insets, and the `IsDefaultShadowEnabled` template binding. |
| The current presenter template uses `FlyoutContentPadding=16,15,16,17`, a one-pixel normal border, `FlyoutThemeMinWidth`, `OverlayCornerRadius`, `BackgroundSizing=InnerBorderEdge`, a ScrollViewer, and a padded ContentPresenter. | Matched value-for-value in `ModernWpf.Controls/Flyout/FlyoutPresenter.xaml`. WPF's separate transparent popup HWND is the documented platform substitute. |

## Gallery example and accessibility mapping

The retained page matches the current official example:

- header `A button with a flyout`;
- trigger `Control1`, content `Empty cart`;
- message `All items will be removed. Do you want to continue?` with bottom
  margin 12 and `BaseTextBlockStyle`;
- confirmation button `Yes, empty my cart`;
- the click handler hides `Control1.Flyout`;
- the unused page resource `SharedFlyout` remains present.

The WPF sample exposes Button automation roles and Invoke providers for both
buttons, and a Text role/name for the confirmation message. The Gallery test
opens the flyout and invokes its source close path.

## Strict live evidence

The visual harness now requires the official `Control1` primary crop and a
nonblank popup-window proof. A common `FlyoutOpenSurface` crop walks from the
unique message to the presenter root and captures the complete surface from
screen coordinates, including message, confirmation button, border, corner,
padding, and background. This avoids the previous false confidence from a
text-only 356x43 crop and works across WPF's separate popup HWND.

- Light: `artifacts/visual-checks/20260718-233022-683-69796/report.md`
  - resting button delta `2.63`, exact `90x32`;
  - open surface delta `10.07`, `366x96` versus `366x97`.
- Dark: `artifacts/visual-checks/20260718-233125-103-42428/report.md`
  - resting button delta `2.21`, exact `90x32`;
  - open surface delta `3.72`, `366x96` versus `366x97`.

Required thresholds are 3.0 with exact primary size and 11.0 with at most the
observed one-pixel open-surface height difference.

Fresh Light OpenRepeat recording
`artifacts/gallery-recordings/20260718-233216-705/report.md` passes in `8.1s`
with `0.659` maximum frame delta and `17.753` maximum local delta. Fresh Dark
recording `artifacts/gallery-recordings/20260718-233313-024/report.md` passes in
`8.2s` with `0.529` / `23.521`. Both prove open, source confirmation-button
close, reopen, and generate dense-transition review sheets.

Focused product tests pass 24/24. The focused Gallery sample, source-audit,
surface/harness, popup-placement, and recorder slice passes 6/6 on both net8
and net10. The Gallery and dependent control projects build successfully for
net462, net8, and net10 (the build retains the repository's existing warnings
and recurring WinRT resolver message).

## Validation commands

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --configuration Debug --no-restore --filter "FullyQualifiedName~FlyoutBaseApiTests|FullyQualifiedName~FlyoutPresenterApiTests"
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Debug --no-restore --filter "FullyQualifiedName~FlyoutSampleMatchesWinUIGalleryExample|FullyQualifiedName~FlyoutSourceAuditTests|FullyQualifiedName~GalleryVisualChecksEnforceFlyoutCurrentSourceSurfaceParity"
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls Flyout -Theme Light -Reference InstalledWinUI3Gallery -IncludeInteractions -FailOnDifference -TimeoutSeconds 30
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls Flyout -Theme Dark -Reference InstalledWinUI3Gallery -IncludeInteractions -FailOnDifference -TimeoutSeconds 30
```
