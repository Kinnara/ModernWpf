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

The existing ModernWpf generated-property companion files are manifest-backed:
`AutoSuggestBox`, `CommandBarFlyoutCommandBarTemplateSettings`,
`NavigationView`, `NavigationViewItem`, `NumberBox`, `PersonPicture`,
`RatingControl`, and `SplitView`.

Some ModernWpf controls still keep dependency properties inline even though
WinUI generates their counterparts. Move those during the corresponding
whole-control parity slice, because that changes the handwritten control file
rather than only a generated-property companion.
