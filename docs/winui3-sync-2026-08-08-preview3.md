# WinUI 3 Preview 3 Milestone Cutoff — 2026-08-08

This is the finite upstream disposition for the `1.0.0-preview.3` milestone.
It advances the product `winui3/main` target from the Preview 2 boundary in
`docs/winui3-sync-2026-08-06.md`. Stable WinUI and WinUI Gallery have not
moved. Later upstream commits open the next milestone's review interval and
block Preview 3 only if they contain an applicable security, data-loss,
startup, crash, core-input, or comparably critical regression.

## Pinned sources

| Source | Preview 2 cutoff | Preview 3 cutoff |
| --- | --- | --- |
| Latest stable WinUI 3 | `winui3/release/2.3.1` at `a97562621a1d1ea397a38a3f512c9eef99db52d8` | unchanged |
| WinUI 3 `winui3/main` | `d5bdbb190cdba0b7f1baec4b3981208a9685a360` | `6a556bb28fc227acd2ec8fe67ee64853f559084b` |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | unchanged |

The complete product comparison contains 35 paths. Stable and Gallery contain
zero changed paths. A local exact-object git comparison was used, so there is
no REST 300-file truncation or incomplete path set.

## Complete path disposition

| Count | Paths | Disposition |
| ---: | --- | --- |
| 4 | `Samples/TableViewSampleApp/**` | TableView/Chart sample and editor diagnostics. Neither control ships in Preview 3. |
| 2 | `controls/dev/NumberBox/TestUI/NumberBoxAxeTestPage.xaml`; `dxaml/xcp/dxaml/lib/TextBoxPlaceholderTextHelper.cpp` | Applicable placeholder accessibility change. ModernWpf's visible NumberBox placeholder now uses a private peer in the UIA Control view, with a focused test. |
| 2 | `dxaml/test/managed/controls/TextBox/TextBoxTests.cs`; the RichEditBox placeholder master | Upstream regression evidence. The applicable NumberBox behavior is ported; ModernWpf does not ship the deleted WinUI RichEditBox page/control. |
| 17 | `controls/dev/InkCanvas/**`, `controls/dev/InkToolBar/**`, and the InkToolbar resource accessor hook | InkCanvas/InkToolbar fixes. ModernWpf ships neither control. |
| 9 | `.github/**`, `.gitignore`, `build/**`, `test/CreateTestPayload.ps1` | Repository, scenario-test, mirroring, sample-build, and payload plumbing; no shipped control behavior or API. |
| 1 | `controls/dev/inc/TVDiag.h` | TableView-only diagnostic helper; TableView is unshipped. |
| **35** | **All changed paths** | **Complete; no unclassified remainder.** |

The shared `controls/dev/ResourceHelper/ResourceAccessor.h` change belongs to
the InkToolbar commit and adds only InkToolbar resource accessors. It is
included in the 17-path Ink disposition rather than treated as a shared
ModernWpf resource change.

## Commit disposition

| Commit | Disposition |
| --- | --- |
| `1e3a4355af601ae518e963c22b449f5b2760d1d1` | TableView cell editing and Chart/sample build inputs; unshipped. |
| `0a194fc0225435575715888473033a66af704eed` | TableView group-header resource; unshipped. |
| `9ba10e29dfcb20f6afad5fdc1dcde3663410602d` | Applicable visible-placeholder UIA change; ported for NumberBox with focused coverage. |
| `111629dc40ac0d543c9633f4cf9a019375e350ec` | Upstream scenario-test workflow plumbing; nonproduct. |
| `d20e95164ff39f9c2dd3e7d19e76be45e1f46d3f` | Content sync containing the classified changes above. |
| `a0ffda4d15bbc354b0f2f6d3ff5cf3219b26bb37` | Main merge; no additional applicable control delta. |
| `7b26e3c8de62b4d248f1dc86e731234392725266` | Repository-local build-tool ignore; nonproduct. |
| `b83803739576fa3acd9b665133429eef5c8c8781` | Mirroring script; nonproduct. |
| `e551a456523117071150cb66290bdab7c485b1b1` | WinUI Gallery build validation; nonproduct. |
| `1448d548041868bb2af100a0256e281cc52f5ae4` | InkCanvas compositor fix; control unshipped. |
| `066c564f79ccf88d820802d0a8c711e89dc19a69` | InkToolbar behavior fixes; control unshipped. |
| `6a556bb28fc227acd2ec8fe67ee64853f559084b` | Scenario-test merge; no additional applicable product delta. |

## Preview 3 adoption

TimePicker and TwoPaneView source families are adopted at this exact product
revision. Their detailed source files, immutable blob IDs, behavior, public
shape, WPF adaptations, Gallery contracts, and focused tests are recorded in
`docs/timepicker-winui3-source-audit.md` and
`docs/twopaneview-winui3-source-audit.md`.

The epoch may advance to this cutoff only with those audits, the applicable
NumberBox accessibility port, updated monitor families, complete tests,
package/API/resource verification, and the Preview 3 release gate. No later
upstream movement silently changes this accepted boundary.
