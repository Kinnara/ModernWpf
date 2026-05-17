# ListView / GridView WinUI 3 Source Audit

Date: 2026-05-17

This audit treats `D:\repos\microsoft-ui-xaml` as the source of truth for the existing ModernWpf `ListView` / `GridView` family. The old WPF trigger-driven item visuals are deleted and replaced with a WPF-adapted source state model rather than patched incrementally.

## WinUI 3 Source Inputs

- `src\dxaml\xcp\dxaml\lib\ListViewBase_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ListViewBase_Partial_Interaction.cpp`
- `src\dxaml\xcp\dxaml\lib\ListViewBase_Partial_Selection.cpp`
- `src\dxaml\xcp\dxaml\lib\ListViewBaseItem_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\ListView_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\GridView_Partial.cpp`
- `src\controls\dev\CommonStyles\ListViewItem_themeresources.xaml`
- `src\controls\dev\CommonStyles\GridViewItem_themeresources.xaml`
- `src\dxaml\test\native\external\controls\listviewbaseitem\ListViewBaseItemIntegrationTests.cpp`
- `src\dxaml\test\native\external\enterprise\ListView\ListViewIntegrationTests.cpp`
- `src\dxaml\test\native\external\enterprise\GridView\GridViewIntegrationTests.cpp`

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

## Current Validation

- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore` passed.
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~ListViewApiTests` passed 7/7.
