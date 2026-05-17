# RadioButtons WinUI 3 Source Audit

ModernWpf now treats `RadioButtons` as a WinUI 3 source-backed WPF port, not as the old WinUI 2-era guessed surface.

Source of truth: `D:\repos\microsoft-ui-xaml`

Snapshot used:

```text
c70471c511a0168b61dcca13af9556465f26b673
reference/winui3-current
```

## WinUI 3 Source Files

- `src\controls\dev\RadioButtons\RadioButtons.idl`
- `src\controls\dev\RadioButtons\RadioButtons.cpp`
- `src\controls\dev\RadioButtons\RadioButtons.xaml`
- `src\controls\dev\RadioButtons\RadioButtonsAutomationPeer.cpp`
- `src\controls\dev\RadioButtons\RadioButtonsElementFactory.cpp`
- `src\controls\dev\RadioButtons\ColumnMajorUniformToLargestGridLayout.cpp`
- `src\controls\dev\RadioButtons\RadioButtonsTestHooks.cpp`
- `src\controls\dev\RadioButtons\APITests\RadioButtonsTests.cs`
- `src\controls\dev\RadioButtons\InteractionTests\RadioButtonsTests.cs`

## ModernWpf Files

- `ModernWpf.Controls\RadioButtons\RadioButtons.cs`
- `ModernWpf.Controls\RadioButtons\RadioButtons.xaml`
- `ModernWpf.Controls\RadioButtons\RadioButtonsAutomationPeer.cs`
- `ModernWpf.Controls\RadioButtons\RadioButtonsElementFactory.cs`
- `ModernWpf.Controls\RadioButtons\ColumnMajorUniformToLargestGridLayout.cs`
- `ModernWpf.Controls\RadioButtons\RadioButtonsTestHooks.cs`
- `test\ModernWpf.WinUI.Tests\RadioButtons\RadioButtonsApiTests.cs`
- `test\ModernWpf.WinUI.Tests\RadioButtons\RadioButtonsInteractionTests.cs`

## Source Alignment

- The public surface follows the WinUI IDL: `ItemsSource`, read-only `Items`, `ItemTemplate`, `ContainerFromIndex`, `SelectedIndex`, `SelectedItem`, `SelectionChanged`, `MaxColumns`, `Header`, and `HeaderTemplate`.
- `MaxColumns` now validates values greater than zero, matching the generated WinUI property validation path.
- The default template keeps the WinUI source shape: disabled header state via `VisualStateEx.Setters`, `ContentPresenterEx` for the header slot, `ItemsRepeater`, and `ColumnMajorUniformToLargestGridLayout`.
- `ColumnMajorUniformToLargestGridLayout.MaxColumns` is bound from the template, matching the WinUI XAML ownership model, instead of being re-bound imperatively from `OnApplyTemplate`.
- `RadioButtonsAutomationPeer` now exists and maps the WinUI peer behavior: class name `RadioButtons`, control type `Group`, and header text as the name fallback when no explicit automation name is set.
- `RadioButtonsElementFactory` now recognizes `DataTemplate`, `DataTemplateSelector`, and custom `IElementFactoryShim`, matching the WinUI factory order. WPF also forwards `ContentTemplateSelector` to the wrapped `RadioButton` because WPF exposes that direct equivalent.
- Selection, checked-state synchronization, inserted checked item handling, `SelectionChanged`, UIA position/size metadata, layout test hooks, and collection updates already follow the WinUI source algorithm with WPF event and dependency-property substitutions.

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
