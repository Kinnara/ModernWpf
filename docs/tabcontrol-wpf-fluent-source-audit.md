# TabControl Official WPF Fluent Source Audit

## Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\TabControl.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

## ModernWpf Files

- `ModernWpf\Styles\TabControl.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\TabView\TabViewResourceTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Summary

ModernWpf now treats WPF `TabControl` and `TabItem` as stock WPF controls whose
primary source is official WPF Fluent. The previous WinUI `TabView`-shaped
default style was deleted instead of preserved as a compatibility baseline.

The active `TabControl.xaml` is copied from official WPF Fluent and keeps the
official `DefaultTopTabControlStyle`, `DefaultBottomTabControlStyle`,
`DefaultLeftTabControlStyle`, `DefaultRightTabControlStyle`,
`DefaultTabControlStyle`, `DefaultTabItemStyle`, and implicit `TabControl` /
`TabItem` styles.

## Deleted Guessed Layer

- `ModernWpf\Controls\Primitives\TabItemHelper.cs` was removed.
- `ModernWpf\Controls\Primitives\TabControlHelper.cs` was removed.
- The stock TabControl template no longer uses `VisualStateEx`,
  `VisualStateManagerEx`, `ContentPresenterEx`, `ThemeShadowChrome`, or
  helper-driven header/footer/icon state tracking.
- WinUI `TabView` was excluded from this stock-control phase because WPF
  `TabControl` exposes a different item, close, reorder, and overflow model.
  Preview 5 adds `ModernWpf.Controls.TabView` as a separate source-audited
  control; it does not replace or restyle the stock controls covered here.

## Substitutions

| Official WPF Fluent surface | ModernWpf substitution | Reason |
| --- | --- | --- |
| Official `TabViewForeground`, `TabViewItemForegroundSelected`, `TabViewBorderBrush`, and `TabViewSelectedItemBorderBrush` resources | Added as ModernWpf theme aliases across Light, Dark, and HighContrast | Required by the copied official templates while keeping ModernWpf's theme alias conventions. |
| `DefaultControlFocusVisualStyle` | Added as an alias to ModernWpf's existing `HighVisibilityFocusVisual` style | Official WPF Fluent stock styles consume this key directly. |
| WinUI `TabView` control surface | Excluded from the stock-control mapping; implemented separately in Preview 5 | Stock WPF `TabControl` continues to follow official WPF Fluent. `ModernWpf.Controls.TabView` is governed by `docs/tabview-winui3-source-audit.md` and has its own items, close, overflow, reorder, automation, and WPF tear-out behavior. |
| Dragablz sample `TabItemHelper.Icon` usage | Removed from the sample | The deleted helper was part of the old guessed TabView-shaped layer, not official WPF Fluent stock TabControl behavior. |

## Test Evidence

- `TabViewResourceTests` now checks official WPF Fluent TabControl theme aliases,
  resource keys, WPF presenter slots, selected trigger behavior, and removal of
  the old WinUI sizing/helper assumptions from the stock style.
- `LayoutCompatibilityApiTests` now expects stock TabControl and TabItem
  templates to use plain WPF `ContentPresenter` slots.
- `TemplateParityTests` classifies `Styles\TabControl.xaml` as an official WPF
  Fluent stock template file that should not use `VisualStateEx` or
  `ContentPresenterEx`.
