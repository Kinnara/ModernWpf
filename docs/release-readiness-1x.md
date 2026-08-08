# ModernWpf 1.x Release Readiness

This document defines the release gate for the active ModernWPF 1.x preview line.

## Supported package shape

The `ModernWpfUI` package is expected to ship these target frameworks:

- `net462`
- `net8.0-windows7.0`
- `net10.0-windows7.0`

The package must not ship the retired 0.x assets:

- `net45`
- `netcoreapp3.0`
- `net5.0-windows`

For each supported target framework, the package must contain:

- `ModernWpf.dll`
- `ModernWpf.xml`
- `ModernWpf.Controls.dll`
- `ModernWpf.Controls.xml`

The main package must not contain PDBs. The companion `.snupkg` must contain
portable SourceLinked PDBs for both assemblies on every target. Package
metadata must declare `readme.md`, the Git repository URL and exact commit,
dependency groups for all supported target frameworks, and WPF
framework-reference groups for the modern .NET targets. NuGet normalizes the
`net462` dependency group to `.NETFramework4.6.2` in the generated nuspec. Its
dependency versions must match the central values in `Directory.Build.props`.

## Preview governance and stable contract gate

`1.0.0-preview.1` is the first audit and migration baseline. It records the
public surface users received, but it does not freeze later 1.0 previews to that
exact API. Compatibility with 0.9.x is not a release requirement.

During the 1.0 preview series, current applicable WinUI API shape remains
authoritative for WinUI-derived controls. A deliberate upstream parity change
may break an earlier preview only when the source audit, WPF adaptation,
inventory rebaseline, focused tests, and `## Breaking changes` migration notes
land together. When the active package baseline advances, that section must
contain `Preview compatibility baseline: <old> → <new>`, at least one
breaking-change bullet, and explicit `**Migration:**` guidance; a no-change
placeholder cannot satisfy the gate. Stable `1.0.0` establishes the SemVer
baseline; later 1.x releases must preserve it or defer an upstream break to the
next major version.

The release gate enforces:

- The shipped CLR inventories in `ModernWpf/PublicAPI.Shipped.txt` and
  `ModernWpf.Controls/PublicAPI.Shipped.txt`. New APIs normally go in the
  corresponding `PublicAPI.Unshipped.txt`; removals and signature changes fail
  until an accepted preview break deliberately updates the inventories.
- NuGet package validation, including strict validation between compatible
  target frameworks. `ModernWpfPackageValidationBaselineVersion` selects the
  active published-package comparison. During previews it may advance to the
  current development version only as part of the audited breaking-change
  workflow; equality disables the unavailable previous-package comparison
  while the inventories remain authoritative. The immutable
  `ModernWpfPreviewAuditBaselineVersion` identifies the published
  `1.0.0-preview.1` package for explicit historical migration audits; it is
  informational and is not the active NuGet compatibility gate.
- The source-qualified public resource-key inventories in
  `ModernWpf/PublicResourceKeys.Shipped.txt` and
  `ModernWpf/PublicResourceKeys.Unshipped.txt`.
- When the current package version equals the active package baseline, every
  unshipped CLR and resource-key inventory must contain no contract entries.
- Package export checks. Public top-level types must be in `ModernWpf`
  namespaces, apart from WPF's compiler-generated
  `XamlGeneratedNamespace.GeneratedInternalTypeHelper`, and the supported
  top-level type set must agree across all three target frameworks.
- XML documentation checks that reject entries for non-public types.

Template parts, visual states, implicit/type resource keys, and unlisted style
or template resources are intentionally outside this contract. See
`docs/public-api-contract-1x.md` for the complete governance policy.

## Finite upstream milestone cutoff

Moving WinUI branches are monitored continuously, but a release candidate must
have a finite, reviewable source boundary. At the start of each preview
milestone, record the exact observed product stable, product main, and WinUI
Gallery SHAs. Classify every changed path through those cutoffs in the
milestone's dated synchronization disposition. Port applicable changes and
record an explicit reason for every excluded or deferred change before
advancing the accepted epoch.

