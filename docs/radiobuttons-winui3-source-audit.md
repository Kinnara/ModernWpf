# RadioButtons WinUI 3 Source Audit

ModernWpf now treats `RadioButtons` as a WinUI 3 source-backed WPF port, not as the old WinUI 2-era guessed surface.

Date: 2026-07-18

Source of truth: `D:\repos\microsoft-ui-xaml`

Snapshot used:

```text
de3e767333c2f0717a6a70cb22bd192ced5ad885
winui3/main
```

## WinUI 3 Source Files

- `controls/dev/RadioButtons/RadioButtons.idl`
- `controls/dev/RadioButtons/RadioButtons.cpp`
- `controls/dev/RadioButtons/RadioButtons.xaml`
- `controls/dev/RadioButtons/RadioButtons_themeresources.xaml`
- `controls/dev/RadioButtons/RadioButtonsAutomationPeer.cpp`
- `controls/dev/RadioButtons/RadioButtonsElementFactory.cpp`
- `controls/dev/RadioButtons/ColumnMajorUniformToLargestGridLayout.cpp`
- `controls/dev/RadioButtons/RadioButtonsTestHooks.cpp`
- `controls/dev/Generated/RadioButtons.properties.cpp`
- `controls/dev/RadioButtons/APITests/RadioButtonsTests.cs`
- `controls/dev/RadioButtons/InteractionTests/RadioButtonsTests.cs`

## ModernWpf Files

- `ModernWpf.Controls\RadioButtons\RadioButtons.cs`
- `ModernWpf.Controls\RadioButtons\RadioButtons.xaml`
- `ModernWpf.Controls\RadioButtons\RadioButtonsAutomationPeer.cs`
- `ModernWpf.Controls\RadioButtons\RadioButtonsElementFactory.cs`
- `ModernWpf.Controls\RadioButtons\ColumnMajorUniformToLargestGridLayout.cs`
- `ModernWpf.Controls\RadioButtons\RadioButtonsTestHooks.cs`
- `test\ModernWpf.WinUI.Tests\RadioButtons\RadioButtonsApiTests.cs`
- `test\ModernWpf.WinUI.Tests\RadioButtons\RadioButtonsInteractionTests.cs`

## Current Source Identity And Delta

Most current product blobs remain byte-identical to snapshot
`c70471c511a0168b61dcca13af9556465f26b673`, but current WinUI contains one
user-visible behavior fix that required a ModernWpf port:

- `8855743667ca40c93bf655f3779f36cb576088ac` changes `Select` so
  `SelectionChangedEventArgs.RemovedItems` and `AddedItems` are empty when no
  corresponding item exists. The former code emitted a one-item collection
  containing `null` for first selection, deselection, or an out-of-range index.
  ModernWpf now constructs the same conditional empty/single-item collections,
  and ports the upstream four-scenario regression.
- `c7e2f98d978c81c2b7b0054eb042a6f8f816ec9c` compiles product test hooks only
  in upstream Debug builds. ModernWpf retains its internal hooks so the
  source-derived layout and keyboard tests remain deterministic; they do not
  alter the public control surface or rendered output.
- `beabd047460bf5d43a41fcf8bddf7730188bd5a7` adds existing RadioButtons,
  RadioButton, and theme dictionaries to perf2026 packaging without changing
  their XAML blobs.
- `8463f45162149de0ec3ad7df752596893fe3e13e` moves the source mirror from
  `src/controls/...` to `controls/...`.

Current authoritative blob identities:

| Upstream file | Git blob |
| --- | --- |
| `RadioButtons.idl` | `3c1b084f84c091c9cf67797ccf14a639ab3d87a6` |
| `RadioButtons.cpp` | `8d585ed47a80dcb34a2e823e120a5114c9dfb040` |
| `RadioButtons.xaml` | `40a2827458d18bd57fd164c3f00fc88b19501fb8` |
| `RadioButtons_themeresources.xaml` | `efbc00b43e516636dcbe421bd5f5640a64c9260a` |
| `RadioButtonsAutomationPeer.cpp` | `d2d9079db8676ae87fa349d6ddaf8d21e87333a9` |
| `RadioButtonsElementFactory.cpp` | `53ca7b0b4c32b06819d8339ab37eaa26eef47d3f` |
| `ColumnMajorUniformToLargestGridLayout.cpp` | `31e52ec57445d2256aaad05a1df510e46f85c377` |
| `Generated/RadioButtons.properties.cpp` | `b31cc1e741ac6a8f9a0d8e60b8b7d3d2fbe9e39a` |
| `APITests/RadioButtonsTests.cs` | `0796f498aaa3a51f2b16bd00094336922e992438` |
| `InteractionTests/RadioButtonsTests.cs` | `7d1bd8046b88f33520a1a0d5f3c5dc29c19292fd` |

