# Gallery Visual Checks

ModernWpf Gallery has two validation layers for WinUI parity work:

- Always-run tests in `ModernWpf.Gallery.Tests` cover route parsing, every Gallery catalog route, page construction, layout, and stable automation hooks.
- Local visual checks capture ModernWpf Gallery beside the installed official WinUI 3 Gallery and write screenshots, UIA tree dumps, and reports under ignored `artifacts/visual-checks/`.

Run the unit/runtime checks:

```powershell
dotnet test test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --no-restore
```

Run the local visual pass:

```powershell
.\tools\visual-checks\Run-GalleryVisualChecks.ps1 -Build -Reference InstalledWinUI3Gallery
```

The script launches ModernWpf Gallery with `--visual-test`, `--route`, `--theme`, and `--visual-artifact-dir`. In that mode the Gallery exposes hidden UIA fields:

- `GalleryVisualTestCurrentRoute`
- `GalleryVisualTestReadyState`
- `GalleryVisualTestLastException`

The visual pass intentionally does not make strict screenshot diffs a default CI gate. It fails on blank captures, wrong or missing required sample elements, and Gallery exceptions; image deltas are reported for manual review and can be made strict with `-FailOnDifference` once the harness is stable across machines.