Commits arriving after the frozen cutoff open the next review interval and do
not invalidate an otherwise complete candidate. The exception is a newly
observed change that indicates an applicable security, data-loss, startup,
crash, or equivalently critical defect in a surface shipped by the candidate.
The release notes must identify the accepted cutoff record.

## Local release gate

Run these commands from the repository root:

```powershell
dotnet restore ModernWpf.sln
dotnet build ModernWpf.sln --configuration Release --no-restore
dotnet test .\test\ModernWpf.Tools.Tests\ModernWpf.Tools.Tests.csproj --configuration Release --framework net10.0 --no-build --no-restore --logger "trx;LogFileName=tools-net10.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Theme.Tests\ModernWpf.Theme.Tests.csproj --configuration Release --framework net8.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=theme-net8.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Theme.Tests\ModernWpf.Theme.Tests.csproj --configuration Release --framework net10.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=theme-net10.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --framework net8.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=gallery-net8.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.Gallery.Tests\ModernWpf.Gallery.Tests.csproj --configuration Release --framework net10.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=gallery-net10.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --configuration Release --framework net8.0-windows7.0 --no-build --no-restore --logger "trx;LogFileName=winui-net8.trx" --results-directory .\artifacts\test-results
dotnet test .\test\ModernWpfTestApp\ModernWpfTestApp.csproj --configuration Release --framework net48 --no-build --no-restore --logger "trx;LogFileName=legacy-net48.trx" --results-directory .\artifacts\test-results
.\tools\test\Assert-TestResults.ps1 -ResultsPath .\artifacts\test-results
dotnet pack .\ModernWpf.Controls\ModernWpf.Controls.csproj --configuration Release --no-build --no-restore
$package = Get-ChildItem .\artifacts\ModernWpfUI.*.nupkg | Sort-Object LastWriteTime -Descending | Select-Object -First 1
.\tools\release\Verify-ModernWpfPackage.ps1 -PackagePath $package.FullName
.\tools\release\Test-ModernWpfPackageSmoke.ps1 -PackagePath $package.FullName
```

Build and test are intentionally serialized. Running solution build and test builds in parallel can create shared `obj` file locks in WPF projects.
Package verification also requires the packaged `icon.png` bytes to match the
reviewed, checked-in ModernWPF logo exactly.

The WinUI run above is the complete suite. Before merge, run it three
consecutive times from the final clean tip without retries. The retained
legacy suite permits only the documented retirements in
`docs/legacy-test-retirements.md`; every skipped result must include a reason.
Restore treats every moderate-or-higher NuGet audit finding as an error.

The smoke script builds and executes applications from the actual `.nupkg`
using both `FluentControlsResources` and `XamlControlsResources` on all three
targets. It treats assembly-conflict warning `MSB3277` as an error.

## Visual and manual Gallery gate

Every preview, release candidate, and stable release must use the final clean
tip to capture and review the Gallery in Light, Dark, and a real OS High
Contrast theme. Simulating High Contrast through an application theme switch
does not satisfy this check. Retain the accepted screenshots or visual-audit
report with the release evidence.

Manually exercise Gallery startup, theme switching, window chrome, navigation,
menus, `ContentDialog`, `CommandBarFlyout`, keyboard focus, and mouse dismissal
on `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`. Record the tested
Windows build, target framework, resource entry, and outcome. A release does
not pass this gate while a visual or interaction failure remains unexplained.

## Downstream compatibility canaries

Before an RC or stable release, run the manually dispatched downstream-canary
workflow against the exact candidate package. The workflow builds pinned,
unchanged baselines and then applies only the reviewed 0.9-to-1.0 migration
transformations before rebuilding from the candidate's local package feed. It
must not launch third-party applications, receive repository secrets, write to
the source repositories, or restore the candidate package from nuget.org.

The accepted result set must contain three green migrated builds covering:

- BililiveRecorder on .NET Framework 4.7.2;
- OpenKh on .NET 8;
- BilibiliLiveRecordDownLoader on .NET 10.

