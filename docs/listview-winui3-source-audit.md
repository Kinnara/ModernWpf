# ListView / GridView WinUI 3 Source Audit

Date: 2026-07-17

This audit treats the current `microsoft-ui-xaml/main` tree as the source of truth for the existing ModernWpf `ListView` / `GridView` family. The upstream paths and mappings below were rechecked at commit `3cae15f071f1ab8565f9a7592dbf27f04bafe651` on 2026-07-17, and the rendered GridView example was compared with the installed WinUI 3 Gallery. The old WPF trigger-driven item visuals are deleted and replaced with a WPF-adapted source state model rather than patched incrementally.

## WinUI 3 Source Inputs

- `dxaml\xcp\dxaml\lib\ListViewBase_Partial.cpp`
- `dxaml\xcp\dxaml\lib\ListViewBase_Partial_Interaction.cpp`
- `dxaml\xcp\dxaml\lib\ListViewBase_Partial_Selection.cpp`
- `dxaml\xcp\dxaml\lib\ListViewBaseItem_Partial.cpp`
- `dxaml\xcp\dxaml\lib\ListView_Partial.cpp`
- `dxaml\xcp\dxaml\lib\GridView_Partial.cpp`
- `controls\dev\CommonStyles\ListViewItem_themeresources.xaml`
- `controls\dev\CommonStyles\GridViewItem_themeresources.xaml`
- `dxaml\test\native\external\controls\listviewbaseitem\ListViewBaseItemIntegrationTests.cpp`
- `dxaml\test\native\external\enterprise\ListView\ListViewIntegrationTests.cpp`
- `dxaml\test\native\external\enterprise\GridView\GridViewIntegrationTests.cpp`

## ModernWpf Artifacts

- `ModernWpf.Controls\ListView\ListViewBase.cs`
- `ModernWpf.Controls\ListView\ListViewBaseItem.cs`
- `ModernWpf.Controls\ListView\ListViewItem.cs`
- `ModernWpf.Controls\ListView\GridViewItem.cs`
- `ModernWpf.Controls\ListView\ListView.xaml`
- `ModernWpf.Controls\ListView\GridView.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\ListView\ListViewApiTests.cs`
- `test\ModernWpf.Gallery.Tests\GalleryAutomationHookTests.cs`
- `test\ModernWpf.Gallery.Tests\WpfGallerySourceShapeTests.cs`
- `tools\visual-checks\Run-GalleryVisualChecks.ps1`

## Implementation Mapping

| WinUI source behavior | ModernWpf mapping |
| --- | --- |
| `ListViewBase` raises `ItemClick` from the primary interaction gesture and uses the contained content when the clicked item is its own container. | `NotifyListItemClicked` now falls back from the WPF generator result to the item content when the container is the item, matching the source own-container path. |
| Source treats Enter and Space as primary item gestures while avoiding Alt+Space. | `ListViewBaseItem` drives the same keyboard click path for Enter and Space, with Alt+Space excluded for the WPF system-menu chord. |
| Source item visuals include `Normal`, `PointerOver`, `Pressed`, `Selected`, `PointerOverSelected`, `PressedSelected`, `SelectedDisabled`, and `Disabled`. | The WPF item templates now use source-named `CommonStates` with `VisualStateEx.Setters`; the old WPF `ControlTemplate.Triggers` visual logic was deleted. |
| Source multiselect item visuals use `NoMultiSelect`, `ListMultiSelect`, and `GridMultiSelect`. | `ListViewBaseItem` now drives those source state names directly, selecting `ListMultiSelect` for `ListViewItem` and `GridMultiSelect` for `GridViewItem`. |
| Source common styles expose selected-disabled item resources. | ModernWpf now carries `ListViewItemBackgroundSelectedDisabled` and `GridViewItemBackgroundSelectedDisabled` in Light, Dark, and HighContrast dictionaries. |
| Source focus-visual brush properties are brushes. | `FocusVisualPrimaryBrush` and `FocusVisualSecondaryBrush` are now `Brush` CLR wrappers instead of the old guessed `Thickness` wrappers. |

## WPF Substitutions

- WinUI 3 uses `ListViewItemPresenter` for the compact default template. This slice does not add a new presenter control under the no-new-controls rule; the WPF template maps the presenter properties into explicit `Border`, `ContentPresenterEx`, selection border, and checkbox parts.
- WinUI can use duplicate visual-state names in different groups. WPF template namescopes reject duplicate state names, so selected item visuals are folded into the source-named `CommonStates` rather than duplicating `Selected` in a separate `SelectionStates` group.
- WinUI selection, virtualization, drag/drop, semantic zoom, and gamepad paths remain platform services. ModernWpf keeps WPF `ListBoxItem` selection as the substrate and ports only the visible/source-feasible item-click and visual-state behavior in this slice.
- WinUI native storyboards and `VisualState.Setters` are represented through ModernWpf `VisualStateEx.Setters`.

## Installed-Gallery Pixel And Interaction Lock

- No additional product-template change was required in the 2026-07-17 refresh. The existing source-backed GridView example already matches the installed Gallery at exact `657x412` geometry.
- The live harness now resolves the reference Gallery's `ClickOutput0` rather than falling back to the whole `BasicGridView` sample when proving `ItemClick`.
- Both galleries give the output text element the remaining example width, so the harness extracts the pixels changed by `You clicked Item 1.` before comparing them. A bounded one-pixel alignment accounts for the platforms' one-pixel text-box height/baseline difference; a separate size gate still caps the total crop metric difference at four pixels.
- Static primary crops are protected by a `2.0` mean-delta gate and an exact-size gate. Interaction crops are required, use an `8.0` mean-delta gate, and must expose the expected click-result text.
- Final installed-Gallery evidence passes in Light at `artifacts/visual-checks/20260717-092950-082-76648/report.md` (`1.61` static, `6.40` interaction) and Dark at `artifacts/visual-checks/20260717-092919-846-50492/report.md` (`1.60` static, `6.64` interaction). Interaction crops are `122x18` versus `120x19`, within the strict four-pixel metric gate.

## Current Validation

- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1 -p:UseSharedCompilation=false -nodeReuse:false` passed for `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0` with existing warnings only.
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~ListViewApiTests" --no-restore` passed 9/9. This test project targets only net8.
- `dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj -f net8.0-windows7.0 --filter "FullyQualifiedName~GridViewSampleMatchesWinUIGalleryExamples|FullyQualifiedName~GalleryVisualChecksClicksCommonSelectionInteractionControls|FullyQualifiedName~GalleryVisualChecksEnforceGridViewPixelParityThreshold" --no-restore` passed 3/3.
- The same Gallery sample, interaction-contract, and strict-gate slice passed 3/3 on `net10.0-windows7.0`.
- The PowerShell parser check for `Run-GalleryVisualChecks.ps1` passed.