## Current WinUI Gallery Coverage

The official WinUI Gallery tree at
`29f62479d5c046a0b854a5868e5a7cd484572d87` contains no RadioButtons sample or page. Its current `RadioButton` page covers the singular platform control and is
not a reference for the distinct RadioButtons collection control. This row
therefore uses current product source, behavior/layout/accessibility tests, and
multi-target builds without substituting the singular control page.

## Source Alignment

- The public surface follows the WinUI IDL: `ItemsSource`, read-only `Items`, `ItemTemplate`, `ContainerFromIndex`, `SelectedIndex`, `SelectedItem`, `SelectionChanged`, `MaxColumns`, `Header`, and `HeaderTemplate`.
- `MaxColumns` now validates values greater than zero, matching the generated WinUI property validation path.
- The default template keeps the WinUI source shape: disabled header state via `VisualStateEx.Setters`, `ContentPresenterEx` for the header slot, `ItemsRepeater`, and `ColumnMajorUniformToLargestGridLayout`.
- `ColumnMajorUniformToLargestGridLayout.MaxColumns` is bound from the template, matching the WinUI XAML ownership model, instead of being re-bound imperatively from `OnApplyTemplate`.
- `RadioButtonsAutomationPeer` now exists and maps the WinUI peer behavior: class name `RadioButtons`, control type `Group`, and header text as the name fallback when no explicit automation name is set.
- `RadioButtonsElementFactory` now recognizes `DataTemplate`, `DataTemplateSelector`, and custom `IElementFactoryShim`, matching the WinUI factory order. WPF also forwards `ContentTemplateSelector` to the wrapped `RadioButton` because WPF exposes that direct equivalent.
- Selection, checked-state synchronization, inserted checked item handling, `SelectionChanged`, UIA position/size metadata, layout test hooks, and collection updates already follow the WinUI source algorithm with WPF event and dependency-property substitutions.
- `SelectionChanged` now matches current source by omitting nonexistent items from `RemovedItems` and `AddedItems`; the collections never contain a `null` placeholder.

## WPF Substitutions

- WinUI `IVector<object>` is represented by a WPF `ObservableCollection<object>` stored in the read-only `Items` property slot.
- WinUI's direct `RelativeSource TemplatedParent` binding on the layout object is represented with the existing WPF `BindingProxy` helper because WPF layout objects are not in the template visual tree and do not receive live `TemplatedParent` binding updates directly.
- WinUI `GettingFocus` and `FocusManager.TryMoveFocus` are represented with WPF keyboard-focus events and control focus traversal. This preserves the tested keyboard, control-modifier, disabled-item, and multi-column behavior but does not expose WinUI gamepad-specific `OriginalKey` paths.
- WinUI attached child-handler revokers are represented by explicit WPF `Checked` / `Unchecked` unhooking during `ElementClearing`.
- WinUI `AccessKeyInvoked` has no direct ModernWpf `AccessKey` dependency-property equivalent; the older upstream access-key test is still excluded for WPF.
- WinUI automation APIs are mapped through WPF `AutomationPeer`, `AutomationProperties.PositionInSet`, and `AutomationProperties.SizeOfSet`.

## Tests

- `RadioButtonsApiTests` covers the WinUI APITest-derived custom item-template and enabled-state behavior plus the source peer, template `MaxColumns` binding, `MaxColumns` validation, header presenter, and `DataTemplateSelector` factory path.
- `RadioButtonsInteractionTests` covers selected-index and selected-item synchronization, checked-item insertion, focus handoff, keyboard traversal, control-modifier behavior, disabled-item traversal, multi-column and single-row traversal, scroll/focus regression behavior, layout test hooks, and UIA position/size metadata.
- `RadioButtonsApiTests.VerifySelectionChangedArgsDoNotContainNullItems` ports
  the current upstream first-selection, selection-switch, out-of-range, and
  deselection regression.

## Validation

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --framework net8.0-windows7.0 --filter FullyQualifiedName~RadioButtons --no-restore -m:1`
  - Passed 23/23, including the current four-scenario `SelectionChanged`
    regression and current-source identity gate.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --framework <net462|net8.0-windows7.0|net10.0-windows7.0> --no-restore -m:1`
  - All three targets passed with zero errors. The incremental net8 build
    reported zero warnings. Full net462 and net10 recompiles reported 18
    existing warnings, all in NavigationView, PersonPicture, or ItemsRepeater;
    no warning points to the RadioButtons change. Modern targets also retain the
    informational `Failed to resolve WinRT.Runtime.dll.` message.