Every failure must be classified as a baseline/environment failure, a
documented migration requirement, or a ModernWPF regression. RC evidence must
include at least two successful, documented 0.9-to-1.0 migrations. See
`docs/downstream-canaries.md` for the pinned inputs, isolation model, report
format, and invocation instructions.

## RC and stable graduation

`1.0.0-rc.1` freezes the intended stable CLR API and explicitly shipped public
resource-key surface. Before publishing it, rerun the complete gate and the
visual/manual Gallery gate from the exact tag candidate across all supported
target frameworks.

The 14-day soak begins only after the RC artifact, downstream-canary report,
and visual/manual evidence are accepted. During the soak there may be no
unresolved security, data-loss, startup/crash, core-input, or equivalent P0/P1
release blocker. Any CLR API or public resource-key change requires a new RC
and restarts the soak. Stable `1.0.0` must otherwise differ from the accepted RC
only in version and release documentation. Download counts are informational
and never satisfy or block this gate. The complete milestone sequence is in
`docs/roadmap-1.0.md`.

## Publication

`.github/workflows/build.yml` performs validation only and supports manual
dispatch so an exact branch tip can be validated if a pull-request event is
delayed or dropped. This is a diagnostic fallback only: tagged publication
still requires a successful Build run whose event is a push to `master`, so a
manual run cannot satisfy the release gate.

Publication uses the manually dispatched `.github/workflows/release.yml`, which
accepts an existing annotated `v<Version>` tag on `master`. The tag version must
match `Directory.Build.props`. Dispatch the workflow from that same tag ref;
for example:

```powershell
gh workflow run release.yml --ref v1.0.0-preview.3 -f tag=v1.0.0-preview.3
```

Stable publication also names the accepted RC explicitly:

```powershell
gh workflow run release.yml --ref v1.0.0 `
  -f tag=v1.0.0 `
  -f accepted_rc_tag=v1.0.0-rc.1
```

The workflow builds and tests the tag once, retains the packages, symbols, TRX
results, release notes, and `SHA256SUMS`, then pauses at the protected
`nuget-production` environment. The publication job verifies the downloaded
artifact, prepares a draft GitHub release, publishes the exact `.nupkg` to
NuGet, and only then publishes the GitHub release. Preview and RC versions are
marked as prereleases; a stable SemVer tag is not.

For a stable tag, supply `accepted_rc_tag=v<Version>-rc.N`. It must identify an
annotated ancestor tag whose package version matches the RC tag and whose
GitHub release is a published, non-draft prerelease. The tag commit must also
have a successful `release.yml` run dispatched from that exact tag, proving the
trusted publication workflow completed. Production approval for an RC must
happen only after its downstream-canary and visual/manual evidence is accepted,
because the stable workflow measures the 14-day soak from that RC's GitHub
publication time.

The stable tree may contain only added or modified version/release-document
paths from the accepted RC. Renames, deletions, product/source changes, and
public-contract inventory changes fail. Within `Directory.Build.props`, only
`Version` and `ModernWpfPackageValidationBaselineVersion` may differ, and the
stable baseline must equal the accepted RC version. The exact path and property
checks live in `tools/release/Assert-StableReleaseLineage.ps1`.

Every release dispatch, including previews and RCs, must use the release tag as
both `--ref` and the `tag` input. The workflow rejects a branch dispatch or any
ref, input, checkout, and commit mismatch.

Active-development notes carry a `RELEASE-NOTES: DRAFT` marker. The release
workflow refuses to create an artifact until that marker is removed.

When preparing the retained artifact, the workflow converts documentation links
that are relative to the checked-in release-notes file into immutable,
tag-pinned GitHub URLs. Keep those links relative in source so they also work
when the notes are read under `docs/`.

NuGet publication uses Trusted Publishing rather than a stored API key. The
nuget.org policy is owned by the `kinnara` account and accepts OIDC identities
only from:

- repository owner `Kinnara`
- repository `ModernWpf`
- workflow `release.yml`
- environment `nuget-production`

