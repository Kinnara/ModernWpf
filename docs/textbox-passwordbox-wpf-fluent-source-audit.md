# TextBox / PasswordBox Official WPF Fluent Source Audit

ModernWpf now uses the official WPF Fluent text-entry styles as the source for
the stock WPF `TextBox`, `TextBoxBase`, and `PasswordBox` templates.

## Source

- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\TextBox.xaml`
- `D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles\PasswordBox.xaml`

## ModernWpf Files

- `ModernWpf\Styles\TextBox.xaml`
- `ModernWpf\Styles\PasswordBox.xaml`
- `test\ModernWpf.WinUI.Tests\CommonStyles\TextBoxPasswordBoxVisualStateTests.cs`
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs`

## Synced Behavior

- `DefaultTextBoxBaseStyle`, `DefaultTextBoxStyle`, and
  `DefaultPasswordBoxStyle` now follow the official WPF Fluent stock template
  structure with `ContentBorder`, `PART_ContentHost`, and WPF
  `ControlTemplate.Triggers`.
- The stock `TextBox` clear button now follows the official WPF Fluent
  two-column layout, delete-button style, visibility triggers, and glyph host.
- The stock `PasswordBox` template now follows official WPF Fluent's simple
  `ContentBorder` / `PART_ContentHost` shape.
- The old WinUI-derived stock `TextBox` and `PasswordBox` header,
  placeholder, description, helper visual-state, reveal-button, and
  `ContentPresenterEx` template branches were deleted for these stock WPF
  controls.
- `TemplateParityTests` now classifies `TextBox.xaml` and `PasswordBox.xaml`
  as official WPF Fluent stock template files that should not use
  `VisualStateEx`.

## ModernWpf Substitutions

- Official WPF Fluent uses `Border.CornerRadius`; ModernWpf uses
  `Border.CornerRadius` for older target-framework support.
- Official WPF Fluent uses `DefaultControlContextMenu`; ModernWpf keeps
  `TextControlContextMenu` plus `TextContextMenu.UsingTextContextMenu` so the
  existing text-control context menu integration remains available.
- ModernWpf keeps `Validation.ErrorTemplate` for `TextBox` and `TextBoxBase`
  validation chrome, but no longer redirects their adorner to the internal
  `ContentBorder`. WPF initializes an error template's data context from
  `Validation.Errors` on the adorned element; redirecting to the border made
  normal custom-template bindings such as `{Binding}` observe an empty
  collection. The official stock template has no header or description branch
  and `ContentBorder` fills the `TextBox`, so the standard TextBox-owned adorner
  retains the same chrome bounds while restoring normal WPF error-template
  semantics. Initial-error coverage also verifies that the adorner is removed
  after the value becomes valid.
- Official WPF Fluent's `TextBox` clear button invokes the newer WPF
  `TemplateButtonCommand`. ModernWpf older targets do not expose that platform
  property, so the clear button uses the existing
  `TextBoxHelper.IsDeleteButton` click hook while retaining the official
  template shape and trigger matrix.
- ModernWpf keeps the shipped `TextBoxHelper.IsDeleteButtonVisible` attached
  property as an additional gate on the official clear-button visibility
  rules. `DefaultTextBoxStyle` enables the gate, so a derived style can set it
  to `False` to hide clear buttons by default while an individual `TextBox`
  can set it back to `True`.
- ModernWpf tags the accent `GradientStop` in
  `TextControlElevationBorderFocusedBrush` with `DynamicColor`. This preserves
  the official focused-border gradient while allowing runtime accent changes
  to update the existing brush in Light, Dark, and High Contrast themes.
- `DataGridTextBoxStyle` is retained as a ModernWpf support style because the
  callers that reference it directly; the stock DataGrid template no longer
  wires it through `DataGridHelper`.
- `TextBoxTopHeaderMargin` and `PasswordBoxTopHeaderMargin` are retained as
  unused public aliases for existing resource consumers, but the official
  stock templates no longer have header presenters.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\TextBoxPasswordBoxVisualStateTests.cs`
  covers the official WPF Fluent style setter surfaces, template parts,
  trigger shapes, clear-button substitution and visibility opt-out,
  `DataGridTextBoxStyle`, initial-error validation-adorners, normal
  `Validation.Errors` binding in a custom error template, and deletion of the
  old WinUI-derived template branches.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies
  `ModernWpf\Styles\TextBox.xaml` and
  `ModernWpf\Styles\PasswordBox.xaml` as official WPF Fluent stock templates
  that should not use `VisualStateEx`.
- `test\ModernWpf.Theme.Tests\ColorsHelperTests.cs` verifies that runtime
  accent updates reach the focused-border gradient in Light, Dark, and High
  Contrast themes.
