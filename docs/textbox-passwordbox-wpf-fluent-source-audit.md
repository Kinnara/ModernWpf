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
- ModernWpf keeps `Validation.ErrorTemplate` and
  `ValidationHelper.IsTemplateValidationAdornerSite` for `TextBox` and
  `TextBoxBase` validation chrome. When validation is already invalid before
  the template is applied, the helper suppresses the deferred original-site
  adorner while redirecting validation to the template border, then restores
  the existing error-template value source at dispatcher `Loaded` priority.
  This prevents the original-site adorner from remaining after validation
  succeeds.
- Official WPF Fluent's `TextBox` clear button invokes the newer WPF
  `TemplateButtonCommand`. ModernWpf older targets do not expose that platform
  property, so the clear button uses the existing
  `TextBoxHelper.IsDeleteButton` click hook while retaining the official
  template shape and trigger matrix.
- `DataGridTextBoxStyle` is retained as a ModernWpf support style because the
  callers that reference it directly; the stock DataGrid template no longer
  wires it through `DataGridHelper`.
- `TextBoxTopHeaderMargin` and `PasswordBoxTopHeaderMargin` are retained as
  unused public aliases for existing resource consumers, but the official
  stock templates no longer have header presenters.

## Validation

- `test\ModernWpf.WinUI.Tests\CommonStyles\TextBoxPasswordBoxVisualStateTests.cs`
  covers the official WPF Fluent style setter surfaces, template parts,
  trigger shapes, clear-button substitution, `DataGridTextBoxStyle`,
  initial-error validation-adorners, and deletion of the old WinUI-derived
  template branches.
- `test\ModernWpf.WinUI.Tests\TemplateParityTests.cs` classifies
  `ModernWpf\Styles\TextBox.xaml` and
  `ModernWpf\Styles\PasswordBox.xaml` as official WPF Fluent stock templates
  that should not use `VisualStateEx`.
