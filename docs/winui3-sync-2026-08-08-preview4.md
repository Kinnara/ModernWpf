# WinUI 3 Preview 4 Milestone Cutoff — 2026-08-08

This is the finite upstream disposition for the `1.0.0-preview.4` milestone.
It advances the product `winui3/main` target from the Preview 3 boundary in
`docs/winui3-sync-2026-08-08-preview3.md`. Stable WinUI and WinUI Gallery have
not moved. Later upstream commits open the next milestone's review interval
and block Preview 4 only if they contain an applicable security, data-loss,
startup, crash, core-input, or comparably critical regression.

## Pinned sources

| Source | Preview 3 cutoff | Preview 4 cutoff |
| --- | --- | --- |
| Latest stable WinUI 3 | `winui3/release/2.3.1` at `a97562621a1d1ea397a38a3f512c9eef99db52d8` | unchanged |
| WinUI 3 `winui3/main` | `6a556bb28fc227acd2ec8fe67ee64853f559084b` | `e1aa8f64df98d6229f6cd4074d59b654616254da` |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | unchanged |

The complete product comparison contains four paths in one commit. Stable
and Gallery contain zero changed paths. A local exact-object git comparison
was used, so there is no REST 300-file truncation or incomplete path set.

## Complete path and commit disposition

| Commit | Paths | Disposition |
| --- | --- | --- |
| `e1aa8f64df98d6229f6cd4074d59b654616254da` | `Samples/TableViewSampleApp/MainWindow.xaml`, `MainWindow.xaml.cs`, `SelectionPage.xaml`, and `SelectionPage.xaml.cs` | Adds a single-row-selection sample for TableView. ModernWpf does not ship TableView and it is not assigned to the 1.0 Preview 4–7 roadmap, so the change is non-applicable and noncritical. |
| **Total** | **4 of 4 changed paths** | **Complete; no unclassified remainder.** |

The commit does not change TitleBar source. Preview 4 nevertheless pins the
new epoch because the source-shaped TitleBar implementation was audited at
this exact head. Its API, implementation, template, resources, automation,
tests, Gallery examples, immutable blobs, and WPF input/chrome adaptations
are recorded in `docs/titlebar-winui3-gallery-parity.md`.

## Preview 4 adoption

Preview 4 adds the WinUI-derived `ModernWpf.Controls.TitleBar` family and the
separate WPF-native `WindowBackdrop` adapter for Mica and Desktop Acrylic.
The backdrop is governed by the official WPF source pin and Microsoft DWM
contracts recorded in `docs/window-backdrop-wpf-source-audit.md`; it is not a
guessed WinUI control port.

The epoch may advance to this cutoff only with both feature audits, documented
WPF adaptations, Gallery pages, CLR and resource inventories, focused behavior
and theme coverage, package verification, and the complete Preview 4 release
gate. No later upstream movement silently changes this accepted boundary.
