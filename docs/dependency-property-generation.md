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
`validate`, and `setterGuard`.

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
