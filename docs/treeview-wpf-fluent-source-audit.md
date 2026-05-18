# TreeView Official WPF Fluent Source Audit

For stock WPF `TreeView` and `TreeViewItem`, the primary source is official
WPF Fluent rather than WinUI `TreeView`.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\TreeView.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\TreeViewItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## Ported ModernWpf Files

- `ModernWpf\Styles\TreeView.xaml`
- `ModernWpf\Styles\TreeViewItem.xaml`
- `ModernWpf\StockControlsResources.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\TreeView\TreeViewResourceTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Behavior Aligned

- `DefaultTreeViewStyle` now follows official WPF Fluent's stock `TreeView`
  setter surface, transparent chrome, rounded border, WPF `ScrollViewer`, and
  virtualization trigger.
- `DefaultTreeViewItemStyle` now follows official WPF Fluent's WPF
  `TreeViewItem` template with `Expander`, `ChevronIcon`, `ActiveRectangle`,
  plain WPF `ContentPresenter`, WPF triggers, and official chevron glyph
  resources.
- The old ModernWpf WinUI-shaped `TreeViewItemHelper`, `VisualStateEx`,
  `ContentPresenterEx`, `FontIconFallback`, indentation helper, selected-state
  setter matrix, and `ExpandCollapseChevron` template path were deleted from
  the stock WPF style path.
- `TreeViewItemBackground`, `TreeViewItemBackgroundPointerOver`,
  `TreeViewItemBackgroundSelected`, `TreeViewItemForeground`, and
  `TreeViewItemSelectionIndicatorForeground` now map to official Fluent theme
  concepts.

## ModernWpf Substitutions

| Official WPF Fluent source | ModernWpf substitution | Reason |
| --- | --- | --- |
| `System.Runtime` namespace for `system:Double` / `system:String` | `mscorlib` | Keeps copied resources compatible with ModernWpf's older target frameworks. |
| `Border.CornerRadius` attached setter/template binding on `TreeViewItem` | `primitives:ControlHelper.CornerRadius` | Older ModernWpf targets do not expose the official attached property. |
| Split official style dictionaries | `TreeViewItem.xaml` merged before `TreeView.xaml` | Keeps copied source ownership clear and satisfies `StaticResource` lookup order. |
| Old TreeView density resources | Retained as unused public aliases and compact-resource API keys | Existing compact-resource tests and public resource lookup still depend on them, but official TreeView templates no longer consume them. |

## Validation

- `test\ModernWpf.WinUI.Tests\TreeView\TreeViewResourceTests.cs` covers the
  official WPF Fluent style surface, chevron glyph resources, WPF presenter
  slots, expansion behavior, selection indicator, and deletion of the old helper
  path.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies
  `TreeView.xaml` and `TreeViewItem.xaml` as official WPF Fluent stock
  templates that should not use `VisualStateEx` or `ContentPresenterEx`.
