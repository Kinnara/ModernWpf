# NavigationView WinUI 3 Source Audit

Date: 2026-05-17

This audit treats `D:\repos\microsoft-ui-xaml` as the source of truth for the existing ModernWpf `NavigationView` surface. Unlike the old `MenuBar` wrapper, there is no small WPF wrapper file to delete here: the current ModernWpf NavigationView files already carry a large source-shaped port. This slice closes the stale "ported existing WPF surface" classification, records the source mapping, and fixes the remaining public automation-peer surface mismatch.

## WinUI 3 Source Inputs

- `src\controls\dev\NavigationView\NavigationView.cpp`
- `src\controls\dev\NavigationView\NavigationView.h`
- `src\controls\dev\NavigationView\NavigationView.idl`
- `src\controls\dev\NavigationView\NavigationView.xaml`
- `src\controls\dev\NavigationView\NavigationView_themeresources.xaml`
- `src\controls\dev\NavigationView\NavigationBackButton.xaml`
- `src\controls\dev\NavigationView\NavigationViewAutomationPeer.cpp`
- `src\controls\dev\NavigationView\NavigationViewItem.cpp`
- `src\controls\dev\NavigationView\NavigationViewItem.h`
- `src\controls\dev\NavigationView\NavigationViewItemBase.cpp`
- `src\controls\dev\NavigationView\NavigationViewItemBase.h`
- `src\controls\dev\NavigationView\NavigationViewItemPresenter.cpp`
- `src\controls\dev\NavigationView\NavigationViewItemPresenter.h`
- `src\controls\dev\NavigationView\NavigationViewItemAutomationPeer.cpp`
- `src\controls\dev\NavigationView\NavigationViewItemHeader.cpp`
- `src\controls\dev\NavigationView\NavigationViewItemSeparator.cpp`
- `src\controls\dev\NavigationView\TopNavigationViewDataProvider.cpp`
- `src\controls\dev\NavigationView\NavigationViewItemsFactory.cpp`
- `src\controls\dev\NavigationView\NavigationViewTemplateSettings.cpp`
- `src\controls\dev\NavigationView\NavigationView_ApiTests\NavigationViewTests.cs`
- `src\controls\dev\NavigationView\NavigationView_InteractionTests\*.cs`

## ModernWpf Artifacts

- `ModernWpf.Controls\NavigationView\NavigationView.cs`
- `ModernWpf.Controls\NavigationView\NavigationView.properties.cs`
- `ModernWpf.Controls\NavigationView\NavigationView.xaml`
- `ModernWpf.Controls\NavigationView\NavigationViewAutomationPeer.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItem.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItem.properties.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemBase.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemPresenter.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemPresenterTemplateSettings.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemAutomationPeer.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemHeader.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemSeparator.cs`
- `ModernWpf.Controls\NavigationView\TopNavigationViewDataProvider.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewItemsFactory.cs`
- `ModernWpf.Controls\NavigationView\NavigationViewTemplateSettings.cs`
- `ModernWpf\Styles\NavigationView.xaml`
- `ModernWpf\Styles\NavigationBackButton.xaml`
- `test\ModernWpf.WinUI.Tests\NavigationView\NavigationViewApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf mapping |
| --- | --- |
| `NavigationView` owns source-shaped template parts, pane state, menu/footer collections, selected item state, top-navigation data provider, and display-mode state. | ModernWpf keeps the source-style field/method shape in `NavigationView.cs`, including root split view, pane/title/header/footer hosts, top-navigation repeaters, overflow host, selection model, and display-mode updates. |
| Source template uses `RootSplitView`, left/top menu hosts, pane/header/footer presenters, top overflow button, visual-state groups, and NavigationViewItemPresenter slots. | `NavigationView.xaml` and `Styles\NavigationView.xaml` expose the matching named parts and use `VisualStateEx.Setters` for WPF-compatible source setter blocks. |
| Source `NavigationBackButton.xaml` owns pointer-over, pressed, and disabled back-button chrome through the template visual states. | `Styles\NavigationBackButton.xaml` now has no WPF `ControlTemplate.Triggers`; pointer-over, pressed, disabled foreground, and static `AnimatedIcon.State` fallback behavior live in `VisualStateEx.Setters` against the source `CommonStates` table. |
| Source `NavigationViewItem` owns the presenter, child repeater, split-view closed-compact behavior, chevron state, child flyout, and hierarchical item propagation. | ModernWpf keeps `NavigationViewItem`, `NavigationViewItemBase`, `NavigationViewItemPresenter`, `NavigationViewItemHeader`, and `NavigationViewItemSeparator` as source-shaped WPF controls with documented WPF input/flyout substitutions. |
| Source uses `NavigationViewAutomationPeer` as a selection provider and returns the selected container's provider. | `NavigationViewAutomationPeer` now has a public peer surface and implements the source selection-provider shape; tests cover public visibility, selection pattern, empty selection, and selected-container provider count. |
| Source API and interaction tests cover defaults, pane behavior, selection, item automation, top mode, resource/style contracts, and template states. | `NavigationViewApiTests` covers defaults, coercion, selected item clearing, expand/collapse peer availability, settings item behavior, footer/top host details, presenter states, visual-state setter conversions, back-button source state setters, theme resources, and automation selection-provider shape. |

## WPF Substitutions

- WinUI `ItemsRepeater`, `ItemsView`, top navigation overflow, and focus movement are represented by ModernWpf's WPF repeater and focus-helper stack.
- WinUI `SplitView`, `Flyout`, popup root, and XamlRoot behavior are represented by the existing WPF `SplitView`, `FlyoutBase`, and popup substitutes.
- WinUI keyboard accelerators, access keys, gamepad focus, composition header animations, ThemeShadow, and raw Axe/TestUI automation remain documented platform gaps.
- WinUI's full top-navigation overflow measuring, x:Bind phasing, recycle-container internals, and per-XamlRoot metadata are WPF substitutions rather than guessed behavior.

## Current Validation

Run after NavigationView changes:

```powershell
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~NavigationViewApiTests
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~SyncMatrixTests
git diff --check
```
