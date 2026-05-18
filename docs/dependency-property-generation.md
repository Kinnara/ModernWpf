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
Use `registrationName` when a non-attached dependency property is intentionally
generated without a CLR wrapper and therefore cannot use `nameof(Property)`.

Use `setterGuard` for simple conditional `SetValue` wrappers. Use
`setterBody` only when the generated property needs a custom setter body, such
as a WinUI-style validation/coercion path that cannot be expressed as a guard.

## Conversion Rules

- Keep generated files in source control.
- Convert one coherent control/family slice or template-settings type at a
  time.
- Preserve public field names, CLR wrapper names, default values, callback
  routing, setter accessibility, and readonly key accessibility.
- Keep unusual hand-written behavior in the non-generated partial class until
  the generator explicitly supports that pattern.
- If a legacy dependency-property alias must point at a generated field, assign
  the readonly alias in the type's static constructor. Do not use a field
  initializer that depends on static initialization order across partial files.
- For WinUI parity work, copy source defaults/callback intent into the manifest
  during the control audit; do not treat the manifest as a replacement for the
  source comparison.

## Current Coverage

The existing ModernWpf control/property-owner generated-property companion
files are manifest-backed: `AnnotatedScrollBar`, `AppBarButton`,
`AppBarElementContainer`, `AppBarElementProperties`, `AppBarSeparator`,
`AppBarToggleButton`, `AutoSuggestBox`, `BreadcrumbBar`,
`BreadcrumbBarItem`, `ColorPicker`, `ColorPickerSlider`, `ColorSpectrum`,
`CommandBar`, `CommandBarFlyoutCommandBar`,
`CommandBarFlyoutCommandBarTemplateSettingsProxy`,
`CommandBarOverflowPresenter`, `ContentDialog`, `ContextFlyoutService`,
`DropDownButton`, `Flyout`, `FlyoutBase`, `FlyoutPresenter`,
`FlyoutService`, `HyperlinkButton`, `InfoBadge`, `InfoBar`,
`InfoBarPanel`, `LayoutPanel`, `ListViewBase`, `ListViewBaseItem`,
`MenuBarItem`, `MenuFlyout`, `MenuFlyoutPresenter`, `NavigationView`,
`NavigationViewItem`, `NavigationViewItemBase`,
`NavigationViewItemPresenter`, `NumberBox`, `PagerControl`,
`ParallaxView`, `PersonPicture`, `PipsPager`, `ProgressRing`,
`RadioButtons`, `RadioMenuItem`, `RatingControl`, `RatingItemImageInfo`,
`RefreshContainer`, `RefreshVisualizer`, `SelectorBar`, `SelectorBarItem`,
`SplitButton`, `SplitView`, `SwipeControl`, `SwipeItem`, `TeachingTip`,
`ToggleSplitButton`, `ToggleSwitch`, `TwoPaneView`, and `WrapPanel`.

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

The ItemsRepeater runtime dependency properties are manifest-backed:
`ItemsRepeater` and `RecyclePool`.

The AutoSuggestBox dependency properties are manifest-backed:
`AutoSuggestBox` and `AutoSuggestBoxListView`.

The CommandBar/AppBar family dependency properties are manifest-backed:
`AppBarButton`, `AppBarElementContainer`, `AppBarElementProperties`,
`AppBarSeparator`, `AppBarToggleButton`, `CommandBar`,
`CommandBarOverflowPresenter`, `CommandBarFlyoutCommandBar`, and
`CommandBarFlyoutCommandBarTemplateSettingsProxy`.

WinUI has two generated-property pools that matter for ModernWpf parity:
MUX generated files under `src/controls/dev/Generated/*.properties.*` and
framework generated files under `src/dxaml/xcp/dxaml/lib/winrtgeneratedclasses`
(`*.g.*`). Inventory against both pools is required; checking only
`*.properties.*` misses framework-generated types such as
`AppBarButtonTemplateSettings` and `SplitViewTemplateSettings`.

After the template-settings, Repeater layout, CommandBar/AppBar family, WinUI
source-backed controls, ColorPicker-family round, and the final compiled-owner
round, the remaining ModernWpf types with inline DP/AddOwner sites whose type
names match generated WinUI sources are 0 types / 0 sites. A raw scan for
`DependencyProperty.Register*` and `.AddOwner(` in `ModernWpf.Controls`
outside `*.properties.g.cs` is clean.

The final compiled-owner round converted 73 inline sites across
`AutoSuggestBoxListView`, `ContentDialog`, `ContextFlyoutService`,
`FlyoutService`, `ItemsRepeater`, `NavigationViewItemBase`,
`NavigationViewItemPresenter`, `RadioMenuItem`, `RecyclePool`, `SplitButton`,
`ToggleSplitButton`, and `ToggleSwitch`. `AutoSuggestBoxHelper` was deleted
instead of generated because it was explicitly excluded from compilation,
included as `None`, and had no call sites or template references.

The small-control round converted 49 inline sites across `DropDownButton`,
`SelectorBar`, `SelectorBarItem`, `BreadcrumbBar`, `BreadcrumbBarItem`,
`ListViewBase`, `ListViewBaseItem`, `LayoutPanel`, and
`RatingItemImageInfo`. `LayoutPanel.Layout` intentionally keeps its manual CLR
wrapper because the old getter returns the initialized `m_layout` field instead
of the raw dependency-property storage value.

The ColorPicker-family round converted 33 inline sites across `ColorPicker`,
`ColorPickerSlider`, and `ColorSpectrum`, with defaults checked against the
local WinUI `dev/ColorPicker/*.idl` source.
