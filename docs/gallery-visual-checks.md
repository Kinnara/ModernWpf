# Gallery Visual Checks

ModernWpf Gallery has two local validation paths for visual parity work:

- Always-run tests in `ModernWpf.Gallery.Tests` cover route parsing, every Gallery catalog route, page construction, layout, and stable automation hooks.
- Local visual checks capture ModernWpf Gallery beside the installed official WinUI 3 Gallery and write screenshots, UIA tree dumps, control crops, and reports under ignored `artifacts/visual-checks/`.
- Local WPF Gallery visual audits capture WPF-equivalent ModernWpf Gallery pages beside the official WPF Gallery checkout and write screenshots, content crops, UIA tree dumps, JSON, and Markdown reports under ignored `artifacts/wpf-gallery-visual-audit/`.

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

List WPF Gallery visual audit cases:

```powershell
.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -ListCases
```

Run a focused WPF Gallery visual audit:

```powershell
.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -BuildModern -Cases Home,Button -Reference OfficialWpfGallery
```

When Windows OS High Contrast is enabled, run WPF Gallery visual audits with
`-Theme HighContrast`. The audit script rejects `-Theme Light` and
`-Theme Dark` in that environment because WPF still applies High Contrast
non-client/content sizing to ModernWpf while the official direct host would be
captured as a normal Light/Dark reference, producing invalid crop-size evidence.

Run WPF Gallery visual audits sequentially. The audit script takes a process
mutex and rejects concurrent GUI runs because overlapping ModernWpf/official
capture sessions can shift focus or crop targets and create invalid comparison
evidence.

Run a ModernWpf-only smoke capture when the official WPF Gallery executable is not built:

```powershell
.\tools\visual-checks\Run-WpfGalleryVisualAudit.ps1 -BuildModern -Cases Home -Reference None
```

Record a short repro clip when a failure only shows up during interaction:

```powershell
.\tools\visual-checks\Record-Window.ps1 -ListWindows
.\tools\visual-checks\Record-Window.ps1 -ProcessName ModernWpf.Gallery -DurationSeconds 8 -FrameRate 10
.\tools\visual-checks\Record-Window.ps1 -WindowTitle "WPF Gallery" -DurationSeconds 8 -FrameRate 10
.\tools\visual-checks\Record-Window.ps1 -Left 60 -Top 60 -Width 1180 -Height 820 -DurationSeconds 8 -FrameRate 10
```

Run a per-control recording audit when the interaction needs to be driven and
checked in one pass:

```powershell
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls CommandBarFlyout -Theme Dark -DurationSeconds 8 -FrameRate 10 -Build
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls TeachingTip,ComboBox,ContentDialog,Flyout,Popup,MenuFlyout -Theme Light -DurationSeconds 8 -FrameRate 10
```

The per-control recorder launches ModernWpf Gallery with `--visual-test`, records
rendered `PrintWindow` frames for the Gallery process and popup HWNDs, drives
the primary interaction, extracts poster frames with FFmpeg, and writes
`recording-manifest.json` plus `report.md` under
`artifacts/gallery-recordings/<stamp>/`. It does not rely on desktop `gdigrab`
because that can capture the desktop background instead of the Gallery window in
this environment. `-CaptureMode Screen` exists for local diagnostics, but runs
from this Codex desktop session must be manually rejected if the decoded frames
show the Windows background instead of the Gallery app.

The recorder writes under `artifacts/window-recordings/` by default. With
`ffmpeg` on `PATH`, `-Recorder Auto` writes compressed MP4 files through
FFmpeg's `gdigrab` input. Without `ffmpeg`, it falls back to the dependency-free
raw AVI writer. Pass `-Recorder Avi` explicitly when testing the fallback path.

The WPF audit script defaults the official reference executable to
`D:\repos\WPF-Samples\Sample Applications\WPFGallery\bin\Debug\net10.0-windows\WPFGallery.exe`.
Pass `-BuildOfficial` to restore/build the reference checkout first, or pass
`-WpfGalleryExe` when using a different official Gallery build. The script
routes ModernWpf with `--visual-test --route`, drives the official WPF Gallery
Settings theme picker and navigation tree through UI Automation, captures full
windows, crops the main content region, and reports normalized content-crop
deltas. After changing the official theme through Settings, the audit returns
the reference app to Home before navigating to the target case. Native mouse
input to the official WPF Gallery tree can be unavailable in the current
execution context, so section and item pages fall back to invoking official
WPF Gallery navigation-card buttons inside `RootContentFrame`. ModernWpf visual
test launches also render `ContentRootGrid.png` and `GalleryContentHost.png`
in-process; the WPF Gallery audit prefers `ContentRootGrid.png` because it is
the closest equivalent to the official page-root crop, then falls back to
`GalleryContentHost.png` or a screenshot crop when needed. Non-Home official
captures crop `RootContentFrame`; Home keeps the wider dashboard content region
because its page intentionally uses negative margins. Use `-FailOnDifference`
only for local triage once a machine has stable capture behavior; the first
milestone uses the artifacts and report as manual visual evidence rather than a
CI gate.

The script launches ModernWpf Gallery with `--visual-test`, `--route`, `--theme`, and `--visual-artifact-dir`. In that mode the Gallery exposes hidden UIA fields:

- `GalleryVisualTestCurrentRoute`
- `GalleryVisualTestReadyState`
- `GalleryVisualTestLastException`

With `-IncludeInteractions`, the TeachingTip pass first closes any `--open-interactions` prepared TeachingTip state, captures a closed baseline, opens the sample, then captures 0ms, 150ms, 300ms, and 450ms screen-rect frames. Screen capture is used for those frames when available so popup content is included. The interaction probe records UIA evidence or trusted visual delta evidence, and reports normalized crop delta and crop size differences against the reference Gallery when both sides have comparable crops.

