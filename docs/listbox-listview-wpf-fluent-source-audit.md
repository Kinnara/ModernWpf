# ListBox / ListView Official WPF Fluent Source Audit

Date: 2026-05-18

## Source Inspected

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ListBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ListBoxItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GridView.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ListView.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ListViewItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## Ported Shape

- `ModernWpf\Styles\ListBox.xaml` now carries the official stock `ListBox` template and no longer embeds the old WinUI-like item template.
- `ModernWpf\Styles\ListBoxItem.xaml` now carries the official stock `ListBoxItem` style and template.
- `ModernWpf\Styles\GridView.xaml` now carries the official stock `GridViewColumnHeader`, `GridViewScrollViewerStyleKey`, and `GridViewTemplate` resources.
- `ModernWpf\Styles\ListViewItem.xaml` now carries the official stock `ListViewItem` and `GridViewItemContainerStyleKey` styles.
- `ModernWpf\Styles\ListView.xaml` now carries the official stock `ListView` style and switches to the official `GridViewTemplate` when `View` is a WPF `GridView`.
- `ModernWpf\StockControlsResources.xaml` now merges the split official dictionaries in dependency order.

## Deleted Guesses

- Removed the stock `ListBoxItem` / `ListViewItem` dependency on `ContentPresenterEx`.
- Removed the old stock ListBox/ListView `FocusVisualHelper` and system-focus-visual bridge.
- Removed the old stock ListView `ScrollViewerEx` template path.
- Removed the old stock `ListViewBaseItemRoundedChromeEnabled` resource from the WPF stock style file.
- Removed the old WPF trigger matrix around `Selector.IsSelectionActive` for stock ListBoxItem.

## Intentional Differences

| Source detail | ModernWpf substitution | Reason |
| --- | --- | --- |
| Split source dictionaries | Same split under `ModernWpf\Styles\ListBoxItem.xaml`, `GridView.xaml`, and `ListViewItem.xaml`, merged from `StockControlsResources.xaml` | Preserves source file ownership while keeping the public stock controls resource entry point. |
| `System.Runtime` namespace in `GridView.xaml` / `ListViewItem.xaml` | `mscorlib` | Keeps older ModernWpf targets compatible. |
| Official `Border.CornerRadius` attached setters on item containers / gripper | `Border.CornerRadius` | Older ModernWpf targets do not expose the newer official WPF attached property. |
| `Fluent.Controls.ViewIsGridViewConverter` | Existing `ModernWpf.Controls.Primitives.IsGridViewConverter` | ModernWpf already ships the equivalent WPF converter; avoids adding the official WPF Fluent helper assembly namespace. |
| Official resource brush definitions | ModernWpf aliases in `ThemeResources\Light.xaml`, `Dark.xaml`, and `HighContrast.xaml` | Keeps ModernWpf's theme-resource model while exposing the official keys required by the imported templates. |

## Test Evidence

- `test\ModernWpf.WinUI.Tests\CommonStyles\ListBoxListViewVisualStateTests.cs` covers official stock setter surfaces, presenter shape, selection indicator shape, GridView header template shape, and deleted guesses.
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs` now expects stock ListBox/ListView/GridView presenters to be plain WPF `ContentPresenter` instances.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies `ListBox.xaml`, `ListBoxItem.xaml`, `GridView.xaml`, `ListView.xaml`, and `ListViewItem.xaml` as official WPF Fluent stock templates that should not use `VisualStateEx`.
