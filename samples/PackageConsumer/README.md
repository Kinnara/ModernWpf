# ModernWPF package consumer

This small application validates the same experience as a normal NuGet
consumer. It is intentionally not part of `ModernWpf.sln` and references no
repository product project.

By default it restores the version in
`ModernWpfPackageValidationBaselineVersion` from `Directory.Build.props`:

```powershell
dotnet run --project .\samples\PackageConsumer\ModernWpf.PackageConsumer.csproj --framework net8.0-windows7.0
```

To exercise a package from a local feed, override both the exact version and
the feed directory:

```powershell
dotnet run --project .\samples\PackageConsumer\ModernWpf.PackageConsumer.csproj `
    --framework net8.0-windows7.0 `
    -p:ModernWpfPackageVersion=1.0.0-preview.5 `
    -p:ModernWpfPackageSource=.\artifacts
```

The project targets `net462`, `net8.0-windows7.0`, and
`net10.0-windows7.0`. The normal sample uses the recommended
`FluentControlsResources`; release smoke tests retain explicit coverage for the
legacy `XamlControlsResources` entry.