The publication job requests its short-lived key immediately before the NuGet
push. Keep `id-token: write` scoped to that job, and do not add a long-lived
`NUGET_API_KEY` secret.

The NuGet push intentionally does not use `--skip-duplicate`. A duplicate
version is a hard failure because NuGet versions are immutable and the workflow
must never accept an unverified package merely because its ID and version
already exist. Do not bypass that failure or publish the GitHub prerelease from
the failed run.

Before publication, the workflow resets an existing draft's title and notes
from the retained artifact, removes every existing draft asset, and uploads the
exact verified asset set again. A stale or manually prepared draft therefore
cannot supply release text or files to the published release.

If NuGet accepted the package but a later GitHub-release step failed, keep the
GitHub release as a draft and recover manually from the retained workflow
artifact. Wait for indexing, download the package from nuget.org, verify its
repository signature, ID, version, repository commit, API/resource surface,
and every ZIP entry against the retained `.nupkg`, allowing only NuGet's added
`.signature.p7s` entry to differ. Reverify the draft assets against the retained
`SHA256SUMS`. Only after that equivalence is proven may the existing draft
prerelease be published without another NuGet push. If equivalence cannot be
proven, leave the draft unpublished, treat the immutable version as an incident,
and prepare a new version; never overwrite or silently accept it.

After publication:

1. Confirm NuGet has indexed the exact version, then install it from nuget.org
   into clean `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`
   applications. Start each application with the recommended
   `FluentControlsResources` entry; retain the tagged package-smoke evidence for
   the legacy `XamlControlsResources` path.
2. Verify the annotated tag SHA, package repository commit, `.nupkg`, `.snupkg`,
   `SHA256SUMS`, GitHub prerelease, and rendered release notes all identify the
   same commit and version.
3. For Preview 2, mark every listed 0.9.x version as **Legacy** while keeping it
   listed and restorable. The deprecation message must link to the migration
   guide and state that 0.9.x is frozen and unsupported.
4. Land a small follow-up change that advances development to
   `1.0.0-preview.3` and makes `1.0.0-preview.2` the active package-validation
   baseline. Do not combine that bump with the tagged Preview 2 tree.

When adding an explicitly supported resource key, run:

```powershell
.\tools\api-contracts\Update-PublicResourceKeyContract.ps1
```

Review the resulting unshipped entries. Promote them to the shipped resource
manifest only as part of a release baseline update.

## Source-backed WinUI parity surface

The 1.x preview keeps the WinUI-derived ModernWpf control library, not only a theme layer. The layout/template infrastructure has source-backed parity coverage for:

- `BorderEx`
- `ContentPresenterEx`
- `GridEx`
- `StackPanelEx`
- `RelativePanel`
- `CanvasEx`
- `LayoutPanel`
- Repeater layout primitives used by templates and gallery controls

The important supported behaviors are:

- WinUI-compatible XAML property surfaces for template parsing.
- Border/background chrome where WinUI exposes it.
- `BackgroundSizing` handling where the WinUI source exposes it.
- Padding and border participation in measure and arrange.
- Rounded layout clipping and rounded hit testing.
- Dynamic `CornerRadius` child-clip refresh.
- `ContentPresenterEx` use for template presenter slots instead of `ContentControlEx`.

## WPF-adapted gaps

Some WinUI behavior is intentionally adapted rather than copied:

- WPF platform controls remain WPF controls where WinUI owns a different platform primitive.
- WinUI compositor-backed features, animated visual infrastructure, DComp shadows, TestUI process automation, and Axe scans are not represented as package gates.
- WinUI visual baseline and raw pixel parity are tracked through focused gallery visual checks, not the package gate.
- Official WPF Fluent remains an input to stock-control styling, but ModernWpf still owns WinUI-compatible resource dictionaries, element theme islands, and ModernWpf-specific controls.

Current per-control parity policy and status live in
`docs/winui3-source-parity.md` and
`docs/winui3-control-source-coverage.md`. The WinUI 2.8.7 matrix is retained
only as a historical migration snapshot; it is not an active release authority.
This file defines the release gate for packaging and consumption.