Static window captures use `PrintWindow` first and fall back to an activated screen-rect capture when `PrintWindow` returns a blank image. If a capture returns a nonblank but invalid reference surface, such as a desktop/wallpaper or Mica backdrop crop from a WinUI composition window, the harness rejects the result when the primary crop has very low visible luminance variation and fails the run instead of reporting false parity.

Static comparisons include primary sample crops for each curated control. The ModernWpf Gallery also writes rendered `GallerySample_*` and `GalleryContentHost` element artifacts under `modernwpf-artifacts/` during visual-test launches; the harness uses those rendered element artifacts before falling back to window screenshot crops. The report ranks controls by primary crop delta plus crop-size mismatch so visual triage can focus on real control-level differences instead of full-window shell noise.

The visual pass intentionally does not make strict screenshot diffs a default CI gate. It fails on blank captures, wrong or missing required sample elements, failed TeachingTip interaction probes, and Gallery exceptions; image deltas are reported for manual review and can be made strict with `-FailOnDifference` once the harness is stable across machines.

## Current triage

Latest full static reference runs with usable installed WinUI Gallery crops:

- Dark: `artifacts/visual-checks/20260513-040058/report.md`
- Light: `artifacts/visual-checks/20260513-040225/report.md`

Findings:

- `InfoBar` is the current control/resource fix baseline. Primary crop delta is about `9.8` in Dark and `9.66` in Light, with only a 3 px height difference against WinUI Gallery.
- `Button` sample parity is fixed in the current branch. Focused checks now report `165x32` vs `166x32`, with primary crop deltas of `10.18` in Dark (`artifacts/visual-checks/20260513-021319/report.md`) and `10.08` in Light (`artifacts/visual-checks/20260513-021337/report.md`).
- `ComboBox` sample parity is fixed in the current branch. Focused checks now report `200x59` vs `208x64`, with primary crop deltas of `6.22` in Dark (`artifacts/visual-checks/20260513-022103/report.md`) and `6.1` in Light (`artifacts/visual-checks/20260513-022005/report.md`).
- `ContentDialog`, `NavigationView`, and `TeachingTip` static sample sizing is fixed in the current branch. The installed reference runs show the remaining targets as `ContentDialog` `101x32`, `NavigationView` `745x460`, and `TeachingTip` `135x32`; ModernWpf-only verification reports `100x32`, `745x460`, and `135x32` in Dark (`artifacts/visual-checks/20260513-024120/report.md`) and Light (`artifacts/visual-checks/20260513-024234/report.md`).
- `NavigationView` header parity is fixed in the current branch. Manual inspection found the valid WinUI reference crop in `artifacts/visual-checks/20260513-030925/NavigationView/winui3-NavigationView-primary-crop.png` showing `Sample Page 1`, while the ModernWpf crop from that run still showed `This is Header Text`. The sample now selects the first item after the `SelectionChanged` handler is attached, matching the WinUI Gallery source behavior. Current installed-reference verification passes with `745x460` crops in Dark (`artifacts/visual-checks/20260513-040058/report.md`) and Light (`artifacts/visual-checks/20260513-040225/report.md`).
- `TeachingTip` expand animation now uses WinUI's final 2.8.7 minimum start scale expression, and ModernWpf interaction verification passes through the in-app rendered open-content artifact: Dark `artifacts/visual-checks/20260513-040451/report.md`, Light `artifacts/visual-checks/20260513-040526/report.md`.
- The current local execution context cannot always produce reliable installed WinUI Gallery XAML pixels through direct screen capture, DWM thumbnail capture, or Windows Graphics Capture experiments; these paths can return backdrop/wallpaper or black client content instead of XAML. The harness keeps `PrintWindow` first and fails invalid installed-reference captures instead of reporting false parity. Example rejected reports: `artifacts/visual-checks/20260513-023339/report.md`, `artifacts/visual-checks/20260513-032310/report.md`, and the screen-first experiment in `artifacts/visual-checks/20260513-035440/report.md`.

Current mismatch classification:

| Control | Classification | Current evidence |
| --- | --- | --- |
| `Button` | Gallery sample mismatch fixed; remaining difference is a 1 px crop-width/text rendering difference. | Dark `10.18`, `165x32` vs `166x32`; Light `10.08`, `165x32` vs `166x32`. |
| `InfoBar` | Control/resource parity improved; remaining difference is a small height/text rendering difference. | Dark `9.8`, `560x92` vs `560x95`; Light `9.66`, `560x92` vs `560x95`. |
| `ComboBox` | Gallery sample mismatch fixed; remaining difference is WPF-vs-WinUI control metric/rendering drift. | Dark `6.22`, `200x59` vs `208x64`; Light `6.1`, `200x59` vs `208x64`. |
| `ContentDialog` | Gallery sample mismatch fixed; remaining difference is a 1 px crop-width/text rendering difference. | Dark `8.32`, `100x32` vs `101x32`; Light `8.4`, `100x32` vs `101x32`. |
| `NavigationView` | Gallery sample event-order mismatch fixed; remaining difference is WPF-vs-WinUI rendering drift with matching crop size and header text. | Dark `7.41`, `745x460` vs `745x460`; Light `7.55`, `745x460` vs `745x460`. |
| `TeachingTip` | Control animation/template parity fixed and interaction checks pass; remaining static button difference is rendering drift. | Dark `3.65`, `135x32` vs `135x32`; Light `5.34`, `135x32` vs `135x32`. |
