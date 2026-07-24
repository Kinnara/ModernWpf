# Gallery Visual Checks

ModernWpf Gallery has two local validation paths for visual parity work:

- Always-run tests in `ModernWpf.Gallery.Tests` cover route parsing, every Gallery catalog route, page construction, layout, and stable automation hooks.
- Local visual checks capture ModernWpf Gallery beside the installed official WinUI 3 Gallery and write screenshots, UIA tree dumps, control crops, and reports under ignored `artifacts/visual-checks/`.
- Local WPF Gallery visual audits capture WPF-equivalent ModernWpf Gallery pages beside the official WPF Gallery checkout and write screenshots, content crops, UIA tree dumps, JSON, and Markdown reports under ignored `artifacts/wpf-gallery-visual-audit/`.

Run the unit/runtime checks:

```powershell
dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --no-restore
```

## Common Commands

After code changes that affect Gallery visuals, build the Gallery first. If the
visual script's `-Build` path fails without useful output, run this direct build
and then rerun the visual script without `-Build`:

```powershell
dotnet build .\ModernWpf.Gallery\ModernWpf.Gallery.csproj -f net8.0-windows7.0 -c Debug --no-restore
```

Run the focused CommandBarFlyout test class:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --framework net8.0-windows7.0 --filter "FullyQualifiedName~CommandBarFlyoutApiTests"
```

Run the focused CommandBarFlyout alignment and second-open regression test:

```powershell
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --framework net8.0-windows7.0 --filter "FullyQualifiedName~CommandBarFlyoutApiTests.ExpandedFlyoutOverflowAlignsAndSurvivesSecondOpen"
```

Run CommandBarFlyout static plus interaction visual parity against a cached
WinUI reference. The requested `-Theme` must match the cached reference run:

```powershell
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls CommandBarFlyout -Theme Dark -WinUIReferenceRunDir .\artifacts\visual-checks\<winui-reference-run> -TimeoutSeconds 20 -ModernWpfRetries 0 -IncludeInteractions
```

Refresh a CommandBarFlyout WinUI reference when the installed reference app,
theme, or capture machine changes. Do this once, then reuse the output directory
with `-WinUIReferenceRunDir` for fast ModernWpf iterations:

```powershell
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Controls CommandBarFlyout -Theme Dark -Reference InstalledWinUI3Gallery -TimeoutSeconds 30 -IncludeInteractions
```

Run a CommandBarFlyout recording proof when animation, flicker, close behavior,
or repeat-open behavior is under review:

```powershell
.\tools\visual-checks\Record-GalleryControlInteractions.ps1 -Controls CommandBarFlyout -Theme Dark -DurationSeconds 8 -FrameRate 10 -Build
```

Use `-Controls`, not `-Scenario`, with `Run-GalleryVisualChecks.ps1`. The visual
script launches the Gallery as a normal window, moves it to `60,60`, captures,
and then closes it in `finally`; a focused run can be visible only briefly.

Run the local WinUI-backed visual pass for ported WinUI controls:

```powershell
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Build -Reference InstalledWinUI3Gallery
```

Pass `-Theme Light` or `-Theme Dark` to match the installed WinUI Gallery theme before comparing image deltas.
Do not use this script with WPF Gallery stock-control pages such as `Button`,
`CheckBox`, `ComboBox`, `RadioButton`, `Slider`, `TextBox`, `PasswordBox`, or
`RichTextEdit`; use `Run-WpfGalleryVisualAudit.ps1 -Reference
OfficialWpfGallery` for those pages.

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

Small state/value interactions can have low full-frame delta. The recorder only
passes those cases when the manifest also proves the control-level change, such
as before/after toggle state or a reached numeric target value.

Output controls are stricter: broad frame delta is not enough for an automatic
pass. If UI Automation cannot expose changed output text, keep the report at
`NeedsReview` and cite reviewed poster frames before marking the control
recorded in the matrix.

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

For focused WinUI parity loops, pass `-WinUIReferenceRunDir` with a previous
`Run-GalleryVisualChecks.ps1` output directory to reuse the installed WinUI
Gallery capture instead of relaunching the reference app every iteration. The
cached report and referenced screenshots are validated before use. For
CommandBarFlyout, interaction parity is strict: the harness crops the open
surface from the screen-bounds union of the primary commands, ellipsis, and
expanded secondary commands, requires both ModernWpf and WinUI to use that
`CommandBarFlyoutOpenSurfaceScreen` crop source, and fails on crop delta,
crop-size drift, or a missing/low-variation combined crop. This catches
secondary-menu gaps and edge misalignment that broad full-window screenshots or
secondary-popup-only captures can hide.

Static window captures use `PrintWindow` first and fall back to an activated screen-rect capture when `PrintWindow` returns a blank image. If a capture returns a nonblank but invalid reference surface, such as a desktop/wallpaper or Mica backdrop crop from a WinUI composition window, the harness rejects the result when the primary crop has very low visible luminance variation and fails the run instead of reporting false parity.

Static comparisons include primary sample crops for each curated control. The ModernWpf Gallery also writes rendered `GallerySample_*` and `GalleryContentHost` element artifacts under `modernwpf-artifacts/` during visual-test launches; the harness uses those rendered element artifacts before falling back to window screenshot crops. The report ranks controls by primary crop delta plus crop-size mismatch so visual triage can focus on real control-level differences instead of full-window shell noise.

`NavigationView` also has a feature-level indicator gate. Inside the exact
`745x460` primary crops, the harness groups the solid saturated pixels in the
left 24-pixel strip and requires ModernWpf and WinUI to have identical color,
count, and bounds. This prevents the 3x16 high-salience selection indicator
from moving several pixels while remaining invisible to a whole-crop mean.
The current Light executable-gate report is
`artifacts/visual-checks/20260719-113416-596-81168/report.md`: both sides have
38 solid `#0067C0` pixels at `4,97,3x14` (the rounded edge pixels are
antialiased and intentionally excluded).

