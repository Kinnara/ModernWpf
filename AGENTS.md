# Repository agent guide

This file applies to the entire repository unless a more specific `AGENTS.md`
exists below the directory being changed.

## Product direction

- Work against the active ModernWpf 1.x line. Treat 0.9.x as frozen,
  unsupported historical input rather than a compatibility baseline.
- Preserve the supported package targets:
  - `net462`
  - `net8.0-windows7.0`
  - `net10.0-windows7.0`
- Do not reintroduce retired `net45`, `netcoreapp3.0`, or
  `net5.0-windows` assets.
- `ModernWpfUI.MahApps` is not part of the 1.x product.
- Follow `docs/roadmap-1.0.md` for the fixed Preview 2 through Preview 7,
  release-candidate, and stable sequence. Do not compress or renumber those
  milestones; `PipsPager` is deferred to 1.1.
- For WinUI-derived controls, current WinUI is the behavioral and API-shape
  authority unless WPF needs a documented adaptation. For stock WPF controls,
  official WPF Fluent is the styling and behavior authority.
- Read the related source-audit document under `docs/` before doing control
  parity or template synchronization work.

## Compatibility contract

- `1.0.0-preview.1` is the first audit and migration baseline, not an
  immutable API freeze across later previews.
- During the 1.0 preview series, current applicable WinUI API shape remains
  authoritative for WinUI-derived controls. A source-audited parity change may
  deliberately add, change, or remove a Preview 1 CLR API or public resource
  key when the same change updates the checked-in inventories, documents any
  WPF adaptation, adds migration guidance, and includes focused tests.
- Treat the checked-in inventories and NuGet package baseline as drift gates.
  Unexpected changes fail; an intentional preview-era break must explicitly
  rebaseline them in the reviewed change.
- Stable `1.0.0` establishes the SemVer compatibility baseline. After that
  release, preserve public CLR APIs and explicitly shipped public resource keys
  throughout 1.x; adopt an upstream breaking change through a compatible
  adaptation or in the next ModernWpf major version.
- Public API inventories live in:
  - `ModernWpf/PublicAPI.Shipped.txt`
  - `ModernWpf/PublicAPI.Unshipped.txt`
  - `ModernWpf.Controls/PublicAPI.Shipped.txt`
  - `ModernWpf.Controls/PublicAPI.Unshipped.txt`
- Public resource-key inventories live in the corresponding
  `PublicResourceKeys.Shipped.txt` and `PublicResourceKeys.Unshipped.txt`
  files.
- Prefer internal implementation types. Do not expose template helpers,
  converters, automation details, or WinRT projection types as package API.
- For ModernWpf-originated additions, do not add members to an already shipped
  public interface; add a capability interface or extensible base-class member.
  During previews, mirror a source-audited WinUI interface change when parity
  requires it and record the resulting migration.
- When adding a deliberate public resource key, run:

  ```powershell
  .\tools\api-contracts\Update-PublicResourceKeyContract.ps1
  ```

- Consult `docs/public-api-contract-1x.md` for the complete governance policy and
  `docs/migrating-from-0.9.md` for intentional legacy breaks.

## Implementation rules

- Keep changes narrowly scoped and preserve behavior across every supported
  target framework.
- Follow `.editorconfig`: four-space indentation, underscore-prefixed private
  fields, block-scoped namespaces, and CRLF for C#.
- Preserve Light, Dark, High Contrast, and compact-resource behavior when
  changing styles or templates.
- Treat user secrets as sensitive. Do not copy passwords, tokens, or other
  secrets into string-backed controls, dependency properties, logs, test
  output, or long-lived managed objects unless the user explicitly requests
  visible plaintext.
- Never log secret test values. Security regressions need a focused automated
  test that verifies the sensitive copy is not created or retained.
- Do not edit build outputs under `bin/`, `obj/`, `artifacts/`,
  `TestResults/`, or `.artifacts/`.

## Test placement

- `test/ModernWpf.WinUI.Tests`: control APIs, behavior, templates, layout,
  input, and focused WPF-hosted regressions.
- `test/ModernWpf.Theme.Tests`: theme dictionaries, resource contracts, and
  cross-theme validation.
- `test/ModernWpf.Gallery.Tests`: Gallery catalog, page, snippet, and runtime
  coverage.
- `test/ModernWpf.Tools.Tests`: repository and release tooling.
- `test/ModernWpfTestApp`: retained .NET Framework compatibility suite.
- Use the existing `WpfTestHost` and `TestWindowHost` infrastructure for
  dispatcher-bound WPF tests.
- Every skipped test must include a specific reason. Do not expand the legacy
  skip list without updating `docs/legacy-test-retirements.md`.

## Validation

- Run restore, build, and tests serially. Parallel solution builds and test
  builds can lock shared WPF `obj` files.
- Start with the smallest relevant test filter, then run the complete affected
  test project.
- For product-code changes, build the solution in Release:

  ```powershell
  dotnet restore ModernWpf.sln
  dotnet build ModernWpf.sln --configuration Release --no-restore
  ```

- A typical focused WinUI test run is:

  ```powershell
  dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj `
    --configuration Release `
    --framework net8.0-windows7.0 `
    --no-build `
    --no-restore `
    --filter "FullyQualifiedName~RelevantTestName"
  ```

- Before release or after broad cross-cutting changes, run the complete
  serialized gate in `docs/release-readiness-1x.md`. The complete WinUI suite
  must pass three consecutive times from the final clean tip.
- Restore treats moderate-or-higher NuGet audit findings as errors.
- If package shape or public surface changes, also run the package verification
  and smoke scripts documented in `docs/release-readiness-1x.md`.

## Issue and pull-request work

- Read the full issue or pull-request context before changing code.
- Validate reports against the current 1.x implementation; age alone is not a
  reason to close an issue.
- A bug fix should include a regression test that fails for the reported
  behavior and passes with the fix.
- Report the exact commands run, test counts, skipped tests, and any validation
  not performed.
