# TwoPaneView WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\controls\dev\TwoPaneView\TwoPaneView.idl`
- `src\controls\dev\TwoPaneView\TwoPaneView.h`
- `src\controls\dev\TwoPaneView\TwoPaneView.cpp`
- `src\controls\dev\TwoPaneView\TwoPaneView.xaml`
- `src\controls\dev\TwoPaneView\DisplayRegionHelper.h`
- `src\controls\dev\TwoPaneView\DisplayRegionHelper.cpp`
- `src\controls\dev\TwoPaneView\DisplayRegionHelperTestApi.idl`
- `src\controls\dev\TwoPaneView\APITests\TwoPaneViewTests.cs`
- `src\controls\dev\TwoPaneView\InteractionTests\TwoPaneViewTests.cs`

ModernWpf files:

- `ModernWpf.Controls\TwoPaneView\TwoPaneView.cs`
- `ModernWpf.Controls\TwoPaneView\TwoPaneViewEnums.cs`
- `ModernWpf.Controls\TwoPaneView\TwoPaneView.xaml`
- `test\ModernWpf.WinUI.Tests\TwoPaneView\TwoPaneViewApiTests.cs`

## Ported Source Behavior

- The old guessed `TwoPaneViewMode`-only layout path was replaced with WinUI's source-shaped internal `ViewMode` model: `Pane1Only`, `Pane2Only`, `LeftRight`, `RightLeft`, `TopBottom`, and `BottomTop`.
- Layout now follows the source `UpdateMode` and `UpdateRowsColumns` order. Row and column lengths update on every mode refresh, while the public `ModeChanged` event only fires when the public `Mode` changes.
- The template now follows the WinUI source pane host shape: each pane is hosted by a `ScrollViewer` containing a `Border`, with vertical scrolling enabled and no extra guessed horizontal scrollbar setting.
- `DisplayRegionHelper` and the internal test API were ported as WPF-adapted helpers. Tests can simulate the source wide/tall split rectangles, including the 12px middle region and source offset math.
- The public event shape now uses `TypedEventHandler<TwoPaneView, object>`, matching WinUI's typed sender/args contract.

## WPF Substitutions

- WPF has no `ApplicationViewMode.Spanning`, `IApplicationViewSpanningRects`, `XamlRoot`, or WinUI foldable display-region service. The production helper returns `SinglePane` unless the internal test API enables simulated regions.
- WinUI sets `ScrollContentPresenter.SizesContentToTemplatedParent`; WPF has no equivalent property. ModernWpf stretches the WPF `ScrollContentPresenter` as the closest template-parent sizing substitute.
- `Border.Child` is not a WPF dependency property, so ModernWpf keeps the source `Border` host shape and assigns the child from `Pane1` / `Pane2` in `TwoPaneView` when the template or pane values change.
- WPF templates cannot use native WinUI `VisualState.Setters`, so the source state setters remain represented by `VisualStateEx.Setters`.
- The WPF display-region math clamps negative generated pixel lengths before assigning `GridLength`; WinUI receives platform-spanning rectangles that avoid those negative values.

## Verification

Focused tests cover WinUI defaults/basic setters, source public-mode event semantics, wide/tall/single-pane layout states, source pane host shape, and simulated display-region sizing/offset math.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TwoPaneView`
  - Passed 5/5.
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter FullyQualifiedName~TemplateParityTests`
  - Passed 16/16.
- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --no-restore --filter Name=WinUI287SyncMatrixDocumentsSourceAndTestPolicy`
  - Passed 1/1.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore`
  - Passed with existing warnings.
- `git diff --check`
  - No whitespace errors; CRLF normalization warnings only.