The NavigationView page exports a stable rendered artifact for every one of its
eight Gallery examples: default, Top, adaptive, tabs, Data binding, footer,
hierarchy, and API. The installed-reference pass scrolls the matching
`nvSample5`, `nvSample6`, `nvSample2`, `nvSample7`, `nvSample4`, `nvSample9`,
`nvSample8`, and `nvSample` controls fully into view and captures the exact
control bounds. The ModernWpf artifact path applies parent-offset correction to
every `GallerySample_NavigationView_*` NavigationView, so offscreen page position
does not shift or truncate a crop.

The NavigationView sample matrix is a strict `-FailOnDifference` gate. It
requires all 16 artifacts to exist, be nonblank, have visible variation, and
match exact dimensions. It then compares each ModernWpf/WinUI pair with a
sample-specific threshold; the longer adaptive/tabs text surfaces allow the
small WPF/WinUI glyph-raster residual while exact geometry remains mandatory.
Fresh Light
`artifacts/visual-checks/20260719-131911-804-89328/report.md` and Dark
`artifacts/visual-checks/20260719-131949-955-99496/report.md` runs both pass
`8/8`. Sizes are `745x460` for the first five samples, `592x460` for footer,
`565x460` for hierarchy, and `458x540` for API on both sides. Light deltas range
from `0.52` to `3.17`; Dark deltas range from `0.47` to `3.03`.

The Gallery runtime matrix complements the pixels: it checks every sample's
initial size and mode, Top item/indicator geometry, expanded Left pane/list
geometry, LeftCompact label suppression, the Data binding
`ClosedCompact + ListSizeCompact` pair, footer/hierarchy option transitions,
API option behavior, normal pane-title font, initially empty Frame, and
selection-driven Frame population. This replaces the earlier primary-plus-one-
supplemental coverage that missed several visibly wrong lower samples.

The final branch-wide port sweep is Light
`artifacts/visual-checks/all-ported-postfix-light/20260719-234456-059-46680/report.md`
and Dark
`artifacts/visual-checks/all-ported-postfix-dark/20260719-235452-227-27096/report.md`.
Each run has 74 successful capture rows covering all 37 retained WinUI Gallery
controls in both applications, all 94 expected sample cards are present and
nonblank, all 94 ModernWpf/reference pairs pass their control-specific gates,
and all 37 review sheets were checked. SHA-256 comparison against the preceding
reviewed matrices left 27 identical Light sheets and 28 identical Dark sheets;
the remaining 10 Light and 9 Dark sheets were reviewed directly. Their changes
are animated ProgressRing/ProgressBar phase, randomized Gallery data, or
one-pixel WPF/WinUI card-height raster drift. No sample has clipped content,
misplaced selection chrome, compact-pane label leakage, or excess host spacing.

