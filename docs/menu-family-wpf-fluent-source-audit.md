# Menu Family Official WPF Fluent Source Audit

ModernWpf maps `Menu`, `ContextMenu`, and `MenuItem` to WPF's platform
controls. For these stock WPF controls, the primary source is official WPF
Fluent rather than WinUI 3 common styles or ModernWpf's earlier WinUI-shaped
template guesses.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\Menu.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\ContextMenu.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\MenuItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Light.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\Dark.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Resources\Theme\HC.xaml`

The copied style files in the local WPF repository carry WPF UI contributor
attribution inside the official `PresentationFramework.Fluent` source tree; the
headers are preserved in ModernWpf.

## ModernWpf Files

- `ModernWpf\Styles\Menu.xaml`
- `ModernWpf\Styles\ContextMenu.xaml`
- `ModernWpf\Styles\MenuItem.xaml`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\MenuFamilyVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- `Menu` now follows official WPF Fluent's `DefaultMenuStyle`: transparent
  menu-bar background, menu-bar foreground alias, non-focusable menu root,
  override-default-style behavior, snap-to-device-pixels, and an `ItemsPresenter`
  clipped inside a WPF `Border`.
- `ContextMenu` now follows official WPF Fluent's stock style: context-menu
  foreground/background/border aliases, `MinWidth=140`, `HasDropShadow=False`,
  `PopupAnimation=None`, a rounded WPF `Border`, a WPF `ScrollViewer`, and a
  vertical `StackPanel` items host.
- `MenuItem` now uses official WPF `MenuItem.TopLevelHeaderTemplateKey`,
  `TopLevelItemTemplateKey`, `SubmenuHeaderTemplateKey`, and
  `SubmenuItemTemplateKey` templates, with plain WPF `ContentPresenter` slots,
  `Popup` submenu hosts, WPF trigger behavior, and official glyph resources.
- The old ModernWpf menu-family guesses were deleted for these stock controls:
  `VisualStateEx`, `ContentPresenterEx`, `BorderEx`, `ThemeShadowChrome`,
  `MenuPopup`, `Border.CornerRadius`, and
  `MenuItemHelper.VisualStateSettersEnabled` are no longer used by the stock
  `Menu`, `ContextMenu`, or `MenuItem` styles.
- Theme dictionaries now expose the official menu-family aliases:
  `MenuBarForeground`, `ContextMenuBackground`, `ContextMenuBorderBrush`,
  `ContextMenuForeground`, `FlyoutBackground`, `FlyoutBorderBrush`,
  `CheckBoxBackground`, and `CheckBoxBorderBrush`.

## WPF Substitutions

- Official WPF Fluent uses `System.Runtime` for `system:String`; ModernWpf uses
  `mscorlib` so the copied `MenuItem` resources remain compatible with older
  target frameworks.
- ModernWpf keeps `TextControlContextMenu` in `ContextMenu.xaml` for existing
  text-control context-menu integration; it is outside the stock
  `DefaultContextMenuStyle`.
- Official `MenuItem.xaml` references `CheckBoxBackground` and
  `CheckBoxBorderBrush` for checkable submenu item chrome. ModernWpf exposes
  those aliases explicitly and maps them to the same transparent/system brush
  concepts used by the official check-box resources.
- `DefaultCollectionFocusVisualStyle` is exposed as a ModernWpf theme alias
  based on `HighVisibilityFocusVisual` so the official `MenuItem` focus-style
  setter resolves through ModernWpf's existing focus visual bridge.

## Tests

- `MenuFamilyVisualStateTests.DefaultMenuStyleUsesOfficialWpfFluentTemplateShape`
  covers the stock `Menu` style key, setter surface, and clipped
  `ItemsPresenter` template shape.
- `MenuFamilyVisualStateTests.DefaultContextMenuStyleUsesOfficialWpfFluentTemplateShape`
  covers the official `ContextMenu` setter surface, rounded WPF `Border`, and
  deletion of `ThemeShadowChrome`.
- `MenuFamilyVisualStateTests.DefaultMenuItemStyleUsesOfficialWpfFluentTemplateShape`
  covers official `MenuItem` setter behavior, role-template trigger mapping,
  template keys, and separator style.
- `MenuFamilyVisualStateTests.MenuFamilyDeletesModernWpfSpecificTemplateGuesses`
  verifies the deleted WinUI-shaped helper/template surface.
- `MenuFamilyVisualStateTests.ThemeDictionariesExposeOfficialMenuFamilyAliases`
  verifies official menu-family aliases across Light, Dark, and HighContrast.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `Menu`, `ContextMenu`, and `MenuItem` as official WPF Fluent stock
  styles.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter MenuFamilyVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter TemplateParityTests
dotnet build ModernWpf.sln --no-restore
```
