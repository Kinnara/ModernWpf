# PipsPager WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\controls\dev\PipsPager\PipsPager.cpp`
- `src\controls\dev\PipsPager\PipsPager.xaml`
- `src\controls\dev\PipsPager\PipsPager.idl`
- `src\controls\dev\PipsPager\PipsPagerAutomationPeer.cpp`
- `src\controls\dev\PipsPager\PipsPager_themeresources.xaml`
- `src\controls\dev\PipsPager\Strings\en-us\Resources.resw`

ModernWpf files:

- `ModernWpf.Controls\PipsPager\PipsPager.cs`
- `ModernWpf.Controls\PipsPager\PipsPager.xaml`
- `ModernWpf.Controls\PipsPager\PipsPagerAutomationPeer.cs`
- `ModernWpf.Controls\PipsPager\PipsPagerTemplateSettings.cs`
- `ModernWpf.Controls\PipsPager\PipsPagerWrapMode.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `ModernWpf\Resources\Strings.resx`
- `ModernWpf\Resources\Strings.Designer.cs`
- `test\ModernWpf.WinUI.Tests\PipsPager\PipsPagerApiTests.cs`

## Ported Source Behavior

- The old guessed `PART_PipsPanel` / manual visible-window implementation was removed. The template now follows WinUI's `RootPanel`, `PreviousPageButton`, `PipsPagerScrollViewer`, `PipsPagerItemsRepeater`, and `NextPageButton` shape.
- `TemplateSettings.PipsPagerItems` now follows WinUI's source collection behavior: finite pagers store every page as 1-based item values, and infinite pagers grow the item source as selection moves forward.
- Pip realization now happens through `ItemsRepeater.ElementPrepared` / `ElementIndexChanged`; realized pip buttons receive the selected or normal source styles, source page automation names, UIA position/size metadata, and click handlers from the repeater index.
- Scroll viewport sizing now follows WinUI's `CalculateScrollViewerSize(default, selected, numberOfPages, maxVisiblePips)` formula instead of clipping the item source.
- `WrapMode` was added with the WinUI `None` / `Wrap` values. Previous/next button navigation wraps in finite mode when enabled, and the source virtualization rule disables `StackLayout.IsVirtualizationEnabled` while wrapping is active.
- Navigation-button visibility now uses WinUI's visible/hidden/collapsed and enabled/disabled state model, including pointer/focus reveal behavior and wrap-mode edge visibility.
- The default template resources now use the WinUI glyphs, 24x24 navigation buttons, 12x24 horizontal pip metrics, 24x12 vertical pip metrics, pressed scale, and source PipsPager theme-resource aliases for light, dark, and high contrast.
- Automation now follows the WinUI source peer shape: control type `Menu`, class name `PipsPager`, selection pattern, source `Pager` control name, `Previous Page` / `Next Page` button names, and `Page N` pip names.

## WPF Substitutions

- WPF has no native WinUI `VisualState.Setters`, so the template uses `VisualStateEx.Setters`.
- WPF `Grid` lacks WinUI `CornerRadius`, `BorderBrush`, and `BorderThickness`; the pip and navigation button template roots use `GridEx`.
- WinUI `FontIcon.MirroredWhenRightToLeft` and `AutomationProperties.AccessibilityView=Raw` have no direct WPF equivalent in the current ModernWpf surface.
- WPF `BringIntoView` does not expose WinUI `BringIntoViewOptions` alignment ratios, so selected pips call the WPF `BringIntoView` substitute.
- WPF `StackLayout` does not inherit a templated-parent binding like the WinUI template path; ModernWpf syncs the repeater layout orientation from the source-shaped `OnOrientationChanged` path.
- WinUI's pointer-exit bounds tolerance and focus redirection hooks depend on WinUI pointer/focus event args; ModernWpf keeps the WPF mouse-enter/leave and keyboard-focus substitutes.

## Verification

Focused tests cover source defaults, `WrapMode`, automation peer shape, pip UIA metadata and names, empty pager behavior, source item-source collection behavior, scroll viewer sizing, pip and navigation clicks, wrap navigation, layout virtualization disablement for wrap mode, source visual-state setter names, orientation state routing, and default pip metrics.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter FullyQualifiedName~PipsPager --no-restore`
  - Passed 12/12.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with existing repository warnings.
- `rg -n "PART_PipsPanel|PART_RootPanel|PART_PreviousButton|PART_NextButton|VisiblePipWindow|DefaultPipsPager|Previous page|Next page" .\ModernWpf.Controls\PipsPager .\test\ModernWpf.WinUI.Tests\PipsPager`
  - No stale guessed PipsPager template or old automation-name symbols remain in source or focused tests.