The final automated cross-control pass is also order-sensitive rather than a
collection of isolated green tests. `ModernWpf.Gallery.Tests` passes 703/703 and
`ModernWpf.WinUI.Tests` passes 1,002/1,002. The WinUI host eagerly creates all
theme dictionaries on its shared STA; resource-only audits run through that
host; detached GridEx render tests disconnect their child visual and drain the
dispatcher; and animated ToggleSwitch, ScrollViewer, and CommandBarFlyout tests
wait for their documented settled state. These guards prevent an earlier test
from silently corrupting a later control's layout or resources.

The visual pass intentionally does not make strict screenshot diffs a default CI gate. It fails on blank captures, wrong or missing required sample elements, failed TeachingTip interaction probes, and Gallery exceptions; image deltas are reported for manual review and can be made strict with `-FailOnDifference` once the harness is stable across machines.

## Current triage

Latest full static reference runs with usable installed WinUI Gallery crops:

- Dark: `artifacts/visual-checks/20260513-040058/report.md`
- Light: `artifacts/visual-checks/20260513-040225/report.md`

Findings:

- `CommandBarFlyout` open-surface join and edge alignment are fixed in the current branch. The focused ModernWpf interaction capture passes at `artifacts/visual-checks/20260607-163906-967-188756/report.md`; the open-surface crop has continuous row coverage across the primary/secondary join and a `229` px painted width. The cached WinUI reference at `artifacts/visual-checks/20260607-121845-471-226920` is still useful for manual pixel review, but its interaction crop source predates the stricter `CommandBarFlyoutOpenSurfaceScreen` source and fails the refreshed harness source check until a new installed WinUI capture can be recorded.
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

## 2026-07-21 Per-State and Non-Pixel Evidence Contract

`Run-GalleryVisualChecks.ps1` now separates three kinds of evidence:

- `Pixel` compares the complete stable ControlExample image and applies the
  control-specific mean-delta and geometry gate.
- `VolatileDataGeometry` is used by randomized ItemsRepeater Example 6. It
  requires exact width and bounded height drift, and explicitly requires
  separate semantic evidence rather than comparing random names, colors,
  ingredients, and ordering.
- `AnimatedTemporal` is used by indeterminate ProgressRing and
  WinUIProgressBar. It applies the same geometry contract and requires recorder
  evidence that pixels change over time and that the configured state changes.

The report and review sheet print the mode and evidence contract for every
row. Geometry-only rows no longer contribute their arbitrary frame pixels to
the crop ranking or the pixel pass/fail gate. Fresh Light and Dark validation is
in `artifacts/visual-checks/volatile-animated-light-v1/20260721-184321-529-51384/report.md`
and
`artifacts/visual-checks/volatile-animated-dark-v1/20260721-184430-145-41176/report.md`.

With `-IncludeInteractions`, supported button, toggle, split, and toggle-split
controls also receive paired state matrices. The harness uses real pointer
down/up and keyboard Tab input, captures a four-pixel focus gutter, requires
exact or explicitly bounded geometry, compares the same named state across
ModernWpf and WinUI Gallery, and verifies that required hover/press/focus/check
states visibly differ from rest in both implementations. CommandBarFlyout has
its own ordered ten-transition matrix, including first open and collapse after
the expanded ellipsis is clicked.

The state matrix is authoritative for live interaction pixels; detached primary
artifacts remain advisory for controls covered by that matrix. Cached reference
interactions normalize `Frames` to an array before selecting the last frame, so
a single deserialized frame cannot be misread as a dictionary-sized sequence.

Final post-fix state evidence includes:

- CommandBarFlyout Light/Dark 10/10:
  `commandbar-state-gate-light-v5/20260721-025151-971-41592` and
  `commandbar-state-gate-dark-v2/20260721-025458-794-39604`;
- HyperlinkButton and ToggleButton Light/Dark 15/15 combined:
  `state-matrix-buttons-light-postfocus-v6/20260721-191616-798-64492` and
  `state-matrix-buttons-dark-postfocus-v1/20260721-191706-513-5272`;
- AppBarButton Dark 4/4:
  `state-matrix-appbar-dark-final-v1/20260721-191841-629-69416`.

All paths above are beneath `artifacts/visual-checks/` and contain the complete
paired crops, JSON evidence, and Markdown report.
