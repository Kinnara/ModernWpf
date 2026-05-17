# MenuBar WinUI 3 Source Audit

Date: 2026-05-17

This audit treats `D:\repos\microsoft-ui-xaml` as the source of truth and closes the old guessed WPF `Menu` / `MenuItem` wrapper. ModernWpf now carries a WPF-adapted source-shaped `MenuBar`, `MenuBarItem`, `MenuBarItemFlyout`, style, automation peers, and tests.

## WinUI 3 Source Inputs

- `src\controls\dev\MenuBar\MenuBar.cpp`
- `src\controls\dev\MenuBar\MenuBar.h`
- `src\controls\dev\MenuBar\MenuBar.xaml`
- `src\controls\dev\MenuBar\MenuBar_themeresources.xaml`
- `src\controls\dev\MenuBar\MenuBarItem.cpp`
- `src\controls\dev\MenuBar\MenuBarItem.h`
- `src\controls\dev\MenuBar\MenuBarItem.xaml`
- `src\controls\dev\MenuBar\MenuBarItemFlyout.cpp`
- `src\controls\dev\MenuBar\MenuBarAutomationPeer.cpp`
- `src\controls\dev\MenuBar\MenuBarItemAutomationPeer.cpp`
- `src\controls\dev\MenuBar\APITests\MenuBarTests.cs`
- `src\controls\dev\MenuBar\MenuBar_InteractionTests\MenuBarTests.cs`

## ModernWpf Artifacts

- `ModernWpf.Controls\MenuBar\MenuBar.cs`
- `ModernWpf.Controls\MenuBar\MenuBarItem.cs`
- `ModernWpf.Controls\MenuBar\MenuBarItemFlyout.cs`
- `ModernWpf.Controls\MenuBar\MenuBar.xaml`
- `ModernWpf.Controls\MenuBar\MenuBarAutomationPeer.cs`
- `ModernWpf.Controls\MenuBar\MenuBarItemAutomationPeer.cs`
- `ModernWpf.Controls\MenuFlyout\MenuFlyout.cs`
- `test\ModernWpf.WinUI.Tests\MenuBar\MenuBarApiTests.cs`

## Implementation Mapping

| WinUI source behavior | ModernWpf mapping |
| --- | --- |
| `MenuBar` is a control with an owned `MenuBarItem` vector, not a platform `Menu`. | Deleted the old `MenuBar : Menu` wrapper and `MenuBarStyleHelper`. `MenuBar` is now a `Control` with an owned `ObservableCollection<MenuBarItem>`. |
| `MenuBar` template owns `LayoutRoot` and `ContentRoot`; `ContentRoot.ItemsSource` is the owned items vector. | `MenuBar.xaml` carries the source-shaped root and horizontal items host, and `OnApplyTemplate` wires `ContentRoot.ItemsSource = Items`. |
| Source updates automation set metadata for each menu bar item. | `MenuBar.UpdateAutomationSizeAndPosition` sets `PositionInSet` and `SizeOfSet` on supported target frameworks. |
| `MenuBarItem` is a control with an owned flyout item vector. | Deleted the old `MenuBarItem : MenuItem` wrapper. `MenuBarItem` is now a `Control` with an owned `ObservableCollection<object>` mirrored into `MenuBarItemFlyout.Items`. |
| Source `MenuBarItem` creates `MenuBarItemFlyout`, tracks opening/closed state, and updates parent `MenuBar.IsFlyoutOpen`. | ModernWpf creates the flyout in `OnApplyTemplate`, hooks `Opening` / `Closed`, mirrors open state, and drives source visual states. |
| Source template uses `ContentButton`, common visual states, selected/open states, and `VisualState.Setters`. | `MenuBar.xaml` exposes `ContentButton`, uses `ContentPresenterEx`, and represents the source setters with `VisualStateEx.Setters`. |
| Source pointer, keyboard, and access-key paths open and close the item flyout. | ModernWpf maps pointer enter, mouse down/up, Down/Enter/Space, Left/Right, and access-key handling to WPF input events. |
| Source has `MenuBarAutomationPeer` and `MenuBarItemAutomationPeer` with invoke and expand/collapse behavior. | ModernWpf carries WPF automation peers for MenuBar and MenuBarItem and tests control type, class name, invoke, expand, collapse, and expand/collapse state. |
| Source tests assert item collection behavior, empty-item no-popup behavior, sizing, and template behavior. | `MenuBarApiTests` now cover collection add/remove, template parts, flyout item mirroring, empty-item no-popup, source 40px minimum height, XAML content property, open/close state, and automation peers. |

## WPF Substitutions

- WinUI `OverlayInputPassThroughElement` is represented as a stored `PassThroughElement`; WPF popup input has no equivalent pass-through overlay API.
- WinUI `ContextFlyout`, `FlyoutShowOptions`, island placement, and presenter-subtree key routing are represented through ModernWpf's existing WPF `MenuFlyout` / `MenuFlyoutPresenter` stack.
- WinUI `XYFocusKeyboardNavigation`, gamepad behavior, Axe/TestUI coverage, access-key display mode, and `AutomationProperties.AccessibilityView=Raw` remain platform gaps or WPF substitutions.
- WinUI restricts menu bar flyout items to the WinUI menu flyout item model. ModernWpf still accepts WPF `MenuItem` / `Separator`-style objects because menu flyout items are currently mapped through WPF menu primitives.

## Current Validation

- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore` passed.
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~MenuBar` passed 9/9.
