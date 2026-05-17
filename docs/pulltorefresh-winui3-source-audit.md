# PullToRefresh WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml` at `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI 3 source inspected:

- `src\controls\dev\PullToRefresh\RefreshContainer\RefreshContainer.idl`
- `src\controls\dev\PullToRefresh\RefreshContainer\RefreshContainerPrivate.idl`
- `src\controls\dev\PullToRefresh\RefreshContainer\RefreshContainer.h`
- `src\controls\dev\PullToRefresh\RefreshContainer\RefreshContainer.cpp`
- `src\controls\dev\PullToRefresh\RefreshContainer\RefreshContainer.xaml`
- `src\controls\dev\PullToRefresh\RefreshContainer\RefreshContainer_themeresources.xaml`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\RefreshVisualizer.idl`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\RefreshVisualizerPrivate.idl`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\RefreshVisualizer.h`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\RefreshVisualizer.cpp`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\RefreshVisualizer.xaml`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\RefreshVisualizer_themeresources.xaml`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\ScrollViewerIRefreshInfoProviderAdapter.idl`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\ScrollViewerIRefreshInfoProviderAdapter.h`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\ScrollViewerIRefreshInfoProviderAdapter.cpp`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\RefreshInfoProviderImpl.h`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\RefreshInfoProviderImpl.cpp`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\ScrollViewerIRefreshInfoProviderDefaultAnimationHandler.cpp`
- `src\controls\dev\PullToRefresh\RefreshContainer\InteractionTests\RefreshContainerTests.cs`
- `src\controls\dev\PullToRefresh\RefreshVisualizer\APITests\RefreshVisualizerTests.cs`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\APITests\ScrollViewerAdapterTests.cs`
- `src\controls\dev\PullToRefresh\ScrollViewerIRefreshInfoProviderAdapter\InteractionTests\ScrollViewerAdapterTests.cs`

ModernWpf files:

- `ModernWpf.Controls\PullToRefresh\RefreshContainer.cs`
- `ModernWpf.Controls\PullToRefresh\RefreshVisualizer.cs`
- `ModernWpf.Controls\PullToRefresh\RefreshInfoProviderPrimitives.cs`
- `ModernWpf.Controls\PullToRefresh\RefreshContainer.xaml`
- `ModernWpf.Controls\PullToRefresh\RefreshVisualizer.xaml`
- `ModernWpf.Controls\PullToRefresh\PullToRefresh.xaml`
- `test\ModernWpf.WinUI.Tests\PullToRefresh\RefreshContainerApiTests.cs`
- `test\ModernWpf.WinUI.Tests\PullToRefresh\PullToRefreshApiTests.cs`

## Ported Shape

The old ModernWpf implementation treated pull detection as `RefreshContainer` mouse state with hard-coded thresholds. That guessed path has been deleted. The control family is now mapped as a source-backed WPF port against the local WinUI 3 implementation:

- `RefreshContainer` now owns a default `ScrollViewerIRefreshInfoProviderAdapter`, tracks whether the default visualizer and adapter are active, attaches the visualizer to `RefreshVisualizerPresenter`, and only applies default visualizer pull direction and 100px sizing when the visualizer is container-created, matching the WinUI source ownership model.
- `RefreshContainer` now adapts the template/content tree through `IRefreshInfoProviderAdapter.AdaptFromTree`, then assigns the result to `RefreshVisualizer.InfoProvider` instead of pushing pull ratios directly into the visualizer.
- `RefreshVisualizer` now exposes source-shaped `InfoProviderProperty`, subscribes to `IsInteractingForRefreshChanged` and `InteractionRatioChanged`, and follows WinUI's state machine for `Idle`, `Peeking`, `Interacting`, `Pending`, and `Refreshing`.
- `RefreshVisualizer.RequestRefresh` now follows the source flow: enter `Refreshing`, notify the info provider via `OnRefreshStarted`, raise `RefreshRequested`, then return to `Idle` when the request deferral completes and notify the provider via `OnRefreshCompleted`.
- `RefreshRequestedEventArgs` now has the source internal deferral-count guard used while raising `RefreshRequested`, so early deferral completion during event dispatch does not complete the refresh before all handlers run.
- `RefreshInfoProviderImpl` carries WinUI's default execution ratio (`0.8`), interaction-ratio throttling cadence, threshold tolerance, refresh started/completed events, and peeking/interacting gate.
- `ScrollViewerIRefreshInfoProviderAdapter` now owns WPF ScrollViewer pointer handling, boundary checks, tree search depth, and provider creation as the WPF substitute for WinUI's `InteractionTracker` adapter.
- Focused tests now cover the provider-driven visualizer state machine, default adapter ownership, custom visualizer sizing preservation, ScrollViewer-boundary refresh, below-threshold idle behavior, away-from-boundary suppression, request propagation, deferrals, template content hosting, and final resource mappings.

## WPF Substitutions

- WinUI uses `InteractionTracker`, `VisualInteractionSource`, composition property sets, `Translation`, and direct-manipulation callbacks. ModernWpf keeps those responsibilities isolated in `ScrollViewerIRefreshInfoProviderAdapter`, using WPF ScrollViewer mouse events, offsets, and transforms.
- WinUI can adapt arbitrary `IRefreshInfoProvider` implementations from the visual tree. ModernWpf keeps the provider interfaces internal because they are private WinUI control contracts, but preserves the source tree-search and adapter ownership shape.
- WinUI composition expression animations are represented by WPF `RenderTransform` updates and storyboard-backed scale/rotation on the visualizer content.
- WinUI touch-specific inertia, `CancelDirectManipulations`, compositor batches, raw TestUI touch automation, and localized resource fan-out remain platform gaps.

## Verification

Focused validation:

```text
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~PullToRefresh
```

Result: 18 passed.

Matrix validation:

```text
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter Name=WinUI287SyncMatrixDocumentsSourceAndTestPolicy
```

Result: 1 passed.

Controls build validation:

```text
dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore
```

Result: passed with existing warnings.
