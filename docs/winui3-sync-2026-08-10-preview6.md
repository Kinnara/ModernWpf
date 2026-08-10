# WinUI 3 Preview 6 Milestone Cutoff — 2026-08-10

This is the finite upstream disposition for the `1.0.0-preview.6` milestone.
It advances the product and Gallery targets from the Preview 5 boundaries used
by `docs/tabview-winui3-source-audit.md`. Stable WinUI has not moved. Later
upstream commits open the next milestone's review interval and block Preview 6
only if they contain an applicable security, data-loss, startup, crash,
core-input, or comparably critical regression.

## Pinned sources

| Source | Preview 5 cutoff | Preview 6 cutoff |
| --- | --- | --- |
| Latest stable WinUI 3 | `winui3/release/2.3.1` at `a97562621a1d1ea397a38a3f512c9eef99db52d8` | unchanged |
| WinUI 3 `winui3/main` | `e1aa8f64df98d6229f6cd4074d59b654616254da` | `23a73be03d194ea0ece97da71de98b6b53021b70` |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | `b78c440193aab788215888561e45adf72da848cb` |

The complete product comparison contains four unique paths in four commits.
The complete Gallery comparison contains two paths in one commit. Direct
GitHub compare and per-commit object queries were used; neither comparison
approaches the REST 300-file limit, so there is no truncated or unclassified
remainder.

## Product path and commit disposition

| Commit | Paths | Disposition |
| --- | --- | --- |
| `84e8da797e06cb0165204a954d0d6fb2214b0cdb` | `eng/Version.Details.xml` | Dependency synchronization metadata; ignored build input and noncritical. |
| `b1722a7ab209b0e3c9d6560569169adc6464af65` and merge `7b0796de839d5100353a40b9f00563a4cdceb77e` | `Samples/WinUIGallery` submodule pointer | Upstream sample-submodule bookkeeping. The Gallery repository is audited independently below; no product behavior changes. |
| `23a73be03d194ea0ece97da71de98b6b53021b70` | `controls/dev/InkToolBar/InkToolbarEraserButton.cpp`, `InkToolbarStencilButton.cpp` | Fixes an InkToolbar eraser crash by guarding ToggleButton casts. ModernWPF does not ship InkToolbar and it is not assigned to the 1.0 roadmap, so the change is non-applicable and noncritical. |
| **Total** | **4 of 4 unique changed paths** | **Complete; no unclassified remainder.** |

## Gallery path and commit disposition

| Commit | Paths | Disposition |
| --- | --- | --- |
| `b78c440193aab788215888561e45adf72da848cb` | `WinUIGallery/SampleSupport/SamplePages/DetailedInfoPage.xaml.cs`, `WinUIGallery/Samples/ConnectedAnimation/ConnectedAnimationListPage.txt` | Keeps keyboard focus visible after navigating back from a ConnectedAnimation detail page. The Preview 6 ItemContainer/LinedFlowLayout Gallery surface does not use that page or helper. The change is accessibility-positive, non-applicable to the shipped surface, and noncritical for Preview 6. |
| **Total** | **2 of 2 changed paths** | **Complete; no unclassified remainder.** |

## Preview 6 adoption

Preview 6 adds `ItemContainer`, `LinedFlowLayout`, the complete current item
collection transition family, and a WPF ScrollViewer/controller bridge needed
by Preview 7 ItemsView. Exact product, test, TestUI, design, and Gallery inputs
are pinned in `docs/itemcontainer-winui3-source-audit.md`,
`docs/linedflowlayout-winui3-source-audit.md`, and
`docs/itemsview-scrolling-wpf-adaptation.md`.

The epoch may advance to this cutoff only with those source audits, documented
WPF adaptations, Gallery pages, CLR and resource inventories, focused
behavior, virtualization, automation, theme and transition coverage, package
verification, and the complete Preview 6 release gate. No later upstream
movement silently changes this accepted boundary.
