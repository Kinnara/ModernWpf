# Downstream compatibility canaries

The manual **Downstream canaries** workflow compiles three pinned public WPF
applications against both their unchanged ModernWpfUI 0.9.x dependency and the
ModernWPF 1.x package built from the selected workflow commit. These builds are
maintainer-run release evidence; the workflow does not contact maintainers, create
pull requests, or launch an application. The candidate 1.x package is never
restored from nuget.org; restoring the unchanged 0.9.x baselines can still add
historical NuGet download traffic.

## Reviewed consumers

| Canary | Commit | Project and target | Baseline |
| --- | --- | --- | --- |
| `HMBSbige/BilibiliLiveRecordDownLoader` | `77113a04d631715abc48368da450ed4c4205ae32` | `BilibiliLiveRecordDownLoader/BilibiliLiveRecordDownLoader.csproj` (`net10.0-windows10.0.26100.0`) | ModernWpfUI 0.9.6 |
| `BililiveRecorder/BililiveRecorder` | `d263506c9ae97370e88f27620014cddb6e8c3e58` | `BililiveRecorder.WPF/BililiveRecorder.WPF.csproj` (`net472`) | ModernWpfUI 0.9.4 |
| `OpenKH/OpenKh` | `5153c6752e0855444aca88572068f73ad349de29` | `OpenKh.Tools.Kh2ObjectEditor/OpenKh.Tools.Kh2ObjectEditor.csproj` (`net8.0-windows`) | ModernWpfUI 0.9.6 |

The reviewed manifest and its schema are under `tools/downstream-canaries/`.
Changing a repository, commit, project, build tool, or migration requires a
normal reviewed repository change. Moving branches are never used.
OpenKh's four reviewed gitlink paths are also manifest-locked; the workflow
checks out those exact submodule commits transiently without committing their
source to ModernWPF.

## What the workflow does

1. It builds one candidate `.nupkg` from the exact manually selected ModernWPF
   commit and retains that package as a workflow artifact.
2. A separate disposable `windows-2022` matrix job anonymously fetches each
   exact downstream commit and attaches it to a synthetic local branch. The
   local branch gives versioning tools a branch name without consulting or
   trusting a moving upstream branch.
3. The unchanged project restores and builds in `Debug` with an isolated NuGet
   cache and an explicit nuget.org-only configuration. The full-MSBuild
   .NET Framework canary pins an isolated .NET 8-only SDK resolver so its
   upstream `latestMajor` policy cannot select an SDK that Visual Studio 2022
   MSBuild cannot load.
4. A second pristine checkout receives only reviewed migrations from the 0.9
   guide: replace the `ModernWpfUI` package version and, where the pinned source
   uses it, rename `SimpleStackPanel` to `StackPanelEx`. Exact file paths and
   occurrence counts are manifest-locked. The existing `XamlControlsResources`
   entry remains in place as the documented staged migration path.
5. The candidate restores through package-source mapping that maps the exact
   `ModernWpfUI` ID to the downloaded local feed. Other dependencies may come
   from nuget.org; the candidate package cannot.
6. The migrated project is compiled, but its executable is never launched.
   JSON, Markdown, the exact generated patch, and all restore/build logs are
   retained for 30 days.

These are intentionally the smallest [documented 0.9 migrations](migrating-from-0.9.md)
needed to compile the pinned consumers.
It tests package adoption without copying substantive GPL or Apache-licensed
source into this repository. Application owners should separately adopt
`FluentControlsResources` and perform interactive theme, input, and window
validation.

## Isolation and interpretation

The workflow is `workflow_dispatch` only and grants `contents: read`. Checkout
does not persist credentials, no OIDC permission or repository secret is
available, NuGet credential caching is disabled, and every job has a timeout.
Third-party MSBuild targets still execute as part of compilation, so every
consumer runs alone on a disposable hosted runner and never in release or pull
request jobs.

Each result has one classification:

- `migrated`: unchanged baseline and locally mapped candidate both build.
- `baseline-failure`: the pinned application no longer restores or builds even
  before ModernWPF is changed.
- `environmental`: source retrieval, networking, TLS, rate limits, or runner
  capacity prevented a meaningful comparison.
- `modernwpf-regression`: the baseline succeeds but candidate restore or build
  fails after the reviewed migration.
- `infrastructure-failure`: the manifest, candidate package, migration
  boundary, or canary runner itself is invalid.

Only `migrated` is green. A green compile is useful compatibility evidence, not
a substitute for ModernWPF's package smoke tests or Gallery visual/manual gate.

## Running and reviewing

Prefer the GitHub CLI/API to the web form. Dispatch the workflow at the branch
or tag to evaluate—an immutable release tag is preferred for release evidence—
then watch the returned run:

```powershell
gh workflow run downstream-canaries.yml --repo Kinnara/ModernWpf --ref <branch-or-tag>
gh run list --repo Kinnara/ModernWpf --workflow downstream-canaries.yml --limit 1 --json databaseId,headSha,status,conclusion
gh run watch <run-id> --repo Kinnara/ModernWpf --exit-status
```

The equivalent direct API dispatch is:

```powershell
gh api --method POST repos/Kinnara/ModernWpf/actions/workflows/downstream-canaries.yml/dispatches -f ref='<branch-or-tag>'
```

The workflow can also be started from GitHub Actions by choosing **Downstream
canaries** and **Run workflow**. For a release candidate, require all three
matrix jobs to be `migrated`; review each `result.md`,
`migration.patch`, candidate SHA-256, pinned migration-guide revision, and
logs. Retry only a clearly
`environmental` failure. A `modernwpf-regression` requires investigation or a
reviewed addition to the public migration guide, never retry masking.
