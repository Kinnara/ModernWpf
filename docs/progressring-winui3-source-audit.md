# ProgressRing WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\controls\dev\ProgressRing\ProgressRing.cpp`
- `src\controls\dev\ProgressRing\ProgressRing.h`
- `src\controls\dev\ProgressRing\ProgressRing.xaml`
- `src\controls\dev\ProgressRing\ProgressRing.idl`
- `src\controls\dev\ProgressRing\ProgressRingAutomationPeer.cpp`
- `src\controls\dev\ProgressRing\ProgressRingAutomationPeer.h`
- `src\controls\dev\ProgressRing\AnimatedVisuals\ProgressRingDeterminate.cpp`
- `src\controls\dev\ProgressRing\AnimatedVisuals\ProgressRingDeterminate.h`
- `src\controls\dev\ProgressRing\AnimatedVisuals\ProgressRingIndeterminate.cpp`
- `src\controls\dev\ProgressRing\AnimatedVisuals\ProgressRingIndeterminate.h`
- `src\controls\dev\ProgressRing\ProgressRing_themeresources.xaml`
- `src\controls\dev\Generated\ProgressRing.properties.cpp`
- `src\controls\dev\Generated\ProgressRingTemplateSettings.properties.cpp`
- `src\controls\dev\ProgressRing\APITests\ProgressRingTests.cs`
- `src\controls\dev\ProgressRing\InteractionTests\ProgressRingTests.cs`

ModernWpf files:

- `ModernWpf.Controls\ProgressRing\ProgressRing.cs`
- `ModernWpf.Controls\ProgressRing\ProgressRing.xaml`
- `ModernWpf.Controls\ProgressRing\ProgressRingAutomationPeer.cs`
- `ModernWpf.Controls\ProgressRing\ProgressRingIndicator.cs`
- `ModernWpf.Controls\ProgressRing\ProgressRingTemplateSettings.cs`
- `ModernWpf.Gallery\Testing\GalleryDiagnostics.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\ProgressRing\ProgressRingApiTests.cs`
- `test\ModernWpf.WinUI.Tests\ProgressRing\ProgressRingInteractionTests.cs`

## Ported Source Behavior

- The old template root named `Ring` was removed. The template now uses the WinUI 3 source names `LayoutRoot` and `LottiePlayer`.
- The old WPF `SizeStates` group and `SixthCircle` visibility switch were removed. WinUI 3 source only has the `CommonStates` group with `Inactive`, `DeterminateActive`, and `Active`.
- Inactive state opacity now targets `LayoutRoot.Opacity`, matching the source setter. ModernWpf no longer collapses the ring when inactive.
- `OnApplyTemplate` now discovers `LayoutRoot`, updates determinate progress, and then updates visual states, matching the source ordering.
- `Loaded` now refreshes visual states like source.
- `SizeChanged` now only applies template settings instead of also driving visual states.
- `Value`, `Minimum`, and `Maximum` changes now run the source-shaped determinate progress update path when the ring is determinate.
- `ApplyTemplateSettings` now follows the source width calculation from `ActualWidth` rather than the old `min(ActualWidth, ActualHeight)` guess.
- `CoerceValue` now uses the source-shaped `IsInBounds` helper.
- The automation peer name path now follows source by prefixing the indeterminate status as `Busy {name}` for active indeterminate rings.
- Theme resources already matched the WinUI 3 source resource aliases.
- `ProgressRingIndicator` now derives its radius and stroke width from the generated 32x32 animated visual: an 8px ellipse and a 1.5px stroke under the source 1.77 shape scale. At the Gallery's 60px sample size this produces the source 26.55px radius and 4.98px effective stroke instead of the previous empirical 27px radius and 6px stroke.

## WPF Substitutions

- WinUI 3 renders through `AnimatedVisualPlayer`, `IAnimatedVisualSource`, and Lottie-generated `ProgressRingIndeterminate` / `ProgressRingDeterminate` visuals. ModernWpf does not reference the WinUI animation pipeline in the WPF controls assembly, so `LottiePlayer` is represented by a WPF storyboard-backed grid and a native WPF arc. The substitute uses the generated visual's current radius, stroke, round-cap, sweep, and foreground/background geometry; only Composition's animation/rasterization engine remains substituted.
- WinUI `DeterminateSource` and `IndeterminateSource` are preview animation-source properties. They remain omitted because exposing them as `object` would create a misleading API, and the correct WinUI type is not available in the WPF control assembly.
- WinUI sets `AutomationProperties.AccessibilityView` to `Content` or `Raw` from control code and template setters. WPF has no equivalent property, so ModernWpf keeps the app-visible behavior through `ProgressRingAutomationPeer.IsControlElementCore`.
- WPF visual-state setters do not fully reset held storyboard values the same way WinUI setters do, so `UpdateStates` also resets `LayoutRoot.Opacity` before navigating to the source state.
- `ProgressRingTemplateSettings` remains for WUXC compatibility. Current WinUI 3 keeps these template settings but no longer consumes them from the default Lottie template; ModernWpf's WPF storyboard substitute still uses them for ellipse geometry.

## Verification

Focused tests cover source defaults/resources, `LayoutRoot` / `LottiePlayer` template shape, deletion of the old `Ring` root, source `CommonStates`, inactive `LayoutRoot.Opacity`, automation control-view fallback, source indeterminate automation name shape, determinate range automation, min/max/value coercion, and a 60px/65% raster regression for the generated Lottie geometry.

The live comparison is deterministic: both galleries set the source-equivalent determinate example to 65 through its `ProgressValue` NumberBox, compare the determinate ring directly at 60x60, and enforce a primary-crop mean-delta ceiling of 1.0. Whole-gallery window pixels are only a fallback when no primary control crops exist.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~ProgressRing" --no-restore`
  - Passed 10/10 for `net8.0-windows7.0`.
- `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --filter "FullyQualifiedName~GalleryVisualChecksCompareProgressRingAtAFixedDeterminateValue" --no-restore -m:1`
  - Passed 1/1 for both `net8.0-windows7.0` and `net10.0-windows7.0`.
- `.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls ProgressRing -Reference InstalledWinUI3Gallery -Theme Light -TimeoutSeconds 45 -FailOnDifference`
  - Passed: `artifacts\visual-checks\20260716-233459-296-65888\report.md`, primary delta `0.64`, exact `60x60` crops.
- `.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls ProgressRing -Reference InstalledWinUI3Gallery -Theme Dark -TimeoutSeconds 45 -FailOnDifference`
  - Passed: `artifacts\visual-checks\20260716-233521-049-35744\report.md`, primary delta `0.63`, exact `60x60` crops.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with 0 warnings.
- `rg -n 'x:Name="Ring"|SizeStates|SixthCircle|s_RingName|s_SmallStateName|s_LargeStateName|ActiveStates' .\ModernWpf.Controls\ProgressRing .\test\ModernWpf.WinUI.Tests\ProgressRing .\docs\progressring-winui3-source-audit.md`
  - Only audit note references to deleted behavior and the verification command remain; no stale implementation or test helper symbols remain.
