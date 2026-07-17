# TitleBar WinUI 3 Gallery Parity Audit

Date: 2026-07-17

WinUI 3 source snapshot:

```text
D:\repos\microsoft-ui-xaml
c70471c511a0168b61dcca13af9556465f26b673
```

WinUI Gallery source snapshot:

```text
D:\repos\WinUI-Gallery
1d490ef14f96d5c52de253b94063168eecde08e9
```

## Scope

This audit covers the retained WinUI Gallery `TitleBar` preview and its direct
installed-Gallery visual comparison. ModernWpf's `TitleBarControl` and attached
`TitleBar` APIs remain the documented WPF window-shell substitution; this audit
does not misclassify that legacy shell surface as a complete port of the newer
`Microsoft.UI.Xaml.Controls.TitleBar` API.

## Source Files

- `src\controls\dev\TitleBar\TitleBar.xaml`
- `src\controls\dev\TitleBar\TitleBar_themeresources.xaml`
- `src\controls\dev\TitleBar\TitleBar.cpp`
- `src\controls\dev\TitleBar\TitleBarAutomationPeer.cpp`
- `WinUIGallery\Samples\ControlPages\TitleBarPage.xaml`

## ModernWpf Mapping

- `ModernWpf.Gallery\Pages\WindowingSampleFactory.cs`
- `ModernWpf.Gallery\Testing\GalleryDiagnostics.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`

The source expanded-height preview is 48 pixels tall. With the back button,
pane toggle, and left header collapsed, the hosted AppWindow inset is zero and
the template places 14 pixels before a 16x16 icon, followed by a 16-pixel icon
margin. The source-backed sample therefore places its 186x32 constrained
AutoSuggestBox at x=46 and the 30x30 right header at x=248 in the 470x48 crop.

WinUI Gallery owns the card background, one-pixel stroke, and rounded corners
on the parent example Grid rather than on `TitleBarControl`. The WPF preview
represents that parent ownership with a `-1` surface margin: straight stroke
edges stay outside the control crop while the rounded corner samples remain
visible, matching the installed Gallery crop without changing control size or
content hit targets.

## Validation

- Focused Gallery sample tests pass on `net8.0-windows7.0` and
  `net10.0-windows7.0`.
- Light installed-WinUI Gallery proof:
  `artifacts/visual-checks/20260717-061731-160-44460/report.md`, exact `470x48`
  crops at primary delta `0.74`.
- Dark installed-WinUI Gallery proof:
  `artifacts/visual-checks/20260717-061752-652-53268/report.md`, exact `470x48`
  crops at primary delta `0.82`.
- `Run-GalleryVisualChecks.ps1` enforces a strict TitleBar primary-crop
  threshold of `1.0`.
