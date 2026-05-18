# Dependency Property Generation

ModernWpf keeps generated dependency-property files checked in. This mirrors the
WinUI source pattern at a C# level while keeping normal WPF builds independent
from generator execution.

## Commands

```powershell
dotnet run --project .\tools\ModernWpf.DependencyPropertyGenerator -- generate
dotnet run --project .\tools\ModernWpf.DependencyPropertyGenerator -- check
```

Use `--file <manifest>` for one manifest, or `--root <path>` when running
outside the repository root.

## Manifest Shape

Manifests live next to the generated control code and use the suffix
`.dprops.json`. The default output is the same path with
`.properties.g.cs`.

```json
{
  "usings": [ "System.Windows" ],
  "namespace": "ModernWpf.Controls",
  "type": {
    "declaration": "public partial class",
    "name": "SampleControl"
  },
  "properties": [
    {
      "name": "Value",
      "type": "double",
      "default": "0d",
      "changed": "OnValuePropertyChanged",
      "changedForwardTo": "OnValuePropertyChanged"
    }
  ]
}
```

Supported property kinds are `Register`, `RegisterReadOnly`,
`RegisterAttached`, `RegisterAttachedReadOnly`, and `AddOwner`.

Supported metadata fields include `metadata`, `metadataType`, `default`,
`options`, `changed`, `changedForwardTo`, `changedBody`, `coerce`,
`validate`, `setterGuard`, and `setterBody`.

Use `setterGuard` for simple conditional `SetValue` wrappers. Use
`setterBody` only when the generated property needs a custom setter body, such
as a WinUI-style validation/coercion path that cannot be expressed as a guard.

## Conversion Rules

- Keep generated files in source control.
- Convert one control or template-settings type at a time.
- Preserve public field names, CLR wrapper names, default values, callback
  routing, setter accessibility, and readonly key accessibility.
- Keep unusual hand-written behavior in the non-generated partial class until
  the generator explicitly supports that pattern.
- For WinUI parity work, copy source defaults/callback intent into the manifest
  during the control audit; do not treat the manifest as a replacement for the
  source comparison.

## Current Coverage

The existing ModernWpf control generated-property companion files are
manifest-backed: `AutoSuggestBox`, `NavigationView`, `NavigationViewItem`,
`NumberBox`, `PersonPicture`, `RatingControl`, and `SplitView`.

The template-settings dependency properties are also manifest-backed:
`AppBarButtonTemplateSettings`, `AppBarToggleButtonTemplateSettings`,
`CommandBarFlyoutCommandBarTemplateSettings`, `CommandBarTemplateSettings`,
`InfoBadgeTemplateSettings`, `InfoBarTemplateSettings`,
`NavigationViewItemPresenterTemplateSettings`, `NavigationViewTemplateSettings`,
`PersonPictureTemplateSettings`, `ProgressRingTemplateSettings`,
`SplitViewTemplateSettings`, `TeachingTipTemplateSettings`, and
`ToggleSwitchTemplateSettings`.

The Repeater layout dependency properties are manifest-backed:
`ColumnMajorUniformToLargestGridLayout`, `FlowLayout`, `StackLayout`, and
`UniformGridLayout`.

WinUI has two generated-property pools that matter for ModernWpf parity:
MUX generated files under `src/controls/dev/Generated/*.properties.*` and
framework generated files under `src/dxaml/xcp/dxaml/lib/winrtgeneratedclasses`
(`*.g.*`). Inventory against both pools is required; checking only
`*.properties.*` misses framework-generated types such as
`AppBarButtonTemplateSettings` and `SplitViewTemplateSettings`.

After the template-settings and Repeater layout conversions, the remaining
ModernWpf types with inline DP/AddOwner sites whose type names match generated
WinUI sources are 49 types / 351 sites:
`AnnotatedScrollBar`, `AutoSuggestBoxHelper`, `BreadcrumbBar`, `ColorPicker`,
`ColorPickerSlider`, `ColorSpectrum`, `AppBarButton`,
`AppBarElementContainer`, `AppBarSeparator`, `AppBarToggleButton`,
`CommandBar`, `CommandBarOverflowPresenter`, `CommandBarFlyoutCommandBar`,
`ContentDialog`, `Flyout`, `FlyoutBase`, `FlyoutPresenter`,
`HyperlinkButton`, `InfoBadge`, `InfoBar`, `InfoBarPanel`, `LayoutPanel`,
`ListViewBase`, `ListViewBaseItem`, `MenuBarItem`, `MenuFlyout`,
`MenuFlyoutPresenter`, `NavigationViewItemBase`,
`NavigationViewItemPresenter`, `PagerControl`, `ParallaxView`, `PipsPager`,
`ProgressRing`, `RefreshContainer`, `RefreshVisualizer`,
`RadioButtons`, `RatingItemImageInfo`, `ItemsRepeater`, `RecyclePool`,
`SelectorBar`, `SelectorBarItem`, `SplitButton`, `ToggleSplitButton`,
`SwipeControl`, `SwipeItem`, `TeachingTip`, `ToggleSwitch`, `TwoPaneView`, and
`WrapPanel`.

Those remaining sites are control implementation work, not pure storage-class
cleanup. Convert them in coherent control/family slices so the manifest changes
can be validated with the corresponding WinUI source defaults, callbacks,
coercion, attached-property accessors, and tests.
