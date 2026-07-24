# GroupItem Official WPF Fluent Source Audit

ModernWpf maps `GroupItem` to WPF's platform
`System.Windows.Controls.GroupItem`. For this stock WPF grouping container, the
primary source is official WPF Fluent rather than WinUI 3 common styles.

## Official WPF Fluent Source Files

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\GroupItem.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\CollectionViewGroup.xaml`

## ModernWpf Files

- `ModernWpf\Styles\GroupItem.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\GroupItemVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Ported Behavior

- The default `GroupItem` style now follows official WPF Fluent's implicit
  `{x:Type GroupItem}` style shape.
- The previous ModernWpf `DefaultGroupItemStyle` key, `OverridesDefaultStyle`
  setter, and `ListViewHeaderItem` wrapper were removed for this stock WPF
  container.
- The template now uses a plain WPF `ContentPresenter` named `PART_Header` and
  an `ItemsPresenter` named `ItemsPresenter` with the official `5,0,0,0`
  margin.
- The official `CollectionViewGroup` data template is included in the same
  ModernWpf style file so grouped view headers bind to `CollectionViewGroup.Name`
  like the official Fluent theme.

## WPF Substitutions

- Official WPF Fluent stores `GroupItem` and `CollectionViewGroup` in separate
  source files that are merged into the platform theme. ModernWpf keeps them
  together in `Styles\GroupItem.xaml` because that file is the existing
  grouped-view style entry point under `StockControlsResources`.
- The old `ListViewHeaderItem` visual was a ModernWpf grouping guess. It is not
  retained for the stock `GroupItem` path because official WPF Fluent uses
  plain WPF presenters.

## Tests

- `GroupItemVisualStateTests.GroupItemStyleUsesOfficialWpfFluentTemplateShape`
  covers the implicit style key, official setter/template shape, `ItemsPresenter`
  margin, and deletion of the old `ListViewHeaderItem` wrapper.
- `GroupItemVisualStateTests.CollectionViewGroupTemplateUsesOfficialWpfFluentHeaderBinding`
  verifies the official grouped-header data template and `Name` binding.
- `TemplateParityTests.OfficialWpfFluentStockTemplatesDoNotUseVisualStateEx`
  classifies `ModernWpf\Styles\GroupItem.xaml` as an official WPF Fluent stock
  control style.

Validation:

```text
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter GroupItemVisualStateTests
dotnet test test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-build --filter "GroupItemVisualStateTests|TemplateParityTests|SyncMatrixTests"
dotnet build ModernWpf.sln --no-restore
```
