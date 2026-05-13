# Gallery Visual Checks

ModernWpf Gallery has two validation layers for WinUI parity work:

- Always-run tests in `ModernWpf.Gallery.Tests` cover route parsing, every Gallery catalog route, page construction, layout, and stable automation hooks.
- Local visual checks capture ModernWpf Gallery beside the installed official WinUI 3 Gallery and write screenshots, UIA tree dumps, control crops, and reports under ignored `artifacts/visual-checks/`.

Run the unit/runtime checks:

```powershell
dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --no-restore
```

Run the local visual pass:

```powershell
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Build -Reference InstalledWinUI3Gallery
```

Pass `-Theme Light` or `-Theme Dark` to match the installed WinUI Gallery theme before comparing image deltas.

Run the TeachingTip interaction pass:

```powershell
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls TeachingTip -Reference InstalledWinUI3Gallery -IncludeInteractions
```

The script launches ModernWpf Gallery with `--visual-test`, `--route`, `--theme`, and `--visual-artifact-dir`. In that mode the Gallery exposes hidden UIA fields:

- `GalleryVisualTestCurrentRoute`
- `GalleryVisualTestReadyState`
- `GalleryVisualTestLastException`

With `-IncludeInteractions`, the TeachingTip pass captures a closed baseline, invokes the sample button, then captures 0ms, 150ms, 300ms, and 450ms screen-rect frames. Screen capture is used for those frames so WPF `Popup` content is included. The interaction probe records both UIA evidence, when available, and the open-vs-closed image delta. It also writes an opened-tip crop based on the `ContentRootGrid` UIA element when possible, falling back to the largest changed image region, and reports normalized crop delta and crop size differences against the reference Gallery.

Static window captures use `PrintWindow` first and fall back to an activated screen-rect capture when `PrintWindow` returns a blank image. If a fallback capture returns a nonblank but invalid reference surface, such as a desktop/wallpaper crop from a WinUI composition window, the harness rejects the result when the primary crop has very low visible luminance variation and retries the installed WinUI Gallery reference from a fresh launch.

Static comparisons include primary sample crops for each curated control. The ModernWpf Gallery also writes rendered `GallerySample_*` element artifacts under `modernwpf-artifacts/` during visual-test launches; the harness uses those rendered element artifacts before falling back to window screenshot crops. The report ranks controls by primary crop delta plus crop-size mismatch so visual triage can focus on real control-level differences instead of full-window shell noise.

The visual pass intentionally does not make strict screenshot diffs a default CI gate. It fails on blank captures, wrong or missing required sample elements, failed TeachingTip interaction probes, and Gallery exceptions; image deltas are reported for manual review and can be made strict with `-FailOnDifference` once the harness is stable across machines.

## Current triage

Latest full static reference runs:

- Dark: `artifacts/visual-checks/20260513-020316/report.md`
- Light: `artifacts/visual-checks/20260513-022417/report.md`

Findings:

- `InfoBar` is the current control/resource fix baseline. Primary crop delta is about `9.8` in Dark and `9.66` in Light, with only a 3 px height difference against WinUI Gallery.
- `Button` sample parity is fixed in the current branch. Focused checks now report `165x32` vs `166x32`, with primary crop deltas of `10.18` in Dark (`artifacts/visual-checks/20260513-021319/report.md`) and `10.08` in Light (`artifacts/visual-checks/20260513-021337/report.md`).
- `ComboBox` sample parity is fixed in the current branch. Focused checks now report `200x59` vs `208x64`, with primary crop deltas of `6.22` in Dark (`artifacts/visual-checks/20260513-022103/report.md`) and `6.1` in Light (`artifacts/visual-checks/20260513-022005/report.md`).
- `ContentDialog`, `NavigationView`, and `TeachingTip` static sample sizing is fixed in the current branch. The earlier Light reference run showed the remaining targets as `ContentDialog` `101x32`, `NavigationView` `745x460`, and `TeachingTip` `135x32`; current ModernWpf-only verification reports `100x32`, `745x460`, and `135x32` in Dark (`artifacts/visual-checks/20260513-024120/report.md`) and Light (`artifacts/visual-checks/20260513-024234/report.md`).
- The current local execution context cannot produce reliable installed WinUI Gallery pixels through `CopyFromScreen`; the harness now fails those invalid reference captures instead of reporting false parity. Example rejected report: `artifacts/visual-checks/20260513-023339/report.md`.
- TeachingTip interaction capture remains a harness gap for ModernWpf because external UIA can verify the WinUI popup crop, but the WPF sample button does not reliably open through the same external automation path.
